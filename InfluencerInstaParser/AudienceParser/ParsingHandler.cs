using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.AuthorizedParsing;
using InfluencerInstaParser.AudienceParser.AuthorizedParsing.SessionData;
using InfluencerInstaParser.AudienceParser.WebParsing;
using InfluencerInstaParser.Database;

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
            var owner = new User(_targetAccount);
            var agents = new UserAgentCreator();
            var web = new WebParser(agents.GetUserAgent(), owner);
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
                var like = new Task(() =>
                    new WebParser(agents.GetUserAgent(), owner).GetUsernamesFromPostLikes(shortCode));
                var comment = new Task(() =>
                    new WebParser(agents.GetUserAgent(), owner).GetUsernamesFromPostComments(shortCode));
                tasks.Add(like);
                tasks.Add(comment);
                like.Start();
                comment.Start();
            }


            Console.WriteLine("All threads started");

            foreach (var follower in followers.Result)
                _parsingSet.AddUnprocessedUser(follower, owner);

            Task.WaitAll(tasks.ToArray());
        }
    }
}