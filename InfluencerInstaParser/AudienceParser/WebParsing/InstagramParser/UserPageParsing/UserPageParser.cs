using System;
using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.UserPageParsing.JsonToPostConverting;
using InfluencerInstaParser.AudienceParser.WebParsing.PageDownload;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.UserPageParsing
{
    public class UserPageParser : IUserPageParser
    {
        private readonly IPageDownloader _pageDownloader;
        private readonly IInstagramUserPageScraper _pageContentScraper;
        private readonly IJsonToPostConverter _converter;
        private readonly IResponseJsonScraper _jObjectScraper;

        private const int MaxPaginationToDownload = 2; //TODO Get this parameter from DI

        public UserPageParser(IServiceProvider serviceProvider)
        {
            _pageDownloader = serviceProvider.GetService<IPageDownloader>();
            _pageContentScraper = serviceProvider.GetService<IInstagramUserPageScraper>();
            _converter = serviceProvider.GetService<IJsonToPostConverter>();
            _jObjectScraper = serviceProvider.GetService<IResponseJsonScraper>();
        }

        public string GetUserPage(string username)
        {
            var userUrl = MakeUserUrl(username);
            var userPageContent = _pageDownloader.GetPageContent(userUrl);
            return userPageContent;
        }

        public bool IsUserPageValid(string userPageContent)
        {
            return !_pageContentScraper.IsUserPageEmpty(userPageContent) &&
                   !_pageContentScraper.IsUserPagePrivate(userPageContent);
        }

        public ulong GetUserId(string userPageContent)
        {
            return _pageContentScraper.GetUserIdFromUserPage(userPageContent);
        }

        public IEnumerable<Post> GetPostsFromUser(ulong userId)
        {
            var requestUrl = RequestParamsCreator.GetQueryUrlForPosts(userId);
            var firstJsonFromUserPage = GetJsonFromInstagram(requestUrl);
            return PaginationDownload(firstJsonFromUserPage, userId);
        }

        private IEnumerable<Post> PaginationDownload(JObject postsJson, ulong userId)
        {
            var downloadCounter = 1;
            var posts = new List<Post>();
            while (_jObjectScraper.IsNextPageExistsForPosts(postsJson) && downloadCounter < MaxPaginationToDownload)
            {
                posts.AddRange(ConvertJsonToPosts(postsJson));
                var nextCursor = _jObjectScraper.GetNextCursorForPosts(postsJson);
                var nextQuery = RequestParamsCreator.GetQueryUrlForPosts(userId, nextCursor);
                postsJson = GetJsonFromInstagram(nextQuery);
                downloadCounter++;
            }

            if (downloadCounter < MaxPaginationToDownload) posts.AddRange(ConvertJsonToPosts(postsJson));
            return posts;
        }

        private IEnumerable<Post> ConvertJsonToPosts(JObject json)
        {
            return _converter.GetPosts(json);
        }


        private JObject GetJsonFromInstagram(string query)
        {
            var responseBody = _pageDownloader.GetPageContent(query);
            _pageDownloader.SetClientFree();
            return JObject.Parse(responseBody);
        }

        private static string MakeUserUrl(string username)
        {
            return $"https://www.instagram.com/{username}/";
        }
    }
}