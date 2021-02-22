using AppModel;
using ClosedXML.Excel;
using CmsApp.Helpers;
using CmsApp.Helpers.Injections;
using CmsApp.Models;
using Omu.ValueInjecter;
using Resources;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CmsApp.Controllers
{
    public class RegionalController : AdminController
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public void ExportRegionalsList(int id, int seasonId)
        {
            if (!IsRegionalFederationEnabled(id, 0, seasonId))
                return;

            var regionals = regionalsRepo.GetRegionalsByUnionAndSeason(id, seasonId);

            var regionalsIds = regionals.Select(x => x.RegionalId).ToArray();
            var regionalManagers = db.UsersJobs.Where(x => regionalsIds.Contains(x.RegionalId ?? 0)).AsNoTracking().ToList();

            var unionName = unionsRepo.GetById(id)?.Name ?? string.Empty;

            using (var workbook =
                new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var ws = workbook.AddWorksheet(Messages.RegionalList);
                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });
                addCell("#");
                addCell(Messages.Name);
               // addCell(RegionalConstants.IsArchived);
                addCell(Messages.RegionalManager);
                addCell(Messages.Email);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                foreach (var row in regionals)
                {
                    var regionalManager = regionalManagers.FirstOrDefault(x => x.RegionalId == row.RegionalId)?.User;

                    addCell(row.RegionalId.ToString());
                    addCell(row.Name);
                    //   addCell(row.IsArchive.ToString());
                    addCell(regionalManager?.FullName);
                    addCell(regionalManager?.Email);
                 //   addCell(unionName);

                    rowCounter++;
                    columnCounter = 1;
                }

                ws.Columns().AdjustToContents();

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                Response.AddHeader("content-disposition",
                    $"attachment;filename= {unionName?.Replace(' ', '_')}_{Messages.RegionalList.ToLower().Replace(" ", "_")}.xlsx");

                using (var myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        [HttpPost]
        public ActionResult Save(RegionalsForm model)
        {
            var regional = new Regional
            {
                UnionId = model.UnionId > 0 ? model.UnionId : null,
                SeasonId = model.SeasonId > 0 ? model.SeasonId : null
            };

            if (!IsRegionalFederationEnabled(0, model.UnionId ?? 0, model.SeasonId ?? 0))
            {
                return RedirectToAction("Edit", "Unions", new { id = model.UnionId, seasonId = model.SeasonId });
            }

            if (regional.SeasonId == null)
            {
                var currSeason = seasonsRepository.GetLastSeasonByUnionId(model.UnionId ?? 0);
                regional.SeasonId = currSeason?.Id;
            }

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                if (model.RegionalId.HasValue)
                {
                    regional = regionalsRepo.GetById(model.RegionalId.Value);
                    regional.Name = model.Name;
                    UpdateModel(regional);
                    regionalsRepo.Save();
                }
                else if (model.UnionId.HasValue)
                {
                    regional.Name = model.Name;
                    UpdateModel(regional);
                    regionalsRepo.AddRegional(regional);
                }
                else
                {
                    regional.Name = model.Name;
                    UpdateModel(regional);
                    regional.SeasonId = null;
                    regionalsRepo.AddRegional(regional);
                }
            }
            return RedirectToRegionalList(regional);
        }

        private ActionResult RedirectToRegionalList(Regional item)
        {
            if (item.UnionId.HasValue && item.SeasonId.HasValue)
            {
                return RedirectToAction(nameof(ListRegional), new { id = item.UnionId, seasonId = item.SeasonId });
            }
            else if (item.SeasonId.HasValue)
            {
                return RedirectToAction(nameof(ListRegional), new { id = 0, seasonId = item.SeasonId });
            }
            else
            {
                return RedirectToAction(nameof(ListRegional), new { id = item.UnionId, seasonId = 0 });
            }
        }

        public ActionResult Delete(int id)
        {
            var regional = regionalsRepo.GetById(id);

            if (regional != null)
            {
                regionalsRepo.Delete(id);

                return RedirectToRegionalList(regional);
            }

            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult Update(int regionalID, string name)
        {
            var item = regionalsRepo.GetById(regionalID);
            item.Name = name;
            regionalsRepo.Save();

            TempData["SavedId"] = regionalID;

            return RedirectToRegionalList(item);
        }

        public ActionResult Edit(int id, int? seasonId, int? unionId)
        {
            var regional = regionalsRepo.GetById(id);

            //  SetIsSectionClubLevel(club.UnionId == null);
            // SetCurrentClubId(id);

            // ViewBag.KarateMesssage = TempData["KarateClubMessage"]?.ToString();

            seasonId = seasonId.GetValueOrDefault(0) == 0 ? (int?)null : seasonId.Value;

            var isUnionClubManager = (usersRepo.GetTopLevelJob(AdminId)?.Equals(JobRole.ClubManager) == true || usersRepo.GetTopLevelJob(AdminId)?.Equals(JobRole.ClubSecretary) == true) && regional.UnionId.HasValue;

            if (seasonId != null)
            {
                if (regional.UnionId == null)
                {
                    SetClubCurrentSeason(seasonId.Value);
                }
                else
                {
                    SetUnionCurrentSeason(seasonId.Value);
                }
            }

            if (regional.IsArchive)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var unionName = unionsRepo.GetById(regional.UnionId.Value)?.Name ?? string.Empty;

            // int currSeasonID = seasonsRepository.GetLastSeasonByUnionId(regional.UnionId.Value)?.Id ?? -1;

            var viewModel = new EditRegionalViewModel();
            {
                viewModel.Id = id;
                viewModel.Name = regional.Name;
                viewModel.UnionId = regional.UnionId;
                viewModel.UnionName = unionName;
                viewModel.SeasonId = seasonId;
                viewModel.CurrentSeasonId = seasonId ?? (seasonsRepository.GetLastSeasonByUnionId(regional.UnionId.Value)?.Id ?? -1);
                viewModel.CurrentSeasonName = seasonId.HasValue ? seasonsRepository.GetById(seasonId.Value).Name : "";
                // Seasons = regional.SeasonId > 0  ? seasonsRepository.GetClubsSeasons(id, false) : new List<Season>(),

            };

            var roleName = usersRepo.GetTopLevelJob(AdminId);

            viewModel.CanEdit = true;
            var jobRole = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.JobRole = jobRole;

            var view = View(viewModel);
            return view;
        }

        public ActionResult Details(int id)
        {
            var item = regionalsRepo.GetById(id);

            var vm = new RegionalDetailsForm
            {
                RegionalId = item.RegionalId,
                CreateDate = item.CreateDate,
                Address = item.Address,
                Description = item.Description,
                Email = item.Email,
                IndexAbout = item.IndexAbout,
                IndexImage = item.IndexImage,
                IsArchive = item.IsArchive,
                Logo = item.Logo,
                Name = item.Name,
                Phone = item.Phone,
                PrimaryImage = item.PrimaryImage,
                SeasonId = item.SeasonId,
                UnionId = item.UnionId,
                Culture = getCulture(),
                UnionFormTitle = item.Union?.Name
            };

            if (TempData["ViewData"] != null)
            {
                ViewData = (ViewDataDictionary)TempData["ViewData"];
            }

            return PartialView("_Details", vm);
        }

        public ActionResult DeleteImage(int id, RegionalImageType imageType)
        {
            var regional = regionalsRepo.GetById(id);

            if (regional != null)
            {
                switch (imageType)
                {
                    case RegionalImageType.IndexImage:
                        regional.IndexImage = null;
                        break;
                    case RegionalImageType.Logo:
                        regional.Logo = null;
                        break;
                    case RegionalImageType.PrimaryImage:
                        regional.PrimaryImage = null;
                        break;
                    default:
                        return HttpNotFound($"Unknown image type {imageType}");
                }

                regionalsRepo.Save();
            }

            return RedirectToAction("Edit", new {id, regional?.SeasonId, regional?.UnionId});
        }

        [HttpPost]
        public ActionResult Details(RegionalDetailsForm frm)
        {
            if (!IsRegionalFederationEnabled(frm.RegionalId, frm.UnionId ?? 0))
            {
                ModelState.AddModelError("RegionalNotEnabled", Messages.RegionalNotEnabled);
            }

            var maxFileSize = GlobVars.MaxFileSize * 1000;
            var savePath = Server.MapPath(GlobVars.RegionalContentPath);
            var item = regionalsRepo.GetById(frm.RegionalId);

            item.Name = frm.Name ?? item.Name;
            item.Phone = frm.Phone;
            item.Email = frm.Email;
            item.Description = frm.Description;
            item.IndexAbout = frm.IndexAbout;
            item.Address = frm.Address;

            var error = string.Empty;// usersRepo.AddAppCredentials("club", frm.ClubId, frm.AppLogin, frm.AppPassword);
            if (!string.IsNullOrWhiteSpace(error))
                ModelState.AddModelError("AppLogin", error);

            var imageFile = GetPostedFile("ImageFile");
            if (imageFile != null)
            {
                if (imageFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("ImageFile", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(imageFile, "img");
                    if (newName == null)
                    {
                        ModelState.AddModelError("ImageFile", Messages.FileError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.PrimaryImage))
                            FileUtil.DeleteFile(savePath + item.PrimaryImage);

                        item.PrimaryImage = newName;
                    }
                }
            }

            var logoFile = GetPostedFile("LogoFile");
            if (logoFile != null)
            {
                if (logoFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("LogoFile", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(logoFile, "logo");
                    if (newName == null)
                    {
                        ModelState.AddModelError("LogoFile", Messages.FileError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.Logo))
                            FileUtil.DeleteFile(savePath + item.Logo);

                        item.Logo = newName;
                    }
                }
            }

            var indexFile = GetPostedFile("IndexFile");
            if (indexFile != null)
            {
                if (indexFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("IndexFile", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(indexFile, "img");
                    if (newName == null)
                    {
                        ModelState.AddModelError("IndexFile", Messages.FileError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.IndexImage))
                            FileUtil.DeleteFile(savePath + item.IndexImage);

                        item.IndexImage = newName;
                    }
                }
            }

            if (ModelState.IsValid)
            {
                regionalsRepo.Save();

                TempData["Saved"] = true;
            }
            else
            {
                TempData["ViewData"] = ViewData;
            }

            return RedirectToAction("Details", new { id = item.RegionalId, seasonId = frm.SeasonId });
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

            var savePath = Server.MapPath(GlobVars.RegionalContentPath);

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

        public ActionResult ListRegional(int id, int seasonId)
        {
            var regionals = regionalsRepo.GetRegionalsByUnionAndSeason(id, seasonId);

            var regionalsIds = regionals.Select(x => x.RegionalId).ToArray();
            var regionalManagers = db.UsersJobs
                .Where(x => regionalsIds.Contains(x.RegionalId ?? 0))
                .Include(x => x.User)
                .AsNoTracking()
                .ToList();

            var vm = new RegionalsForm
            {
                UnionId = id,
                SeasonId = seasonId,
                Regionals = regionals,
                RegionalManagers = regionalManagers
            };
            return PartialView("_ListRegionals", vm);
        }

        // For Clubs

        private string GetClubManagers(Club club)
        {
            var clubManagersList = club.UsersJobs.Where(j => JobRole.ClubManager.Equals(j.Job.JobsRole.RoleName, StringComparison.OrdinalIgnoreCase))?.Select(uj => uj.User.FullName);
            return clubManagersList.Any() ? string.Join(",", clubManagersList.Distinct()) : string.Empty;
        }

        public ActionResult ListClubs(int id, int? seasonId, int unionId = 0)
        {
            var varUnionid = 0;
            if (unionId == 0)
            {
                var item = regionalsRepo.GetById(id);
                varUnionid = item.UnionId ?? 0;
            }
            else
            {
                varUnionid = unionId;
            }
            seasonId = seasonId ?? GetUnionCurrentSeasonFromSession();
            var regionalClubs = db.Clubs.Where(c => c.RegionalId == id);
            var lstRegionalClubs = new List<RegionalClub>();

            foreach (var club in regionalClubs)
            {
                var regClub = new RegionalClub
                {
                    ClubId = club.ClubId,
                    IsClubApproveByRegional = club.IsClubApproveByRegional,
                    ClubName = club.Name,
                    ClubManager = GetClubManagers(club)
                };
                lstRegionalClubs.Add(regClub);
            }

            var regClubIds = lstRegionalClubs.Select(a => a.ClubId);

            var vm = new RegionalClubsForm
            {
                RegionalId = id,
                UnionId = unionId,
                SeasonId = seasonId,
                IsRegionalFederation = true
            };
            
            var clubs = clubsRepo.GetByUnion(varUnionid, seasonId).Where(c => !regClubIds.Contains(c.ClubId));
          
            /*
            if (false && clubs?.Count() > 0)
            {
                List<string> SelectedValues = new List<string>();
                var regClubs = new MultiSelectList(clubs?.Select(c =>
                {
                    return c;
                })
                ?? new List<AppModel.Club>(), nameof(AppModel.Club.ClubId), nameof(AppModel.Club.Name), SelectedValues);

                vm.DdlOptions = regClubs;
            }
            else*/
            {
                var ddlItems = clubs?.Select(a => new { a.ClubId, a.Name }).OrderBy(b => b.Name);
               // List<string> lstNames = clubs?.Select(a => a.Name).ToList();
                var ddlOptions = new MultiSelectList(ddlItems, "ClubId", "Name");
                vm.DdlOptions = ddlOptions;
            }

            vm.RegionalClubs = lstRegionalClubs;
           
            return PartialView("_ListClubs", vm);
        }

        [HttpPost]
        public ActionResult MapClubWithRegional(RegionalClubsForm model)
        {
            var clubIds = model?.SelectedValues?.ToList() ?? new List<string>();
            var alreadyAssignedClubs = new List<string>();
            if (clubIds?.Count > 0)
            {
                foreach (var cid in clubIds)
                {
                    if (string.IsNullOrWhiteSpace(cid))
                        continue;
                    var club = db.Clubs.FirstOrDefault(c => c.ClubId.ToString() == cid);
                    if (club?.RegionalId > 0 && club?.RegionalId != model.RegionalId)
                    {
                        alreadyAssignedClubs.Add(club.Name);
                    }
                    else
                    {
                        club.RegionalId = model.RegionalId.Value;
                        db.SaveChanges();
                    }
                }

                if (alreadyAssignedClubs?.Count() > 0)
                {
                    var error = string.Join(",", alreadyAssignedClubs) + Messages.RegionalClubAlreadyMapped;
                    TempData["RegionalClubMappingError"] = error;
                }
            }

            /*
            int varUnionid = 0;
            if (model.UnionId == null || model.UnionId == 0)
            {
                var item = regionalsRepo.GetById(model.RegionalId.Value);
                varUnionid = item.Regional.UnionId.Value;
            }
            else
                varUnionid = model.UnionId.Value;

            var section = unionsRepo.GetById(varUnionid)?.Section;
            var vm = new RegionalClubsForm
            {
                RegionalId = model.RegionalId,
                UnionId = model.UnionId,
                SeasonId = model.SeasonId,
                IsRegionalFederation = true
            };
            List<string> SelectedValues = new List<string>();

            var clubs = clubsRepo.GetByUnion(varUnionid, model.SeasonId, false);
            if (clubs?.Count() > 0)
            {
                var regClubs = new MultiSelectList(clubs?.Select(c =>
                {
                    return c;
                })
                ?? new List<AppModel.Club>(), nameof(AppModel.Club.ClubId), nameof(AppModel.Club.Name), SelectedValues);
                vm.DdlOptions = regClubs;
            }

            var regionalClubs = db.Clubs.Where(c => c.RegionalId == model.RegionalId)
                .Select(c => new RegionalClub
                {
                    ClubId = c.ClubId,
                    IsClubApproveByRegional = c.IsClubApproveByRegional,
                    ClubName = c.Name
                });

            vm.RegionalClubs = regionalClubs;

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.FromUnion = true;
            return PartialView("_ListClubs", vm);
            */

            return RedirectToAction("ListClubs", new { id = model.RegionalId, seasonId = model.SeasonId, unionId = model.UnionId });

        }

        public ActionResult RemoveRegionalClubMapping(int id, int clubid)
        {
            var item = db.Clubs.FirstOrDefault(c => c.RegionalId == id && c.ClubId == clubid);
            if (item?.ClubId > 0)
            {
                item.RegionalId = null;
                item.IsClubApproveByRegional = false;
                item.DateOfClubApprovalByRegional = null;
                db.SaveChanges();
            }

            return RedirectToAction("ListClubs", new { id = id });
        }

        [HttpPost]
        public void UpdateClubApprovelByRegional(int id, int clubid, bool isApproved)
        {
            var item = db.Clubs.FirstOrDefault(c => c.RegionalId == id && c.ClubId == clubid);
            if (item?.ClubId > 0)
            {
                item.IsClubApproveByRegional = isApproved;
                item.DateOfClubApprovalByRegional = isApproved ? DateTime.Now : (DateTime?)null;
                db.SaveChanges();
            }
        }

        //public ActionResult ListClubs(int id, int seasonId, int unionId = 0)
        private ActionResult RedirectToRegionalClubList(Regional item)
        {
            if (item.UnionId.HasValue && item.SeasonId.HasValue)
            {
                return RedirectToAction(nameof(ListClubs), new { id = item.RegionalId, seasonId = item.SeasonId, unionId = item.UnionId, });
            }
            else if (item.UnionId.HasValue)
            {
                return RedirectToAction(nameof(ListClubs), new { id = item.RegionalId, seasonId = item.SeasonId });
            }
            else
            {
                return RedirectToAction(nameof(ListClubs), new { id = item.RegionalId, seasonId = 0 });
            }
        }

    }
}