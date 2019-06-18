using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using InfluencerInstaParser.Database.Model;
using InfluencerInstaParser.Exceptions;
using Neo4jClient;

namespace InfluencerInstaParser.Database
{
    public static class Neo4JClientHandler
    {
        public static void CreateTarget(GraphClient graphClient, string username)
        {
            var id = GenerateId();
            graphClient.Cypher
                .Create($"(target:Target {{name: {username}, id: {id}}})")
                .ExecuteWithoutResults();
        }

        public static void CreateAnalysis(GraphClient graphClient, DateTime dateOfParsing,
            string targetUsername, out string id)
        {
            id = GenerateId();
            graphClient.Cypher
                .Create($"(analysis:Analysis {{target: {targetUsername}, date: {dateOfParsing}, id: {id}}})")
                .Match("(target:Target)")
                .Where($"target.name = {targetUsername}")
                .Create("analysis-[:TARGET]->target")
                .ExecuteWithoutResults();
        }

        public static string GetUserId(GraphClient graphClient, DateTime dateOfParsing, string username)
        {
            var date = dateOfParsing.ToString(CultureInfo.InvariantCulture);
            var idResult = graphClient.Cypher
                .Match("(user:User)")
                .Where((ModelUser user) => user.Username == username && user.DateOfParsing == date)
                .Return(user => new
                {
                    user.As<ModelUser>().Id
                }).Results;
            var idResultList = idResult.ToList();
            if (idResultList.Count != 0) return (from id in idResultList select id.Id).ToList()[0];
            throw new NoSuchUserInDatabaseException($"user {username} not exist in database on {dateOfParsing}");
        }

        public static Analysis GetAnalysisById(GraphClient graphClient, string analysisId)
        {
            var analyses = graphClient.Cypher
                .Match("analysis:Analysis")
                .Where((Analysis analysis) => analysis.Id == analysisId)
                .Return(analysis => new
                {
                    Result = analysis.As<Analysis>()
                })
                .Results
                .ToList();
            if (analyses.Count != 0) return analyses[0].Result;
            throw new NoSuchAnalysisInDatabaseException($"Analysis with {analysisId} ID not exist in database");
        }

        public static void CreateUsers(GraphClient graphClient, IEnumerable<ModelUser> users, string targetUsername,
            string analysisId)
        {
            foreach (var user in users)
            {
                user.AnalysisId = analysisId;
                var userId = GenerateId();
                graphClient.Cypher
                    .Create("(user:User {newUser})")
                    .WithParam("newUser", user)
                    .Set($"user.id = {userId}")
                    .ExecuteWithoutResults();
                if (user.Username == targetUsername)
                {
                    CreateRelationUserAnalysis(graphClient, analysisId, userId);
                }
            }
        }

        private static void CreateRelationUserAnalysis(GraphClient graphClient, string analysisId, string userId)
        {
            graphClient.Cypher
                .Match("(analysis:Analysis)", "(target:User)")
                .Where((Analysis analysis) => analysis.Id == analysisId)
                .AndWhere((ModelUser target) => target.Id == userId)
                .Set($"analysis.targetId = {userId}")
                .Create($"analysis-[:ANALYSIS {{id: {analysisId}}}]->target")
                .ExecuteWithoutResults();
        }

        public static void CreateLocations(GraphClient graphClient, IEnumerable<Location> locations)
        {
            foreach (var location in locations)
            {
                var id = GenerateId();
                graphClient.Cypher
                    .Create("(location:Location {newLocation})")
                    .WithParam("newLocation", location)
                    .Set($"location.id = {id}")
                    .ExecuteWithoutResults();
            }
        }

        public static void CreateUsersRelations(GraphClient graphClient, IEnumerable<ModelRelation> relations)
        {
            foreach (var relation in relations)
            {
                var id = GenerateId();
                graphClient.Cypher
                    .Match("(parent:User)", "(child:User)")
                    .Where((ModelUser child) =>
                        child.Username == relation.Child && child.DateOfParsing == relation.DateOfParsing)
                    .AndWhere((ModelUser parent) =>
                        parent.Username == relation.Parent && parent.DateOfParsing == relation.DateOfParsing)
                    .Create(
                        $"child-[:CONNECTED {{likes: {relation.Likes}, comments: {relation.Comments}, follower: {relation.Follower}, date: {relation.DateOfParsing}, id: {id}}}]->parent")
                    .ExecuteWithoutResults();
            }
        }

        public static void CreateLocationsRelations(GraphClient graphClient,
            IEnumerable<LocationRelationInformation> relationInformation)
        {
            foreach (var relation in relationInformation)
            {
                var id = GenerateId();
                graphClient.Cypher
                    .Match("(user:User)", "(location:Location)")
                    .Where((ModelUser user) =>
                        user.Username == relation.Child && user.DateOfParsing == relation.DateOfParsing)
                    .AndWhere(
                        (Location location) => location.Name == relation.Name && location.Owner == relation.Parent &&
                                               location.DateOfParsing == relation.DateOfParsing)
                    .Create(
                        $"user-[:VISITED {{count: {relation.Count}, date: {relation.DateOfParsing}, id: {id}}}]->location")
                    .ExecuteWithoutResults();
            }
        }

        public static List<Analysis> GetListOfAnalyses(GraphClient graphClient, string targetUsername)
        {
            var analyses = graphClient.Cypher
                .Match($"(analysis:Analysis {{target: '{targetUsername}'}})")
                .Return(analysis => new
                {
                    Analysis = analysis.As<Analysis>()
                })
                .Results;
            var listOfAnalyses = analyses.ToList();
            return listOfAnalyses.Count == 0
                ? new List<Analysis>()
                : (from analysis in listOfAnalyses select analysis.Analysis).ToList();
        }

        public static ModelUser GetUserById(GraphClient graphClient, string userId)
        {
            var userResult = graphClient.Cypher
                .Match("(user:User)")
                .Where((ModelUser user) => user.Id == userId)
                .Return(user => new
                {
                    User = user.As<ModelUser>()
                })
                .Results
                .ToList();
            if (userResult.Count != 0) return userResult[0].User;
            throw new NoSuchUserInDatabaseException($"user with {userId} ID in database");
        }

        public static Analysis GetLastAnalysis(GraphClient graphClient, string targetUsername)
        {
            var analyses = GetListOfAnalyses(graphClient, targetUsername);
            var currentDateValue = DateTime.MinValue;
            var currentAnalysis = new Analysis();
            if (analyses.Count == 0) return currentAnalysis;
            foreach (var analysis in analyses)
            {
                var time = DateTime.Parse(analysis.Date);
                if (DateTime.Compare(time, currentDateValue) > 0) currentDateValue = time;
                currentAnalysis = analysis;
            }

            return currentAnalysis;
        }

        public static List<ModelUser> GetListOfInfluencers(GraphClient graphClient, DateTime dateOfParsing,
            string targetUsername)
        {
            var date = dateOfParsing.ToString(CultureInfo.InvariantCulture);
            var users = graphClient.Cypher
                .Match("(user:User)")
                // ReSharper disable once RedundantBoolCompare
                .Where((ModelUser user) => user.IsInfluencer == true && user.DateOfParsing == date)
                .Return(user => new
                {
                    User = user.As<ModelUser>()
                })
                .Results;
            return (from user in users where user.User.Parents.Contains(targetUsername) select user.User).ToList();
        }

        public static List<Location> GetListOfLocationsFromTarget(GraphClient graphClient, DateTime dateOfParsing,
            string targetUsername)
        {
            var date = dateOfParsing.ToString(CultureInfo.InvariantCulture);
            var locations = graphClient.Cypher
                .Match("(location:Location)")
                .Where((Location location) => location.Owner == targetUsername && location.DateOfParsing == date)
                .Return(location => new
                {
                    Loc = location.As<Location>()
                })
                .Results;
            return (from location in locations select location.Loc).ToList();
        }

        public static Dictionary<ModelUser, int> GetRankedDictOfInfluencers(GraphClient graphClient,
            DateTime dateOfParsing, string targetUsername)
        {
            const int commentCoefficient = 2;
            var date = dateOfParsing.ToString(CultureInfo.InvariantCulture);
            var rankedInfluencers = graphClient.Cypher
                .Match("p=(influencer:User)-[relation:CONNECTED]->()")
                // ReSharper disable once RedundantBoolCompare
                .Where((ModelUser influencer) => influencer.IsInfluencer == true && influencer.DateOfParsing == date)
                .AndWhere((ModelRelation relation) => relation.Parent == targetUsername)
                .Return((influencer, relation) => new
                {
                    User = influencer.As<ModelUser>(),
                    Rank = relation.As<ModelRelation>().Comments * commentCoefficient +
                           relation.As<ModelRelation>().Likes
                })
                .Results;
            return new Dictionary<ModelUser, int>(rankedInfluencers.Select(rankedInfluencer =>
                new KeyValuePair<ModelUser, int>(rankedInfluencer.User, rankedInfluencer.Rank)));
        }

        public static List<ModelUser> GetListOfInfluencers(GraphClient graphClient, string analysisId)
        {
            var influencers = graphClient.Cypher
                .Match("p=(analysis:Analysis)--(target:User)--(influencer:User)")
                .Where((Analysis analysis) => analysis.Id == analysisId)
                // ReSharper disable once RedundantBoolCompare
                .AndWhere((ModelUser influencer) => influencer.IsInfluencer == true)
                .Return(influencer => new
                {
                    Influencer = influencer.As<ModelUser>()
                })
                .Results
                .ToList();
            return (from influencer in influencers select influencer.Influencer).ToList();
        }

        private static string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}