using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy;

namespace InfluencerInstaParser.AudienceParser.InstagramClient.ProxyClientCreating
{
    public interface IProxyClientCreator
    {
        Task<IProxyClient> GetClientAsync();
        Task<IProxyClient> GetClientAsync(IProxyClient spentClient);
        void FreeTheClient(IProxyClient proxyClient);
    }
}