using InfluencerInstaParser.AudienceParser.InstagramClient.ProxyClientCreating;
using InfluencerInstaParser.AudienceParser.Proxy;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.PostPageParsing.CommentsParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.PostPageParsing.LikesParsing;
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
            serviceCollection.AddTransient<IPageDownloader>(downloader =>
                new PageDownloader(serviceCollection.BuildServiceProvider()));
            serviceCollection.AddScoped<ILocator>(locator =>
                new Locator(serviceCollection.BuildServiceProvider(), "citiesLocations.txt"));
            serviceCollection.AddSingleton<IInstagramLocationPageScraper, LocationPageScraper>();
            serviceCollection.AddSingleton<IInstagramUserPageScraper, UserPageScraper>();
            serviceCollection.AddSingleton<IInstagramPostPageScraper, PostPageScraper>();
            serviceCollection.AddTransient<ICommentsParser, CommentsParser>();
            serviceCollection.AddTransient<ILikesParser, LikesParser>();


            return serviceCollection;
        }
    }
}