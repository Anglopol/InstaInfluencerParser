using Newtonsoft.Json;

namespace InfluencerInstaParser.Database.ModelView
{
    public class Analysis
    {
        [JsonProperty("name")] public string TargetName { get; set; }
        [JsonProperty("date")] public string Date { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
    }
}