using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.API;

namespace InfluencerInstaParser.AudienceParser.AudienceDownloader
{
    public interface IFollowersDownloader
    {
        Task<IEnumerable<string>> GetFollowers(string username, IInstaApi api);
    }
}