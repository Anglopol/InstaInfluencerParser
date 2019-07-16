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
            var value = Regex.Match(locationPageContent, "location:latitude\" content=\"[^\"]*")
                .ToString().Split("\"")[2].Replace(".", ",");
            if (double.TryParse(value, out var result)) return result;
            _logger.Error("Can't parse Latitude value:{page}", value);
            return 0;
        }


        public double GetLongitudeFromLocationPage(string locationPageContent)
        {
            var value = Regex.Match(locationPageContent, "location:longitude\" content=\"[^\"]*")
                .ToString().Split("\"")[2].Replace(".", ",");
            if (double.TryParse(value, out var result)) return result;
            _logger.Error("Can't parse Longitude value:{value}", value);
            return 0;
        }
    }
}