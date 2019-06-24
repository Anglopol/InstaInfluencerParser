using System;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy;
using InfluencerInstaParser.AudienceParser.InstagramClient.ProxyClientCreating;
using Microsoft.Extensions.DependencyInjection;

namespace InfluencerInstaParser.AudienceParser.WebParsing.PageDownload
{
    public class PageDownloader :  IPageDownloader
    {
        private readonly IServiceProvider _serviceProvider;
        private IProxyClient _client;
        private IProxyClientCreator _proxyClientCreator;
        public PageDownloader(IServiceProvider provider)
        {
            _serviceProvider = provider;
        }
        
        public string GetPage(string pageUrl)
        {
            CheckClient();
            return _client.GetPageContent(pageUrl);
        }

        public void SetClientFree(IProxyClient proxyClient)
        {
            _proxyClientCreator.FreeTheClient(proxyClient);
        }

        private void CheckClient()
        {
            if(_client == null) InitializeClient();
            if(_client.GetRequestCounter() >= 170) RefreshClient();
        }

        private void InitializeClient()
        {
            _proxyClientCreator = _serviceProvider.GetService<IProxyClientCreator>();
            RefreshClient();
        }

        private void RefreshClient()
        {
            if (_client == null)
            {
                _client = GetClient();
                return;
            }
            _client = GetClient(_client);
        }

        private IProxyClient GetClient()
        {
            var clientTask = Task.Run(async () => await _proxyClientCreator.GetClientAsync());
            clientTask.Wait();
            return clientTask.Result;
        }
        
        private IProxyClient GetClient(IProxyClient proxyClient)
        {
            var clientTask = Task.Run(async () => await _proxyClientCreator.GetClientAsync(proxyClient));
            clientTask.Wait();
            return clientTask.Result;
        }
    }
}