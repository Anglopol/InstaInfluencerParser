using System;

namespace InfluencerInstaParser.AudienceParser.WebParsing.PageDownload
{
    public interface IPageDownloader : IDisposable
    {
        string GetPageContent(string pageUrl);
        void SetClientFree();
    }
}