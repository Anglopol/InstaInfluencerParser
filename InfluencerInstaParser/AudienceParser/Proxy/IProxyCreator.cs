using System.Collections.Generic;
using System.Net;

namespace InfluencerInstaParser.AudienceParser.Proxy
{
    public interface IProxyCreator
    {
        IEnumerable<IWebProxy> GetWebProxies();
    }
}