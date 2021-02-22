using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AppModel;
using CmsApp.Models;
using DataService;
using CmsApp.Helpers;

namespace CmsApp.Controllers
{
    public class SettingsController : AdminController
    {
        //
        // GET: /Settings/

        public ActionResult Index()
        {
            var sRepo = new SettingsRepo();
            var item = sRepo.GetById(1);

            int[] intervals = { 7, 15, 30 };
            ViewBag.PushIntervals = new SelectList(intervals);

            return View(item);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Index(Settings frm, HttpPostedFileBase videoFile)
        {
            var sRepo = new SettingsRepo();
            var item = sRepo.GetById(1);

            if(!ModelState.IsValid)
            {
                return View(frm);
            }

            UpdateModel(item);

            if (videoFile != null)
            {
                bool isValidVideo = videoFile.FileName.ToLower().EndsWith(".mp4");
                if (!isValidVideo)
                {
                    ModelState.AddModelError("VideoFile", "נא לבחור וידאו חוקי");
                }

                string savePath = GlobVars.ContentPath + "video/";
                if (!string.IsNullOrEmpty(item.BgVideo))
                {
                    //var util = new FileUtil();
                    //util.DeleteFile(savePath + item.BgVideo);
                }

                string newName = "bg_movie.mp4";
                //videoFile.SaveAs(savePath + newName);
                item.BgVideo = newName;
            }

            sRepo.Save();

            TempData["Saved"] = true;

            return RedirectToAction("Index");
        }

    }
}
