using Aws.GameLift.Server.Model;
using NUnit.Framework;

namespace Aws.GameLift
{
    [TestFixture]
    public class GetInstanceCertificateOutcomeTest
    {
        [Test]
        public void GIVEN_getInstanceCertificateResult_WHEN_creatingGetInstanceCertificateOutcome_THEN_noError()
        {
            // GIVEN
            var result = new GetInstanceCertificateResult();

            // WHEN
            var outcome = new GetInstanceCertificateOutcome(result);

            // THEN
            Assert.AreEqual(outcome.Result, result);
            Assert.IsNull(outcome.Error);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_gameLiftError_WHEN_creatingGetInstanceCertificateOutcome_THEN_setsError()
        {
            // GIVEN
            var error = new GameLiftError();

            // WHEN
            var outcome = new GetInstanceCertificateOutcome(error);

            // THEN
            Assert.AreEqual(outcome.Error, error);
            Assert.IsNull(outcome.Result);
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void GIVEN_gameLiftErrorAndResult_WHEN_creatingGetInstanceCertificateOutcome_THEN_setsErrorAndResult()
        {
            // GIVEN
            var error = new GameLiftError();
            var result = new GetInstanceCertificateResult();

            // WHEN
            var outcome = new GetInstanceCertificateOutcome(error, result);

            // THEN
            Assert.AreEqual(outcome.Error, error);
            Assert.AreEqual(outcome.Result, result);
            Assert.IsFalse(outcome.Success);
        }
    }
}