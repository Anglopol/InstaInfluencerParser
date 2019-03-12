using System;
using System.IO;
using System.Net;
using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Logger;

namespace InfluencerInstaParser
{
    public class SessionCreator
    {
        private UserSessionData _sessionData;
        private IRequestDelay _delay;


        public SessionCreator(string login, string password, IRequestDelay delay)
        {
            _sessionData = new UserSessionData
            {
                UserName = login,
                Password = password
            };
            _delay = delay;
        }

        public IInstaApi GetInstaApi()
        {
            var instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(_sessionData)
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .SetRequestDelay(_delay)
                .Build();

            return instaApi;
        }
    }
}