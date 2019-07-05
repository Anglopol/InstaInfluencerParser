using InfluencerInstaParser.AudienceParser.ResultOfParsing;
using InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;

namespace InfluencerInstaParser.AudienceParser
{
    public interface IInstagramParser
    {
        IParsingResult ParseByUsername(string username);
        IParsingResult ParseById(ulong userId);
        IParsingResult ParseOnlyPostsAndLocations(ParsedUserFromJson userFromJson);
        IParsingResult SecondLevelParsingForInfluencers(IUser influencer);
    }
}