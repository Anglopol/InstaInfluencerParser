using System;

namespace InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy
{
    public interface IProxyClient
    {
        string GetPageContent(string pageUrl);
        int GetRequestCounter();
        void ResetRequestCounter();
        DateTime GetLastUsageTime();
    }
}