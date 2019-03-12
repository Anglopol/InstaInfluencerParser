using System;
using System.IO;
using System.Threading.Tasks;
using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Logger;

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
            var userSession = new UserSessionData
            {
                UserName = "petrova_eyebrows",
                Password = "****"
            };
            
            var delay = RequestDelay.FromSeconds(2, 2);
            
            _instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .SetRequestDelay(delay)
                .Build();
            
            const string stateFile = "state.bin";
            try
            {
                if (File.Exists(stateFile))
                {
                    Console.WriteLine("Loading state from file");
                    using (var fs = File.OpenRead(stateFile))
                    {
                        _instaApi.LoadStateDataFromStream(fs);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            if (!_instaApi.IsUserAuthenticated)
            {
                
                Console.WriteLine($"Logging in as {userSession.UserName}");
                delay.Disable();
                var logInResult = await _instaApi.LoginAsync();
                delay.Enable();
                if (!logInResult.Succeeded)
                {
                    Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                    return false;
                }
            }
            
            var state = _instaApi.GetStateDataAsStream();
            using (var fileStream = File.Create(stateFile))
            {
                state.Seek(0, SeekOrigin.Begin);
                state.CopyTo(fileStream);
            }

//            var followers = await _instaApi.GetUserFollowersAsync(userSession.UserName,
//                PaginationParameters.MaxPagesToLoad(200)); //7435427285

//            await _instaApi.FollowUserAsync(7435427285);

            return true;
        }
    }
}