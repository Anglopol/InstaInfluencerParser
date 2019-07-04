using System;
using InfluencerInstaParser.AudienceParser;
using InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser;
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
            var targetUser = parsingResult.CreateUser();
            if (targetUser.IsUserEmpty) return;
            FirstLevelAudienceProcessing(targetUser);
        }

        private void FirstLevelAudienceProcessing(IUser user)
        {
            var usersToParse = GetDistinctParsedUsers(user);
            foreach (var userToParse in usersToParse)
            {
                var instaParser = _serviceProvider.GetService<IInstagramParser>();
                if (userToParse.IsPrivate) continue;
                var postAndLocationsParseResult = instaParser.ParseOnlyPostsAndLocations(userToParse);
            }
        }
    }
}