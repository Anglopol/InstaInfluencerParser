using System;
using System.Threading;
using Newtonsoft.Json.Linq;
using NLog;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class QueryRequester
    {
        private readonly PageDownloaderProxy _downloaderProxy;
        private readonly JObjectHandler _jObjectHandler;

        private readonly Logger _logger;
        private readonly WebProcessor _proc;
        private readonly string _userAgent;

        public QueryRequester(string userAgent, PageDownloaderProxy downloaderProxy)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _userAgent = userAgent;
            _proc = new WebProcessor();
            _downloaderProxy = downloaderProxy;
            _jObjectHandler = new JObjectHandler();
        }

        public JObject GetJsonPageContent(string userPageContent, long userId, string rhxGis)
        {
            var endOfCursor = _proc.GetEndOfCursorFromPageContent(userPageContent);
            var instagramGis = _proc.MakeInstagramGis(rhxGis, userId, 50, endOfCursor);
            var queryUrl = _proc.GetQueryUrlForPosts(userId, 50, endOfCursor);
            try
            {
                return _jObjectHandler.GetObjectFromJsonString(
                    _downloaderProxy.GetPageContentWithProxy(queryUrl, _userAgent, instagramGis));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"GetJsonPageContent on user: {userId}");
                Console.WriteLine(e.StackTrace);
                Thread.Sleep(10000);
                return GetJsonPageContent(userPageContent, userId, rhxGis);
            }
        }

        public JObject GetJsonPageContent(string userPageContent, string shortCode, string rhxGis)
        {
            var endOfCursor = _proc.GetEndOfCursorFromPageContent(userPageContent);
            var instagramGis = _proc.MakeInstagramGis(rhxGis, shortCode, 50, endOfCursor);
            var queryUrl = _proc.GetQueryUrlForComments(shortCode, 50, endOfCursor);
            try
            {
                return _jObjectHandler.GetObjectFromJsonString(
                    _downloaderProxy.GetPageContentWithProxy(queryUrl, _userAgent, instagramGis));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"GetJsonPageContent on short code: {shortCode}");
                Console.WriteLine(e.StackTrace);
                Thread.Sleep(10000);
                return GetJsonPageContent(userPageContent, shortCode, rhxGis);
            }
        }

        public JObject GetJson(long userId, string rhxGis, string endOfCursor)
        {
            var instagramGis = _proc.MakeInstagramGis(rhxGis, userId, 50, endOfCursor);
            var queryUrl = _proc.GetQueryUrlForPosts(userId, 50, endOfCursor);
            try
            {
                return _jObjectHandler.GetObjectFromJsonString(
                    _downloaderProxy.GetPageContentWithProxy(queryUrl, _userAgent, instagramGis));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"GetJson on user: {userId}");
                Console.WriteLine(e.StackTrace);
                Thread.Sleep(10000);
                return GetJson(userId, rhxGis, endOfCursor);
            }
        }

        public JObject GetJson(string shortCode, string rhxGis, string endOfCursor)
        {
            var instagramGis = _proc.MakeInstagramGis(rhxGis, shortCode, 50, endOfCursor);
            var queryUrl = _proc.GetQueryUrlForComments(shortCode, 50, endOfCursor);
            try
            {
                return _jObjectHandler.GetObjectFromJsonString(
                    _downloaderProxy.GetPageContentWithProxy(queryUrl, _userAgent, instagramGis));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"GetJson on short code: {shortCode}");
                Console.WriteLine(e.StackTrace);
                Thread.Sleep(10000);
                return GetJson(shortCode, rhxGis, endOfCursor);
            }
        }

        public JObject GetJsonForLikes(string shortCode, string rhxGis, string endOfCursor)
        {
            var instagramGis = _proc.MakeInstagramGisForLikes(rhxGis, shortCode, 50, endOfCursor);
            var queryUrl = _proc.GetQueryUrlForLikes(shortCode, 50, endOfCursor);
            Console.WriteLine(queryUrl);
            try
            {
                return _jObjectHandler.GetObjectFromJsonString(
                    _downloaderProxy.GetPageContentWithProxy(queryUrl, _userAgent, instagramGis));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"GetJsonForLikes on short code: {shortCode}");
                Console.WriteLine(e.StackTrace);
                Thread.Sleep(10000);
                return GetJsonForLikes(shortCode, rhxGis, endOfCursor);
            }
        }
    }
}