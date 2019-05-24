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
using NLog;

namespace InfluencerInstaParser.Database
{
    public class Neo4JClient : IDisposable
    {
        private readonly IDriver _driver;
        private readonly Logger _logger;

        public Neo4JClient(IConnectionSettings settings)
        {
            _driver = GraphDatabase.Driver(settings.Uri, settings.AuthToken);
            _logger = LogManager.GetCurrentClassLogger();
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
            _logger.Info(_logger);
            foreach (var modelUser in modelUsers)
            {
                _logger.Info($"{modelUser.Username}");
            }

            var cypher = new StringBuilder()
                .AppendLine("UNWIND {modelUsers} AS user")
                .AppendLine("MERGE (u:User {name: user.name})")
                .AppendLine("SET u = user")
                .ToString();

            using (var session = _driver.Session())
            {
                await session.RunAsync(cypher,
                    new Dictionary<string, object> {{"modelUsers", ParameterSerializer.ToDictionary(modelUsers)}});
            }
        }

        public async Task CreateLocations(IList<Location> locations)
        {
            _logger.Info(_logger);
            foreach (var loc in locations)
            {
                _logger.Info($"{loc.Name}");
            }

            var cypher = new StringBuilder()
                .AppendLine("UNWIND {locations} AS location")
                .AppendLine("MERGE (l:Location {name: location.name, audienceFrom: location.audienceFrom})")
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
                .AppendLine("UNWIND {relationsList} AS relation")
                .AppendLine("MATCH (p:User { name: relation.parent })")
                .AppendLine("MATCH (c:User { name: relation.child })")
                .AppendLine(
                    "MERGE (c)-[:CONNECTED {likes: relation.likes, comments: relation.comments, follower: relation.follower}]->(p)")
                .ToString();


            using (var session = _driver.Session())
            {
                await session.RunAsync(cypher,
                    new Dictionary<string, object>
                        {{"relationsList", ParameterSerializer.ToDictionary(relationsList)}});
            }
        }

        public async Task CreateLocationsRelationships(IList<User> users)
        {
            var modelRelations = (from user in users
                    from locations in user.Locations.Values
                    from values in locations.Values
                    select values)
                .ToList();
            var cypher = new StringBuilder()
                .AppendLine("UNWIND {locationsRelations} AS relation")
                .AppendLine("MATCH (u:User { name: relation.child })")
                .AppendLine("MATCH (l:Location { name: relation.name, audienceFrom: relation.parent })")
                .AppendLine(
                    "MERGE (u)-[:VISITED { count: relation.count}]->(l)")
                .ToString();


            using (var session = _driver.Session())
            {
                await session.RunAsync(cypher,
                    new Dictionary<string, object>
                        {{"locationsRelations", ParameterSerializer.ToDictionary(modelRelations)}});
            }
        }
    }
}