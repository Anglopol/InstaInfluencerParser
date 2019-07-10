using System;
using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser;

namespace InfluencerInstaParser.Database.DataClasses
{
    public class Model : IModel
    {
        public IEnumerable<UserToUserRelation> UserToUserRelations { get; }
        public IEnumerable<UserToLocationRelation> UserToLocationRelations { get; }
        public IEnumerable<IUser> Users { get; }
        public IEnumerable<Location> Locations { get; }
        public IUser Target { get; }
        public DateTime ModelCreatingDate { get; }

        public Model(IEnumerable<UserToUserRelation> userToUserRelations,
            IEnumerable<UserToLocationRelation> userToLocationRelations, IEnumerable<IUser> users,
            IEnumerable<Location> locations, IUser traget)
        {
            UserToUserRelations = userToUserRelations;
            UserToLocationRelations = userToLocationRelations;
            Users = users;
            Locations = locations;
            ModelCreatingDate = DateTime.UtcNow;
            Target = traget;
        }
    }
}