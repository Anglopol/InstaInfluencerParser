using System;
using System.Collections.Specialized;
using System.Configuration;
using InstagramApiSharp.Classes;

namespace InfluencerInstaParser
{
    public class UserSessionDataCreator
    {
        
        private static int _currentAccount = 0;
        public static UserSessionData MakeDataFromConfigs()
        {
            var accountsSection = (NameValueCollection)ConfigurationManager.GetSection("accounts"); //TODO using set
            var login = accountsSection.GetKey(_currentAccount);
            var password = accountsSection.Get(login);
            _currentAccount++;

            return new UserSessionData()
            {
                UserName = login,
                Password = password
            };
        }
        
        public static UserSessionData MakeDataFromConfigsLogin(string login)
        {
            var accountsSection = (NameValueCollection)ConfigurationManager.GetSection("accounts");
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
    }
}