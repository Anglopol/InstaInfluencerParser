using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy
{
    public class ProxyClient : IProxyClient
    {
        private readonly ILogger _logger;
        private DateTime _timeOfLastUsage;
        private readonly HttpClient _httpClient;
        private readonly TimeSpan _defaultTimeSpanBetweenRequests;

        private const string DefaultUserAgent =
            "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";

        private const int MaxTimeSpanForRequestInMin = 2;
        private const int TimeSpanBetweenRequestsInMs = 1000;

        public int RequestCounter { get; private set; }


        public ProxyClient(HttpClientHandler httpClientHandler, ILogger logger)
        {
            _logger = logger;
            _timeOfLastUsage = DateTime.MinValue;
            RequestCounter = 0;
            _httpClient = new HttpClient(httpClientHandler, true)
            {
                Timeout = TimeSpan.FromMinutes(MaxTimeSpanForRequestInMin)
            };
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(DefaultUserAgent);
            _defaultTimeSpanBetweenRequests = TimeSpan.FromMilliseconds(TimeSpanBetweenRequestsInMs);
        }

        public ProxyClientResponse GetResponse(string pageUrl)
        {
            WaitIfRequired();
            try
            {
                var responseMessage = GetResponseMessage(pageUrl);
                RequestCounter++;
                Console.WriteLine(responseMessage.StatusCode + " " + pageUrl); //TODO remove 
                if (!responseMessage.IsSuccessStatusCode)
                    return new ProxyClientResponse("", responseMessage.StatusCode);
                var responseBody = GetResponseBody(responseMessage);
                return new ProxyClientResponse(responseBody, HttpStatusCode.OK);
            }
            catch (AggregateException e)
            {
                _logger.Error("Aggregate {exception} ", e);
                return GetResponse(pageUrl);
            }
        }

        public ProxyClientResponse GetResponse(string pageUrl, string userAgent)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
            return GetResponse(pageUrl);
        }

        private HttpResponseMessage GetResponseMessage(string pageUrl)
        {
            var responseTask = Task.Run(async () => await GetResponseMessageAsync(pageUrl).ConfigureAwait(false));
            _timeOfLastUsage = DateTime.Now;
            responseTask.Wait();
            return responseTask.Result;
        }

        private static string GetResponseBody(HttpResponseMessage response)
        {
            var responseBodyTask = Task.Run(async () => await GetResponseBodyAsync(response).ConfigureAwait(false));
            responseBodyTask.Wait();
            return responseBodyTask.Result;
        }

        private async Task<HttpResponseMessage> GetResponseMessageAsync(string link)
        {
            return await _httpClient.GetAsync(link).ConfigureAwait(false);
        }

        private static async Task<string> GetResponseBodyAsync(HttpResponseMessage responseMessage)
        {
            responseMessage.Version = HttpVersion.Version10;
            return await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private void WaitIfRequired()
        {
            var delay = DateTime.Now.Subtract(_timeOfLastUsage);
            if (delay - _defaultTimeSpanBetweenRequests > TimeSpan.Zero)
                Thread.Sleep(_defaultTimeSpanBetweenRequests); //TODO пересчитать 
        }

        public void ResetRequestCounter()
        {
            RequestCounter = 0;
        }

        public void OverloadRequestCounter()
        {
            RequestCounter = int.MaxValue;
        }

        public DateTime GetLastUsageTime()
        {
            return _timeOfLastUsage;
        }
    }
}