using InfluencerInstaParser.AudienceParser.WebParsing.PageDownload;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class Locator
    {
        private readonly PageDownloaderProxy _proxy;
        private readonly PageContentScrapper _scrapper;
        private readonly string _userAgent;

        public Locator(PageDownloaderProxy downloaderProxy, PageContentScrapper scrapper, string userAgent)
        {
            _scrapper = scrapper;
            _proxy = downloaderProxy;
            _userAgent = userAgent;
        }

        public bool TryGetPostLocation(string pageContent, out string city)
        {
            if (_scrapper.IsLocationHasAddress(pageContent))
            {
                city = _scrapper.GetPostAddressLocation(pageContent);
                return true;
            }

            var locationUrl =
                $"/explore/locations/{_scrapper.GetLocationId(pageContent)}/{_scrapper.GetLocationSlug(pageContent)}/";
            var locationPage = _proxy.GetPageContent(locationUrl, _userAgent);
            if (locationPage.Contains("\"city\""))
            {
                city = _scrapper.GetCity(pageContent);
                return true;
            }

            city = "";
            return false;
        }
    }
}