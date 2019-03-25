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
            return proc.GetObjectFromJsonString(await PageDownloader.GetPageContent(queryUrl, instagramGis));
        }

        public async Task<JObject> GetJson(long userId, string rhxGis, string endOfCursor)
        {
            var proc = new WebProcessor();
            var instagramGis = proc.MakeInstagramGisForPosts(rhxGis, userId, 12, endOfCursor);
            var queryUrl = proc.GetQueryUrlForPosts(userId, 12, endOfCursor);
            return proc.GetObjectFromJsonString(await PageDownloader.GetPageContent(queryUrl, instagramGis));
        }
    }
}