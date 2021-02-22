using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CmsApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Resources", "Resources/Messages.js",
                new { controller = "Resources", action = "GetMessages" });
            routes.MapRoute("apple-app-site-association", "apple-app-site-association",
                new { controller = "Admin", action = "AppleSiteAssociation" });
            routes.MapRoute(".well-known/apple-app-site-association", ".well-known/apple-app-site-association",
                new { controller = "Admin", action = "AppleSiteAssociation" });
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}