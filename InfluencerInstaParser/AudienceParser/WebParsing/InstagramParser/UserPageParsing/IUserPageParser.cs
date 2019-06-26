using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.UserPageParsing
{
    public interface IUserPageParser
    {
        string GetUserPage(string username);
        bool IsUserPageValid(string userPageContent);
        void DownloadLocationsAndPostsShortCodesForTargetUser(string userPage);
        IEnumerable<string> GetUsersFromDownloadedShortCodes();
        IEnumerable<ulong> GetDownloadedLocationsId();
    }
}