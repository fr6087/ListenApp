using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Rest;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Web;

namespace WcfService1
{
    //Memo an mich selbst: das Projekt hat einen anderen Speicherort als die anderen Projekte der Projektmappe ListenToMe
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Klassennamen "Service1" sowohl im Code als 
    //auch in der SVC- und der Konfigurationsdatei ändern.
    // HINWEIS: Wählen Sie zum Starten des WCF-Testclients zum Testen dieses Diensts Service1.svc oder Service1.svc.cs im 
    //Projektmappen-Explorer aus, und starten Sie das Debuggen.
    public class Service1 : IService1
    {
        private WebsiteCrawler wc = new WebsiteCrawler();
        private DirectLineClient _directLine;
        private string _conversationId;
        private string _directLineSecret = ConfigurationManager.AppSettings["DirectLineSecret"];
        private string botId = ConfigurationManager.AppSettings["BotId"];


        public void Login(String _userName, String _password)
        {
            wc.getWebContentPersist("http://10.150.50.21/irj/portal?login_submit=on&login_do_redirect=1&no_cert_storing=on&j_salt=y8C8h6SDoM5Lih9OlYiQa2ACs4c%3D&j_username=" + _userName + "&j_password=" + _password + "&uidPasswordLogon=Anmelden");
        }

        public string GetForm(String domainOfForm)
        {
            //http://10.150.50.21/formularservice/formular/A_FOREX_ANTRAG_ESF_2/appl/d556026e-991d-11e7-9fb1-27c0f1da4ec4/?lang=de (is domainForm)

            String form = wc.getWebContentPersist(domainOfForm + "&backURL=aHR0cCUzQSUyRiUyRjEwLjE1MC41MC4yMSUyRmlyaiUyRnBvcnRhbCUzRk5hdmlnYXRpb25UYXJnZXQlM0RST0xFUyUzQSUyRnBvcnRhbF9jb250ZW50JTJGRVUtRExSX1JlZmFjdG9yaW5nJTJGT0FNX1BPUlRBTF9BUFBMSUNBTlRfSU5ESVZJRFVBTCUyRk9ubGluZUFwcGxpY2F0aW9uQUUlMjZhcHBsaWNhdGlvbklEJTNEODEwMDQ3MTA%3D&transactionID=2dac5e8f-0e58-4069-a4d4-e3892c0ca1f0", "$('#inn').length == 0");
            Debug.WriteLine(form);
            return form;
        }
        public List<string> GetInputs(string username, string password, string domain)
        {
            Login(username, password);
            String form = GetForm(domain);
            List<String> headings = new List<String>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(form);
            var xpath = "//*[self::span[@ng-bind='::text.label']] ";
            foreach (var node in doc.DocumentNode.SelectNodes(xpath))
            {
                String inputLabel = HttpUtility.HtmlDecode(node.InnerHtml);
              
                if(!String.IsNullOrWhiteSpace(inputLabel))
                    headings.Add(node.InnerHtml);

            }
            return headings;
        }
        /*Is returning headings slightly modified, cuts the number from the heading e.g*/
        public List<string> GetHeadings(string username, string password, string domain)
        {
            Login(username, password);
            String form = GetForm(domain);
            List<String> headings = new List<String>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(form);
            var xpath = "//*[self::h3]";// or self::h4 or self::h5 or self::span[@ng-bind='::text.label']] ";
            foreach (var node in doc.DocumentNode.SelectNodes(xpath))
            {
                //this separates the headings into String and Number part
                string headingWithoutOrdinalNumber = HttpUtility.HtmlDecode(node.InnerHtml);
                Regex regex = new Regex("^(\\d+\\.)((.)*)"); //^search Matches at the beginning [d]of type digit that have a following dot after (.) no or * many signs into two groups ()()
                if (regex.IsMatch(headingWithoutOrdinalNumber))
                {
                    MatchCollection matches = regex.Matches(node.InnerHtml);
                    String number = "";
                    foreach (Match match in matches)
                    {
                        Debug.WriteLine("Number:        {0}", match.Groups[1].Value);
                        number = match.Groups[1].Value;
                        Debug.WriteLine("heading: {0}", match.Groups[2].Value);
                        Debug.WriteLine("");
                    }
                    
                    headingWithoutOrdinalNumber = headingWithoutOrdinalNumber.Substring(number.Length).Trim();
                }
                headings.Add(headingWithoutOrdinalNumber);

            }
            return headings;
        }

        /*GetJason returns headings and input fields from Html-Page. This might prove utile when the Luis-Model has to be
         build. Instead of building in manually, on Luis.ai there is a possibility to import entities from Json-Files.*/
        public String GetJason(String username, String password, String domain)
        {
            Login(username, password);
            String form = GetForm(domain);
            String Out = "";
            String JsonNode = "";
            var doc = new HtmlDocument();
            doc.LoadHtml(form);
            var xpath = "//*[self::h3 or self::h4 or self::h5 or self::span[@ng-bind='::text.label']] ";
            foreach (var node in doc.DocumentNode.SelectNodes(xpath))
            {
                Control control = new Control();
                control.Type = node.Name;
                control.Text = node.InnerHtml;
                List<KeyValuePair<String, String>> at_tributes = new List<KeyValuePair<String, String>>();
                foreach (var attribute in node.Attributes)
                {
                    at_tributes.Add(new KeyValuePair<string, string>(attribute.Name, attribute.Value));
                }
                control.Attributes = at_tributes;

                JsonNode = JsonConvert.SerializeObject(control);

                Debug.WriteLine(JsonNode);
            }
            Out += JsonNode;
            return Out;
        }

        private String convertHTMLDocToXMLDoc(HtmlDocument doc)
        {
            doc.OptionOutputAsXml = true;
            System.IO.StringWriter sw = new System.IO.StringWriter();
            System.Xml.XmlTextWriter xw = new System.Xml.XmlTextWriter(sw);
            doc.Save(xw);
            return sw.ToString();
        }

    } 
}
