using Newtonsoft.Json;

namespace InfluencerInstaParser.Database.ModelView
{
    public class Analysis
    {
        [JsonProperty("name")] public string TargetName { get; set; }
        [JsonProperty("name")] public string Date { get; set; }
        [JsonProperty("name")] public string Id { get; set; }
    }
}