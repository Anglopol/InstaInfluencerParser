using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.PostParsing.CommentsParsing
{
    public interface ICommentsParser
    {
        IEnumerable<ParsedUser> GetUsersFromComments(Post post);
    }
}