using Newtonsoft.Json;

namespace InfluencerInstaParser.AudienceParser
{
    public class User
    {
        public User(string username = null, int likes = 0, int comments = 0, int following = 0, int followers = 0,
            User from = null, CommunicationType communicationType = CommunicationType.Target)
        {
            Username = username;
            Likes = likes;
            Comments = comments;
            Following = following;
            Followers = followers;
            From = from;
            CommunicationType = communicationType;
        }

        [JsonProperty("name")] public string Username { get; }

        [JsonProperty("likes")] public int Likes { get; }

        [JsonProperty("comments")] public int Comments { get; }

        [JsonProperty("following")] public int Following { get; }

        [JsonProperty("followers")] public int Followers { get; }

        [JsonProperty("from")] public User From { get; }

        [JsonProperty("communication")] public CommunicationType CommunicationType { get; }
    }
}