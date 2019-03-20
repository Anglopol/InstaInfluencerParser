using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace InfluencerInstaParser.AudienceParser
{
    public class SingletonParsingSet
    {
        private static SingletonParsingSet _instance;
        private ConcurrentDictionary<string, string> _handledUsers;
        private ConcurrentQueue<string> _unprocessedUsers;


        private SingletonParsingSet()
        {
            _handledUsers = new ConcurrentDictionary<string, string>();
            _unprocessedUsers = new ConcurrentQueue<string>();
        }

        public static SingletonParsingSet GetInstance()
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
            _unprocessedUsers.TryDequeue(out var result);
            return result;
        }

        public bool IsProcessed(string username)
        {
            return _handledUsers.ContainsKey(username);
        }

        public bool IsInQueue(string username)
        {
            return _unprocessedUsers.Contains(username);
        }

        public bool IsInParsingSet(string username)
        {
            return IsProcessed(username) || IsInQueue(username);
        }

        public void AddInHandledSet(string username, string fromUsername)
        {
            if (!IsProcessed(username))
            {
                _handledUsers.TryAdd(username, fromUsername);
            }
        }

        public ConcurrentDictionary<string, string> GetHandledSet()
        {
            return _handledUsers;
        }

        public ConcurrentQueue<string> GetQueue()
        {
            return _unprocessedUsers;
        }
    }
}