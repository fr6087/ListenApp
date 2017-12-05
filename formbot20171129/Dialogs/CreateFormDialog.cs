using FormBot;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Resource;

namespace LuisBot.Dialogs
{
    public class CreateFormDialog : IDialog<FormOptions>
    {
        public String intent { get; set; }//for later usage when Luis is starting this dialog

        public Task StartAsync(IDialogContext context)
        {
            context.UserData.SetValue<FormOptions>("FormOptions", new FormOptions());

            var descriptions = new List<string>() { "Anrede (0=Herr/1=Frau)" };//, "6 Sides", "8 Sides", "10 Sides", "12 Sides", "20 Sides" };
            var choices = new Dictionary<string, IReadOnlyList<string>>()
             {
                { "Anrede", new List<string> { "Herr", "Frau" } }
            };

            var promptOptions = new PromptOptionsWithSynonyms<string>(
                "Bitte wählen Sie eine Anrede",
                choices: choices,
                descriptions: descriptions,
                speak: SSMLHelper.Speak("keine Anrede"));//Utils.RandomPick(Resources.ChooseSidesSSML)));

           // PromptDialog.Choice(context, this.AddressChoiceReceivedAsync, promptOptions);
            return new Task(() => PromptDialog.Choice(context, this.AddressChoiceReceivedAsync, promptOptions));
        }

        private async Task AddressChoiceReceivedAsync(IDialogContext context, IAwaitable<string> result)
        {
            FormOptions options;
            if (context.UserData.TryGetValue<FormOptions>("FormOptions", out options))
            {
                int anrede;
                if (int.TryParse(await result, out anrede) | result.ToString().Equals("Herr") )//toDo test whether Herr logic will work like that
                {
                    if (anrede == 0)
                        options.Anrede = "Herr";
                    else
                        options.Anrede = "Frau";
                    context.UserData.SetValue<FormOptions>("FormOptions", options);
                }
                /* Ich glaube dieser Teil ist ein nächster Post.
                var promptText = string.Format("Wählen Sie eine Anrede", anrede);
                
                // TODO: When supported, update to pass Min and Max paramters
                var promptOption = new PromptOptions<long>(promptText, speak: SSMLHelper.Speak(Utils.RandomPick(Resources.ChooseCountSSML)));

                var prompt = new PromptDialog.PromptInt64(promptOption, min: 1, max: 100);
                context.Call<long>(prompt, this.DiceNumberReceivedAsync);*/
                context.Done(options);
            }
        }
    }
}