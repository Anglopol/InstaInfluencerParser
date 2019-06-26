
using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.UserInformation;
using InfluencerInstaParser.Database.Model;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser
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