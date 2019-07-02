using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.PostParsing.CommentsParsing
{
    public interface ICommentsParser
    {
        IEnumerable<ParsedUser> GetUsersFromComments(Post post);
    }
}