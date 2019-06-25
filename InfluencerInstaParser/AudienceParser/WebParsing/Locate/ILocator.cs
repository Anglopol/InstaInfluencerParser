using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Locate
{
    public interface ILocator
    {
        LocatorScrapingResult GetNearestCityByLocationPageContent(string locationPageContent, ulong locationId);
        bool IsCityAlreadyCached(ulong locationId);
        LocatorScrapingResult GetCachedCityByLocationId(ulong locationId);
        string GetLocationPageContentByLocationId(ulong locationId);
        bool IsLocationPageContentValid(string locationPageContent);
    }
}