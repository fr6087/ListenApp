using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace WcfService1
{
    public class Alexa : IDisposable
    {
        private string _conversationId;
        private readonly string _from;
        private readonly string _secret;

        public HttpClient Client;
        private string MessagesPath => $"/api/conversations/{_conversationId}/messages";

        public Alexa(string secret, string from)
        {
            _secret = secret;
            _from = from;
            Client = CreateHttpClient().Result;
        }

        private async Task<HttpClient> CreateHttpClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://directline.botframework.com")
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("BotConnector", _secret);
            client.DefaultRequestHeaders.Add("ETag", "*");

            var response =
                await client.GetAsync("/api/tokens/", HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            response =
                await client.PostAsJsonAsync("/api/conversations/", new object())
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var conversationInfo =
                await response.Content.ReadAsAsync<JObject>()
                .ConfigureAwait(false);

            _conversationId = (string)conversationInfo["conversationId"];
            var scopedToken = (string)conversationInfo["token"];

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("BotConnector", scopedToken);

            return client;
        }

        public async Task<IEnumerable<string>> FetchRecentReplies()
        {
            var json =
                await Client.GetStringAsync(MessagesPath)
                .ConfigureAwait(false);

            var messagesContainer = JsonConvert.DeserializeObject<JObject>(json);

            var recentReplies = messagesContainer["messages"]
                .Where(a => (string)a["from"] != _conversationId
                         && (string)a["from"] != _from)
                .Select(m => ((string)m["text"])?.Trim());

            return recentReplies;
        }

        public async Task SendMessage(string outgoingText)
        {
            var message = new
            {
                id = Guid.NewGuid().ToString(),
                created = DateTime.UtcNow,
                from = _from,
                text = outgoingText,
                eTag = "*"
            };

            var response =
                await Client.PostAsJsonAsync(MessagesPath, message)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }


        public void Dispose()
        {
            ((IDisposable)Client).Dispose();
        }
    }
}