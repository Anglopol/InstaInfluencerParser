using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class WebParser
    {
        private readonly WebProcessor _webProcessor;
        private QueryRequester _queryRequester;
        private string _userAgent;
        private string _rhxGis;
        private SingletonParsingSet _usersSet;
        private PageDownloaderSingleton _downloaderSingleton;
        private readonly JObjectHandler _jObjectHandler;

        private Logger _logger;

        private static object _downloaderLocker = new object();

        private static readonly object QueueLocker = new object();
        private static readonly object UnprocessedSetLocker = new object();

        public WebParser(string userAgent)
        {
            
            _logger = LogManager.GetCurrentClassLogger();
            _webProcessor = new WebProcessor();
            _queryRequester = new QueryRequester(userAgent);
            _userAgent = userAgent;
            _usersSet = SingletonParsingSet.GetInstance();
            _downloaderSingleton = PageDownloaderSingleton.GetInstance();
            _jObjectHandler = new JObjectHandler();
        }

        public void GetPostsShortCodesFromUser(string username, int countOfLoading = 4)
        {
            var userUrl = "/" + username + "/";
            string userPageContent;
            lock (_downloaderLocker)
            {
                userPageContent = Task.Run(() => _downloaderSingleton.GetPageContentWithProxy(userUrl, _userAgent))
                    .GetAwaiter()
                    .GetResult();
            }

            if (_webProcessor.IsPrivate(userPageContent))
            {
                Console.WriteLine($"{username} is private");
                return;
            }

            var userId = long.Parse(_webProcessor.GetUserIdFromPageContent(userPageContent));
            _rhxGis = _rhxGis ?? _webProcessor.GetRhxGisParameter(userPageContent);
            var resultList = _webProcessor.GetListOfShortCodesFromPageContent(userPageContent);
            if (!_webProcessor.HasNextPageForPageContent(userPageContent))
            {
                lock (QueueLocker)
                {
                    foreach (var shortCode in resultList)
                    {
                        _usersSet.AddInShortCodesQueue(shortCode);
                    }
                }

                return;
            }

            JObject jsonPage;
            lock (_downloaderLocker)
            {
                jsonPage = Task.Run(() => _queryRequester.GetJsonPageContent(userPageContent, userId, _rhxGis))
                    .GetAwaiter().GetResult();
            }

            resultList.AddRange(_jObjectHandler.GetListOfShortCodesFromQueryContent(jsonPage));
            var count = 0;
            while (_jObjectHandler.HasNextPageForPosts(jsonPage) && count < countOfLoading)
            {
                count++;
                var nextCursor = _jObjectHandler.GetEndOfCursorFromJsonForPosts(jsonPage);

                lock (_downloaderLocker)
                {
                    jsonPage = Task.Run(() => _queryRequester.GetJson(userId, _rhxGis, nextCursor)).GetAwaiter()
                        .GetResult();
                }

                resultList.AddRange(_jObjectHandler.GetListOfShortCodesFromQueryContent(jsonPage));
            }

            lock (QueueLocker)
            {
                foreach (var shortCode in resultList)
                {
                    _usersSet.AddInShortCodesQueue(shortCode);
                }
            }
        }

        public void GetUsernamesFromPostComments(string postShortCode)
        {
            var postUrl = "/p/" + postShortCode + "/";
            Console.WriteLine(postShortCode + "Comments");
            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting users from post: {postShortCode}");
            string postPageContent;
            lock (_downloaderLocker)
            {
                postPageContent = Task.Run(() => _downloaderSingleton.GetPageContentWithProxy(postUrl, _userAgent))
                    .GetAwaiter()
                    .GetResult();
            }

            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting users from post successed: {postShortCode}");
            _rhxGis = _rhxGis ?? _webProcessor.GetRhxGisParameter(postPageContent);
            var resultList = new List<string>();
            try
            {
                resultList.AddRange(_webProcessor.GetListOfUsernamesFromPageContent(postPageContent));
            }
            catch (Exception e)
            {
                Console.WriteLine(e + "\n" + postShortCode);
                throw;
            }

            if (!_webProcessor.HasNextPageForPageContent(postPageContent))
            {
                lock (UnprocessedSetLocker)
                {
                    foreach (var user in resultList)
                    {
                        _usersSet.AddUnprocessedUser(user);
                    }
                }

                return;
            }

            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting json users from post: {postShortCode}");
            JObject jsonPage;
            lock (_downloaderLocker)
            {
                jsonPage = Task.Run(() => _queryRequester.GetJsonPageContent(postPageContent, postShortCode, _rhxGis))
                    .GetAwaiter().GetResult();
            }

            resultList.AddRange(_jObjectHandler.GetListOfUsernamesFromQueryContentForPost(jsonPage));
            while (_jObjectHandler.HasNextPageForComments(jsonPage))
            {
                var nextCursor = _jObjectHandler.GetEndOfCursorFromJsonForComments(jsonPage);
                lock (_downloaderLocker)
                {
                    jsonPage = Task.Run(() => _queryRequester.GetJson(postShortCode, _rhxGis, nextCursor)).GetAwaiter()
                        .GetResult();
                }

                resultList.AddRange(_jObjectHandler.GetListOfUsernamesFromQueryContentForPost(jsonPage));
            }

            lock (UnprocessedSetLocker)
            {
                foreach (var user in resultList)
                {
                    _usersSet.AddUnprocessedUser(user);
                }
            }
        }

        public void GetUsernamesFromPostLikes(string postShortCode)
        {
            var postUrl = "/p/" + postShortCode + "/";
            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting users from post likes: {postShortCode}");
            Console.WriteLine(postShortCode + " Likes");
            string postPageContent;
            lock (_downloaderLocker)
            {
                postPageContent = Task.Run(() => _downloaderSingleton.GetPageContentWithProxy(postUrl, _userAgent))
                    .GetAwaiter()
                    .GetResult();
            }

            _logger.Info(
                $"Thread: {Thread.CurrentThread.Name} getting users from post likes seccessed: {postShortCode}");
            if (_webProcessor.IsVideo(postPageContent))
            {
                Console.WriteLine($"Post {postShortCode} is video");
                return;
            }

            _rhxGis = _rhxGis ?? _webProcessor.GetRhxGisParameter(postPageContent);
            var resultList = new List<string>();
            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting json users from post likes: {postShortCode}");
            JObject jsonPage;
            lock (_downloaderLocker)
            {
                jsonPage = Task.Run(() => _queryRequester.GetJsonForLikes(postShortCode, _rhxGis, ""))
                    .GetAwaiter().GetResult();
            }

            resultList.AddRange(_jObjectHandler.GetListOfUsernamesFromQueryContentForLikes(jsonPage));
            while (_jObjectHandler.HasNextPageForLikes(jsonPage))
            {
                var nextCursor = _jObjectHandler.GetEndOfCursorFromJsonForLikes(jsonPage);

                _logger.Info(
                    $"Thread: {Thread.CurrentThread.Name} getting json users from post likes: {postShortCode}");
                lock (_downloaderLocker)
                {
                    jsonPage = Task.Run(() => _queryRequester.GetJsonForLikes(postShortCode, _rhxGis, nextCursor))
                        .GetAwaiter().GetResult();
                }

                resultList.AddRange(_jObjectHandler.GetListOfUsernamesFromQueryContentForLikes(jsonPage));
            }

            lock (UnprocessedSetLocker)
            {
                foreach (var user in resultList)
                {
                    _usersSet.AddUnprocessedUser(user);
                }
            }
        }

//        private void ChangeUserAgent(string userAgent)
//        {
//            _userAgent = userAgent;
//            _queryRequester = new QueryRequester(userAgent);
//        }
    }
}