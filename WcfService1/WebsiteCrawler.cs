using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfService1
{
    public class WebsiteCrawler
    {
        static PhantomJSDriver persistingPhantomDriver = new PhantomJSDriver();

        public string getWebContentPersist(string url, string finishedCondition = null)
        {
            string finishedCond = "document.readyState == 'complete'";

            if (finishedCondition != null)
            {
                finishedCond += " && " + finishedCondition;
            }

            persistingPhantomDriver.Navigate().GoToUrl(url);

            IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(persistingPhantomDriver, TimeSpan.FromSeconds(30.00));
            wait.Until(driver1 => ((IJavaScriptExecutor)persistingPhantomDriver).ExecuteScript("return (" + finishedCond + ")").Equals(true));
            /*likely here is some mistake
           persistingPhantomDriver.ExecuteScript(//add afunction to the document that can look for "inn-"-beginning attributes
               @"$.extend($.expr[':'],{
                   checkNonBuildAttibutes: function(e){
                       return /^inn\-/i.test(this.nodeName);
                   }
               });"
           );

           finishedCondition = "checkNonBuildAttributes().Length == 0";
           IWait<IWebDriver> wait2 = new OpenQA.Selenium.Support.UI.WebDriverWait(persistingPhantomDriver, TimeSpan.FromSeconds(30.00));
           wait.Until(driver1 => ((IJavaScriptExecutor)persistingPhantomDriver).ExecuteScript("return (" + finishedCond + ")").Equals(true));
           */
            return persistingPhantomDriver.PageSource;
        }

        public string getWebContent(string url, string finishedCondition = null, List<Cookie> cookies = null)
        {
            using (IWebDriver phantomDriver = new PhantomJSDriver())
            {
                string finishedCond = "document.readyState == 'complete'";

                if (finishedCondition != null)
                {
                    finishedCond += " && " + finishedCondition;
                }

                if(cookies != null)
                {
                    phantomDriver.Navigate().GoToUrl(url);

                    foreach (Cookie cookie in cookies)
                    {
                        phantomDriver.Manage().Cookies.AddCookie(cookie);
                    }
                }

                phantomDriver.Navigate().GoToUrl(url);

                IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(phantomDriver, TimeSpan.FromSeconds(30.00));
                wait.Until(driver1 => ((IJavaScriptExecutor)phantomDriver).ExecuteScript("return (" + finishedCond + ")").Equals(true));
               
                return phantomDriver.PageSource;
            }
        }
    }
}