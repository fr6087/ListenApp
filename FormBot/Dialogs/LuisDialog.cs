using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FormBot.Dialogs
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    
    [LuisModel("57c99374-f736-4f77-a952-1a1fe90500be", "499b2f80014047168cf1e56b32fa7d41")]
    [Serializable]
    public partial class LuisDialog : LuisDialog<object>
    {
        public LuisDialog() : base(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["LuisAppId"], ConfigurationManager.AppSettings["LuisAPIKey"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached the none intent. You said: {result.Query}"); //
            context.Wait(MessageReceived);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "MyIntent" with the name of your newly created intent in the following handler
        [LuisIntent("Company.Name")]
        public async Task CompanyName(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached the CompanyName intent. You said: {result.Query}"); //
            context.Wait(MessageReceived);
        }

        [LuisIntent("Company.Email")]
        public async Task CompanyEmail(IDialogContext context, LuisResult result)
        {
            string message = "Email";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Field.Edit")]
        public async Task FieldEdit(IDialogContext context, LuisResult result)
        {
            string message = "Yes, edit field.";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Field.FillIn")]
        public async Task FieldFilledIn(IDialogContext context, LuisResult result)
        {
            string message = "FilledField";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Hilfe")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            string message = "Ich helfe dir ... doch nicht. Gr..ahhhhh";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Page.Back")]
        public async Task PageBack(IDialogContext context, LuisResult result)
        {
            string message = "Run back";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Page.Next")]
        public async Task PageNext(IDialogContext context, LuisResult result)
        {
            string message = "Run forward";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

    }
}