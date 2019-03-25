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

        public WebParser()
        {
            _webProcessor = new WebProcessor();
            _queryRequester = new QueryRequester();
        }

        public async Task<List<string>> GetPostsShortCodesFromUser(InstaUserInfo user, int countOfLoading = 4)
        {
            var userUrl = "/" + user.Username + "/";
//            var userUrl = "/varlamov/";
            var userPageContent = await PageDownloader.GetPageContent(userUrl);
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
            var postPageContent = await PageDownloader.GetPageContent(postUrl);
            var rhxGis = _webProcessor.GetRhxGisParameter(postPageContent);
            var resultList = _webProcessor.GetListOfUsernamesFromPageContent(postPageContent);
            if (!_webProcessor.HasNextPageForPageContent(postPageContent)) return resultList;
            var jsonPage = await _queryRequester.GetJsonPageContent(postPageContent, postShortCode, rhxGis);
            resultList.AddRange(_webProcessor.GetListOfUsernamesFromQueryContent(jsonPage));
            while (_webProcessor.HasNextPageForComments(jsonPage))
            {
                var nextCursor = _webProcessor.GetEndOfCursorFromJsonForComments(jsonPage);
                Thread.Sleep(400);
                jsonPage = await _queryRequester.GetJson(postShortCode, rhxGis, nextCursor);
                resultList.AddRange(_webProcessor.GetListOfUsernamesFromQueryContent(jsonPage));
            }

            return resultList;
        }
    }
}