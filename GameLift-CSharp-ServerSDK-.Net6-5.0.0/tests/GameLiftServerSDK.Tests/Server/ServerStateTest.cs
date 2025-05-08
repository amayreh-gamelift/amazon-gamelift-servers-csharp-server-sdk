using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Aws.GameLift.Server;
using Aws.GameLift.Server.Model;
using Moq;
using NUnit.Framework;
using static Moq.It;

namespace Aws.GameLift.Tests.Server
{
    [TestFixture]
    public class ServerStateTest
    {
        private static readonly string ENVIRONMENT_VARIABLE_WEBSOCKET_URL = "GAMELIFT_SDK_WEBSOCKET_URL";
        private static readonly string ENVIRONMENT_VARIABLE_PROCESS_ID = "GAMELIFT_SDK_PROCESS_ID";
        private static readonly string ENVIRONMENT_VARIABLE_HOST_ID = "GAMELIFT_SDK_HOST_ID";
        private static readonly string ENVIRONMENT_VARIABLE_FLEET_ID = "GAMELIFT_SDK_FLEET_ID";
        private static readonly string ENVIRONMENT_VARIABLE_AUTH_TOKEN = "GAMELIFT_SDK_AUTH_TOKEN";
        private static readonly string WEBSOCKET_URL = "webSocketUrl";
        private static readonly string PROCESS_ID = "processId";
        private static readonly string HOST_ID = "hostId";
        private static readonly string FLEET_ID = "fleetId";
        private static readonly string PLAYER_SESSION_ID = "playerSessionId";
        private static readonly string PLAYER_SESSION_STATUS_FILTER = "playerSessionStatusFilter";
        private static readonly string PLAYER_ID = "playerId";
        private static readonly string GAME_SESSION_ID = "gameSessionId";
        private static readonly string AUTH_TOKEN = "authToken";
        private static readonly string COMPUTE_CERT = "computeCert";
        private static readonly int PORT_NUMBER = 1234;
        private static readonly List<string> LOG_PATHS = new List<string> { "C:\\game\\logs", "C:\\game\\error" };
        private static string NEXT_TOKEN = "nextToken";

        private static readonly ProcessParameters GENERIC_PROCESS_PARAMS = new ProcessParameters(
            (gameSession) => { },
            (updateGameSession) => { },
            () => { },
            () => { return false; },
            PORT_NUMBER,
            new LogParameters(LOG_PATHS
            ));

        private static readonly string TICKET_ID = "SomeTicket";
        private static readonly string GAME_SESSION_ARN = "TODO_SOME_ARN";
        private static readonly string MATCHMAKER_ARN = "TODO_SOME_MATCHMAKER_ARN";

        private Mock<IGameLiftWebSocket> mockWebSocket;
        private Mock<GameLiftWebSocketRequestHandler> mockRequestHandler;
        private ServerState state;

        private IDictionary<string, string> oldEnvironmentVariables = new Dictionary<string, string>();

        [SetUp]
        public void SetUp()
        {
            mockWebSocket = new Mock<IGameLiftWebSocket>();
            mockRequestHandler = new Mock<GameLiftWebSocketRequestHandler>();
            state = new ServerState(mockWebSocket.Object, mockRequestHandler.Object);
        }

        private void UsingEnvironmentVariables(IDictionary<string, string> environmentVariables, Action testCode)
        {
            oldEnvironmentVariables.Clear();
            foreach (var environmentVariable in environmentVariables)
            {
                oldEnvironmentVariables[environmentVariable.Key] = Environment.GetEnvironmentVariable(environmentVariable.Key);
                Environment.SetEnvironmentVariable(environmentVariable.Key, environmentVariable.Value);
            }

            try
            {
                testCode.Invoke();
            }
            finally
            {
                foreach (var environmentVariable in oldEnvironmentVariables)
                {
                    Environment.SetEnvironmentVariable(environmentVariable.Key, environmentVariable.Value);
                }
            }
        }

        [Test]
        public void WhenInstanceThenInstanceIsReturned()
        {
            Assert.IsNotNull(ServerState.Instance);
        }

        [Test]
        public void SameInstanceIsAlwaysReturned()
        {
            ServerState firstInstance = ServerState.Instance;
            ServerState secondInstance = ServerState.Instance;
            Assert.AreSame(firstInstance, secondInstance);
        }

        [Test]
        public void GIVEN_serverParameters_WHEN_initializeNetworking_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.Connect(IsAny<string>(), IsAny<string>(),
                IsAny<string>(), IsAny<string>(), IsAny<string>())).Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.InitializeNetworking(new ServerParameters(WEBSOCKET_URL, PROCESS_ID,
                HOST_ID, FLEET_ID, AUTH_TOKEN));

            // THEN
            mockWebSocket.Verify(websocket => websocket.Connect(WEBSOCKET_URL, PROCESS_ID,
                HOST_ID, FLEET_ID, AUTH_TOKEN));
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_serverParameters_WHEN_initializeNetworkingWithEnvVars_THEN_returnsSuccessOutcome()
        {
            const string websocketUrlOverride = "newWebsocketUrl";
            const string processIdOverride = "newProcessId";
            const string hostIdOverride = "newHostId";
            const string fleetIdOverride = "newFleetId";
            const string authTokenOverride = "newAuthToken";

            UsingEnvironmentVariables(new Dictionary<string, string>
            {
                [ENVIRONMENT_VARIABLE_WEBSOCKET_URL] = websocketUrlOverride,
                [ENVIRONMENT_VARIABLE_PROCESS_ID] = processIdOverride,
                [ENVIRONMENT_VARIABLE_HOST_ID] = hostIdOverride,
                [ENVIRONMENT_VARIABLE_FLEET_ID] = fleetIdOverride,
                [ENVIRONMENT_VARIABLE_AUTH_TOKEN] = authTokenOverride
            }, () =>
            {
                // GIVEN
                mockWebSocket.Setup(websocket => websocket.Connect(IsAny<string>(), IsAny<string>(),
                    IsAny<string>(), IsAny<string>(), IsAny<string>())).Returns(new GenericOutcome());

                // WHEN
                GenericOutcome outcome = state.InitializeNetworking(new ServerParameters(WEBSOCKET_URL, PROCESS_ID,
                    HOST_ID, FLEET_ID, AUTH_TOKEN));

                // THEN
                mockWebSocket.Verify(websocket => websocket.Connect(websocketUrlOverride,
                    processIdOverride,
                    hostIdOverride,
                    fleetIdOverride,
                    authTokenOverride));
                Assert.IsTrue(outcome.Success);
            });
        }
        
        [Test]
        public void GIVEN_refreshParameters_WHEN_onRefreshConnection_THEN_callsRefreshCallback()
        {
            // GIVEN
            const string newAuthToken = "newAuthToken";
            const string newWebSocketUrl = "newWebsocketUrl";
            mockWebSocket.Setup(websocket => websocket.Connect(IsAny<string>(), IsAny<string>(),
                IsAny<string>(), IsAny<string>(), IsAny<string>())).Returns(new GenericOutcome());
            state.InitializeNetworking(new ServerParameters(WEBSOCKET_URL, PROCESS_ID,
                HOST_ID, FLEET_ID, AUTH_TOKEN));
            state.ProcessReady(GENERIC_PROCESS_PARAMS);

            // WHEN
            state.OnRefreshConnection(newWebSocketUrl, newAuthToken);

            // THEN
            mockWebSocket.Verify(websocket => websocket.Connect(WEBSOCKET_URL, PROCESS_ID,
                HOST_ID, FLEET_ID, AUTH_TOKEN));
            mockWebSocket.Verify(websocket => websocket.Connect(newWebSocketUrl, PROCESS_ID,
                HOST_ID, FLEET_ID, newAuthToken));
        }
        
        [Test]
        public void GIVEN_refreshParametersAndConnectionFails_WHEN_onRefreshConnection_THEN_doesNotRefreshConnection()
        {
            // GIVEN
            const string newAuthToken = "newAuthToken";
            const string newWebSocketUrl = "newWebsocketUrl";
            mockWebSocket.Setup(websocket => websocket.Connect(
                IsAny<string>(), 
                IsAny<string>(),
                IsAny<string>(), 
                IsAny<string>(), 
                IsAny<string>())
            ).Returns(new GenericOutcome());
            mockWebSocket.Setup(websocket => websocket.Connect(
                Is<string>(it => it == newWebSocketUrl), 
                IsAny<string>(),
                IsAny<string>(), 
                IsAny<string>(), 
                Is<string>(it => it == newAuthToken))
            ).Returns(new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION)));
            state.InitializeNetworking(new ServerParameters(WEBSOCKET_URL, PROCESS_ID,
                HOST_ID, FLEET_ID, AUTH_TOKEN));
            state.ProcessReady(GENERIC_PROCESS_PARAMS);

            // WHEN
            state.OnRefreshConnection(newWebSocketUrl, newAuthToken);

            // THEN
            mockWebSocket.Verify(websocket => websocket.Connect(WEBSOCKET_URL, PROCESS_ID,
                HOST_ID, FLEET_ID, AUTH_TOKEN));
            mockWebSocket.Verify(websocket => websocket.Connect(newWebSocketUrl, PROCESS_ID,
                HOST_ID, FLEET_ID, newAuthToken));
        }

        [Test]
        public void GIVEN_processParameters_WHEN_processReady_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<Message>()))
                .Returns(new GenericOutcome(new GameLiftError()));
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<ActivateServerProcessRequest>()))
                .Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.ProcessReady(GENERIC_PROCESS_PARAMS);

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(Is<ActivateServerProcessRequest>(
                req => req.SdkVersion == "5.0.0" && req.SdkLanguage == "CSharp" && req.Port == PORT_NUMBER &&
                        req.LogPaths == LOG_PATHS)), Times.Once);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_serverStateInstance_WHEN_processEnding_THEN_outcomeSucceeds()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<Message>()))
                .Returns(new GenericOutcome(new GameLiftError()));
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<TerminateServerProcessRequest>()))
                .Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.ProcessEnding();

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(IsAny<TerminateServerProcessRequest>()), Times.Once);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_gameSessionStarted_WHEN_activateGameSession_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            startGameSession();

            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<ActivateGameSessionRequest>()))
                .Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.ActivateGameSession();

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(Is<ActivateGameSessionRequest>(
                req => req.GameSessionId == GAME_SESSION_ID)), Times.Once);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_gameSessionStarted_WHEN_updatePlayerSessionCreationPolicy_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            startGameSession();
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<UpdatePlayerSessionCreationPolicyRequest>()))
                .Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.UpdatePlayerSessionCreationPolicy(PlayerSessionCreationPolicy.ACCEPT_ALL);

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(Is<UpdatePlayerSessionCreationPolicyRequest>(
                req => req.PlayerSessionPolicy == PlayerSessionCreationPolicy.ACCEPT_ALL.ToString() 
                       && req.GameSessionId == GAME_SESSION_ID)), Times.Once);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_gameSessionStarted_WHEN_acceptPlayerSession_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            startGameSession();

            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<AcceptPlayerSessionRequest>()))
                .Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.AcceptPlayerSession(PLAYER_SESSION_ID);

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(Is<AcceptPlayerSessionRequest>(
                    req => req.GameSessionId == GAME_SESSION_ID && req.PlayerSessionId == PLAYER_SESSION_ID)),
                Times.Once);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_gameSessionStarted_WHEN_removePlayerSession_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            startGameSession();

            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<RemovePlayerSessionRequest>()))
                .Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.RemovePlayerSession(PLAYER_SESSION_ID);

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(Is<RemovePlayerSessionRequest>(
                    req => req.GameSessionId == GAME_SESSION_ID && req.PlayerSessionId == PLAYER_SESSION_ID)),
                Times.Once);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_gameSessionNotStarted_WHEN_activateGameSession_THEN_returnsFailedOutcome()
        {
            // GIVEN/WHEN
            GenericOutcome outcome = state.ActivateGameSession();

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(IsAny<Message>()), Times.Never);
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void GIVEN_gameSessionNotStarted_WHEN_updatePlayerSessionCreationPolicy_THEN_returnsFailedOutcome()
        {
            // GIVEN/WHEN
            GenericOutcome outcome = state.UpdatePlayerSessionCreationPolicy(PlayerSessionCreationPolicy.ACCEPT_ALL);

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(IsAny<Message>()), Times.Never);
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void GIVEN_gameSessionNotStarted_WHEN_acceptPlayerSession_THEN_returnsFailedOutcome()
        {
            // GIVEN/WHEN
            GenericOutcome outcome = state.AcceptPlayerSession(PLAYER_SESSION_ID);

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(IsAny<Message>()), Times.Never);
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void GIVEN_gameSessionNotStarted_WHEN_removePlayerSession_THEN_returnsFailedOutcome()
        {
            // GIVEN/WHEN
            GenericOutcome outcome = state.RemovePlayerSession(PLAYER_SESSION_ID);

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(IsAny<Message>()), Times.Never);
            Assert.IsFalse(outcome.Success);
        }

        private void startGameSession()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<Message>()))
                .Returns(new GenericOutcome(new GameLiftError()));
            GameSession gameSession = new GameSession { GameSessionId = GAME_SESSION_ID };
            state.ProcessReady(GENERIC_PROCESS_PARAMS);
            state.OnStartGameSession(gameSession);
        }

        [Test]
        public void GIVEN_validRequest_WHEN_describePlayerSessions_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            var initialNextToken = "initialNextToken";
            var initialLimit = 10;
            var capturedRequests = new List<DescribePlayerSessionsRequest>();
            PlayerSession playerSession = new PlayerSession();
            playerSession.PlayerId = PLAYER_ID;
            List<PlayerSession> playerSessions = new List<PlayerSession> { playerSession };

            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests), IsAny<int>()))
                .Returns(
                    new DescribePlayerSessionsOutcome(new DescribePlayerSessionsResult(playerSessions, NEXT_TOKEN)));
            DescribePlayerSessionsRequest request = new DescribePlayerSessionsRequest
            {
                GameSessionId = GAME_SESSION_ID,
                PlayerSessionId = PLAYER_SESSION_ID,
                PlayerId = PLAYER_ID,
                PlayerSessionStatusFilter = PLAYER_SESSION_STATUS_FILTER,
                NextToken = initialNextToken,
                Limit = initialLimit
            };

            // WHEN
            DescribePlayerSessionsOutcome outcome = state.DescribePlayerSessions(request);

            // THEN
            Assert.IsTrue(outcome.Success);
            Assert.AreEqual(NEXT_TOKEN, outcome.Result.NextToken);
            Assert.AreEqual(playerSession, outcome.Result.PlayerSessions[0]);
            Assert.That(capturedRequests, Has.Count.EqualTo(1));
            Assert.AreEqual(capturedRequests.First().Action, MessageActions.DESCRIBE_PLAYER_SESSIONS);
            Assert.AreEqual(capturedRequests.First().GameSessionId, GAME_SESSION_ID);
            Assert.AreEqual(capturedRequests.First().PlayerId, PLAYER_ID);
            Assert.AreEqual(capturedRequests.First().PlayerSessionId, PLAYER_SESSION_ID);
            Assert.AreEqual(capturedRequests.First().PlayerSessionStatusFilter, PLAYER_SESSION_STATUS_FILTER);
            Assert.AreEqual(capturedRequests.First().Limit, initialLimit);
            Assert.AreEqual(capturedRequests.First().NextToken, initialNextToken);
        }

        [Test]
        public void GIVEN_validRequest_AND_noLimitSpecified_WHEN_describePlayerSessions_THEN_defaultLimitUsed()
        {
            // GIVEN
            var capturedRequests = new List<DescribePlayerSessionsRequest>();
            PlayerSession playerSession = new PlayerSession();
            playerSession.PlayerId = PLAYER_ID;
            List<PlayerSession> playerSessions = new List<PlayerSession> { playerSession };

            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests), IsAny<int>()))
                .Returns(
                    new DescribePlayerSessionsOutcome(new DescribePlayerSessionsResult(playerSessions, NEXT_TOKEN)));
            DescribePlayerSessionsRequest request = new DescribePlayerSessionsRequest
            {
                GameSessionId = GAME_SESSION_ID,
                PlayerSessionId = PLAYER_SESSION_ID,
                PlayerId = PLAYER_ID
            };

            // WHEN
            DescribePlayerSessionsOutcome outcome = state.DescribePlayerSessions(request);

            // THEN
            Assert.IsTrue(outcome.Success);
            Assert.AreEqual(NEXT_TOKEN, outcome.Result.NextToken);
            Assert.AreEqual(playerSession, outcome.Result.PlayerSessions[0]);
            Assert.That(capturedRequests, Has.Count.EqualTo(1));
            Assert.AreEqual(capturedRequests.First().Action, MessageActions.DESCRIBE_PLAYER_SESSIONS);
            Assert.AreEqual(capturedRequests.First().GameSessionId, GAME_SESSION_ID);
            Assert.AreEqual(capturedRequests.First().PlayerId, PLAYER_ID);
            Assert.AreEqual(capturedRequests.First().PlayerSessionId, PLAYER_SESSION_ID);
            Assert.AreEqual(capturedRequests.First().Limit, 50);
            Assert.Null(capturedRequests.First().PlayerSessionStatusFilter);
            Assert.Null(capturedRequests.First().NextToken);
        }

        [Test]
        public void GIVEN_validRequestFails_WHEN_describePlayerSessions_THEN_returnsErrorOutcome()
        {
            // GIVEN
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(IsAny<Message>(), IsAny<int>()))
                .Returns(new GenericOutcome(new GameLiftError()));
            DescribePlayerSessionsRequest request = new DescribePlayerSessionsRequest
            {
                GameSessionId = GAME_SESSION_ID,
                PlayerSessionId = PLAYER_SESSION_ID,
                PlayerId = PLAYER_ID
            };

            // WHEN
            DescribePlayerSessionsOutcome outcome = state.DescribePlayerSessions(request);

            // THEN
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void GIVEN_validRequest_WHEN_startMatchBackfill_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            var capturedRequests = new List<StartMatchBackfillRequest>();
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests), IsAny<int>()))
                .Returns(new StartMatchBackfillOutcome(new StartMatchBackfillResult(TICKET_ID)));
            var players = GetPlayers(3);
            StartMatchBackfillRequest request = new StartMatchBackfillRequest(GAME_SESSION_ARN, MATCHMAKER_ARN, players)
            {
                TicketId = TICKET_ID
            };

            // WHEN
            StartMatchBackfillOutcome outcome = state.StartMatchBackfill(request);

            // THEN
            Assert.IsTrue(outcome.Success);
            Assert.AreEqual(TICKET_ID, outcome.Result.TicketId);
            Assert.That(capturedRequests, Has.Count.EqualTo(1));
            Assert.AreEqual(capturedRequests.First().Action, MessageActions.START_MATCH_BACKFILL);
            Assert.AreEqual(capturedRequests.First().TicketId, TICKET_ID);
            Assert.AreEqual(capturedRequests.First().GameSessionArn, GAME_SESSION_ARN);
            Assert.AreEqual(capturedRequests.First().MatchmakingConfigurationArn, MATCHMAKER_ARN);
            Assert.AreEqual(capturedRequests.First().Players, players);
        }

        [Test]
        public void GIVEN_validRequestFails_WHEN_startMatchBackfill_THEN_returnsErrorOutcome()
        {
            // GIVEN
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(IsAny<Message>(), IsAny<int>()))
                .Returns(new GenericOutcome(new GameLiftError()));
            var players = GetPlayers(3);
            StartMatchBackfillRequest request = new StartMatchBackfillRequest(GAME_SESSION_ARN, MATCHMAKER_ARN, players)
            {
                TicketId = TICKET_ID
            };

            // WHEN
            StartMatchBackfillOutcome outcome = state.StartMatchBackfill(request);

            // THEN
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void GIVEN_validRequest_WHEN_stopMatchBackfill_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            var capturedRequests = new List<StopMatchBackfillRequest>();
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests), IsAny<int>()))
                .Returns(new GenericOutcome());
            StopMatchBackfillRequest request = new StopMatchBackfillRequest(GAME_SESSION_ARN, MATCHMAKER_ARN, TICKET_ID);

            // WHEN
            GenericOutcome outcome = state.StopMatchBackfill(request);

            // THEN
            Assert.IsTrue(outcome.Success);
            Assert.That(capturedRequests, Has.Count.EqualTo(1));
            Assert.AreEqual(capturedRequests.First().Action, MessageActions.STOP_MATCH_BACKFILL);
            Assert.AreEqual(capturedRequests.First().TicketId, TICKET_ID);
            Assert.AreEqual(capturedRequests.First().GameSessionArn, GAME_SESSION_ARN);
            Assert.AreEqual(capturedRequests.First().MatchmakingConfigurationArn, MATCHMAKER_ARN);
        }

        [Test]
        public void GIVEN_validRequestFails_WHEN_stopMatchBackfill_THEN_returnsErrorOutcome()
        {
            // GIVEN
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(IsAny<Message>(), IsAny<int>()))
                .Returns(new GenericOutcome(new GameLiftError()));
            StopMatchBackfillRequest request = new StopMatchBackfillRequest(GAME_SESSION_ARN, MATCHMAKER_ARN, TICKET_ID);

            // WHEN
            GenericOutcome outcome = state.StopMatchBackfill(request);

            // THEN
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void GIVEN_healthCheckTrue_WHEN_heartbeatServerProcess_THEN_sendsHeartbeatServerProcessRequest()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<Message>()))
                .Returns(new GenericOutcome(new GameLiftError()));
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<HeartbeatServerProcessRequest>()))
                .Returns(new GenericOutcome());
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<ActivateServerProcessRequest>()))
                .Returns(new GenericOutcome());
            ProcessParameters processParams = new ProcessParameters(
                (gameSession) => { },
                (updateGameSession) => { },
                () => { },
                () => true,
                1234,
                new LogParameters(new List<string> { "C:\\game\\logs", "C:\\game\\error" }));

            // WHEN
            GenericOutcome outcome = state.ProcessReady(processParams);
            Thread.Sleep(1000);

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(Is<ActivateServerProcessRequest>(
                req => req.SdkVersion == "5.0.0" && req.SdkLanguage == "CSharp" && req.Port == PORT_NUMBER &&
                        req.LogPaths.SequenceEqual(LOG_PATHS))), Times.Once);
            mockRequestHandler.Verify(requestHandler => requestHandler.SendRequest(
                Is<HeartbeatServerProcessRequest>(req => req.HealthStatus == true &&
                                               req.Action == MessageActions.HEARTBEAT_SERVER_PROCESS),
                IsAny<int>()), Times.Once);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_healthCheckFalse_WHEN_heartbeatServerProcess_THEN_sendsHeartbeatServerProcessRequest()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<Message>()))
                .Returns(new GenericOutcome(new GameLiftError()));
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<HeartbeatServerProcessRequest>()))
                .Returns(new GenericOutcome());
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<ActivateServerProcessRequest>()))
                .Returns(new GenericOutcome());
            ProcessParameters processParams = new ProcessParameters(
                (gameSession) => { },
                (updateGameSession) => { },
                () => { },
                () => false, // OnHealthCheck
                1234,
                new LogParameters(new List<string>() { "C:\\game\\logs", "C:\\game\\error" }));

            // WHEN
            GenericOutcome outcome = state.ProcessReady(processParams);
            Thread.Sleep(1000);

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(Is<ActivateServerProcessRequest>(
                req => req.SdkVersion == "5.0.0" && req.SdkLanguage == "CSharp" && req.Port == PORT_NUMBER &&
                        req.LogPaths.SequenceEqual(LOG_PATHS))), Times.Once);
            mockRequestHandler.Verify(requestHandler => requestHandler.SendRequest(
                Is<HeartbeatServerProcessRequest>(req => req.HealthStatus == false &&
                                               req.Action == MessageActions.HEARTBEAT_SERVER_PROCESS),
                 IsAny<int>()), Times.Once);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_heartbeatServerProcess_WHEN_waits71Seconds_THEN_reportsHealthTwice()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<Message>()))
                .Returns(new GenericOutcome(new GameLiftError()));
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<HeartbeatServerProcessRequest>()))
                .Returns(new GenericOutcome());
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<ActivateServerProcessRequest>()))
                .Returns(new GenericOutcome());
            ProcessParameters processParams = new ProcessParameters(
                (gameSession) => { },
                (updateGameSession) => { },
                () => { },
                () => true, // OnHealthCheck
                1234,
                new LogParameters(new List<string> { "C:\\game\\logs", "C:\\game\\error" }));

            // WHEN
            GenericOutcome outcome = state.ProcessReady(processParams);
            Console.WriteLine("Sleeping for 71 seconds to ensure 2 heartbeats occur...");
            Thread.Sleep(71000);

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(Is<ActivateServerProcessRequest>(
                req => req.SdkVersion == "5.0.0" && req.SdkLanguage == "CSharp" && req.Port == PORT_NUMBER &&
                        req.LogPaths.SequenceEqual(LOG_PATHS))), Times.Once);
            mockRequestHandler.Verify(requestHandler => requestHandler.SendRequest(
                Is<HeartbeatServerProcessRequest>(req => req.HealthStatus == true &&
                                               req.Action == MessageActions.HEARTBEAT_SERVER_PROCESS),
                IsAny<int>()), Times.Exactly(2));
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_validRequest_WHEN_getComputeCertificate_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(IsAny<Message>(), IsAny<int>()))
                .Returns(new GetComputeCertificateOutcome(new GetComputeCertificateResult(COMPUTE_CERT, HOST_ID)));

            // WHEN
            GenericOutcome outcome = state.GetComputeCertificate();

            // THEN
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_validRequestFails_WHEN_getComputeCertificate_THEN_returnsErrorOutcome()
        {
            // GIVEN
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(IsAny<Message>(), IsAny<int>()))
                .Returns(new GenericOutcome(new GameLiftError()));

            // WHEN
            GenericOutcome outcome = state.GetComputeCertificate();

            // THEN
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void GIVEN_validRequest_WHEN_GetFleetRoleCredentials_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            var result = new GetFleetRoleCredentialsResult("a", "b", "c", "d", "e", DateTime.UtcNow);
            mockRequestHandler.Setup(requestHandler => requestHandler.SendRequest(
                Is<Message>(it => ((GetFleetRoleCredentialsRequest) it).RoleSessionName == $"{FLEET_ID}-{HOST_ID}"),
                IsAny<int>())
            ).Returns(new GetFleetRoleCredentialsOutcome(result));
            var request = new GetFleetRoleCredentialsRequest("Arn");
            var serverParams = new ServerParameters
            {
                fleetId = FLEET_ID,
                hostId = HOST_ID
            };
            mockWebSocket.Setup(it => it.Connect(
                    IsAny<string>(),
                    IsAny<string>(),
                    IsAny<string>(),
                    IsAny<string>(),
                    IsAny<string>()))
                .Returns(new GenericOutcome());
            // Initialize networking sets the fleetId and hostId which are used to make the role session name
            state.InitializeNetworking(serverParams);

            // WHEN
            GenericOutcome outcome = state.GetFleetRoleCredentials(request);

            // THEN
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_validRequest_WHEN_GetFleetRoleCredentialsFails_THEN_returnsErrorOutcome()
        {
            // GIVEN
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(IsAny<Message>(), IsAny<int>()))
                .Returns(new GenericOutcome(new GameLiftError()));
            var request = new GetFleetRoleCredentialsRequest("Arn");

            // WHEN
            GenericOutcome outcome = state.GetFleetRoleCredentials(request);

            // THEN
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void GIVEN_validRequestWithSessionName_WHEN_GetFleetRoleCredentials_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            var result = new GetFleetRoleCredentialsResult("a", "b", "c", "d", "e", DateTime.UtcNow);
            mockRequestHandler.Setup(requestHandler => requestHandler.SendRequest(
                    Is<Message>(it => ((GetFleetRoleCredentialsRequest) it).RoleSessionName == "CustomRoleSessionName"),
                    IsAny<int>())
                ).Returns(new GetFleetRoleCredentialsOutcome(result));
            var request = new GetFleetRoleCredentialsRequest("Arn")
            {
                RoleSessionName = "CustomRoleSessionName"
            };

            // WHEN
            GenericOutcome outcome = state.GetFleetRoleCredentials(request);

            // THEN
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_requestWithLongSessionName_WHEN_GetFleetRoleCredentials_THEN_returnsFailureOutcome()
        {
            // GIVEN
            var request = new GetFleetRoleCredentialsRequest("Arn")
            {
                RoleSessionName = "AVeryLongRoleSessionNameThatWouldIamWouldntAssumeSinceItsOverSixtyFourCharacters"
            };

            // WHEN
            GenericOutcome outcome = state.GetFleetRoleCredentials(request);

            // THEN
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void GIVEN_validRequestWithLongFleetName_WHEN_GetFleetRoleCredentials_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            var result = new GetFleetRoleCredentialsResult("a", "b", "c", "d", "e", DateTime.UtcNow);
            mockRequestHandler.Setup(requestHandler => requestHandler.SendRequest(
                Is<Message>(it => ((GetFleetRoleCredentialsRequest) it).RoleSessionName.Length == 64),
                IsAny<int>())
            ).Returns(new GetFleetRoleCredentialsOutcome(result));
            var request = new GetFleetRoleCredentialsRequest("Arn");
            var serverParams = new ServerParameters
            {
                fleetId = "AVeryLongRoleSessionNameThatWouldIamWouldntAssumeSinceItsOverSixtyFourCharacters",
                hostId = HOST_ID
            };
            mockWebSocket.Setup(it => it.Connect(
                    IsAny<string>(),
                    IsAny<string>(),
                    IsAny<string>(),
                    IsAny<string>(),
                    IsAny<string>()))
                .Returns(new GenericOutcome());
            // Initialize networking sets the fleetId and hostId which are used to make the role session name
            state.InitializeNetworking(serverParams);

            // WHEN
            GenericOutcome outcome = state.GetFleetRoleCredentials(request);

            // THEN
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_validOnPremRequest_WHEN_GetFleetRoleCredentials_THEN_returnsCachedErrorOutcomes()
        {
            // GIVEN
            var result = new GetFleetRoleCredentialsResult("", "", "", "", "", DateTime.UtcNow);
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(IsAny<Message>(), IsAny<int>()))
                .Returns(new GetFleetRoleCredentialsOutcome(result));
            var request = new GetFleetRoleCredentialsRequest("Arn");

            // WHEN
            GenericOutcome outcome1 = state.GetFleetRoleCredentials(request);
            GenericOutcome outcome2 = state.GetFleetRoleCredentials(request);

            // THEN
            Assert.IsFalse(outcome1.Success);
            Assert.IsFalse(outcome2.Success);
            mockRequestHandler.Verify(it => it.SendRequest(IsAny<Message>(), IsAny<int>()), Times.Once);
        }

        [Test]
        public void GIVEN_validRequests_WHEN_GetFleetRoleCredentials_THEN_cachesAndReturnsSuccessOutcomes()
        {
            // GIVEN
            var expiration = DateTime.UtcNow
                .Add(ServerState.INSTANCE_ROLE_CREDENTIAL_TTL_MIN)
                .Add(TimeSpan.FromMinutes(1));
            var result = new GetFleetRoleCredentialsResult("a", "b", "c", "d", "e", expiration);
            mockRequestHandler.Setup(requestHandler => requestHandler.SendRequest(
                Is<Message>(it => ((GetFleetRoleCredentialsRequest) it).RoleSessionName == "CustomRoleSessionName"),
                IsAny<int>())
            ).Returns(new GetFleetRoleCredentialsOutcome(result));
            var request = new GetFleetRoleCredentialsRequest("Arn")
            {
                RoleSessionName = "CustomRoleSessionName"
            };

            // WHEN
            GenericOutcome outcome1 = state.GetFleetRoleCredentials(request);
            GenericOutcome outcome2 = state.GetFleetRoleCredentials(request);

            // THEN
            Assert.IsTrue(outcome1.Success);
            Assert.IsTrue(outcome2.Success);
            mockRequestHandler.Verify(it => it.SendRequest(IsAny<Message>(), IsAny<int>()), Times.Once);
        }

        [Test]
        public void GIVEN_validRequestsAfterTimeout_WHEN_GetFleetRoleCredentials_THEN_doesNotCacheAndReturnsSuccessOutcomes()
        {
            // GIVEN
            var result = new GetFleetRoleCredentialsResult("a", "b", "c", "d", "e", DateTime.UtcNow);
            mockRequestHandler.Setup(requestHandler => requestHandler.SendRequest(
                Is<Message>(it => ((GetFleetRoleCredentialsRequest) it).RoleSessionName == "CustomRoleSessionName"),
                IsAny<int>())
            ).Returns(new GetFleetRoleCredentialsOutcome(result));
            var request = new GetFleetRoleCredentialsRequest("Arn")
            {
                RoleSessionName = "CustomRoleSessionName"
            };

            // WHEN
            GenericOutcome outcome1 = state.GetFleetRoleCredentials(request);
            GenericOutcome outcome2 = state.GetFleetRoleCredentials(request);

            // THEN
            Assert.IsTrue(outcome1.Success);
            Assert.IsTrue(outcome2.Success);
            mockRequestHandler.Verify(it => it.SendRequest(IsAny<Message>(), IsAny<int>()), Times.Exactly(2));
        }

        [Test]
        public void GIVEN_validRequest_WHEN_onSuccessResponse_THEN_doesNothing()
        {
            // GIVEN
            var requestId = "requestId";

            //WHEN
            state.OnSuccessResponse(requestId);
            //THEN
            mockRequestHandler.Verify(it => it.HandleResponse(
                            Is<string>(reqId => reqId == requestId),
                            Is<GenericOutcome>(outcome => outcome.Success)));
        }

        [Test]
        public void GIVEN_nullRequestId_WHEN_onSuccessResponse_THEN_LogsWithoutCall()
        {
            //GIVEN
            string requestId = null;
            
            //WHEN
            state.OnSuccessResponse(requestId);
            
            //THEN
            mockRequestHandler.Verify(it => 
                it.HandleResponse(requestId, new GenericOutcome()),
                Times.Never()
                );
        }
        
        [Test]
        public void GIVEN_error_WHEN_onErrorResponse_THEN_returnsErrorOutcome()
        {
            // Given
            var requestId = "requestId";
            var statusCode = (int) HttpStatusCode.BadRequest;
            var message = "Failure";
            
            // When
            state.OnErrorResponse(requestId, statusCode, message);
            
            // Then
            mockRequestHandler.Verify(it => it.HandleResponse(
                Is<string>(reqId => reqId == requestId), 
                Is<GenericOutcome>(outcome => !outcome.Success)));
        }

        private Player[] GetPlayers(int n)
        {
            Player[] players = new Player[n];

            for (int x = 0; x < n; x++)
            {
                players[x] = GetPlayer(x);
            }

            return players;
        }

        private Player GetPlayer(int playerNum)
        {
            Player player = new Player();
            player.PlayerId = GetPlayerId(playerNum);
            player.Team = GetTeam(playerNum);

            player.LatencyInMS = new Dictionary<string, int>();
            player.LatencyInMS.Add("region1", 100 + playerNum);
            player.LatencyInMS.Add("region2", 120 + playerNum);

            player.PlayerAttributes = new Dictionary<string, AttributeValue>();
            player.PlayerAttributes.Add("string", new AttributeValue("abc" + playerNum));
            player.PlayerAttributes.Add("number", new AttributeValue(200 + playerNum));
            player.PlayerAttributes.Add("stringlist",
                new AttributeValue(new string[] { "string1-" + playerNum, "string2-" + playerNum }));

            Dictionary<string, double> sdm = new Dictionary<string, double>();
            sdm.Add("string1-" + playerNum, 100.0 + playerNum);
            sdm.Add("string2-" + playerNum, 101.0 + playerNum);
            player.PlayerAttributes.Add("stringdoublemap", new AttributeValue(sdm));

            return player;
        }

        private string GetTeam(int playerNum)
        {
            return "Team" + playerNum;
        }

        private string GetPlayerId(int playerNum)
        {
            return "Player" + playerNum;
        }
    }
}
