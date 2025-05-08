using Aws.GameLift.Server.Model;
using NUnit.Framework;

namespace Aws.GameLift.Tests.Server
{
    [TestFixture]
    public class MatchmakerDataTest
    {
        [Test]
        public void ParseMatchmakerDataTest()
        {
            string json = "{\"matchId\":\"50119f4a-efdd-41db-b2f5-7346e558cab5\"," +
                "\"matchmakingConfigurationArn\":\"arn:aws:gamelift:us-west-2:302776016398:matchmakingconfiguration/MM-ConfName-d1c38523-f992-4894-893f-686a6a6eb8ad\"," +
                // first team
                "\"teams\":[{\"name\":\"team1\",\"players\":[" +
                "{\"playerId\":\"id-test-player-920\",\"attributes\":{" +
                "\"world\":{\"attributeType\":\"DOUBLE\",\"valueAttribute\":1.0}," +
                "\"role\":{\"attributeType\":\"STRING_LIST\",\"valueAttribute\":[\"human\"]}," +
                "\"sdmValue\":{\"attributeType\":\"STRING_DOUBLE_MAP\",\"valueAttribute\":{\"key1\":123.5,\"key2\":12}}," +
                "\"mmr\":{\"attributeType\":\"DOUBLE\",\"valueAttribute\":0.0}," +
                "\"level\":{\"attributeType\":\"DOUBLE\",\"valueAttribute\":1.0}," +
                "\"type\":{\"attributeType\":\"STRING\",\"valueAttribute\":\"TurnBased\"}}}" +
                "]}," +
                // second team
                "{\"name\":\"team2\",\"players\":[" +
                "{\"playerId\":\"id-test-player-114\",\"attributes\":" +
                "{\"world\":{\"attributeType\":\"DOUBLE\",\"valueAttribute\":1.0}," +
                "\"role\":{\"attributeType\":\"STRING_LIST\",\"valueAttribute\":[\"robot\",\"king\"]}," +
                "\"mmr\":{\"attributeType\":\"DOUBLE\",\"valueAttribute\":3.0}," +
                "\"level\":{\"attributeType\":\"DOUBLE\",\"valueAttribute\":4.0}," +
                "\"type\":{\"attributeType\":\"STRING\",\"valueAttribute\":\"TurnBasedMod1\"}}}" +
                "]}" +
                "]}";

            MatchmakerData data = MatchmakerData.FromJson(json);

            Assert.AreEqual("50119f4a-efdd-41db-b2f5-7346e558cab5", data.MatchId);
            Assert.AreEqual("arn:aws:gamelift:us-west-2:302776016398:matchmakingconfiguration/MM-ConfName-d1c38523-f992-4894-893f-686a6a6eb8ad", data.MatchmakingConfigurationArn);
            Assert.AreEqual(2, data.Players.Count);

            Assert.AreEqual("id-test-player-920", data.Players[0].PlayerId);
            Assert.AreEqual("team1", data.Players[0].Team);
            Assert.AreEqual(6, data.Players[0].PlayerAttributes.Count);
            Assert.AreEqual(1.0, data.Players[0].PlayerAttributes["world"].N);
            Assert.AreEqual(1, data.Players[0].PlayerAttributes["role"].SL.Length);
            Assert.AreEqual("human", data.Players[0].PlayerAttributes["role"].SL[0]);
            Assert.AreEqual(2, data.Players[0].PlayerAttributes["sdmValue"].SDM.Count);
            Assert.AreEqual(123.5, data.Players[0].PlayerAttributes["sdmValue"].SDM["key1"]);
            Assert.AreEqual(12, data.Players[0].PlayerAttributes["sdmValue"].SDM["key2"]);
            Assert.AreEqual(0, data.Players[0].PlayerAttributes["mmr"].N);
            Assert.AreEqual(1.0, data.Players[0].PlayerAttributes["level"].N);
            Assert.AreEqual("TurnBased", data.Players[0].PlayerAttributes["type"].S);

            Assert.AreEqual(5, data.Players[1].PlayerAttributes.Count);
            Assert.AreEqual(1.0, data.Players[1].PlayerAttributes["world"].N);
            Assert.AreEqual(2, data.Players[1].PlayerAttributes["role"].SL.Length);
            Assert.AreEqual("robot", data.Players[1].PlayerAttributes["role"].SL[0]);
            Assert.AreEqual("king", data.Players[1].PlayerAttributes["role"].SL[1]);
            Assert.AreEqual(3.0, data.Players[1].PlayerAttributes["mmr"].N);
            Assert.AreEqual(4.0, data.Players[1].PlayerAttributes["level"].N);
            Assert.AreEqual("TurnBasedMod1", data.Players[1].PlayerAttributes["type"].S);
        }
    }
}
