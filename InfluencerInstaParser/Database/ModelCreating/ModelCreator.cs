using System.Collections.Generic;
using System.Linq;
using InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser;
using InfluencerInstaParser.Database.DataClasses;

namespace InfluencerInstaParser.Database.ModelCreating
{
    public class ModelCreator : IModelCreator
    {
        public IModel CreateModel(IUser target, IEnumerable<IUser> firstLevelUsers, IEnumerable<IUser> secondLevelUsers)
        {
            var firstLevelList = firstLevelUsers.ToList();
            var secondLevelList = secondLevelUsers.ToList();
            var allUsers = firstLevelList.Union(secondLevelList).Append(target).ToList();
            var userToUserRelations = GetUserRelations(target, firstLevelList, secondLevelList).ToList();
            var locations = GetLocations(target, firstLevelList, secondLevelList).ToList();
            var userToLocationRelations =
                GetUserToLocationRelations(locations, userToUserRelations, GetUidIUserDictionary(allUsers));
            return new Model(userToUserRelations, userToLocationRelations, allUsers.Distinct(), locations, target);
        }

        private static IEnumerable<UserToUserRelation> GetUserRelations(IUser target,
            IEnumerable<IUser> firstLevelUsers, IEnumerable<IUser> secondLevelUsers)
        {
            var listOfFirstLevelUsers = firstLevelUsers.ToList();
            var firstLevelRelations = from user in listOfFirstLevelUsers select target.GetRelation(user);
            var influencers = GetInfluencers(listOfFirstLevelUsers);
            var secondLevelRelations = from influencer in influencers
                from secondLevelUser in secondLevelUsers
                select influencer.GetRelation(secondLevelUser);
            var allRelations = firstLevelRelations.Union(secondLevelRelations);
            return from relation in allRelations where !relation.IsEmpty select relation;
        }

        private static IEnumerable<Location> GetLocations(IUser target, IEnumerable<IUser> firstLevelUsers,
            IEnumerable<IUser> secondLevelUsers)
        {
            var listOfFirstLevelUsers = firstLevelUsers.ToList();
            var result = GetLocationsForInfluencer(target, listOfFirstLevelUsers);
            var influencers = GetInfluencers(listOfFirstLevelUsers);
            var locationsForInfluencers = from influencer in influencers
                select GetLocationsForInfluencer(influencer, secondLevelUsers);
            return locationsForInfluencers.Aggregate(result,
                (current, locationsForInfluencer) => current.Union(locationsForInfluencer));
        }

        private static IEnumerable<Location> GetLocationsForInfluencer(IUser influencer, IEnumerable<IUser> users)
        {
            var locations = new Dictionary<string, Location>();
            var relevantScrapResults =
                from user in users
                where influencer.HasRelationToUser(user)
                from location in user.Locations
                select location;
            foreach (var scrapResult in relevantScrapResults)
            {
                var locationName = scrapResult.Name;
                var locationPublicId = scrapResult.PublicId;
                locations.TryAdd(locationName, new Location(locationName, influencer.Uid, locationPublicId));
            }

            return locations.Values;
        }

        private static IEnumerable<UserToLocationRelation> GetUserToLocationRelations(IEnumerable<Location> locations,
            IEnumerable<UserToUserRelation> userToUserRelations, Dictionary<string, IUser> users)
        {
            return from userToUserRelation in userToUserRelations
                from location in locations
                where location.InfluencerUid == userToUserRelation.ParentUid
                from user in users
                where user.Value.Uid == userToUserRelation.ChildUid
                from locationFromUser in user.Value.Locations
                select new UserToLocationRelation(userToUserRelation.ChildUid, location.Uid, locationFromUser.Distance,
                    locationFromUser.InstagramId, locationFromUser.Lat, locationFromUser.Long);
        }

        private static IEnumerable<IUser> GetInfluencers(IEnumerable<IUser> users)
        {
            return from influencer in users where influencer.IsInfluencer select influencer;
        }

        private static Dictionary<string, IUser> GetUidIUserDictionary(IEnumerable<IUser> users)
        {
            var result = new Dictionary<string, IUser>();
            foreach (var user in users)
            {
                result.TryAdd(user.Uid, user);
            }

            return result;
        }
    }
}