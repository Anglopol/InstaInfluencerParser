using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using InfluencerInstaParser.Database.ModelView;

namespace InfluencerInstaParser.AudienceParser.UserInformation
{
    public class User
    {
        private readonly object _setCommentsLocker;
        private readonly object _setFollowingLocker;
        private readonly object _setFollowersLocker;
        private readonly object _setLikesLocker;
        private int _comments;
        private int _likes;
        private int _following;
        private int _followers;
        private readonly ModelUser _modelUser;

        public bool IsLocationProcessed { get; set; }

        public ModelUser ModelViewUser
        {
            get
            {
                _modelUser.Locations = Locations.Keys.ToList();
                return _modelUser;
            }
        }

        private ConcurrentDictionary<string, int> Locations { get; }
        public ConcurrentDictionary<User, RelationInformation> Relations { get; }

        public bool IsInfluencer { get; }

        public User(string username, User parent, CommunicationType type, bool isInfluencer,
            int likes = 0, int comments = 0, int following = 0, int followers = 0)
        {
            _setFollowersLocker = new object();
            _setFollowingLocker = new object();
            _setCommentsLocker = new object();
            _setLikesLocker = new object();
            IsLocationProcessed = false;
            _modelUser = new ModelUser
            {
                Likes = likes, Comments = comments, Username = username, Followers = followers, Following = following,
                IsInfluencer = isInfluencer, Locations = new List<string>()
            };
            IsInfluencer = isInfluencer;
            Locations = new ConcurrentDictionary<string, int>();
            Username = username;
            Likes = likes;
            Comments = comments;
            Following = following;
            Followers = followers;
            Parent = parent;
            Relations = new ConcurrentDictionary<User, RelationInformation>();
            switch (type)
            {
                case CommunicationType.Liker:
                    Relations.TryAdd(parent, new RelationInformation(parent.Username, Username, likes: 1));
                    break;
                case CommunicationType.Commentator:
                    Relations.TryAdd(parent, new RelationInformation(parent.Username, Username, comments: 1));
                    break;
                default:
                    Relations.TryAdd(parent, new RelationInformation(parent.Username, Username));
                    break;
            }
        }

        public User(string username, int likes = 0, int comments = 0, int following = 0, int followers = 0)
        {
            _setFollowersLocker = new object();
            _setFollowingLocker = new object();
            _setCommentsLocker = new object();
            _setLikesLocker = new object();
            IsLocationProcessed = false;
            _modelUser = new ModelUser
            {
                Likes = likes, Comments = comments, Username = username, Followers = followers, Following = following,
                IsInfluencer = false, Locations = new List<string>()
            };
            Relations = new ConcurrentDictionary<User, RelationInformation>();
            Locations = new ConcurrentDictionary<string, int>();
            Username = username;
            Likes = likes;
            Comments = comments;
            Following = following;
            Followers = followers;
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
                    _modelUser.Likes = _likes;
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
                    _modelUser.Comments = _comments;
                }
            }
        }

        public int Following
        {
            get => _following;
            private set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                lock (_setFollowingLocker)
                {
                    _following = value;
                    _modelUser.Following = _following;
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
                    _modelUser.Followers = _followers;
                }
            }
        }

        public User Parent { get; }

        public void AddNewRelation(User parent)
        {
            if (parent.Username == Username) return;

            Relations.TryAdd(parent, new RelationInformation(parent.Username, Username));
        }

        public void AddLikesForRelation(User parent, int count = 1)
        {
            if (parent.Username == Username) return;

            if (!Relations.TryAdd(parent,
                new RelationInformation(parent.Username, Username, count)))
                Relations[Parent].Likes++;
        }

        public void AddCommentsForRelation(User parent, int count = 1)
        {
            if (parent.Username == Username) return;
            if (!Relations.TryAdd(parent,
                new RelationInformation(parent.Username, Username, comments: count)))
                Relations[Parent].Comments++;
        }

        public void AddLocation(string locationName, int cityId)
        {
            Locations.TryAdd(locationName, 0);
            Locations[locationName]++;
            var set = ParsingSetSingleton.GetInstance();
            if (set.Locations.TryAdd(locationName, new Dictionary<int, Location>()))
            {
                set.Locations[locationName].Add(cityId,
                    new Location {Name = locationName, Owner = Parent?.Username, Id = cityId, CountOfUsers = 0});
            }

            set.Locations[locationName][cityId].CountOfUsers++;
        }
    }
}