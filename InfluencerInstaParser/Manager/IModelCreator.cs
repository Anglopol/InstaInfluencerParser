using System.Collections.Generic;
using InfluencerInstaParser.AudienceParser.UserCreating.ParsedUser;

namespace InfluencerInstaParser.Manager
{
    public interface IModelCreator
    {
        IModel CreateModel(IUser target, IEnumerable<IUser> firstLevelUsers, IEnumerable<IUser> secondLevelUsers);
    }
}