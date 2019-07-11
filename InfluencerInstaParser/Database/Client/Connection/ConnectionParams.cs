using System;

namespace InfluencerInstaParser.Database.Client.Connection
{
    public class ConnectionParams : IConnectionParams
    {
        private const string Login = "neo4j";
        private const string Passphrase = "J9xMXjhMsZKn8$";

        public Uri ConnectionUri { get; } = new Uri("http://188.68.210.25:7474/db/data/");
        public string Username => Login;
        public string Password => Passphrase;
    }
}