using System.Web.Mvc;

namespace CmsApp.Helpers.ModelStateAttributes
{
    public class SetTempDataModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            filterContext.Controller.TempData["ModelState"] =
                filterContext.Controller.ViewData.ModelState;
        }
    }
}