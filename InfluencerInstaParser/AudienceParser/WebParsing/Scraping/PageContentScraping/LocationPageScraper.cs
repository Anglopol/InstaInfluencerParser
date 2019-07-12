using System.Text.RegularExpressions;
using Serilog;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping
{
    public class LocationPageScraper : IInstagramLocationPageScraper
    {
        private ILogger _logger;

        public LocationPageScraper(ILogger logger)
        {
            _logger = logger;
        }

        public bool IsContentHasNextPage(string pageContent)
        {
            return pageContent.Contains("\"has_next_page\":true");
        }

        public double GetLatitudeFromLocationPage(string locationPageContent)
        {
            if (double.TryParse(
                Regex.Match(locationPageContent, "location:latitude\" content=\"[^\"]*")
                    .ToString().Split("\"")[2], out var result)) return result;
            _logger.Error("Can't parse Latitude for {page} \nlocation page", locationPageContent);
            return 0;
        }


        public double GetLongitudeFromLocationPage(string locationPageContent)
        {
            if (double.TryParse(
                Regex.Match(locationPageContent, "location:longitude\" content=\"[^\"]*")
                    .ToString().Split("\"")[2], out var result)) return result;
            _logger.Error("Can't parse Longitude for {page} \nlocation page", locationPageContent);
            return 0;
        }
    }
}