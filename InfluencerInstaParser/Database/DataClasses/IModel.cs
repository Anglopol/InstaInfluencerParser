using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser;

namespace InfluencerInstaParser.Database.DataClasses
{
    public interface IModel
    {
        IEnumerable<UserToUserRelation> UserToUserRelations { get; }
        IEnumerable<UserToLocationRelation> UserToLocationRelations { get; }
        IEnumerable<IUser> Users { get; }
        IEnumerable<Location> Locations { get; }
    }
}