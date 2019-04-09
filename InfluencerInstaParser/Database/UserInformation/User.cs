using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace InfluencerInstaParser.Database.UserInformation
{
    public class User
    {
        private readonly HashSet<RelationInformation> _relations;
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
            _relations = new HashSet<RelationInformation> {new RelationInformation(parent, type)};
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

        public void AddNewRelation(User parent, CommunicationType type)
        {
            if (_relations.) _relations.Add(new RelationInformation(parent, type));
        }

        public void AddLikesForRelation(User parent, int count)
        {
            var relation = FindRelation(parent);
            if (relation == null) throw new
        }

        private RelationInformation FindRelation(User user)
        {
            var relationInformatics =
                (from relation in _relations.AsEnumerable() where relation.Parent == user select relation)
                .ToList();
            return relationInformatics.Count != 0 ? relationInformatics[0] : null;
        }
    }
}