using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace InfluencerInstaParser.AudienceParser
{
    public class SingletonParsingSet
    {
        private static SingletonParsingSet _instance;
        private ConcurrentDictionary<string, byte> _handledUsers;
        private ConcurrentQueue<string> _unprocessedUsers;

        private readonly object _initLocker = new object();
        private bool _isInit = false;

        private SingletonParsingSet()
        {
            _handledUsers = new ConcurrentDictionary<string, byte>();
            _unprocessedUsers = new ConcurrentQueue<string>();
        }

        public SingletonParsingSet GetInstance()
        {
            if (!_isInit)
            {
                lock (_initLocker)
                {
                    if (!_isInit)
                    {
                        _isInit = true;
                        return _instance = new SingletonParsingSet();
                    }
                }
            }

            return _instance;
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

        public void AddInHandledSet(string username)
        {
            if (!IsProcessed(username))
            {
                _handledUsers.TryAdd(username, 0);
            }
        }

        public ConcurrentDictionary<string, byte> GetHandledSet()
        {
            return _handledUsers;
        }

        public ConcurrentQueue<string> GetQueue()
        {
            return _unprocessedUsers;
        }
    }
}