using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.WebParsing;
using NLog;

namespace InfluencerInstaParser.AudienceParser
{
    public class ProxyCreator
    {
        private const string DefaultProxyUrl = "https://api.getproxylist.com/proxy?";
        private const string DefaultApiKey = "deeb3af2c0618ff4f4229b1fa30a1b866f022b0f";

        private static readonly string[] DefaultProxyParams =
        {
            $"apiKey={DefaultApiKey}", "protocol=http", "allowsUserAgentHeader=1", "allowsCustomHeaders=1",
            "minDownloadSpeed=1000", "anonymity[]=high%20anonymity", "allowsHttps=1", "all=1"
        };

        private Logger _logger;
        
        private Queue<WebProxy> _proxyQueue;
        private readonly string[] _proxyParams;
        private readonly string _proxyUrl;

        public ProxyCreator(string proxyUrl, string[] proxyParams)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _proxyParams = proxyParams;
            _proxyUrl = proxyUrl;
            _proxyQueue = new Queue<WebProxy>();
        }

        public ProxyCreator()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _proxyParams = DefaultProxyParams;
            _proxyUrl = DefaultProxyUrl;
            _proxyQueue = new Queue<WebProxy>();
        }

        public WebProxy GetProxy()
        {
            if (_proxyQueue.Count == 0) FillQueue();
            var proxy = _proxyQueue.Dequeue();
            return proxy;
        }

        private void FillQueue()
        {
            if (_proxyQueue.Count != 0) return;
            _logger.Info("Downloading new proxys");
            var requestUrl = _proxyParams.Aggregate(_proxyUrl, (current, param) => current + (param + "&"));
            var downloader = PageDownloader.GetInstance();
            var jsonHandler = new JObjectHandler();
            var jsonProxies = jsonHandler.GetObjectFromJsonString(Task
                .Run(() => downloader.GetPageContent(requestUrl))
                .GetAwaiter().GetResult());
            var ipAddresses = jsonHandler.GetProxyIps(jsonProxies);
            var ports = jsonHandler.GetProxyPorts(jsonProxies);
            for (var i = 1; i < ipAddresses.Count; i++)
            {
                var ip = ipAddresses[i];
                var port = ports[i];
                var proxy = new WebProxy()
                {
                    Address = new Uri($"http://{ip}:{port}"),
                    UseDefaultCredentials = true,
                    BypassProxyOnLocal = false
                };
                _proxyQueue.Enqueue(proxy);
            }
        }
    }
}