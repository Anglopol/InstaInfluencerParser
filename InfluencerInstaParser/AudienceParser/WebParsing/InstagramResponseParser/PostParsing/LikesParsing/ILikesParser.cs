using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.PostParsing.LikesParsing
{
    public interface ILikesParser
    {
        IEnumerable<ParsedUser> GetUsersFromLikes(Post post);
    }
}