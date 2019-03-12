using System;
using System.IO;
using InstaSharper.API;
using InstaSharper.Classes;

namespace InfluencerInstaParser
{
    public class Authorizator
    {
        private const string StateFile = "state.bin";

        public async void Authorization(IInstaApi instaApi, IRequestDelay delay)
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
                Console.WriteLine($"Logging");
                delay.Disable();
                var logInResult = await instaApi.LoginAsync();
                delay.Enable();
                if (!logInResult.Succeeded)
                {
                    Console.WriteLine($"Unable to login");
                    return;
                }
            }

            var state = instaApi.GetStateDataAsStream();
            using (var fileStream = File.Create(StateFile))
            {
                state.Seek(0, SeekOrigin.Begin);
                state.CopyTo(fileStream);
            }
        }
    }
}