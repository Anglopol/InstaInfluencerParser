using System;
using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.Proxy.PageDownload;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.JsonToParsedUserConverting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.PostParsing.LikesParsing
{
    public class LikesParser : ILikesParser
    {
        private readonly IResponseJsonScraper _jObjectScraper;
        private readonly IPageDownloader _pageDownloader;
        private readonly IJsonToParsedUsersConverter _converter;

        private const int MaxPaginationToDownload = 2; //TODO Get this parameter from DI

        public LikesParser(IServiceProvider serviceProvider)
        {
            _pageDownloader = serviceProvider.GetService<IPageDownloader>();
            _jObjectScraper = serviceProvider.GetService<IResponseJsonScraper>();
            _converter = serviceProvider.GetService<IJsonToParsedUsersConverter>();
        }

        public IEnumerable<ParsedUser> GetUsersFromLikes(Post post)
        {
            return post.IsVideo ? new List<ParsedUser>() : DownloadUsernamesFromPagination(post);
        }

        private IEnumerable<ParsedUser> DownloadUsernamesFromPagination(Post post)
        {
            var firstQuery = RequestParamsCreator.GetQueryUrlForLikes(post.ShortCode);
            var json = GetJsonFromInstagram(firstQuery);
            return PaginationDownload(json, post.ShortCode);
        }

        private IEnumerable<ParsedUser> PaginationDownload(JObject likesJson, string shortCode)
        {
            var downloadCounter = 1;
            var parsedUsers = new List<ParsedUser>();
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

        private IEnumerable<ParsedUser> GetUsersFromJson(JObject json)
        {
            return _converter.GetUsersFromLikes(json);
        }

        private JObject GetJsonFromInstagram(string query)
        {
            var responseBody = _pageDownloader.GetPageContent(query);
            _pageDownloader.SetClientFree();
            return JObject.Parse(responseBody);
        }
    }
}