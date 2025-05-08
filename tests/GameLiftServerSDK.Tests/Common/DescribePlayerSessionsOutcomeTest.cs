using System.Collections.Generic;
using Aws.GameLift.Server.Model;
using NUnit.Framework;

namespace Aws.GameLift
{
    [TestFixture]
    public class DescribePlayerSessionsOutcomeTest
    {
        private const string NextToken = "nextToken";
        private static readonly List<PlayerSession> PlayerSessions = new List<PlayerSession>();

        [Test]
        public void GIVEN_describePlayerSessionsResult_WHEN_creatingDescribePlayerSessionsOutCome_THEN_noError()
        {
            // GIVEN
            var result = new DescribePlayerSessionsResult(PlayerSessions, NextToken);

            // WHEN
            var outcome = new DescribePlayerSessionsOutcome(result);

            // THEN
            Assert.AreEqual(result, outcome.Result);
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
            Assert.AreEqual(error, outcome.Error);
            Assert.IsNull(outcome.Result);
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void GIVEN_gameLiftErrorAndResult_WHEN_creatingDescribePlayerSessionsOutCome_THEN_setsErrorAndResult()
        {
            // GIVEN
            var error = new GameLiftError();
            var result = new DescribePlayerSessionsResult(PlayerSessions, NextToken);

            // WHEN
            var outcome = new DescribePlayerSessionsOutcome(error, result);

            // THEN
            Assert.AreEqual(error, outcome.Error);
            Assert.AreEqual(result, outcome.Result);
            Assert.IsFalse(outcome.Success);
        }
    }
}
