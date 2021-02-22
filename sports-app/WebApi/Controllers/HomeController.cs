using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PushServiceLib;

namespace WebApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Loglig";

            //try
            //{
            //    var pushServ = new PushService(Settings.IsTest);

            //    string idNum = pushServ.GetTokenId(false, "dy51QVz1T6g:APA91bE5lTTSD61Okp7sUyeadJ362AzuoEBW67aK6cWZ6sEFvhkw1p7ZN-hCx9Z-F_Dg8J0ECsSKCamy4iYU9ifcmCsGN3DCXmc0lN95kVs2eVclnWVzpMN5jpwwJy1-HQSNSNyzMt35", "", "", 1);

            //    int id = int.Parse(idNum.Replace("\"", ""));
            //}
            //catch(Exception err)
            //{
            //    string res = "";
            //    if(err.InnerException != null)
            //    {
            //        res = err.InnerException.Message;
            //    }

            //    res += ", " + err.Message;

            //    return Content(res);
            //}

            return RedirectToAction("Index", "Help", new { area = "HelpPage" });
        }
    }
}
