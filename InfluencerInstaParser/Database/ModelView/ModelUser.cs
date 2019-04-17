using Newtonsoft.Json;

namespace InfluencerInstaParser.Database.ModelView
{
    public class ModelUser
    {
//        public IList<string> Locations { get; set; }
        [JsonProperty("name")] public string Username { get; set; }
        [JsonProperty("likes")] public int Likes { get; set; }
        [JsonProperty("comments")] public int Comments { get; set; }
        [JsonProperty("following")] public int Following { get; set; }
        [JsonProperty("followers")] public int Followers { get; set; }
    }
}