using System;
using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.WebParsing.PageDownload;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser
{
    public class InstagramParser : IInstagramParser
    {
        private IServiceProvider _serviceProvider;
        private readonly IPageDownloader _pageDownloader;
        private readonly IInstagramPageContentScraper _pageContentScraper;
        private readonly QueryRequester _queryRequester;
        private readonly JObjectScraper _jObjectScraper;
        private List<string> _shortCodes;
        private List<ulong> _locationsId;

        private const int MaxPaginationToDownload = 2; //TODO Get this parameter from DI

        public InstagramParser(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _pageDownloader = serviceProvider.GetService<IPageDownloader>();
            _pageContentScraper = serviceProvider.GetService<IInstagramPageContentScraper>();
            _queryRequester = new QueryRequester(serviceProvider); //TODO Make DI
            _jObjectScraper = new JObjectScraper(); //TODO Make DI
            _shortCodes = new List<string>();
            _locationsId = new List<ulong>();
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

        public void DownloadLocationsAndPostsShortCodesForTargetUser(string userPage)
        {
            var userId = GetUserId(userPage);
            DownloadLocationsIdAndShortCodesFromPageContent(userPage);
            DownloadLocationsAndShortCodesFromPagination(userPage, userId);
        }

        public IEnumerable<string> GetUsersFromDownloadedShortCodes()
        {
            return _shortCodes;
        }

        public IEnumerable<ulong> GetDownloadedLocationsId()
        {
            return _locationsId;
        }

        private void DownloadLocationsIdAndShortCodesFromPageContent(string userPageContent)
        {
            DownloadLocationsIdFromPageContent(userPageContent);
            DownloadShortCodesFromPageContent(userPageContent);
        }

        private void DownloadLocationsAndShortCodesFromPagination(string userPageContent, ulong userId)
        {
            if (!_pageContentScraper.IsContentHasNextPage(userPageContent)) return;
            var json = _queryRequester.GetJsonForUserPage(userPageContent, userId);
            PaginationDownload(json, userId);
        }

        private void PaginationDownload(JObject postsJson, ulong userId)
        {
            var downloadCounter = 1;
            while (_jObjectScraper.HasNextPageForPosts(postsJson) && downloadCounter < MaxPaginationToDownload)
            {
                AddDataFromJsonInHeap(postsJson);
                var nextCursor = _jObjectScraper.GetEndOfCursorFromJsonForPosts(postsJson);
                postsJson = _queryRequester.GetJson(userId, nextCursor);
                downloadCounter++;
            }

            if (downloadCounter < MaxPaginationToDownload) AddDataFromJsonInHeap(postsJson);
        }

        private void AddDataFromJsonInHeap(JObject json)
        {
            AddLocationsIdFromJsonInHeap(json);
            AddShortCodesFromJsonInHeap(json);
        }

        private void AddShortCodesFromJsonInHeap(JObject json)
        {
            _shortCodes.AddRange(_jObjectScraper.GetShortCodesFromQueryContent(json));
        }

        private void AddLocationsIdFromJsonInHeap(JObject json)
        {
            _locationsId.AddRange(_jObjectScraper.GetLocationsIdFromQueryContent(json));
        }

        private void DownloadShortCodesFromPageContent(string userPageContent)
        {
            _shortCodes.AddRange(_pageContentScraper.GetShortCodesFromUserPage(userPageContent));
        }

        private void DownloadLocationsIdFromPageContent(string userPageContent)
        {
            _locationsId.AddRange(_pageContentScraper.GetLocationsIdFromUserPage(userPageContent));
        }

        private ulong GetUserId(string userPageContent)
        {
            return _pageContentScraper.GetUserIdFromUserPage(userPageContent);
        }

        private static string MakePostUrl(string postShortCode)
        {
            return $"https://www.instagram.com/p/{postShortCode}/";
        }

        private static string MakeUserUrl(string username)
        {
            return $"https://www.instagram.com/{username}/";
        }
    }
}