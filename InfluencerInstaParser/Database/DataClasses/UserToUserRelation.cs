namespace InfluencerInstaParser.Database.DataClasses
{
    public class UserToUserRelation
    {
        public string ParentUid { get; }
        public string ChildUid { get; }
        public int Likes { get; set; }
        public int Comments { get; set; }
        public bool IsEmpty => Likes + Comments > 0;

        public UserToUserRelation(string parentUid, string childUid)
        {
            ParentUid = parentUid;
            ChildUid = childUid;
        }
    }
}