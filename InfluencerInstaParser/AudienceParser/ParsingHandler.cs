using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.UserInformation;
using InfluencerInstaParser.AudienceParser.WebParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.PageDownload;
using NLog;

namespace InfluencerInstaParser.AudienceParser
{
    public class ParsingHandler
    {
        private readonly ParsingSetSingleton _parsingSet;
        private readonly UserAgentCreator _agentCreator;
        private readonly string _targetAccount;
        private readonly Logger _logger;
        private DateTime _timeOfParsing;

        public ParsingHandler(string username, DateTime timeOfParsing)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _parsingSet = ParsingSetSingleton.GetInstance();
            _targetAccount = username;
            _timeOfParsing = timeOfParsing;
            _agentCreator = new UserAgentCreator();
        }

        public void Parse()
        {
            _logger.Warn("!!!!!!!");
            var owner = new User(_targetAccount, _timeOfParsing);
            _parsingSet.AddProcessedUser(owner);
            _logger.Info("target User added in processed set");
            var web = new WebParser(_agentCreator.GetUserAgent(), owner, _timeOfParsing);
            web.TryGetPostsShortCodesAndLocationsIdFromUser(_targetAccount, out var shortCodes, out _);
            _logger.Info("Short codes of target user downloaded");
//            var sessionData = new ConfigSessionDataFactory().MakeSessionData();
//            var api = Task.Run(() => AuthApiCreator.MakeAuthApi(sessionData)).GetAwaiter().GetResult();
//            var followers = Task.Run(() => new AudienceDownloader().GetFollowers(_targetAccount, api));
            var shortCodesTask = Task.Run(() => ShortCodesProcessing(owner, shortCodes));
//            followers.Wait();
            _logger.Info("Followers of target user added");
//            _parsingSet.ProcessedUsers[_targetAccount].Followers = followers.Result.Count();
            shortCodesTask.Wait();
//            web.FillUnprocessedSet(followers.Result, CommunicationType.Follower);
            var users = _parsingSet.UnprocessedUsers.Values.ToList();
            var secondLevelTasks = new List<Task>();
            var locationTasks = new List<Task>();
            foreach (var user in users)
            {
                _logger.Info($"Checking {user.Username} is influencer");
                if (!user.IsInfluencer || user.Username == _targetAccount) continue;
                _parsingSet.AddProcessedUser(user);
                _logger.Info($"Starting second level parsing for {user.Username}");
                var secondLevelTask = Task.Run(() => SecondLevelParsing(user));
                secondLevelTasks.Add(secondLevelTask);
            }

            Task.WaitAll(secondLevelTasks.ToArray());
            var locationDict = new Dictionary<string, int>();
            users = _parsingSet.UnprocessedUsers.Values.ToList();
            foreach (var user in users)
            {
                var needToGetLocation = true;
                if (!user.ModelViewUser.Parents.Contains(_targetAccount))
                {
                    foreach (var parent in user.ModelViewUser.Parents)
                    {
                        locationDict.TryAdd(parent, 0);
                        locationDict[parent]++;
                        if (locationDict[parent] >= 50) needToGetLocation = false;
                    }
                }

                _parsingSet.AddProcessedUser(user);
                if (!needToGetLocation) continue;
                Console.WriteLine($"Getting location for {user.Username}");
                var locationTask = Task.Run(() =>
                    new WebParser(_agentCreator.GetUserAgent(), user, _timeOfParsing).DetermineUserLocations(
                        user.ModelViewUser.Parents,
                        100000));
                locationTasks.Add(locationTask);
            }

            Task.WaitAll(locationTasks.ToArray());
        }

//        private void SecondLevelParsing(User user, IInstaApi authApi)
        private void SecondLevelParsing(User user)
        {
            var locator = new Locator(new PageDownloaderProxy(), new PageContentScrapper(),
                _agentCreator.GetUserAgent());
            var web = new WebParser(_agentCreator.GetUserAgent(), user, _timeOfParsing);
//            var followers = Task.Run(() => new AudienceDownloader().GetFollowers(user.Username, authApi));
            if (!web.TryGetPostsShortCodesAndLocationsIdFromUser(user.Username, out var shortCodes,
                out var locationsId, 1)) return;
            var shortCodesTask = Task.Run(() => ShortCodesProcessing(user, shortCodes, 1));
            var count = 0; // TODO: Delete this 
            foreach (var locationId in locationsId)
            {
                if (count > 5) break; // TODO: Delete this 
                count++; // TODO: Delete this 
                if (locator.TryGetLocationByLocationId(locationId, 100000, out var city, out var publicId))
                    user.AddLocation(city, publicId, _targetAccount);
            }

//            followers.Wait();
            shortCodesTask.Wait();
//            web.FillUnprocessedSet(followers.Result, CommunicationType.Follower);
        }

        private void LocationsProcessing(User user, ulong locationId, int maxDistance, Locator locator)
        {
            if (locator.TryGetLocationByLocationId(locationId, 100000, out var city, out var publicId))
                user.AddLocation(city, publicId, _targetAccount);
        }

        private void ShortCodesProcessing(User user, IEnumerable<string> shortCodes, int handlingCount = int.MaxValue)
        {
            var tasks = new List<Task>();
            var downloaderProxy = new PageDownloaderProxy();
            foreach (var shortCode in shortCodes)
            {
                if (handlingCount == 0) break;
                handlingCount--;
                var postUrl = "/p/" + shortCode + "/";

                var postContent = downloaderProxy.GetPageContent(postUrl, _agentCreator.GetUserAgent());
                var like = Task.Run(() =>
                    new WebParser(_agentCreator.GetUserAgent(), user, _timeOfParsing)
                        .GetUsernamesFromPostLikes(shortCode, postContent, 1));
                var comment = Task.Run(() =>
                    new WebParser(_agentCreator.GetUserAgent(), user, _timeOfParsing).GetUsernamesFromPostComments(
                        shortCode,
                        postContent, 1));
                tasks.Add(like);
                tasks.Add(comment);
            }

            downloaderProxy.SetProxyFree();
            Task.WaitAll(tasks.ToArray());
        }
    }
}