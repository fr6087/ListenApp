using ListenToMe.Model;
using Microsoft.Bot.Connector.DirectLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Windows.Data.Json;
using Windows.Foundation;
using Microsoft.Bot.Connector.DirectLine.Models;
using Microsoft.Rest;

namespace VoiceCommandService
{

    /// <summary>
    /// the Bot class establishes connection to LUIS API. It extracts the intent that it recognises behind the user input. For example,
    /// a user input like "what is the stock price of dax?" will return JsonObject:
    /// Response{
 /// "query": "what about dax",
  ///"topScoringIntent": {
   /// "intent": "StockPrice2",
   /// "score": 0.3969668
  ///},
  ///"intents": [
   /// {
   ///   "intent": "StockPrice2",
   ///   "score": 0.3969668
   /// },
    ///{
      ///"intent": "None",
      ///"score": 0.372112036
    ///},
 /// ],
  ///"entities": [
    ///{
      ///"entity": "dax",
      ///"type": "StockSymbol"
   /// }
  ///]
///}
///The HttpClientConnection is from an old state of the App. The App uses now a DirectLine-Connection via http. Therefore it contains
///methods and fields that support DirectLine as well. With the DirectLine Connection it is not only possible to reach the luis model
///but also the bot. This enables to create a more sophisticated bot e.g. containing FormFlow and ccommunicate with it.
    /// </summary>
        public sealed class Bot
        {

        //reference: https://github.com/Psychlist1972/InternetOfStrangerThings
        #region Secret
        const string _directLineSecret = "gwSQkgDM-h4.cwA.huI.LjySX_oD-XTJQDRBkw9l4WBbyZf5rN8cd5qkVXM6qUA";
        #endregion


        // TODO: Change the URL to match your bot
        private const string _botBaseUrl = "https://luisformbot20171129.azurewebsites.net/api/messages";
        
        

        private DirectLineClient _directLine;
        private string _conversationId;
        

        public IAsyncOperation<Rootobject> SendMessageAndGetIntentFromBot(string message)
            {
                return Task.Run<Rootobject>(async () =>
                {

                    string intent = "none";
                    Rootobject myObject = null;
                    try
                    {
                        myObject= await Proxy.GetJSON(message);//toDo return the rootobject (because it also has discovered entities)
                        var topscoringIntent = myObject.topScoringIntent;
                        try{
                            intent = topscoringIntent.intent;
                        }catch(System.NullReferenceException e)
                            {
                            Debug.WriteLine("No topScoringIntent discovered by Bot.cs.SendMessageAndGetIntentFromBot()");
                            }
                        Debug.WriteLine("topScoringIntent" + intent);

                    }
                    catch (Exception e)
                    {
                        // no op
                        Debug.WriteLine(e.Message);
                    }
                    Debug.WriteLine("Bot is returning "+myObject.ToString());
                    return myObject;
                }).AsAsyncOperation();
            }

        //these are methods that have yet to be tested.

        public async Task ConnectAsync()
        {
            Debug.WriteLine("1");
            _directLine = new DirectLineClient(_directLineSecret);
            Debug.WriteLine("2");
            
            var conversation = await _directLine.Conversations.NewConversationWithHttpMessagesAsync();//StartConversationWithHttpMessagesAsync();// NewConversationWithHttpMessagesAsync();
            Debug.WriteLine("3");
            _conversationId = conversation.Body.ConversationId;//.Body.ConversationId;

            System.Diagnostics.Debug.WriteLine("Bot connection set up."+_conversationId);
        }

        private async Task<string> GetResponse()
        {
            try
            {
                var httpMessages = await _directLine.Conversations.GetMessagesWithHttpMessagesAsync(_conversationId);
                var messages = httpMessages.Body.Messages;//Activities;

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

        public async Task<string> TalkToTheBotAsync(String message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Sending bot message");

                Message msg = new Message();//type was in original Message
                msg.Text = message;


                System.Diagnostics.Debug.WriteLine("Posting "+_conversationId);
                try
                {
                    await _directLine.Conversations.PostMessageAsync(_conversationId, msg);
                }
                catch (Microsoft.Rest.HttpOperationException e)
                {
                    Debug.WriteLine(e.Source+e.Message);
                    Debug.WriteLine("");
                    Debug.WriteLine("");
                }
                

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

