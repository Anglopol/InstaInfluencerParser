using System;

namespace InfluencerInstaParser.Database.Client.Connection
{
    public interface IConnectionParams
    {
        Uri ConnectionUri { get; } 
        string Username { get; } 
        string Password { get; }
    }
}