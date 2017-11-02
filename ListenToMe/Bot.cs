using ListenToMe.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;

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
            // You will need to create your own API key and enable simple API
            private string key = "a5de48ea62014f2bbdc4ad05943f2081";//"<INSERT YOUR API KEY HERE>";

            // You will need to either use a bot that  has been enabled for the API
            // or create your own bot. I've been using the example bot from the API
            // but be warned, it's very much rated R. The id is 6
            private string botId = "93dfe8db-873d-46aa-9604-d5c22df499ad";//"<INSERT YOUR BOT NUMBER HERE>";

            public IAsyncOperation<string> SendMessageAndGetIntentFromBot(string message)
            {
                return Task.Run<string>(async () =>
                {
                    if (key == "<INSERT YOUR API KEY HERE>" || botId == "<INSERT YOUR BOT NUMBER HERE>")
                    {
                        return "Please update the API key and/or botId in Bot.cs in order to talk to me!";
                    }

                    string intent = "none";

                    try
                    {
                        Rootobject myObject= await Proxy.GetJSON(message);//toDo return the rootobject (because it also has discovered entities)
                        var topscoringIntent = myObject.topScoringIntent;
                        intent = topscoringIntent.intent;
                        Debug.WriteLine("topScoringIntent" + intent);

                    }
                    catch (Exception e)
                    {
                        // no op
                        Debug.WriteLine(e.Message);
                    }

                    return intent;
                }).AsAsyncOperation();
            }
        }
    }

