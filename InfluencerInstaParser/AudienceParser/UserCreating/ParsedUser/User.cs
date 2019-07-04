using System;
using System.Collections.Generic;
using System.Linq;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing;

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
        private readonly Dictionary<ulong, ParsedUserFromJson> _usersFromComments;
        private readonly Dictionary<ulong, ParsedUserFromJson> _usersFromLikes;

        public IEnumerable<ParsedUserFromJson> UsersFromLikes
        {
            get => _usersFromLikes.Values;
            set => EnumToUsersDict(value, _usersFromLikes);
        }

        public IEnumerable<ParsedUserFromJson> UsersFromComments
        {
            get => _usersFromComments.Values;
            set => EnumToUsersDict(value, _usersFromComments);
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
            _usersFromComments = new Dictionary<ulong, ParsedUserFromJson>();
            _usersFromLikes = new Dictionary<ulong, ParsedUserFromJson>();
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

        private static void EnumToUsersDict(IEnumerable<ParsedUserFromJson> userFromJsons,
            IDictionary<ulong, ParsedUserFromJson> dictionary)
        {
            dictionary.Clear();
            foreach (var fromJson in userFromJsons)
            {
                dictionary.Add(fromJson.UserId, fromJson);
            }
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
                select fromLike.Value;
        }
        
        private IEnumerable<ParsedUserFromJson> GetUniqueUsersFromComments()
        {
            return from fromComments in _usersFromComments
                where !_usersFromLikes.ContainsKey(fromComments.Key)
                select fromComments.Value;
        }

        private IEnumerable<ParsedUserFromJson> GetNonUniqueUsersFromDictionariesWithValidPrivacy()
        {
            return from fromLike in _usersFromLikes
                from fromComment in _usersFromComments
                where fromLike.Key == fromComment.Key
                let name = fromLike.Value.Name
                let id = fromLike.Value.UserId
                let isPrivate = PrivacyDisjunction(fromLike.Value, fromComment.Value)
                select new ParsedUserFromJson(name, id, isPrivate);
        }

        private static bool PrivacyDisjunction(ParsedUserFromJson firstUser, ParsedUserFromJson secondUser)
        {
            return firstUser.IsPrivate || secondUser.IsPrivate;
        }
    }
}