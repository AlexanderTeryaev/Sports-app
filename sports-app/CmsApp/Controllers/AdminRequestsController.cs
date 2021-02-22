using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsApp.Controllers
{
    public class AdminRequestsController : AdminController
    {
        public ActionResult Index()
        {
            return Json(new { Success = true });
        }


        public ActionResult SectionValidity(int id)
        {
            if (User.IsInAnyRole(AppRole.Admins))
            {
                playersRepo.UpdateMedicalCertificatesToSection(id);
                return RedirectToAction("Success");
            }
            return RedirectToAction("Index", "NotAuthorized");
        }
        public ActionResult UnionValidity(int id)
        {
            if (User.IsInAnyRole(AppRole.Admins))
            {
                playersRepo.UpdateMedicalCertificatesToUnion(id);
                return RedirectToAction("Success");
            }
            return RedirectToAction("Index", "NotAuthorized");
        }

        public ActionResult ValidityAll()
        {
            if (User.IsInAnyRole(AppRole.Admins))
            {
                Startup.RunMedicalCertificateChecks();
                return RedirectToAction("Success");
            }
            return RedirectToAction("Index", "NotAuthorized");
        }
        public ActionResult Success()
        {
            return View();
        }


    }
}