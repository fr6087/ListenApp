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
using System.Collections.Generic;
//assembly LuisBot.dll
//namespace for rview tool Method to build Form: ESF2CompanyDetailsForm.BuildForm();
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

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached the none intent AGAIN. You said: {result.Query}"); //
            context.Wait(MessageReceived);
        }

        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            var DataBag = context.UserData;
           
            await context.PostAsync($"{result.Query}. Ich bin dein Assistent beim Ausfüllen von Formularen. Versuche einmal <Ich möchte die Unternehmensangaben" +
                $"im ESF_2 Formular ausfüllen>."); 
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

        [LuisIntent("Utilities.GoBack")]
        public async Task GoBackIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached the goBack intent. You said: {result.Query}"); //
            context.Wait(MessageReceived);
        }

        [LuisIntent("Utilities.Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.Text = "Klar helfe ich dir.";
            reply.Speak = "This is the text that will be spoken. In der Hilfe.";
            // reply.InputHint = InputHints.AcceptingInput;
            await context.PostAsync(reply);

            context.Call(new HelpDialog(), AfterDialog);
        }
 
        [LuisIntent("Upload")]
        public async Task UploadIntent(IDialogContext context, LuisResult result)
        {
           /* var reply = context.MakeMessage();
            reply.Text = Resource1.UploadOptions;
            reply.Speak = reply.Text;

            await context.PostAsync(reply);*/

            context.Call(new UploadDialog(), AfterDialog);
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


            // See if user answered yes

            if (message.Text.ToLower().Equals(Resource1.Yes))

            {
                //wenn du von help zurückkommst
                //wenn du von unternhemensangaben zurückkommst
                await context.PostAsync(Resource1.Saved);
            }
            else
            {

                context.Call(this, AfterDialog);
                
            }

        }

        /*Diese 2 Methoden sind toter Code, die Implementierung eines SignIn-Prozesses ist umfangreicher und muss mit OAuth abgesichert werden. Siehe auch
         https://stackoverflow.com/questions/39080683/bot-framework-sign-in-card-how-get-auth-result*/
        public async Task DisplaySignInCard(IDialogContext context, IAwaitable<string> result)
        {
            var selectedCard = await result;

            var message = context.MakeMessage();

            var attachment = GetSigninCard();
            message.Attachments.Add(attachment);

            await context.PostAsync(message);

            context.Wait(this.MessageReceivedAsync);
        }
        private static Attachment GetSigninCard()
        {
            var signinCard = new SigninCard
            {
                Text = "BotFramework Sign-in Card",
                Buttons = new List<CardAction> { new CardAction(ActionTypes.Signin, "Sign-in", value: "http://10.150.50.21/irj/portal/") }//https://login.microsoft-online.de
            };

            return signinCard.ToAttachment();
        }

        public async Task AfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            var confirm = await result;

            
            await context.PostAsync(Resource1.GotHelpedQuestion);

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
            [Prompt("Straße?")]
            public String Straße { get; set; }
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

            /*this code is buggy*/
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
                            .Confirm("Möchtest du deine Daten {Vorname} {Nachname}, arbeitet bei {Unternahmensname} mit der Telefonnummer {Telefon} und der Emailadresse {Email} abspeichern?")
                            .AddRemainingFields()
                            .Message("Danke. Ich habe dich gerne beim Formularausfüllen unterstützt!")
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