using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using NLog;

namespace InfluencerInstaParser.AudienceParser.Proxy
{
    public class ProxyFromFileCreatorSingleton : IProxyCreatorSingleton
    {
        private static ProxyFromFileCreatorSingleton _instance;

        private readonly Logger _logger;

        private readonly Queue<WebProxy> _proxyQueue;
        private bool _isQueueInit;
        private readonly Dictionary<WebProxy, DateTime> _usedProxy;

        private readonly TimeSpan _defaultDelayTime = TimeSpan.Parse("00:03:00");

        private ProxyFromFileCreatorSingleton()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _proxyQueue = new Queue<WebProxy>();
            _usedProxy = new Dictionary<WebProxy, DateTime>();
        }

        public static IProxyCreatorSingleton GetInstance()
        {
            return _instance ?? (_instance = new ProxyFromFileCreatorSingleton());
        }

        public WebProxy GetProxy()
        {
            if (!_proxyQueue.Any() && !_isQueueInit) FillQueue();
            if (!_proxyQueue.Any() && _isQueueInit) return GetFirstFreeProxy();
            return _proxyQueue.Dequeue();
        }

        public WebProxy GetProxy(WebProxy usedProxy)
        {
            SetProxyFree(usedProxy);
            return GetProxy();
        }

        public void SetProxyFree(WebProxy usedProxy)
        {
            if (!_usedProxy.TryAdd(usedProxy, DateTime.Now)) _usedProxy[usedProxy] = DateTime.Now;
        }

        private void FillQueue()
        {
            _isQueueInit = true;
            var proxyLines = File.ReadAllLines("proxies.txt", Encoding.UTF8);
            foreach (var line in proxyLines)
            {
                var proxyParams = line.Split(":");
                var proxyUri = $"http://{proxyParams[0]}:{proxyParams[1]}";
                var userName = proxyParams[2];
                var password = proxyParams[3];
                var proxy = new WebProxy
                {
                    Address = new Uri(proxyUri),
                    BypassProxyOnLocal = true,
                    Credentials = new NetworkCredential(userName, password)
                };
                _proxyQueue.Enqueue(proxy);
            }
        }

        private WebProxy GetFirstFreeProxy()
        {
            while (true)
            {
                foreach (var (proxy, lastUse) in _usedProxy)
                {
                    if (!IsProxyCanBeUsed(proxy, lastUse)) continue;
                    _usedProxy.Remove(proxy);
                    return proxy;
                }

                Thread.Sleep(1000);
            }
        }

        private bool IsProxyCanBeUsed(WebProxy proxy, DateTime lastUse)
        {
            if (_proxyQueue.Contains(proxy)) return false;
            var delay = DateTime.Now.Subtract(lastUse);
            return TimeSpan.Compare(delay, _defaultDelayTime) >= 0;
        }
    }
}