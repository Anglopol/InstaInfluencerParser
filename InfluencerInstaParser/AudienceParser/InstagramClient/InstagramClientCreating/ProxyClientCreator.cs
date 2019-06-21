using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace InfluencerInstaParser.AudienceParser.InstagramClient.InstagramClientCreating
{
    public class ProxyClientCreator : IProxyClientCreator
    {
        private ConcurrentBag<IProxyClient> _proxyClients;
        private IList<WebProxy> _proxies;

        public ProxyClientCreator(IList<WebProxy> proxies)
        {
            _proxies = proxies;
            Initialize();
        }

        private void Initialize()
        {
            _proxyClients = new ConcurrentBag<IProxyClient>();
            foreach (var webProxy in _proxies)
            {
                
            }
        }
        
        public IProxyClient GetClient()
        {
            throw new System.NotImplementedException();
        }

        public IProxyClient GetClient(IProxyClient spentClient)
        {
            throw new System.NotImplementedException();
        }

        public void FreeTheClient(IProxyClient proxyClient)
        {
            throw new System.NotImplementedException();
        }
    }
}