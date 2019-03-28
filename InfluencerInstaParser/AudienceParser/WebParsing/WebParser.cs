using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class WebParser
    {
        private readonly WebProcessor _webProcessor;
        private QueryRequester _queryRequester;
        private string _userAgent;
        private string _rhxGis;
        private SingletonParsingSet _usersSet;
        private PageDownloader _downloader;


        public WebParser(string userAgent)
        {
            _webProcessor = new WebProcessor();
            _queryRequester = new QueryRequester(userAgent);
            _userAgent = userAgent;
            _usersSet = SingletonParsingSet.GetInstance();
            _downloader = PageDownloader.GetInstance();
        }

        public void ChangeUserAgent(string userAgent)
        {
            _userAgent = userAgent;
            _queryRequester = new QueryRequester(userAgent);
        }

        public void GetPostsShortCodesFromUser(string username, int countOfLoading = 0)
        {
            var userUrl = "/" + username + "/";
            var userPageContent = Task.Run(() => _downloader.GetPageContentWithProxy(userUrl, _userAgent)).GetAwaiter()
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
                Console.WriteLine(shortCode);
                _usersSet.AddInQueue(shortCode);
                System.IO.File.WriteAllText("shortcodes.txt", shortCode);
            }
        }

        public void GetUsernamesFromPostComments(string postShortCode)
        {
            var postUrl = "/p/" + postShortCode + "/";
            var postPageContent = Task.Run(() => _downloader.GetPageContentWithProxy(postUrl, _userAgent)).GetAwaiter()
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
                Console.WriteLine(user);
            }
        }

        public void GetUsernamesFromPostLikes(string postShortCode)
        {
            var postUrl = "/p/" + postShortCode + "/";
            var postPageContent = Task.Run(() => _downloader.GetPageContentWithProxy(postUrl, _userAgent)).GetAwaiter()
                .GetResult();
            _rhxGis = _rhxGis ?? _webProcessor.GetRhxGisParameter(postPageContent);
            var resultList = new List<string>();
            var jsonPage = Task.Run(() => _queryRequester.GetJsonForLikes(postShortCode, _rhxGis, ""))
                .GetAwaiter().GetResult();
            resultList.AddRange(_webProcessor.GetListOfUsernamesFromQueryContentForLikes(jsonPage));
            var count = 0;
            while (_webProcessor.HasNextPageForLikes(jsonPage))
            {
                count++;
                if (count > 190)
                {
                    ChangeUserAgent(
                        "Mozilla/5.0 (X11; CrOS i686 4319.74.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.57 Safari/537.36"); //TODO refactor
                    postPageContent = Task.Run(() => _downloader.GetPageContentWithProxy(postUrl, _userAgent))
                        .GetAwaiter()
                        .GetResult();
                    _rhxGis = _webProcessor.GetRhxGisParameter(postPageContent);
                    count = 0;
                }

                var nextCursor = _webProcessor.GetEndOfCursorFromJsonForLikes(jsonPage);
                Thread.Sleep(600);
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