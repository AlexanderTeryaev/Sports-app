using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CmsApp.Helpers;
using CmsApp.Models;

namespace CmsApp.Controllers
{
    public class CommonController : AdminController
    {
        //
        // GET: /Common/

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Login");
        }

        public ActionResult Logged()
        {
            var vm = new LoggedView();
            vm.UserName = CookiesHelper.GetCookieValue("uname");
            vm.RoleType = CookiesHelper.GetCookieValue("utype");

            return PartialView("_Logged", vm);
        }

        public ActionResult Protect()
        {
            var configFile = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/Content");
            var configSection = configFile.Sections["connectionStrings"];
            if (!configSection.SectionInformation.IsProtected)
            {
                configSection.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");
                //configSection.SectionInformation.UnprotectSection();
                //configFile.Save();
            }
            return Content("ok");
        }

        public ActionResult SetCulture(string lang)
        {
            var culture = CultEnum.He_IL; // default

            if (lang == "en")
            {
                culture = CultEnum.En_US;
            }
            else if(lang == "uk")
            {
                culture = CultEnum.Uk_UA;
            }

            this.SetCulture(culture);

            var user = db.Users.Find(AdminId);
            if (user != null)
            {
                user.LastCmsCulture = culture.ToString();
                db.SaveChanges();
            }

            return Redirect(Request.UrlReferrer.ToString());
        }
    }
}
