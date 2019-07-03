using System;
using InfluencerInstaParser.AudienceParser;
using Microsoft.Extensions.DependencyInjection;

namespace InfluencerInstaParser.Manager
{
    public class InstagramParserManager : IInstagramParserManager
    {
        private readonly IServiceProvider _serviceProvider;
        public InstagramParserManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void AnalyzeUser(string username)
        {
            var instaParser = _serviceProvider.GetService<IInstagramParser>();
            var parsingResult = instaParser.ParseByUsername(username);
        }
    }
}