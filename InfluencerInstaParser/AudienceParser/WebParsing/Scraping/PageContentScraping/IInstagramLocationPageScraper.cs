namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping
{
    public interface IInstagramLocationPageScraper
    {
        double GetLatitudeFromLocationPage(string locationPageContent);
        double GetLongitudeFromLocationPage(string locationPageContent);
    }
}