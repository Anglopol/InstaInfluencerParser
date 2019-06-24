using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using InfluencerInstaParser.AudienceParser.UserInformation;
using InfluencerInstaParser.AudienceParser.WebParsing.PageDownload;
using Newtonsoft.Json.Linq;
using NLog;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class WebParser
    {
        private readonly PageDownloaderProxy _downloaderProxy;
        private readonly JObjectHandler _jObjectHandler;

        private readonly Logger _logger;
        private readonly User _owner;
        private readonly QueryRequester _queryRequester;
        private readonly string _userAgent;
        private readonly ParsingSetSingleton _usersSet;
        private readonly PageContentScrapper _pageContentScrapper;
        private string _rhxGis;
        private readonly DateTime _timeOfParsing;

        public List<string> ShortCodes { get; private set; }
        public List<ulong> LocationsId { get; private set; }

        public WebParser(string userAgent, User owner, DateTime timeOfParsing)
        {
            _owner = owner;
            _timeOfParsing = timeOfParsing;
            _logger = LogManager.GetCurrentClassLogger();
            _pageContentScrapper = new PageContentScrapper();
            _userAgent = userAgent;
            _usersSet = ParsingSetSingleton.GetInstance();
            _downloaderProxy = new PageDownloaderProxy();
            _jObjectHandler = new JObjectHandler();
            _queryRequester = new QueryRequester(userAgent, _downloaderProxy);
            ShortCodes = new List<string>();
            LocationsId = new List<ulong>();
        }

        public bool TryGetPostsShortCodesAndLocationsIdFromUser(string username, int countOfLoading = 10)
        {
            var userUrl = MakeUserUrl(username);
            ShortCodes = new List<string>();
            LocationsId = new List<ulong>();
            var userPageContent = _downloaderProxy.GetPageContent(userUrl, _userAgent);
            if (CheckPageOnPrivate(userPageContent)) return false;
            var userId = GetUserId(userPageContent);
            _rhxGis = GetRhxGis(userPageContent);
            GetFirstQueries(userPageContent);
            if (!_pageContentScrapper.HasNextPageForPageContent(userPageContent))
            {
                _downloaderProxy.SetProxyFree();
                return true;
            }

            var jsonPage = _queryRequester.GetJsonPageContent(userPageContent, userId, _rhxGis);
            countOfLoading--;
            GetMiddleQueries(jsonPage, userId, countOfLoading);
            GetLastQueries(jsonPage);
            _downloaderProxy.SetProxyFree();
            return true;
        }

        private bool CheckPageOnPrivate(string userPageContent)
        {
            if (!_pageContentScrapper.IsPrivate(userPageContent) &&
                !_pageContentScrapper.IsEmpty(userPageContent)) return true;
            _downloaderProxy.SetProxyFree();
            return false;
        }

        private long GetUserId(string userPageContent)
        {
            return long.Parse(_pageContentScrapper.GetUserIdFromPageContent(userPageContent));
        }

        private void GetFirstQueries(string userPageContent)
        {
            ShortCodes = _pageContentScrapper.GetListOfShortCodesFromPageContent(userPageContent);
            LocationsId = _pageContentScrapper.GetListOfLocationsFromPageContent(userPageContent);
        }

        private void GetMiddleQueries(JObject pageJson, long userId, int countOfLoading)
        {
            while (_jObjectHandler.HasNextPageForPosts(pageJson) && countOfLoading > 0)
            {
                ShortCodes.AddRange(_jObjectHandler.GetEnumerableOfShortCodesFromQueryContent(pageJson));
                LocationsId.AddRange(
                    (from queryShortCode in _jObjectHandler.GetEnumerableOfLocationsFromQueryContent(pageJson)
                        select ulong.Parse(queryShortCode)).ToList());
                var nextCursor = _jObjectHandler.GetEndOfCursorFromJsonForPosts(pageJson);
                pageJson = _queryRequester.GetJson(userId, _rhxGis, nextCursor);
                countOfLoading--;
            }
        }

        private void GetLastQueries(JObject jsonPage)
        {
            ShortCodes.AddRange(_jObjectHandler.GetEnumerableOfShortCodesFromQueryContent(jsonPage));
            LocationsId.AddRange(
                (from queryShortCode in _jObjectHandler.GetEnumerableOfLocationsFromQueryContent(jsonPage)
                    select ulong.Parse(queryShortCode)).ToList());
        }

        public void GetUsernamesFromPostComments(string postShortCode, string postPageContent = null,
            int countOfLoading = 10)
        {
            var postUrl = MakePostUrl(postShortCode);
            Console.WriteLine(postShortCode + "Comments");
            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting users from post: {postShortCode}");

            postPageContent = postPageContent ?? _downloaderProxy.GetPageContent(postUrl, _userAgent);

            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting users from post successed: {postShortCode}");

            _rhxGis = GetRhxGis(postPageContent);
            var resultList = new List<string>();
            try
            {
                resultList.AddRange(_pageContentScrapper.GetEnumerableOfUsernamesFromPageContent(postPageContent));
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "\n" + postShortCode);
                _downloaderProxy.SetProxyFree();
                throw;
            }

            if (!_pageContentScrapper.HasNextPageForPageContent(postPageContent))
            {
                FillUnprocessedSet(resultList, CommunicationType.Commentator);
                _owner.Comments += resultList.Count;
                _downloaderProxy.SetProxyFree();
                return;
            }

            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting json users from post: {postShortCode}");

            var jsonPage = _queryRequester.GetJsonPageContent(postPageContent, postShortCode, _rhxGis);
            countOfLoading--;
            while (_jObjectHandler.HasNextPageForComments(jsonPage) && countOfLoading > 0)
            {
                resultList.AddRange(_jObjectHandler.GetEnumerableOfUsernamesFromQueryContentForPost(jsonPage));
                var nextCursor = _jObjectHandler.GetEndOfCursorFromJsonForComments(jsonPage);
                jsonPage = _queryRequester.GetJson(postShortCode, _rhxGis, nextCursor);
                countOfLoading--;
            }

            resultList.AddRange(_jObjectHandler.GetEnumerableOfUsernamesFromQueryContentForPost(jsonPage));

            FillUnprocessedSet(resultList, CommunicationType.Commentator);
            _owner.Comments += resultList.Count;
            _downloaderProxy.SetProxyFree();
        }

        public void GetUsernamesFromPostLikes(string postShortCode, string postPageContent = null,
            int countOfLoading = 10)
        {
            var postUrl = MakePostUrl(postShortCode);
            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting users from post likes: {postShortCode}");
            Console.WriteLine(postShortCode + " Likes");

            postPageContent = postPageContent ?? _downloaderProxy.GetPageContent(postUrl, _userAgent);

            _logger.Info(
                $"Thread: {Thread.CurrentThread.Name} getting users from post likes seccessed: {postShortCode}");
            if (_pageContentScrapper.IsVideo(postPageContent))
            {
                Console.WriteLine($"Post {postShortCode} is video");
                _downloaderProxy.SetProxyFree();
                return;
            }

            _rhxGis = GetRhxGis(postPageContent);
            var resultList = new List<string>();
            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting json users from post likes: {postShortCode}");

            var jsonPage = _queryRequester.GetJsonForLikes(postShortCode, _rhxGis, "");

            countOfLoading--;
            while (_jObjectHandler.HasNextPageForLikes(jsonPage) && countOfLoading > 0)
            {
                resultList.AddRange(_jObjectHandler.GetEnumerableOfUsernamesFromQueryContentForLikes(jsonPage));
                var nextCursor = _jObjectHandler.GetEndOfCursorFromJsonForLikes(jsonPage);
                _logger.Info(
                    $"Thread: {Thread.CurrentThread.Name} getting json users from post likes: {postShortCode}");
                jsonPage = _queryRequester.GetJsonForLikes(postShortCode, _rhxGis, nextCursor);
                countOfLoading--;
            }

            resultList.AddRange(_jObjectHandler.GetEnumerableOfUsernamesFromQueryContentForLikes(jsonPage));

            FillUnprocessedSet(resultList, CommunicationType.Liker);
            _owner.Likes += resultList.Count;
            _downloaderProxy.SetProxyFree();
        }

        public void DetermineUserLocations(List<string> parents, double maxDistance)
        {
            _logger.Info($"Getting locations for {_owner.Username}");
            if (_owner.IsLocationProcessed)
            {
                _downloaderProxy.SetProxyFree();
                _logger.Info($"Getting locations for {_owner.Username} completed");
                return;
            }

            _owner.IsLocationProcessed = true;
            if (!TryGetPostsShortCodesAndLocationsIdFromUser(_owner.Username))
            {
                _downloaderProxy.SetProxyFree();
                _logger.Info($"Getting locations for {_owner.Username} completed");
                return;
            }

            var locator = new Locator(_downloaderProxy, _pageContentScrapper, _userAgent);
            var count = 0; // TODO: Delete this 
            foreach (var locationId in LocationsId)
            {
                if (count > 5) break; // TODO: Delete this 
                count++; // TODO: Delete this 
                if (locator.TryGetLocationByLocationId(locationId, maxDistance, out var city, out var publicId))
                    _owner.AddLocation(city, publicId, parents);
            }

            _logger.Info($"Getting locations for {_owner.Username} completed");
            _downloaderProxy.SetProxyFree();
        }

        public void FillUnprocessedSet(IEnumerable<string> list, CommunicationType type)
        {
            foreach (var username in list)
            {
                _logger.Info($"filling {username} in Set parent {_owner.Username}");
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

                _logger.Info($"Checking {username} parent {_owner.Username}");
                var isInfluencer = CheckUser(username, out var followers, out var following);
                _usersSet.AddUnprocessedUser(username, _timeOfParsing, _owner, followers, following, isInfluencer,
                    type);
            }
        }

        private bool CheckUser(string username, out int followers, out int following)
        {
            var userUrl = MakeUserUrl(username);
            var userPageContent = _downloaderProxy.GetPageContent(userUrl, _userAgent);
//            var parsingArguments = (NameValueCollection) ConfigurationManager.GetSection("parsingarguments");
//            var minNumberOfFollowers = int.Parse(parsingArguments.Get("MinFollowersValue"));
//            var subscriptionProportion = float.Parse(parsingArguments.Get("SubscriptionProportion"));
            var minNumberOfFollowers = 1000; //TODO Refactor
            var subscriptionProportion = (float) 0.2;
            followers = _pageContentScrapper.GetNumberOfFollowers(userPageContent);
            following = _pageContentScrapper.GetNumberOfFollowing(userPageContent);
            return followers > minNumberOfFollowers &&
                   following / (double) followers < subscriptionProportion;
        }

        private static string MakePostUrl(string postShortCode)
        {
            return "/p/" + postShortCode + "/";
        }

        private static string MakeUserUrl(string username)
        {
            return "/" + username + "/";
        }

        private string GetRhxGis(string pageContent)
        {
//            return _rhxGis = _rhxGis ?? _pageContentScrapper.GetRhxGisParameter(pageContent);
            return "";
        }
    }
}