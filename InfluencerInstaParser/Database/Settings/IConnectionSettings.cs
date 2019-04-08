using Neo4j.Driver.V1;

namespace InfluencerInstaParser.Database.Settings
{
    public interface IConnectionSettings
    {
        string Uri { get; }

        IAuthToken AuthToken { get; }
    }
}