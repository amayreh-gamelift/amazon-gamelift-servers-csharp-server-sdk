using NUnit.Framework;

namespace Aws.GameLift
{
    [TestFixture]
    public class GenericOutcomeTest
    {
        [Test]
        public void defaultOutcomeHasSuccessNoError()
        {
            // Given
            // When
            GenericOutcome outcome = new GenericOutcome();

            // Then
            Assert.AreEqual(outcome.Error, null);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void errorAlwaysCreatesOutcomeWithNoSuccess()
        {
            // Given
            GameLiftError error = new GameLiftError();

            // When
            GenericOutcome outcome = new GenericOutcome(error);

            // Then
            Assert.AreEqual(outcome.Error, error);
            Assert.IsFalse(outcome.Success);
        }
    }
}