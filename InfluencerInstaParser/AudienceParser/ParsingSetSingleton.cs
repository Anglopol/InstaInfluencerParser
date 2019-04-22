using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using InfluencerInstaParser.AudienceParser.UserInformation;
using InfluencerInstaParser.Database.ModelView;

namespace InfluencerInstaParser.AudienceParser
{
    public class ParsingSetSingleton
    {
        private static ParsingSetSingleton _instance;

        private ParsingSetSingleton()
        {
            ShortCodesQueue = new ConcurrentQueue<string>();
            UnprocessedUsers = new ConcurrentDictionary<string, User>();
            ProcessedUsers = new ConcurrentDictionary<string, User>();
            Locations = new ConcurrentDictionary<string, Location>();
        }

        public ConcurrentQueue<string> ShortCodesQueue { get; }
        public ConcurrentDictionary<string, User> UnprocessedUsers { get; }
        public ConcurrentDictionary<string, User> ProcessedUsers { get; }

        public ConcurrentDictionary<string, Location> Locations { get; }

        public static ParsingSetSingleton GetInstance()
        {
            return _instance ?? (_instance = new ParsingSetSingleton());
        }

        public void AddUnprocessedUser(string username, User parent, int followers, int following, bool isInfluencer,
            CommunicationType type = CommunicationType.Follower)
        {
            if (ProcessedUsers.ContainsKey(username) || UnprocessedUsers.ContainsKey(username))
            {
                var currentUser = UnprocessedUsers.ContainsKey(username)
                    ? UnprocessedUsers[username]
                    : ProcessedUsers[username];
                switch (type)
                {
                    case CommunicationType.Liker:
                    {
                        currentUser.AddLikesForRelation(parent);
                        break;
                    }
                    case CommunicationType.Commentator:
                    {
                        currentUser.AddCommentsForRelation(parent);
                        break;
                    }
                    case CommunicationType.Follower:
                    {
                        currentUser.AddNewRelation(parent);
                        break;
                    }
                }
            }

            if (!ProcessedUsers.ContainsKey(username))
                UnprocessedUsers.TryAdd(username,
                    new User(username, parent, type, isInfluencer, followers: followers, following: following));
        }

        public void AddProcessedUser(User user)
        {
            ProcessedUsers.TryAdd(user.Username, user);
        }

        public void AddInShortCodesQueue(string shortCode)
        {
            ShortCodesQueue.Enqueue(shortCode);
        }

        public List<User> GetAllUsers()
        {
            return ProcessedUsers.Values.ToList().Union(UnprocessedUsers.Values.ToList()).ToList();
        }
    }
}