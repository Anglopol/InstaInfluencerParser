using Neo4j.Driver.V1;

namespace InfluencerInstaParser.Database.Settings
{
    public class ConnectionSettings : IConnectionSettings
    {
        public ConnectionSettings(string uri, IAuthToken authToken)
        {
            Uri = uri;
            AuthToken = authToken;
        }

        public string Uri { get; }
        public IAuthToken AuthToken { get; }

        public static ConnectionSettings CreateBasicAuth(string uri, string username, string password)
        {
            return new ConnectionSettings(uri, AuthTokens.Basic(username, password));
        }
    }
}