using InfluencerInstaParser.AudienceParser.InstagramClient.ProxyClientCreating;
using InfluencerInstaParser.AudienceParser.Proxy;
using InfluencerInstaParser.AudienceParser.WebParsing.Locate;
using InfluencerInstaParser.AudienceParser.WebParsing.PageDownload;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping;
using Microsoft.Extensions.DependencyInjection;

namespace InfluencerInstaParser
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddInfluncerInstaParserLibrary(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IProxyClientCreator, ProxyClientCreator>();
            serviceCollection.AddSingleton<IProxyCreatorSingleton>(client => new ProxyFromFileCreator("proxies.txt"));
            serviceCollection.AddScoped<IPageDownloader>(downloader =>
                new PageDownloader(serviceCollection.BuildServiceProvider()));
            serviceCollection.AddScoped<ILocator>(locator =>
                new Locator(serviceCollection.BuildServiceProvider(), "citiesLocations.txt"));
            serviceCollection.AddSingleton<IInstagramPageContentScraper, PageContentScraper>();
            serviceCollection.AddSingleton<IInstagramLocationPageScraper, PageContentScraper>();


            return serviceCollection;
        }
    }
}