using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace ListenToMe.Common
{
    internal static class ValidationHelper
    {

        internal static bool MandatoryFieldsFilled(TextBox[] boxes, string v)
        {
            foreach (TextBox b in boxes)
            {
                if (String.IsNullOrWhiteSpace(b.Text))
                    return false;
            }
            return true;
        }
    }

}
