using System;
using Newtonsoft.Json;

namespace InfluencerInstaParser.Database.UserInformation
{
    public class RelationInformation
    {
        private readonly object _setCommentsLocker;
        private readonly object _setLikesLocker;
        private int _likes;
        private int _comments;

        [JsonProperty("communication")] public CommunicationType CommunicationType { get; }

        [JsonProperty("likes")]
        public int Likes
        {
            get => _likes;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                lock (_setLikesLocker)
                {
                    _likes = value;
                }
            }
        }

        [JsonProperty("comments")]
        public int Comments
        {
            get => _comments;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                lock (_setCommentsLocker)
                {
                    _comments = value;
                }
            }
        }

        public User Parent { get; }

        public RelationInformation(User parent, CommunicationType communicationType)
        {
            Parent = parent;
            CommunicationType = communicationType;
            _setLikesLocker = new object();
            _setCommentsLocker = new object();
        }
    }
}