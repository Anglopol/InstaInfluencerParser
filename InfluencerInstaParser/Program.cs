using System;
using System.IO;
using System.Threading.Tasks;
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
            var userSession = UserSessionDataFactory.MakeDataFromConfigs();

            var delay = RequestDelay.FromSeconds(2, 2);

            _instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .SetRequestDelay(delay)
                .Build();


            return true;
        }
    }
}