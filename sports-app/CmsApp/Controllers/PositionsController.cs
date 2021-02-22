using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Omu.ValueInjecter;
using CmsApp.Models;
using AppModel;
using DataService;

namespace CmsApp.Controllers
{
    public class PositionsController : AdminController
    {
        PositionsRepo repo = new PositionsRepo();

        // GET: Positions
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List(int id, int posId = 0)
        {
            var vm = new PositionsForm { SectionId = id };
            vm.Positions = repo.GetBySection(id);

            if(posId != 0)
            {
                var item = repo.GetById(posId);
                vm.InjectFrom(item);
            }

            return PartialView("_List", vm);
        }

        public ActionResult Delete(int id)
        {
            var item = repo.GetById(id);
            item.IsArchive = true;

            repo.Save();

            return RedirectToAction("List", new { id = item.SectionId });
        }

        [HttpPost]
        public ActionResult Save(PositionsForm item)
        {
            var pos = new Position();

            if (item.PosId == 0)
                repo.Create(pos);
            else
                pos = repo.GetById(item.PosId);

            UpdateModel(pos);

            repo.Save();

            return RedirectToAction("List", new { id = pos.SectionId });
        }
    }
}