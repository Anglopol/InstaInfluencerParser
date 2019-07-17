using System.Collections.Generic;
using JetBrains.Annotations;

namespace InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser
{
    public class  Post
    {
        public string Owner { get; }
        public ulong OwnerId { get; }
        public string ShortCode { get; }
        public ulong LocationId { get; set; }
        [CanBeNull] public string NextCommentsCursor { get; set; }
        public IEnumerable<ParsedUserFromJson> UsersFromCommentsPreview { get; }
        public bool IsVideo { get; }
        public bool HasLocation { get; set; }
        public bool HasNextCursor { get; set; }
        public int Likes { get; }
        public int Comments { get; }

        public Post(string owner, ulong ownerId, string shortCode, IEnumerable<ParsedUserFromJson> usersFromCommentsPreview,
            bool isVideo, int likes, int comments)
        {
            Owner = owner;
            OwnerId = ownerId;
            ShortCode = shortCode;
            UsersFromCommentsPreview = usersFromCommentsPreview;
            IsVideo = isVideo;
            HasLocation = false;
            HasNextCursor = false;
            Likes = likes;
            Comments = comments;
        }
    }
}