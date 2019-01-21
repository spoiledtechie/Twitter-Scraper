using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Neos07.Checking;
using Twitter;
using HtmlAgilityPack;
using CsvLibrary;

namespace Twitter_Followers_Scapper
{
    class Program
    {
        // 5400 followers per hour or 300 requests* 18 followers per hour
        private static Config config;
        //  private static readonly string cookie = "personalization_id=\"v1_Gs1KfvQ14y+RvX7dtPO1Eg==\"; guest_id=v1%3A154807100735741768; _twitter_sess=BAh7CSIKZmxhc2hJQzonQWN0aW9uQ29udHJvbGxlcjo6Rmxhc2g6OkZsYXNo%250ASGFzaHsABjoKQHVzZWR7ADoPY3JlYXRlZF9hdGwrCBn2OHBoAToMY3NyZl9p%250AZCIlYWQwZDNhMjEzODAzODJiZmE0ODg0MDhmMTI5YTFkZTU6B2lkIiUxNjZm%250ANGNlMTU5ZTMzYzEzZmY2MmJmYjMzZjZmODUzYg%253D%253D--aa9a02790ae8bdd58aeab80efd11c96f23350be0; ct0=1f6b8b69d7c0643e7e816bdc44a8b173; eu_cn=1; gt=1087314825313767424; kdt=05iDT7vk8OomEgkNrMko3rlUVxVDkuiW59OUCiCJ; auth_token=7fd122e536358e65fcae1c52994b54046936791b; dnt=1; csrf_same_site_set=1; lang=en; csrf_same_site=1";
        private static readonly string cookie = "lang=en; dnt=1; kdt=aUEAlOQTAXy0sJ8gTtY7j9oVKCBDBMbc5uYhPyC2; _twitter_sess=BAh7CiIKZmxhc2hJQzonQWN0aW9uQ29udHJvbGxlcjo6Rmxhc2g6OkZsYXNo%250ASGFzaHsABjoKQHVzZWR7ADoPY3JlYXRlZF9hdGwrCF3I16dmAToMY3NyZl9p%250AZCIlMThmY2M1NDg2MzliMGY2NzM0OTI5YjUzY2M0NDkxZmE6B2lkIiVlNGVk%250AMzcxOWFlOGRhM2U5MjQ1YWI2ODczZmM4OTJlZjoJdXNlcmwrB%252FD%252BPGY%253D--14f13e1e7181e4909c33eb192ab1a0879c0730b3; csrf_same_site_set=1; csrf_same_site=1; eu_cn=1; remember_checked_on=0; ct0=6de96db60c934dcd2793f93bd2d92de0; gt=1087327763663601665; personalization_id=\"v1_shRZn8PLx4GESeuch0BAzA==\"; guest_id=v1%3A154807511280595263; ads_prefs=\"HBISAAA=\"; twid=\"u=1715273456\"; auth_token=1a3b36042b115794b7913dafe552a8352186d268";



        static void Main(string[] args)
        {
            if (File.Exists("config.json"))
            {
                config = Config.ParseFromJson(File.ReadAllText("config.json"));
            }
            else
            {
                Config temp = new Config();
                File.WriteAllText("config.json", temp.ToJson());
                Console.WriteLine("Config.json file not found. A new one is created, set the configs and run the program again.");
                Console.Read();
                return;
            }

            Checker checker = new Checker();
            Speedometer speedometer = new Speedometer();
            speedometer.Start();

            FollowingScraper scraper = new FollowingScraper(checker, config.Target, cookie);
            Task.Run(async () =>
            {
                try
                {
                   while (scraper.IsThereMoreItems)
                    {
                        User[] users = await scraper.NextAsync();
                        speedometer.Progress += users.Length;
                        Console.Title = $"Progress: {speedometer.Progress} users | Speed: {speedometer.GetProgressPerHour()} users/hour";

                        foreach (User user in users)
                        {
                            File.AppendAllText($"{config.Target}.csv", user.ToCsv() + "\n");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Console.WriteLine("Done!");
            });

            while (true)
                Console.Read();
        }
    }
}
