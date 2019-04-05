using System;
using InfluencerInstaParser.AudienceParser.AuthorizedParsing.SessionData;
using InstagramApiSharp.Classes;

namespace InfluencerInstaParser.SessionData
{
    public class DBSessionDataFactory : IUserSessionDataFactory
    {
        public UserSessionData MakeSessionData()
        {
            throw new NotImplementedException();
        }

        public UserSessionData MakeSessionDataFromLogin(string login)
        {
            throw new NotImplementedException();
        }
    }
}