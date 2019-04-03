using System;
using System.Threading;
using InfluencerInstaParser.AudienceParser.WebParsing;

namespace InfluencerInstaParser.AudienceParser
{
    public class ParsingHandler
    {
        private readonly SingletonParsingSet _parsingSet;

        private readonly string _targetAccount;
//        private int _countOfThreads;

        public ParsingHandler(string username)
        {
            _parsingSet = SingletonParsingSet.GetInstance();
            _targetAccount = username;
        }

        public void Parse()
        {
            var agents = new UserAgentCreator();
            var web = new WebParser(agents.GetUserAgent());
            web.GetPostsShortCodesFromUser(_targetAccount);
            Console.WriteLine("Short Codes downloaded");
            ThreadPool.SetMaxThreads(3, 3);
            using (var countdownEvent = new CountdownEvent(_parsingSet.ShortCodesQueue.Count * 2))
            {
                foreach (var shortcode in _parsingSet.ShortCodesQueue)
                {
                    ThreadPool.QueueUserWorkItem(obj =>
                    {
                        new WebParser(agents.GetUserAgent()).GetUsernamesFromPostLikes(shortcode);
                        countdownEvent.Signal();
                    });

                    ThreadPool.QueueUserWorkItem(obj =>
                    {
                        new WebParser(agents.GetUserAgent()).GetUsernamesFromPostComments(shortcode);
                        countdownEvent.Signal();
                    });
                }

                Console.WriteLine("All threads started");

                countdownEvent.Wait();
            }
        }
    }
}