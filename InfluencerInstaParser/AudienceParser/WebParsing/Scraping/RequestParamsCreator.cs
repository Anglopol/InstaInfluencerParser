using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping
{
    public static class RequestParamsCreator
    {
        public static string GetQueryUrlForLikes(string shortCode, string endOfCursor = "", int count = 50)
        {
            const string defaultQueryUrl =
                @"https://www.instagram.com/graphql/query/?query_hash=d5d763b1e2acf209d62d22d184488e57&variables=";
            var signatureUrlString = HttpUtility.UrlEncode(MakeSignatureStringForLikes(shortCode, count, endOfCursor));
            return defaultQueryUrl + signatureUrlString;
        }

        public static string GetQueryUrlForPosts(ulong userId, string endOfCursor = "", int count = 50)
        {
            const string defaultQueryUrl =
                @"https://www.instagram.com/graphql/query/?query_hash=f2405b236d85e8296cf30347c9f08c2a&variables=";
            var signatureUrlString = HttpUtility.UrlEncode(MakeSignatureString(userId, count, endOfCursor));
            return defaultQueryUrl + signatureUrlString;
        }

        public static string GetQueryUrlForComments(string shortCode, string endOfCursor, int count = 50)
        {
            const string defaultQueryUrl =
                @"https://www.instagram.com/graphql/query/?query_hash=97b41c52301f77ce508f55e66d17620e&variables=";
            var signatureUrlString = HttpUtility.UrlEncode(MakeSignatureString(shortCode, count, endOfCursor));
            return defaultQueryUrl + signatureUrlString;
        }
        public static string MakeInstagramGis(string rhxGis, ulong userId, int count, string endOfCursor)
        {
            var signatureParams = $"{rhxGis}:{MakeSignatureString(userId, count, endOfCursor)}";
            return GetCalculateMd5Hash(signatureParams);
        }

        public static string MakeInstagramGis(string rhxGis, string shortCode, int count, string endOfCursor)
        {
            var signatureParams = $"{rhxGis}:{MakeSignatureString(shortCode, count, endOfCursor)}";
            return GetCalculateMd5Hash(signatureParams);
        }

        public static string MakeInstagramGisForLikes(string rhxGis, string shortCode, int count, string endOfCursor)
        {
            var signatureParams = $"{rhxGis}:{MakeSignatureStringForLikes(shortCode, count, endOfCursor)}";
            return GetCalculateMd5Hash(signatureParams);
        }

        private static string GetCalculateMd5Hash(string input)
        {
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            foreach (var character in hash) sb.Append(character.ToString("x2"));

            return sb.ToString();
        }
        
        private static string MakeSignatureString(ulong userId, int count, string endOfCursor)
        {
            return endOfCursor == "" 
                ? $"{{\"id\":\"{userId}\",\"first\":{count}}}"
                : $"{{\"id\":\"{userId}\",\"first\":{count},\"after\":\"{endOfCursor}\"}}";
        }

        private static string MakeSignatureString(string shortCode, int count, string endOfCursor)
        {
            return $"{{\"shortcode\":\"{shortCode}\",\"first\":{count},\"after\":\"{endOfCursor}\"}}";
        }

        private static string MakeSignatureStringForLikes(string shortCode, int count, string endOfCursor)
        {
            return endOfCursor == ""
                ? $"{{\"shortcode\":\"{shortCode}\",\"include_reel\":true,\"first\":{count}}}"
                : $"{{\"shortcode\":\"{shortCode}\",\"include_reel\":true,\"first\":{count},\"after\":\"{endOfCursor}\"}}";
        }
    }
}