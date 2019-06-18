using System.Collections.Generic;
using Newtonsoft.Json;

namespace Test
{
    public class ModelUser
    {
        [JsonProperty("locations")] public List<string> Locations { get; set; }
        [JsonProperty("name")] public string Username { get; set; }
        [JsonProperty("parents")] public List<string> Parents { get; set; }
        [JsonProperty("likes")] public int Likes { get; set; }
        [JsonProperty("comments")] public int Comments { get; set; }
        [JsonProperty("following")] public int Following { get; set; }
        [JsonProperty("followers")] public int Followers { get; set; }
        [JsonProperty("influencer")] public bool IsInfluencer { get; set; }
    }
}