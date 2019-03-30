using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Twitter
{
    class SearchScraper : Scraper<Tweet>
    {
        private static int counter = 0;
        private readonly HttpClientHandler _httpClientHandler;
        private readonly HttpClient _httpClient;
        string _target;
        DateTime _start;
        DateTime _end;

        public static List<string> _userAgents = new List<string>();


        //private static readonly Step step = new Step()
        //{
        //};

        public SearchScraper(string target, DateTime start, DateTime end)
        {

            _userAgents.Add("Mozilla/5.0 (Windows; U; Windows NT 6.1; x64; fr; rv:1.9.2.13) Gecko/20101203 Firebird/3.6.13");
            _userAgents.Add("Mozilla/5.0 (compatible, MSIE 11, Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");
            _userAgents.Add("Mozilla/5.0 (Windows; U; Windows NT 6.1; rv:2.2) Gecko/20110201");
            _userAgents.Add("Opera/9.80 (X11; Linux i686; Ubuntu/14.10) Presto/2.12.388 Version/12.16");
            _userAgents.Add("Mozilla/5.0 (Windows NT 5.2; RW; rv:7.0a1) Gecko/20091211 SeaMonkey/9.23a1pre");
            _userAgents.Add("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:64.0) Gecko/20100101 Firefox/64.0");




            _httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                UseCookies = true,
            };
            _httpClient = new HttpClient(_httpClientHandler, disposeHandler: false);
            _target = target;
            _start = start;
            _end = end;
        }

        public override async Task<Tweet[]> NextAsync()
        {
            Random rand = new Random();
            var r = rand.Next(5);
            Tweet[] tweets = null;

            // If it's the first request
            HtmlDocument htmlDocument = null;
            if (Position == null)
            {

                string url = "https://twitter.com/search?l=&q=from%3A" + _target + "%20since%3A" + _start.ToString("yyyy-MM-dd") + "%20until%3A" + _end.ToString("yyyy-MM-dd") + "&src=typd";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                var Headers = new Dictionary<string, string>()
            {
                { "accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8" },
                { "accept-language", "en-US,en;q=0.9" },
                { "referer" , "https://twitter.com/search-advanced/" },
                { "user-agent", _userAgents[r]},
            };
                foreach (var item in Headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }

                Console.WriteLine(url);

                var response = await _httpClient.SendAsync(request);

                var result = await response.Content.ReadAsStringAsync();
                counter++;
                htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(result);

                string dataMinPosition = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='timeline']/div[contains(@class, 'stream-container')]").GetAttributeValue("data-min-position", "");

                Position = dataMinPosition;
            }
            else
            {


                string url = "https://twitter.com/i/search/timeline?vertical=default&q=from%3A" + _target + "%20since%3A" + _start.ToString("yyyy-MM-dd") + "%20until%3A" + _end.ToString("yyyy-MM-dd") + "&src=typd&include_available_features=1&include_entities=1&lang=en&max_position=" + Position + "&reset_error_state=false";


                var Headers = new Dictionary<string, string>()
                        {
                            { "accept", "application/json, text/javascript, */*; q=0.01" },
                            { "accept-language", "en-US,en;q=0.9" },
                            { "referer", "https://twitter.com/search?l=&q=from%3A"+_target+"%20since%3A"+ _start.ToString("yyyy-MM-dd") + "%20until%3A"+ _end.ToString("yyyy-MM-dd") + "&src=typd&lang=en" },
                            { "user-agent", _userAgents[r]},
                            { "x-requested-with", "XMLHttpRequest" },
                            { "x-twitter-active-user", "yes" }
                        };

                Console.WriteLine(url);

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                foreach (var item in Headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }

                Console.WriteLine(url);

                var response = await _httpClient.SendAsync(request);

                string result = await response.Content.ReadAsStringAsync();

                counter++;
                if (result.Contains("Sorry, you are rate limited."))
                {
                    throw new RateLimitedException("Rate limited");
                }
                else
                {
                    Response responses = Response.ParseFromJson(result);
                    IsThereMoreItems = responses.HasMoreItems;
                    htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(responses.ItemsHtml);
                    Position = responses.MinPosition;
                }
            }

            HtmlNodeCollection tweetsNodes = htmlDocument.DocumentNode.SelectNodes("//li[@data-item-type='tweet']");
            if (tweetsNodes == null)
            {
                tweets = new Tweet[0];
                IsThereMoreItems = false;
            }
            else
            {
                tweets = new Tweet[tweetsNodes.Count];
                for (int i = 0; i < tweetsNodes.Count; i++)
                {
                    tweets[i] = GetTweet(tweetsNodes[i]);
                }

                if (Position == "0")
                    IsThereMoreItems = false;
            }



            return tweets;
        }

        // Get a Twitter.User object from a li.js-stream-item.stream-item.stream-item html element
        private static Tweet GetTweet(HtmlNode htmlNode)
        {
            Tweet tweet = new Tweet
            {
                Name = htmlNode.SelectSingleNode(".//strong[contains(@class,'fullname')]")?.InnerText.Trim(),
                Handle = htmlNode.SelectSingleNode(".//span[contains(@class, 'username')]")?.InnerText.Trim(),
                Text = htmlNode.SelectSingleNode(".//div[@class='js-tweet-text-container']").InnerText.Trim()
            };
            // original-tweet 
            string timeEpoch = htmlNode.SelectSingleNode(".//small[@class='time']/a/span").GetAttributeValue("data-time", "");
            try
            {
                tweet.Time = DateTimeFromUnixEpoch(int.Parse(timeEpoch));
            }
            catch (Exception e)
            {
                tweet.Time = new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            }

            string tweetId = htmlNode.SelectSingleNode(".//div[contains(@class,' original-tweet ')]")?.GetAttributeValue("data-tweet-id", "");
            tweet.Id = tweetId;

            string likesCount = htmlNode.SelectSingleNode(".//span[contains(@class, 'ProfileTweet-action--favorite')]/span[@class='ProfileTweet-actionCount']").GetAttributeValue("data-tweet-stat-count", "");
            try
            {
                tweet.Likes = int.Parse(likesCount);
            }
            catch (Exception)
            {
                tweet.Likes = -1;
            }

            string retweetsCount = htmlNode.SelectSingleNode(".//span[contains(@class, 'ProfileTweet-action--retweet')]/span[@class='ProfileTweet-actionCount']").GetAttributeValue("data-tweet-stat-count", "");
            try
            {
                tweet.Retweets = int.Parse(retweetsCount);
            }
            catch (Exception)
            {
                tweet.Retweets = -1;
            }

            string repliesCount = htmlNode.SelectSingleNode(".//span[contains(@class, 'ProfileTweet-action--reply')]/span[@class='ProfileTweet-actionCount']").GetAttributeValue("data-tweet-stat-count", "");
            try
            {
                tweet.Replies = int.Parse(repliesCount);
            }
            catch (Exception)
            {
                tweet.Replies = -1;
            }

            return tweet;
        }

        private static DateTime DateTimeFromUnixEpoch(int Unix)
        {
            DateTime time = new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            time = time.AddSeconds(Unix).ToLocalTime();
            return time;
        }
    }
}
