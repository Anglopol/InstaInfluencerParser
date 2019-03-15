using System;
using System.Collections.Generic;
using System.IO;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;

namespace InfluencerInstaParser
{
    public class Authorizator
    {
        public async void MassAuthorize(IEnumerable<IInstaApi> instaApiList, IRequestDelay delay) 
        {
            foreach (var api in instaApiList)
            {
                Authorize(api, delay);
            }
        }
        public async void Authorize(IInstaApi instaApi, IRequestDelay delay)
        {
            var username = instaApi.UserProcessor.GetCurrentUserAsync().Result.Value.UserName;
            var stateFile = username + ".bin";

            if (instaApi.IsUserAuthenticated)
            {
                Console.WriteLine("User already authenticated");
                return;
            }

            try
            {
                StateFileLoad(instaApi, stateFile);
            }
            catch (Exception e) //TODO custom exception
            {
                Console.WriteLine(e);
            }

            if (!instaApi.IsUserAuthenticated)
            {
                Console.WriteLine($"Logging {username}");
                delay.Disable();
                var logInResult = await instaApi.LoginAsync();
                delay.Enable();
                if (!logInResult.Succeeded)
                {
                    Console.WriteLine($"Unable to login for {username}");
                    return;
                }
            }

            StateFileCreate(instaApi, stateFile);
        }

        private void StateFileLoad(IInstaApi instaApi, string stateFile)
        {
            if (!File.Exists(stateFile)) return;
            Console.WriteLine($"Loading state from file: {stateFile}");
            using (var fs = File.OpenRead(stateFile))
            {
                instaApi.LoadStateDataFromStream(fs);
            }
        }

        private void StateFileCreate(IInstaApi instaApi, string stateFile)
        {
            var state = instaApi.GetStateDataAsStream();
            using (var fileStream = File.Create(stateFile))
            {
                state.Seek(0, SeekOrigin.Begin);
                state.CopyTo(fileStream);
            }
        }
    }
}