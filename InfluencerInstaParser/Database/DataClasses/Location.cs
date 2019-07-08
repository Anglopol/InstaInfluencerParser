using System;

namespace InfluencerInstaParser.Database.DataClasses
{
    public class Location
    {
        public string Name { get; }
        public int PublicId { get; }
        public string Uid { get; }
        public string InfluencerUid { get; }

        public Location(string name, string influencerUid, int publicId)
        {
            Name = name;
            InfluencerUid = influencerUid;
            PublicId = publicId;
            Uid = Guid.NewGuid().ToString();
        }
    }
}