using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsApp.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }

        public ActionResult Internal()
        {
            Response.StatusCode = 500;
            return View("Internal");
        }
    }
}