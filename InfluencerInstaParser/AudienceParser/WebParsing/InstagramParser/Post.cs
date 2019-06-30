using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser
{
    public class Post
    {
        public string Owner { get; set; }
        public ulong OwnerId { get; set; }
        public string ShortCode { get; set; }
        public ulong LocationId { get; set; }
        public string NextCommentsCursor { get; set; }
        public IEnumerable<ulong> usersFromCommentsPreview { get; set; }
        public bool isVideo { get; set; }
    }
}