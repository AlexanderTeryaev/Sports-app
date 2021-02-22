using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CmsApp.Helpers;
using LogLigFront.Helpers;

namespace LogLigFront
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ModelBinders.Binders.Add(typeof(DateTime?), new DateModelBinder());

            ClientDataTypeModelValidatorProvider.ResourceClassKey = "Messages";
            DefaultModelBinder.ResourceClassKey = "Messages";
        }

        protected void Application_BeginRequest()
        {
            HttpCookie cookie = Request.Cookies[LocaleHelper.LocaleCookieName];

            if (cookie != null)
            {
                var ci = CultureInfo.GetCultureInfo(cookie.Value);

                //Thread.CurrentThread.CurrentCulture = ci; // new CultureInfo("he-IL");
                Thread.CurrentThread.CurrentUICulture = ci;
            }
            else
            {
                //Thread.CurrentThread.CurrentCulture = new CultureInfo("he-IL");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("he-IL");
            }
        }
    }
}
