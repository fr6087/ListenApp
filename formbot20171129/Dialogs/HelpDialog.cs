using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class HelpDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("you have reached helpDialog. What can I do for you?");
            var message = context.MakeMessage();
            message.Speak = SSMLHelper.Speak("Willkommen in der Hilfe. Dieser Bot ist dazu da Formularangaben zu machen. Versuchen Sie 'Meine Firma ist innobis' um das Feld Firma zu füllen.");
            message.InputHint = InputHints.AcceptingInput;

            /*
            message.Attachments = new List<Attachment>
            {
                new HeroCard(Resources.HelpTitle)
                {
                    Buttons = new List<CardAction>
                    {
                        new CardAction(ActionTypes.ImBack, "Roll Dice", value: RollDiceOptionValue),
                        new CardAction(ActionTypes.ImBack, "Play Craps", value: PlayCrapsOptionValue)
                    }
                }.ToAttachment()
            };*/

            await context.PostAsync(message);

            context.Done<object>(null);
        }
    }
}