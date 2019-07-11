using System;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping
{
    public class ResponseJsonScraper : IResponseJsonScraper
    {
        private const string PathToPostsInJson = "data.user.edge_owner_to_timeline_media.edges";

        public JArray GetPostsEdges(JObject json)
        {
            return (JArray) json.SelectToken(PathToPostsInJson);
        }

        public JToken GetPostFromEdge(JToken edge)
        {
            return edge.SelectToken("node");
        }

        public string GetNextCursorForPosts(JObject postsJson)
        {
            var endOfCursor =
                (string) postsJson.SelectToken("data.user.edge_owner_to_timeline_media.page_info.end_cursor");
            return endOfCursor;        }

        public string GetNextCursorForLikes(JObject likesJson)
        {
            var endOfCursor =
                (string) likesJson.SelectToken("data.shortcode_media.edge_liked_by.page_info.end_cursor");
            return endOfCursor;
        }

        public string GetNextCursorForComments(JObject commentsJson)
        {
            var endOfCursor =
                (string) commentsJson.SelectToken("data.shortcode_media.edge_media_to_comment.page_info.end_cursor");
            return endOfCursor;
        }

        public JObject GetObjectFromJsonString(string jsonString)
        {
            return JObject.Parse(jsonString);
        }

        public bool IsNextPageExistsForPosts(JObject postsJson)
        {
            var nextPageProperty =
                (string) postsJson.SelectToken("data.user.edge_owner_to_timeline_media.page_info.has_next_page");
            return Convert.ToBoolean(nextPageProperty);
        }

        public bool IsNextPageExistsForLikes(JObject likesJson)
        {
            var nextPageProperty =
                (string) likesJson.SelectToken("data.shortcode_media.edge_liked_by.page_info.has_next_page");
            return Convert.ToBoolean(nextPageProperty);        }

        public bool IsNextPageExistsForComments(JObject commentsJson)
        {
            var nextPageProperty =
                (string) commentsJson.SelectToken("data.shortcode_media.edge_media_to_comment.page_info.has_next_page");
            return Convert.ToBoolean(nextPageProperty);        }
    }
}