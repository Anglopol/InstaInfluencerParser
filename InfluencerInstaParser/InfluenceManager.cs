using System;
using System.Collections.Generic;
using System.Linq;
using InfluencerInstaParser.AudienceParser;
using InfluencerInstaParser.AudienceParser.Proxy;
using InfluencerInstaParser.AudienceParser.UserInformation;
using InfluencerInstaParser.Database;
using InfluencerInstaParser.Database.ModelView;
using Neo4jClient;

namespace InfluencerInstaParser
{
    public class InfluenceManager
    {
        private readonly Uri _databaseConnectionUri;
        private readonly string _databaseUsername;
        private readonly string _databasePassword;
        private readonly string _pathToProxyFile;

        public InfluenceManager(Uri databaseConnectionUri, string databaseUsername, string databasePassword,
            string pathToProxyFile)
        {
            _databaseConnectionUri =
                databaseConnectionUri ?? throw new ArgumentNullException(nameof(databaseConnectionUri));
            _databaseUsername = databaseUsername ?? throw new ArgumentNullException(nameof(databaseUsername));
            _databasePassword = databasePassword ?? throw new ArgumentNullException(nameof(databasePassword));
            _pathToProxyFile = pathToProxyFile ?? throw new ArgumentNullException(nameof(pathToProxyFile));
        }

        public void ProcessUser(string targetUsername)
        {
            var client = new GraphClient(_databaseConnectionUri, _databaseUsername, _databasePassword);
            client.Connect();
            ProxyFromFileCreatorSingleton.GetInstance().SetPathToProxyFile(_pathToProxyFile);
            var parser = new ParsingHandler(targetUsername);
            parser.Parse();
            var locations = ParsingSetSingleton.GetInstance().GetListOfLocations();
            var users = ParsingSetSingleton.GetInstance().GetProcessedUsers();
            FillDatabase(client, users, locations);
        }

        private static void FillDatabase(GraphClient client, IEnumerable<User> users, IEnumerable<Location> locations)
        {
            var enumerable = users.ToList();
            var modelUsers = (from user in enumerable select user.ModelViewUser).ToList();
            Neo4jClientHandler.CreateUsers(client, modelUsers);
            Neo4jClientHandler.CreateLocations(client, locations);
            var usersRelations =
                (from user in enumerable from userDict in user.Relations select userDict.Value).ToList();
            var usersModelRelations = (from relation in usersRelations select relation.Relation).ToList();
            Neo4jClientHandler.CreateUsersRelations(client, usersModelRelations);
            var locationsRelationsDict =
                from user in enumerable from locationDict in user.Locations select locationDict.Value;
            var locationRelation =
                (from relation in locationsRelationsDict from values in relation.Values select values).ToList();
            Neo4jClientHandler.CreateLocationsRelations(client, locationRelation);
        }
    }
}