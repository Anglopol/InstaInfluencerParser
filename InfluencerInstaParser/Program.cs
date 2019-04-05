using System;
using InfluencerInstaParser.AudienceParser;

namespace InfluencerInstaParser
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var parser = new ParsingHandler("tasyabraun");
            parser.Parse();
            var set = ParsingSetSingleton.GetInstance();
            foreach (var VARIABLE in set.UnprocessedUsers) Console.WriteLine(VARIABLE);
        }
    }
}