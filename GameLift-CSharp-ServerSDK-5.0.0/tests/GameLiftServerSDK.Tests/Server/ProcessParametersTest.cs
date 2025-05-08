using System;
using System.Collections.Generic;
using Aws.GameLift.Server;
using Aws.GameLift.Server.Model;
using NUnit.Framework;

namespace Aws.GameLift.Tests.Server
{
    [TestFixture]
    public class ProcessParametersTest
    {
        [Test]
        public void GIVEN_noProcessParams_WHEN_Instantiate_THEN_expectedObjectCreated()
        {
            // When
            ProcessParameters processParams = new ProcessParameters();

            // Then
            Assert.AreEqual(processParams.Port, -1);
            Assert.AreEqual(processParams.LogParameters.LogPaths.Count, 0);
            Assert.IsTrue(processParams.OnHealthCheck.Invoke());
            Assert.IsNotNull(processParams.OnStartGameSession);
            Assert.IsNotNull(processParams.OnUpdateGameSession);
            Assert.IsNotNull(processParams.OnProcessTerminate);
        }

        [Test]
        public void GIVEN_processParams_WHEN_Instantiate_THEN_expectedObjectCreated()
        {
            // Given
            ProcessParameters.OnStartGameSessionDelegate onStartGameSession = (GameSession) => { };
            ProcessParameters.OnProcessTerminateDelegate onProcessTerminate = () => { };
            ProcessParameters.OnHealthCheckDelegate onHealthCheck = () => { return false; };
            ProcessParameters.OnUpdateGameSessionDelegate onUpdateGameSession = (UpdateGameSession) => { };
            int port = 4242;

            List<String> inputLogPaths = new List<String>()
            {
                "C:\\game\\logs",
                "C:\\game\\error"
            };
            LogParameters logParams = new LogParameters(inputLogPaths);


            // When
            ProcessParameters processParams = new ProcessParameters(onStartGameSession,
                                                                    onUpdateGameSession,
                                                                    onProcessTerminate,
                                                                    onHealthCheck,
                                                                    port,
                                                                    logParams);

            // Then
            Assert.AreEqual(processParams.OnStartGameSession, onStartGameSession);
            Assert.AreEqual(processParams.OnProcessTerminate, onProcessTerminate);
            Assert.AreEqual(processParams.OnHealthCheck, onHealthCheck);
            Assert.AreEqual(processParams.Port, 4242);
            Assert.AreEqual(processParams.LogParameters.LogPaths.Count, 2);

            Assert.IsFalse(processParams.OnHealthCheck.Invoke());
            processParams.OnStartGameSession.Invoke(new GameSession());
            processParams.OnProcessTerminate.Invoke();
        }
    }
}
