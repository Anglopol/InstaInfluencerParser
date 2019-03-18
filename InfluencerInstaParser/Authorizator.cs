using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;

namespace InfluencerInstaParser
{
    public class Authorizator
    {
        public static async void MassAuthorize(IEnumerable<IInstaApi> instaApiList, IRequestDelay delay) 
        {
            foreach (var api in instaApiList)
            {
                Authorize(api, delay);
            }
        }
        public static async Task<bool> Authorize(IInstaApi instaApi, IRequestDelay delay)
        {
//            var user = instaApi.
//            var username = user.Value.UserName;
//            var stateFile = username + ".bin";

            if (instaApi.IsUserAuthenticated)
            {
                Console.WriteLine("User already authenticated");
                return true;
            }

//            try
//            {
//                StateFileLoad(instaApi, stateFile);
//            }
//            catch (Exception e) //TODO custom exception
//            {
//                Console.WriteLine(e);
//            }

            if (!instaApi.IsUserAuthenticated)
            {
//                Console.WriteLine($"Logging {username}");
                delay.Disable();
                var logInResult = await instaApi.LoginAsync();
                delay.Enable();
                if (!logInResult.Succeeded)
                {
//                    Console.WriteLine($"Unable to login for {username}");
                    return false;
                }

                return true;
            }

            return true;

//            StateFileCreate(instaApi, stateFile);
        }

        private static void StateFileLoad(IInstaApi instaApi, string stateFile)
        {
            if (!File.Exists(stateFile)) return;
            Console.WriteLine($"Loading state from file: {stateFile}");
            using (var fs = File.OpenRead(stateFile))
            {
                instaApi.LoadStateDataFromStream(fs);
            }
        }

        private static void StateFileCreate(IInstaApi instaApi, string stateFile)
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