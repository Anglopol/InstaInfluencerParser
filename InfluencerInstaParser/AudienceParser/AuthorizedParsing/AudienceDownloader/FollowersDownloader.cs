using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;

namespace InfluencerInstaParser.AudienceParser.AuthorizedParsing.AudienceDownloader
{
    public class FollowersDownloader : IFollowersDownloader
    {
        public async Task<IEnumerable<string>> GetFollowers(string username, IInstaApi api)
        {
            var followers = await SafeFollowersDownload(username, api, PaginationParameters.Empty);
            var followersList = from instaUserShort in followers
                select instaUserShort.UserName;
            return followersList;
        }

        private async Task<List<InstaUserShort>> SafeFollowersDownload(string username, IInstaApi api,
            PaginationParameters parameters,
            List<InstaUserShort> followers = null)
        {
            if (followers == null)
                followers = new List<InstaUserShort>();
            // load more followers
            Console.WriteLine($"Loaded so far: {followers.Count}, loading more" + api.IsUserAuthenticated);
            var result = await api.UserProcessor.GetUserFollowersAsync(username, parameters);

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
            return await SafeFollowersDownload(username, api, parameters.StartFromMaxId(nextId), followers);
        }
    }
}