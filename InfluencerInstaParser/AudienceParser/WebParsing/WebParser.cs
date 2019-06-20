using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using InfluencerInstaParser.AudienceParser.UserInformation;
using InfluencerInstaParser.AudienceParser.WebParsing.PageDownload;
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
        }

        public bool TryGetPostsShortCodesAndLocationsIdFromUser(string username, out List<string> shortCodes,
            out List<ulong> locationsId, int countOfLoading = 10)
        {
            var userUrl = "/" + username + "/";
            var userPageContent = _downloaderProxy.GetPageContent(userUrl, _userAgent);
            if (_pageContentScrapper.IsPrivate(userPageContent) || _pageContentScrapper.IsEmpty(userPageContent))
            {
                _downloaderProxy.SetProxyFree();
                shortCodes = new List<string>();
                locationsId = new List<ulong>();
                return false;
            }

            var userId = long.Parse(_pageContentScrapper.GetUserIdFromPageContent(userPageContent));

            _rhxGis = "";
//            _rhxGis = _rhxGis ?? _pageContentScrapper.GetRhxGisParameter(userPageContent);
            shortCodes = _pageContentScrapper.GetListOfShortCodesFromPageContent(userPageContent);
            locationsId = _pageContentScrapper.GetListOfLocationsFromPageContent(userPageContent);
            if (!_pageContentScrapper.HasNextPageForPageContent(userPageContent))
            {
                _downloaderProxy.SetProxyFree();
                return true;
            }


            var jsonPage = _queryRequester.GetJsonPageContent(userPageContent, userId, _rhxGis);
            countOfLoading--;
            while (_jObjectHandler.HasNextPageForPosts(jsonPage) && countOfLoading > 0)
            {
                shortCodes.AddRange(_jObjectHandler.GetEnumerableOfShortCodesFromQueryContent(jsonPage));
                locationsId.AddRange(
                    (from queryShortCode in _jObjectHandler.GetEnumerableOfLocationsFromQueryContent(jsonPage)
                        select ulong.Parse(queryShortCode)).ToList());
                var nextCursor = _jObjectHandler.GetEndOfCursorFromJsonForPosts(jsonPage);
                jsonPage = _queryRequester.GetJson(userId, _rhxGis, nextCursor);
                countOfLoading--;
            }

            shortCodes.AddRange(_jObjectHandler.GetEnumerableOfShortCodesFromQueryContent(jsonPage));
            locationsId.AddRange(
                (from queryShortCode in _jObjectHandler.GetEnumerableOfLocationsFromQueryContent(jsonPage)
                    select ulong.Parse(queryShortCode)).ToList());
            _downloaderProxy.SetProxyFree();
            return true;
        }

        public void GetUsernamesFromPostComments(string postShortCode, string postPageContent = null,
            int countOfLoading = 10)
        {
            var postUrl = "/p/" + postShortCode + "/";
            Console.WriteLine(postShortCode + "Comments");
            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting users from post: {postShortCode}");

            postPageContent = postPageContent ?? _downloaderProxy.GetPageContent(postUrl, _userAgent);

            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting users from post successed: {postShortCode}");

            _rhxGis = "";
//            _rhxGis = _rhxGis ?? _pageContentScrapper.GetRhxGisParameter(postPageContent);
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
            var postUrl = "/p/" + postShortCode + "/";
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

            _rhxGis = "";
//            _rhxGis = _rhxGis ?? _pageContentScrapper.GetRhxGisParameter(postPageContent);
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

        public void
            DetermineUserLocations(List<string> parents,
                double maxDistance) //TODO make restriction for number of query downloads
        {
            _logger.Info($"Getting locations for {_owner.Username}");
            if (_owner.IsLocationProcessed)
            {
                _downloaderProxy.SetProxyFree();
                _logger.Info($"Getting locations for {_owner.Username} completed");
                return;
            }

            _owner.IsLocationProcessed = true;
            if (!TryGetPostsShortCodesAndLocationsIdFromUser(_owner.Username, out _, out var locationsId))
            {
                _downloaderProxy.SetProxyFree();
                _logger.Info($"Getting locations for {_owner.Username} completed");
                return;
            }

            var locator = new Locator(_downloaderProxy, _pageContentScrapper, _userAgent);
            var count = 0; // TODO: Delete this 
            foreach (var locationId in locationsId)
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
            var userUrl = "/" + username + "/";
            var userPageContent = _downloaderProxy.GetPageContent(userUrl, _userAgent);
//            var parsingArguments = (NameValueCollection) ConfigurationManager.GetSection("parsingarguments");
//            var minNumberOfFollowers = int.Parse(parsingArguments.Get("MinFollowersValue"));
//            var subscriptionProportion = float.Parse(parsingArguments.Get("SubscriptionProportion"));
            var minNumberOfFollowers = 1000;
            var subscriptionProportion = (float) 0.2;
            followers = _pageContentScrapper.GetNumberOfFollowers(userPageContent);
            following = _pageContentScrapper.GetNumberOfFollowing(userPageContent);
            return followers > minNumberOfFollowers &&
                   following / (double) followers < subscriptionProportion;
        }
    }
}