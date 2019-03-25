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

        public async Task<List<string>> GetPostsShortCodesFromUser(InstaUserInfo user, int countOfLoading = 10)
        {
            var userUrl = "/" + user.Username + "/";
//            var userUrl = "/varlamov/";
            var userPageContent = await PageDownloader.GetPageContent(userUrl);
            var rhxGis = _webProcessor.GetRhxGisParameter(userPageContent);
            var resultList = _webProcessor.GetListOfShortCodesFromPageContent(userPageContent);
            if (!_webProcessor.HasNextPage(userPageContent)) return resultList;
            var jsonPage = await _queryRequester.GetJson(userPageContent, user.Pk, rhxGis);
            resultList.AddRange(_webProcessor.GetListOfShortCodesFromQueryContent(jsonPage));
            var count = 0;
            while (_webProcessor.HasNextPage(jsonPage) && count < countOfLoading)
            {
                count++;
                var nextCursor = _webProcessor.GetEndOfCursorFromJson(jsonPage);
                Thread.Sleep(400);
                jsonPage = await _queryRequester.GetJson(user.Pk, rhxGis, nextCursor);
                resultList.AddRange(_webProcessor.GetListOfShortCodesFromQueryContent(jsonPage));
            }

            return resultList;
        }
    }
}