using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using InfluencerInstaParser.AudienceParser.WebParsing;
using NLog;

namespace InfluencerInstaParser.AudienceParser.Proxy
{
    public class ProxyRotatorCreatorSingleton : IProxyCreatorSingleton
    {
        private const string DefaultProxyUrl = "http://falcon.proxyrotator.com:51337/?";
        private const string DefaultApiKey = "7fekhDEoU2dPvJVpYryzCgFbRqtSQsnw";
        private static ProxyRotatorCreatorSingleton _instance;

        private static readonly string[] DefaultProxyParams =
        {
            $"apiKey={DefaultApiKey}", "referer=true", "userAgent=true", "get=true"
        };

        private readonly TimeSpan _defaultDelayTime = TimeSpan.Parse("00:03:00");

        private readonly Logger _logger;
        private readonly PageDownloader _pageDownloader;

        private readonly string[] _proxyParams;
        private readonly Queue<WebProxy> _proxyQueue;
        private readonly string _proxyUrl;
        private readonly Dictionary<WebProxy, DateTime> _usedProxy;
        private readonly WebProcessor _webProcessor;

        private ProxyRotatorCreatorSingleton()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _proxyParams = DefaultProxyParams;
            _proxyUrl = DefaultProxyUrl;
            _proxyQueue = new Queue<WebProxy>();
            _usedProxy = new Dictionary<WebProxy, DateTime>();
            _pageDownloader = new PageDownloader();
            _webProcessor = new WebProcessor();
        }

        public static IProxyCreatorSingleton GetInstance()
        {
            return _instance ?? (_instance = new ProxyRotatorCreatorSingleton());
        }

        public WebProxy GetProxy()
        {
            if (_proxyQueue.Count == 0)
                if (!FillQueue())
                    return GetFirstValidProxy();

            var proxy = _proxyQueue.Dequeue();
            return proxy;
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

        private bool FillQueue()
        {
            if (_proxyQueue.Count != 0) return true;
            _logger.Info("Downloading new proxys");
            var requestUrl = _proxyParams.Aggregate(_proxyUrl, (current, param) => current + param + "&");
            var pageContent = _pageDownloader.GetPageContent(requestUrl);
            if (!_webProcessor.IsProxyListAvailable(pageContent)) return false;
            var proxies = _webProcessor.GetListOfProxies(pageContent);
            foreach (var uri in proxies)
            {
                var proxy = new WebProxy
                {
                    Address = new Uri($"http://{uri}"),
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

        private WebProxy GetFirstValidProxy()
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
            var requestUrl = _proxyParams.Aggregate(_proxyUrl, (current, param) => current + param + "&");
            var pageContent = _pageDownloader.GetPageContent(requestUrl);
            var jsonHandler = new JObjectHandler();
            var jsonProxy = jsonHandler.GetObjectFromJsonString(pageContent);
            var proxyString = jsonHandler.GetProxyString(jsonProxy);
            return new WebProxy
            {
                Address = new Uri($"http://{proxyString}"),
                UseDefaultCredentials = true,
                BypassProxyOnLocal = false
            };
        }
    }
}