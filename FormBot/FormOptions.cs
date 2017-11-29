using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.FormFlow;

namespace FormBot
{
    public enum AnredeOptions
    {
       Herr, Frau
    };
    public enum VorzugssteuerberechtigungsOptions { Ja, Nein };
    /*
     reference: https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-formflow
{
    

    [Serializable]
    public class SandwichOrder
    {
        public SandwichOptions? Sandwich;
        public LengthOptions? Length;
        public BreadOptions? Bread;
        public CheeseOptions? Cheese;
        public List<ToppingOptions> Toppings;
        public List<SauceOptions> Sauce;

        public static IForm<SandwichOrder> BuildForm()
        {
            return new FormBuilder<SandwichOrder>()
                    .Message("Welcome to the simple sandwich order bot!")
                    .Build();
        }
    };
}
         */
    [Serializable]
    public class FormOptions
    {
        public String Name;
        public VorzugssteuerberechtigungsOptions? SteuerOptions;
        public String Straße;
        public short Hausnummer;
        public String Telefon;
        public String Fax;
        public String Email;
        public String Url;
        public static IForm<FormOptions> BuildForm()
        {
            return new FormBuilder<FormOptions>()
                    .Message("Welcome to the esf2 form bot!")
                    .Build();
        }
    }
}