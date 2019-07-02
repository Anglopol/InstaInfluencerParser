using System;
using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.PostScraping;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.JsonToPostConverting
{
    public class JsonToPostConverter : IJsonToPostConverter
    {
        private readonly IResponseJsonScraper _jsonScraper;
        private readonly IPostJsonScraper _postJsonScraper;

        public JsonToPostConverter(IServiceProvider serviceProvider)
        {
            _jsonScraper = serviceProvider.GetService<IResponseJsonScraper>();
            _postJsonScraper = serviceProvider.GetService<IPostJsonScraper>();
        }

        public IEnumerable<Post> GetPosts(JObject json)
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
            var ownerName = _postJsonScraper.GetOwnerName(postJson);
            var ownerId = _postJsonScraper.GetOwnerId(postJson);
            var shortCode = _postJsonScraper.GetShortCode(postJson);
            var usersFromPreview = _postJsonScraper.GetUsersFromCommentsPreview(postJson);
            var isPostVideo = _postJsonScraper.IsPostVideo(postJson);
            var post = new Post(ownerName, ownerId, shortCode, usersFromPreview, isPostVideo);
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
    }
}