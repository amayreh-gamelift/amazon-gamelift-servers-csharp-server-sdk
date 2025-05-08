using NUnit.Framework;

namespace Aws.GameLift
{
    [TestFixture]
    public class AwsStringOutcomeTest
    {
        [Test]
        public void resultAlwaysCreatesOutcomeWithSuccessAndNoError()
        {
            // Given
            // When
            AwsStringOutcome outcome = new AwsStringOutcome("result");

            // Then
            Assert.AreEqual(outcome.Result, "result");
            Assert.IsNull(outcome.Error);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void errorAlwaysCreatesOutcomeWithNoSuccessAndNoResult()
        {
            // Given
            GameLiftError error = new GameLiftError();

            // When
            AwsStringOutcome outcome = new AwsStringOutcome(error);

            // Then
            Assert.IsNull(outcome.Result);
            Assert.AreEqual(outcome.Error, error);
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void errorAndResultAlwaysCreatesOutcomeWithNoSuccessAndResult()
        {
            // Given
            GameLiftError error = new GameLiftError();

            // When
            AwsStringOutcome outcome = new AwsStringOutcome(error, "result");

            // Then
            Assert.AreEqual(outcome.Result, "result");
            Assert.AreEqual(outcome.Error, error);
            Assert.IsFalse(outcome.Success);
        }
    }
}