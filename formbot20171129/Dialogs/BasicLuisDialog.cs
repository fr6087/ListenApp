using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using LuisBot.Dialogs;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["LuisAppId"], ConfigurationManager.AppSettings["LuisAPIKey"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached the none intent AGAIN. You said: {result.Query}"); //
            context.Wait(MessageReceived);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        [LuisIntent("Field.FillIn")]
        public async Task FillInIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached the fillin Intent intent. You said: {result.Query}"); //
            context.Wait(MessageReceived);
        }

        [LuisIntent("Utilities.GoBack")]
        public async Task GoBackIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached the goBack intent. You said: {result.Query}"); //
            context.Wait(MessageReceived);
        }
        [LuisIntent("Utilities.Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            /* Activity reply = activity.CreateReply("This is the text that will be displayed.");
             reply.Speak = "This is the text that will be spoken.";
             reply.InputHint = InputHints.AcceptingInput;
             await connector.Conversations.ReplyToActivityAsync(reply);*/
            
            await context.PostAsync($"You have reached the help intent. You said: {result.Query}"); //
            
            context.Wait(MessageReceived);
            context.Call(new HelpDialog(), AfterDialog);
        }

        private static async Task AfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Done<object>(null);
        }

        /*This snippet is for asking Cortana user related infos
         
         if (activity.Entities != null)
{
    var userInfo = activity.Entities.FirstOrDefault(e => e.Type.Equals("UserInfo"));
    if(userInfo != null)
    {
        var email = userInfo.Properties.Value<string>("UserEmail");

        if (!string.IsNullOrEmpty(email))
        {
            //Do something with the user's email address.
        }

        var currentLocation = userInfo.Properties["CurrentLocation"];

        if (currentLocation != null)
        {
            var hub = currentLocation["Hub"];

            //Access the latitude and longitude values of the user's location.
            var lat = hub.Value<double>("Latitude");
            var lon = hub.Value<double>("Longitude");

            //Do something with the user's location information.
        }
    }
}
         */

    }
}