using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        static void Main(string[] args)
        {
            var result = Task.Run(MainAsync).GetAwaiter().GetResult();
            if (result)
                return;
        }

        public static async Task<bool> MainAsync()
        {
            var set = SingletonParsingSet.GetInstance();
            var agents = new UserAgentCreator();
            var web = new WebParser(agents.GetUserAgent());
            web.GetPostsShortCodesFromUser("varlamov");
            foreach (var VARIABLE in set.ShortCodesQueue)
            {
                Console.WriteLine(VARIABLE);
            }
            return true;
        }
    }
}