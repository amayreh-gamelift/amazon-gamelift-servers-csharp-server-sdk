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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aws.GameLift.Server.Model;
using log4net;
using WebSocketSharp;

namespace Aws.GameLift.Server
{
    public sealed class ServerState : WebSocketMessageHandler
    {
        private const string ENVIRONMENT_VARIABLE_WEBSOCKET_URL = "GAMELIFT_SDK_WEBSOCKET_URL";
        private const string ENVIRONMENT_VARIABLE_PROCESS_ID = "GAMELIFT_SDK_PROCESS_ID";
        private const string ENVIRONMENT_VARIABLE_HOST_ID = "GAMELIFT_SDK_HOST_ID";
        private const string ENVIRONMENT_VARIABLE_FLEET_ID = "GAMELIFT_SDK_FLEET_ID";
        private const string ENVIRONMENT_VARIABLE_AUTH_TOKEN = "GAMELIFT_SDK_AUTH_TOKEN";

        private const int ROLE_SESSION_NAME_MAX_LENGTH = 64;
        // When within 15 minutes of expiration we retrieve new instance role credentials
        public static readonly TimeSpan INSTANCE_ROLE_CREDENTIAL_TTL_MIN = TimeSpan.FromMinutes(15);
        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0);

        private static readonly Random random = new Random();

        private const double HEALTHCHECK_INTERVAL_SECONDS = 60;
        private const double HEALTHCHECK_MAX_JITTER_SECONDS = 10;
        private const double HEALTHCHECK_TIMEOUT_SECONDS = HEALTHCHECK_INTERVAL_SECONDS - HEALTHCHECK_MAX_JITTER_SECONDS;
        private const string SDK_LANGUAGE = "CSharp";

        private readonly IGameLiftWebSocket gameLiftWebSocket;
        private readonly GameLiftWebSocketRequestHandler webSocketRequestHandler;

        private ProcessParameters processParameters;
        private volatile bool processReady = false;
        private string gameSessionId;
        private DateTime terminationTime = DateTime.MinValue; //init to 1/1/0001 12:00:00 AM
        private string fleetId;
        private string hostId;
        private string processId;
        // Assume we're on managed EC2, if GetFleetRoleCredentials fails we know to set this to false
        private bool onManagedEC2 = true;
        // Map of RoleArn -> Credentials for that role
        private readonly IDictionary<string, GetFleetRoleCredentialsResult> instanceRoleResultCache = new Dictionary<string, GetFleetRoleCredentialsResult>();

        public static ServerState Instance { get; } = new ServerState();

        public static ILog Log { get; } = LogManager.GetLogger(typeof(ServerState));

        private ServerState()
        {
            gameLiftWebSocket = new GameLiftWebSocket(this);
            webSocketRequestHandler = new GameLiftWebSocketRequestHandler(gameLiftWebSocket);
        }

        public ServerState(IGameLiftWebSocket webSocket, GameLiftWebSocketRequestHandler requestHandler)
        {
            gameLiftWebSocket = webSocket;
            webSocketRequestHandler = requestHandler;
        }

        public GenericOutcome ProcessReady(ProcessParameters procParameters)
        {
            processReady = true;
            processParameters = procParameters;

            GenericOutcome result = gameLiftWebSocket.SendMessage(new ActivateServerProcessRequest(
                    GameLiftServerAPI.GetSdkVersion().Result, SDK_LANGUAGE, processParameters.Port)
            {
                LogPaths = processParameters.LogParameters.LogPaths
            });

            Task.Run(() => StartHealthCheck());

            return result;
        }

        public GenericOutcome ProcessEnding()
        {
            processReady = false;

            GenericOutcome result = gameLiftWebSocket.SendMessage(
                new TerminateServerProcessRequest());

            return result;
        }

        public GenericOutcome ActivateGameSession()
        {
            if (String.IsNullOrEmpty(gameSessionId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }
            return gameLiftWebSocket.SendMessage(new ActivateGameSessionRequest(gameSessionId));
        }

        public AwsStringOutcome GetGameSessionId()
        {
            if (String.IsNullOrEmpty(gameSessionId))
            {
                return new AwsStringOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }
            return new AwsStringOutcome(gameSessionId);
        }

        public AwsDateTimeOutcome GetTerminationTime()
        {
            if (DateTime.MinValue == terminationTime)
            {
                return new AwsDateTimeOutcome(new GameLiftError(GameLiftErrorType.TERMINATION_TIME_NOT_SET));
            }
            return new  AwsDateTimeOutcome(terminationTime);
        }

        public GenericOutcome UpdatePlayerSessionCreationPolicy(PlayerSessionCreationPolicy playerSessionPolicy)
        {
            if (String.IsNullOrEmpty(gameSessionId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }
            return gameLiftWebSocket.SendMessage(new UpdatePlayerSessionCreationPolicyRequest(gameSessionId, playerSessionPolicy));
        }

        public GenericOutcome AcceptPlayerSession(string playerSessionId)
        {
            if (String.IsNullOrEmpty(gameSessionId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }
            return gameLiftWebSocket.SendMessage(new AcceptPlayerSessionRequest(gameSessionId, playerSessionId));
        }

        public GenericOutcome RemovePlayerSession(string playerSessionId)
        {
            if (String.IsNullOrEmpty(gameSessionId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }
            return gameLiftWebSocket.SendMessage(new RemovePlayerSessionRequest(gameSessionId, playerSessionId));
        }

        public DescribePlayerSessionsOutcome DescribePlayerSessions(DescribePlayerSessionsRequest request)
        {
            GenericOutcome outcome = webSocketRequestHandler.SendRequest(request);
            if (!outcome.Success)
            {
                return new DescribePlayerSessionsOutcome(outcome.Error);
            }

            return (DescribePlayerSessionsOutcome) outcome;

        }

        public StartMatchBackfillOutcome StartMatchBackfill(StartMatchBackfillRequest request)
        {
            GenericOutcome outcome = webSocketRequestHandler.SendRequest(request);
            if (!outcome.Success)
            {
                return new StartMatchBackfillOutcome(outcome.Error);
            }

            return (StartMatchBackfillOutcome) outcome;
        }

        public GenericOutcome StopMatchBackfill(StopMatchBackfillRequest request)
        {
            return webSocketRequestHandler.SendRequest(request);
        }

        void StartHealthCheck()
        {
            Log.Debug("HealthCheck thread started.");
            while (processReady)
            {
                Task.Run(() => HeartbeatServerProcess());
                Thread.Sleep(TimeSpan.FromSeconds(GetNextHealthCheckIntervalSeconds()));
            }
        }

        static double GetNextHealthCheckIntervalSeconds()
        {
            // Jitter the healthCheck interval +/- a random value between [-MAX_JITTER_SECONDS, MAX_JITTER_SECONDS]
            double jitter = HEALTHCHECK_MAX_JITTER_SECONDS * (2 * random.NextDouble() - 1);
            return HEALTHCHECK_INTERVAL_SECONDS + jitter;
        }

        void HeartbeatServerProcess()
        {
            // duplicate ProcessReady check here right before invoking
            if (!processReady)
            {
                Log.Debug("Reporting Health on an inactive process. Ignoring.");
                return;
            }

            Log.Debug("Reporting health using the OnHealthCheck callback.");
            var result = Task.Run(() => processParameters.OnHealthCheck());

            bool healthCheckResult;
            if (!result.Wait(TimeSpan.FromSeconds(HEALTHCHECK_TIMEOUT_SECONDS)))
            {
                Log.Debug("Timed out waiting for health response from the server process. Reporting as unhealthy.");
                healthCheckResult = false;
            }
            else
            {
                healthCheckResult = result.Result;
                Log.DebugFormat("Received health response from the server process: {0}", healthCheckResult);
            }

            HeartbeatServerProcessRequest request = new HeartbeatServerProcessRequest(healthCheckResult);
            GenericOutcome outcome = webSocketRequestHandler.SendRequest(request);
            if (outcome?.Success != true)
            {
                Log.Warn("Could not send health status");
            }
        }

        public GenericOutcome InitializeNetworking(ServerParameters serverParameters)
        {
            var websocketUrl = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE_WEBSOCKET_URL) ?? serverParameters.webSocketUrl;
            processId = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE_PROCESS_ID) ?? serverParameters.processId;
            hostId = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE_HOST_ID) ?? serverParameters.hostId;
            fleetId = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE_FLEET_ID) ?? serverParameters.fleetId;
            var authToken = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE_AUTH_TOKEN) ?? serverParameters.authToken;
            return EstablishNetworking(websocketUrl, authToken);
        }

        private GenericOutcome EstablishNetworking(string webSocketUrl, string authToken)
        {
            GenericOutcome connectOutcome = gameLiftWebSocket.Connect(webSocketUrl, processId, hostId, fleetId, authToken);
            return connectOutcome.Success ? new GenericOutcome() : new GenericOutcome(new GameLiftError(GameLiftErrorType.LOCAL_CONNECTION_FAILED));
        }

        public GetComputeCertificateOutcome GetComputeCertificate()
        {
            Log.DebugFormat("Calling GetComputeCertificate");

            GetComputeCertificateRequest webSocketRequest = new GetComputeCertificateRequest();
            GenericOutcome outcome = webSocketRequestHandler.SendRequest(webSocketRequest);
            if (!outcome.Success)
            {
                return new GetComputeCertificateOutcome(outcome.Error);
            }

            return (GetComputeCertificateOutcome) outcome;
        }

        public GetFleetRoleCredentialsOutcome GetFleetRoleCredentials(GetFleetRoleCredentialsRequest request)
        {
            Log.DebugFormat("Calling GetFleetRoleCredentials: {0}", request);

            // If we've decided we're not on managed EC2, fail without making an APIGW call
            if (!onManagedEC2)
            {
                Log.DebugFormat("SDK is not running on managed EC2, fast-failing the request");
                return new GetFleetRoleCredentialsOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION));
            }

            // Check if we're cached credentials recently that still have at least 15 minutes before expiration
            if (instanceRoleResultCache.ContainsKey(request.RoleArn))
            {
                var previousResult = instanceRoleResultCache[request.RoleArn];
                if (previousResult.Expiration.Subtract(INSTANCE_ROLE_CREDENTIAL_TTL_MIN) > DateTime.UtcNow)
                {
                    Log.DebugFormat("Returning cached credentials which expire in {0} seconds",
                        (previousResult.Expiration - DateTime.UtcNow).Seconds);
                    return new GetFleetRoleCredentialsOutcome(previousResult);
                }

                instanceRoleResultCache.Remove(request.RoleArn);
            }

            // If role session name was not provided, default to fleetId-hostId
            if (request.RoleSessionName.IsNullOrEmpty())
            {
                var generatedRoleSessionName = $"{fleetId}-{hostId}";
                if (generatedRoleSessionName.Length > ROLE_SESSION_NAME_MAX_LENGTH)
                {
                    generatedRoleSessionName = generatedRoleSessionName.Substring(0, ROLE_SESSION_NAME_MAX_LENGTH);
                }

                request.RoleSessionName = generatedRoleSessionName;
            }

            // Role session name cannot be over 64 chars (enforced by IAM's AssumeRole API)
            if (request.RoleSessionName.Length > ROLE_SESSION_NAME_MAX_LENGTH)
            {
                return new GetFleetRoleCredentialsOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION));
            }

            var rawOutcome = webSocketRequestHandler.SendRequest(request);
            if (!rawOutcome.Success)
            {
                return new GetFleetRoleCredentialsOutcome(rawOutcome.Error);
            }

            var outcome = (GetFleetRoleCredentialsOutcome)rawOutcome;
            var result = outcome.Result;

            // If we get a success response from APIGW with empty fields we're not on managed EC2
            if (result.AccessKeyId == "")
            {
                onManagedEC2 = false;
                return new GetFleetRoleCredentialsOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION));
            }

            instanceRoleResultCache[request.RoleArn] = result;
            return outcome;
        }

        public void OnErrorResponse(string requestId, int statusCode, string errorMessage)
        {
            webSocketRequestHandler.HandleResponse(requestId, new GenericOutcome(new GameLiftError(statusCode, errorMessage)));
        }

        public void OnSuccessResponse(string requestId)
        {
            if(requestId != null)
            {
                webSocketRequestHandler.HandleResponse(requestId, new GenericOutcome());
            } 
            else 
            {
                Log.Info("RequestId was null");
            }
        }

        public void OnStartGameSession(GameSession gameSession)
        {
            // Inject data that already exists on the server
            gameSession.FleetId = fleetId;

            Log.DebugFormat("ServerState got the startGameSession signal. GameSession : {0}", gameSession);

            if (!processReady)
            {
                Log.Debug("Got a game session on inactive process. Ignoring.");
                return;
            }
            gameSessionId = gameSession.GameSessionId;

            Task.Run(() =>
            {
                processParameters.OnStartGameSession(gameSession);
            });
        }

        public void OnUpdateGameSession(GameSession gameSession, UpdateReason updateReason, string backfillTicketId)
        {
            Log.DebugFormat("ServerState got the updateGameSession signal. GameSession : {0}", gameSession);

            if (!processReady)
            {
                Log.Warn("Got an updated game session on inactive process.");
                return;
            }

            Task.Run(() =>
            {
                processParameters.OnUpdateGameSession(new UpdateGameSession(gameSession, updateReason, backfillTicketId));
            });
        }

        public void OnTerminateProcess(long terminationTime)
        {
            // TerminationTime is milliseconds that have elapsed since Unix epoch time begins (00:00:00 UTC Jan 1 1970).
            this.terminationTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(terminationTime);

            Log.DebugFormat("ServerState got the terminateProcess signal. termination time : {0}", this.terminationTime);

            Task.Run(() =>
            {
                processParameters.OnProcessTerminate();
            });
        }

        public void OnStartMatchBackfillResponse(string requestId, string ticketId)
        {
            StartMatchBackfillResult result = new StartMatchBackfillResult(ticketId);
            webSocketRequestHandler.HandleResponse(requestId, new StartMatchBackfillOutcome(result));
        }

        public void OnDescribePlayerSessionsResponse(string requestId, List<PlayerSession> playerSessions, string nextToken)
        {
            DescribePlayerSessionsResult result = new DescribePlayerSessionsResult(playerSessions, nextToken);
            webSocketRequestHandler.HandleResponse(requestId, new DescribePlayerSessionsOutcome(result));
        }

        public void OnGetComputeCertificateResponse(string requestId, string certificatePath, string computeName)
        {
            GetComputeCertificateResult result = new GetComputeCertificateResult(certificatePath, computeName);
            webSocketRequestHandler.HandleResponse(requestId, new GetComputeCertificateOutcome(result));
        }

        public void OnGetFleetRoleCredentialsResponse(
            string requestId,
            string assumedRoleUserArn,
            string assumedRoleId,
            string accessKeyId,
            string secretAccessKey,
            string sessionToken,
            long expiration)
        {
            var result = new GetFleetRoleCredentialsResult(
                assumedRoleUserArn,
                assumedRoleId,
                accessKeyId,
                secretAccessKey,
                sessionToken,
                EPOCH.AddMilliseconds(expiration)
            );
            webSocketRequestHandler.HandleResponse(requestId, new GetFleetRoleCredentialsOutcome(result));
        }

        public void OnRefreshConnection(string refreshConnectionEndpoint, string authToken)
        {
            var outcome = EstablishNetworking(refreshConnectionEndpoint, authToken);

            if (!outcome.Success)
            {
                Log.ErrorFormat("Failed to refresh websocket connection. The GameLift SDK will try again each minute " +
                                "until the refresh succeeds, or the websocket is forcibly closed. {0}", outcome.Error);
            }
        }

        public void Shutdown()
        {
            this.processReady = false;

            //Sleep thread for 1 sec.
            //This is to help deal with race conditions related to processReady flag being turned off (i.e. HeartbeatServerProcess)
            Thread.Sleep(1000);

            gameLiftWebSocket.Disconnect();
        }
    }
}
