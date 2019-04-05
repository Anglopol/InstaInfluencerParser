using System;
using System.Threading.Tasks;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;

namespace InfluencerInstaParser.AudienceParser.AuthorizedParsing
{
    public class AuthApiCreator
    {
        public static async Task<IInstaApi> MakeAuthApi(UserSessionData userSession)
        {
            var api = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .Build();
            await Authorizator.Authorize(api);
            Console.WriteLine(api.IsUserAuthenticated);
            return api;
        }
    }
}