using NUnit.Framework;

using Aws.GameLift.Server.Model;

namespace Aws.GameLift.Tests.Server
{
    [TestFixture]
    public class UpdateReasonTest
    {
        [Test]
        public void testMapping()
        {
            validateMapping("MATCHMAKING_DATA_UPDATED");
            validateMapping("BACKFILL_FAILED");
            validateMapping("BACKFILL_TIMED_OUT");
            validateMapping("BACKFILL_CANCELLED");
        }

        private void validateMapping(string name)
        {
            UpdateReason reason = UpdateReasonMapper.GetUpdateReasonForName(name);
            string newName = UpdateReasonMapper.GetNameForUpdateReason(reason);
            Assert.AreEqual(name, newName);
        }
    }
}
