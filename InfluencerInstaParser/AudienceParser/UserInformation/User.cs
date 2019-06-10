using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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
        private DateTime _timeOfParsing;

        public bool IsLocationProcessed { get; set; }

        public ModelUser ModelViewUser
        {
            get
            {
                _modelUser.Locations = Locations.Keys.ToList();
                return _modelUser;
            }
        }

        public ConcurrentDictionary<string, Dictionary<string, LocationRelationInformation>> Locations { get; }
        public ConcurrentDictionary<User, RelationInformation> Relations { get; }

        public bool IsInfluencer { get; }

        public User(string username, DateTime timeOfParsing, User parent, CommunicationType type, bool isInfluencer,
            int likes = 0, int comments = 0, int following = 0, int followers = 0)
        {
            _setFollowersLocker = new object();
            _setFollowingLocker = new object();
            _setCommentsLocker = new object();
            _setLikesLocker = new object();
            _timeOfParsing = timeOfParsing;
            IsLocationProcessed = false;
            _modelUser = new ModelUser
            {
                Likes = likes, Parents = new List<string> {parent.Username}, Comments = comments, Username = username,
                Followers = followers, Following = following,
                IsInfluencer = isInfluencer, Locations = new List<string>(),
                DateOfParsing = timeOfParsing.ToString(CultureInfo.InvariantCulture)
            };
            IsInfluencer = isInfluencer;
            Locations = new ConcurrentDictionary<string, Dictionary<string, LocationRelationInformation>>();
            Username = username;
            Likes = likes;
            Comments = comments;
            Following = following;
            Followers = followers;
            Relations = new ConcurrentDictionary<User, RelationInformation>();
            switch (type)
            {
                case CommunicationType.Liker:
                    Relations.TryAdd(parent,
                        new RelationInformation(parent.Username, _timeOfParsing, Username, likes: 1));
                    break;
                case CommunicationType.Commentator:
                    Relations.TryAdd(parent,
                        new RelationInformation(parent.Username, _timeOfParsing, Username, comments: 1));
                    break;
                default:
                    Relations.TryAdd(parent,
                        new RelationInformation(parent.Username, _timeOfParsing, Username, true));
                    break;
            }
        }

        public User(string username, DateTime timeOfParsing, int likes = 0, int comments = 0, int following = 0,
            int followers = 0)
        {
            _setFollowersLocker = new object();
            _setFollowingLocker = new object();
            _setCommentsLocker = new object();
            _setLikesLocker = new object();
            IsLocationProcessed = false;
            _modelUser = new ModelUser
            {
                Likes = likes, Parents = new List<string> {"@target"}, Comments = comments, Username = username,
                Followers = followers,
                Following = following,
                IsInfluencer = false, Locations = new List<string>(),
                DateOfParsing = timeOfParsing.ToString(CultureInfo.InvariantCulture)
            };

            Relations = new ConcurrentDictionary<User, RelationInformation>();
            Locations = new ConcurrentDictionary<string, Dictionary<string, LocationRelationInformation>>();
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

        public void AddNewFollowRelation(User parent)
        {
            if (parent.Username == Username) return;

            if (!Relations.TryAdd(parent, new RelationInformation(parent.Username, _timeOfParsing, Username, true)))
                Relations[parent].Relation.Follower = true;
        }

        public void AddLikesForRelation(User parent, int count = 1)
        {
            if (parent.Username == Username) return;

            if (!Relations.TryAdd(parent,
                new RelationInformation(parent.Username, _timeOfParsing, Username, likes: count)))
                Relations[parent].Likes++;
        }

        public void AddCommentsForRelation(User parent, int count = 1)
        {
            if (parent.Username == Username) return;
            if (!Relations.TryAdd(parent,
                new RelationInformation(parent.Username, _timeOfParsing, Username, comments: count)))
                Relations[parent].Comments++;
        }

        public void AddLocation(string locationName, int cityId, string parentName)
        {
            AddLocationToDict(locationName, parentName);
            AddLocationToParsingSet(locationName, cityId, parentName);
        }

        public void AddLocation(string locationName, int cityId, IEnumerable<string> parents)
        {
            foreach (var parentName in parents)
            {
                AddLocationToDict(locationName, parentName);
                AddLocationToParsingSet(locationName, cityId, parentName);
            }
        }

        private void AddLocationToDict(string locationName, string parentName)
        {
            Locations.TryAdd(locationName, new Dictionary<string, LocationRelationInformation>
            {
                {
                    parentName,
                    new LocationRelationInformation
                    {
                        Count = 0, Name = locationName, Parent = parentName, Child = Username,
                        DateOfParsing = _timeOfParsing.ToString(CultureInfo.InvariantCulture)
                    }
                }
            });
            Locations[locationName].TryAdd(parentName,
                new LocationRelationInformation
                {
                    Count = 0, Name = locationName, Parent = parentName, Child = Username,
                    DateOfParsing = _timeOfParsing.ToString(CultureInfo.InvariantCulture)
                });
            Locations[locationName][parentName].Count++;
        }

        private void AddLocationToParsingSet(string locationName, int cityId, string parentName)
        {
            var set = ParsingSetSingleton.GetInstance();
            if (set.Locations.TryAdd(locationName, new Dictionary<int, List<Location>>()))
            {
                set.Locations[locationName].Add(cityId,
                    new List<Location>
                    {
                        new Location
                        {
                            Name = locationName, Owner = parentName, CityId = cityId, CountOfUsers = 1,
                            DateOfParsing = _timeOfParsing.ToString(CultureInfo.InvariantCulture)
                        }
                    });
            }
            else
            {
                var contains = false;
                var currentLocation = new Location
                {
                    Name = locationName, Owner = parentName, CityId = cityId, CountOfUsers = 1,
                    DateOfParsing = _timeOfParsing.ToString(CultureInfo.InvariantCulture)
                };
                foreach (var location in set.Locations[locationName][cityId])
                {
                    if (!location.Equals(currentLocation)) continue;
                    location.CountOfUsers++;
                    contains = true;
                    break;
                }

                if (!contains) set.Locations[locationName][cityId].Add(currentLocation);
            }
        }
    }
}