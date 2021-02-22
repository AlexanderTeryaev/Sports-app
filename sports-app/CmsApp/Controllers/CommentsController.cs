using System.Linq;
using System.Web.Mvc;
using CmsApp.Models;
using DataService;

namespace CmsApp.Controllers
{
    public class CommentsController : AdminController
    {
        CommentsRepo cRepo = new CommentsRepo();
        //
        // GET: /Comments/

        public ActionResult Index(int? gridNum, CommentsSearch frm)
        {
            ViewBag.GridNum = AppFunc.GetGridItemsCombo(gridNum);

            var query = cRepo.GetQuery(false);

            if (frm.DateFrom.HasValue)
                query = query.Where(t => t.AddDate >= frm.DateFrom);

            if (frm.DateTo.HasValue)
                query = query.Where(t => t.AddDate <= frm.DateTo);

            if (!string.IsNullOrWhiteSpace(frm.Search))
                query = query.Where(t => t.Comment.Contains(frm.Search));

            frm.CommentsQuery = query;

            return View(frm);
        }

        public ActionResult DeleteList(int[] arrId)
        {
            if (arrId != null)
            {
                cRepo.RemoveList(arrId);
                cRepo.Save();
            }
            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult Delete(int id)
        {
            cRepo.Remove(id);
            cRepo.Save();

            return Redirect(Request.UrlReferrer.ToString());
        }

    }
}
