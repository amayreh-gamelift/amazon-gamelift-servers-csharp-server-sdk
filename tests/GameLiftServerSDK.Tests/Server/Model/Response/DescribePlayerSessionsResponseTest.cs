using Aws.GameLift.Server.Model;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Aws.GameLift.Tests.Server.Model
{
    [TestFixture]
    public class DescribePlayerSessionsResponseTest
    {
        [Test]
        public void GIVEN_validStringJsonResponse_WHEN_Deserialize_THEN_deserializesSuccessfully()
        {
            // GIVEN
            string jsonResponse = "{" +
                "\"Action\": \"DescribePlayerSessions\"," +
                "\"RequestId\": \"TestRequestId\"," +
                "\"NextToken\": \"TestNextToken\"," +
                "\"PlayerSessions\": [" +
                    "{" +
                        "\"PlayerId\":\"TestPlayerId\"," +
                        "\"PlayerSessionId\":\"TestPlayerSessionId\"," +
                        "\"GameSessionId\":\"TestGameSessionId\"," +
                        "\"FleetId\":\"TestFleetId\"," +
                        "\"IpAddress\":\"TestIpAddress\"," +
                        "\"PlayerData\":\"TestPlayerData\"," +
                        "\"Port\":12345," +
                        "\"ExtraValue\":\"TestExtraValue\"," +
                        "\"CreationTime\":1665019827030," +
                        "\"TerminationTime\":1665019827030," +
                        "\"Status\":\"TIMEDOUT\"," +
                        "\"DnsName\":\"TestDnsName\"" +
                    "}," +
                    "{" +
                        "\"PlayerId\":\"TestPlayerId\"," +
                        "\"PlayerSessionId\":\"TestPlayerSessionId\"," +
                        "\"GameSessionId\":\"TestGameSessionId\"," +
                        "\"FleetId\":\"TestFleetId\"," +
                        "\"IpAddress\":\"TestIpAddress\"," +
                        "\"PlayerData\":\"TestPlayerData\"," +
                        "\"Port\":12345," +
                        "\"ExtraValue\":\"TestExtraValue\"," +
                        "\"CreationTime\":1665019827030," +
                        "\"TerminationTime\":1665019827030," +
                        "\"Status\":\"ACTIVE\"," +
                        "\"DnsName\":\"TestDnsName\"" +
                    "}" +
                "]" +
            "}";

            // WHEN - JsonConvert is used in GameLiftWebSocket when receiving messages over the websocket
            DescribePlayerSessionsResponse response = JsonConvert.DeserializeObject<DescribePlayerSessionsResponse>(
                    jsonResponse, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            // THEN
            Assert.AreEqual("DescribePlayerSessions", response.Action);
            Assert.AreEqual("TestRequestId", response.RequestId);
            Assert.AreEqual("TestNextToken", response.NextToken);

            Assert.AreEqual(2, response.PlayerSessions.Count);
            foreach (PlayerSession session in response.PlayerSessions)
            {
                Assert.AreEqual("TestPlayerId", session.PlayerId);
                Assert.AreEqual("TestPlayerSessionId", session.PlayerSessionId);
                Assert.AreEqual("TestGameSessionId", session.GameSessionId);
                Assert.AreEqual("TestFleetId", session.FleetId);
                Assert.AreEqual("TestIpAddress", session.IpAddress);
                Assert.AreEqual("TestPlayerData", session.PlayerData);
                Assert.AreEqual(12345, session.Port);
                Assert.AreEqual(1_665_019_827_030, session.CreationTime);
                Assert.AreEqual(1_665_019_827_030, session.TerminationTime);
                Assert.AreEqual("TestDnsName", session.DnsName);
            }

            Assert.AreEqual(PlayerSessionStatus.TIMEDOUT, response.PlayerSessions[0].Status);
            Assert.AreEqual(PlayerSessionStatus.ACTIVE, response.PlayerSessions[1].Status);
        }
    }
}
