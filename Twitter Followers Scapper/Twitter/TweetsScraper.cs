﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HtmlAgilityPack;
using CheckingLib;

namespace Twitter
{
    class TweetsScraper : Scraper<Tweet>
    {
        private static int counter = 0;

        private static readonly Step initStep = new Step()
        {
            Url = "https://twitter.com/%TARGET%",
            Method = "GET",
            Headers = new Dictionary<string, string>()
            {
                { "accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8" },
                { "accept-language", "en-US,en;q=0.9" },
                { "referer" , "https://twitter.com/" },
                { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:64.0) Gecko/20100101 Firefox/64.0" },
            }
        };

        private static readonly Step step = new Step()
        {
            Url = "https://twitter.com/i/profiles/show/%TARGET%/timeline/tweets?include_available_features=1&include_entities=1&lang=en&max_position=%MAXPOSITION%&reset_error_state=false",
            Method = "GET",
            Headers = new Dictionary<string, string>()
                {
                    { "accept", "application/json, text/javascript, */*; q=0.01" },
                    { "accept-language", "en-US,en;q=0.9" },
                    { "referer", "https://twitter.com/%TARGET%" },
                    { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:64.0) Gecko/20100101 Firefox/64.0" },
                    { "x-requested-with", "XMLHttpRequest" },
                    { "x-twitter-active-user", "yes" }
                }
        };

        public TweetsScraper(Checker checker, string target)
            : base(checker)
        {
            checker.Variables["TARGET"] = target;
        }

        public override async Task<Tweet[]> NextAsync()
        {
            if (!IsThereMoreItems)
                throw new NoMoreItemsExceptions("There is no more users to scrape.");

            Tweet[] tweets = null;

            // If it's the first request
            HtmlDocument htmlDocument;
            if (Position == null)
            {
                string result = await checker.ExecuteAsync(initStep);
                counter++;
                htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(result);

                string dataMinPosition = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='timeline']/div[contains(@class, 'stream-container')]").GetAttributeValue("data-min-position", "");

                Position = dataMinPosition;
            }
            else
            {
                string result = await checker.ExecuteAsync(step);
                counter++;
                if (result.Contains("Sorry, you are rate limited."))
                {
                    throw new RateLimitedException("Rate limited");
                }
                else
                {
                    Response response = Response.ParseFromJson(result);
                    IsThereMoreItems = response.HasMoreItems;
                    htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(response.ItemsHtml);
                    Position = response.MinPosition;
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

            string timeEpoch = htmlNode.SelectSingleNode(".//small[@class='time']/a/span").GetAttributeValue("data-time", "");
            try
            {
                tweet.Time = DateTimeFromUnixEpoch(int.Parse(timeEpoch));
            }
            catch (Exception e)
            {
                tweet.Time = new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            }

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
