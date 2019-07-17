using System;
using System.Net;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy;
using InfluencerInstaParser.AudienceParser.InstagramClient.ProxyClientCreating;
using Serilog;

namespace InfluencerInstaParser.AudienceParser.Proxy.PageDownload
{
    public class PageDownloader :  IPageDownloader
    {
        private IProxyClient _client;
        private readonly IProxyClientCreator _proxyClientCreator;
        private const int MaxValueOfRequests = 180;
        private readonly ILogger _logger;
        public PageDownloader(IProxyClientCreator clientCreator, ILogger logger)
        {
            _proxyClientCreator = clientCreator;
            _logger = logger;
        }
        
        public string GetPageContent(string pageUrl)
        {
            CheckClient();
            var response =  _client.GetResponse(pageUrl);
            return StatusCodeCheck(response, pageUrl);
        }

        public void SetClientFree()
        {
            _proxyClientCreator.FreeTheClient(_client);
        }

        private string StatusCodeCheck(ProxyClientResponse response, string pageUrl)
        {
            switch (response.Code)
            {
                case HttpStatusCode.OK: return response.PageContent;
                case HttpStatusCode.TooManyRequests:
                    PutTheClientToSleep();
                    return GetPageContent(pageUrl);
                default: return GetPageContent(pageUrl);
            }
        }

        private void CheckClient()
        {
            if(_client == null) RefreshClient();
            if(_client.RequestCounter >= MaxValueOfRequests) RefreshClient();
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
            _logger.Verbose("Getting new client");
            var clientTask = Task.Run(async () => await _proxyClientCreator.GetClientAsync());
            clientTask.Wait();
            _logger.Verbose("Client getting complete");
            return clientTask.Result;
        }
        
        private IProxyClient GetClient(IProxyClient proxyClient)
        {
            var clientTask = Task.Run(async () => await _proxyClientCreator.GetClientAsync(proxyClient));
            clientTask.Wait();
            return clientTask.Result;
        }

        private void PutTheClientToSleep()
        {
            _client.OverloadRequestCounter();
        }

        private void ReleaseUnmanagedResources()
        {
            SetClientFree();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~PageDownloader()
        {
            ReleaseUnmanagedResources();
        }
    }
}