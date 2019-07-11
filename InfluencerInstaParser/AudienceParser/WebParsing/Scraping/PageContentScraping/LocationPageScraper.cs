using System;
using System.Text.RegularExpressions;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping
{
    public class LocationPageScraper : IInstagramLocationPageScraper
    {
        public bool IsContentHasNextPage(string pageContent)
        {
            return pageContent.Contains("\"has_next_page\":true");
        }

        public double GetLatitudeFromLocationPage(string locationPageContent)
        {
            return Convert.ToDouble(
                Regex.Match(locationPageContent, "location:latitude\" content=\"[^\"]*")
                    .ToString().Split("\"")[2]);        }

        public double GetLongitudeFromLocationPage(string locationPageContent)
        {
            return Convert.ToDouble(
                Regex.Match(locationPageContent, "location:longitude\" content=\"[^\"]*")
                    .ToString().Split("\"")[2]);
        }
    }
}