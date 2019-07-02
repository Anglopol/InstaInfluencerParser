using InfluencerInstaParser.AudienceParser.ResultOfParsing;

namespace InfluencerInstaParser.AudienceParser
{
    public interface IInstagramParser
    {
        IParsingResult ParseByUsername(string username);
        IParsingResult ParseById(ulong userId);
    }
}