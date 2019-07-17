using System;

namespace InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy
{
    public interface IProxyClient
    {
        ProxyClientResponse GetResponse(string pageUrl);
        ProxyClientResponse GetResponse(string pageUrl, string userAgent);
        int RequestCounter { get; }
        void ResetRequestCounter();
        void OverloadRequestCounter();
        DateTime GetLastUsageTime();
    }
}