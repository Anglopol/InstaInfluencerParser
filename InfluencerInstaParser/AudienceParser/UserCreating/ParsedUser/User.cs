using System;
using System.Collections.Generic;
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

        public User Clone()
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
    }
}