using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.PostScraping
{
    public class PostJsonScraper : IPostJsonScraper
    {
        public IEnumerable<ulong> GetUsersIdFromCommentsPreview(JToken post)
        {
            var usersId = new List<ulong>();
            var commentsToken = GetCommentsToken(post);
            if (!IsPostContainsComments(commentsToken)) return usersId;
            var commentsEdges = GetCommentsEdges(commentsToken);
            foreach (var edge in commentsEdges)
            {
                usersId.Add(GetUserIdFromCommentsEdge(edge));
            }

            return usersId;
        }

        public string GetOwnerName(JToken post)
        {
            return (string) post.SelectToken("owner.username");
        }

        public ulong GetOwnerId(JToken post)
        {
            return (ulong) post.SelectToken("owner.id");
        }

        public string GetShortCode(JToken post)
        {
            return (string) post.SelectToken("shortcode");
        }

        public bool TryGetLocationId(JToken post, out ulong locationId)
        {
            locationId = 0;
            var locationToken = post.SelectToken("location");
            if (!locationToken.HasValues) return false;
            locationId = (ulong) locationToken.SelectToken("id");
            return true;
        }

        public bool TryGetNextCommentsCursor(JToken post, out string nextCursor)
        {
            nextCursor = "";
            var commentsToken = GetCommentsToken(post);
            if (!IsPostContainsComments(commentsToken)) return false;
            if (!IsPostHasNextCursorForComments(commentsToken)) return false;
            nextCursor = GetNextCursorForComments(commentsToken);
            return true;
        }

        public bool IsPostVideo(JToken post)
        {
            return (bool) post.SelectToken("is_video");
        }

        private static JToken GetCommentsToken(JToken post)
        {
            return post.SelectToken("edge_media_to_comment");
        }

        private static JArray GetCommentsEdges(JToken commentsToken)
        {
            return (JArray) commentsToken.SelectToken("edges");
        }

        private static bool IsPostContainsComments(JToken commentsToken)
        {
            return (int) commentsToken.SelectToken("count") != 0;
        }

        private static bool IsPostHasNextCursorForComments(JToken commentsToke)
        {
            return (bool) commentsToke.SelectToken("page_info.has_next_page");
        }

        private static string GetNextCursorForComments(JToken commentsToken)
        {
            return (string) commentsToken.SelectToken("page_info.end_cursor");
        }

        private ulong GetUserIdFromCommentsEdge(JToken edge)
        {
            return (ulong) edge.SelectToken("node.owner.id");
        }
    }
}