using System.Collections.Concurrent;
using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing;

namespace InfluencerInstaParser.AudienceParser.ResultOfParsing
{
    public class ParsingResult : IParsingResult
    {
        public readonly ConcurrentBag<Post> Posts;
        public ConcurrentBag<LocatorScrapingResult> ScrapedLocations;
        public ConcurrentBag<ParsedUser> UsersFromLikes { get; }
        public ConcurrentBag<ParsedUser> UsersFromComments { get; }

        public ParsingResult()
        {
            Posts = new ConcurrentBag<Post>();
            ScrapedLocations = new ConcurrentBag<LocatorScrapingResult>();
            UsersFromLikes = new ConcurrentBag<ParsedUser>();
            UsersFromComments = new ConcurrentBag<ParsedUser>();
        }

        public User CreateUser()
        {
            throw new System.NotImplementedException();
        }

        public void AddUsersFromComments(IEnumerable<ParsedUser> users)
        {
            foreach (var parsedUser in users)
            {
                UsersFromComments.Add(parsedUser);
            }
        }

        public void AddUsersFromLikes(IEnumerable<ParsedUser> users)
        {
            foreach (var parsedUser in users)
            {
                UsersFromLikes.Add(parsedUser);
            }
        }

    }
}