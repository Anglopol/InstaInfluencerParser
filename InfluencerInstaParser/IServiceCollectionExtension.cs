using InfluencerInstaParser.AudienceParser;
using InfluencerInstaParser.AudienceParser.InstagramClient.ProxyClientCreating;
using InfluencerInstaParser.AudienceParser.Proxy;
using InfluencerInstaParser.AudienceParser.Proxy.PageDownload;
using InfluencerInstaParser.AudienceParser.ResultOfParsing;
using InfluencerInstaParser.AudienceParser.UserCreating;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.PostParsing.CommentsParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.PostParsing.LikesParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.UserParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.JsonToParsedUserConverting;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.JsonToPostConverting;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.PostScraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping;
using InfluencerInstaParser.Database.Client;
using InfluencerInstaParser.Database.Client.Connection;
using InfluencerInstaParser.Database.ModelCreating;
using InfluencerInstaParser.Manager;
using Microsoft.Extensions.DependencyInjection;

namespace InfluencerInstaParser
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddInfluncerInstaParserLibrary(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IProxyClientCreator, ProxyClientCreator>();
            serviceCollection.AddSingleton<IProxyCreator, ProxyFromFileCreator>();
            serviceCollection.AddTransient<IPageDownloader, PageDownloader>();
            serviceCollection.AddScoped<ILocator, Locator>();
            serviceCollection.AddSingleton<IInstagramLocationPageScraper, LocationPageScraper>();
            serviceCollection.AddSingleton<IInstagramUserPageScraper, UserPageScraper>();
            serviceCollection.AddSingleton<IInstagramPostPageScraper, PostPageScraper>();
            serviceCollection.AddTransient<ICommentsParser, CommentsParser>();
            serviceCollection.AddTransient<ILikesParser, LikesParser>();
            serviceCollection.AddSingleton<IResponseJsonScraper, ResponseJsonScraper>();
            serviceCollection.AddSingleton<IJsonToPostConverter, JsonToPostConverter>();
            serviceCollection.AddSingleton<IPostJsonScraper, PostJsonScraper>();
            serviceCollection.AddSingleton<IJsonToParsedUsersConverter, JsonToParsedUsersConverter>();
            serviceCollection.AddTransient<IParsingResult, ParsingResult>();
            serviceCollection.AddSingleton<IConnectionParams, ConnectionParams>();
            serviceCollection.AddScoped<IDatabaseClientHandler, Neo4JDatabaseInstagramClient>();
            serviceCollection.AddTransient<IModelCreator, ModelCreator>();
            serviceCollection.AddTransient<IInstagramParserManager, InstagramParserManager>();
            serviceCollection.AddScoped<IInstagramParser, InstagramParser>();
            serviceCollection.AddSingleton<IParsingResultFactory, ParsingResultFactory>();
            serviceCollection.AddTransient<IUserPageParser, UserPageParser>();
            serviceCollection.AddSingleton<IUserFactory, UserFactory>();

            return serviceCollection;
        }
    }
}