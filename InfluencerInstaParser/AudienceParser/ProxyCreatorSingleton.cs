using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.WebParsing;
using NLog;

namespace InfluencerInstaParser.AudienceParser
{
    public class ProxyCreatorSingleton
    {
        private const string DefaultProxyUrl = "https://api.getproxylist.com/proxy?";
        private const string DefaultApiKey = "deeb3af2c0618ff4f4229b1fa30a1b866f022b0f";
        private static ProxyCreatorSingleton _instance;

        private static readonly string[] DefaultProxyParams =
        {
            $"apiKey={DefaultApiKey}", "protocol=http", "allowsUserAgentHeader=1", "allowsCustomHeaders=1",
            "minDownloadSpeed=800", "anonymity[]=high%20anonymity", "allowsHttps=1", "all=1"
        };

        private readonly TimeSpan _defaultDelayTime = TimeSpan.Parse("00:03:00");
        private readonly object _fillLocker;

        private readonly Logger _logger;

        private readonly Queue<WebProxy> _proxyQueue;
        private readonly Dictionary<WebProxy, DateTime> _usedProxy;
        private string[] _proxyParams;
        private string _proxyUrl;

        private ProxyCreatorSingleton()
        {
            _fillLocker = new object();
            _logger = LogManager.GetCurrentClassLogger();
            _proxyParams = DefaultProxyParams;
            _proxyUrl = DefaultProxyUrl;
            _proxyQueue = new Queue<WebProxy>();
            _usedProxy = new Dictionary<WebProxy, DateTime>();
        }

        public static ProxyCreatorSingleton GetInstance()
        {
            return _instance ?? (_instance = new ProxyCreatorSingleton());
        }

        public void SetNewProxyRotator(string proxyUrl, string[] proxyParams)
        {
            _proxyParams = proxyParams;
            _proxyUrl = proxyUrl;
        }

        public WebProxy GetProxy()
        {
            if (_proxyQueue.Count == 0) FillQueue();


            var proxy = _proxyQueue.Dequeue();
            return proxy;
        }

        public WebProxy GetProxy(WebProxy usedProxy)
        {
            if (!_usedProxy.TryAdd(usedProxy, DateTime.Now)) _usedProxy[usedProxy] = DateTime.Now;

            if (_proxyQueue.Count == 0)
                lock (_fillLocker)
                {
                    if (_proxyQueue.Count == 0) FillQueue();
                }

            var proxy = _proxyQueue.Dequeue();
            return proxy;
        }

        private void FillQueue()
        {
            if (_proxyQueue.Count != 0) return;
            _logger.Info("Downloading new proxys");
            var requestUrl = _proxyParams.Aggregate(_proxyUrl, (current, param) => current + param + "&");
            var jsonHandler = new JObjectHandler();
            var jsonProxies = jsonHandler.GetObjectFromJsonString(Task
                .Run(() => PageDownloader.GetPageContent(requestUrl))
                .GetAwaiter().GetResult());
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
        }

        private bool IsProxyCanBeUsed(WebProxy proxy)
        {
            if (!_usedProxy.ContainsKey(proxy)) return true;
            var delay = DateTime.Now.Subtract(_usedProxy[proxy]);
            return TimeSpan.Compare(delay, _defaultDelayTime) >= 0;
        }
    }
}