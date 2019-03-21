using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class WebParser
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

        public string MakeInstagramGis(string rhxGis, int userId, int count, string endOfCursor)
        {
            var signatureParams = $"{rhxGis}:{{\"id\":\"{userId}\",\"first\":{count},\"after\":\"{endOfCursor}\"}}";
            using (var md5 = MD5.Create())
            {
                var gis = md5.ComputeHash(Encoding.UTF8.GetBytes(signatureParams));
                return Convert.ToBase64String(gis);
            }
        }
    }
}