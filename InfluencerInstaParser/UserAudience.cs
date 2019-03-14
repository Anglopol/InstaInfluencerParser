using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;

namespace InfluencerInstaParser
{
    public class UserAudience
    {
        private IInstaApi _instaApi;
        public UserAudience(IInstaApi instaApi)
        {
            _instaApi = instaApi;
        }
        public async Task<List<InstaUserShort>> GetFollowersList(string username,
            PaginationParameters parameters, List<InstaUserShort> followers = null)
        {
            if (followers == null)
                followers = new List<InstaUserShort>();
            // load more followers
            Console.WriteLine($"Loaded so far: {followers.Count}, loading more");
            var result = await _instaApi.UserProcessor.GetUserFollowersAsync(username, parameters);

            // merge results
            if (result.Value != null)
                followers = result.Value.Union(followers).ToList();

            if (result.Succeeded)
                return followers;

            // prepare nex id
            var nextId = result.Value?.NextMaxId ?? parameters.NextMaxId;

            // setup some delay
            var delay = TimeSpan.FromSeconds(new Random(DateTime.Now.Millisecond).Next(60, 120));
            if (result.Info.ResponseType == ResponseType.RequestsLimit)
                delay = TimeSpan.FromSeconds(new Random(DateTime.Now.Millisecond).Next(120, 360));
            Console.WriteLine($"Not able to load full list of followers, retry in {delay.TotalSeconds} seconds");
            await Task.Delay(delay);
            return await GetFollowersList(username, parameters.StartFromMaxId(nextId), followers);
        }

        public async Task<List<InstaUserShort>> GetFollowingList(string username,
            PaginationParameters parameters, List<InstaUserShort> following = null)
        {
            if (following == null)
                following = new List<InstaUserShort>();
            // load more followers
            Console.WriteLine($"Loaded so far: {following.Count}, loading more");
            var result = await _instaApi.UserProcessor.GetUserFollowingAsync(username, parameters);

            // merge results
            if (result.Value != null)
                following = result.Value.Union(following).ToList();

            if (result.Succeeded)
                return following;

            // prepare nex id
            var nextId = result.Value?.NextMaxId ?? parameters.NextMaxId;

            // setup some delay
            var delay = TimeSpan.FromSeconds(new Random(DateTime.Now.Millisecond).Next(60, 120));
            if (result.Info.ResponseType == ResponseType.RequestsLimit)
                delay = TimeSpan.FromSeconds(new Random(DateTime.Now.Millisecond).Next(120, 360));
            Console.WriteLine($"Not able to load full list of following users, retry in {delay.TotalSeconds} seconds");
            await Task.Delay(delay);
            return await GetFollowingList(username, parameters.StartFromMaxId(nextId), following);
        }
    }
}