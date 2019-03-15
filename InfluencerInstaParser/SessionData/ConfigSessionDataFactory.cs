using System;
using System.Collections.Specialized;
using System.Configuration;
using InstagramApiSharp.Classes;

namespace InfluencerInstaParser.SessionData
{
    public class ConfigSessionDataFactory : IUserSessionDataFactory
    {
        private static int _currentAccount = 0;
        private static int _numberOfSessions = 0;

        public UserSessionData MakeSessionDataFromLogin(string login)
        {
            var accountsSection = (NameValueCollection) ConfigurationManager.GetSection("accounts");
            var password = accountsSection.Get(login);
            if (password == null)
            {
                throw new Exception("There is no such login in configs"); //TODO add custom exception
            }

            return new UserSessionData()
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

            if (_numberOfSessions >= 19)
            {
                _currentAccount++;
                _numberOfSessions = 0;
            }

            return new UserSessionData()
            {
                UserName = login,
                Password = password
            };
        }
    }
}