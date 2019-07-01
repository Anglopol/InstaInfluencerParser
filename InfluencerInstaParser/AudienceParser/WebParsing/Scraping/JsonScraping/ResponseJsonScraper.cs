using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping
{
    public class ResponseJsonScraper : IResponseJsonScraper
    {
        private const string PathToPostsInJson = "data.user.edge_owner_to_timeline_media.edges";
        public IEnumerable<string> GetUsernamesFromLikes(JObject likesJson)
        {
            var edges = (JArray) likesJson.SelectToken("data.shortcode_media.edge_liked_by.edges");
            var users = new List<string>();
            foreach (var edge in edges) users.Add((string) edge.SelectToken("node.username"));

            return users;
        }

        public JArray GetPostsEdges(JObject json)
        {
            return (JArray) json.SelectToken(PathToPostsInJson);
        }

        public JToken GetPostFromEdge(JToken edge)
        {
            return edge.SelectToken("node");
        }

        public IEnumerable<string> GetUsernamesFromComments(JObject commentsJson)
        {
            var edges = (JArray) commentsJson.SelectToken("data.shortcode_media.edge_media_to_comment.edges");
            var users = new List<string>();
            foreach (var edge in edges) users.Add((string) edge.SelectToken("node.owner.username"));

            return users;
        }

        public IEnumerable<string> GetShortCodesFromPosts(JObject postsJson)
        {
            var edges = (JArray) postsJson.SelectToken(PathToPostsInJson);
            var shortCodes = new List<string>();
            foreach (var edge in edges) shortCodes.Add((string) edge.SelectToken("node.shortcode"));

            return shortCodes;
        }

        public IEnumerable<ulong> GetLocationsIdFromPosts(JObject postsJson)
        {
            var edges = (JArray) postsJson.SelectToken(PathToPostsInJson);
            var locations = new List<ulong>();
            foreach (var edge in edges)
            {
                var locationJson = edge.SelectToken("node.location");
                if (!locationJson.HasValues) continue;
                locations.Add(ulong.Parse((string) edge.SelectToken("node.location.id")));
            }

            return locations;
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
            return bool.Parse(nextPageProperty);
        }

        public bool IsNextPageExistsForLikes(JObject likesJson)
        {
            var nextPageProperty =
                (string) likesJson.SelectToken("data.shortcode_media.edge_liked_by.page_info.has_next_page");
            return bool.Parse(nextPageProperty);        }

        public bool IsNextPageExistsForComments(JObject commentsJson)
        {
            var nextPageProperty =
                (string) commentsJson.SelectToken("data.shortcode_media.edge_media_to_comment.page_info.has_next_page");
            return bool.Parse(nextPageProperty);        }
    }
}