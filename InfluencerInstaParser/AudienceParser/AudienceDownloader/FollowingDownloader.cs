using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;

namespace InfluencerInstaParser.AudienceParser.AudienceDownloader
{
    public class FollowingDownloader : IFollowingDownloader
    {
        public async Task<IEnumerable<string>> GetFollowing(string username, IInstaApi api)
        {
            var followers = await SafeFollowingDownload(username, api, PaginationParameters.Empty);
            var followersList = from instaUserShort in followers
                select instaUserShort.UserName;
            return followersList;

        }

        private async Task<List<InstaUserShort>> SafeFollowingDownload(string username, IInstaApi api, PaginationParameters parameters,
            List<InstaUserShort> following = null)
        {
            if (following == null)
                following = new List<InstaUserShort>();
            // load more following
            Console.WriteLine($"Loaded so far: {following.Count}, loading more" + api.IsUserAuthenticated);
            var result = await api.UserProcessor.GetUserFollowingAsync(username, parameters);

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
            Console.WriteLine($"Not able to load full list of followers, retry in {delay.TotalSeconds} seconds");
            await Task.Delay(delay);
            return await SafeFollowingDownload(username,api,parameters.StartFromMaxId(nextId), following);
        }
    }
}