using System.Web.Mvc;
using CmsApp.Models;
using DataService;
using AppModel;
using System.Threading.Tasks;
using System;
using System.Linq;
using CmsApp.Services;
using System.Web;
using System.Collections.Generic;
using CmsApp.Helpers;
using Resources;

namespace CmsApp.Controllers
{
    public class BannersController : AdminController
    {
        NotesMessagesRepo notesRep = new NotesMessagesRepo();
        SeasonsRepo seasonsRepo = new SeasonsRepo();

        // GET: Notifications
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List(int parentId, bool isUnion)
        {
            var bvm = new BannerModel
            {
                parentId = parentId
            };

            bvm.Banners = new List<BannerForm>();
            if(isUnion)
            {
                bvm.isUnion = 1;
            } else
            {
                bvm.isUnion = 0;
            }

            if (isUnion)
            {
                var banners = db.AdvBanners.Where(x => x.leagueId == parentId).ToList();
                for(int i = 0; i < banners.Count; i++)
                {
                    var bannerItem = new BannerForm();
                    bannerItem.Id = banners[i].id;
                    bannerItem.Count = banners[i].count;
                    bannerItem.ImageUrl = banners[i].image;
                    bannerItem.LinkUrl = banners[i].linkurl;
                    bannerItem.ClubId = banners[i].clubId.HasValue ? banners[i].clubId.Value : 0;
                    bannerItem.UnionId = banners[i].leagueId.HasValue ? banners[i].leagueId.Value : 0;
                    bvm.Banners.Add(bannerItem);
                }
            } else
            {
                var banners = db.AdvBanners.Where(x => x.clubId == parentId).ToList();
                for (int i = 0; i < banners.Count; i++)
                {
                    var bannerItem = new BannerForm();
                    bannerItem.Id = banners[i].id;
                    bannerItem.Count = banners[i].count;
                    bannerItem.ImageUrl = banners[i].image;
                    bannerItem.LinkUrl = banners[i].linkurl;
                    bannerItem.ClubId = banners[i].clubId.HasValue ? banners[i].clubId.Value : 0;
                    bannerItem.UnionId = banners[i].leagueId.HasValue ? banners[i].leagueId.Value : 0;
                    bvm.Banners.Add(bannerItem);
                }
            }
            

            return PartialView("_List", bvm);
        }

        [HttpPost]
        public ActionResult AddBanner(BannerForm frm)
        {
            
            var maxFileSize = GlobVars.MaxFileSize * 1000;
            var savePath = Server.MapPath(GlobVars.ContentPath + "/banners/");

            int paramParent = 0;
            bool paramIsUnion = false;

            AdvBanner addItem = new AdvBanner();
            addItem.linkurl = frm.LinkUrl;
            if(frm.isUnion == 1)
            {
                addItem.leagueId = frm.UnionId;
                paramParent = frm.UnionId;
                paramIsUnion = true;
            } else
            {
                addItem.clubId = frm.ClubId;
                paramParent = frm.ClubId;
                paramIsUnion = false;
            }
            addItem.count = 0;

            if (string.IsNullOrEmpty(frm.LinkUrl))
            {
                return RedirectToAction("List", new { parentId = paramParent, isUnion = paramIsUnion });
            }

            var indexFile = GetPostedFile("BannerFile");
            if(indexFile == null)
            {
                return RedirectToAction("List", new { parentId = paramParent, isUnion = paramIsUnion });
            }
            if (indexFile != null)
            {
                if (indexFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("BannerFile", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(indexFile, "img");
                    if (newName == null)
                    {
                        ModelState.AddModelError("BannerFile", Messages.FileError);
                    }
                    else
                    {
                        addItem.image = newName;
                    }
                }
            }

            db.AdvBanners.Add(addItem);
            db.SaveChanges();

            return RedirectToAction("List", new { parentId = paramParent, isUnion = paramIsUnion });
        }

        [HttpPost]
        public ActionResult UpdateBanner(BannerForm frm)
        {
            var bannerItem = db.AdvBanners.Where(x => x.id == frm.Id).FirstOrDefault();
            var maxFileSize = GlobVars.MaxFileSize * 1000;
            var savePath = Server.MapPath(GlobVars.ContentPath + "/banners/");

            bannerItem.linkurl = frm.LinkUrl;

            var indexFile = GetPostedFile("BannerUpdateFile");
            if (indexFile != null)
            {
                if (indexFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("BannerUpdateFile", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(indexFile, "img");
                    if (newName == null)
                    {
                        ModelState.AddModelError("BannerUpdateFile", Messages.FileError);
                    }
                    else
                    {
                        bannerItem.image = newName;
                    }
                }
            }

            db.SaveChanges();

            int paramParent = 0;
            bool paramIsUnion = false;

            if (frm.isUnion == 1)
            {
                paramParent = frm.UnionId;
                paramIsUnion = true;
            }
            else
            {
                paramParent = frm.ClubId;
                paramIsUnion = false;
            }

            string url = Request.UrlReferrer.AbsolutePath;

            return Redirect(url);
            //return Json(true);
        }

        [HttpPost]
        public JsonResult SaveBanner(BannerForm frm)
        {
            AdvBanner item = db.AdvBanners.Where(x => x.id == frm.Id).FirstOrDefault();
            item.linkurl = frm.LinkUrl;
            db.SaveChanges();

            return Json(new { stat = "ok", id = frm.Id });
        }

        public ActionResult DeleteBanner(int bannerId, int parentId, int isUnion)
        {
            var item = db.AdvBanners.Where(x => x.id == bannerId).FirstOrDefault();
            db.AdvBanners.Remove(item);
            db.SaveChanges();

            return RedirectToAction("List", new { parentId = parentId, isUnion = isUnion == 1 ? true : false});
        }

        [NonAction]
        private string SaveFile(HttpPostedFileBase file, string name)
        {
            string ext = System.IO.Path.GetExtension(file.FileName).ToLower();

            if (!GlobVars.ValidImages.Contains(ext))
                return null;

            string newName = name + "_" + AppFunc.GetUniqName() + ext;

            var savePath = Server.MapPath(GlobVars.ContentPath + "/banners/");

            var di = new System.IO.DirectoryInfo(savePath);
            if (!di.Exists)
                di.Create();

            byte[] imgData;
            using (var reader = new System.IO.BinaryReader(file.InputStream))
            {
                imgData = reader.ReadBytes(file.ContentLength);
            }
            System.IO.File.WriteAllBytes(savePath + newName, imgData);
            return newName;
        }

        [NonAction]
        private HttpPostedFileBase GetPostedFile(string name)
        {
            if (Request.Files[name] == null)
                return null;

            if (Request.Files[name].ContentLength == 0)
                return null;

            return Request.Files[name];
        }

        public ActionResult Edit(int id, int isUnion)
        {
            var banner = db.AdvBanners.Where(x => x.id == id).FirstOrDefault();
            var vm = new BannerForm
            {
                Id = banner.id,
                LinkUrl = banner.linkurl,
                ImageUrl = banner.image,
                isUnion = isUnion,
                ClubId = banner.clubId.HasValue ? banner.clubId.Value : 0,
                UnionId = banner.leagueId.HasValue ? banner.leagueId.Value : 0,
            };

            return PartialView("Edit", vm);
        }
    }
}