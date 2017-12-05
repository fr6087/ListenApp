
using Microsoft.Bot.Connector.DirectLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Rest;
using System.Configuration;

namespace WcfService1
{
    public class CortanaDirectLineClient
    {
        // TODO: Change the URL to match your bot
   //     private const string _botBaseUrl = "https://luisformbot.azurewebsites.net/api/messages";
   

        private DirectLineClient _directLine;
        private string _conversationId;
        private string _directLineSecret = ConfigurationManager.AppSettings["DirectLineSecret"];
        private string botId= ConfigurationManager.AppSettings["BotId"];



        //these are methods that work except for herocards reference Microsoft Botbuidler-Samples on Github

       /* public async Task ConnectAsync()
        {


            DirectLineClient client = new DirectLineClient(directLineSecret);

            var conversation = await client.Conversations.StartConversationAsync();

            new System.Threading.Thread(async () => await ReadBotMessagesAsync(client, conversation.ConversationId)).Start();
            
            _conversationId = conversation.ConversationId; //there was a body inbetween


            System.Diagnostics.Debug.WriteLine("Bot connection set up.");
        }*/
        public async Task ConnectAsync()
        {
            _directLine = new DirectLineClient(_directLineSecret);

            HttpOperationResponse<Conversation> conversation = await _directLine.Conversations.StartConversationWithHttpMessagesAsync();// NewConversationWithHttpMessagesAsync();
            _conversationId = conversation.Body.ConversationId;

            System.Diagnostics.Debug.WriteLine("Bot connection set up.");
        }

        private async Task<string> GetResponse()
        {
            try
            {
                var httpMessages = await _directLine.Conversations.GetActivitiesWithHttpMessagesAsync(_conversationId);
                var messages = httpMessages.Body.Activities;

                // our bot only returns a single message, so we won't loop through
                // First message is the question, second message is the response
                if (messages?.Count > 1)
                {
                    // select latest message -- the response
                    var text = messages[messages.Count - 1].Text;
                    System.Diagnostics.Debug.WriteLine("Response from bot was: " + text);

                    return text;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Response from bot was empty.");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                throw;
            }

        }
        /*
        private async Task ReadBotMessagesAsync(DirectLineClient client, string conversationId)
        {
            string watermark = null;

            while (true)
            {
                var activitySet = await client.Conversations.GetActivitiesAsync(conversationId, watermark);
                watermark = activitySet?.Watermark;

                var activities = from x in activitySet.Activities
                                 where x.From.Id == botId
                                 select x;

                foreach (Activity activity in activities)
                {
                    Console.WriteLine(activity.Text);

                    if (activity.Attachments != null)
                    {
                        foreach (Attachment attachment in activity.Attachments)
                        {
                            switch (attachment.ContentType)
                            {
                                case "application/vnd.microsoft.card.hero":
                                    RenderHeroCard(attachment);
                                    break;

                                case "image/png":
                                    Console.WriteLine($"Opening the requested image '{attachment.ContentUrl}'");

                                    Process.Start(attachment.ContentUrl);
                                    break;
                            }
                        }
                    }

                    Console.Write("Command> ");
                }

                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        }

        private void RenderHeroCard(Attachment attachment)
        {
            
        }*/



        public async Task<string> TalkToTheBotAsync(string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Sending bot message");

                Activity msg = new Activity();//type was in original Message
                msg.Text = message;


                System.Diagnostics.Debug.WriteLine("Posting");

                await _directLine.Conversations.PostActivityAsync(_conversationId, msg);

                System.Diagnostics.Debug.WriteLine("Post complete");

                return await GetResponse();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                throw;
            }
        }


    }
}
