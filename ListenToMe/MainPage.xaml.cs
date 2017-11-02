using ListenToMe.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
            //loadFormESF_2Pages();

        mainFrame.Navigate(typeof(CompanyPage), mainFrame);
    }


        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
    {
    }

    private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
    {
    }
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
            messages = new ObservableCollection<Message>();
            bot = new VoiceCommandService.Bot();
            Media.MediaEnded += Media_MediaEnded;
            await InitContinuousRecognition();

            if (e.Parameter != null && e.Parameter is bool) //if activated with voicecommand?
            {
                var intent = await bot.SendMessageAndGetIntentFromBot("hello there"); //might be later rootobject
                var response = determineResponse(intent);
                messages.Add(new Message() { Text = "  > " + response });
                await SpeakAsync(response);
                await SetListeningAsync(true);
            }
            else if (e.Parameter != null && e.Parameter is string && !string.IsNullOrWhiteSpace(e.Parameter as string))//if activated with text?
            {
                await SendMessage(e.Parameter as string, true);
                await SetListeningAsync(true);
            }
            navigationHelper.OnNavigatedTo(e);
    }
        /// <summary>
        /// simple debugging method
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        private string determineResponse(string intent)
        {
            var page = (CompanyPage)mainFrame.Content;//this is static, todo mke dynamic mainFrame.SourcePageType???
            TextBox box = (TextBox)page.FindName("_11Name");
            box.Text = intent;
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
            determineResponse(response);
            if (speak)
            {
                Debug.WriteLine("starting to speak");
                await SpeakAsync(response);
                Debug.WriteLine("done speaking");

            }

            return response;
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
