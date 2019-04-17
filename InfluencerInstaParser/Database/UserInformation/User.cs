using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluencerInstaParser.Database.UserInformation
{
    public class User
    {
        private readonly object _setCommentsLocker;
        private readonly object _setLikesLocker;
        private readonly object _relationLocker;
        private int _comments;
        private int _likes;
        public Dictionary<string, int> Locations { get; }
        public Dictionary<User, RelationInformation> Relations { get; }

        public User(string username, User parent, CommunicationType type,
            int likes = 0, int comments = 0, int following = 0, int followers = 0)
        {
            _setCommentsLocker = new object();
            _setLikesLocker = new object();
            _relationLocker = new object();
            Locations = new Dictionary<string, int>();
            Username = username;
            Likes = likes;
            Comments = comments;
            Following = following;
            Followers = followers;
            Parent = parent;
            switch (type)
            {
                case CommunicationType.Liker:
                    Relations = new Dictionary<User, RelationInformation>
                        {{parent, new RelationInformation(parent.Username, Username, type, likes: 1)}};
                    break;
                case CommunicationType.Commentator:
                    Relations = new Dictionary<User, RelationInformation>
                        {{parent, new RelationInformation(parent.Username, Username, type, comments: 1)}};
                    break;
                default:
                    Relations = new Dictionary<User, RelationInformation>
                        {{parent, new RelationInformation(parent.Username, Username, type)}};
                    break;
            }
        }

        public User(string username, int likes = 0, int comments = 0, int following = 0, int followers = 0)
        {
            _setCommentsLocker = new object();
            _setLikesLocker = new object();
            Relations = new Dictionary<User, RelationInformation>();
            Locations = new Dictionary<string, int>();
            Username = username;
            Likes = likes;
            Comments = comments;
            Following = following;
            Followers = followers;
            CommunicationType = CommunicationType.Target;
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
        [JsonProperty("from")] public User Parent { get; }
        [JsonProperty("communication")] public CommunicationType CommunicationType { get; }

        public void AddNewRelation(User parent, CommunicationType type = CommunicationType.Follower)
        {
            lock (_relationLocker)
            {
                Relations.TryAdd(parent, new RelationInformation(parent.Username, Username, type));
            }
        }

        public void AddLikesForRelation(User parent, int count = 1)
        {
            lock (_relationLocker)
            {
                if (!Relations.TryAdd(parent,
                    new RelationInformation(parent.Username, Username, CommunicationType.Liker, count)))
                    Relations[Parent].Likes++;
            }
        }

        public void AddCommentsForRelation(User parent, int count = 1)
        {
            lock (_relationLocker)
            {
                if (!Relations.TryAdd(parent,
                    new RelationInformation(parent.Username, Username,
                        CommunicationType.Commentator, comments: count)))
                    Relations[Parent].Comments++;
            }
        }

        public void AddLocation(string location)
        {
            if (!Locations.TryAdd(location, 1)) Locations[location]++;
        }
    }
}