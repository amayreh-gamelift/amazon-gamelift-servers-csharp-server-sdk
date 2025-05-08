using Aws.GameLift.Server.Model;
using NUnit.Framework;

namespace Aws.GameLift.Tests.Server
{
    [TestFixture]
    public class UpdateReasonTest
    {
        [Test]
        public void TestMapping()
        {
            ValidateMapping("MATCHMAKING_DATA_UPDATED");
            ValidateMapping("BACKFILL_FAILED");
            ValidateMapping("BACKFILL_TIMED_OUT");
            ValidateMapping("BACKFILL_CANCELLED");
        }

        private static void ValidateMapping(string name)
        {
            UpdateReason reason = UpdateReasonMapper.GetUpdateReasonForName(name);
            string newName = UpdateReasonMapper.GetNameForUpdateReason(reason);
            Assert.AreEqual(name, newName);
        }
    }
}
