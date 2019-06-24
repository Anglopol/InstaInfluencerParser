using InfluencerInstaParser.AudienceParser.InstagramClient.ProxyClientCreating;
using Microsoft.Extensions.DependencyInjection;

namespace InfluencerInstaParser
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddInfluncerInstaParserLibrary(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IProxyClientCreator, ProxyClientCreator>();
            
            return serviceCollection;
        }
    }
}