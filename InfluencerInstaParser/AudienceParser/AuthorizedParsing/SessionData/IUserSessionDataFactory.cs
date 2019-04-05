using InstagramApiSharp.Classes;

namespace InfluencerInstaParser.AudienceParser.AuthorizedParsing.SessionData
{
    public interface IUserSessionDataFactory
    {
        UserSessionData MakeSessionData();
        UserSessionData MakeSessionDataFromLogin(string login);
    }
}