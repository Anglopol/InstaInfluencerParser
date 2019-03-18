using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace InfluencerInstaParser.AudienceParser
{
    public class SingletonParsingSet
    {
        private static SingletonParsingSet _instance;
        private HashSet<string> _handledUsers;
        private Queue<string> _unprocessedUsers;
        private SingletonParsingSet()
        {
            _handledUsers = new HashSet<string>();
            _unprocessedUsers = new Queue<string>();

        }

        public SingletonParsingSet GetInstance()
        {
            return _instance ?? (_instance = new SingletonParsingSet());
        }

        public void AddInQueue(string username)
        {
            if (!IsInQueue(username))
            {
                _unprocessedUsers.Enqueue(username);
            }
        }

        public string Dequeue(string username)
        {
            return _unprocessedUsers.Dequeue();
        }

        public bool IsProcessed(string username)
        {
            return _handledUsers.Contains(username);
        }

        public bool IsInQueue(string username)
        {
            return _unprocessedUsers.Contains(username);
        }

        public bool IsInParsingSet(string username)
        {
            return IsProcessed(username) || IsInQueue(username);
        }

        public void AddInHandledSet(string username)
        {
            if (!IsProcessed(username))
            {
                _handledUsers.Add(username);
            }
        }

        public HashSet<string> GetHandledSet()
        {
            return _handledUsers;
        }

        public Queue<string> GetQueue()
        {
            return _unprocessedUsers;
        }
    }
}