using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.UserParsing
{
    public interface IUserPageParser
    {
        string GetUserPage(string username);
        bool IsUserPageValid(string userPageContent);
        ulong GetUserId(string userPageContent);
        IEnumerable<Post> GetPostsFromUser(ulong userId);
    }
}