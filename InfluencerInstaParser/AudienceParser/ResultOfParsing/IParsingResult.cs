using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.LocationParsing;

namespace InfluencerInstaParser.AudienceParser.ResultOfParsing
{
    public interface IParsingResult
    {
        IUser CreateUser();
        void AddUsersFromComments(IEnumerable<ParsedUserFromJson> users);
        void AddUsersFromLikes(IEnumerable<ParsedUserFromJson> users);
        void AddLocationScrapResult(LocatorScrapingResult scrapingResult);
        void AddLocationScrapResult(IEnumerable<LocatorScrapingResult> scrapingResult);
        void AddPost(Post post);
        void AddPosts(IEnumerable<Post> posts);
        
    }
}