namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser
{
    public class ParsedUser
    {
        public string Name { get; }
        public ulong UserId { get; }

        public ParsedUser(string name, ulong userId)
        {
            Name = name;
            UserId = userId;
        }
    }
}