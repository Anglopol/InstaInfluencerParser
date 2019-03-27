using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class PageDownloader
    {
        private static PageDownloader _instance;

        private const string InstagramUrl = @"https://www.instagram.com";
        private static int _requestCounter = 0;
        private WebProxy _proxy;

        public WebProxy Proxy
        {
            set
            {
                _proxy = value;
                _httpClientHandler.Proxy = value;
                _client = new HttpClient(handler: _httpClientHandler, disposeHandler: true);
            }
            get => _proxy;
        }

        private HttpClientHandler _httpClientHandler;
        private HttpClient _client;
        private object _proxyChangerLock;

        private PageDownloader()
        {
            _httpClientHandler = new HttpClientHandler();
            _proxyChangerLock = new object();
            _client = new HttpClient(handler: _httpClientHandler, disposeHandler: true);
        }

        public static PageDownloader GetInstance()
        {
            return _instance ?? (_instance = new PageDownloader());
        }

        public async Task<string> GetPageContent(string url, string userAgent, string instGis = "")
        {
            var link = InstagramUrl + url;


            _client.DefaultRequestHeaders.Clear();

            _client.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
            _client.DefaultRequestHeaders.Add("x-instagram-gis", instGis);

            try
            {
                var response = await _client.GetAsync(link);
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                Console.Write(" " + _requestCounter);
                return body;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!   " + _requestCounter);
                Console.WriteLine("Message :{0} ", e.Message);
                Thread.Sleep(1000);
                return await GetPageContent(url, userAgent, instGis);
            }
        }

        public async Task<string> GetPageContentWithProxy(string url, string userAgent, string instGis = "")
        {
            var link = InstagramUrl + url;

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
            _client.DefaultRequestHeaders.Add("x-instagram-gis", instGis);

            try
            {
                _requestCounter++;
                if (_requestCounter > 190)
                {
                    ChangeProxy(null); //TODO refactor
                }

                var response = await _client.GetAsync(link);
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
                return await GetPageContent(url, userAgent, instGis);
            }
        }

        private void ChangeProxy(WebProxy proxy)
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