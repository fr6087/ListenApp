using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace ListenToMe.ESF_2
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class AddressPage : Page
    {
        
        public AddressPage()
        {
           
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
          //Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested +=
            //ListenToMe.Common.RootFrameNavigationHelper.SystemNavigationManager_BackRequested();

        }

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Debug.WriteLine("called BackButtonRequest");
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;

            // Navigate back if possible, and if the event has not 
            // already been handled .
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        private void buttonGoTo34_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LegalFormRegistrationPage));
        }
    }
}
