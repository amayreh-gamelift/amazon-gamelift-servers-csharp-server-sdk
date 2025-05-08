using System.Collections.Generic;
using System.Threading.Tasks;
using Aws.GameLift.Server;
using Aws.GameLift.Server.Model;
using Moq;
using NUnit.Framework;

namespace Aws.GameLift.Tests.Server
{
    [TestFixture]
    public class GameLiftWebSocketRequestHandlerTest
    {
        private const string TicketId = "ticketId";
        private const string NextToken = "nextToken";
        private static readonly List<PlayerSession> PlayerSessions = new List<PlayerSession>();

        private Mock<IGameLiftWebSocket> mockWebSocket;
        private GameLiftWebSocketRequestHandler requestHandler;

        [SetUp]
        public void SetUp()
        {
            mockWebSocket = new Mock<IGameLiftWebSocket>();
            requestHandler = new GameLiftWebSocketRequestHandler(mockWebSocket.Object);
        }

        [Test]
        public void GIVEN_duplicateRequest_WHEN_sendRequestWithResponse_THEN_failsSecondGenericOutcome()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.SendMessage(It.IsAny<Message>()))
                .Returns(new GenericOutcome());
            Message request = new Message();

            // WHEN
            Task.Run(async () =>
            {
                await Task.Delay(2000);
                requestHandler.HandleResponse(request.RequestId, new GenericOutcome());
            });
            GenericOutcome outcome1 = requestHandler.SendRequest(request);
            GenericOutcome outcome2 = requestHandler.SendRequest(request);

            // THEN
            Assert.IsTrue(outcome1.Success);
            Assert.IsFalse(outcome2.Success);
        }

        [Test]
        public void GIVEN_request_WHEN_sendRequest_THEN_successGenericOutcome()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.SendMessage(It.IsAny<Message>()))
                .Returns(new GenericOutcome());
            Message request = new Message();

            // WHEN
            Task.Run(async () =>
            {
                await Task.Delay(2000);
                requestHandler.HandleResponse(request.RequestId, new GenericOutcome());
            });
            GenericOutcome outcome = requestHandler.SendRequest(request);

            // THEN
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_requestSendsResponseFails_WHEN_sendRequest_THEN_failureGenericOutcome()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.SendMessage(It.IsAny<Message>()))
                .Returns(new GenericOutcome());
            Message request = new Message();

            // WHEN
            Task.Run(async () =>
            {
                await Task.Delay(2000);
                requestHandler.HandleResponse(request.RequestId, new GenericOutcome(new GameLiftError()));
            });
            GenericOutcome outcome = requestHandler.SendRequest(request);

            // THEN
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void GIVEN_requestSendsResponseTimesout_WHEN_sendRequest_THEN_failureGenericOutcome()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.SendMessage(It.IsAny<Message>()))
                .Returns(new GenericOutcome());
            Message request = new Message();

            // WHEN
            GenericOutcome outcome = requestHandler.SendRequest(request, timeoutMillis: 100);

            // THEN
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(GameLiftErrorType.WEBSOCKET_CONNECT_FAILURE_TIMEOUT, outcome.Error.ErrorType);
        }

        [Test]
        public void
            GIVEN_requestSendsWithStartMatchBackfillOutcome_WHEN_sendRequest_THEN_successStartMatchBackfillOutcome()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.SendMessage(It.IsAny<Message>()))
                .Returns(new GenericOutcome());
            Message request = new Message();

            // WHEN
            Task.Run(async () =>
            {
                await Task.Delay(2000);
                requestHandler.HandleResponse(request.RequestId, new StartMatchBackfillOutcome(new StartMatchBackfillResult(TicketId)));
            });
            StartMatchBackfillOutcome outcome = (StartMatchBackfillOutcome)requestHandler.SendRequest(request);

            // THEN
            Assert.IsTrue(outcome.Success);
            Assert.AreEqual(TicketId, outcome.Result.TicketId);
        }

        [Test]
        public void
            GIVEN_requestSendsWithDescribePlayerSessionsOutcome_WHEN_sendRequest_THEN_successDescribePlayerSessionsOutcome()
        {
            // GIVEN
            mockWebSocket.Setup(websocket => websocket.SendMessage(It.IsAny<Message>()))
                .Returns(new GenericOutcome());
            Message request = new Message();

            // WHEN
            Task.Run(async () =>
            {
                await Task.Delay(2000);
                requestHandler.HandleResponse(request.RequestId, new DescribePlayerSessionsOutcome(new DescribePlayerSessionsResult(PlayerSessions, NextToken)));
            });
            DescribePlayerSessionsOutcome outcome = (DescribePlayerSessionsOutcome)requestHandler.SendRequest(request);

            // THEN
            Assert.IsTrue(outcome.Success);
            Assert.AreEqual(PlayerSessions, outcome.Result.PlayerSessions);
            Assert.AreEqual(NextToken, outcome.Result.NextToken);
        }
    }
}
