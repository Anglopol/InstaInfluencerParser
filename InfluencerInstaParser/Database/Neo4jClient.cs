using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser.UserInformation;
using InfluencerInstaParser.Database.ModelView;
using InfluencerInstaParser.Database.Serializer;
using InfluencerInstaParser.Database.Settings;
using Neo4j.Driver.V1;

namespace InfluencerInstaParser.Database
{
    public class Neo4JClient : IDisposable
    {
        private readonly IDriver _driver;

        public Neo4JClient(IConnectionSettings settings)
        {
            _driver = GraphDatabase.Driver(settings.Uri, settings.AuthToken);
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }

        public async Task CreateIndices()
        {
            string[] queries =
            {
                "CREATE INDEX ON :User(name)",
                "CREATE INDEX ON :Location(name)"
            };

            using (var session = _driver.Session())
            {
                foreach (var query in queries) await session.RunAsync(query);
            }
        }

        public async Task CreateUsers(IList<User> users)
        {
            var modelUsers = (from user in users select user.ModelViewUser).ToList();
            var cypher = new StringBuilder()
                .AppendLine("UNWIND {users} AS user")
                .AppendLine("MERGE (u:User)")
                .AppendLine("SET u = user")
                .ToString();

            using (var session = _driver.Session())
            {
                await session.RunAsync(cypher,
                    new Dictionary<string, object> {{"users", ParameterSerializer.ToDictionary(modelUsers)}});
            }
        }

        public async Task CreateLocations(IList<Location> locations)
        {
            var cypher = new StringBuilder()
                .AppendLine("UNWIND {locations} AS location")
                .AppendLine("MERGE (l:Location)")
                .AppendLine("SET l = location")
                .ToString();

            using (var session = _driver.Session())
            {
                await session.RunAsync(cypher,
                    new Dictionary<string, object> {{"locations", ParameterSerializer.ToDictionary(locations)}});
            }
        }

        public async Task CreateUsersRelationships(IList<User> users)
        {
            var relationsList = (from user in users from relation in user.Relations select relation.Value.Relation)
                .ToList();
            var cypher = new StringBuilder()
                .AppendLine("UNWIND {relations} AS relation")
                .AppendLine("MATCH (p:User { name: relation.parent })")
                .AppendLine("MATCH (c:User { name: relation.child })")
                .AppendLine(
                    "MERGE (c)-[:CONNECTED {likes: relation.likes, comments: relation.comments, follower: relation.follower}]->(p)")
                .ToString();


            using (var session = _driver.Session())
            {
                await session.RunAsync(cypher,
                    new Dictionary<string, object> {{"relations", ParameterSerializer.ToDictionary(relationsList)}});
            }
        }

        public async Task CreateLocationsRelationships(IList<User> users)
        {
            var modelUsers = (from user in users select user.ModelViewUser).ToList();
            var cypher = new StringBuilder()
                .AppendLine("UNWIND {users} AS user")
                .AppendLine("UNWIND user.locations AS location")
                .AppendLine("UNWIND user.parents AS parent")
                .AppendLine("MATCH (u:User { name: user.name })")
                .AppendLine("MATCH (l:Location { name: location, audienceFrom: parent })")
                .AppendLine(
                    "MERGE (u)-[:VISITED]->(l)")
                .ToString();


            using (var session = _driver.Session())
            {
                await session.RunAsync(cypher,
                    new Dictionary<string, object> {{"users", ParameterSerializer.ToDictionary(modelUsers)}});
            }
        }
    }
}