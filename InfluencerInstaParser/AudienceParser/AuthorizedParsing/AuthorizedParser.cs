using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Threading.Tasks;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes.Models;

namespace InfluencerInstaParser.AudienceParser.AuthorizedParsing
{
    public class AuthorizedParser
    {
        private readonly int _minNumberOfFollowers;
        private readonly float _subscriptionProportion;

        public AuthorizedParser()
        {
            var parsingArguments = (NameValueCollection) ConfigurationManager.GetSection("parsingarguments");
            _minNumberOfFollowers = int.Parse(parsingArguments.Get("MinFollowersValue"));
            _subscriptionProportion = float.Parse(parsingArguments.Get("SubscriptionProportion"));
        }

        public async Task<List<InstaUserInfo>> GetParsedFollowers(string username, IInstaApi api)
        {
            var followers = new AudienceDownloader().GetFollowers(username, api);

            return await Parse(followers, api);
        }

        private async Task<List<InstaUserInfo>> Parse(IEnumerable<string> rawFollowers, IInstaApi api)
        {
            var parsedFollowers = new List<InstaUserInfo>();
            foreach (var user in rawFollowers)
            {
                var userInformation = await api.UserProcessor.GetUserInfoByUsernameAsync(user);
                if (CheckUser(userInformation.Value)) parsedFollowers.Add(userInformation.Value);
            }

            return parsedFollowers;
        }

        private bool CheckUser(InstaUserInfo user)
        {
            return user.FollowerCount > _minNumberOfFollowers &&
                   user.FollowingCount / (double) user.FollowerCount < _subscriptionProportion;
        }
    }
}