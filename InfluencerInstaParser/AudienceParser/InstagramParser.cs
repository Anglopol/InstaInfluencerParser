using System;
using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.ResultOfParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.UserParsing;
using Microsoft.Extensions.DependencyInjection;

namespace InfluencerInstaParser.AudienceParser
{
    public class InstagramParser : IInstagramParser
    {
        private readonly IUserPageParser _userPageParser;

        public InstagramParser(IServiceProvider serviceProvider)
        {
            _userPageParser = serviceProvider.GetService<IUserPageParser>();
        }
        public IParsingResult ParseByUsername(string username)
        {
            var userPage = _userPageParser.GetUserPage(username);
            var userId = _userPageParser.GetUserId(userPage);
            return _userPageParser.IsUserPageValid(userPage) ? ParseById(userId) : new ParsingResult();
        }

        public IParsingResult ParseById(ulong userId)
        {
            var posts = _userPageParser.GetPostsFromUser(userId);
            return PostsProcessing(posts);
        }

        private IParsingResult PostsProcessing(IEnumerable<Post> posts)
        {
            foreach (var post in posts)
            {
                
            }
        }
    }
}