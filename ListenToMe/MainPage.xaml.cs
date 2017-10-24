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

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace ListenToMe
{
    public partial class MainPage : Page { 
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    NavigationHelper navigationHelper;
        

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
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        navigationHelper.OnNavigatedTo(e);
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
    }
}
