using System.Collections.Generic;
using InfluencerInstaParser.Database.ModelView;
using Neo4jClient;

namespace InfluencerInstaParser.Database
{
    public class Neo4jClientHandler
    {
        public static void CreateUsers(GraphClient graphClient, IEnumerable<ModelUser> users)
        {
            foreach (var user in users)
            {
                graphClient.Cypher
                    .Create("(user:User {newUser})")
                    .WithParam("newUser", user)
                    .ExecuteWithoutResults();
            }
        }

        public static void CreateLocations(GraphClient graphClient, IEnumerable<Location> locations)
        {
            foreach (var location in locations)
            {
                graphClient.Cypher
                    .Create("(location:Location {newLocation})")
                    .WithParam("newLocation", location)
                    .ExecuteWithoutResults();
            }
        }

        public static void CreateUsersRelations(GraphClient graphClient, IEnumerable<ModelRelation> relations)
        {
            foreach (var relation in relations)
            {
                graphClient.Cypher
                    .Match("(parent:User)", "(child:User)")
                    .Where((ModelUser child) => child.Username == relation.Child)
                    .AndWhere((ModelUser parent) => parent.Username == relation.Parent)
                    .Create(
                        $"child-[:CONNECTED {{likes: {relation.Likes}, comments: {relation.Comments}, follower: {relation.Follower}}}]->parent")
                    .ExecuteWithoutResults();
            }
        }

        public static void CreateLocationsRelations(GraphClient graphClient,
            IEnumerable<LocationRelationInformation> relationInformation)
        {
            foreach (var relation in relationInformation)
            {
                graphClient.Cypher
                    .Match("(user:User)", "(location:Location)")
                    .Where((ModelUser user) => user.Username == relation.Child)
                    .AndWhere(
                        (Location location) => location.Name == relation.Name && location.Owner == relation.Parent)
                    .Create(
                        $"user-[:VISITED {{count: {relation.Count}}}]->location")
                    .ExecuteWithoutResults();
            }
        }
    }
}