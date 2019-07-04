using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing;

namespace InfluencerInstaParser.AudienceParser.UserCreating
{
    public class UserFactory : IUserFactory
    {
        public IUser CreateUser(string username, ulong userId, bool isInfluencer,
            IEnumerable<LocatorScrapingResult> scrapingResults)
        {
            return new User(username, userId, isInfluencer, scrapingResults);
        }

        public IUser CreateUser()
        {
            return new User();
        }
    }
}