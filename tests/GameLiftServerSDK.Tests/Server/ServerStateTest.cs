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
        private const string SdkVersion = "5.1.2";
        private const string EnvironmentVariableWebsocketUrl = "GAMELIFT_SDK_WEBSOCKET_URL";
        private const string EnvironmentVariableProcessId = "GAMELIFT_SDK_PROCESS_ID";
        private const string EnvironmentVariableHostId = "GAMELIFT_SDK_HOST_ID";
        private const string EnvironmentVariableFleetId = "GAMELIFT_SDK_FLEET_ID";
        private const string EnvironmentVariableAuthToken = "GAMELIFT_SDK_AUTH_TOKEN";
        private const string WebsocketUrl = "webSocketUrl";
        private const string ProcessId = "processId";
        private const string HostId = "hostId";
        private const string FleetId = "fleetId";
        private const string PlayerSessionId = "playerSessionId";
        private const string PlayerSessionStatusFilter = "playerSessionStatusFilter";
        private const string PlayerId = "playerId";
        private const string GameSessionId = "gameSessionId";
        private const string AuthToken = "authToken";
        private const string ComputeCert = "computeCert";
        private const int PortNumber = 1234;
        private const string NextToken = "nextToken";
        private const string TicketId = "SomeTicket";
        private const string GameSessionArn = "TODO_SOME_ARN";
        private const string MatchmakerArn = "TODO_SOME_MATCHMAKER_ARN";

        private static readonly List<string> LogPaths = new List<string> { "C:\\game\\logs", "C:\\game\\error" };

        private static readonly ProcessParameters GenericProcessParams = new ProcessParameters(
            (gameSession) => { },
            (updateGameSession) => { },
            () => { },
            () => { return false; },
            PortNumber,
            new LogParameters(LogPaths));

        private readonly IDictionary<string, string> oldEnvironmentVariables = new Dictionary<string, string>();

        private Mock<IGameLiftWebSocket> mockWebSocket;
        private Mock<GameLiftWebSocketRequestHandler> mockRequestHandler;
        private ServerState state;

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
            mockWebSocket.Setup(websocket => websocket.Connect(
                IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>())).Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.InitializeNetworking(new ServerParameters(WebsocketUrl, ProcessId, HostId, FleetId, AuthToken));

            // THEN
            mockWebSocket.Verify(websocket => websocket.Connect(WebsocketUrl, ProcessId, HostId, FleetId, AuthToken));
            AssertSuccessOutcome(outcome);
        }

        [Test]
        public void GIVEN_nullWebsocketUrl_WHEN_initializeNetworking_THEN_returnsErrorOutcome()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.Connect(
                IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>())).Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.InitializeNetworking(new ServerParameters(null, ProcessId, HostId, FleetId, AuthToken));

            // THEN
            AssertInitializeNetworkingFailure(outcome);
        }

        [Test]
        public void GIVEN_nullProcessId_WHEN_initializeNetworking_THEN_returnsErrorOutcome()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.Connect(
                IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>())).Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.InitializeNetworking(new ServerParameters(WebsocketUrl, null, HostId, FleetId, AuthToken));

            // THEN
            AssertInitializeNetworkingFailure(outcome);
        }

        [Test]
        public void GIVEN_nullHostId_WHEN_initializeNetworking_THEN_returnsErrorOutcome()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.Connect(
                IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>())).Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.InitializeNetworking(new ServerParameters(WebsocketUrl, ProcessId, null, FleetId, AuthToken));

            // THEN
            AssertInitializeNetworkingFailure(outcome);
        }

        [Test]
        public void GIVEN_nullFleetId_WHEN_initializeNetworking_THEN_returnsErrorOutcome()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.Connect(
                IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>())).Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.InitializeNetworking(new ServerParameters(WebsocketUrl, ProcessId, HostId, null, AuthToken));

            // THEN
            AssertInitializeNetworkingFailure(outcome);
        }

        [Test]
        public void GIVEN_nullAuthToken_WHEN_initializeNetworking_THEN_returnsErrorOutcome()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.Connect(
                IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>())).Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.InitializeNetworking(new ServerParameters(WebsocketUrl, ProcessId, HostId, FleetId, null));

            // THEN
            AssertInitializeNetworkingFailure(outcome);
        }

        private void AssertInitializeNetworkingFailure(GenericOutcome outcome)
        {
            mockWebSocket.Verify(websocket => websocket.Connect(
                IsAny<String>(), IsAny<String>(), IsAny<String>(), IsAny<String>(), IsAny<String>()), Times.Never);
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(GenericOutcome));
            Assert.AreEqual(GameLiftErrorType.BAD_REQUEST_EXCEPTION, outcome.Error.ErrorType);
        }

        [Test]
        public void GIVEN_defaultServerParameters_WHEN_initializeNetworking_THEN_returnsSuccessOutcome()
        {
            const string websocketUrlOverride = "newWebsocketUrl";
            const string processIdOverride = "newProcessId";
            const string hostIdOverride = "newHostId";
            const string fleetIdOverride = "newFleetId";
            const string authTokenOverride = "newAuthToken";

            UsingEnvironmentVariables(
                new Dictionary<string, string>
                {
                    [EnvironmentVariableWebsocketUrl] = websocketUrlOverride,
                    [EnvironmentVariableProcessId] = processIdOverride,
                    [EnvironmentVariableHostId] = hostIdOverride,
                    [EnvironmentVariableFleetId] = fleetIdOverride,
                    [EnvironmentVariableAuthToken] = authTokenOverride,
                },
                () =>
                {
                    // GIVEN
                    mockWebSocket.Setup(websocket => websocket.Connect(
                        IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>())).Returns(new GenericOutcome());

                    // WHEN
                    GenericOutcome outcome = state.InitializeNetworking(default);

                    // THEN
                    mockWebSocket.Verify(websocket => websocket.Connect(
                        websocketUrlOverride,
                        processIdOverride,
                        hostIdOverride,
                        fleetIdOverride,
                        authTokenOverride));
                    AssertSuccessOutcome(outcome);
                });
        }

        [Test]
        public void GIVEN_serverParameters_WHEN_initializeNetworkingWithEnvVars_THEN_returnsSuccessOutcome()
        {
            const string websocketUrlOverride = "newWebsocketUrl";
            const string processIdOverride = "newProcessId";
            const string hostIdOverride = "newHostId";
            const string fleetIdOverride = "newFleetId";
            const string authTokenOverride = "newAuthToken";

            UsingEnvironmentVariables(
                new Dictionary<string, string>
                {
                    [EnvironmentVariableWebsocketUrl] = websocketUrlOverride,
                    [EnvironmentVariableProcessId] = processIdOverride,
                    [EnvironmentVariableHostId] = hostIdOverride,
                    [EnvironmentVariableFleetId] = fleetIdOverride,
                    [EnvironmentVariableAuthToken] = authTokenOverride,
                },
                () =>
                {
                    // GIVEN
                    mockWebSocket.Setup(websocket => websocket.Connect(
                        IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>())).Returns(new GenericOutcome());

                    // WHEN
                    GenericOutcome outcome = state.InitializeNetworking(new ServerParameters(WebsocketUrl, ProcessId, HostId, FleetId, AuthToken));

                    // THEN
                    mockWebSocket.Verify(websocket => websocket.Connect(
                        websocketUrlOverride,
                        processIdOverride,
                        hostIdOverride,
                        fleetIdOverride,
                        authTokenOverride));
                    AssertSuccessOutcome(outcome);
                });
        }

        [Test]
        public void GIVEN_refreshParameters_WHEN_onRefreshConnection_THEN_callsRefreshCallback()
        {
            // GIVEN
            const string newAuthToken = "newAuthToken";
            const string newWebSocketUrl = "newWebsocketUrl";
            mockWebSocket.Setup(websocket => websocket.Connect(
                IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>(), IsAny<string>())).Returns(new GenericOutcome());
            state.InitializeNetworking(new ServerParameters(WebsocketUrl, ProcessId, HostId, FleetId, AuthToken));
            state.ProcessReady(GenericProcessParams);

            // WHEN
            state.OnRefreshConnection(newWebSocketUrl, newAuthToken);

            // THEN
            mockWebSocket.Verify(websocket => websocket.Connect(WebsocketUrl, ProcessId, HostId, FleetId, AuthToken));
            mockWebSocket.Verify(websocket => websocket.Connect(newWebSocketUrl, ProcessId, HostId, FleetId, newAuthToken));
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
                IsAny<string>()))
                .Returns(new GenericOutcome());
            mockWebSocket.Setup(websocket => websocket.Connect(
                Is<string>(it => it == newWebSocketUrl),
                IsAny<string>(),
                IsAny<string>(),
                IsAny<string>(),
                Is<string>(it => it == newAuthToken)))
                .Returns(new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION)));
            state.InitializeNetworking(new ServerParameters(WebsocketUrl, ProcessId, HostId, FleetId, AuthToken));
            state.ProcessReady(GenericProcessParams);

            // WHEN
            state.OnRefreshConnection(newWebSocketUrl, newAuthToken);

            // THEN
            mockWebSocket.Verify(websocket => websocket.Connect(WebsocketUrl, ProcessId, HostId, FleetId, AuthToken));
            mockWebSocket.Verify(websocket => websocket.Connect(newWebSocketUrl, ProcessId, HostId, FleetId, newAuthToken));
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
            GenericOutcome outcome = state.ProcessReady(GenericProcessParams);

            // THEN
            mockWebSocket.Verify(
                websocket => websocket.SendMessage(Is<ActivateServerProcessRequest>(
                    req => req.SdkVersion == SdkVersion && req.SdkLanguage == "CSharp" && req.Port == PortNumber && req.LogPaths == LogPaths)), Times.Once);
            AssertSuccessOutcome(outcome);
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
            AssertSuccessOutcome(outcome);
        }

        [Test]
        public void GIVEN_gameSessionStarted_WHEN_activateGameSession_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            StartGameSession();

            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<ActivateGameSessionRequest>()))
                .Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.ActivateGameSession();

            // THEN
            mockWebSocket.Verify(
                websocket => websocket.SendMessage(Is<ActivateGameSessionRequest>(
                    req => req.GameSessionId == GameSessionId)), Times.Once);
            AssertSuccessOutcome(outcome);
        }

        [Test]
        public void GIVEN_gameSessionStarted_WHEN_updatePlayerSessionCreationPolicy_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            StartGameSession();
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<UpdatePlayerSessionCreationPolicyRequest>()))
                .Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.UpdatePlayerSessionCreationPolicy(PlayerSessionCreationPolicy.ACCEPT_ALL);

            // THEN
            mockWebSocket.Verify(
                websocket => websocket.SendMessage(Is<UpdatePlayerSessionCreationPolicyRequest>(
                    req => req.PlayerSessionPolicy == PlayerSessionCreationPolicy.ACCEPT_ALL.ToString() && req.GameSessionId == GameSessionId)), Times.Once);
            AssertSuccessOutcome(outcome);
        }

        [Test]
        public void GIVEN_gameSessionStarted_WHEN_acceptPlayerSession_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            StartGameSession();

            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<AcceptPlayerSessionRequest>()))
                .Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.AcceptPlayerSession(PlayerSessionId);

            // THEN
            mockWebSocket.Verify(
                websocket => websocket.SendMessage(Is<AcceptPlayerSessionRequest>(
                    req => req.GameSessionId == GameSessionId && req.PlayerSessionId == PlayerSessionId)),
                Times.Once);
            AssertSuccessOutcome(outcome);
        }

        [Test]
        public void GIVEN_gameSessionStarted_WHEN_removePlayerSession_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            StartGameSession();

            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<RemovePlayerSessionRequest>()))
                .Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.RemovePlayerSession(PlayerSessionId);

            // THEN
            mockWebSocket.Verify(
                websocket => websocket.SendMessage(Is<RemovePlayerSessionRequest>(
                    req => req.GameSessionId == GameSessionId && req.PlayerSessionId == PlayerSessionId)),
                Times.Once);
            AssertSuccessOutcome(outcome);
        }

        [Test]
        public void GIVEN_gameSessionNotStarted_WHEN_activateGameSession_THEN_returnsFailedOutcome()
        {
            // GIVEN/WHEN
            GenericOutcome outcome = state.ActivateGameSession();

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(IsAny<Message>()), Times.Never);
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(GenericOutcome));
            Assert.AreEqual(GameLiftErrorType.GAMESESSION_ID_NOT_SET, outcome.Error.ErrorType);
        }

        [Test]
        public void GIVEN_gameSessionNotStarted_WHEN_getGameSession_THEN_returnsFailedOutcome()
        {
            // GIVEN/WHEN
            GenericOutcome outcome = state.GetGameSessionId();

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(IsAny<Message>()), Times.Never);
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(AwsStringOutcome));
            Assert.AreEqual(GameLiftErrorType.GAMESESSION_ID_NOT_SET, outcome.Error.ErrorType);
        }

        [Test]
        public void GIVEN_gameSessionNotStarted_WHEN_updatePlayerSessionCreationPolicy_THEN_returnsFailedOutcome()
        {
            // GIVEN/WHEN
            GenericOutcome outcome = state.UpdatePlayerSessionCreationPolicy(PlayerSessionCreationPolicy.ACCEPT_ALL);

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(IsAny<Message>()), Times.Never);
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(GenericOutcome));
            Assert.AreEqual(GameLiftErrorType.GAMESESSION_ID_NOT_SET, outcome.Error.ErrorType);
        }

        [Test]
        public void GIVEN_gameSessionNotStarted_WHEN_acceptPlayerSession_THEN_returnsFailedOutcome()
        {
            // GIVEN/WHEN
            GenericOutcome outcome = state.AcceptPlayerSession(PlayerSessionId);

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(IsAny<Message>()), Times.Never);
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(GenericOutcome));
            Assert.AreEqual(GameLiftErrorType.GAMESESSION_ID_NOT_SET, outcome.Error.ErrorType);
        }

        [Test]
        public void GIVEN_gameSessionNotStarted_WHEN_removePlayerSession_THEN_returnsFailedOutcome()
        {
            // GIVEN/WHEN
            GenericOutcome outcome = state.RemovePlayerSession(PlayerSessionId);

            // THEN
            mockWebSocket.Verify(websocket => websocket.SendMessage(IsAny<Message>()), Times.Never);
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(GenericOutcome));
            Assert.AreEqual(GameLiftErrorType.GAMESESSION_ID_NOT_SET, outcome.Error.ErrorType);
        }

        private void StartGameSession()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.SendMessage(IsAny<Message>()))
                .Returns(new GenericOutcome(new GameLiftError()));
            GameSession gameSession = new GameSession { GameSessionId = GameSessionId };
            state.ProcessReady(GenericProcessParams);
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
            playerSession.PlayerId = PlayerId;
            List<PlayerSession> playerSessions = new List<PlayerSession> { playerSession };

            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests)))
                .Returns(
                    new DescribePlayerSessionsOutcome(new DescribePlayerSessionsResult(playerSessions, NextToken)));
            DescribePlayerSessionsRequest request = new DescribePlayerSessionsRequest
            {
                GameSessionId = GameSessionId,
                PlayerSessionId = PlayerSessionId,
                PlayerId = PlayerId,
                PlayerSessionStatusFilter = PlayerSessionStatusFilter,
                NextToken = initialNextToken,
                Limit = initialLimit,
            };

            // WHEN
            DescribePlayerSessionsOutcome outcome = state.DescribePlayerSessions(request);

            // THEN
            AssertSuccessOutcome(outcome);
            Assert.AreEqual(NextToken, outcome.Result.NextToken);
            Assert.AreEqual(playerSession, outcome.Result.PlayerSessions[0]);
            Assert.That(capturedRequests, Has.Count.EqualTo(1));
            Assert.AreEqual(MessageActions.DescribePlayerSessions, capturedRequests.First().Action);
            Assert.AreEqual(GameSessionId, capturedRequests.First().GameSessionId);
            Assert.AreEqual(PlayerId, capturedRequests.First().PlayerId);
            Assert.AreEqual(PlayerSessionId, capturedRequests.First().PlayerSessionId);
            Assert.AreEqual(PlayerSessionStatusFilter, capturedRequests.First().PlayerSessionStatusFilter);
            Assert.AreEqual(initialLimit, capturedRequests.First().Limit);
            Assert.AreEqual(initialNextToken, capturedRequests.First().NextToken);
        }

        [Test]
        public void GIVEN_validRequest_AND_noLimitSpecified_WHEN_describePlayerSessions_THEN_defaultLimitUsed()
        {
            // GIVEN
            var capturedRequests = new List<DescribePlayerSessionsRequest>();
            PlayerSession playerSession = new PlayerSession();
            playerSession.PlayerId = PlayerId;
            List<PlayerSession> playerSessions = new List<PlayerSession> { playerSession };

            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests)))
                .Returns(
                    new DescribePlayerSessionsOutcome(new DescribePlayerSessionsResult(playerSessions, NextToken)));
            DescribePlayerSessionsRequest request = new DescribePlayerSessionsRequest
            {
                GameSessionId = GameSessionId,
                PlayerSessionId = PlayerSessionId,
                PlayerId = PlayerId,
            };

            // WHEN
            DescribePlayerSessionsOutcome outcome = state.DescribePlayerSessions(request);

            // THEN
            AssertSuccessOutcome(outcome);
            Assert.AreEqual(NextToken, outcome.Result.NextToken);
            Assert.AreEqual(playerSession, outcome.Result.PlayerSessions[0]);
            Assert.That(capturedRequests, Has.Count.EqualTo(1));
            Assert.AreEqual(MessageActions.DescribePlayerSessions, capturedRequests.First().Action);
            Assert.AreEqual(GameSessionId, capturedRequests.First().GameSessionId);
            Assert.AreEqual(PlayerId, capturedRequests.First().PlayerId);
            Assert.AreEqual(PlayerSessionId, capturedRequests.First().PlayerSessionId);
            Assert.AreEqual(50, capturedRequests.First().Limit);
            Assert.Null(capturedRequests.First().PlayerSessionStatusFilter);
            Assert.Null(capturedRequests.First().NextToken);
        }

        [Test]
        public void GIVEN_validRequestFails_WHEN_describePlayerSessions_THEN_returnsErrorOutcome()
        {
            // GIVEN
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(IsAny<Message>()))
                .Returns(new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION)));
            DescribePlayerSessionsRequest request = new DescribePlayerSessionsRequest
            {
                GameSessionId = GameSessionId,
                PlayerSessionId = PlayerSessionId,
                PlayerId = PlayerId,
            };

            // WHEN
            DescribePlayerSessionsOutcome outcome = state.DescribePlayerSessions(request);

            // THEN
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(DescribePlayerSessionsOutcome));
            Assert.AreEqual(GameLiftErrorType.BAD_REQUEST_EXCEPTION, outcome.Error.ErrorType);
        }

        [Test]
        public void GIVEN_validRequest_WHEN_startMatchBackfill_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            var capturedRequests = new List<StartMatchBackfillRequest>();
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests)))
                .Returns(new StartMatchBackfillOutcome(new StartMatchBackfillResult(TicketId)));
            var players = GetPlayers(3);
            StartMatchBackfillRequest request = new StartMatchBackfillRequest(GameSessionArn, MatchmakerArn, players)
            {
                TicketId = TicketId,
            };

            // WHEN
            StartMatchBackfillOutcome outcome = state.StartMatchBackfill(request);

            // THEN
            AssertSuccessOutcome(outcome);
            Assert.AreEqual(TicketId, outcome.Result.TicketId);
            Assert.That(capturedRequests, Has.Count.EqualTo(1));
            Assert.AreEqual(MessageActions.StartMatchBackfill, capturedRequests.First().Action);
            Assert.AreEqual(TicketId, capturedRequests.First().TicketId);
            Assert.AreEqual(GameSessionArn, capturedRequests.First().GameSessionArn);
            Assert.AreEqual(MatchmakerArn, capturedRequests.First().MatchmakingConfigurationArn);
            Assert.AreEqual(players, capturedRequests.First().Players);
        }

        [Test]
        public void GIVEN_nullRequest_WHEN_startMatchBackfill_THEN_returnsErrorOutcome()
        {
            // GIVEN
            var capturedRequests = new List<StartMatchBackfillRequest>();
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests)))
                .Returns(new StartMatchBackfillOutcome(new StartMatchBackfillResult(TicketId)));

            // WHEN
            StartMatchBackfillOutcome outcome = state.StartMatchBackfill(null);

            // THEN
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(StartMatchBackfillOutcome));
            Assert.AreEqual(GameLiftErrorType.BAD_REQUEST_EXCEPTION, outcome.Error.ErrorType);
            mockRequestHandler.Verify(requestHandler => requestHandler.SendRequest(It.IsAny<Message>()), Times.Never());
            Assert.That(capturedRequests, Has.Count.EqualTo(0));
        }

        [Test]
        public void GIVEN_request_nullGameSessionArn_WHEN_startMatchBackfill_THEN_returnsErrorOutcome()
        {
            // GIVEN
            var capturedRequests = new List<StartMatchBackfillRequest>();
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests)))
                .Returns(new StartMatchBackfillOutcome(new StartMatchBackfillResult(TicketId)));

            var players = GetPlayers(3);

            StartMatchBackfillRequest request = new StartMatchBackfillRequest(null, MatchmakerArn, players)
            {
                TicketId = TicketId,
            };

            // WHEN
            StartMatchBackfillOutcome outcome = state.StartMatchBackfill(request);

            // THEN
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(StartMatchBackfillOutcome));
            Assert.AreEqual(GameLiftErrorType.BAD_REQUEST_EXCEPTION, outcome.Error.ErrorType);
            mockRequestHandler.Verify(requestHandler => requestHandler.SendRequest(It.IsAny<Message>()), Times.Never());
            Assert.That(capturedRequests, Has.Count.EqualTo(0));
        }

        [Test]
        public void GIVEN_request_nullMatchmakingConfigurationArn_WHEN_startMatchBackfill_THEN_returnsErrorOutcome()
        {
            // GIVEN
            var capturedRequests = new List<StartMatchBackfillRequest>();
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests)))
                .Returns(new StartMatchBackfillOutcome(new StartMatchBackfillResult(TicketId)));

            var players = GetPlayers(3);

            StartMatchBackfillRequest request = new StartMatchBackfillRequest(GameSessionArn, null, players)
            {
                TicketId = TicketId,
            };

            // WHEN
            StartMatchBackfillOutcome outcome = state.StartMatchBackfill(request);

            // THEN
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(StartMatchBackfillOutcome));
            Assert.AreEqual(GameLiftErrorType.BAD_REQUEST_EXCEPTION, outcome.Error.ErrorType);
            mockRequestHandler.Verify(requestHandler => requestHandler.SendRequest(It.IsAny<Message>()), Times.Never());
            Assert.That(capturedRequests, Has.Count.EqualTo(0));
        }

        [Test]
        public void GIVEN_request_nullTicketId_WHEN_startMatchBackfill_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            var capturedRequests = new List<StartMatchBackfillRequest>();
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests)))
                .Returns(new StartMatchBackfillOutcome(new StartMatchBackfillResult(TicketId)));

            var players = GetPlayers(3);

            StartMatchBackfillRequest request = new StartMatchBackfillRequest(GameSessionArn, MatchmakerArn, players)
            {
                TicketId = null,
            };

            // WHEN
            StartMatchBackfillOutcome outcome = state.StartMatchBackfill(request);

            // THEN
            AssertSuccessOutcome(outcome);
            Assert.AreEqual(TicketId, outcome.Result.TicketId);
            Assert.That(capturedRequests, Has.Count.EqualTo(1));
            Assert.AreEqual(MessageActions.StartMatchBackfill, capturedRequests.First().Action);
            Assert.IsNull(capturedRequests.First().TicketId);
            Assert.AreEqual(GameSessionArn, capturedRequests.First().GameSessionArn);
            Assert.AreEqual(MatchmakerArn, capturedRequests.First().MatchmakingConfigurationArn);
            Assert.AreEqual(players, capturedRequests.First().Players);
        }

        [Test]
        public void GIVEN_validRequestFails_WHEN_startMatchBackfill_THEN_returnsErrorOutcome()
        {
            // GIVEN
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(IsAny<Message>()))
                .Returns(new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION)));
            var players = GetPlayers(3);
            StartMatchBackfillRequest request = new StartMatchBackfillRequest(GameSessionArn, MatchmakerArn, players)
            {
                TicketId = TicketId,
            };

            // WHEN
            StartMatchBackfillOutcome outcome = state.StartMatchBackfill(request);

            // THEN
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(StartMatchBackfillOutcome));
            Assert.AreEqual(GameLiftErrorType.BAD_REQUEST_EXCEPTION, outcome.Error.ErrorType);
        }

        [Test]
        public void GIVEN_validRequest_WHEN_stopMatchBackfill_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            var capturedRequests = new List<StopMatchBackfillRequest>();
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests)))
                .Returns(new GenericOutcome());
            StopMatchBackfillRequest request = new StopMatchBackfillRequest(GameSessionArn, MatchmakerArn, TicketId);

            // WHEN
            GenericOutcome outcome = state.StopMatchBackfill(request);

            // THEN
            AssertSuccessOutcome(outcome);
            Assert.That(capturedRequests, Has.Count.EqualTo(1));
            Assert.AreEqual(MessageActions.StopMatchBackfill, capturedRequests.First().Action);
            Assert.AreEqual(TicketId, capturedRequests.First().TicketId);
            Assert.AreEqual(GameSessionArn, capturedRequests.First().GameSessionArn);
            Assert.AreEqual(MatchmakerArn, capturedRequests.First().MatchmakingConfigurationArn);
        }

        [Test]
        public void GIVEN_validRequestFails_WHEN_stopMatchBackfill_THEN_returnsErrorOutcome()
        {
            // GIVEN
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(IsAny<Message>()))
                .Returns(new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION)));
            StopMatchBackfillRequest request = new StopMatchBackfillRequest(GameSessionArn, MatchmakerArn, TicketId);

            // WHEN
            GenericOutcome outcome = state.StopMatchBackfill(request);

            // THEN
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(GenericOutcome));
            Assert.AreEqual(GameLiftErrorType.BAD_REQUEST_EXCEPTION, outcome.Error.ErrorType);
        }

        [Test]
        public void GIVEN_nullRequest_WHEN_stopMatchBackfill_THEN_returnsErrorOutcome()
        {
            // GIVEN
            var capturedRequests = new List<StopMatchBackfillRequest>();
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests)))
                .Returns(new GenericOutcome());

            // WHEN
            GenericOutcome outcome = state.StopMatchBackfill(null);

            // THEN
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(GenericOutcome));
            Assert.AreEqual(GameLiftErrorType.BAD_REQUEST_EXCEPTION, outcome.Error.ErrorType);
            mockRequestHandler.Verify(requestHandler => requestHandler.SendRequest(It.IsAny<Message>()), Times.Never());
            Assert.That(capturedRequests, Has.Count.EqualTo(0));
        }

        [Test]
        public void GIVEN_request_nullGameSessionArn_WHEN_stopMatchBackfill_THEN_returnsErrorOutcome()
        {
            // GIVEN
            var capturedRequests = new List<StopMatchBackfillRequest>();
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests)))
                .Returns(new GenericOutcome());
            StopMatchBackfillRequest request = new StopMatchBackfillRequest(null, MatchmakerArn, TicketId);

            // WHEN
            GenericOutcome outcome = state.StopMatchBackfill(request);

            // THEN
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(GenericOutcome));
            Assert.AreEqual(GameLiftErrorType.BAD_REQUEST_EXCEPTION, outcome.Error.ErrorType);
            mockRequestHandler.Verify(requestHandler => requestHandler.SendRequest(It.IsAny<Message>()), Times.Never());
            Assert.That(capturedRequests, Has.Count.EqualTo(0));
        }

        [Test]
        public void GIVEN_request_nullMatchmakingConfigurationArn_WHEN_stopMatchBackfill_THEN_returnsErrorOutcome()
        {
            // GIVEN
            var capturedRequests = new List<StopMatchBackfillRequest>();
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests)))
                .Returns(new GenericOutcome());
            StopMatchBackfillRequest request = new StopMatchBackfillRequest(GameSessionArn, null, TicketId);

            // WHEN
            GenericOutcome outcome = state.StopMatchBackfill(request);

            // THEN
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(GenericOutcome));
            Assert.AreEqual(GameLiftErrorType.BAD_REQUEST_EXCEPTION, outcome.Error.ErrorType);
            mockRequestHandler.Verify(requestHandler => requestHandler.SendRequest(It.IsAny<Message>()), Times.Never());
            Assert.That(capturedRequests, Has.Count.EqualTo(0));
        }

        [Test]
        public void GIVEN_request_nullTicketId_WHEN_stopMatchBackfill_THEN_returnsErrorOutcome()
        {
            // GIVEN
            var capturedRequests = new List<StopMatchBackfillRequest>();
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(Capture.In(capturedRequests)))
                .Returns(new GenericOutcome());
            StopMatchBackfillRequest request = new StopMatchBackfillRequest(GameSessionArn, MatchmakerArn, null);

            // WHEN
            GenericOutcome outcome = state.StopMatchBackfill(request);

            // THEN
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(GenericOutcome));
            Assert.AreEqual(GameLiftErrorType.BAD_REQUEST_EXCEPTION, outcome.Error.ErrorType);
            mockRequestHandler.Verify(requestHandler => requestHandler.SendRequest(It.IsAny<Message>()), Times.Never());
            Assert.That(capturedRequests, Has.Count.EqualTo(0));
        }

        [Test]
        public void GIVEN_onHealthCheckReturnsTrue_WHEN_ProcessReady_THEN_reportAsHealthy()
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
            
            // ProcessReady sends heartbeat messages asynchronously, in a separate thread. Need to delay before verifying mocks.
            Thread.Sleep(5000);

            // THEN
            mockWebSocket.Verify(
                websocket => websocket.SendMessage(
                    Is<ActivateServerProcessRequest>(req => req.SdkVersion == SdkVersion && req.SdkLanguage == "CSharp" && req.Port == PortNumber && req.LogPaths.SequenceEqual(LogPaths))), Times.Once);
            mockRequestHandler.Verify(
                requestHandler => requestHandler.SendRequest(
                    Is<HeartbeatServerProcessRequest>(req => req.HealthStatus && req.Action == MessageActions.HeartbeatServerProcess)), Times.Once);
            AssertSuccessOutcome(outcome);
        }

        [Test]
        public void GIVEN_onHealthCheckReturnsFalse_WHEN_ProcessReady_THEN_reportAsUnhealthy()
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
                new LogParameters(new List<string> { "C:\\game\\logs", "C:\\game\\error" }));

            // WHEN
            GenericOutcome outcome = state.ProcessReady(processParams);
            
            // ProcessReady sends heartbeat messages asynchronously, in a separate thread. Need to delay before verifying mocks.
            Thread.Sleep(5000);

            // THEN
            mockWebSocket.Verify(
                websocket => websocket.SendMessage(
                    Is<ActivateServerProcessRequest>(req => req.SdkVersion == SdkVersion && req.SdkLanguage == "CSharp" && req.Port == PortNumber && req.LogPaths.SequenceEqual(LogPaths))), Times.Once);
            mockRequestHandler.Verify(
                requestHandler => requestHandler.SendRequest(
                    Is<HeartbeatServerProcessRequest>(req => !req.HealthStatus && req.Action == MessageActions.HeartbeatServerProcess)), Times.Once);
            AssertSuccessOutcome(outcome);
        }

        [Test]
        public void GIVEN_onHealthCheckTimesOut_WHEN_ProcessReady_THEN_reportAsUnhealthy()
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
                () =>
                {
                    Thread.Sleep(70000);
                    return true;
                },
                1234,
                new LogParameters(new List<string> { "C:\\game\\logs", "C:\\game\\error" }));

            // WHEN
            GenericOutcome outcome = state.ProcessReady(processParams);
            Console.WriteLine("Sleeping for 60 seconds to ensure onHealthCheck() times out...");
            Thread.Sleep(60000);

            // THEN
            mockWebSocket.Verify(
                websocket => websocket.SendMessage(
                    Is<ActivateServerProcessRequest>(req => req.SdkVersion == SdkVersion && req.SdkLanguage == "CSharp" && req.Port == PortNumber && req.LogPaths.SequenceEqual(LogPaths))), Times.Once);
            mockRequestHandler.Verify(
                requestHandler => requestHandler.SendRequest(
                    Is<HeartbeatServerProcessRequest>(req => !req.HealthStatus && req.Action == MessageActions.HeartbeatServerProcess)), Times.Once);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_wait71Seconds_WHEN_ProcessReady_THEN_heartbeatTwice()
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
            mockWebSocket.Verify(
                websocket => websocket.SendMessage(
                    Is<ActivateServerProcessRequest>(req => req.SdkVersion == SdkVersion && req.SdkLanguage == "CSharp" && req.Port == PortNumber && req.LogPaths.SequenceEqual(LogPaths))), Times.Once);
            mockRequestHandler.Verify(
                requestHandler => requestHandler.SendRequest(
                    Is<HeartbeatServerProcessRequest>(req => req.HealthStatus && req.Action == MessageActions.HeartbeatServerProcess)), Times.Exactly(2));
            AssertSuccessOutcome(outcome);
        }

        [Test]
        public void GIVEN_validRequest_WHEN_getComputeCertificate_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(IsAny<Message>()))
                .Returns(new GetComputeCertificateOutcome(new GetComputeCertificateResult(ComputeCert, HostId)));

            // WHEN
            GenericOutcome outcome = state.GetComputeCertificate();

            // THEN
            AssertSuccessOutcome(outcome);
        }

        [Test]
        public void GIVEN_validRequestFails_WHEN_getComputeCertificate_THEN_returnsErrorOutcome()
        {
            // GIVEN
            mockRequestHandler.Setup(requestHandler =>
                    requestHandler.SendRequest(IsAny<Message>()))
                .Returns(new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION)));

            // WHEN
            GenericOutcome outcome = state.GetComputeCertificate();

            // THEN
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(GetComputeCertificateOutcome));
            Assert.AreEqual(GameLiftErrorType.BAD_REQUEST_EXCEPTION, outcome.Error.ErrorType);
        }

        [Test]
        public void GIVEN_validRequest_WHEN_GetFleetRoleCredentials_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            var result = new GetFleetRoleCredentialsResult("a", "b", "c", "d", "e", DateTime.UtcNow);
            mockRequestHandler.Setup(requestHandler => requestHandler.SendRequest(
                Is<Message>(it => ((GetFleetRoleCredentialsRequest)it).RoleSessionName == $"{FleetId}-{HostId}")))
                .Returns(new GetFleetRoleCredentialsOutcome(result));
            var request = new GetFleetRoleCredentialsRequest("Arn");
            var serverParams = new ServerParameters
            {
                FleetId = FleetId,
                HostId = HostId,
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
            AssertSuccessOutcome(outcome);
        }

        [Test]
        public void GIVEN_validRequest_WHEN_GetFleetRoleCredentialsFails_THEN_returnsErrorOutcome()
        {
            // GIVEN
            mockRequestHandler.Setup(requestHandler => requestHandler.SendRequest(IsAny<Message>()))
                .Returns(new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION)));
            var request = new GetFleetRoleCredentialsRequest("Arn");

            // WHEN
            GenericOutcome outcome = state.GetFleetRoleCredentials(request);

            // THEN
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(GetFleetRoleCredentialsOutcome));
            Assert.AreEqual(GameLiftErrorType.BAD_REQUEST_EXCEPTION, outcome.Error.ErrorType);
        }

        [Test]
        public void GIVEN_validRequestWithSessionName_WHEN_GetFleetRoleCredentials_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            var result = new GetFleetRoleCredentialsResult("a", "b", "c", "d", "e", DateTime.UtcNow);
            mockRequestHandler.Setup(requestHandler => requestHandler.SendRequest(
                    Is<Message>(it => ((GetFleetRoleCredentialsRequest)it).RoleSessionName == "CustomRoleSessionName")))
                .Returns(new GetFleetRoleCredentialsOutcome(result));
            var request = new GetFleetRoleCredentialsRequest("Arn")
            {
                RoleSessionName = "CustomRoleSessionName",
            };

            // WHEN
            GenericOutcome outcome = state.GetFleetRoleCredentials(request);

            // THEN
            AssertSuccessOutcome(outcome);
        }

        [Test]
        public void GIVEN_requestWithLongSessionName_WHEN_GetFleetRoleCredentials_THEN_returnsFailureOutcome()
        {
            // GIVEN
            var request = new GetFleetRoleCredentialsRequest("Arn")
            {
                RoleSessionName = "AVeryLongRoleSessionNameThatWouldIamWouldntAssumeSinceItsOverSixtyFourCharacters",
            };

            // WHEN
            GenericOutcome outcome = state.GetFleetRoleCredentials(request);

            // THEN
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(outcome.GetType(), typeof(GetFleetRoleCredentialsOutcome));
            Assert.AreEqual(GameLiftErrorType.BAD_REQUEST_EXCEPTION, outcome.Error.ErrorType);
        }

        [Test]
        public void GIVEN_validRequestWithLongFleetName_WHEN_GetFleetRoleCredentials_THEN_returnsSuccessOutcome()
        {
            // GIVEN
            var result = new GetFleetRoleCredentialsResult("a", "b", "c", "d", "e", DateTime.UtcNow);
            mockRequestHandler.Setup(requestHandler => requestHandler.SendRequest(
                Is<Message>(it => ((GetFleetRoleCredentialsRequest)it).RoleSessionName.Length == 64)))
                .Returns(new GetFleetRoleCredentialsOutcome(result));
            var request = new GetFleetRoleCredentialsRequest("Arn");
            var serverParams = new ServerParameters
            {
                FleetId = "AVeryLongRoleSessionNameThatWouldIamWouldntAssumeSinceItsOverSixtyFourCharacters",
                HostId = HostId,
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
            AssertSuccessOutcome(outcome);
        }

        [Test]
        public void GIVEN_validOnPremRequest_WHEN_GetFleetRoleCredentials_THEN_returnsCachedErrorOutcomes()
        {
            // GIVEN
            var result = new GetFleetRoleCredentialsResult(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.UtcNow);
            mockRequestHandler.Setup(requestHandler => requestHandler.SendRequest(IsAny<Message>()))
                .Returns(new GetFleetRoleCredentialsOutcome(result));
            var request = new GetFleetRoleCredentialsRequest("Arn");

            // WHEN
            GenericOutcome outcome1 = state.GetFleetRoleCredentials(request);
            GenericOutcome outcome2 = state.GetFleetRoleCredentials(request);

            // THEN
            Assert.IsFalse(outcome1.Success);
            Assert.IsFalse(outcome2.Success);
            mockRequestHandler.Verify(it => it.SendRequest(IsAny<Message>()), Times.Once);
        }

        [Test]
        public void GIVEN_validRequests_WHEN_GetFleetRoleCredentials_THEN_cachesAndReturnsSuccessOutcomes()
        {
            // GIVEN
            var expiration = DateTime.UtcNow
                .Add(ServerState.InstanceRoleCredentialTtlMin)
                .Add(TimeSpan.FromMinutes(1));
            var result = new GetFleetRoleCredentialsResult("a", "b", "c", "d", "e", expiration);
            mockRequestHandler.Setup(requestHandler => requestHandler.SendRequest(
                Is<Message>(it => ((GetFleetRoleCredentialsRequest)it).RoleSessionName == "CustomRoleSessionName")))
                .Returns(new GetFleetRoleCredentialsOutcome(result));
            var request = new GetFleetRoleCredentialsRequest("Arn")
            {
                RoleSessionName = "CustomRoleSessionName",
            };

            // WHEN
            GenericOutcome outcome1 = state.GetFleetRoleCredentials(request);
            GenericOutcome outcome2 = state.GetFleetRoleCredentials(request);

            // THEN
            Assert.IsTrue(outcome1.Success);
            Assert.IsTrue(outcome2.Success);
            mockRequestHandler.Verify(it => it.SendRequest(IsAny<Message>()), Times.Once);
        }

        [Test]
        public void GIVEN_validRequestsAfterTimeout_WHEN_GetFleetRoleCredentials_THEN_doesNotCacheAndReturnsSuccessOutcomes()
        {
            // GIVEN
            var result = new GetFleetRoleCredentialsResult("a", "b", "c", "d", "e", DateTime.UtcNow);
            mockRequestHandler.Setup(requestHandler => requestHandler.SendRequest(
                Is<Message>(it => ((GetFleetRoleCredentialsRequest)it).RoleSessionName == "CustomRoleSessionName")))
                .Returns(new GetFleetRoleCredentialsOutcome(result));
            var request = new GetFleetRoleCredentialsRequest("Arn")
            {
                RoleSessionName = "CustomRoleSessionName",
            };

            // WHEN
            GenericOutcome outcome1 = state.GetFleetRoleCredentials(request);
            GenericOutcome outcome2 = state.GetFleetRoleCredentials(request);

            // THEN
            Assert.IsTrue(outcome1.Success);
            Assert.IsTrue(outcome2.Success);
            mockRequestHandler.Verify(it => it.SendRequest(IsAny<Message>()), Times.Exactly(2));
        }

        [Test]
        public void GIVEN_validRequest_WHEN_onSuccessResponse_THEN_doesNothing()
        {
            // GIVEN
            var requestId = "requestId";

            // WHEN
            state.OnSuccessResponse(requestId);
            // THEN
            mockRequestHandler.Verify(it => it.HandleResponse(
                            Is<string>(reqId => reqId == requestId),
                            Is<GenericOutcome>(outcome => outcome.Success)));
        }

        [Test]
        public void GIVEN_nullRequestId_WHEN_onSuccessResponse_THEN_LogsWithoutCall()
        {
            // GIVEN
            string requestId = null;

            // WHEN
            state.OnSuccessResponse(requestId);

            // THEN
            mockRequestHandler.Verify(it => it.HandleResponse(requestId, new GenericOutcome()), Times.Never());
        }

        [Test]
        public void GIVEN_error_WHEN_onErrorResponse_THEN_returnsErrorOutcome()
        {
            // Given
            var requestId = "requestId";
            var statusCode = (int)HttpStatusCode.BadRequest;
            var message = "Failure";

            // When
            state.OnErrorResponse(requestId, statusCode, message);

            // Then
            mockRequestHandler.Verify(it => it.HandleResponse(
                Is<string>(reqId => reqId == requestId),
                Is<GenericOutcome>(outcome => !outcome.Success)));
        }

        private static Player[] GetPlayers(int n)
        {
            Player[] players = new Player[n];

            for (int x = 0; x < n; x++)
            {
                players[x] = GetPlayer(x);
            }

            return players;
        }

        private static Player GetPlayer(int playerNum)
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
            player.PlayerAttributes.Add("stringlist", new AttributeValue(new[] { "string1-" + playerNum, "string2-" + playerNum }));

            Dictionary<string, double> sdm = new Dictionary<string, double>();
            sdm.Add("string1-" + playerNum, 100.0 + playerNum);
            sdm.Add("string2-" + playerNum, 101.0 + playerNum);
            player.PlayerAttributes.Add("stringdoublemap", new AttributeValue(sdm));

            return player;
        }

        private static string GetTeam(int playerNum)
        {
            return "Team" + playerNum;
        }

        private static string GetPlayerId(int playerNum)
        {
            return "Player" + playerNum;
        }

        private static void AssertSuccessOutcome(GenericOutcome outcome)
        {
            Assert.IsTrue(outcome.Success);
            Assert.IsNull(outcome.Error);
        }
    }
}
