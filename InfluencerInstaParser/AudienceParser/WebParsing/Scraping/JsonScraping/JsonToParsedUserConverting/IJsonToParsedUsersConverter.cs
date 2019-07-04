using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.JsonToParsedUserConverting
{
    public interface IJsonToParsedUsersConverter
    {
        IEnumerable<ParsedUserFromJson> GetUsersFromLikes(JObject json);
        IEnumerable<ParsedUserFromJson> GetUsersFromComments(JObject json);
    }
}