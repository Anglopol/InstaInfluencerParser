using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.UserInformation;
using InfluencerInstaParser.AudienceParser.WebParsing;
using InfluencerInstaParser.AudienceParser.WebParsing.Locate;
using Microsoft.Extensions.DependencyInjection;

namespace InfluencerInstaParser.AudienceParser
{
    public class ParsingHandler
    {
        private readonly ParsingSetSingleton _parsingSet;
        private readonly UserAgentCreator _agentCreator;
        private readonly string _targetAccount;
        private readonly DateTime _timeOfParsing;
        private IServiceProvider _serviceProvider;

        public ParsingHandler(IServiceProvider serviceProvider, string username, DateTime timeOfParsing)
        {
            _serviceProvider = serviceProvider;
            _parsingSet = ParsingSetSingleton.GetInstance();
            _targetAccount = username;
            _timeOfParsing = timeOfParsing;
            _agentCreator = new UserAgentCreator();
        }

        public void Parse()
        {
            var owner = new User(_targetAccount, _timeOfParsing);
            _parsingSet.AddProcessedUser(owner);
            var web = new WebParser(_serviceProvider, owner, _timeOfParsing);
            web.TryGetPostsShortCodesAndLocationsIdFromUser(_targetAccount);
            var shortCodes = web.ShortCodes;
            var shortCodesTask = Task.Run(() => ShortCodesProcessing(owner, shortCodes));
            shortCodesTask.Wait();
            var users = _parsingSet.UnprocessedUsers.Values.ToList();
            var secondLevelTasks = new List<Task>();
            var locationTasks = new List<Task>();
            foreach (var user in users)
            {
                if (!user.IsInfluencer || user.Username == _targetAccount) continue;
                _parsingSet.AddProcessedUser(user);

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
                var locationTask = Task.Run(() =>
                    new WebParser(_serviceProvider, user, _timeOfParsing).DetermineUserLocations(
                        user.ModelViewUser.Parents,
                        100000));
                locationTasks.Add(locationTask);
            }

            Task.WaitAll(locationTasks.ToArray());
        }

        private void SecondLevelParsing(User user)
        {
            var countOfLoading = 1;
            var locator = _serviceProvider.GetService<ILocator>();
            var web = new WebParser(_serviceProvider, user, _timeOfParsing);
            if (!web.TryGetPostsShortCodesAndLocationsIdFromUser(user.Username, countOfLoading)) return;
            var shortCodes = web.ShortCodes;
            var locationsId = web.LocationsId;
            var shortCodesTask = Task.Run(() => ShortCodesProcessing(user, shortCodes, 1));
            var count = 0; // TODO: Delete this 
            foreach (var locationId in locationsId)
            {
                if (count > 5) break; // TODO: Delete this 
                count++; // TODO: Delete this 
                if (locator.TryGetLocationByLocationId(locationId, 100000, out var city, out var publicId))
                    user.AddLocation(city, publicId, _targetAccount);
            }
            shortCodesTask.Wait();
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