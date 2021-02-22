using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            GlobalConfiguration.Configuration.EnsureInitialized(); 
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Request.HttpMethod == "OPTIONS")
            {
                Response.AddHeader("Access-Control-Allow-Origin", "*");
                Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                Response.AddHeader("Access-Control-Max-Age", "1728000");
                Response.AddHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Key, Authorization, AuthenticationType, _culture, id");
                Response.End();
            }
        }

        protected void Application_BeginRequest()
        {
            string header = Request.Headers["_culture"];
            if (!string.IsNullOrEmpty(header))
            {
                var ci = CultureInfo.GetCultureInfo(header);

                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }
        }
    }
}
