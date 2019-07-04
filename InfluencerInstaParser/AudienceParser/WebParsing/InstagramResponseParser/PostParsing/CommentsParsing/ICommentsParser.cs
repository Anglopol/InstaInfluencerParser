using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.PostParsing.CommentsParsing
{
    public interface ICommentsParser
    {
        IEnumerable<ParsedUserFromJson> GetUsersFromComments(Post post);
    }
}