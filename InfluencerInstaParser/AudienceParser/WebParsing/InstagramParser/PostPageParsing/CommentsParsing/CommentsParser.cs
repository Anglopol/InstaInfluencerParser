using System;
using System.Collections.Generic;
using System.Linq;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.PostPageParsing.CommentsParsing
{
    public class CommentsParser : ICommentsParser
    {
        private readonly IInstagramPostPageScraper _postPageScraper;
        private readonly QueryRequester _queryRequester;
        private readonly JObjectScraper _jObjectScraper;

        private const int MaxPaginationToDownload = 2; //TODO Get this parameter from DI

        public CommentsParser(IServiceProvider serviceProvider)
        {
            _postPageScraper = serviceProvider.GetService<IInstagramPostPageScraper>();
            _queryRequester = new QueryRequester(serviceProvider); //TODO Make DI
            _jObjectScraper = new JObjectScraper(); //TODO Make DI
        }

        public IEnumerable<string> GetUsernamesFromComments(string shortCode, string pageContent)
        {
            var usernames = DownloadUsernamesFromPageContent(pageContent);
            var resultHeapOfUsernames = usernames.Union(DownloadUsernamesFromPagination(shortCode, pageContent));
            return resultHeapOfUsernames.Distinct();
        }

        private IEnumerable<string> DownloadUsernamesFromPageContent(string pageContent)
        {
            return _postPageScraper.GetUsernamesFromPostPage(pageContent);
        }

        private IEnumerable<string> DownloadUsernamesFromPagination(string shortCode, string pageContent)
        {
            if (!_postPageScraper.IsContentHasNextPage(pageContent)) return new List<string>();
            var json = _queryRequester.GetJsonPageContent(pageContent, shortCode);
            return PaginationDownload(json, shortCode);
        }

        private IEnumerable<string> PaginationDownload(JObject commentsJson, string shortCode)
        {
            var downloadCounter = 1;
            var resultListOfUsernames = new List<string>();
            while (_jObjectScraper.HasNextPageForComments(commentsJson) && downloadCounter < MaxPaginationToDownload)
            {
                resultListOfUsernames.AddRange(GetUsernamesFromJson(commentsJson));
                var nextCursor = _jObjectScraper.GetEndOfCursorFromJsonForComments(commentsJson);
                commentsJson = _queryRequester.GetJson(shortCode, nextCursor);
                downloadCounter++;
            }

            if (downloadCounter < MaxPaginationToDownload)
                resultListOfUsernames.AddRange(GetUsernamesFromJson(commentsJson));
            return resultListOfUsernames.Distinct();
        }

        private IEnumerable<string> GetUsernamesFromJson(JObject json)
        {
            return _jObjectScraper.GetUsernamesFromQueryContentForPost(json);
        }
    }
}