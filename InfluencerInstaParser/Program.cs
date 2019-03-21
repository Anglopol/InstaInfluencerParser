using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser;
using InfluencerInstaParser.AudienceParser.AudienceDownloader;
using InfluencerInstaParser.AudienceParser.WebParsing;
using InfluencerInstaParser.SessionData;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;

namespace InfluencerInstaParser
{
    class Program
    {
        private static IInstaApi _instaApi;

        static void Main(string[] args)
        {
            var result = Task.Run(MainAsync).GetAwaiter().GetResult();
            if (result)
                return;
        }

        public static async Task<bool> MainAsync()
        {
            Console.WriteLine(new WebParser().GetEndOfCursorOnFirstPage("varlamov"));
//            var userSession = new ConfigSessionDataFactory().MakeSessionData();
//            Console.WriteLine(userSession.UserName + userSession.Password);
//
//            var delay = RequestDelay.FromSeconds(2, 2);
//
//            var api = await AuthApiCreator.MakeAuthApi(userSession, delay);
//
//            var followers = await new AuthorizedParser().GetParsedFollowers("neprostohronika", api);
//
//            foreach (var user in followers)
//            {
//                Console.WriteLine(user);
//            }
//            var client = new HttpClient();
//            client.DefaultRequestHeaders.Add("user-agent","Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36");


            return true;
        }
    }
}