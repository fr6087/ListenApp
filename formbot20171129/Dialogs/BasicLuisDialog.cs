using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using LuisBot.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Resources;
using LuisBot;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO;
using Microsoft.Bot.Builder.FormFlow.Json;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        private Func<IForm<ESF2CompanyDetailsForm>> buildForm;

        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["LuisAppId"], ConfigurationManager.AppSettings["LuisAPIKey"])))
        {
           
        }
        /*
        public BasicLuisDialog(Func<IForm<FeedbackForm>> buildForm)
        {
            this.buildForm = buildForm;
        }*/

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached the none intent AGAIN. You said: {result.Query}"); //
            context.Wait(MessageReceived);
        }

        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"{result.Query}. Ich bin dein Assistent beim Ausf�llen von Formularen. Versuche einmal <Ich m�chte die Unternehmensangaben" +
                $"im ESF_2 Formular ausf�llen>."); 
            context.Wait(MessageReceived);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        [LuisIntent("Field.FillIn")]
        public async Task FillInIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached the fillin Intent intent. You said: {result.Query}"); //
            var feedbackForm = new FormDialog<ESF2CompanyDetailsForm>(new ESF2CompanyDetailsForm(), ESF2CompanyDetailsForm.BuildForm, FormOptions.PromptInStart, result.Entities);
            context.Call(feedbackForm, FeedbackFormComplete);
            //context.Wait(MessageReceived);
        }

        [LuisIntent("ESF2.Unternehmensangaben")]
        public async Task CompanyDetailsIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached the companydetails intent.");
            IMessageActivity message = Activity.CreateMessageActivity();
            message.Text = result.TopScoringIntent.Intent;
            message.TextFormat = "plain";
            message.Locale = System.Globalization.CultureInfo.CurrentCulture.Name;
            await Conversation.SendAsync(message, MessagesController.MakeRootDialog);
            //var feedbackForm = new FormDialog<ESF2CompanyDetailsForm>(new ESF2CompanyDetailsForm(), ESF2CompanyDetailsForm.BuildLocalizedForm, FormOptions.PromptInStart, result.Entities);
            //context.Call(feedbackForm, FeedbackFormComplete);
        }

        private async  Task FeedbackFormComplete(IDialogContext context, IAwaitable<ESF2CompanyDetailsForm> result)
        {
            var confirm = await result;

            await context.PostAsync(Resource1.Save);

            context.Wait(MessageReceivedAsync);

        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {


            // Get the text passed

            var message = await argument;


            // See if a number was passed

            if (message.Text.ToLower().Equals("ja"))

            {
                await context.PostAsync(Resource1.Saved);
            }
            else
            {
                await context.PostAsync("Ok.");
            }

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

        private async Task AfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            var confirm = await result;


            await context.PostAsync("Got helped?");

            context.Wait(MessageReceivedAsync);
        }
        // Cache of culture specific forms. reference https://github.com/Microsoft/BotBuilder/tree/master/CSharp/Samples/AnnotatedSandwichBot
        private static ConcurrentDictionary<CultureInfo, IForm<ESF2CompanyDetailsForm>> _forms = new ConcurrentDictionary<CultureInfo, IForm<ESF2CompanyDetailsForm>>();

        [Serializable]
        [Template(TemplateUsage.NotUnderstood, "Dies habe ich nicht verstanden: \"{0}\".", "Nochmal versuchen, ich verstehe \"{0}\" nicht.")]
       // [Template(TemplateUsage.EnumSelectOne, "What kind of {&} would you like on your sandwich? {||}")]
        public class ESF2CompanyDetailsForm
        {
            [Prompt("Was ist dein {&}?")]
            public string Vorname { get; set; }

            [Prompt("Und dein {&}?")]
            public string Nachname { get; set; }

            [Prompt("Nenne deinen {&}.")]
            public string Unternehmensname { get; set; }
            [Prompt("Stra�e?")]
            public String Stra�e { get; set; }
            [Prompt("Hausnummer?")]
            public String Hausnummer { get; set; }
            [Prompt("Telefonnummer?")]
            [Pattern(@"(\(\d{3}\))?\s*\d{3}(-|\s*)\d{4}")]
            public string Telefon { get; set; }

            [Prompt("Emailadresse")]
            [Pattern(@"^[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$")]
            public string Email { get; set; }

            [Prompt("Firmen-URL")] //[Optional] nur bei enum sinnvoll?
            public string URL { get; set; }

            public static IForm<ESF2CompanyDetailsForm> BuildForm()
            {
                return new FormBuilder<ESF2CompanyDetailsForm>()
                    .Message("Willkommen im ESF_2 Bot.")
                    .Build();
            }

            public static IForm<ESF2CompanyDetailsForm> BuildLocalizedForm()
            {
                var culture = Thread.CurrentThread.CurrentUICulture;
                IForm<ESF2CompanyDetailsForm> form;
                if (!_forms.TryGetValue(culture, out form))
                {
                    OnCompletionAsyncDelegate<ESF2CompanyDetailsForm> processOrder = async (context, state) =>
                    {
                        await context.PostAsync(Resource1.Processing);//vormals Properties.Processing
                    };
                    // Form builder uses the thread culture to automatically switch framework strings
                    // and also your static strings as well.  Dynamically defined fields must do their own localization.
                    var builder = new FormBuilder<ESF2CompanyDetailsForm>()
                            .Message("Welcome to the sandwich order bot!")
                            .Field(nameof(Vorname))
                            .Field(nameof(Nachname))
                            .Field(nameof(Unternehmensname))
                            .Field(nameof(Telefon))
                            .Field(nameof(Email))
                            .Field(nameof(URL))
                            .Confirm("M�chtest du deine Daten {Vorname} {Nachname}, arbeitet bei {Unternahmensname} mit der Telefonnummer {Telefon} und der Emailadresse {Email} abspeichern?")
                            .AddRemainingFields()
                            .Message("Danke. Ich habe dich gerne beim Formularausf�llen unterst�tzt!")
                            .OnCompletion(processOrder);
                    builder.Configuration.DefaultPrompt.ChoiceStyle = ChoiceStyleOptions.Auto;
                    form = builder.Build();
                    _forms[culture] = form;
                }
                return form;
            }

            public static IForm<JObject> BuildJsonForm()
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.Bot.Sample.AnnotatedSandwichBot.AnnotatedSandwich.json"))
                {
                    var schema = JObject.Parse(new StreamReader(stream).ReadToEnd());
                    return new FormBuilderJson(schema)
                        .AddRemainingFields()
                        .Build();
                }
            }
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