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
        private WebProcessor _webProcessor;
        private QueryRequester _queryRequester;
        private readonly string _userAgent;
        private string _rhxGis;


        public WebParser(string userAgent)
        {
            _webProcessor = new WebProcessor();
            _queryRequester = new QueryRequester(userAgent);
            _userAgent = userAgent;
        }

        public async Task<List<string>> GetPostsShortCodesFromUser(string username, int countOfLoading = 4)
        {
            var userUrl = "/" + username + "/";
            var userPageContent = await PageDownloader.GetPageContent(userUrl, _userAgent);
            var userId = long.Parse(_webProcessor.GetUserIdFromPageContent(userPageContent));
            _rhxGis = _rhxGis ?? _webProcessor.GetRhxGisParameter(userPageContent);
            var resultList = _webProcessor.GetListOfShortCodesFromPageContent(userPageContent);
            if (!_webProcessor.HasNextPageForPageContent(userPageContent)) return resultList;
            var jsonPage = await _queryRequester.GetJsonPageContent(userPageContent, userId, _rhxGis);
            resultList.AddRange(_webProcessor.GetListOfShortCodesFromQueryContent(jsonPage));
            var count = 0;
            while (_webProcessor.HasNextPageForPosts(jsonPage) && count < countOfLoading)
            {
                count++;
                var nextCursor = _webProcessor.GetEndOfCursorFromJsonForPosts(jsonPage);
                Thread.Sleep(400);
                jsonPage = await _queryRequester.GetJson(userId, _rhxGis, nextCursor);
                resultList.AddRange(_webProcessor.GetListOfShortCodesFromQueryContent(jsonPage));
            }

            return resultList;
        }

        public async Task<List<string>> GetUsernamesFromPostComments(string postShortCode)
        {
            var postUrl = "/p/" + postShortCode + "/";
            var postPageContent = await PageDownloader.GetPageContent(postUrl, _userAgent);
            _rhxGis = _rhxGis ?? _webProcessor.GetRhxGisParameter(postPageContent);
            var resultList = _webProcessor.GetListOfUsernamesFromPageContent(postPageContent);
            if (!_webProcessor.HasNextPageForPageContent(postPageContent)) return resultList;
            var jsonPage = await _queryRequester.GetJsonPageContent(postPageContent, postShortCode, _rhxGis);
            resultList.AddRange(_webProcessor.GetListOfUsernamesFromQueryContentForPost(jsonPage));
            while (_webProcessor.HasNextPageForComments(jsonPage))
            {
                var nextCursor = _webProcessor.GetEndOfCursorFromJsonForComments(jsonPage);
                Thread.Sleep(400);
                jsonPage = await _queryRequester.GetJson(postShortCode, _rhxGis, nextCursor);
                resultList.AddRange(_webProcessor.GetListOfUsernamesFromQueryContentForPost(jsonPage));
            }

            return resultList;
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
                Thread.Sleep(400);
                jsonPage = Task.Run(() => _queryRequester.GetJsonForLikes(postShortCode, _rhxGis, nextCursor))
                    .GetAwaiter().GetResult();
                resultList.AddRange(_webProcessor.GetListOfUsernamesFromQueryContentForLikes(jsonPage));
            }

            foreach (var VARIABLE in resultList)
            {
                Console.WriteLine(Thread.CurrentThread.Name + " " + VARIABLE);
            }

//            return resultList;
        }
    }
}