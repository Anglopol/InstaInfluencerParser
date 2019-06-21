using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy;
using Microsoft.Extensions.DependencyInjection;

namespace InfluencerInstaParser.AudienceParser.InstagramClient.InstagramClientCreating
{
    public class ProxyClientCreator : IProxyClientCreator
    {
        private ConcurrentBag<IProxyClient> _proxyClients;
        private IList<WebProxy> _proxies;
        private IServiceProvider _serviceProvider;

        public ProxyClientCreator(IServiceProvider provider, IList<WebProxy> proxies)
        {
            _proxies = proxies;
            _serviceProvider = provider;
            Initialize();
        }

        private void Initialize()
        {
            _proxyClients = new ConcurrentBag<IProxyClient>();
            foreach (var webProxy in _proxies)
            {
                var handler = new HttpClientHandler {Proxy = webProxy};
                var client = _serviceProvider.GetService<IProxyClient>();
                _proxyClients.Add();
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