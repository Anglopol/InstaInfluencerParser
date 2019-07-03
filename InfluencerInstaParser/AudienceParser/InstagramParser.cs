using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.ResultOfParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.PostParsing.CommentsParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.PostParsing.LikesParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.UserParsing;
using Microsoft.Extensions.DependencyInjection;

namespace InfluencerInstaParser.AudienceParser
{
    public class InstagramParser : IInstagramParser
    {
        private readonly IUserPageParser _userPageParser;
        private readonly IServiceProvider _serviceProvider;

        public InstagramParser(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
            var parsingResult = new ParsingResult();
            var postProcessingTasks = posts.Select(post =>
                Task.Factory.StartNew(() => SinglePostProcessing(post, parsingResult),
                    TaskCreationOptions.LongRunning));
            Task.WaitAll(postProcessingTasks.ToArray());

            return parsingResult;
        }

        private void SinglePostProcessing(Post post, ParsingResult parsingResult)
        {
            parsingResult.Posts.Add(post);
            
            Task.Factory.StartNew(() => CommentsPostProcessing(post, parsingResult),
                TaskCreationOptions.AttachedToParent);
            Task.Factory.StartNew(() => LikesPostProcessing(post, parsingResult),
                TaskCreationOptions.AttachedToParent);
            Task.Factory.StartNew(() => LocationPostProcessing(post, parsingResult),
                TaskCreationOptions.AttachedToParent);
        }

        private void CommentsPostProcessing(Post post, ParsingResult parsingResult)
        {
            var commentsParser = _serviceProvider.GetService<ICommentsParser>();
            parsingResult.AddUsersFromComments(commentsParser.GetUsersFromComments(post));
        }

        private void LikesPostProcessing(Post post, ParsingResult parsingResult)
        {
            var likesParser = _serviceProvider.GetService<ILikesParser>();
            parsingResult.AddUsersFromLikes(likesParser.GetUsersFromLikes(post));
        }
        private void LocationPostProcessing(Post post, ParsingResult parsingResult)
        {
            var locator = _serviceProvider.GetService<ILocator>();
            var locationScrapResult = locator.GetLocatorScrapingResultByLocationId(post.LocationId);
            parsingResult.ScrapedLocations.Add(locationScrapResult);
        }
    }
}