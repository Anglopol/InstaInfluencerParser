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

        public WebParser(string userAgent)
        {
            _webProcessor = new WebProcessor();
            _queryRequester = new QueryRequester(userAgent);
            _userAgent = userAgent;
        }

        public async Task<List<string>> GetPostsShortCodesFromUser(InstaUserInfo user, int countOfLoading = 4)
        {
            var userUrl = "/" + user.Username + "/";
//            var userUrl = "/varlamov/";
            var userPageContent = await PageDownloader.GetPageContent(userUrl, _userAgent);
            var rhxGis = _webProcessor.GetRhxGisParameter(userPageContent);
            var resultList = _webProcessor.GetListOfShortCodesFromPageContent(userPageContent);
            if (!_webProcessor.HasNextPageForPageContent(userPageContent)) return resultList;
            var jsonPage = await _queryRequester.GetJsonPageContent(userPageContent, user.Pk, rhxGis);
            resultList.AddRange(_webProcessor.GetListOfShortCodesFromQueryContent(jsonPage));
            var count = 0;
            while (_webProcessor.HasNextPageForPosts(jsonPage) && count < countOfLoading)
            {
                count++;
                var nextCursor = _webProcessor.GetEndOfCursorFromJsonForPosts(jsonPage);
                Thread.Sleep(400);
                jsonPage = await _queryRequester.GetJson(user.Pk, rhxGis, nextCursor);
                resultList.AddRange(_webProcessor.GetListOfShortCodesFromQueryContent(jsonPage));
            }

            return resultList;
        }

        public async Task<List<string>> GetUsernamesFromPostComments(string postShortCode)
        {
            var postUrl = "/p/" + postShortCode + "/";
            var postPageContent = await PageDownloader.GetPageContent(postUrl, _userAgent);
            var rhxGis = _webProcessor.GetRhxGisParameter(postPageContent);
            var resultList = _webProcessor.GetListOfUsernamesFromPageContent(postPageContent);
            if (!_webProcessor.HasNextPageForPageContent(postPageContent)) return resultList;
            var jsonPage = await _queryRequester.GetJsonPageContent(postPageContent, postShortCode, rhxGis);
            resultList.AddRange(_webProcessor.GetListOfUsernamesFromQueryContentForPost(jsonPage));
            while (_webProcessor.HasNextPageForComments(jsonPage))
            {
                var nextCursor = _webProcessor.GetEndOfCursorFromJsonForComments(jsonPage);
                Thread.Sleep(400);
                jsonPage = await _queryRequester.GetJson(postShortCode, rhxGis, nextCursor);
                resultList.AddRange(_webProcessor.GetListOfUsernamesFromQueryContentForPost(jsonPage));
            }

            return resultList;
        }

        public async Task<List<string>> GetUsernamesFromPostLikes(string postShortCode)
        {
            var postUrl = "/p/" + postShortCode + "/";
            var postPageContent = await PageDownloader.GetPageContent(postUrl, _userAgent);
            var rhxGis = _webProcessor.GetRhxGisParameter(postPageContent);
            var resultList = new List<string>();
            var jsonPage = await _queryRequester.GetJsonForLikes(postShortCode, rhxGis, "");
            resultList.AddRange(_webProcessor.GetListOfUsernamesFromQueryContentForLikes(jsonPage));
            while (_webProcessor.HasNextPageForLikes(jsonPage))
            {
                var nextCursor = _webProcessor.GetEndOfCursorFromJsonForLikes(jsonPage);
                Thread.Sleep(400);
                jsonPage = await _queryRequester.GetJsonForLikes(postShortCode, rhxGis, nextCursor);
                resultList.AddRange(_webProcessor.GetListOfUsernamesFromQueryContentForLikes(jsonPage));
            }

            return resultList;
        }
    }
}