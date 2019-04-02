using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class PageDownloaderProxy
    {
        private Logger _logger;
        private const string InstagramUrl = @"https://www.instagram.com";
        private int _requestCounter;
        private WebProxy _proxy;
        private HttpClient _proxyClient;
        private ProxyCreatorSingleton _proxyCreatorSingleton;

        private static readonly object GetProxyLocker = new object();

        private WebProxy Proxy
        {
            set
            {
                _proxy = value;
                _proxyClient = new HttpClient(new HttpClientHandler {Proxy = value}, true);
                _requestCounter = 0;
                _logger.Info($"Proxy changed on {value.Address}");
            }
            get => _proxy;
        }

        public PageDownloaderProxy()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _proxyCreatorSingleton = ProxyCreatorSingleton.GetInstance();
        }

        public string GetPageContentWithProxy(string url, string userAgent, string instGis = "")
        {
            if (Proxy == null)
            {
                lock (GetProxyLocker)
                {
                    SetProxy(_proxyCreatorSingleton.GetProxy());
                }
            }

            var link = InstagramUrl + url;
            _proxyClient.DefaultRequestHeaders.Clear();
            _proxyClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
            _proxyClient.DefaultRequestHeaders.Add("x-instagram-gis", instGis);

            try
            {
                if (_requestCounter > 180)
                {
                    lock (GetProxyLocker)
                    {
                        SetProxy(_proxyCreatorSingleton.GetProxy());
                    }
                }

                _logger.Info($"Getting url: {url}\nDownload counter: {_requestCounter}");
                var response = Task.Run(() => _proxyClient.GetAsync(link)).GetAwaiter().GetResult();
                _requestCounter++;

                response.EnsureSuccessStatusCode();
                var responseBody = Task.Run(() => response.Content.ReadAsStringAsync()).GetAwaiter().GetResult();
                Thread.Sleep(600);
                return responseBody;
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message + $"\nOn url: {url}\nWith proxy: {_proxy.Address}");
                lock (GetProxyLocker)
                {
                    SetProxy(_proxyCreatorSingleton.GetProxy());
                }

                Console.WriteLine("\nException Caught!   " + _requestCounter);
                Console.WriteLine($"on {url}");
                Thread.Sleep(1000);
                return Task.Run(() => GetPageContentWithProxy(url, userAgent, instGis)).GetAwaiter().GetResult();
            }
        }

        private void SetProxy(WebProxy proxy)
        {
            _proxyClient?.CancelPendingRequests();
            Proxy = proxy;
            Console.WriteLine("Proxy changed");
        }
    }
}