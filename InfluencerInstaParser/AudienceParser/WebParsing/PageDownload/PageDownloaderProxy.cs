using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.Proxy;
using NLog;

namespace InfluencerInstaParser.AudienceParser.WebParsing.PageDownload
{
    public class PageDownloaderProxy
    {
        private const string InstagramUrl = @"https://www.instagram.com";

        private readonly Logger _logger;
        private readonly IProxyCreatorSingleton _proxyCreator;
        private WebProxy _proxy;
        private HttpClient _proxyClient;
        private int _requestCounter;
        private int _errorCounter;

        public PageDownloaderProxy()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _proxyCreator = ProxyFromFileCreatorSingleton.GetInstance();
        }

        private WebProxy Proxy
        {
            set
            {
                _errorCounter = 0;
                _proxy = value;
                _proxyClient = new HttpClient(new HttpClientHandler {Proxy = value}, true);
                _requestCounter = 0;
                _logger.Info($"Proxy changed on {value.Address}");
            }
            get => _proxy;
        }

        public string GetPageContent(string url, string userAgent, string instGis = "")
        {
            if (Proxy == null) SetProxy(_proxyCreator.GetProxy());

            var link = InstagramUrl + url;
            _proxyClient.DefaultRequestHeaders.Clear();
            _proxyClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
            _proxyClient.DefaultRequestHeaders.Add("x-instagram-gis", instGis);

            try
            {
                if (_requestCounter > 180) SetProxy(_proxyCreator.GetProxy(Proxy));

                _logger.Info($"Getting url: {url}\nDownload counter: {_requestCounter}");
                var responseTask = Task.Run(async () => await GetResponseMessageAsync(link));
                responseTask.Wait();
                var response = responseTask.Result;
                _requestCounter++;

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return "";
                }

                response.EnsureSuccessStatusCode();
                var responseBodyTask = Task.Run(async () => await GetResponseBodyAsync(response));
                responseBodyTask.Wait();
                var responseBody = responseBodyTask.Result;
                Thread.Sleep(600);
                return responseBody;
            }
            catch (Exception e)
            {
                _errorCounter++;
                _logger.Error(e, e.Message + $"\nOn url: {url}\nWith proxy: {_proxy.Address}");

                if (_errorCounter > 3)
                {
                    SetProxy(_requestCounter > 2
                        ? _proxyCreator.GetProxy(Proxy)
                        : _proxyCreator.GetProxy());
                }

                Console.WriteLine("\nException Caught!   " + _requestCounter);
                Console.WriteLine($"on {url}");
                Thread.Sleep(1000);
                return GetPageContent(url, userAgent, instGis);
            }
        }

        public void SetProxyFree()
        {
            _proxyClient?.CancelPendingRequests();
            if (Proxy != null) _proxyCreator.SetProxyFree(Proxy);
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
            _proxyClient?.CancelPendingRequests();
            Proxy = proxy;
            Console.WriteLine("Proxy changed");
        }
    }
}