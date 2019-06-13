using System;
using System.Collections.Generic;
using System.Linq;
using InfluencerInstaParser.AudienceParser;
using InfluencerInstaParser.AudienceParser.Proxy;
using InfluencerInstaParser.AudienceParser.UserInformation;
using InfluencerInstaParser.Database;
using InfluencerInstaParser.Database.ModelView;
using Neo4jClient;

namespace InfluencerInstaParser.Manager
{
    public class InfluenceManager
    {
        private readonly GraphClient _graphClient;
        private readonly DateTime _currentDate;

        public InfluenceManager(Uri databaseConnectionUri, string databaseUsername, string databasePassword)
        {
            _graphClient = new GraphClient(databaseConnectionUri, databaseUsername, databasePassword);
            _currentDate = DateTime.UtcNow;
        }

        public void ProcessUser(string targetUsername, string pathToProxyFile)
        {
            _graphClient.Connect();
            ProxyFromFileCreatorSingleton.GetInstance().SetPathToProxyFile(pathToProxyFile);
            var parser = new ParsingHandler(targetUsername, _currentDate);
            parser.Parse();
            var locations = ParsingSetSingleton.GetInstance().GetListOfLocations();
            var users = ParsingSetSingleton.GetInstance().GetProcessedUsers();
            FillDatabase(targetUsername, users, locations);
        }

        public List<ModelUser> GetListOfInfluencers(string targetUsername, DateTime dateOfParsing)
        {
            return Neo4JClientHandler.GetListOfInfluencers(_graphClient, dateOfParsing, targetUsername);
        }

        public List<ModelUser> GetListOfInfluencers(string targetUsername, string analysisId)
        {
            var currentAnalysis = GetAnalysisById(analysisId);
            var dateOfParsing = DateTime.Parse(currentAnalysis.Date);
            return Neo4JClientHandler.GetListOfInfluencers(_graphClient, dateOfParsing, targetUsername);
        }

        public List<Location> GetListOfLocationsFromTarget(string targetUsername, DateTime dateOfParsing)
        {
            return Neo4JClientHandler.GetListOfLocationsFromTarget(_graphClient, dateOfParsing, targetUsername);
        }

        public List<Location> GetListOfLocationsFromTargetById(string targetUsername, string userId)
        {
            var user = GetUserById(userId);
            var dateOfParsing = DateTime.Parse(user.DateOfParsing);
            return Neo4JClientHandler.GetListOfLocationsFromTarget(_graphClient, dateOfParsing, targetUsername);
        }

        public List<Analysis> GetListOfAnalyses(string targetUsername)
        {
            return Neo4JClientHandler.GetListOfAnalyses(_graphClient, targetUsername);
        }

        public List<Analysis> GetListOfAnalysesById(string userId)
        {
            var user = Neo4JClientHandler.GetUserById(_graphClient, userId);
            return Neo4JClientHandler.GetListOfAnalyses(_graphClient, user.Username);
        }

        public Analysis GetLastParsingTime(string targetUsername)
        {
            return Neo4JClientHandler.GetLastAnalysis(_graphClient, targetUsername);
        }

        public Analysis GetAnalysisById(string analysisId)
        {
            return Neo4JClientHandler.GetAnalysisById(_graphClient, analysisId);
        }

        public List<ModelUser> GetInfluencersByAnalysisId(string analysisId)
        {
            return Neo4JClientHandler.GetListOfInfluencers(_graphClient, analysisId);
        }

        public ModelUser GetUserById(string userId)
        {
            return Neo4JClientHandler.GetUserById(_graphClient, userId);
        }

        private void FillDatabase(string targetUsername, IEnumerable<User> users, IEnumerable<Location> locations)
        {
            _graphClient.Connect();
            var usersList = users.ToList();
            var modelUsers = (from user in usersList select user.ModelViewUser).ToList();
            Neo4JClientHandler.CreateAnalysis(_graphClient, _currentDate, targetUsername, out var analysisId);
            Neo4JClientHandler.CreateUsers(_graphClient, modelUsers, targetUsername, analysisId);
            Neo4JClientHandler.CreateLocations(_graphClient, locations);
            var usersRelations =
                (from user in usersList from userDict in user.Relations select userDict.Value).ToList();
            var usersModelRelations = (from relation in usersRelations select relation.Relation).ToList();
            Neo4JClientHandler.CreateUsersRelations(_graphClient, usersModelRelations);
            var locationsRelationsDict =
                from user in usersList from locationDict in user.Locations select locationDict.Value;
            var locationRelation =
                (from relation in locationsRelationsDict from values in relation.Values select values).ToList();
            Neo4JClientHandler.CreateLocationsRelations(_graphClient, locationRelation);
        }
    }
}