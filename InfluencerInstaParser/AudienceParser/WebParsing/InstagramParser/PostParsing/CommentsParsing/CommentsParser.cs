using System;
using System.Collections.Generic;
using System.Linq;
using InfluencerInstaParser.AudienceParser.Proxy.PageDownload;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.JsonScraping.JsonToParsedUserConverting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.PostParsing.CommentsParsing
{
    public class CommentsParser : ICommentsParser
    {
        private readonly IPageDownloader _pageDownloader;
        private readonly IResponseJsonScraper _jObjectScraper;
        private readonly IJsonToParsedUsersConverter _converter;

        private const int MaxPaginationToDownload = 2; //TODO Get this parameter from DI

        public CommentsParser(IServiceProvider serviceProvider)
        {
            _pageDownloader = serviceProvider.GetService<IPageDownloader>();
            _jObjectScraper = serviceProvider.GetService<IResponseJsonScraper>();
            _converter = serviceProvider.GetService<IJsonToParsedUsersConverter>();
        }

        public IEnumerable<ParsedUser> GetUsersFromComments(Post post)
        {
            return !post.HasNextCursor ? post.UsersFromCommentsPreview : GetParsedUsers(post);
        }

        private IEnumerable<ParsedUser> GetParsedUsers(Post post)
        {
            return post.UsersFromCommentsPreview.Union(DownloadUsernamesFromPagination(post));
        }

        private IEnumerable<ParsedUser> DownloadUsernamesFromPagination(Post post)
        {
            var firstQuery = RequestParamsCreator.GetQueryUrlForComments(post.ShortCode, post.NextCommentsCursor);
            var json = GetJsonFromInstagram(firstQuery);
            return PaginationDownload(json, post.ShortCode);
        }

        private IEnumerable<ParsedUser> PaginationDownload(JObject commentsJson, string shortCode)
        {
            var downloadCounter = 1;
            var parsedUsers = new List<ParsedUser>();
            while (_jObjectScraper.IsNextPageExistsForComments(commentsJson) && downloadCounter < MaxPaginationToDownload)
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

        private IEnumerable<ParsedUser> GetUsersFromJson(JObject json)
        {
            return _converter.GetUsersFromComments(json);
        }
        
        private JObject GetJsonFromInstagram(string query)
        {
            var responseBody = _pageDownloader.GetPageContent(query);
            _pageDownloader.SetClientFree();
            return JObject.Parse(responseBody);
        }
    }
}