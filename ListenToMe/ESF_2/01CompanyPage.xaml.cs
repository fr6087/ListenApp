using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Input;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace ListenToMe.ESF_2
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class CompanyPage : Page
    {
        
        public CompanyPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            //Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested +=
    //App_BackRequested;

        }


        private async void buttonGoTo2_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (ValidatePage())
            {
                Frame.Navigate(typeof(AddressPage));
            }
            else
            {
                await new MessageDialog("please correct input.").ShowAsync();
            }
            
        }

        private Boolean ValidatePage()
        {
            /*String name = "^[0-9a-zA-Z]{1,50}$";//alphanum that is 1 to fifty signs
            String email = "^[A-Z0-9._%+-]{1,50}@[A-Z0-9.-].[A-Z]{2,}$";
            if (checkField(name, _11Name) && checkField(email, _13email))
            {
                return true;
            }
            return false;*/
            return true;
        }

        private Boolean checkField(String ex, object sender)
        {
            Regex filter = new Regex(ex.Trim());
            var textBoxSender = (TextBox)sender;
            var red = new SolidColorBrush(Colors.Red);
            if (!filter.IsMatch(textBoxSender.Text))
            {

                textBoxSender.Background = red;
                return false;
            }
            return true;

        }

        private void Some_AlphaNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            String regex = "";
            var textbox = (TextBox)sender;
            if (textbox.Name.Equals(_11Name)){
                regex += "[0-9a-zA-Z]";
               
                App.Filter_Changed(sender, e, regex);
                
            }
            else if (textbox.Name.Equals(_13email))
            {
                regex += "[A-Z0-9._%+-]{1,50}@[A-Z0-9.-].[A-Z]{2,}";
               
                App.Filter_Changed(sender, e, regex);
            }
        }

        /// <summary>
        /// does not work as well
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Name_KeyDown(TextBox sender, KeyRoutedEventArgs e)
        {
            /*if (!(((e.KeyCode < Keys.NumPad0) || (e.KeyCode > Keys.NumPad9)) && ((e.KeyCode < Keys.A) || (e.KeyCode > Keys.E))))
            {
                e.SuppressKeyPress = false;
            }
            else
            {
                e.SuppressKeyPress = true;
            }
            */


        }
    }
}
