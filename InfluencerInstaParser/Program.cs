using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser;
using InfluencerInstaParser.Database;
using InfluencerInstaParser.Database.Settings;

namespace InfluencerInstaParser
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var parser = new ParsingHandler("tasyabraun");
            parser.Parse();
            var set = ParsingSetSingleton.GetInstance();
            Task.Run(() => FillDb(set.UnprocessedUsers.Values.ToList())).GetAwaiter();
        }

        private static async Task FillDb(IList<User> users)
        {
            var settings = ConnectionSettings.CreateBasicAuth("bolt://localhost:7687/db/users", "neo4j", "1111");

            using (var client = new Neo4jClient(settings))
            {

                // Create Indices for faster Lookups:
                await client.CreateIndices();

                // Create Base Data:
                await client.CreateUsers(users);
                await client.CreateRelationships(users);
            }
        }
    }
}