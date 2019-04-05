using System.Collections.Specialized;
using System.Configuration;
using InfluencerInstaParser.Exceptions;
using InstagramApiSharp.Classes;

namespace InfluencerInstaParser.AudienceParser.AuthorizedParsing.SessionData
{
    public class ConfigSessionDataFactory : IUserSessionDataFactory
    {
        private const int MaxValueOfSessions = 19;

        private static int _currentAccount;
        private static int _numberOfSessions;


        public UserSessionData MakeSessionDataFromLogin(string login)
        {
            var accountsSection = (NameValueCollection) ConfigurationManager.GetSection("accounts");
            var password = accountsSection.Get(login);
            if (password == null) throw new NoSuchLoginException($"There is no such login({login}) in configs");

            return new UserSessionData
            {
                UserName = login,
                Password = password
            };
        }

        public UserSessionData MakeSessionData()
        {
            var accountsSection = (NameValueCollection) ConfigurationManager.GetSection("accounts"); //TODO using set
            var login = accountsSection.GetKey(_currentAccount);
            var password = accountsSection.Get(login);
            _numberOfSessions++;

            if (_numberOfSessions >= MaxValueOfSessions)
            {
                _currentAccount++;
                _numberOfSessions = 0;
            }

            return new UserSessionData
            {
                UserName = login,
                Password = password
            };
        }
    }
}