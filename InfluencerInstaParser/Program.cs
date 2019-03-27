using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using InfluencerInstaParser.AudienceParser;
using InfluencerInstaParser.AudienceParser.AudienceDownloader;
using InfluencerInstaParser.AudienceParser.WebParsing;
using InfluencerInstaParser.SessionData;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;

namespace InfluencerInstaParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = Task.Run(MainAsync).GetAwaiter().GetResult();
            if (result)
                return;
        }

        public static async Task<bool> MainAsync()
        {
            var userAgentsQueue = new Queue<string>();
            using (var reader = new StreamReader("useragents.txt", System.Text.Encoding.Default))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    userAgentsQueue.Enqueue(line);
                }
            }
            var q = new Queue<string>();
            
            using (var reader = new StreamReader("posts.txt", System.Text.Encoding.Default))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    q.Enqueue(line);
                }
            }
//            var postsParser = new WebParser(userAgentsQueue.Dequeue());
//            postsParser.GetPostsShortCodesFromUser("varlamov", 2);
            var set = SingletonParsingSet.GetInstance();
            

            while (q.Count > 0)
            {
                var parser = new WebParser(userAgentsQueue.Dequeue());
                var post = q.Dequeue();
                var likes = new Thread(() => parser.GetUsernamesFromPostLikes(post)) {Name = post};
                var comments  = new Thread(() => parser.GetUsernamesFromPostComments(post)) {Name = post};
                likes.Start();
//                comments.Start();
            }

            
            Console.WriteLine();
            foreach (var user in set.HandledUsers)
            {
                Console.WriteLine(user.Key);
            }

            return true;
        }
    }
}