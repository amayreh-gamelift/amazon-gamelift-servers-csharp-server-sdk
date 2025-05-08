using System.Collections.Generic;
using Aws.GameLift.Server.Model;
using NUnit.Framework;

namespace Aws.GameLift
{
    [TestFixture]
    public class DescribePlayerSessionsOutcomeTest
    {
        private static List<PlayerSession> PLAYER_SESSIONS = new List<PlayerSession>();
        private static string NEXT_TOKEN = "nextToken";

        [Test]
        public void GIVEN_describePlayerSessionsResult_WHEN_creatingDescribePlayerSessionsOutCome_THEN_noError()
        {
            // GIVEN
            var result = new DescribePlayerSessionsResult(PLAYER_SESSIONS, NEXT_TOKEN);

            // WHEN
            var outcome = new DescribePlayerSessionsOutcome(result);

            // THEN
            Assert.AreEqual(outcome.Result, result);
            Assert.IsNull(outcome.Error);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_gameLiftError_WHEN_creatingDescribePlayerSessionsOutCome_THEN_setsError()
        {
            // GIVEN
            var error = new GameLiftError();

            // WHEN
            var outcome = new DescribePlayerSessionsOutcome(error);

            // THEN
            Assert.AreEqual(outcome.Error, error);
            Assert.IsNull(outcome.Result);
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void GIVEN_gameLiftErrorAndResult_WHEN_creatingDescribePlayerSessionsOutCome_THEN_setsErrorAndResult()
        {
            // GIVEN
            var error = new GameLiftError();
            var result = new DescribePlayerSessionsResult(PLAYER_SESSIONS, NEXT_TOKEN);

            // WHEN
            var outcome = new DescribePlayerSessionsOutcome(error, result);

            // THEN
            Assert.AreEqual(outcome.Error, error);
            Assert.AreEqual(outcome.Result, result);
            Assert.IsFalse(outcome.Success);
        }
    }
}
