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
            serviceCollection.AddSingleton<IProxyClientCreator, ProxyClientCreator>();
            serviceCollection.AddSingleton<IProxyCreator>(client => new ProxyFromFileCreator("proxies.txt"));
            serviceCollection.AddTransient<IPageDownloader>(downloader =>
                new PageDownloader(serviceCollection.BuildServiceProvider()));
            serviceCollection.AddScoped<ILocator>(locator =>
                new Locator(serviceCollection.BuildServiceProvider(), "citiesLocations.txt"));
            serviceCollection.AddSingleton<IInstagramLocationPageScraper, LocationPageScraper>();
            serviceCollection.AddSingleton<IInstagramUserPageScraper, UserPageScraper>();
            serviceCollection.AddSingleton<IInstagramPostPageScraper, PostPageScraper>();
            serviceCollection.AddTransient<ICommentsParser, CommentsParser>();
            serviceCollection.AddTransient<ILikesParser, LikesParser>();
            serviceCollection.AddSingleton<IResponseJsonScraper, ResponseJsonScraper>();
            serviceCollection.AddSingleton<IJsonToPostConverter, JsonToPostConverter>();
            serviceCollection.AddSingleton<IPostJsonScraper, PostJsonScraper>();
            serviceCollection.AddSingleton<IJsonToParsedUsersConverter, JsonToParsedUsersConverter>();

            return serviceCollection;
        }
    }
}