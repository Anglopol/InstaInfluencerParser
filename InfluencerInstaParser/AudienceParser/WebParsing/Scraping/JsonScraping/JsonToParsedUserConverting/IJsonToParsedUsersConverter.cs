using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.JsonToParsedUserConverting
{
    public interface IJsonToParsedUsersConverter
    {
        IEnumerable<ParsedUser> GetUsersFromLikes(JObject json);
        IEnumerable<ParsedUser> GetUsersFromComments(JObject json);
    }
}