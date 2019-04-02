using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InfluencerInstaParser.Exceptions;
using NLog;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class PageDownloader
    {
        private static PageDownloader _instance;

        private Logger _logger;

        private const string InstagramUrl = @"https://www.instagram.com";

        private static int _proxySetterCounter;
        private static int _requestCounter;


        private WebProxy _proxy;
        private ProxyCreator _proxyCreator;
        private object _headersLocker;

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

        private HttpClientHandler _httpClientHandler;
        private HttpClient _proxyClient;
        private readonly HttpClient _client;
        private readonly object _proxyChangerLock;

        private PageDownloader()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _headersLocker = new object();
            _httpClientHandler = new HttpClientHandler();
            _proxyChangerLock = new object();
            _client = new HttpClient();
            _proxyCreator = new ProxyCreator();
        }

        public static PageDownloader GetInstance()
        {
            return _instance ?? (_instance = new PageDownloader());
        }

        public async Task<string> GetPageContent(string url)
        {
            try
            {
                var response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return await GetPageContent(url);
            }
        }

        public async Task<string> GetPageContentWithProxy(string url, string userAgent, string instGis = "")
        {
            if (Proxy == null)
            {
                lock (_proxyChangerLock)
                {
                    if (Proxy == null)
                    {
                        SetProxy(_proxyCreator.GetProxy());
                        _proxySetterCounter++;
                    }
                }
            }

            var link = InstagramUrl + url;
            lock (_headersLocker)
            {
                _proxyClient.DefaultRequestHeaders.Clear();
                _proxyClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
                _proxyClient.DefaultRequestHeaders.Add("x-instagram-gis", instGis);
            }

            try
            {
                if (_requestCounter > 180)
                {
                    lock (_proxyChangerLock)
                    {
                        if (_requestCounter > 180) SetProxy(_proxyCreator.GetProxy());
                        _proxySetterCounter++;
                    }
                }

                _logger.Info($"Getting url: {url}\nDownload counter: {_requestCounter}");
                var response = await _proxyClient.GetAsync(link);
                _requestCounter++;

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message + $"\nOn url: {url}\nWith proxy: {_proxy.Address}");
                var checkCounter = _proxySetterCounter;
                lock (_proxyChangerLock)
                {
                    if (checkCounter == _proxySetterCounter)
                    {
                        SetProxy(_proxyCreator.GetProxy());
                        _proxySetterCounter++;
                    }
                }
                
                Console.WriteLine("\nException Caught!   " + _requestCounter);
                Console.WriteLine($"on {url}");
                Thread.Sleep(1000);
                return await GetPageContentWithProxy(url, userAgent, instGis);
            }
        }

        private void SetProxy(WebProxy proxy)
        {
            if (_requestCounter <= 180 && Proxy != null) return;
            _requestCounter = 0;
            Proxy = proxy;
            Console.WriteLine("Proxy changed");
            _logger.Info($"Proxy changed on {proxy.Address}");
        }
    }
}