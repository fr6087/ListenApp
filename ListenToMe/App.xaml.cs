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

        private RootFrameNavigationHelper rootFrameNavigationHelper;

        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt. Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendung durch den Endbenutzer normal gestartet wird. Weitere Einstiegspunkte
        /// werden z. B. verwendet, wenn die Anwendung gestartet wird, um eine bestimmte Datei zu öffnen.
        /// </summary>
        /// <param name="e">Details über Startanforderung und -prozess.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
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
            }
            catch (Exception ex)
            {
                Debug.Write("Failed to register custom commands because " + ex.Message);
            }
        }

        protected async override void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);
            Type navigationToPageType;
            Model.ListenToMeVoiceCommand.VoiceCommand? navigationCommand = null;

            MessageDialog dialog = new MessageDialog("");
            if (e.Kind == ActivationKind.VoiceCommand )
            {
                VoiceCommandActivatedEventArgs cmdArgs = e as VoiceCommandActivatedEventArgs;
                SpeechRecognitionResult result = cmdArgs.Result;

                string commandName = result.RulePath[0];
                Debug.Write("command found: " + commandName);
                dialog.Content = "You have a voice command activation";
                await dialog.ShowAsync();
                switch (commandName)
                {
                    case "ShutDownComputer":
                        dialog.Content = "shut computer down.";

                        Debug.WriteLine("found shut down command");
                        await dialog.ShowAsync();
                        navigationToPageType = typeof(AddressPage);
                        break;
                    //do something
                    case "Edit":
                        await dialog.ShowAsync();
                        dialog.Content = "Edit";//+ result.RulePath[1];

                        Debug.WriteLine("found edit command");
                        await dialog.ShowAsync();
                        navigationToPageType = typeof(ConfirmationsPage);
                        break;
                    default:
                        Debug.WriteLine("Couldn't find command name");
                        dialog.Content = "default of onActivated";
                        await dialog.ShowAsync();
                        navigationToPageType = typeof(MainPage);
                        break;


                }
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
                navigationToPageType = typeof(ESF_2.CompanyPage);
            }
                else
                {
                dialog.Content = "something else";
                await dialog.ShowAsync();
                    // If we were launched via any other mechanism, fall back to the main page view.
                    // Otherwise, we'll hang at a splash screen.
                    //navigationToPageType = typeof(View.TripListView);
                    navigationToPageType = typeof(MainPage);
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
                rootFrame.Navigate(navigationToPageType, navigationCommand);

                // Ensure the current window is active
                Window.Current.Activate();

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
    }
}
