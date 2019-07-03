namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing
{
    public interface ILocator
    { 
        LocatorScrapingResult GetLocatorScrapingResultByLocationId(ulong locationId);
    }
}