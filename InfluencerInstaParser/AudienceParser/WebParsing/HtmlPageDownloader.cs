using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class HtmlPageDownloader
    {
        public const string InstagramUrl = @"https://www.instagram.com/";
        public const string ProfilePageContainerUrl = @"/static/bundles/metro/ProfilePageContainer.js";

        public static HtmlDocument UserPageDownload(string username)
        {
            var url = InstagramUrl + username + "/";
            var web = new HtmlWeb();
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

        public static async Task<string> GetPageContainer(string url)
        {
            var link = @"https://www.instagram.com" + url;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(link);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
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