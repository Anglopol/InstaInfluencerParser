using InstagramApiSharp.Classes;

namespace InfluencerInstaParser.SessionData
{
    public interface IUserSessionDataFactory
    {
        UserSessionData MakeSessionData();
        UserSessionData MakeSessionDataFromLogin(string login);
    }
}