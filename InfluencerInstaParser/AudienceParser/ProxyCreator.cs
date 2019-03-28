using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.WebParsing;

namespace InfluencerInstaParser.AudienceParser
{
    public class ProxyCreator
    {
        private ConcurrentQueue<WebProxy> _proxyQueue;
        private readonly string[] _proxyParams;
        private readonly string _proxyUrl;
        private readonly object _fillLocker;

        public ProxyCreator(string proxyUrl, string[] proxyParams)
        {
            _proxyParams = proxyParams;
            _proxyUrl = proxyUrl;
            _fillLocker = new object();
        }

        public WebProxy GetProxy()
        {
            if (_proxyQueue == null || _proxyQueue.IsEmpty) FillQueue();
            WebProxy proxy;
            while (!_proxyQueue.TryDequeue(out proxy))
            {
            }

            return proxy;
        }

        private void FillQueue()
        {
            lock (_fillLocker)
            {
                if (!(_proxyQueue == null || _proxyQueue.IsEmpty)) return;
                if (_proxyQueue == null) _proxyQueue = new ConcurrentQueue<WebProxy>();
                var requestUrl = _proxyParams.Aggregate(_proxyUrl, (current, param) => current + (param + "&"));
                var downloader = PageDownloader.GetInstance();
                var jsonHandler = new JObjectHandler();
                var jsonProxies = jsonHandler.GetObjectFromJsonString(Task
                    .Run(() => downloader.GetPageContent(requestUrl))
                    .GetAwaiter().GetResult());
                var ipAddresses = jsonHandler.GetProxyIps(jsonProxies);
                var ports = jsonHandler.GetProxyPorts(jsonProxies);
                for (var i = 0; i < ipAddresses.Count; i++)
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
}