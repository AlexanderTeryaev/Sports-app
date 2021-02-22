using AppModel;
using CmsApp.Helpers;
using CmsApp.Models;
using DataService;
using Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsApp.Controllers
{
    public class CoachController : AdminController
    {
        UsersRepo uRepo = new UsersRepo();

        public ActionResult Index(int id, int seasonId)
        {
            var user = uRepo.GetById(id);
            var jobId = user.UsersJobs.Where(c => c.Job.JobsRole.RoleName == JobRole.UnionCoach).FirstOrDefault().Id;
            ViewBag.CoachId = jobId;
            ViewBag.SeasonId = seasonId;
            return View();
        }

        public ActionResult EditCoach(int id, int seasonId)
        {
            var userJob = jobsRepo.GetUsersJobById(id);

            var model = new CoachForm
            {
                SeasonId = seasonId,
                UserJobId = id,
                WithholdingTax = userJob.WithhodlingTax,
                Phone = userJob.User.Telephone,
                Address = userJob.User.Address,
                City = userJob.User.City,
                FirstName = userJob.User.FirstName,
                LastName = userJob.User.LastName,
                MiddleName = userJob.User.MiddleName,
                IdentNum = userJob.User.IdentNum,
                Email = userJob.User.Email,
                BirthDate = userJob.User.BirthDay,
                UserId = userJob.UserId,
                CoachCertificate = userJob.User.RefereeCertificate
            };

            var education = usersRepo.GetEducationByUserId(userJob.UserId);
            if (education != null)
            {
                model.Education = education.Education;
                model.PlaceOfEducation = education.PlaceOfEducation;
                model.DateOfEdIssue = education.DateOfEdIssue;
                model.EducationCert = education.EducationCert;
            }
            if (!string.IsNullOrEmpty(userJob.User.Password))
            {
                model.Password = Protector.Decrypt(userJob.User.Password);
            }
            if (!string.IsNullOrEmpty(userJob.User.RefereeCertificate))
            {
                model.CoachCertificate = userJob.User.RefereeCertificate;
            }
            return PartialView("_EditCoach", model);
        }

        [HttpPost]
        public ActionResult EditCoach(CoachForm model)
        {
            var user = usersRepo.GetById(model.UserId);
            var userJob = jobsRepo.GetUsersJobById(model.UserJobId);
            if (user == null)
            {
                var err = Messages.UserNotExists;
                ModelState.AddModelError("FullName", err);
            }

            if (userJob == null)
            {
                var err = Messages.RoleNotExists;
                ModelState.AddModelError("UserJob", err);
            }

            if (usersRepo.GetByIdentityNumber(model.IdentNum) != null && model.IdentNum != user.IdentNum)
            {
                var tst = Messages.IdIsAlreadyExists;
                tst = String.Format(tst, "\"");
                ModelState.AddModelError("IdentNum", tst);
            }

            if (model.Address == null)
            {
                ModelState.AddModelError("Address",
                    Messages.AddressModelStateError.Replace("{0}", Messages.Workers.ToLowerInvariant()));
            }

            if (ModelState.IsValid)
            {
                var savePath = Server.MapPath(GlobVars.ContentPath + "/coach/");
                if (model.RemoveCoachCert)
                {
                    if (!string.IsNullOrEmpty(user.RefereeCertificate))
                        FileUtil.DeleteFile(savePath + user.RefereeCertificate);
                    user.RefereeCertificate = null;
                }
                else
                {
                    var maxFileSize = GlobVars.MaxFileSize * 1000;
                    var imageFile = GetPostedFile("RefereeCertificateFile");
                    if (imageFile != null)
                    {
                        if (imageFile.ContentLength > maxFileSize)
                        {
                            ModelState.AddModelError("RefereeCertificateFile", Messages.FileSizeError);
                        }
                        else
                        {
                            var newName = SaveFile(imageFile, "img");
                            if (newName == null)
                            {
                                ModelState.AddModelError("RefereeCertificateFile", Messages.FileError);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(user.RefereeCertificate))
                                    FileUtil.DeleteFile(savePath + user.RefereeCertificate);

                                user.RefereeCertificate = newName;
                            }
                        }
                    }
                }
            
                usersRepo.UpdateEducationInfo(user.UserId, model.Education, model.PlaceOfEducation, model.DateOfEdIssue);
                ProcessUserFiles(user, PlayerFileType.EducationCert, model.RemoveEducationCert);
                userJob.WithhodlingTax = model.WithholdingTax;
                user.Address = model.Address;
                user.BirthDay = model.BirthDate;
                user.City = model.City;
                user.Telephone = model.Phone;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.MiddleName = model.MiddleName;
                user.Email = model.Email;
                userJob.WithhodlingTax = model.WithholdingTax;
                user.Password = Protector.Encrypt(model.Password);
                user.IdentNum = model.IdentNum;
                usersRepo.Save();
                jobsRepo.Save();
                return RedirectToAction("Index", "Coach", new { id = AdminId, seasonId = model.SeasonId });
            }
            return PartialView("_EditCoach", model);
        }

        [NonAction]
        private void ProcessUserFiles(User player, PlayerFileType fileType, bool removeFile)
        {
            var playerFile = player?.UsersEducations?.FirstOrDefault();

            if (removeFile)
            {
                if (!string.IsNullOrEmpty(playerFile.EducationCert))
                {
                    FileUtil.DeleteFile(playerFile.EducationCert);
                    playerFile.EducationCert = string.Empty;
                }
                return;
            }

            var maxFileSize = GlobVars.MaxFileSize * 1000;

            var postedFile = Request.Files["EducationCertFile"];

            if (postedFile == null || postedFile.ContentLength == 0) return;

            if (postedFile.ContentLength > maxFileSize)
            {
                ModelState.AddModelError(fileType.ToString(), Messages.FileSizeError);
            }
            else
            {
                var newName = PlayerFileHelper.SaveFile(postedFile, player.UserId, fileType);
                if (newName == null)
                {
                    ModelState.AddModelError(fileType.ToString(), Messages.FileError);
                }
                else
                {
                    if (!string.IsNullOrEmpty(playerFile.EducationCert))
                    {
                        FileUtil.DeleteFile(playerFile.EducationCert);
                        playerFile.EducationCert = string.Empty;
                    }
                    else
                    {
                        playerFile.EducationCert = newName;
                    }
                }
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

            var savePath = Server.MapPath(GlobVars.ContentPath + "/coach/");

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