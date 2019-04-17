using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.AuthorizedParsing;
using InfluencerInstaParser.AudienceParser.AuthorizedParsing.SessionData;
using InfluencerInstaParser.AudienceParser.UserInformation;
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
            var owner = new User(_targetAccount);
            _parsingSet.AddProcessedUser(owner);
            var agents = new UserAgentCreator();
            var web = new WebParser(agents.GetUserAgent(), owner);
            web.GetPostsShortCodesFromUser(_targetAccount);
            Console.WriteLine("Short Codes downloaded");
            var sessionData = new ConfigSessionDataFactory().MakeSessionData();
            var api = Task.Run(() => AuthApiCreator.MakeAuthApi(sessionData)).GetAwaiter().GetResult();
            var tasks = new List<Task>();
            var followers = Task.Run(() => new AudienceDownloader().GetFollowers(_targetAccount, api));
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
            followers.Wait();
            _parsingSet.ProcessedUsers[_targetAccount].Followers = followers.Result.Count();
            Task.WaitAll(tasks.ToArray());
            foreach (var follower in followers.Result)
                _parsingSet.AddUnprocessedUser(follower, owner);
        }
    }
}