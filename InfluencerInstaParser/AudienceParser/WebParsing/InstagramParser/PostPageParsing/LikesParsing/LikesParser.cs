using System;
using System.Collections.Generic;
using System.Linq;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser.PostPageParsing.LikesParsing
{
    public class LikesParser : ILikesParser
    {
        private readonly IInstagramPostPageScraper _postPageScraper;
        private readonly QueryRequester _queryRequester;
        private readonly IResponseJsonScraper _jObjectScraper;

        private const int MaxPaginationToDownload = 2; //TODO Get this parameter from DI

        public LikesParser(IServiceProvider serviceProvider)
        {
            _postPageScraper = serviceProvider.GetService<IInstagramPostPageScraper>();
            _queryRequester = new QueryRequester(serviceProvider); //TODO Make DI
            _jObjectScraper = serviceProvider.GetService<IResponseJsonScraper>();
        }

        public IEnumerable<string> GetUsernamesFromLikes(string shortCode, string pageContent)
        {
            if (IsPostVideo(pageContent)) return new List<string>();
            return DownloadUsernamesFromPagination(shortCode);
        }

        private IEnumerable<string> DownloadUsernamesFromPagination(string shortCode)
        {
            var json = _queryRequester.GetJsonForLikes(shortCode);
            return PaginationDownLoad(json, shortCode);
        }

        private IEnumerable<string> PaginationDownLoad(JObject likesJson, string shortCode)
        {
            var downloadCounter = 1;
            var resultListOfUsernames = new List<string>();
            while (_jObjectScraper.IsNextPageExistsForLikes(likesJson) && downloadCounter < MaxPaginationToDownload)
            {
                resultListOfUsernames.AddRange(GetUsernamesFromJson(likesJson));
                var nextCursor = _jObjectScraper.GetNextCursorForLikes(likesJson);
                likesJson = _queryRequester.GetJsonForLikes(shortCode, nextCursor);
                downloadCounter++;
            }

            if (downloadCounter < MaxPaginationToDownload)
                resultListOfUsernames.AddRange(GetUsernamesFromJson(likesJson));
            return resultListOfUsernames.Distinct();
        }

        private IEnumerable<string> GetUsernamesFromJson(JObject json)
        {
            return _jObjectScraper.GetUsernamesFromLikes(json);
        }

        private bool IsPostVideo(string postPageContent)
        {
            return _postPageScraper.IsPostVideo(postPageContent);
        }
    }
}