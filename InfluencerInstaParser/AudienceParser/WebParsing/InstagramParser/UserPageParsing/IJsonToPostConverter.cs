using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.UserPageParsing
{
    public interface IJsonToPostConverter
    {
        IEnumerable<Post> GetPosts(JObject json);
    }
}