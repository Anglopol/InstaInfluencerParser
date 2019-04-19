using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser;
using InfluencerInstaParser.AudienceParser.UserInformation;
using InfluencerInstaParser.Database;
using InfluencerInstaParser.Database.ModelView;
using InfluencerInstaParser.Database.Settings;

namespace InfluencerInstaParser
{
    internal class Program
    {
        private static void Main()
        {
            var targetUsername = "neprostohronika";
            var parser = new ParsingHandler(targetUsername);
            parser.Parse();
            var set = ParsingSetSingleton.GetInstance();
            Task.Run(() => FillDb(set.GetAllUsers(), set.Locations.Values.ToList(), targetUsername)).GetAwaiter()
                .GetResult();
        }

        private static async Task FillDb(IList<User> users, IList<Location> locations, string target)
        {
            var settings = ConnectionSettings.CreateBasicAuth("bolt://localhost:7687/db/users", "neo4j", "1111");

            using (var client = new Neo4JClient(settings))
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
}