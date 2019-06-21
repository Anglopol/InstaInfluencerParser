using InfluencerInstaParser.AudienceParser.InstagramClient;
using InfluencerInstaParser.AudienceParser.InstagramClient.ClientWithProxy;
using InfluencerInstaParser.AudienceParser.InstagramClient.InstagramClientCreating;
using Microsoft.Extensions.DependencyInjection;

namespace InfluencerInstaParser
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddInfluncerInstaParserLibrary(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IProxyClient>(client => new ProxyClient());
            serviceCollection.AddSingleton<IProxyClientCreator, ProxyClientCreator>();
            
            return serviceCollection;
        }
    }
}