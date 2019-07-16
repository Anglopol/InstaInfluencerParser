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
using Serilog;

namespace InfluencerInstaParser.AudienceParser
{
    public class InstagramParser : IInstagramParser
    {
        private readonly IUserPageParser _userPageParser;
        private readonly IParsingResultFactory _parsingResultFactory;
        private readonly ICommentsParser _commentsParser;
        private readonly ILikesParser _likesParser;
        private readonly ILocator _locator;
        private readonly ILogger _logger;

        public InstagramParser(IUserPageParser userPageParser, IParsingResultFactory parsingResultFactory,
            ICommentsParser commentsParser, ILikesParser likesParser, ILocator locator, ILogger logger)
        {
            _userPageParser = userPageParser;
            _parsingResultFactory = parsingResultFactory;
            _commentsParser = commentsParser;
            _likesParser = likesParser;
            _locator = locator;
            _logger = logger;
        }

        public IParsingResult ParseByUsername(string username)
        {
            var userPage = _userPageParser.GetUserPage(username);
            var userId = _userPageParser.GetUserId(userPage);
            _logger.Verbose("UserId is {id}", userId);
            return _userPageParser.IsUserPageValid(userPage)
                ? ParseById(userId)
                : _parsingResultFactory.MakeParsingResult();
        }

        public IParsingResult ParseById(ulong userId)
        {
            var posts = GetPosts(userId);
            _logger.Debug("All posts downloaded");
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
            _logger.Debug("Start Posts Processing");
            var parsingResult = _parsingResultFactory.MakeParsingResult();
            var postProcessingTasks = posts.Select(post => Task.Run(() => SinglePostProcessing(post, parsingResult)));
            Task.WaitAll(postProcessingTasks.ToArray());
            _logger.Debug("End posts processing");
            return parsingResult;
        }

        private IParsingResult OnlyLocationsPostProcessing(IEnumerable<Post> posts)
        {
            var parsingResult = _parsingResultFactory.MakeParsingResult();
            var postProcessingTasks = posts.Select(post =>
                Task.Factory.StartNew(() => OnlyLocationsSinglePostProcessing(post, parsingResult),
                    TaskCreationOptions.LongRunning));
            Task.WaitAll(postProcessingTasks.ToArray());

            return parsingResult;
        }

        private IParsingResult SecondLevelParsingPostProcessing(IEnumerable<Post> posts)
        {
            var parsingResult = _parsingResultFactory.MakeParsingResult();
            var postProcessingTasks = posts.Select(post =>
                Task.Factory.StartNew(() => SecondLevelSinglePostProcessing(post, parsingResult),
                    TaskCreationOptions.LongRunning));
            Task.WaitAll(postProcessingTasks.ToArray());

            return parsingResult;
        }

        private void SinglePostProcessing(Post post, IParsingResult parsingResult)
        {
            if (post == null) return;
            _logger.Verbose("Processing {@Post}", post);
            parsingResult.AddPost(post);
            var tasks = new List<Task>();
            var commentTask = Task.Run(() => CommentsPostProcessing(post, parsingResult));
            tasks.Add(commentTask);
            var likesTask = Task.Run(() => LikesPostProcessing(post, parsingResult));
            tasks.Add(likesTask);
            var locationTask = Task.Run(() => LocationPostProcessing(post, parsingResult));
            tasks.Add(locationTask);
            Task.WaitAll(tasks.ToArray());
            _logger.Debug("POST PROCESSED");
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
            parsingResult.AddUsersFromComments(_commentsParser.GetUsersFromComments(post));
        }

        private void LikesPostProcessing(Post post, IParsingResult parsingResult)
        {
            parsingResult.AddUsersFromLikes(_likesParser.GetUsersFromLikes(post));
        }

        private void LocationPostProcessing(Post post, IParsingResult parsingResult)
        {
            var locationScrapResult = _locator.GetLocatorScrapingResultByLocationId(post.LocationId);
            parsingResult.AddLocationScrapResult(locationScrapResult);
        }

        private IEnumerable<Post> GetPosts(ulong userId)
        {
            return _userPageParser.GetPostsFromUser(userId);
        }
    }
}