using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class WebParser
    {
        private static readonly object QueueLocker = new object();
        private static readonly object UnprocessedSetLocker = new object();
        private readonly PageDownloaderProxy _downloaderProxy;
        private readonly JObjectHandler _jObjectHandler;

        private readonly Logger _logger;
        private readonly QueryRequester _queryRequester;
        private readonly string _userAgent;
        private readonly SingletonParsingSet _usersSet;
        private readonly WebProcessor _webProcessor;
        private string _rhxGis;

        public WebParser(string userAgent)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _webProcessor = new WebProcessor();
            _userAgent = userAgent;
            _usersSet = SingletonParsingSet.GetInstance();
            _downloaderProxy = new PageDownloaderProxy();
            _jObjectHandler = new JObjectHandler();
            _queryRequester = new QueryRequester(userAgent, _downloaderProxy);
        }

        public void GetPostsShortCodesFromUser(string username, int countOfLoading = 4)
        {
            var userUrl = "/" + username + "/";

            var userPageContent = Task.Run(() => _downloaderProxy.GetPageContentWithProxy(userUrl, _userAgent))
                .GetAwaiter()
                .GetResult();

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
                FillShortCodesQueue(resultList);
                return;
            }


            var jsonPage = Task.Run(() => _queryRequester.GetJsonPageContent(userPageContent, userId, _rhxGis))
                .GetAwaiter().GetResult();
            resultList.AddRange(_jObjectHandler.GetListOfShortCodesFromQueryContent(jsonPage));
            var count = 0;
            while (_jObjectHandler.HasNextPageForPosts(jsonPage) && count < countOfLoading)
            {
                count++;
                var nextCursor = _jObjectHandler.GetEndOfCursorFromJsonForPosts(jsonPage);
                jsonPage = Task.Run(() => _queryRequester.GetJson(userId, _rhxGis, nextCursor)).GetAwaiter()
                    .GetResult();

                resultList.AddRange(_jObjectHandler.GetListOfShortCodesFromQueryContent(jsonPage));
            }

            FillShortCodesQueue(resultList);
        }

        public void GetUsernamesFromPostComments(string postShortCode)
        {
            var postUrl = "/p/" + postShortCode + "/";
            Console.WriteLine(postShortCode + "Comments");
            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting users from post: {postShortCode}");

            var postPageContent = Task.Run(() => _downloaderProxy.GetPageContentWithProxy(postUrl, _userAgent))
                .GetAwaiter()
                .GetResult();

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
                FillUnprocessedSet(resultList);
                return;
            }

            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting json users from post: {postShortCode}");

            var jsonPage = Task.Run(() => _queryRequester.GetJsonPageContent(postPageContent, postShortCode, _rhxGis))
                .GetAwaiter().GetResult();

            resultList.AddRange(_jObjectHandler.GetListOfUsernamesFromQueryContentForPost(jsonPage));
            while (_jObjectHandler.HasNextPageForComments(jsonPage))
            {
                var nextCursor = _jObjectHandler.GetEndOfCursorFromJsonForComments(jsonPage);
                jsonPage = Task.Run(() => _queryRequester.GetJson(postShortCode, _rhxGis, nextCursor)).GetAwaiter()
                    .GetResult();
                resultList.AddRange(_jObjectHandler.GetListOfUsernamesFromQueryContentForPost(jsonPage));
            }

            FillUnprocessedSet(resultList);
        }

        public void GetUsernamesFromPostLikes(string postShortCode)
        {
            var postUrl = "/p/" + postShortCode + "/";
            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting users from post likes: {postShortCode}");
            Console.WriteLine(postShortCode + " Likes");

            var postPageContent = Task.Run(() => _downloaderProxy.GetPageContentWithProxy(postUrl, _userAgent))
                .GetAwaiter()
                .GetResult();

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

            var jsonPage = Task.Run(() => _queryRequester.GetJsonForLikes(postShortCode, _rhxGis, ""))
                .GetAwaiter().GetResult();

            resultList.AddRange(_jObjectHandler.GetListOfUsernamesFromQueryContentForLikes(jsonPage));
            while (_jObjectHandler.HasNextPageForLikes(jsonPage))
            {
                var nextCursor = _jObjectHandler.GetEndOfCursorFromJsonForLikes(jsonPage);

                _logger.Info(
                    $"Thread: {Thread.CurrentThread.Name} getting json users from post likes: {postShortCode}");

                jsonPage = Task.Run(() => _queryRequester.GetJsonForLikes(postShortCode, _rhxGis, nextCursor))
                    .GetAwaiter().GetResult();

                resultList.AddRange(_jObjectHandler.GetListOfUsernamesFromQueryContentForLikes(jsonPage));
            }

            FillUnprocessedSet(resultList);
        }

        private void FillUnprocessedSet(IEnumerable<string> list)
        {
            lock (UnprocessedSetLocker)
            {
                foreach (var user in list) _usersSet.AddUnprocessedUser(user);
            }
        }

        private void FillShortCodesQueue(IEnumerable<string> list)
        {
            lock (QueueLocker)
            {
                foreach (var shortCode in list) _usersSet.AddInShortCodesQueue(shortCode);
            }
        }
    }
}