using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
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

        public WebParser(string userAgent, User owner)
        {
            _owner = owner;
            _logger = LogManager.GetCurrentClassLogger();
            _pageContentScrapper = new PageContentScrapper();
            _userAgent = userAgent;
            _usersSet = ParsingSetSingleton.GetInstance();
            _downloaderProxy = new PageDownloaderProxy();
            _jObjectHandler = new JObjectHandler();
            _queryRequester = new QueryRequester(userAgent, _downloaderProxy);
        }

        public bool TryGetPostsShortCodesFromUser(string username, out List<string> shortCodes, int countOfLoading = 0)
        {
            var userUrl = "/" + username + "/";

            var userPageContent = _downloaderProxy.GetPageContent(userUrl, _userAgent);

            if (_pageContentScrapper.IsPrivate(userPageContent) || _pageContentScrapper.IsEmpty(userPageContent))
            {
                Console.WriteLine($"{username} is invalid");
                _downloaderProxy.SetProxyFree();
                shortCodes = new List<string>();
                return false;
            }

            var userId = long.Parse(_pageContentScrapper.GetUserIdFromPageContent(userPageContent));
            _rhxGis = _rhxGis ?? _pageContentScrapper.GetRhxGisParameter(userPageContent);
            var resultList = _pageContentScrapper.GetListOfShortCodesFromPageContent(userPageContent);
            if (!_pageContentScrapper.HasNextPageForPageContent(userPageContent))
            {
                _downloaderProxy.SetProxyFree();
                shortCodes = resultList;
                return true;
            }


            var jsonPage = _queryRequester.GetJsonPageContent(userPageContent, userId, _rhxGis);

            resultList.AddRange(_jObjectHandler.GetListOfShortCodesFromQueryContent(jsonPage));
            var count = 0;
            while (_jObjectHandler.HasNextPageForPosts(jsonPage) && count < countOfLoading)
            {
                count++;
                var nextCursor = _jObjectHandler.GetEndOfCursorFromJsonForPosts(jsonPage);
                jsonPage = _queryRequester.GetJson(userId, _rhxGis, nextCursor);

                resultList.AddRange(_jObjectHandler.GetListOfShortCodesFromQueryContent(jsonPage));
            }

            shortCodes = resultList;
            _downloaderProxy.SetProxyFree();
            return true;
        }

        public void GetUsernamesFromPostComments(string postShortCode)
        {
            var postUrl = "/p/" + postShortCode + "/";
            Console.WriteLine(postShortCode + "Comments");
            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting users from post: {postShortCode}");

            var postPageContent = _downloaderProxy.GetPageContent(postUrl, _userAgent);

            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting users from post successed: {postShortCode}");
            _rhxGis = _rhxGis ?? _pageContentScrapper.GetRhxGisParameter(postPageContent);
            var resultList = new List<string>();
            try
            {
                resultList.AddRange(_pageContentScrapper.GetListOfUsernamesFromPageContent(postPageContent));
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

            resultList.AddRange(_jObjectHandler.GetListOfUsernamesFromQueryContentForPost(jsonPage));
            while (_jObjectHandler.HasNextPageForComments(jsonPage))
            {
                var nextCursor = _jObjectHandler.GetEndOfCursorFromJsonForComments(jsonPage);
                jsonPage = _queryRequester.GetJson(postShortCode, _rhxGis, nextCursor);
                resultList.AddRange(_jObjectHandler.GetListOfUsernamesFromQueryContentForPost(jsonPage));
            }

            FillUnprocessedSet(resultList, CommunicationType.Commentator);
            _owner.Comments += resultList.Count;
            _downloaderProxy.SetProxyFree();
        }

        public void GetUsernamesFromPostLikes(string postShortCode)
        {
            var postUrl = "/p/" + postShortCode + "/";
            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting users from post likes: {postShortCode}");
            Console.WriteLine(postShortCode + " Likes");

            var postPageContent = _downloaderProxy.GetPageContent(postUrl, _userAgent);

            _logger.Info(
                $"Thread: {Thread.CurrentThread.Name} getting users from post likes seccessed: {postShortCode}");
            if (_pageContentScrapper.IsVideo(postPageContent))
            {
                Console.WriteLine($"Post {postShortCode} is video");
                _downloaderProxy.SetProxyFree();
                return;
            }

            _rhxGis = _rhxGis ?? _pageContentScrapper.GetRhxGisParameter(postPageContent);
            var resultList = new List<string>();
            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting json users from post likes: {postShortCode}");

            var jsonPage = _queryRequester.GetJsonForLikes(postShortCode, _rhxGis, "");

            resultList.AddRange(_jObjectHandler.GetListOfUsernamesFromQueryContentForLikes(jsonPage));
            while (_jObjectHandler.HasNextPageForLikes(jsonPage))
            {
                var nextCursor = _jObjectHandler.GetEndOfCursorFromJsonForLikes(jsonPage);

                _logger.Info(
                    $"Thread: {Thread.CurrentThread.Name} getting json users from post likes: {postShortCode}");

                jsonPage = _queryRequester.GetJsonForLikes(postShortCode, _rhxGis, nextCursor);

                resultList.AddRange(_jObjectHandler.GetListOfUsernamesFromQueryContentForLikes(jsonPage));
            }

            FillUnprocessedSet(resultList, CommunicationType.Liker);
            _owner.Likes += resultList.Count;
            _downloaderProxy.SetProxyFree();
        }

        public void GetUserLocations()
        {
            _logger.Info($"Getting locations for {_owner.Username}");
            if (_owner.IsLocationProcessed)
            {
                _downloaderProxy.SetProxyFree();
                _logger.Info($"Getting locations for {_owner.Username} completed");
                return;
            }

            _owner.IsLocationProcessed = true;
            if (!TryGetPostsShortCodesFromUser(_owner.Username, out var shortCodes))
            {
                _downloaderProxy.SetProxyFree();
                _logger.Info($"Getting locations for {_owner.Username} completed");
                return;
            }

            foreach (var shortCode in shortCodes)
            {
                _logger.Info($"Getting location for {_owner.Username} from {shortCode}");
                var postUrl = "/p/" + shortCode + "/";
                var postPageContent = _downloaderProxy.GetPageContent(postUrl, _userAgent);
                _logger.Info($"Page {shortCode} for {_owner.Username} received");
                if (!_pageContentScrapper.IsPostHasLocation(postPageContent)) continue;
                _logger.Info($"Starting locator for {shortCode}");
                var locator = new Locator(_downloaderProxy, _pageContentScrapper, _userAgent);
                if (locator.TryGetPostLocation(postPageContent, out var city)) _owner.AddLocation(city);
            }

            _logger.Info($"Getting locations for {_owner.Username} completed");
            _downloaderProxy.SetProxyFree();
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
                    _usersSet.AddUnprocessedUser(username, _owner, currentUser.Followers, currentUser.Following,
                        currentUser.IsInfluencer, type);
                    continue;
                }

                var isInfluencer = CheckUser(username, out var followers, out var following);
                _usersSet.AddUnprocessedUser(username, _owner, followers, following, isInfluencer, type);
            }
        }

        public void FillShortCodesQueue(IEnumerable<string> list)
        {
            foreach (var shortCode in list) _usersSet.AddInShortCodesQueue(shortCode);
        }

        private bool CheckUser(string username, out int followers, out int following)
        {
            var userUrl = "/" + username + "/";
            var userPageContent = _downloaderProxy.GetPageContent(userUrl, _userAgent);
            var parsingArguments = (NameValueCollection) ConfigurationManager.GetSection("parsingarguments");
            var minNumberOfFollowers = int.Parse(parsingArguments.Get("MinFollowersValue"));
            var subscriptionProportion = float.Parse(parsingArguments.Get("SubscriptionProportion"));
            followers = _pageContentScrapper.GetNumberOfFollowers(userPageContent);
            following = _pageContentScrapper.GetNumberOfFollowing(userPageContent);
            return followers > minNumberOfFollowers &&
                   following / (double) followers < subscriptionProportion;
        }
    }
}