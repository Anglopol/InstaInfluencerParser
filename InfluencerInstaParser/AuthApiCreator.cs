using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;

namespace InfluencerInstaParser
{
    public class AuthApiCreator
    {
        public static List<IInstaApi> MakeList(int countOfApis, UserSessionData userSession,
            IRequestDelay delay)
        {
            var apis = new List<IInstaApi>(countOfApis);
            for (var i = 0; i < countOfApis; i++)
            {
                apis.Add(InstaApiBuilder.CreateBuilder()
                    .SetUser(userSession)
                    .UseLogger(new DebugLogger(LogLevel.Exceptions)) //TODO add logger 
                    .SetRequestDelay(delay)
                    .Build());
            }

            Authorizator.MassAuthorize(apis, delay);

            return apis;
        }

        public static async Task<IInstaApi> MakeAuthApi(UserSessionData userSession, IRequestDelay delay) 
        {
            var api = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.Exceptions)) //TODO add logger 
                .SetRequestDelay(delay)
                .Build();
            await Authorizator.Authorize(api, delay);
            Console.WriteLine(api.IsUserAuthenticated);
            return api;
        }
    }
}