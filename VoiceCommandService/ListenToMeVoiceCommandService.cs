
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources.Core;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;

namespace ListenToMe.VoiceCommands
{
    public sealed class ListenToMeVoiceCommandService : IBackgroundTask
    {
        /// <summary>
        /// the service connection is maintained for the lifetime of a cortana session, once a voice command
        /// has been triggered via Cortana.
        /// </summary>
        VoiceCommandServiceConnection voiceServiceConnection;

        /// <summary>
        /// Lifetime of the background service is controlled via the BackgroundTaskDeferral object, including
        /// registering for cancellation events, signalling end of execution, etc. Cortana may terminate the 
        /// background service task if it loses focus, or the background task takes too long to provide.
        /// 
        /// Background tasks can run for a maximum of 30 seconds.
        /// </summary>
        private BackgroundTaskDeferral backgroundTaskDeferral;
        private AppServiceConnection appServiceconnection;
        private String[] inventoryItems = new string[] { "Robot vacuum", "Chair" };
        private double[] inventoryPrices = new double[] { 129.99, 88.99 };

        /// <summary>
        /// ResourceMap containing localized strings for display in Cortana.
        /// </summary>
        ResourceMap cortanaResourceMap;

        /// <summary>
        /// The context for localized strings.
        /// </summary>
        ResourceContext cortanaContext;

        /// <summary>
        /// Get globalization-aware date formats.
        /// </summary>
        DateTimeFormatInfo dateFormatInfo;
        /// <summary>
        /// The background task entrypoint. 
        /// 
        /// Background tasks must respond to activation by Cortana within 0.5 seconds, and must 
        /// report progress to Cortana every 5 seconds (unless Cortana is waiting for user
        /// input). There is no execution time limit on the background task managed by Cortana,
        /// but developers should use plmConsole (https://msdn.microsoft.com/library/windows/hardware/jj680085%28v=vs.85%29.aspx)
        /// on the Cortana app package in order to prevent Cortana timing out the task during
        /// Consoleging.
        /// 
        /// The Cortana UI is dismissed if Cortana loses focus. 
        /// The background task is also dismissed even if being Consoleged. 
        /// Use of Remote Consoleging is recommended in order to Console background task behaviors. 
        /// Open the project properties for the app package (not the background task project), 
        /// and enable Console -> "Do not launch, but Console my code when it starts". 
        /// Alternatively, add a long initial progress screen, and attach to the background task process while it executes.
        /// </summary>
        /// <param name="taskInstance">Connection to the hosting background service process.</param>

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            backgroundTaskDeferral = taskInstance.GetDeferral();
            Debug.WriteLine("running VoiceCommandService");
            

            // Register to receive an event if Cortana dismisses the background task. This will
            // occur if the task takes too long to respond, or if Cortana's UI is dismissed.
            // Any pending operations should be cancelled or waited on to clean up where possible.
            taskInstance.Canceled += OnTaskCanceled;

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            Debug.WriteLine("We have some TriggerDetails named " + triggerDetails.Name);

            // Load localized resources for strings sent to Cortana to be displayed to the user.
            cortanaResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");

            // Select the system language, which is what Cortana should be running as.
            cortanaContext = ResourceContext.GetForViewIndependentUse();

            // Get the currently used system date format
            dateFormatInfo = CultureInfo.CurrentCulture.DateTimeFormat;

            // This should match the uap:AppService and VoiceCommandService references from the 
            // package manifest and VCD files, respectively. Make sure we've been launched by
            // a Cortana Voice Command.
            if (triggerDetails != null && triggerDetails.Name == "VoiceCommandService")
            {
                Debug.WriteLine("received something spoken");
                try
                {
                    voiceServiceConnection =
                        VoiceCommandServiceConnection.FromAppServiceTriggerDetails(
                            triggerDetails);

                    voiceServiceConnection.VoiceCommandCompleted += OnVoiceCommandCompleted;

                    // GetVoiceCommandAsync establishes initial connection to Cortana, and must be called prior to any 
                    // messages sent to Cortana. Attempting to use ReportSuccessAsync, ReportProgressAsync, etc
                    // prior to calling this will produce undefined behavior.
                    VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();
                    Console.Write("received voicecommand" + voiceCommand.ToString());
                    // Depending on the operation (defined in AdventureWorks:AdventureWorksCommands.xml
                    // perform the appropriate command.
                    performCommand(voiceCommand.CommandName);
                }
                catch (Exception ex)
                {
                    Debug.Fail("Handling Voice Command Console.Error.WriteLineAsynced " + ex.ToString());
                }
            }
        }
        private async void performCommand(String voiceCommand)
        {
            switch (voiceCommand)
            {
                case "Shutdown":
                    //var destination = voiceCommand.Properties["destination"][0];
                    Debug.WriteLine("Got wrc Shutdown ");
                    //await SendCompletionMessageForDestinationAsync(destination);
                    break;
                case "Edit":
                    MessageDialog message = new MessageDialog("VoiceCommandService, Edit called");
                    await message.ShowAsync();
                    //var cancelDestination = voiceCommand.Properties["destination"][0];
                    //await SendCompletionMessageForCancellationAsync(cancelDestination);
                    Debug.WriteLine("Got wrc Edit ");
                    break;
                case "Information":
                    Debug.WriteLine("Information at VoiceCommandService isn't implemented.");
                    break;
                case "Page":
                    //var pageName = voiceCommand.Properties["destination"][0];
                    Debug.WriteLine("Got wrc Page ");
                    //await SendCompletionMessageForCancellationAsync(pageName);
                    break;
                case "Upload":
                    Debug.WriteLine("Got wrc Upload ");
                    //var uploadFile = voiceCommand.Properties["destination"][0];
                    // await SendCompletionMessageForCancellationAsync(uploadFile);
                    break;
                default:

                    Debug.WriteLine("Got nothing. starting app default ");
                    // As with app activation VCDs, we need to handle the possibility that
                    // an app update may remove a voice command that is still registered.
                    // This can happen if the user hasn't run an app since an update.
                    LaunchAppInForegroundAsync();
                    break;
            }
        }
        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // Get a deferral because we use an awaitable API below to respond to the message
            // and we don't want this call to get cancelled while we are waiting.
            var messageDeferral = args.GetDeferral();

            ValueSet message = args.Request.Message;
            ValueSet returnData = new ValueSet();

            string command = message["Command"] as string;
            int? inventoryIndex = message["ID"] as int?;

            if (inventoryIndex.HasValue &&
                 inventoryIndex.Value >= 0 &&
                 inventoryIndex.Value < inventoryItems.GetLength(0))
            {
                switch (command)
                {
                    case "Price":
                        {
                            returnData.Add("Result", inventoryPrices[inventoryIndex.Value]);
                            returnData.Add("Status", "OK");
                            break;
                        }

                    case "Item":
                        {
                            returnData.Add("Result", inventoryItems[inventoryIndex.Value]);
                            returnData.Add("Status", "OK");
                            break;
                        }

                    default:
                        {
                            returnData.Add("Status", "Console.Error.WriteLineAsync: unknown command");
                            break;
                        }
                }
            }
            else
            {
                returnData.Add("Status", "Console.Error.WriteLineAsync: Index out of range");
            }

            try
            {
                await args.Request.SendResponseAsync(returnData); // Return the data to the caller.
            }
            catch (Exception e)
            {
                // your exception handling code here
                Debug.Fail(e.Message);
            }
            finally
            {
                // Complete the deferral so that the platform knows that we're done responding to the app service call.
                // Note for error handling: this must be called even if SendResponseAsync() throws an exception.
                messageDeferral.Complete();
            }
        }

        /* private async Task SendCompletionMessageForDestinationAsync(string destination)
         {
             // If this operation is expected to take longer than 0.5 seconds, the task must
             // provide a progress response to Cortana prior to starting the operation, and
             // provide updates at most every 5 seconds.
             string loadingPageToEdit = string.Format(
                        cortanaResourceMap.GetValue("LoadingPageToEdit", cortanaContext).ValueAsString,//vormals LoadingTripToDestination
                        destination);
             await ShowProgressScreenAsync(loadingPageToEdit);
             //Model.TripStore store = new Model.TripStore(); 
             //ListenToMe.Model.PageStore store = new ListenToMe.Model.PageStore();
             //await store.LoadTrips();
             //await store.LoadPages();

             // Look for the specified trip. The destination *should* be pulled from the grammar we
             // provided, and the subsequently updated phrase list, so it should be a 1:1 match, including case.
             // However, we might have multiple trips to the destination. For now, we just pick the first.
             //IEnumerable<Model.Trip> trips = store.Trips.Where(p => p.Destination == destination);
             //IEnumerable<ListenToMe.Model.Page> pages = store.Pages.Where(page => page.destination == destination);
             var userMessage = new VoiceCommandUserMessage();
             var destinationsContentTiles = new List<VoiceCommandContentTile>();
             if (pages.Count() == 0)
             {
                 // In this scenario, perhaps someone has modified data on your service outside of your 
                 // control. If you're accessing a remote service, having a background task that
                 // periodically refreshes the phrase list so it's likely to be in sync is ideal.
                 // This is unlikely to occur for this sample app, however.
                 string foundNoPageToEdit = string.Format(
                        cortanaResourceMap.GetValue("FoundNoPageToEdit", cortanaContext).ValueAsString,//vormals FoundNoTripToDestination
                        destination);
                 userMessage.DisplayMessage = foundNoPageToEdit;
                 userMessage.SpokenMessage = foundNoPageToEdit;
             }
             else
             {
                 // Set a title message for the page.
                 string message = "";
                 //if (pages.Count() > 1)
                 //{
                 message = cortanaResourceMap.GetValue("PluralUpcomingPages", cortanaContext).ValueAsString;
                 /*}
                 else
                 {
                     message = cortanaResourceMap.GetValue("SingularUpcomingTrip", cortanaContext).ValueAsString;
                 }
                 userMessage.DisplayMessage = message;
                 userMessage.SpokenMessage = message;

                 // file in tiles for each destination, to display information about the trips without
                 // launching the app.
                 foreach (ListenToMe.Model.Page page in pages)
                 {
                     int i = 1;

                     var destinationTile = new VoiceCommandContentTile();

                     // To handle UI scaling, Cortana automatically looks up files with FileName.scale-<n>.ext formats based on the requested filename.
                     // See the VoiceCommandService\Images folder for an example.
                     destinationTile.ContentTileType = VoiceCommandContentTileType.TitleWith68x68IconAndText;
                     //destinationTile.Image = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///AdventureWorks.VoiceCommands/Images/GreyTile.png"));

                     destinationTile.AppLaunchArgument = page.destination;
                     //toDo: find out wht the app is doing over here; why it does find Startdate
                     destinationTile.Title = page.destination;
                     /*if (page.StartDate != null)
                     {
                         destinationTile.TextLine1 = page.StartDate.Value.ToString(dateFormatInfo.LongDatePattern);
                     }
                     else
                     {
                     destinationTile.TextLine1 = page.destination + " " + i;
                     //}

                     destinationsContentTiles.Add(destinationTile);
                     i++;
                 }
             }

             var response = VoiceCommandResponse.CreateResponse(userMessage, destinationsContentTiles);

             /*if (pages.Count() > 0)
             {
                 response.AppLaunchArgument = destination;
             }

                 await voiceServiceConnection.ReportSuccessAsync(response);
             }*/

        /// <summary>
        /// search for and find single page in cortana
        /// </summary>
        /// <param name="uploadFile"></param>
        /// <returns></returns>
        //private async Task SendCompletionMessageForCancellationAsync(string destination)
        //{
        // If this operation is expected to take longer than 0.5 seconds, the task must
        // provide a progress response to Cortana prior to starting the operation, and
        // provide updates at most every 5 seconds.
        /*string loadingPageToDestination = string.Format(
                   cortanaResourceMap.GetValue("LoadingTripToDestination", cortanaContext).ValueAsString,
                   destination);
        await ShowProgressScreenAsync(loadingPageToDestination);
        ListenToMe.Model.PageStore store = new ListenToMe.Model.PageStore();
        await store.LoadPages();

        // Look for the specified trip. The destination *should* be pulled from the grammar we
        // provided, and the subsequently updated phrase list, so it should be a 1:1 match, including case.
        // However, we might have multiple trips to the destination. For now, we just pick the first.
        IEnumerable<ListenToMe.Model.Page> pages = store.Pages.Where(p => p.destination == destination);

        var userMessage = new VoiceCommandUserMessage();
        var destinationsContentTiles = new List<VoiceCommandContentTile>();
        if (pages.Count() == 0)
        {
            // In this scenario, perhaps someone has modified data on your service outside of your 
            // control. If you're accessing a remote service, having a background task that
            // periodically refreshes the phrase list so it's likely to be in sync is ideal.
            // This is unlikely to occur for this sample app, however.
            string foundNoTripToDestination = string.Format(
                   cortanaResourceMap.GetValue("FoundNoPageToDestination", cortanaContext).ValueAsString,
                   destination);
            userMessage.DisplayMessage = foundNoTripToDestination;
            userMessage.SpokenMessage = foundNoTripToDestination;
        }
        else
        {
            // Set a title message for the page.
            string message = "";
            if (pages.Count() > 1)
            {
                message = cortanaResourceMap.GetValue("PluralUpcomingPages", cortanaContext).ValueAsString;
            }
            else
            {
                message = cortanaResourceMap.GetValue("SingularUpcomingPage", cortanaContext).ValueAsString;
            }
            userMessage.DisplayMessage = message;
            userMessage.SpokenMessage = message;

            // file in tiles for each destination, to display information about the trips without
            // launching the app.
            foreach (ListenToMe.Model.Page page in pages)
            {
                int i = 1;

                var destinationTile = new VoiceCommandContentTile();

                // To handle UI scaling, Cortana automatically looks up files with FileName.scale-<n>.ext formats based on the requested filename.
                // See the VoiceCommandService\Images folder for an example.
                destinationTile.ContentTileType = VoiceCommandContentTileType.TitleWith68x68IconAndText;
                //destinationTile.Image = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///AdventureWorks.VoiceCommands/Images/GreyTile.png"));

                destinationTile.AppLaunchArgument = page.destination;
                destinationTile.Title = page.destination;
                /*if (page.StartDate != null) find out about motive here
                {
                    destinationTile.TextLine1 = page.StartDate.Value.ToString(dateFormatInfo.LongDatePattern);
                }
                else
                {
                destinationTile.TextLine1 = page.destination + " " + i;
                //}

                destinationsContentTiles.Add(destinationTile);
                i++;
            }
        }

        var response = VoiceCommandResponse.CreateResponse(userMessage, destinationsContentTiles);

        if (pages.Count() > 0)
        {
            response.AppLaunchArgument = destination;
        }

        await voiceServiceConnection.ReportSuccessAsync(response);*/
        // }

        private async Task ShowProgressScreenAsync(string message)
            {
                var userProgressMessage = new VoiceCommandUserMessage();
                userProgressMessage.DisplayMessage = userProgressMessage.SpokenMessage = message;

                VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userProgressMessage);
                await voiceServiceConnection.ReportProgressAsync(response);
            }

            /// <summary>
            /// wenn die app unterbrochen wird oder sonstig beendet, räume netzwerkverbindungen (u.a.) auf
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="args"></param>
            private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
            {
                if (this.backgroundTaskDeferral != null)
                {
                    this.backgroundTaskDeferral.Complete();
                }
            }

            private async void LaunchAppInForegroundAsync()
            {
            Debug.WriteLine("called VCS. LaunchAppinForeground");
                var nextMessage = new VoiceCommandUserMessage();
                nextMessage.SpokenMessage = cortanaResourceMap.GetValue("LaunchingListenToMe", cortanaContext).ValueAsString;

                var response = VoiceCommandResponse.CreateResponse(nextMessage);

                response.AppLaunchArgument = "";

                await voiceServiceConnection.RequestAppLaunchAsync(response);
            }

            private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
            {
                Debug.WriteLine("Task cancelled, clean up");
                if (this.backgroundTaskDeferral != null)
                {
                    //Complete the service deferral
                    this.backgroundTaskDeferral.Complete();
                }
            }
        }
    }






