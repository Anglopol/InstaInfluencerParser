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
            if (!web.TryGetPostsShortCodesAndLocationsIdFromUser(_targetAccount, out var shortCodes, out _)) return;
            web.FillShortCodesQueue(shortCodes);

            Console.WriteLine("Short Codes downloaded");
            var sessionData = new ConfigSessionDataFactory().MakeSessionData();
            var api = Task.Run(() => AuthApiCreator.MakeAuthApi(sessionData)).GetAwaiter().GetResult();
            var tasks = new List<Task>();
            var followers = Task.Run(() => new AudienceDownloader().GetFollowers(_targetAccount, api));
            while (_parsingSet.ShortCodesQueue.Count != 0)
            {
                _parsingSet.ShortCodesQueue.TryDequeue(out var shortCode);
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
            var locationTasks = new List<Task>();
            web.FillUnprocessedSet(followers.Result, CommunicationType.Follower);
            var users = _parsingSet.UnprocessedUsers.Values.ToList();
            foreach (var user in users)
            {
                _parsingSet.AddProcessedUser(user);
                if (user.IsInfluencer)
                {
                    SecondLevelParsing(user);
                    continue;
                }

                Console.WriteLine($"Getting location for {user.Username}");
                var locationTask = new Task(() =>
                    new WebParser(agents.GetUserAgent(), user).DetermineUserLocations(_targetAccount, 100000));
                locationTasks.Add(locationTask);
                locationTask.Start();
            }

            Task.WaitAll(locationTasks.ToArray());
        }

        private void SecondLevelParsing(User user)
        {
        }
    }
}