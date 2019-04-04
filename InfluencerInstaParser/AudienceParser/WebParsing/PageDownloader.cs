using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class PageDownloader
    {
        private readonly Logger _logger;

        public PageDownloader()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task<string> GetPageContent(string url)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);
                    Thread.Sleep(3000);
                    return await GetPageContent(url);
                }
            }
        }
    }
}