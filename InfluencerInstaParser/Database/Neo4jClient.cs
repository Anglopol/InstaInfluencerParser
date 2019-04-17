using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfluencerInstaParser.Database.Serializer;
using InfluencerInstaParser.Database.Settings;
using InfluencerInstaParser.Database.UserInformation;
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
                .AppendLine(
                    "MERGE (u:User {name: user.name, parent: user.parent.name, likes: user.likes, comments: user.comments, followers: user.followers, following: user.following})")
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
            var relationsList = (from user in users from relation in user.Relations select relation.Value).ToList();
            var cypher = new StringBuilder()
                .AppendLine("UNWIND {relations} AS relation")
                // Find the User:
                .AppendLine("MATCH (p:User { name: relation.parent })")
                .AppendLine("MATCH (c:User { name: relation.child })")
                // Create Relationships:
                .AppendLine(
                    "MERGE (c)-[:CHILD]->(r:Relation {likes: relation.likes, comments: relation.comments})-[:PARENT]->(p)")
                .ToString();


            using (var session = _driver.Session())
            {
                await session.RunAsync(cypher,
                    new Dictionary<string, object> {{"relations", ParameterSerializer.ToDictionary(relationsList)}});
            }
        }
    }
}