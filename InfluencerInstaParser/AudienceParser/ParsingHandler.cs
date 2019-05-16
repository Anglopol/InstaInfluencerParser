using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.AuthorizedParsing;
using InfluencerInstaParser.AudienceParser.AuthorizedParsing.SessionData;
using InfluencerInstaParser.AudienceParser.UserInformation;
using InfluencerInstaParser.AudienceParser.WebParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.PageDownload;
using InstagramApiSharp.API;

namespace InfluencerInstaParser.AudienceParser
{
    public class ParsingHandler
    {
        private readonly ParsingSetSingleton _parsingSet;
        private readonly UserAgentCreator _agentCreator;
        private readonly string _targetAccount;

        public ParsingHandler(string username)
        {
            _parsingSet = ParsingSetSingleton.GetInstance();
            _targetAccount = username;
            _agentCreator = new UserAgentCreator();
        }

        public void Parse()
        {
            var owner = new User(_targetAccount);
            _parsingSet.AddProcessedUser(owner);
            var web = new WebParser(_agentCreator.GetUserAgent(), owner);
            if (!web.TryGetPostsShortCodesAndLocationsIdFromUser(_targetAccount, out var shortCodes, out _)) return;
            Console.WriteLine("Short Codes downloaded");
            var sessionData = new ConfigSessionDataFactory().MakeSessionData();
            var api = Task.Run(() => AuthApiCreator.MakeAuthApi(sessionData)).GetAwaiter().GetResult();
            var tasks = new List<Task>();
            var followers = Task.Run(() => new AudienceDownloader().GetFollowers(_targetAccount, api));
            var shortCodesTask = new Task(() => ShortCodesProcessing(owner, shortCodes));
            tasks.Add(shortCodesTask);
            shortCodesTask.Start();
            Console.WriteLine("All threads started");
            followers.Wait();
            _parsingSet.ProcessedUsers[_targetAccount].Followers = followers.Result.Count();
            Task.WaitAll(tasks.ToArray());
            var locationTasks = new List<Task>();
            web.FillUnprocessedSet(followers.Result, CommunicationType.Follower);
            var users = _parsingSet.UnprocessedUsers.Values.ToList();
            foreach (var user in users)
            {
                if (!user.IsInfluencer) continue;
                _parsingSet.AddProcessedUser(user);
                var secondLevelTask = new Task(() => SecondLevelParsing(user, api));
                tasks.Add(secondLevelTask);
                secondLevelTask.Start();
            }

            Task.WaitAll(tasks.ToArray());

            foreach (var user in users)
            {
                _parsingSet.AddProcessedUser(user);
                Console.WriteLine($"Getting location for {user.Username}");
                var locationTask = new Task(() =>
                    new WebParser(_agentCreator.GetUserAgent(), user).DetermineUserLocations(user.ModelViewUser.Parents,
                        100000));
                locationTasks.Add(locationTask);
                locationTask.Start();
            }

            Task.WaitAll(locationTasks.ToArray());
        }

        private void SecondLevelParsing(User user, IInstaApi authApi)
        {
            var locator = new Locator(new PageDownloaderProxy(), new PageContentScrapper(),
                _agentCreator.GetUserAgent());
            var web = new WebParser(_agentCreator.GetUserAgent(), user);
            var followers = Task.Run(() => new AudienceDownloader().GetFollowers(user.Username, authApi));
            if (!web.TryGetPostsShortCodesAndLocationsIdFromUser(_targetAccount, out var shortCodes,
                out var locationsId, 1)) return;
            var shortCodesTask = new Task(() => ShortCodesProcessing(user, shortCodes, 5));
            shortCodesTask.Start();
            foreach (var locationId in locationsId)
            {
                if (locator.TryGetLocationByLocationId(locationId, 100000, out var city, out var publicId))
                    user.AddLocation(city, publicId, _targetAccount);
            }

            followers.Wait();
            shortCodesTask.Wait();
            web.FillUnprocessedSet(followers.Result, CommunicationType.Follower);
        }

        private void ShortCodesProcessing(User user, List<string> shortCodes, int handlingCount = int.MaxValue)
        {
            var tasks = new List<Task>();
            foreach (var shortCode in shortCodes)
            {
                if (handlingCount == 0) break;
                handlingCount--;
                var like = new Task(() =>
                    new WebParser(_agentCreator.GetUserAgent(), user).GetUsernamesFromPostLikes(shortCode));
                var comment = new Task(() =>
                    new WebParser(_agentCreator.GetUserAgent(), user).GetUsernamesFromPostComments(shortCode));
                like.Start();
                comment.Start();
                tasks.Add(like);
                tasks.Add(comment);
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}