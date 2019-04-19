using Newtonsoft.Json;

namespace InfluencerInstaParser.Database.ModelView
{
    public class Location
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("countOfUsers")] public int CountOfUsers { get; set; }
    }
}