using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.API;

namespace InfluencerInstaParser.AudienceParser
{
    public interface IParser
    {
        Task<List<string>> GetParsedFollowers(string username, IInstaApi api);
    }
}