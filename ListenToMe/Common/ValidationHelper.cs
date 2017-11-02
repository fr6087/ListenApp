using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ListenToMe.Common
{
    internal static class ValidationHelper
    {

        internal static bool MandatoryFieldsFilled(TextBox[] boxes)
        {
            foreach (TextBox b in boxes)
            {
                if (String.IsNullOrWhiteSpace(b.Text))
                {
                    b.Background = new SolidColorBrush(Colors.Red);
                    return false;
                }
            }
            return true;
        }
    }

}
