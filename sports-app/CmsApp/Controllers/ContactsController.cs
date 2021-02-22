using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AppModel;
using CmsApp.Models;
using DataService;
using System.Text;

namespace CmsApp.Controllers
{
    public class ContactsController : AdminController
    {
        //
        // GET: /Contacts/

        ContactsRepo contRepo = new ContactsRepo();

        public ActionResult Index(int? gridNum, string search, bool isArchive = false)
        {
            ViewBag.GridNum = AppFunc.GetGridItemsCombo(gridNum);
            ViewBag.PageSize = gridNum ?? GlobVars.GridItems;

            var query = contRepo.GetQuery(isArchive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.Email.Contains(search)
                    || t.Phone.Contains(search)
                    || t.Company.Contains(search)
                    || t.FullName.Contains(search)
                    || t.LastName.Contains(search));
            }

            query = query.OrderBy(t => t.IsDone).ThenByDescending(t => t.ContactId);

            return View(query);
        }

        public ActionResult Export()
        {
            var query = contRepo.GetQuery(false).OrderByDescending(t => t.ContactId);

            var sb = new StringBuilder();
            var resList = query.ToList();

            sb.Append("<table style='1px solid black; font-size:14px;'>");
            sb.Append("<tr>");
            sb.Append("<td style='width:100px;'><b>שם משפחה</b></td>");
            sb.Append("<td style='width:100px;'><b>שם פרטי</b></td>");
            sb.Append("<td style='width:100px;'><b>מס' טלפון</b></td>");
            sb.Append("<td style='width:140px;'><b>Mail</b></td>");
            sb.Append("<td style='width:100px;'><b>קוד קמפיין</b></td>");
            sb.Append("<td style='width:140px;'><b>שם מפיין</b></td>");
            sb.Append("<td style='width:100px;'><b>תאריך קמפיין</b></td>");
            sb.Append("<td style='width:140px;'><b>תאור מקור פניה</b></td>");
            sb.Append("<td style='width:80px;'><b>סוג מדיה</b></td>");
            sb.Append("<td style='width:50px;'><b>תאור סטטוס</b></td>");
            sb.Append("<td style='width:100px;'><b>תאריך</b></td>");
            //sb.Append("<td style='width:200px;'><b>מקור הגעה</b></td>");
            sb.Append("<td style='width:100px;'><b>מאשר דיוור</b></td>");
            sb.Append("</tr>");

            foreach (var m in resList)
            {
                string cDate = "";
                string cName = "";
                string cRef = "";
                string cCode = "";
                string sType = m.IsMobile ? "מובייל" : "ווב";
                //string refName = "טופס יצירת קשר";
                string getAd = m.IsGetAds ? "כן" : "לא";
                string state = m.IsDone ? "טופל" : "חדש";

                sb.Append("<tr>");
                sb.Append("<td>" + m.LastName + "</td>");
                sb.Append("<td>" + m.FullName + "</td>");
                sb.Append("<td>" + m.Phone + "</td>");
                sb.Append("<td>" + m.Email + "</td>");
                sb.Append("<td>" + cCode + "</td>");
                sb.Append("<td>" + cName + "</td>");
                sb.Append("<td>" + cDate + "</td>");
                sb.Append("<td>" + cRef + "</td>");
                sb.Append("<td>" + sType + "</td>");
                sb.Append("<td>" + state + "</td>");
                sb.Append("<td>" + m.SendDate + "</td>");
                //sb.Append("<td>" + cRef + "</td>");
                sb.Append("<td>" + getAd + "</td>");
                sb.Append("</tr>");
            }

            sb.Append("</table>");

            Response.Clear();
            Response.Buffer = true;
            Response.ContentEncoding = Encoding.Unicode;
            Response.AddHeader("content-disposition", "attachment; filename=contacts.xls");
            Response.ContentType = "application/vnd.ms-excel";
            Response.BinaryWrite(Encoding.Unicode.GetPreamble());

            byte[] buffer = Encoding.Unicode.GetBytes(sb.ToString());
            return File(buffer, "application/vnd.ms-excel");
        }

        public ActionResult Edit(int id)
        {
            var item = contRepo.GetById(id);
            return PartialView("_Edit", item);
        }

        public ActionResult Move(int id, bool archive)
        {
            contRepo.Move(id, archive);
            contRepo.Save();

            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        public ActionResult RemoveAll()
        {
            contRepo.ArchiveAll();
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        public ActionResult Update(int id, bool val)
        {
            var item = contRepo.GetById(id);
            item.IsDone = val;
            item.DoneDate = DateTime.Now;
            contRepo.Save();

            return Content("ok");
        }

    }
}
