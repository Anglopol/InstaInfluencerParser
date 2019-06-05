using System;
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

        public ConcurrentDictionary<string, User> UnprocessedUsers { get; }
        public ConcurrentDictionary<string, User> ProcessedUsers { get; }
        public ConcurrentDictionary<string, Dictionary<int, List<Location>>> Locations { get; }

        private ParsingSetSingleton()
        {
            UnprocessedUsers = new ConcurrentDictionary<string, User>();
            ProcessedUsers = new ConcurrentDictionary<string, User>();
            Locations = new ConcurrentDictionary<string, Dictionary<int, List<Location>>>();
        }

        public static ParsingSetSingleton GetInstance()
        {
            return _instance ?? (_instance = new ParsingSetSingleton());
        }

        public void AddUnprocessedUser(string username, DateTime timeOfParsing, User parent, int followers,
            int following, bool isInfluencer,
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
                        currentUser.AddNewFollowRelation(parent);
                        break;
                    }
                }

                return;
            }

            UnprocessedUsers.TryAdd(username,
                new User(username, timeOfParsing, parent, type, isInfluencer, followers: followers,
                    following: following));
        }

        public void AddProcessedUser(User user)
        {
            ProcessedUsers.TryAdd(user.Username, user);
            UnprocessedUsers.TryRemove(user.Username, out _);
        }

        public List<User> GetProcessedUsers()
        {
            return (from user in ProcessedUsers select user.Value).ToList();
        }

        public IEnumerable<Location> GetListOfLocations()
        {
            return from dict in Locations.Values from secDict in dict.Values from values in secDict select values;
        }
    }
}