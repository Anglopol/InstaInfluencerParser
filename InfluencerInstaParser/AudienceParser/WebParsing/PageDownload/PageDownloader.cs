using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace InfluencerInstaParser.AudienceParser.WebParsing.PageDownload
{
    public class PageDownloader
    {
        private readonly Logger _logger;

        public PageDownloader()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public string GetPageContent(string url)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var responseTask = Task.Run(async () => await client.GetAsync(url));
                    responseTask.Wait();
                    var response = responseTask.Result;
                    response.EnsureSuccessStatusCode();
                    var contentTask = Task.Run(async () => await response.Content.ReadAsStringAsync());
                    contentTask.Wait();
                    return contentTask.Result;
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);
                    Thread.Sleep(3000);
                    return GetPageContent(url);
                }
            }
        }
    }
}