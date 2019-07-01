using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.PostPageParsing.CommentsParsing
{
    public interface ICommentsParser
    {
        IEnumerable<ParsedUser> GetUsersFromComments(Post post);
    }
}