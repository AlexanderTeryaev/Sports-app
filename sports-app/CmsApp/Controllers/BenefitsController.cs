using AppModel;
using CmsApp.Models;
using CmsApp.Models.Mappers;
using System;
using System.Net;
using System.Linq;
using System.Web.Mvc;
using System.Web;
using Resources;
using System.IO;

namespace CmsApp.Controllers
{
    public class BenefitsController : AdminController
    {
        public ActionResult Index(int unionId, int seasonId)
        {
            return PartialView("List", new BenefitModel { UnionId = unionId, SeasonId = seasonId });
        }

        // GET: Benefits
        public ActionResult List(int unionId, int seasonId)
        {
            BenefitModel bn = new BenefitModel()
            {
                SeasonId = seasonId,
                UnionId = unionId,
                BenefitList = benefitsRepo.GetBenefits(unionId, seasonId)
            };
            ViewBag.IsUnionViewer = usersRepo.GetTopLevelJob(AdminId) == JobRole.Unionviewer;
            return PartialView("_Benefits", bn);
        }

        [HttpGet]
        public ActionResult AddBenefit(int seasonId, int unionId)
        {
            var bf = new BenefitForm
            {
                SeasonId = seasonId,
                UnionId = unionId,
                UnionName = unionsRepo.GetById(unionId).Name
            };

            return PartialView("_AddBenefit", bf);
        }

        [HttpPost]
        public ActionResult AddBenefit(BenefitForm bf)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AddBenefit", bf);
            }
            var e = bf.ToBenefit();

            //upload image and add to benefit
            var maxFileSize = GlobVars.MaxFileSize * 1000;
            HttpPostedFileBase postedFile = GetPostedFile("ImageFile");

            if (postedFile != null && postedFile.ContentLength > 0)
            {
                if (postedFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("ImageFile", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(postedFile, "benefitImg");
                    if (newName == null)
                    {
                        ModelState.AddModelError("ImageFile", Messages.FileError);
                    }
                    else
                    {
                        e.Image = newName;
                    }
                }
            }
            benefitsRepo.Create(e);
            return RedirectToAction("List", new { seasonId = bf.SeasonId, unionId = bf.UnionId });
        }

        [HttpPost]
        public ActionResult UpdateBenefit(BenefitUpdateForm bn)
        {
            var benefitObj = benefitsRepo.GetById(bn.BenefitId);
            var imgChanged = false;
            var imgPath = "";
            //upload image and add to benefit
            var maxFileSize = GlobVars.MaxFileSize * 1000;
            HttpPostedFileBase postedFile = GetPostedFile("ImageFile");
            if (postedFile != null && postedFile.ContentLength > 0)
            {
                if (postedFile.ContentLength > maxFileSize)
                {
                    return Json(new { stat = "failed", id = bn.BenefitId, Message = Messages.FileSizeError });
                }
                else
                {
                    var newName = SaveFile(postedFile, "benefitImg");
                    if (newName == null)
                    {
                        return Json(new { stat = "failed", id = bn.BenefitId, Message = Messages.FileError });
                    }
                    else
                    {
                        benefitObj.Image = newName;
                        imgChanged = true;
                        imgPath = GlobVars.ContentPath + "/Benefits/" + newName;
                    }
                }
            }
            else
            {
                if (bn.RemoveBenefitImageFile) benefitObj.Image = null;
            }

            benefitObj.Title = bn.Title;
            benefitObj.Company = bn.Company;
            benefitObj.Code = bn.Code;
            benefitObj.Description = bn.Description;
            benefitsRepo.Update(benefitObj);
            return Json(new { stat = "ok", id = bn.BenefitId, imgChanged, imgPath });
        }

        public ActionResult DeleteBenefit(int benefitId, int? seasonId, int? unionId)
        {
            benefitsRepo.Delete(benefitId);
            return RedirectToAction("List", new { seasonId = seasonId, unionId = unionId });
        }

        public ActionResult PublishBenefit(int benefitId, bool isPublished)
        {
            try
            {
                var bn = benefitsRepo.GetById(benefitId);
                bn.IsPublished = isPublished;
                benefitsRepo.Update(bn);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, e.ToString());
            }
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

        [NonAction]
        private string SaveFile(HttpPostedFileBase file, string name)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!GlobVars.ValidImages.Contains(ext))
                return null;

            var newName = name + "_" + AppFunc.GetUniqName() + ext;

            var savePath = Server.MapPath(GlobVars.BenefitContentPath);

            var di = new DirectoryInfo(savePath);
            if (!di.Exists)
                di.Create();

            byte[] imgData;
            using (var reader = new BinaryReader(file.InputStream))
            {
                imgData = reader.ReadBytes(file.ContentLength);
            }

            System.IO.File.WriteAllBytes(savePath + newName, imgData);
            return newName;
        }
    }
}