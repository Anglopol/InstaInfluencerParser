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
        private HttpClientHandler _httpClientHandler;
        private HttpClient _proxyClient;
        private ProxyCreatorSingleton _proxyCreatorSingleton;

        public WebProxy Proxy
        {
            private set
            {
                _proxy = value;
                _httpClientHandler.Proxy = value;
                _proxyClient = new HttpClient(handler: _httpClientHandler, disposeHandler: true);
            }
            get => _proxy;
        }

        public PageDownloaderProxy()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _httpClientHandler = new HttpClientHandler();
            _proxyCreatorSingleton = ProxyCreatorSingleton.GetInstance();
        }

        public string GetPageContentWithProxy(string url, string userAgent, string instGis = "")
        {
            if (Proxy == null)
            {
                SetProxy(_proxyCreatorSingleton.GetProxy());
            }

            var link = InstagramUrl + url;
            _proxyClient.DefaultRequestHeaders.Clear();
            _proxyClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
            _proxyClient.DefaultRequestHeaders.Add("x-instagram-gis", instGis);

            try
            {
                if (_requestCounter > 180)
                {
                    SetProxy(_proxyCreatorSingleton.GetProxy());
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
                SetProxy(_proxyCreatorSingleton.GetProxy());
                Console.WriteLine("\nException Caught!   " + _requestCounter);
                Console.WriteLine($"on {url}");
                Thread.Sleep(1000);
                return Task.Run(() => GetPageContentWithProxy(url, userAgent, instGis)).GetAwaiter().GetResult();
            }
        }

        private void SetProxy(WebProxy proxy)
        {
            _requestCounter = 0;
            Proxy = proxy;
            Console.WriteLine("Proxy changed");
            _logger.Info($"Proxy changed on {proxy.Address}");
        }
    }
}