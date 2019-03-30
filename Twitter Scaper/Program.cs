using System;
using System.IO;
using System.Threading.Tasks;

using Twitter;

namespace Twitter_Followers_Scapper
{
    class Program
    {
        // 5400 followers per hour or 300 requests* 18 followers per hour
        private static Config config;
        private static readonly string cookie = "";

        static void Main(string[] args)
        {
            Speedometer.Speedometer speedometer = new Speedometer.Speedometer();
            speedometer.Start();
            //TimelineScraper scraper = new TimelineScraper("realDonaldTrump");
            SearchScraper scraper = new SearchScraper("realDonaldTrump", new DateTime(2009, 04, 01), new DateTime(2009, 10, 01));
            Task.Run(async () =>
            {
                try
                {
                    File.WriteAllText($"realDonaldTrump_Tweets.csv", Tweet.GetCsvHeader() + "\n");
                    while (scraper.IsThereMoreItems)
                    {
                        Tweet[] tweets = await scraper.NextAsync();
                        speedometer.Progress += tweets.Length;
                        Console.Title = $"Scraping tweets from @realDonaldTrump | Progress: {speedometer.Progress} tweets | Speed: {speedometer.GetProgressPerHour()} tweets/hour";

                        Console.WriteLine(speedometer.Progress);

                        foreach (Tweet tweet in tweets)
                        {
                            File.AppendAllText($"realDonaldTrump_Tweets.csv", tweet.ToCsv() + "\n");
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
            return;
            /*

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
                Console.Read();*/
        }
    }
}
