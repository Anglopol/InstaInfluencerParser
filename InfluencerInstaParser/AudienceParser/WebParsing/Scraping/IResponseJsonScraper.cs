using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping
{
    public interface IResponseJsonScraper
    {
        IEnumerable<string> GetUsernamesFromLikes(JObject likesJson);
        IEnumerable<string> GetUsernamesFromComments(JObject commentsJson);
        IEnumerable<string> GetShortCodesFromPosts(JObject postsJson);
        IEnumerable<ulong> GetLocationsIdFromPosts(JObject postsJson);
        string GetNextCursorForPosts(JObject postsJson);
        string GetNextCursorForLikes(JObject likesJson);
        string GetNextCursorForComments(JObject commentsJson);
        JObject GetObjectFromJsonString(string jsonString);
        bool IsNextPageExistsForPosts(JObject postsJson);
        bool IsNextPageExistsForLikes(JObject likesJson);
        bool IsNextPageExistsForComments(JObject commentsJson);
    }
}