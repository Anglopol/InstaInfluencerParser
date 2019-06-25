using System;
using InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy;

namespace InfluencerInstaParser.AudienceParser.WebParsing.PageDownload
{
    public interface IPageDownloader : IDisposable
    {
        string GetPageContent(string pageUrl);
        void SetClientFree(IProxyClient proxyClient);
    }
}