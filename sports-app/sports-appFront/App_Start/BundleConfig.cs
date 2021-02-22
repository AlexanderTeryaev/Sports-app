using System.Threading;
using System.Web.Optimization;

namespace LogLigFront
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/main").Include(
                       "~/Scripts/main.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/scheduleScripts").Include(
                        "~/Scripts/bracketsmodified.js",
                        "~/Content/app/schedulesCtrl.js"));
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));


            var style = new StyleBundle("~/style/css");
            style.Include("~/Content/bootstrap.css");
            if (Thread.CurrentThread.CurrentUICulture.Name == Locales.He_IL)
            {
                style.Include("~/Content/css/bootstrap-rtl.css");
            }
            style.Include("~/Content/site.css");

            bundles.Add(style);


        }
    }
}
