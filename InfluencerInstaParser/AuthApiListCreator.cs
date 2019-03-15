using System.Collections.Generic;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;

namespace InfluencerInstaParser
{
    public class AuthApiListCreator
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

            return apis;
        }
    }
}