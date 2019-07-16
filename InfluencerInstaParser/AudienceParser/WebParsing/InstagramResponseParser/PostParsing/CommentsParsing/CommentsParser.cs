using System.Collections.Generic;
using System.Linq;
using InfluencerInstaParser.AudienceParser.Proxy.PageDownload;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.JsonToParsedUserConverting;
using Newtonsoft.Json.Linq;
using Serilog;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.PostParsing.CommentsParsing
{
    public class CommentsParser : ICommentsParser
    {
        private readonly IPageDownloader _pageDownloader;
        private readonly IResponseJsonScraper _jObjectScraper;
        private readonly IJsonToParsedUsersConverter _converter;
        private readonly ILogger _logger;

        private const int MaxPaginationToDownload = 2; //TODO Get this parameter from DI

        public CommentsParser(IPageDownloader pageDownloader,
            IResponseJsonScraper responseJsonScraper,
            IJsonToParsedUsersConverter jsonToParsedUsersConverter,
            ILogger logger)
        {
            _pageDownloader = pageDownloader;
            _jObjectScraper = responseJsonScraper;
            _converter = jsonToParsedUsersConverter;
            _logger = logger;
        }

        public IEnumerable<ParsedUserFromJson> GetUsersFromComments(Post post)
        {
            return !post.HasNextCursor ? post.UsersFromCommentsPreview : GetParsedUsers(post);
        }

        private IEnumerable<ParsedUserFromJson> GetParsedUsers(Post post)
        {
            return post.UsersFromCommentsPreview.Union(DownloadUsernamesFromPagination(post));
        }

        private IEnumerable<ParsedUserFromJson> DownloadUsernamesFromPagination(Post post)
        {
            var firstQuery = RequestParamsCreator.GetQueryUrlForComments(post.ShortCode, post.NextCommentsCursor);
            var json = GetJsonFromInstagram(firstQuery);
            return PaginationDownload(json, post.ShortCode);
        }

        private IEnumerable<ParsedUserFromJson> PaginationDownload(JObject commentsJson, string shortCode)
        {
            var downloadCounter = 1;
            var parsedUsers = new List<ParsedUserFromJson>();
            while (_jObjectScraper.IsNextPageExistsForComments(commentsJson) &&
                   downloadCounter < MaxPaginationToDownload)
            {
                parsedUsers.AddRange(GetUsersFromJson(commentsJson));
                var nextCursor = _jObjectScraper.GetNextCursorForComments(commentsJson);
                var nextQuery = RequestParamsCreator.GetQueryUrlForComments(shortCode, nextCursor);
                commentsJson = GetJsonFromInstagram(nextQuery);
                downloadCounter++;
            }

            if (downloadCounter < MaxPaginationToDownload)
                parsedUsers.AddRange(GetUsersFromJson(commentsJson));
            return parsedUsers;
        }

        private IEnumerable<ParsedUserFromJson> GetUsersFromJson(JObject json)
        {
            return _converter.GetUsersFromComments(json);
        }

        private JObject GetJsonFromInstagram(string query)
        {
            var responseBody = _pageDownloader.GetPageContent(query);
            var result = JObject.Parse(responseBody);
            _pageDownloader.SetClientFree();
            return result;
        }
    }
}