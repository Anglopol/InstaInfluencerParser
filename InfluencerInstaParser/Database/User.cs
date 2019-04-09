using System;
using Newtonsoft.Json;

namespace InfluencerInstaParser.Database
{
    public class User
    {
        private readonly object _setCommentsLocker;
        private readonly object _setLikesLocker;
        private int _comments;
        private int _likes;
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
            _setCommentsLocker = new object();
            _setLikesLocker = new object();
        }

        [JsonProperty("name")] public string Username { get; }

        [JsonProperty("likes")]
        public int Likes
        {
            get => _likes;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                lock (_setLikesLocker)
                {
                    _likes = value;
                }
            }
        }

        [JsonProperty("comments")]
        public int Comments
        {
            get => _comments;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                lock (_setCommentsLocker)
                {
                    _comments = value;
                }
            }
        }

        [JsonProperty("following")] public int Following { get; set; }
        [JsonProperty("followers")] public int Followers { get; set; }
        [JsonProperty("from")] public User From { get; }
        [JsonProperty("communication")] public CommunicationType CommunicationType { get; }
    }
}