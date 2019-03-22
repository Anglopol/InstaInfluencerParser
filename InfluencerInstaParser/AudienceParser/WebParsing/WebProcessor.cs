using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json.Linq;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class WebProcessor
    {
        public string GetRhxGisParameter(string pageContent)
        {
            return Regex.Matches(pageContent, "rhx_gis.{3}[^\"]*")[0].ToString()
                .Split(":")[1].Remove(0, 1);
        }

        public string GetEndOfCursorOnFirstPage(string pageContent)
        {
            return Regex.Matches(pageContent,
                    "\"has_next_page\":true,\"end_cursor.{3}[^\"]*")[0].ToString()
                .Split(":")[2].Remove(0, 1);
        }

        public string MakeInstagramGisForPosts(string rhxGis, int userId, int count, string endOfCursor)
        {
            var signatureParams = $"{rhxGis}:{MakeSignatureString(userId, count, endOfCursor)}";
            Console.WriteLine(signatureParams);
            return CalculateMD5Hash(signatureParams);
        }

        public string CalculateMD5Hash(string input)
        {
            var md5 = MD5.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            foreach (var character in hash)
            {
                sb.Append(character.ToString("x2"));
            }

            return sb.ToString();
        }

        public string MakeSignatureString(int userId, int count, string endOfCursor)
        {
            return $"{{\"id\":\"{userId}\",\"first\":{count},\"after\":\"{endOfCursor}\"}}";
        }

        public string GetQueryUrlForPosts(int userId, int count, string endOfCursor)
        {
            const string defaultQueryUrl =
                @"/graphql/query/?query_hash=f2405b236d85e8296cf30347c9f08c2a&variables=";
            var signatureUrlString = HttpUtility.UrlEncode(MakeSignatureString(userId, count, endOfCursor));
            return defaultQueryUrl + signatureUrlString;
        }

        public JObject GetObjectFromJsonString(string jsonString)
        {
            return JObject.Parse(jsonString);
        }
    }
}