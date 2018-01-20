using System;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow.Json;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Collections;
using LuisBot.Resource;

namespace LuisBot.Dialogs
{
    
    //somehow this tool looks in the wrong folder. But I am missing 30 reputation to add a comment to that discussion below to ask about that.
    //assembly LuisBot.dll -> but rview doesn't find that assembly reference: https://stackoverflow.com/questions/46199439/how-to-use-rview-tool-in-bot-builder-c-sdk-to-localize-strings-in-form-flow
    //namespace for rview tool Method to build Form: ESF2CompanyDetailsForm.BuildForm();
    
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
                //await Conversation.SendAsync(message, MessagesController.MakeRootDialog);//this exeption: objektverweis ohne instanza
                var feedbackForm = new FormDialog<ESF2CompanyDetailsForm>(new ESF2CompanyDetailsForm(), ESF2CompanyDetailsForm.BuildLocalizedForm, FormOptions.PromptInStart, result.Entities);
                context.Call(feedbackForm, FeedbackFormComplete);
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

            private async Task FeedbackFormComplete(IDialogContext context, IAwaitable<ESF2CompanyDetailsForm> result)
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


                [Prompt(" Was ist dein {&}?")]
                public string Vorname { get; set; }

                [Prompt("Und dein {&}?")]
                public string Nachname { get; set; }

                [Prompt("Nenne deinen {&}.")]
                public string Unternehmensname { get; set; }
                [Prompt("Straße?")]
                public String Straße { get; set; }

                //toDo: Ask Adrian about Regex Hausnummer
                [Prompt("Hausnummer?")]
                public String Hausnummer { get; set; }
                [Prompt("Telefonnummer?")]
                [Pattern(@"(\(\d{3}\))?\s*\d{3}(-|\s*)\d{4}")]
                public string Telefon { get; set; }

                [Prompt("Emailadresse")]
                [Pattern(@"^[a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$")]
                public string Email { get; set; }

                [Pattern(@"[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)")]
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
                            //get the string from the ressource file that shows you are processing the form
                            await context.PostAsync(Resource1.Processing);//vormals Properties.Processing 
                        };
                        // Form builder uses the thread culture to automatically switch framework strings
                        // and also your static strings as well.  Dynamically defined fields must do their own localization.
                        var builder = new FormBuilder<ESF2CompanyDetailsForm>()
                                .Message(Resource1.FormWelcomeMessage)
                                .Field(nameof(Vorname))
                                .Field(nameof(Nachname))
                                .Field(nameof(Unternehmensname))
                                .Field(nameof(Telefon))
                                .Field(nameof(Email))
                                .Field(nameof(URL))
                                .Confirm(Resource1.AskForSave + "{Vorname}, {Nachname}, {Unternehmensname}, {Telefon}, {Email}, {URL} ?", null, new Confirmations())//is this your selection\n{*}?
                                .AddRemainingFields()
                                .Message(Resource1.Thanks)
                                .OnCompletion(processOrder);
                        builder.Configuration.DefaultPrompt.ChoiceStyle = ChoiceStyleOptions.Auto;
                        form = builder.Build();
                        _forms[culture] = form;
                    }
                    return form;
                }

                public static IForm<JObject> BuildJsonForm()
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("JsonDummyForBot.json"))
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

        //reference https://stackoverflow.com/questions/11296810/how-do-i-implement-ienumerablet
        //try to use this for .Confirm
        class Confirmations : IEnumerable<string>
        {
            List<string> mylist = new List<string>() { Resource1.Yes, Resource1.No };

            public string this[int index]
            {
                get { return mylist[index]; }
                set { mylist.Insert(index, value); }
            }

            public IEnumerator GetEnumerator()
            {
                return this.GetEnumerator();
            }

            IEnumerator<string> IEnumerable<string>.GetEnumerator()
            {
                return mylist.GetEnumerator();
            }
        }
    }