using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

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
        // Finally replace "MyIntent" with the name of your newly created intent in the following handler
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
            await context.PostAsync($"You have reached the help intent. You said: {result.Query}"); //
            context.Wait(MessageReceived);
        }
    }
}