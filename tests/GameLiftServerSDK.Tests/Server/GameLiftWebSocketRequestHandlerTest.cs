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
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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

        /**
         * This test is designed to verify whether the SDK can handle many simultaneous pending requests.
         * The test currently has issues when numberOfRequests > 45 .
         */
        [Test]
        public async Task GIVEN_manyDescribePlayerSessionsRequests_WHEN_sendRequest_THEN_successAllRequestsDescribePlayerSessionsOutcome()
        {
            // GIVEN
            const int numberOfRequests = 100;

            if (Environment.Version.Major == 4 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Set the thread pool size to the same size as request count to resolve timeout issue in .NET 4 on Windows.
                Console.WriteLine(
                    "Setting thread pool minimum threads to {0} for {1} Net{2} Platform",
                    numberOfRequests,
                    OSPlatform.Windows, Environment.Version.Major);
                ThreadPool.SetMinThreads(numberOfRequests, numberOfRequests);
            }

            var describeSessionsResult = new DescribePlayerSessionsResult(PlayerSessions, NextToken);
            var outcome = new DescribePlayerSessionsOutcome(describeSessionsResult);

            // Setup mock with more efficient callback
            mockWebSocket.Setup(websocket => websocket.SendMessage(It.IsAny<Message>()))
                .Callback<Message>(request =>
                    requestHandler.HandleResponse(request.RequestId, outcome))
                .Returns(new GenericOutcome());

            // WHEN
            var tasks = new Task[numberOfRequests];
            for (var i = 0; i < numberOfRequests; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    var result = requestHandler.SendRequest(new Message());
                    Assert.IsTrue(result.Success);
                });
            }

            // THEN
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

    }
}
