using System;
using System.Collections.Generic;
using System.Threading;
using InfluencerInstaParser.Database.UserInformation;
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
        private readonly User _owner;
        private readonly QueryRequester _queryRequester;
        private readonly string _userAgent;
        private readonly ParsingSetSingleton _usersSet;
        private readonly WebProcessor _webProcessor;
        private string _rhxGis;

        public WebParser(string userAgent, User owner)
        {
            _owner = owner;
            _logger = LogManager.GetCurrentClassLogger();
            _webProcessor = new WebProcessor();
            _userAgent = userAgent;
            _usersSet = ParsingSetSingleton.GetInstance();
            _downloaderProxy = new PageDownloaderProxy();
            _jObjectHandler = new JObjectHandler();
            _queryRequester = new QueryRequester(userAgent, _downloaderProxy);
        }

        public void GetPostsShortCodesFromUser(string username, int countOfLoading = 0)
        {
            var userUrl = "/" + username + "/";

            var userPageContent = _downloaderProxy.GetPageContentWithProxy(userUrl, _userAgent);

            if (_webProcessor.IsPrivate(userPageContent) || _webProcessor.IsEmpty(userPageContent))
            {
                Console.WriteLine($"{username} is invalid");
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

            FillShortCodesQueue(resultList);
        }

        public void GetUsernamesFromPostComments(string postShortCode)
        {
            var postUrl = "/p/" + postShortCode + "/";
            Console.WriteLine(postShortCode + "Comments");
            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting users from post: {postShortCode}");

            var postPageContent = _downloaderProxy.GetPageContentWithProxy(postUrl, _userAgent);

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
                FillUnprocessedSet(resultList, CommunicationType.Commentator);
                _owner.Comments += resultList.Count;
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
        }

        public void GetUsernamesFromPostLikes(string postShortCode)
        {
            var postUrl = "/p/" + postShortCode + "/";
            _logger.Info($"Thread: {Thread.CurrentThread.Name} getting users from post likes: {postShortCode}");
            Console.WriteLine(postShortCode + " Likes");

            var postPageContent = _downloaderProxy.GetPageContentWithProxy(postUrl, _userAgent);

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
        }

        private void FillUnprocessedSet(IEnumerable<string> list, CommunicationType type)
        {
            lock (UnprocessedSetLocker)
            {
                foreach (var userName in list) _usersSet.AddUnprocessedUser(userName, _owner, type);
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