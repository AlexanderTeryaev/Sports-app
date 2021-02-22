using System.Web.Optimization;

namespace CmsApp
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                "~/content/js/jquery-{version}.js",
                "~/content/js/jquery-ui.min.js",
                "~/content/js/jquery.validate*",
                "~/content/js/jquery.dataTables.min.js",
                "~/content/js/dataTables.bootstrap.min.js",
                "~/content/js/dataTables.scroller.min.js",
                "~/content/js/jszip.min.js",
                "~/content/js/pdfmake.min.js",
                "~/content/js/vfs_fonts.js",
                "~/content/js/dataTables.buttons.min.js",
                "~/content/js/buttons.flash.min.js",
                "~/content/js/buttons.html5.min.js",
                "~/content/js/buttons.print.min.js",
                "~/content/js/dataTables.colReorder.min.js",
                "~/content/js/jquery-migrate-1.2.1.min.js",
                "~/content/js/jquery.unobtrusive-ajax.js",
                "~/content/typeahead/typeahead.bundle.js",
                "~/content/js/jquery.form.js",
                "~/content/js/messages_en.js",
                "~/content/js/bootstrap.js",
                "~/content/js/gridmvc.js",
                "~/content/js/main.js",
                "~/content/js/bootstrap-switch.js",
                "~/content/js/domJSON.min.js",
                "~/content/js/bootstrap-multiselect.js",
                "~/content/js/moment-with-locales.min.js",
                "~/content/js/bootstrap-editable.min.js",
                "~/content/js/jquery.inputmask.bundle.js",
                "~/content/js/jquery.lazy.min.js"
            ));

            bundles.Add(new StyleBundle("~/content/style/css").Include(
                "~/content/css/bootstrap.css",
                "~/content/css/bootstrap-theme.css",
                "~/content/css/Gridmvc.css",
                "~/content/typeahead/typeahead-bootstrap.css",
                "~/content/css/dataTables.bootstrap.min.css",
                "~/content/css/buttons.dataTables.min.css",
                "~/content/css/colReorder.dataTables.min.css",
                "~/content/css/style.css",
                "~/content/css/bootstrap-switch.css",
                "~/content/css/bootstrap-multiselect.css",
                "~/content/css/bootstrap-editable.css"
            ));

            /* Localization */

            bundles.Add(new ScriptBundle("~/bundles/js/he").Include(
                "~/content/js/messages_he.js"));

            bundles.Add(new StyleBundle("~/content/style/rtl").Include(
                "~/content/css/bootstrap-rtl.css",
                "~/content/css/rtl-fix.css"));
        }
    }
}
