using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace Neos07.Checking
{
    class Checker
    {
        private readonly HttpClientHandler ClientHandler;
        private readonly HttpClient Client;
        public readonly Variables Variables;

        public IWebProxy Proxy
        {
            get
            {
                return ClientHandler.Proxy;
            }
            set
            {
                ClientHandler.UseProxy = value != null;
                ClientHandler.Proxy = value;
            }
        }

        private CookieContainer CookieContainer
        {
            get
            {
                return ClientHandler.CookieContainer;
            }
            set
            {
                ClientHandler.CookieContainer = value;
            }
        }

        public Checker()
        {
            ClientHandler = new HttpClientHandler();
            Client = new HttpClient(ClientHandler);

            ClientHandler.UseDefaultCredentials = true;
            ClientHandler.UseCookies = true;
            ClientHandler.CookieContainer = new CookieContainer();
            ClientHandler.AllowAutoRedirect = false;

            Variables = new Variables();
        }

        public async Task<String> ExecuteAsync(Step Step)
        {
            var Message = new HttpRequestMessage()
            {
                Method = new HttpMethod(Step.Method),
                RequestUri = new Uri(Variables.Transform(Step.Url)),
                Content = new StringContent(Variables.Transform(Step.PostData ?? ""))
            };
            
            foreach (KeyValuePair<String, String> header in Step.Headers)
            {
                Message.Headers.Add(header.Key, Variables.Transform(header.Value));
            }

            if (!String.IsNullOrEmpty(Step.ContentType))
                Message.Content.Headers.ContentType = new MediaTypeHeaderValue(Step.ContentType);
            
            using (HttpResponseMessage response = await Client.SendAsync(Message))
            using (HttpContent content = response.Content)
            {
                string result = await content.ReadAsStringAsync();
                return result;
            }
        }

        ~Checker()
        {
            Client.Dispose();
            ClientHandler.Dispose();
        }
    }
}
