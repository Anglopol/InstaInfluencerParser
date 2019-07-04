using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.PostParsing.LikesParsing
{
    public interface ILikesParser
    {
        IEnumerable<ParsedUserFromJson> GetUsersFromLikes(Post post);
    }
}