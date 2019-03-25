using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class QueryRequester
    {
        public async Task<JObject> GetJson(string userPageContent, long userId, string rhxGis)
        {
            var proc = new WebProcessor();
            var endOfCursor = proc.GetEndOfCursorOnFirstPage(userPageContent);
            var instagramGis = proc.MakeInstagramGisForPosts(rhxGis, userId, 12, endOfCursor);
            var queryUrl = proc.GetQueryUrlForPosts(userId, 12, endOfCursor);
            try
            {
                return proc.GetObjectFromJsonString(await PageDownloader.GetPageContent(queryUrl, instagramGis));
            }
            catch (Exception e)
            {
                Thread.Sleep(60000);
                return await GetJson(userPageContent, userId, rhxGis);
            }
        }

        public async Task<JObject> GetJson(long userId, string rhxGis, string endOfCursor)
        {
            var proc = new WebProcessor();
            var instagramGis = proc.MakeInstagramGisForPosts(rhxGis, userId, 12, endOfCursor);
            var queryUrl = proc.GetQueryUrlForPosts(userId, 12, endOfCursor);
            try
            {
                return proc.GetObjectFromJsonString(await PageDownloader.GetPageContent(queryUrl, instagramGis));
            }
            catch (Exception e)
            {
                Thread.Sleep(60000);
                return await GetJson(userId, rhxGis, endOfCursor);
            }
        }
    }
}