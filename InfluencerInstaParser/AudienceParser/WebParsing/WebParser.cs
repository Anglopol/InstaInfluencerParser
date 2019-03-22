using System.Collections.Generic;
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

        public async Task<List<string>> GetAllPostsShortCodesFromUser(InstaUserInfo user)
        {
            var userUrl = "/" + user.Username + "/";
            var userPageContent = await PageDownloader.GetPageContent(userUrl);
            var rhxGis = _webProcessor.GetRhxGisParameter(userPageContent);
            var resultList = _webProcessor.GetListOfShortCodesFromPageContent(userPageContent);
            var jsonFromUserPage = await _queryRequester.GetJsonFromUserPage(userPageContent, user.Pk, rhxGis);
            
        }
    }
}