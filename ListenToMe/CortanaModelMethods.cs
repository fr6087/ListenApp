using ListenToMe.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;

namespace ListenToMe
{
    class CortanaModelMethods : IModelMethods
    {

        internal ObservableCollection<string> headings= new ObservableCollection<string>();
        internal ObservableCollection<string> inputs = new ObservableCollection<string>();

        /// <summary>
        /// reference TripViewModel.UpdateDestinationPhraseList in AdventureWorks project
        /// called when App Activates VoiceCommands, this method fills the collections of tags from the html Form and updates
        /// the VoiceCommand-xml File
        /// </summary>
        public async Task<List<String>> UpdatePhraseList(String phraselistName)
        {
            try
            {
                // Update the destination phrase list, so that Cortana voice commands can use destinations added by users.
                // When saving a trip, the UI navigates automatically back to this page, so the phrase list will be
                // updated automatically.
                VoiceCommandDefinition commandDefinitions;

                string countryCode = CultureInfo.CurrentCulture.Name.ToLower();
                if (countryCode.Length == 0)
                {
                    countryCode = "de";//en-us
                }
                else
                {
                    Debug.WriteLine("COUNTRYCODE: " + countryCode);
                }
                Debug.WriteLine(VoiceCommandDefinitionManager.InstalledCommandDefinitions.Count());

                var dic = VoiceCommandDefinitionManager.InstalledCommandDefinitions;
                foreach(var keyValPair in dic)
                {
                    Debug.WriteLine(keyValPair.Key + keyValPair.Value);
                }
                if (VoiceCommandDefinitionManager.InstalledCommandDefinitions.TryGetValue("ListenToMeCommandSet_de", out commandDefinitions))
                {
                    Service1Client client = new Service1Client();
                    ObservableCollection<String> observable = new ObservableCollection<string>();
                    if (phraselistName.Equals("Page"))
                    {
                        headings = await client.GetHeadingsAsync(App.userName, App.userPassword, App.uri);
                        observable = headings;
                    }   
                    else if (phraselistName.Equals("Field"))
                    {
                        inputs = await client.GetInputsAsync(App.userName, App.userPassword, App.uri);
                        observable = inputs;
                    }
                    List<string> items = observable.ToList();
                    //toDo: extract this from client
                    foreach (String item in items)
                    {
                        Debug.WriteLine(item);

                    }
                    await commandDefinitions.SetPhraseListAsync(phraselistName, items);
                    return items;

                }
                return null; //unfortunately this returns null somewhen

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Updating Phrase list for VCDs: " + ex.ToString());
                return null;
            }
        }
    }
}
