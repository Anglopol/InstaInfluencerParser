using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping
{
    public interface IInstagramPostPageScraper : IInstagramPageContentScraper
    {
        bool IsPostVideo(string postPageContent);
        IEnumerable<string> GetUsernamesFromPostPage(string postPageContent);
    }
}