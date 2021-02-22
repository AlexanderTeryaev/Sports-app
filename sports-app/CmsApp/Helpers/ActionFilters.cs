using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

public class ActionFilters
{
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class NoCacheAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext fc)
    {
        fc.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
        fc.HttpContext.Response.Cache.SetValidUntilExpires(false);
        fc.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
        fc.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        fc.HttpContext.Response.Cache.SetNoStore();

        base.OnResultExecuting(fc);
    }
}

public class JsonRequestBehaviorAttribute : ActionFilterAttribute
{
    private JsonRequestBehavior Behavior { get; set; }

    public JsonRequestBehaviorAttribute()
    {
        Behavior = JsonRequestBehavior.AllowGet;
    }

    public override void OnResultExecuting(ResultExecutingContext filterContext)
    {
        var result = filterContext.Result as JsonResult;

        if (result != null)
        {
            result.JsonRequestBehavior = Behavior;
        }
    }
}