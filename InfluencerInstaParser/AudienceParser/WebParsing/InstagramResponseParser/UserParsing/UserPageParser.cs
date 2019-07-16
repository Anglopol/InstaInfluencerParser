using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.Proxy.PageDownload;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.JsonToPostConverting;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping;
using Newtonsoft.Json.Linq;
using Serilog;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.UserParsing
{
    public class UserPageParser : IUserPageParser
    {
        private readonly IPageDownloader _pageDownloader;
        private readonly IInstagramUserPageScraper _pageContentScraper;
        private readonly IJsonToPostConverter _converter;
        private readonly IResponseJsonScraper _jObjectScraper;
        private readonly ILogger _logger;

        private const int MaxPaginationToDownload = 2; //TODO Get this parameter from DI

        public UserPageParser(IPageDownloader pageDownloader, IInstagramUserPageScraper userPageScraper,
            IJsonToPostConverter toPostConverter, IResponseJsonScraper jsonScraper, ILogger logger)
        {
            _pageDownloader = pageDownloader;
            _pageContentScraper = userPageScraper;
            _converter = toPostConverter;
            _jObjectScraper = jsonScraper;
            _logger = logger;
        }

        public string GetUserPage(string username)
        {
            var userUrl = MakeUserUrl(username);
            var userPageContent = _pageDownloader.GetPageContent(userUrl);
            _pageDownloader.SetClientFree();
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
            _logger.Debug("Getting Posts from {userId}", userId);
            var requestUrl = RequestParamsCreator.GetQueryUrlForPosts(userId);
            _logger.Verbose("Request url: {url}", requestUrl);
            var firstJsonFromUserPage = GetJsonFromInstagram(requestUrl);
            _logger.Verbose("Starting pagination download");
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
            _logger.Verbose("JSON: {value}", responseBody);
            var result = JObject.Parse(responseBody);
            _pageDownloader.SetClientFree();
            return result;
        }

        private static string MakeUserUrl(string username)
        {
            return $"https://www.instagram.com/{username}/";
        }
    }
}