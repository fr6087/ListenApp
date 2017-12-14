using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListenToMe.Model
{
    public sealed class myPage
    {
        public String destination; //name of page
        public List<String> fields { get; set; } //data from TextBlocks, comboBoxes, TextBoxes, DatePicker
        public List<Boolean> switches { get; set; } //data from ToggleSwitches, two-Options-RadioButtons
        public List<Object> others { get; set; } //other data

        public String toString()
        {
            String textToPrint = "";
            foreach (String s in fields)
            {
                textToPrint += s + " \n";
            }
            textToPrint += "/n";

            foreach (Boolean b in switches)
            {
                textToPrint += b.ToString() + " ";
            }
            textToPrint += "/n";

            return textToPrint;


        }

    }
}
