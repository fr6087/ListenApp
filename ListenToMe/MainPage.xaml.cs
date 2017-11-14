using ListenToMe.Common;
using System;
using System.Collections.Generic;

using Windows.Media.SpeechRecognition;
using Windows.ApplicationModel.VoiceCommands;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.AppService;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using WinRTXamlToolkit.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ListenToMe.ESF_2;
using System.Diagnostics;
using System.Threading;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VoiceCommandService;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Popups;
using Newtonsoft.Json.Linq;
using Windows.Networking.Sockets;
using Windows.Networking;
using Windows.Storage.Streams;
using Windows.Web.Http;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Windows.Foundation.Collections;
using Windows.Foundation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace ListenToMe
{

    public partial class MainPage : Page { 
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    NavigationHelper navigationHelper;
    bool listening = false;
        private SpeechRecognizer speechRecognizerContinuous;
        private VoiceCommandService.Bot bot;
        private SpeechRecognizer speechRecognizer;
        private ObservableCollection<Message> messages; //toDo needed?
        private ManualResetEvent manualResetEvent;
        private AppServiceConnection inventoryService;
        //used for login at ILB Brandenburg website
        private String username = "fr6087";
        private String password = "OraEtLabora%211";

        public static RootFrameNavigationHelper smallFrameNavigationHelper;//can be used for navigating between pages of subframe

        public MainPage()
    {
        this.InitializeComponent();
       // NavigationCacheMode = NavigationCacheMode.Enabled;
        
        this.navigationHelper = new NavigationHelper(this);
        this.navigationHelper.LoadState += navigationHelper_LoadState;
        this.navigationHelper.SaveState += navigationHelper_SaveState;
            /*webViewBrowser.DOMContentLoaded += domloaded;
            webViewBrowser.NavigationStarting += navstarted;
            webViewBrowser.NavigationCompleted += testWebViwe_OnNavigated;*/
            
        
            //loadFormESF_2Pages();

        mainFrame.Navigate(typeof(CompanyPage), mainFrame);
        
        
    }

        private void navstarted(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            Debug.WriteLine("NavStarted");
        }

        private void domloaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {

            Debug.WriteLine("DomLoaded");
        }

        private void testWebViwe_OnNavigated(WebView sender, WebViewNavigationCompletedEventArgs args)
        {

            Debug.WriteLine("Has Navigated"); ;
        }

        //private TypedEventHandler<WebView, WebViewDOMContentLoadedEventArgs> OnDomContentLoaded()
        //{
        //  searchForInPutFields();
        // return TypedEventHandler<WebView, WebViewDOMContentLoadedEventArgs>;
        //}

       /* private async void searchForInPutFields()
        {
            var url = await testWebView.InvokeScriptAsync("eval", new String[] { "document.location.href;" });
            Debug.WriteLine("Yeah. new URL found in searchforInputFields"+url);
        }*/

        private async void testHttpsConnection()
        {
            // Define some variables and set values
        StreamSocket clientSocket = new StreamSocket();
            HostName serverHost = new HostName("ecosia.org");
            string serverServiceName = "https";

        // Try to connect to contoso using HTTP (port 80)
        try {
            
            // Call ConnectAsync method with a plain socket
            await clientSocket.ConnectAsync(serverHost, serverServiceName, SocketProtectionLevel.PlainSocket);

            NotifyUser("Connected");

        }
        catch (Exception exception) {
                        Debug.WriteLine("Connection Console.Error.WriteLineAsynced.");
            // If this is an unknown status it means that the error is fatal and retry will likely Console.Error.WriteLineAsync.
            if (Windows.Networking.Sockets.SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown) {
                throw;
            }

                        NotifyUser("Connect Console.Error.WriteLineAsynced with error: " + exception.Message);//, NotifyType.ErrorMessage);
            // Could retry the connection, but for this simple example
            // just close the socket.

            clientSocket.Dispose();
            clientSocket = null; 
            return;
        }

        // Now try to sent some data
        DataWriter writer = new DataWriter(clientSocket.OutputStream);
        string hello = "Hello World ☺ ";
        Int32 len = (int) writer.MeasureString(hello); // Gets the UTF-8 string length.
        writer.WriteInt32(len);
        writer.WriteString(hello);
        NotifyUser("Client: sending hello");

        try {
            // Call StoreAsync method to store the hello message
            await writer.StoreAsync();

            NotifyUser("Client: sent data");

            writer.DetachStream(); // Detach stream, if not, DataWriter destructor will close it.
        }
        catch (Exception exception) {
            NotifyUser("Store Console.Error.WriteLineAsynced with error: " + exception.Message);
            // Could retry the store, but for this simple example
                // just close the socket.

                clientSocket.Dispose();
                clientSocket = null; 
                return;
        }

        }

        private void NotifyUser(string message)
        {
            Debug.WriteLine("called Notify. Message: "+message);
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
    {
    }

    private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
    {
            //testWebView.NavigationCompleted += testWebView_NavigationCompleted;

    }

        /// <summary>
        /// This method is called whenever the WebView showing the HTML-Page receives Navigation. After navigating to a subpage e.g.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
    private void testWebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
    {
       
        if (args.IsSuccess == true)
        {
            Debug.WriteLine("Navigation to " + sender.DocumentTitle + " completed successfully.");
        }
        else
        {
                Debug.WriteLine("Navigation to: " + args.Uri.ToString() +
                                    " Console.Error.WriteLineAsynced with error " + args.WebErrorStatus.ToString());
        }
            
    }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
            Debug.WriteLine("MainPage_OnNavig");
            messages = new ObservableCollection<Message>();
            bot = new VoiceCommandService.Bot();
            Media.MediaEnded += Media_MediaEnded;
            await InitContinuousRecognition();

            if (e.Parameter != null && e.Parameter is bool) //if activated with voicecommand?
            {
                var rootObject = await bot.SendMessageAndGetIntentFromBot("hello there"); //might be later rootobject
                Debug.WriteLine("in Text: "+rootObject.ToString());
                String intent = rootObject.topScoringIntent.intent; //the intent
                var fieldValue = rootObject.entities[0].entity; //the value of the field, if discovered
                var fieldType = rootObject.entities[0].type; //the Type e.g. "Address"
                Boolean fieldfilledIn = await FieldFillIn(intent, fieldValue, fieldType);
                if(fieldfilledIn)
                    await SpeakAsync("Ok.");
                
                await SetListeningAsync(true);
            }
            else if (e.Parameter != null && e.Parameter is string && !string.IsNullOrWhiteSpace(e.Parameter as string))//if activated with text?
            {
                Debug.WriteLine("MainPage OnNavig activated with "+ e.Parameter);
                await SendMessage(e.Parameter as string, true);
                await SetListeningAsync(true);
            }
            //test switchcase
            //App.performCommandAsync("Edit");
            //establishAppServiceConnection();
            await loginToILBBaukastenWebView();
            //testHelloWorldLogin();

            navigationHelper.OnNavigatedTo(e);
    }

        private async void establishAppServiceConnection()
        {
            // Add the connection.
            if (this.inventoryService == null)
            {
                this.inventoryService = new AppServiceConnection();

                // Here, we use the app service name defined in the app service provider's Package.appxmanifest file in the <Extension> section.
                this.inventoryService.AppServiceName = "ListenToMeVoiceCommandService";

                // Use Windows.ApplicationModel.Package.Current.Id.FamilyName within the app service provider to get this value.
                this.inventoryService.PackageFamilyName = App.familyName;
                Debug.WriteLine(App.familyName + "-thats a family name.");

                var status = await this.inventoryService.OpenAsync();
                if (status != AppServiceConnectionStatus.Success)
                {
                    text.Text = "Console.Error.WriteLineAsynced to connect";
                    return;
                }
            }
        }

        /// Call the service.
        private async Task<String> callVoiceCommandServiceAsync()
        {
            int idx = 0;//int.Parse(text.Text);
            var message = new ValueSet();
            message.Add("Command", "Item");
            message.Add("ID", idx);
            AppServiceResponse response = await this.inventoryService.SendMessageAsync(message);
            string result = "";

            if (response.Status == AppServiceResponseStatus.Success)
            {
                // Get the data  that the service sent  to us.
                if (response.Message["Status"] as string == "OK")
                {
                    result = response.Message["Result"] as string;
                }
            }

            message.Clear();
            message.Add("Command", "Price");
            message.Add("ID", idx);
            response = await this.inventoryService.SendMessageAsync(message);

            if (response.Status == AppServiceResponseStatus.Success)
            {
                // Get the data that the service sent to us.
                if (response.Message["Status"] as string == "OK")
                {
                    result += " : Price = " + response.Message["Result"] as string;
                }
            }
            Debug.WriteLine("Got result as "+ result);
            return result;
        }
            

        private async Task<System.Net.Http.HttpResponseMessage> loginToILBBaukastenWebView()
        {
            var uri = new Uri("http://10.150.50.21/irj/portal/anonymous/login");
            string returnData = string.Empty;
            var handler = new HttpClientHandler() { UseCookies = false };
            var httpClient = new System.Net.Http.HttpClient(handler) { BaseAddress = uri };
            var response = new System.Net.Http.HttpResponseMessage();
            try
            {
                //Set up the request
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:51.0) Gecko/20100101 Firefox/51.0");
                httpClient.DefaultRequestHeaders.Add("Host", "10.150.50.21");
                httpClient.DefaultRequestHeaders.Add("Referer", "http://10.150.50.21/irj/portal/anonymous/login");
                httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                httpClient.DefaultRequestHeaders.Add("AcceptLanguage", "de,en-US;q=0.7,en;q=0.3");
                httpClient.DefaultRequestHeaders.Add("AcceptEncoding", "gzip, deflate");
                httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

                //Format the POST data
                String addParamString = "login_submit=on&login_do_redirect=1&no_cert_storing=on&j_salt=y8C8h6SDoM5Lih9OlYiQa2ACs4c%3D&j_username=" + username + "&j_password=" + password + "&uidPasswordLogon=Anmelden";

                using (handler)
                using (httpClient)
                {
                    var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, uri);
                    //reference https://stackoverflow.com/questions/12373738/how-do-i-set-a-cookie-on-httpclients-httprequestmessage
                    req.Headers.Add("Cookie", "com.sap.engine.security.authentication.original_application_url = GET#ihTSVz9am7qcDynF6Qyz%2FiNnc21FZGAVbAk2TrEFWaojNAECmcOsZRdzgH%2F5VzBGPdM7T8ORPHFRI3PmBTDxV%2BrdvPZqenQyIOyJhBrYQvKR9mGToNomIg%3D%3D; PortalAlias=portal/anonymous; saplb_*=(J2EE1212320)1212350; JSESSIONID=DVV9kKB_pYvfAxWO-Z8CMRV_ExSmXwG-fxIA_SAPNZFSv0VwhtyWPGvgo_zax24H; sap-usercontext=sap-language=DE&sap-client=901; MYSAPSSO2=AjExMDAgAA1wb3J0YWw6RlI2MDg3iAATYmFzaWNhdXRoZW50aWNhdGlvbgEABkZSNjA4NwIAAzAwMAMAA09RMgQADDIwMTcxMTEwMTMyMgUABAAAAAgKAAZGUjYwODf%2FAQQwggEABgkqhkiG9w0BBwKggfIwge8CAQExCzAJBgUrDgMCGgUAMAsGCSqGSIb3DQEHATGBzzCBzAIBATAiMB0xDDAKBgNVBAMTA09RMjENMAsGA1UECxMESjJFRQIBADAJBgUrDgMCGgUAoF0wGAYJKoZIhvcNAQkDMQsGCSqGSIb3DQEHATAcBgkqhkiG9w0BCQUxDxcNMTcxMTEwMTMyMjIwWjAjBgkqhkiG9w0BCQQxFgQUSbhdInLJC2lw!MpfkbFiOgbPGkIwCQYHKoZIzjgEAwQuMCwCFHCwV9PiKmlq7TWfDJEj!9aq5RhIAhQYOvQGg2OlKW3DUFz3ccmjJvnslw%3D%3D; JSESSIONMARKID=AVDG2Qx20889PgQpiLbUXEWvmZGIKs11zS6r5_EgA; SAP_SESSIONID_FQ2_901=njV7NvjCNY8ZjRms7B5f3y4GRl7GGhHngO0AUFarFvM%3d");
                    req.Content = new StringContent("application/x-www-form-urlencoded");
                    req.Content.Headers.ContentLength = 163;
                    req.RequestUri = new Uri(uri + "?" + addParamString);
                    testWebView.Navigate(new Uri(uri+ "?" + addParamString));
                    response = await httpClient.PostAsync(new Uri(uri+"?"+addParamString), null);
                    //return await Task.Run(() =>; JsonObject.Parse(resp.toString()));
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
               // httpClient.Dispose();
            }
            return response;
        }

        /// <summary>
        /// simple Consoleging method
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        private async Task<Boolean> FieldFillIn(string intent, string textValue, string usersFieldName)
        {

            Debug.WriteLine(intent + " textWert: " + textValue + "Feldname " + usersFieldName);
            var page = mainFrame.Content as Page;//this is static, todo mke dynamic mainFrame.SourcePageType???
            usersFieldName = "_11Name"; //toDo: write function that mapps usersFieldName to FormFieldsName
            TextBox textfield = (TextBox)page.FindName(usersFieldName);
            if (textfield != null && !String.IsNullOrWhiteSpace(usersFieldName) && !String.IsNullOrWhiteSpace(textValue)) //if the user has named a field to label the value in
            {
                textfield.Text = textValue;
            }
            else//if not, then append the intent to output textbox
            {
                if (String.IsNullOrWhiteSpace(intent))
                    await Console.Error.WriteAsync("Mainpage, FieldFillIn. Leider konnte keine Intention erkannt werden");
                else
                {
                    //give intent to 


                    //CommandService's switch construct
                   // App.performCommandAsync(intent);

                    //toDo: Access win run component ListenToMe.VoiceCommands.performCommand(intent);
                }
                text.Text = intent;
                return false;
                //DependencyObject child = VisualTreeHelper.GetChild(page, 0);
                //TextBox box = (TextBox)page.F;
            }
            return true;

        }

        private async Task InitContinuousRecognition()
        {
            try
            {

                if (speechRecognizerContinuous == null)
                {
                    speechRecognizerContinuous = new SpeechRecognizer();
                    speechRecognizerContinuous.Constraints.Add(
                        new SpeechRecognitionListConstraint(
                            new List<String>() { "Start Listening" }, "start"));
                    SpeechRecognitionCompilationResult contCompilationResult =
                        await speechRecognizerContinuous.CompileConstraintsAsync();


                    if (contCompilationResult.Status != SpeechRecognitionResultStatus.Success)
                    {
                        throw new Exception();
                    }
                    speechRecognizerContinuous.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
                }

                await speechRecognizerContinuous.ContinuousRecognitionSession.StartAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
                args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                if (args.Result.Text == "Start Listening")
                {
                    await Media.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        await SetListeningAsync(true);
                    });
                }
            }
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            manualResetEvent.Set();
        }

        //protected override void OnNavigatedFrom(NavigationEventArgs e)
        //{
        ///     navigationHelper.OnNavigatedFrom(e);
        ///     }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainFrame.CanGoBack)
            {
                mainFrame.GoBack();
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(typeof(WebPage));
            
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainFrame.CanGoForward)
            {
                mainFrame.GoForward();
            }
        }

        /// <summary>
        /// Button is hit for microphone activation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button_Click(object sender, RoutedEventArgs e)
        {
            await SetListeningAsync(!listening);
        }

        private async System.Threading.Tasks.Task SetListeningAsync(bool toListen)
        {
            if (toListen)
            {
                listening = true;
                text.IsEnabled = false;
                symbol.Symbol = Symbol.FontColor;
                text.PlaceholderText = "Höre dir zu.";

                if (speechRecognizerContinuous != null)
                {
                    await speechRecognizerContinuous.ContinuousRecognitionSession.CancelAsync();
                }

                StartListenMode();
            }
            else
            {
                listening = false;
                text.IsEnabled = true;
                symbol.Symbol = Symbol.Microphone;
                Listening.IsActive = false;
                text.Text = "";
                text.PlaceholderText = "Type something or say 'Start Listening'";

                if (speechRecognizerContinuous != null)
                    await speechRecognizerContinuous.ContinuousRecognitionSession.StartAsync();
            }
        }

        private async void StartListenMode()
        {
            while (listening)
            {
                string spokenText = await ListenForText();
                while (string.IsNullOrWhiteSpace(spokenText) && listening)
                    spokenText = await ListenForText();

                if (spokenText.ToLower().Contains("stop listening"))
                {
                    speechRecognizer.UIOptions.AudiblePrompt = "Are you sure you want me to stop listening?";
                    speechRecognizer.UIOptions.ExampleText = "Yes/No";
                    speechRecognizer.UIOptions.ShowConfirmation = false;
                    //SpeakAsync(speechRecognizer.UIOptions.AudiblePrompt);
                    var result = await speechRecognizer.RecognizeWithUIAsync();

                    if (!string.IsNullOrWhiteSpace(result.Text) && result.Text.ToLower() == "yes")
                    {
                        await SetListeningAsync(false);
                    }
                }

                if (listening)
                {
                    await SendMessage(spokenText, true);
                }
            }
        }

        private async Task<String> SendMessage(string message, bool speak=false)
        {

            Debug.WriteLine("sending: " + message);
            messages.Add(new Message() { Text = message });
            
            var Rootobject = await bot.SendMessageAndGetIntentFromBot(message);
            messages.Add(new Message() { Text = "  > " + Rootobject });
            await FieldFillIn(Rootobject.topScoringIntent.intent, null, null);
            if (speak)
            {
                Debug.WriteLine("starting to speak");
                await SpeakAsync("Ich bin noch in der Entstehungsphase.");
                Debug.WriteLine("done speaking");

            }
            return "Change line 280 in MainPage, F!";
            //return response;
        }

        private async Task SpeakAsync(string toSpeak)
        {
            text.Text = "Speaking...";
            SpeechSynthesizer speechSyntesizer = new SpeechSynthesizer();
            SpeechSynthesisStream syntStream = await speechSyntesizer.SynthesizeTextToStreamAsync(toSpeak);
            Media.SetSource(syntStream, syntStream.ContentType);

            Task t = Task.Run(() =>
            {
                manualResetEvent.Reset();
                manualResetEvent.WaitOne();
            });

            await t;
            text.Text = "";
        }

        private  async Task<string> ListenForText()
        {
            string result = "";
            try
            {
 
                await InitSpeech();
                Listening.IsActive = true;
                text.Text = "Listening...";
                SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeAsync();
                if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
                {
                    result = speechRecognitionResult.Text;
                }
            }
            catch (Exception ex)
            {
                // Define a variable that holds the error for the speech recognition privacy policy. 
                // This value maps to the SPERR_SPEECH_PRIVACY_POLICY_NOT_ACCEPTED error, 
                // as described in the Windows.Phone.Speech.Recognition error codes section later on.
                const int privacyPolicyHResult = unchecked((int)0x80045509);

                // Check whether the error is for the speech recognition privacy policy.
                if (ex.HResult == privacyPolicyHResult)
                {
                    await Console.Error.WriteAsync("Unfortunately no Permisson for using Speech recognition");
                }

                Debug.WriteLine(ex.Message);
            }
            finally
            {
                Listening.IsActive = false;
                text.Text = "";
            }

            return result;
        }

        private async Task InitSpeech()
        {
            if (speechRecognizer == null)
            {
                try
                {
                    speechRecognizer = new SpeechRecognizer();

                    SpeechRecognitionCompilationResult compilationResult = await speechRecognizer.CompileConstraintsAsync();
                    speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;

                    if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
                        throw new Exception();

                    Debug.WriteLine("SpeechInit AOK");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SpeechInit Console.Error.WriteLineAsynced"+ex.Message);
                    speechRecognizer = null;
                }
            }
        }

        private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            await text.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                text.Text = args.Hypothesis.Text;
            });
        }

        /// <summary>
        /// Method that is reacting each time a key is hit while the textinput field has focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void text_KeyDown(object sender, KeyRoutedEventArgs e) //System.Windows.Forms.KeyEventArgs e)
        {
            //await testconnectionStreamSocket();
            TextBox box = (TextBox) sender;
            if(e.Key==Windows.System.VirtualKey.Enter && !string.IsNullOrWhiteSpace(box.Text))
            {
                VoiceCommandUserMessage message = new VoiceCommandUserMessage();
                await callVoiceCommandServiceAsync();//await SendMessage(box.Text, false);
                Debug.WriteLine("Hey. got the appservice feedback as "+ message.DisplayMessage);
                //toDo generate intent from Bot via DirectLine
                //var rootModel = await bot.SendMessageAndGetIntentFromBot(box.Text);
                //var intent = rootModel.topScoringIntent.intent;
                //await bot.ConnectAsync();
                App.performCommandAsync(box.Text);
                box.Text = "";
            }
        }

        private async Task testHttpStreamSocket()
        {
            StreamSocket clientSocket = new StreamSocket();
            HostName serverHost = new HostName("10.150.50.21/irj/portal");
            string serverServiceName = "http";

            // Try to connect to contoso using HTTP (port 80)
            try
            {
                // Call ConnectAsync method with a plain socket
                await clientSocket.ConnectAsync(serverHost, serverServiceName, SocketProtectionLevel.PlainSocket);

                NotifyUser("Connected");

            }
            catch (Exception exception)
            {
                Debug.WriteLine("Connection Console.Error.WriteLineAsynced."+exception.Message);
                // If this is an unknown status it means that the error is fatal and retry will likely Console.Error.WriteLineAsync.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                NotifyUser("Connect Console.Error.WriteLineAsynced with error: " + exception.Message);//, NotifyType.ErrorMessage);
                                                                              // Could retry the connection, but for this simple example
                                                                              // just close the socket.

                clientSocket.Dispose();
                clientSocket = null;
                return;
            }
        }
    }
    public class Message
    {
        public string Text { get; set; }
    }
}
