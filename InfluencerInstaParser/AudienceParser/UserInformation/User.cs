using System;
using System.Collections.Generic;
using InfluencerInstaParser.Database.ModelView;

namespace InfluencerInstaParser.AudienceParser.UserInformation
{
    public class User
    {
        private readonly object _setCommentsLocker;
        private readonly object _setFollowingLocker;
        private readonly object _setFollowersLocker;
        private readonly object _setLikesLocker;
        private readonly object _relationLocker;
        private int _comments;
        private int _likes;
        private int _following;
        private int _followers;

        public ModelUser ModelViewUser { get; }
        public Dictionary<string, int> Locations { get; }
        public Dictionary<User, RelationInformation> Relations { get; }

        public User(string username, User parent, CommunicationType type,
            int likes = 0, int comments = 0, int following = 0, int followers = 0)
        {
            _setFollowersLocker = new object();
            _setFollowingLocker = new object();
            _setCommentsLocker = new object();
            _setLikesLocker = new object();
            _relationLocker = new object();
            ModelViewUser = new ModelUser
                {Likes = likes, Comments = comments, Username = username, Followers = followers, Following = following};
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
                        {{parent, new RelationInformation(parent.Username, Username, likes: 1)}};
                    break;
                case CommunicationType.Commentator:
                    Relations = new Dictionary<User, RelationInformation>
                        {{parent, new RelationInformation(parent.Username, Username, comments: 1)}};
                    break;
                default:
                    Relations = new Dictionary<User, RelationInformation>
                        {{parent, new RelationInformation(parent.Username, Username)}};
                    break;
            }
        }

        public User(string username, int likes = 0, int comments = 0, int following = 0, int followers = 0)
        {
            _setFollowersLocker = new object();
            _setFollowingLocker = new object();
            _setCommentsLocker = new object();
            _setLikesLocker = new object();
            _relationLocker = new object();
            ModelViewUser = new ModelUser
                {Likes = likes, Comments = comments, Username = username, Followers = followers, Following = following};
            Relations = new Dictionary<User, RelationInformation>();
            Locations = new Dictionary<string, int>();
            Username = username;
            Likes = likes;
            Comments = comments;
            Following = following;
            Followers = followers;
            Parent = this;
        }

        public string Username { get; }

        public int Likes
        {
            get => _likes;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                lock (_setLikesLocker)
                {
                    _likes = value;
                    ModelViewUser.Likes = _likes;
                }
            }
        }

        public int Comments
        {
            get => _comments;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                lock (_setCommentsLocker)
                {
                    _comments = value;
                    ModelViewUser.Comments = _comments;
                }
            }
        }

        public int Following
        {
            get => _following;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                lock (_setFollowingLocker)
                {
                    _following = value;
                    ModelViewUser.Following = _following;
                }
            }
        }

        public int Followers
        {
            get => _followers;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                lock (_setFollowersLocker)
                {
                    _followers = value;
                    ModelViewUser.Followers = _followers;
                }
            }
        }

        public User Parent { get; }

        public void AddNewRelation(User parent)
        {
            if (parent.Username == Username) return;
            lock (_relationLocker)
            {
                Relations.TryAdd(parent, new RelationInformation(parent.Username, Username));
            }
        }

        public void AddLikesForRelation(User parent, int count = 1)
        {
            if (parent.Username == Username) return;
            lock (_relationLocker)
            {
                if (!Relations.TryAdd(parent,
                    new RelationInformation(parent.Username, Username, count)))
                    Relations[Parent].Likes++;
            }
        }

        public void AddCommentsForRelation(User parent, int count = 1)
        {
            if (parent.Username == Username) return;
            lock (_relationLocker)
            {
                if (!Relations.TryAdd(parent,
                    new RelationInformation(parent.Username, Username, comments: count)))
                    Relations[Parent].Comments++;
            }
        }

        public void AddLocation(string location)
        {
            if (!Locations.TryAdd(location, 1)) Locations[location]++;
        }
    }
}