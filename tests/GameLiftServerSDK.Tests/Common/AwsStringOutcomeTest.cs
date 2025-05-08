using NUnit.Framework;

namespace Aws.GameLift
{
    [TestFixture]
    public class AwsStringOutcomeTest
    {
        [Test]
        public void ResultAlwaysCreatesOutcomeWithSuccessAndNoError()
        {
            // Given
            // When
            AwsStringOutcome outcome = new AwsStringOutcome("result");

            // Then
            Assert.AreEqual("result", outcome.Result);
            Assert.IsNull(outcome.Error);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void ErrorAlwaysCreatesOutcomeWithNoSuccessAndNoResult()
        {
            // Given
            GameLiftError error = new GameLiftError();

            // When
            AwsStringOutcome outcome = new AwsStringOutcome(error);

            // Then
            Assert.IsNull(outcome.Result);
            Assert.AreEqual(error, outcome.Error);
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void ErrorAndResultAlwaysCreatesOutcomeWithNoSuccessAndResult()
        {
            // Given
            GameLiftError error = new GameLiftError();

            // When
            AwsStringOutcome outcome = new AwsStringOutcome(error, "result");

            // Then
            Assert.AreEqual("result", outcome.Result);
            Assert.AreEqual(error, outcome.Error);
            Assert.IsFalse(outcome.Success);
        }
    }
}
