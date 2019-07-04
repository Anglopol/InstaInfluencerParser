using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.PostScraping
{
    public class PostJsonScraper : IPostJsonScraper
    {
        public IEnumerable<ParsedUserFromJson> GetUsersFromCommentsPreview(JToken post)
        {
            var parsedUsers = new List<ParsedUserFromJson>();
            var commentsToken = GetCommentsToken(post);
            if (!IsPostContainsComments(commentsToken)) return parsedUsers;
            var commentsEdges = GetCommentsEdges(commentsToken);
            foreach (var edge in commentsEdges)
            {
                parsedUsers.Add(GetParsedUserFromCommentEdge(edge));
            }

            return parsedUsers;
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

        public int GetNumberOfLikes(JToken post)
        {
            var likesToken = GetLikesToken(post);
            return GetCount(likesToken);
        }

        public int GetNumberOfComments(JToken post)
        {
            var commentsToken = GetCommentsToken(post);
            return GetCount(commentsToken);
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
        
        private static bool IsPostContainsComments(JToken commentsToken)
        {
            return GetCount(commentsToken) != 0;
        }

        private ParsedUserFromJson GetParsedUserFromCommentEdge(JToken edge)
        {
            var id = GetUserIdFromCommentEdge(edge);
            var name = GetUsernameFromCommentEdge(edge);
            return new ParsedUserFromJson(name, id);
        }

        private static JToken GetCommentsToken(JToken post)
        {
            return post.SelectToken("edge_media_to_comment");
        }

        private static JToken GetLikesToken(JToken post)
        {
            return post.SelectToken("edge_media_preview_like");
        }

        private static JArray GetCommentsEdges(JToken commentsToken)
        {
            return (JArray) commentsToken.SelectToken("edges");
        }

        private static bool IsPostHasNextCursorForComments(JToken commentsToke)
        {
            return (bool) commentsToke.SelectToken("page_info.has_next_page");
        }

        private static string GetNextCursorForComments(JToken commentsToken)
        {
            return (string) commentsToken.SelectToken("page_info.end_cursor");
        }

        private static ulong GetUserIdFromCommentEdge(JToken edge)
        {
            return (ulong) edge.SelectToken("node.owner.id");
        }

        private static string GetUsernameFromCommentEdge(JToken edge)
        {
            return (string) edge.SelectToken("node.owner.username");
        }

        private static int GetCount(JToken token)
        {
            return (int) token.SelectToken("count");
        }
    }
}