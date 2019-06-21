using InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy;

namespace InfluencerInstaParser.AudienceParser.InstagramClient.InstagramClientCreating
{
    public interface IProxyClientCreator
    {
        IProxyClient GetClient();
        IProxyClient GetClient(IProxyClient spentClient);
        void FreeTheClient(IProxyClient proxyClient);
    }
}