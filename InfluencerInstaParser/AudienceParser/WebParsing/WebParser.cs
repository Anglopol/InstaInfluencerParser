using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class WebParser
    {
        private readonly WebProcessor _webProcessor;
        private readonly QueryRequester _queryRequester;
        private readonly string _userAgent;
        private string _rhxGis;
        private SingletonParsingSet _usersSet;


        public WebParser(string userAgent)
        {
            _webProcessor = new WebProcessor();
            _queryRequester = new QueryRequester(userAgent);
            _userAgent = userAgent;
            _usersSet = SingletonParsingSet.GetInstance();
        }

        public void GetPostsShortCodesFromUser(string username, int countOfLoading = 0)
        {
            var userUrl = "/" + username + "/";
            var userPageContent = Task.Run(() => PageDownloader.GetPageContent(userUrl, _userAgent)).GetAwaiter()
                .GetResult();
            var userId = long.Parse(_webProcessor.GetUserIdFromPageContent(userPageContent));
            _rhxGis = _rhxGis ?? _webProcessor.GetRhxGisParameter(userPageContent);
            var resultList = _webProcessor.GetListOfShortCodesFromPageContent(userPageContent);
            if (!_webProcessor.HasNextPageForPageContent(userPageContent))
            {
                foreach (var shortCode in resultList)
                {
                    _usersSet.AddInQueue(shortCode);
                }
                return;
            }
            var jsonPage = Task.Run(() => _queryRequester.GetJsonPageContent(userPageContent, userId, _rhxGis))
                .GetAwaiter().GetResult();
            resultList.AddRange(_webProcessor.GetListOfShortCodesFromQueryContent(jsonPage));
            var count = 0;
            while (_webProcessor.HasNextPageForPosts(jsonPage) && count < countOfLoading)
            {
                count++;
                var nextCursor = _webProcessor.GetEndOfCursorFromJsonForPosts(jsonPage);
                Thread.Sleep(1000);
                jsonPage = Task.Run(() => _queryRequester.GetJson(userId, _rhxGis, nextCursor)).GetAwaiter()
                    .GetResult();
                resultList.AddRange(_webProcessor.GetListOfShortCodesFromQueryContent(jsonPage));
            }

            foreach (var shortCode in resultList)
            {
                _usersSet.AddInQueue(shortCode);
            }
        }

        public void GetUsernamesFromPostComments(string postShortCode)
        {
            var postUrl = "/p/" + postShortCode + "/";
            var postPageContent = Task.Run(() => PageDownloader.GetPageContent(postUrl, _userAgent)).GetAwaiter()
                .GetResult();
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
                foreach (var user in resultList)
                {
                    _usersSet.AddInHandledSet(user);
                }

                return;
            }

            var jsonPage = Task.Run(() => _queryRequester.GetJsonPageContent(postPageContent, postShortCode, _rhxGis))
                .GetAwaiter().GetResult();
            resultList.AddRange(_webProcessor.GetListOfUsernamesFromQueryContentForPost(jsonPage));
            while (_webProcessor.HasNextPageForComments(jsonPage))
            {
                var nextCursor = _webProcessor.GetEndOfCursorFromJsonForComments(jsonPage);
                Thread.Sleep(600);
                jsonPage = Task.Run(() => _queryRequester.GetJson(postShortCode, _rhxGis, nextCursor)).GetAwaiter()
                    .GetResult();
                resultList.AddRange(_webProcessor.GetListOfUsernamesFromQueryContentForPost(jsonPage));
            }

            foreach (var user in resultList)
            {
                _usersSet.AddInHandledSet(user);
            }
        }

        public void GetUsernamesFromPostLikes(string postShortCode)
        {
            var postUrl = "/p/" + postShortCode + "/";
            var postPageContent = Task.Run(() => PageDownloader.GetPageContent(postUrl, _userAgent)).GetAwaiter()
                .GetResult();
            _rhxGis = _rhxGis ?? _webProcessor.GetRhxGisParameter(postPageContent);
            var resultList = new List<string>();
            var jsonPage = Task.Run(() => _queryRequester.GetJsonForLikes(postShortCode, _rhxGis, ""))
                .GetAwaiter().GetResult();
            resultList.AddRange(_webProcessor.GetListOfUsernamesFromQueryContentForLikes(jsonPage));
            while (_webProcessor.HasNextPageForLikes(jsonPage))
            {
                var nextCursor = _webProcessor.GetEndOfCursorFromJsonForLikes(jsonPage);
                Thread.Sleep(2000);
                jsonPage = Task.Run(() => _queryRequester.GetJsonForLikes(postShortCode, _rhxGis, nextCursor))
                    .GetAwaiter().GetResult();
                resultList.AddRange(_webProcessor.GetListOfUsernamesFromQueryContentForLikes(jsonPage));
            }

            foreach (var user in resultList)
            {
                _usersSet.AddInHandledSet(user);
            }
        }
    }
}