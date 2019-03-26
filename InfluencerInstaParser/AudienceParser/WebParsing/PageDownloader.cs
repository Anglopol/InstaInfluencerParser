using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class PageDownloader
    {
        private const string InstagramUrl = @"https://www.instagram.com";

        public static async Task<string> GetPageContent(string url, string userAgent, string instGis = "")
        {
            var link = InstagramUrl + url;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
                client.DefaultRequestHeaders.Add("x-instagram-gis", instGis);
                try
                {
                    var response = await client.GetAsync(link);
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }

            return "";
        }
    }
}