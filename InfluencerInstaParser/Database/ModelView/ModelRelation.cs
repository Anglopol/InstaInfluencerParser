using Newtonsoft.Json;

namespace InfluencerInstaParser.Database.ModelView
{
    public class ModelRelation
    {
        [JsonProperty("likes")] public int Likes { get; set; }
        [JsonProperty("comments")] public int Comments { get; set; }
        [JsonProperty("parent")] public string Parent { get; set; }
        [JsonProperty("child")] public string Child { get; set; }
        [JsonProperty("follower")] public bool Follower { get; set; }
    }
}