using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using NLog;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping
{
    public class PageContentScraper
    {
        private readonly Logger _logger;

        public PageContentScraper()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public string GetRhxGisParameter(string pageContent)
        {
            return Regex.Matches(pageContent, "rhx_gis.{3}[^\"]*")[0].ToString()
                .Split(":")[1].Remove(0, 1);
        }

        public string GetEndOfCursorFromPageContent(string pageContent)
        {
            if (HasNextPageForPageContent(pageContent))
                return Regex.Match(pageContent,
                        "\"has_next_page\":true,\"end_cursor.{3}[^\"]*").ToString()
                    .Split(":")[2].Remove(0, 1);

            return "";
        }

        public string MakeInstagramGis(string rhxGis, long userId, int count, string endOfCursor)
        {
            var signatureParams = $"{rhxGis}:{MakeSignatureString(userId, count, endOfCursor)}";
            _logger.Info(signatureParams);
            return CalculateMD5Hash(signatureParams);
        }

        public string MakeInstagramGis(string rhxGis, string shortCode, int count, string endOfCursor)
        {
            var signatureParams = $"{rhxGis}:{MakeSignatureString(shortCode, count, endOfCursor)}";
            _logger.Info(signatureParams);
            return CalculateMD5Hash(signatureParams);
        }

        public string MakeInstagramGisForLikes(string rhxGis, string shortCode, int count, string endOfCursor)
        {
            var signatureParams = $"{rhxGis}:{MakeSignatureStringForLikes(shortCode, count, endOfCursor)}";
            _logger.Info(signatureParams);
            return CalculateMD5Hash(signatureParams);
        }

        private string CalculateMD5Hash(string input)
        {
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            foreach (var character in hash) sb.Append(character.ToString("x2"));

            return sb.ToString();
        }

        private string MakeSignatureString(long userId, int count, string endOfCursor)
        {
            return $"{{\"id\":\"{userId}\",\"first\":{count},\"after\":\"{endOfCursor}\"}}";
        }

        private string MakeSignatureString(string shortCode, int count, string endOfCursor)
        {
            return $"{{\"shortcode\":\"{shortCode}\",\"first\":{count},\"after\":\"{endOfCursor}\"}}";
        }

        private string MakeSignatureStringForLikes(string shortCode, int count, string endOfCursor)
        {
            return endOfCursor == ""
                ? $"{{\"shortcode\":\"{shortCode}\",\"include_reel\":true,\"first\":{count}}}"
                : $"{{\"shortcode\":\"{shortCode}\",\"include_reel\":true,\"first\":{count},\"after\":\"{endOfCursor}\"}}";
        }

        public string GetQueryUrlForPosts(long userId, int count, string endOfCursor)
        {
            const string defaultQueryUrl =
                @"/graphql/query/?query_hash=f2405b236d85e8296cf30347c9f08c2a&variables=";
            var signatureUrlString = HttpUtility.UrlEncode(MakeSignatureString(userId, count, endOfCursor));
            return defaultQueryUrl + signatureUrlString;
        }

        public string GetQueryUrlForComments(string shortCode, int count, string endOfCursor)
        {
            const string defaultQueryUrl =
                @"/graphql/query/?query_hash=f0986789a5c5d17c2400faebf16efd0d&variables=";
            var signatureUrlString = HttpUtility.UrlEncode(MakeSignatureString(shortCode, count, endOfCursor));
            return defaultQueryUrl + signatureUrlString;
        }

        public string GetQueryUrlForLikes(string shortCode, int count, string endOfCursor = "")
        {
            const string defaultQueryUrl =
                @"/graphql/query/?query_hash=e0f59e4a1c8d78d0161873bc2ee7ec44&variables=";
            var signatureUrlString = HttpUtility.UrlEncode(MakeSignatureStringForLikes(shortCode, count, endOfCursor));
            return defaultQueryUrl + signatureUrlString;
        }

        public string GetUserIdFromPageContent(string pageContent)
        {
            return Regex.Match(pageContent, "owner\":{\"id.{3}[^\"]*").ToString()
                .Split(":")[2].Remove(0, 1);
        }

        public List<string> GetListOfShortCodesFromPageContent(string pageContent)
        {
            return Regex.Matches(pageContent, "shortcode.{3}[^\"]*").Select(match => match.Value.ToString()
                .Split(":")[1].Remove(0, 1)).ToList();
        }

        public IEnumerable<string> GetEnumerableOfUsernamesFromPageContent(string pageContent)
        {
            return Regex.Matches(pageContent, "username\".{2}[^\"]*").Select(match => match.Value.ToString()
                .Split(":")[1].Remove(0, 1)).ToList();
        }

        public List<ulong> GetListOfLocationsFromPageContent(string pageContent)
        {
            var listOfStrings = Regex.Matches(pageContent, "location\":[^,]*")
                .Where(match => !match.Value.Contains("null")).Select(
                    match => match.Value.ToString().Split("\"")[4]).ToList();
            return (from shortCode in listOfStrings select ulong.Parse(shortCode)).ToList();
        }

        public bool IsPostHasLocation(string pageContent)
        {
            return !pageContent.Contains("\"location\":null");
        }

        public bool IsLocationHasAddress(string pageContent)
        {
            return !pageContent.Contains("\"address_json\":null")
                   && !pageContent.Contains("city_name\\\": \\\"\\\"");
        }

        public string GetPostAddressLocation(string pageContent)
        {
            var city = Regex.Match(pageContent, "\"city_name[^,]*")
                .ToString().Split("\\")[2].Remove(0, 1);
            return city;
        }

        public string GetLocationSlug(string pageContent)
        {
            return Regex.Match(pageContent, "\"slug.{3}[^\"]*").ToString()
                .Split(":")[1].Remove(0, 1);
        }

        public string GetLocationId(string pageContent)
        {
            var location = Regex.Match(pageContent, "\"location\":[^}]*").ToString();
            return Regex.Match(location, "id.{3}[^\"]*").ToString()
                .Split(":")[1].Remove(0, 1);
        }

        public string GetCity(string locationPageContent)
        {
            var cityJson = Regex.Match(locationPageContent, "\"city\":[^}]*").ToString();
            var cityName = Regex.Match(cityJson, "\"name\".{2}[^\"]*").ToString()
                .Split(":")[1].Remove(0, 1);
            return cityName;
        }

        public double GetLocationLat(string pageContent)
        {
            return double.Parse(
                Regex.Match(pageContent, "location:latitude\" content=\"[^\"]*")
                    .ToString().Split("\"")[2]);
        }

        public double GetLocationLong(string pageContent)
        {
            return double.Parse(
                Regex.Match(pageContent, "location:longitude\" content=\"[^\"]*")
                    .ToString().Split("\"")[2]);
        }

        public IEnumerable<string> GetListOfProxies(string pageContent)
        {
            return pageContent.Split("\n").ToList();
        }

        public int GetNumberOfFollowers(string pageContent)
        {
            var number = int.Parse(Regex.Match(pageContent, "edge_followed_by\":{\"count\":[^}]*")
                .ToString().Split(":")[2]);
            return number;
        }

        public int GetNumberOfFollowing(string pageContent)
        {
            var number = int.Parse(Regex.Match(pageContent, "edge_follow\":{\"count\":[^}]*")
                .ToString().Split(":")[2]);
            return number;
        }

        public bool HasNextPageForPageContent(string pageContent)
        {
            return pageContent.Contains("\"has_next_page\":true");
        }

        public bool IsPrivate(string pageContent)
        {
            return pageContent.Contains("\"is_private\":true");
        }

        public bool IsVideo(string pageContent)
        {
            return pageContent.Contains("\"is_video\":true");
        }

        public bool IsEmpty(string pageContent)
        {
            return pageContent.Contains("edge_owner_to_timeline_media\":{\"count\":0") || pageContent == "";
        }

        public bool IsProxyListAvailable(string pageContent)
        {
            return !pageContent.Contains("error");
        }
    }
}