using InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy;

namespace InfluencerInstaParser.AudienceParser.WebParsing.PageDownload
{
    public interface IPageDownloader
    {
        string GetPageContent(string pageUrl);
        void SetClientFree(IProxyClient proxyClient);
    }
}