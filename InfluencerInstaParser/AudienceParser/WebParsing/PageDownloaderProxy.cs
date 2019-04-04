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
        private const string InstagramUrl = @"https://www.instagram.com";

        private static readonly object GetProxyLocker = new object();

        private readonly Logger _logger;
        private readonly ProxyCreatorSingleton _proxyCreatorSingleton;
        private WebProxy _proxy;
        private HttpClient _proxyClient;
        private int _requestCounter;

        public PageDownloaderProxy()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _proxyCreatorSingleton = ProxyCreatorSingleton.GetInstance();
        }

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

        public string GetPageContentWithProxy(string url, string userAgent, string instGis = "")
        {
            if (Proxy == null) SetProxy(_proxyCreatorSingleton.GetProxy());

            var link = InstagramUrl + url;
            _proxyClient.DefaultRequestHeaders.Clear();
            _proxyClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
            _proxyClient.DefaultRequestHeaders.Add("x-instagram-gis", instGis);

            try
            {
                if (_requestCounter > 180) SetProxy(_proxyCreatorSingleton.GetProxy(Proxy));

                _logger.Info($"Getting url: {url}\nDownload counter: {_requestCounter}");
                var responseTask = Task.Run(async () => await GetResponseMessageAsync(link));
                responseTask.Wait();
                var response = responseTask.Result;
                _requestCounter++;
                response.EnsureSuccessStatusCode();
                var responseBodyTask = Task.Run(async () => await GetResponseBodyAsync(response));
                responseBodyTask.Wait();
                var responseBody = responseBodyTask.Result;
                Thread.Sleep(600);
                return responseBody;
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message + $"\nOn url: {url}\nWith proxy: {_proxy.Address}");
                SetProxy(_requestCounter > 2
                    ? _proxyCreatorSingleton.GetProxy(Proxy)
                    : _proxyCreatorSingleton.GetProxy());
                Console.WriteLine("\nException Caught!   " + _requestCounter);
                Console.WriteLine($"on {url}");
                Thread.Sleep(1000);
                return GetPageContentWithProxy(url, userAgent, instGis);
            }
        }

        private async Task<HttpResponseMessage> GetResponseMessageAsync(string link)
        {
            return await _proxyClient.GetAsync(link).ConfigureAwait(false);
        }

        private async Task<string> GetResponseBodyAsync(HttpResponseMessage responseMessage)
        {
            return await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private void SetProxy(WebProxy proxy)
        {
            lock (GetProxyLocker)
            {
                _proxyClient?.CancelPendingRequests();
                Proxy = proxy;
                Console.WriteLine("Proxy changed");
            }
        }
    }
}