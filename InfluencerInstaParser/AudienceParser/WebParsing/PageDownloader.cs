using System.Net.Http;
using System.Threading.Tasks;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class PageDownloader
    {
        public static async Task<string> GetPageContent(string url)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            } 
        }
    }
}