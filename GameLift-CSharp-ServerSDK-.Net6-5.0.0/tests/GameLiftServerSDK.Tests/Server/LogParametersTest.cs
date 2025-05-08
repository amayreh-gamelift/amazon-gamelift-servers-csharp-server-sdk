using System;
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
            List<String> logPaths = logParams.LogPaths;

            // Then
            Assert.AreEqual(logPaths.Count, 0);
        }

        [Test]
        public void GIVEN_logParams_WHEN_GetLogPaths_THEN_returnLogPaths()
        {
            // Given
            List<String> inputLogPaths = new List<String>()
            {
                "C:\\game\\logs",
                "C:\\game\\error"
            };
            LogParameters logParams = new LogParameters(inputLogPaths);

            // When
            List<String> outputLogPaths = logParams.LogPaths;

            // Then
            Assert.AreEqual(inputLogPaths, outputLogPaths);
        }
    }
}