using System.Collections.Concurrent;
using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.UserCreating;
using InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing;

namespace InfluencerInstaParser.AudienceParser.ResultOfParsing
{
    public class ParsingResult : IParsingResult
    {
        private const int NormOfAverageActivityForInfluencer = 1000;
        private const int CommentCoefficient = 2;
        
        public readonly ConcurrentBag<Post> Posts;
        public readonly ConcurrentBag<LocatorScrapingResult> ScrapedLocations;
        public ConcurrentBag<ParsedUserFromJson> UsersFromLikes { get; }
        public ConcurrentBag<ParsedUserFromJson> UsersFromComments { get; }
        private readonly IUserFactory _userFactory;

        public ParsingResult(IUserFactory userFactory)
        {
            _userFactory = userFactory;
            Posts = new ConcurrentBag<Post>();
            ScrapedLocations = new ConcurrentBag<LocatorScrapingResult>();
            UsersFromLikes = new ConcurrentBag<ParsedUserFromJson>();
            UsersFromComments = new ConcurrentBag<ParsedUserFromJson>();
        }

        public IUser CreateUser()
        {
            if (Posts.IsEmpty) _userFactory.CreateUser();
            var username = GetUsername();
            var userId = GetUserId();
            var isInfluencer = IsInfluencer();
            var likes = GetLikes();
            var comments = GetComments();
            var newUser = _userFactory.CreateUser(username, userId, isInfluencer, ScrapedLocations, likes, comments);
            newUser.UsersFromLikes = UsersFromLikes;
            newUser.UsersFromComments = UsersFromComments;
            return newUser;
        }

        public void AddUsersFromComments(IEnumerable<ParsedUserFromJson> users)
        {
            foreach (var parsedUser in users)
            {
                UsersFromComments.Add(parsedUser);
            }
        }

        public void AddUsersFromLikes(IEnumerable<ParsedUserFromJson> users)
        {
            foreach (var parsedUser in users)
            {
                UsersFromLikes.Add(parsedUser);
            }
        }

        public void AddLocationScrapResult(LocatorScrapingResult scrapingResult)
        {
            ScrapedLocations.Add(scrapingResult);
        }

        public void AddLocationScrapResult(IEnumerable<LocatorScrapingResult> scrapingResult)
        {
            foreach (var locatorScrapingResult in scrapingResult)
            {
                AddLocationScrapResult(locatorScrapingResult);
            }
        }

        public void AddPost(Post post)
        {
            Posts.Add(post);
        }

        public void AddPosts(IEnumerable<Post> posts)
        {
            foreach (var post in posts)
            {
                AddPost(post);
            }
        }

        private bool IsInfluencer()
        {
            var numberOfPosts = Posts.Count;
            var countOfActivity = GetCountOfActivity();
            return countOfActivity / numberOfPosts >= NormOfAverageActivityForInfluencer;
        }

        private int GetCountOfActivity()
        {
            return GetLikes() + GetComments() * CommentCoefficient;
        }

        private int GetLikes()
        {
            var count = 0;
            foreach (var post in Posts)
            {
                count += post.Likes;
            }

            return count;
        }

        private int GetComments()
        {
            var count = 0;
            foreach (var post in Posts)
            {
                count += post.Comments;
            }

            return count;
        }

        private string GetUsername()
        {
            Posts.TryPeek(out var randomPost);
            return randomPost.Owner;
        }

        private ulong GetUserId()
        {
            Posts.TryPeek(out var randomPost);
            return randomPost.OwnerId;
        }

    }
}