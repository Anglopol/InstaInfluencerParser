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

        public static HtmlDocument UserPageDownload(string username)
        {
            var url = InstagramUrl + username + "/";
            var web = new HtmlWeb {UserAgent = DefaultUserAgent};

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

        public static string GetJsonStringFromUserPage(string username)
        {
            var htmlDocument = UserPageDownload(username);
            var script = htmlDocument.DocumentNode.SelectSingleNode("//body/script");
            var jsonStr = script.InnerText.Remove(0, "window._sharedData = ".Length)
                .Remove(script.InnerText.Remove(0, "window._sharedData = ".Length).Length - 1);
            return jsonStr;
        }

        public static async Task<string> GetPageContent(string url, string instGis = "")
        {
            var link = @"https://www.instagram.com" + url;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.TryParseAdd(DefaultUserAgent);
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