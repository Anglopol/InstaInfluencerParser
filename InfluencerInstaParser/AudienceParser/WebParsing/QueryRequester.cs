using System;
using InfluencerInstaParser.AudienceParser.WebParsing.PageDownload;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping;
using InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class QueryRequester
    {
        private readonly IPageDownloader _downloaderProxy;
        private readonly JObjectScraper _jObjectScraper;
        private readonly IInstagramUserPageScraper _pageContentScraper;

        public QueryRequester(IServiceProvider serviceProvider)
        {
            _pageContentScraper = serviceProvider.GetService<IInstagramUserPageScraper>();
            _jObjectScraper = new JObjectScraper();
            _downloaderProxy = serviceProvider.GetService<IPageDownloader>();
        }

        public JObject GetJsonForUserPage(string userPageContent, ulong userId)
        {
            var endOfCursor = _pageContentScraper.GetEndOfCursorFromUserPage(userPageContent);
//            var instagramGis = _proc.MakeInstagramGis(rhxGis, userId, 50, endOfCursor);
            var queryUrl = RequestParamsCreator.GetQueryUrlForPosts(userId, 50, endOfCursor);
            return _jObjectScraper.GetObjectFromJsonString(_downloaderProxy.GetPageContent(queryUrl));
        }

        public JObject GetJsonPageContent(string userPageContent, string shortCode)
        {
            var endOfCursor = _pageContentScraper.GetEndOfCursorFromUserPage(userPageContent);
//            var instagramGis = _proc.MakeInstagramGis(rhxGis, shortCode, 50, endOfCursor);
            var queryUrl = RequestParamsCreator.GetQueryUrlForComments(shortCode, 50, endOfCursor);
            return _jObjectScraper.GetObjectFromJsonString(_downloaderProxy.GetPageContent(queryUrl));
        }

        public JObject GetJson(ulong userId, string endOfCursor)
        {
//            var instagramGis = _proc.MakeInstagramGis(rhxGis, userId, 50, endOfCursor);
            var queryUrl = RequestParamsCreator.GetQueryUrlForPosts(userId, 50, endOfCursor);
            return _jObjectScraper.GetObjectFromJsonString(_downloaderProxy.GetPageContent(queryUrl));
        }

        public JObject GetJson(string shortCode, string endOfCursor)
        {
//            var instagramGis = _proc.MakeInstagramGis(rhxGis, shortCode, 50, endOfCursor);
            var queryUrl = RequestParamsCreator.GetQueryUrlForComments(shortCode, 50, endOfCursor);
            return _jObjectScraper.GetObjectFromJsonString(_downloaderProxy.GetPageContent(queryUrl));
        }

        public JObject GetJsonForLikes(string shortCode, string endOfCursor)
        {
//            var instagramGis = _proc.MakeInstagramGisForLikes(rhxGis, shortCode, 50, endOfCursor);
            var queryUrl = RequestParamsCreator.GetQueryUrlForLikes(shortCode, 50, endOfCursor);
            return _jObjectScraper.GetObjectFromJsonString(_downloaderProxy.GetPageContent(queryUrl));
        }
    }
}