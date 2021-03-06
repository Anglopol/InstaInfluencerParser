using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing;
using InfluencerInstaParser.Database.DataClasses;

namespace InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser
{
    public interface IUser
    {
        IEnumerable<ParsedUserFromJson> UsersFromLikes { get; set; }
        IEnumerable<ParsedUserFromJson> UsersFromComments { get; set; }
        string Name { get; }
        string Uid { get; }
        ulong InstagramId { get; }
        bool IsUserEmpty { get; }
        bool IsInfluencer { get; }
        int Likes { get; }
        int Comments { get; }
        IEnumerable<LocatorScrapingResult> Locations { get; }
        IEnumerable<Post> Posts { get; set; }
        IEnumerable<ParsedUserFromJson> GetUsersToParse();
        IUser Clone();
        UserToUserRelation GetRelation(IUser user);
        bool HasRelationToUser(IUser user);
    }
}