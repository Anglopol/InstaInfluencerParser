namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser
{
    public class ParsedUserFromJson
    {
        public string Name { get; }
        public ulong UserId { get; }
        public bool IsPrivate { get; }

        public ParsedUserFromJson(string name, ulong userId)
        {
            Name = name;
            UserId = userId;
            IsPrivate = false;
        }
        
        public ParsedUserFromJson(string name, ulong userId, bool isPrivate)
        {
            Name = name;
            UserId = userId;
            IsPrivate = isPrivate;
        }
    }
}