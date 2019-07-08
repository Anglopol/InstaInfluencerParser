using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser;
using InfluencerInstaParser.Database.DataClasses;

namespace InfluencerInstaParser.Database.ModelCreating
{
    public interface IModelCreator
    {
        IModel CreateModel(IUser target, IEnumerable<IUser> firstLevelUsers, IEnumerable<IUser> secondLevelUsers);
    }
}