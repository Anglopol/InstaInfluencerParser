using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser;
using InfluencerInstaParser.Database;
using InfluencerInstaParser.Database.Settings;
using InfluencerInstaParser.Database.UserInformation;

namespace InfluencerInstaParser
{
    internal class Program
    {
        private static void Main()
        {
            var targetUsername = "tasyabraun";
            var parser = new ParsingHandler(targetUsername);
            parser.Parse();
            var set = ParsingSetSingleton.GetInstance();
            Task.Run(() => FillDb(set.UnprocessedUsers.Values.ToList(), targetUsername)).GetAwaiter();
        }

        private static async Task FillDb(IList<User> users, string target)
        {
            var settings = ConnectionSettings.CreateBasicAuth("bolt://localhost:7687/db/users", "neo4j", "1111");

            using (var client = new Neo4JClient(settings))
            {
                // Create Indices for faster Lookups:
                await client.CreateIndices();

                // Create Base Data:
                await client.CreateUsers(users);
                var relationUsers = from user in users where user.Username != target select user;
                await client.CreateRelationships(relationUsers.ToList());
            }
        }
    }
}