using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.Proxy.PageDownload;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.JsonToParsedUserConverting;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser.PostParsing.LikesParsing
{
    public class LikeParserOptions
    {
    }

    public class LikesParser : ILikesParser
    {
        private readonly IResponseJsonScraper _jObjectScraper;
        private readonly IPageDownloader _pageDownloader;
        private readonly IJsonToParsedUsersConverter _converter;

        private const int MaxPaginationToDownload = 2; //TODO Get this parameter from DI

        public LikesParser(IPageDownloader pageDownloader, IResponseJsonScraper responseJsonScraper,
            IJsonToParsedUsersConverter jsonToParsedUsersConverter)
        {
            _pageDownloader = pageDownloader;
            _jObjectScraper = responseJsonScraper;
            _converter = jsonToParsedUsersConverter;
        }

        public IEnumerable<ParsedUserFromJson> GetUsersFromLikes(Post post)
        {
            return post.IsVideo ? new List<ParsedUserFromJson>() : DownloadUsernamesFromPagination(post);
        }

        private IEnumerable<ParsedUserFromJson> DownloadUsernamesFromPagination(Post post)
        {
            var firstQuery = RequestParamsCreator.GetQueryUrlForLikes(post.ShortCode);
            var json = GetJsonFromInstagram(firstQuery);
            return PaginationDownload(json, post.ShortCode);
        }

        private IEnumerable<ParsedUserFromJson> PaginationDownload(JObject likesJson, string shortCode)
        {
            var downloadCounter = 1;
            var parsedUsers = new List<ParsedUserFromJson>();
            while (_jObjectScraper.IsNextPageExistsForLikes(likesJson) && downloadCounter < MaxPaginationToDownload)
            {
                parsedUsers.AddRange(GetUsersFromJson(likesJson));
                var nextCursor = _jObjectScraper.GetNextCursorForLikes(likesJson);
                var nextQuery = RequestParamsCreator.GetQueryUrlForLikes(shortCode, nextCursor);
                likesJson = GetJsonFromInstagram(nextQuery);
                downloadCounter++;
            }

            if (downloadCounter < MaxPaginationToDownload) parsedUsers.AddRange(GetUsersFromJson(likesJson));
            return parsedUsers;
        }

        private IEnumerable<ParsedUserFromJson> GetUsersFromJson(JObject json)
        {
            return _converter.GetUsersFromLikes(json);
        }

        private JObject GetJsonFromInstagram(string query)
        {
            var responseBody = _pageDownloader.GetPageContent(query);
            var result =  JObject.Parse(responseBody);
            _pageDownloader.SetClientFree();
            return result;
        }
    }
}