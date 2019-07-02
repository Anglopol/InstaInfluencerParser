using System.Collections.Generic;
using System.Linq;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.JsonToParsedUserConverting
{
    public class JsonToParsedUsersConverter : IJsonToParsedUsersConverter
    {
        private const string PathToCommentsEdges = "data.shortcode_media.edge_media_to_parent_comment.edges";
        private const string PathToLikesEdges = "data.shortcode_media.edge_liked_by.edges";

        public IEnumerable<ParsedUser> GetUsersFromLikes(JObject json)
        {
            var likesEdges = GetLikesEdges(json);
            return GetUsersFromLikesEdges(likesEdges);
        }

        public IEnumerable<ParsedUser> GetUsersFromComments(JObject json)
        {
            var commentsEdges = GetCommentsEdges(json);
            return GetUsersFromCommentsEdges(commentsEdges);
        }

        private static IEnumerable<ParsedUser> GetUsersFromLikesEdges(JToken edges)
        {
            return from edge in edges
                select GetNodeFromEdge(edge)
                into node
                let username = GetUsername(node)
                let id = GetUserId(node)
                select new ParsedUser(username, id);
        }

        private static IEnumerable<ParsedUser> GetUsersFromCommentsEdges(JToken edges)
        {
            return from edge in edges
                select GetNodeFromEdge(edge)
                into node
                select GetOwnerFromNode(node)
                into owner
                let username = GetUsername(owner)
                let id = GetUserId(owner)
                select new ParsedUser(username, id);
        }

        private static JArray GetCommentsEdges(JToken json)
        {
            return (JArray) json.SelectToken(PathToCommentsEdges);
        }

        private static JArray GetLikesEdges(JToken json)
        {
            return (JArray) json.SelectToken(PathToLikesEdges);
        }

        private static JToken GetNodeFromEdge(JToken edge)
        {
            return edge.SelectToken("node");
        }

        private static JToken GetOwnerFromNode(JToken node)
        {
            return node.SelectToken("owner");
        }

        private static string GetUsername(JToken token)
        {
            return (string) token.SelectToken("username");
        }

        private static ulong GetUserId(JToken token)
        {
            return (ulong) token.SelectToken("id");
        }
    }
}