using System;

namespace InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy
{
    public interface IProxyClient
    {
        string GetPageContent(string pageUrl);
        string GetPageContent(string pageUrl, string userAgent);
        int GetRequestCounter();
        void ResetRequestCounter();
        DateTime GetLastUsageTime();
    }
}