using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.PostPageParsing.LikesParsing
{
    public interface ILikesParser
    {
        IEnumerable<ParsedUser> GetUsersFromLikes(Post post);
    }
}