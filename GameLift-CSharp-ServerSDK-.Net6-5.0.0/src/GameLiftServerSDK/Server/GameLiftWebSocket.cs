/*
* All or portions of this file Copyright (c) Amazon.com, Inc. or its affiliates or
* its licensors.
*
* For complete copyright and license terms please see the LICENSE at the root of this
* distribution (the "License"). All use of this software is governed by the License,
* or, if provided, by the license below or the license accompanying this file. Do not
* remove or modify any license notices. This file is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*
*/

using System;
using System.Net;
using Aws.GameLift.Server.Model;
using log4net;
using Newtonsoft.Json;
using Polly;
using WebSocketSharp;

namespace Aws.GameLift.Server
{
    /// <summary>
    /// Methods and classes to handle the connection between your game servers and GameLift.
    /// </summary>
    public class GameLiftWebSocket : IGameLiftWebSocket
    {
        // Websocket library has a built in 10 retry max before all connect attempts raise exceptions.
        private const int MAX_CONNECT_RETRIES = 5;
        private const int INITIAL_CONNECT_RETRY_DELAY_SECONDS = 2;
        private const int MAX_DISCONNECT_WAIT_RETRIES = 5;
        private const int DISCONNECT_WAIT_STEP_MILLIS = 200;
        private const string PID_KEY = "pID";
        private const string SDK_VERSION_KEY = "sdkVersion";
        private const string FLAVOR_KEY = "sdkLanguage";
        private const string FLAVOR = "CSharp";
        private const string AUTH_TOKEN_KEY = "Authorization";
        private const string COMPUTE_ID_KEY = "ComputeId";
        private const string FLEET_ID_KEY = "FleetId";
        private const string SOCKET_CLOSING_ERROR_MESSAGE = "An error has occurred in closing the connection";

        private static readonly ILog log = LogManager.GetLogger(typeof(GameLiftWebSocket));

        private WebSocket socket;
        private string websocketUrl;
        private string processId;
        private string hostId;
        private string fleetId;
        private string authToken;
        private readonly WebSocketMessageHandler handler;

        public GameLiftWebSocket(WebSocketMessageHandler handler)
        {
            this.handler = handler;
        }

        public GenericOutcome Connect(string websocketUrl, string processId, string hostId, string fleetId, string authToken)
        {
            log.DebugFormat("Performing connect with: URL: {0}, processId: {1}, hostId: {2}, fleetId: {3}",
                websocketUrl, processId, hostId, fleetId);
            this.websocketUrl = websocketUrl;
            this.processId = processId;
            this.hostId = hostId;
            this.fleetId = fleetId;
            this.authToken = authToken;

            if (!PerformConnect())
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.LOCAL_CONNECTION_FAILED));
            }
            return new GenericOutcome();
        }

        public GenericOutcome Disconnect()
        {
            log.DebugFormat("Disconnecting. Socket state is: {0}", socket.ReadyState);
            // If the websocket is already closing (potentially initiated by GameLift from a ProcessEnding call earlier)
            // Attempt to wait for it to close.
            if (socket.ReadyState == WebSocketState.Closing)
            {
                log.Info("WebSocket is in Closing state. Attempting to wait for socket to close");
                if (!WaitForSocketToClose())
                {
                    log.Warn("Timed out waiting for the socket to close. Will retry closing.");
                }
            }

            if (socket.ReadyState != WebSocketState.Closed)
            {
                log.Debug("Socket is not yet closed. Closing.");
                socket.Close();
            }

            log.DebugFormat("Completed Disconnect. Socket state is: {0}", socket.ReadyState);

            return new GenericOutcome();
        }

        /**
          * Develop utility method for simple local testing of sending a message across the websocket.
          * Update the "action" and message/additional fields to test an API Gateway route/response
          */
        public GenericOutcome SendMessage(Message message)
        {
            string json = JsonConvert.SerializeObject(message);
            try
            {
                log.Debug($"Sending socket message: \n{json}");
                socket.Send(json);
            } catch (Exception e)
            {
                log.Error($"could not send socket message. Exception \n{e.Data}", e);
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.SERVICE_CALL_FAILED));
            }
            return new GenericOutcome();
        }

        private bool PerformConnect()
        {
            var newSocket = new WebSocket(CreateURI());

            // re-route websocket-sharp logs to use the SDK logger
            newSocket.Log.Output = LogWithGameLiftServerSdk;

            // modify websocket-sharp logging-level to match the SDK's logging level
            // Note: Override if you would like to log websocket library at a different level from the rest of the SDK.
            newSocket.Log.Level = GetLogLevelForWebsockets();

            // Socket connection failed during handshake for TLS errors without this protocol enabled
            newSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            newSocket.OnOpen += (sender, e) =>
            {
                log.Info("Connected to GameLift API Gateway.");
            };

            newSocket.OnClose += (sender, e) =>
            {
                log.InfoFormat("Socket disconnected. Code is {0}. Reason is {1}", e.Code, e.Reason);
            };

            newSocket.OnError += (sender, e) =>
            {
                if (e.Message != null && e.Message.Contains(SOCKET_CLOSING_ERROR_MESSAGE))
                {
                    log.Warn("WebSocket reported error on closing connection. This may be because the connection is already closed");
                } else
                {
                    log.ErrorFormat("Error received from GameLift API Gateway. Error is {0}", e.Message, e.Exception);
                }
            };

            newSocket.OnMessage += (sender, e) =>
            {
                if (e.IsPing)
                {
                    log.Debug("Received ping from GameLift API Gateway.");
                    return;
                }

                if (!e.IsText)
                {
                    log.WarnFormat("Unknown Data received. Data is \n{0}", e.Data);
                    return;
                }

                try
                {
                    // Parse message as a response message. This has error fields in it which will be null for a
                    // successful response or generic message not associated with a request.
                    ResponseMessage message = JsonConvert.DeserializeObject<ResponseMessage>(e.Data);
                    if (message == null)
                    {
                        log.Error($"could not parse message. Data is \n{e.Data}");
                        return;
                    }
                    
                    log.InfoFormat("Received {0} for GameLift with status {1}. Data is \n {2}", 
                        message.Action, e.Data, message.StatusCode);

                    // It's safe to cast enums to ints in C#. Each HttpStatusCode enum is associated with its numerical
                    // status code. RequestId will be null when we get a message not associated with a request.
                    if (message.StatusCode != (int) HttpStatusCode.OK && message.RequestId != null)
                    {
                        log.WarnFormat("Received unsuccessful status code {0} for request {1} with message '{2}'", 
                            message.StatusCode, message.RequestId, message.ErrorMessage);
                        handler.OnErrorResponse(message.RequestId, message.StatusCode, message.ErrorMessage);
                        return;
                    }

                    switch (message.Action)
                    {
                        case MessageActions.CREATE_GAME_SESSION:
                        {
                            CreateGameSessionMessage createGameSessionMessage = JsonConvert.DeserializeObject<CreateGameSessionMessage>(e.Data);
                            GameSession gameSession = new GameSession(createGameSessionMessage);
                            handler.OnStartGameSession(gameSession);
                            break;
                        }
                        case MessageActions.UPDATE_GAME_SESSION:
                        {
                            UpdateGameSessionMessage updateGameSessionMessage = JsonConvert.DeserializeObject<UpdateGameSessionMessage>(e.Data);
                            handler.OnUpdateGameSession(updateGameSessionMessage.GameSession,
                                UpdateReasonMapper.GetUpdateReasonForName(updateGameSessionMessage.UpdateReason),
                                updateGameSessionMessage.BackfillTicketId);
                            break;
                        }
                        case MessageActions.TERMINATE_PROCESS:
                        {
                            TerminateProcessMessage terminateProcessMessage = JsonConvert.DeserializeObject<TerminateProcessMessage>(e.Data);
                            handler.OnTerminateProcess(terminateProcessMessage.TerminationTime);
                            break;
                        }
                        case MessageActions.START_MATCH_BACKFILL:
                        {
                            StartMatchBackfillResponse startMatchBackfillResponse = JsonConvert.DeserializeObject<StartMatchBackfillResponse>(e.Data);
                            handler.OnStartMatchBackfillResponse(startMatchBackfillResponse.RequestId, startMatchBackfillResponse.TicketId);
                            break;
                        }
                        case MessageActions.DESCRIBE_PLAYER_SESSIONS:
                        {
                            DescribePlayerSessionsResponse describePlayerSessionsResponse = JsonConvert.DeserializeObject<DescribePlayerSessionsResponse>(e.Data,
                                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                            handler.OnDescribePlayerSessionsResponse(describePlayerSessionsResponse.RequestId, describePlayerSessionsResponse.PlayerSessions, describePlayerSessionsResponse.NextToken);
                            break;
                        }
                        case MessageActions.GET_COMPUTE_CERTIFICATE:
                        {
                            GetComputeCertificateResponse getComputeCertificateResponse = JsonConvert.DeserializeObject<GetComputeCertificateResponse>(e.Data);
                            handler.OnGetComputeCertificateResponse(getComputeCertificateResponse.RequestId,
                                getComputeCertificateResponse.CertificatePath,
                                getComputeCertificateResponse.ComputeName);
                            break;
                        }
                        case MessageActions.GET_FLEET_ROLE_CREDENTIALS:
                        {
                            var response = JsonConvert.DeserializeObject<GetFleetRoleCredentialsResponse>(e.Data);
                            handler.OnGetFleetRoleCredentialsResponse(
                                response.RequestId,
                                response.AssumedRoleUserArn,
                                response.AssumedRoleId,
                                response.AccessKeyId,
                                response.SecretAccessKey,
                                response.SessionToken,
                                response.Expiration);
                            break;
                        }
                        case MessageActions.REFRESH_CONNECTION:
                        {
                            var refreshConnectionMessage = JsonConvert.DeserializeObject<RefreshConnectionMessage>(e.Data);
                            handler.OnRefreshConnection(refreshConnectionMessage.RefreshConnectionEndpoint, refreshConnectionMessage.AuthToken);
                            break;
                        }
                        default:
                            handler.OnSuccessResponse(message.RequestId);
                            break;
                    }
                } 
                catch (Exception ex)
                {
                    log.Error($"could not parse message. Data is \n{e.Data}", ex);
                }
            };

            // Policy that retries if function returns false with exponential backoff.
            var retryPolicy = Policy
                .HandleResult<bool>(r => !r)
                .WaitAndRetry(MAX_CONNECT_RETRIES, retry => TimeSpan.FromSeconds(Math.Pow(INITIAL_CONNECT_RETRY_DELAY_SECONDS, retry)));

            // Specific exceptions that prevent connection are swallowed and logged during connect.
            // Exceptions thrown for invalid arguments and max retries, which are not retriable.
            var wasSuccessful = retryPolicy.Execute(() =>
            {
                newSocket.Connect();
                return newSocket.IsAlive;
            });

            if (!wasSuccessful)
            {
                try
                {
                    newSocket.CloseAsync(CloseStatusCode.Normal);
                }
                catch (Exception e)
                {
                    log.Warn("Failed to close new websocket after a connection failure, ignoring", e);
                }
                return false;
            }

            // "Flip" traffic from our old websocket to our new websocket. Close the old one if necessary
            var oldSocket = socket;
            socket = newSocket;
            try
            {
                oldSocket?.CloseAsync(CloseStatusCode.Normal);
            }
            catch (Exception e)
            {
                log.Warn("Failed to close old websocket after a connection refresh, ignoring", e);
            }

            return true;
        }

        private string CreateURI()
        {
            var queryString = string.Format("{0}={1}&{2}={3}&{4}={5}&{6}={7}&{8}={9}&{10}={11}",
                                                PID_KEY,
                                                processId,
                                                SDK_VERSION_KEY,
                                                GameLiftServerAPI.GetSdkVersion().Result,
                                                FLAVOR_KEY,
                                                FLAVOR,
                                                AUTH_TOKEN_KEY,
                                                authToken,
                                                COMPUTE_ID_KEY,
                                                hostId,
                                                FLEET_ID_KEY,
                                                fleetId
                                               );
            var endpoint = string.Format("{0}?{1}", websocketUrl, queryString);
            return endpoint;
        }

        private bool WaitForSocketToClose()
        {
            // Policy that retries if function returns false with with constant interval
            var retryPolicy = Policy
                .HandleResult<bool>(r => !r)
                .WaitAndRetry(MAX_DISCONNECT_WAIT_RETRIES, retry => TimeSpan.FromMilliseconds(DISCONNECT_WAIT_STEP_MILLIS));

            return retryPolicy.Execute(() =>
            {
                return socket.ReadyState == WebSocketState.Closed;
            });
        }

        // Helper method to link WebsocketSharp logger with the GameLift SDK logger
        private static void LogWithGameLiftServerSdk(LogData data, string path)
        {
            string socketLogData = data.ToString();
            switch (data.Level)
            {
                case LogLevel.Info:
                    log.Info(socketLogData);
                    break;
                case LogLevel.Warn:
                    log.Warn(socketLogData);
                    break;
                case LogLevel.Error:
                    log.Error(socketLogData);
                    break;
                case LogLevel.Fatal:
                    log.Fatal(socketLogData);
                    break;
                case LogLevel.Trace:
                case LogLevel.Debug:
                default:
                    log.Debug(socketLogData);
                    break;
            }
        }

        // Helper method to get the logging level the websocket (websocketsharp library) should use.
        // Uses the same logging level as used for GameLift Server SDK.
        private static LogLevel GetLogLevelForWebsockets()
        {
            if (log.IsDebugEnabled)
                return LogLevel.Trace;
            if (log.IsInfoEnabled)
                return LogLevel.Info;
            if (log.IsWarnEnabled)
                return LogLevel.Warn;
            if (log.IsErrorEnabled)
                return LogLevel.Error;

            // otherwise, only log fatal by default
            return LogLevel.Fatal;
        }
    }
}
