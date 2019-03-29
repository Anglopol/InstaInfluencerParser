using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser
{
    public class SingletonParsingSet
    {
        private static SingletonParsingSet _instance;
        
        public Queue<string> ShortCodesQueue { get; }
        public HashSet<string> UnprocessedUsers { get; }
        public HashSet<string> ProcessedUsers { get; }

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