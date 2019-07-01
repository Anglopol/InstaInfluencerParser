using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.PostScraping
{
    public interface IPostJsonScraper
    {
        IEnumerable<ulong> GetUsersIdFromCommentsPreview(JToken post);
        string GetOwnerName(JToken post);
        ulong GetOwnerId(JToken post);
        string GetShortCode(JToken post);
        bool TryGetLocationId(JToken post, out ulong locationId);
        bool TryGetNextCommentsCursor(JToken post, out string nextCursor);
        bool IsPostVideo(JToken post);
    }
}