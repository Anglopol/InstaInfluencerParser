namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping
{
    public interface IInstagramLocationPageScraper : IInstagramPageContentScraper
    {
        double GetLatitudeFromLocationPage(string locationPageContent);
        double GetLongitudeFromLocationPage(string locationPageContent);
    }
}