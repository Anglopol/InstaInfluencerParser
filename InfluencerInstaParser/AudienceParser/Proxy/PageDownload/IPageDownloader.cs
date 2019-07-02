using System;

namespace InfluencerInstaParser.AudienceParser.Proxy.PageDownload
{
    public interface IPageDownloader : IDisposable
    {
        string GetPageContent(string pageUrl);
        void SetClientFree();
    }
}