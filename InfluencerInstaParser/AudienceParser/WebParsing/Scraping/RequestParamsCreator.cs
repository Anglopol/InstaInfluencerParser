using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping
{
    public static class RequestParamsCreator
    {
        public static string GetQueryUrlForLikes(string shortCode, int count, string endOfCursor = "")
        {
            const string defaultQueryUrl =
                @"https://www.instagram.com/graphql/query/?query_hash=e0f59e4a1c8d78d0161873bc2ee7ec44&variables=";
            var signatureUrlString = HttpUtility.UrlEncode(MakeSignatureStringForLikes(shortCode, count, endOfCursor));
            return defaultQueryUrl + signatureUrlString;
        }

        public static string GetQueryUrlForPosts(ulong userId, int count, string endOfCursor)
        {
            const string defaultQueryUrl =
                @"https://www.instagram.com/graphql/query/?query_hash=f2405b236d85e8296cf30347c9f08c2a&variables=";
            var signatureUrlString = HttpUtility.UrlEncode(MakeSignatureString(userId, count, endOfCursor));
            return defaultQueryUrl + signatureUrlString;
        }

        public static string GetQueryUrlForComments(string shortCode, int count, string endOfCursor)
        {
            const string defaultQueryUrl =
                @"https://www.instagram.com/graphql/query/?query_hash=f0986789a5c5d17c2400faebf16efd0d&variables=";
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
            return $"{{\"id\":\"{userId}\",\"first\":{count},\"after\":\"{endOfCursor}\"}}";
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