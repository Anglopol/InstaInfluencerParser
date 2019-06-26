namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping
{
    public interface IInstagramPageContentScraper : IInstagramLocationPageScraper, IInstagramPostPageScraper,
        IInstagramUserPageScraper
    {
        bool IsContentHasNextPage(string pageContent);
    }
}