using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class WebProcessor
    {
        private string _userAgent;
//        public WebProcessor(string userAgent)
//        {
//            _userAgent = userAgent;
//        }
        public string GetRhxGisParameter(string pageContent)
        {
            return Regex.Matches(pageContent, "rhx_gis.{3}[^\"]*")[0].ToString()
                .Split(":")[1].Remove(0, 1);
        }

        public string GetEndOfCursorFromPageContent(string pageContent)
        {
            if (HasNextPageForPageContent(pageContent))
            {
                return Regex.Matches(pageContent,
                        "\"has_next_page\":true,\"end_cursor.{3}[^\"]*")[0].ToString()
                    .Split(":")[2].Remove(0, 1);
            }

            return "";
        }

        public string MakeInstagramGis(string rhxGis, long userId, int count, string endOfCursor)
        {
            var signatureParams = $"{rhxGis}:{MakeSignatureString(userId, count, endOfCursor)}";
            Console.WriteLine(signatureParams);
            return CalculateMD5Hash(signatureParams);
        }

        public string MakeInstagramGis(string rhxGis, string shortCode, int count, string endOfCursor)
        {
            var signatureParams = $"{rhxGis}:{MakeSignatureString(shortCode, count, endOfCursor)}";
            Console.WriteLine(signatureParams);
            return CalculateMD5Hash(signatureParams);
        }

        private string CalculateMD5Hash(string input)
        {
            var md5 = MD5.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            foreach (var character in hash)
            {
                sb.Append(character.ToString("x2"));
            }

            return sb.ToString();
        }

        public string MakeSignatureString(long userId, int count, string endOfCursor)
        {
            return $"{{\"id\":\"{userId}\",\"first\":{count},\"after\":\"{endOfCursor}\"}}";
        }

        public string MakeSignatureString(string shortCode, int count, string endOfCursor)
        {
            return $"{{\"shortcode\":\"{shortCode}\",\"first\":{count},\"after\":\"{endOfCursor}\"}}";
        }

        public string MakeSignatureStringForLikes(string shortCode, int count, string endOfCursor)
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
            var signatureUrlString = HttpUtility.UrlEncode(MakeSignatureString(shortCode, count, endOfCursor));
            return defaultQueryUrl + signatureUrlString;
        }

        public JObject GetObjectFromJsonString(string jsonString)
        {
            return JObject.Parse(jsonString);
        }

        public List<string> GetListOfShortCodesFromPageContent(string pageContent)
        {
            return Regex.Matches(pageContent, "shortcode.{3}[^\"]*").Select(match => match.Value.ToString()
                .Split(":")[1].Remove(0, 1)).ToList();
        }

        public List<string> GetListOfUsernamesFromPageContent(string pageContent)
        {
            return Regex.Matches(pageContent, "username.{3}[^\"]*").Select(match => match.Value.ToString()
                .Split(":")[1].Remove(0, 1)).ToList();
        }

        public List<string> GetListOfUsernamesFromQueryContentForPost(JObject queryContent)
        {
            var edges = (JArray) queryContent.SelectToken("data.shortcode_media.edge_media_to_comment.edges");
            var users = new List<string>();
            foreach (var edge in edges)
            {
                users.Add((string) edge.SelectToken("node.owner.username"));
            }

            return users;
        }

        public List<string> GetListOfUsernamesFromQueryContentForLikes(JObject queryContent)
        {
            var edges = (JArray) queryContent.SelectToken("data.shortcode_media.edge_liked_by.edges");
            var users = new List<string>();
            foreach (var edge in edges)
            {
                users.Add((string) edge.SelectToken("node.username"));
            }

            return users;
        }

        public List<string> GetListOfShortCodesFromQueryContent(JObject queryContent)
        {
            var edges = (JArray) queryContent.SelectToken("data.user.edge_owner_to_timeline_media.edges");
            var shortCodes = new List<string>();
            foreach (var edge in edges)
            {
                shortCodes.Add((string) edge.SelectToken("node.shortcode"));
            }

            return shortCodes;
        }

        public bool HasNextPageForPosts(JObject queryContent)
        {
            var nextPageProperty =
                (string) queryContent.SelectToken("data.user.edge_owner_to_timeline_media.page_info.has_next_page");
            return bool.Parse(nextPageProperty);
        }

        public bool HasNextPageForPageContent(string pageContent)
        {
            return pageContent.Contains("\"has_next_page\":true");
        }

        public bool HasNextPageForComments(JObject queryContent)
        {
            var nextPageProperty =
                (string) queryContent.SelectToken("data.shortcode_media.edge_media_to_comment.page_info.has_next_page");
            return bool.Parse(nextPageProperty);
        }

        public bool HasNextPageForLikes(JObject queryContent)
        {
            var nextPageProperty =
                (string) queryContent.SelectToken("data.shortcode_media.edge_liked_by.page_info.has_next_page");
            return bool.Parse(nextPageProperty);
        }

        public string GetEndOfCursorFromJsonForPosts(JObject queryContent)
        {
            var endOfCursor =
                (string) queryContent.SelectToken("data.user.edge_owner_to_timeline_media.page_info.end_cursor");
            return endOfCursor;
        }

        public string GetEndOfCursorFromJsonForComments(JObject queryContent)
        {
            var endOfCursor =
                (string) queryContent.SelectToken("data.shortcode_media.edge_media_to_comment.page_info.end_cursor");
            return endOfCursor;
        }

        public string GetEndOfCursorFromJsonForLikes(JObject queryContent)
        {
            var endOfCursor =
                (string) queryContent.SelectToken("data.shortcode_media.edge_liked_by.page_info.end_cursor");
            return endOfCursor;
        }
    }
}