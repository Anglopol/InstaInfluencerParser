using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using InfluencerInstaParser.AudienceParser.WebParsing;
using NLog;

namespace InfluencerInstaParser.AudienceParser
{
    public class ProxyCreatorSingleton
    {
        private const string DefaultProxyUrl = "http://falcon.proxyrotator.com:51337/?";
        private const string DefaultApiKey = "7fekhDEoU2dPvJVpYryzCgFbRqtSQsnw";
        private static ProxyCreatorSingleton _instance;

        private static readonly string[] DefaultProxyParams =
        {
            $"apiKey={DefaultApiKey}", "referer=true", "userAgent=true", "get=true"
        };

        private readonly TimeSpan _defaultDelayTime = TimeSpan.Parse("00:03:00");

        private readonly Logger _logger;
        private readonly PageDownloader _pageDownloader;
        private readonly Queue<WebProxy> _proxyQueue;
        private readonly Dictionary<WebProxy, DateTime> _usedProxy;
        private readonly WebProcessor _webProcessor;

        private readonly string[] _proxyParams;
        private readonly string _proxyUrl;

        private ProxyCreatorSingleton()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _proxyParams = DefaultProxyParams;
            _proxyUrl = DefaultProxyUrl;
            _proxyQueue = new Queue<WebProxy>();
            _usedProxy = new Dictionary<WebProxy, DateTime>();
            _pageDownloader = new PageDownloader();
            _webProcessor = new WebProcessor();
        }

        public static ProxyCreatorSingleton GetInstance()
        {
            return _instance ?? (_instance = new ProxyCreatorSingleton());
        }

        public WebProxy GetProxy()
        {
            if (_proxyQueue.Count == 0)
            {
                FillQueue();
                if (_proxyQueue.Count == 0) return GetFirstValidProxyFromUsedSet();
            }

            var proxy = _proxyQueue.Dequeue();
            return proxy;
        }

        public WebProxy GetProxy(WebProxy usedProxy)
        {
            if (!_usedProxy.TryAdd(usedProxy, DateTime.Now)) _usedProxy[usedProxy] = DateTime.Now;
            return GetProxy();
        }

        private bool FillQueue()
        {
            if (_proxyQueue.Count != 0) return true;
            _logger.Info("Downloading new proxys");
            var requestUrl = _proxyParams.Aggregate(_proxyUrl, (current, param) => current + param + "&");
            var pageContent = _pageDownloader.GetPageContent(requestUrl);
            if (!_webProcessor.IsProxyListAvailable(pageContent)) return false;
            var jsonHandler = new JObjectHandler();
            var jsonProxies = jsonHandler.GetObjectFromJsonString(pageContent);
            var ipAddresses = jsonHandler.GetProxyIps(jsonProxies);
            var ports = jsonHandler.GetProxyPorts(jsonProxies);
            for (var i = 1; i < ipAddresses.Count; i++)
            {
                var ip = ipAddresses[i];
                var port = ports[i];
                var proxy = new WebProxy
                {
                    Address = new Uri($"http://{ip}:{port}"),
                    UseDefaultCredentials = true,
                    BypassProxyOnLocal = false
                };
                if (IsProxyCanBeUsed(proxy)) _proxyQueue.Enqueue(proxy);
            }

            _logger.Info($"Added {_proxyQueue.Count} proxies");
            return _proxyQueue.Count != 0;
        }

        private bool IsProxyCanBeUsed(WebProxy proxy)
        {
            if (!_usedProxy.ContainsKey(proxy)) return true;
            if (_proxyQueue.Contains(proxy)) return false;
            var delay = DateTime.Now.Subtract(_usedProxy[proxy]);
            return TimeSpan.Compare(delay, _defaultDelayTime) >= 0;
        }

        private WebProxy GetFirstValidProxyFromUsedSet()
        {
            if (_usedProxy.Count == 0) return GetRandomProxyFromRotator();
            while (true)
            {
                foreach (var (proxy, _) in _usedProxy)
                {
                    if (!IsProxyCanBeUsed(proxy)) continue;
                    _usedProxy.Remove(proxy);
                    return proxy;
                }

                Thread.Sleep(1000);
            }
        }

        private WebProxy GetRandomProxyFromRotator()
        {
        }
    }
}