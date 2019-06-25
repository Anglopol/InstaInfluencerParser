using System;
using InfluencerInstaParser.AudienceParser.WebParsing.PageDownload;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class QueryRequester
    {
        private readonly IPageDownloader _downloaderProxy;
        private readonly JObjectScraper _jObjectScraper;
        private readonly PageContentScraper _proc;

        public QueryRequester(IServiceProvider serviceProvider)
        {
            _proc = new PageContentScraper();
            _jObjectScraper = new JObjectScraper();
            _downloaderProxy = serviceProvider.GetService<IPageDownloader>();
        }

        public JObject GetJsonPageContent(string userPageContent, long userId)
        {
            var endOfCursor = _proc.GetEndOfCursorFromPageContent(userPageContent);
//            var instagramGis = _proc.MakeInstagramGis(rhxGis, userId, 50, endOfCursor);
            var queryUrl = _proc.GetQueryUrlForPosts(userId, 50, endOfCursor);
            return _jObjectScraper.GetObjectFromJsonString(_downloaderProxy.GetPageContent(queryUrl));
        }

        public JObject GetJsonPageContent(string userPageContent, string shortCode)
        {
            var endOfCursor = _proc.GetEndOfCursorFromPageContent(userPageContent);
//            var instagramGis = _proc.MakeInstagramGis(rhxGis, shortCode, 50, endOfCursor);
            var queryUrl = _proc.GetQueryUrlForComments(shortCode, 50, endOfCursor);
            return _jObjectScraper.GetObjectFromJsonString(_downloaderProxy.GetPageContent(queryUrl));
        }

        public JObject GetJson(long userId, string endOfCursor)
        {
//            var instagramGis = _proc.MakeInstagramGis(rhxGis, userId, 50, endOfCursor);
            var queryUrl = _proc.GetQueryUrlForPosts(userId, 50, endOfCursor);
            return _jObjectScraper.GetObjectFromJsonString(_downloaderProxy.GetPageContent(queryUrl));
        }

        public JObject GetJson(string shortCode, string endOfCursor)
        {
//            var instagramGis = _proc.MakeInstagramGis(rhxGis, shortCode, 50, endOfCursor);
            var queryUrl = _proc.GetQueryUrlForComments(shortCode, 50, endOfCursor);
            return _jObjectScraper.GetObjectFromJsonString(_downloaderProxy.GetPageContent(queryUrl));
        }

        public JObject GetJsonForLikes(string shortCode, string endOfCursor)
        {
//            var instagramGis = _proc.MakeInstagramGisForLikes(rhxGis, shortCode, 50, endOfCursor);
            var queryUrl = _proc.GetQueryUrlForLikes(shortCode, 50, endOfCursor);
            return _jObjectScraper.GetObjectFromJsonString(_downloaderProxy.GetPageContent(queryUrl));
        }
    }
}