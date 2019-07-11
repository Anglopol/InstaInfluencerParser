using InfluencerInstaParser.AudienceParser.UserCreating;

namespace InfluencerInstaParser.AudienceParser.ResultOfParsing
{
    public class ParsingResultFactory : IParsingResultFactory
    {
        private readonly IUserFactory _factory;
        public ParsingResultFactory(IUserFactory factory)
        {
            _factory = factory;
        }
        public IParsingResult MakeParsingResult()
        {
            return new ParsingResult(_factory);
        }
    }
}