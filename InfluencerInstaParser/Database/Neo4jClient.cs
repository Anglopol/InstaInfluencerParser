using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using InfluencerInstaParser.Database.Serializer;
using InfluencerInstaParser.Database.Settings;
using Neo4j.Driver.V1;

namespace InfluencerInstaParser.Database
{
    public class Neo4jClient
    {
        private readonly IDriver _driver;

        public Neo4jClient(IConnectionSettings settings)
        {
            _driver = GraphDatabase.Driver(settings.Uri, settings.AuthToken);
        }

        public async Task CreateIndices()
        {
            string[] queries =
            {
                "CREATE INDEX ON :User(name)"
            };

            using (var session = _driver.Session())
            {
                foreach (var query in queries) await session.RunAsync(query);
            }
        }

        public async Task CreateUsers(IList<User> users)
        {
            var cypher = new StringBuilder()
                .AppendLine("UNWIND {users} AS user")
                .AppendLine("MERGE (p:User {name: user.name})")
                .AppendLine("SET u = user")
                .ToString();

            using (var session = _driver.Session())
            {
                await session.RunAsync(cypher,
                    new Dictionary<string, object> {{"users", ParameterSerializer.ToDictionary(users)}});
            }
        }

        public async Task CreateRelationships(IList<User> users)
        {
            var cypher = new StringBuilder()
                .AppendLine("UNWIND {users} AS user")
                // Find the User:
                .AppendLine("MATCH (s:User { name: user.name })")
                // Create Relationships:
                .AppendLine("UNWIND user.from AS master")
                .AppendLine("MATCH (m:User { name: master.name })")
                .AppendLine("MERGE (s)-[r:{user.communication}]->(m)")
                .ToString();


            using (var session = _driver.Session())
            {
                await session.RunAsync(cypher,
                    new Dictionary<string, object> {{"users", ParameterSerializer.ToDictionary(users)}});
            }
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}