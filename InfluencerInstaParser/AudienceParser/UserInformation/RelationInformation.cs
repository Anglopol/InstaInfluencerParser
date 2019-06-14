using System;
using System.Globalization;
using InfluencerInstaParser.Database.Model;

namespace InfluencerInstaParser.AudienceParser.UserInformation
{
    public class RelationInformation
    {
        private readonly object _setCommentsLocker;
        private readonly object _setLikesLocker;
        private int _likes;
        private int _comments;

        public ModelRelation Relation { get; }

        public int Likes
        {
            get => _likes;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                lock (_setLikesLocker)
                {
                    _likes = value;
                    Relation.Likes = _likes;
                }
            }
        }

        public int Comments
        {
            get => _comments;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                lock (_setCommentsLocker)
                {
                    _comments = value;
                    Relation.Comments = _comments;
                }
            }
        }

        public RelationInformation(string parentName, DateTime timeOfParsing, string childName, bool isFollower = false,
            int likes = 0, int comments = 0)
        {
            Relation = new ModelRelation
            {
                Child = childName, Parent = parentName, Likes = likes, Comments = comments, Follower = isFollower,
                DateOfParsing = timeOfParsing.ToString(CultureInfo.InvariantCulture)
            };
            _likes = likes;
            _comments = comments;
            _setLikesLocker = new object();
            _setCommentsLocker = new object();
        }
    }
}