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

        public String Name { get; set; }
        public String Anrede { get; set; }
        public VorzugssteuerberechtigungsOptions? SteuerOptions { get; set; }
        public String Straße { get; set; }
        public short Hausnummer { get; set; }
        public String Telefon { get; set; }
        public String Fax { get; set; }
        public String Email { get; set; }
        public String Url { get; set; }

        public FormOptions()
        {

        }
        public FormOptions(String _Name, String _Anrede, VorzugssteuerberechtigungsOptions? _options, String _Straße, short _Hausnummer, String _Telefon, String _Fax, String _Email, String _Url )
        {
            Name = _Name;
            Anrede = _Anrede;
            SteuerOptions = _options;
            Straße = _Straße;
            Hausnummer = _Hausnummer;
            Telefon = _Telefon;
            Fax = _Fax;
            Email = _Email;
            Url = _Url;
        }
        public static IForm<FormOptions> BuildForm()
        {
            return new FormBuilder<FormOptions>()
                    .Message("Welcome to the esf2 form bot!")
                    .Build();
        }
    }
}