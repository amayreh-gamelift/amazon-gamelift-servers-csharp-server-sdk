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
            Assert.AreEqual(-1, processParams.Port);
            Assert.AreEqual(0, processParams.LogParameters.LogPaths.Count);
            Assert.IsTrue(processParams.OnHealthCheck.Invoke());
            Assert.IsNotNull(processParams.OnStartGameSession);
            Assert.IsNotNull(processParams.OnUpdateGameSession);
            Assert.IsNotNull(processParams.OnProcessTerminate);
        }

        [Test]
        public void GIVEN_processParams_WHEN_Instantiate_THEN_expectedObjectCreated()
        {
            // Given
            ProcessParameters.OnStartGameSessionDelegate onStartGameSession = (gameSession) => { };
            ProcessParameters.OnProcessTerminateDelegate onProcessTerminate = () => { };
            ProcessParameters.OnHealthCheckDelegate onHealthCheck = () => { return false; };
            ProcessParameters.OnUpdateGameSessionDelegate onUpdateGameSession = (updateGameSession) => { };
            int port = 4242;

            List<string> inputLogPaths = new List<string>
            {
                "C:\\game\\logs",
                "C:\\game\\error",
            };
            LogParameters logParams = new LogParameters(inputLogPaths);

            // When
            ProcessParameters processParams = new ProcessParameters(
                onStartGameSession,
                onUpdateGameSession,
                onProcessTerminate,
                onHealthCheck,
                port,
                logParams);

            // Then
            Assert.AreEqual(onStartGameSession, processParams.OnStartGameSession);
            Assert.AreEqual(onProcessTerminate, processParams.OnProcessTerminate);
            Assert.AreEqual(onHealthCheck, processParams.OnHealthCheck);
            Assert.AreEqual(4242, processParams.Port);
            Assert.AreEqual(2, processParams.LogParameters.LogPaths.Count);

            Assert.IsFalse(processParams.OnHealthCheck.Invoke());
            processParams.OnStartGameSession.Invoke(new GameSession());
            processParams.OnProcessTerminate.Invoke();
        }
    }
}
