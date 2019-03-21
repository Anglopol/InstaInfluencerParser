using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class WebProcessor
    {
        public string GetRhxGisParameter(string username)
        {
            return Regex.Matches(HtmlPageDownloader.GetJsonStringFromUserPage(username), "rhx_gis.{35}")[0].ToString()
                .Split(":")[1].Remove(0, 1);
        }

        public string GetEndOfCursorOnFirstPage(string username)
        {
            return Regex.Matches(HtmlPageDownloader.GetJsonStringFromUserPage(username),
                    "\"has_next_page\":true,\"end_cursor\":\".{120}")[0].ToString()
                .Split(":")[2].Remove(0, 1);
        }

        public string MakeInstagramGisForPosts(string rhxGis, int userId, int count, string endOfCursor)
        {
            var signatureParams = $"{rhxGis}:{MakeSignatureString(userId, count, endOfCursor)}";
            using (var md5 = MD5.Create())
            {
                var gis = md5.ComputeHash(Encoding.UTF8.GetBytes(signatureParams));
                return Convert.ToBase64String(gis);
            }
        }

        public string MakeSignatureString(int userId, int count, string endOfCursor)
        {
            return $"{{\"id\":\"{userId}\",\"first\":{count},\"after\":\"{endOfCursor}\"}}";
        }

        public string GetQueryUrlForPosts(int userId, int count, string endOfCursor)
        {
            const string defaultQueryUrl = 
                @"https://www.instagram.com/graphql/query/?query_hash=f2405b236d85e8296cf30347c9f08c2a&variables=";
            var signatureUrlString = HttpUtility.UrlEncode(MakeSignatureString(userId, count, endOfCursor));
            return defaultQueryUrl + signatureUrlString;
        }
    }
}