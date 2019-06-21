using System;
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

        public ProxyClient(HttpClientHandler httpClientHandler)
        {
            _timeOfLastUsage = DateTime.MinValue;
            _requestCounter = 0;
            _httpClient = new HttpClient(httpClientHandler, true);
            _defaultTimeSpanBetweenRequests = TimeSpan.FromMilliseconds(600);
        }

        public string GetPageContent(string pageUrl)
        {
            WaitIfRequired();
            try
            {
                var response = GetResponse(pageUrl);
                _requestCounter++;
                response.EnsureSuccessStatusCode();
                var responseBody = GetResponseBody(response);
                _httpClient.CancelPendingRequests();
                return responseBody;
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

        private HttpResponseMessage GetResponse(string pageUrl)
        {
            var responseTask = Task.Run(async () => await GetResponseMessageAsync(pageUrl));
            _timeOfLastUsage = DateTime.Now;
            responseTask.Wait();
            return responseTask.Result;
        }

        private string GetResponseBody(HttpResponseMessage response)
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