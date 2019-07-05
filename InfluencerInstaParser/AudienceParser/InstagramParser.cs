using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.ResultOfParsing;
using InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser;
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
            return _userPageParser.IsUserPageValid(userPage)
                ? ParseById(userId)
                : _serviceProvider.GetService<IParsingResult>();
        }

        public IParsingResult ParseById(ulong userId)
        {
            var posts = GetPosts(userId);
            return PostsProcessing(posts);
        }

        public IParsingResult ParseOnlyPostsAndLocations(ParsedUserFromJson userFromJson)
        {
            var posts = GetPosts(userFromJson.UserId);
            return OnlyLocationsPostProcessing(posts);
        }

        public IParsingResult SecondLevelParsingForInfluencers(IUser influencer)
        {
            var posts = influencer.Posts.ToList();
            var secondLevelResult = SecondLevelParsingPostProcessing(posts);
            secondLevelResult.AddPosts(posts);
            secondLevelResult.AddLocationScrapResult(influencer.Locations);
            return secondLevelResult;
        }

        private IParsingResult PostsProcessing(IEnumerable<Post> posts)
        {
            var parsingResult = _serviceProvider.GetService<IParsingResult>();
            var postProcessingTasks = posts.Select(post =>
                Task.Factory.StartNew(() => SinglePostProcessing(post, parsingResult),
                    TaskCreationOptions.LongRunning));
            Task.WaitAll(postProcessingTasks.ToArray());

            return parsingResult;
        }

        private IParsingResult OnlyLocationsPostProcessing(IEnumerable<Post> posts)
        {
            var parsingResult = _serviceProvider.GetService<IParsingResult>();
            var postProcessingTasks = posts.Select(post =>
                Task.Factory.StartNew(() => OnlyLocationsSinglePostProcessing(post, parsingResult),
                    TaskCreationOptions.LongRunning));
            Task.WaitAll(postProcessingTasks.ToArray());

            return parsingResult;
        }

        private IParsingResult SecondLevelParsingPostProcessing(IEnumerable<Post> posts)
        {
            var parsingResult = _serviceProvider.GetService<IParsingResult>();
            var postProcessingTasks = posts.Select(post =>
                Task.Factory.StartNew(() => SecondLevelSinglePostProcessing(post, parsingResult),
                    TaskCreationOptions.LongRunning));
            Task.WaitAll(postProcessingTasks.ToArray());

            return parsingResult;
        }

        private void SinglePostProcessing(Post post, IParsingResult parsingResult)
        {
            parsingResult.AddPost(post);

            Task.Factory.StartNew(() => CommentsPostProcessing(post, parsingResult),
                TaskCreationOptions.AttachedToParent);
            Task.Factory.StartNew(() => LikesPostProcessing(post, parsingResult),
                TaskCreationOptions.AttachedToParent);
            Task.Factory.StartNew(() => LocationPostProcessing(post, parsingResult),
                TaskCreationOptions.AttachedToParent);
        }

        private void OnlyLocationsSinglePostProcessing(Post post, IParsingResult parsingResult)
        {
            parsingResult.AddPost(post);
            Task.Factory.StartNew(() => LocationPostProcessing(post, parsingResult),
                TaskCreationOptions.AttachedToParent);
        }

        private void SecondLevelSinglePostProcessing(Post post, IParsingResult parsingResult)
        {
            Task.Factory.StartNew(() => CommentsPostProcessing(post, parsingResult),
                TaskCreationOptions.AttachedToParent);
            Task.Factory.StartNew(() => LikesPostProcessing(post, parsingResult),
                TaskCreationOptions.AttachedToParent);
        }

        private void CommentsPostProcessing(Post post, IParsingResult parsingResult)
        {
            var commentsParser = _serviceProvider.GetService<ICommentsParser>();
            parsingResult.AddUsersFromComments(commentsParser.GetUsersFromComments(post));
        }

        private void LikesPostProcessing(Post post, IParsingResult parsingResult)
        {
            var likesParser = _serviceProvider.GetService<ILikesParser>();
            parsingResult.AddUsersFromLikes(likesParser.GetUsersFromLikes(post));
        }

        private void LocationPostProcessing(Post post, IParsingResult parsingResult)
        {
            var locator = _serviceProvider.GetService<ILocator>();
            var locationScrapResult = locator.GetLocatorScrapingResultByLocationId(post.LocationId);
            parsingResult.AddLocationScrapResult(locationScrapResult);
        }

        private IEnumerable<Post> GetPosts(ulong userId)
        {
            return _userPageParser.GetPostsFromUser(userId);
        }
    }
}