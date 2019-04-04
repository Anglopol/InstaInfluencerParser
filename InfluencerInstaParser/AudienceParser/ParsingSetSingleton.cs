using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser
{
    public class ParsingSetSingleton
    {
        private static ParsingSetSingleton _instance;

        private ParsingSetSingleton()
        {
            ShortCodesQueue = new Queue<string>();
            UnprocessedUsers = new HashSet<string>();
            ProcessedUsers = new HashSet<string>();
        }

        public Queue<string> ShortCodesQueue { get; }
        public HashSet<string> UnprocessedUsers { get; }
        public HashSet<string> ProcessedUsers { get; }

        public static ParsingSetSingleton GetInstance()
        {
            return _instance ?? (_instance = new ParsingSetSingleton());
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