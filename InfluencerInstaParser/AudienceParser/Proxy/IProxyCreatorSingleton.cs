using System.Net;

namespace InfluencerInstaParser.AudienceParser.Proxy
{
    public interface IProxyCreatorSingleton
    {
        WebProxy GetProxy();
        WebProxy GetProxy(WebProxy usedProxy);
        void SetProxyFree(WebProxy usedProxy);
    }
}