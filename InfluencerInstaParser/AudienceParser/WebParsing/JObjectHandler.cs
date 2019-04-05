using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class JObjectHandler
    {
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

        public List<string> GetListOfUsernamesFromQueryContentForPost(JObject queryContent)
        {
            var edges = (JArray) queryContent.SelectToken("data.shortcode_media.edge_media_to_comment.edges");
            var users = new List<string>();
            foreach (var edge in edges) users.Add((string) edge.SelectToken("node.owner.username"));

            return users;
        }

        public List<string> GetListOfUsernamesFromQueryContentForLikes(JObject queryContent)
        {
            var edges = (JArray) queryContent.SelectToken("data.shortcode_media.edge_liked_by.edges");
            var users = new List<string>();
            foreach (var edge in edges) users.Add((string) edge.SelectToken("node.username"));

            return users;
        }

        public List<string> GetListOfShortCodesFromQueryContent(JObject queryContent)
        {
            var edges = (JArray) queryContent.SelectToken("data.user.edge_owner_to_timeline_media.edges");
            var shortCodes = new List<string>();
            foreach (var edge in edges) shortCodes.Add((string) edge.SelectToken("node.shortcode"));

            return shortCodes;
        }

        public bool HasNextPageForPosts(JObject queryContent)
        {
            var nextPageProperty =
                (string) queryContent.SelectToken("data.user.edge_owner_to_timeline_media.page_info.has_next_page");
            return bool.Parse(nextPageProperty);
        }

        public JObject GetObjectFromJsonString(string jsonString)
        {
            return JObject.Parse(jsonString);
        }

        public string GetProxyString(JObject jObject)
        {
            return jObject.Property("proxy").Value.ToString();
        }
    }
}