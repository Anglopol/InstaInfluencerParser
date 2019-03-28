using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InfluencerInstaParser.Exceptions;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class PageDownloader
    {
        private static PageDownloader _instance;

        private const string InstagramUrl = @"https://www.instagram.com";
        private static int _requestCounter;
        private WebProxy _proxy;
        private ProxyCreator _proxyCreator;

        public WebProxy Proxy
        {
            set
            {
                _proxy = value;
                _httpClientHandler.Proxy = value;
                _proxyClient = new HttpClient(handler: _httpClientHandler, disposeHandler: true);
            }
            get => _proxy;
        }

        private HttpClientHandler _httpClientHandler;
        private HttpClient _proxyClient;
        private HttpClient _client;
        private readonly object _proxyChangerLock;

        private PageDownloader()
        {
            _httpClientHandler = new HttpClientHandler();
            _proxyChangerLock = new object();
            _client = new HttpClient();
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
            if (Proxy == null) throw new ProxyNotInitializeException("You need to set proxy to use this method");
            var link = InstagramUrl + url;

            _proxyClient.DefaultRequestHeaders.Clear();
            _proxyClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
            _proxyClient.DefaultRequestHeaders.Add("x-instagram-gis", instGis);

            try
            {
                _requestCounter++;
                if (_requestCounter > 190)
                {
                    SetProxy(null); //TODO refactor
                }

                var response = await _proxyClient.GetAsync(link);
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                Console.Write(" " + _requestCounter);
                return body;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!   " + _requestCounter);
                Console.WriteLine("Message :{0} ", e.Message);
                Thread.Sleep(180000);
                return await GetPageContentWithProxy(url, userAgent, instGis);
            }
        }

        private void SetProxy(WebProxy proxy)
        {
            lock (_proxyChangerLock)
            {
                if (_requestCounter <= 190) return;
                _requestCounter = 0;
                Proxy = proxy;
                Console.WriteLine("Proxy changed");
            }
        }
    }
}