using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.Media.SpeechRecognition;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;
using System.Threading.Tasks;
using System.Diagnostics;
using ListenToMe.ESF_2;
using ListenToMe.Common;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ListenToMe.ServiceReference1;
using System.Collections.ObjectModel;

namespace ListenToMe
{
    /// <summary>
    /// Stellt das anwendungsspezifische Verhalten bereit, um die Standardanwendungsklasse zu ergänzen.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Navigation service, provides a decoupled way to trigger the UI Frame
        /// to transition between views.
        /// </summary>
       public static NavigationService NavigationService { get; private set; }

        private Service1Client client;
        public static JsonSerializer JsonFormatter { get; internal set; }
        private RootFrameNavigationHelper rootFrameNavigationHelper;
        internal static readonly string userName = "fr6087";
        internal static readonly string userPassword= "Pellkart0ffelSal%40";
        //note: this form ist available in Polish and English as well. But the forms of these languages still contain 50% German so I'm neglecting these other languages.
        internal static readonly string uri = "http://10.150.50.21/formularservice/formular/A_FOREX_ANTRAG_ESF_2/appl/d556026e-991d-11e7-9fb1-27c0f1da4ec4/?lang=de";
        internal static readonly Dictionary<String, Type> HeadingsNavigations = new Dictionary<string, Type>();
        internal static readonly Dictionary<String, String> InputsNavigations = new Dictionary<string, String>();


        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt. Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            client = new Service1Client();
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendung durch den Endbenutzer normal gestartet wird. Weitere Einstiegspunkte
        /// werden z. B. verwendet, wenn die Anwendung gestartet wird, um eine bestimmte Datei zu öffnen.
        /// </summary>
        /// <param name="e">Details über Startanforderung und -prozess.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            MessageDialog message = new MessageDialog("OnLaunched was called.");
            await message.ShowAsync();
            Frame rootFrame = Window.Current.Content as Frame;

            // App-Initialisierung nicht wiederholen, wenn das Fenster bereits Inhalte enthält.
            // Nur sicherstellen, dass das Fenster aktiv ist.
            if (rootFrame == null)
            {
                // Frame erstellen, der als Navigationskontext fungiert und zum Parameter der ersten Seite navigieren
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                App.NavigationService = new NavigationService(rootFrame);

                // Use the RootFrameNavigationHelper to respond to keyboard and mouse shortcuts.
                this.rootFrameNavigationHelper = new RootFrameNavigationHelper(rootFrame);


                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Zustand von zuvor angehaltener Anwendung laden
                }

                // Den Frame im aktuellen Fenster platzieren
                Window.Current.Content = rootFrame;
                

                // Wenn der Navigationsstapel nicht wiederhergestellt wird, zur ersten Seite navigieren
                // und die neue Seite konfigurieren, indem die erforderlichen Informationen als Navigationsparameter
                // übergeben werden

                // Determine if we're being activated normally, or with arguments from Cortana.
                if (string.IsNullOrEmpty(e.Arguments))
                {
                    // Launching normally.
                    rootFrame.Navigate(typeof(MainPage), "");
                }
                else
                {
                    // Launching with arguments. We assume, for now, that this is likely
                    // to be in the form of "destination=<location>" from activation via Cortana.
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
            }
            // Sicherstellen, dass das aktuelle Fenster aktiv ist
            Window.Current.Activate();

            ActivateVoiceCommands();


        }
        private async void ActivateVoiceCommands()
        {
            try
            {
                Debug.Write("Let's search for xml in " + Package.Current.InstalledLocation.Name);
                StorageFile vcd = await Package.Current.InstalledLocation.GetFileAsync(@"VoiceCommands.xml");
                await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcd);
                CortanaModelMethods meth = new CortanaModelMethods();
                Debug.WriteLine("****************1******************");
                List<String> headings = await meth.UpdatePhraseList("Page"); //this is supposed to load the headings from html file and add them as pages to voice command file
                Debug.WriteLine(headings.Count + "headingsCount");
                Debug.WriteLine("*****************2*****************");
                List<String> inputs = await meth.UpdatePhraseList("Field");
                
                
                
                //create two dictionaries for translating items to the XAML-Names I chose. This is not necessary, when creating pages dynamic. but since I already have the static ones, I'm going to use them :)
                fillPageDictionary(headings);
                //fillFieldDictionary(inputs); //not yet implemented

            }
            catch (Exception ex)
            {
                Debug.Write("Failed to register custom commands because " + ex.Message);
            }
        }

        



        /*OnActivated is called whenever an external application e.g- Cortana launches the ListenToMeApp*/
        protected async override void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);
           
            Model.ListenToMeVoiceCommand.VoiceCommand? navigationCommand = null;
            //toDo: rootFrameNavigationHelper
            MessageDialog dialog = new MessageDialog("");
            if (e.Kind == ActivationKind.VoiceCommand )
            {
                VoiceCommandActivatedEventArgs cmdArgs = e as VoiceCommandActivatedEventArgs;
                
                SpeechRecognitionResult result = cmdArgs.Result;
               
                string commandName = result.RulePath[0];
                int len = result.RulePath.ToArray().Length;
                String rules = result.Text;
                Debug.Write("command found: " + commandName);
                dialog.Content = "You have a voice command activation "+ rules;
                await dialog.ShowAsync();
                initGui();//load default frames and pages
                //Install VoiceCommands
                ActivateVoiceCommands();
                await performCommandAsync(commandName, rules);

            }
            else if (e.Kind == ActivationKind.Protocol)
            {
                dialog.Content = "Activated by Protocol";
                await dialog.ShowAsync();

                // Extract the launch context. In this case, we're just using the destination from the phrase set (passed
                // along in the background task inside Cortana), which makes no attempt to be unique. A unique id or 
                // identifier is ideal for more complex scenarios. We let the destination page check if the 
                // destination trip still exists, and navigate back to the trip list if it doesn't.
                var commandArgs = e as ProtocolActivatedEventArgs;
                Windows.Foundation.WwwFormUrlDecoder decoder = new Windows.Foundation.WwwFormUrlDecoder(commandArgs.Uri.Query);
                var destination = decoder.GetFirstValueByName("LaunchContext");

                navigationCommand = new Model.ListenToMeVoiceCommand.VoiceCommand(
                                        "protocolLaunch",
                                        "text",
                                        "destination",
                                        destination);

                
                MessageDialog mymessage = new MessageDialog("Todo here some navigation by protocol ");
                await mymessage.ShowAsync();
                //--navigationToPageType = typeof(ESF_2.CompanyPage);
                NavigationService.Navigate(typeof(ESF_2.CompanyPage), navigationCommand);
            }
            else
            {
                dialog.Content = "something else";
                await dialog.ShowAsync();
                // If we were launched via any other mechanism, fall back to the main page view.
                // Otherwise, we'll hang at a splash screen.
                //navigationToPageType = typeof(View.TripListView);
                //--navigationToPageType = typeof(MainPage);
            }

            // Repeat the same basic initialization as OnLaunched() above, taking into account whether
            // or not the app is already active.
                Frame rootFrame = Window.Current.Content as Frame;

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (rootFrame == null)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame = new Frame();
                    App.NavigationService = new Common.NavigationService(rootFrame);

                    rootFrame.NavigationFailed += OnNavigationFailed;

                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }

            // Since we're expecting to always show a details page, navigate even if 
            // a content frame is in place (unlike OnLaunched).
            // Navigate to either the main trip list page, or if a valid voice command
            // was provided, to the details page for that trip.
            //--rootFrame.Navigate(navigationToPageType, navigationCommand);
            NavigationService.Navigate(typeof(MainPage), navigationCommand);
                // Ensure the current window is active
                Window.Current.Activate();

        }

        private void initGui()
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // App-Initialisierung nicht wiederholen, wenn das Fenster bereits Inhalte enthält.
            // Nur sicherstellen, dass das Fenster aktiv ist.
            if (rootFrame == null)
            {
                // Frame erstellen, der als Navigationskontext fungiert und zum Parameter der ersten Seite navigieren
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                App.NavigationService = new NavigationService(rootFrame);

                // Use the RootFrameNavigationHelper to respond to keyboard and mouse shortcuts.
                this.rootFrameNavigationHelper = new RootFrameNavigationHelper(rootFrame);

                /*
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Zustand von zuvor angehaltener Anwendung laden
                }*/

                // Den Frame im aktuellen Fenster platzieren
                Window.Current.Content = rootFrame;


                // Wenn der Navigationsstapel nicht wiederhergestellt wird, zur ersten Seite navigieren
                // und die neue Seite konfigurieren, indem die erforderlichen Informationen als Navigationsparameter
                // übergeben werden

                // Determine if we're being activated normally, or with arguments from Cortana.
                if (string.IsNullOrEmpty(""))//e.Arguments
                {
                    // Launching normally.
                    rootFrame.Navigate(typeof(MainPage), "");
                }
                /*else
                {
                    // Launching with arguments. We assume, for now, that this is likely
                    // to be in the form of "destination=<location>" from activation via Cortana.
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }*/
            }
            // Sicherstellen, dass das aktuelle Fenster aktiv ist
            Window.Current.Activate();

        }

        private async Task performCommandAsync(string commandName, String result)
        {
           
            Frame bigFrame = rootFrameNavigationHelper.Frame;//Window.Current.Content as Frame;
            MessageDialog message = new MessageDialog("sss"+bigFrame.Name);
            await message.ShowAsync();
            var page = (MainPage)bigFrame.Content;
            Frame subFrame = page.mainFrame;
            Window.Current.Content = bigFrame;
            MessageDialog dialog = new MessageDialog("");
            switch (commandName)
            {
                //do something
                case "Edit":

                    dialog.Content = "Edit. result.Text " + result;//result.RulePath[1];
                    Debug.WriteLine("found 2 edit command ");
                    await dialog.ShowAsync();
                    NavigationService nana = new NavigationService(subFrame);
                    var navPageType = typeof(MainPage);
                    if (HeadingsNavigations.Count < 1) //this should not happen, but it does whenever the App starts with OnActivated instead of OnLaunched
                    {
                        CortanaModelMethods meth = new CortanaModelMethods();
                        List<String> headings = await meth.UpdatePhraseList("Page");
                        fillPageDictionary(headings);
                    }

                    await FindPageToNavigateAndJump(nana, result);
                    
                    break;

                case "Information":
                    VoiceCommandUserMessage messageM = new VoiceCommandUserMessage();
                    messageM.DisplayMessage = "You can say: 'Edit <Field xyz>' or 'Shutdown'";
                    Debug.WriteLine("found information command");
                    //Application.Current.Exit();
                    //return typeof(MainPage);
                    break;
                case "Menu":
                    dialog.Content = "showing menu";
                    Debug.WriteLine("found shut down command");
                    await dialog.ShowAsync();
                    Debug.WriteLine("found information command");
                    NavigationService.Navigate(typeof(MainPage));
                    break;
                case "Shutdown":
                    dialog.Content = "shut computer down.";
                    Debug.WriteLine("found shut down command");
                    await dialog.ShowAsync();
                    Application.Current.Exit();
                    break;// return typeof(MainPage);
                default:
                    Debug.WriteLine("Couldn't find command name");
                    dialog.Content = "default of onActivated";
                    await dialog.ShowAsync();
                    NavigationService.Navigate(typeof(MainPage));
                    break;
                    //return typeof(MainPage);

            }
        }

        private Task<bool> FindPageToNavigateAndJump(NavigationService nana, String result)
        {
            foreach (var heading in HeadingsNavigations) //go through the headings on the page
            {

                if (result.Contains(heading.Key)) //test whether the heading was mentioned in the Speech input by the user
                {
                    var navPageType = HeadingsNavigations[heading.Key];//if yes, get the pagetype to which to navigate
                    nana.Navigate(navPageType);//and navigate
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }


        /// <summary>
        /// Wird aufgerufen, wenn die Navigation auf eine bestimmte Seite fehlschlägt
        /// </summary>
        /// <param name="sender">Der Rahmen, bei dem die Navigation fehlgeschlagen ist</param>
        /// <param name="e">Details über den Navigationsfehler</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Ausführung der Anwendung angehalten wird.  Der Anwendungszustand wird gespeichert,
        /// ohne zu wissen, ob die Anwendung beendet oder fortgesetzt wird und die Speicherinhalte dabei
        /// unbeschädigt bleiben.
        /// </summary>
        /// <param name="sender">Die Quelle der Anhalteanforderung.</param>
        /// <param name="e">Details zur Anhalteanforderung.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Anwendungszustand speichern und alle Hintergrundaktivitäten beenden
            deferral.Complete();
        }

        /// <summary>
        /// Filter changed is supposed to filter every sign from input string that's not alphanumerical. Unfortunately,
        /// TextchangedEvent is only triggered between posts to the server, so this is inept try.
        /// see reference: https://msdn.microsoft.com/en-us/library/system.web.ui.webcontrols.textbox.textchanged(v=vs.110).aspx
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void Filter_Changed(object sender, TextChangedEventArgs e, String regex)
        {
            Debug.WriteLine("Text Changed ");
            var textboxSender = (TextBox)sender;
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "^(?!"+regex+")$", "");
            textboxSender.SelectionStart = cursorPosition;

        }
        private void fillPageDictionary(List<String> headings)
        {
            var Type = typeof(MainPage);
            for (int i = 0; i < headings.Count; i++)//entry 0 has MainPageType and FormHeading
            {

                switch (i)
                {
                    case 1:
                        Type = typeof(ESF_2.CompanyPage);
                        break;
                    case 2:
                        Type = typeof(ESF_2.AddressPage);
                        break;
                    case 3:
                    case 4:
                        Type = typeof(ESF_2.LegalFormRegistrationPage);
                        break;
                    case 5:
                    case 6:
                        Type = typeof(ESF_2.RegisterEntriesPage);
                        break;
                    case 7:
                    case 8:
                        Type = typeof(ESF_2.PetitionsPage);
                        break;
                    case 9:
                        Type = typeof(ESF_2.LocationPage);
                        break;
                    case 10:
                    case 11:
                        Type = typeof(ESF_2.ScheduleHRPage);
                        break;
                    case 12:
                        Type = typeof(ESF_2.TrainingProvidersPage);
                        break;
                    case 13:
                        Type = typeof(ESF_2.FinancingPage);
                        break;
                    case 14:
                        Type = typeof(ESF_2.ConfirmationsPage);
                        break;
                    case 15:
                        Type = typeof(ESF_2.DeclarationsPage);
                        break;

                }
                //Debug.WriteLine("Dict. Heading: " + headings[i] + " " + Type.Name +" | "+ Type.FullName);
                HeadingsNavigations.Add(headings[i], Type);
            }
        }

        private void fillFieldDictionary(List<String> inputs)
        {
            String textBoxName = "";

            int i = 0;
            InputsNavigations.Add("_11Name", inputs[i]);//Name
            i++;
            InputsNavigations.Add( "_13Anrede", inputs[i]);//Anrede
            i++;
            InputsNavigations.Add( "_13surname", inputs[i]);//Nachname
            i++;
            InputsNavigations.Add( "_13givenName", inputs[i]);//Vorname
            i++;
            InputsNavigations.Add( "_13Function", inputs[i]);//Funktion
            i++;
            InputsNavigations.Add( "_13telephone", inputs[i]);//Telefon
            i++;
            InputsNavigations.Add( "_13Anrede", inputs[i]);//Fax
            i++;
            InputsNavigations.Add( "_13surname", inputs[i]);//E - Mail-Adresse
            i++;
            InputsNavigations.Add( "_13givenName", inputs[i]);//Straße
            i++;
            InputsNavigations.Add( "_13Function", inputs[i]);//Hausnr.
            i++;
            InputsNavigations.Add( "_13telephone", inputs[i]);//PLZ
            i++;
            InputsNavigations.Add( "_2PostalCode", inputs[i]);//Ort
            i++;
            InputsNavigations.Add( "_2township", inputs[i]);//Gemeinde
            i++;
            InputsNavigations.Add( "_2townshipKey", inputs[i]);//Amtlicher Gemeindeschlüssel (AGS)
            i++;
            InputsNavigations.Add( "*********************************************", inputs[i]);//Landkreis
            i++;
            InputsNavigations.Add( "_2POB", inputs[i]);//Postfach
            i++;
            InputsNavigations.Add( "_2postalcodePOB", inputs[i]);//Postleitzahl Postfach
            i++;
            InputsNavigations.Add("_2telephone", inputs[i]);//Telefon
            i++;
            InputsNavigations.Add("_2email", inputs[i]);//E - Mail-Adresse
            i++;
            InputsNavigations.Add( "_13givenName", inputs[i]);//Internetadresse (URL) des Antragstellers
            i++;
            InputsNavigations.Add( "_13Function", inputs[i]);//IBAN
            i++;
            InputsNavigations.Add( "_13telephone", inputs[i]);//3.1 Rechtsform
            i++;
            
            /*


3.2 Gründungsdatum bzw. Geburtsdatum (bei natürlichen Personen):
4.1 Datum der Bescheinigung der Gewerbeanmeldung
4.2 Datum des Beginns der angemeldeten Tätigkeit
4.3 Angemeldete Tätigkeit (entsprechend Ziffer 15 der Gewerbeanmeldung)
5.1 Handelsregister-Nummer
Datum des Eintrags ins HR
Seiten-Nummer letztes Blatt HR
Datum des letzten Eintrags
5.2 Vereinsregister-Nummer
Datum des Eintrags ins VR
5.3 Genossenschaftsregister-Nummer
Datum des Eintrags in GR
5.4 Steuernummer
Zuständiges Finanzamt
Anrede
Titel
Name
Vorname
Funktion im Unternehmen
Bitte wählen Sie einen Branchenschlüssel
Anzahl weibliche Beschäftigte
Anzahl männliche Beschäftigte
Anzahl der Beschäftigten gesamt
Ist zum Zeitpunkt der Antragstellung eine verbindliche Anmeldung für die Weiterbildungsmaßnahme/den Kurs erfolgt oder wird vor Erhalt des Zuwendungsbescheides der LASA eine Anmeldung erfolgen?
Das Unternehmen befindet sich in folgendem Regionalen Wachstumskern:
Das Unternehmen ist folgendem Zukunftscluster zuzuordnen:
Bezirk der Agentur für Arbeit
Landkreis
PLZ
Maßnahmeort
Gemeinde
Amtlicher Gemeindeschlüssel (AGS)
Zuordnungskriterium: Kreisstadt von Sitz/Betriebsstätte des Unternehmens im Land Brandenburg
10.1 Kurzbezeichnung der Weiterbildungsmaßnahme/n
10.2 Bitte beschreiben Sie die mittel- und langfristigen Entwicklungsziele Ihres Unternehmens, zu deren Erreichung die  Weiterbildungsmaßnahme/n beitragen soll/en:
von
bis
Frauen:
Männer:
Mitarbeiter/innen gesamt:
Weibliche Teilnehmende:
Männliche Teilnehmende:
Teilnehmende gesamt:
Gesamtinvestitionen:
Gesamtinvestitionen:
Zuschuss:
Gesamt:
Öffentliche Mittel EU:
Gesamt:
Ort
Datum der Antragstellung
Nachname, Vorname der/des Zeichnungsberechtigten
Anzahl Frauen:*
Anzahl Männer:*
Gesamt:
Zahl der Erwerbstätigen (einschl. Selbstständige, Auszubildende) gesamt:*
Davon Anzahl Frauen:*
Darunter Zahl der Selbstständigen (einschl. Existenzgründer):*
Davon Anzahl Frauen:*
Gemeldete Arbeitslose gesamt:*
Davon Anzahl Frauen:*
Darunter Zahl der Langzeitarbeitslosen:*
Davon Anzahl Frauen:*
Zahl der Nicht-Erwerbstätigen gesamt:*
Davon Anzahl Frauen:*
Darunter Zahl der Nicht-Erwerbstätigen in beruflicher Ausbildung:
Davon Anzahl Frauen:*
Summe 4.2:
Davon Summe Frauen:
Zahl der Jugendlichen (zwischen 15 und 24 Jahren):
Davon Anzahl Frauen:*
Zahl der Älteren (zwischen 25 und 44 Jahren):
Davon Anzahl Frauen:*
Zahl der Älteren (zwischen 45 und 54 Jahren):
Davon Anzahl Frauen:*
Zahl der Älteren (zwischen 55 und 64 Jahren):
Davon Anzahl Frauen:*
Summe 4.3:
Davon Summe Frauen:
Angehörige einer nationalen Minderheit:
Davon Anzahl Frauen:*
Zahl der Migranten und Migrantinnen:
Davon Anzahl Frauen:*
Zahl der Menschen mit Behinderungen:
Davon Anzahl Frauen:*
mit Hauptschulabschluss:
Davon Anzahl Frauen:*
mit mittlerer Reife (10. Klasse):
Davon Anzahl Frauen:*
mit (Fach)Hochschulreife:
Davon Anzahl Frauen:*
mit (Fach)Hochschulabschluss, Meisterabschluss, Promotion:
Davon Anzahl Frauen:*
Summe 4.5:
Davon Summe Frauen:
Jahr:
Anzahl Frauen:*
Anzahl Männer:*
Darunter Zahl der Nicht-Erwerbstätigen in beruflicher Ausbildung:
Zahl der Nicht-Erwerbstätigen gesamt:*
Darunter Zahl der Langzeitarbeitslosen:*
Gemeldete Arbeitslose gesamt:*
Darunter Zahl der Selbstständigen (einschl. Existenzgründer):
Zahl der Erwerbstätigen gesamt:
Theorie:
Praxis:
Arbeitserfahrung:
Beratung / Orientierung (SOLL):
Gesamtteilnehmerstunden:
Gesamt je Teilnehmer:
Jahr:
Anzahl Frauen:*
Anzahl Männer:*
Darunter Zahl der Nicht-Erwerbstätigen in beruflicher Ausbildung:
Zahl der Nicht-Erwerbstätigen gesamt:*
Darunter Zahl der Langzeitarbeitslosen:*
Gemeldete Arbeitslose gesamt:*
Darunter Zahl der Selbstständigen (einschl. Existenzgründer):
Zahl der Erwerbstätigen gesamt:
Theorie:
Praxis:
Arbeitserfahrung:
Beratung / Orientierung (SOLL):
Gesamtteilnehmerstunden:
Gesamt je Teilnehmer:
             */ /*
            switch (i)
            {
                case 1:
                    textBoxName = typeof(ESF_2.CompanyPage);
                    break;
                case 2:
                    textBoxName = textBoxNameof(ESF_2.AddressPage);
                    break;
                case 3:
                case 4:
                    textBoxName = textBoxNameof(ESF_2.LegalFormRegistrationPage);
                    break;
                case 5:
                case 6:
                    textBoxName = textBoxNameof(ESF_2.RegisterEntriesPage);
                    break;
                case 7:
                case 8:
                    textBoxName = textBoxNameof(ESF_2.PetitionsPage);
                    break;
                case 9:
                    textBoxName = textBoxNameof(ESF_2.LocationPage);
                    break;
                case 10:
                case 11:
                    textBoxName = textBoxNameof(ESF_2.ScheduleHRPage);
                    break;
                case 12:
                    textBoxName = textBoxNameof(ESF_2.TrainingProvidersPage);
                    break;
                case 13:
                    textBoxName = textBoxNameof(ESF_2.FinancingPage);
                    break;
                case 14:
                    textBoxName = textBoxNameof(ESF_2.ConfirmationsPage);
                    break;
                case 15:
                    textBoxName = textBoxNameof(ESF_2.DeclarationsPage);
                    break;

            }
            Debug.WriteLine("Dict. Heading: " + heads[i] + " " + Type.Name + " | " + Type.FullName);
            HeadingsNavigations.Add(heads[i], Type);*/

        }
        }
    }
