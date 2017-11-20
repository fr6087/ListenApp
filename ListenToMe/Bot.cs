using ListenToMe.Model;
using Microsoft.Bot.Connector.DirectLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
//using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Microsoft.Bot.Connector.DirectLine.Models;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

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
    /// </summary>
        public sealed class Bot
        {

        //reference: https://github.com/Psychlist1972/InternetOfStrangerThings
        #region Secret
        const string _directLineSecret = "gwSQkgDM-h4.cwA.huI.LjySX_oD-XTJQDRBkw9l4WBbyZf5rN8cd5qkVXM6qUA";
        #endregion


        // TODO: Change the URL to match your bot
        private const string _botBaseUrl = "https://luisformbot.azurewebsites.net/api/messages";

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
            _directLine = new DirectLineClient(_directLineSecret);
            
            var conversation = await _directLine.Conversations.NewConversationWithHttpMessagesAsync();
            _conversationId = conversation.Body.ConversationId;

            System.Diagnostics.Debug.WriteLine("Bot connection set up.");
        }

        private async Task<string> GetResponse()
        {
            try
            {
                var httpMessages = await _directLine.Conversations.GetMessagesWithHttpMessagesAsync(_conversationId);
                var messages = httpMessages.Body.Messages;

                // our bot only returns a single message, so we won't loop through
                // First message is the question, second message is the response
                if (messages?.Count > 1)
                {
                    // select latest message -- the response
                    var text = messages[messages.Count()-1].Text;
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


        public async Task<string> TalkToTheUpsideDownAsync(string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Sending bot message");

                var msg = new Message();
                msg.Text = message;


                System.Diagnostics.Debug.WriteLine("Posting");

                await _directLine.Conversations.PostMessageAsync(_conversationId, msg);

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

