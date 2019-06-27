using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping
{
    public class UserPageScraper : IInstagramUserPageScraper
    {
        public string GetRhxGisParameterFromUserPage(string pageContent)
        {
            return Regex.Matches(pageContent, "rhx_gis.{3}[^\"]*")[0].ToString()
                .Split(":")[1].Remove(0, 1);
        }

        public string GetEndOfCursorFromUserPage(string pageContent)
        {
            if (IsContentHasNextPage(pageContent))
                return Regex.Match(pageContent,
                        "\"has_next_page\":true,\"end_cursor.{3}[^\"]*").ToString()
                    .Split(":")[2].Remove(0, 1);

            return "";
        }

        public ulong GetUserIdFromUserPage(string pageContent)
        {
            return ulong.Parse(Regex.Match(pageContent, "owner\":{\"id.{3}[^\"]*").ToString()
                .Split(":")[2].Remove(0, 1));
        }

        public int GetNumberOfSubscribersFromUserPage(string pageContent)
        {
            var number = int.Parse(Regex.Match(pageContent, "edge_followed_by\":{\"count\":[^}]*")
                .ToString().Split(":")[2]);
            return number;
        }

        public int GetNumberOfFollowingFromUserPage(string pageContent)
        {
            var number = int.Parse(Regex.Match(pageContent, "edge_follow\":{\"count\":[^}]*")
                .ToString().Split(":")[2]);
            return number;
        }

        public IEnumerable<string> GetShortCodesFromUserPage(string pageContent)
        {
            return Regex.Matches(pageContent, "shortcode.{3}[^\"]*").Select(match => match.Value.ToString()
                .Split(":")[1].Remove(0, 1));
        }

        public IEnumerable<ulong> GetLocationsIdFromUserPage(string pageContent)
        {
            var listOfStrings = Regex.Matches(pageContent, "location\":[^,]*")
                .Where(match => !match.Value.Contains("null")).Select(
                    match => match.Value.ToString().Split("\"")[4]);
            return from shortCode in listOfStrings select ulong.Parse(shortCode);
        }

        public bool IsUserPagePrivate(string pageContent)
        {
            return pageContent.Contains("\"is_private\":true");
        }

        public bool IsUserPageEmpty(string pageContent)
        {
            return pageContent.Contains("edge_owner_to_timeline_media\":{\"count\":0") || pageContent == "";
        }

        public bool IsContentHasNextPage(string pageContent)
        {
            return pageContent.Contains("\"has_next_page\":true");
        }
    }
}