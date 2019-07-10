namespace InfluencerInstaParser.Database.DataClasses
{
    public class UserToLocationRelation
    {
        public string UserUid { get; }
        public string LocationUid { get; }
        public double Distance { get; }
        public ulong PlaceInstagramId { get; }
        public double Lat { get; }
        public double Long { get; }
        public int CountOfTags { get; set; }

        public UserToLocationRelation(string userUid, string locationUid, double distance, ulong placeInstagramId,
            double lat, double l)
        {
            UserUid = userUid;
            LocationUid = locationUid;
            Distance = distance;
            PlaceInstagramId = placeInstagramId;
            Lat = lat;
            Long = l;
        }
    }
}