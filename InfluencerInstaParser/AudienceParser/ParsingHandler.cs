namespace InfluencerInstaParser.AudienceParser
{
    public class ParsingHandler
    {
        private SingletonParsingSet _parsingSet;

        public ParsingHandler()
        {
            _parsingSet = SingletonParsingSet.GetInstance();
        }
        
    }
}