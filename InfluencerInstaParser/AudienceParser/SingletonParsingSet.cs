using System.Collections.Concurrent;
using System.Linq;

namespace InfluencerInstaParser.AudienceParser
{
    public class SingletonParsingSet
    {
        private static SingletonParsingSet _instance;
        public ConcurrentDictionary<string, byte> HandledUsers { get; }
        public ConcurrentQueue<string> UnprocessedUsers { get; }

        private SingletonParsingSet()
        {
            HandledUsers = new ConcurrentDictionary<string, byte>();
            UnprocessedUsers = new ConcurrentQueue<string>();
        }

        public static SingletonParsingSet GetInstance()
        {
            return _instance ?? (_instance = new SingletonParsingSet());
        }

        public void AddInQueue(string username)
        {
            if (!IsInQueue(username))
            {
                UnprocessedUsers.Enqueue(username);
            }
        }

        public string Dequeue(string username)
        {
            UnprocessedUsers.TryDequeue(out var result);
            return result;
        }

        public bool IsProcessed(string username)
        {
            return HandledUsers.ContainsKey(username);
        }

        public bool IsInQueue(string username) => UnprocessedUsers.Contains(username);


        public bool IsInParsingSet(string username)
        {
            return IsProcessed(username) || IsInQueue(username);
        }

        public void AddInHandledSet(string username)
        {
            if (!IsProcessed(username))
            {
                HandledUsers.TryAdd(username, 1);
            }
        }
    }
}