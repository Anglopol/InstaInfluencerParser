using System;
using System.IO;
using System.Threading.Tasks;
using InstagramApiSharp.API;

namespace InfluencerInstaParser.AudienceParser.AuthorizedParsing
{
    public class Authorizator
    {
        private const string StateFile = "state.bin";

        public static async Task Authorize(IInstaApi instaApi)
        {
            if (instaApi.IsUserAuthenticated)
            {
                Console.WriteLine("User already authenticated");
                return;
            }

            try
            {
                if (File.Exists(StateFile))
                {
                    Console.WriteLine("Loading state from file");
                    using (var fs = File.OpenRead(StateFile))
                    {
                        instaApi.LoadStateDataFromStream(fs);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (!instaApi.IsUserAuthenticated)
            {
                var logInResult = await instaApi.LoginAsync();
                if (!logInResult.Succeeded)
                {
                    Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                    return;
                }
            }

// save session in file
            var state = instaApi.GetStateDataAsStream();
// in .net core or uwp apps don't use GetStateDataAsStream.
// use this one:
// var state = _instaApi.GetStateDataAsString();
// this returns you session as json string.
            using (var fileStream = File.Create(StateFile))
            {
                state.Seek(0, SeekOrigin.Begin);
                state.CopyTo(fileStream);
            }
        }
    }
}