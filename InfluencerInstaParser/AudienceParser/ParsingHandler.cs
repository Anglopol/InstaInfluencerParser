using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.AuthorizedParsing;
using InfluencerInstaParser.AudienceParser.AuthorizedParsing.SessionData;
using InfluencerInstaParser.AudienceParser.WebParsing;

namespace InfluencerInstaParser.AudienceParser
{
    public class ParsingHandler
    {
        private readonly ParsingSetSingleton _parsingSet;
        private readonly string _targetAccount;

        public ParsingHandler(string username)
        {
            _parsingSet = ParsingSetSingleton.GetInstance();
            _targetAccount = username;
        }

        public void Parse()
        {
            var agents = new UserAgentCreator();
            var web = new WebParser(agents.GetUserAgent());
            web.GetPostsShortCodesFromUser(_targetAccount);
            Console.WriteLine("Short Codes downloaded");
            var sessionData = new ConfigSessionDataFactory().MakeSessionData();
            var api = Task.Run(() => AuthApiCreator.MakeAuthApi(sessionData)).GetAwaiter().GetResult();
            var tasks = new List<Task>();
            var followers = Task.Run(() => new AudienceDownloader().GetFollowers(_targetAccount, api));
            tasks.Add(followers);
            while (_parsingSet.ShortCodesQueue.Count != 0)
            {
                var shortCode = _parsingSet.ShortCodesQueue.Dequeue();
                var likes = new Task(() => new WebParser(agents.GetUserAgent()).GetUsernamesFromPostLikes(shortCode));
                var comments = new Task(() =>
                    new WebParser(agents.GetUserAgent()).GetUsernamesFromPostComments(shortCode));
                tasks.Add(likes);
                tasks.Add(comments);
                likes.Start();
                comments.Start();
            }


            Console.WriteLine("All threads started");

            foreach (var follower in followers.Result) _parsingSet.UnprocessedUsers.Add(follower);

            Task.WaitAll(tasks.ToArray());
        }
    }
}