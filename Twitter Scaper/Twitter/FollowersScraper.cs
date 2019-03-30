//using HtmlAgilityPack;
//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Threading.Tasks;

//namespace Twitter
//{
//    class FollowersScraper : Scraper<User>
//    {
//        private static readonly Step initStep = new Step()
//        {
//            Url = "https://twitter.com/%TARGET%/followers",
//            Method = "GET",
//            Headers = new Dictionary<string, string>()
//            {
//                { "accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8" },
//                { "accept-language", "en-US,en;q=0.9" },
//                { "referer" , "https://twitter.com/" },
//                { "cookie", "%COOKIE%"},
//                { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:64.0) Gecko/20100101 Firefox/64.0" },
//            }
//        };

//        private static readonly Step step = new Step()
//        {
//            Url = "https://twitter.com/%TARGET%/followers/users?include_available_features=1&include_entities=1&max_position=%MAXPOSITION%&reset_error_state=false",
//            Method = "GET",
//            Headers = new Dictionary<string, string>()
//                {
//                    { "accept", "application/json, text/javascript, */*; q=0.01" },
//                    { "accept-language", "en-US,en;q=0.9" },
//                    { "cookie", "%COOKIE%" },
//                    { "referer", "https://twitter.com/%TARGET%/followers" },
//                    { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:64.0) Gecko/20100101 Firefox/64.0" },
//                    { "x-requested-with", "XMLHttpRequest" },
//                    { "x-twitter-active-user", "yes" }
//                }
//        };

//        public FollowersScraper(Checker checker, string target, string cookie)
//            : base(checker)
//        {
//            checker.Variables["TARGET"] = target;
//            checker.Variables["COOKIE"] = cookie;
//        }

//        public override async Task<User[]> NextAsync()
//        {

//            if (!IsThereMoreItems)
//                throw new NoMoreItemsExceptions("There is no more users to scrape.");

//            User[] users = null;

//            // If it's the first request
//            if (Position == null)
//            {
//                string result = await checker.ExecuteAsync(initStep);
//                HtmlDocument htmlDocument = new HtmlDocument();
//                htmlDocument.LoadHtml(result);
//                HtmlNode gridNode = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'GridTimeline-items')]");
//                HtmlNodeCollection profileNodes = gridNode.SelectNodes(".//div[@class='ProfileCard-userFields']");
//                users = new User[profileNodes.Count];
//                for (int i = 0; i < profileNodes.Count; i++)
//                {
//                    users[i] = GetUser(profileNodes[i]);
//                }
//                string dataMinPosition = gridNode.GetAttributeValue("data-min-position", "");

//                if (dataMinPosition != "")
//                {
//                    Position = dataMinPosition;
//                }
//                else
//                {
//                    throw new Exception("Couldn't find data-min-position value.");
//                }
//            }
//            else
//            {
//                string result = await checker.ExecuteAsync(step);
//                if (result.Contains("Sorry, you are rate limited."))
//                {
//                    throw new RateLimitedException("Rate limited");
//                }
//                else
//                {
//                    Response response = Response.ParseFromJson(result);
//                    IsThereMoreItems = response.HasMoreItems;
//                    HtmlDocument htmlDocument = new HtmlDocument();
//                    htmlDocument.LoadHtml(response.ItemsHtml);
//                    HtmlNodeCollection profileNodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='ProfileCard-userFields']");
//                    users = new User[profileNodes.Count];
//                    for (int i = 0; i < profileNodes.Count; i++)
//                    {
//                        users[i] = GetUser(profileNodes[i]);
//                    }
//                    Position = response.MinPosition;
//                }
//            }

//            if (Position == "0")
//                IsThereMoreItems = false;

//            return users;
//        }

//        // Get a Twitter.User object from a ProfileCard-userFields html element
//        private static User GetUser(HtmlNode htmlNode)
//        {
//            User user = new User
//            {
//                Name = WebUtility.HtmlDecode(htmlNode.SelectSingleNode(".//a[contains(@class,'fullname')]")?.InnerText.Trim()),
//                Bio = htmlNode.SelectSingleNode(".//p[contains(@class, 'ProfileCard-bio')]")?.InnerText.Trim(),
//                Handle = htmlNode.SelectSingleNode(".//span[contains(@class, 'username')]")?.InnerText.Trim()
//            };
//            return user;
//        }

//    }
//}
