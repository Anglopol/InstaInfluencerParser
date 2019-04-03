using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.API;

namespace InfluencerInstaParser.AudienceParser.AuthorizedParsing.AudienceDownloader
{
    public interface IFollowingDownloader
    {
        Task<IEnumerable<string>> GetFollowing(string username, IInstaApi api);
    }
}