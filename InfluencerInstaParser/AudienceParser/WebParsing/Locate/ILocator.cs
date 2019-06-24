using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Locate
{
    public interface ILocator
    {
        LocatorScrapingResult GetNearestCityByLocationId(ulong locationId);
    }
}