using InstagramApiSharp.Classes;

namespace InfluencerInstaParser
{
    public interface IUserSessionDataFactory
    {
        UserSessionData MakeSessionData();
        UserSessionData MakeSessionDataFromLogin(string login);
    }
}