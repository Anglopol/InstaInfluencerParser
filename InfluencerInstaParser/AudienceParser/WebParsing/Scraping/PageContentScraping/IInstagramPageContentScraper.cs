namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping
{
    public interface IInstagramPageContentScraper 
    {
        bool IsContentHasNextPage(string pageContent);
    }
}