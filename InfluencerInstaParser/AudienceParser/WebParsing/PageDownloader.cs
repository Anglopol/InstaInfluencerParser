using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class PageDownloader
    {
        public const string InstagramUrl = @"https://www.instagram.com/";
        public const string ProfilePageContainerUrl = @"/static/bundles/metro/ProfilePageContainer.js";

        public const string DefaultUserAgent =
            @"Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36";

        public static HtmlDocument UserPageDownload(string username, string userAgent)
        {
            var url = InstagramUrl + username + "/";
            var web = new HtmlWeb {UserAgent = userAgent};

            return web.Load(url);
        }

        public static string ProfilePageJsUrlParser(HtmlDocument htmlDocument)
        {
            var links = htmlDocument.DocumentNode.SelectNodes("//head/link");
            foreach (var link in links)
            {
                if (link.Attributes["href"].Value.StartsWith(ProfilePageContainerUrl))
                {
                    return link.Attributes["href"].Value;
                }
            }

            return "";
        }

        public static async Task<string> GetPageContent(string url, string userAgent, string instGis = "")
        {
            var link = @"https://www.instagram.com" + url;
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