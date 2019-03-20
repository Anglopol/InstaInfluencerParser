using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InstagramApiSharp.API;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class WebParser
    {
        public string GetRhxGisParameter(string username)
        {
            return Regex.Matches(HtmlPageDownloader.GetJsonStringFromUserPage(username), "rhx_gis.{35}")[0].ToString()
                .Split(":")[1].Remove(0, 1);
        }
    }
}