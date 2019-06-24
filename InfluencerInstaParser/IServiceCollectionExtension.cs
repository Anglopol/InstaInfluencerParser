using InfluencerInstaParser.AudienceParser.InstagramClient.ProxyClientCreating;
using InfluencerInstaParser.AudienceParser.Proxy;
using InfluencerInstaParser.AudienceParser.WebParsing.PageDownload;
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

            return serviceCollection;
        }
    }
}