using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy;
using JetBrains.Annotations;

namespace InfluencerInstaParser.AudienceParser.InstagramClient.ProxyClientCreating
{
    public class ProxyClientCreator : IProxyClientCreator
    {
        private ConcurrentDictionary<IProxyClient, DateTime> _proxyClients;
        private readonly IList<IWebProxy> _proxies;
        private const int MaxValueOfRequests = 170;
        private static readonly TimeSpan ProxyClientRestTime = TimeSpan.FromMinutes(3);

        public ProxyClientCreator([NotNull] IList<IWebProxy> proxies)
        {
            _proxyClients = new ConcurrentDictionary<IProxyClient, DateTime>();
            _proxies = proxies;
            Initialize();
        }

        private void Initialize()
        {
            foreach (var webProxy in _proxies)
            {
                var handler = new HttpClientHandler {Proxy = webProxy};
                var client = new ProxyClient(handler);
                _proxyClients.TryAdd(client, client.GetLastUsageTime());
            }
        }

        public async Task<IProxyClient> GetClientAsync()
        {
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