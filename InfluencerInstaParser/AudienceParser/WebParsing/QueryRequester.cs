using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class QueryRequester
    {
        public async Task<JObject> GetJsonPageContent(string userPageContent, long userId, string rhxGis)
        {
            var proc = new WebProcessor();
            var endOfCursor = proc.GetEndOfCursorFromPageContent(userPageContent);
            var instagramGis = proc.MakeInstagramGisForPosts(rhxGis, userId, 12, endOfCursor);
            var queryUrl = proc.GetQueryUrlForPosts(userId, 12, endOfCursor);
            try
            {
                return proc.GetObjectFromJsonString(await PageDownloader.GetPageContent(queryUrl, instagramGis));
            }
            catch (Exception e)
            {
                Thread.Sleep(60000);
                return await GetJsonPageContent(userPageContent, userId, rhxGis);
            }
        }
        
        public async Task<JObject> GetJsonPageContent(string userPageContent, string shortCode, string rhxGis)
        {
            var proc = new WebProcessor();
            var endOfCursor = proc.GetEndOfCursorFromPageContent(userPageContent);
            var instagramGis = proc.MakeInstagramGisForComments(rhxGis, shortCode, 12, endOfCursor);
            var queryUrl = proc.GetQueryUrlForComments(shortCode, 12, endOfCursor);
            try
            {
                return proc.GetObjectFromJsonString(await PageDownloader.GetPageContent(queryUrl, instagramGis));
            }
            catch (Exception e)
            {
                Thread.Sleep(60000);
                return await GetJsonPageContent(userPageContent, shortCode, rhxGis);
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
        
        public async Task<JObject> GetJson(string shortCode, string rhxGis, string endOfCursor)
        {
            var proc = new WebProcessor();
            var instagramGis = proc.MakeInstagramGisForComments(rhxGis, shortCode, 12, endOfCursor);
            var queryUrl = proc.GetQueryUrlForComments(shortCode, 12, endOfCursor);
            try
            {
                return proc.GetObjectFromJsonString(await PageDownloader.GetPageContent(queryUrl, instagramGis));
            }
            catch (Exception e)
            {
                Thread.Sleep(60000);
                return await GetJson(shortCode, rhxGis, endOfCursor);
            }
        }
    }
}