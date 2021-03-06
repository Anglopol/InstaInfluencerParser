using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.PostScraping;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.JsonToPostConverting
{
    public class JsonToPostConverter : IJsonToPostConverter
    {
        private readonly IResponseJsonScraper _jsonScraper;
        private readonly IPostJsonScraper _postJsonScraper;

        public JsonToPostConverter(IResponseJsonScraper jsonScraper, IPostJsonScraper postJsonScraper)
        {
            _jsonScraper = jsonScraper;
            _postJsonScraper = postJsonScraper;
        }

        public IEnumerable<Post>  GetPosts(JObject json)
        {
            var posts = new List<Post>();
            var edges = _jsonScraper.GetPostsEdges(json);
            foreach (var edge in edges)
            {
                posts.Add(GetPost(_jsonScraper.GetPostFromEdge(edge)));
            }

            return posts;
        }

        private Post GetPost(JToken postJson)
        {
            var post = CreatePost(postJson);
            if (_postJsonScraper.TryGetLocationId(postJson, out var locationId))
            {
                post.LocationId = locationId;
                post.HasLocation = true;
            }

            if (!_postJsonScraper.TryGetNextCommentsCursor(postJson, out var nextCursor)) return post;
            post.NextCommentsCursor = nextCursor;
            post.HasNextCursor = true;
            return post;
        }

        private Post CreatePost(JToken postJson)
        {
            var ownerName = _postJsonScraper.GetOwnerName(postJson);
            var ownerId = _postJsonScraper.GetOwnerId(postJson);
            var shortCode = _postJsonScraper.GetShortCode(postJson);
            var usersFromPreview = _postJsonScraper.GetUsersFromCommentsPreview(postJson);
            var isPostVideo = _postJsonScraper.IsPostVideo(postJson);
            var comments = _postJsonScraper.GetNumberOfComments(postJson);
            var likes = _postJsonScraper.GetNumberOfLikes(postJson);
            return new Post(ownerName, ownerId, shortCode, usersFromPreview, isPostVideo, likes, comments);
        }
    }
}