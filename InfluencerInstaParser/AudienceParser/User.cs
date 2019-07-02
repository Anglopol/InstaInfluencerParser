using System;
using System.Collections.Generic;
using InfluencerInstaParser.Database.Model;

namespace InfluencerInstaParser.AudienceParser
{
    public class User
    {
        public string Name { get; }
        public string Uid { get; }
        public ulong InstagramId { get; }
        public IEnumerable<Location> Locations { get; set; }
        public ModelUser ModelUser { get; }

        public User(string name, ulong instagramId)
        {
            Name = name;
            InstagramId = instagramId;
            Uid = Guid.NewGuid().ToString();
        }
        
        private User(string name, ulong instagramId, IEnumerable<Location> locations)
        {
            Locations = locations;
            Name = name;
            InstagramId = instagramId;
            Uid = Guid.NewGuid().ToString();
        }

        public User Clone()
        {
            return new User(Name, InstagramId, Locations);
        }
    }
}