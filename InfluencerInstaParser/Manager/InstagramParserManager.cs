using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser;
using InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser;
using InfluencerInstaParser.AudienceParser.WebParsing.InstagramResponseParser;
using Microsoft.Extensions.DependencyInjection;

namespace InfluencerInstaParser.Manager
{
    public class InstagramParserManager : IInstagramParserManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<ulong, IUser> _firstLevelUsers;
        private readonly ConcurrentDictionary<ulong, IUser> _secondLevelUsers;
        private IUser _targetUser;

        public InstagramParserManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _firstLevelUsers = new ConcurrentDictionary<ulong, IUser>();
            _secondLevelUsers = new ConcurrentDictionary<ulong, IUser>();
        }

        public void AnalyzeUser(string username)
        {
            var instaParser = _serviceProvider.GetService<IInstagramParser>();
            var parsingResult = instaParser.ParseByUsername(username);
            _targetUser = parsingResult.CreateUser();
            if (_targetUser.IsUserEmpty) return;
            FirstLevelAudienceProcessing(_targetUser);
            SecondLevelAudienceProcessing(); 
            InfluencersAudienceProcessing();
            //TODO: var model = IModelCreator.CreateMode(_targetAccount, _firstLevelUsers, _secondLevelUsers);
            //TODO: INeo4jClientHandler.FillDatabase(model);
        }

        private void FirstLevelAudienceProcessing(IUser user)
        {
            var usersToParse = user.GetUsersToParse();
            var firstLevelTasks = from userToParse in usersToParse
                where !userToParse.IsPrivate
                select Task.Factory.StartNew(() => FirstLevelUserAnalyze(userToParse),
                    TaskCreationOptions.LongRunning);
            Task.WaitAll(firstLevelTasks.ToArray());
        }

        private void SecondLevelAudienceProcessing()
        {
            var influencers = GetInfluencers();
            var secondLevelTasks = from influencer in influencers
                select Task.Factory.StartNew(() => SecondLevelUsersAnalyze(influencer),
                    TaskCreationOptions.LongRunning);
            Task.WaitAll(secondLevelTasks.ToArray());
        }

        private void InfluencersAudienceProcessing()
        {
            var influencers = GetInfluencers();
            var influencersTasks = from influencer in influencers
                select Task.Factory.StartNew(() => SingleInfluencerProcessing(influencer),
                    TaskCreationOptions.LongRunning);
            Task.WaitAll(influencersTasks.ToArray());
        }

        private void FirstLevelUserAnalyze(ParsedUserFromJson userFromJson)
        {
            var instaParser = _serviceProvider.GetService<IInstagramParser>();
            var postAndLocationsParseResult = instaParser.ParseOnlyPostsAndLocations(userFromJson);
            var user = postAndLocationsParseResult.CreateUser();
            _firstLevelUsers.TryAdd(user.InstagramId, user);
        }

        private void SecondLevelUsersAnalyze(IUser influencer)
        {
            var instaParser = _serviceProvider.GetService<IInstagramParser>();
            var parseResult = instaParser.SecondLevelParsingForInfluencers(influencer);
            var newInfluencer = parseResult.CreateUser();
            ReplaceInfluencer(influencer, newInfluencer);
        }

        private void SingleInfluencerProcessing(IUser influencer)
        {
            var usersToParse = influencer.GetUsersToParse();
            var usersFromInfluencerTasks = from userToParse in usersToParse
                where !userToParse.IsPrivate
                select Task.Factory.StartNew(() => SingleInfluencerSingleUserProcessing(userToParse),
                    TaskCreationOptions.LongRunning);
            Task.WaitAll(usersFromInfluencerTasks.ToArray());
        }
        
        private void SingleInfluencerSingleUserProcessing(ParsedUserFromJson userFromJson)
        {
            var userId = userFromJson.UserId;
            if (IsUserCached(userId))
            {
                _secondLevelUsers.TryAdd(userId, GetCachedUser(userId));
                return;
            }
            var instaParser = _serviceProvider.GetService<IInstagramParser>();
            var postAndLocationsParseResult = instaParser.ParseOnlyPostsAndLocations(userFromJson);
            var user = postAndLocationsParseResult.CreateUser();
            _secondLevelUsers.TryAdd(userId, user);
        }

        private IUser GetCachedUser(ulong userId)
        {
            if (_secondLevelUsers.TryGetValue(userId, out var user)) return user;
            _firstLevelUsers.TryGetValue(userId, out user);
            return user;
        }

        private bool IsUserCached(ulong userId)
        {
            return _firstLevelUsers.ContainsKey(userId) || _secondLevelUsers.ContainsKey(userId);
        }

        private IEnumerable<IUser> GetInfluencers()
        {
            return from firstLevelUser in _firstLevelUsers.Values
                where firstLevelUser.IsInfluencer
                select firstLevelUser;
        }

        private void ReplaceInfluencer(IUser influencer, IUser newInfluencer)
        {
            _firstLevelUsers[influencer.InstagramId] = newInfluencer;
        }
    }
}