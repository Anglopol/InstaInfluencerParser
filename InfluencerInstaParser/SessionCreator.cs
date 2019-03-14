using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;

namespace InfluencerInstaParser
{
    public class SessionCreator
    {
        private UserSessionData _sessionData;
        private IRequestDelay _delay;


        public SessionCreator(UserSessionData sessionData, IRequestDelay delay)
        {
            _sessionData = sessionData;
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