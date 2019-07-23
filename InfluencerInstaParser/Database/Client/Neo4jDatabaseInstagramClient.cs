using System;
using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser;
using InfluencerInstaParser.Database.Client.Connection;
using InfluencerInstaParser.Database.DataClasses;
using Neo4jClient;

namespace InfluencerInstaParser.Database.Client
{
    public class Neo4JDatabaseInstagramClient : IDatabaseClientHandler
    {
        private readonly GraphClient _graphClient;

        public Neo4JDatabaseInstagramClient(IConnectionParams connectionParams)
        {
            _graphClient = new GraphClient(connectionParams.ConnectionUri, connectionParams.Username,
                connectionParams.Password);
            _graphClient.Connect();
        }

        public void CreateAnalysis(IModel model)
        {
            CreateTargetAndAnalysisNodes(model);
            CreateUsers(model.Users, model.Target);
            CreateLocations(model.Locations);
            CreateUserToUserRelations(model.UserToUserRelations);
            CreateUserToLocationRelations(model.UserToLocationRelations);
        }

        private void CreateTargetAndAnalysisNodes(IModel model)
        {
            var analysisUid = Guid.NewGuid().ToString();
            _graphClient.Cypher
                .Merge($"(target:Target {{instagramId: {model.Target.InstagramId}}})")
                .Create(
                    $"(analysis:Analysis {{date: \"{model.ModelCreatingDate}\", uid: \"{analysisUid}\", targetInstagramId: {model.Target.InstagramId}, targetUid: \"{model.Target.Uid}\"}})")
                .Merge($"(analysis)-[:INSTAGRAM_ANALYSIS {{date: \"{model.ModelCreatingDate}\"}}]->(target)")
                .ExecuteWithoutResults();
        }

        private void CreateUsers(IEnumerable<IUser> users, IUser target)
        {
            foreach (var user in users)
            {
                _graphClient.Cypher
                    .Create(
                        $"(u:User {{name: \"{user.Name}\", uid: \"{user.Uid}\", instagramId: {user.InstagramId}, likes: {user.Likes}, comments: {user.Comments}}})")
                    .ExecuteWithoutResults();
            }
            CreateUserToAnalysisRelation(target);
        }

        private void CreateUserToAnalysisRelation(IUser user)
        {
            _graphClient.Cypher
                .Match("(analysis:Analysis)", "(user:User)")
                .Where($"analysis.targetUid = {user.Uid}")
                .AndWhere($"user.uid = {user.Uid}")
                .Merge("(user)-[:ANALYZED]->(analysis)")
                .ExecuteWithoutResults();
        }

        private void CreateLocations(IEnumerable<Location> locations)
        {
            foreach (var location in locations)
            {
                _graphClient.Cypher
                    .Create(
                        $"(l:Location {{name: \"{location.Name}\", cityId: {location.PublicId}, uid: \"{location.Uid}\", influencerUid: \"{location.InfluencerUid}\"}})")
                    .ExecuteWithoutResults();
            }
        }

        private void CreateUserToUserRelations(IEnumerable<UserToUserRelation> relations)
        {
            foreach (var relation in relations)
            {
                _graphClient.Cypher
                    .Match("(child:User)", "(parent:User)")
                    .Where($"child.uid = \"{relation.ChildUid}\"")
                    .AndWhere($"parent.uid = \"{relation.ParentUid}\"")
                    .Create(
                        $"(child)-[FROM_POSTS: {{comments: {relation.Comments}, likes: {relation.Likes}}}]->(parent)")
                    .ExecuteWithoutResults();
            }
        }

        private void CreateUserToLocationRelations(IEnumerable<UserToLocationRelation> relations)
        {
            foreach (var relation in relations)
            {
                _graphClient.Cypher
                    .Match("(location:Location)", "(user:User)")
                    .Where($"user.uid = \"{relation.UserUid}\"")
                    .AndWhere($"location.uid = \"{relation.LocationUid}\"")
                    .Create(
                        $"(user)-[:TAGGED {{placeId: {relation.PlaceInstagramId}, distance: {relation.Distance}, lat: {relation.Lat}, long: {relation.Long}}}]->(location)")
                    .ExecuteWithoutResults();
            }
        }
    }
}