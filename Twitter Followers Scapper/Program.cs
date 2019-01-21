using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Neos07.Checking;
using HtmlAgilityPack;
using CsvLibrary;

namespace Twitter_Followers_Scapper
{
    class Program
    {
        // 5400 followers per hour or 300 requests* 18 followers per hour
        private static readonly string target = "oldschoolrs";
      //  private static readonly string cookie = "personalization_id=\"v1_Gs1KfvQ14y+RvX7dtPO1Eg==\"; guest_id=v1%3A154807100735741768; _twitter_sess=BAh7CSIKZmxhc2hJQzonQWN0aW9uQ29udHJvbGxlcjo6Rmxhc2g6OkZsYXNo%250ASGFzaHsABjoKQHVzZWR7ADoPY3JlYXRlZF9hdGwrCBn2OHBoAToMY3NyZl9p%250AZCIlYWQwZDNhMjEzODAzODJiZmE0ODg0MDhmMTI5YTFkZTU6B2lkIiUxNjZm%250ANGNlMTU5ZTMzYzEzZmY2MmJmYjMzZjZmODUzYg%253D%253D--aa9a02790ae8bdd58aeab80efd11c96f23350be0; ct0=1f6b8b69d7c0643e7e816bdc44a8b173; eu_cn=1; gt=1087314825313767424; kdt=05iDT7vk8OomEgkNrMko3rlUVxVDkuiW59OUCiCJ; auth_token=7fd122e536358e65fcae1c52994b54046936791b; dnt=1; csrf_same_site_set=1; lang=en; csrf_same_site=1";
         private static readonly string cookie = "lang=en; dnt=1; kdt=aUEAlOQTAXy0sJ8gTtY7j9oVKCBDBMbc5uYhPyC2; _twitter_sess=BAh7CiIKZmxhc2hJQzonQWN0aW9uQ29udHJvbGxlcjo6Rmxhc2g6OkZsYXNo%250ASGFzaHsABjoKQHVzZWR7ADoPY3JlYXRlZF9hdGwrCF3I16dmAToMY3NyZl9p%250AZCIlMThmY2M1NDg2MzliMGY2NzM0OTI5YjUzY2M0NDkxZmE6B2lkIiVlNGVk%250AMzcxOWFlOGRhM2U5MjQ1YWI2ODczZmM4OTJlZjoJdXNlcmwrB%252FD%252BPGY%253D--14f13e1e7181e4909c33eb192ab1a0879c0730b3; csrf_same_site_set=1; csrf_same_site=1; eu_cn=1; remember_checked_on=0; ct0=6de96db60c934dcd2793f93bd2d92de0; personalization_id=\"v1_50/3NkoSko7PdP9ay1du4Q==\"; guest_id=v1%3A154807411255084268; gt=1087327763663601665; auth_token=56be492d20efcabc13309d868fb7c9921de5a0ee";
        private static int retries = 0;

        private static Step initStep = new Step()
        {
            Url = $"https://twitter.com/{target}/followers",
            Method = "GET",
            Headers = new Dictionary<string, string>()
            {
                {"accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8"},
                { "accept-language", "en-US,en;q=0.9" },
                {"referer" , "https://twitter.com/"},
                { "cookie", cookie},
                { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:64.0) Gecko/20100101 Firefox/64.0" },
            }
        };

        private static Step step = new Step()
        {
            Url = $"https://twitter.com/{target}/followers/users?include_available_features=1&include_entities=1&max_position=%MAXPOSITION%&reset_error_state=false",
            Method = "GET",
            Headers = new Dictionary<string, string>()
                {
                    { "accept", "application/json, text/javascript, */*; q=0.01" },
                    { "accept-language", "en-US,en;q=0.9" },
                    { "cookie", cookie },
                    { "referer", $"https://twitter.com/{target}/followers" },
                    { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:64.0) Gecko/20100101 Firefox/64.0" },
                    { "x-requested-with", "XMLHttpRequest" },
                    { "x-twitter-active-user", "yes" }
                }
        };

        static void Main(string[] args)
        {
            Checker checker = new Checker();
            Speedometer speedometer = new Speedometer();
            speedometer.Start();
            Task.Run(async () =>
            {
                if (!File.Exists($"{target}_lastminpos.txt"))
                {
                    try
                    {
                        string result = await checker.ExecuteAsync(initStep);
                        HtmlDocument htmlDocument = new HtmlDocument();
                        htmlDocument.LoadHtml(result);
                        HtmlNode gridNode = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'GridTimeline-items')]");
                        HtmlNodeCollection profileNodes = gridNode.SelectNodes(".//div[@class='ProfileCard-userFields']");
                        foreach (HtmlNode node in profileNodes)
                        {
                            Twitter.User user = GetTwitterUser(node);
                            speedometer.Progress++;
                            Console.Title = $"Progress: {speedometer.Progress} users | Speed: {speedometer.GetProgressPerHour()} users/hour | Retries: {retries}";
                            File.AppendAllText($"{target}.csv", user.ToCsv() + "\n");
                        }
                        string dataMinPosition = gridNode.GetAttributeValue("data-min-position", "");

                        if (dataMinPosition != "")
                        {
                            checker.Variables["MAXPOSITION"] = dataMinPosition;
                        }
                        else
                        {
                            throw new Exception("Couldn't find data-min-position value.");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Problem at the initStep.");
                        Console.WriteLine(e.ToString());
                        return;
                    }
                }
                else
                {
                    checker.Variables["MAXPOSITION"] = File.ReadAllText($"{target}_lastminpos.txt");
                }

                Console.WriteLine(checker.Variables["MAXPOSITION"]);
                try
                {
                    bool flag = true;
                    while (flag)
                    {
                        string result = await checker.ExecuteAsync(step);
                        if (result.Contains("Sorry, you are rate limited."))
                        {
                            // throw new Twitter.RateLimitedException("Rate limited");
                            File.WriteAllText($"{target}_lastminpos.txt", checker.Variables["MAXPOSITION"]);
                            Console.Title = $"Progress: {speedometer.Progress} users | Speed: {speedometer.GetProgressPerHour()} users/hour | Retries: {retries}";
                            Task.Delay(60 * 1000).Wait();
                            retries++;
                            continue;
                        }
                        try
                        {
                            Twitter.Response response = Twitter.Response.ParseFromJson(result);
                            flag = response.HasMoreItems;
                            HtmlDocument htmlDocument = new HtmlDocument();
                            htmlDocument.LoadHtml(response.ItemsHtml);
                            HtmlNodeCollection profileNodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='ProfileCard-userFields']");
                            foreach (HtmlNode node in profileNodes)
                            {
                                Twitter.User user = GetTwitterUser(node);
                                speedometer.Progress++;
                                Console.Title = $"Progress: {speedometer.Progress} users | Speed: {speedometer.GetProgressPerHour()} users/hour | Retries: {retries}";
                                File.AppendAllText($"{target}.csv", user.ToCsv() + "\n");
                            }
                            checker.Variables["MAXPOSITION"] = response.MinPosition;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            Console.WriteLine("Source: ");
                            Console.WriteLine(result);
                            File.WriteAllText($"{target}_lastminpos.txt", checker.Variables["MAXPOSITION"]);
                            Console.Title = $"Progress: {speedometer.Progress} users | Speed: {speedometer.GetProgressPerHour()} users/hour | Retries: {retries}";
                            Task.Delay(60 * 1000).Wait();
                            retries++;
                        }
                    }
                    Console.WriteLine("DONNNE!!");

                }
                /* catch(Twitter.RateLimitedException)
                 {
                     Console.WriteLine("Rate limited!");
                     File.WriteAllText($"{target}_lastminpos.txt", checker.Variables["MAXPOSITION"]);
                 }*/
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    File.WriteAllText($"{target}_lastminpos.txt", checker.Variables["MAXPOSITION"]);
                }
            });

            while (true)
                Console.Read();

        }

        // Get a Twitter.User object from a ProfileCard-userFields html element
        private static Twitter.User GetTwitterUser(HtmlNode htmlNode)
        {
            Twitter.User user = new Twitter.User();
            user.Name = WebUtility.HtmlDecode(htmlNode.SelectSingleNode(".//a[contains(@class,'fullname')]")?.InnerText.Trim());
            user.Bio = htmlNode.SelectSingleNode(".//p[contains(@class, 'ProfileCard-bio')]")?.InnerText.Trim();
            user.Handle = htmlNode.SelectSingleNode(".//span[contains(@class, 'username')]")?.InnerText.Trim();
            return user;
        }

        private static string LogTime()
        {
            DateTime time = DateTime.Now;
            return time.ToString("MM/dd/yy h:mm:ss tt");
        }
    }
}
