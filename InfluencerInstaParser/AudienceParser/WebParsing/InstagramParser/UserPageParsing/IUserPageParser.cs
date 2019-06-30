using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.UserPageParsing
{
    public interface IUserPageParser
    {
        string GetUserPage(string username);
        bool IsUserPageValid(string userPageContent);
        IEnumerable<Post> GetPostsFromUser(ulong userId);
        IEnumerable<string> GetUsersFromDownloadedShortCodes();
        IEnumerable<ulong> GetDownloadedLocationsId();
    }
}