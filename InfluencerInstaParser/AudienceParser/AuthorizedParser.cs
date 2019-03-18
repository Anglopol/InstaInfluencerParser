using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.AudienceDownloader;
using InstagramApiSharp.API;

namespace InfluencerInstaParser.AudienceParser
{
    public class AuthorizedParser : IParser
    {
        public async Task<List<string>> GetParsedFollowers(string username, IInstaApi api)
        {
            var followers = await new FollowersDownloader().GetFollowers(username, api);
            var parsingArguments = (NameValueCollection) ConfigurationManager.GetSection("parsingarguments");
            var minNumberOfFollowers = int.Parse(parsingArguments.Get("MinFollowersValue"));
            var subscriptionProportion = float.Parse(parsingArguments.Get("SubscriptionProportion"));
            return await Parse(followers, api, minNumberOfFollowers, subscriptionProportion);
        }

        private async Task<List<string>> Parse(IEnumerable<string> rawFollowers, IInstaApi api, int minNumberOfFollowers,
            float subscriptionProportion)
        {
            var parsedFollowers = new List<string>();
            foreach (var user in rawFollowers)
            {
                var userInformation = await api.UserProcessor.GetUserInfoByUsernameAsync(user);
                if(userInformation.Value.FollowerCount > minNumberOfFollowers &&
                   (userInformation.Value.FollowingCount / (double)userInformation.Value.FollowerCount) < subscriptionProportion)
                {
                    parsedFollowers.Add(user);
                }
            }

            return parsedFollowers;
        }
        
    }
}