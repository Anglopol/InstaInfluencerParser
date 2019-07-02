using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.JsonToPostConverting
{
    public interface IJsonToPostConverter
    {
        IEnumerable<Post> GetPosts(JObject json);
    }
}