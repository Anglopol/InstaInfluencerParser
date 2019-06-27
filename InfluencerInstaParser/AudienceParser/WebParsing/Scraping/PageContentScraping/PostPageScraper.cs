using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace InfluencerInstaParser.AudienceParser.WebParsing.Scraping.PageContentScraping
{
    public class PostPageScraper : IInstagramPostPageScraper
    {
        public bool IsContentHasNextPage(string pageContent)
        {
            return pageContent.Contains("\"has_next_page\":true");
        }

        public bool IsPostVideo(string postPageContent)
        {
            return postPageContent.Contains("\"is_video\":true");
        }

        public IEnumerable<string> GetUsernamesFromPostPage(string postPageContent)
        {
            return Regex.Matches(postPageContent, "username\".{2}[^\"]*").Select(match => match.Value.ToString()
                .Split(":")[1].Remove(0, 1)).ToList();
        }
    }
}