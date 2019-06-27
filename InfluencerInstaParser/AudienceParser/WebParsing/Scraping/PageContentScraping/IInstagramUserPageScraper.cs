using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping
{
    public interface IInstagramUserPageScraper : IInstagramPageContentScraper
    {
        string GetRhxGisParameterFromUserPage(string pageContent);
        string GetEndOfCursorFromUserPage(string pageContent);
        ulong GetUserIdFromUserPage(string pageContent);
        int GetNumberOfSubscribersFromUserPage(string pageContent);
        int GetNumberOfFollowingFromUserPage(string pageContent);
        IEnumerable<string> GetShortCodesFromUserPage(string pageContent);
        IEnumerable<ulong> GetLocationsIdFromUserPage(string pageContent);
        bool IsUserPagePrivate(string pageContent);
        bool IsUserPageEmpty(string pageContent);
        
    }
}