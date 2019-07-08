namespace InfluencerInstaParser.Database.DataClasses
{
    public class UserToLocationRelation
    {
        public string UserUid { get; }
        public string LocationUid { get; }
        public int CountOfTags { get; set; }

        public UserToLocationRelation(string userUid, string locationUid)
        {
            UserUid = userUid;
            LocationUid = locationUid;
        }
    }
}