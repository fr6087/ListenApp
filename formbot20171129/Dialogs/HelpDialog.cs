using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.LuisBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FormBot;
using System.Diagnostics;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class HelpDialog : IDialog<object>
    {
        private const string CompanyValue="Firmenangaben";
        private const string AddressValue="Adressangaben";
        private const string LegalValue ="Juristisches";
        private const string RegisterValue ="Registereinträge";
        private const string PetitionValue ="Petitionsdetails";
        private const string LocationValue ="Ortsangaben";
        private const string ScheduleValue ="Zeitangaben";
        private const string TrainingProvidersValue ="Fortbildungsanbieter";
        private const string FinancingValue ="Finanzen";

        private IEnumerable<string>
             options = new List<string> {CompanyValue, AddressValue, LegalValue, RegisterValue, PetitionValue, LocationValue, ScheduleValue, TrainingProvidersValue, FinancingValue};

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(Resource1.HelpClarification);
            var message = context.MakeMessage();
            message.Speak = SSMLHelper.Speak(Resource1.HelpWelcomeMessage);
            message.InputHint = InputHints.AcceptingInput;

            PromptDialog.Choice<string>(
                context,
                this.StartSelectedDialog,
                this.options,
                "What dialog would like to test?",
                "Ooops, what you wrote is not a valid option, please try again",
                3,
                PromptStyle.PerLine);
            /*
            message.Attachments = new List<Attachment>
            {
                new HeroCard(Resource1.HelpTitel)
                {
                    Buttons = new List<CardAction>
                    {
                        new CardAction(ActionTypes.ImBack, "Firma", value: CompanyValue),
                        new CardAction(ActionTypes.ImBack, "Adresse", value: AddressValue),

                        new CardAction(ActionTypes.ImBack, "Juristisches", value: LegalValue),
                        new CardAction(ActionTypes.ImBack, "Registereinträge", value: RegisterValue),

                        new CardAction(ActionTypes.ImBack, "Petition", value: PetitionValue),
                        new CardAction(ActionTypes.ImBack, "Ortsangaben", value: LocationValue),

                        new CardAction(ActionTypes.ImBack, "Zeit und Personal", value: ScheduleValue),
                        new CardAction(ActionTypes.ImBack, "Fortbildungsanbieter", value: TrainingProvidersValue),
                        new CardAction(ActionTypes.ImBack, "Finanzen", value: FinancingValue)
                    }
                }.ToAttachment()
            };*/

            await context.PostAsync(message);

            context.Done<object>(null);
        }

        private async Task StartSelectedDialog(IDialogContext context, IAwaitable<string> result)
        {
            String selectedDialog = await result;

            await GetSelectedDialog(selectedDialog, context);
        }
        
        private static async Task GetSelectedDialog(string selectedDialog, IDialogContext context)
        {
            switch (selectedDialog)
            {
                case CompanyValue:
                    context.Call(new CreateFormDialog(), AfterDialog);
                    break;
                case TrainingProvidersValue:
                    context.Call(new UploadDialog(), After2Dialog);
                    break;
                case AddressValue:
                  
                case LegalValue:
                    
                case RegisterValue:
                    
                case PetitionValue:
                    
                case LocationValue:
                    
                case ScheduleValue:
                    
                default:
                    context.Done("exiting help");
                    break;
            }
            
        }

        private async static Task After2Dialog(IDialogContext context, IAwaitable<object> result)
        {
            var res = await result;
            context.Done(res);
        }

        private async static Task AfterDialog(IDialogContext context, IAwaitable<FormOptions> result)
        {
            var res = await result;
            context.Done(res);
        }
    }
}