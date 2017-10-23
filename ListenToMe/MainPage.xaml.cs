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
    List<Object> ESF_2_Pages;
        

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

       /* private void loadFormESF_2Pages()
        {
            ESF_2_Pages.Add(this);
            ESF_2_Pages.Add(new CompanyPage());
            ESF_2_Pages.Add(new AddressPage());
            ESF_2_Pages.Add(new LegalFormRegistrationPage());
            ESF_2_Pages.Add(new RegisterEntriesPage());
            ESF_2_Pages.Add(new Pet


        }*/

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
            String site = "https://www.ecosia.org";
            Debug.Write("http://10.150.50.21/irj/portal");
            WebView HTMLWebView = new WebView();
            TopStackPanel.Children.Insert(0, HTMLWebView);
            //mainFrame.
           //HTMLWebView.Navigate("http://contonso.com");
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
