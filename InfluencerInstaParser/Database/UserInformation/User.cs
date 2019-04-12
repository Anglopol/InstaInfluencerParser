using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluencerInstaParser.Database.UserInformation
{
    public class User
    {
        private readonly Dictionary<User, RelationInformation> _relations;
        private readonly object _setCommentsLocker;
        private readonly object _setLikesLocker;
        private int _comments;
        private int _likes;

        public User(string username, User parent, CommunicationType type,
            int likes = 0, int comments = 0, int following = 0, int followers = 0)
        {
            Username = username;
            Likes = likes;
            Comments = comments;
            Following = following;
            Followers = followers;
            Parent = parent;
            _setCommentsLocker = new object();
            _setLikesLocker = new object();
            _relations = new Dictionary<User, RelationInformation> {{parent, new RelationInformation(parent, type)}};
        }

        public User(string username, int likes = 0, int comments = 0, int following = 0, int followers = 0)
        {
            Username = username;
            Likes = likes;
            Comments = comments;
            Following = following;
            Followers = followers;
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
        [JsonProperty("from")] public User Parent { get; }
        [JsonProperty("communication")] public CommunicationType CommunicationType { get; }

        public void AddNewRelation(User parent, CommunicationType type = CommunicationType.Follower)
        {
            if (FindRelation(parent) != null) _relations.Add(parent, new RelationInformation(parent, type));
        }

        public void AddLikesForRelation(User parent, int count) //Todo проверить в нормальном состоянии 
        {
            var relation = FindRelation(parent);
            if (relation == null) AddNewRelation(parent);
            _relations[parent].Likes += count;
        }

        public void AddCommentsForRelation(User parent, int count) //Todo проверить в нормальном состоянии 
        {
            var relation = FindRelation(parent);
            if (relation == null) AddNewRelation(parent);
            _relations[parent].Comments += count;
        }

        private RelationInformation FindRelation(User user)
        {
            return _relations.ContainsKey(user) ? _relations[user] : null;
        }
    }
}