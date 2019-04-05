namespace InfluencerInstaParser.AudienceParser
{
    public class User
    {
        public User(string username, int likes, int comments, int following, int followers)
        {
            Username = username;
            Likes = likes;
            Comments = comments;
            Following = following;
            Followers = followers;
        }

        public string Username { get; }
        public int Likes { get; }
        public int Comments { get; }
        public int Following { get; }
        public int Followers { get; }
    }
}