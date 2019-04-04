using System;
using System.Threading.Tasks;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;

namespace InfluencerInstaParser.AudienceParser.AuthorizedParsing
{
    public class AuthApiCreator
    {
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