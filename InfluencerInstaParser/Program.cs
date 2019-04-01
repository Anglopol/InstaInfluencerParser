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
            var parser = new ParsingHandler("tasyabraun");
            parser.Parse();
            var set = SingletonParsingSet.GetInstance();
            foreach (var VARIABLE in set.UnprocessedUsers)
            {
                Console.WriteLine(VARIABLE);
            }
            return true;
        }
    }
}