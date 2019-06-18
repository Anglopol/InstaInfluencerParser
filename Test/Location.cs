using Newtonsoft.Json;

namespace Test
{
    public class Location
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("countOfUsers")] public int CountOfUsers { get; set; }
        [JsonProperty("publicId")] public int Id { get; set; }
        [JsonProperty("audienceFrom")] public string Owner { get; set; }
    }
}