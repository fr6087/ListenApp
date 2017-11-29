
using System.Web.Http;


namespace FormBot
{
    public class WebApiApplication : System.Web.HttpApplication //before no "my"
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
