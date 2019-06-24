using System.Collections.Generic;
using System.Net;

namespace InfluencerInstaParser.AudienceParser.Proxy
{
    public interface IProxyCreatorSingleton
    {
        IEnumerable<IWebProxy> GetWebProxies();
    }
}