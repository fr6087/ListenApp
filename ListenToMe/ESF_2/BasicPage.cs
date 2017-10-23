using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ListenToMe.ESF_2
{
    class BasicPage : Page
    {
        protected short pageNumberTest;
        /// <summary>
        /// stores the succession of the pages in ESF_2-Form for OnNavigatedTo-Method
        /// </summary>
        

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Frame.Navigate(typeof(pagesDic.pageNumber));
        }
        
    }
}
