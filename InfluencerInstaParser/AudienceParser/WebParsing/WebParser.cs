using System;
using System.Collections.Generic;
using System.Linq;
using InfluencerInstaParser.AudienceParser.UserInformation;
using InfluencerInstaParser.AudienceParser.WebParsing.Locate;
using InfluencerInstaParser.AudienceParser.WebParsing.PageDownload;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class WebParser
    {
        private readonly IPageDownloader _downloaderProxy;
        private readonly JObjectScraper _jObjectScraper;
        private readonly User _owner;
        private readonly QueryRequester _queryRequester;
        private readonly ParsingSetSingleton _usersSet;
        private readonly PageContentScraper _pageContentScraper;
        private readonly DateTime _timeOfParsing;
        private readonly IServiceProvider _serviceProvider;

        public List<string> ShortCodes { get; private set; }
        public List<ulong> LocationsId { get; private set; }

        public WebParser(IServiceProvider serviceProvider, User owner, DateTime timeOfParsing)
        {
            _serviceProvider = serviceProvider;
            _downloaderProxy = _serviceProvider.GetService<IPageDownloader>();
            _owner = owner;
            _timeOfParsing = timeOfParsing;
            _pageContentScraper = new PageContentScraper();
            _usersSet = ParsingSetSingleton.GetInstance();
            _jObjectScraper = new JObjectScraper();
            _queryRequester = new QueryRequester(serviceProvider);
            ShortCodes = new List<string>();
            LocationsId = new List<ulong>();
        }

        public bool TryGetPostsShortCodesAndLocationsIdFromUser(string username, int countOfLoading = 10)
        {
            var userUrl = MakeUserUrl(username);
            ShortCodes = new List<string>();
            LocationsId = new List<ulong>();
            var userPageContent = _downloaderProxy.GetPageContent(userUrl);
            if (CheckPageOnPrivate(userPageContent)) return false;
            var userId = GetUserId(userPageContent);
            GetFirstQueries(userPageContent);
            if (!_pageContentScraper.HasNextPageForPageContent(userPageContent))
            {
                _downloaderProxy.SetClientFree();
                return true;
            }

            var jsonPage = _queryRequester.GetJsonForUserPage(userPageContent, userId);
            countOfLoading--;
            GetMiddleQueries(jsonPage, userId, countOfLoading);
            GetLastQueries(jsonPage);
            _downloaderProxy.SetClientFree();
            return true;
        }

        private bool CheckPageOnPrivate(string userPageContent)
        {
            return !_pageContentScraper.IsPrivate(userPageContent) &&
                   !_pageContentScraper.IsEmpty(userPageContent);
        }

        private long GetUserId(string userPageContent)
        {
            return long.Parse(_pageContentScraper.GetUserIdFromPageContent(userPageContent));
        }

        private void GetFirstQueries(string userPageContent)
        {
            ShortCodes = _pageContentScraper.GetListOfShortCodesFromPageContent(userPageContent);
            LocationsId = _pageContentScraper.GetListOfLocationsFromPageContent(userPageContent);
        }

        private void GetMiddleQueries(JObject pageJson, long userId, int countOfLoading)
        {
            while (_jObjectScraper.HasNextPageForPosts(pageJson) && countOfLoading > 0)
            {
                ShortCodes.AddRange(_jObjectScraper.GetShortCodesFromQueryContent(pageJson));
                LocationsId.AddRange(
                    (from queryShortCode in _jObjectScraper.GetLocationsIdFromQueryContent(pageJson)
                        select ulong.Parse(queryShortCode)).ToList());
                var nextCursor = _jObjectScraper.GetEndOfCursorFromJsonForPosts(pageJson);
                pageJson = _queryRequester.GetJson(userId, nextCursor);
                countOfLoading--;
            }
        }

        private void GetLastQueries(JObject jsonPage)
        {
            ShortCodes.AddRange(_jObjectScraper.GetShortCodesFromQueryContent(jsonPage));
            LocationsId.AddRange(
                (from queryShortCode in _jObjectScraper.GetLocationsIdFromQueryContent(jsonPage)
                    select ulong.Parse(queryShortCode)).ToList());
        }

        public void GetUsernamesFromPostComments(string postShortCode, string postPageContent = null,
            int countOfLoading = 10)
        {
            var postUrl = MakePostUrl(postShortCode);
            postPageContent = postPageContent ?? _downloaderProxy.GetPageContent(postUrl);
            var resultList = new List<string>();
            resultList.AddRange(_pageContentScraper.GetEnumerableOfUsernamesFromPageContent(postPageContent));
            if (!_pageContentScraper.HasNextPageForPageContent(postPageContent))
            {
                FillUnprocessedSet(resultList, CommunicationType.Commentator);
                _owner.Comments += resultList.Count;
                _downloaderProxy.SetClientFree();
                return;
            }
            var jsonPage = _queryRequester.GetJsonPageContent(postPageContent, postShortCode);
            countOfLoading--;
            while (_jObjectScraper.HasNextPageForComments(jsonPage) && countOfLoading > 0)
            {
                resultList.AddRange(_jObjectScraper.GetUsernamesFromQueryContentForPost(jsonPage));
                var nextCursor = _jObjectScraper.GetEndOfCursorFromJsonForComments(jsonPage);
                jsonPage = _queryRequester.GetJson(postShortCode, nextCursor);
                countOfLoading--;
            }
            resultList.AddRange(_jObjectScraper.GetUsernamesFromQueryContentForPost(jsonPage));
            FillUnprocessedSet(resultList, CommunicationType.Commentator);
            _owner.Comments += resultList.Count;
            _downloaderProxy.SetClientFree();
        }

        public void GetUsernamesFromPostLikes(string postShortCode, string postPageContent = null,
            int countOfLoading = 10)
        {
            var postUrl = MakePostUrl(postShortCode);
            postPageContent = postPageContent ?? _downloaderProxy.GetPageContent(postUrl);

            if (_pageContentScraper.IsVideo(postPageContent))
            {
                _downloaderProxy.SetClientFree();
                return;
            }

            var resultList = new List<string>();
            var jsonPage = _queryRequester.GetJsonForLikes(postShortCode);

            countOfLoading--;
            while (_jObjectScraper.HasNextPageForLikes(jsonPage) && countOfLoading > 0)
            {
                resultList.AddRange(_jObjectScraper.GetUsernamesFromQueryContentForLikes(jsonPage));
                var nextCursor = _jObjectScraper.GetEndOfCursorFromJsonForLikes(jsonPage);
                jsonPage = _queryRequester.GetJsonForLikes(postShortCode, nextCursor);
                countOfLoading--;
            }

            resultList.AddRange(_jObjectScraper.GetUsernamesFromQueryContentForLikes(jsonPage));

            FillUnprocessedSet(resultList, CommunicationType.Liker);
            _owner.Likes += resultList.Count;
            _downloaderProxy.SetClientFree();
        }

        public void DetermineUserLocations(IEnumerable<string> parents, double maxDistance)
        {
            if (_owner.IsLocationProcessed)
            {
                _downloaderProxy.SetClientFree();
                return;
            }

            _owner.IsLocationProcessed = true;
            if (!TryGetPostsShortCodesAndLocationsIdFromUser(_owner.Username))
            {
                _downloaderProxy.SetClientFree();
                return;
            }

            var locator = _serviceProvider.GetService<ILocator>();
            var count = 0; // TODO: Delete this 
            foreach (var locationId in LocationsId)
            {
                if (count > 5) break; // TODO: Delete this 
                count++; // TODO: Delete this
                if (locator.IsCityAlreadyCached(locationId))
                {
                    var scrapingResult = locator.GetCachedCityByLocationId(locationId);
                    if (scrapingResult.Distance <= maxDistance)
                        _owner.AddLocation(scrapingResult.Name, scrapingResult.PublicId, parents);
                    continue;
                }

                var locationPage = locator.GetLocationPageContentByLocationId(locationId);
                if (!locator.IsLocationPageContentValid(locationPage)) continue;
                var scrapResult = locator.GetNearestCityByLocationPageContent(locationPage, locationId);
                if (scrapResult.Distance <= maxDistance)
                    _owner.AddLocation(scrapResult.Name, scrapResult.PublicId, parents);
            }

            _downloaderProxy.SetClientFree();
        }

        public void FillUnprocessedSet(IEnumerable<string> list, CommunicationType type)
        {
            foreach (var username in list)
            {
                if (_usersSet.ProcessedUsers.ContainsKey(username) ||
                    _usersSet.UnprocessedUsers.ContainsKey(username))
                {
                    var currentUser = _usersSet.UnprocessedUsers.ContainsKey(username)
                        ? _usersSet.UnprocessedUsers[username]
                        : _usersSet.ProcessedUsers[username];
                    _usersSet.AddUnprocessedUser(username, _timeOfParsing, _owner, currentUser.Followers,
                        currentUser.Following,
                        currentUser.IsInfluencer, type);
                    continue;
                }

                var isInfluencer = CheckUser(username, out var followers, out var following);
                _usersSet.AddUnprocessedUser(username, _timeOfParsing, _owner, followers, following, isInfluencer,
                    type);
            }
        }

        private bool CheckUser(string username, out int followers, out int following)
        {
            var userUrl = MakeUserUrl(username);
            var userPageContent = _downloaderProxy.GetPageContent(userUrl);
            var minNumberOfFollowers = 1000; //TODO Refactor
            var subscriptionProportion = (float) 0.2;
            followers = _pageContentScraper.GetNumberOfFollowers(userPageContent);
            following = _pageContentScraper.GetNumberOfFollowing(userPageContent);
            return followers > minNumberOfFollowers &&
                   following / (double) followers < subscriptionProportion;
        }

        private static string MakePostUrl(string postShortCode)
        {
            return $"https://www.instagram.com/p/{postShortCode}/";
        }

        private static string MakeUserUrl(string username)
        {
            return $"https://www.instagram.com/{username}/";
        }

//        private static string GetRhxGis(string pageContent)
//        {
//            return _rhxGis = _rhxGis ?? _pageContentScrapper.GetRhxGisParameter(pageContent);
//        }
    }
}