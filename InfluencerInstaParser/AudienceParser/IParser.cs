using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser
{
    public interface IParser
    {
        List<string> GetFollowers(string username);
    }
}