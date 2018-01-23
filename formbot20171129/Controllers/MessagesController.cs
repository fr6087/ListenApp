using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;
using System.Diagnostics;
using Microsoft.Bot.Builder.FormFlow;
using Newtonsoft.Json.Linq;
using static LuisBot.Dialogs.BasicLuisDialog;

namespace Microsoft.Bot.Sample.LuisBot
{
    /// <summary>
    /// The class MessagesController is the central entry point of the bot. It receives all messages by the user in the post()-method, where
    /// it specifies what to do with them. In the Post-Method it is possible to create a new static dialog (via method MakeRootDialog) or to build
    /// one from JSON. Currently I'm debugging the BuildFromJason()-Method. I regard that one very promising, because if I manage to create a similar 
    /// JSON output in the WCF-Service, then the bot and the WCF-Service will be able to communicate directly and fill out the form alone together.
    /// If this works out, then the UWP-App will have no central meaning any more, enabling downscaling of the application.
    /// On devices that have no screen and have higher storage restictions (e.g. smartwatches, some have 'only' 4 GB capacity)
    /// it could be a significant advantage to need less components for ListenToMe.
    /// reference: https://www.engadget.com/2017/02/08/lg-watch-sport-review/
    /// // reference: https://github.com/Microsoft/BotBuilder/tree/master/CSharp/Samples/AnnotatedSandwichBot
    /// </summary>
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// MakeRootDialog uses a JSON-Object to bind the user's answers to. It uses FormFlow to create a FormDialog in which the user
        /// iterates though all the properties of the Object and is asked questions about them.
        /// </summary>
        /// <returns>The FormDialog. The user is able to quit the form dialog by typing or saying 'quit'.</returns>
        internal static IDialog<ESF2CompanyDetailsForm> MakeRootDialog()
        {
            return Chain.From(() => FormDialog.FromForm(ESF2CompanyDetailsForm.BuildLocalizedForm))
                .Do(async (context, order) =>
                {
                    try
                    {
                        var completed = await order;
                        // Actually process the form...
                        await Conversation.SendAsync(new Activity("Formular ausgefüllt"), MakeRootDialog);
                    }
                    catch (FormCanceledException<ESF2CompanyDetailsForm> e)
                    {
                        string reply;
                        if (e.InnerException == null)
                        {
                            reply = $"Du hast bei {e.Last} aufgehört. Nächstes Mal kannst du dort weiterarbeiten!";
                        }
                        else
                        {
                            reply = "Entschuldigung, ich bin begriffsstutzig. Versuch es später noch mal.";
                        }
                        await Conversation.SendAsync(new Activity(reply), MakeRootDialog);
                    }
                });
        }

        /// <summary>
        /// MakeJsonRootDialog is calling a JSON file from the assembly with BuildJsonForm. It binds to the same static class, which might later
        /// be problematic. To make this dynamic, one has to delve deeper into C# classes. Surely there is somewhere a concept, that solves this problem.
        /// Note that the build action of JsonDummyForBot.json has to be set to Embedded rescouce for this to work.
        /// </summary>
        /// <returns>The dialog generated from the file JsonDummyForBot.json</returns>
        internal static IDialog<JObject> MakeJsonRootDialog()
        {
            return Chain.From(() => FormDialog.FromForm(ESF2CompanyDetailsForm.BuildJsonForm))
                .Do(async (context, order) =>
                {
                    try
                    {
                        var completed = await order;
                        // Actually process the sandwich order...
                        Activity activity = new Activity("Processed your order!");
                        activity.Code = "Form";
                        await context.PostAsync(activity);
                    }
                    catch (FormCanceledException<JObject> e)
                    {
                        string reply;
                        if (e.InnerException == null)
                        {
                            reply = $"You quit on {e.Last}--maybe you can finish next time!";
                        }
                        else
                        {
                            reply = "Sorry, I've had a short circuit.  Please try again.";
                        }
                        Activity activity = new Activity(reply);
                        activity.Code = "Form";
                        await context.PostAsync(activity);
                    }
                });
        }

        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies. This is also the method in which the bot might receive Cortana information such as
        /// the user name. Below the method is a snippet that needs to be integrated in the swich case for that to work.
        /// Warning: The Bot will only be able to receive Cortana information if the computer in question has reagion and language settions
        /// set to USA. Cortana skills are only open for US-market as it is.
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity != null)
            {
                // one of these will have an interface and process it erreicht er auch bei formdialog, warum?
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:

                        var listofnames = this.GetType().Assembly.GetManifestResourceNames();

                        await Conversation.SendAsync(activity, MakeJsonRootDialog);
                       // await Conversation.SendAsync(activity, () => new BasicLuisDialog());
                        
                        break;

                    case ActivityTypes.ConversationUpdate:
                    case ActivityTypes.ContactRelationUpdate:
                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                    default:
                        HandleSystemMessage(activity);
                        Trace.TraceError($"Unknown activity type ignored: {activity.GetActivityType()}");
                        break;
                }
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
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

        /// <summary>
        /// Handles Events that might be raised during conversation.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>a message that informs the user of the changed conservation state</returns>
        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels

            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}