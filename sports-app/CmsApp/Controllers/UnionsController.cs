using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataService;
using AppModel;
using CmsApp.Models;
using Omu.ValueInjecter;
using System.IO;
using Resources;
using CmsApp.Helpers;
using CmsApp.Helpers.Injections;
using ClosedXML.Excel;
using DataService.DTO;
using DataService.Services;

namespace CmsApp.Controllers
{
    public class UnionsController : AdminController
    {
        public ActionResult EditReferees(int id, int? seasonId = null)
        {
            ViewBag.UnionId = id;
            ViewBag.SeasonId = seasonId;
            return View();
        }

        public ActionResult Edit(int? id, int? seasonId = null, string roleType = null)
        {

            if (!id.HasValue)
            {
                return RedirectToAction("NotFound", "Error");
            }

            if (!string.IsNullOrWhiteSpace(roleType))
            {
                this.SetWorkerSession(roleType);
            }

            playersRepo.CheckForExclusions();

            var union = unionsRepo.GetById(id.Value);
            SetIsSectionClubLevel(false);
            if (seasonId != null)
            {
                SetUnionCurrentSeason(seasonId.Value);
            }

            if (union.IsArchive)
            {
                return RedirectToAction("NotFound", "Error");
            }

            if (User.IsInAnyRole(AppRole.Workers) && !AuthSvc.AuthorizeUnionByIdAndManagerId(id.Value, AdminId))
            {
                return RedirectToAction("Index", "NotAuthorized");
            }

            var seasons = seasonsRepository.GetSeasonsByUnion(id.Value, false).ToList();

            Session["CurrentUnionId"] = id.Value;
            var disciplineSections = new List<string> { GamesAlias.Athletics, GamesAlias.Gymnastic, GamesAlias.Motorsport, GamesAlias.WeightLifting, GamesAlias.Swimming, GamesAlias.Rowing, GamesAlias.Bicycle, GamesAlias.Climbing };

            var model = new EditUnionForm
            {
                UnionId = id.Value,
                UnionName = union.Name,
                SectionId = union.Section.SectionId,
                SectionAlias = union.Section.Alias,
                SectionName = union.Section.Name,
                HasDisciplines = disciplineSections.Contains(union.Section.Alias),
                IsCatchBall = union.Section.Alias == GamesAlias.NetBall,
                IsRowing = union.Section.Alias == GamesAlias.Rowing,
                SeasonId = seasonId ?? GetUnionCurrentSeasonFromSession(),
                Seasons = seasons,
                SectionIsIndividual = union.Section.IsIndividual,
                IsReportsEnabled = union.IsReportsEnabled
            };
            ViewBag.SectionAlias = union.Section.Alias;
            if (id.Value == 38)
            {
                var unionClubs = union.Clubs.Where(c => !c.IsArchive);
                if (unionClubs != null && unionClubs.Any())
                {
                    foreach (var club in unionClubs)
                    {
                        clubsRepo.CheckForTrainingTeam(club.ClubId, club.SeasonId ?? 0);
                    }
                }
            }
            ViewBag.JobRole = User.GetSessionWorkerValueOrTopLevelSeasonJob(seasonId ?? GetUnionCurrentSeasonFromSession());  //usersRepo.GetTopLevelJob(AdminId);
            ViewBag.IsUnionViewer = ViewBag.JobRole == JobRole.Unionviewer ? AuthSvc.AuthorizeUnionViewerByManagerId(id.Value, AdminId) : false;

            ViewBag.IsRegionallevelEnabled = (union.IsRegionallevelEnabled);

            return View(model);
        }

        #region Info

        public ActionResult Details(int id, int? seasonId)
        {
            var item = unionsRepo.GetById(id);
            var vm = new UnionDetailsForm();
            vm.InjectFrom<CloneInjection>(item);

            string regionalAction = Convert.ToString(TempData["RegionalAction"] ?? string.Empty);

            var doc = unionsRepo.GetTermsDoc(id);
            if (doc != null)
            {
                vm.DocId = doc.DocId;
            }

            vm.TotoReportMaxBirthYear = item.TotoReportMaxBirthYear;
            vm.AvailableSports = new List<SelectListItem>();
            vm.SeasonId = seasonId;
            var availableSports = SportsRepo.GetBySectionId(item.SectionId).ToList().OrderBy(x => x.Name);
            if (availableSports.Any())
            {
                vm.AvailableSports.AddRange(availableSports.Select(x => new SelectListItem
                {
                    Text = !IsHebrew ? x.Name : x.NameHeb,
                    Value = x.Id.ToString(),
                    Selected = x.Id == item.SportType
                }));
            }

            var user = usersRepo.GetByUnionName("union." + vm.UnionId);
            if (user != null)
            {
                vm.AppLogin = user.UserName;
                vm.AppPassword = Protector.Decrypt(user.Password);
            }

            var unionForms = unionsRepo.GetUnionForms(vm.UnionId, vm.SeasonId);

            var unionFormsModel = unionForms.Count() > 0
                ? unionForms.Select(uf => new UnionFormModel { FormId = uf.FormId, SeasonId = uf.SeasonId, Title = uf.Title, UnionId = uf.UnionId, Path = uf.FilePath })
                : new List<UnionFormModel>();

            vm.UnionForms = unionFormsModel;
            vm.IsUnionManager = usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager;

            #region Union to club prices

            vm.UnionToClubCompetingRegistrationPrices = item.UnionPrices
                .Where(p => p.SeasonId == seasonId && p.PriceType == (int)UnionPriceType.UnionToClubCompetingRegistrationPrice)
                .Select(p => new UnionPriceModel
                {
                    EndDate = p.EndDate,
                    Price = p.Price,
                    StartDate = p.StartDate,
                    FromBirthday = p.FromBirthday,
                    ToBirthday = p.ToBirthday,
                    CardComProductId = p.CardComProductId
                }).ToList();
            vm.UnionToClubRegularRegistrationPrices = item.UnionPrices
                .Where(p => p.SeasonId == seasonId && p.PriceType == (int)UnionPriceType.UnionToClubRegularRegistrationPrice)
                .Select(p => new UnionPriceModel
                {
                    EndDate = p.EndDate,
                    Price = p.Price,
                    StartDate = p.StartDate,
                    FromBirthday = p.FromBirthday,
                    ToBirthday = p.ToBirthday,
                    CardComProductId = p.CardComProductId
                }).ToList();
            vm.UnionToClubInsurancePrices = item.UnionPrices
                .Where(p => p.SeasonId == seasonId && p.PriceType == (int)UnionPriceType.UnionToClubInsurancePrice)
                .Select(p => new UnionPriceModel
                {
                    EndDate = p.EndDate,
                    Price = p.Price,
                    StartDate = p.StartDate,
                    FromBirthday = p.FromBirthday,
                    ToBirthday = p.ToBirthday,
                    CardComProductId = p.CardComProductId
                }).ToList();
            vm.UnionToClubTenicardPrices = item.UnionPrices
                .Where(p => p.SeasonId == seasonId && p.PriceType == (int)UnionPriceType.UnionToClubTenicardPrice)
                .Select(p => new UnionPriceModel
                {
                    EndDate = p.EndDate,
                    Price = p.Price,
                    StartDate = p.StartDate,
                    FromBirthday = p.FromBirthday,
                    ToBirthday = p.ToBirthday,
                    CardComProductId = p.CardComProductId
                }).ToList();

            #endregion

            #region Set official settings

            var refereeSettings = item.UnionOfficialSettings
                .FirstOrDefault(x => x.JobsRolesId == jobsRepo.GetJobRoleByRoleName(JobRole.Referee).RoleId);
            var spectatorSettings = item.UnionOfficialSettings
                .FirstOrDefault(x => x.JobsRolesId == jobsRepo.GetJobRoleByRoleName(JobRole.Spectator).RoleId);
            var deskSettings = item.UnionOfficialSettings
                .FirstOrDefault(x => x.JobsRolesId == jobsRepo.GetJobRoleByRoleName(JobRole.Desk).RoleId);

            if (refereeSettings != null)
            {
                vm.RefereeFeePerGame = refereeSettings.PaymentPerGame;
                vm.RefereePaymentCurrencyUnits = (CurrencyUnits)refereeSettings.PaymentPerGameCurrency;
                vm.RefereePaymentForTravel = refereeSettings.PaymentTravel;
                vm.RefereeTravelCurrencyUnits = (CurrencyUnits)refereeSettings.TravelPaymentCurrencyType;
                vm.RefereeTravelMetricUnits = (MetricUnits)refereeSettings.TravelMetricType;
                vm.RateAForTravel = refereeSettings.RateAForTravel;
                vm.RateAPerGame = refereeSettings.RateAPerGame;
                vm.RateBForTravel = refereeSettings.RateBForTravel;
                vm.RateBPerGame = refereeSettings.RateBPerGame;
                vm.RateCForTravel = refereeSettings.RateCForTravel;
                vm.RateCPerGame = refereeSettings.RateCPerGame;
            }

            if (deskSettings != null)
            {
                vm.DeskFeePerGame = deskSettings.PaymentPerGame;
                vm.DeskPaymentCurrencyUnits = (CurrencyUnits)deskSettings.PaymentPerGameCurrency;
                vm.DeskPaymentForTravel = deskSettings.PaymentTravel;
                vm.DeskTravelCurrencyUnits = (CurrencyUnits)deskSettings.TravelPaymentCurrencyType;
                vm.DeskTravelMetricUnits = (MetricUnits)deskSettings.TravelMetricType;
            }

            if (spectatorSettings != null)
            {
                vm.SpectatorFeePerGame = spectatorSettings.PaymentPerGame;
                vm.SpectatorPaymentCurrencyUnits = (CurrencyUnits)spectatorSettings.PaymentPerGameCurrency;
                vm.SpectatorPaymentForTravel = spectatorSettings.PaymentTravel;
                vm.SpectatorTravelCurrencyUnits = (CurrencyUnits)spectatorSettings.TravelPaymentCurrencyType;
                vm.SpectatorTravelMetricUnits = (MetricUnits)spectatorSettings.TravelMetricType;
            }
            #endregion

            if (id == 37)
            {
                var unionPaymentSettings = unionsRepo.GetKaratePaymentsSettings(id, seasonId);
                vm.UnionPaymentForm = unionPaymentSettings != null && unionPaymentSettings.Any()
                    ? unionPaymentSettings.Select(p => new KarateUnionPaymentForm { FromSportNumber = p.FromNumber, ToSportNumber = p.ToNumber, PricePerSportsman = p.Price })
                    : Enumerable.Empty<KarateUnionPaymentForm>();
            }
            if (TempData["ViewData"] != null)
            {
                ViewData = (ViewDataDictionary)TempData["ViewData"];
            }

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);

            return PartialView("_Details", vm);
        }

        public ActionResult LoadTennisPointSettings(int levelId, int seasonId)
        {
            var pointsSettings = GetTennisPointsSettings(levelId, seasonId);
            ViewBag.LevelId = levelId;
            ViewBag.SeasonId = seasonId;
            //check if bicycle 
            var season = seasonsRepository.GetById(seasonId);

            ViewBag.IsBicycle = season?.Union?.Section?.Alias == SectionAliases.Bicycle;


            return PartialView("_TennisPointSettingsForm", pointsSettings);
        }

        [HttpPost]
        public ActionResult CreateTennisPointSettings(TennisPositionSettingsDto form)
        {

            var vm = new LevelPointsSetting
            {
                LevelId = form.LevelId,
                SeasonId = form.SeasonId,
                Rank = form.Rank,
                Points = form.Points,
                PointsForPairs = form.PointsForPairs
            };

            db.LevelPointsSettings.Add(vm);
            db.SaveChanges();
            int id = vm.Id;
            return Json(new { Success = true, Data = vm });
            //return RedirectToAction(nameof(LoadTennisPointSettings), new { levelId = form.LevelId, seasonId = form.SeasonId });
        }

        [HttpPost]
        public ActionResult UpdateTennisPointSettings(int levelId, int seasonId, int id, int rank, int points, int? pointsForPairs)
        {
            var vm = db.LevelPointsSettings.Where(x => x.Id == id).FirstOrDefault();
            vm.Rank = rank;
            vm.Points = points;
            vm.PointsForPairs = pointsForPairs;
            db.SaveChanges();

            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult DeleteTennisPointSettings(int id)
        {
            var vm = db.LevelPointsSettings.Where(x => x.Id == id).FirstOrDefault();
            db.LevelPointsSettings.Remove(vm);
            db.SaveChanges();

            return Json(new { Success = true });
        }

        private IEnumerable<TennisPositionSettingsDto> GetTennisPointsSettings(int levelId, int seasonId)
        {
            var pointSettings = db.LevelPointsSettings.OrderBy(x => x.Rank).Where(x => x.LevelId == levelId && x.SeasonId == seasonId).ToList();
            if (pointSettings.Any())
            {
                foreach (var pointSetting in pointSettings)
                {
                    yield return new TennisPositionSettingsDto
                    {
                        Id = pointSetting.Id,
                        Rank = pointSetting.Rank,
                        Points = pointSetting.Points,
                        PointsForPairs = pointSetting.PointsForPairs
                    };
                }
            }
        }

        [HttpPost]
        public ActionResult Details(UnionDetailsForm frm)
        {
            var maxFileSize = GlobVars.MaxFileSize * 1000;
            var savePath = Server.MapPath(GlobVars.ContentPath + "/league/");

            bool oldRegionalValue = false;

            var item = unionsRepo.GetById(frm.UnionId);

            oldRegionalValue = item.IsRegionallevelEnabled;

            UpdateModel(item);

            var error = usersRepo.AddAppCredentials("union", frm.UnionId, frm.AppLogin, frm.AppPassword);

            if (error != null)
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

            var docFile = GetPostedFile("DocFile");
            if (docFile != null)
            {
                if (docFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("DocFile", Messages.FileSizeError);
                }
                var isValid = SaveDocument(docFile, frm.UnionId);
                if (!isValid)
                {
                    ModelState.AddModelError("DocFile", Messages.FileError);
                }
            }

            #region Officials settings

            // remove old settings
            db.UnionOfficialSettings.RemoveRange(item.UnionOfficialSettings);

            item.UnionOfficialSettings.Add(new UnionOfficialSetting
            {
                JobsRolesId = jobsRepo.GetJobRoleByRoleName(JobRole.Referee).RoleId,
                PaymentPerGame = frm.RefereeFeePerGame,
                PaymentPerGameCurrency = (int)frm.RefereePaymentCurrencyUnits,
                PaymentTravel = frm.RefereePaymentForTravel,
                TravelPaymentCurrencyType = (int)frm.RefereeTravelCurrencyUnits,
                TravelMetricType = (int)frm.RefereeTravelMetricUnits,
                RateAForTravel = frm.RateAForTravel,
                RateBForTravel = frm.RateBForTravel,
                RateAPerGame = frm.RateAPerGame,
                RateBPerGame = frm.RateBPerGame,
                RateCForTravel = frm.RateCForTravel,
                RateCPerGame = frm.RateCPerGame,
                SeasonId = frm.SeasonId
            });

            item.UnionOfficialSettings.Add(new UnionOfficialSetting
            {
                JobsRolesId = jobsRepo.GetJobRoleByRoleName(JobRole.Spectator).RoleId,
                PaymentPerGame = frm.SpectatorFeePerGame,
                PaymentPerGameCurrency = (int)frm.SpectatorPaymentCurrencyUnits,
                PaymentTravel = frm.SpectatorPaymentForTravel,
                TravelPaymentCurrencyType = (int)frm.SpectatorTravelCurrencyUnits,
                TravelMetricType = (int)frm.SpectatorTravelMetricUnits,
                SeasonId = frm.SeasonId
            });

            item.UnionOfficialSettings.Add(new UnionOfficialSetting
            {
                JobsRolesId = jobsRepo.GetJobRoleByRoleName(JobRole.Desk).RoleId,
                PaymentPerGame = frm.DeskFeePerGame,
                PaymentPerGameCurrency = (int)frm.DeskPaymentCurrencyUnits,
                PaymentTravel = frm.DeskPaymentForTravel,
                TravelPaymentCurrencyType = (int)frm.DeskTravelCurrencyUnits,
                TravelMetricType = (int)frm.DeskTravelMetricUnits,
                SeasonId = frm.SeasonId
            });

            #endregion

            #region Union to club prices

            db.UnionPrices.RemoveRange(item.UnionPrices.Where(x => x.SeasonId == frm.SeasonId));

            for (int i = 0; i < frm.UnionToClubCompetingRegistrationPrices.Count; i++)
            {
                var priceItem = frm.UnionToClubCompetingRegistrationPrices[i];

                item.UnionPrices.Add(new UnionPrice
                {
                    Price = priceItem.Price,
                    PriceType = (int)UnionPriceType.UnionToClubCompetingRegistrationPrice,
                    StartDate = priceItem.StartDate,
                    EndDate = priceItem.EndDate,
                    FromBirthday = priceItem.FromBirthday,
                    ToBirthday = priceItem.ToBirthday,
                    SeasonId = frm.SeasonId ?? 0,
                    CardComProductId = priceItem.CardComProductId
                });
            }

            for (int i = 0; i < frm.UnionToClubRegularRegistrationPrices.Count; i++)
            {
                var priceItem = frm.UnionToClubRegularRegistrationPrices[i];

                item.UnionPrices.Add(new UnionPrice
                {
                    Price = priceItem.Price,
                    PriceType = (int)UnionPriceType.UnionToClubRegularRegistrationPrice,
                    StartDate = priceItem.StartDate,
                    EndDate = priceItem.EndDate,
                    FromBirthday = priceItem.FromBirthday,
                    ToBirthday = priceItem.ToBirthday,
                    SeasonId = frm.SeasonId ?? 0,
                    CardComProductId = priceItem.CardComProductId
                });
            }

            for (int i = 0; i < frm.UnionToClubInsurancePrices.Count; i++)
            {
                var priceItem = frm.UnionToClubInsurancePrices[i];

                item.UnionPrices.Add(new UnionPrice
                {
                    Price = priceItem.Price,
                    PriceType = (int)UnionPriceType.UnionToClubInsurancePrice,
                    StartDate = priceItem.StartDate,
                    EndDate = priceItem.EndDate,
                    FromBirthday = priceItem.FromBirthday,
                    ToBirthday = priceItem.ToBirthday,
                    SeasonId = frm.SeasonId ?? 0,
                    CardComProductId = priceItem.CardComProductId
                });
            }

            for (int i = 0; i < frm.UnionToClubTenicardPrices.Count; i++)
            {
                var priceItem = frm.UnionToClubTenicardPrices[i];

                item.UnionPrices.Add(new UnionPrice
                {
                    Price = priceItem.Price,
                    PriceType = (int)UnionPriceType.UnionToClubTenicardPrice,
                    StartDate = priceItem.StartDate,
                    EndDate = priceItem.EndDate,
                    FromBirthday = priceItem.FromBirthday,
                    ToBirthday = priceItem.ToBirthday,
                    SeasonId = frm.SeasonId ?? 0,
                    CardComProductId = priceItem.CardComProductId
                });
            }

            #endregion

            if (ModelState.IsValid)
            {
                if (item != null && item.UnionId == 37)
                {
                    unionsRepo.UpdateKarateUnionSettings(item, frm.UnionPaymentForm, frm.SeasonId);
                }
                unionsRepo.Save();

                TempData["Saved"] = true;
            }
            else
            {
                TempData["ViewData"] = ViewData;
            }

            if (oldRegionalValue != item.IsRegionallevelEnabled)
            {
                if (oldRegionalValue && !item.IsRegionallevelEnabled)
                    TempData["RegionalAction"] = "hide";
                else if (!oldRegionalValue && item.IsRegionallevelEnabled)
                    TempData["RegionalAction"] = "show";
            }

            // return RedirectToAction("Edit", new { id = item.UnionId });
            // else
            return RedirectToAction("Details", new { id = item.UnionId, seasonId = frm.SeasonId });
        }

        [HttpPost]
        public JsonResult GetUnionPaymentDetails(int clubId, int seasonId)
        {
            var message = string.Empty;
            var club = clubsRepo.GetById(clubId);
            if (club != null)
            {
                var union = club?.Union;
                if (union != null)
                {
                    var unionPayments = union.KarateUnionPayments;
                    var clubPlayers = playersRepo.GetPlayersStatusesByClubId(clubId, seasonId);
                    var clubApprovedPlayersCount = clubPlayers != null && clubPlayers.Any() ? clubPlayers.Count(c => c.IsApproveChecked && c.IsActive == true) : 0;
                    if (unionPayments != null && unionPayments.Any() && clubApprovedPlayersCount > 0)
                    {
                        message = GetUnionPaymentMessage(unionPayments, clubApprovedPlayersCount, club.Name);
                    }
                }
            }
            return Json(new { Message = message });
        }

        private string GetUnionPaymentMessage(ICollection<KarateUnionPayment> unionPayments, int unionApprovedPlayersCount, string clubName)
        {
            string message = string.Empty;
            var necessaryPaymentType = unionPayments.FirstOrDefault(p => p.FromNumber <= unionApprovedPlayersCount && p.ToNumber >= unionApprovedPlayersCount);
            if (necessaryPaymentType != null)
            {
                if (unionApprovedPlayersCount == necessaryPaymentType.ToNumber)
                {
                    //unionsRepo.SetIsShownPaymentStatus(necessaryPaymentType.Id);
                    var nextValue = unionPayments.OrderBy(c => c.FromNumber).FirstOrDefault(c => c.FromNumber >= unionApprovedPlayersCount);
                    if (nextValue != null)
                    {
                        message = Messages.RegistrationsOfKarateClub.Replace("[maxnumber]", necessaryPaymentType.ToNumber?.ToString())
                            .Replace("[clubname]", clubName)
                            .Replace("[price]", nextValue.Price?.ToString());
                    }
                }
            }
            return message;
        }

        public ActionResult DeleteImage(int unionId, string image)
        {
            var item = unionsRepo.GetById(unionId);
            if (item == null || string.IsNullOrEmpty(image))
                return RedirectToAction("Edit", new { id = unionId });

            if (image == "PrimaryImage")
            {
                item.PrimaryImage = null;
            }

            if (image == "IndexImage")
            {
                item.IndexImage = null;
            }

            if (image == "Logo")
            {
                item.Logo = null;
            }
            unionsRepo.Save();

            return RedirectToAction("Edit", new { id = unionId });
        }

        [HttpPost]
        public void DeleteUnionForm(int formId)
        {
            unionsRepo.DeleteUnionForm(formId);
        }

        public ActionResult ShowDoc(int id)
        {
            var doc = unionsRepo.GetDocById(id);

            Response.AddHeader("content-disposition", "inline;filename=" + doc.FileName + ".pdf");

            return this.File(doc.DocFile, "application/pdf");
        }

        public ActionResult RemoveDoc(int id, int unionId)
        {
            unionsRepo.RemoveDoc(id);

            return RedirectToAction("Edit", new { id = unionId });
        }

        [HttpPost]
        public ActionResult AddUnionForm(UnionFormModel frm)
        {
            var maxFileSize = GlobVars.MaxFileSize * 1000;
            var unionFile = GetPostedFile("UnionFormUploadFile");
            if (unionFile?.ContentLength > maxFileSize)
            {
                return RedirectToAction("Edit", new { id = frm.UnionId, seasonId = frm.SeasonId });
            }
            if (unionFile == null)
            {
                return RedirectToAction("Edit", new { id = frm.UnionId, seasonId = frm.SeasonId });
            }

            var path = SaveUnionFile(unionFile, frm.UnionId, frm.Title);

            if (String.IsNullOrEmpty(path))
            {
                ModelState.AddModelError("UnionFormUploadFile", Messages.FileError);
            }
            else
            {
                var form = new UnionForm
                {
                    UnionId = frm.UnionId,
                    Title = frm.Title,
                    FilePath = path,
                    SeasonId = frm.SeasonId.Value
                };

                unionsRepo.CreateUnionForm(form);
            }

            return RedirectToAction("Edit", new { id = frm.UnionId, seasonId = frm.SeasonId });
        }

        private string SaveUnionFile(HttpPostedFileBase unionFile, int unionId, string title)
        {
            string ext = Path.GetExtension(unionFile.FileName).ToLower();
            if (unionFile != null)
            {
                string newName = title.Replace(' ', '_') + "_" + AppFunc.GetUniqName() + ext;

                var savePath = Server.MapPath(GlobVars.ContentPath + "/union/");
                var di = new DirectoryInfo(savePath);
                if (!di.Exists)
                    di.Create();

                byte[] fileData;
                using (var reader = new BinaryReader(unionFile.InputStream))
                {
                    fileData = reader.ReadBytes(unionFile.ContentLength);
                }
                System.IO.File.WriteAllBytes(savePath + newName, fileData);

                return newName;
            }
            return string.Empty;
        }

        [NonAction]
        private bool SaveDocument(HttpPostedFileBase file, int unionId)
        {
            string ext = Path.GetExtension(file.FileName).ToLower();

            if (ext != ".pdf")
            {
                return false;
            }

            var doc = unionsRepo.GetTermsDoc(unionId);
            if (doc == null)
            {
                doc = new UnionsDoc { UnionId = unionId };
                unionsRepo.CreateDoc(doc);
            }

            doc.FileName = file.FileName;

            byte[] docData;
            using (var reader = new BinaryReader(file.InputStream))
            {
                docData = reader.ReadBytes(file.ContentLength);
            }

            //TODO: commented due to YIT's metascan server failure 
            //var req = new MetascanHelper.MetadataRequest(Guid.NewGuid().ToString(),
            //    file.FileName,
            //    docData,
            //    MetascanHelper.MetaScanAction.PostFileToMetaScan
            //    );

            //string dataid = req.PostFileForScanning();
            //MetaScanScanStatus metaScanStatus;
            //req.CheckFileScan(dataid, out metaScanStatus);

            //int tries = 3;
            //while (metaScanStatus == MetaScanScanStatus.GeneralError && dataid != "" && tries > 0)
            //{
            //    tries--;
            //    System.Threading.Thread.Sleep(2000);
            //    req.CheckFileScan(dataid, out metaScanStatus);
            //}

            //if (metaScanStatus == MetaScanScanStatus.Valid)
            //{
            //    doc.DocFile = docData;
            //    unionsRepo.Save();
            //    return true;
            //}

            //return false;

            doc.DocFile = docData;
            unionsRepo.Save();
            return true;
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
            string ext = Path.GetExtension(file.FileName).ToLower();

            if (!GlobVars.ValidImages.Contains(ext))
                return null;

            string newName = name + "_" + AppFunc.GetUniqName() + ext;

            var savePath = Server.MapPath(GlobVars.ContentPath + "/union/");

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

        #endregion

        public ActionResult List(int id)
        {
            var vm = new UnionsForm { SectionId = id };

            if (User.IsInAnyRole(AppRole.Workers))
            {
                switch (usersRepo.GetTopLevelJob(base.AdminId))
                {
                    case JobRole.UnionManager:
                        vm.UnionsList = new UnionsRepo().GetByManagerId(base.AdminId);
                        break;
                    case JobRole.LeagueManager:
                        break;
                    case JobRole.TeamManager:
                        break;
                }
            }
            else
                vm.UnionsList = unionsRepo.GetBySection(id);

            return PartialView("_List", vm);
        }

        public ActionResult Delete(int id)
        {
            var u = unionsRepo.GetById(id);

            bool isHasLeagues = u.Leagues.Any(t => t.IsArchive == false);
            if (isHasLeagues)
            {
                TempData["ErrId"] = id;
            }
            else
            {
                u.IsArchive = true;
                unionsRepo.Save();
            }

            return RedirectToAction("List", new { id = u.SectionId });
        }

        [HttpPost]
        public ActionResult Save(UnionsForm frm)
        {
            var u = new Union { SectionId = frm.SectionId, Name = frm.Name };
            unionsRepo.Create(u);
            unionsRepo.Save();

            return RedirectToAction("List", new { id = frm.SectionId });
        }

        [HttpPost]
        public ActionResult Update(int unionId, string name)
        {
            var u = unionsRepo.GetById(unionId);
            u.Name = name;
            unionsRepo.Save();

            TempData["SavedId"] = unionId;

            return RedirectToAction("List", new { id = u.SectionId });
        }

        public ActionResult Leagues(int id, int seasonId)
        {
            var resList = new List<League>();

            var roleName = usersRepo.GetTopLevelJob(AdminId);
            var isUnionViewer = AuthSvc.AuthorizeUnionViewerByManagerId(id, AdminId);
            var userType = usersRepo.GetTopLevelJob(base.AdminId);
            if (isUnionViewer) userType = JobRole.Unionviewer;
            if (User.IsInAnyRole(AppRole.Workers) || isUnionViewer)
            {
                switch (userType)
                {
                    case JobRole.UnionManager:
                    case JobRole.Unionviewer:
                    case JobRole.RefereeAssignment:
                        resList = leagueRepo.GetByUnion(id, seasonId).Where(x => x.EilatTournament == null || x.EilatTournament == false).OrderBy(x => x.SortOrder).ToList();
                        break;
                    case JobRole.LeagueManager:
                        resList = leagueRepo.GetByManagerId(base.AdminId, seasonId).Where(x => x.EilatTournament == null || x.EilatTournament == false).OrderBy(x => x.SortOrder).ToList();
                        break;
                    case JobRole.TeamManager:
                        break;
                }
            }
            else
            {
                resList = leagueRepo.GetByUnion(id, seasonId).Where(x => x.EilatTournament == null || x.EilatTournament == false).OrderBy(x => x.SortOrder).ToList();
            }


            var section = unionsRepo.GetById(id)?.Section;
            var sectionAlias = section?.Alias;

            var result = new TournamentsPDF
            {
                UnionId = id,
                listLeagues = resList,
                SeasonId = seasonId,
                Et = TournamentsPDF.EditType.LgUnion,
                IsIndividual = section?.IsIndividual ?? false,
                Section = sectionAlias ?? ""
            };

            ViewBag.LeaguesTab = true;
            ViewBag.JobRole = roleName;

            ViewBag.LeaguePlayersCount = GetLeaguePlayersCountDictionary(resList, sectionAlias, id, seasonId);
            if (sectionAlias == GamesAlias.Gymnastic)
            {
                ViewBag.GymnasticLeaguePlayersCount = GetGymnasticsLeaguePlayersCountDictionary(resList);
            }

            return PartialView("_Leagues", result);
        }

        private Dictionary<int, int> GetGymnasticsLeaguePlayersCountDictionary(List<League> resList)
        {
            var result = new Dictionary<int, int>();

            foreach (var league in resList)
            {
                result.Add(league.LeagueId, playersRepo.CheckCompetitionRegistrationsCount(null, league.LeagueId, league.SeasonId ?? 0));
            }

            return result;
        }

        private Dictionary<int, int> GetLeaguePlayersCountDictionary(List<League> resList, string sectionAlias, int unionId, int seasonId)
        {
            var result = new Dictionary<int, int>();
            if (resList != null && resList.Any())
            {
                if (sectionAlias.Equals(GamesAlias.MartialArts))
                {
                    foreach (var league in resList)
                    {
                        var leaguePlayersCount = league.SportsRegistrations.Count;
                        result.Add(league.LeagueId, leaguePlayersCount);
                    }
                }
                else
                {
                    var players = Caching.GetTeamPlayersByUnionIdShort(playersRepo, unionId, seasonId);
                    foreach (var league in resList)
                    {
                        var leaguePlayersCount = players.Count(c =>
                            c.Team.LeagueTeams.Any(lt => lt.LeagueId == league.LeagueId && lt.SeasonId == seasonId) &&
                            c.LeagueId == league.LeagueId &&
                            c.SeasonId == seasonId);
                        result.Add(league.LeagueId, leaguePlayersCount);
                    }
                }
            }
            return result;
        }

        [HttpPost]
        public ActionResult Leagues(TournamentsPDF model, HttpPostedFileBase PDF1_file, HttpPostedFileBase PDF2_file, HttpPostedFileBase PDF3_file, HttpPostedFileBase PDF4_file)
        {
            var routeToPDF = GlobVars.PdfRoute;
            HttpPostedFileBase[] pdfArr = new HttpPostedFileBase[] { PDF1_file, PDF2_file, PDF3_file, PDF4_file };
            for (int i = 0; i < pdfArr.Length; i++)
            {
                var fileFullName = $"{routeToPDF}PDF{i + 1}.pdf";
                if ((string.IsNullOrEmpty(model[i]) || pdfArr[i] != null)
                    && System.IO.File.Exists(fileFullName))
                {
                    System.IO.File.Delete(fileFullName);
                }
                if (pdfArr[i] != null)
                {
                    pdfArr[i].SaveAs(fileFullName);
                }
            }
            return RedirectToAction("Edit", model.UnionId != null ? new { id = model.UnionId } : null);
        }

        public ActionResult ShowGlobalDoc(string name)
        {
            var url = GlobVars.PdfUrl + name.Split('/').Last();
            return Redirect(url);
        }

        public ActionResult EilatTournament(int? unionId, int? disciplineId, int? clubId, int seasonId)
        {
            var result = new TournamentsPDF
            {
                UnionId = unionId,
                Et = TournamentsPDF.EditType.TmntSectionClub,
                SeasonId = seasonId,
                ClubId = clubId,
                IsDepartment = false
            };

            if (clubId.HasValue)
            {
                var club = clubsRepo.GetById(clubId.Value);
                result.IsDepartment = club?.ParentClubId != null ? true : false;
            }

            if (clubId.HasValue)
            {
                result.listLeagues = leagueRepo.GetByClub(clubId.Value, seasonId)
                    .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true).OrderBy(x => x.SortOrder)
                    .ToList();
                if (unionId.HasValue)
                {
                    result.Et = TournamentsPDF.EditType.TmntUnionClub;
                }
                else
                {
                    result.Et = TournamentsPDF.EditType.TmntSectionClub;
                }
            }
            else if (disciplineId.HasValue)
            {
                result.DisciplineId = disciplineId;
                result.UnionId = disciplinesRepo.GetById(disciplineId.Value).UnionId;
                result.listLeagues = leagueRepo.GetByDiscipline(disciplineId.Value, seasonId)
                    .Where(x => x.EilatTournament != null && x.EilatTournament == true).OrderByDescending(l => l.LeagueStartDate ?? DateTime.MinValue)
                    .ToList();
                result.Et = unionId.HasValue ? TournamentsPDF.EditType.TmntUnionClub : TournamentsPDF.EditType.TmntSectionClub;
            }
            else if (unionId.HasValue)
            {
                result.Et = TournamentsPDF.EditType.TmntUnion;
                if (User.IsInAnyRole(AppRole.Workers))
                {
                    switch (usersRepo.GetTopLevelJob(base.AdminId))
                    {
                        case JobRole.UnionManager:
                        case JobRole.Unionviewer:
                        case JobRole.RefereeAssignment:
                            result.listLeagues = leagueRepo.GetByUnion(unionId.Value, seasonId)
                                .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true).OrderBy(x => x.SortOrder)
                                .ToList();
                            break;
                        case JobRole.LeagueManager:
                            result.listLeagues = leagueRepo.GetByManagerId(base.AdminId, seasonId)
                                .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true).OrderBy(x => x.SortOrder)
                                .ToList();
                            break;
                        case JobRole.TeamManager:
                            break;
                    }
                }
                else
                {
                    result.listLeagues = leagueRepo.GetByUnion(unionId.Value, seasonId)
                        .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true).OrderBy(x => x.SortOrder)
                        .ToList();
                }

                var routeToPDF = GlobVars.PdfRoute;
                for (int i = 0; i < result.Count; i++)
                {
                    if (System.IO.File.Exists($"{routeToPDF}PDF{i + 1}.pdf"))
                    {
                        result[i] = $"PDF{i + 1}.pdf";
                    }
                }
            }
            if (clubId.HasValue)
            {
                result.Section = clubsRepo.GetById(clubId.Value)?.Section?.Alias ?? clubsRepo.GetById(clubId.Value).Union?.Section?.Alias;
            }
            else if (disciplineId.HasValue)
            {
                var disciplineUnionId = disciplinesRepo.GetById(disciplineId.Value)?.UnionId;
                if (disciplineUnionId.HasValue)
                {
                    result.Section = unionsRepo.GetById(disciplineUnionId.Value)?.Section?.Alias;
                }
            }
            else
            {
                result.Section = unionsRepo.GetById(unionId.Value)?.Section?.Alias;
            }

            if (result.Section.Equals(GamesAlias.Gymnastic))
            {
                ViewBag.GymnasticLeaguePlayersCount = GetGymnasticsLeaguePlayersCountDictionary(result.listLeagues);
            }
            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.LeaguePlayersCount = GetLeaguePlayersCountDictionary(result.listLeagues, result.Section, result.UnionId ?? 0, result.SeasonId);
            result.IsIndividual = unionsRepo.GetById(result.UnionId ?? 0)?.Section?.IsIndividual ?? false;
            return PartialView("_Leagues", result);
        }

        [HttpPost]
        public JsonResult AddCompetitionLevel(string levelName, int unionId, int seasonId)
        {
            var level = new CompetitionLevel
            {
                level_name = levelName,
                UnionId = unionId,
                SeasonId = seasonId
            };
            db.CompetitionLevels.Add(level);
            if (db.SaveChanges() > 0)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }

        }

        [HttpPost]
        public JsonResult EditCompetitionLevel(int levelId, string levelName)
        {
            var level = db.CompetitionLevels.Where(x => x.id == levelId).FirstOrDefault();
            level.level_name = levelName;
            if (db.SaveChanges() > 0)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }

        }

        [HttpPost]
        public JsonResult RemoveCompetitionLevel(int levelId)
        {
            var level = db.CompetitionLevels.Where(x => x.id == levelId).FirstOrDefault();
            db.CompetitionLevels.Remove(level);
            if (db.SaveChanges() > 0)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }

        }

        
        [HttpPost]
        public JsonResult AddBicycleCompetitionDiscipline(string name, int unionId, int seasonId)
        {
            var bdisc = new BicycleCompetitionDiscipline
            {
                Name = name,
                UnionId = unionId,
                SeasonId = seasonId
            };
            db.BicycleCompetitionDisciplines.Add(bdisc);
            if (db.SaveChanges() > 0)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }

        }

        [HttpPost]
        public JsonResult EditBicycleCompetitionDiscipline(int disciplineId, string name)
        {
            var bdisc = db.BicycleCompetitionDisciplines.Where(x => x.Id == disciplineId).FirstOrDefault();
            bdisc.Name = name;
            if (db.SaveChanges() > 0)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }

        }

        [HttpPost]
        public JsonResult RemoveBicycleCompetitionDiscipline(int disciplineId)
        {
            var bdisc = db.BicycleCompetitionDisciplines.Where(x => x.Id == disciplineId).FirstOrDefault();
            if(bdisc.DisciplineExpertises.Any())
            {
                return Json(new { Success = false, Error = Messages.ExistingRelationToExpertise });
            }
            db.BicycleCompetitionDisciplines.Remove(bdisc);
            if (db.SaveChanges() > 0)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }

        }

        
        [HttpPost]
        public JsonResult AddDisciplineExpertise(string name, int? disciplineId)
        {
            var discExp = new DisciplineExpertise
            {
                Name = name,
                BicycleCompetitionDisciplineId = disciplineId > 0 ? disciplineId : null
            };
            db.DisciplineExpertises.Add(discExp);
            if (db.SaveChanges() > 0)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }

        }

        [HttpPost]
        public JsonResult EditDisciplineExpertise(int expertiseId,string name, int? disciplineId)
        {
            var discExp = db.DisciplineExpertises.Where(x => x.Id == expertiseId).FirstOrDefault();
            discExp.Name = name;
            discExp.BicycleCompetitionDisciplineId = disciplineId > 0 ? disciplineId : null;
            if (db.SaveChanges() > 0)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }

        }

        [HttpPost]
        public JsonResult RemoveDisciplineExpertise(int expertiseId)
        {
            var discExp = db.DisciplineExpertises.Where(x => x.Id == expertiseId).FirstOrDefault();
            db.DisciplineExpertises.Remove(discExp);
            if (db.SaveChanges() > 0)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }

        }
             


        [HttpPost]
        public JsonResult AddBicycleCompetitionHeat(string name, int unionId, int seasonId)
        {
            var bheat = new BicycleCompetitionHeat
            {
                Name = name,
                UnionId = unionId,
                SeasonId = seasonId
            };
            db.BicycleCompetitionHeats.Add(bheat);
            if (db.SaveChanges() > 0)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }

        }

        [HttpPost]
        public JsonResult EditBicycleCompetitionHeat(int heatId, string name)
        {
            var bheat = db.BicycleCompetitionHeats.Where(x => x.Id == heatId).FirstOrDefault();
            bheat.Name = name;
            if (db.SaveChanges() > 0)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }

        }

        [HttpPost]
        public JsonResult RemoveBicycleCompetitionHeat(int heatId)
        {
            var bheat = db.BicycleCompetitionHeats.Where(x => x.Id == heatId).FirstOrDefault();
            db.BicycleCompetitionHeats.Remove(bheat);
            if (db.SaveChanges() > 0)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }

        } 
        



        [HttpPost]
        public JsonResult AddCompetitionRegion(string regionName, int unionId, int seasonId)
        {
            var region = new CompetitionRegion
            {
                region_name = regionName,
                UnionId = unionId,
                SeasonId = seasonId
            };
            db.CompetitionRegions.Add(region);
            if (db.SaveChanges() > 0)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }

        }

        [HttpPost]
        public JsonResult EditCompetitionRegion(int regionId, string regionName)
        {
            var region = db.CompetitionRegions.Where(x => x.id == regionId).FirstOrDefault();
            region.region_name = regionName;
            if (db.SaveChanges() > 0)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }

        }

        [HttpPost]
        public JsonResult RemoveCompetitionRegion(int regionId)
        {
            var region = db.CompetitionRegions.Where(x => x.id == regionId).FirstOrDefault();
            db.CompetitionRegions.Remove(region);
            if (db.SaveChanges() > 0)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }

        }


        [HttpPost]
        public ActionResult SetMinimumParticipationRequired(int Requirement, int SeasonId)
        {
            seasonsRepository.SetMinimumParticipationRequired(Requirement, SeasonId);
            return Json(new { Success = true });
        }

        [HttpPost]
        public JsonResult AddCompetitionAge(string ageName, string frombirth, string tobirth,
            int unionId, int seasonId, int? gender, int? fromWeight, int? toWeight, int? disciplineId, int? friendshipTypeId, int? averageAge, int? fromAge, int? toAge, bool? isUCICategory, bool? isMix,
            int[] bicycleDiscIds, int[] friendshipTypeIds, bool? isIsraelChampionship, bool? isHidden, string foreignName)
        {
            var age = new CompetitionAge
            {
                age_name = ageName,
                from_birth = string.IsNullOrEmpty(frombirth) ? (DateTime?)null : DateTime.Parse(frombirth),
                to_birth = string.IsNullOrEmpty(tobirth) ? (DateTime?)null : DateTime.Parse(tobirth),
                from_weight = fromWeight,
                to_weight = toWeight,
                gender = gender,
                UnionId = unionId,
                SeasonId = seasonId,
                AverageAge = averageAge,
                IsMix = isMix,
                IsHidden = isHidden
            };

            var success = 1;
            var sectionAlias = unionsRepo.GetById(age.UnionId.Value)?.Section?.Alias;
            if (string.Equals(sectionAlias, GamesAlias.Athletics, StringComparison.CurrentCultureIgnoreCase))
            {
                age.IsHidden = isHidden;
            }
            if (string.Equals(sectionAlias, GamesAlias.Bicycle, StringComparison.CurrentCultureIgnoreCase))
            {
                age.DisciplineId = null;
                age.FriendshipTypeId = null;
                age.from_age = fromAge;
                age.to_age = toAge;
                age.IsUCICategory = isUCICategory;
                age.IsIsraelChampionship = isIsraelChampionship;
                age.age_foreign_name = foreignName;

                bicycleDiscIds = bicycleDiscIds?.Where(x => x != 0).ToArray();
                friendshipTypeIds = friendshipTypeIds?.Where(x => x != 0).ToArray();

                if (bicycleDiscIds != null && bicycleDiscIds.Length > 0)
                {        
                    //validate disciplines
                    if(isIsraelChampionship == true)
                    {
                        foreach(var b in bicycleDiscIds)
                        {
                            var d = disciplinesRepo.GetById(b);
                            if (d != null && d.RoadHeat != true)
                            {
                                return Json(new { Success = false, Error = Messages.IsraelChampionshipAllowedOnlyForRoadHeats });
                            }
                        }
                    }

                    if (friendshipTypeIds != null && friendshipTypeIds.Length > 0)
                    {
                        foreach (var b in bicycleDiscIds)
                        {
                            foreach (var f in friendshipTypeIds)
                            {
                                var bic_age = CopyCompetitionAge(age);
                                bic_age.DisciplineId = b;
                                bic_age.FriendshipTypeId = f;
                                db.CompetitionAges.Add(bic_age);
                                success = success & db.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        foreach (var b in bicycleDiscIds)
                        {
                            var bic_age = CopyCompetitionAge(age);
                            bic_age.DisciplineId = b;
                            db.CompetitionAges.Add(bic_age);
                            success = success & db.SaveChanges();
                        }
                    }
                }
                else
                {
                    if (friendshipTypeIds != null && bicycleDiscIds.Length > 0)
                    {
                        foreach (var f in friendshipTypeIds)
                        {
                            var bic_age = CopyCompetitionAge(age);
                            bic_age.FriendshipTypeId = f;
                            db.CompetitionAges.Add(bic_age);
                            success = success & db.SaveChanges();
                        }
                    }
                    else
                        db.CompetitionAges.Add(age);
                        success = success & db.SaveChanges();
                }

            
            }
            else
            {
                db.CompetitionAges.Add(age);
                success = success & db.SaveChanges();
            }
            if (success > 0)
            {
                return Json(new { Success = true, Id = db.CompetitionAges.OrderByDescending(x => x.id).FirstOrDefault().id });
            }
            else
            {
                return Json(new { Success = false, Error = "Database Error" });
            }


        }

        private CompetitionAge CopyCompetitionAge(CompetitionAge age)
        {
            return new CompetitionAge()
            {
                age_name = age.age_name,
                from_birth = age.from_birth,
                to_birth = age.to_birth,
                from_weight = age.from_weight,
                to_weight = age.to_weight,
                gender = age.gender,
                UnionId = age.UnionId,
                SeasonId = age.SeasonId,
                AverageAge = age.AverageAge,
                from_age = age.from_age,
                to_age = age.to_age,
                IsUCICategory = age.IsUCICategory,
                IsIsraelChampionship = age.IsIsraelChampionship
            };
        }

        [HttpPost]
        public JsonResult EditCompetitionAge(int ageId, string ageName, string frombirth,
            string tobirth, int? gender, decimal? fromWeight, decimal? toWeight, int? disciplineId, int? friendshipTypeId, int? averageAge, int? fromAge, int? toAge, bool? isUCICategory, bool? isMix, bool? isIsraelChampionship, bool? isHidden, string foreignName)
        {
            var age = db.CompetitionAges.FirstOrDefault(x => x.id == ageId);
            age.age_name = ageName;
            age.from_birth = string.IsNullOrEmpty(frombirth) ? (DateTime?)null : DateTime.Parse(frombirth);
            age.to_birth = string.IsNullOrEmpty(tobirth) ? (DateTime?)null : DateTime.Parse(tobirth);
            age.from_weight = fromWeight;
            age.to_weight = toWeight;
            age.gender = gender;
            age.AverageAge = averageAge;
            age.IsMix = isMix;
            var sectionAlias = unionsRepo.GetById(age.UnionId.Value)?.Section?.Alias;
            if (string.Equals(sectionAlias, GamesAlias.Athletics, StringComparison.CurrentCultureIgnoreCase))
            {
                age.IsHidden = isHidden;
            }
            if (string.Equals(sectionAlias, GamesAlias.Bicycle, StringComparison.CurrentCultureIgnoreCase))
            {
                if (disciplineId == 0)
                {
                    disciplineId = null;
                }
                if (friendshipTypeId == 0)
                {
                    friendshipTypeId = null;
                }
                //validate disciplines
                if (isIsraelChampionship == true && disciplineId != null)
                {
                    var d = disciplinesRepo.GetById(disciplineId.Value);

                    if (d != null && d.RoadHeat != true)
                    {
                        return Json(new { Success = false, Error = Messages.IsraelChampionshipAllowedOnlyForRoadHeats });
                    }                    
                }

                age.DisciplineId = disciplineId;
                age.FriendshipTypeId = friendshipTypeId;
                age.from_age = fromAge;
                age.to_age = toAge;
                age.IsUCICategory = isUCICategory;
                age.IsIsraelChampionship = isIsraelChampionship;
                age.age_foreign_name = foreignName;
            }

            if (db.SaveChanges() > 0)
            {
                db.Entry(age).Reload();
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = Messages.NoChangesWereMade });
            }
        }

        [HttpPost]
        public JsonResult EditAthleticLeague(int id, string name)
        {
            var league = db.AthleticLeagues.FirstOrDefault(x => x.Id == id);
            league.Name = name;
            if (db.SaveChanges() > 0)
            {
                db.Entry(league).Reload();
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = Messages.NoChangesWereMade });
            }

        }
        [HttpPost]
        public JsonResult RemoveCompetitionAge(int ageId)
        {
            var age = db.CompetitionAges.Where(x => x.id == ageId).FirstOrDefault();
            if (age != null)
            {
                var relatedCompsActive = db.CompetitionDisciplines.Where(x => x.CategoryId == ageId && !x.IsDeleted);
                var relatedCompsDeleted = db.CompetitionDisciplines.Where(x => x.CategoryId == ageId && x.IsDeleted);

                var relatedExperties = db.CompetitionExpertiesHeatsAges.Where(x => x.CompetitionAgeId == ageId);
                if (relatedCompsActive.Count() > 0 || relatedExperties.Count() > 0)
                {
                    return Json(new { Success = false, Error = Messages.HasRelatedCompetition });
                }
                else
                {
                    foreach (var comp in relatedCompsDeleted)
                    {
                        comp.CategoryId = 1;
                    }
                    db.CompetitionAges.Remove(age);
                    if (db.SaveChanges() > 0)
                    {
                        return Json(new { Success = true });
                    }
                    else
                    {
                        return Json(new { Success = false, Error = Messages.NoChangesWereMade });
                    }
                }

            }

            return Json(new { Success = false, Error = "Database Error" });


        }


        [HttpPost]
        public JsonResult RemoveCompetitionFromLeague(int athleticLeagueId)
        {
            var league = db.AthleticLeagues.Where(x => x.Id == athleticLeagueId).FirstOrDefault();
            if (league != null)
            {
                var relatedCompsActive = db.Leagues.Where(x => x.AthleticLeagueId == athleticLeagueId);
                if (relatedCompsActive.Count() > 0)
                {
                    return Json(new { Success = false, Error = Messages.HasRelatedCompetition });
                }
                else
                {
                    db.AthleticLeagues.Remove(league);
                    db.SaveChanges();
                    return Json(new { Success = true });
                }

            }

            return Json(new { Success = false, Error = "Database Error" });
        }



        public ActionResult Competitions(int? unionId, int? disciplineId, int? clubId, int seasonId)
        {
            var result = new TournamentsPDF
            {
                UnionId = unionId,
                Et = TournamentsPDF.EditType.TmntSectionClub,
                SeasonId = seasonId,
                ClubId = clubId,
                IsDepartment = false
            };

            result.listLevels = db.CompetitionLevels.Where(x => x.UnionId == unionId && x.SeasonId == seasonId).ToList();
            result.listRegions = db.CompetitionRegions.Where(x => x.UnionId == unionId && x.SeasonId == seasonId).ToList();
            result.listAges = db.CompetitionAges.Where(x => x.UnionId == unionId && x.SeasonId == seasonId).ToList();
            result.listAthleticLeagues = db.AthleticLeagues.Where(x => x.SeasonId == seasonId).ToList();
            result.listClubs = clubsRepo.GetByUnion(unionId.Value, seasonId).Select(c => new SelectListItem {
                Text = c.Name,
                Value = c.ClubId.ToString(),
                Selected = false
            });
            result.listBicycleDisciplines = db.BicycleCompetitionDisciplines.Where(x => x.UnionId == unionId && x.SeasonId == seasonId).ToList();
            result.listBicycleCompetitionHeats = db.BicycleCompetitionHeats.Where(x => x.UnionId == unionId && x.SeasonId == seasonId).ToList();
            result.listDisciplineExpertise = result.listBicycleDisciplines.SelectMany(x => x.DisciplineExpertises).OrderBy(x=> x.Id).ToList();

            if (clubId.HasValue)
            {
                var club = clubsRepo.GetById(clubId.Value);
                result.IsDepartment = club?.ParentClubId != null ? true : false;
            }

            if (clubId.HasValue)
            {
                result.listLeagues = leagueRepo.GetByClub(clubId.Value, seasonId)
                    .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                    .ToList();
                if (unionId.HasValue)
                {
                    result.Et = TournamentsPDF.EditType.TmntUnionClub;
                }
                else
                {
                    result.Et = TournamentsPDF.EditType.TmntSectionClub;
                }
            }
            else if (disciplineId.HasValue)
            {
                result.DisciplineId = disciplineId;
                result.UnionId = disciplinesRepo.GetById(disciplineId.Value).UnionId;
                result.listLeagues = leagueRepo.GetByDiscipline(disciplineId.Value, seasonId)
                    .Where(x => x.EilatTournament != null && x.EilatTournament == true)
                    .ToList();
                result.Et = unionId.HasValue ? TournamentsPDF.EditType.TmntUnionClub : TournamentsPDF.EditType.TmntSectionClub;
            }
            else if (unionId.HasValue)
            {
                result.Et = TournamentsPDF.EditType.TmntUnion;
                if (User.IsInAnyRole(AppRole.Workers))
                {
                    switch (usersRepo.GetTopLevelJob(base.AdminId))
                    {
                        case JobRole.UnionManager:
                        case JobRole.Unionviewer:
                        case JobRole.RefereeAssignment:
                            result.listLeagues = leagueRepo.GetByUnion(unionId.Value, seasonId)
                                .Where(x => x.EilatTournament == true)
                                .ToList();
                            break;
                        case JobRole.LeagueManager:
                            result.listLeagues = leagueRepo.GetByManagerId(base.AdminId, seasonId)
                                .Where(x => x.EilatTournament == true)
                                .ToList();
                            break;
                        case JobRole.TeamManager:
                            break;
                    }
                }
                else
                {
                    result.listLeagues = leagueRepo.GetByUnion(unionId.Value, seasonId)
                        .Where(x => x.EilatTournament == true)
                        .ToList();
                }

                var routeToPDF = GlobVars.PdfRoute;
                for (int i = 0; i < result.Count; i++)
                {
                    if (System.IO.File.Exists($"{routeToPDF}PDF{i + 1}.pdf"))
                    {
                        result[i] = $"PDF{i + 1}.pdf";
                    }
                }
            }
            if (clubId.HasValue)
            {
                result.Section = clubsRepo.GetById(clubId.Value)?.Section?.Alias ?? clubsRepo.GetById(clubId.Value).Union?.Section?.Alias;
            }
            else if (disciplineId.HasValue)
            {
                var disciplineUnionId = disciplinesRepo.GetById(disciplineId.Value)?.UnionId;
                if (disciplineUnionId.HasValue)
                {
                    result.Section = unionsRepo.GetById(disciplineUnionId.Value)?.Section?.Alias;
                }
            }
            else
            {
                result.Section = unionsRepo.GetById(unionId.Value)?.Section?.Alias;
            }
            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            if (result.listLeagues != null)
            {
                result.listLeagues = result.listLeagues.OrderBy(c => c.SortOrder).ToList();
            }
            ViewBag.LeaguePlayersCount = GetLeaguePlayersCountDictionary(result.listLeagues, result.Section, unionId.Value, seasonId);
            if (unionId.HasValue && string.Equals(result.Section, GamesAlias.Bicycle, StringComparison.CurrentCultureIgnoreCase))
            {
                FillFriendshipTypesAndDisciplines(unionId, result);
            }

            return PartialView("_Competitions", result);
        }

        public ActionResult BicycleCompetitionAges(int? unionId, int? disciplineId, int? clubId, int seasonId)
        {
            var result = new TournamentsPDF
            {
                UnionId = unionId,
                Et = TournamentsPDF.EditType.TmntSectionClub,
                SeasonId = seasonId,
                ClubId = clubId,
                IsDepartment = false
            };

            result.listAges = db.CompetitionAges.Where(x => x.UnionId == unionId && x.SeasonId == seasonId).ToList();
            result.Section = unionsRepo.GetById(unionId.Value)?.Section?.Alias;

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.LeaguePlayersCount = GetLeaguePlayersCountDictionary(result.listLeagues, result.Section, unionId.Value, seasonId);
            FillFriendshipTypesAndDisciplines(unionId, result, false);

            return PartialView("_BicycleCompetitionAges", result);
        }

        private void FillFriendshipTypesAndDisciplines(int? unionId, TournamentsPDF result, bool addSelect = true)
        {
            if(addSelect)
                result.listDisciplines.Add(new SelectListItem
                {
                    Value = "0",
                    Text = Messages.Select,
                    Selected = true
                });
            var disciplines = disciplinesRepo.GetAllByUnionId(unionId.Value);
            result.listDisciplines.AddRange(disciplines.Select(x => new SelectListItem
            {
                Value = x.DisciplineId.ToString(),
                Text = x.Name,
                Selected = false
            }));
            if(addSelect)
                result.listFriendshipsTypes.Add(new SelectListItem
                {
                    Value = "0",
                    Text = Messages.Select,
                    Selected = true
                });
            var friendshipTypes = friendshipTypesRepo.GetAllByUnionId(unionId.Value);
            result.listFriendshipsTypes.AddRange(friendshipTypes.Select(x => new SelectListItem
            {
                Value = x.FriendshipsTypesId.ToString(),
                Text = x.Name,
                Selected = false
            }));

            foreach (var age in result.listAges)
            {
                List<SelectListItem> disciplinesOptionsList = new List<SelectListItem>();
                disciplinesOptionsList.Add(new SelectListItem
                {
                    Value = "0",
                    Text = Messages.Select,
                    Selected = age.DisciplineId == null
                });
                disciplinesOptionsList.AddRange(disciplines.Select(x => new SelectListItem
                {
                    Value = x.DisciplineId.ToString(),
                    Text = x.Name,
                    Selected = age.DisciplineId == x.DisciplineId
                }));
                result.listAgeDisciplines.Add(age.id, disciplinesOptionsList);

                List<SelectListItem> friendshipTypesOptionsList = new List<SelectListItem>();
                friendshipTypesOptionsList.Add(new SelectListItem
                {
                    Value = "0",
                    Text = Messages.Select,
                    Selected = age.FriendshipTypeId == null
                });
                friendshipTypesOptionsList.AddRange(friendshipTypes.Select(x => new SelectListItem
                {
                    Value = x.FriendshipsTypesId.ToString(),
                    Text = x.Name,
                    Selected = age.FriendshipTypeId == x.FriendshipsTypesId
                }));
                result.listAgeFriendshipsTypes.Add(age.id, friendshipTypesOptionsList);
            }

            result.listBicycleDisciplinesForSelection.Add(new SelectListItem
            {
                Value = "0",
                Text = Messages.Select
            });
            if(result.listBicycleDisciplines != null)
                result.listBicycleDisciplinesForSelection.AddRange(result.listBicycleDisciplines?.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                }));
            if(result.listDisciplineExpertise != null)
                foreach(var exp in result.listDisciplineExpertise)
                {
                    if (exp.BicycleCompetitionDisciplineId == null)
                    {
                        result.listExistingBicycleDisciplinesForExpertiseSelection.Add(exp.Id, result.listBicycleDisciplinesForSelection);
                    }
                    else
                    {
                        List<SelectListItem> discList = new List<SelectListItem>();
                        discList.Add(new SelectListItem
                        {
                            Value = "0",
                            Text = Messages.Select,
                            Selected = exp.BicycleCompetitionDisciplineId == null
                        });
                        discList.AddRange(result.listBicycleDisciplines.Select(x => new SelectListItem
                        {
                            Value = x.Id.ToString(),
                            Text = x.Name,
                            Selected = exp.BicycleCompetitionDisciplineId == x.Id
                        }));
                        result.listExistingBicycleDisciplinesForExpertiseSelection.Add(exp.Id, discList);
                    }
                }
        }

        public ActionResult EilatClubTournament(int clubId)
        {
            return PartialView("_ClubLeagues");
        }

        public ActionResult DeleteLeagues(int id, bool isCompetition = false, bool isTennis = false)
        {
            var lRepo = new LeagueRepo();
            var item = lRepo.GetById(id);

            bool isHasTeams = item.LeagueTeams.Any(t => t.Teams.IsArchive == false);
            if (isHasTeams)
            {
                TempData["ErrId"] = id;
            }
            else
            {
                item.IsArchive = true;
                lRepo.Save();
            }

            if (isCompetition)
            {
                if (isTennis)
                {
                    return RedirectToAction("Competitions", new { unionId = item.UnionId, seasonId = item.SeasonId });
                }
                else
                {
                    return RedirectToAction("EilatTournament", new { unionId = item.UnionId, seasonId = item.SeasonId });
                }
            }
            return RedirectToAction("Leagues", new { id = item.UnionId, seasonId = item.SeasonId });
        }

        public void ExportReferees(int id, int? seasonId = null)
        {
            var unionsLeagues = leagueRepo.GetByUnion(id, seasonId ?? 0).ToList();
            var unionsGames = new List<GamesCycle>();
            foreach (var league in unionsLeagues)
            {
                unionsGames.AddRange(gamesRepo.GetGamesQuery(c => c.Stage.LeagueId == league.LeagueId)
                    .ToList());
            }

            #region Getting number of referees/spectators/desks

            int maxNumberOfReferees = 0;
            int maxNumberOfSpectators = 0;
            int maxNumberOfDesks = 0;
            for (int i = 0; i < unionsGames.Count; i++)
            {
                if (unionsGames[i].RefereeIds != null)
                {
                    var currentNumberOfReferees = unionsGames[i].RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.ToList()?.Count ?? 0;
                    var currentNumberOfSpectators = unionsGames[i].SpectatorIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.ToList()?.Count ?? 0;
                    var currentNumberOfDesks = unionsGames[i].RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.ToList()?.Count ?? 0;
                    if (i == 0)
                    {
                        maxNumberOfReferees = currentNumberOfReferees;
                        maxNumberOfSpectators = currentNumberOfSpectators;
                        maxNumberOfDesks = currentNumberOfDesks;

                    }
                    else
                    {
                        maxNumberOfReferees = currentNumberOfReferees > maxNumberOfReferees ? currentNumberOfReferees : maxNumberOfReferees;
                        maxNumberOfSpectators = currentNumberOfSpectators > maxNumberOfSpectators ? currentNumberOfSpectators : maxNumberOfSpectators;
                        maxNumberOfDesks = currentNumberOfDesks > maxNumberOfDesks ? currentNumberOfDesks : maxNumberOfDesks;
                    }
                }
            }

            //int currentSpectatorCellNum = 6 + maxNumberOfReferees;
            int currentSpectatorCellNum = 7 + maxNumberOfReferees + maxNumberOfDesks;
            int currentDeskCellNum = 6 + maxNumberOfReferees;

            #endregion

            using (XLWorkbook workBook = new XLWorkbook(XLEventTracking.Disabled))
            {

                var ws = workBook.AddWorksheet("ExportReferees");

                #region Columns

                ws.Cell(1, 1).Value = "League";
                ws.Cell(1, 2).Value = "Start Date";
                ws.Cell(1, 3).Value = "Home Team";
                ws.Cell(1, 4).Value = "Guest Team";
                ws.Cell(1, 5).Value = "Auditorium";

                for (int i = 6; i < (6 + maxNumberOfReferees); i++)
                {
                    ws.Cell(1, i).Value = (i == 6) ? "Main referee" : $"Referee #{(i - 6) + 1}";
                }

                for (int i = currentDeskCellNum; i < (currentDeskCellNum + maxNumberOfDesks); i++)
                {
                    ws.Cell(1, i).Value = $"Desk #{(i - currentDeskCellNum) + 1}";
                }

                for (int i = currentSpectatorCellNum; i < (currentSpectatorCellNum + maxNumberOfSpectators); i++)
                {
                    ws.Cell(1, i).Value = $"Spectator #{(i - currentSpectatorCellNum + 1)}";
                }

                #endregion

                for (var i = 0; i < unionsGames.Count; i++)
                {
                    //var refeereeService = new RefereeService(unionsGames[i].CycleId, unionsGames[i].Stage?.LeagueId ?? 0);
                    var refereesIds = unionsGames[i].RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var refereeNames = gamesRepo.GetRefereesNames(unionsGames[i].CycleId);

                    var spectatorIds = unionsGames[i].SpectatorIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var spectatorNames = gamesRepo.GetSpectatorsNames(spectatorIds, unionsGames[i].CycleId);

                    var desksIds = unionsGames[i].DeskIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var deskNames = gamesRepo.GetDeskNames(unionsGames[i].CycleId);

                    ws.Cell(i + 2, 1).DataType = XLDataType.Text;
                    ws.Cell(i + 2, 1).SetValue(unionsGames[i]?.Stage?.League?.Name ?? "");

                    ws.Cell(i + 2, 2).DataType = XLDataType.DateTime;
                    ws.Cell(i + 2, 2).SetValue(unionsGames[i]?.StartDate);

                    ws.Cell(i + 2, 3).DataType = XLDataType.Text;
                    ws.Cell(i + 2, 3).SetValue(unionsGames[i]?.HomeTeam?.Title ?? "");

                    ws.Cell(i + 2, 4).DataType = XLDataType.Text;
                    ws.Cell(i + 2, 4).SetValue(unionsGames[i]?.GuestTeam?.Title ?? "");

                    ws.Cell(i + 2, 5).DataType = XLDataType.Text;
                    ws.Cell(i + 2, 5).SetValue(unionsGames[i]?.Auditorium?.Name ?? "");

                    if (refereeNames != null)
                    {
                        for (int j = 6; j < (6 + maxNumberOfReferees); j++)
                        {
                            string refereeName = "";
                            if (j - 6 < refereeNames.Count)
                            {
                                refereeName = String.IsNullOrEmpty(refereeNames[j - 6]) ? "" : refereeNames[j - 6];
                                ws.Cell(i + 2, j).DataType = XLDataType.Text;
                                ws.Cell(i + 2, j).SetValue(refereeName);
                            }
                        }
                    }

                    if (deskNames != null)
                    {
                        for (int j = currentDeskCellNum; j < (currentDeskCellNum + maxNumberOfDesks); j++)
                        {
                            string deskName = "";
                            if (j - currentDeskCellNum < deskNames.Count)
                            {
                                deskName = String.IsNullOrEmpty(deskNames[j - currentDeskCellNum]) ? "" : deskNames[j - currentDeskCellNum];
                                ws.Cell(i + 2, j).DataType = XLDataType.Text;
                                ws.Cell(i + 2, j).SetValue(deskName);
                            }
                        }
                    }

                    if (spectatorNames != null)
                    {
                        for (int j = currentSpectatorCellNum; j < (currentSpectatorCellNum + maxNumberOfSpectators); j++)
                        {
                            string spectatorName = "";
                            if (j - currentSpectatorCellNum < spectatorNames.Count)
                            {
                                spectatorName = String.IsNullOrEmpty(spectatorNames[j - currentSpectatorCellNum]) ? "" : spectatorNames[j - currentSpectatorCellNum];
                                ws.Cell(i + 2, j).DataType = XLDataType.Text;
                                ws.Cell(i + 2, j).SetValue(spectatorName);
                            }
                        }
                    }
                }

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", $"attachment;filename= ExportReferees.xlsx");

                using (MemoryStream myMemoryStream = new MemoryStream())
                {
                    workBook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        [HttpPost]
        public void ChangeOrder(int unionId, short[] ids, int seasonId, bool isLeagues = true, bool isEilatTournament = false)
        {
            short sortOrder = 0;
            List<League> resList = null;
            if (isEilatTournament)
            {
                if (User.IsInAnyRole(AppRole.Workers))
                {
                    switch (usersRepo.GetTopLevelJob(base.AdminId))
                    {
                        case JobRole.UnionManager:
                            resList = leagueRepo.GetByUnion(unionId, seasonId).Where(x => x.EilatTournament != null && x.EilatTournament == true).OrderBy(x => x.SortOrder).ToList();
                            break;
                        case JobRole.LeagueManager:
                            resList = leagueRepo.GetByManagerId(base.AdminId, seasonId).Where(x => x.EilatTournament != null && x.EilatTournament == true).OrderBy(x => x.SortOrder).ToList();
                            break;
                        case JobRole.TeamManager:
                            break;
                    }
                }
                else
                {
                    resList = leagueRepo.GetByUnion(unionId, seasonId).Where(x => x.EilatTournament != null && x.EilatTournament == true).OrderBy(x => x.SortOrder).ToList();
                }
            }
            else if (isLeagues)
            {
                if (User.IsInAnyRole(AppRole.Workers))
                {
                    switch (usersRepo.GetTopLevelJob(base.AdminId))
                    {
                        case JobRole.UnionManager:
                            resList = leagueRepo.GetByUnion(unionId, seasonId).Where(x => x.EilatTournament == null || ((bool)x.EilatTournament) == false).OrderBy(x => x.SortOrder).ToList();
                            break;
                        case JobRole.LeagueManager:
                            resList = leagueRepo.GetByManagerId(base.AdminId, seasonId).Where(x => x.EilatTournament == null || ((bool)x.EilatTournament) == false).OrderBy(x => x.SortOrder).ToList();
                            break;
                        case JobRole.TeamManager:
                            break;
                    }
                }
                else
                {
                    resList = leagueRepo.GetByUnion(unionId, seasonId).Where(x => x.EilatTournament == null || ((bool)x.EilatTournament) == false).OrderBy(x => x.SortOrder).ToList();
                }
            }
            else
            {
                if (User.IsInAnyRole(AppRole.Workers))
                {
                    switch (usersRepo.GetTopLevelJob(base.AdminId))
                    {
                        case JobRole.UnionManager:
                            resList = leagueRepo.GetByUnion(unionId, seasonId).Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true).OrderBy(x => x.SortOrder).ToList();
                            break;
                        case JobRole.LeagueManager:
                            resList = leagueRepo.GetByManagerId(base.AdminId, seasonId).Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true).OrderBy(x => x.SortOrder).ToList();
                            break;
                        case JobRole.TeamManager:
                            break;
                    }
                }
                else
                {
                    resList = leagueRepo.GetByUnion(unionId, seasonId).Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true).OrderBy(x => x.SortOrder).ToList();
                }
            }
            if (resList != null && resList.Count > 0)
            {
                foreach (var id in ids)
                {
                    var firstOrDefault = resList.FirstOrDefault(x => x.LeagueId == id);
                    if (firstOrDefault != null)
                        firstOrDefault.SortOrder = sortOrder;

                    sortOrder++;

                }
                leagueRepo.Save();
            }
        }

        public ActionResult Calendar(int? unionId, int? seasonId, int? sectionId = null, int? clubId = null)
        {
            string sectionAlias = secRepo.GetById(sectionId ?? 0)?.Alias;
            var isMultiSport = !(String.IsNullOrEmpty(sectionAlias)) && sectionAlias == SectionAliases.MultiSport ? true : false;
            var isDepartment = false;
            if (clubId.HasValue)
            {
                var club = clubsRepo.GetById(clubId.Value);
                isDepartment = club.ParentClub == null ? false : true;
                ViewBag.IsUnionClub = club?.IsUnionClub;
                ViewBag.IsIndividual =
                    club.IsUnionClub.Value ? club.Union.Section.IsIndividual : club.Section.IsIndividual;
            }
            ViewBag.UnionId = unionId;
            ViewBag.SeasonId = seasonId;
            ViewBag.IsMultiSport = isMultiSport;
            ViewBag.IsDepartment = isDepartment;
            ViewBag.ClubId = clubId;
            return PartialView("_Calendar");
        }

       public ActionResult CalendarObject(int unionId, int seasonId, int? clubId)
       {
            if (unionId != 0)
            {
                var unionCalendarService = new UnionGamesService(unionId, seasonId);
                var isIndividual = unionsRepo.GetById(unionId)?.Section?.IsIndividual;
                IEnumerable<GameDto> games = unionCalendarService.GetAllGames(isIndividual);
                IEnumerable<GameDto> competitions = null;
                var union = unionsRepo.GetById(unionId);
                if (union.Section.IsIndividual)
                {
                    competitions = unionCalendarService.GetAllCompetitions(union.Section.Alias == SectionAliases.Tennis);
                }

                var unionEvents = unionsRepo.GetAllUnionEvents(unionId, true);
                var calendarHelper = new UnionCalendarHelper(games, competitions, unionEvents);
                var model = calendarHelper.GetCalendarObject(isIndividual, unionId);
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            else if(clubId.HasValue)
            {
                var unionCalendarService = new UnionGamesService(unionId, seasonId);
                IEnumerable<GameDto> games = unionCalendarService.GetAllClubGames(clubId.Value);
                var calendarHelper = new UnionCalendarHelper(games, null, null);
                var model = calendarHelper.GetClubsCalendarObject();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateAthleticLeague(string name, int seasonId)
        {
            leagueRepo.CreateAthleticLeague(name, seasonId);
            return Json(new { });
        }



        [HttpPost]
        public ActionResult CreateNewRecord(string Name, int? Format, int SectionId, int UnionId, int SeasonId)
        {
            if (Format.HasValue)
            {
                unionsRepo.CreateNewRecord(Name, Format.Value, UnionId);
            }
            return RedirectToAction("Records", new { sectionId = SectionId, unionId = UnionId, seasonId = SeasonId });
        }


        [HttpPost]
        public ActionResult ChangeDisciplineRelatedToRecord(int disciplineId, int recordId, bool isChecked)
        {
            unionsRepo.ChangeDisciplineRelatedToRecord(disciplineId, recordId, isChecked);
            return Json(new { });
        }

        [HttpPost]
        public ActionResult ChangeCategoryRelatedToRecord(int categoryId, int recordId, bool isChecked)
        {
            unionsRepo.ChangeCategoryRelatedToRecord(categoryId, recordId, isChecked);
            return Json(new { });
        }


        [HttpPost]
        public ActionResult EditRecordBests(int RecordId, int SectionId, int UnionId, int SeasonId, string IntentionalIsraeliRecord, string IsraeliRecord, string SeasonRecord)
        {
            unionsRepo.EditRecordBests(RecordId, IntentionalIsraeliRecord, IsraeliRecord, SeasonRecord, SeasonId);
            return RedirectToAction("Records", new { sectionId = SectionId, unionId = UnionId, seasonId = SeasonId });
        }
        


        public ActionResult RemoveRecord(int Id, int SectionId, int UnionId, int SeasonId)
        {
            unionsRepo.RemoveRecord(Id);
            return RedirectToAction("Records", new {sectionId = SectionId, unionId = UnionId, seasonId = SeasonId });
        }
        

        public ActionResult Records(int sectionId, int unionId, int seasonId)
        {
            ViewBag.Disciplines = disciplinesRepo.GetBySection(sectionId, unionId);
            ViewBag.Categories = db.CompetitionAges.Where(x => x.UnionId == unionId && x.SeasonId == seasonId).ToList();
            ViewBag.UnionId = unionId;
            ViewBag.SectionId = sectionId;
            ViewBag.SeasonId = seasonId;
            var disciplineRecords = unionsRepo.GetById(unionId).DisciplineRecords.ToList();

            return PartialView("_Records", disciplineRecords);
        }


        public ActionResult Benefits(int sectionId, int unionId, int seasonId)
        {
            /*
            ViewBag.Disciplines = disciplinesRepo.GetBySection(sectionId, unionId);
            ViewBag.Categories = db.CompetitionAges.Where(x => x.UnionId == unionId && x.SeasonId == seasonId).ToList();
            ViewBag.UnionId = unionId;
            ViewBag.SectionId = sectionId;
            ViewBag.SeasonId = seasonId;
            var disciplineRecords = unionsRepo.GetById(unionId).DisciplineRecords.ToList();
            */
            return PartialView("_Benefits", new { });
        }

        public void DownloadExcel(int? unionId, int? clubId, int? leagueId)
        {
            string sectionAlias = secRepo.GetAlias(unionId, clubId, leagueId);
            bool isSectionClub = false;
            if (clubId.HasValue)
            {
                isSectionClub = clubsRepo.GetById(clubId.Value)?.IsSectionClub == true;
            }

            using (var workbook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var ws = workbook.AddWorksheet(Messages.ImportPlayers);

                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                #region Excel header

                addCell("*" + Messages.FirstName);
                addCell("*" + Messages.LastName);
                addCell(Messages.MiddleName);
                addCell($"*{Messages.Team} {Messages.Id}");
                addCell($"*{Messages.IdentNum}");
                addCell($"{Messages.Email}");
                addCell($"{Messages.Phone} {Messages.Number.ToLower()}");
                addCell($"*{Messages.BirthDay} (dd/mm/yyyy)");
                addCell($"*{Messages.Gender} {Messages.Male.ToLower()}/{Messages.Female.ToLower()}");
                addCell(Messages.Height);
                addCell(Messages.City);
                addCell(Messages.PlayerCardNumber);
                addCell(Messages.ShirtNumber);
                addCell(Messages.MedExamDate);
                addCell(Messages.DateOfInsuranceValidity);
                if (sectionAlias.Equals(GamesAlias.Bicycle, StringComparison.OrdinalIgnoreCase))
                {
                    addCell(Messages.FriendshipName);
                    addCell(Messages.FrienshipPriceTypeName);
                    addCell(Messages.RoadHeat);
                    addCell(Messages.MountainHeat);
                    addCell(Messages.RoadIronNumber);
                    addCell(Messages.MountainIronNumber);
                    addCell(Messages.VelodromeIronNumber);
                    addCell(Messages.UciId);
                    addCell(Messages.ChipNumber);
                    addCell(Messages.KitStatus + "(" + Messages.Ready + "/" + Messages.Provided + "/" + Messages.Printed + ")");
                }
                if (sectionAlias.Equals(GamesAlias.Tennis, StringComparison.OrdinalIgnoreCase))
                    addCell(Messages.TenicardValidity);
                if (isSectionClub)
                    addCell(Messages.Comment);
                addCell(Messages.ForeignFirstName);
                addCell(Messages.ForeignLastName);
                addCell(Messages.PostalCode);
                addCell(Messages.PassportNum);
                addCell(Messages.Address);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                #endregion

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", $"attachment;filename={Messages.ImportPlayers.ToLower().Replace(" ", "-")}.xlsx");

                using (MemoryStream myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        [HttpPost]
        public ActionResult ImportImages(EditUnionForm model)
        {
            var identNums = new List<string>();
            for (var i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];

                if (file != null)
                {
                    identNums.Add(Path.GetFileNameWithoutExtension(file.FileName));
                }
            }

            var playersToImport = usersRepo.GetCollection<User>(x => identNums.Contains(x.IdentNum)).ToList();
            var savePath = Server.MapPath($"{GlobVars.ContentPath}/players/");

            for (var i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];
                if (file == null) continue;

                var identNum = Path.GetFileNameWithoutExtension(file.FileName);

                var player = playersToImport.FirstOrDefault(x => x.IdentNum == identNum);
                if (player == null) continue;

                var fileName = FileUtil.SaveFile(file, savePath, identNum, PlayerFileType.PlayerImage);

                if (fileName == null) continue;

                player.Image = fileName;
            }

            usersRepo.Save();

            return RedirectToAction("Edit", new { id = model.UnionId, seasonId = model.SeasonId });
        }

        [HttpPost]
        public ActionResult ImportIdFiles(EditUnionForm model)
        {
            var identNums = new List<string>();
            for (var i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];

                if (file != null)
                {
                    identNums.Add(Path.GetFileNameWithoutExtension(file.FileName));
                }
            }

            var playersToImport = usersRepo.GetCollection<User>(x => identNums.Contains(x.IdentNum)).ToList();
            var savePath = Server.MapPath($"{GlobVars.ContentPath}/players/");

            for (var i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];
                if (file == null) continue;

                var identNum = Path.GetFileNameWithoutExtension(file.FileName);

                var player = playersToImport.FirstOrDefault(x => x.IdentNum == identNum);
                if (player == null) continue;

                var fileName = FileUtil.SaveFile(file, savePath, identNum, PlayerFileType.IDFile);

                if (fileName == null) continue;

                player.IDFile = fileName;
            }

            usersRepo.Save();

            return RedirectToAction("Edit", new { id = model.UnionId, seasonId = model.SeasonId });
        }

        // GET: MedicalInstitutes
        public ActionResult MedicalInstitutes(int? unionId, int? seasonId)
        {
            var medicalInstitutes = unionsRepo.GetMedicalInstitutes(unionId, seasonId).Select(x=> new MedicalInstituteModel
            {
                MedicalInstitutesId = x.MedicalInstitutesId,
                Name = x.Name,
                Address = x.Address
            }).ToList();
            var vm = new MedicalInstituteForm
            {
                UnionId = unionId,
                SeasonId = seasonId,
                MedicalInstitutes = medicalInstitutes
            };

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.UnionId = unionId;
            ViewBag.SeasonId = seasonId;
            
            return PartialView("_MedicalInstitutes", vm);

        }

        [HttpPost]
        public ActionResult CreateMedicalInstitute(MedicalInstituteForm frm)
        {
            MedicalInstitute medicalInstitute;
            if (frm.MedicalInstitutesId > 0)
            {
                medicalInstitute = unionsRepo.GetMedicalInstituteById(frm.MedicalInstitutesId);
                medicalInstitute.Name = frm.Name;
                medicalInstitute.Address = frm.Address;
            }
            else
            {
                medicalInstitute = new MedicalInstitute
                {
                    UnionId = frm.UnionId ?? 0,
                    SeasonId = frm.SeasonId ?? 0,
                    Name = frm.Name,
                    Address = frm.Address
                };
                unionsRepo.CreateMedicalInstitute(medicalInstitute);
            }

            unionsRepo.Save();

            TempData["SavedId"] = medicalInstitute.MedicalInstitutesId;

            return RedirectToAction("MedicalInstitutes", new { unionId = frm.UnionId, seasonId = frm.SeasonId });
        }

        public ActionResult DeleteMedicalInstitute(int id, int? unionId, int? seasonId)
        {
            unionsRepo.DeleteMedicalInstituteById(id);
            return RedirectToAction("MedicalInstitutes", new { unionId, seasonId });
        }

    }
}