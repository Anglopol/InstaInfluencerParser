using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser;
using InfluencerInstaParser.AudienceParser.Proxy;
using InfluencerInstaParser.AudienceParser.UserInformation;
using InfluencerInstaParser.Database;
using InfluencerInstaParser.Database.ModelView;
using InfluencerInstaParser.Database.Settings;

namespace InfluencerInstaParser
{
    public class InfluenceManager
    {
        private readonly string _databaseConnectionUri;
        private readonly string _databaseUsername;
        private readonly string _databasePassword;
        private readonly string _pathToProxyFile;

        public InfluenceManager(string databaseConnectionUri, string databaseUsername, string databasePassword,
            string pathToProxyFile)
        {
            _databaseConnectionUri =
                databaseConnectionUri ?? throw new ArgumentNullException(nameof(databaseConnectionUri));
            _databaseUsername = databaseUsername ?? throw new ArgumentNullException(nameof(databaseUsername));
            _databasePassword = databasePassword ?? throw new ArgumentNullException(nameof(databasePassword));
            _pathToProxyFile = pathToProxyFile ?? throw new ArgumentNullException(nameof(pathToProxyFile));
        }

        public async Task ProcessUserAsync(string targetUsername)
        {
            var client = CreateDatabaseClient();
            ProxyFromFileCreatorSingleton.GetInstance().SetPathToProxyFile(_pathToProxyFile);
            var parser = new ParsingHandler(targetUsername);
            parser.Parse();
            var locations = ParsingSetSingleton.GetInstance().GetListOfLocations();
            var users = ParsingSetSingleton.GetInstance().GetProcessedUsers();
            await FillDatabase(client, users, locations, targetUsername);
        }

        private Neo4JClient CreateDatabaseClient()
        {
            var settings =
                ConnectionSettings.CreateBasicAuth(_databaseConnectionUri, _databaseUsername, _databasePassword);
            return new Neo4JClient(settings);
        }

        private static async Task FillDatabase(Neo4JClient client, IList<User> users, IList<Location> locations,
            string target)
        {
            // Create Indices for faster Lookups:
            await client.CreateIndices();

            // Create Base Data:
            await client.CreateUsers(users);
            await client.CreateLocations(locations);
            var relationUsers = (from user in users where user.Username != target select user).ToList();
            await client.CreateUsersRelationships(relationUsers);
            await client.CreateLocationsRelationships(relationUsers);
        }
    }
}