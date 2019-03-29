using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser
{
    public class SingletonParsingSet
    {
        private static SingletonParsingSet _instance;
        
        private Queue<string> ShortCodesQueue { get; }
        private HashSet<string> UnprocessedUsers { get; }
        private HashSet<string> ProcessedUsers { get; }

        private SingletonParsingSet()
        {
            ShortCodesQueue = new Queue<string>();
            UnprocessedUsers = new HashSet<string>();
            ProcessedUsers = new HashSet<string>();
        }

        public static SingletonParsingSet GetInstance()
        {
            return _instance ?? (_instance = new SingletonParsingSet());
        }

        public void AddUnprocessedUser(string username)
        {
            if (ProcessedUsers.Contains(username)) return;
            UnprocessedUsers.Add(username);
        }

        public void AddProcessedUser(string username)
        {
            ProcessedUsers.Add(username);
        }

        public void AddInShortCodesQueue(string shortCode)
        {
            ShortCodesQueue.Enqueue(shortCode);
        }
    }
}