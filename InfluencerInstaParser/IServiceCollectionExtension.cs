using InfluencerInstaParser.AudienceParser.InstagramClient.ProxyClientCreating;
using InfluencerInstaParser.AudienceParser.Proxy;
using InfluencerInstaParser.AudienceParser.Proxy.PageDownload;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.PostParsing.CommentsParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.PostParsing.LikesParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.JsonToParsedUserConverting;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.JsonToPostConverting;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.PostScraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping;
using Microsoft.Extensions.DependencyInjection;

namespace InfluencerInstaParser
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddInfluncerInstaParserLibrary(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IProxyClientCreator>(creator => new ProxyClientCreator(serviceCollection.BuildServiceProvider()));
            serviceCollection.AddSingleton<IProxyCreator>(client => new ProxyFromFileCreator("proxies.txt"));
            serviceCollection.AddTransient<IPageDownloader>(downloader =>
                new PageDownloader(serviceCollection.BuildServiceProvider()));
            serviceCollection.AddScoped<ILocator>(locator =>
                new Locator(serviceCollection.BuildServiceProvider(), "citiesLocations.txt"));
            serviceCollection.AddSingleton<IInstagramLocationPageScraper, LocationPageScraper>();
            serviceCollection.AddSingleton<IInstagramUserPageScraper, UserPageScraper>();
            serviceCollection.AddSingleton<IInstagramPostPageScraper, PostPageScraper>();
            serviceCollection.AddTransient<ICommentsParser>(parser => new CommentsParser(serviceCollection.BuildServiceProvider()));
            serviceCollection.AddTransient<ILikesParser>(parser => new LikesParser(serviceCollection.BuildServiceProvider()));
            serviceCollection.AddSingleton<IResponseJsonScraper, ResponseJsonScraper>();
            serviceCollection.AddSingleton<IJsonToPostConverter>(converter => new JsonToPostConverter(serviceCollection.BuildServiceProvider()));
            serviceCollection.AddSingleton<IPostJsonScraper, PostJsonScraper>();
            serviceCollection.AddSingleton<IJsonToParsedUsersConverter, JsonToParsedUsersConverter>();

            return serviceCollection;
        }
    }
}