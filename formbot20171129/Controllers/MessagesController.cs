using System;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;
using System.Diagnostics;
using LuisBot.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Newtonsoft.Json.Linq;
using static LuisBot.Dialogs.BasicLuisDialog;

namespace Microsoft.Bot.Sample.LuisBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
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
        /// receive a message from a user and send replies
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