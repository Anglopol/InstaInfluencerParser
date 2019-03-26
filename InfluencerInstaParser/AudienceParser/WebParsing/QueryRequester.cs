using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class QueryRequester
    {
        private string _userAgent;
        private WebProcessor _proc;

        public QueryRequester(string userAgent)
        {
            _userAgent = userAgent;
            _proc = new WebProcessor();
        }

        public async Task<JObject> GetJsonPageContent(string userPageContent, long userId, string rhxGis)
        {
            var endOfCursor = _proc.GetEndOfCursorFromPageContent(userPageContent);
            var instagramGis = _proc.MakeInstagramGis(rhxGis, userId, 50, endOfCursor);
            var queryUrl = _proc.GetQueryUrlForPosts(userId, 50, endOfCursor);
            try
            {
                return _proc.GetObjectFromJsonString(
                    await PageDownloader.GetPageContent(queryUrl, _userAgent, instagramGis));
            }
            catch (Exception e)
            {
                Thread.Sleep(60000);
                return await GetJsonPageContent(userPageContent, userId, rhxGis);
            }
        }

        public async Task<JObject> GetJsonPageContent(string userPageContent, string shortCode, string rhxGis)
        {
            var endOfCursor = _proc.GetEndOfCursorFromPageContent(userPageContent);
            var instagramGis = _proc.MakeInstagramGis(rhxGis, shortCode, 50, endOfCursor);
            var queryUrl = _proc.GetQueryUrlForComments(shortCode, 50, endOfCursor);
            try
            {
                return _proc.GetObjectFromJsonString(
                    await PageDownloader.GetPageContent(queryUrl, _userAgent, instagramGis));
            }
            catch (Exception e)
            {
                Thread.Sleep(60000);
                return await GetJsonPageContent(userPageContent, shortCode, rhxGis);
            }
        }

        public async Task<JObject> GetJson(long userId, string rhxGis, string endOfCursor)
        {
            var instagramGis = _proc.MakeInstagramGis(rhxGis, userId, 50, endOfCursor);
            var queryUrl = _proc.GetQueryUrlForPosts(userId, 50, endOfCursor);
            try
            {
                return _proc.GetObjectFromJsonString(
                    await PageDownloader.GetPageContent(queryUrl, _userAgent, instagramGis));
            }
            catch (Exception e)
            {
                Thread.Sleep(60000);
                return await GetJson(userId, rhxGis, endOfCursor);
            }
        }

        public async Task<JObject> GetJson(string shortCode, string rhxGis, string endOfCursor)
        {
            var instagramGis = _proc.MakeInstagramGis(rhxGis, shortCode, 50, endOfCursor);
            var queryUrl = _proc.GetQueryUrlForComments(shortCode, 50, endOfCursor);
            try
            {
                return _proc.GetObjectFromJsonString(
                    await PageDownloader.GetPageContent(queryUrl, _userAgent, instagramGis));
            }
            catch (Exception e)
            {
                Thread.Sleep(60000);
                return await GetJson(shortCode, rhxGis, endOfCursor);
            }
        }

        public async Task<JObject> GetJsonForLikes(string shortCode, string rhxGis, string endOfCursor)
        {
            var instagramGis = _proc.MakeInstagramGis(rhxGis, shortCode, 50, endOfCursor);
            var queryUrl = _proc.GetQueryUrlForLikes(shortCode, 50, endOfCursor);
            try
            {
                return _proc.GetObjectFromJsonString(
                    await PageDownloader.GetPageContent(queryUrl, _userAgent, instagramGis));
            }
            catch (Exception e)
            {
                Thread.Sleep(60000);
                return await GetJsonForLikes(shortCode, rhxGis, endOfCursor);
            }
        }
    }
}