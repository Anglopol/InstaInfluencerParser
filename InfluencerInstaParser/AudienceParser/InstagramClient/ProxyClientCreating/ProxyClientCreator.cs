using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy;
using InfluencerInstaParser.AudienceParser.Proxy;
using JetBrains.Annotations;
using Serilog;

namespace InfluencerInstaParser.AudienceParser.InstagramClient.ProxyClientCreating
{
    public class ProxyClientCreator : IProxyClientCreator
    {
        private readonly ConcurrentDictionary<IProxyClient, DateTime> _proxyClients;
        private IEnumerable<IWebProxy> _proxies;
        private const int MaxValueOfRequests = 170;
        private static readonly TimeSpan ProxyClientRestTime = TimeSpan.FromMinutes(3);
        private readonly IProxyCreator _proxyCreator;
        private bool _isCreatorInit;
        private readonly ILogger _logger;

        public ProxyClientCreator(IProxyCreator creator, ILogger logger)
        {
            _logger = logger;
            _proxyCreator = creator;
            _proxyClients = new ConcurrentDictionary<IProxyClient, DateTime>();
            _isCreatorInit = false;
        }

        private void Initialize()
        {
            _isCreatorInit = true;
            _proxies = _proxyCreator.GetWebProxies();
            foreach (var webProxy in _proxies)
            {
                var handler = new HttpClientHandler {Proxy = webProxy};
                var client = new ProxyClient(handler, _logger);
                _proxyClients.TryAdd(client, client.GetLastUsageTime());
            }
        }

        public async Task<IProxyClient> GetClientAsync()
        {
            if(!_isCreatorInit) Initialize();
            foreach (var (client, _) in _proxyClients)
            {
                if (client.GetRequestCounter() < MaxValueOfRequests && _proxyClients.TryRemove(client, out _))
                    return client;
            }

            return await GetFirstValidProxyClient();
        }

        private async Task<IProxyClient> GetFirstValidProxyClient()
        {
            while (true)
            {
                var oldestDateOfUsage = DateTime.Now;
                var maxSubtract = TimeSpan.Zero;
                foreach (var (client, lastUsageTime) in _proxyClients)
                {
                    var curSubtract = oldestDateOfUsage.Subtract(lastUsageTime);
                    if (curSubtract > maxSubtract) maxSubtract = curSubtract;
                    if (!CheckDateTimeOnValid(lastUsageTime) || !_proxyClients.TryRemove(client, out _)) continue;
                    client.ResetRequestCounter();
                    return client;
                }
                await WaitFirstProxy(maxSubtract);
            }
        }

        private static Task WaitFirstProxy(TimeSpan maxSubtract)
        {
            if (maxSubtract == TimeSpan.Zero) maxSubtract = TimeSpan.FromSeconds(1);
            Thread.Sleep(ProxyClientRestTime.Subtract(maxSubtract));
            return Task.CompletedTask;
        }

        private static bool CheckDateTimeOnValid(DateTime lastUsageTime)
        {
            var delay = DateTime.Now.Subtract(lastUsageTime);
            return TimeSpan.Compare(delay, ProxyClientRestTime) >= 0;
        }

        public async Task<IProxyClient> GetClientAsync([NotNull] IProxyClient spentClient)
        {
            _proxyClients.TryAdd(spentClient, spentClient.GetLastUsageTime());
            return await GetClientAsync();
        }

        public void FreeTheClient([NotNull] IProxyClient proxyClient)
        {
            _proxyClients.TryAdd(proxyClient, proxyClient.GetLastUsageTime());
        }
    }
}