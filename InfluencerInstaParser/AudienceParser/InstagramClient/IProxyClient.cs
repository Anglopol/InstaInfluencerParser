using System;

namespace InfluencerInstaParser.AudienceParser.InstagramClient
{
    public interface IProxyClient
    {
        string GetPageContent(string pageUrl);
        int GetRequestCounter();
        void ResetRequestCounter();
        DateTime GetLastUsageTime();
    }
}