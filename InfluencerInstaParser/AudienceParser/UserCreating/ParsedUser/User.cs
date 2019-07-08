using System;
using System.Collections.Generic;
using System.Linq;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing;
using InfluencerInstaParser.Database.DataClasses;

namespace InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser
{
    public class User : IUser
    {
        public string Name { get; }
        public string Uid { get; private set; }
        public ulong InstagramId { get; }

        public bool IsUserEmpty { get; }
        public bool IsInfluencer { get; }
        public IEnumerable<LocatorScrapingResult> Locations { get; }
        public IEnumerable<Post> Posts { get; set; }
        private readonly Dictionary<ulong, UsersFromJsonWithCounter> _usersFromComments;
        private readonly Dictionary<ulong, UsersFromJsonWithCounter> _usersFromLikes;

        public IEnumerable<ParsedUserFromJson> UsersFromLikes
        {
            get => UsersDictToEnum(_usersFromLikes);
            set => EnumToUsersDict(value, _usersFromLikes);
        }

        public IEnumerable<ParsedUserFromJson> UsersFromComments
        {
            get => UsersDictToEnum(_usersFromComments);
            set => EnumToUsersDict(value, _usersFromComments);
        }
        
        private class UsersFromJsonWithCounter
        {
            public ParsedUserFromJson UserFromJson { get; }
            public int Counter { get; set; }

            public UsersFromJsonWithCounter(ParsedUserFromJson fromJson)
            {
                UserFromJson = fromJson;
                Counter = 1;
            }
        }

        public User()
        {
            IsUserEmpty = true;
        }

        public User(string name, ulong instagramId, bool isInfluencer, IEnumerable<LocatorScrapingResult> locations)
        {
            Name = name;
            InstagramId = instagramId;
            Locations = locations;
            Uid = Guid.NewGuid().ToString();
            IsUserEmpty = false;
            IsInfluencer = isInfluencer;
            _usersFromComments = new Dictionary<ulong, UsersFromJsonWithCounter>();
            _usersFromLikes = new Dictionary<ulong, UsersFromJsonWithCounter>();
        }

        public IEnumerable<ParsedUserFromJson> GetUsersToParse()
        {
            return GetNonUniqueUsersFromDictionariesWithValidPrivacy().Union(GetUniqueUsersFromDictionaries());
        }

        public IUser Clone()
        {
            var copy = (User) MemberwiseClone();
            copy.Uid = Guid.NewGuid().ToString();
            return copy;
        }

        public UserToUserRelation GetRelation(IUser user)
        {
            var childId = user.InstagramId;
            var relation = new UserToUserRelation(Uid, user.Uid);
            if (_usersFromLikes.TryGetValue(childId, out var child)) relation.Likes = child.Counter;
            if (_usersFromComments.TryGetValue(childId, out child)) relation.Comments = child.Counter;
            return relation;
        }

        public bool HasRelationToUser(IUser user)
        {
            var userId = user.InstagramId;
            return _usersFromComments.ContainsKey(userId) || _usersFromLikes.ContainsKey(userId);
        }

        private static void EnumToUsersDict(IEnumerable<ParsedUserFromJson> userFromJsons,
            IDictionary<ulong, UsersFromJsonWithCounter> dictionary)
        {
            dictionary.Clear();
            foreach (var fromJson in userFromJsons)
            {
                if (!dictionary.TryAdd(fromJson.UserId, new UsersFromJsonWithCounter(fromJson)))
                    dictionary[fromJson.UserId].Counter++;
            }
        }

        private static IEnumerable<ParsedUserFromJson> UsersDictToEnum(
            IDictionary<ulong, UsersFromJsonWithCounter> dictionary)
        {
            return from usersFromJsonWithCounter in dictionary select usersFromJsonWithCounter.Value.UserFromJson;
        }

        private IEnumerable<ParsedUserFromJson> GetUniqueUsersFromDictionaries()
        {
            var uniqueFromLikes = GetUniqueUsersFromLikes();
            var uniqueFromComments = GetUniqueUsersFromComments();
            return uniqueFromLikes.Union(uniqueFromComments);
        }

        private IEnumerable<ParsedUserFromJson> GetUniqueUsersFromLikes()
        {
            return from fromLike in _usersFromLikes
                where !_usersFromComments.ContainsKey(fromLike.Key)
                select fromLike.Value.UserFromJson;
        }
        
        private IEnumerable<ParsedUserFromJson> GetUniqueUsersFromComments()
        {
            return from fromComments in _usersFromComments
                where !_usersFromLikes.ContainsKey(fromComments.Key)
                select fromComments.Value.UserFromJson;
        }

        private IEnumerable<ParsedUserFromJson> GetNonUniqueUsersFromDictionariesWithValidPrivacy()
        {
            return from fromLike in _usersFromLikes
                from fromComment in _usersFromComments
                where fromLike.Key == fromComment.Key
                let name = fromLike.Value.UserFromJson.Name
                let id = fromLike.Value.UserFromJson.UserId
                let isPrivate = PrivacyDisjunction(fromLike.Value.UserFromJson, fromComment.Value.UserFromJson)
                select new ParsedUserFromJson(name, id, isPrivate);
        }

        private static bool PrivacyDisjunction(ParsedUserFromJson firstUser, ParsedUserFromJson secondUser)
        {
            return firstUser.IsPrivate || secondUser.IsPrivate;
        }
    }
}