using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy
{
    public class ProxyClient : IProxyClient
    {
        private int _requestCounter;
        private DateTime _timeOfLastUsage;
        private readonly HttpClient _httpClient;
        private readonly TimeSpan _defaultTimeSpanBetweenRequests;
        private const string DefaultUserAgent =
            "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";

        public ProxyClient(HttpClientHandler httpClientHandler)
        {
            _timeOfLastUsage = DateTime.MinValue;
            _requestCounter = 0;
            _httpClient = new HttpClient(httpClientHandler, true);
            _defaultTimeSpanBetweenRequests = TimeSpan.FromMilliseconds(600);
        }

        public string GetPageContent(string pageUrl)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(DefaultUserAgent);
            WaitIfRequired();
            try
            {
                var response = GetResponse(pageUrl);
                _requestCounter++;
                if (response.StatusCode == HttpStatusCode.NotFound) return "";
                response.EnsureSuccessStatusCode();
                var responseBody = GetResponseBody(response);
                _httpClient.CancelPendingRequests();
                return responseBody;
            }
            catch (HttpRequestException)
            {
                _httpClient.CancelPendingRequests();
                return GetPageContent(pageUrl);
            }
            catch (TaskCanceledException)
            {
                _httpClient.CancelPendingRequests();
                return GetPageContent(pageUrl);
            }
            catch (Exception)
            {
                _httpClient.CancelPendingRequests();
                return "";
            }
        }

        public string GetPageContent(string pageUrl, string userAgent)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
            return GetPageContent(pageUrl);
        }

        private HttpResponseMessage GetResponse(string pageUrl)
        {
            var responseTask = Task.Run(async () => await GetResponseMessageAsync(pageUrl));
            _timeOfLastUsage = DateTime.Now;
            responseTask.Wait();
            return responseTask.Result;
        }

        private static string GetResponseBody(HttpResponseMessage response)
        {
            var responseBodyTask = Task.Run(async () => await GetResponseBodyAsync(response));
            responseBodyTask.Wait();
            return responseBodyTask.Result;
        }

        private async Task<HttpResponseMessage> GetResponseMessageAsync(string link)
        {
            return await _httpClient.GetAsync(link, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);
        }

        private static async Task<string> GetResponseBodyAsync(HttpResponseMessage responseMessage)
        {
            return await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private void WaitIfRequired()
        {
            var delay = DateTime.Now.Subtract(_timeOfLastUsage);
            if (TimeSpan.Compare(delay, _defaultTimeSpanBetweenRequests) >= 0)
                Thread.Sleep(delay.Subtract(_defaultTimeSpanBetweenRequests));
        }

        public int GetRequestCounter()
        {
            return _requestCounter;
        }

        public void ResetRequestCounter()
        {
            _requestCounter = 0;
        }

        public DateTime GetLastUsageTime()
        {
            return _timeOfLastUsage;
        }
    }
}