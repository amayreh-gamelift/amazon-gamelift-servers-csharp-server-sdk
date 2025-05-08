using Aws.GameLift.Server;
using NUnit.Framework;

namespace Aws.GameLift.Tests.Server
{
    [TestFixture]
    public class GameLiftServerApiTest
    {
        [Test]
        public void GIVEN_validSdkVersion_WHEN_GetSdkVersion_THEN_returnsVersion()
        {
            // Given
            // When
            AwsStringOutcome outcome = GameLiftServerAPI.GetSdkVersion();

            // Then
            Assert.IsTrue(outcome.Success);
            Assert.AreEqual("5.2.1", outcome.Result);
        }
    }
}
