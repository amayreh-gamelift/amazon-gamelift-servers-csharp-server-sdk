using System.Collections.Generic;
using Aws.GameLift.Server;
using NUnit.Framework;

namespace Aws.GameLift.Tests.Server
{
    [TestFixture]
    public class LogParametersTest
    {
        [Test]
        public void GIVEN_noLogParams_WHEN_GetLogPaths_THEN_noLogPathsReturned()
        {
            // Given
            LogParameters logParams = new LogParameters();

            // When
            IList<string> logPaths = logParams.LogPaths;

            // Then
            Assert.AreEqual(0, logPaths.Count);
        }

        [Test]
        public void GIVEN_logParams_WHEN_GetLogPaths_THEN_returnLogPaths()
        {
            // Given
            List<string> inputLogPaths = new List<string>
            {
                "C:\\game\\logs",
                "C:\\game\\error",
            };
            LogParameters logParams = new LogParameters(inputLogPaths);

            // When
            IList<string> outputLogPaths = logParams.LogPaths;

            // Then
            Assert.AreEqual(inputLogPaths, outputLogPaths);
        }
    }
}
