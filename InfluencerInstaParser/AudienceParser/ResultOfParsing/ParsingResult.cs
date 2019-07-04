using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.UserCreating;
using InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing;
using Microsoft.Extensions.DependencyInjection;

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
        private IServiceProvider _serviceProvider;

        public ParsingResult(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Posts = new ConcurrentBag<Post>();
            ScrapedLocations = new ConcurrentBag<LocatorScrapingResult>();
            UsersFromLikes = new ConcurrentBag<ParsedUserFromJson>();
            UsersFromComments = new ConcurrentBag<ParsedUserFromJson>();
        }

        public IUser CreateUser()
        {
            var userFactory = _serviceProvider.GetService<IUserFactory>();
            if (Posts.IsEmpty) userFactory.CreateUser();
            var username = GetUsername();
            var userId = GetUserId();
            var isInfluencer = IsInfluencer();
            var newUser = userFactory.CreateUser(username, userId, isInfluencer, ScrapedLocations);
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

        public void AddPost(Post post)
        {
            Posts.Add(post);
        }

        private bool IsInfluencer()
        {
            var numberOfPosts = Posts.Count;
            var countOfActivity = GetCountOfActivity();
            return countOfActivity / numberOfPosts >= NormOfAverageActivityForInfluencer;
        }

        private int GetCountOfActivity()
        {
            var count = 0;
            foreach (var post in Posts)
            {
                count += post.Comments * CommentCoefficient + post.Likes;
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