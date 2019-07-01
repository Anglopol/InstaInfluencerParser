using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.PostPageParsing.JsonToParsedUserConverting
{
    public interface IJsonToParsedUsersConverter
    {
        IEnumerable<ParsedUser> GetUsersFromLikes(JObject json);
        IEnumerable<ParsedUser> GetUsersFromComments(JObject json);
    }
}