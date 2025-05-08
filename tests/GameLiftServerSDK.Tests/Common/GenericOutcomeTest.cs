using NUnit.Framework;

namespace Aws.GameLift
{
    [TestFixture]
    public class GenericOutcomeTest
    {
        [Test]
        public void DefaultOutcomeHasSuccessNoError()
        {
            // Given
            // When
            GenericOutcome outcome = new GenericOutcome();

            // Then
            Assert.AreEqual(null, outcome.Error);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void ErrorAlwaysCreatesOutcomeWithNoSuccess()
        {
            // Given
            GameLiftError error = new GameLiftError();

            // When
            GenericOutcome outcome = new GenericOutcome(error);

            // Then
            Assert.AreEqual(error, outcome.Error);
            Assert.IsFalse(outcome.Success);
        }
    }
}
