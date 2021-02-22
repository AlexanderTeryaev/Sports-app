using System.IO;
using System.Web.Mvc;

namespace CmsApp.Helpers
{
    public class RazorViewHelper
    {
        public static string RenderRazorViewToString(ControllerContext controllerContext, string viewName, object model)
        {
            controllerContext.Controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var viewContext = new ViewContext(controllerContext, viewResult.View, controllerContext.Controller.ViewData, controllerContext.Controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}