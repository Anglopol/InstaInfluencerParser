using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.PostScraping
{
    public interface IPostJsonScraper
    {
        IEnumerable<ParsedUserFromJson> GetUsersFromCommentsPreview(JToken post);
        string GetOwnerName(JToken post);
        ulong GetOwnerId(JToken post);
        string GetShortCode(JToken post);
        int GetNumberOfLikes(JToken post);
        int GetNumberOfComments(JToken post);
        bool TryGetLocationId(JToken post, out ulong locationId);
        bool TryGetNextCommentsCursor(JToken post, out string nextCursor);
        bool IsPostVideo(JToken post);
    }
}