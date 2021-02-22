using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LogLigFront.Helpers;

namespace LogLigFront.Controllers
{
    public class CommonController : Controller
    {
        public ActionResult SetCulture(string lang)
        {
            string culture = "he-IL"; // default

            if (lang == "en")
            {
                culture = "en-US";
            }
            else if (lang == "uk")
            {
                culture = "uk-UA";
            }

            LocaleHelper.SetCulture(this, culture);

            return Redirect(Request.UrlReferrer.ToString());
        }
    }
}