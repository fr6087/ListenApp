﻿using ListenToMe.Common;
using System;
using System.Collections.Generic; 
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ListenToMe.ESF_2;
using System.Diagnostics;
using Windows.Media.SpeechRecognition;
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
using System.Text;

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

        public MainPage()
    {
        this.InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Enabled;

        this.navigationHelper = new NavigationHelper(this);
        this.navigationHelper.LoadState += navigationHelper_LoadState;
        this.navigationHelper.SaveState += navigationHelper_SaveState;

            testWebView.InvokeScriptAsync("eval", new[]
            {
                @"(function()
                {
                    for (var i = 0; i < document.links.length; i++) { document.links[i].onclick = function() { window.external.notify('LaunchLink:' + this.href); return false; } }
                    

                    // Codestrecke 2
                    var hyperlinks = document.getElementsByTagName('a');
                    for(var i = 0; i < hyperlinks.length; i++)
                    {
                        if(hyperlinks[i].getAttribute('target') != null)
                        {
                            hyperlinks[i].setAttribute('target', '_self');
                        }
                    }
                })()"
            });

            testWebView.ScriptNotify += WebView_ScriptNotify;

            //loadFormESF_2Pages();

            mainFrame.Navigate(typeof(CompanyPage), mainFrame);
        
    }

        private async void testHttpConnection()
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
                        Debug.WriteLine("Connection failed.");
            // If this is an unknown status it means that the error is fatal and retry will likely fail.
            if (Windows.Networking.Sockets.SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown) {
                throw;
            }

                        NotifyUser("Connect failed with error: " + exception.Message);//, NotifyType.ErrorMessage);
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
    NotifyUser("Store failed with error: " + exception.Message);
    // Could retry the store, but for this simple example
        // just close the socket.

        clientSocket.Dispose();
        clientSocket = null; 
        return;
}

    /*// Now upgrade the client to use SSL
    try {
    // Try to upgrade to SSL
    await clientSocket.UpgradeToSslAsync(SocketProtectionLevel.Ssl, serverHost);

    NotifyUser("Client: upgrade to SSL completed");

    // Add code to send and receive data 
    // The close clientSocket when done
    }
    catch (Exception exception) {
    // If this is an unknown status it means that the error is fatal and retry will likely fail.
    if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown) {
        throw;
    }

    NotifyUser("Upgrade to SSL failed with error: " + exception.Message);

    clientSocket.Dispose();
    clientSocket = null; 
    return;
    }*/
        }

        private void NotifyUser(string message)
        {
            Debug.WriteLine("called Notify. Message: "+message);
            //MessageDialog dialog = new MessageDialog(message);
            //await dialog.ShowAsync();
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
    {
    }

    private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
    {
    }
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
            Debug.Write("MainPage_OnNavig");
            messages = new ObservableCollection<Message>();
            bot = new VoiceCommandService.Bot();
            Media.MediaEnded += Media_MediaEnded;
            await InitContinuousRecognition();

            if (e.Parameter != null && e.Parameter is bool) //if activated with voicecommand?
            {
                var rootObject = await bot.SendMessageAndGetIntentFromBot("hello there"); //might be later rootobject
                Debug.Write("in Text: "+rootObject.ToString());
                String intent = rootObject.topScoringIntent.intent; //the intent
                var fieldValue = rootObject.entities[0].entity; //the value of the field, if discovered
                var fieldType = rootObject.entities[0].type; //the Type e.g. "Address"
                var response = determineResponse(intent, fieldValue, fieldType);
                messages.Add(new Message() { Text = "  > " + response });
                await SpeakAsync(response);
                await SetListeningAsync(true);
            }
            else if (e.Parameter != null && e.Parameter is string && !string.IsNullOrWhiteSpace(e.Parameter as string))//if activated with text?
            {
                await SendMessage(e.Parameter as string, true);
                await SetListeningAsync(true);
            }
            //testHttpConnection();
            //testHTTPWebCon();
           // testHelloWorldLogin();
           
            navigationHelper.OnNavigatedTo(e);
            testWebView.Navigate(new Uri(testWebView.Source+ "?login_submit=on&login_do_redirect=1&no_cert_storing=on&j_salt=y8C8h6SDoM5Lih9OlYiQa2ACs4c%3D&j_username=fr6087&j_password=OraEtLabora%211&uidPasswordLogon=Anmelden"));
    }

        private async void testHelloWorldLogin()
        {
            string formUrl = "https://moodle.hs-emden-leer.de/moodle/login/index.php?"; // NOTE: This is the URL the form POSTs to, not the URL of the form (you can find this in the "action" attribute of the HTML's form tag
            string formParams = string.Format("username={0}&password={1}", "fr6087", "aTXB3Oir");
          
            string teamResponse = formUrl+formParams;
            Debug.WriteLine(teamResponse);

            Windows.Web.Http.HttpClient client = new Windows.Web.Http.HttpClient();

            try
            {
                Windows.Web.Http.HttpResponseMessage response = await client.PostAsync(new Uri(teamResponse), null);

                response.EnsureSuccessStatusCode();
                testWebView.Navigate(new Uri(teamResponse));
                string responseBody = await response.Content.ReadAsStringAsync();

                Debug.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine("\nException Caught!");
                Debug.WriteLine("Message :{0} ", e.Message);
            }

        }

        async private void WebView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            try
            {
                string data = e.Value;
                Debug.WriteLine("foo");
                if (data.ToLower().StartsWith("launchlink:"))
                {
                    Debug.WriteLine(new Uri(data.Substring("launchlink:".Length), UriKind.Absolute).ToString());

                    // await Launcher.LaunchUriAsync(new Uri(data.Substring("launchlink:".Length), UriKind.Absolute));
                }
            }
            catch (Exception)
            {
                // Could not build a proper Uri. Abandon.
            }
        }
        private async void testHTTPWebCon()
        {
            var uri = new Uri("http://10.150.50.21/irj/portal/anonymous/login");
            string returnData = string.Empty;
            var handler = new HttpClientHandler() { UseCookies = false };
            var httpClient = new System.Net.Http.HttpClient(handler) { BaseAddress = uri };
            

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
                StringBuilder postData = new StringBuilder();
                postData.Append("login_submit=on&login_do_redirect=1&no_cert_storing=on&j_salt=y8C8h6SDoM5Lih9OlYiQa2ACs4c%3D&j_username=fr6087&j_password=OraEtLabora%211&uidPasswordLogon=Anmelden");
                using (handler)
                using (httpClient)
                {
                    var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, uri);
                    //reference https://stackoverflow.com/questions/12373738/how-do-i-set-a-cookie-on-httpclients-httprequestmessage
                    req.Headers.Add("Cookie", "com.sap.engine.security.authentication.original_application_url = GET#ihTSVz9am7qcDynF6Qyz%2FiNnc21FZGAVbAk2TrEFWaojNAECmcOsZRdzgH%2F5VzBGPdM7T8ORPHFRI3PmBTDxV%2BrdvPZqenQyIOyJhBrYQvKR9mGToNomIg%3D%3D; PortalAlias=portal/anonymous; saplb_*=(J2EE1212320)1212350; JSESSIONID=DVV9kKB_pYvfAxWO-Z8CMRV_ExSmXwG-fxIA_SAPNZFSv0VwhtyWPGvgo_zax24H; sap-usercontext=sap-language=DE&sap-client=901; MYSAPSSO2=AjExMDAgAA1wb3J0YWw6RlI2MDg3iAATYmFzaWNhdXRoZW50aWNhdGlvbgEABkZSNjA4NwIAAzAwMAMAA09RMgQADDIwMTcxMTEwMTMyMgUABAAAAAgKAAZGUjYwODf%2FAQQwggEABgkqhkiG9w0BBwKggfIwge8CAQExCzAJBgUrDgMCGgUAMAsGCSqGSIb3DQEHATGBzzCBzAIBATAiMB0xDDAKBgNVBAMTA09RMjENMAsGA1UECxMESjJFRQIBADAJBgUrDgMCGgUAoF0wGAYJKoZIhvcNAQkDMQsGCSqGSIb3DQEHATAcBgkqhkiG9w0BCQUxDxcNMTcxMTEwMTMyMjIwWjAjBgkqhkiG9w0BCQQxFgQUSbhdInLJC2lw!MpfkbFiOgbPGkIwCQYHKoZIzjgEAwQuMCwCFHCwV9PiKmlq7TWfDJEj!9aq5RhIAhQYOvQGg2OlKW3DUFz3ccmjJvnslw%3D%3D; JSESSIONMARKID=AVDG2Qx20889PgQpiLbUXEWvmZGIKs11zS6r5_EgA; SAP_SESSIONID_FQ2_901=njV7NvjCNY8ZjRms7B5f3y4GRl7GGhHngO0AUFarFvM%3d");
                    req.Content = new StringContent("application/x-www-form-urlencoded");
                    //req.Content.Headers.ContentType = new MediaTypeHeaderValue();
                    testWebView.Navigate(new Uri(uri + "?" + postData.ToString()));
                    req.Content.Headers.ContentLength = 163;
                    var resp = await httpClient.SendAsync(req);
                    resp.EnsureSuccessStatusCode();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                httpClient.Dispose();
            }
            
                
            
            
            /*
            request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers["Host"]= "";
            request.Headers["UserAgent"] = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:51.0) Gecko/20100101 Firefox/51.0";
            request.Headers["Referer"] = "";
            request.Headers["AllowAutoRedirect"] = "true";
            request.Headers["KeepAlive"] = "true";
            request.CookieContainer = cookies;


            var httpClient = new HttpClient();
            

                // Always catch network exceptions for async methods
                try
                {
                    var result = await httpClient.GetStringAsync(uri);
                    Debug.WriteLine("hey, got connected." + result);
                }
                catch
                {
                    Debug.WriteLine("Connection Failed...........");
                // Details in ex.Message and ex.HResult.       
                }
                finally
                {
                    // Once your app is done using the HttpClient object call dispose to 
                    // free up system resources (the underlying socket and memory used for the object)
                    httpClient.Dispose();
                }*/
             
            

            
        }

        /// <summary>
        /// simple debugging method
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        private string determineResponse(string intent, string textValue, string usersFieldName)
        {
            Debug.WriteLine(intent + " textWert: " + textValue + "Feldname " + usersFieldName);
            var page = mainFrame.Content as Page;//this is static, todo mke dynamic mainFrame.SourcePageType???
            usersFieldName = "_11Name"; //toDo: write function that mapps usersFieldName to FormFieldsName
            TextBox textfield = (TextBox)page.FindName(usersFieldName);
            if (textfield != null && !String.IsNullOrWhiteSpace(usersFieldName)) //if the user has named a field to label the value in
            {
                textfield.Text = textValue;
            }
            else//if not, then append the intent to output textbox
            {
                text.Text = intent;
                //DependencyObject child = VisualTreeHelper.GetChild(page, 0);
                //TextBox box = (TextBox)page.F;
            }
            return "done";

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
            
            var response = await bot.SendMessageAndGetIntentFromBot(message);
            messages.Add(new Message() { Text = "  > " + response });
           // determineResponse(response);
            if (speak)
            {
                Debug.WriteLine("starting to speak");
                //await SpeakAsync(response);
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
            await InitSpeech();
            try
            {
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
                    MessageDialog dialog = new MessageDialog("Auf Ihrem Gerät müssen Sie noch die Spracherkennungserlaubnis erteilen.");
                    await dialog.ShowAsync();
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
                    Debug.WriteLine("SpeechInit Failed"+ex.Message);
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
        private async void text_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            TextBox box = (TextBox) sender;
            if(e.Key==Windows.System.VirtualKey.Enter && !string.IsNullOrWhiteSpace(box.Text))
            {
                await SendMessage(box.Text);
                box.Text = "";
            }
        }

       
    }
    public class Message
    {
        public string Text { get; set; }
    }
}
