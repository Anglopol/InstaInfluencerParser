using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser
{
    public class Post
    {
        public string Owner { get; set; }
        public string ShortCode { get; set; }
        public ulong LocationId { get; set; }
        public string NextCommentsCursor { get; set; }
        public IEnumerable<string> usernamesFromCommentsPreview { get; set; }
        public bool isVideo { get; set; }
    }
}