using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping
{
    public interface IResponseJsonScraper
    {
        JArray GetPostsEdges(JObject json);
        JToken GetPostFromEdge(JToken edge);
        string GetNextCursorForPosts(JObject postsJson);
        string GetNextCursorForLikes(JObject likesJson);
        string GetNextCursorForComments(JObject commentsJson);
        JObject GetObjectFromJsonString(string jsonString);
        bool IsNextPageExistsForPosts(JObject postsJson);
        bool IsNextPageExistsForLikes(JObject likesJson);
        bool IsNextPageExistsForComments(JObject commentsJson);
    }
}