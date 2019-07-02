using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.PostParsing.LikesParsing
{
    public interface ILikesParser
    {
        IEnumerable<ParsedUser> GetUsersFromLikes(Post post);
    }
}