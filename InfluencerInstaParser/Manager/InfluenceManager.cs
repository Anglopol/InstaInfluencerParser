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
        private readonly string _targetUsername;
        private readonly DateTime _currentDate;

        public InfluenceManager(Uri databaseConnectionUri, string databaseUsername, string databasePassword,
            string targetUsername)
        {
            _targetUsername = targetUsername;
            _graphClient = new GraphClient(databaseConnectionUri, databaseUsername, databasePassword);
            _currentDate = DateTime.UtcNow;
        }

        public void ProcessUser(string pathToProxyFile)
        {
            _graphClient.Connect();
            ProxyFromFileCreatorSingleton.GetInstance().SetPathToProxyFile(pathToProxyFile);
            var parser = new ParsingHandler(_targetUsername, _currentDate);
            parser.Parse();
            var locations = ParsingSetSingleton.GetInstance().GetListOfLocations();
            var users = ParsingSetSingleton.GetInstance().GetProcessedUsers();
            FillDatabase(users, locations);
        }

        public List<ModelUser> GetListOfInfluencers(DateTime dateOfParsing)
        {
            return Neo4JClientHandler.GetListOfInfluencers(_graphClient, dateOfParsing, _targetUsername);
        }

        public List<Location> GetListOfLocationsFromTarget(DateTime dateOfParsing)
        {
            return Neo4JClientHandler.GetListOfLocationsFromTarget(_graphClient, dateOfParsing, _targetUsername);
        }

        public List<DateTime> GetListOfParsingTimes()
        {
            return Neo4JClientHandler.GetListOfDatesOfProcessing(_graphClient, _targetUsername);
        }

        public DateTime GetLastParsingTime()
        {
            return Neo4JClientHandler.GetLastDateOfProcessing(_graphClient, _targetUsername);
        }

        private void FillDatabase(IEnumerable<User> users, IEnumerable<Location> locations)
        {
            _graphClient.Connect();
            var usersList = users.ToList();
            var modelUsers = (from user in usersList select user.ModelViewUser).ToList();
            Neo4JClientHandler.CreateUsers(_graphClient, modelUsers);
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