using System.Collections.Generic;
using Aws.GameLift.Server.Model;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Aws.GameLift.Tests.Server.Model
{
    [TestFixture]
    public class StartMatchBackfillRequestTest
    {
        // Sanity test to verify this request can get properly serialized;
        // This request is fairly complex compared to our other ones due to the player attributes,
        // so this verifies that the SDK serialization is consistent with what we expect server-side
        [Test]
        public void GIVEN_validRequest_WHEN_SerializeToJson_THEN_returnsExpectedJson()
        {
            // GIVEN
            Dictionary<string, int> latencyMap = new Dictionary<string, int>
            {
                { "region1", 111 },
                { "region2", 222 },
            };

            Dictionary<string, double> stringDoubleMapValue = new Dictionary<string, double>
            {
                { "stringDoubleMapValue1", 1234.1234 },
                { "stringDoubleMapValue2", 4567.4567 },
            };

            Dictionary<string, AttributeValue> completeAttributeMap = new Dictionary<string, AttributeValue>
            {
                { "stringAttribute", new AttributeValue("stringAttributeValue") },
                { "doubleAttribute", new AttributeValue(1234.1234) },
                { "stringListAttribute", new AttributeValue(new[] { "stringListValue1", "stringListValue2", "stringListValue3" }) },
                { "stringDoubleMapAttribute", new AttributeValue(stringDoubleMapValue) },
            };

            Player player1 = new Player
            {
                PlayerId = "Player1",
                Team = "Team1",
                LatencyInMS = latencyMap,
                PlayerAttributes = completeAttributeMap,
            };

            Player player2 = new Player
            {
                PlayerId = "Player2",
                Team = "Team2",
                LatencyInMS = latencyMap,
                PlayerAttributes = completeAttributeMap,
            };

            string gameSessionArn = "TestGameSessionArn";
            string matchmakingConfigArn = "TestMatchmakingConfigArn";
            StartMatchBackfillRequest request =
                    new StartMatchBackfillRequest(gameSessionArn, matchmakingConfigArn, new[] { player1, player2 });
            request.TicketId = "TestTicketId";

            // WHEN - JsonConvert is used by GameLiftWebSocket.SendMessage when sending the request over the websocket
            string serializedRequest = JsonConvert.SerializeObject(request);

            // THEN
#pragma warning disable S103
            string expected = "{" +
                    "\"GameSessionArn\":\"TestGameSessionArn\"," +
                    "\"MatchmakingConfigurationArn\":\"TestMatchmakingConfigArn\"," +
                    "\"Players\":[" +
                         "{\"PlayerId\":\"Player1\"," +
                         "\"PlayerAttributes\":{" +
                            "\"stringAttribute\":{\"attrType\":\"STRING\",\"S\":\"stringAttributeValue\",\"N\":0.0,\"SL\":null,\"SDM\":null}," +
                            "\"doubleAttribute\":{\"attrType\":\"DOUBLE\",\"S\":null,\"N\":1234.1234,\"SL\":null,\"SDM\":null}," +
                            "\"stringListAttribute\":{\"attrType\":\"STRING_LIST\",\"S\":null,\"N\":0.0,\"SL\":[\"stringListValue1\",\"stringListValue2\",\"stringListValue3\"],\"SDM\":null}," +
                            "\"stringDoubleMapAttribute\":{\"attrType\":\"STRING_DOUBLE_MAP\",\"S\":null,\"N\":0.0,\"SL\":null,\"SDM\":{\"stringDoubleMapValue1\":1234.1234,\"stringDoubleMapValue2\":4567.4567}}" +
                         "}," +
                         "\"Team\":\"Team1\"," +
                         "\"LatencyInMS\":{\"region1\":111,\"region2\":222}}," +

                         "{\"PlayerId\":\"Player2\"," +
                         "\"PlayerAttributes\":{" +
                            "\"stringAttribute\":{\"attrType\":\"STRING\",\"S\":\"stringAttributeValue\",\"N\":0.0,\"SL\":null,\"SDM\":null}," +
                            "\"doubleAttribute\":{\"attrType\":\"DOUBLE\",\"S\":null,\"N\":1234.1234,\"SL\":null,\"SDM\":null}," +
                            "\"stringListAttribute\":{\"attrType\":\"STRING_LIST\",\"S\":null,\"N\":0.0,\"SL\":[\"stringListValue1\",\"stringListValue2\",\"stringListValue3\"],\"SDM\":null}," +
                            "\"stringDoubleMapAttribute\":{\"attrType\":\"STRING_DOUBLE_MAP\",\"S\":null,\"N\":0.0,\"SL\":null,\"SDM\":{\"stringDoubleMapValue1\":1234.1234,\"stringDoubleMapValue2\":4567.4567}}" +
                         "}," +
                         "\"Team\":\"Team2\"," +
                         "\"LatencyInMS\":{\"region1\":111,\"region2\":222}}" +
                    "]," +
                    "\"TicketId\":\"TestTicketId\"," +
                    "\"Action\":\"StartMatchBackfill\"," +
                    "\"RequestId\":\"" + request.RequestId + "\"" +
            "}";
#pragma warning restore S103
            Assert.AreEqual(expected, serializedRequest);
        }
    }
}
