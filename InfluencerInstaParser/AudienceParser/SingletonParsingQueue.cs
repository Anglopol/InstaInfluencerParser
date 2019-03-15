using System.Collections;
using System.Collections.Generic;

namespace InfluencerInstaParser.AudienceParser
{
    public class SingletonParsingQueue
    {
        private static SingletonParsingQueue _instance;
        private Queue<string> _users;

        private SingletonParsingQueue()
        {
            _users = new Queue<string>();

        }

        public SingletonParsingQueue GetInstance()
        {
            return _instance ?? (_instance = new SingletonParsingQueue());
        }
    }
}