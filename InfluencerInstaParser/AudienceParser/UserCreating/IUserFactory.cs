using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing;

namespace InfluencerInstaParser.AudienceParser.UserCreating
{
    public interface IUserFactory
    {
        IUser CreateUser(string username, ulong userId, bool isInfluencer, IEnumerable<LocatorScrapingResult> scrapingResults);
        IUser CreateUser();
    }
}