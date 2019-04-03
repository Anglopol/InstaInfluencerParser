using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InfluencerInstaParser.AudienceParser.WebParsing
{
    public class UserAgentCreator
    {
        private const string DefaultUserAgentsFilePath = "useragents.txt";
        private readonly string _pathToUserAgentsFile;
        private readonly Queue<string> _userAgentsQueue;

        public UserAgentCreator(string filePath)
        {
            _pathToUserAgentsFile = filePath;
            _userAgentsQueue = new Queue<string>();
        }

        public UserAgentCreator()
        {
            _pathToUserAgentsFile = DefaultUserAgentsFilePath;
            _userAgentsQueue = new Queue<string>();
        }

        public string GetUserAgent()
        {
            if (_userAgentsQueue.Count == 0) FillQueue();
            return _userAgentsQueue.Dequeue();
        }

        private void FillQueue()
        {
            using (var reader = new StreamReader(_pathToUserAgentsFile, Encoding.Default))
            {
                string line;
                while ((line = reader.ReadLine()) != null) _userAgentsQueue.Enqueue(line);
            }
        }
    }
}