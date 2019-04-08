using System.Collections.Generic;
using InfluencerInstaParser.Database;

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

        public void AddUnprocessedUser(string username, User from, CommunicationType type = CommunicationType.Follower)
        {
            if (ProcessedUsers.ContainsKey(username)) return;
            UnprocessedUsers.Add(username, new User(username, communicationType: type, from: from));
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