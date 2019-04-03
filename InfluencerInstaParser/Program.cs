using System;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser;

namespace InfluencerInstaParser
{
    internal class Program
    {
        private static void Main(string[] args)
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
            foreach (var VARIABLE in set.UnprocessedUsers) Console.WriteLine(VARIABLE);
            return true;
        }
    }
}