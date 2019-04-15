using System.Collections.Generic;
using InfluencerInstaParser.Database.UserInformation;

namespace InfluencerInstaParser.AudienceParser
{
    public class ParsingSetSingleton
    {
        private static ParsingSetSingleton _instance;

        private ParsingSetSingleton()
        {
            ShortCodesQueue = new Queue<string>();
            UnprocessedUsers = new Dictionary<string, User>();
            ProcessedUsers = new Dictionary<string, User>();
        }

        public Queue<string> ShortCodesQueue { get; }
        public Dictionary<string, User> UnprocessedUsers { get; }
        public Dictionary<string, User> ProcessedUsers { get; }

        public static ParsingSetSingleton GetInstance()
        {
            return _instance ?? (_instance = new ParsingSetSingleton());
        }

        public void AddUnprocessedUser(string username, User parent,
            CommunicationType type = CommunicationType.Follower)
        {
            if (ProcessedUsers.ContainsKey(username) || UnprocessedUsers.ContainsKey(username))
            {
                var currentUser = UnprocessedUsers.ContainsKey(username)
                    ? UnprocessedUsers[username]
                    : ProcessedUsers[username];
                switch (type)
                {
                    case CommunicationType.Liker:
                    {
                        currentUser.AddLikesForRelation(parent);
                        break;
                    }
                    case CommunicationType.Commentator:
                    {
                        currentUser.AddCommentsForRelation(parent);
                        break;
                    }
                    case CommunicationType.Follower:
                    {
                        currentUser.AddNewRelation(parent);
                        break;
                    }
                }
            }

            if (!UnprocessedUsers.ContainsKey(username))
                UnprocessedUsers.Add(username, new User(username, parent, type));
        }

        public void AddProcessedUser(User user)
        {
            ProcessedUsers.Add(user.Username, user);
        }

        public void AddInShortCodesQueue(string shortCode)
        {
            ShortCodesQueue.Enqueue(shortCode);
        }
    }
}