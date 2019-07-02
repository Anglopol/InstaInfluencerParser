using System.Collections.Generic;
using JetBrains.Annotations;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramParser
{
    public class Post
    {
        public string Owner { get; }
        public ulong OwnerId { get; }
        public string ShortCode { get; }
        public ulong LocationId { get; set; }
        [CanBeNull] public string NextCommentsCursor { get; set; }
        public IEnumerable<ParsedUser> UsersFromCommentsPreview { get; }
        public bool IsVideo { get; }
        public bool HasLocation { get; set; }
        public bool HasNextCursor { get; set; }

        public Post(string owner, ulong ownerId, string shortCode, IEnumerable<ParsedUser> usersFromCommentsPreview,
            bool isVideo)
        {
            Owner = owner;
            OwnerId = ownerId;
            ShortCode = shortCode;
            UsersFromCommentsPreview = usersFromCommentsPreview;
            IsVideo = isVideo;
            HasLocation = false;
            HasNextCursor = false;
        }
    }
}