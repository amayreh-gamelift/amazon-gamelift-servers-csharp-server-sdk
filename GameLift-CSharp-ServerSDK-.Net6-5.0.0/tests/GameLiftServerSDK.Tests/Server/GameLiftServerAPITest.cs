using NUnit.Framework;
using Aws.GameLift.Server;

namespace Aws.GameLift.Tests.Server
{
    [TestFixture]
    public class GameLiftServerAPITest
    {
        [Test]
        public void GIVEN_validSdkVersion_WHEN_GetSdkVersion_THEN_returnsVersion()
        {
            // Given
            // When
            AwsStringOutcome outcome = GameLiftServerAPI.GetSdkVersion();

            // Then
            Assert.IsTrue(outcome.Success);
            Assert.AreEqual(outcome.Result, "5.0.0");
        }
    }
}
