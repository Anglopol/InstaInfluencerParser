using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
                    .Where((ModelUser child) =>
                        child.Username == relation.Child && child.DateOfParsing == relation.DateOfParsing)
                    .AndWhere((ModelUser parent) =>
                        parent.Username == relation.Parent && parent.DateOfParsing == relation.DateOfParsing)
                    .Create(
                        $"child-[:CONNECTED {{likes: {relation.Likes}, comments: {relation.Comments}, follower: {relation.Follower}, date: {relation.DateOfParsing}}}]->parent")
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
                    .Where((ModelUser user) =>
                        user.Username == relation.Child && user.DateOfParsing == relation.DateOfParsing)
                    .AndWhere(
                        (Location location) => location.Name == relation.Name && location.Owner == relation.Parent &&
                                               location.DateOfParsing == relation.DateOfParsing)
                    .Create(
                        $"user-[:VISITED {{count: {relation.Count}, date: {relation.DateOfParsing}}}]->location")
                    .ExecuteWithoutResults();
            }
        }

        public static List<DateTime> GetListOfDatesOfProcessing(GraphClient graphClient, string targetUsername)
        {
            var dates = graphClient.Cypher
                .Match("(user:User)")
                .Where((ModelUser user) => user.Username == targetUsername)
                .Return(user => new
                {
                    Date = DateTime.Parse(user.As<ModelUser>().DateOfParsing)
                })
                .Results;
            var listOfDates = dates.ToList();
            return listOfDates.Count == 0 ? new List<DateTime>() : (from date in listOfDates select date.Date).ToList();
        }

        public static DateTime GetLastDateOfProcessing(GraphClient graphClient, string targetUsername)
        {
            var dates = GetListOfDatesOfProcessing(graphClient, targetUsername);
            var currentDateValue = DateTime.MinValue;
            if (dates.Count == 0) return currentDateValue;
            foreach (var date in dates)
            {
                if (DateTime.Compare(date, currentDateValue) > 0) currentDateValue = date;
            }

            return currentDateValue;
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

//        public static List<KeyValuePair<ModelUser, int>> GetRankedListOfInfluencers(GraphClient graphClient,
//            DateTime dateOfParsing, string targetUsername)
//        {
//            var rankedInfluencers = graphClient
//        }
    }
}