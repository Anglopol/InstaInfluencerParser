using Newtonsoft.Json;

namespace InfluencerInstaParser.Database.ModelView
{
    public class LocationRelationInformation
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("parent")] public string Parent { get; set; }
        [JsonProperty("child")] public string Child { get; set; }
        [JsonProperty("count")] public int Count { get; set; }
        [JsonProperty("date")] public string DateOfParsing { get; set; }
        [JsonProperty("Id")] public string Id { get; set; }
    }
}