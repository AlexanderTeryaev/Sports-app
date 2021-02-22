using AppModel;
using ClosedXML.Excel;
using CmsApp.Models;
using Resources;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using CmsApp.Helpers;
using CmsApp.Helpers.ActivityHelpers;
using CmsApp.Services;
using DataService;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using Newtonsoft.Json;

namespace CmsApp.Controllers
{
    public class ActivityController : AdminController
    {
        private const string ExportActivitiesResultSessionKey = "ExportActivities_Result";
        private const string ExportActivitiesResultFileNameSessionKey = "ExportActivities_ResultFileName";


        public ActionResult List(int? unionId, int? seasonId, int? clubId)
        {
            var jobRole = usersRepo.GetTopLevelJob(AdminId);

            ViewBag.IsActivityManager = jobsRepo.IsActivityManager(AdminId);
            ViewBag.IsActivityViewer = jobsRepo.IsActivityViewer(AdminId);
            ViewBag.UnionId = unionId;
            ViewBag.JobRole = jobRole;
            ViewBag.ClubLevel = false;
            ViewBag.IsUnionViewer = AuthSvc.AuthorizeUnionViewerByManagerId(unionId ?? 0, AdminId);
            if (jobRole != JobRole.UnionManager && jobRole != JobRole.ClubManager && jobRole != JobRole.ClubSecretary && jobRole != JobRole.DepartmentManager &&
                !User.IsInAnyRole(AppRole.Admins) && !ViewBag.IsActivityManager && !ViewBag.IsActivityViewer && !ViewBag.IsUnionViewer)
            {
                return PartialView("_List", new List<ActivityBranchViewModel>());
            }

            if (clubId.HasValue && clubId.Value > 0)
            {
                ViewBag.ClubLevel = true;
                ViewBag.ClubId = clubId;

                seasonId = seasonId ?? seasonsRepository.GetLastSeasonIdByCurrentClubId((int) clubId);

                ViewBag.SeasonId = seasonId;
                var clubActivities = LoadActivities(null, seasonId, clubId, jobRole, ViewBag.IsActivityManager,
                    ViewBag.IsActivityViewer, "all");
                return PartialView("_List", clubActivities);
            }

            seasonId = seasonId ?? GetUnionCurrentSeasonFromSession();
            ViewBag.SeasonId = seasonId;

            var activities = LoadActivities(unionId, seasonId, null, jobRole, ViewBag.IsActivityManager,
                ViewBag.IsActivityViewer, "all");

            return PartialView("_List", activities);
        }

        public ActionResult ActivityList(int? unionId, int? seasonId, int? clubId, string roleType = null)
        {
            if (!string.IsNullOrWhiteSpace(roleType))
            {
                this.SetWorkerSession(roleType);
            }
            return View();
        }


        [HttpGet]
        public ActionResult EditBranch(int? branchId, int? unionId, int? seasonId, int? clubId)
        {
            ActivityBranchForm branchForm = null;

            if (branchId.HasValue)
            {
                var branch = activityRepo.GetBranchById(branchId.Value);

                branchForm = new ActivityBranchForm(branch);
            }
            else
            {
                branchForm = new ActivityBranchForm { UnionId = unionId, ClubId = clubId };
            }

            return PartialView("_EditBranch", branchForm);
        }

        [HttpPost]
        public ActionResult EditBranch(ActivityBranchForm model)
        {
            if (ModelState.IsValid)
            {
                var branch = activityRepo.GetBranchById(model.ActivityBranchId);

                if (branch == null)
                {
                    branch = new ActivityBranch();
                    branch.SeasonId = model.SeasonId;
                    branch.UnionId = model.UnionId;
                    branch.ClubId = model.ClubId;
                    db.ActivityBranches.Add(branch);
                }
                else
                {
                    db.Entry(branch).State = System.Data.Entity.EntityState.Modified;
                }

                branch.BranchName = model.ActivityBranchName;

                activityRepo.Save();

                ViewBag.Saved = true;
            }

            return PartialView("_EditBranch", model);
        }

        [HttpGet]
        public ActionResult Edit(int? activityId, int? unionId, int? clubId, int? seasonId, int? branchId,
            bool? readOnly)
        {
            ActivityModel activityForm;
            if (activityId.HasValue)
            {
                var activity = activityRepo.GetByIdAsNoTracking(activityId.Value);

                activityForm = new ActivityModel(activity)
                {
                    ActivitiesPrices = activity.ActivitiesPrices
                        .Select(p => new ActivitiesPriceForm
                        {
                            Price = p.Price,
                            ActivityId = p.ActivityId,
                            ActivityPeriodId = p.ActivityPeriodId,
                            EndDate = p.EndDate,
                            StartDate = p.StartDate,
                            PaymentDescription = p.PaymentDescription
                        })
                        .ToList()
                };

                var restrictedLeagues = !string.IsNullOrWhiteSpace(activity.RestrictedLeagues)
                    ? JsonConvert.DeserializeObject<List<int>>(activity.RestrictedLeagues)
                    : new List<int>();
                activityForm.RestrictedLeagues =
                    leagueRepo.GetCollection<League>(x => restrictedLeagues.Contains(x.LeagueId)).ToList();
                activityForm.RestrictedLeaguesJson = activity.RestrictedLeagues;
                activityForm.Leagues = activity.UnionId != null && activity.ClubId == null && activity.SeasonId != null
                    ? leagueRepo.GetByUnion(activity.UnionId.Value, activity.SeasonId.Value).ToList()
                    : new List<League>();

                if (activity.Union?.Section?.IsIndividual == true)
                {
                    activityForm.Leagues = activityForm.Leagues.Where(x => x.EilatTournament == true).ToList();
                }

                var restrictedSchools = !string.IsNullOrWhiteSpace(activity.RestrictedSchools)
                    ? JsonConvert.DeserializeObject<List<int>>(activity.RestrictedSchools)
                    : new List<int>();
                activityForm.RestrictedSchools =
                    SchoolRepo.GetCollection<School>(x => restrictedSchools.Contains(x.Id)).ToList();
                activityForm.RestrictedSchoolsJson = activity.RestrictedSchools;
                activityForm.Schools = activity.UnionId == null && activity.ClubId != null && activity.SeasonId != null
                    ? SchoolRepo.GetCollection<School>(x => x.ClubId == activity.ClubId && x.SeasonId == activity.SeasonId).ToList()
                    : new List<School>();

                activityForm.RestrictedTeamsJson = activity.RestrictedTeams;

                activityForm.ActivityManager = activity.ActivitiesUsers.Where(p => p.UserGroup == 0)
                    .Select(p => new ActivitiesUserForm { UserId = p.UserId, UserName = p.User.FullName })
                    .ToList();
                activityForm.ActivityViewer = activity.ActivitiesUsers.Where(p => p.UserGroup == 1)
                    .Select(p => new ActivitiesUserForm { UserId = p.UserId, UserName = p.User.FullName })
                    .ToList();
                //activityForm.UnionManager = jobsRepo.GetUnionUsersJobs(activity.UnionId).Select(p => new SelectListItem { Value = p.UserId.ToString(), Text = p.FullName }).ToList();

                var official = new List<UserJobDto>();

                if (activity.UnionId.HasValue)
                {
                    official = jobsRepo.GetUnionUsersJobs(activity.UnionId ?? 0, activity.SeasonId).ToList();

                    var leagues = leagueRepo.GetByUnion(activity.UnionId ?? 0, activity.SeasonId.Value);
                    foreach (var league in leagues)
                    {
                        official.AddRange(jobsRepo.GetLeagueUsersJobs(league.LeagueId, activity.SeasonId));

                        var teams = teamRepo.GetTeamsByLeague(league.LeagueId);

                        var teamsIds = teams.Select(x => x.TeamId).ToArray();

                        official.AddRange(jobsRepo.GetTeamUsersJobs(teamsIds, activity.SeasonId));
                    }
                } else if (activity.ClubId.HasValue)
                {
                    official = jobsRepo.GetClubOfficials(activity.ClubId.Value, activity.SeasonId).ToList();
                }

                activityForm.UnionManager = official.GroupBy(p => p.UserId)
                    .Select(p => p.FirstOrDefault())
                    .ToList()
                    .Select(p => new SelectListItem { Value = p.UserId.ToString(), Text = p.FullName })
                    .ToList();
            }
            else
            {
                activityForm = new ActivityModel
                {
                    DefaultLanguage = "he",
                    UnionId = unionId,
                    ClubId = clubId,
                    BranchId = branchId.Value,
                    SeasonId = seasonId ?? getCurrentSeason()?.Id,
                    RestrictLeagues = false,
                    RestrictedLeagues = new List<League>(),
                    Leagues = unionId != null && clubId == null && seasonId != null
                        ? leagueRepo.GetByUnion(unionId.Value, seasonId.Value).ToList()
                        : new List<League>(),
                    RestrictSchools = false,
                    RestrictedSchools = new List<School>(),
                    Schools = unionId == null && clubId != null && seasonId != null
                        ? SchoolRepo.GetCollection<School>(x => x.ClubId == clubId && x.SeasonId == seasonId).ToList()
                        : new List<School>(),
                    //UnionManager = jobsRepo.GetUnionUsersJobs(unionId.Value).Select(p => new SelectListItem { Value = p.UserId.ToString(), Text = p.FullName }).ToList();
                };

                var official = new List<UserJobDto>();
                if (unionId.HasValue)
                {
                    var union = unionsRepo.GetById(unionId.Value);

                    activityForm.IsIndividualSection = union?.Section?.IsIndividual == true;

                    activityForm.IsUkrainianUnion = unionId == GlobVars.UkraineGymnasticUnionId;

                    official = jobsRepo.GetUnionUsersJobs(unionId.Value, seasonId).ToList();

                    var leagues = leagueRepo.GetByUnion(unionId.Value, seasonId.Value);
                    foreach (var league in leagues)
                    {
                        official.AddRange(jobsRepo.GetLeagueUsersJobs(league.LeagueId, seasonId));

                        var teams = teamRepo.GetTeamsByLeague(league.LeagueId);

                        var teamsIds = teams.Select(x => x.TeamId).ToArray();

                        official.AddRange(jobsRepo.GetTeamUsersJobs(teamsIds, seasonId));
                    }
                }
                else if (clubId.HasValue)
                {
                    official = jobsRepo.GetClubOfficials(clubId.Value, seasonId).ToList();

                    var club = clubsRepo.GetById(clubId.Value);

                    activityForm.IsUkrainianUnion = club?.UnionId == GlobVars.UkraineGymnasticUnionId;
                }

                activityForm.UnionManager = official.GroupBy(p => p.UserId)
                    .Select(p => p.FirstOrDefault())
                    .ToList()
                    .Select(p => new SelectListItem { Value = p.UserId.ToString(), Text = p.FullName })
                    .ToList();
            }

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);

            ViewBag.IsActivityManager = jobsRepo.IsActivityManager(AdminId);
            ViewBag.IsActivityViewer = activityForm.ActivityViewer.Any(p => p.UserId == AdminId);

            if (readOnly == null)
            {
                if (User.IsInAnyRole(AppRole.Admins) ||
                    User.HasTopLevelJob(JobRole.UnionManager) ||
                    User.HasTopLevelJob(JobRole.ClubManager) ||
                    User.HasTopLevelJob(JobRole.ClubSecretary) ||
                    User.HasTopLevelJob(JobRole.DepartmentManager))
                {
                    ViewBag.IsReadOnly = false;
                }
                else
                {
                    ViewBag.IsReadOnly = activityForm.ActivityManager.All(p => p.UserId != AdminId);
                }
            }
            else
            {
                ViewBag.IsReadOnly = readOnly;
            }


            return PartialView("_Edit", activityForm);
        }

        [HttpPost]
        public ActionResult AddNewCustomPrice(int activityId, int index, bool isUkrainianUnion = false)
        {
#if DEBUG
            Thread.Sleep(1000);
#endif
            var newPrice = new ActivityCustomPrice
            {
                TitleEng = Messages.Activity_CustomPrices_TitleEng_Default,
                TitleHeb = Messages.Activity_CustomPrices_TitleHeb_Default,
                TitleUk = Messages.Activity_CustomPrices_TitleUk_Default,
                Price = 0,
                MaxQuantity = 1
            };

            return PartialView("_CustomPriceTemplate", new Tuple<ActivityCustomPrice, int, bool>(newPrice, index, isUkrainianUnion));
        }

        [HttpPost]
        public ActionResult RemoveCustomPrice(int id)
        {
#if DEBUG
            Thread.Sleep(1000);
#endif

            var price = activityRepo.GetCollection<ActivityCustomPrice>(x => x.Id == id).FirstOrDefault();

            if (price != null)
            {
                var customPriceFilter = $"\"{nameof(ActivityCustomPriceModel.PropertyName)}\":\"customPrice-{price.Id}\"";

                var registrationWithCustomPrice = db.ActivityFormsSubmittedDatas
                    .FirstOrDefault(x =>
                        x.CustomPrices.Contains(customPriceFilter) &&
                        x.ActivityId == price.ActivityId);

                if (registrationWithCustomPrice != null)
                {
                    return Json(new {error = Messages.Activity_CustomPrices_HasRegistrationsErrorOnRemove});
                }

                db.Entry(price).State = EntityState.Deleted;

                activityRepo.Save();
            }

            return Json(new { });
        }

        [HttpPost]
        public ActionResult Edit(ActivityModel model)
        {
            if (ModelState.IsValid)
            {
                Activity activity;
                if (model.ActivityId > 0)
                {
                    activity = activityRepo.GetByIdAsNoTracking(model.ActivityId);
                    db.Entry(activity).State = EntityState.Modified;
                }
                else
                {
                    activity = new Activity
                    {
                        ActivityBranchId = model.BranchId,
                        UnionId = model.UnionId,
                        ClubId = model.ClubId,
                        SeasonId = model.SeasonId,
                        Date = DateTime.Now
                    };


                    //db.Activities.Add(activity);
                    activityRepo.Add(activity);
                }

                activity.AttachDocuments = model.AttachDocuments;
                activity.ByBenefactor = model.ByBenefactor;
                activity.Description = model.Description;
                activity.EndDate = model.EndDate;
                activity.FormPayment = model.FormPayment;
                activity.InsuranceCertificate = model.InsuranceCertificate;
                //activity.IsPublished = model.IsPublished;
                activity.MedicalCertificate = model.MedicalCertificate;
                activity.Name = model.Name;
                activity.PaymentDescription = model.PaymentDescription;
                activity.Price = model.Price;
                activity.StartDate = model.StartDate;
                activity.Type = model.Type;
                activity.DefaultLanguage = model.DefaultLanguage;

                activity.CheckCompetitionAge = model.CheckCompetitionAge;

                activity.RestrictGenders = model.RestricGenders;
                activity.RestrictGendersByTeam = model.RestricGendersByTeams;

                activity.RestrictLeaguesByAge = model.RestrictLeaguesByAge;

                activity.ClubLeagueTeamsOnly = model.ClubLeagueTeamsOnly;
                activity.PostponeParticipationPayment = model.PostponeParticipationPayment;

                activity.MultiTeamRegistrations = model.MultiTeamRegistration;

                activity.SecondTeamDiscount = model.SecondTeamRegistrationDiscountAmount;
                activity.SecondTeamNoInsurance = model.SecondTeamRegistrationNoInsurancePayment;

                activity.EnableBrotherDiscount = model.EnableBrotherDiscount;
                activity.BrotherDiscountAmount = model.BrotherDiscountAmount;
                activity.BrotherDiscountInPercent = model.BrotherDiscountInPercent;

                activity.UnionApprovedPlayerDiscount = model.UnionApprovedPlayerDiscountAmount;
                activity.UnionApprovedPlayerNoInsurance = model.UnionApprovedPlayerNoInsurancePayment;
                activity.EscortDiscount = model.EscortDiscountAmount;
                activity.EscortNoInsurance = model.EscortNoInsurancePayment;

                activity.RestrictLeagues = model.RestrictLeagues;
                activity.RestrictedLeagues = model.RestrictedLeaguesJson;

                activity.RestrictSchools = model.RestrictSchools;
                activity.RestrictedSchools = model.RestrictedSchoolsJson;

                activity.RestrictTeams = model.RestrictTeams;
                activity.RestrictedTeams = model.RestrictedTeamsJson;

                activity.RegistrationPrice = model.RegistrationPrice;
                activity.InsurancePrice = model.InsurancePrice;
                activity.ParticipationPrice = model.ParticipationPrice;
                activity.MembersFee = model.MembersFee;
                activity.HandlingFee = model.HandlingFee;
                activity.AllowNoRegistrationPayment = model.AllowNoRegistrationPayment;
                activity.AllowNoInsurancePayment = model.AllowNoInsurancePayment;
                activity.AllowNoParticipationPayment = model.AllowNoParticipationPayment;
                activity.AllowNoFeePayment = model.AllowNoFeePayment;
                activity.AllowNoHandlingFeePayment = model.AllowNoHandlingFeePayment;
                activity.NoPriceOnBuiltInRegistration = model.NoPriceOnBuiltInRegistration;
                activity.NoTeamRegistration = model.NoTeamRegistration;
                activity.AllowNewTeamRegistration = model.AllowNewTeamRegistration;
                activity.AllowEscortRegistration = model.AllowEscortRegistration;
                activity.ShowOnlyApprovedTeams = model.ShowOnlyApprovedTeams;

                activity.DisableRegPaymentForExistingClubs = model.DisableRegPaymentForExistingClubs;

                activity.RegisterToTrainingTeamsOnly = model.RegisterToTrainingTeamsOnly;
                activity.CreateClubTeam = model.CreateClubTeam;
                activity.AllowCompetitiveMembers = model.AllowCompetitiveMembers;
                activity.AllowOnlyApprovedMembers = model.AllowOnlyApprovedMembers;
                activity.DoNotAllowDuplicateRegistrations = model.DoNotAllowDuplicateRegistrations;
                activity.OnlyApprovedClubs = model.OnlyApprovedClubs;

                activity.RegistrationForMembers = (int)model.RegistrationForMembers;

                activity.RestrictCustomPricesToOneItem = model.RestrictCustomPricesToOneItem;
                activity.AllowNoCustomPricesSelected = model.AllowNoCustomPricesSelected;
                activity.CustomPricesEnabled = model.CustomPricesEnabled;

                activity.RedirectLinkOnSuccess = model.RedirectLinkOnSuccess;

                activity.MovePlayerToTeam = model.MovePlayerToTeam;

                activity.AdjustRegistrationPriceByDate = model.AdjustRegistrationPriceByDate;
                activity.AdjustParticipationPriceByDate = model.AdjustParticipationPriceByDate;
                activity.AdjustInsurancePriceByDate = model.AdjustInsurancePriceByDate;
                activity.AllowToEnterDateToAdjustPrices = model.AllowToEnterDateToAdjustPrices;

                activity.PaymentMethod = (int)model.PaymentMethod;

                activity.ForbidToChangeNameForExistingPlayers = model.ForbidToChangeNameForExistingPlayers;

                activity.RegistrationsByCompetitionsCategory = model.RegistrationsByCompetitionsCategory;

                if (model.ActivityCustomPrices?.Any() == true)
                {
                    foreach (var customPrice in model.ActivityCustomPrices
                        .Where(x => !string.IsNullOrWhiteSpace(x.TitleEng) ||
                                    !string.IsNullOrWhiteSpace(x.TitleHeb) ||
                                    !string.IsNullOrWhiteSpace(x.TitleUk)))
                    {
                        if (customPrice.Id == 0)
                        {
                            activity.ActivityCustomPrices.Add(new ActivityCustomPrice
                            {
                                TitleEng = customPrice.TitleEng,
                                TitleHeb = customPrice.TitleHeb,
                                TitleUk = customPrice.TitleUk,
                                Price = customPrice.Price,
                                MaxQuantity = customPrice.MaxQuantity,
                                DefaultQuantity = customPrice.DefaultQuantity,
                                CardComProductId = customPrice.CardComProductId
                            });

                            continue;
                        }

                        var activityPrice = activity.ActivityCustomPrices?.FirstOrDefault(x => x.Id == customPrice.Id);

                        if (activityPrice != null)
                        {
                            activityPrice.TitleEng = customPrice.TitleEng;
                            activityPrice.TitleHeb = customPrice.TitleHeb;
                            activityPrice.TitleUk = customPrice.TitleUk;
                            activityPrice.Price = customPrice.Price;
                            activityPrice.MaxQuantity = customPrice.MaxQuantity;
                            activityPrice.DefaultQuantity = customPrice.DefaultQuantity;
                            activityPrice.CardComProductId = customPrice.CardComProductId;
                        }
                    }
                }

                var activityManagerJob = (from j in db.Jobs
                                          let jr = j.JobsRole
                                          where jr.RoleName == JobRole.Activitymanager
                                          select j).FirstOrDefault();
                var activityViewerJob = (from j in db.Jobs
                                         let jr = j.JobsRole
                                         where jr.RoleName == JobRole.Activityviewer
                                         select j).FirstOrDefault();


                while (activity.ActivitiesUsers.Count > 0)
                {
                    var oldUser = activity.ActivitiesUsers.First();

                    if (oldUser.UserGroup == 0) //TODO: duplicated code?
                    {
                        var userJob =
                            db.UsersJobs
                                .Include(x => x.TravelInformations)
                                .FirstOrDefault(
                                    x => x.JobId == activityManagerJob.JobId &&
                                         x.UserId == oldUser.UserId &&
                                         x.SeasonId == model.SeasonId &&
                                         x.UnionId == model.UnionId);

                        if (userJob != null)
                        {
                            var travelInfos = userJob.TravelInformations?.ToList();

                            if (travelInfos?.Any() == true)
                            {
                                foreach (var travelInfo in travelInfos)
                                {
                                    db.TravelInformations.Remove(travelInfo);
                                }
                            }

                            db.UsersJobs.Remove(userJob);
                        }
                    }

                    if (oldUser.UserGroup == 1) //TODO: duplicated code?
                    {
                        var userJob =
                            db.UsersJobs
                                .Include(x => x.TravelInformations)
                                .FirstOrDefault(
                                    x => x.JobId == activityViewerJob.JobId &&
                                         x.UserId == oldUser.UserId &&
                                         x.SeasonId == model.SeasonId &&
                                         x.UnionId == model.UnionId);

                        if (userJob != null)
                        {
                            var travelInfos = userJob.TravelInformations?.ToList();

                            if (travelInfos?.Any() == true)
                            {
                                foreach (var travelInfo in travelInfos)
                                {
                                    db.TravelInformations.Remove(travelInfo);
                                }
                            }

                            db.UsersJobs.Remove(userJob);
                        }
                    }

                    db.Entry(oldUser).State = EntityState.Deleted;

                    activity.ActivitiesUsers.Remove(oldUser);
                }
                db.SaveChanges();

                #region ActivityManager

                var aUsers = usersRepo.GetByIds(model.ActivityManager.Select(p => p.UserId).ToArray());

                foreach (var item in aUsers)
                {
                    activity.ActivitiesUsers.Add(
                        new ActivitiesUser { UserId = item.UserId, UserGroup = 0, Activity = activity });
                }

                foreach (var user in activity.ActivitiesUsers.Where(p => p.UserGroup == 0))
                {
                    var role = (from u in db.Users
                                from j in u.UsersJobs
                                let r = j.Job
                                where u.UserId == user.UserId && j.Job.JobsRole.RoleName == JobRole.Activitymanager
                                select r).FirstOrDefault();


                    if (role == null)
                    {
                        var uJob = new UsersJob
                        {
                            JobId = activityManagerJob.JobId,
                            UserId = user.UserId,
                            SeasonId = model.SeasonId,
                            UnionId = model.UnionId
                        };

                        jobsRepo.AddUsersJob(uJob);
                    }
                }

                #endregion

                #region ActivityViewer

                aUsers = usersRepo.GetByIds(model.ActivityViewer.Select(p => p.UserId).ToArray());

                foreach (var item in aUsers)
                {
                    activity.ActivitiesUsers.Add(
                        new ActivitiesUser { UserId = item.UserId, UserGroup = 1, Activity = activity });
                }

                foreach (var user in activity.ActivitiesUsers.Where(p => p.UserGroup == 1))
                {
                    var role = (from u in db.Users
                                from j in u.UsersJobs
                                let r = j.Job
                                where u.UserId == user.UserId && j.Job.JobsRole.RoleName == JobRole.Activityviewer
                                select r).FirstOrDefault();


                    if (role == null)
                    {
                        var uJob = new UsersJob
                        {
                            JobId = activityViewerJob.JobId,
                            UserId = user.UserId,
                            SeasonId = model.SeasonId,
                            UnionId = model.UnionId
                        };

                        jobsRepo.AddUsersJob(uJob);
                    }
                }

                #endregion

                var aList = activity.ActivitiesPrices.ToList();
                for (int i = 0; i < aList.Count; i++)
                {
                    var item = aList[i];
                    db.Entry(item).State = EntityState.Deleted;
                }
                activity.ActivitiesPrices.Clear();

                if (model.FormPayment == ActivityFormPaymentType.Periods)
                {
                    foreach (var item in model.ActivitiesPrices)
                    {
                        activity.ActivitiesPrices.Add(new ActivitiesPrice
                        {
                            Activity = activity,
                            EndDate = item.EndDate,
                            Price = item.Price,
                            StartDate = item.StartDate,
                            PaymentDescription = item.PaymentDescription
                        });
                    }
                }

                activityRepo.Save();

                ViewBag.Saved = true;

                var restrictedLeagues = !string.IsNullOrWhiteSpace(activity.RestrictedLeagues)
                    ? JsonConvert.DeserializeObject<List<int>>(activity.RestrictedLeagues)
                    : new List<int>();
                model.RestrictedLeagues =
                    leagueRepo.GetCollection<League>(x => restrictedLeagues.Contains(x.LeagueId)).ToList();
                model.RestrictedLeaguesJson = activity.RestrictedLeagues;
                model.Leagues = activity.UnionId != null && activity.ClubId == null && activity.SeasonId != null
                    ? leagueRepo.GetByUnion(activity.UnionId.Value, activity.SeasonId.Value).ToList()
                    : new List<League>();
            }

            return PartialView("_Edit", model);
        }

        public ActionResult DeleteBranch(int? branchId, int? unionId, int? seasonId)
        {
            var branch = db.ActivityBranches.FirstOrDefault(p => p.AtivityBranchId == branchId);

            db.ActivityBranches.Remove(branch);

            db.SaveChanges();

            ViewBag.Result = true;

            return PartialView("_DeleteBranch");
        }

        public ActionResult DeleteActivity(int id)
        {
            var activity = db.Activities.FirstOrDefault(p => p.ActivityId == id);

            var form = activity.ActivityForms.FirstOrDefault();
            if (form != null)
            {
                foreach (var formsDetail in form.ActivityFormsDetails.ToList())
                {
                    db.Entry(formsDetail).State = EntityState.Deleted;
                }

                db.SaveChanges();
            }

            //db.Activities.Remove(activity);
            db.Entry(activity).State = EntityState.Deleted;

            db.SaveChanges();

            ViewBag.Result = true;

            return PartialView("_DeleteActivity");
        }

        [HttpPost]
        public void DeleteRegistration(int activityId, int regId)
        {
            var activity = activityRepo.GetById(activityId);

            var registration = activity?.ActivityFormsSubmittedDatas?.FirstOrDefault(x => x.Id == regId);

            if (registration != null)
            {
                db.Entry(registration).State = EntityState.Deleted;

                db.SaveChanges();
            }
        }

        public ActionResult ExportLIst(int? unionId, int? seasonId, string type)
        {
            //var activities = db.Activities
            //    .Where(p => p.UnionId == unionId && p.SeasonId == seasonId.Value && (type == "all" || (type == p.Type)))
            //    .Select(p => new ActivityViewModel
            //    {
            //        ActivityId = p.ActivityId,
            //        Date = p.Date,
            //        Description = p.Description,
            //        EndDate = p.EndDate,
            //        IsPublished = p.IsPublished,
            //        Name = p.Name,
            //        StartDate = p.StartDate,
            //        Type = p.Type,
            //        SeasonName = p.Season == null ? "" : p.Season.Name
            //    }).ToList();
            var jobRole = usersRepo.GetTopLevelJob(AdminId);
            var activityBranches = LoadActivities(unionId, seasonId, null, jobRole, jobsRepo.IsActivityManager(AdminId),
                jobsRepo.IsActivityViewer(AdminId), type);

            seasonId = seasonId ?? GetUnionCurrentSeasonFromSession();
            var season = seasonsRepository.GetById(seasonId.Value);

            MemoryStream result = new MemoryStream();

            using (XLWorkbook workBook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var ws = workBook.AddWorksheet(Messages.ImportPlayers_ErrorWorksheetName);

                #region Header

                ws.Cell(1, 1).Value = Messages.Activity_BranchName;
                //    ws.Cell(1, 2).Value = Messages.Activity_CreateDate;
                ws.Cell(1, 2).Value = Messages.Activity_Name;
                ws.Cell(1, 3).Value = Messages.Activity_Description;
                ws.Cell(1, 4).Value = Messages.Activity_Type;
                ws.Cell(1, 5).Value = Messages.Activity_StartDate;
                ws.Cell(1, 6).Value = Messages.Activity_EndDate;
                //    ws.Cell(1, 8).Value = Messages.Activity_ExternalLink;

                #endregion

                ws.Columns(1, 8).AdjustToContents();
                int rowNum = 2;
                foreach (var row in activityBranches)
                {
                    if (row.Activities.Count > 0 || jobRole == JobRole.UnionManager || User.IsInAnyRole(AppRole.Admins))
                    {
                        ws.Cell(rowNum, 1).SetValue(row.ActivityBranchName);

                        rowNum++;

                        foreach (var activity in row.Activities)
                        {
                            //     ws.Cell(rowNum, 2).SetValue(activity.Date.ToString("dd-MM-yyyy"));  
                            ws.Cell(rowNum, 2).SetValue(string.Format("{0}   {1}", activity.Name, season.Name));
                            ws.Cell(rowNum, 3).SetValue(activity.Description);
                            ws.Cell(rowNum, 4)
                                .SetValue(activity.Type == ActivityType.Group
                                    ? Messages.Activity_Type_Group
                                    : Messages.Activity_Type_Personal);
                            ws.Cell(rowNum, 5)
                                .SetValue(activity.StartDate != null
                                    ? activity.StartDate.Value.ToString("dd-MM-yyyy")
                                    : "");
                            ws.Cell(rowNum, 6)
                                .SetValue(activity.EndDate != null
                                    ? activity.EndDate.Value.ToString("dd-MM-yyyy")
                                    : "");
                            //     ws.Cell(rowNum, 8).SetValue("www");

                            rowNum++;
                        }
                    }
                }
                ws.Columns(1, 8).AdjustToContents();
                workBook.SaveAs(result);
                result.Position = 0;
            }

            string typeName = Messages.All;
            if (type == ActivityType.Group)
            {
                typeName = Messages.Activity_Type_Group;
            }
            else
            {
                typeName = Messages.Activity_Type_Personal;
            }

            CreateExportFile(result, string.Format("Activities-{0}_{1}.xlsx", typeName, season.Name));

            ViewBag.ExportResult = true;

            return PartialView("_ExportList");
        }

        [HttpGet]
        public ActionResult DownloadExportFile()
        {
            var fileByteObj = Session[ExportActivitiesResultSessionKey];
            if (fileByteObj == null)
            {
                throw new System.IO.FileNotFoundException();
            }

            var fileBytes = (byte[])fileByteObj;
            var fileName = Session[ExportActivitiesResultFileNameSessionKey] as string;

            return File(fileBytes, "application/octet-stream", fileName);
        }

        [HttpGet]
        public ActionResult GetActivityForm(int id)
        {
            var activity = activityRepo.GetByIdAsNoTracking(id);

#if DEBUG
            Thread.Sleep(1000);
#endif
            ViewBag.IsUkrainianUnion = activity?.UnionId == GlobVars.UkraineGymnasticUnionId ||
                                       activity?.Club?.UnionId == GlobVars.UkraineGymnasticUnionId;

            var form = activity.ActivityForms.FirstOrDefault();

            if (form == null || !form.ActivityFormsDetails.Any())
                return Json(new ActivityFormEditModel //no saved form, create from defaults
                {
                    FormName = activity.Name,
                    FormDescription = activity.Description,
                    EndDate = activity.EndDate,
                    FormContent = CreateDefaultActivityForm(activity)
                }, JsonRequestBehavior.AllowGet);

            return Json(new ActivityFormEditModel
            {
                FormName = form.Name,
                FormDescription = form.Description,
                EndDate = activity.EndDate,
                FormContent = BuildForm(activity, form.ActivityFormsDetails, false, getCulture())
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ResetForm(int id)
        {
            var activity = activityRepo.GetById(id);

            var form = activity.ActivityForms.FirstOrDefault();

            if (form != null)
            {
                db.ActivityFormsDetails.RemoveRange(form.ActivityFormsDetails);

                activityRepo.Save();
            }

            return null;
        }

        [HttpPost]
        [AllowAnonymous]
        [OutputCache(Duration = 0, NoStore = true)]
        public ActionResult Form(int id, ActivityFormPublishSubmitModel model)
        {
            var activity = activityRepo.GetById(id);
            var user = usersRepo.GetByIdentityNumber(model.PlayerId);
            var league = leagueRepo.GetById(model.LeagueId);

            var team = teamRepo.GetById(model.TeamId);

            var multiTeamTeams = model.PlayerTeamMultiple?.Any() == true
                ? teamRepo.GetCollection<Team>(x => model.PlayerTeamMultiple.Contains(x.TeamId)).ToList()
                : new List<Team>();
            if (model.PlayerTeamMultiple?.Any() == true && model.PlayerTeamMultiple.Count() != multiTeamTeams.Count)
            {
                return Content(string.Format(Messages.Activity_MultiTeam_TeamsNotFound,
                    string.Join(", ", model.PlayerTeamMultiple.Where(x => !multiTeamTeams.Any(t => t.TeamId == x)))));
            }

            var club = clubsRepo.GetById(model.ClubId ?? 0);

            var activityClub = clubsRepo.GetById(activity?.ClubId ?? 0);

            var activityFormType = activity.GetFormType();

            if ((activityFormType != ActivityFormType.CustomPersonal && !activity.NoTeamRegistration) &&
                (activityFormType != ActivityFormType.ClubCustomPersonal && !activity.NoTeamRegistration) &&
                (activityFormType != ActivityFormType.DepartmentClubCustomPersonal && !activity.NoTeamRegistration) &&
                activityFormType != ActivityFormType.UnionClub &&
                activityFormType != ActivityFormType.UnionPlayerToClub &&
                league == null && activityClub == null || activity == null || !activity.IsPublished ||
                !activity.ActivityForms.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.NoContent);
            }

            if (user == null && 
                activityFormType != ActivityFormType.ClubPlayerRegistration &&
                activityFormType != ActivityFormType.DepartmentClubPlayerRegistration &&
                activityFormType != ActivityFormType.ClubCustomPersonal &&
                activityFormType != ActivityFormType.DepartmentClubCustomPersonal &&
                activityFormType != ActivityFormType.CustomPersonal &&
                activityFormType != ActivityFormType.CustomGroup &&
                activityFormType != ActivityFormType.UnionClub &&
                activityFormType != ActivityFormType.UnionPlayerToClub)
            {
                return PartialView("ActivityFormPublishedSubmitResult", Messages.Activity_BuildForm_PlayerNotFound);
            }

            switch (activityFormType)
            {
                case ActivityFormType.TeamRegistration:
                    if (activity.ActivityFormsSubmittedDatas.Any(
                        x => x.TeamId == model.TeamId &&
                             x.LeagueId == league.LeagueId &&
                             x.Activity.Type == ActivityType.Group &&
                             x.Activity.IsAutomatic.HasValue && x.Activity.IsAutomatic.Value))
                    {
                        return Content(Messages.Activity_TeamAlreadyRegistered);
                    }
                    break;

                case ActivityFormType.PlayerRegistration:
                    if (activity.ActivityFormsSubmittedDatas.Any(
                        x => x.PlayerId == user.UserId &&
                             x.TeamId == model.TeamId &&
                             x.LeagueId == model.LeagueId &&
                             x.Activity.Type == ActivityType.Personal &&
                             x.Activity.IsAutomatic.HasValue && x.Activity.IsAutomatic.Value))
                    {
                        return Content(Messages.Activity_TeamAlreadyRegistered);
                    }
                    break;

                    //case ActivityFormType.CustomGroup:
                    //    throw new NotImplementedException();
                    //case ActivityFormType.CustomPersonal:
                    //    throw new NotImplementedException();

                    //case ActivityFormType.ClubCustomGroup:
                    //    throw new NotImplementedException();
                    //case ActivityFormType.ClubCustomPersonal:
                    //    throw new NotImplementedException();
            }

            if (team == null &&
                !(activityFormType == ActivityFormType.CustomPersonal || activity.NoTeamRegistration) &&
                !(activityFormType == ActivityFormType.ClubCustomPersonal || activity.NoTeamRegistration) &&
                !(activityFormType == ActivityFormType.ClubPlayerRegistration && activity.MultiTeamRegistrations) &&
                !(activityFormType == ActivityFormType.DepartmentClubCustomPersonal || activity.NoTeamRegistration) &&
                activityFormType != ActivityFormType.CustomGroup &&
                activityFormType != ActivityFormType.UnionClub &&
                !(activityFormType == ActivityFormType.UnionPlayerToClub || activity.MultiTeamRegistrations))
            {
                return Content(Messages.TeamNotFound);
            }

            var customFields = !string.IsNullOrWhiteSpace(model.CustomFields)
                ? JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(model.CustomFields)
                : new List<ActivityFormCustomField>();

            #region Process custom prices
            var customPrices = new List<ActivityCustomPriceModel>();

            if (activity.CustomPricesEnabled)
            {
                var formDetails = activity.ActivityForms.First().ActivityFormsDetails.ToList();

                foreach (var customPriceField in customFields.Where(x => x.Type == ActivityFormControlType.CustomPrice))
                {
                    var quantity = 1;
                    int.TryParse(customPriceField.Value, out quantity);

                    var activityCustomPrice = formDetails
                        .FirstOrDefault(x => x.PropertyName == customPriceField.PropertyName)
                        ?.ActivityCustomPrice;

                    customPrices.Add(new ActivityCustomPriceModel
                    {
                        PropertyName = customPriceField.PropertyName,
                        TitleEng = activityCustomPrice?.TitleEng ?? "custom price",
                        TitleHeb = activityCustomPrice?.TitleHeb ?? "custom price",
                        TitleUk = activityCustomPrice?.TitleUk ?? "custom price",
                        Quantity = quantity,
                        Price = activityCustomPrice?.Price ?? 0,
                        TotalPrice = (activityCustomPrice?.Price ?? 0) * quantity,
                        CardComProductId = activityCustomPrice.CardComProductId
                    });
                }
            }
            #endregion

            #region Process custom files

            foreach (var customFile in customFields.Where(x => x.Type == ActivityFormControlType.CustomFileUpload))
            {
                customFile.Value = SaveFormFile(id, customFile.PropertyName);
            }

            #endregion

            model.CustomFields = JsonConvert.SerializeObject(customFields);

            if (user?.IsActive == false)
            {
                user.IsActive = true;
            }

            if (!string.IsNullOrWhiteSpace(model.PlayerFullName?.Trim()))
            {
                model.PlayerFirstName = playersRepo.GetFirstNameByFullName(model.PlayerFullName);
                model.PlayerLastName = playersRepo.GetLastNameByFullName(model.PlayerFullName);

                model.PlayerFullName = null;
            }

            var registerToTeams = activity.MultiTeamRegistrations
                ? multiTeamTeams
                : new List<Team> {team};

            switch (activityFormType)
            {
                case ActivityFormType.PlayerRegistration:
                    return SavePlayerForm(model, activity, league, team, user, customPrices);

                case ActivityFormType.TeamRegistration:
                    return SaveTeamForm(model, activity, league, team, user, customPrices);

                case ActivityFormType.ClubPlayerRegistration:
                    return SaveClubPlayerForm(model, activity, activityClub, registerToTeams, user, customPrices);

                case ActivityFormType.ClubCustomPersonal:
                    return SaveCustomClubPlayerForm(model, activity, activityClub, registerToTeams, user, customPrices);

                case ActivityFormType.DepartmentClubPlayerRegistration:
                    return SaveDepartmentClubPlayerForm(model, activity, activityClub, team, user, customPrices);

                case ActivityFormType.DepartmentClubCustomPersonal:
                    return SaveCustomDepartmentClubPlayerForm(model, activity, activityClub, team, user, customPrices);

                case ActivityFormType.CustomPersonal:
                    if (activity.RegistrationsByCompetitionsCategory)
                    {
                        return SaveUnionCustomPersonalFormByCompetitionCategories(model, activity, user, customPrices);
                    }
                    else
                    {
                        return SaveUnionCustomPersonalForm(model, activity, league, team, user, customPrices);
                    }

                case ActivityFormType.CustomGroup:
                    return SaveUnionCustomTeamForm(model, activity, league, team, user, customPrices);

                case ActivityFormType.UnionClub:
                    return SaveUnionClubForm(model, activity, user, club, customPrices);

                case ActivityFormType.UnionPlayerToClub:
                    return SaveUnionPlayerToClubForm(model, activity, user, club, registerToTeams, customPrices);

                default:
                    throw new Exception($"Unknown form type for activity {activity.ActivityId}");
            }
        }

        private ActionResult SaveUnionPlayerToClubForm(ActivityFormPublishSubmitModel model, Activity activity,
            User user, Club club, List<Team> registerToTeams, List<ActivityCustomPriceModel> customPrices)
        {
            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }
            var seasonId = activity.SeasonId.Value;
            var season = seasonsRepository.GetById(seasonId);
            if (user == null)
            {
                //create user
                user = new User
                {
                    Password = Protector.Encrypt("123abc12"),
                    TypeId = 4, //player
                    IdentNum = model.PlayerId,

                    //FullName = model.PlayerFullName,
                    FirstName = model.PlayerFirstName,
                    LastName = model.PlayerLastName,
                    MiddleName = model.PlayerMiddleName,

                    #region Ukraine gymnastic union specific fields

                    MotherName = model.PlayerMotherName,
                    ParentPhone = model.PlayerParentPhone,
                    IdentCard = model.PlayerIdentCard,
                    LicenseValidity = model.PlayerLicenseDate,
                    ParentEmail = model.PlayerParentEmail,

                    #endregion

                    GenderId = model.PlayerGender,
                    ParentName = model.PlayerParentName,
                    Email = model.PlayerEmail,
                    Telephone = model.PlayerPhone,
                    City = model.PlayerCity,
                    Address = model.PlayerAddress,
                    BirthDay = model.PlayerBirthDate,
                    IsActive = true,
                    IsCompetitiveMember = model.IsCompetitiveMember,
                    SeasonIdOfCreation = seasonId,

                    PostalCode = model.PlayerPostalCode,
                    PassportNum = model.PlayerPassportNumber,
                    ForeignFirstName = model.PlayerForeignFirstName,
                    ForeignLastName = model.PlayerForeignLastName
                };
                usersRepo.Create(user);

                user = usersRepo.GetById(user.UserId);
            }
            else {
               
                if (season != null && season.Union.Section.Alias == GamesAlias.Athletics) {
                    var oldSeasonIdOfCreation = user.SeasonIdOfCreation;
                    if (oldSeasonIdOfCreation.HasValue)
                    {
                        var oldSeason = seasonsRepository.GetById(oldSeasonIdOfCreation.Value);
                        if (oldSeason.Union.Section.Alias != GamesAlias.Athletics)
                        {
                            user.SeasonIdOfCreation = seasonId;
                            usersRepo.Save();
                        }
                    }
                    else {
                        user.SeasonIdOfCreation = seasonId;
                        usersRepo.Save();
                    }
                }
            }

            var teamsPlayer = user.TeamsPlayers.Where(x => x.SeasonId == activity.SeasonId && x.Club?.UnionId == activity.UnionId);
            int? prevClubId = null;
            if (teamsPlayer.Any())
            {
                //save previous club only for climbing section
                if(season.Union.Section.Alias == GamesAlias.Climbing) prevClubId = teamsPlayer.FirstOrDefault().ClubId;
                db.TeamsPlayers.RemoveRange(teamsPlayer);

                db.SaveChanges();
            }

            var playerPricesHelper = new PlayerPriceHelper();
            var prices = playerPricesHelper.GetUnionClubPlayerPrice(activity.UnionId ?? 0, seasonId, user.BirthDay);
            if (!activity.RegistrationPrice)
            {
                prices.RegularRegistrationPrice = 0;
                prices.CompetingRegistrationPrice = 0;
            }

            prices.InsurancePrice = activity.InsurancePrice
                ? model.IsSchoolInsurance ? 0 : prices.InsurancePrice
                : 0;
            prices.TenicardPrice = model.DoNotPayTenicardPrice ? 0 : prices.TenicardPrice;

            var registrations = new List<ActivityFormsSubmittedData>();

            foreach (var team in registerToTeams)
            {
                var teamLeagues = team.LeagueTeams.Where(x => x.SeasonId == seasonId).ToList();

                if (teamLeagues.Any())
                {
                    foreach (var teamLeague in teamLeagues)
                    {
                        user.TeamsPlayers.Add(new TeamsPlayer
                        {
                            SeasonId = activity.SeasonId,
                            LeagueId = teamLeague.LeagueId,
                            IsActive = true,
                            TeamId = team.TeamId,
                            UserId = user.UserId,
                            ShirtNum = team.TeamsPlayers.Any(x => x.LeagueId == teamLeague.LeagueId)
                                ? team.TeamsPlayers.Where(x => x.LeagueId == teamLeague.LeagueId).Max(x => x.ShirtNum) + 1
                                : 1
                        });
                    }
                }
                else
                {
                    user.TeamsPlayers.Add(new TeamsPlayer
                    {
                        SeasonId = activity.SeasonId,
                        ClubId = club.ClubId,
                        IsActive = true,
                        TeamId = team.TeamId,
                        UserId = user.UserId,
                        ShirtNum = team.TeamsPlayers.Any(x => x.ClubId == club.ClubId)
                            ? team.TeamsPlayers.Where(x => x.ClubId == club.ClubId).Max(x => x.ShirtNum) + 1
                            : 1
                    });
                }

                usersRepo.Save();

                var newDataEntry = new ActivityFormsSubmittedData
                {
                    ActivityId = activity.ActivityId,
                    PlayerId = user.UserId,
                    Comments = model.Comments,
                    DateSubmitted = DateTime.Now,
                    ClubId = club.ClubId,
                    LeagueId = teamLeagues?.FirstOrDefault(x => x.SeasonId == seasonId)?.LeagueId,
                    TeamId = team?.TeamId,
                    Document = SaveFormFile(activity.ActivityId, "document"),
                    PaymentByBenefactor = model.PaymentByBenefactor,
                    IsPaymentByBenefactor = model.IsPaymentByBenefactor,

                    CustomFields = model.CustomFields,

                    CustomPrices = team == registerToTeams.First()
                        ? customPrices.Any() ? JsonConvert.SerializeObject(customPrices) : null
                        : null,

                    RegistrationPrice = model.IsCompetitiveMember
                        ? prices.CompetingRegistrationPrice
                        : prices.RegularRegistrationPrice,
                    InsurancePrice = prices.InsurancePrice,
                    TenicardPrice = prices.TenicardPrice,

                    RegistrationCardComProductId = activity.RegistrationPrice
                        ? model.IsCompetitiveMember ? prices.CompetingRegistrationCardComProductId :
                        prices.RegularRegistrationCardComProductId
                        : null,
                    InsuranceCardComProductId = activity.InsurancePrice ? prices.InsuranceCardComProductId : null,
                    TenicardCardComProductId = prices.TenicardCardComProductId,

                    IsSchoolInsurance = model.IsSchoolInsurance,
                    DoNotPayTenicardPrice = model.DoNotPayTenicardPrice,
                    PreviousClubId = activity.Union?.Section?.Alias == SectionAliases.Climbing ? prevClubId : null,
                };

                registrations.Add(newDataEntry);
            }

            #region UpdateUser

            SaveFileToPlayer("playerProfilePicture", user, activity.SeasonId ?? 0, PlayerFileType.PlayerImage);
            SaveFileToPlayer("medicalCert", user, activity.SeasonId ?? 0, PlayerFileType.MedicalCertificate);
            SaveFileToPlayer("insuranceCert", user, activity.SeasonId ?? 0, PlayerFileType.Insurance);
            SaveFileToPlayer("idFile", user, activity.SeasonId ?? 0, PlayerFileType.IDFile);

            if (!string.IsNullOrWhiteSpace(model.PlayerCity))
            {
                user.City = model.PlayerCity;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerAddress))
            {
                user.Address = model.PlayerAddress;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerEmail))
            {
                user.Email = model.PlayerEmail;
            }

            //if (!string.IsNullOrWhiteSpace(model.PlayerFullName))
            //{
            //    user.FullName = model.PlayerFullName;
            //}
            if (!string.IsNullOrWhiteSpace(model.PlayerFirstName))
            {
                user.FirstName = model.PlayerFirstName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerLastName))
            {
                user.LastName = model.PlayerLastName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerMiddleName))
            {
                user.MiddleName = model.PlayerMiddleName;
            }

            if (model.PlayerGender.HasValue)
            {
                user.GenderId = model.PlayerGender.Value;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerPhone))
            {
                user.Telephone = model.PlayerPhone;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerParentName))
            {
                user.ParentName = model.PlayerParentName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerMotherName))
            {
                user.MotherName = model.PlayerMotherName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerParentPhone))
            {
                user.ParentPhone = model.PlayerParentPhone;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerIdentCard))
            {
                user.IdentCard = model.PlayerIdentCard;
            }
            if (model.PlayerLicenseDate > DateTime.MinValue)
            {
                user.LicenseValidity = model.PlayerLicenseDate;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerParentEmail))
            {
                user.ParentEmail = model.PlayerParentEmail;
            }

            if (!string.IsNullOrWhiteSpace(model.PlayerPostalCode))
            {
                user.PostalCode = model.PlayerPostalCode;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerPassportNumber))
            {
                user.PassportNum = model.PlayerPassportNumber;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerForeignFirstName))
            {
                user.ForeignFirstName = model.PlayerForeignFirstName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerForeignLastName))
            {
                user.ForeignLastName = model.PlayerForeignLastName;
            }

            user.IsCompetitiveMember = model.IsCompetitiveMember;
            usersRepo.Save();

            #endregion

            var priceTotal = registrations.Sum(x => x.RegistrationPrice) +
                             registrations.Sum(x => x.InsurancePrice) +
                             registrations.Sum(x => x.TenicardPrice) +
                             customPrices.Sum(x => x.TotalPrice);

            var redirectToPaymentUrl = string.Empty;

            #region CardCom payment for catchball union

            if (!model.IsPaymentByBenefactor && activity.UnionId == 15 &&
                priceTotal > 0)
            {
                var result = CardComHelper.UnionPlayerToClub_CatchBall(activity, user, registerToTeams, customPrices, priceTotal,
                    prices, model.IsCompetitiveMember);

                registrations.ForEach(x => x.CardComLpc = result?.Lpc);
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for Rugby union

            if (!model.IsPaymentByBenefactor && activity.UnionId == 54 &&
                priceTotal > 0)
            {
                var result = CardComHelper.UnionPlayerToClub_Rugby(activity, user, registerToTeams, customPrices, priceTotal,
                    prices, model.IsCompetitiveMember);

                registrations.ForEach(x => x.CardComLpc = result?.Lpc);
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for wave surfing union

            if (activity.Union?.UnionId == 41 && priceTotal > 0)
            {
                var result = CardComHelper.UnionPlayerToClub_WaveSurfing(activity, user, registerToTeams, customPrices, priceTotal,
                    prices, model.IsCompetitiveMember);

                registrations.ForEach(x => x.CardComLpc = result?.Lpc);
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for tennis union

            if (activity.UnionId == 38 && priceTotal > 0)
            {
                var result = CardComHelper.UnionPlayerToClub_Tennis(activity, user, registerToTeams, customPrices, priceTotal,
                    prices, model.IsCompetitiveMember);

                registrations.ForEach(x => x.CardComLpc = result?.Lpc);
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region LiqPay payment for UA gymnastic federation union

            if (activity.UnionId == 52 && priceTotal > 0)
            {
                var result = LiqPayHelper.UnionPlayerToClub_UaGymnastic(activity, user, registerToTeams, customPrices, priceTotal,
                    prices, model.IsCompetitiveMember);

                registrations.ForEach(x => x.LiqPayOrderId = result?.OrderId);
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for waterpolo union

            if (activity.UnionId == 32 && priceTotal > 0)
            {
                var result = CardComHelper.UnionPlayerToClub_Waterpolo(activity, user, registerToTeams, customPrices, priceTotal,
                    prices, model.IsCompetitiveMember);

                registrations.ForEach(x => x.CardComLpc = result?.Lpc);
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for Climbing union 59

            if (!model.IsPaymentByBenefactor && activity.UnionId == 59 &&
                priceTotal > 0)
            {
                var result = CardComHelper.UnionPlayerToClub_Climbing59(activity, user, registerToTeams, customPrices, priceTotal,
                    prices, model.IsCompetitiveMember);

                registrations.ForEach(x => x.CardComLpc = result?.Lpc);
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            registrations.ForEach(x => activity.ActivityFormsSubmittedDatas.Add(x));

            activityRepo.Save();

            if (!string.IsNullOrWhiteSpace(redirectToPaymentUrl))
                return Redirect(redirectToPaymentUrl);

            var paymentIdentifier = registrations.FirstOrDefault(x => x.CardComLpc != null)?.CardComLpc 
                                    ?? registrations.FirstOrDefault(x => x.LiqPayOrderId != null)?.LiqPayOrderId;

            return PartialView("ActivityFormPublishedSubmitResult", new ActivityFormSuccessResultModel { Activity = activity, PaymentIdentifier = paymentIdentifier });
        }

        private ActionResult SaveUnionCustomPersonalFormByCompetitionCategories(ActivityFormPublishSubmitModel model, Activity activity,
            User user, List<ActivityCustomPriceModel> customPrices)
        {
            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }
            var seasonId = activity.SeasonId.Value;

            if (user == null)
            {
                //create user
                user = new User
                {
                    Password = Protector.Encrypt("123abc12"),
                    TypeId = 4, //player
                    IdentNum = model.PlayerId,

                    //FullName = model.PlayerFullName,
                    FirstName = model.PlayerFirstName,
                    LastName = model.PlayerLastName,
                    MiddleName = model.PlayerMiddleName,

                    ParentName = model.PlayerParentName,
                    Email = model.PlayerEmail,
                    Telephone = model.PlayerPhone,
                    Address = model.PlayerAddress,
                    City = model.PlayerCity,
                    BirthDay = model.PlayerBirthDate,
                    IsActive = true
                };
                usersRepo.Create(user);

                user = usersRepo.GetById(user.UserId);
            }

            var playerPricesHelper = new PlayerPriceHelper();

            var playerRegistrationsInOtherActivities = user.ActivityFormsSubmittedDatas.Where(
                x => //x.Activity.IsAutomatic == true &&
                    x.Activity.Type == ActivityType.Personal &&
                    x.Activity.SeasonId == activity.SeasonId &&
                    x.Activity.UnionId == activity.UnionId &&
                    x.Activity.ClubId == activity.ClubId &&
                    x.LeagueId != null);

            var isPlayerAlreadyRegisteredInBuiltInActivity = playerRegistrationsInOtherActivities.Any(x => x.Activity.IsAutomatic == true);

            var playerRegistrationsInActivity =
                activity.ActivityFormsSubmittedDatas
                    .Where(x => x.PlayerId == user.UserId)
                    .ToList();

            var isPlayerSecondRegistration =
                playerRegistrationsInActivity.Count() == 1;

            var alreadyRegisteredToCompetitions = playerRegistrationsInActivity
                .Where(x => x.CompetitionDisciplineId > 0)
                .Select(x => x.CompetitionDisciplineId)
                .ToArray();

            var paidRegistrationsToBeRemoved = new List<ActivityFormsSubmittedData>();
            if (model.RemovedRegistrations?.Any() == true)
            {
                paidRegistrationsToBeRemoved = playerRegistrationsInActivity
                    .Where(x => model.RemovedRegistrations.Contains(x.Id) &&
                                (x.RegistrationPaid > 0 ||
                                 x.HandlingFeePaid > 0 ||
                                 x.InsurancePaid > 0 ||
                                 x.MembersFeePaid > 0))
                    .ToList();
            }

            model.PlayerCompetitionCategory = //exclude categories to which the player is already registered
                model.PlayerCompetitionCategory
                    .Where(x => !alreadyRegisteredToCompetitions.Contains(x))
                    .ToArray();

            var competitionDisciplines = db.CompetitionDisciplines
                .Where(x => model.PlayerCompetitionCategory.Contains(x.Id))
                .ToList();

            var registrations = new List<ActivityFormsSubmittedData>();

            var competitionsPrices = new List<LeaguePlayerPrice>();

            foreach (var competitionDiscipline in competitionDisciplines)
            {
                var removedPaidRegistration = paidRegistrationsToBeRemoved.FirstOrDefault();

                var prices = playerPricesHelper.GetCompetitionDisciplinePrices(competitionDiscipline.Id);

                if (activity.NoPriceOnBuiltInRegistration && isPlayerAlreadyRegisteredInBuiltInActivity)
                {
                    prices.RegistrationPrice = 0;
                    prices.InsurancePrice = 0;
                    prices.MembersFee = 0;
                    prices.HandlingFee = 0;
                }

                prices.RegistrationPrice = activity.RegistrationPrice ? (model.DisableRegistrationPayment ? 0 : prices.RegistrationPrice) : 0;
                prices.InsurancePrice =
                    activity.InsurancePrice ? (model.DisableInsurancePayment ? 0 : prices.InsurancePrice) : 0;
                prices.MembersFee = activity.MembersFee ? (model.DisableMembersFeePayment ? 0 : prices.MembersFee) : 0;
                prices.HandlingFee = activity.HandlingFee ? (model.DisableHandlingFeePayment ? 0 : prices.HandlingFee) : 0;

                if (isPlayerSecondRegistration)
                {
                    prices.InsurancePrice =
                        activity.SecondTeamNoInsurance
                            ? 0
                            : prices.InsurancePrice;

                    prices.RegistrationPrice =
                        Math.Max(0, prices.RegistrationPrice - (activity.SecondTeamDiscount ?? 0));
                }

                if (activity.AllowEscortRegistration && model.PlayerIsEscort)
                {
                    //discount for escort player
                    foreach (var customPrice in customPrices)
                    {
                        customPrice.Price = Math.Max(0, customPrice.Price - (activity.EscortDiscount ?? 0m));

                        customPrice.TotalPrice = customPrice.Price * customPrice.Quantity;
                    }

                    if (activity.EscortNoInsurance)
                    {
                        prices.InsurancePrice = 0m;
                    }
                }
                else if (playerRegistrationsInOtherActivities.Any(x => x.IsActive))
                {
                    //discount for approved player
                    foreach (var customPrice in customPrices)
                    {
                        customPrice.Price = Math.Max(0, customPrice.Price - (activity.UnionApprovedPlayerDiscount ?? 0m));

                        customPrice.TotalPrice = customPrice.Price * customPrice.Quantity;
                    }

                    if (activity.UnionApprovedPlayerNoInsurance)
                    {
                        prices.InsurancePrice = 0m;
                    }
                }

                var newDataEntry = new ActivityFormsSubmittedData
                {
                    ActivityId = activity.ActivityId,
                    PlayerId = user.UserId,
                    CompetitionDisciplineId = competitionDiscipline.Id,
                    Comments = model.Comments,
                    DateSubmitted = DateTime.Now,
                    //NameForInvoice = model.NameForInvoice,
                    PaymentByBenefactor = model.PaymentByBenefactor,
                    IsPaymentByBenefactor = model.IsPaymentByBenefactor,
                    Document = SaveFormFile(activity.ActivityId, "document"),
                    //MedicalCert = SaveFormFile(activity.ActivityId, "medicalCert"),
                    //InsuranceCert = SaveFormFile(activity.ActivityId, "insuranceCert"),
                    CustomFields = model.CustomFields,
                    CustomPrices = customPrices.Any() ? JsonConvert.SerializeObject(customPrices) : null,

                    RegistrationPrice = prices.RegistrationPrice,
                    InsurancePrice = prices.InsurancePrice,
                    MembersFee = prices.MembersFee,
                    HandlingFee = prices.HandlingFee,

                    RegistrationCardComProductId = prices.RegistrationPriceCardComProductId,
                    InsuranceCardComProductId = prices.InsuranceCardComProductId,
                    MembersFeeCardComProductId = prices.MembersFeeCardComProductId,
                    HandlingFeeCardComProductId = prices.HandlingFeeCardComProductId,

                    DisableRegistrationPayment = model.DisableRegistrationPayment,
                    DisableInsurancePayment = model.DisableInsurancePayment,
                    DisableMembersFeePayment = model.DisableMembersFeePayment,
                    DisableHandlingFeePayment = model.DisableHandlingFeePayment,

                    //transfer paid status
                    RegistrationPaid = removedPaidRegistration == null
                                       ? 0
                                       : removedPaidRegistration.RegistrationPaid,
                    InsurancePaid = removedPaidRegistration == null
                                       ? 0
                                       : removedPaidRegistration.InsurancePaid,
                    HandlingFeePaid = removedPaidRegistration == null
                                       ? 0
                                       : removedPaidRegistration.HandlingFeePaid,
                    MembersFeePaid = removedPaidRegistration == null
                                       ? 0
                                       : removedPaidRegistration.MembersFeePaid
                };

                registrations.Add(newDataEntry);

                if (removedPaidRegistration == null)
                {
                    //The registration is not replacing already paid registration which is about to be removed
                    competitionsPrices.Add(prices);
                }
                else
                {
                    //The registration has replaced already paid registration which is about to be removed
                    paidRegistrationsToBeRemoved.Remove(removedPaidRegistration);
                }
            }
            
            var priceTotal = competitionsPrices.Sum(x => x.RegistrationPrice) +
                             competitionsPrices.Sum(x => x.InsurancePrice) +
                             competitionsPrices.Sum(x => x.MembersFee) +
                             competitionsPrices.Sum(x => x.HandlingFee) +
                             customPrices.Sum(x => x.TotalPrice);

            var redirectToPaymentUrl = string.Empty;

            #region CardCom payment for Climbing union 59

            if (!model.IsPaymentByBenefactor && activity.UnionId == 59 && priceTotal > 0)
            {
                var result = CardComHelper.UnionCustomPersonal_Climbing59CompetitionCategory(activity, user,
                    competitionDisciplines,
                    competitionsPrices,
                    customPrices,
                    priceTotal);

                registrations.ForEach(x => x.CardComLpc = result?.Lpc);
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            registrations.ForEach(x => activity.ActivityFormsSubmittedDatas.Add(x));

            activityRepo.Save();

            var player = playersRepo.GetTeamPlayerByUserIdAndSeasonId(user.UserId, activity.SeasonId ?? 0);
            if (player != null)
            {
                foreach (var competitionDiscipline in competitionDisciplines)
                {
                    disciplinesRepo.RegisterAthleteUnderCompetitionDiscipline(competitionDiscipline.Id, player.ClubId,
                        user.UserId);
                }
            }

            #region Payment by benefactor email

            //            if (model.IsPaymentByBenefactor)
            //            {
            //                var emailService = new EmailService();

            //                var hostBaseUrl = ConfigurationManager.AppSettings["SiteUrl"];
            //                var activitiesUrl = string.Format("{0}/Unions/Edit/{1}#activities",
            //                    hostBaseUrl,
            //                    activity.UnionId);

            //                var teamName = team?.TeamsDetails.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ??
            //                               team?.Title ??
            //                               Messages.Activity_NoTeamPlaceholder;
            //                var leagueName = league?.Name ?? string.Empty;

            //                string emailBody = $@"
            //{string.Format(Messages.Activity_BuildForm_Email_Header, teamName, leagueName)}
            //<br />
            //{Messages.Activity_BuildForm_Email_BenefactorName}: {model.PaymentByBenefactor}<br />
            //{Messages.Activity_BuildForm_Email_Price}: {prices.RegistrationPrice}<br />
            //{Messages.Insurance}: {prices.InsurancePrice}<br />
            //{Messages.LeagueDetail_MemberFees}: {prices.MembersFee}<br />
            //<br />
            //{
            //                    string.Join("<br />", customPrices.Select(x => $"{CustomPriceHelper.GetPriceTitle(getCulture(), x)}: {x.TotalPrice}"))
            //}
            //<br /><br />
            //<a href=""{activitiesUrl}"">{Messages.Activity_BuildForm_Email_OpenActivities}</a>
            //";

            //                var managerToSendEmail = usersRepo.GetUnionWorkers(activity.UnionId ?? 0, JobRole.UnionManager, activity.SeasonId)
            //                    .ToList();
            //                managerToSendEmail.AddRange(usersRepo.GetUnionWorkers(activity.UnionId ?? 0, JobRole.Activitymanager, activity.SeasonId));
            //                foreach (var manager in managerToSendEmail)
            //                {
            //                    string email = manager.Email;

            //#if DEBUG
            //                    email = "info@loglig.com";
            //#endif

            //                    if (!string.IsNullOrWhiteSpace(email))
            //                    {
            //                        try
            //                        {
            //                            emailService.SendAsync(email, emailBody);
            //                        }
            //                        catch (Exception)
            //                        {
            //                            // ¯\_(ツ)_/¯
            //                        }
            //                    }
            //                }
            //            }

            #endregion

            #region UpdateUser

            SaveFileToPlayer("playerProfilePicture", user, activity.SeasonId ?? 0, PlayerFileType.PlayerImage);
            SaveFileToPlayer("medicalCert", user, activity.SeasonId ?? 0, PlayerFileType.MedicalCertificate);
            SaveFileToPlayer("insuranceCert", user, activity.SeasonId ?? 0, PlayerFileType.Insurance);
            SaveFileToPlayer("idFile", user, activity.SeasonId ?? 0, PlayerFileType.IDFile);

            if (!string.IsNullOrWhiteSpace(model.PlayerCity))
            {
                user.City = model.PlayerCity;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerAddress))
            {
                user.Address = model.PlayerAddress;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerEmail))
            {
                user.Email = model.PlayerEmail;
            }

            //if (!string.IsNullOrWhiteSpace(model.PlayerFullName))
            //{
            //    user.FullName = model.PlayerFullName;
            //}
            if (!string.IsNullOrWhiteSpace(model.PlayerFirstName))
            {
                user.FirstName = model.PlayerFirstName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerLastName))
            {
                user.LastName = model.PlayerLastName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerMiddleName))
            {
                user.MiddleName = model.PlayerMiddleName;
            }

            if (model.PlayerGender.HasValue)
            {
                user.GenderId = model.PlayerGender.Value;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerPhone))
            {
                user.Telephone = model.PlayerPhone;
            }
            //if (model.PlayerBirthDate.HasValue)
            //{
            //    user.BirthDay = model.PlayerBirthDate.Value;
            //}
            usersRepo.Save();

            #endregion

            if (model.RemovedRegistrations?.Any() == true)
            {
                var registrationsToRemove = db.ActivityFormsSubmittedDatas
                    .Where(x => x.ActivityId == activity.ActivityId &&
                                model.RemovedRegistrations.Contains(x.Id))
                    .ToList();

                var competitionsDisciplinesIds = registrationsToRemove
                    .Select(x => x.CompetitionDisciplineId)
                    .ToArray();

                var competitionDisciplineRegistrationsToRemove = db.CompetitionDisciplineRegistrations
                    .Where(x => competitionsDisciplinesIds.Contains(x.CompetitionDisciplineId) &&
                                x.UserId == user.UserId)
                    .ToList();

                if (registrationsToRemove.Any())
                {
                    db.ActivityFormsSubmittedDatas.RemoveRange(registrationsToRemove);
                    db.SaveChanges();
                }

                if (competitionDisciplineRegistrationsToRemove.Any())
                {
                    db.CompetitionDisciplineRegistrations.RemoveRange(competitionDisciplineRegistrationsToRemove);
                    db.SaveChanges();
                }
            }

            if (!string.IsNullOrWhiteSpace(redirectToPaymentUrl))
                return Redirect(redirectToPaymentUrl);

            var paymentIdentifier = registrations.FirstOrDefault(x => x.CardComLpc != null)?.CardComLpc
                                    ?? registrations.FirstOrDefault(x => x.LiqPayOrderId != null)?.LiqPayOrderId
                                    ?? registrations.FirstOrDefault(x => x.PayPalLogLigId != null)?.PayPalLogLigId;

            return PartialView("ActivityFormPublishedSubmitResult", new ActivityFormSuccessResultModel { Activity = activity, PaymentIdentifier = paymentIdentifier });
        }

        private ActionResult SaveUnionCustomPersonalForm(ActivityFormPublishSubmitModel model, Activity activity,
            League league, Team team, User user, List<ActivityCustomPriceModel> customPrices)
        {
            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }
            var seasonId = activity.SeasonId.Value;

            if (user == null)
            {
                //create user
                user = new User
                {
                    Password = Protector.Encrypt("123abc12"),
                    TypeId = 4, //player
                    IdentNum = model.PlayerId,

                    //FullName = model.PlayerFullName,
                    FirstName = model.PlayerFirstName,
                    LastName = model.PlayerLastName,
                    MiddleName = model.PlayerMiddleName,

                    ParentName = model.PlayerParentName,
                    Email = model.PlayerEmail,
                    Telephone = model.PlayerPhone,
                    City = model.PlayerCity,
                    Address = model.PlayerAddress,
                    BirthDay = model.PlayerBirthDate,
                    IsActive = true
                };
                usersRepo.Create(user);

                user = usersRepo.GetById(user.UserId);
            }

            if (!activity.NoTeamRegistration)
            {
                var teamPlayer = user.TeamsPlayers.FirstOrDefault(x => x.SeasonId == activity.SeasonId && x.TeamId == model.TeamId && x.LeagueId == model.LeagueId);
                if (teamPlayer == null)
                {
                    user.TeamsPlayers.Add(new TeamsPlayer
                    {
                        SeasonId = activity.SeasonId,
                        LeagueId = league.LeagueId,
                        IsActive = true,
                        IsApprovedByManager = activity.UnionId == 38 && activity.AllowOnlyApprovedMembers //tennis specific
                            ? true
                            : (bool?) null,
                        TeamId = team.TeamId,
                        UserId = user.UserId,
                        ShirtNum = team.TeamsPlayers.Any(x => x.LeagueId == model.LeagueId)
                            ? team.TeamsPlayers.Where(x => x.LeagueId == model.LeagueId).Max(x => x.ShirtNum) + 1
                            : 1,
                        IsTrainerPlayer = model.PlayerIsTrainer,
                        IsEscortPlayer = model.PlayerIsEscort
                    });

                    usersRepo.Save();
                }
                else
                {
                    teamPlayer.IsTrainerPlayer = model.PlayerIsTrainer;
                    teamPlayer.IsEscortPlayer = model.PlayerIsEscort;
                    teamPlayer.IsActive = true;
                }
            }

            var playerPricesHelper = new PlayerPriceHelper();

            LeaguePlayerPrice prices = playerPricesHelper.GetPlayerPrices(user.UserId, team?.TeamId ?? 0, league?.LeagueId ?? 0, seasonId);

            var playerRegistrationsInActivities = user.ActivityFormsSubmittedDatas.Where(
                x => //x.Activity.IsAutomatic == true &&
                     x.Activity.Type == ActivityType.Personal &&
                     x.Activity.SeasonId == activity.SeasonId &&
                     x.Activity.UnionId == activity.UnionId &&
                     x.Activity.ClubId == activity.ClubId &&
                     x.LeagueId != null);

            var isPlayerAlreadyRegisteredInBuiltInActivity = playerRegistrationsInActivities.Any(x => x.Activity.IsAutomatic == true);

            if (activity.NoPriceOnBuiltInRegistration && isPlayerAlreadyRegisteredInBuiltInActivity)
            {
                prices.RegistrationPrice = 0;
                prices.InsurancePrice = 0;
                prices.MembersFee = 0;
                prices.HandlingFee = 0;
            }

            var isPlayerSecondRegistration =
                activity.ActivityFormsSubmittedDatas.Count(x => x.PlayerId == user.UserId) == 1;

            prices.RegistrationPrice = activity.RegistrationPrice ? (model.DisableRegistrationPayment ? 0 : prices.RegistrationPrice) : 0;
            prices.InsurancePrice =
                activity.InsurancePrice ? (model.DisableInsurancePayment ? 0 : prices.InsurancePrice) : 0;
            prices.MembersFee = activity.MembersFee ? (model.DisableMembersFeePayment ? 0 : prices.MembersFee) : 0;
            prices.HandlingFee = activity.HandlingFee ? (model.DisableHandlingFeePayment ? 0 : prices.HandlingFee) : 0;

            if (isPlayerSecondRegistration)
            {
                prices.InsurancePrice =
                    activity.SecondTeamNoInsurance
                        ? 0
                        : prices.InsurancePrice;

                prices.RegistrationPrice =
                    Math.Max(0, prices.RegistrationPrice - (activity.SecondTeamDiscount ?? 0));
            }

            if (activity.AllowEscortRegistration && model.PlayerIsEscort)
            {
                //discount for escort player
                foreach (var customPrice in customPrices)
                {
                    customPrice.Price = Math.Max(0, customPrice.Price - (activity.EscortDiscount ?? 0m));

                    customPrice.TotalPrice = customPrice.Price * customPrice.Quantity;
                }

                if (activity.EscortNoInsurance)
                {
                    prices.InsurancePrice = 0m;
                }
            }
            else if (playerRegistrationsInActivities.Any(x => x.IsActive))
            {
                //discount for approved player
                foreach (var customPrice in customPrices)
                {
                    customPrice.Price = Math.Max(0, customPrice.Price - (activity.UnionApprovedPlayerDiscount ?? 0m));

                    customPrice.TotalPrice = customPrice.Price * customPrice.Quantity;
                }

                if (activity.UnionApprovedPlayerNoInsurance)
                {
                    prices.InsurancePrice = 0m;
                }
            }

            var priceTotal = prices.RegistrationPrice +
                             prices.InsurancePrice +
                             prices.MembersFee +
                             prices.HandlingFee + 
                             customPrices.Sum(x => x.TotalPrice);

            var redirectToPaymentUrl = string.Empty;

            var newDataEntry = new ActivityFormsSubmittedData
            {
                ActivityId = activity.ActivityId,
                PlayerId = user.UserId,
                Comments = model.Comments,
                DateSubmitted = DateTime.Now,
                LeagueId = league?.LeagueId,
                //NameForInvoice = model.NameForInvoice,
                PaymentByBenefactor = model.PaymentByBenefactor,
                IsPaymentByBenefactor = model.IsPaymentByBenefactor,
                TeamId = team?.TeamId,
                Document = SaveFormFile(activity.ActivityId, "document"),
                //MedicalCert = SaveFormFile(activity.ActivityId, "medicalCert"),
                //InsuranceCert = SaveFormFile(activity.ActivityId, "insuranceCert"),
                CustomFields = model.CustomFields,
                CustomPrices = customPrices.Any() ? JsonConvert.SerializeObject(customPrices) : null,

                RegistrationPrice = prices.RegistrationPrice,
                InsurancePrice = prices.InsurancePrice,
                MembersFee = prices.MembersFee,
                HandlingFee = prices.HandlingFee,

                RegistrationCardComProductId = prices.RegistrationPriceCardComProductId,
                InsuranceCardComProductId = prices.InsuranceCardComProductId,
                MembersFeeCardComProductId = prices.MembersFeeCardComProductId,
                HandlingFeeCardComProductId = prices.HandlingFeeCardComProductId,

                DisableRegistrationPayment = model.DisableRegistrationPayment,
                DisableInsurancePayment = model.DisableInsurancePayment,
                DisableMembersFeePayment = model.DisableMembersFeePayment,
                DisableHandlingFeePayment = model.DisableHandlingFeePayment
            };

            #region CardCom payment for catchball union

            if (!model.IsPaymentByBenefactor && activity.UnionId == 15 && priceTotal > 0)
            {
                if (activity.PaymentMethod == (int)ActivityPaymentMethod.PayPal)
                {
                    var result = PayPalHelper.UnionCustomPersonal_Catchball15(activity, user, team, customPrices,
                        priceTotal,
                        prices);

                    newDataEntry.PayPalLogLigId = result.PayPalLogLigId;
                    newDataEntry.PayPalPaymentId = result.PayPalPaymentId;
                    redirectToPaymentUrl = result.RedirectUrl;
                }
                else
                {
                    var result = CardComHelper.UnionCustomPersonal_Catchball(activity, user, team, customPrices, priceTotal, prices);

                    newDataEntry.CardComLpc = result?.Lpc;
                    redirectToPaymentUrl = result?.RedirectUrl;
                }
            }

            #endregion

            #region CardCom payment for Rugby union

            if (!model.IsPaymentByBenefactor && activity.UnionId == 54 && priceTotal > 0)
            {
                var result = CardComHelper.UnionCustomPersonal_Rugby(activity, user, team, customPrices, priceTotal, prices);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion
            
            #region CardCom payment for wave surfing union

            if (activity.Union?.UnionId == 41 && priceTotal > 0)
            {
                var result = CardComHelper.UnionCustomPersonal_WaveSurfing(activity, user, team, customPrices, priceTotal, prices);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for tennis union

            if (activity.UnionId == 38 && priceTotal > 0)
            {
                var result = CardComHelper.UnionCustomPersonal_Tennis(activity, user, team, customPrices, priceTotal, prices);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for waterpolo union

            if (activity.UnionId == 32 && priceTotal > 0)
            {
                var result = CardComHelper.UnionCustomPersonal_Waterpolo(activity, user, team, customPrices, priceTotal, prices);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for Climbing union 59

            if (!model.IsPaymentByBenefactor && activity.UnionId == 59 && priceTotal > 0)
            {
                var result = CardComHelper.UnionCustomPersonal_Climbing59(activity, user, team, customPrices, priceTotal, prices);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for Gymnastic union 36

            if (!model.IsPaymentByBenefactor && activity.UnionId == 36 && priceTotal > 0)
            {
                var result = CardComHelper.UnionCustomPersonal_Gymnastic36(activity, user, team, customPrices, priceTotal, prices);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion
            
            activity.ActivityFormsSubmittedDatas.Add(newDataEntry);

            activityRepo.Save();

            #region Payment by benefactor email

            if (model.IsPaymentByBenefactor)
            {
                var emailService = new EmailService();

                var hostBaseUrl = ConfigurationManager.AppSettings["SiteUrl"];
                var activitiesUrl = string.Format("{0}/Unions/Edit/{1}#activities",
                    hostBaseUrl,
                    activity.UnionId);

                var teamName = team?.TeamsDetails.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ??
                               team?.Title ??
                               Messages.Activity_NoTeamPlaceholder;
                var leagueName = league?.Name ?? string.Empty;

                string emailBody = $@"
{string.Format(Messages.Activity_BuildForm_Email_Header, teamName, leagueName)}
<br />
{Messages.Activity_BuildForm_Email_BenefactorName}: {model.PaymentByBenefactor}<br />
{Messages.Activity_BuildForm_Email_Price}: {prices.RegistrationPrice}<br />
{Messages.Insurance}: {prices.InsurancePrice}<br />
{Messages.LeagueDetail_MemberFees}: {prices.MembersFee}<br />
<br />
{
                    string.Join("<br />", customPrices.Select(x => $"{CustomPriceHelper.GetPriceTitle(getCulture(), x)}: {x.TotalPrice}"))
}
<br /><br />
<a href=""{activitiesUrl}"">{Messages.Activity_BuildForm_Email_OpenActivities}</a>
";

                var managerToSendEmail = usersRepo.GetUnionWorkers(activity.UnionId ?? 0, JobRole.UnionManager, activity.SeasonId)
                    .ToList();
                managerToSendEmail.AddRange(usersRepo.GetUnionWorkers(activity.UnionId ?? 0, JobRole.Activitymanager, activity.SeasonId));
                foreach (var manager in managerToSendEmail)
                {
                    string email = manager.Email;

#if DEBUG
                    email = "info@loglig.com";
#endif

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        try
                        {
                            emailService.SendAsync(email, emailBody);
                        }
                        catch (Exception)
                        {
                            // ¯\_(ツ)_/¯
                        }
                    }
                }
            }

            #endregion

            #region UpdateUser

            SaveFileToPlayer("playerProfilePicture", user, activity.SeasonId ?? 0, PlayerFileType.PlayerImage);
            SaveFileToPlayer("medicalCert", user, activity.SeasonId ?? 0, PlayerFileType.MedicalCertificate);
            SaveFileToPlayer("insuranceCert", user, activity.SeasonId ?? 0, PlayerFileType.Insurance);
            SaveFileToPlayer("idFile", user, activity.SeasonId ?? 0, PlayerFileType.IDFile);

            if (!string.IsNullOrWhiteSpace(model.PlayerCity))
            {
                user.City = model.PlayerCity;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerAddress))
            {
                user.Address = model.PlayerAddress;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerEmail))
            {
                user.Email = model.PlayerEmail;
            }

            //if (!string.IsNullOrWhiteSpace(model.PlayerFullName))
            //{
            //    user.FullName = model.PlayerFullName;
            //}
            if (!string.IsNullOrWhiteSpace(model.PlayerFirstName))
            {
                user.FirstName = model.PlayerFirstName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerLastName))
            {
                user.LastName = model.PlayerLastName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerMiddleName))
            {
                user.MiddleName = model.PlayerMiddleName;
            }

            if (model.PlayerGender.HasValue)
            {
                user.GenderId = model.PlayerGender.Value;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerPhone))
            {
                user.Telephone = model.PlayerPhone;
            }
            //if (model.PlayerBirthDate.HasValue)
            //{
            //    user.BirthDay = model.PlayerBirthDate.Value;
            //}
            usersRepo.Save();

            #endregion

            if (!string.IsNullOrWhiteSpace(redirectToPaymentUrl))
                return Redirect(redirectToPaymentUrl);

            var paymentIdentifier = newDataEntry.CardComLpc ?? newDataEntry.LiqPayOrderId ?? newDataEntry.PayPalLogLigId;

            return PartialView("ActivityFormPublishedSubmitResult", new ActivityFormSuccessResultModel { Activity = activity, PaymentIdentifier = paymentIdentifier});
        }

        private ActionResult SaveClubPlayerForm(ActivityFormPublishSubmitModel model, Activity activity, Club club,
            List<Team> registerToTeams, User user, List<ActivityCustomPriceModel> customPrices)
        {
            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }
            var seasonId = activity.SeasonId.Value;

            if (user == null)
            {
                //create user
                user = new User
                {
                    Password = Protector.Encrypt("123abc12"),
                    TypeId = 4, //player
                    IdentNum = model.PlayerId,

                    //FullName = model.PlayerFullName,
                    FirstName = model.PlayerFirstName,
                    LastName = model.PlayerLastName,
                    MiddleName = model.PlayerMiddleName,

                    ParentName = model.PlayerParentName,

                    Email = model.PlayerEmail,
                    Telephone = model.PlayerPhone,
                    City = model.PlayerCity,
                    BirthDay = model.PlayerBirthDate,
                    IsActive = true
                };
                usersRepo.Create(user);
            }

            var playerPricesHelper = new PlayerPriceHelper();
            var teamsPrices = new List<TeamPlayerPrice>();
            var registrations = new List<ActivityFormsSubmittedData>();

            if (activity.MovePlayerToTeam)
            {
                var registeringToTeamsIds = registerToTeams.Select(x => x.TeamId).ToArray();

                var currentTeams = db.TeamsPlayers
                    .Include(x => x.Team.LeagueTeams)
                    .Where(x => x.SeasonId == seasonId &&
                                x.UserId == user.UserId &&
                                x.ClubId == activity.ClubId &&
                                !registeringToTeamsIds.Contains(x.TeamId))
                    .ToList();
                
                foreach (var currentTeam in currentTeams)
                {
                    var currentTournaments = currentTeam.Team.LeagueTeams
                        .Where(x => x.SeasonId == seasonId)
                        .SelectMany(x => x.Leagues.TeamsPlayers
                            .Where(tp => tp.UserId == user.UserId &&
                                         tp.TeamId == currentTeam.TeamId &&
                                         tp.SeasonId == seasonId))
                        .ToList();

                    if (currentTournaments.Any())
                    {
                        db.TeamsPlayers.RemoveRange(currentTournaments);
                    }
                }
                db.TeamsPlayers.RemoveRange(currentTeams);
            }

            foreach (var team in registerToTeams)
            {
                var existingTeamPlayer =
                team.TeamsPlayers.FirstOrDefault(x => x.SeasonId == seasonId && x.UserId == user.UserId && x.ClubId == activity.ClubId);

                if (existingTeamPlayer == null)
                {
                    team.TeamsPlayers.Add(new TeamsPlayer
                    {
                        SeasonId = seasonId,
                        TeamId = team.TeamId,
                        UserId = user.UserId,
                        IsActive = true,
                        ShirtNum = team.TeamsPlayers.Any(x => x.ClubId == club.ClubId)
                            ? team.TeamsPlayers.Where(x => x.ClubId == club.ClubId).Max(x => x.ShirtNum) + 1
                            : 1,
                        ClubId = club.ClubId
                    });

                    foreach (var tournament in team.LeagueTeams) //add player also to all club's tournaments
                    {
                        tournament.Leagues.TeamsPlayers.Add(new TeamsPlayer
                        {
                            SeasonId = seasonId,
                            TeamId = team.TeamId,
                            UserId = user.UserId,
                            IsActive = true,
                            ShirtNum = team.TeamsPlayers.Any(x => x.ClubId == club.ClubId)
                                ? team.TeamsPlayers.Where(x => x.ClubId == club.ClubId).Max(x => x.ShirtNum) + 1
                                : 1
                        });
                    }
                }

                var prices = playerPricesHelper.GetTeamPlayerPrice(user.UserId, team.TeamId, club.ClubId, seasonId, activity,
                    brotherIdNum: model.PlayerBrotherIdForDiscount,
                    startDateForPriceAdjustment: model.PlayerAdjustPricesStartDate);

                prices.PlayerRegistrationAndEquipmentPrice =
                    activity.RegistrationPrice ? prices.PlayerRegistrationAndEquipmentPrice : 0;
                prices.PlayerInsurancePrice = activity.InsurancePrice
                    ? (model.DisableInsurancePayment ? 0 : prices.PlayerInsurancePrice)
                    : 0;
                prices.ParticipationPrice = activity.ParticipationPrice
                    ? (model.DisableParticipationPayment ? 0 : prices.ParticipationPrice)
                    : 0;

                var isPlayerSecondRegistration =
                    activity.ActivityFormsSubmittedDatas.Count(x => x.PlayerId == user.UserId) == 1;

                if (isPlayerSecondRegistration)
                {
                    prices.PlayerInsurancePrice =
                        activity.SecondTeamNoInsurance
                            ? 0
                            : prices.PlayerInsurancePrice;

                    prices.PlayerRegistrationAndEquipmentPrice =
                        Math.Max(0, prices.PlayerRegistrationAndEquipmentPrice - (activity.SecondTeamDiscount ?? 0));
                }

                teamsPrices.Add(prices);

                var newDataEntry = new ActivityFormsSubmittedData
                {
                    ActivityId = activity.ActivityId,
                    PlayerId = user.UserId,
                    Comments = model.Comments,
                    DateSubmitted = DateTime.Now,
                    //NameForInvoice = model.NameForInvoice,
                    PaymentByBenefactor = model.PaymentByBenefactor,
                    IsPaymentByBenefactor = model.IsPaymentByBenefactor,
                    TeamId = team.TeamId,
                    ClubId = club.ClubId,
                    Document = SaveFormFile(activity.ActivityId, "document"),
                    //MedicalCert = SaveFormFile(activity.ActivityId, "medicalCert"),
                    //InsuranceCert = SaveFormFile(activity.ActivityId, "insuranceCert"),
                    CustomFields = model.CustomFields,
                    CustomPrices = team == registerToTeams.First() && customPrices.Any()
                        ? JsonConvert.SerializeObject(customPrices)
                        : null,

                    RegistrationPrice = prices.PlayerRegistrationAndEquipmentPrice,
                    InsurancePrice = prices.PlayerInsurancePrice,
                    ParticipationPrice = prices.ParticipationPrice,

                    RegistrationCardComProductId = prices.PlayerRegistrationAndEquipmentCardComProductId,
                    InsuranceCardComProductId = prices.PlayerInsuranceCardComProductId,
                    ParticipationCardComProductId = prices.ParticipationCardComProductId,

                    DisableParticipationPayment = model.DisableParticipationPayment,
                    PostponeParticipationPayment = model.PostponeParticipationPayment,

                    DisableInsurancePayment = model.DisableInsurancePayment,

                    BrotherUserId = string.IsNullOrWhiteSpace(model.PlayerBrotherIdForDiscount)
                        ? null
                        : activity.ActivityFormsSubmittedDatas
                            .FirstOrDefault(x => x.User.IdentNum == model.PlayerBrotherIdForDiscount)
                            ?.User
                            ?.UserId,

                    PlayerStartDate = model.PlayerAdjustPricesStartDate
                };

                registrations.Add(newDataEntry);
            }

            var priceTotal = teamsPrices.Sum(x => x.PlayerRegistrationAndEquipmentPrice) +
                             teamsPrices.Sum(x => x.PlayerInsurancePrice) +
                             (model.PostponeParticipationPayment ? 0 : teamsPrices.Sum(x => x.ParticipationPrice))
                             + customPrices.Sum(x => x.TotalPrice);

            var redirectToPaymentUrl = string.Empty;

            #region CardCom payment for Basketball union

            if (!model.IsPaymentByBenefactor && activity.ClubId == 1194 && priceTotal > 0)
            {
                var result = CardComHelper.ClubPlayer_Basketball(
                    activity,
                    user,
                    registerToTeams,
                    customPrices,
                    priceTotal,
                    teamsPrices,
                    model.PostponeParticipationPayment);

                registrations.ForEach(x => x.CardComLpc = result?.Lpc);
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for Rugby union

            if (!model.IsPaymentByBenefactor && activity.Club?.UnionId == 54 && priceTotal > 0)
            {
                var result = CardComHelper.ClubPlayer_Rugby(
                    activity,
                    user,
                    registerToTeams,
                    customPrices,
                    priceTotal,
                    teamsPrices,
                    model.PostponeParticipationPayment);

                registrations.ForEach(x => x.CardComLpc = result?.Lpc);
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for Soccer club

            if (!model.IsPaymentByBenefactor && activity.ClubId == 2541 && priceTotal > 0)
            {
                var result = CardComHelper.ClubPlayer_Soccer(activity, user,
                    registerToTeams,
                    customPrices,
                    priceTotal,
                    teamsPrices);

                registrations.ForEach(x => x.CardComLpc = result?.Lpc);
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for Gymnastic Club 3610

            if (!model.IsPaymentByBenefactor && activity.ClubId == 3610 && priceTotal > 0)
            {
                var result = CardComHelper.ClubPlayer_GymnasticClub3610(activity, user,
                    registerToTeams,
                    customPrices,
                    priceTotal,
                    teamsPrices);

                registrations.ForEach(x => x.CardComLpc = result?.Lpc);
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            registrations.ForEach(x => activity.ActivityFormsSubmittedDatas.Add(x));

            activityRepo.Save();

            #region UpdateUser

            SaveFileToPlayer("playerProfilePicture", user, activity.SeasonId ?? 0, PlayerFileType.PlayerImage);
            SaveFileToPlayer("medicalCert", user, activity.SeasonId ?? 0, PlayerFileType.MedicalCertificate);
            SaveFileToPlayer("insuranceCert", user, activity.SeasonId ?? 0, PlayerFileType.Insurance);

            if (!string.IsNullOrWhiteSpace(model.PlayerCity))
            {
                user.City = model.PlayerCity;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerAddress))
            {
                user.Address = model.PlayerAddress;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerEmail))
            {
                user.Email = model.PlayerEmail;
            }

            //if (!string.IsNullOrWhiteSpace(model.PlayerFullName))
            //{
            //    user.FullName = model.PlayerFullName;
            //}
            if (!string.IsNullOrWhiteSpace(model.PlayerFirstName))
            {
                user.FirstName = model.PlayerFirstName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerLastName))
            {
                user.LastName = model.PlayerLastName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerMiddleName))
            {
                user.MiddleName = model.PlayerMiddleName;
            }

            if (model.PlayerGender.HasValue)
            {
                user.GenderId = model.PlayerGender.Value;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerPhone))
            {
                user.Telephone = model.PlayerPhone;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerParentName))
            {
                user.ParentName = model.PlayerParentName;
            }
            usersRepo.Save();

            #endregion

            if (!string.IsNullOrWhiteSpace(redirectToPaymentUrl))
                return Redirect(redirectToPaymentUrl);

            var paymentIdentifier = registrations.FirstOrDefault(x => x.CardComLpc != null)?.CardComLpc
                                    ?? registrations.FirstOrDefault(x => x.LiqPayOrderId != null)?.LiqPayOrderId;

            return PartialView("ActivityFormPublishedSubmitResult", new ActivityFormSuccessResultModel { Activity = activity, PaymentIdentifier = paymentIdentifier});
        }

        private ActionResult SaveCustomClubPlayerForm(ActivityFormPublishSubmitModel model, Activity activity, Club club,
            List<Team> registerToTeams, User user, List<ActivityCustomPriceModel> customPrices)
        {
            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }
            var seasonId = activity.SeasonId.Value;

            if (user == null)
            {
                //create user
                user = new User
                {
                    Password = Protector.Encrypt("123abc12"),
                    TypeId = 4, //player
                    IdentNum = model.PlayerId,

                    //FullName = model.PlayerFullName,
                    FirstName = model.PlayerFirstName,
                    LastName = model.PlayerLastName,
                    MiddleName = model.PlayerMiddleName,

                    ParentName = model.PlayerParentName,
                    Email = model.PlayerEmail,
                    Telephone = model.PlayerPhone,
                    City = model.PlayerCity,
                    BirthDay = model.PlayerBirthDate,
                    IsActive = true
                };
                usersRepo.Create(user);
            }

            if (!activity.NoTeamRegistration)
            {
                if (activity.MovePlayerToTeam)
                {
                    var registeringToTeamsIds = registerToTeams.Select(x => x.TeamId).ToArray();

                    var currentTeams = db.TeamsPlayers
                        .Include(x => x.Team.LeagueTeams)
                        .Where(x => x.SeasonId == seasonId &&
                                    x.UserId == user.UserId &&
                                    x.ClubId == activity.ClubId &&
                                    !registeringToTeamsIds.Contains(x.TeamId))
                        .ToList();

                    foreach (var currentTeam in currentTeams)
                    {
                        var currentTournaments = currentTeam.Team.LeagueTeams
                            .Where(x => x.SeasonId == seasonId)
                            .SelectMany(x => x.Leagues.TeamsPlayers
                                .Where(tp => tp.UserId == user.UserId &&
                                             tp.TeamId == currentTeam.TeamId &&
                                             tp.SeasonId == seasonId))
                            .ToList();

                        if (currentTournaments.Any())
                        {
                            db.TeamsPlayers.RemoveRange(currentTournaments);
                        }
                    }
                    db.TeamsPlayers.RemoveRange(currentTeams);
                }

                foreach (var team in registerToTeams)
                {
                    var existingTeamPlayer =
                        team.TeamsPlayers.FirstOrDefault(x => x.SeasonId == seasonId && x.UserId == user.UserId && x.ClubId == activity.ClubId);

                    if (existingTeamPlayer == null)
                    {
                        team.TeamsPlayers.Add(new TeamsPlayer
                        {
                            SeasonId = seasonId,
                            TeamId = team.TeamId,
                            UserId = user.UserId,
                            IsActive = true,
                            ShirtNum = team.TeamsPlayers.Any(x => x.ClubId == club.ClubId)
                                ? team.TeamsPlayers.Where(x => x.ClubId == club.ClubId).Max(x => x.ShirtNum) + 1
                                : 1,
                            ClubId = club.ClubId
                        });

                        foreach (var tournament in team.LeagueTeams) //add player also to all club's tournaments
                        {
                            tournament.Leagues.TeamsPlayers.Add(new TeamsPlayer
                            {
                                SeasonId = seasonId,
                                TeamId = team.TeamId,
                                UserId = user.UserId,
                                IsActive = true,
                                ShirtNum = team.TeamsPlayers.Any(x => x.ClubId == club.ClubId)
                                    ? team.TeamsPlayers.Where(x => x.ClubId == club.ClubId).Max(x => x.ShirtNum) + 1
                                    : 1
                            });
                        }
                    }
                }
            }

            var playerPricesHelper = new PlayerPriceHelper();

            var teamsPrices = new List<TeamPlayerPrice>();
            var registrations = new List<ActivityFormsSubmittedData>();

            foreach (var team in registerToTeams)
            {
                var prices = playerPricesHelper.GetTeamPlayerPrice(user.UserId, team?.TeamId ?? 0, club.ClubId, seasonId, activity,
                    brotherIdNum: model.PlayerBrotherIdForDiscount,
                    startDateForPriceAdjustment: model.PlayerAdjustPricesStartDate);

                prices.PlayerRegistrationAndEquipmentPrice =
                    activity.RegistrationPrice ? prices.PlayerRegistrationAndEquipmentPrice : 0;
                prices.PlayerInsurancePrice = activity.InsurancePrice
                    ? (model.DisableInsurancePayment ? 0 : prices.PlayerInsurancePrice)
                    : 0;
                prices.ParticipationPrice = activity.ParticipationPrice
                    ? (model.DisableParticipationPayment ? 0 : prices.ParticipationPrice)
                    : 0;

                var isPlayerSecondRegistration =
                    activity.ActivityFormsSubmittedDatas.Count(x => x.PlayerId == user.UserId) == 1;

                if (isPlayerSecondRegistration)
                {
                    prices.PlayerInsurancePrice =
                        activity.SecondTeamNoInsurance
                            ? 0
                            : prices.PlayerInsurancePrice;

                    prices.PlayerRegistrationAndEquipmentPrice =
                        Math.Max(0, prices.PlayerRegistrationAndEquipmentPrice - (activity.SecondTeamDiscount ?? 0));
                }

                teamsPrices.Add(prices);

                var newDataEntry = new ActivityFormsSubmittedData
                {
                    ActivityId = activity.ActivityId,
                    PlayerId = user.UserId,
                    Comments = model.Comments,
                    DateSubmitted = DateTime.Now,
                    //NameForInvoice = model.NameForInvoice,
                    PaymentByBenefactor = model.PaymentByBenefactor,
                    IsPaymentByBenefactor = model.IsPaymentByBenefactor,
                    TeamId = team?.TeamId,
                    ClubId = club.ClubId,
                    Document = SaveFormFile(activity.ActivityId, "document"),
                    //MedicalCert = SaveFormFile(activity.ActivityId, "medicalCert"),
                    //InsuranceCert = SaveFormFile(activity.ActivityId, "insuranceCert"),
                    CustomFields = model.CustomFields,

                    CustomPrices = team == registerToTeams.First()
                        ? customPrices.Any() ? JsonConvert.SerializeObject(customPrices) : null
                        : null,

                    RegistrationPrice = prices.PlayerRegistrationAndEquipmentPrice,
                    InsurancePrice = prices.PlayerInsurancePrice,
                    ParticipationPrice = prices.ParticipationPrice,

                    RegistrationCardComProductId = prices.PlayerRegistrationAndEquipmentCardComProductId,
                    InsuranceCardComProductId = prices.PlayerInsuranceCardComProductId,
                    ParticipationCardComProductId = prices.ParticipationCardComProductId,

                    DisableParticipationPayment = model.DisableParticipationPayment,
                    DisableInsurancePayment = model.DisableInsurancePayment,

                    BrotherUserId = string.IsNullOrWhiteSpace(model.PlayerBrotherIdForDiscount)
                        ? null
                        : activity.ActivityFormsSubmittedDatas
                            .FirstOrDefault(x => x.User.IdentNum == model.PlayerBrotherIdForDiscount)
                            ?.User
                            ?.UserId,
                    PlayerStartDate = model.PlayerAdjustPricesStartDate
                };

                registrations.Add(newDataEntry);
            }

            var priceTotal = teamsPrices.Sum(x => x.PlayerRegistrationAndEquipmentPrice) +
                             teamsPrices.Sum(x => x.PlayerInsurancePrice) +
                             teamsPrices.Sum(x => x.ParticipationPrice) +
                             customPrices.Sum(x => x.TotalPrice);

            var redirectToPaymentUrl = string.Empty;

            #region CardCom payment for Basketball union

            if (!model.IsPaymentByBenefactor && activity.ClubId == 1194 && priceTotal > 0)
            {
                var result = CardComHelper.ClubPlayer_Basketball(activity, user, registerToTeams, customPrices, priceTotal,
                    teamsPrices);

                registrations.ForEach(x => x.CardComLpc = result?.Lpc);
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for Soccer club

            if (!model.IsPaymentByBenefactor && activity.ClubId == 2541 && priceTotal > 0)
            {
                var result = CardComHelper.ClubPlayer_Soccer(activity, user, registerToTeams, customPrices, priceTotal,
                    teamsPrices);

                registrations.ForEach(x => x.CardComLpc = result?.Lpc);
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for Gymnastic Club 3610

            if (!model.IsPaymentByBenefactor && activity.ClubId == 3610 && priceTotal > 0)
            {
                var result = CardComHelper.ClubPlayer_GymnasticClub3610(activity, user, registerToTeams, customPrices, priceTotal,
                    teamsPrices);

                registrations.ForEach(x => x.CardComLpc = result?.Lpc);
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            registrations.ForEach(x => activity.ActivityFormsSubmittedDatas.Add(x));

            activityRepo.Save();

            #region UpdateUser

            SaveFileToPlayer("playerProfilePicture", user, activity.SeasonId ?? 0, PlayerFileType.PlayerImage);
            SaveFileToPlayer("medicalCert", user, activity.SeasonId ?? 0, PlayerFileType.MedicalCertificate);
            SaveFileToPlayer("insuranceCert", user, activity.SeasonId ?? 0, PlayerFileType.Insurance);

            if (!string.IsNullOrWhiteSpace(model.PlayerAddress))
            {
                user.Address = model.PlayerAddress;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerCity))
            {
                user.City = model.PlayerCity;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerEmail))
            {
                user.Email = model.PlayerEmail;
            }
            //if (!string.IsNullOrWhiteSpace(model.PlayerFullName))
            //{
            //    user.FullName = model.PlayerFullName;
            //}
            if (!string.IsNullOrWhiteSpace(model.PlayerFirstName))
            {
                user.FirstName = model.PlayerFirstName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerLastName))
            {
                user.LastName = model.PlayerLastName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerMiddleName))
            {
                user.MiddleName = model.PlayerMiddleName;
            }
            if (model.PlayerGender.HasValue)
            {
                user.GenderId = model.PlayerGender.Value;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerPhone))
            {
                user.Telephone = model.PlayerPhone;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerParentName))
            {
                user.ParentName = model.PlayerParentName;
            }
            usersRepo.Save();

            #endregion

            if (!string.IsNullOrWhiteSpace(redirectToPaymentUrl))
                return Redirect(redirectToPaymentUrl);

            var paymentIdentifier = registrations.FirstOrDefault(x => x.CardComLpc != null)?.CardComLpc
                                    ?? registrations.FirstOrDefault(x => x.LiqPayOrderId != null)?.LiqPayOrderId;

            return PartialView("ActivityFormPublishedSubmitResult", new ActivityFormSuccessResultModel { Activity = activity, PaymentIdentifier = paymentIdentifier});
        }

        private ActionResult SaveDepartmentClubPlayerForm(ActivityFormPublishSubmitModel model, Activity activity, Club club,
            Team team, User user, List<ActivityCustomPriceModel> customPrices)
        {
            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }
            var seasonId = activity.SeasonId.Value;

            if (user == null)
            {
                //create user
                user = new User
                {
                    Password = Protector.Encrypt("123abc12"),
                    TypeId = 4, //player
                    IdentNum = model.PlayerId,

                    //FullName = model.PlayerFullName,
                    FirstName = model.PlayerFirstName,
                    LastName = model.PlayerLastName,
                    MiddleName = model.PlayerMiddleName,

                    ParentName = model.PlayerParentName,
                    Email = model.PlayerEmail,
                    Telephone = model.PlayerPhone,
                    City = model.PlayerCity,
                    BirthDay = model.PlayerBirthDate,
                    IsActive = true
                };
                usersRepo.Create(user);

                team.TeamsPlayers.Add(new TeamsPlayer
                {
                    SeasonId = seasonId,
                    TeamId = team.TeamId,
                    UserId = user.UserId,
                    IsActive = true,
                    ShirtNum = team.TeamsPlayers.Any(x => x.ClubId == club.ClubId) ? team.TeamsPlayers.Where(x => x.ClubId == club.ClubId).Max(x => x.ShirtNum) + 1 : 1,
                    ClubId = club.ClubId
                });
            }

            var playerPricesHelper = new PlayerPriceHelper();

            var prices = playerPricesHelper.GetTeamPlayerPrice(user.UserId, team.TeamId, club.ClubId, seasonId, activity);

            var redirectToPaymentUrl = string.Empty;

            prices.PlayerRegistrationAndEquipmentPrice =
                activity.RegistrationPrice ? prices.PlayerRegistrationAndEquipmentPrice : 0;
            prices.PlayerInsurancePrice = activity.InsurancePrice
                ? (model.DisableInsurancePayment ? 0 : prices.PlayerInsurancePrice)
                : 0;
            prices.ParticipationPrice = activity.ParticipationPrice ? prices.ParticipationPrice : 0;

            var isPlayerSecondRegistration =
                activity.ActivityFormsSubmittedDatas.Count(x => x.PlayerId == user.UserId) == 1;

            if (isPlayerSecondRegistration)
            {
                prices.PlayerInsurancePrice =
                    activity.SecondTeamNoInsurance
                        ? 0
                        : prices.PlayerInsurancePrice;

                prices.PlayerRegistrationAndEquipmentPrice =
                    Math.Max(0, prices.PlayerRegistrationAndEquipmentPrice - (activity.SecondTeamDiscount ?? 0));
            }

            var priceTotal = prices.PlayerRegistrationAndEquipmentPrice +
                             prices.PlayerInsurancePrice + 
                             prices.ParticipationPrice + 
                             customPrices.Sum(x => x.TotalPrice);

            var newDataEntry = new ActivityFormsSubmittedData
            {
                ActivityId = activity.ActivityId,
                PlayerId = user.UserId,
                Comments = model.Comments,
                DateSubmitted = DateTime.Now,
                //NameForInvoice = model.NameForInvoice,
                PaymentByBenefactor = model.PaymentByBenefactor,
                IsPaymentByBenefactor = model.IsPaymentByBenefactor,
                TeamId = team.TeamId,
                ClubId = club.ClubId,
                Document = SaveFormFile(activity.ActivityId, "document"),
                //MedicalCert = SaveFormFile(activity.ActivityId, "medicalCert"),
                //InsuranceCert = SaveFormFile(activity.ActivityId, "insuranceCert"),
                CustomFields = model.CustomFields,
                CustomPrices = customPrices.Any() ? JsonConvert.SerializeObject(customPrices) : null,

                RegistrationPrice = prices.PlayerRegistrationAndEquipmentPrice,
                InsurancePrice = prices.PlayerInsurancePrice,
                ParticipationPrice = prices.ParticipationPrice,

                RegistrationCardComProductId = prices.PlayerRegistrationAndEquipmentCardComProductId,
                InsuranceCardComProductId = prices.PlayerInsuranceCardComProductId,
                ParticipationCardComProductId = prices.ParticipationCardComProductId,
            };

            #region CardCom payment for Basketball union

            if (!model.IsPaymentByBenefactor && activity.ClubId == 1194 && priceTotal > 0)
            {
                var result = CardComHelper.DepartmentClubPlayer_Basketball(activity, user, team, customPrices,
                    priceTotal, prices);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            activity.ActivityFormsSubmittedDatas.Add(newDataEntry);

            activityRepo.Save();

            #region UpdateUser

            SaveFileToPlayer("playerProfilePicture", user, activity.SeasonId ?? 0, PlayerFileType.PlayerImage);
            SaveFileToPlayer("medicalCert", user, activity.SeasonId ?? 0, PlayerFileType.MedicalCertificate);
            SaveFileToPlayer("insuranceCert", user, activity.SeasonId ?? 0, PlayerFileType.Insurance);

            if (!string.IsNullOrWhiteSpace(model.PlayerCity))
            {
                user.City = model.PlayerCity;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerEmail))
            {
                user.Email = model.PlayerEmail;
            }

            //if (!string.IsNullOrWhiteSpace(model.PlayerFullName))
            //{
            //    user.FullName = model.PlayerFullName;
            //}
            if (!string.IsNullOrWhiteSpace(model.PlayerFirstName))
            {
                user.FirstName = model.PlayerFirstName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerLastName))
            {
                user.LastName = model.PlayerLastName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerMiddleName))
            {
                user.MiddleName = model.PlayerMiddleName;
            }

            if (model.PlayerGender.HasValue)
            {
                user.GenderId = model.PlayerGender.Value;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerPhone))
            {
                user.Telephone = model.PlayerPhone;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerParentName))
            {
                user.ParentName = model.PlayerParentName;
            }
            usersRepo.Save();

            #endregion

            if (!string.IsNullOrWhiteSpace(redirectToPaymentUrl))
                return Redirect(redirectToPaymentUrl);

            var paymentIdentifier = newDataEntry.CardComLpc ?? newDataEntry.LiqPayOrderId;

            return PartialView("ActivityFormPublishedSubmitResult", new ActivityFormSuccessResultModel { Activity = activity, PaymentIdentifier = paymentIdentifier});
        }

        private ActionResult SaveCustomDepartmentClubPlayerForm(ActivityFormPublishSubmitModel model, Activity activity, Club club,
            Team team, User user, List<ActivityCustomPriceModel> customPrices)
        {
            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }
            var seasonId = activity.SeasonId.Value;

            if (user == null)
            {
                //create user
                user = new User
                {
                    Password = Protector.Encrypt("123abc12"),
                    TypeId = 4, //player
                    IdentNum = model.PlayerId,

                    //FullName = model.PlayerFullName,
                    FirstName = model.PlayerFirstName,
                    LastName = model.PlayerLastName,
                    MiddleName = model.PlayerMiddleName,

                    ParentName = model.PlayerParentName,
                    Email = model.PlayerEmail,
                    Telephone = model.PlayerPhone,
                    City = model.PlayerCity,
                    BirthDay = model.PlayerBirthDate,
                    IsActive = true
                };
                usersRepo.Create(user);

                if (!activity.NoTeamRegistration)
                {
                    team.TeamsPlayers.Add(new TeamsPlayer
                    {
                        SeasonId = seasonId,
                        TeamId = team.TeamId,
                        UserId = user.UserId,
                        IsActive = true,
                        ShirtNum = team.TeamsPlayers.Any(x => x.ClubId == club.ClubId) ? team.TeamsPlayers.Where(x => x.ClubId == club.ClubId).Max(x => x.ShirtNum) + 1 : 1,
                        ClubId = club.ClubId
                    });
                }
            }

            var playerPricesHelper = new PlayerPriceHelper();

            var prices = playerPricesHelper.GetTeamPlayerPrice(user.UserId, team?.TeamId ?? 0, club.ClubId, seasonId, activity);

            var redirectToPaymentUrl = string.Empty;

            prices.PlayerRegistrationAndEquipmentPrice =
                activity.RegistrationPrice ? 0 : prices.PlayerRegistrationAndEquipmentPrice;
            prices.PlayerInsurancePrice = activity.InsurancePrice
                ? (model.DisableInsurancePayment ? 0 : prices.PlayerInsurancePrice)
                : 0;
            prices.ParticipationPrice = activity.ParticipationPrice
                ? (model.DisableParticipationPayment ? 0 : prices.ParticipationPrice)
                : 0;

            var isPlayerSecondRegistration =
                activity.ActivityFormsSubmittedDatas.Count(x => x.PlayerId == user.UserId) == 1;

            if (isPlayerSecondRegistration)
            {
                prices.PlayerInsurancePrice =
                    activity.SecondTeamNoInsurance
                        ? 0
                        : prices.PlayerInsurancePrice;

                prices.PlayerRegistrationAndEquipmentPrice =
                    Math.Max(0, prices.PlayerRegistrationAndEquipmentPrice - (activity.SecondTeamDiscount ?? 0));
            }

            var priceTotal = prices.PlayerRegistrationAndEquipmentPrice +
                             prices.PlayerInsurancePrice +
                             prices.ParticipationPrice + 
                             customPrices.Sum(x => x.TotalPrice);

            var newDataEntry = new ActivityFormsSubmittedData
            {
                ActivityId = activity.ActivityId,
                PlayerId = user.UserId,
                Comments = model.Comments,
                DateSubmitted = DateTime.Now,
                //NameForInvoice = model.NameForInvoice,
                PaymentByBenefactor = model.PaymentByBenefactor,
                IsPaymentByBenefactor = model.IsPaymentByBenefactor,
                TeamId = team?.TeamId,
                ClubId = club.ClubId,
                Document = SaveFormFile(activity.ActivityId, "document"),
                //MedicalCert = SaveFormFile(activity.ActivityId, "medicalCert"),
                //InsuranceCert = SaveFormFile(activity.ActivityId, "insuranceCert"),
                CustomFields = model.CustomFields,
                CustomPrices = customPrices.Any() ? JsonConvert.SerializeObject(customPrices) : null,

                RegistrationPrice = prices.PlayerRegistrationAndEquipmentPrice,
                InsurancePrice = prices.PlayerInsurancePrice,
                ParticipationPrice = prices.ParticipationPrice,

                RegistrationCardComProductId = prices.PlayerRegistrationAndEquipmentCardComProductId,
                InsuranceCardComProductId = prices.PlayerInsuranceCardComProductId,
                ParticipationCardComProductId = prices.ParticipationCardComProductId,

                DisableParticipationPayment = model.DisableParticipationPayment,
                DisableInsurancePayment = model.DisableInsurancePayment
            };

            #region CardCom payment for Basketball union

            if (!model.IsPaymentByBenefactor && activity.ClubId == 1194 && priceTotal > 0)
            {
                var result = CardComHelper.DepartmentClubPlayer_Basketball(activity, user, team, customPrices,
                    priceTotal, prices);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            activity.ActivityFormsSubmittedDatas.Add(newDataEntry);

            activityRepo.Save();

            #region UpdateUser

            SaveFileToPlayer("playerProfilePicture", user, activity.SeasonId ?? 0, PlayerFileType.PlayerImage);
            SaveFileToPlayer("medicalCert", user, activity.SeasonId ?? 0, PlayerFileType.MedicalCertificate);
            SaveFileToPlayer("insuranceCert", user, activity.SeasonId ?? 0, PlayerFileType.Insurance);

            if (!string.IsNullOrWhiteSpace(model.PlayerCity))
            {
                user.City = model.PlayerCity;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerEmail))
            {
                user.Email = model.PlayerEmail;
            }

            //if (!string.IsNullOrWhiteSpace(model.PlayerFullName))
            //{
            //    user.FullName = model.PlayerFullName;
            //}
            if (!string.IsNullOrWhiteSpace(model.PlayerFirstName))
            {
                user.FirstName = model.PlayerFirstName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerLastName))
            {
                user.LastName = model.PlayerLastName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerMiddleName))
            {
                user.MiddleName = model.PlayerMiddleName;
            }

            if (model.PlayerGender.HasValue)
            {
                user.GenderId = model.PlayerGender.Value;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerPhone))
            {
                user.Telephone = model.PlayerPhone;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerParentName))
            {
                user.ParentName = model.PlayerParentName;
            }
            usersRepo.Save();

            #endregion

            if (!string.IsNullOrWhiteSpace(redirectToPaymentUrl))
                return Redirect(redirectToPaymentUrl);

            var paymentIdentifier = newDataEntry.CardComLpc ?? newDataEntry.LiqPayOrderId;

            return PartialView("ActivityFormPublishedSubmitResult", new ActivityFormSuccessResultModel { Activity = activity, PaymentIdentifier = paymentIdentifier});
        }

        private ActionResult SavePlayerForm(ActivityFormPublishSubmitModel model, Activity activity, League league,
            Team team, User user, List<ActivityCustomPriceModel> customPrices)
        {
            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }
            var seasonId = activity.SeasonId.Value;

            var playerPricesHelper = new PlayerPriceHelper();

            var isPlayerSecondRegistration =
                activity.ActivityFormsSubmittedDatas.Count(x => x.PlayerId == user.UserId) == 1;

            var playerPrices = playerPricesHelper.GetPlayerPrices(user.UserId, team.TeamId, league.LeagueId, seasonId);

            playerPrices.RegistrationPrice = activity.RegistrationPrice ? playerPrices.RegistrationPrice : 0;
            playerPrices.InsurancePrice = activity.InsurancePrice ? playerPrices.InsurancePrice : 0;
                        
            if (isPlayerSecondRegistration)
            {
                playerPrices.InsurancePrice =
                    activity.SecondTeamNoInsurance
                        ? 0
                        : playerPrices.InsurancePrice;

                playerPrices.RegistrationPrice =
                    Math.Max(0, playerPrices.RegistrationPrice - (activity.SecondTeamDiscount ?? 0));
            }

            var priceTotal = playerPrices.RegistrationPrice + playerPrices.InsurancePrice + customPrices.Sum(x => x.TotalPrice);

            var redirectToPaymentUrl = string.Empty;

            var newDataEntry = new ActivityFormsSubmittedData
            {
                ActivityId = activity.ActivityId,
                PlayerId = user.UserId,
                Comments = model.Comments,
                DateSubmitted = DateTime.Now,
                LeagueId = league.LeagueId,
                //NameForInvoice = model.NameForInvoice,
                PaymentByBenefactor = model.PaymentByBenefactor,
                IsPaymentByBenefactor = model.IsPaymentByBenefactor,
                TeamId = team.TeamId,
                Document = SaveFormFile(activity.ActivityId, "document"),
                //MedicalCert = SaveFormFile(activity.ActivityId, "medicalCert"),
                //InsuranceCert = SaveFormFile(activity.ActivityId, "insuranceCert"),
                CustomFields = model.CustomFields,
                CustomPrices = customPrices.Any() ? JsonConvert.SerializeObject(customPrices) : null,

                RegistrationPrice = playerPrices.RegistrationPrice,
                InsurancePrice = playerPrices.InsurancePrice,

                RegistrationCardComProductId = playerPrices.RegistrationPriceCardComProductId,
                InsuranceCardComProductId = playerPrices.InsuranceCardComProductId
            };

            #region UpdateUser

            SaveFileToPlayer("playerProfilePicture", user, activity.SeasonId ?? 0, PlayerFileType.PlayerImage);
            SaveFileToPlayer("medicalCert", user, activity.SeasonId ?? 0, PlayerFileType.MedicalCertificate);
            SaveFileToPlayer("insuranceCert", user, activity.SeasonId ?? 0, PlayerFileType.Insurance);

            if (!string.IsNullOrWhiteSpace(model.PlayerAddress))
            {
                user.Address = model.PlayerAddress;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerCity))
            {
                user.City = model.PlayerCity;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerEmail))
            {
                user.Email = model.PlayerEmail;
            }

            //if (!string.IsNullOrWhiteSpace(model.PlayerFullName))
            //{
            //    user.FullName = model.PlayerFullName;
            //}
            if (!string.IsNullOrWhiteSpace(model.PlayerFirstName))
            {
                user.FirstName = model.PlayerFirstName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerLastName))
            {
                user.LastName = model.PlayerLastName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerMiddleName))
            {
                user.MiddleName = model.PlayerMiddleName;
            }

            if (model.PlayerGender.HasValue)
            {
                user.GenderId = model.PlayerGender.Value;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerPhone))
            {
                user.Telephone = model.PlayerPhone;
            }
            //if (model.PlayerBirthDate.HasValue)
            //{
            //    user.BirthDay = model.PlayerBirthDate.Value;
            //}
            usersRepo.Save();

            #endregion

            #region CardCom payment for catchball union

            if (!model.IsPaymentByBenefactor && activity.UnionId == 15 &&
                priceTotal > 0)
            {
                var result = CardComHelper.UnionPlayer_Catchball(activity, user, team, customPrices, priceTotal, playerPrices);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for Rugby union

            if (!model.IsPaymentByBenefactor && activity.UnionId == 54 &&
                priceTotal > 0)
            {
                var result = CardComHelper.UnionPlayer_Rugby(activity, user, team, customPrices, priceTotal, playerPrices);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for wave surfing union

            if (activity.Union?.UnionId == 41 && priceTotal > 0)
            {
                var result = CardComHelper.UnionPlayer_WaveSurfing(activity, user, team, customPrices, priceTotal, playerPrices);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for tennis union

            if (activity.UnionId == 38 && priceTotal > 0)
            {
                var result = CardComHelper.UnionPlayer_Tennis(activity, user, team, customPrices, priceTotal, playerPrices);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region LiqPay payment for UA gymnastic federation union

            if (activity.UnionId == 52 && priceTotal > 0)
            {
                var result = LiqPayHelper.UnionPlayer_UaGymnastic(activity, user, team, customPrices, priceTotal, playerPrices);

                newDataEntry.LiqPayOrderId = result?.OrderId; //TODO:
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            activity.ActivityFormsSubmittedDatas.Add(newDataEntry);

            activityRepo.Save();

            #region Payment by benefactor email

            if (model.IsPaymentByBenefactor)
            {
                var emailService = new EmailService();

                var hostBaseUrl = ConfigurationManager.AppSettings["SiteUrl"];
                var activitiesUrl = string.Format("{0}/Unions/Edit/{1}#activities",
                    hostBaseUrl,
                    activity.UnionId);

                string emailBody = $@"
{
                        string.Format(Messages.Activity_BuildForm_Email_Header,
                            team.TeamsDetails.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ??
                            team.Title,
                            league.Name)
                    }
<br />
{Messages.Activity_BuildForm_Email_BenefactorName}: {model.PaymentByBenefactor}<br />
{Messages.Activity_BuildForm_Email_Price}: {
                        league.LeaguesPrices.FirstOrDefault(
                                x => x.PriceType == (int?)LeaguePriceType.TeamRegistrationPrice &&
                                     x.StartDate <= newDataEntry.DateSubmitted &&
                                     x.EndDate >= newDataEntry.DateSubmitted)
                            ?.Price ?? 0
                    }<br />
<br /><br />
<a href=""{activitiesUrl}"">{Messages.Activity_BuildForm_Email_OpenActivities}</a>
";

                var managerToSendEmail = usersRepo.GetUnionWorkers(activity.UnionId ?? 0, JobRole.UnionManager, activity.SeasonId)
                    .ToList();
                managerToSendEmail.AddRange(usersRepo.GetUnionWorkers(activity.UnionId ?? 0, JobRole.Activitymanager, activity.SeasonId));
                foreach (var manager in managerToSendEmail)
                {
                    string email = manager.Email;

#if DEBUG
                    email = "info@loglig.com";
#endif

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        try
                        {
                            emailService.SendAsync(email, emailBody);
                        }
                        catch (Exception ex)
                        {
                            //log.Error(Messages.TeamBenefactor_SendEmailLogException, ex);
                        }
                    }
                }
            }

            #endregion

            if (!string.IsNullOrWhiteSpace(redirectToPaymentUrl))
                return Redirect(redirectToPaymentUrl);

            var paymentIdentifier = newDataEntry.CardComLpc ?? newDataEntry.LiqPayOrderId;

            return PartialView("ActivityFormPublishedSubmitResult", new ActivityFormSuccessResultModel { Activity = activity, PaymentIdentifier = paymentIdentifier});
        }

        private string SaveFileToPlayer(string fileInRequest, User user, int seasonId, PlayerFileType fileType)
        {
            if (Request.Files[fileInRequest] != null && Request.Files[fileInRequest].ContentLength != 0)
            {
                var postedFile = Request.Files[fileInRequest];
                var maxFileSize = GlobVars.MaxFileSize * 1000;

                if (postedFile.ContentLength > maxFileSize)
                {
                    return Messages.FileSizeError;
                }

                var newName = PlayerFileHelper.SaveFile(postedFile, user.UserId, fileType);
                if (newName == null)
                {
                    return Messages.FileError;
                }

                var playerFile =
                    user.PlayerFiles.FirstOrDefault(x =>
                        x.SeasonId == seasonId &&
                        x.FileType == (int)fileType);
                if (playerFile != null)
                {
                    db.Entry(playerFile).State = EntityState.Deleted;
                }

                if (fileType == PlayerFileType.IDFile)
                {
                    user.IDFile = newName;
                }
                else
                {
                    user.PlayerFiles.Add(new PlayerFile
                    {
                        DateCreated = DateTime.Now,
                        FileName = newName,
                        FileType = (int)fileType,
                        PlayerId = user.UserId,
                        SeasonId = seasonId
                    });
                }
            }

            return null;
        }

        private ActionResult SaveTeamForm(ActivityFormPublishSubmitModel model, Activity activity, League league,
            Team team, User user, List<ActivityCustomPriceModel> customPrices)
        {
            string redirectToPaymentUrl = null;

            var leagueRegistrationPrice = league.LeaguesPrices.FirstOrDefault(x => x.PriceType == (int?)LeaguePriceType.TeamRegistrationPrice &&
                                                         x.StartDate <= DateTime.Now && x.EndDate > DateTime.Now);
            var registrationPrice = leagueRegistrationPrice?.Price ?? 0;

            var newDataEntry = new ActivityFormsSubmittedData
            {
                ActivityId = activity.ActivityId,
                PlayerId = user.UserId,
                Comments = model.Comments,
                DateSubmitted = DateTime.Now,
                LeagueId = league.LeagueId,
                //NameForInvoice = model.NameForInvoice,
                PaymentByBenefactor = model.PaymentByBenefactor,
                IsPaymentByBenefactor = model.IsPaymentByBenefactor,
                TeamId = team.TeamId,
                SelfInsurance = model.SelfInsurance,
                Document = SaveFormFile(activity.ActivityId, "document"),
                MedicalCert = SaveFormFile(activity.ActivityId, "medicalCert"),
                InsuranceCert = SaveFormFile(activity.ActivityId, "insuranceCert"),
                CustomFields = model.CustomFields,
                CustomPrices = customPrices.Any() ? JsonConvert.SerializeObject(customPrices) : null,

                RegistrationPrice = registrationPrice,

                RegistrationCardComProductId = leagueRegistrationPrice?.CardComProductId
            };

            team.NeedShirts = model.NeedShirts;

            var priceTotal = registrationPrice + customPrices.Sum(x => x.TotalPrice);

            #region CardCom payment for catchball union

            if (!model.IsPaymentByBenefactor && activity.UnionId == 15 &&
                priceTotal > 0)
            {
                var result =
                    CardComHelper.UnionTeam_Catchball(activity, user, team, customPrices, priceTotal, leagueRegistrationPrice);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for Rugby union

            if (!model.IsPaymentByBenefactor && activity.UnionId == 54 &&
                priceTotal > 0)
            {
                var result =
                    CardComHelper.UnionTeam_Rugby(activity, user, team, customPrices, priceTotal, leagueRegistrationPrice);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for wave surfing union

            if (activity.Union?.UnionId == 41 && priceTotal > 0)
            {
                var result =
                    CardComHelper.UnionTeam_WaveSurfing(activity, user, team, customPrices, priceTotal, leagueRegistrationPrice);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for tennis union

            if (activity.UnionId == 38 && priceTotal > 0)
            {
                var result =
                    CardComHelper.UnionTeam_Tennis(activity, user, team, customPrices, priceTotal, leagueRegistrationPrice);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            //#region CardCom payment for waterpolo union

            //if (activity.UnionId == 32 && priceTotal > 0)
            //{
            //    var result =
            //        CardComHelper.UnionTeam_Waterpolo(activity, user, team, customPrices, priceTotal, leagueRegistrationPrice);

            //    newDataEntry.CardComLpc = result?.Lpc;
            //    redirectToPaymentUrl = result?.RedirectUrl;
            //}

            //#endregion

            activity.ActivityFormsSubmittedDatas.Add(newDataEntry);

            activityRepo.Save();

            #region Payment by benefactor email

            if (model.IsPaymentByBenefactor)
            {
                var emailService = new EmailService();

                var hostBaseUrl = ConfigurationManager.AppSettings["SiteUrl"];
                var activitiesUrl = string.Format("{0}/Unions/Edit/{1}#activities",
                    hostBaseUrl,
                    activity.UnionId);

                string emailBody = $@"
{string.Format(Messages.Activity_BuildForm_Email_Header, team.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title, league.Name)}
<br />
{Messages.Activity_BuildForm_Email_BenefactorName}: {model.PaymentByBenefactor}<br />
{Messages.Activity_BuildForm_Email_Price}: {
                        league.LeaguesPrices.FirstOrDefault(
                                x => x.PriceType == (int?)LeaguePriceType.TeamRegistrationPrice &&
                                     x.StartDate <= newDataEntry.DateSubmitted &&
                                     x.EndDate >= newDataEntry.DateSubmitted)
                            ?.Price ?? 0
                    }<br />
<br /><br />
<a href=""{activitiesUrl}"">{Messages.Activity_BuildForm_Email_OpenActivities}</a>
";

                var managerToSendEmail = usersRepo.GetUnionWorkers(activity.UnionId ?? 0, JobRole.UnionManager, activity.SeasonId)
                    .ToList();
                managerToSendEmail.AddRange(usersRepo.GetUnionWorkers(activity.UnionId ?? 0, JobRole.Activitymanager, activity.SeasonId));
                foreach (var manager in managerToSendEmail)
                {
                    string email = manager.Email;

#if DEBUG
                    email = "info@loglig.com";
#endif

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        try
                        {
                            emailService.SendAsync(email, emailBody);
                        }
                        catch (Exception ex)
                        {
                            //log.Error(Messages.TeamBenefactor_SendEmailLogException, ex);
                        }
                    }
                }
            }

            #endregion

            #region UpdateUser

            if (!string.IsNullOrWhiteSpace(model.PlayerCity))
            {
                user.City = model.PlayerCity;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerEmail))
            {
                user.Email = model.PlayerEmail;
            }

            //if (!string.IsNullOrWhiteSpace(model.PlayerFullName))
            //{
            //    user.FullName = model.PlayerFullName;
            //}
            if (!string.IsNullOrWhiteSpace(model.PlayerFirstName))
            {
                user.FirstName = model.PlayerFirstName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerLastName))
            {
                user.LastName = model.PlayerLastName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerMiddleName))
            {
                user.MiddleName = model.PlayerMiddleName;
            }

            if (!string.IsNullOrWhiteSpace(model.PlayerPhone))
            {
                user.Telephone = model.PlayerPhone;
            }
            if (model.PlayerBirthDate.HasValue)
            {
                user.BirthDay = model.PlayerBirthDate.Value;
            }
            usersRepo.Save();

            #endregion

            if (!string.IsNullOrWhiteSpace(redirectToPaymentUrl))
                return Redirect(redirectToPaymentUrl);

            var paymentIdentifier = newDataEntry.CardComLpc ?? newDataEntry.LiqPayOrderId;

            return PartialView("ActivityFormPublishedSubmitResult", new ActivityFormSuccessResultModel { Activity = activity, PaymentIdentifier = paymentIdentifier});
        }

        private ActionResult SaveUnionCustomTeamForm(ActivityFormPublishSubmitModel model, Activity activity, League league,
            Team team, User user, List<ActivityCustomPriceModel> customPrices)
        {
            string redirectToPaymentUrl = null;

            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }
            var seasonId = activity.SeasonId.Value;

            if (user == null)
            {
                //create user
                user = new User
                {
                    Password = Protector.Encrypt("123abc12"),
                    TypeId = 4, //player
                    IdentNum = model.PlayerId,

                    //FullName = model.PlayerFullName,
                    FirstName = model.PlayerFirstName,
                    LastName = model.PlayerLastName,
                    MiddleName = model.PlayerMiddleName,

                    ParentName = model.PlayerParentName,
                    Email = model.PlayerEmail,
                    Telephone = model.PlayerPhone,
                    Address = model.PlayerAddress,
                    BirthDay = model.PlayerBirthDate,
                    IsActive = true
                };
                usersRepo.Create(user);

                user = usersRepo.GetById(user.UserId);
            }

            var leagueRegistrationPrice =
                league.LeaguesPrices.FirstOrDefault(x => x.PriceType == (int?)LeaguePriceType.TeamRegistrationPrice &&
                                                         x.StartDate <= DateTime.Now && x.EndDate > DateTime.Now);

            if (activity.AllowNewTeamRegistration)
            {
                if (string.IsNullOrWhiteSpace(model.PlayerTeam))
                {
                    return Content("Team name is empty");
                }

                var newTeam = new Team
                {
                    Title = model.PlayerTeam,
                    NeedShirts = model.NeedShirts
                };

                teamRepo.Create(newTeam);

                newTeam.LeagueTeams.Add(new LeagueTeams
                {
                    LeagueId = league.LeagueId,
                    SeasonId = seasonId
                });
                teamRepo.Save();

                team = newTeam;

                var jobs = jobsRepo.GetByTeam(team.TeamId);

                var teamManagerJob = jobs.FirstOrDefault(x => x.JobsRole.RoleName == JobRole.TeamManager);

                if (teamManagerJob != null)
                {
                    var uJob = new UsersJob
                    {
                        JobId = teamManagerJob.JobId,
                        UserId = user.UserId,
                        SeasonId = seasonId,
                        TeamId = team.TeamId,
                        Active = true
                    };

                    jobsRepo.AddUsersJob(uJob);
                }
            }

            var newDataEntry = new ActivityFormsSubmittedData
            {
                ActivityId = activity.ActivityId,
                PlayerId = user.UserId,
                Comments = model.Comments,
                DateSubmitted = DateTime.Now,
                LeagueId = league.LeagueId,
                //NameForInvoice = model.NameForInvoice,
                PaymentByBenefactor = model.PaymentByBenefactor,
                IsPaymentByBenefactor = model.IsPaymentByBenefactor,
                TeamId = team?.TeamId,
                SelfInsurance = model.SelfInsurance,
                Document = SaveFormFile(activity.ActivityId, "document"),
                MedicalCert = SaveFormFile(activity.ActivityId, "medicalCert"),
                InsuranceCert = SaveFormFile(activity.ActivityId, "insuranceCert"),
                CustomFields = model.CustomFields,
                CustomPrices = customPrices.Any() ? JsonConvert.SerializeObject(customPrices) : null,

                RegistrationPrice = leagueRegistrationPrice?.Price ?? 0,

                RegistrationCardComProductId = leagueRegistrationPrice?.CardComProductId
            };

            var priceTotal = (leagueRegistrationPrice?.Price ?? 0) + customPrices.Sum(x => x.TotalPrice);

            #region CardCom payment for catchball union

            if (!model.IsPaymentByBenefactor && activity.UnionId == 15 &&
                priceTotal > 0)
            {
                var result = CardComHelper.UnionCustomTeam_Catchball(activity, user, team, customPrices, priceTotal,
                    leagueRegistrationPrice);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for catchball 66 union

            if (!model.IsPaymentByBenefactor && activity.UnionId == 66 &&
                priceTotal > 0)
            {
                var result = CardComHelper.UnionCustomTeam_Catchball66(activity, user, team, customPrices, priceTotal,
                    leagueRegistrationPrice);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for Rugby union

            if (!model.IsPaymentByBenefactor && activity.UnionId == 54 &&
                priceTotal > 0)
            {
                var result = CardComHelper.UnionCustomTeam_Rugby(activity, user, team, customPrices, priceTotal,
                    leagueRegistrationPrice);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion
            
            #region CardCom payment for wave surfing union

            if (activity.Union?.UnionId == 41 && priceTotal > 0)
            {
                var result = CardComHelper.UnionCustomTeam_WaveSurfing(activity, user, team, customPrices, priceTotal,
                    leagueRegistrationPrice);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for tennis union

            if (activity.UnionId == 38 && priceTotal > 0)
            {
                var result = CardComHelper.UnionCustomTeam_Tennis(activity, user, team, customPrices, priceTotal,
                    leagueRegistrationPrice);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            activity.ActivityFormsSubmittedDatas.Add(newDataEntry);

            activityRepo.Save();

            #region Payment by benefactor email

            if (model.IsPaymentByBenefactor)
            {
                var emailService = new EmailService();

                var hostBaseUrl = ConfigurationManager.AppSettings["SiteUrl"];
                var activitiesUrl = string.Format("{0}/Unions/Edit/{1}#activities",
                    hostBaseUrl,
                    activity.UnionId);

                string emailBody = $@"
{string.Format(Messages.Activity_BuildForm_Email_Header, team.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title, league.Name)}
<br />
{Messages.Activity_BuildForm_Email_BenefactorName}: {model.PaymentByBenefactor}<br />
{Messages.Activity_BuildForm_Email_Price}: {
                        league.LeaguesPrices.FirstOrDefault(
                                x => x.PriceType == (int?)LeaguePriceType.TeamRegistrationPrice &&
                                     x.StartDate <= newDataEntry.DateSubmitted &&
                                     x.EndDate >= newDataEntry.DateSubmitted)
                            ?.Price ?? 0
                    }<br />
<br /><br />
<a href=""{activitiesUrl}"">{Messages.Activity_BuildForm_Email_OpenActivities}</a>
";

                var managerToSendEmail = usersRepo.GetUnionWorkers(activity.UnionId ?? 0, JobRole.UnionManager)
                    .ToList();
                managerToSendEmail.AddRange(usersRepo.GetUnionWorkers(activity.UnionId ?? 0, JobRole.Activitymanager));
                foreach (var manager in managerToSendEmail)
                {
                    string email = manager.Email;

#if DEBUG
                    email = "info@loglig.com";
#endif

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        try
                        {
                            emailService.SendAsync(email, emailBody);
                        }
                        catch (Exception ex)
                        {
                            //log.Error(Messages.TeamBenefactor_SendEmailLogException, ex);
                        }
                    }
                }
            }

            #endregion

            #region UpdateUser

            if (!string.IsNullOrWhiteSpace(model.PlayerAddress))
            {
                user.Address = model.PlayerAddress;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerCity))
            {
                user.City = model.PlayerCity;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerEmail))
            {
                user.Email = model.PlayerEmail;
            }

            //if (!string.IsNullOrWhiteSpace(model.PlayerFullName))
            //{
            //    user.FullName = model.PlayerFullName;
            //}
            if (!string.IsNullOrWhiteSpace(model.PlayerFirstName))
            {
                user.FirstName = model.PlayerFirstName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerLastName))
            {
                user.LastName = model.PlayerLastName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerMiddleName))
            {
                user.MiddleName = model.PlayerMiddleName;
            }

            if (!string.IsNullOrWhiteSpace(model.PlayerPhone))
            {
                user.Telephone = model.PlayerPhone;
            }
            if (model.PlayerBirthDate.HasValue)
            {
                user.BirthDay = model.PlayerBirthDate.Value;
            }
            user.IsArchive = false;
            usersRepo.Save();

            #endregion

            if (!string.IsNullOrWhiteSpace(redirectToPaymentUrl))
                return Redirect(redirectToPaymentUrl);

            var paymentIdentifier = newDataEntry.CardComLpc ?? newDataEntry.LiqPayOrderId;

            return PartialView("ActivityFormPublishedSubmitResult", new ActivityFormSuccessResultModel { Activity = activity, PaymentIdentifier = paymentIdentifier});
        }

        private ActionResult SaveUnionClubForm(ActivityFormPublishSubmitModel model, Activity activity, User user, Club club,
            List<ActivityCustomPriceModel> customPrices)
        {
            string redirectToPaymentUrl = null;

            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }
            var seasonId = activity.SeasonId.Value;

            if (activity.DisableRegPaymentForExistingClubs && club != null)
            {
                customPrices = new List<ActivityCustomPriceModel>(); // drop all prices if registering existing club with disabled payments
            }

            if (user == null)
            {
                //create user
                user = new User
                {
                    Password = Protector.Encrypt("123abc12"),
                    TypeId = 4, //player
                    IdentNum = model.PlayerId,

                    //FullName = model.PlayerFullName,
                    FirstName = model.PlayerFirstName,
                    LastName = model.PlayerLastName,
                    MiddleName = model.PlayerMiddleName,

                    ParentName = model.PlayerParentName,
                    Email = model.PlayerEmail,
                    Telephone = model.PlayerPhone,
                    City = model.PlayerCity,
                    BirthDay = model.PlayerBirthDate,
                };
                usersRepo.Create(user);

                user = usersRepo.GetById(user.UserId);
            }

            int? clubId = club?.ClubId;
            if (club == null)
            {
                var newClub = new Club
                {
                    UnionId = activity.UnionId,
                    SeasonId = activity.SeasonId,
                    Name = model.ClubName,
                    NumberOfCourts = model.ClubNumberOfCourts ?? 0,
                    NGO_Number = model.ClubNgoNumber,
                    SportCenterId = model.ClubSportsCenter,
                    Address = model.ClubAddress,
                    ContactPhone = model.ClubPhone,
                    Email = model.ClubEmail,
                    CertificateOfIncorporation = SaveFormFile(activity.ActivityId, "certificateOfIncorporation", "clubs"),
                    ApprovalOfInsuranceCover = SaveFormFile(activity.ActivityId, "approvalOfInsuranceCover", "clubs"),
                    AuthorizedSignatories = SaveFormFile(activity.ActivityId, "authorizedSignatories", "clubs"),
                    RegionalId = model.Region
                };
                clubsRepo.Create(newClub);
                clubsRepo.Save();
                db.Entry(newClub).State = EntityState.Detached;
                newClub = clubsRepo.GetById(newClub.ClubId);

                var jobs = jobsRepo.GetClubJobs(newClub.ClubId);

                var clubManagerJob = jobs.FirstOrDefault(x => x.JobsRole.RoleName == JobRole.ClubManager || x.JobsRole.RoleName == JobRole.ClubSecretary);

                if (clubManagerJob != null)
                {
                    var uJob = new UsersJob
                    {
                        JobId = clubManagerJob.JobId,
                        UserId = user.UserId,
                        SeasonId = seasonId,
                        ClubId = newClub.ClubId,
                        Active = true
                    };

                    jobsRepo.AddUsersJob(uJob);
                }

                club = newClub;
            }
            else
            {
                club.Name = model.ClubName;
                club.NumberOfCourts = model.ClubNumberOfCourts ?? 0;
                club.NGO_Number = model.ClubNgoNumber;
                club.SportCenterId = model.ClubSportsCenter;
                club.Address = model.ClubAddress;
                club.ContactPhone = model.ClubPhone;
                club.Email = model.ClubEmail;
                club.CertificateOfIncorporation = SaveFormFile(activity.ActivityId, "certificateOfIncorporation", "clubs");
                club.ApprovalOfInsuranceCover = SaveFormFile(activity.ActivityId, "approvalOfInsuranceCover", "clubs");
                club.AuthorizedSignatories = SaveFormFile(activity.ActivityId, "authorizedSignatories", "clubs");
            }

            if (activity.CreateClubTeam)
            {
                var clubTeam = club.ClubTeams.FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                                                  (x.Team.Title == club.Name || x.Team.TeamsDetails
                                                                       .FirstOrDefault(t => t.SeasonId == activity.SeasonId)?.TeamName == club.Name));

                if (clubTeam == null)
                {
                    var newTeam = new Team
                    {
                        Title = club.Name
                    };

                    teamRepo.Create(newTeam);

                    club.ClubTeams.Add(new ClubTeam
                    {
                        SeasonId = activity.SeasonId ?? 0,
                        TeamId = newTeam.TeamId
                    });
                }
            }

            var newDataEntry = new ActivityFormsSubmittedData
            {
                ActivityId = activity.ActivityId,
                ClubId = club.ClubId,
                PlayerId = user.UserId,
                Comments = model.Comments,
                PaymentByBenefactor = model.PaymentByBenefactor,
                IsPaymentByBenefactor = model.IsPaymentByBenefactor,
                DateSubmitted = DateTime.Now,
                Document = SaveFormFile(activity.ActivityId, "document"),
                MedicalCert = SaveFormFile(activity.ActivityId, "medicalCert"),
                InsuranceCert = SaveFormFile(activity.ActivityId, "insuranceCert"),
                CustomFields = model.CustomFields,
                CustomPrices = customPrices.Any() ? JsonConvert.SerializeObject(customPrices) : null,
            };

            #region UpdateUser

            if (!string.IsNullOrWhiteSpace(model.PlayerCity))
            {
                user.City = model.PlayerCity;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerEmail))
            {
                user.Email = model.PlayerEmail;
            }

            //if (!string.IsNullOrWhiteSpace(model.PlayerFullName))
            //{
            //    user.FullName = model.PlayerFullName;
            //}
            if (!string.IsNullOrWhiteSpace(model.PlayerFirstName))
            {
                user.FirstName = model.PlayerFirstName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerLastName))
            {
                user.LastName = model.PlayerLastName;
            }
            if (!string.IsNullOrWhiteSpace(model.PlayerMiddleName))
            {
                user.MiddleName = model.PlayerMiddleName;
            }

            if (!string.IsNullOrWhiteSpace(model.PlayerPhone))
            {
                user.Telephone = model.PlayerPhone;
            }
            if (model.PlayerBirthDate.HasValue)
            {
                user.BirthDay = model.PlayerBirthDate.Value;
            }
            user.IsArchive = false;
            usersRepo.Save();

            #endregion

            var priceTotal = customPrices.Sum(x => x.TotalPrice);
            
            #region CardCom payment for catchball union

            if (!model.IsPaymentByBenefactor && activity.UnionId == 15 &&
                priceTotal > 0)
            {
                var result = CardComHelper.UnionClub_Catchball(activity, user, customPrices, priceTotal);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion
            
            #region CardCom payment for wave surfing union

            if (activity.Union?.UnionId == 41 && priceTotal > 0)
            {
                var result = CardComHelper.UnionClub_WaveSurfing(activity, user, customPrices, priceTotal);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for tennis union

            if (activity.UnionId == 38 && priceTotal > 0)
            {
                var result = CardComHelper.UnionClub_Tennis(activity, user, customPrices, priceTotal);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion
            
            #region LiqPay payment for UA gymnastic federation union

            if (activity.UnionId == GlobVars.UkraineGymnasticUnionId && priceTotal > 0)
            {
                var result = LiqPayHelper.UnionClub_UaGymnastics(activity, user, customPrices, priceTotal);

                newDataEntry.LiqPayOrderId = result?.OrderId; //TODO:
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for waterpolo union

            if (activity.UnionId == 32 && priceTotal > 0)
            {
                var result = CardComHelper.UnionClub_Waterpolo(activity, user, customPrices, priceTotal);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            #region CardCom payment for Climbing union 59

            if (!model.IsPaymentByBenefactor && activity.UnionId == 59 &&
                priceTotal > 0)
            {
                var result = CardComHelper.UnionClub_Climbing59(activity, user, customPrices, priceTotal);

                newDataEntry.CardComLpc = result?.Lpc;
                redirectToPaymentUrl = result?.RedirectUrl;
            }

            #endregion

            activity.ActivityFormsSubmittedDatas.Add(newDataEntry);

            activityRepo.Save();
            
            if (!string.IsNullOrWhiteSpace(redirectToPaymentUrl))
                return Redirect(redirectToPaymentUrl);

            var paymentIdentifier = newDataEntry.CardComLpc ?? newDataEntry.LiqPayOrderId;

            return PartialView("ActivityFormPublishedSubmitResult", new ActivityFormSuccessResultModel { Activity = activity, PaymentIdentifier = paymentIdentifier });
        }

        [AllowAnonymous]
        public ActionResult LiqPayPaymentSuccessful(Guid orderId)
        {
            var registrations = ActivityFormsRepo.GetRegistrationByLiqPayOrderId(orderId);

            return PartialView("ActivityFormPublishedSubmitResult",
                new ActivityFormSuccessResultModel
                {
                    Activity = registrations.FirstOrDefault()?.Activity,
                    PaymentIdentifier = orderId,
                    IsLiqPay = true
                });
        }

        [AllowAnonymous]
        public void LiqPayPaymentUpdate(LiqPayPaymentStatusUpdate model)
        {
            var correctSignature = LiqPayHelper.GetSignature(model.Data);

            if (model.Signature == correctSignature)
            {
                var decodedJson = Encoding.UTF8.GetString(Convert.FromBase64String(model.Data));
                var paymentData = JsonConvert.DeserializeObject<LiqPayPaymentStatusUpdateData>(decodedJson);

                db.LiqPayPaymentsNotifications.Add(new LiqPayPaymentsNotification
                {
                    OrderId = paymentData.OrderId,
                    Status = paymentData.Status,
                    JsonData = decodedJson,
                    DateCreated = DateTime.Now
                });
                db.SaveChanges();

                if (string.Equals(paymentData.Status, "sandbox", StringComparison.CurrentCultureIgnoreCase) ||
                    string.Equals(paymentData.Status, "success", StringComparison.CurrentCultureIgnoreCase) ||
                    string.Equals(paymentData.Status, "wait_accept", StringComparison.CurrentCultureIgnoreCase))
                {
                    var registrations = ActivityFormsRepo.GetRegistrationByLiqPayOrderId(paymentData.OrderId);

                    if (registrations.Any())
                    {
                        foreach (var registration in registrations)
                        {
                            var fullPrice = FillPaidValues(registration);

                            registration.LiqPayPaymentCompleted = true;
                            registration.LiqPayPaymentDate = DateTime.Now;
                            registration.Paid = fullPrice;

                            registration.PostponeParticipationPayment = false;
                        }

                        ActivityFormsRepo.Save();
                    }
                }
                else
                {
                    //TODO: payment failed for some reason, do something probably?
                }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult PaymentSuccessful(Guid lowProfileCode) //CardCom's notification
        {
            var registrations = ActivityFormsRepo.GetRegistrationByCardComLpc(lowProfileCode);

            if (!registrations.Any())// || registration.CardComPaymentCompleted || registration.CardComPaymentDate.HasValue)
            {
                return Content("Registrations not found");
            }

            return PartialView("ActivityFormPublishedSubmitResult",
                new ActivityFormSuccessResultModel
                {
                    Activity = registrations.FirstOrDefault()?.Activity,
                    PaymentIdentifier = lowProfileCode
                });
        }

        private decimal FillPaidValues(ActivityFormsSubmittedData registration)
        {
            decimal regPrice = 0;

            var regCustomPrices = !string.IsNullOrWhiteSpace(registration.CustomPrices)
                ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(registration.CustomPrices)
                : new List<ActivityCustomPriceModel>();
            foreach (var regCustomPrice in regCustomPrices)
            {
                regCustomPrice.Paid = regCustomPrice.TotalPrice;
            }
            registration.CustomPrices = JsonConvert.SerializeObject(regCustomPrices);

            switch (registration.Activity.GetFormType())
            {
                case ActivityFormType.TeamRegistration:
                    //regPrice = registration.RegistrationPrice;
                    regPrice = registration.RegistrationPrice;
                    registration.RegistrationPaid = regPrice;
                    break;

                case ActivityFormType.PlayerRegistration:
                    var playerRegPrice = registration.Activity.RegistrationPrice ? registration.RegistrationPrice : 0;
                    var playerInsurancePrice = registration.Activity.InsurancePrice ? registration.InsurancePrice : 0;

                    regPrice = playerRegPrice + playerInsurancePrice;
                    break;

                case ActivityFormType.UnionPlayerToClub:
                    registration.RegistrationPaid = registration.RegistrationPrice;
                    registration.InsurancePaid = registration.InsurancePrice;
                    registration.TenicardPaid = registration.TenicardPrice;

                    regPrice = registration.RegistrationPaid + registration.InsurancePaid + registration.TenicardPaid;

                    if (registration.TenicardPaid > 0)
                    {
                        registration.User.TenicardValidity = new DateTime(2020, 08, 31);
                    }
                    if (registration.InsurancePaid > 0)
                    {
                        registration.User.DateOfInsurance = new DateTime(DateTime.Now.Year, 12, 31);
                    }

                    break;

                case ActivityFormType.CustomGroup:
                    registration.RegistrationPaid = registration.RegistrationPrice;

                    regPrice = registration.RegistrationPrice;
                    break;

                case ActivityFormType.CustomPersonal:
                    registration.RegistrationPaid = registration.Activity.RegistrationPrice
                        ? registration.RegistrationPrice
                        : 0;
                    registration.InsurancePaid = registration.Activity.InsurancePrice
                        ? (registration.DisableInsurancePayment ? 0 : registration.InsurancePrice)
                        : 0;
                    registration.MembersFeePaid = registration.Activity.MembersFee
                        ? (registration.DisableMembersFeePayment ? 0 : registration.MembersFee)
                        : 0;
                    registration.HandlingFeePaid = registration.Activity.HandlingFee
                        ? (registration.DisableHandlingFeePayment ? 0 : registration.HandlingFee)
                        : 0;

                    regPrice = registration.RegistrationPaid + registration.InsurancePaid + registration.MembersFeePaid + registration.HandlingFeePaid;
                    break;

                case ActivityFormType.ClubPlayerRegistration:
                    registration.RegistrationPaid = registration.Activity.RegistrationPrice
                        ? registration.RegistrationPrice
                        : 0;
                    registration.InsurancePaid = registration.Activity.InsurancePrice
                        ? (registration.DisableInsurancePayment ? 0 : registration.InsurancePrice)
                        : 0;
                    registration.ParticipationPaid = registration.Activity.ParticipationPrice
                        ? (!registration.CardComPaymentCompleted && registration.PostponeParticipationPayment
                            ? 0
                            : registration.ParticipationPrice)
                        : 0;

                    regPrice = registration.RegistrationPaid + registration.InsurancePaid + registration.ParticipationPaid;
                    break;

                case ActivityFormType.ClubCustomPersonal:
                    registration.RegistrationPaid = registration.Activity.RegistrationPrice
                        ? registration.RegistrationPrice
                        : 0;
                    registration.InsurancePaid = registration.Activity.InsurancePrice
                        ? (registration.DisableInsurancePayment ? 0 : registration.InsurancePrice)
                        : 0;
                    registration.ParticipationPaid = registration.Activity.ParticipationPrice
                        ? registration.ParticipationPrice
                        : 0;

                    regPrice = registration.RegistrationPaid + registration.InsurancePaid + registration.ParticipationPaid;
                    break;

                case ActivityFormType.DepartmentClubPlayerRegistration:
                    registration.RegistrationPaid = registration.Activity.RegistrationPrice
                        ? registration.RegistrationPrice
                        : 0;
                    registration.InsurancePaid = registration.Activity.InsurancePrice
                        ? (registration.DisableInsurancePayment ? 0 : registration.InsurancePrice)
                        : 0;
                    registration.ParticipationPaid = registration.Activity.ParticipationPrice
                        ? registration.ParticipationPrice
                        : 0;

                    regPrice = registration.RegistrationPaid + registration.InsurancePaid + registration.ParticipationPaid;
                    break;

                case ActivityFormType.DepartmentClubCustomPersonal:
                    registration.RegistrationPaid = registration.Activity.RegistrationPrice
                        ? registration.RegistrationPrice
                        : 0;
                    registration.InsurancePaid = registration.Activity.InsurancePrice
                        ? (registration.DisableInsurancePayment ? 0 : registration.InsurancePrice)
                        : 0;
                    registration.ParticipationPaid = registration.Activity.ParticipationPrice
                        ? registration.ParticipationPrice
                        : 0;

                    regPrice = registration.RegistrationPaid + registration.InsurancePaid + registration.ParticipationPaid;
                    break;

                case ActivityFormType.ClubTeamRegistration:
                    break;
                case ActivityFormType.ClubCustomGroup:
                    break;
                case ActivityFormType.UnionClub:
                    break;

                default:
                    throw new Exception($"Unknown form type for activity {registration.Activity.ActivityId}");
            }

            return regPrice;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult PaymentFailure(Guid lowProfileCode)
        {
            return PartialView("PaymentFailure");
        }

        private string SaveFormFile(int activityId, string requestFileName, string path = "publishedactivityforms")
        {
            var file = Request.Files[requestFileName];

            if (!ValidateFileSize(file))
            {
                return null;
            }

            if (!ValidateFileType(file))
            {
                return null;
            }

            var ext = Path.GetExtension(file.FileName)?.ToLower();

            var newName = $"Activity_{activityId}_{AppFunc.GetUniqName()}{ext}";

            var savePath = Server.MapPath(GlobVars.ContentPath + $"/{path}/");

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

        private bool ValidateFileSize(HttpPostedFileBase file)
        {
            var maxFileSize = GlobVars.MaxFileSize * 1000;
            if (file == null || file.ContentLength > maxFileSize)
            {
                return false;
            }

            return true;
        }

        private bool ValidateFileType(HttpPostedFileBase file)
        {
            var ext = Path.GetExtension(file.FileName)?.ToLower();

            if (!GlobVars.ValidImages.Contains(ext) &&
                !string.Equals(ext, ".xls", StringComparison.CurrentCultureIgnoreCase) &&
                !string.Equals(ext, ".xlsx", StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        [HttpGet]
        [AllowAnonymous]
        [OutputCache(Duration = 0, NoStore = true)]
        public ActionResult Form(int id, string lang)
        {
            var activity = activityRepo.GetById(id);

            if (activity == null)
            {
                return new EmptyResult();
            }

            if (!activity.IsPublished || !activity.ActivityForms.Any())
            {
                return View("ActivityFormBlank", activity);
            }

            var form = activity.ActivityForms.First();

            lang = lang ?? activity.DefaultLanguage;
            var culture = LocaleHelper.GetCultureByLanguge(lang);
            CookiesHelper.SetCookie(LocaleHelper.LocaleCookieName, LocaleHelper.GetLocale(culture), DateTime.Now.AddDays(1));
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(LocaleHelper.GetLocale(culture));
            
            return View("ActivityFormPublished",
                new ActivityFormPublish
                {
                    ActivityId = activity.ActivityId,
                    UnionId = activity.UnionId,

                    FormName = form.Name,
                    FormDescription = form.Description,
                    ActivityEndDate = activity.EndDate,
                    Image = form.ImageFile,
                    Body = BuildForm(activity, form.ActivityFormsDetails, true, culture),
                    FormType = activity.GetFormType(),
                    RestrictCustomPricesToOne = activity.RestrictCustomPricesToOneItem,
                    DoNotRestrictCustomPrices = activity.AllowNoCustomPricesSelected,
                    AlternativePlayerIdentity = activity.UnionId == GlobVars.UkraineGymnasticUnionId ||
                                                activity.Club?.UnionId == GlobVars.UkraineGymnasticUnionId,
                    ForbidToChangeNameForExistingPlayers = activity.ForbidToChangeNameForExistingPlayers,
                    Culture = culture
                });
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetEscortDiscount(int activityId, string playerId)
        {
            var activity = activityRepo.GetById(activityId);
            var user = usersRepo.GetByIdentityNumber(playerId);

            return null;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetSchoolTeamsByAge(int? schoolId, string date, int activityId, string idNum, string brotherIdNum, DateTime? startDateForPriceAdjustment)
        {
            if (!schoolId.HasValue)
            {
                return new EmptyResult();
            }

            DateTime birthDate;
            if (!DateTime.TryParse(date, out birthDate))
            {
                return Json(new { error = Messages.Activity_GetSchoolTeamsByAge_UnknownDate },
                    JsonRequestBehavior.AllowGet);
            }

            var activity = activityRepo.GetById(activityId);

            var user = usersRepo.GetByIdentityNumber(idNum);

            var school = SchoolRepo.GetCollection(x => x.Id == schoolId).FirstOrDefault();

            if (activity == null)
            {
                return Json(new { error = Messages.Activity_NotFound }, JsonRequestBehavior.AllowGet);
            }

            if (school == null)
            {
                return Json(new { error = Messages.Activity_SchoolNotFound }, JsonRequestBehavior.AllowGet);
            }

            var playerPriceHelper = new PlayerPriceHelper();

            var teams = school.SchoolTeams
                .OrderBy(x => x.Team.SortOrder)
                .Where(x => (x.Team.MinimumAge >= birthDate || x.Team.MinimumAge == null) &&
                            (x.Team.MaximumAge <= birthDate || x.Team.MaximumAge == null))
                .Select(x =>
                {
                    var prices =
                        playerPriceHelper.GetTeamPlayerPrice(user?.UserId ?? 0, x.TeamId, school.ClubId,
                            school.SeasonId, activity,
                            brotherIdNum: brotherIdNum,
                            startDateForPriceAdjustment: startDateForPriceAdjustment);

                    return new ActivityFormGetClubPlayerUserTeamModel
                    {
                        TeamId = x.TeamId,
                        TeamName = HttpUtility.HtmlEncode(
                            x.Team.TeamsDetails.FirstOrDefault(t => t.SeasonId == school.SeasonId)?.TeamName ??
                            x.Team.Title),
                        RegistrationAndEquipmentPrice = prices.PlayerRegistrationAndEquipmentPrice,
                        ParticipationPrice = prices.ParticipationPrice,
                        InsurancePrice = prices.PlayerInsurancePrice
                    };
                })
                .ToList();

            var selectedTeams = !string.IsNullOrWhiteSpace(activity.RestrictedTeams)
                ? JsonConvert.DeserializeObject<List<int>>(activity.RestrictedTeams)
                : new List<int>();

            if (activity.RestrictTeams)
            {
                teams = teams.Where(x => selectedTeams.Contains(x.TeamId)).ToList();
            }

            if (!teams.Any())
            {
                return Json(new { error = Messages.Activity_NoSuitableTeamsSchool }, JsonRequestBehavior.AllowGet);
            }

            return Json(
                new ActivityFormGetClubPlayerUserModel
                {
                    MultiTeamEnabled = activity.MultiTeamRegistrations,
                    Teams = teams
                }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSelectTeamsDropdown(int activityId, int[] schools)
        {
            var result = new List<ActivitySelectTeamsDropdown>();

            var activity = activityRepo.GetById(activityId);

            if (activity != null)
            {
                var restrictedTeams = !string.IsNullOrWhiteSpace(activity.RestrictedTeams)
                    ? JsonConvert.DeserializeObject<List<int>>(activity.RestrictedTeams)
                    : new List<int>();

                if (activity.UnionId == null && activity.ClubId != null)
                {
                    var clubLeagueTeams =
                        db.ClubTeams
                            .Include(x => x.Club)
                            .Include(x => x.Team)
                            .Include(x => x.Team.TeamsDetails)
                            .Where(x => x.SeasonId == activity.SeasonId && x.ClubId == activity.ClubId)
                            .ToList();
                    var clubTeamsGrouped = clubLeagueTeams.GroupBy(x => x.Club.Name).ToList();

                    result.AddRange(clubTeamsGrouped.Select(x => new ActivitySelectTeamsDropdown
                    {
                        label = $"{Messages.Club} - {x.Key}",
                        children = x.Select(ct => new ActivitySelectTeamsDropdownItem
                            {
                                label = ct.Team.TeamsDetails.FirstOrDefault(td => td.SeasonId == activity.SeasonId)
                                            ?.TeamName
                                        ?? ct.Team.Title,
                                value = ct.TeamId.ToString(),
                                selected = restrictedTeams.Contains(ct.TeamId)
                            })
                            .ToList()
                    }));

                    if (activity.ClubLeagueTeamsOnly)
                    {
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }

                    var schoolTeams = db.SchoolTeams
                        .Include(x => x.School)
                        .Include(x => x.Team)
                        .Include(x => x.Team.TeamsDetails)
                        .Where(x => x.School.SeasonId == activity.SeasonId &&
                                    x.School.ClubId == activity.ClubId)
                        .ToList();

                    if (schools?.Any() == true)
                    {
                        schoolTeams = schoolTeams.Where(x => schools.Contains(x.SchoolId)).ToList();
                    }

                    var schoolTeamsGrouped = schoolTeams.GroupBy(x => x.School.Name).ToList();

                    result.AddRange(schoolTeamsGrouped.Select(x => new ActivitySelectTeamsDropdown
                    {
                        label = $"{Messages.School} - {x.Key}",
                        children = x.Select(st => new ActivitySelectTeamsDropdownItem
                        {
                            label = st.Team.TeamsDetails.FirstOrDefault(td => td.SeasonId == activity.SeasonId)?.TeamName 
                                    ?? st.Team.Title,
                            value = st.TeamId.ToString(),
                            selected = restrictedTeams.Contains(st.TeamId)
                        })
                        .ToList()
                    }));
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetAllAvailableTeams(int activityId, string playerId, bool onlyLeagues, string birthDate)
        {
            var activity = activityRepo.GetById(activityId);
            var user = usersRepo.GetByIdentityNumber(playerId);

            DateTime? playerBirthDate = user?.BirthDay;
            DateTime parsedBirthDate;
            if (playerBirthDate == null && DateTime.TryParse(birthDate, out parsedBirthDate))
            {
                playerBirthDate = parsedBirthDate;
            }

            var response = new ActivityFormGetUnionCustomPersonalPlayerUserModel
            {
                PlayerExist = user != null,
                Teams = new List<ActivityFormGetUnionCustomPersonalPlayerUserTeamModel>(),
                Leagues = new List<ActivityFormGetUnionCustomPersonalPlayerLeagueModel>()
            };

            if (activity.NoTeamRegistration)
            {
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            var pricesHelper = new PlayerPriceHelper();

            var selectedLeagues = !string.IsNullOrWhiteSpace(activity.RestrictedLeagues)
                ? JsonConvert.DeserializeObject<List<int>>(activity.RestrictedLeagues)
                : new List<int>();

            List<League> leagues;
            if (activity.RestrictLeagues)
            {
                leagues = activity.Union.Leagues
                    .Where(x => x.SeasonId == activity.SeasonId && selectedLeagues.Contains(x.LeagueId))
                    .ToList();
            }
            else
            {
                leagues = activity.Union.Leagues.Where(x => x.SeasonId == activity.SeasonId).ToList();
            }

            if (activity.RestrictGenders)
            {
                leagues = leagues.Where(x => x.GenderId == user?.GenderId || x.Gender.Title == "All").ToList();
                if (!leagues.Any())
                {
                    return Json(new {error = Messages.Activity_NoLeagueForGender}, JsonRequestBehavior.AllowGet);
                }
            }

            if (activity.RestrictLeaguesByAge)
            {
                leagues = leagues.Where(x =>
                        (x.MinimumAge >= (user?.BirthDay ?? playerBirthDate) || x.MinimumAge == null) &&
                        (x.MaximumAge <= (user?.BirthDay ?? playerBirthDate) || x.MaximumAge == null))
                    .ToList();

                if (!leagues.Any())
                {
                    return Json(new { error = Messages.Activity_NoLeaguesForAge}, JsonRequestBehavior.AllowGet);
                }
            }

            //if (activity.CheckCompetitionAge && activity.Union?.CompetitionAges?.Any() == true)
            //{
            //    if (!activity.Union.CompetitionAges.Any(x => x.from_birth <= user.BirthDay && x.to_birth >= user.BirthDay && x.SeasonId == activity.SeasonId))
            //    {
            //        return Json(new { error = Messages.Activity_Competition_AgeMismatch },
            //            JsonRequestBehavior.AllowGet);
            //    }
            //}

            var approvedTeams = new List<ActivityFormsSubmittedData>();
            if (activity.ShowOnlyApprovedTeams)
            {
                approvedTeams = activityRepo.GetCollection<ActivityFormsSubmittedData>(
                        x => (x.Activity == null || x.Activity.IsAutomatic == false) &&
                             x.Activity.Type == ActivityType.Group &&
                             x.Activity.SeasonId == activity
                                 .SeasonId &&
                             x.Activity.UnionId == activity
                                 .UnionId &&
                             x.Activity.ClubId == activity
                                 .ClubId &&
                             x.IsActive)
                    .ToList();
            }

            foreach (var league in leagues)
            {
                if (onlyLeagues)
                {
                    response.Leagues.Add(new ActivityFormGetUnionCustomPersonalPlayerLeagueModel
                    {
                        LeagueId = league.LeagueId,
                        LeagueName = league.Name
                    });
                }
                else
                {
                    foreach (var leagueTeam in league.LeagueTeams.ToList())
                    {
                        var team = leagueTeam.Teams;

                        if (activity.ShowOnlyApprovedTeams && !approvedTeams.Any(x => x.TeamId == team?.TeamId && x.LeagueId == league?.LeagueId))
                        {
                            continue;
                        }

                        if (!activity.ActivityFormsSubmittedDatas.Any(
                            x => x.PlayerId == user?.UserId && x.LeagueId == league.LeagueId &&
                                 x.TeamId == team.TeamId))
                        {
                            LeaguePlayerPrice prices = null;

                            var playerRegistrationsInActivities = user?.ActivityFormsSubmittedDatas.Where(
                                                                      x => //x.Activity.IsAutomatic == true &&
                                                                          x.Activity.Type == ActivityType.Personal &&
                                                                          x.Activity.SeasonId == activity.SeasonId &&
                                                                          x.Activity.UnionId == activity.UnionId &&
                                                                          x.Activity.ClubId == activity.ClubId &&
                                                                          x.LeagueId != null)
                                                                  ?? new List<ActivityFormsSubmittedData>();

                            var isPlayerAlreadyRegisteredInBuiltInActivity =
                                playerRegistrationsInActivities.Any(x => x.Activity.IsAutomatic == true);

                            if (!activity.NoPriceOnBuiltInRegistration || !isPlayerAlreadyRegisteredInBuiltInActivity)
                            {
                                prices = pricesHelper.GetPlayerPrices(user?.UserId ?? 0, team.TeamId, league.LeagueId,
                                    activity.SeasonId ?? 0);
                            }

                            if (prices != null)
                            {
                                if (!activity.RegistrationPrice)
                                {
                                    prices.RegistrationPrice = 0m;
                                }
                                if (!activity.InsurancePrice || (playerRegistrationsInActivities.Any(x => x.IsActive) && activity.UnionApprovedPlayerNoInsurance))
                                {
                                    prices.InsurancePrice = 0m;
                                }
                                if (!activity.MembersFee)
                                {
                                    prices.MembersFee = 0m;
                                }
                                if (!activity.HandlingFee)
                                {
                                    prices.HandlingFee = 0m;
                                }
                           }

                            response.Teams.Add(new ActivityFormGetUnionCustomPersonalPlayerUserTeamModel
                            {
                                TeamId = team.TeamId,
                                TeamName = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == activity.SeasonId)
                                               ?.TeamName ??
                                           team.Title,
                                League = new ActivityFormGetUnionCustomPersonalPlayerLeagueModel
                                {
                                    LeagueId = league.LeagueId,
                                    LeagueName = league.Name
                                },

                                RegistrationPrice = prices?.RegistrationPrice ?? 0,
                                InsurancePrice = prices?.InsurancePrice ?? 0,
                                MembersFee = prices?.MembersFee ?? 0,
                                HandlingFee = prices?.HandlingFee ?? 0
                            });
                        }
                    }
                }
            }

            response.Leagues = response.Leagues?.OrderBy(x => x.LeagueName).ToList();
            response.Teams = response.Teams?.OrderBy(x => x.TeamName).ToList();

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetLeagueTeams(int leagueId, int activityId, string playerId)
        {
            var league = leagueRepo.GetById(leagueId);
            var activity = activityRepo.GetById(activityId);
            var user = usersRepo.GetByIdentityNumber(playerId);

            var response = new ActivityFormGetUnionCustomPersonalPlayerUserModel
            {
                PlayerExist = user != null,
                Teams = new List<ActivityFormGetUnionCustomPersonalPlayerUserTeamModel>()
            };

            var pricesHelper = new PlayerPriceHelper();

            var approvedTeams = new List<ActivityFormsSubmittedData>();
            if (activity.ShowOnlyApprovedTeams)
            {
                approvedTeams = activityRepo.GetCollection<ActivityFormsSubmittedData>(
                        x => (x.Activity.IsAutomatic == null || x.Activity.IsAutomatic == false) &&
                             x.Activity.Type == ActivityType.Group &&
                             x.Activity.SeasonId == activity
                                 .SeasonId &&
                             x.Activity.UnionId == activity
                                 .UnionId &&
                             x.Activity.ClubId == activity
                                 .ClubId &&
                             x.IsActive)
                    .ToList();
            }

            foreach (var leagueTeam in league.LeagueTeams)
            {
                var team = leagueTeam.Teams;

                if (activity.ShowOnlyApprovedTeams && !approvedTeams.Any(x => x.TeamId == team?.TeamId && x.LeagueId == league?.LeagueId))
                {
                    continue;
                }

                if (activity.RestrictGendersByTeam && user.GenderId != team.CompetitionAge?.gender)
                {
                    continue;
                }

                if (activity.CheckCompetitionAge &&
                    (user.BirthDay < team.CompetitionAge?.from_birth || user.BirthDay > team.CompetitionAge?.to_birth))
                {
                    continue;
                }

                LeaguePlayerPrice prices = null;

                var playerRegistrationsInActivities = user?.ActivityFormsSubmittedDatas.Where(
                                                          x => //x.Activity.IsAutomatic == true &&
                                                              x.Activity.Type == ActivityType.Personal &&
                                                              x.Activity.SeasonId == activity.SeasonId &&
                                                              x.Activity.UnionId == activity.UnionId &&
                                                              x.Activity.ClubId == activity.ClubId &&
                                                              x.LeagueId != null)
                                                      ?? new List<ActivityFormsSubmittedData>();

                var isPlayerAlreadyRegisteredInBuiltInActivity = playerRegistrationsInActivities.Any(x => x.Activity.IsAutomatic == true);

                if (!activity.NoPriceOnBuiltInRegistration || !isPlayerAlreadyRegisteredInBuiltInActivity)
                {
                    prices = pricesHelper.GetPlayerPrices(user?.UserId ?? 0, team.TeamId, league.LeagueId,
                        activity.SeasonId ?? 0);
                }

                if (prices != null)
                {
                    if (!activity.RegistrationPrice)
                    {
                        prices.RegistrationPrice = 0m;
                    }
                    if (!activity.InsurancePrice || (playerRegistrationsInActivities.Any(x => x.IsActive) && activity.UnionApprovedPlayerNoInsurance))
                    {
                        prices.InsurancePrice = 0m;
                    }
                    if (!activity.MembersFee)
                    {
                        prices.MembersFee = 0m;
                    }
                    if (!activity.HandlingFee)
                    {
                        prices.HandlingFee = 0m;
                    }
                }

                response.Teams.Add(new ActivityFormGetUnionCustomPersonalPlayerUserTeamModel
                {
                    TeamId = team.TeamId,
                    TeamName = team.TeamsDetails.FirstOrDefault(t => t.SeasonId == activity.SeasonId)
                                   ?.TeamName ??
                               team.Title,
                    League = new ActivityFormGetUnionCustomPersonalPlayerLeagueModel
                    {
                        LeagueId = league.LeagueId,
                        LeagueName = league.Name
                    },

                    RegistrationPrice = prices?.RegistrationPrice ?? 0,
                    InsurancePrice = prices?.InsurancePrice ?? 0,
                    MembersFee = prices?.MembersFee ?? 0,
                    HandlingFee = prices?.HandlingFee ?? 0
                });
            }

            if (response.Teams?.Any() != true)
            {
                return Json(new { error = Messages.Activity_LeagueTeams_NoSuitableTeams }, JsonRequestBehavior.AllowGet);
            }

            response.Teams = response.Teams?.OrderBy(x => x.TeamName).ThenBy(x => x.League.LeagueName).ToList();

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetUnionCustomPersonalPlayerUser(string idNum, int activityId)
        {
            var activity = activityRepo.GetById(activityId);
            var user = usersRepo.GetByIdentityNumber(idNum);

            if (activity == null) return Json(new { error = Messages.Activity_NotFound }, JsonRequestBehavior.AllowGet);

            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }

            var restartCheck = CheckPaymentRestartNeeded(activity, user?.UserId);
            if (restartCheck.Item1)
            {
                return restartCheck.Item2;
            }

            if (activity.RegistrationForMembers == (int)RegistrationForMembers.OnlyCompetitive && user?.IsCompetitiveMember != true)
            {
                return Json(new { error = Messages.Activity_ErrorOnlyCompetitiveMembersAllowed }, JsonRequestBehavior.AllowGet);
            }
            if (activity.RegistrationForMembers == (int)RegistrationForMembers.OnlyRegular && user?.IsCompetitiveMember != false)
            {
                return Json(new { error = Messages.Activity_ErrorOnlyRegularMembersAllowed }, JsonRequestBehavior.AllowGet);
            }

            if (activity.AllowOnlyApprovedMembers)
            {
                if (user == null)
                {
                    return Json(new {error = Messages.Activity_OnlyApprovedMembersAllowed},
                        JsonRequestBehavior.AllowGet);
                }
                else if (activity.RegistrationsByCompetitionsCategory)
                {
                    var teamPlayer = playersRepo.GetTeamPlayerByUserIdAndSeasonId(user.UserId, activity.SeasonId ?? 0);

                    if (teamPlayer?.IsActive != true || teamPlayer?.IsApprovedByManager != true)
                    {
                        return Json(new { error = Messages.Activity_OnlyApprovedMembersAllowed },
                            JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    var playerRegistrations = user.ActivityFormsSubmittedDatas
                        .OrderBy(x => x.ActivityId)
                        .Where(
                            x => (x.LeagueId != null || x.ClubId != null) && x.TeamId != null &&
                                 x.Activity.SeasonId == activity.SeasonId &&
                                 (x.Activity.Type == ActivityType.Personal ||
                                  x.Activity.Type == ActivityType.UnionPlayerToClub) &&
                                 x.IsActive);
                    if (
                        (playerRegistrations.FirstOrDefault(x => x.Activity.IsAutomatic == true) ??
                         playerRegistrations.FirstOrDefault(x =>
                             x.Activity.IsAutomatic == false || x.Activity.IsAutomatic == null)
                        ) == null
                    )
                    {
                        if (activity.UnionId == 38) //Tennis section: check approved in teamplayers
                        {
                            var teamPlayer = user.TeamsPlayers
                                .OrderByDescending(x => x.Id)
                                .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                                     x.IsActive);
                            if (teamPlayer?.IsApprovedByManager != true)
                            {
                                return Json(new {error = Messages.Activity_OnlyApprovedMembersAllowed_Tennis},
                                    JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json(new {error = Messages.Activity_OnlyApprovedMembersAllowed},
                                JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            if (user == null)
            {
                if (activity.RegistrationsByCompetitionsCategory)
                {
                    return Json(new { error = Messages.Activity_PlayerNotFoundOrInactive },
                        JsonRequestBehavior.AllowGet);
                }

                return Json(new ActivityFormGetUnionCustomPersonalPlayerUserModel
                {
                    EscortPlayerCustomPricesDiscount = activity.EscortDiscount ?? 0m,
                    EscortNoInsurance = activity.EscortNoInsurance,
                    RestrictLeaguesByAge = activity.RestrictLeaguesByAge
                }, JsonRequestBehavior.AllowGet);
            }

            if (activity.DoNotAllowDuplicateRegistrations)
            {
                if (activity.ActivityFormsSubmittedDatas.Any(x => x.PlayerId == user.UserId))
                {
                    return Json(new { error = Messages.Activity_AlreadyRegistered },
                        JsonRequestBehavior.AllowGet);
                }
            }

            var profilePicture = user.PlayerFiles
                                     .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                                          x.FileType == (int)PlayerFileType.PlayerImage)
                                     ?.FileName ?? user.Image;
            var medicalCert = user.PlayerFiles
                .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                     x.FileType == (int)PlayerFileType.MedicalCertificate && !x.IsArchive)
                ?.FileName;
            var insuranceCert = user.PlayerFiles
                .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                     x.FileType == (int)PlayerFileType.Insurance)
                ?.FileName;

            var idFile = user.PlayerFiles
                             .FirstOrDefault(f => f.SeasonId == activity.SeasonId &&
                                                  f.FileType == (int) PlayerFileType.IDFile)
                             ?.FileName ?? user.IDFile;

            var playerRegistrationInBuiltInActivity = user?.ActivityFormsSubmittedDatas.FirstOrDefault(
                x => x.Activity.IsAutomatic == true &&
                     x.Activity.Type == ActivityType.Personal &&
                     x.Activity.SeasonId == activity.SeasonId &&
                     x.Activity.UnionId == activity.UnionId &&
                     x.Activity.ClubId == activity.ClubId &&
                     x.LeagueId != null);

            if (playerRegistrationInBuiltInActivity == null) //if built-in is not found, get first custom
            {
                playerRegistrationInBuiltInActivity = user?.ActivityFormsSubmittedDatas.FirstOrDefault(
                    x => x.Activity.IsAutomatic != true &&
                         x.Activity.Type == ActivityType.Personal &&
                         x.Activity.SeasonId == activity.SeasonId &&
                         x.Activity.UnionId == activity.UnionId &&
                         x.Activity.ClubId == activity.ClubId &&
                         x.LeagueId != null);
            }

            var response = new ActivityFormGetUnionCustomPersonalPlayerUserModel
            {
                Id = user.UserId,
                PlayerExist = true,
                ApprovedPlayerCustomPricesDiscount = playerRegistrationInBuiltInActivity?.IsActive == true ? (activity.UnionApprovedPlayerDiscount ?? 0m) : 0m,
                EscortPlayerCustomPricesDiscount = activity.EscortDiscount ?? 0m,
                EscortNoInsurance = activity.EscortNoInsurance,
                RestrictLeaguesByAge = activity.RestrictLeaguesByAge,
                IdNum = user.IdentNum,
                Email = user.Email,

                FullName = user.FullName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,

                Gender = user.GenderId,
                ParentName = user.ParentName,
                Address = user.Address,
                City = user.City,
                BirthDate = user.BirthDay?.ToString("d"),
                Phone = user.Telephone,
                ProfilePicture = !string.IsNullOrWhiteSpace(profilePicture)
                    ? $"{GlobVars.ContentPath}/players/{profilePicture}"
                    : null,
                MedicalCert = !string.IsNullOrWhiteSpace(medicalCert)
                    ? $"{GlobVars.ContentPath}/players/{medicalCert}"
                    : null,
                InsuranceCert = !string.IsNullOrWhiteSpace(insuranceCert)
                    ? $"{GlobVars.ContentPath}/players/{insuranceCert}"
                    : null,
                IdFile = !string.IsNullOrWhiteSpace(idFile)
                    ? $"{GlobVars.ContentPath}/players/{idFile}"
                    : null
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetClubPlayerUser(string idNum, int activityId, string birthDate, string brotherIdNum,
            DateTime? startDateForPriceAdjustment)
        {
            var activity = activityRepo.GetById(activityId);
            var user = usersRepo.GetByIdentityNumber(idNum);

            if (activity == null) return Json(new {error = Messages.Activity_NotFound}, JsonRequestBehavior.AllowGet);

            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }

            var restartCheck = CheckPaymentRestartNeeded(activity, user?.UserId);
            if (restartCheck.Item1)
            {
                return restartCheck.Item2;
            }

            if (activity.DoNotAllowDuplicateRegistrations)
            {
                if (activity.ActivityFormsSubmittedDatas.Any(x => x.PlayerId == user?.UserId))
                {
                    return Json(new {error = Messages.Activity_AlreadyRegistered},
                        JsonRequestBehavior.AllowGet);
                }
            }

            DateTime? playerBirthDate = user?.BirthDay;
            DateTime parsedBirthDate;
            if (playerBirthDate == null && DateTime.TryParse(birthDate, out parsedBirthDate))
            {
                playerBirthDate = parsedBirthDate;
            }

            var selectedSchools = !string.IsNullOrWhiteSpace(activity.RestrictedSchools)
                ? JsonConvert.DeserializeObject<List<int>>(activity.RestrictedSchools)
                : new List<int>();

            var selectedTeams = !string.IsNullOrWhiteSpace(activity.RestrictedTeams)
                ? JsonConvert.DeserializeObject<List<int>>(activity.RestrictedTeams)
                : new List<int>();

            List<School> schools;
            if (activity.RestrictSchools)
            {
                schools = activity.Club.Schools
                    .Where(x => x.SeasonId == activity.SeasonId && selectedSchools.Contains(x.Id))
                    .ToList();
            }
            else
            {
                schools = activity.Club.Schools.Where(x => x.SeasonId == activity.SeasonId).ToList();
            }

            var schoolsModel = schools
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => new ActivityFormGetClubPlayerUserSchoolModel
                {
                    SchoolId = x.Id,
                    SchoolName = x.Name
                })
                .ToList();

            if (user == null && !activity.ClubLeagueTeamsOnly)
            {
                var schoolsResponse = new ActivityFormGetClubPlayerUserModel
                {
                    Schools = schoolsModel
                };

                return Json(schoolsResponse, JsonRequestBehavior.AllowGet);
            }

            var isPlayerSecondRegistration =
                activity.ActivityFormsSubmittedDatas.Count(x => x.PlayerId == user?.UserId) == 1;

            var clubTeams = activity.Club.ClubTeams.Where(x => x.SeasonId == activity.SeasonId)
                .Select(x => x.TeamId)
                .ToList();
            List<SchoolTeam> clubSchoolTeams = new List<SchoolTeam>();

            foreach (var schoolTeams in schools.Select(x => x.SchoolTeams))
            {
                foreach (var schoolTeam in schoolTeams)
                {
                    clubTeams.Add(schoolTeam.TeamId);

                    clubSchoolTeams.Add(schoolTeam);
                }
            }

            if (activity.RestrictTeams)
            {
                clubTeams = clubTeams.Where(x => selectedTeams.Contains(x)).ToList();
                clubSchoolTeams = clubSchoolTeams.Where(x => selectedTeams.Contains(x.TeamId)).ToList();
            }

            var playerTeams = user?.TeamsPlayers.Where(x => x.SeasonId == activity.SeasonId &&
                                                            clubTeams.Contains(x.TeamId) &&
                                                            !activity.ActivityFormsSubmittedDatas.Where(
                                                                    s => s.PlayerId == user.UserId &&
                                                                         s.TeamId == x.TeamId)
                                                                .Select(s => s.TeamId)
                                                                .Contains(x.TeamId))
                .ToList();

            var playerPriceHelper = new PlayerPriceHelper();

            var teams = new List<ActivityFormGetClubPlayerUserTeamModel>();

            if (!activity.ClubLeagueTeamsOnly && playerTeams != null)
            {
                teams.AddRange(playerTeams.Select(x =>
                {
                    var prices =
                        playerPriceHelper.GetTeamPlayerPrice(user.UserId, x.TeamId, activity.ClubId ?? 0,
                            activity.SeasonId ?? 0, activity,
                            brotherIdNum: brotherIdNum,
                            startDateForPriceAdjustment: startDateForPriceAdjustment);

                    if (isPlayerSecondRegistration)
                    {
                        prices.PlayerInsurancePrice =
                            activity.SecondTeamNoInsurance
                                ? 0
                                : prices.PlayerInsurancePrice;

                        prices.PlayerRegistrationAndEquipmentPrice =
                            Math.Max(0,
                                prices.PlayerRegistrationAndEquipmentPrice - (activity.SecondTeamDiscount ?? 0));
                    }

                    return new ActivityFormGetClubPlayerUserTeamModel
                    {
                        TeamId = x.TeamId,
                        TeamName = HttpUtility.HtmlEncode(x.Team.TeamsDetails
                                                              .FirstOrDefault(t => t.SeasonId == activity.SeasonId)
                                                              ?.TeamName ??
                                                          x.Team.Title),
                        SchoolId = clubSchoolTeams.FirstOrDefault(s => s.TeamId == x.TeamId)?.SchoolId,

                        RegistrationAndEquipmentPrice = prices.PlayerRegistrationAndEquipmentPrice,
                        ParticipationPrice = prices.ParticipationPrice,
                        InsurancePrice = prices.PlayerInsurancePrice
                    };
                }));
            }
            else
            {
                var clubLeagueTeams = activity.Club.ClubTeams
                    .Where(x =>
                        x.SeasonId == activity.SeasonId)
                    .ToList();

                clubLeagueTeams = clubLeagueTeams.Where(x =>
                        (x.Team.MinimumAge >= (user?.BirthDay ?? playerBirthDate) || x.Team.MinimumAge == null) &&
                        (x.Team.MaximumAge <= (user?.BirthDay ?? playerBirthDate) || x.Team.MaximumAge == null))
                    .ToList();

                if (activity.RestrictTeams)
                {
                    clubLeagueTeams = clubLeagueTeams.Where(x => selectedTeams.Contains(x.TeamId)).ToList();
                }

                if (!clubLeagueTeams.Any() && playerBirthDate != null)
                {
                    return Json(new {error = Messages.Activity_ClubPlayer_NoSuitableTeamsFound},
                        JsonRequestBehavior.AllowGet);
                }

                teams.AddRange(clubLeagueTeams.Select(x =>
                {
                    var prices =
                        playerPriceHelper.GetTeamPlayerPrice(user?.UserId ?? 0, x.TeamId, activity.ClubId ?? 0,
                            activity.SeasonId ?? 0, activity,
                            brotherIdNum: brotherIdNum,
                            startDateForPriceAdjustment: startDateForPriceAdjustment);

                    var leaguesNames = activity.Club?.Section?.Alias == SectionAliases.Tennis
                        ? String.Join(",",
                            leagueRepo.GetTennisTeamLeagues(x.TeamId, activity.SeasonId ?? 0).Select(c => c.Name))
                        : String.Join(",",
                            leagueRepo.GetByTeamAndSeason(x.TeamId, activity.SeasonId ?? 0).Select(c => c.Name));

                    return new ActivityFormGetClubPlayerUserTeamModel
                    {
                        TeamId = x.TeamId,
                        TeamName = HttpUtility.HtmlEncode(x.Team.TeamsDetails
                                                              .FirstOrDefault(t => t.SeasonId == activity.SeasonId)
                                                              ?.TeamName ??
                                                          x.Team.Title),
                        LeaguesNames = leaguesNames,
                        //SchoolId = clubSchoolTeams.FirstOrDefault(s => s.TeamId == x.TeamId)?.SchoolId,

                        RegistrationAndEquipmentPrice = prices.PlayerRegistrationAndEquipmentPrice,
                        ParticipationPrice = prices.ParticipationPrice,
                        InsurancePrice = prices.PlayerInsurancePrice
                    };
                }));
            }

            //if (!playerTeams.Any())
            //{
            //    return Json(new { error = Messages.Activity_PlayerAlreadyRegistered }, JsonRequestBehavior.AllowGet);
            //}

            var profilePicture = user?.PlayerFiles
                                     .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                                          x.FileType == (int) PlayerFileType.PlayerImage)
                                     ?.FileName ?? user?.Image;
            var medicalCert = user?.PlayerFiles
                .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                     x.FileType == (int) PlayerFileType.MedicalCertificate && !x.IsArchive)
                ?.FileName;
            var insuranceCert = user?.PlayerFiles
                .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                     x.FileType == (int) PlayerFileType.Insurance)
                ?.FileName;

            var response = new ActivityFormGetClubPlayerUserModel
            {
                Id = user?.UserId ?? 0,
                IdNum = user?.IdentNum,
                Email = user?.Email,

                FullName = user?.FullName,
                FirstName = user?.FirstName,
                LastName = user?.LastName,
                MiddleName = user?.MiddleName,

                Gender = user?.GenderId,
                ParentName = user?.ParentName,

                City = user?.City,
                Address = user?.Address,
                BirthDate = user?.BirthDay?.ToString("d"),
                Phone = user?.Telephone,
                ProfilePicture = !string.IsNullOrWhiteSpace(profilePicture)
                    ? $"{GlobVars.ContentPath}/players/{profilePicture}"
                    : null,
                MedicalCert = !string.IsNullOrWhiteSpace(medicalCert)
                    ? $"{GlobVars.ContentPath}/players/{medicalCert}"
                    : null,
                InsuranceCert = !string.IsNullOrWhiteSpace(insuranceCert)
                    ? $"{GlobVars.ContentPath}/players/{insuranceCert}"
                    : null,
                HideMedicalCert = user?.TeamsPlayers?.Any(x => x.Team.SchoolTeams.Any()) == true,

                Schools = playerTeams?.Any() == true && !activity.RestrictSchools ? null : schoolsModel,

                Teams = teams,

                MultiTeamEnabled = activity.MultiTeamRegistrations
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetClubCustomPlayerUser(string idNum, int activityId, string brotherIdNum, DateTime? startDateForPriceAdjustment)
        {
            var activity = activityRepo.GetById(activityId);
            var user = usersRepo.GetByIdentityNumber(idNum);

            if (activity == null) return Json(new { error = Messages.Activity_NotFound }, JsonRequestBehavior.AllowGet);

            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }

            var restartCheck = CheckPaymentRestartNeeded(activity, user?.UserId);
            if (restartCheck.Item1)
            {
                return restartCheck.Item2;
            }

            if (activity.AllowOnlyApprovedMembers)
            {
                if (user == null)
                {
                    return Json(new { error = Messages.Activity_OnlyApprovedMembersAllowed },
                        JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var playerRegistrations = user.ActivityFormsSubmittedDatas
                        .OrderBy(x => x.ActivityId)
                        .Where(
                            x => x.ClubId != null && x.TeamId != null &&
                                 x.Activity.SeasonId == activity.SeasonId &&
                                 x.Activity.Type == ActivityType.Personal &&
                                 x.IsActive);
                    if (
                        (playerRegistrations.FirstOrDefault(x => x.Activity.IsAutomatic == true) ??
                         playerRegistrations.FirstOrDefault(x =>
                             x.Activity.IsAutomatic == false || x.Activity.IsAutomatic == null)
                        ) == null
                    )
                    {
                        return Json(new { error = Messages.Activity_OnlyApprovedMembersAllowed },
                            JsonRequestBehavior.AllowGet);
                    }
                }
            }

            if (activity.DoNotAllowDuplicateRegistrations)
            {
                if (activity.ActivityFormsSubmittedDatas.Any(x => x.PlayerId == user?.UserId))
                {
                    return Json(new { error = Messages.Activity_AlreadyRegistered },
                        JsonRequestBehavior.AllowGet);
                }
            }

            var selectedSchools = !string.IsNullOrWhiteSpace(activity.RestrictedSchools)
                ? JsonConvert.DeserializeObject<List<int>>(activity.RestrictedSchools)
                : new List<int>();

            var selectedTeams = !string.IsNullOrWhiteSpace(activity.RestrictedTeams)
                ? JsonConvert.DeserializeObject<List<int>>(activity.RestrictedTeams)
                : new List<int>();

            List<School> schools;
            if (activity.RestrictSchools)
            {
                schools = activity.Club.Schools
                    .Where(x => x.SeasonId == activity.SeasonId && selectedSchools.Contains(x.Id))
                    .ToList();
            }
            else
            {
                schools = activity.Club.Schools.Where(x => x.SeasonId == activity.SeasonId).ToList();
            }

            var schoolsModel = schools
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => new ActivityFormGetClubPlayerUserSchoolModel
                {
                    SchoolId = x.Id,
                    SchoolName = x.Name
                })
                .ToList();

            if (user == null)
            {
                var schoolsResponse = new ActivityFormGetClubPlayerUserModel
                {
                    Schools = schoolsModel
                };

                return Json(schoolsResponse, JsonRequestBehavior.AllowGet);
            }
            var isPlayerSecondRegistration =
                activity.ActivityFormsSubmittedDatas.Count(x => x.PlayerId == user.UserId) == 1;

            var clubTeams = activity.Club.ClubTeams.Where(x => x.SeasonId == activity.SeasonId)
                .Select(x => x.TeamId)
                .ToList();
            List<SchoolTeam> clubSchoolTeams = new List<SchoolTeam>();

            foreach (var schoolTeams in schools.Select(x => x.SchoolTeams))
            {
                foreach (var schoolTeam in schoolTeams)
                {
                    clubTeams.Add(schoolTeam.TeamId);

                    clubSchoolTeams.Add(schoolTeam);
                }
            }

            if (activity.RestrictTeams)
            {
                clubTeams = clubTeams.Where(x => selectedTeams.Contains(x)).ToList();
                clubSchoolTeams = clubSchoolTeams.Where(x => selectedTeams.Contains(x.TeamId)).ToList();
            }

            var playerTeams = user.TeamsPlayers.Where(x => x.SeasonId == activity.SeasonId &&
                                                           clubTeams.Contains(x.TeamId) &&
                                                           !activity.ActivityFormsSubmittedDatas.Where(
                                                                   s => s.PlayerId == user.UserId &&
                                                                        s.TeamId == x.TeamId)
                                                               .Select(s => s.TeamId)
                                                               .Contains(x.TeamId))
                .ToList();

            //if (!playerTeams.Any())
            //{
            //    return Json(new { error = Messages.Activity_PlayerAlreadyRegistered }, JsonRequestBehavior.AllowGet);
            //}

            var profilePicture = user.PlayerFiles
                                     .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                                          x.FileType == (int)PlayerFileType.PlayerImage)
                                     ?.FileName ?? user.Image;
            var medicalCert = user.PlayerFiles
                .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                     x.FileType == (int)PlayerFileType.MedicalCertificate && !x.IsArchive)
                ?.FileName;
            var insuranceCert = user.PlayerFiles
                .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                     x.FileType == (int)PlayerFileType.Insurance)
                ?.FileName;

            var playerPriceHelper = new PlayerPriceHelper();

            var response = new ActivityFormGetClubPlayerUserModel
            {
                Id = user.UserId,
                IdNum = user.IdentNum,
                Email = user.Email,

                FullName = user.FullName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,

                Gender = user.GenderId,
                ParentName = user.ParentName,
                Address = user.City,
                BirthDate = user.BirthDay?.ToString("d"),
                Phone = user.Telephone,
                ProfilePicture = !string.IsNullOrWhiteSpace(profilePicture)
                    ? $"{GlobVars.ContentPath}/players/{profilePicture}"
                    : null,
                MedicalCert = !string.IsNullOrWhiteSpace(medicalCert)
                    ? $"{GlobVars.ContentPath}/players/{medicalCert}"
                    : null,
                InsuranceCert = !string.IsNullOrWhiteSpace(insuranceCert)
                    ? $"{GlobVars.ContentPath}/players/{insuranceCert}"
                    : null,
                HideMedicalCert = user.TeamsPlayers.Any(x => x.Team.SchoolTeams.Any()),

                MultiTeamEnabled = activity.MultiTeamRegistrations,

                Schools = playerTeams.Any() && !activity.RestrictSchools ? null : schoolsModel,

                Teams = playerTeams.Select(x =>
                    {
                        var prices =
                            playerPriceHelper.GetTeamPlayerPrice(user.UserId, x.TeamId, activity.ClubId ?? 0,
                                activity.SeasonId ?? 0, activity,
                                brotherIdNum: brotherIdNum,
                                startDateForPriceAdjustment: startDateForPriceAdjustment);

                        if (isPlayerSecondRegistration)
                        {
                            prices.PlayerInsurancePrice =
                                activity.SecondTeamNoInsurance
                                    ? 0
                                    : prices.PlayerInsurancePrice;

                            prices.PlayerRegistrationAndEquipmentPrice =
                                Math.Max(0,
                                    prices.PlayerRegistrationAndEquipmentPrice - (activity.SecondTeamDiscount ?? 0));
                        }

                        return new ActivityFormGetClubPlayerUserTeamModel
                        {
                            TeamId = x.TeamId,
                            TeamName = HttpUtility.HtmlEncode(x.Team.TeamsDetails.FirstOrDefault(t => t.SeasonId == activity.SeasonId)
                                                                  ?.TeamName ??
                                                              x.Team.Title),
                            SchoolId = clubSchoolTeams.FirstOrDefault(s => s.TeamId == x.TeamId)?.SchoolId,

                            RegistrationAndEquipmentPrice = prices.PlayerRegistrationAndEquipmentPrice,
                            ParticipationPrice = prices.ParticipationPrice,
                            InsurancePrice = prices.PlayerInsurancePrice
                        };
                    })
                    .ToList()
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetDepartmentClubPlayerUser(string idNum, int activityId)
        {
            var activity = activityRepo.GetById(activityId);
            var user = usersRepo.GetByIdentityNumber(idNum);

            if (activity == null) return Json(new { error = Messages.Activity_NotFound }, JsonRequestBehavior.AllowGet);

            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }

            var restartCheck = CheckPaymentRestartNeeded(activity, user?.UserId);
            if (restartCheck.Item1)
            {
                return restartCheck.Item2;
            }

            if (user == null)
            {
                var schools = activity.Club.Schools.Where(x => x.SeasonId == activity.SeasonId);

                var schoolsResponse = new ActivityFormGetClubPlayerUserModel
                {
                    Schools = schools.Select(x => new ActivityFormGetClubPlayerUserSchoolModel
                    {
                        SchoolId = x.Id,
                        SchoolName = x.Name
                    })
                        .ToList()
                };

                return Json(schoolsResponse, JsonRequestBehavior.AllowGet);
            }
            var isPlayerSecondRegistration =
                activity.ActivityFormsSubmittedDatas.Count(x => x.PlayerId == user.UserId) == 1;

            var clubTeams = activity.Club.ClubTeams.Where(x => x.SeasonId == activity.SeasonId)
                .Select(x => x.TeamId)
                .ToList();

            foreach (var schoolTeams in activity.Club?.Schools.Select(x => x.SchoolTeams))
            {
                foreach (var schoolTeam in schoolTeams)
                {
                    clubTeams.Add(schoolTeam.TeamId);
                }
            }

            var playerTeams = user.TeamsPlayers.Where(x => clubTeams.Contains(x.TeamId) &&
                                                           !activity.ActivityFormsSubmittedDatas.Where(
                                                                   s => s.PlayerId == user.UserId &&
                                                                        s.TeamId == x.TeamId)
                                                               .Select(s => s.TeamId)
                                                               .Contains(x.TeamId))
                .ToList();

            if (!playerTeams.Any())
            {
                return Json(new { error = Messages.Activity_PlayerAlreadyRegistered }, JsonRequestBehavior.AllowGet);
            }

            var profilePicture = user.PlayerFiles
                                     .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                                          x.FileType == (int)PlayerFileType.PlayerImage)
                                     ?.FileName ?? user.Image;
            var medicalCert = user.PlayerFiles
                .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                     x.FileType == (int)PlayerFileType.MedicalCertificate && !x.IsArchive)
                ?.FileName;
            var insuranceCert = user.PlayerFiles
                .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                     x.FileType == (int)PlayerFileType.Insurance)
                ?.FileName;

            var playerPriceHelper = new PlayerPriceHelper();

            var response = new ActivityFormGetClubPlayerUserModel
            {
                Id = user.UserId,
                IdNum = user.IdentNum,
                Email = user.Email,

                FullName = user.FullName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,

                Gender = user.GenderId,
                ParentName = user.ParentName,
                Address = user.City,
                BirthDate = user.BirthDay?.ToString("d"),
                Phone = user.Telephone,
                ProfilePicture = !string.IsNullOrWhiteSpace(profilePicture)
                    ? $"{GlobVars.ContentPath}/players/{profilePicture}"
                    : null,
                MedicalCert = !string.IsNullOrWhiteSpace(medicalCert)
                    ? $"{GlobVars.ContentPath}/players/{medicalCert}"
                    : null,
                InsuranceCert = !string.IsNullOrWhiteSpace(insuranceCert)
                    ? $"{GlobVars.ContentPath}/players/{insuranceCert}"
                    : null,
                HideMedicalCert = user.TeamsPlayers.Any(x => x.Team.SchoolTeams.Any()),

                Teams = playerTeams.Select(x =>
                    {
                        var prices =
                            playerPriceHelper.GetTeamPlayerPrice(user.UserId, x.TeamId, activity.ClubId ?? 0,
                                activity.SeasonId ?? 0, activity);

                        if (isPlayerSecondRegistration)
                        {
                            prices.PlayerInsurancePrice =
                                activity.SecondTeamNoInsurance
                                    ? 0
                                    : prices.PlayerInsurancePrice;

                            prices.PlayerRegistrationAndEquipmentPrice =
                                Math.Max(0,
                                    prices.PlayerRegistrationAndEquipmentPrice - (activity.SecondTeamDiscount ?? 0));
                        }

                        return new ActivityFormGetClubPlayerUserTeamModel
                        {
                            TeamId = x.TeamId,
                            TeamName = x.Team.TeamsDetails.FirstOrDefault(t => t.SeasonId == activity.SeasonId)
                                           ?.TeamName ??
                                       x.Team.Title,

                            RegistrationAndEquipmentPrice = prices.PlayerRegistrationAndEquipmentPrice,
                            ParticipationPrice = prices.ParticipationPrice,
                            InsurancePrice = prices.PlayerInsurancePrice
                        };
                    })
                    .ToList()
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetDepartmentClubCustomPlayerUser(string idNum, int activityId)
        {
            var activity = activityRepo.GetById(activityId);
            var user = usersRepo.GetByIdentityNumber(idNum);

            if (activity == null) return Json(new { error = Messages.Activity_NotFound }, JsonRequestBehavior.AllowGet);

            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }

            var restartCheck = CheckPaymentRestartNeeded(activity, user?.UserId);
            if (restartCheck.Item1)
            {
                return restartCheck.Item2;
            }

            if (activity.AllowOnlyApprovedMembers)
            {
                if (user == null)
                {
                    return Json(new { error = Messages.Activity_OnlyApprovedMembersAllowed },
                        JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var playerRegistrations = user.ActivityFormsSubmittedDatas
                        .OrderBy(x => x.ActivityId)
                        .Where(
                            x => x.ClubId != null && x.TeamId != null &&
                                 x.Activity.SeasonId == activity.SeasonId &&
                                 x.Activity.Type == ActivityType.Personal &&
                                 x.IsActive);
                    if (
                        (playerRegistrations.FirstOrDefault(x => x.Activity.IsAutomatic == true) ??
                         playerRegistrations.FirstOrDefault(x =>
                             x.Activity.IsAutomatic == false || x.Activity.IsAutomatic == null)
                        ) == null
                    )
                    {
                        return Json(new { error = Messages.Activity_OnlyApprovedMembersAllowed },
                            JsonRequestBehavior.AllowGet);
                    }
                }
            }

            if (activity.DoNotAllowDuplicateRegistrations)
            {
                if (activity.ActivityFormsSubmittedDatas.Any(x => x.PlayerId == user.UserId))
                {
                    return Json(new { error = Messages.Activity_AlreadyRegistered },
                        JsonRequestBehavior.AllowGet);
                }
            }

            if (user == null)
            {
                var schools = activity.Club.Schools.Where(x => x.SeasonId == activity.SeasonId);

                var schoolsResponse = new ActivityFormGetClubPlayerUserModel
                {
                    Schools = schools.Select(x => new ActivityFormGetClubPlayerUserSchoolModel
                    {
                        SchoolId = x.Id,
                        SchoolName = x.Name
                    })
                        .ToList()
                };

                return Json(schoolsResponse, JsonRequestBehavior.AllowGet);
            }
            var isPlayerSecondRegistration =
                activity.ActivityFormsSubmittedDatas.Count(x => x.PlayerId == user.UserId) == 1;

            var clubTeams = activity.Club.ClubTeams.Where(x => x.SeasonId == activity.SeasonId)
                .Select(x => x.TeamId)
                .ToList();

            foreach (var schoolTeams in activity.Club?.Schools.Select(x => x.SchoolTeams))
            {
                foreach (var schoolTeam in schoolTeams)
                {
                    clubTeams.Add(schoolTeam.TeamId);
                }
            }

            var playerTeams = user.TeamsPlayers.Where(x => clubTeams.Contains(x.TeamId) &&
                                                           !activity.ActivityFormsSubmittedDatas.Where(
                                                                   s => s.PlayerId == user.UserId &&
                                                                        s.TeamId == x.TeamId)
                                                               .Select(s => s.TeamId)
                                                               .Contains(x.TeamId))
                .ToList();

            if (!playerTeams.Any() && !activity.NoTeamRegistration)
            {
                return Json(new { error = Messages.Activity_PlayerAlreadyRegistered }, JsonRequestBehavior.AllowGet);
            }

            var profilePicture = user.PlayerFiles
                                     .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                                          x.FileType == (int)PlayerFileType.PlayerImage)
                                     ?.FileName ?? user.Image;
            var medicalCert = user.PlayerFiles
                .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                     x.FileType == (int)PlayerFileType.MedicalCertificate && !x.IsArchive)
                ?.FileName;
            var insuranceCert = user.PlayerFiles
                .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                     x.FileType == (int)PlayerFileType.Insurance)
                ?.FileName;

            var playerPriceHelper = new PlayerPriceHelper();

            var response = new ActivityFormGetClubPlayerUserModel
            {
                Id = user.UserId,
                IdNum = user.IdentNum,
                Email = user.Email,

                FullName = user.FullName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,

                Gender = user.GenderId,
                ParentName = user.ParentName,
                Address = user.City,
                BirthDate = user.BirthDay?.ToString("d"),
                Phone = user.Telephone,
                ProfilePicture = !string.IsNullOrWhiteSpace(profilePicture)
                    ? $"{GlobVars.ContentPath}/players/{profilePicture}"
                    : null,
                MedicalCert = !string.IsNullOrWhiteSpace(medicalCert)
                    ? $"{GlobVars.ContentPath}/players/{medicalCert}"
                    : null,
                InsuranceCert = !string.IsNullOrWhiteSpace(insuranceCert)
                    ? $"{GlobVars.ContentPath}/players/{insuranceCert}"
                    : null,
                HideMedicalCert = user.TeamsPlayers.Any(x => x.Team.SchoolTeams.Any()),

                Teams = playerTeams.Select(x =>
                    {
                        var prices =
                            playerPriceHelper.GetTeamPlayerPrice(user.UserId, x.TeamId, activity.ClubId ?? 0,
                                activity.SeasonId ?? 0, activity);

                        if (isPlayerSecondRegistration)
                        {
                            prices.PlayerInsurancePrice =
                                activity.SecondTeamNoInsurance
                                    ? 0
                                    : prices.PlayerInsurancePrice;

                            prices.PlayerRegistrationAndEquipmentPrice =
                                Math.Max(0,
                                    prices.PlayerRegistrationAndEquipmentPrice - (activity.SecondTeamDiscount ?? 0));
                        }

                        return new ActivityFormGetClubPlayerUserTeamModel
                        {
                            TeamId = x.TeamId,
                            TeamName = x.Team.TeamsDetails.FirstOrDefault(t => t.SeasonId == activity.SeasonId)
                                           ?.TeamName ??
                                       x.Team.Title,

                            RegistrationAndEquipmentPrice = prices.PlayerRegistrationAndEquipmentPrice,
                            ParticipationPrice = prices.ParticipationPrice,
                            InsurancePrice = prices.PlayerInsurancePrice
                        };
                    })
                    .ToList()
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetPlayerUser(string idNum, int activityId)
        {
            var activity = activityRepo.GetById(activityId);
            var user = usersRepo.GetByIdentityNumber(idNum);

            if (activity == null) return Json(new { error = Messages.Activity_NotFound }, JsonRequestBehavior.AllowGet);

            if (!activity.SeasonId.HasValue)
            {
                return Content("Activity: Unknown season");
            }

            var restartCheck = CheckPaymentRestartNeeded(activity, user?.UserId);
            if (restartCheck.Item1)
            {
                return restartCheck.Item2;
            }

            if (user == null || !user.IsActive ||
                !user.TeamsPlayers.Any(x => x.IsActive && x.SeasonId == activity.SeasonId))
                return Json(new { error = Messages.Activity_PlayerNotFoundOrInactive }, JsonRequestBehavior.AllowGet);

            var teamPlayers = user.TeamsPlayers.Where(x => x.IsActive &&
                                                           x.SeasonId == activity.SeasonId &&
                                                           x.Team.ActivityFormsSubmittedDatas.Where(
                                                                   s => s.Activity.SeasonId == activity.SeasonId &&
                                                                        s.Activity.IsAutomatic == true &&
                                                                        s.LeagueId == x.LeagueId &&
                                                                        s.Activity.Type == ActivityType.Group).Any(s => s.IsActive))
                .ToList();

            if (user.TeamsPlayers.Where(x => x.IsActive && x.SeasonId == activity.SeasonId)
                .All(x =>
                    x.Team.LeagueTeams.Where(lt => lt.SeasonId == activity.SeasonId)
                        .All(lt =>
                            user.ActivityFormsSubmittedDatas
                                .Where(s => s.ActivityId == activity.ActivityId &&
                                            s.Activity.IsAutomatic.HasValue && s.Activity.IsAutomatic.Value &&
                                            s.Activity.Type == ActivityType.Personal)
                                .Select(s => new { TeamId = s.TeamId ?? 0, LeagueId = s.LeagueId ?? 0 })
                                .Contains(new { x.TeamId, lt.LeagueId }))))
                return Json(new { error = Messages.Activity_PlayerAlreadyRegistered }, JsonRequestBehavior.AllowGet);


            if (user.TeamsPlayers.All(x => !x.Team.ActivityFormsSubmittedDatas.Any(r => r.IsActive)))
                return Json(new { error = Messages.Activity_PlayerTeamIsNotApprovedYet }, JsonRequestBehavior.AllowGet);

            var isPlayerSecondRegistration =
                activity.ActivityFormsSubmittedDatas.Count(x => x.PlayerId == user.UserId) == 1;

            var profilePicture = user.PlayerFiles
                                     .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                                          x.FileType == (int)PlayerFileType.PlayerImage)
                                     ?.FileName ?? user.Image;
            var medicalCert = user.PlayerFiles
                .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                     x.FileType == (int)PlayerFileType.MedicalCertificate && !x.IsArchive)
                ?.FileName;
            var insuranceCert = user.PlayerFiles
                .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                     x.FileType == (int)PlayerFileType.Insurance)
                ?.FileName;

            var playerPricesHelper = new PlayerPriceHelper();

            var result = new ActivityFormGetPlayerUserModel
            {
                Id = user.UserId,
                IdNum = user.IdentNum,
                Email = user.Email,

                FullName = user.FullName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,

                Gender = user.GenderId,
                Phone = user.Telephone,
                Address = user.Address,
                City = user.City,
                BirthDate = user.BirthDay?.ToString("d"),
                ProfilePicture = !string.IsNullOrWhiteSpace(profilePicture)
                    ? $"{GlobVars.ContentPath}/players/{profilePicture}"
                    : null,
                MedicalCert = !string.IsNullOrWhiteSpace(medicalCert)
                    ? $"{GlobVars.ContentPath}/players/{medicalCert}"
                    : null,
                InsuranceCert = !string.IsNullOrWhiteSpace(insuranceCert)
                    ? $"{GlobVars.ContentPath}/players/{insuranceCert}"
                    : null,
                Teams = new List<ActivityFormGetPlayerUserTeamModel>()
            };

            foreach (var teamsPlayer in teamPlayers)
            {
                var userAlreadyRegistered = user.ActivityFormsSubmittedDatas.Any(x =>
                    x.ActivityId == activity.ActivityId &&
                    x.Activity.SeasonId == teamsPlayer.SeasonId &&
                    x.TeamId == teamsPlayer.TeamId &&
                    x.LeagueId == teamsPlayer.LeagueId &&
                    x.Activity.IsAutomatic == true &&
                    x.Activity.Type == ActivityType.Personal);

                if (!userAlreadyRegistered)
                {
                    var playerPrice = playerPricesHelper.GetPlayerPrices(user.UserId, teamsPlayer.Team.TeamId,
                        teamsPlayer.LeagueId ?? 0, activity.SeasonId.Value);

                    playerPrice.RegistrationPrice = activity.RegistrationPrice ? playerPrice.RegistrationPrice : 0;
                    playerPrice.InsurancePrice = activity.InsurancePrice ? playerPrice.InsurancePrice : 0;

                    var userTeam = new ActivityFormGetPlayerUserTeamModel
                    {
                        TeamId = teamsPlayer.Team?.TeamId ?? 0,
                        TeamName =
                            teamsPlayer.Team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)
                                ?.TeamName ?? teamsPlayer?.Team?.Title,
                        League = teamsPlayer.League.Name,
                        LeagueId = teamsPlayer?.LeagueId ?? 0,
                        SeasonId = teamsPlayer?.SeasonId ?? 0,

                        PlayerRegistrationPrice = playerPrice.RegistrationPrice,
                        PlayerInsurancePrice = playerPrice.InsurancePrice
                    };

                    if (isPlayerSecondRegistration)
                    {
                        userTeam.PlayerInsurancePrice =
                            activity.SecondTeamNoInsurance
                                ? 0
                                : userTeam.PlayerInsurancePrice;

                        userTeam.PlayerRegistrationPrice =
                            Math.Max(0, userTeam.PlayerRegistrationPrice - (activity.SecondTeamDiscount ?? 0));
                    }

                    result.Teams.Add(userTeam);
                }
            }

            if (!result.Teams.Any())
            {
                return Json(new { error = Messages.Activity_PlayerAlreadyRegistered }, JsonRequestBehavior.AllowGet);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetTeamManagerUser(string idNum, int activityId)
        {
            var activity = activityRepo.GetById(activityId);
            var user = usersRepo.GetByIdentityNumber(idNum);

            ConvertTeamsRegistrationsLeagacyPaidValues(activity.ActivityFormsSubmittedDatas.ToList());

            if (user != null && activity != null)
            {
                var restartCheck = CheckPaymentRestartNeeded(activity, user?.UserId);
                if (restartCheck.Item1)
                {
                    return restartCheck.Item2;
                }

                var teamsAsTeamManager = GetTeamManagerTeamsViewModel(user, activity);
                var teamsAsClubManager = GetClubManagerTeamsViewModel(user, activity);
                
                if (teamsAsTeamManager.Any() || teamsAsClubManager.Any())
                {
                    var result = new ActivityFormGetTeamManagerUserModel
                    {
                        Id = user.UserId,
                        IdNum = user.IdentNum,
                        Email = user.Email,

                        FullName = user.FullName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        MiddleName = user.MiddleName,

                        Address = user.City,
                        BirthDate = user.BirthDay?.ToString("d"),
                        Phone = user.Telephone,
                        Teams = new List<ActivityFormGetTeamManagerTeamModel>()
                    };

                    result.Teams.AddRange(teamsAsTeamManager);
                    result.Teams.AddRange(teamsAsClubManager);

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { error = Messages.Activity_BuildForm_UserUnauthorized }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetUnionCustomTeamManagerUser(string idNum, int activityId)
        {
            var activity = activityRepo.GetById(activityId);
            var user = usersRepo.GetByIdentityNumber(idNum);

            if (activity != null)
            {
                var restartCheck = CheckPaymentRestartNeeded(activity, user?.UserId);
                if (restartCheck.Item1)
                {
                    return restartCheck.Item2;
                }

                var restrictedLeaguesIds = !string.IsNullOrWhiteSpace(activity.RestrictedLeagues)
                    ? JsonConvert.DeserializeObject<List<int>>(activity.RestrictedLeagues)
                    : new List<int>();

                var leagues = activity.RestrictLeagues
                    ? leagueRepo.GetCollection<League>(x => restrictedLeaguesIds.Contains(x.LeagueId)).ToList()
                    : leagueRepo.GetByUnion(activity.UnionId ?? 0, activity.SeasonId ?? 0)?.ToList() ?? new List<League>();

                var result = new ActivityFormGetUnionCustomGroupUserModel
                {
                    Id = user?.UserId ?? 0,
                    IdNum = user?.IdentNum,
                    Email = user?.Email,

                    FullName = user?.FullName,
                    FirstName = user?.FirstName,
                    LastName = user?.LastName,
                    MiddleName = user?.MiddleName,

                    City = user?.City,
                    Address = user?.Address,
                    BirthDate = user?.BirthDay?.ToString("d"),
                    Phone = user?.Telephone,

                    Leagues = leagues.Select(x => new ActivityFormUnionCustomGroupLeagueModel
                        {
                            Id = x.LeagueId,
                            Name = x.Name,
                            TeamRegistrationPrice =
                                x.LeaguesPrices?.FirstOrDefault(
                                    p => p.PriceType == (int) LeaguePriceType.TeamRegistrationPrice &&
                                         p.StartDate <= DateTime.Now && p.EndDate > DateTime.Now)?.Price ?? 0
                        })
                        .ToList()
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(new { error = Messages.Activity_NotFound }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetUnionClubUser(string idNum, int activityId)
        {
            var activity = activityRepo.GetById(activityId);
            var user = usersRepo.GetByIdentityNumber(idNum);

            //if (user == null)
            //{
            //    return Json(new { error = Messages.UserNotExists }, JsonRequestBehavior.AllowGet);
            //}

            if (activity != null)
            {
                var restartCheck = CheckPaymentRestartNeeded(activity, user?.UserId);
                if (restartCheck.Item1)
                {
                    return restartCheck.Item2;
                }

                var alreadyRegisteredClubs = activity.ActivityFormsSubmittedDatas.Where(x => x.ClubId != null).Select(x => x.ClubId).ToList();

                var userClubManagerJobs = jobsRepo.GetAllUsersJobs(user?.UserId ?? 0).Where(x =>
                    (x.Job.JobsRole.RoleName == JobRole.ClubManager || x.Job.JobsRole.RoleName == JobRole.ClubSecretary) && x.SeasonId == activity.SeasonId &&
                    x.Club.UnionId == activity.UnionId);
                var clubIds = userClubManagerJobs.Select(x => x.ClubId).Where(x => x != null).ToList();

                var managerInClubs = clubsRepo.GetCollection<Club>(x => clubIds.Contains(x.ClubId) && !alreadyRegisteredClubs.Contains(x.ClubId) && !x.IsArchive).ToList();

                var regions = activity.Union?.Regionals?.Where(x => x.SeasonId == activity.SeasonId)?.ToList() ?? new List<Regional>();

                var result = new ActivityFormGetUnionClubUserModel
                {
                    Id = user?.UserId ?? 0,
                    IdNum = user?.IdentNum,
                    Email = user?.Email,

                    FullName = user?.FullName,
                    FirstName = user?.FirstName,
                    LastName = user?.LastName,
                    MiddleName = user?.MiddleName,

                    Address = user?.City,
                    BirthDate = user?.BirthDay?.ToString("d"),
                    Phone = user?.Telephone,

                    DisableRegPaymentForExistingClubs = activity.DisableRegPaymentForExistingClubs,

                    SportCenters = activityRepo.GetCollection<SportCenter>(null).Select(x => new ActivityFormUnionClubSportCenterModel
                    {
                        Id = x.Id,
                        Caption = !IsHebrew ? x.Eng : x.Heb
                    }).ToList(),

                    Regions = regions.Select(x => new ActivityFormUnionClubRegionModel
                    {
                        Id = x.RegionalId,
                        Name = x.Name
                    }).ToList(),

                    Clubs = managerInClubs.Select(x => new ActivityFormUnionClubClubModel
                    {
                        Id = x.ClubId,
                        Name = x.Name ?? string.Empty,
                        Address = x.Address ?? string.Empty,
                        Email = x.Email ?? string.Empty,
                        NGONumber = x.NGO_Number,
                        NumberOfCourts = x.NumberOfCourts,
                        Phone = x.ContactPhone ?? string.Empty,
                        SportsCenter = x.SportCenterId
                    }).ToList()
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(new { error = Messages.Activity_NotFound }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetUnionPersonalClubUser(string idNum, int activityId, int regionId, DateTime? birthDate)
        {
            var activity = activityRepo.GetById(activityId);
            var user = usersRepo.GetByIdentityNumber(idNum);

            //if (user == null)
            //{
            //    return Json(new { error = Messages.UserNotExists }, JsonRequestBehavior.AllowGet);
            //}

            if (activity != null)
            {
                var restartCheck = CheckPaymentRestartNeeded(activity, user?.UserId, true);
                if (restartCheck.Item1)
                {
                    return restartCheck.Item2;
                }

                birthDate = birthDate ?? user?.BirthDay;

                if (birthDate == null) //check if player has to enter his birthdate first
                {
                    var unionPricesHaveAges = activity.Union.UnionPrices
                        .Any(x => x.SeasonId == activity.SeasonId &&
                                  (x.StartDate <= DateTime.Now && x.EndDate > DateTime.Now) &&
                                  (x.FromBirthday != null || x.ToBirthday != null));

                    if (unionPricesHaveAges)
                    {
                        return Json(new ActivityFormGetUnionPersonalClubUserModel
                        {
                            BirthdateNeeded = true
                        }, JsonRequestBehavior.AllowGet);
                    }
                }

                var profilePicture = user?.PlayerFiles
                                         .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                                              x.FileType == (int)PlayerFileType.PlayerImage)
                                         ?.FileName ?? user?.Image;
                var idFile = user?.IDFile ?? user?.PlayerFiles
                                         .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                                              x.FileType == (int)PlayerFileType.IDFile)
                                         ?.FileName;
                var medicalCert = user?.PlayerFiles
                    .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                         x.FileType == (int)PlayerFileType.MedicalCertificate && !x.IsArchive)
                    ?.FileName;
                var insuranceCert = user?.PlayerFiles
                    .FirstOrDefault(x => x.SeasonId == activity.SeasonId &&
                                         x.FileType == (int)PlayerFileType.Insurance)
                    ?.FileName;

                var clubs = clubsRepo.GetByUnion(activity.UnionId ?? 0, activity.SeasonId).ToList();
                if (activity.OnlyApprovedClubs)
                {
                    clubs = clubs.Where(x =>
                            x.ActivityFormsSubmittedDatas.Any(
                                r => r.Activity.Type == ActivityType.Club &&
                                     r.Activity.IsAutomatic == true &&
                                     r.Activity.SeasonId == activity.SeasonId &&
                                     r.IsActive) ||
                            (regionId > 0 && x.IsClubApproveByRegional) ||
                            x.IsClubApproved == true
                        )
                        .ToList();
                }

                if (regionId > 0)
                {
                    clubs = clubs
                        .Where(x => x.RegionalId == regionId)
                        .ToList();
                }

                var regions = regionalsRepo.GetRegionalsByUnionAndSeason(activity.UnionId ?? 0, activity.SeasonId ?? 0);

                var clubIds = clubs.Select(x => x.ClubId).ToList();

                var pricesHelper = new PlayerPriceHelper();
                var prices = pricesHelper.GetUnionClubPlayerPrice(activity.UnionId ?? 0, activity.SeasonId ?? 0, birthDate);

                var playerTeams =
                    user?.TeamsPlayers?.Where(x => clubIds.Contains(x.ClubId ?? 0) && x.SeasonId == activity.SeasonId)?.ToList();

                var result = new ActivityFormGetUnionPersonalClubUserModel()
                {
                    Id = user?.UserId ?? 0,
                    IdNum = user?.IdentNum,
                    Email = user?.Email,

                    FullName = user?.FullName,
                    FirstName = user?.FirstName,
                    LastName = user?.LastName,
                    MiddleName = user?.MiddleName,

                    Gender = user?.GenderId,
                    Phone = user?.Telephone,
                    City = user?.City,
                    Address = user?.Address,
                    BirthDate = birthDate?.ToString("d"),

                    PostalCode = user?.PostalCode,
                    PassportNum = user?.PassportNum,
                    ForeignFirstName = user?.ForeignFirstName,
                    ForeignLastName = user?.ForeignLastName,

                    MotherName = user?.MotherName,
                    ParentPhone = user?.ParentPhone,
                    IdentCard = user?.IdentCard,
                    LicenseDate = user?.LicenseValidity?.ToString("d"),
                    ParentEmail = user?.ParentEmail,
                    ParentName = user?.ParentName,
                    IsFilteredByRegion = regionId > 0,

                    ProfilePicture = !string.IsNullOrWhiteSpace(profilePicture)
                        ? $"{GlobVars.ContentPath}/players/{profilePicture}"
                        : null,
                    IdFile = !string.IsNullOrWhiteSpace(idFile)
                        ? $"{GlobVars.ContentPath}/players/{idFile}"
                        : null,
                    MedicalCert = !string.IsNullOrWhiteSpace(medicalCert)
                        ? $"{GlobVars.ContentPath}/players/{medicalCert}"
                        : null,
                    InsuranceCert = !string.IsNullOrWhiteSpace(insuranceCert)
                        ? $"{GlobVars.ContentPath}/players/{insuranceCert}"
                        : null,

                    RegularRegistrationPrice = activity.RegistrationPrice ? prices.RegularRegistrationPrice : 0,
                    CompetitiveRegistrationPrice = activity.RegistrationPrice ? prices.CompetingRegistrationPrice : 0,
                    InsurancePrice = activity.InsurancePrice ? prices.InsurancePrice : 0,
                    TenicardPrice = prices.TenicardPrice,

                    MedExamDate = user?.MedExamDate?.ToString("d"),
                    DateOfInsuranceValidity = user?.DateOfInsurance?.ToString("d"),
                    TenicardValidity = user?.TenicardValidity?.ToString("d"),

                    Clubs = clubs.Select(x => new ActivityFormGetUnionPersonalClubClubModel
                    {
                        Id = x.ClubId,
                        Name = x.Name
                    }).ToList(),

                    Regions = regions.Select(x => new ActivityFormGetUnionPersonalClubRegionModel
                    {
                        Id = x.RegionalId,
                        Name = x.Name
                    }).ToList(),

                    CurrentClubId = (int) (playerTeams?.Count == 1 ? playerTeams.First().ClubId : 0),
                    CurrentTeamId = playerTeams?.Count == 1 ? playerTeams.First().TeamId : 0
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(new { error = Messages.Activity_NotFound }, JsonRequestBehavior.AllowGet);
        }

        private Tuple<bool, JsonResult> CheckPaymentRestartNeeded(Activity activity, int? userId, bool throwErrorIfPaymentIsDone = false)
        {
            var registrations = activity.ActivityFormsSubmittedDatas.Where(x => x.PlayerId == userId);
            foreach (var registration in registrations)
            {
                if (throwErrorIfPaymentIsDone && registration.CardComPaymentCompleted)
                {
                    return new Tuple<bool, JsonResult>(true,
                        Json(new {error = Messages.Activity_PlayerAlreadyRegistered}, JsonRequestBehavior.AllowGet));
                }

                var customPrices = string.IsNullOrWhiteSpace(registration.CustomPrices)
                    ? new List<ActivityCustomPriceModel>()
                    : JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(registration.CustomPrices);

                var paidValueModified = registration.HandlingFeePaid > 0 ||
                                        registration.InsurancePaid > 0 ||
                                        registration.ParticipationPaid > 0 ||
                                        registration.RegistrationPaid > 0 ||
                                        registration.TenicardPaid > 0 ||
                                        registration.MembersFeePaid > 0 ||
                                        customPrices.Any(x => x.Paid > 0);

                var hasUnpaidEntries = (registration.HandlingFee > 0 && registration.HandlingFeePaid == 0) ||
                                       (registration.InsurancePrice > 0 && registration.InsurancePaid == 0) ||
                                       (registration.ParticipationPrice > 0 && registration.ParticipationPaid == 0) ||
                                       (registration.RegistrationPrice > 0 && registration.RegistrationPaid == 0) ||
                                       (registration.TenicardPrice > 0 && registration.TenicardPaid == 0) ||
                                       (registration.MembersFee > 0 && registration.MembersFeePaid == 0) ||
                                       customPrices.Any(x => x.TotalPrice > 0 && x.Paid == 0);

                if (registration.CardComLpc != null) //CardCom payments
                {
                    if (!registration.CardComPaymentCompleted && !paidValueModified)
                    {
                        return new Tuple<bool, JsonResult>(true,
                            Json(
                                new
                                {
                                    restartPayment = true,
                                    regDate = registration.DateSubmitted.ToString(),
                                    regId = registration.Id
                                }, JsonRequestBehavior.AllowGet));
                    }

                    if (registration.CardComPaymentCompleted && hasUnpaidEntries)
                    {
                        return new Tuple<bool, JsonResult>(true,
                            Json(
                                new
                                {
                                    restartPayment = true,
                                    regDate = registration.DateSubmitted.ToString(),
                                    regId = registration.Id
                                }, JsonRequestBehavior.AllowGet));
                    }
                }

                if (registration.PayPalLogLigId != null && //PayPal payments
                    !string.IsNullOrWhiteSpace(registration.PayPalPaymentId) &&
                    string.IsNullOrWhiteSpace(registration.PayPalPayerId))
                {
                    return new Tuple<bool, JsonResult>(true,
                        Json(
                            new
                            {
                                restartPayment = true,
                                regDate = registration.DateSubmitted.ToString(),
                                regId = registration.Id
                            }, JsonRequestBehavior.AllowGet));
                }
            }

            return new Tuple<bool, JsonResult>(false, null);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult RestartPayment(int id)
        {
            var registration = activityRepo.GetCollection<ActivityFormsSubmittedData>(x => x.Id == id).FirstOrDefault();

            if (registration != null)
            {
                var activity = registration.Activity;
                if (activity == null)
                {
                    return Content("Activity no longer exist. Please contact your manager");
                }

                var user = registration.User;
                if (user == null)
                {
                    return Content("User does not exist. Please contact your manager");
                }

                var team = registration.Team;
                if (team == null && !activity.NoTeamRegistration)
                {
                    return Content("Team does not exist. Please contact your manager");
                }

                var redirectUrl = string.Empty;

                var priceTotal = 0m;

                var customPrices = !string.IsNullOrWhiteSpace(registration.CustomPrices)
                    ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(registration.CustomPrices)
                    : new List<ActivityCustomPriceModel>();
                var registrationPrice = Math.Max(0, registration.RegistrationPrice - registration.RegistrationPaid);
                var insurancePrice = Math.Max(0, registration.InsurancePrice - registration.InsurancePaid);
                var tenicardPrice = Math.Max(0, registration.TenicardPrice - registration.TenicardPaid);
                var participationPrice = Math.Max(0, registration.ParticipationPrice - registration.ParticipationPaid);
                var membersFee = Math.Max(0, registration.MembersFee - registration.MembersFeePaid);
                var handlingFee = Math.Max(0, registration.HandlingFee - registration.HandlingFeePaid);

                var activityFormType = registration.Activity.GetFormType();
                switch (activityFormType)
                {
                    case ActivityFormType.UnionPlayerToClub:
                        priceTotal = registrationPrice + insurancePrice + tenicardPrice + customPrices.Sum(x => Math.Max(0, x.TotalPrice - x.Paid));

                        #region CardCom payment for catchball union

                        if (!registration.IsPaymentByBenefactor && registration.Activity.UnionId == 15 &&
                            priceTotal > 0)
                        {
                            var result = CardComHelper.UnionPlayerToClub_CatchBall(activity, user, 
                                new List<Team>{ team }, 
                                customPrices,
                                priceTotal,
                                new UnionClubPlayerPrice
                                {
                                    RegularRegistrationPrice = registrationPrice,
                                    RegularRegistrationCardComProductId = registration.RegistrationCardComProductId
                                }, false);

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for Rugby union

                        if (!registration.IsPaymentByBenefactor && activity.UnionId == 54 && priceTotal > 0)
                        {
                            var result = CardComHelper.UnionPlayerToClub_Rugby(activity, user, new List<Team> { team }, customPrices,
                                priceTotal,
                                new UnionClubPlayerPrice
                                {
                                    RegularRegistrationPrice = registrationPrice,
                                    RegularRegistrationCardComProductId = registration.RegistrationCardComProductId,
                                    InsurancePrice = insurancePrice,
                                    InsuranceCardComProductId = registration.InsuranceCardComProductId,
                                },
                                false);

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion
                        
                        #region CardCom payment for wave surfing union

                        if (registration.Activity.Union?.UnionId == 41 && priceTotal > 0)
                        {
                            var result = CardComHelper.UnionPlayerToClub_WaveSurfing(activity, user, 
                                new List<Team>{ team }, 
                                customPrices, 
                                priceTotal,
                                new UnionClubPlayerPrice
                                {
                                    RegularRegistrationPrice = registrationPrice,
                                    RegularRegistrationCardComProductId = registration.RegistrationCardComProductId,
                                    InsurancePrice = insurancePrice,
                                    InsuranceCardComProductId = registration.InsuranceCardComProductId,
                                    TenicardPrice = tenicardPrice,
                                    TenicardCardComProductId = registration.TenicardCardComProductId
                                }, false);

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for tennis union

                        if (registration.Activity.UnionId == 38 && priceTotal > 0)
                        {
                            var result = CardComHelper.UnionPlayerToClub_Tennis(activity, user, 
                                new List<Team>{ team },
                                customPrices,
                                priceTotal,
                                new UnionClubPlayerPrice
                                {
                                    RegularRegistrationPrice = registrationPrice,
                                    RegularRegistrationCardComProductId = registration.RegistrationCardComProductId,
                                    InsurancePrice = insurancePrice,
                                    InsuranceCardComProductId = registration.InsuranceCardComProductId,
                                    TenicardPrice = tenicardPrice,
                                    TenicardCardComProductId = registration.TenicardCardComProductId
                                }, false);

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for Climbing union 59

                        if (!registration.IsPaymentByBenefactor && registration.Activity.UnionId == 59 &&
                            priceTotal > 0)
                        {
                            var result = CardComHelper.UnionPlayerToClub_Climbing59(activity, user,
                                new List<Team> { team },
                                customPrices,
                                priceTotal,
                                new UnionClubPlayerPrice
                                {
                                    RegularRegistrationPrice = registrationPrice,
                                    RegularRegistrationCardComProductId = registration.RegistrationCardComProductId
                                }, false);

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        break;
                    case ActivityFormType.PlayerRegistration:
                        priceTotal = registrationPrice + insurancePrice + customPrices.Sum(x => Math.Max(0, x.TotalPrice - x.Paid));

                        #region CardCom payment for catchball union

                        if (!registration.IsPaymentByBenefactor && activity.UnionId == 15 &&
                            priceTotal > 0)
                        {
                            var result = CardComHelper.UnionPlayer_Catchball(activity, user, team, customPrices, priceTotal,
                                new LeaguePlayerPrice
                                {
                                    RegistrationPrice = registrationPrice,
                                    RegistrationPriceCardComProductId = registration.RegistrationCardComProductId,
                                    InsurancePrice = insurancePrice,
                                    InsuranceCardComProductId = registration.InsuranceCardComProductId
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for wave surfing union

                        if (activity.Union?.UnionId == 41 && priceTotal > 0)
                        {
                            var result = CardComHelper.UnionPlayer_WaveSurfing(activity, user, team, customPrices, priceTotal,
                                new LeaguePlayerPrice
                                {
                                    RegistrationPrice = registrationPrice,
                                    RegistrationPriceCardComProductId = registration.RegistrationCardComProductId,
                                    InsurancePrice = insurancePrice,
                                    InsuranceCardComProductId = registration.InsuranceCardComProductId
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for tennis union

                        if (activity.UnionId == 38 && priceTotal > 0)
                        {
                            var result = CardComHelper.UnionPlayer_Tennis(activity, user, team, customPrices, priceTotal,
                                new LeaguePlayerPrice
                                {
                                    RegistrationPrice = registrationPrice,
                                    RegistrationPriceCardComProductId = registration.RegistrationCardComProductId,
                                    InsurancePrice = insurancePrice,
                                    InsuranceCardComProductId = registration.InsuranceCardComProductId
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        break;

                    case ActivityFormType.TeamRegistration:
                        priceTotal = registrationPrice + customPrices.Sum(x => Math.Max(0, x.TotalPrice - x.Paid));

                        #region CardCom payment for catchball union

                        if (!registration.IsPaymentByBenefactor && activity.UnionId == 15 &&
                            priceTotal > 0)
                        {
                            var result =
                                CardComHelper.UnionTeam_Catchball(activity, user, team, customPrices, priceTotal, 
                                    new LeaguesPrice
                                    {
                                        Price = registrationPrice,
                                        CardComProductId = registration.RegistrationCardComProductId
                                    });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for wave surfing union

                        if (activity.Union?.UnionId == 41 && priceTotal > 0)
                        {
                            var result =
                                CardComHelper.UnionTeam_WaveSurfing(activity, user, team, customPrices, priceTotal,
                                    new LeaguesPrice
                                    {
                                        Price = registrationPrice,
                                        CardComProductId = registration.RegistrationCardComProductId
                                    });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for tennis union

                        if (activity.UnionId == 38 && priceTotal > 0)
                        {
                            var result =
                                CardComHelper.UnionTeam_Tennis(activity, user, team, customPrices, priceTotal,
                                    new LeaguesPrice
                                    {
                                        Price = registrationPrice,
                                        CardComProductId = registration.RegistrationCardComProductId
                                    });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        break;

                    case ActivityFormType.ClubPlayerRegistration:
                        priceTotal = registrationPrice + insurancePrice + participationPrice + customPrices.Sum(x => Math.Max(0, x.TotalPrice - x.Paid));

                        #region CardCom payment for Basketball union

                        if (!registration.IsPaymentByBenefactor && activity.ClubId == 1194 && priceTotal > 0)
                        {
                            var result = CardComHelper.ClubPlayer_Basketball(activity, user, 
                                new List<Team> {team},
                                customPrices,
                                priceTotal,
                                new List<TeamPlayerPrice>
                                {
                                    new TeamPlayerPrice
                                    {
                                        PlayerRegistrationAndEquipmentPrice = registrationPrice,
                                        PlayerRegistrationAndEquipmentCardComProductId =
                                            registration.RegistrationCardComProductId,
                                        PlayerInsurancePrice = insurancePrice,
                                        PlayerInsuranceCardComProductId = registration.InsuranceCardComProductId,
                                        ParticipationPrice = participationPrice,
                                        ParticipationCardComProductId = registration.ParticipationCardComProductId,
                                        TeamId = team.TeamId
                                    }
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for Rugby union

                        if (!registration.IsPaymentByBenefactor && activity.Club?.UnionId == 54 && priceTotal > 0)
                        {
                            var result = CardComHelper.ClubPlayer_Rugby(
                                activity,
                                user,
                                new List<Team> {team},
                                customPrices,
                                priceTotal,
                                new List<TeamPlayerPrice>
                                {
                                    new TeamPlayerPrice
                                    {
                                        PlayerRegistrationAndEquipmentPrice = registrationPrice,
                                        PlayerRegistrationAndEquipmentCardComProductId =
                                            registration.RegistrationCardComProductId,
                                        PlayerInsurancePrice = insurancePrice,
                                        PlayerInsuranceCardComProductId = registration.InsuranceCardComProductId,
                                        ParticipationPrice = participationPrice,
                                        ParticipationCardComProductId = registration.ParticipationCardComProductId,
                                        TeamId = team.TeamId
                                    }
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for Soccer club

                        if (!registration.IsPaymentByBenefactor && activity.ClubId == 2541 && priceTotal > 0)
                        {
                            var result = CardComHelper.ClubPlayer_Soccer(activity, user,
                                new List<Team> { team },
                                customPrices,
                                priceTotal,
                                new List<TeamPlayerPrice>
                                {
                                    new TeamPlayerPrice
                                    {
                                        PlayerRegistrationAndEquipmentPrice = registrationPrice,
                                        PlayerRegistrationAndEquipmentCardComProductId =
                                            registration.RegistrationCardComProductId,
                                        PlayerInsurancePrice = insurancePrice,
                                        PlayerInsuranceCardComProductId = registration.InsuranceCardComProductId,
                                        ParticipationPrice = participationPrice,
                                        ParticipationCardComProductId = registration.ParticipationCardComProductId,
                                        TeamId = team.TeamId
                                    }
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for Gymnastic Club 3610

                        if (!registration.IsPaymentByBenefactor && activity.ClubId == 3610 && priceTotal > 0)
                        {
                            var result = CardComHelper.ClubPlayer_GymnasticClub3610(activity, user,
                                new List<Team> {team},
                                customPrices,
                                priceTotal,
                                new List<TeamPlayerPrice>
                                {
                                    new TeamPlayerPrice
                                    {
                                        PlayerRegistrationAndEquipmentPrice = registrationPrice,
                                        PlayerRegistrationAndEquipmentCardComProductId =
                                            registration.RegistrationCardComProductId,
                                        PlayerInsurancePrice = insurancePrice,
                                        PlayerInsuranceCardComProductId = registration.InsuranceCardComProductId,
                                        ParticipationPrice = participationPrice,
                                        ParticipationCardComProductId = registration.ParticipationCardComProductId,
                                        TeamId = team.TeamId
                                    }
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        break;

                    case ActivityFormType.ClubCustomPersonal:
                        priceTotal = registrationPrice + insurancePrice + participationPrice + customPrices.Sum(x => Math.Max(0, x.TotalPrice - x.Paid));

                        #region CardCom payment for Basketball union

                        if (!registration.IsPaymentByBenefactor && activity.ClubId == 1194 && priceTotal > 0)
                        {
                            var result = CardComHelper.ClubPlayer_Basketball(activity, user,
                                new List<Team> {team},
                                customPrices,
                                priceTotal,
                                new List<TeamPlayerPrice>
                                {
                                    new TeamPlayerPrice
                                    {
                                        PlayerRegistrationAndEquipmentPrice = registrationPrice,
                                        PlayerRegistrationAndEquipmentCardComProductId =
                                            registration.RegistrationCardComProductId,
                                        PlayerInsurancePrice = insurancePrice,
                                        PlayerInsuranceCardComProductId = registration.InsuranceCardComProductId,
                                        ParticipationPrice = participationPrice,
                                        ParticipationCardComProductId = registration.ParticipationCardComProductId,
                                        TeamId = team?.TeamId
                                    }
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for Soccer club

                        if (!registration.IsPaymentByBenefactor && activity.ClubId == 2541 && priceTotal > 0)
                        {
                            var result = CardComHelper.ClubPlayer_Soccer(activity, user,
                                new List<Team> { team },
                                customPrices, 
                                priceTotal,
                                new List<TeamPlayerPrice>
                                {
                                    new TeamPlayerPrice
                                    {
                                        PlayerRegistrationAndEquipmentPrice = registrationPrice,
                                        PlayerRegistrationAndEquipmentCardComProductId =
                                            registration.RegistrationCardComProductId,
                                        PlayerInsurancePrice = insurancePrice,
                                        PlayerInsuranceCardComProductId = registration.InsuranceCardComProductId,
                                        ParticipationPrice = participationPrice,
                                        ParticipationCardComProductId = registration.ParticipationCardComProductId,
                                        TeamId = team?.TeamId
                                    }
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for Gymnastic Club 3610

                        if (!registration.IsPaymentByBenefactor && activity.ClubId == 3610 && priceTotal > 0)
                        {
                            var result = CardComHelper.ClubPlayer_GymnasticClub3610(activity, user,
                                new List<Team> {team},
                                customPrices, priceTotal,
                                new List<TeamPlayerPrice>
                                {
                                    new TeamPlayerPrice
                                    {
                                        PlayerRegistrationAndEquipmentPrice = registrationPrice,
                                        PlayerRegistrationAndEquipmentCardComProductId =
                                            registration.RegistrationCardComProductId,
                                        PlayerInsurancePrice = insurancePrice,
                                        PlayerInsuranceCardComProductId = registration.InsuranceCardComProductId,
                                        ParticipationPrice = participationPrice,
                                        ParticipationCardComProductId = registration.ParticipationCardComProductId,
                                        TeamId = team?.TeamId
                                    }
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion


                        break;

                    case ActivityFormType.DepartmentClubPlayerRegistration:
                        priceTotal = registrationPrice + insurancePrice + participationPrice + customPrices.Sum(x => Math.Max(0, x.TotalPrice - x.Paid));

                        #region CardCom payment for Basketball union

                        if (!registration.IsPaymentByBenefactor && activity.ClubId == 1194 && priceTotal > 0)
                        {
                            var result = CardComHelper.DepartmentClubPlayer_Basketball(activity, user, team, customPrices,
                                priceTotal,
                                new TeamPlayerPrice
                                {
                                    PlayerRegistrationAndEquipmentPrice = registrationPrice,
                                    PlayerRegistrationAndEquipmentCardComProductId = registration.RegistrationCardComProductId,
                                    PlayerInsurancePrice = insurancePrice,
                                    PlayerInsuranceCardComProductId = registration.InsuranceCardComProductId,
                                    ParticipationPrice = participationPrice,
                                    ParticipationCardComProductId = registration.ParticipationCardComProductId
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        break;

                    case ActivityFormType.DepartmentClubCustomPersonal:
                        priceTotal = registrationPrice + insurancePrice + participationPrice + customPrices.Sum(x => Math.Max(0, x.TotalPrice - x.Paid));

                        #region CardCom payment for Basketball union

                        if (!registration.IsPaymentByBenefactor && activity.ClubId == 1194 && priceTotal > 0)
                        {
                            var result = CardComHelper.DepartmentClubPlayer_Basketball(activity, user, team, customPrices,
                                priceTotal,
                                new TeamPlayerPrice
                                {
                                    PlayerRegistrationAndEquipmentPrice = registrationPrice,
                                    PlayerRegistrationAndEquipmentCardComProductId = registration.RegistrationCardComProductId,
                                    PlayerInsurancePrice = insurancePrice,
                                    PlayerInsuranceCardComProductId = registration.InsuranceCardComProductId,
                                    ParticipationPrice = participationPrice,
                                    ParticipationCardComProductId = registration.ParticipationCardComProductId
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        break;

                    case ActivityFormType.CustomPersonal:
                        priceTotal = registrationPrice + insurancePrice + membersFee + handlingFee + customPrices.Sum(x => Math.Max(0, x.TotalPrice - x.Paid));
                        
                        #region CardCom payment for catchball union

                        if (!registration.IsPaymentByBenefactor && activity.UnionId == 15 && priceTotal > 0)
                        {
                            if (activity.PaymentMethod == (int) ActivityPaymentMethod.PayPal)
                            {
                                var result = PayPalHelper.UnionCustomPersonal_Catchball15(activity, user, team, customPrices, priceTotal,
                                    new LeaguePlayerPrice
                                    {
                                        RegistrationPrice = registrationPrice,
                                        RegistrationPriceCardComProductId = registration.RegistrationCardComProductId,
                                        InsurancePrice = insurancePrice,
                                        InsuranceCardComProductId = registration.InsuranceCardComProductId,
                                        MembersFee = membersFee,
                                        MembersFeeCardComProductId = registration.MembersFeeCardComProductId,
                                        HandlingFee = handlingFee,
                                        HandlingFeeCardComProductId = registration.HandlingFeeCardComProductId
                                    });

                                registration.PayPalLogLigId = result?.PayPalLogLigId;
                                registration.PayPalPaymentId = result?.PayPalPaymentId;
                                redirectUrl = result?.RedirectUrl;
                            }
                            else
                            {
                                var result = CardComHelper.UnionCustomPersonal_Catchball(activity, user, team, customPrices, priceTotal,
                                    new LeaguePlayerPrice
                                    {
                                        RegistrationPrice = registrationPrice,
                                        RegistrationPriceCardComProductId = registration.RegistrationCardComProductId,
                                        InsurancePrice = insurancePrice,
                                        InsuranceCardComProductId = registration.InsuranceCardComProductId,
                                        MembersFee = membersFee,
                                        MembersFeeCardComProductId = registration.MembersFeeCardComProductId,
                                        HandlingFee = handlingFee,
                                        HandlingFeeCardComProductId = registration.HandlingFeeCardComProductId
                                    });

                                registration.CardComLpc = result?.Lpc;
                                redirectUrl = result?.RedirectUrl;
                            }
                        }

                        #endregion

                        #region CardCom payment for Rugby union

                        if (!registration.IsPaymentByBenefactor && activity.UnionId == 54 && priceTotal > 0)
                        {
                            var result = CardComHelper.UnionCustomPersonal_Rugby(activity, user, team, customPrices,
                                priceTotal,
                                new LeaguePlayerPrice
                                {
                                    RegistrationPrice = registrationPrice,
                                    RegistrationPriceCardComProductId = registration.RegistrationCardComProductId,
                                    InsurancePrice = insurancePrice,
                                    InsuranceCardComProductId = registration.InsuranceCardComProductId,
                                    MembersFee = membersFee,
                                    MembersFeeCardComProductId = registration.MembersFeeCardComProductId,
                                    HandlingFee = handlingFee,
                                    HandlingFeeCardComProductId = registration.HandlingFeeCardComProductId
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for wave surfing union

                        if (activity.Union?.UnionId == 41 && priceTotal > 0)
                        {
                            var result = CardComHelper.UnionCustomPersonal_WaveSurfing(activity, user, team, customPrices, priceTotal,
                                new LeaguePlayerPrice
                                {
                                    RegistrationPrice = registrationPrice,
                                    RegistrationPriceCardComProductId = registration.RegistrationCardComProductId,
                                    InsurancePrice = insurancePrice,
                                    InsuranceCardComProductId = registration.InsuranceCardComProductId,
                                    MembersFee = membersFee,
                                    MembersFeeCardComProductId = registration.MembersFeeCardComProductId,
                                    HandlingFee = handlingFee,
                                    HandlingFeeCardComProductId = registration.HandlingFeeCardComProductId
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for tennis union

                        if (activity.UnionId == 38 && priceTotal > 0)
                        {
                            var result = CardComHelper.UnionCustomPersonal_Tennis(activity, user, team, customPrices, priceTotal,
                                new LeaguePlayerPrice
                                {
                                    RegistrationPrice = registrationPrice,
                                    RegistrationPriceCardComProductId = registration.RegistrationCardComProductId,
                                    InsurancePrice = insurancePrice,
                                    InsuranceCardComProductId = registration.InsuranceCardComProductId,
                                    MembersFee = membersFee,
                                    MembersFeeCardComProductId = registration.MembersFeeCardComProductId,
                                    HandlingFee = handlingFee,
                                    HandlingFeeCardComProductId = registration.HandlingFeeCardComProductId
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for Climbing union 59

                        if (!registration.IsPaymentByBenefactor && activity.UnionId == 59 && priceTotal > 0)
                        {
                            CardComRequestResult result;

                            if (activity.RegistrationsByCompetitionsCategory)
                            {
                                result = CardComHelper.UnionCustomPersonal_Climbing59CompetitionCategory(
                                    activity, user,
                                    new List<CompetitionDiscipline> {registration.CompetitionDiscipline},
                                    new List<LeaguePlayerPrice>
                                    {
                                        new LeaguePlayerPrice
                                        {
                                            LeagueId = registration.CompetitionDiscipline?.CompetitionId ?? 0,
                                            RegistrationPrice = registrationPrice,
                                            RegistrationPriceCardComProductId =
                                                registration.RegistrationCardComProductId,
                                            InsurancePrice = insurancePrice,
                                            InsuranceCardComProductId = registration.InsuranceCardComProductId,
                                            MembersFee = membersFee,
                                            MembersFeeCardComProductId = registration.MembersFeeCardComProductId,
                                            HandlingFee = handlingFee,
                                            HandlingFeeCardComProductId = registration.HandlingFeeCardComProductId
                                        }
                                    },
                                    customPrices, priceTotal);
                            }
                            else
                            {
                                result = CardComHelper.UnionCustomPersonal_Climbing59(activity, user, team, customPrices, priceTotal,
                                    new LeaguePlayerPrice
                                    {
                                        RegistrationPrice = registrationPrice,
                                        RegistrationPriceCardComProductId = registration.RegistrationCardComProductId,
                                        InsurancePrice = insurancePrice,
                                        InsuranceCardComProductId = registration.InsuranceCardComProductId,
                                        MembersFee = membersFee,
                                        MembersFeeCardComProductId = registration.MembersFeeCardComProductId,
                                        HandlingFee = handlingFee,
                                        HandlingFeeCardComProductId = registration.HandlingFeeCardComProductId
                                    });
                            }

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for Gymnastic union 36

                        if (!registration.IsPaymentByBenefactor && activity.UnionId == 36 && priceTotal > 0)
                        {
                            var result = CardComHelper.UnionCustomPersonal_Gymnastic36(activity, user, team, customPrices, priceTotal,
                                new LeaguePlayerPrice
                                {
                                    RegistrationPrice = registrationPrice,
                                    RegistrationPriceCardComProductId = registration.RegistrationCardComProductId,
                                    InsurancePrice = insurancePrice,
                                    InsuranceCardComProductId = registration.InsuranceCardComProductId,
                                    MembersFee = membersFee,
                                    MembersFeeCardComProductId = registration.MembersFeeCardComProductId,
                                    HandlingFee = handlingFee,
                                    HandlingFeeCardComProductId = registration.HandlingFeeCardComProductId
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        break;

                    case ActivityFormType.CustomGroup:
                        priceTotal = registrationPrice + customPrices.Sum(x => Math.Max(0, x.TotalPrice - x.Paid));
                        
                        #region CardCom payment for catchball union

                        if (!registration.IsPaymentByBenefactor && activity.UnionId == 15 &&
                            priceTotal > 0)
                        {
                            var result = CardComHelper.UnionCustomTeam_Catchball(activity, user, team, customPrices, priceTotal,
                                new LeaguesPrice
                                {
                                    Price = registrationPrice,
                                    CardComProductId = registration.RegistrationCardComProductId
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for catchball 66 union

                        if (!registration.IsPaymentByBenefactor && activity.UnionId == 66 &&
                            priceTotal > 0)
                        {
                            var result = CardComHelper.UnionCustomTeam_Catchball66(activity, user, team, customPrices, priceTotal,
                                new LeaguesPrice
                                {
                                    Price = registrationPrice,
                                    CardComProductId = registration.RegistrationCardComProductId
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for wave surfing union

                        if (activity.Union?.UnionId == 41 && priceTotal > 0)
                        {
                            var result = CardComHelper.UnionCustomTeam_WaveSurfing(activity, user, team, customPrices, priceTotal,
                                new LeaguesPrice
                                {
                                    Price = registrationPrice,
                                    CardComProductId = registration.RegistrationCardComProductId
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for tennis union

                        if (activity.UnionId == 38 && priceTotal > 0)
                        {
                            var result = CardComHelper.UnionCustomTeam_Tennis(activity, user, team, customPrices, priceTotal,
                                new LeaguesPrice
                                {
                                    Price = registrationPrice,
                                    CardComProductId = registration.RegistrationCardComProductId
                                });

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        break;

                    case ActivityFormType.UnionClub:
                        priceTotal = customPrices.Sum(x => Math.Max(0, x.TotalPrice - x.Paid));
                        
                        #region CardCom payment for catchball union

                        if (!registration.IsPaymentByBenefactor && activity.UnionId == 15 &&
                            priceTotal > 0)
                        {
                            var result = CardComHelper.UnionClub_Catchball(activity, user, customPrices, priceTotal);

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for wave surfing union

                        if (activity.Union?.UnionId == 41 && priceTotal > 0)
                        {
                            var result = CardComHelper.UnionClub_WaveSurfing(activity, user, customPrices, priceTotal);

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for tennis union

                        if (activity.UnionId == 38 && priceTotal > 0)
                        {
                            var result = CardComHelper.UnionClub_Tennis(activity, user, customPrices, priceTotal);

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        #region CardCom payment for Climbing union 59

                        if (!registration.IsPaymentByBenefactor && activity.UnionId == 59 &&
                            priceTotal > 0)
                        {
                            var result = CardComHelper.UnionClub_Climbing59(activity, user, customPrices, priceTotal);

                            registration.CardComLpc = result?.Lpc;
                            redirectUrl = result?.RedirectUrl;
                        }

                        #endregion

                        break;
                }

                if (!string.IsNullOrWhiteSpace(redirectUrl))
                {
                    activityRepo.Save();

                    return Redirect(redirectUrl);
                }

                return Content("CardCom request failed. Please contact your manager");
            }

            return Content("Registration not found. Please contact your manager");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetClubTeams(int clubId, int activityId)
        {
            var activity = activityRepo.GetById(activityId);
            var club = clubsRepo.GetById(clubId);

            if (club != null && activity != null)
            {
                List<Team> teams;
                if (activity.RegisterToTrainingTeamsOnly)
                {
                    teams = club.ClubTeams.Where(x => x.SeasonId == activity.SeasonId && x.IsTrainingTeam).Select(x => x.Team).ToList();
                }
                else
                {
                    teams = teamRepo.GetClubTeamsByClubAndSeasonId(clubId, activity.SeasonId ?? 0);
                    teams.AddRange(teamRepo.GetSchoolTeamsByClubAndSeason(clubId, activity.SeasonId ?? 0));
                }

                var result = new ActivityFormGetUnionPersonalClubTeamsModel
                {
                    Teams = teams.Select(x => new ActivityFormGetUnionPersonalClubTeamsTeamModel
                    {
                        Id = x.TeamId,
                        Name = x.Title
                    }).ToList(),
                    MultiTeamEnabled = activity.MultiTeamRegistrations
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(new { error = Messages.NoDataFound }, JsonRequestBehavior.AllowGet);
        }

        private List<ActivityFormGetTeamManagerTeamModel> GetClubManagerTeamsViewModel(User user, Activity activity)
        {
            var clubManagerJob =
                user.UsersJobs.Where(x => (x.Job?.JobsRole?.RoleName == JobRole.ClubManager || x.Job?.JobsRole?.RoleName == JobRole.ClubSecretary) && x.ClubId > 0 && x.SeasonId == activity.SeasonId);

            var teams = new List<ActivityFormGetTeamManagerTeamModel>();

            foreach (var usersJob in clubManagerJob)
            {
                foreach (var clubTeam in usersJob.Club.ClubTeams.Where(x => x.SeasonId == activity.SeasonId))
                {
                    var team = clubTeam.Team;

                    foreach (var leagueTeam in clubTeam.Team.LeagueTeams.Where(
                        x => x.SeasonId == activity.SeasonId &&
                             !activity.ActivityFormsSubmittedDatas.Any(
                                 reg => reg.TeamId == x.TeamId && reg.LeagueId == x.LeagueId && reg.PlayerId == user.UserId)))
                    {
                        var league = leagueTeam.Leagues;

                        teams.Add(new ActivityFormGetTeamManagerTeamModel
                        {
                            TeamId = team?.TeamId ?? 0,
                            TeamName = HttpUtility.HtmlEncode(team?.TeamsDetails
                                                                  ?.FirstOrDefault(td => td.SeasonId == activity.SeasonId)
                                                                  ?.TeamName ?? team?.Title),
                            League = league?.Name,
                            LeagueId = league?.LeagueId ?? 0,
                            NeedShirts = team?.NeedShirts,
                            SeasonId = league?.SeasonId ?? 0,
                            TeamRegistrationPrice =
                                league?.LeaguesPrices
                                    ?.FirstOrDefault(
                                        p => p.PriceType == (int?) LeaguePriceType.TeamRegistrationPrice &&
                                             p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now)
                                    ?.Price ?? 0
                        });
                    }
                }
            }

            return teams;
        }

        private List<ActivityFormGetTeamManagerTeamModel> GetTeamManagerTeamsViewModel(User user, Activity activity)
        {
            var managerInTeams = new List<LeagueTeams>();

            var teamManagerJob =
                user.UsersJobs.Where(x => x.Job?.JobsRole?.RoleName == JobRole.TeamManager && x.TeamId > 0 && x.SeasonId == activity.SeasonId);

            foreach (var usersJob in teamManagerJob)
            {
                foreach (var leagueTeam in usersJob.Team.LeagueTeams.Where(x => x.SeasonId == activity.SeasonId))
                {
                    managerInTeams.AddRange(leagueTeam.Teams.UsersJobs
                        .Where(x => x.Job?.JobsRole?.RoleName == JobRole.TeamManager && x.UserId == user.UserId && x.SeasonId == activity.SeasonId)
                        .Where(job => !activity.ActivityFormsSubmittedDatas.Any(
                            x => x.TeamId == leagueTeam.TeamId && x.LeagueId == leagueTeam.LeagueId &&
                                 x.PlayerId == user.UserId))
                        .Select(job => leagueTeam));
                }
            }

            return managerInTeams
                .Select(x =>
                {
                    var team = x.Teams;
                    var league = x.Leagues;

                    return new ActivityFormGetTeamManagerTeamModel
                    {
                        TeamId = team?.TeamId ?? 0,
                        TeamName = team?.TeamsDetails
                                       ?.FirstOrDefault(td => td.SeasonId == activity.SeasonId)
                                       ?.TeamName ?? team?.Title,
                        League = league?.Name,
                        LeagueId = league?.LeagueId ?? 0,
                        NeedShirts = team?.NeedShirts,
                        SeasonId = league?.SeasonId ?? 0,
                        TeamRegistrationPrice =
                            league?.LeaguesPrices
                                ?.FirstOrDefault(
                                    p => p.PriceType == (int?) LeaguePriceType.TeamRegistrationPrice &&
                                         p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now)
                                ?.Price ?? 0
                    };
                })
                .ToList();
        }

        [HttpGet]
        public ActionResult CheckTeamRegistration(int teamId, int leagueId, int activityId)
        {
            var activity = activityRepo.GetById(activityId);

            if (activity == null ||
                activity.ActivityFormsSubmittedDatas.Any(x => x.TeamId == teamId && x.LeagueId == leagueId))
            {
                return Json(new { error = Messages.Activity_TeamAlreadyRegistered }, JsonRequestBehavior.AllowGet);
            }

            return null;
        }

        private void ConvertTeamsRegistrationsLeagacyPaidValues(List<ActivityFormsSubmittedData> registrations)
        {
            if (registrations.Any(x => x.Paid > 0 && x.RegistrationPaid == 0))
            {
                foreach (var reg in registrations.Where(x => x.Paid > 0 && x.RegistrationPaid == 0))
                {
                    reg.RegistrationPaid = reg.Paid;
                    reg.Paid = 0;
                }

                activityRepo.Save();
            }
        }

        [HttpGet]
        public ActionResult GetRegistrationsPage(int id)
        {
            var model = new ActivityRegistrationStatusPageModel
            {
                ActivityId = id
            };

            return View("RegistrationStatusPage", model);
        }

        [HttpGet]
        public ActionResult GetRegistrations(int id, bool showAll = false)
        {
            var dbLazyLoad = db.Configuration.LazyLoadingEnabled;
            db.Configuration.LazyLoadingEnabled = false;

            ActionResult result = null;

            var activity = db.Activities
                .Include(x => x.ActivitiesUsers)
                .Include(x => x.ActivityStatusColumnsVisibilities)
                .Include(x => x.ActivityCustomPrices)
                .Include(x => x.ActivityForms)
                .Include(x => x.ActivityForms.Select(af => af.ActivityFormsDetails))
                .Include(x => x.ActivityStatusColumnsOrders)
                .Include(x => x.ActivityStatusColumnNames)
                .Include(x => x.ActivityStatusColumnsSortings)
                .Include(x => x.Union)
                .Include(x => x.Union.Section)
                .AsNoTracking()
                .FirstOrDefault(x => x.ActivityId == id);
            int userId;

            var isTennisActivity = string.Equals(activity?.Union?.Section?.Alias, SectionAliases.Tennis,
                StringComparison.CurrentCultureIgnoreCase);

            if (activity != null && int.TryParse(User.Identity.Name, out userId))
            {
                if (!activity.SeasonId.HasValue)
                {
                    return Content("Activity: Unknown season");
                }

                var truncate = false;
                var formsData = db.ActivityFormsSubmittedDatas
                    .Where(x => x.ActivityId == id)
                    .OrderByDescending(x => x.DateSubmitted)
                    .ToList();

                db.CompetitionDisciplines
                    .Join(db.ActivityFormsSubmittedDatas.Where(x => x.ActivityId == id), x => x.Id, x => x.CompetitionDisciplineId,
                        (competitionDiscipline, reg) => competitionDiscipline)
                    .Load();
                db.Leagues
                    .Where(x => x.SeasonId == activity.SeasonId)
                    .Join(db.CompetitionDisciplines
                        .Join(db.ActivityFormsSubmittedDatas.Where(x => x.ActivityId == id),
                            x => x.Id,
                            x => x.CompetitionDisciplineId,
                            (competitionDiscipline, regionalsRepo) => competitionDiscipline),
                        x => x.LeagueId,
                        x => x.CompetitionId,
                        (league, competitionDiscipline) => league)
                    .Load();
                db.CompetitionAges
                    .Where(x => x.SeasonId == activity.SeasonId)
                    .Join(db.CompetitionDisciplines
                        .Join(db.ActivityFormsSubmittedDatas.Where(x => x.ActivityId == id),
                            x => x.Id,
                            x => x.CompetitionDisciplineId,
                            (competitionDiscipline, regionalsRepo) => competitionDiscipline),
                        x => x.id,
                        x => x.CategoryId,
                        (competitionAge, competitionDiscipline) => competitionAge)
                    .Load();

                db.Clubs
                    .Join(db.ActivityFormsSubmittedDatas.Where(x => x.ActivityId == id), x => x.ClubId, x => x.ClubId,
                        (club, reg) => club)
                    .Include(x => x.Regional)
                    .Include(x => x.SportCenter)
                    .Load();

                db.Leagues
                    .Join(db.ActivityFormsSubmittedDatas.Where(x => x.ActivityId == id), x => x.LeagueId, x => x.LeagueId,
                        (league, reg) => league)
                    .Load();
                db.LeaguesPrices
                    .Join(db.Leagues
                            .Where(x => x.SeasonId == activity.SeasonId)
                            .Join(db.ActivityFormsSubmittedDatas.Where(x => x.ActivityId == id),
                                x => x.LeagueId,
                                x => x.LeagueId,
                                (league, reg) => league),
                        x => x.LeagueId,
                        x => x.LeagueId,
                        (lPrice, team) => lPrice)
                    .Load();

                db.Teams
                    .Join(db.ActivityFormsSubmittedDatas.Where(x => x.ActivityId == id), x => x.TeamId, x => x.TeamId,
                        (team, reg) => team)
                    .Load();
                db.SchoolTeams
                    .Join(db.Teams
                            .Join(db.ActivityFormsSubmittedDatas.Where(x => x.ActivityId == id), 
                                x => x.TeamId,
                                x => x.TeamId,
                                (team, reg) => team),
                        x => x.TeamId,
                        x => x.TeamId,
                        (schoolTeam, team) => schoolTeam)
                    .Include(x => x.School)
                    .Load();

                db.TeamsDetails
                    .Where(x => x.SeasonId == activity.SeasonId)
                    .Join(db.Teams
                            .Join(db.ActivityFormsSubmittedDatas.Where(x => x.ActivityId == id),
                                x => x.TeamId,
                                x => x.TeamId,
                                (team, reg) => team),
                        x => x.TeamId,
                        x => x.TeamId,
                        (details, team) => details)
                    .Load();

                db.Activities
                    .Where(x => x.ActivityId == id)
                    .Load();

                db.Users
                    .Join(db.ActivityFormsSubmittedDatas.Where(x => x.ActivityId == id), x => x.UserId, x => x.PlayerId, (user, reg) => user)
                    .Load();
                db.Users
                    .Join(db.ActivityFormsSubmittedDatas.Where(x => x.ActivityId == id), x => x.UserId, x => x.BrotherUserId, (brotherUser, reg) => brotherUser)
                    .Load();

                db.PlayerFiles
                    .Join(db.ActivityFormsSubmittedDatas.Where(x => x.ActivityId == id), x => x.PlayerId, x => x.PlayerId, (file, reg) => file)
                    .Where(x => x.SeasonId == activity.SeasonId)
                    .Load();

                db.MedicalCertApprovements
                    .Join(db.ActivityFormsSubmittedDatas.Where(x => x.ActivityId == id), x => x.UserId, x => x.PlayerId, (medCert, reg) => medCert)
                    .Where(x => x.SeasonId == activity.SeasonId)
                    .Load();

                db.TeamsPlayers
                    .Join(db.ActivityFormsSubmittedDatas.Where(x => x.ActivityId == id), x => x.UserId, x => x.PlayerId,
                        (tp, reg) => tp)
                    .Where(x => x.SeasonId == activity.SeasonId)
                    .Load();

                //if (!showAll)
                //{
                //    if (formsData.Count > GlobVars.ActivityStatusTruncateThreshold)
                //    {
                //        truncate = true;

                //        if (formsData.Count(x => !x.IsActive) > GlobVars.ActivityStatusTruncateThreshold)
                //        {
                //            formsData = formsData
                //                .Where(x => !x.IsActive)
                //                .OrderByDescending(x => x.DateSubmitted)
                //                .ToList();
                //        }
                //        else
                //        {
                //            formsData = formsData
                //                .OrderBy(x => x.IsActive)
                //                .ThenByDescending(x => x.DateSubmitted)
                //                .Take(GlobVars.ActivityStatusTruncateThreshold)
                //                .ToList();
                //        }
                //    }
                //}

                if (activity.GetFormType() == ActivityFormType.TeamRegistration)
                {
                    ConvertTeamsRegistrationsLeagacyPaidValues(formsData);
                }

                var vm = new ActivityRegistrationsStatusModel
                {
                    ActivityId = activity.ActivityId,
                    Activity = activity,
                    SeasonId = activity.SeasonId ?? 0,
                    BenefactorEnabled = activity.ByBenefactor,
                    DocumentEnabled = activity.AttachDocuments,
                    MedicalCertEnabled = activity.MedicalCertificate,
                    InsuranceCertEnabled = activity.InsuranceCertificate,
                    HiddenColumns = activity.ActivityStatusColumnsVisibilities
                        .Where(x => !x.Visible && x.UserId == AdminId).Select(x => x.ColumnIndex).ToArray(),

                    Title =
                        $"{activity.Name} {string.Format(Messages.Activity_RegistrationsCount, formsData.Count(x => !x.IsActive), formsData.Count(x => x.IsActive))}",

                    CanEdit = User.IsInAnyRole(AppRole.Admins) ||
                              User.HasTopLevelJob(JobRole.UnionManager) ||
                              User.HasTopLevelJob(JobRole.ClubManager) ||
                              User.HasTopLevelJob(JobRole.ClubSecretary) || 
                              activity.ActivitiesUsers.Any(q => q.UserId == AdminId && q.UserGroup == 0),
                    CanPay = User.IsInAnyRole(AppRole.Admins) ||
                              User.HasTopLevelJob(JobRole.UnionManager) ||
                              User.HasTopLevelJob(JobRole.ClubManager) ||
                              activity.ActivitiesUsers.Any(q => q.UserId == AdminId && q.UserGroup == 0),
                    CanDelete = User.IsInAnyRole(AppRole.Admins) ||
                                User.HasTopLevelJob(JobRole.UnionManager) ||
                                User.HasTopLevelJob(JobRole.ClubSecretary) ||
                                User.HasTopLevelJob(JobRole.ClubManager),
                    CanApproveMedicalCert = User.IsInAnyRole(AppRole.Admins) ||
                                            User.HasTopLevelJob(JobRole.UnionManager) ||
                                            User.HasTopLevelJob(JobRole.ClubManager) ||
                                            User.HasTopLevelJob(JobRole.ClubSecretary) ||
                                            activity.ActivitiesUsers.Any(q => q.UserId == AdminId && q.UserGroup == 0),
                    CanLockUnlockPlayers = User.IsInAnyRole(AppRole.Admins) ||
                                           User.HasTopLevelJob(JobRole.UnionManager),
                    Culture = getCulture(),
                    FormFields = activity.ActivityForms.FirstOrDefault()?.ActivityFormsDetails?.ToList() ??
                                 new List<ActivityFormsDetail>(),
                    ActivityCustomPrices = activity.ActivityCustomPrices.ToList(),

                    IsDataTruncated = truncate,
                    ColumnsOrder =
                        activity.ActivityStatusColumnsOrders.FirstOrDefault(x => x.UserId == userId)?.Columns,
                    ColumnsNames = JsonConvert.SerializeObject(activity.ActivityStatusColumnNames.Where(x => x.Language == getCulture().ToString() && x.UserId == userId)
                        .Select(x => new
                        {
                            index = x.ColumnIndex,
                            name = x.ColumnName
                        }).ToList()),
                    ColumnsSorting = activity.ActivityStatusColumnsSortings.FirstOrDefault(x => x.UserId == userId)?.Sorting,

                    IsRegionalLevelEnabled = activity.Union?.IsRegionallevelEnabled == true
                };

                var allBuiltInRegistrations = activityRepo.GetCollection<ActivityFormsSubmittedData>(
                        x => x.Activity.IsAutomatic == true &&
                             x.Activity.Type == ActivityType.Personal &&
                             x.Activity.SeasonId == activity.SeasonId &&
                             x.Activity.UnionId == activity.UnionId &&
                             x.Activity.ClubId == activity.ClubId &&
                             x.IsActive)
                    .ToList();

                foreach (var reg in formsData)
                {
                    var customFields = !string.IsNullOrWhiteSpace(reg.CustomFields)
                        ? JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(reg.CustomFields)
                        : new List<ActivityFormCustomField>();

                    if (customFields.Any())
                    {
                        foreach (var checkbox in customFields.Where(
                            x => x.Type == ActivityFormControlType.CustomCheckBox))
                        {
                            bool checkboxValue;
                            if (bool.TryParse(checkbox.Value, out checkboxValue))
                            {
                                checkbox.Value = checkboxValue ? Messages.Yes : Messages.No;
                            }
                        }

                        reg.CustomFields = JsonConvert.SerializeObject(customFields);
                    }
                }

                switch (activity.GetFormType())
                {
                    case ActivityFormType.TeamRegistration:
                        vm.Registrations = formsData.Select(x =>
                            {
                                var teamPlayer = GetTeamPlayer(x);

                                return new ActivityRegistrationItem
                                {
                                    Id = x.Id,
                                    UserId = x.User.UserId,
                                    UserIdNum = x.User.IdentNum,
                                    Document = x.Document,
                                    InsuranceCert = x.InsuranceCert,
                                    PaymentByBenefactor = x.PaymentByBenefactor,
                                    //NameForInvoice = x.NameForInvoice,
                                    NeedShirts = x.Team.NeedShirts ?? false,
                                    Comments = x.Comments,
                                    PlayerEmail = x.User.Email,
                                    DateSubmitted = x.DateSubmitted,
                                    PlayerPhone = x.User.Telephone,
                                    PlayerBirthDate = x.User.BirthDay,
                                    Team = x.Team.TeamsDetails.FirstOrDefault(td => td.SeasonId == activity.SeasonId)
                                               ?.TeamName ?? x.Team.Title,
                                    TeamId = x.TeamId,
                                    League = x.League.Name,
                                    LeagueId = x.LeagueId,
                                    PlayerFullName = x.User.FullName,
                                    PlayerFirstName = x.User.FirstName,
                                    PlayerLastName = x.User.LastName,
                                    PlayerMiddleName = x.User.MiddleName,
                                    MedicalCert = x.MedicalCert,
                                    SelfInsurance = x.SelfInsurance,
                                    PlayerAddress = x.User.City,
                                    IsActive = x.IsActive,

                                    //LeaguePrice = x.IsPaymentByBenefactor
                                    //    ? (decimal?)null
                                    //    : x.RegistrationPrice,

                                    //ByBenefactorPrice = x.IsPaymentByBenefactor
                                    //    ? x.RegistrationPrice
                                    //    : (decimal?)null,
                                    LeaguePrice = x.IsPaymentByBenefactor
                                        ? (decimal?) null
                                        : (x.League?.LeaguesPrices?.FirstOrDefault(
                                                   lp => lp.PriceType == (int?) LeaguePriceType.TeamRegistrationPrice &&
                                                         lp.StartDate <= x.DateSubmitted &&
                                                         lp.EndDate >= x.DateSubmitted)
                                               ?.Price ?? 0),

                                    ByBenefactorPrice = x.IsPaymentByBenefactor
                                        ? (x.League?.LeaguesPrices?.FirstOrDefault(
                                                   lp => lp.PriceType == (int?) LeaguePriceType.TeamRegistrationPrice &&
                                                         lp.StartDate <= x.DateSubmitted &&
                                                         lp.EndDate >= x.DateSubmitted)
                                               ?.Price ?? 0)
                                        : (decimal?) null,
                                    RegistrationPaid = x.RegistrationPaid,
                                    Paid = x.Paid,
                                    UnionComment = x.UnionComment,
                                    //Paid = activity.Union?.Section?.Alias == SectionAliases.Netball
                                    //    ? (x.CardComPaymentCompleted ? leagueRegPrice : 0)
                                    //    : 0
                                    CustomFields = !string.IsNullOrWhiteSpace(x.CustomFields)
                                        ? JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(x.CustomFields)
                                        : new List<ActivityFormCustomField>(),

                                    CustomPrices = !string.IsNullOrWhiteSpace(x.CustomPrices)
                                        ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(x.CustomPrices)
                                        : new List<ActivityCustomPriceModel>(),

                                    ProfilePicture = x.User.PlayerFiles
                                                         .FirstOrDefault(
                                                             f => f.SeasonId == activity.SeasonId && f.FileType ==
                                                                  (int) PlayerFileType.PlayerImage)
                                                         ?.FileName ?? x.User.Image,
                                    IsLocked = IsPlayerLocked(x, teamPlayer),
                                    TeamPlayerId = teamPlayer?.Id,

                                    RegisteredMoreThanOnce = formsData.Count(f => f.PlayerId == x.PlayerId) > 1,

                                    CardComNumberOfPayments = x.CardComNumberOfPayments,
                                    CardComInvoiceNumber = x.CardComInvoiceNumber
                                };
                            })
                            .ToList();
                        result = PartialView("StatusesByType/_TeamsRegistrationStatusData", vm);
                        break;

                    case ActivityFormType.PlayerRegistration:

                        if (formsData.Any(x => x.Paid > 0 && x.RegistrationPaid == 0 && x.InsurancePaid == 0 &&
                                               x.Paid == x.RegistrationPrice + x.InsurancePrice))
                        {
                            foreach (var reg in formsData.Where(
                                x => x.Paid > 0 && x.RegistrationPaid == 0 && x.InsurancePaid == 0 &&
                                     x.Paid == x.RegistrationPrice + x.InsurancePrice))
                            {
                                var regPaid = reg.Paid - reg.InsurancePrice;
                                var insPaid = reg.Paid - reg.RegistrationPrice;

                                reg.RegistrationPaid = regPaid;
                                reg.InsurancePaid = insPaid;
                                reg.Paid = 0;
                            }

                            activityRepo.Save();
                        }

                        //var teamsIds = formsData.Select(f => f.TeamId).ToList();
                        //var playersIds = formsData.Select(f => f.PlayerId).ToList();

                        //var teamsDetails = teamRepo
                        //    .GetCollection<TeamsDetails>(
                        //        x => x.SeasonId == activity.SeasonId && teamsIds.Contains(x.TeamId)).ToList();
                        //var playersFiles = playersRepo
                        //    .GetCollection<PlayerFile>(x => x.SeasonId == activity.SeasonId &&
                        //                                    playersIds.Contains(x.PlayerId))
                        //    .ToList();

                        vm.Registrations = formsData.Select(x =>
                            {
                                var teamPlayer = GetTeamPlayer(x);

                                return new ActivityRegistrationItem
                                {
                                    Id = x.Id,
                                    UserId = x.User.UserId,
                                    UserIdNum = x.User.IdentNum,
                                    Document = x.Document,
                                    PaymentByBenefactor = x.PaymentByBenefactor,
                                    //NameForInvoice = x.NameForInvoice,
                                    Comments = x.Comments,
                                    PlayerEmail = x.User.Email,
                                    DateSubmitted = x.DateSubmitted,
                                    PlayerPhone = x.User.Telephone,
                                    PlayerBirthDate = x.User.BirthDay,
                                    Team = x.Team.TeamsDetails.FirstOrDefault(td => td.SeasonId == activity.SeasonId)?.TeamName ??
                                           x.Team.Title,
                                    TeamId = x.TeamId,
                                    League = x.League.Name,
                                    LeagueId = x.LeagueId,
                                    PlayerFullName = x.User.FullName,
                                    PlayerFirstName = x.User.FirstName,
                                    PlayerLastName = x.User.LastName,
                                    PlayerMiddleName = x.User.MiddleName,
                                    PlayerGender = x.User.GenderId == 0 ? Messages.Female : x.User.GenderId == 1 ? Messages.Male : string.Empty,
                                    PlayerAddress = x.User.Address,
                                    PlayerCity = x.User.City,
                                    IsActive = x.IsActive,

                                    InsuranceCert = x.User.PlayerFiles
                                        .FirstOrDefault(
                                            f => f.PlayerId == x.PlayerId &&
                                                 f.SeasonId == activity.SeasonId &&
                                                 f.FileType == (int) PlayerFileType.Insurance)
                                        ?.FileName,

                                    MedicalCert = x.User.PlayerFiles
                                        .FirstOrDefault(
                                            f => f.PlayerId == x.PlayerId &&
                                                 f.SeasonId == activity.SeasonId &&
                                                 f.FileType == (int) PlayerFileType.MedicalCertificate && !f.IsArchive) 
                                        ?.FileName,
                                    MedicalCertApproved = x.User.MedicalCertApprovements.FirstOrDefault	(c => c.SeasonId == activity.SeasonId)?.Approved == true,

                                    LeaguePrice = x.IsPaymentByBenefactor
                                        ? (decimal?) null
                                        : x.RegistrationPrice,

                                    ByBenefactorPrice = x.IsPaymentByBenefactor
                                        ? x.RegistrationPrice
                                        : (decimal?) null,

                                    Paid = x.Paid,
                                    UnionComment = x.UnionComment,
                                    //Paid = activity.Union?.Section?.Alias == SectionAliases.Netball
                                    //    ? (x.CardComPaymentCompleted ? leagueRegPrice : 0)
                                    //    : 0
                                    InsurancePrice = x.InsurancePrice,

                                    InsurancePaid = x.InsurancePaid,
                                    RegistrationPaid = x.RegistrationPaid,

                                    CustomFields = !string.IsNullOrWhiteSpace(x.CustomFields)
                                        ? JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(x
                                            .CustomFields)
                                        : new List<ActivityFormCustomField>(),

                                    CustomPrices = !string.IsNullOrWhiteSpace(x.CustomPrices)
                                        ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(x.CustomPrices)
                                        : new List<ActivityCustomPriceModel>(),

                                    ProfilePicture = x.User.PlayerFiles
                                                         .FirstOrDefault(
                                                             f => f.PlayerId == x.PlayerId &&
                                                                  f.SeasonId == activity.SeasonId && f.FileType ==
                                                                  (int) PlayerFileType.PlayerImage)
                                                         ?.FileName ?? x.User.Image,
                                    IsLocked = IsPlayerLocked(x, teamPlayer),
                                    TeamPlayerId = teamPlayer?.Id,

                                    RegisteredMoreThanOnce = formsData.Count(f => f.PlayerId == x.PlayerId) > 1,

                                    CardComNumberOfPayments = x.CardComNumberOfPayments,
                                    CardComInvoiceNumber = x.CardComInvoiceNumber
                                };
                            })
                            .ToList();
                        result = PartialView("StatusesByType/_PlayerRegistrationStatusData", vm);
                        break;

                    case ActivityFormType.ClubPlayerRegistration:
                        vm.Registrations = formsData.Select(x =>
                            {
                                var teamPlayer = GetTeamPlayer(x);

                                return new ActivityRegistrationItem
                                {
                                    Id = x.Id,
                                    UserId = x.User.UserId,
                                    UserIdNum = x.User.IdentNum,
                                    BrotherIdNum = x.User1?.IdentNum,
                                    Document = x.Document,
                                    PaymentByBenefactor = x.PaymentByBenefactor,
                                    //NameForInvoice = x.NameForInvoice,
                                    Comments = x.Comments,
                                    PlayerEmail = x.User.Email,
                                    DateSubmitted = x.DateSubmitted,
                                    PlayerPhone = x.User.Telephone,
                                    PlayerBirthDate = x.User.BirthDay,
                                    Team = x.Team.TeamsDetails
                                               .FirstOrDefault(td => td.SeasonId == activity.SeasonId)
                                               ?.TeamName ?? x.Team.Title,
                                    TeamId = x.TeamId,
                                    Club = x.Club.Name,
                                    ClubId = x.ClubId,
                                    School = x.Team.SchoolTeams.FirstOrDefault()?.School?.Name,
                                    PlayerFullName = x.User.FullName,
                                    PlayerFirstName = x.User.FirstName,
                                    PlayerLastName = x.User.LastName,
                                    PlayerMiddleName = x.User.MiddleName,
                                    PlayerGender = x.User.GenderId == 0 ? Messages.Female : x.User.GenderId == 1 ? Messages.Male : string.Empty,
                                    PlayerAddress = x.User.Address,
                                    PlayerCity = x.User.City,
                                    IsActive = x.IsActive,

                                    InsuranceCert = x.User.PlayerFiles
                                        .FirstOrDefault(
                                            f => f.SeasonId == activity.SeasonId &&
                                                 f.FileType == (int) PlayerFileType.Insurance)
                                        ?.FileName,

                                    MedicalCert = x.User.PlayerFiles
                                        .FirstOrDefault(
                                            f => f.SeasonId == activity.SeasonId &&
                                                 f.FileType == (int) PlayerFileType.MedicalCertificate && !f.IsArchive)
                                        ?.FileName,
                                    MedicalCertApproved = x.User.MedicalCertApprovements.FirstOrDefault (c => c.SeasonId == activity.SeasonId)?.Approved == true,

                                    LeaguePrice = x.IsPaymentByBenefactor
                                        ? (decimal?) null
                                        : x.RegistrationPrice,

                                    ByBenefactorPrice = x.IsPaymentByBenefactor
                                        ? x.RegistrationPrice
                                        : (decimal?) null,

                                    Paid = x.Paid,
                                    ClubComment = x.ClubComment,
                                    //Paid = activity.Union?.Section?.Alias == SectionAliases.Netball
                                    //    ? (x.CardComPaymentCompleted ? leagueRegPrice : 0)
                                    //    : 0
                                    InsurancePrice = x.InsurancePrice,
                                    ParticipationPrice = x.ParticipationPrice,

                                    InsurancePaid = x.InsurancePaid,
                                    RegistrationPaid = x.RegistrationPaid,
                                    ParticipationPaid = x.ParticipationPaid,

                                    CustomFields = !string.IsNullOrWhiteSpace(x.CustomFields)
                                        ? JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(x
                                            .CustomFields)
                                        : new List<ActivityFormCustomField>(),

                                    CustomPrices = !string.IsNullOrWhiteSpace(x.CustomPrices)
                                        ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(x.CustomPrices)
                                        : new List<ActivityCustomPriceModel>(),

                                    ProfilePicture = x.User.PlayerFiles
                                                         .FirstOrDefault(
                                                             f => f.SeasonId == activity.SeasonId && f.FileType ==
                                                                  (int) PlayerFileType.PlayerImage)
                                                         ?.FileName ?? x.User.Image,
                                    IsLocked = IsPlayerLocked(x, teamPlayer),
                                    TeamPlayerId = teamPlayer?.Id,

                                    RegisteredMoreThanOnce = formsData.Count(f => f.PlayerId == x.PlayerId) > 1,

                                    CardComNumberOfPayments = x.CardComNumberOfPayments,
                                    CardComInvoiceNumber = x.CardComInvoiceNumber,

                                    PlayerStartDate = x.PlayerStartDate
                                };
                            })
                            .ToList();
                        result = PartialView("StatusesByType/_ClubPlayerRegistrationStatusData", vm);
                        break;

                    case ActivityFormType.ClubCustomPersonal:
                        vm.Registrations = formsData.Select(x =>
                            {
                                var teamPlayer = GetTeamPlayer(x);

                                return new ActivityRegistrationItem
                                {
                                    Id = x.Id,
                                    UserId = x.User.UserId,
                                    UserIdNum = x.User.IdentNum,
                                    BrotherIdNum = x.User1?.IdentNum,
                                    Document = x.Document,
                                    PaymentByBenefactor = x.PaymentByBenefactor,
                                    //NameForInvoice = x.NameForInvoice,
                                    Comments = x.Comments,
                                    PlayerEmail = x.User.Email,
                                    DateSubmitted = x.DateSubmitted,
                                    PlayerPhone = x.User.Telephone,
                                    PlayerBirthDate = x.User.BirthDay,
                                    Team = x.Team?.TeamsDetails
                                               .FirstOrDefault(td => td.SeasonId == activity.SeasonId)
                                               ?.TeamName ?? x.Team?.Title,
                                    TeamId = x.TeamId,
                                    Club = x.Club.Name,
                                    ClubId = x.ClubId,
                                    School = x.Team?.SchoolTeams.FirstOrDefault()?.School?.Name,
                                    PlayerFullName = x.User.FullName,
                                    PlayerFirstName = x.User.FirstName,
                                    PlayerLastName = x.User.LastName,
                                    PlayerMiddleName = x.User.MiddleName,
                                    PlayerGender = x.User.GenderId == 0 ? Messages.Female : x.User.GenderId == 1 ? Messages.Male : string.Empty,
                                    PlayerAddress = x.User.City,
                                    IsActive = x.IsActive,

                                    InsuranceCert = x.User.PlayerFiles
                                        .FirstOrDefault(
                                            f => f.SeasonId == activity.SeasonId &&
                                                 f.FileType == (int) PlayerFileType.Insurance)
                                        ?.FileName,

                                    MedicalCert = x.User.PlayerFiles
                                        .FirstOrDefault(
                                            f => f.SeasonId == activity.SeasonId &&
                                                 f.FileType == (int) PlayerFileType.MedicalCertificate && !f.IsArchive)
                                        ?.FileName,
                                    MedicalCertApproved = x.User.MedicalCertApprovements.FirstOrDefault (c => c.SeasonId == activity.SeasonId)?.Approved == true,

                                    LeaguePrice = x.IsPaymentByBenefactor
                                        ? (decimal?) null
                                        : x.RegistrationPrice,

                                    ByBenefactorPrice = x.IsPaymentByBenefactor
                                        ? x.RegistrationPrice
                                        : (decimal?) null,

                                    Paid = x.Paid,
                                    ClubComment = x.ClubComment,
                                    //Paid = activity.Union?.Section?.Alias == SectionAliases.Netball
                                    //    ? (x.CardComPaymentCompleted ? leagueRegPrice : 0)
                                    //    : 0
                                    InsurancePrice = x.InsurancePrice,
                                    ParticipationPrice = x.ParticipationPrice,

                                    InsurancePaid = x.InsurancePaid,
                                    RegistrationPaid = x.RegistrationPaid,
                                    ParticipationPaid = x.ParticipationPaid,

                                    CustomFields = !string.IsNullOrWhiteSpace(x.CustomFields)
                                        ? JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(x
                                            .CustomFields)
                                        : new List<ActivityFormCustomField>(),

                                    CustomPrices = !string.IsNullOrWhiteSpace(x.CustomPrices)
                                        ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(x.CustomPrices)
                                        : new List<ActivityCustomPriceModel>(),

                                    ProfilePicture = x.User.PlayerFiles
                                                         .FirstOrDefault(
                                                             f => f.SeasonId == activity.SeasonId && f.FileType ==
                                                                  (int) PlayerFileType.PlayerImage)
                                                         ?.FileName ?? x.User.Image,

                                    DisableParticipationPayment = x.DisableParticipationPayment,
                                    DisableInsurancePayment = x.DisableInsurancePayment,
                                    IsLocked = IsPlayerLocked(x, teamPlayer),
                                    TeamPlayerId = teamPlayer?.Id,

                                    RegisteredMoreThanOnce = formsData.Count(f => f.PlayerId == x.PlayerId) > 1,

                                    CardComNumberOfPayments = x.CardComNumberOfPayments,
                                    CardComInvoiceNumber = x.CardComInvoiceNumber,

                                    PlayerStartDate = x.PlayerStartDate
                                };
                            })
                            .ToList();
                        ViewBag.AllowExportMonthlyPayments = false;
                        if (activity.RestrictSchools || activity.RestrictTeams)
                        {
                            ViewBag.AllowExportMonthlyPayments = true;
                        }

                        result = PartialView("StatusesByType/_ClubCustomPlayerRegistrationStatusData", vm);
                        break;

                    case ActivityFormType.DepartmentClubPlayerRegistration:
                        vm.Registrations = formsData.Select(x =>
                            {
                                var teamPlayer = GetTeamPlayer(x);

                                return new ActivityRegistrationItem
                                {
                                    Id = x.Id,
                                    UserId = x.User.UserId,
                                    UserIdNum = x.User.IdentNum,
                                    Document = x.Document,
                                    PaymentByBenefactor = x.PaymentByBenefactor,
                                    //NameForInvoice = x.NameForInvoice,
                                    Comments = x.Comments,
                                    PlayerEmail = x.User.Email,
                                    DateSubmitted = x.DateSubmitted,
                                    PlayerPhone = x.User.Telephone,
                                    PlayerBirthDate = x.User.BirthDay,
                                    Team = x.Team.TeamsDetails
                                               .FirstOrDefault(td => td.SeasonId == activity.SeasonId)
                                               ?.TeamName ?? x.Team.Title,
                                    TeamId = x.TeamId,
                                    Club = x.Club.Name,
                                    ClubId = x.ClubId,
                                    School = x.Team.SchoolTeams.FirstOrDefault()?.School?.Name,
                                    PlayerFullName = x.User.FullName,
                                    PlayerFirstName = x.User.FirstName,
                                    PlayerLastName = x.User.LastName,
                                    PlayerMiddleName = x.User.MiddleName,
                                    PlayerGender = x.User.GenderId == 0 ? Messages.Female : x.User.GenderId == 1 ? Messages.Male : string.Empty,
                                    PlayerAddress = x.User.City,
                                    IsActive = x.IsActive,

                                    InsuranceCert = x.User.PlayerFiles
                                        .FirstOrDefault(
                                            f => f.SeasonId == activity.SeasonId &&
                                                 f.FileType == (int) PlayerFileType.Insurance)
                                        ?.FileName,

                                    MedicalCert = x.User.PlayerFiles
                                        .FirstOrDefault(
                                            f => f.SeasonId == activity.SeasonId &&
                                                 f.FileType == (int) PlayerFileType.MedicalCertificate && !f.IsArchive)
                                        ?.FileName,
                                    MedicalCertApproved = x.User.MedicalCertApprovements.FirstOrDefault (c => c.SeasonId == activity.SeasonId)?.Approved == true,

                                    LeaguePrice = x.IsPaymentByBenefactor
                                        ? (decimal?) null
                                        : x.RegistrationPrice,

                                    ByBenefactorPrice = x.IsPaymentByBenefactor
                                        ? x.RegistrationPrice
                                        : (decimal?) null,

                                    Paid = x.Paid,
                                    ClubComment = x.ClubComment,
                                    //Paid = activity.Union?.Section?.Alias == SectionAliases.Netball
                                    //    ? (x.CardComPaymentCompleted ? leagueRegPrice : 0)
                                    //    : 0
                                    InsurancePrice = x.InsurancePrice,
                                    ParticipationPrice = x.ParticipationPrice,

                                    InsurancePaid = x.InsurancePaid,
                                    RegistrationPaid = x.RegistrationPaid,
                                    ParticipationPaid = x.ParticipationPaid,

                                    CustomFields = !string.IsNullOrWhiteSpace(x.CustomFields)
                                        ? JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(x
                                            .CustomFields)
                                        : new List<ActivityFormCustomField>(),

                                    CustomPrices = !string.IsNullOrWhiteSpace(x.CustomPrices)
                                        ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(x.CustomPrices)
                                        : new List<ActivityCustomPriceModel>(),

                                    ProfilePicture = x.User.PlayerFiles
                                                         .FirstOrDefault(
                                                             f => f.SeasonId == activity.SeasonId && f.FileType ==
                                                                  (int) PlayerFileType.PlayerImage)
                                                         ?.FileName ?? x.User.Image,
                                    IsLocked = IsPlayerLocked(x, teamPlayer),
                                    TeamPlayerId = teamPlayer?.Id,

                                    RegisteredMoreThanOnce = formsData.Count(f => f.PlayerId == x.PlayerId) > 1,

                                    CardComNumberOfPayments = x.CardComNumberOfPayments,
                                    CardComInvoiceNumber = x.CardComInvoiceNumber
                                };
                            })
                            .ToList();
                        result = PartialView("StatusesByType/_DepartmentClubCustomPlayerRegistrationStatusData", vm);
                        break;

                    case ActivityFormType.DepartmentClubCustomPersonal:
                        vm.Registrations = formsData.Select(x =>
                            {
                                var teamPlayer = GetTeamPlayer(x);

                                return new ActivityRegistrationItem
                                {
                                    Id = x.Id,
                                    UserId = x.User.UserId,
                                    UserIdNum = x.User.IdentNum,
                                    Document = x.Document,
                                    PaymentByBenefactor = x.PaymentByBenefactor,
                                    //NameForInvoice = x.NameForInvoice,
                                    Comments = x.Comments,
                                    PlayerEmail = x.User.Email,
                                    DateSubmitted = x.DateSubmitted,
                                    PlayerPhone = x.User.Telephone,
                                    PlayerBirthDate = x.User.BirthDay,
                                    Team = x.Team?.TeamsDetails
                                               .FirstOrDefault(td => td.SeasonId == activity.SeasonId)
                                               ?.TeamName ?? x.Team?.Title,
                                    TeamId = x.TeamId,
                                    Club = x.Club.Name,
                                    ClubId = x.ClubId,
                                    School = x.Team?.SchoolTeams.FirstOrDefault()?.School?.Name,
                                    PlayerFullName = x.User.FullName,
                                    PlayerFirstName = x.User.FirstName,
                                    PlayerLastName = x.User.LastName,
                                    PlayerMiddleName = x.User.MiddleName,
                                    PlayerGender = x.User.GenderId == 0 ? Messages.Female : x.User.GenderId == 1 ? Messages.Male : string.Empty,
                                    PlayerAddress = x.User.City,
                                    IsActive = x.IsActive,

                                    InsuranceCert = x.User.PlayerFiles
                                        .FirstOrDefault(
                                            f => f.SeasonId == activity.SeasonId &&
                                                 f.FileType == (int) PlayerFileType.Insurance)
                                        ?.FileName,

                                    MedicalCert = x.User.PlayerFiles
                                        .FirstOrDefault(
                                            f => f.SeasonId == activity.SeasonId &&
                                                 f.FileType == (int) PlayerFileType.MedicalCertificate && !f.IsArchive)
                                        ?.FileName,
                                    MedicalCertApproved = x.User.MedicalCertApprovements.FirstOrDefault (c => c.SeasonId == activity.SeasonId)?.Approved == true,

                                    LeaguePrice = x.IsPaymentByBenefactor
                                        ? (decimal?) null
                                        : x.RegistrationPrice,

                                    ByBenefactorPrice = x.IsPaymentByBenefactor
                                        ? x.RegistrationPrice
                                        : (decimal?) null,

                                    Paid = x.Paid,
                                    ClubComment = x.ClubComment,
                                    //Paid = activity.Union?.Section?.Alias == SectionAliases.Netball
                                    //    ? (x.CardComPaymentCompleted ? leagueRegPrice : 0)
                                    //    : 0
                                    InsurancePrice = x.InsurancePrice,
                                    ParticipationPrice = x.ParticipationPrice,

                                    InsurancePaid = x.InsurancePaid,
                                    RegistrationPaid = x.RegistrationPaid,
                                    ParticipationPaid = x.ParticipationPaid,

                                    CustomFields = !string.IsNullOrWhiteSpace(x.CustomFields)
                                        ? JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(x
                                            .CustomFields)
                                        : new List<ActivityFormCustomField>(),

                                    CustomPrices = !string.IsNullOrWhiteSpace(x.CustomPrices)
                                        ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(x.CustomPrices)
                                        : new List<ActivityCustomPriceModel>(),

                                    ProfilePicture = x.User.PlayerFiles
                                                         .FirstOrDefault(
                                                             f => f.SeasonId == activity.SeasonId && f.FileType ==
                                                                  (int) PlayerFileType.PlayerImage)
                                                         ?.FileName ?? x.User.Image,

                                    DisableParticipationPayment = x.DisableParticipationPayment,
                                    DisableInsurancePayment = x.DisableInsurancePayment,
                                    IsLocked = IsPlayerLocked(x, teamPlayer),
                                    TeamPlayerId = teamPlayer?.Id,

                                    RegisteredMoreThanOnce = formsData.Count(f => f.PlayerId == x.PlayerId) > 1,

                                    CardComNumberOfPayments = x.CardComNumberOfPayments,
                                    CardComInvoiceNumber = x.CardComInvoiceNumber
                                };
                            })
                            .ToList();
                        result = PartialView("StatusesByType/_DepartmentClubCustomPlayerRegistrationStatusData", vm);
                        break;

                    case ActivityFormType.CustomGroup:
                        vm.Registrations = formsData.Select(x =>
                            {
                                var teamPlayer = GetTeamPlayer(x);

                                return new ActivityRegistrationItem
                                {
                                    Id = x.Id,
                                    UserId = x.User.UserId,
                                    UserIdNum = x.User.IdentNum,
                                    Document = x.Document,
                                    InsuranceCert = x.InsuranceCert,
                                    PaymentByBenefactor = x.PaymentByBenefactor,
                                    //NameForInvoice = x.NameForInvoice,
                                    NeedShirts = x.Team?.NeedShirts ?? false,
                                    Comments = x.Comments,
                                    PlayerEmail = x.User.Email,
                                    DateSubmitted = x.DateSubmitted,
                                    PlayerPhone = x.User.Telephone,
                                    PlayerBirthDate = x.User.BirthDay,
                                    Team = x.Team?.TeamsDetails.FirstOrDefault(td => td.SeasonId == activity.SeasonId)
                                               ?.TeamName ?? x.Team?.Title ?? Messages.Activity_NoTeamPlaceholder,
                                    TeamId = x.TeamId,
                                    League = x.League.Name,
                                    LeagueId = x.LeagueId,
                                    PlayerFullName = x.User.FullName,
                                    PlayerFirstName = x.User.FirstName,
                                    PlayerLastName = x.User.LastName,
                                    PlayerMiddleName = x.User.MiddleName,
                                    MedicalCert = x.MedicalCert,
                                    SelfInsurance = x.SelfInsurance,
                                    PlayerAddress = x.User.Address,
                                    PlayerCity = x.User.City,
                                    IsActive = x.IsActive,

                                    //LeaguePrice = x.IsPaymentByBenefactor
                                    //    ? (decimal?)null
                                    //    : x.RegistrationPrice,

                                    //ByBenefactorPrice = x.IsPaymentByBenefactor
                                    //    ? x.RegistrationPrice
                                    //    : (decimal?)null,
                                    LeaguePrice = x.IsPaymentByBenefactor
                                        ? (decimal?) null
                                        : (x.League?.LeaguesPrices?.FirstOrDefault(
                                                   lp => lp.PriceType == (int?) LeaguePriceType.TeamRegistrationPrice &&
                                                         lp.StartDate <= x.DateSubmitted &&
                                                         lp.EndDate >= x.DateSubmitted)
                                               ?.Price ?? 0),

                                    ByBenefactorPrice = x.IsPaymentByBenefactor
                                        ? (x.League?.LeaguesPrices?.FirstOrDefault(
                                                   lp => lp.PriceType == (int?) LeaguePriceType.TeamRegistrationPrice &&
                                                         lp.StartDate <= x.DateSubmitted &&
                                                         lp.EndDate >= x.DateSubmitted)
                                               ?.Price ?? 0)
                                        : (decimal?) null,

                                    Paid = x.Paid,

                                    RegistrationPaid = x.RegistrationPaid,
                                    UnionComment = x.UnionComment,
                                    //Paid = activity.Union?.Section?.Alias == SectionAliases.Netball
                                    //    ? (x.CardComPaymentCompleted ? leagueRegPrice : 0)
                                    //    : 0
                                    CustomFields = !string.IsNullOrWhiteSpace(x.CustomFields)
                                        ? JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(x.CustomFields)
                                        : new List<ActivityFormCustomField>(),

                                    CustomPrices = !string.IsNullOrWhiteSpace(x.CustomPrices)
                                        ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(x.CustomPrices)
                                        : new List<ActivityCustomPriceModel>(),

                                    ProfilePicture = x.User.PlayerFiles
                                                         .FirstOrDefault(
                                                             f => f.SeasonId == activity.SeasonId && f.FileType ==
                                                                  (int) PlayerFileType.PlayerImage)
                                                         ?.FileName ?? x.User.Image,
                                    IsLocked = IsPlayerLocked(x, teamPlayer),
                                    TeamPlayerId = teamPlayer?.Id,

                                    RegisteredMoreThanOnce = formsData.Count(f => f.PlayerId == x.PlayerId) > 1,

                                    CardComNumberOfPayments = x.CardComNumberOfPayments,
                                    CardComInvoiceNumber = x.CardComInvoiceNumber
                                };
                            })
                            .ToList();
                        result = PartialView("StatusesByType/_UnionCustomGroupRegistrationStatusData", vm);
                        break;

                    case ActivityFormType.CustomPersonal:
                        vm.Registrations = formsData.Select(x =>
                            {
                                var teamPlayer = GetTeamPlayer(x);

                                return new ActivityRegistrationItem
                                {
                                    Id = x.Id,
                                    UserId = x.User.UserId,
                                    UserIdNum = x.User.IdentNum,
                                    Document = x.Document,
                                    PaymentByBenefactor = x.PaymentByBenefactor,
                                    //NameForInvoice = x.NameForInvoice,
                                    Comments = x.Comments,
                                    PlayerEmail = x.User.Email,
                                    DateSubmitted = x.DateSubmitted,
                                    PlayerPhone = x.User.Telephone,
                                    PlayerBirthDate = x.User.BirthDay,
                                    Team = x.Team?.TeamsDetails
                                               .FirstOrDefault(td => td.SeasonId == activity.SeasonId)
                                               ?.TeamName ?? x.Team?.Title,
                                    TeamId = x.TeamId,
                                    League = x.League?.Name,
                                    LeagueId = x.LeagueId,
                                    PlayerFullName = x.User.FullName,
                                    PlayerFirstName = x.User.FirstName,
                                    PlayerLastName = x.User.LastName,
                                    PlayerMiddleName = x.User.MiddleName,
                                    PlayerGender = x.User.GenderId == 0 ? Messages.Female : x.User.GenderId == 1 ? Messages.Male : string.Empty,
                                    PlayerAddress = x.User.Address,
                                    PlayerCity = x.User.City,
                                    IsActive = x.IsActive,

                                    InsuranceCert = x.User.PlayerFiles
                                        .FirstOrDefault(
                                            f => f.SeasonId == activity.SeasonId &&
                                                 f.FileType == (int) PlayerFileType.Insurance)
                                        ?.FileName,

                                    MedicalCert = x.User.PlayerFiles
                                        .FirstOrDefault(
                                            f => f.SeasonId == activity.SeasonId &&
                                                 f.FileType == (int) PlayerFileType.MedicalCertificate && !f.IsArchive)
                                        ?.FileName,
                                    MedicalCertApproved = x.User.MedicalCertApprovements.FirstOrDefault (c => c.SeasonId == activity.SeasonId)?.Approved == true,

                                    IdFile = x.User.IDFile ?? x.User.PlayerFiles
                                                 .FirstOrDefault(
                                                     f => f.SeasonId == activity.SeasonId && f.FileType ==
                                                          (int)PlayerFileType.IDFile)
                                                 ?.FileName,
                                    
                                    LeaguePrice = x.IsPaymentByBenefactor
                                        ? (decimal?) null
                                        : x.RegistrationPrice,
                                    InsurancePrice = x.InsurancePrice,
                                    MembersFee = x.MembersFee,
                                    HandlingFee = x.HandlingFee,

                                    ByBenefactorPrice = x.IsPaymentByBenefactor
                                        ? x.RegistrationPrice
                                        : (decimal?) null,

                                    Paid = x.Paid,
                                    UnionComment = x.UnionComment,

                                    InsurancePaid = x.InsurancePaid,
                                    RegistrationPaid = x.RegistrationPaid,
                                    MembersFeePaid = x.MembersFeePaid,
                                    HandlingFeePaid = x.HandlingFeePaid,

                                    DisableRegistrationPayment = x.DisableRegistrationPayment,
                                    DisableInsurancePayment = x.DisableInsurancePayment,
                                    DisableMembersFeePayment = x.DisableMembersFeePayment,
                                    DisableHandlingFeePayment = x.DisableHandlingFeePayment,

                                    CustomFields = !string.IsNullOrWhiteSpace(x.CustomFields)
                                        ? JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(x
                                            .CustomFields)
                                        : new List<ActivityFormCustomField>(),

                                    CustomPrices = !string.IsNullOrWhiteSpace(x.CustomPrices)
                                        ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(x.CustomPrices)
                                        : new List<ActivityCustomPriceModel>(),

                                    ProfilePicture = x.User.PlayerFiles
                                                         .FirstOrDefault(
                                                             f => f.SeasonId == activity.SeasonId && f.FileType ==
                                                                  (int) PlayerFileType.PlayerImage)
                                                         ?.FileName ?? x.User.Image,

                                    IsTrainerPlayer =
                                        x.User.TeamsPlayers
                                            .FirstOrDefault(
                                                t => t.TeamId == x.TeamId && t.SeasonId == activity.SeasonId)
                                            ?.IsTrainerPlayer ?? false,
                                    IsEscortPlayer =
                                        x.User.TeamsPlayers
                                            .FirstOrDefault(
                                                t => t.TeamId == x.TeamId && t.SeasonId == activity.SeasonId)
                                            ?.IsEscortPlayer ?? false,
                                    IsUnionPlayer = allBuiltInRegistrations.Any(r => r.PlayerId == x.PlayerId),
                                    ApprovedPlayerNoInsurance = activity.UnionApprovedPlayerNoInsurance,
                                    IsLocked = IsPlayerLocked(x, teamPlayer),
                                    TeamPlayerId = teamPlayer?.Id,

                                    RegisteredMoreThanOnce = formsData.Count(f => f.PlayerId == x.PlayerId) > 1,

                                    IsTennisCompetition = isTennisActivity && x.League?.EilatTournament == true,

                                    CardComNumberOfPayments = x.CardComNumberOfPayments,
                                    CardComInvoiceNumber = x.CardComInvoiceNumber,

                                    CompetitionId = x.CompetitionDiscipline?.CompetitionId,
                                    CompetitionName = x.CompetitionDiscipline?.League?.Name,
                                    CompetitionCategoryName = x.CompetitionDiscipline?.CompetitionAge?.age_name
                                };
                            })
                            .ToList();
                        result = PartialView("StatusesByType/_UnionCustomPersonalRegistrationStatusData", vm);
                        break;

                    case ActivityFormType.UnionClub:
                        vm.Registrations = formsData.Select(x =>
                            {
                                var teamPlayer = GetTeamPlayer(x);

                                var customPrices = !string.IsNullOrWhiteSpace(x.CustomPrices)
                                    ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(x.CustomPrices)
                                    : new List<ActivityCustomPriceModel>();

                                return new ActivityRegistrationItem
                                {
                                    Id = x.Id,
                                    UserId = x.User.UserId,
                                    UserIdNum = x.User.IdentNum,
                                    Document = x.Document,
                                    InsuranceCert = x.InsuranceCert,
                                    //NameForInvoice = x.NameForInvoice,
                                    //NeedShirts = x.Team?.NeedShirts ?? false,
                                    Comments = x.Comments,
                                    PlayerEmail = x.User.Email,
                                    DateSubmitted = x.DateSubmitted,
                                    PlayerPhone = x.User.Telephone,
                                    PlayerBirthDate = x.User.BirthDay,
                                    //Team = x.Team?.TeamsDetails.FirstOrDefault(td => td.SeasonId == activity.SeasonId)
                                    //           ?.TeamName ?? x.Team?.Title ?? Messages.Activity_NoTeamPlaceholder,
                                    //League = x.League.Name,
                                    PlayerFullName = x.User.FullName,
                                    PlayerFirstName = x.User.FirstName,
                                    PlayerLastName = x.User.LastName,
                                    PlayerMiddleName = x.User.MiddleName,
                                    MedicalCert = x.MedicalCert,
                                    SelfInsurance = x.SelfInsurance,
                                    PlayerAddress = x.User.City,
                                    IsActive = x.IsActive,

                                    PaymentByBenefactor = x.PaymentByBenefactor,
                                    ByBenefactorPrice = x.IsPaymentByBenefactor
                                        ? customPrices.Any() ? customPrices.Sum(cp => cp.TotalPrice) : 0
                                        : (decimal?) null,

                                    Paid = x.Paid,

                                    RegistrationPaid = x.RegistrationPaid,
                                    UnionComment = x.UnionComment,
                                    CustomFields = !string.IsNullOrWhiteSpace(x.CustomFields)
                                        ? JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(x.CustomFields)
                                        : new List<ActivityFormCustomField>(),

                                    CustomPrices = customPrices,

                                    ProfilePicture = x.User.PlayerFiles
                                                         .FirstOrDefault(
                                                             f => f.SeasonId == activity.SeasonId && f.FileType ==
                                                                  (int) PlayerFileType.PlayerImage)
                                                         ?.FileName ?? x.User.Image,

                                    ClubCertificateOfIncorporation = x.Club?.CertificateOfIncorporation,
                                    ClubApprovalOfInsuranceCover = x.Club?.ApprovalOfInsuranceCover,
                                    ClubAuthorizedSignatories = x.Club?.AuthorizedSignatories,
                                    Club = x.Club?.Name,
                                    ClubId = x.ClubId,
                                    ClubNumberOfCourts = x.Club?.NumberOfCourts ?? 0,
                                    ClubNGONumber = x.Club?.NGO_Number,
                                    ClubNameOfSportsCentre = !IsHebrew ? x.Club?.SportCenter?.Eng : x.Club?.SportCenter?.Heb,
                                    ClubAddress = x.Club?.Address,
                                    ClubPhone = x.Club?.ContactPhone,
                                    ClubEmail = x.Club?.Email,
                                    ClubRegionalApproved = x.Club?.IsClubApproveByRegional == true,
                                    ClubRegion = x.Club?.Regional?.Name,
                                    IsLocked = IsPlayerLocked(x, teamPlayer),
                                    TeamPlayerId = teamPlayer?.Id,

                                    RegisteredMoreThanOnce = formsData.Count(f => f.PlayerId == x.PlayerId) > 1,

                                    CardComNumberOfPayments = x.CardComNumberOfPayments,
                                    CardComInvoiceNumber = x.CardComInvoiceNumber
                                };
                            })
                            .ToList();
                        result = PartialView("StatusesByType/_UnionClubRegistrationStatusData", vm);
                        break;

                    case ActivityFormType.UnionPlayerToClub:
                        vm.Registrations = formsData.Select(x =>
                            {
                                var teamPlayer = GetTeamPlayer(x);

                                return new ActivityRegistrationItem
                                {
                                    Id = x.Id,
                                    UserId = x.User.UserId,
                                    UserIdNum = x.User.IdentNum,
                                    Document = x.Document,
                                    Comments = x.Comments,
                                    PlayerEmail = x.User.Email,
                                    DateSubmitted = x.DateSubmitted,
                                    PlayerPhone = x.User.Telephone,
                                    PlayerBirthDate = x.User.BirthDay,
                                    Team = x.Team?.TeamsDetails.FirstOrDefault(td => td.SeasonId == activity.SeasonId)
                                               ?.TeamName ?? x.Team?.Title,
                                    TeamId = x.TeamId,
                                    PlayerFullName = x.User.FullName,
                                    PlayerFirstName = x.User.FirstName,
                                    PlayerLastName = x.User.LastName,
                                    PlayerMiddleName = x.User.MiddleName,

                                    PlayerFatherName = x.User.ParentName,
                                    PlayerMotherName = x.User.MotherName,
                                    PlayerParentPhone = x.User.ParentPhone,
                                    PlayerParentEmail = x.User.ParentEmail,

                                    PlayerIdentCard = x.User.IdentCard, //Ukraine gymnastic only
                                    PlayerLicenseDate = x.User.LicenseValidity, //Ukraine gymnastic only

                                    DateOfMedicalExamination = x.User.MedExamDate,
                                    InsuranceValidity = x.User.DateOfInsurance,

                                    PlayerGender = x.User.GenderId == 0 ? Messages.Female : x.User.GenderId == 1 ? Messages.Male : string.Empty,
                                    PlayerCity = x.User.City,
                                    PlayerAddress = x.User.Address,
                                    IsActive = x.IsActive,

                                    LeaguePrice = x.IsPaymentByBenefactor
                                        ? (decimal?) null
                                        : x.RegistrationPrice,
                                    RegistrationPaid = x.RegistrationPaid,

                                    InsurancePrice = x.InsurancePrice,
                                    InsurancePaid = x.InsurancePaid,
                                    IsSchoolInsurance = x.IsSchoolInsurance,
                                    
                                    TenicardPrice = x.TenicardPrice,
                                    TenicardPaid = x.TenicardPaid,
                                    DoNotPayTenicard = x.DoNotPayTenicardPrice,

                                    IsCompetitiveMember = x.User.IsCompetitiveMember,

                                    UnionComment = x.UnionComment,

                                    PaymentByBenefactor = x.PaymentByBenefactor,
                                    ByBenefactorPrice = x.IsPaymentByBenefactor
                                        ? x.RegistrationPrice
                                        : (decimal?) null,

                                    CustomFields = !string.IsNullOrWhiteSpace(x.CustomFields)
                                        ? JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(x.CustomFields)
                                        : new List<ActivityFormCustomField>(),

                                    CustomPrices = !string.IsNullOrWhiteSpace(x.CustomPrices)
                                        ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(x.CustomPrices)
                                        : new List<ActivityCustomPriceModel>(),

                                    IdFile = x.User.IDFile ?? x.User.PlayerFiles
                                                         .FirstOrDefault(
                                                             f => f.SeasonId == activity.SeasonId && f.FileType ==
                                                                  (int) PlayerFileType.IDFile)
                                                         ?.FileName,
                                    ProfilePicture = x.User.PlayerFiles
                                                         .FirstOrDefault(
                                                             f => f.SeasonId == activity.SeasonId && f.FileType ==
                                                                  (int) PlayerFileType.PlayerImage)
                                                         ?.FileName ?? x.User.Image,
                                    InsuranceCert = x.User.PlayerFiles
                                        .FirstOrDefault(
                                            f => f.PlayerId == x.PlayerId &&
                                                 f.SeasonId == activity.SeasonId &&
                                                 f.FileType == (int)PlayerFileType.Insurance)
                                        ?.FileName,

                                    MedicalCert = x.User.PlayerFiles
                                        .FirstOrDefault(
                                            f => f.PlayerId == x.PlayerId &&
                                                 f.SeasonId == activity.SeasonId &&
                                                 f.FileType == (int)PlayerFileType.MedicalCertificate && !f.IsArchive)
                                        ?.FileName,
                                    MedicalCertApproved = x.User.MedicalCertApprovements.FirstOrDefault (c => c.SeasonId == activity.SeasonId)?.Approved == true,

                                    Club = x.Club.Name,
                                    ClubId = x.ClubId,
                                    ClubRegion = x.Club?.Regional?.Name,
                                    LeagueId = x.LeagueId,
                                    IsLocked = IsPlayerLocked(x, teamPlayer),
                                    TeamPlayerId = teamPlayer?.Id,

                                    RegisteredMoreThanOnce = formsData.Count(f => f.PlayerId == x.PlayerId) > 1,

                                    CardComNumberOfPayments = x.CardComNumberOfPayments,
                                    CardComInvoiceNumber = x.CardComInvoiceNumber,

                                    PlayerPostalCode = x.User.PostalCode,
                                    PlayerPassportNumber = x.User.PassportNum,
                                    PlayerForeignFirstName = x.User.ForeignFirstName,
                                    PlayerForeignLastName = x.User.ForeignLastName,
                                };
                            })
                            .ToList();
                        result = PartialView("StatusesByType/_UnionPlayerToClubRegistrationStatusData", vm);
                        break;

                    default:
                        throw new Exception($"Unknown form type for activity {activity.ActivityId}");
                }
            }

            db.Configuration.LazyLoadingEnabled = dbLazyLoad;

            return result;
        }

        private TeamsPlayer GetTeamPlayer(ActivityFormsSubmittedData registration)
        {
            return registration.User.TeamsPlayers.FirstOrDefault(tp => tp.TeamId == registration.TeamId &&
                                                                       tp.LeagueId == registration.LeagueId &&
                                                                       tp.ClubId == registration.ClubId &&
                                                                       tp.SeasonId == registration.Activity.SeasonId);
        }

        private bool? IsPlayerLocked(ActivityFormsSubmittedData registration, TeamsPlayer teamPlayer)
        {
            var league = registration.League;

            if (teamPlayer != null && league != null)
            {
                if (teamPlayer.IsLocked == null)
                {
                    var minAge = league.MinimumAge;
                    var maxAge = league.MaximumAge;

                    if (registration.User.BirthDay != null)
                    {
                        if (minAge != null && registration.User.BirthDay > minAge.Value)
                        {
                            teamPlayer.IsLocked = true;
                        }

                        if (maxAge != null && registration.User.BirthDay < maxAge.Value)
                        {
                            teamPlayer.IsLocked = true;
                        }
                    }
                }
            }

            return teamPlayer?.IsLocked;
        }

        [HttpPost]
        public ActionResult ApproveMedicalCert(string idNum, int activityId, bool value)
        {
            var activity = activityRepo.GetById(activityId);
            var user = usersRepo.GetByIdentityNumber(idNum);

            if (user == null || activity == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            //user.MedicalCertificate = value;
            var medCertApprovement = user.MedicalCertApprovements.FirstOrDefault(x => x.SeasonId == activity.SeasonId);
            if (medCertApprovement != null)
            {
                medCertApprovement.Approved = value;
            }
            else
            {
                user.MedicalCertApprovements.Add(new MedicalCertApprovement
                {
                    SeasonId = activity.SeasonId ?? 0,
                    Approved = value
                });
            }

            usersRepo.Save();

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public ActionResult SetRegistrationActive(int id, int regId, bool value)
        {
            var activity = activityRepo.GetById(id);

            var registration = activity?.ActivityFormsSubmittedDatas.FirstOrDefault(x => x.Id == regId);

            if (registration == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            var formType = activity.GetFormType();

            registration.IsActive = value;

            if (!registration.User.IsActive)
            {
                registration.User.IsActive = true;
            }

            if (formType == ActivityFormType.UnionPlayerToClub)
            {
                var teamPlayer = registration.User?.TeamsPlayers?.FirstOrDefault(x =>
                    x.ClubId == registration.ClubId && x.TeamId == registration.TeamId &&
                    x.SeasonId == activity.SeasonId);

                if (teamPlayer != null)
                {
                    teamPlayer.IsApprovedByManager = value;
                }
            }

            if (value)
            {
                if (registration.ApprovalDate == null)
                {
                    registration.ApprovalDate = DateTime.Now;

                    var emailSvc = new EmailService();

                    var playerTeamName = registration.Team?.TeamsDetails
                                             ?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ??
                                         registration.Team?.Title;

                    switch (formType)
                    {
                        case ActivityFormType.TeamRegistration:
                        case ActivityFormType.CustomGroup:
                            var teamName = registration.Team?.TeamsDetails
                                               ?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ??
                                           registration.Team?.Title;

                            try
                            {
                                var email = string.Empty;
#if DEBUG
                                email = "info@loglig.com";
#else
                                email = registration.User.Email;
#endif
                                emailSvc.SendAsync(email,
                                    string.Format(Messages.Activity_TeamRegistrationApproved_Email_Body,
                                        registration.User?.FullName, teamName),
                                    string.Format(Messages.Activity_TeamRegistrationApproved_Email_Subject, teamName));
                            }
                            catch (Exception)
                            {
                            }
                            break;

                        case ActivityFormType.UnionClub:
                            var clubName = registration.Club?.Name;

                            if (registration.Club != null)
                            {
                                registration.Club.DateOfClubApproval = registration.ApprovalDate;
                                if (!registration.Club.InitialDateOfClubApproval.HasValue && registration.Club.UnionId == 52)
                                {
                                    registration.Club.InitialDateOfClubApproval = DateTime.Now;
                                }
                            }

                            try
                            {
                                var email = string.Empty;
#if DEBUG
                                email = "info@loglig.com";
#else
                                email = registration.User.Email;
#endif
                                if (registration.Activity?.UnionId == GlobVars.UkraineGymnasticUnionId)
                                {
                                    emailSvc.SendAsync(email,
                                        string.Format(Messages.Activity_UnionClubRegistrationApproved_Email_Body_UA,
                                            registration.User?.FullName,
                                            clubName,
                                            registration.User?.IdentNum,
                                            Protector.Decrypt(registration.User?.Password)),
                                        string.Format(Messages.Activity_UnionClubRegistrationApproved_Email_Subject_UA, clubName));
                                }
                                else
                                {
                                    emailSvc.SendAsync(email,
                                        string.Format(Messages.Activity_UnionClubRegistrationApproved_Email_Body, registration.User?.FullName, clubName),
                                        string.Format(Messages.Activity_UnionClubRegistrationApproved_Email_Subject, clubName));
                                }
                            }
                            catch (Exception)
                            {
                            }
                            break;

                        case ActivityFormType.UnionPlayerToClub:
                            try
                            {
                                var email = string.Empty;
#if DEBUG
                                email = "info@loglig.com";
#else
                                email = registration.User.Email;
#endif

                                if (registration.Activity?.UnionId == GlobVars.UkraineGymnasticUnionId)
                                {
                                    var body = string.Format(
                                        Messages.Activity_UnionPlayerToClubRegistrationApproved_Email_Body_UA,
                                        registration.User?.FullName,
                                        registration.Club?.Name,
                                        playerTeamName,
                                        registration.User?.UserId,
                                        registration.User?.IdentNum,
                                        Protector.Decrypt(registration.User?.Password));

                                    emailSvc.SendAsync(email,
                                        body,
                                        string.Format(Messages.Activity_UnionPlayerToClubRegistrationApproved_Email_Subject, registration.User?.FullName));

                                }
                                else
                                {
                                    emailSvc.SendAsync(email,
                                        string.Format(Messages.Activity_UnionPlayerToClubRegistrationApproved_Email_Body, registration.User?.FullName, playerTeamName),
                                        string.Format(Messages.Activity_UnionPlayerToClubRegistrationApproved_Email_Subject, registration.User?.FullName));
                                }
                            }
                            catch (Exception)
                            {
                            }
                            break;

                        case ActivityFormType.ClubCustomPersonal:
                        case ActivityFormType.ClubPlayerRegistration:
                        case ActivityFormType.CustomPersonal:
                        case ActivityFormType.PlayerRegistration:
                            try
                            {
                                var email = string.Empty;
#if DEBUG
                                email = "info@loglig.com";
#else
                                email = registration.User.Email;
#endif

                                emailSvc.SendAsync(email,
                                    string.Format(Messages.Activity_UnionCustomPersonalRegistrationApproved_Email_Body, registration.User?.FullName, activity.Name),
                                    string.Format(Messages.Activity_UnionCustomPersonalRegistrationApproved_Email_Subject, registration.User?.FullName));
                            }
                            catch (Exception)
                            {
                            }
                            break;
                    }
                }
            }
            else
            {
                registration.ApprovalDate = null;
            }

            activityRepo.Save();

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public ActionResult SetTeamSelfInsurance(int id, int regId, bool value)
        {
            var activity = activityRepo.GetById(id);

            var registration = activity?.ActivityFormsSubmittedDatas.FirstOrDefault(x => x.Id == regId);

            if (registration == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            registration.SelfInsurance = value;

            activityRepo.Save();

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public void UpdatePaid(int id, int regId, decimal value)
        {
            var activity = activityRepo.GetById(id);
            var registration = activity?.ActivityFormsSubmittedDatas?.FirstOrDefault(x => x.Id == regId);

            if (registration != null)
            {
                registration.Paid = value;

                activityRepo.Save();
            }
        }

        [HttpPost]
        public void UpdatePlayerInsurancePaid(int id, int regId, decimal value)
        {
            var activity = activityRepo.GetById(id);
            var registration = activity?.ActivityFormsSubmittedDatas?.FirstOrDefault(x => x.Id == regId);

            if (registration != null)
            {
                registration.InsurancePaid = value;

                activityRepo.Save();
            }
        }

        [HttpPost]
        public void UpdatePlayerRegistrationPaid(int id, int regId, decimal value)
        {
            var activity = activityRepo.GetById(id);
            var registration = activity?.ActivityFormsSubmittedDatas?.FirstOrDefault(x => x.Id == regId);

            if (registration != null)
            {
                registration.RegistrationPaid = value;

                activityRepo.Save();
            }
        }

        [HttpPost]
        public void UpdateMembersFeePaid(int id, int regId, decimal value)
        {
            var activity = activityRepo.GetById(id);
            var registration = activity?.ActivityFormsSubmittedDatas?.FirstOrDefault(x => x.Id == regId);

            if (registration != null)
            {
                registration.MembersFeePaid = value;

                activityRepo.Save();
            }
        }

        [HttpPost]
        public void UpdateHandlingFeePaid(int id, int regId, decimal value)
        {
            var activity = activityRepo.GetById(id);
            var registration = activity?.ActivityFormsSubmittedDatas?.FirstOrDefault(x => x.Id == regId);

            if (registration != null)
            {
                registration.HandlingFeePaid = value;

                activityRepo.Save();
            }
        }

        [HttpPost]
        public void UpdateTenicardPaid(int id, int regId, decimal value)
        {
            var activity = activityRepo.GetById(id);
            var registration = activity?.ActivityFormsSubmittedDatas?.FirstOrDefault(x => x.Id == regId);

            if (registration != null)
            {
                registration.TenicardPaid = value;

                activityRepo.Save();
            }
        }

        [HttpPost]
        public void UpdateCustomPricePaid(int id, int regId, decimal value, string property)
        {
            var activity = activityRepo.GetById(id);
            var registration = activity?.ActivityFormsSubmittedDatas?.FirstOrDefault(x => x.Id == regId);

            if (registration != null)
            {
                var customPrices = registration.CustomPrices != null 
                    ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(registration.CustomPrices)
                    : new List<ActivityCustomPriceModel>();

                var customPrice = customPrices.FirstOrDefault(x => x.PropertyName == property);
                if (customPrice != null)
                {
                    customPrice.Paid = value;

                    registration.CustomPrices = JsonConvert.SerializeObject(customPrices);

                    activityRepo.Save();
                }
            }
        }

        public decimal GetRemainForPayment(int id, int regId)
        {
            var activity = activityRepo.GetById(id);
            var registration = activity?.ActivityFormsSubmittedDatas?.FirstOrDefault(x => x.Id == regId);

            var result = 0m;

            if (registration != null)
            {
                var formType = activity.GetFormType();

                var customPrices = !string.IsNullOrWhiteSpace(registration.CustomPrices)
                    ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(registration.CustomPrices)
                    : new List<ActivityCustomPriceModel>();

                switch (formType)
                {
                    case ActivityFormType.PlayerRegistration:
                        result =
                            (
                                (registration.IsPaymentByBenefactor ? 0m : registration.RegistrationPrice) +
                                registration.InsurancePrice
                            )
                            -
                            (
                                registration.RegistrationPaid +
                                registration.InsurancePaid
                            );
                        break;

                    case ActivityFormType.UnionPlayerToClub:
                        result =
                            (
                                registration.RegistrationPrice +
                                registration.InsurancePrice +
                                registration.TenicardPrice
                            )
                            -
                            (
                                registration.RegistrationPaid +
                                registration.InsurancePaid +
                                registration.TenicardPaid
                            );
                        break;

                    case ActivityFormType.TeamRegistration:
                        result =
                            (
                                (registration.IsPaymentByBenefactor
                                    ? 0m
                                    : registration.League?.LeaguesPrices
                                          ?.FirstOrDefault(
                                              x => x.PriceType == (int?) LeaguePriceType.TeamRegistrationPrice &&
                                                   x.StartDate <= registration.DateSubmitted &&
                                                   x.EndDate >= registration.DateSubmitted)?.Price ?? 0m)
                            )
                            -
                            (
                                registration.RegistrationPaid
                            );
                        break;

                    case ActivityFormType.CustomPersonal:
                        result =
                            (
                                (registration.IsPaymentByBenefactor ? 0m : registration.RegistrationPrice) +
                                registration.InsurancePrice +
                                registration.MembersFee +
                                registration.HandlingFee
                            )
                            -
                            (
                                registration.RegistrationPaid +
                                registration.InsurancePaid +
                                registration.MembersFeePaid +
                                registration.HandlingFeePaid
                            );
                        break;

                    case ActivityFormType.CustomGroup:
                        result =
                            (
                                (registration.IsPaymentByBenefactor
                                    ? 0m
                                    : registration.League?.LeaguesPrices
                                          ?.FirstOrDefault(
                                              x => x.PriceType == (int?) LeaguePriceType.TeamRegistrationPrice &&
                                                   x.StartDate <= registration.DateSubmitted &&
                                                   x.EndDate >= registration.DateSubmitted)?.Price ?? 0m)
                            )
                            -
                            (
                                registration.RegistrationPaid
                            );
                        break;

                    case ActivityFormType.ClubPlayerRegistration:
                        result =
                            (
                                (registration.IsPaymentByBenefactor ? 0m : registration.RegistrationPrice) +
                                registration.InsurancePrice +
                                registration.ParticipationPrice
                            )
                            -
                            (
                                registration.RegistrationPaid +
                                registration.InsurancePaid +
                                registration.ParticipationPaid
                            );
                        break;

                    case ActivityFormType.ClubCustomPersonal:
                        result =
                            (
                                (registration.IsPaymentByBenefactor ? 0m : registration.RegistrationPrice) +
                                registration.InsurancePrice +
                                registration.ParticipationPrice
                            )
                            -
                            (
                                registration.RegistrationPaid +
                                registration.InsurancePaid +
                                registration.ParticipationPaid
                            );
                        break;

                    case ActivityFormType.DepartmentClubPlayerRegistration:
                        result =
                            (
                                (registration.IsPaymentByBenefactor ? 0m : registration.RegistrationPrice) +
                                registration.InsurancePrice +
                                registration.ParticipationPrice
                            )
                            -
                            (
                                registration.RegistrationPaid +
                                registration.InsurancePaid +
                                registration.ParticipationPaid
                            );
                        break;

                    case ActivityFormType.DepartmentClubCustomPersonal:
                        result =
                            (
                                (registration.IsPaymentByBenefactor ? 0m : registration.RegistrationPrice) +
                                registration.InsurancePrice +
                                registration.ParticipationPrice
                            )
                            -
                            (
                                registration.RegistrationPaid +
                                registration.InsurancePaid +
                                registration.ParticipationPaid
                            );
                        break;

                    case ActivityFormType.ClubTeamRegistration:
                        break;

                    case ActivityFormType.ClubCustomGroup:
                        break;

                    case ActivityFormType.UnionClub:
                        break;
                }

                result += customPrices.Sum(x => x.TotalPrice) - customPrices.Sum(x => x.Paid);
            }

            return result;
        }

        [HttpPost]
        public void UpdatePlayerParticipationPaid(int id, int regId, decimal value)
        {
            var activity = activityRepo.GetById(id);
            var registration = activity?.ActivityFormsSubmittedDatas?.FirstOrDefault(x => x.Id == regId);

            if (registration != null)
            {
                registration.ParticipationPaid = value;

                activityRepo.Save();
            }
        }

        [HttpPost]
        public void UpdateUnionComment(int id, int regId, string value)
        {
            var activity = activityRepo.GetById(id);
            var registration = activity?.ActivityFormsSubmittedDatas?.FirstOrDefault(x => x.Id == regId);

            if (registration != null)
            {
                registration.UnionComment = value;

                activityRepo.Save();
            }
        }

        [HttpPost]
        public void UpdateClubComment(int id, int regId, string value)
        {
            var activity = activityRepo.GetById(id);
            var registration = activity?.ActivityFormsSubmittedDatas?.FirstOrDefault(x => x.Id == regId);

            if (registration != null)
            {
                registration.ClubComment = value;

                activityRepo.Save();
            }
        }

        [HttpPost]
        public void StatusColumnVisibility(int activityId, int item, bool value)
        {
            var activity = activityRepo.GetById(activityId);
            int userId;

            if (activity != null && int.TryParse(User.Identity.Name, out userId))
            {
                var existingColumn =
                    activity.ActivityStatusColumnsVisibilities.FirstOrDefault(x => x.ColumnIndex == item && x.UserId == userId);
                if (existingColumn != null)
                {
                    existingColumn.Visible = value;
                }
                else
                {
                    activity.ActivityStatusColumnsVisibilities.Add(new ActivityStatusColumnsVisibility
                    {
                        ActivityId = activityId,
                        ColumnIndex = item,
                        UserId = userId,
                        Visible = value
                    });
                }

                activityRepo.Save();
            }
        }

        private string BuildForm(Activity activity, ICollection<ActivityFormsDetail> formDetails, bool isPublish,
            CultEnum culture)
        {
            var stringBuilder = new StringBuilder();

            var activityFormType = activity.GetFormType();

            var isRegionalLevelEnabled = activity.Union?.IsRegionallevelEnabled == true;

            var isTennisActivity = string.Equals(activity.Union?.Section?.Alias, SectionAliases.Tennis,
                StringComparison.CurrentCultureIgnoreCase);
            var isUkraineGymnasticActivity = activity.UnionId == GlobVars.UkraineGymnasticUnionId;

            foreach (var formDetail in formDetails)
            {
                ActivityFormControlType controlType;
                if (!Enum.TryParse(formDetail.Type, out controlType))
                {
                    continue;
                }

                var controlModel = new ActivityFormControlTemplateModel
                {
                    IsPublish = isPublish,
                    PropertyName = formDetail.PropertyName,
                    LabelTextEn = formDetail.LabelTextEn,
                    LabelTextHeb = formDetail.LabelTextHeb,
                    LabelTextUk = formDetail.LabelTextUk,
                    IsRequired = formDetail.IsRequired,
                    IsDisabled = formDetail.IsDisabled,
                    CanBeDisabled = formDetail.CanBeDisabled,
                    CanBeRequired = formDetail.CanBeRequired,
                    Culture = culture,
                    IsReadOnly = formDetail.IsReadOnly,
                    FieldNote = formDetail.FieldNote,
                    CustomDropdownValues = !string.IsNullOrWhiteSpace(formDetail.CustomDropdownValues)
                        ? JsonConvert.DeserializeObject<List<string>>(formDetail.CustomDropdownValues)
                        : new List<string>(),
                    CanBeRemoved = formDetail.CanBeRemoved,
                    HasOptions = formDetail.HasOptions
                };

                if (formDetail.ActivityCustomPrice != null)
                {
                    controlModel.CustomPrice = new CustomPriceItem
                    {
                        Id = formDetail.ActivityCustomPrice.Id,
                        TitleEng = formDetail.ActivityCustomPrice.TitleEng,
                        TitleHeb = formDetail.ActivityCustomPrice.TitleHeb,
                        TitleUk = formDetail.ActivityCustomPrice.TitleUk,
                        Price = formDetail.ActivityCustomPrice.Price,
                        MaxQuantity = formDetail.ActivityCustomPrice.MaxQuantity,
                        DefaultQuantity= formDetail.ActivityCustomPrice.DefaultQuantity
                    };
                }

                switch (controlModel.PropertyName)
                {
                    case "paymentByBenefactor":
                        controlModel.IsDisabledBySettings = !activity.ByBenefactor;
                        break;
                    case "document":
                        controlModel.IsDisabledBySettings = !activity.AttachDocuments;
                        break;
                    case "medicalCert":
                        controlModel.IsDisabledBySettings = !activity.MedicalCertificate;
                        break;
                    case "insuranceCert":
                        controlModel.IsDisabledBySettings = !activity.InsuranceCertificate;
                        break;

                    case "playerTeamMultiple":
                        controlModel.IsDisabledBySettings = activity.NoTeamRegistration || !activity.MultiTeamRegistrations;
                        break;
                }

                switch (activityFormType)
                {
                    case ActivityFormType.PlayerRegistration:
                        switch (controlModel.PropertyName)
                        {
                            case "playerRegistrationPrice":
                                controlModel.IsDisabledBySettings = !activity.RegistrationPrice;
                                break;
                            case "playerInsurancePrice":
                                controlModel.IsDisabledBySettings = !activity.InsurancePrice;
                                break;
                        }
                        break;

                    case ActivityFormType.CustomPersonal:
                        switch (controlModel.PropertyName)
                        {
                            case "playerRegistrationPrice":
                                controlModel.IsDisabledBySettings = !activity.RegistrationPrice;
                                break;
                            case "playerInsurancePrice":
                                controlModel.IsDisabledBySettings = !activity.InsurancePrice;
                                break;
                            case "playerMemberFee":
                                controlModel.IsDisabledBySettings = !activity.MembersFee;
                                break;
                            case "playerHandlingFee":
                                controlModel.IsDisabledBySettings = !activity.HandlingFee;
                                break;

                            case "disableRegistrationPayment":
                                controlModel.IsDisabledBySettings = !activity.AllowNoRegistrationPayment;
                                break;

                            case "disableInsurancePayment":
                                controlModel.IsDisabledBySettings = !activity.AllowNoInsurancePayment;
                                break;

                            case "disableMembersFeePayment":
                                controlModel.IsDisabledBySettings = !activity.AllowNoFeePayment;
                                break;

                            case "disableHandlingFeePayment":
                                controlModel.IsDisabledBySettings = !activity.AllowNoHandlingFeePayment;
                                break;

                            case "playerLeagueDropDown":
                                controlModel.IsDisabledBySettings = !activity.RestrictLeagues || activity.RegistrationsByCompetitionsCategory;
                                break;

                            case "playerTeamDropDown":
                                controlModel.IsDisabledBySettings = activity.NoTeamRegistration || activity.RegistrationsByCompetitionsCategory;
                                break;

                            case "playerCompetitionCategory":
                                controlModel.IsDisabledBySettings = !activity.RegistrationsByCompetitionsCategory;
                                break;

                            case "playerIsEscort":
                                controlModel.IsDisabledBySettings = !activity.AllowEscortRegistration;
                                break;
                        }
                        break;

                    case ActivityFormType.CustomGroup:
                        switch (controlModel.PropertyName)
                        {
                            case "playerTeam":
                                controlModel.IsDisabledBySettings = !activity.AllowNewTeamRegistration;
                                break;
                        }
                        break;

                    case ActivityFormType.ClubPlayerRegistration:
                        switch (controlModel.PropertyName)
                        {
                            case "playerBrotherIdForDiscount":
                                controlModel.IsDisabledBySettings = !activity.EnableBrotherDiscount;
                                break;
                            case "clubSchool":
                                controlModel.IsDisabledBySettings = activity.ClubLeagueTeamsOnly;
                                break;
                            case "playerRegistrationPrice":
                                controlModel.IsDisabledBySettings = !activity.RegistrationPrice;
                                break;
                            case "playerInsurancePrice":
                                controlModel.IsDisabledBySettings = !activity.InsurancePrice;
                                break;
                            case "playerParticipationPrice":
                                controlModel.IsDisabledBySettings = !activity.ParticipationPrice;
                                break;
                            case "disableParticipationPayment":
                                controlModel.IsDisabledBySettings = !activity.AllowNoParticipationPayment;
                                break;
                            case "postponeParticipationPayment":
                                controlModel.IsDisabledBySettings = !activity.PostponeParticipationPayment;
                                break;
                            case "playerTeam":
                                controlModel.IsDisabledBySettings = activity.MultiTeamRegistrations;
                                break;
                            case "disableInsurancePayment":
                                controlModel.IsDisabledBySettings = !activity.AllowNoInsurancePayment;
                                break;
                            case "playerAdjustPricesStartDate":
                                controlModel.IsDisabledBySettings = !activity.AllowToEnterDateToAdjustPrices;
                                break;
                        }
                        break;

                    case ActivityFormType.ClubCustomPersonal:
                        switch (controlModel.PropertyName)
                        {
                            case "playerBrotherIdForDiscount":
                                controlModel.IsDisabledBySettings = !activity.EnableBrotherDiscount;
                                break;
                            case "playerRegistrationPrice":
                                controlModel.IsDisabledBySettings = !activity.RegistrationPrice;
                                break;
                            case "playerInsurancePrice":
                                controlModel.IsDisabledBySettings = !activity.InsurancePrice;
                                break;
                            case "playerParticipationPrice":
                                controlModel.IsDisabledBySettings = !activity.ParticipationPrice;
                                break;
                            case "clubSchool":
                                controlModel.IsDisabledBySettings = activity.NoTeamRegistration;
                                break;
                            case "playerTeam":
                                controlModel.IsDisabledBySettings = activity.NoTeamRegistration || activity.MultiTeamRegistrations;
                                break;
                            case "disableInsurancePayment":
                                controlModel.IsDisabledBySettings = !activity.AllowNoInsurancePayment;
                                break;
                            case "disableParticipationPayment":
                                controlModel.IsDisabledBySettings = !activity.AllowNoParticipationPayment;
                                break;
                            case "postponeParticipationPayment":
                                controlModel.IsDisabledBySettings = !activity.PostponeParticipationPayment;
                                break;
                            case "playerAdjustPricesStartDate":
                                controlModel.IsDisabledBySettings = !activity.AllowToEnterDateToAdjustPrices;
                                break;
                        }
                        break;

                    case ActivityFormType.DepartmentClubPlayerRegistration:
                        switch (controlModel.PropertyName)
                        {
                            case "playerRegistrationPrice":
                                controlModel.IsDisabledBySettings = !activity.RegistrationPrice;
                                break;
                            case "playerInsurancePrice":
                                controlModel.IsDisabledBySettings = !activity.InsurancePrice;
                                break;
                            case "playerParticipationPrice":
                                controlModel.IsDisabledBySettings = !activity.ParticipationPrice;
                                break;
                        }
                        break;

                    case ActivityFormType.DepartmentClubCustomPersonal:
                        switch (controlModel.PropertyName)
                        {
                            case "playerRegistrationPrice":
                                controlModel.IsDisabledBySettings = !activity.RegistrationPrice;
                                break;
                            case "playerInsurancePrice":
                                controlModel.IsDisabledBySettings = !activity.InsurancePrice;
                                break;
                            case "playerParticipationPrice":
                                controlModel.IsDisabledBySettings = !activity.ParticipationPrice;
                                break;
                            case "clubSchool":
                            case "playerTeam":
                                controlModel.IsDisabledBySettings = activity.NoTeamRegistration;
                                break;
                            case "disableInsurancePayment":
                                controlModel.IsDisabledBySettings = !activity.AllowNoInsurancePayment;
                                break;
                            case "disableParticipationPayment":
                                controlModel.IsDisabledBySettings = !activity.AllowNoParticipationPayment;
                                break;
                        }
                        break;

                    case ActivityFormType.UnionPlayerToClub:
                        switch (controlModel.PropertyName)
                        {
                            case "playerTenicardValidity":
                                controlModel.IsDisabledBySettings = !isTennisActivity;
                                break;
                            case "playerTenicardPrice":
                                controlModel.IsDisabledBySettings = !isTennisActivity;
                                break;
                            case "playerRegistrationPrice":
                                controlModel.CustomFlag = activity.AllowCompetitiveMembers;
                                controlModel.IsDisabledBySettings = !activity.RegistrationPrice;
                                break;
                            case "playerInsurancePrice":
                                controlModel.IsDisabledBySettings = !activity.InsurancePrice;
                                break;
                            case "playerTeamMultiple":
                                controlModel.IsDisabledBySettings = !activity.MultiTeamRegistrations;
                                break;
                            case "playerTeam":
                                controlModel.IsDisabledBySettings = activity.MultiTeamRegistrations;
                                break;
                            case "playerRegion":
                                controlModel.IsDisabledBySettings = !isUkraineGymnasticActivity;
                                break;
                        }
                        break;

                    case ActivityFormType.UnionClub:
                        switch (controlModel.PropertyName)
                        {
                            case "region":
                                controlModel.IsDisabledBySettings = !isRegionalLevelEnabled;
                                break;
                        }
                        break;
                }

                stringBuilder.Append(this.GetControlTemplate(controlType, controlModel));
            }

            return stringBuilder.ToString();
        }

        [HttpGet]
        [AllowAnonymous]
        public string GetCustomControlDefinition(int customControlsCount, ActivityFormControlType controlType)
        {
            switch (controlType)
            {
                case ActivityFormControlType.CustomTextBox:
                    return this.GetControlTemplate(ActivityFormControlType.CustomTextBox,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = $"customTextBox-{++customControlsCount}",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_TextBox),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_TextBox),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = 
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_TextBox),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            CanBeRemoved = true,
                            HasOptions = true,
                            Culture = getCulture()
                        });

                case ActivityFormControlType.CustomTextArea:
                    return this.GetControlTemplate(ActivityFormControlType.CustomTextArea,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = $"customTextArea-{++customControlsCount}",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_TextArea),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_TextArea),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = 
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_TextArea),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            CanBeRemoved = true,
                            HasOptions = true,
                            Culture = getCulture()
                        });

                case ActivityFormControlType.CustomDropdown:
                    return this.GetControlTemplate(ActivityFormControlType.CustomDropdown,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = $"customDropdown-{++customControlsCount}",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_DropDown),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_DropDown),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = 
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_DropDown),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            CanBeRemoved = true,
                            HasOptions = true,
                            Culture = getCulture()
                        });

                case ActivityFormControlType.CustomDropdownMultiselect:
                    return this.GetControlTemplate(ActivityFormControlType.CustomDropdownMultiselect,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = $"customDropdownMultiselect-{++customControlsCount}",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_DropdownMultiselect),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_DropdownMultiselect),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = 
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_DropdownMultiselect),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            CanBeRemoved = true,
                            HasOptions = true,
                            Culture = getCulture()
                        });

                case ActivityFormControlType.CustomText:
                    return this.GetControlTemplate(ActivityFormControlType.CustomText,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = $"customText-{++customControlsCount}",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_CustomField_Text),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_CustomField_Text),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = 
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_CustomField_Text),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = false,
                            IsReadOnly = false,
                            CanBeDisabled = true,
                            CanBeRequired = false,
                            CanBeRemoved = true,
                            HasOptions = false,
                            Culture = getCulture()
                        });

                case ActivityFormControlType.CustomTextMultiline:
                    return this.GetControlTemplate(ActivityFormControlType.CustomTextMultiline,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = $"customTextMultiline-{++customControlsCount}",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_CustomField_TextMultiline),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_CustomField_TextMultiline),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = 
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_CustomField_TextMultiline),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = false,
                            IsReadOnly = false,
                            CanBeDisabled = true,
                            CanBeRequired = false,
                            CanBeRemoved = true,
                            HasOptions = false,
                            Culture = getCulture()
                        });

                case ActivityFormControlType.CustomCheckBox:
                    return this.GetControlTemplate(ActivityFormControlType.CustomCheckBox,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = $"customCheckBox-{++customControlsCount}",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_CheckBox),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_CheckBox),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = 
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_CheckBox),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            CanBeRemoved = true,
                            HasOptions = true,
                            Culture = getCulture()
                        });

                case ActivityFormControlType.CustomFileReadonly:
                    return this.GetControlTemplate(ActivityFormControlType.CustomFileReadonly,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = $"customFileReadonly-{++customControlsCount}",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_FileReadonly),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_FileReadonly),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = 
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_FileReadonly),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = false,
                            CanBeRemoved = true,
                            HasOptions = true,
                            Culture = getCulture()
                        });

                case ActivityFormControlType.CustomFileUpload:
                    return this.GetControlTemplate(ActivityFormControlType.CustomFileUpload,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = $"customFileUpload-{++customControlsCount}",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_FileUpload),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_FileUpload),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk = 
                                Messages.ResourceManager.GetString(
                                    nameof(Messages.Activity_BuildForm_CustomField_FileUpload),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            CanBeDisabled = true,
                            CanBeRequired = true,
                            CanBeRemoved = true,
                            HasOptions = true,
                            Culture = getCulture()
                        });

                case ActivityFormControlType.CustomLink:
                    return this.GetControlTemplate(ActivityFormControlType.CustomLink,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = $"customLink-{++customControlsCount}",
                            LabelTextEn =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_CustomField_Link),
                                    CultureInfo.CreateSpecificCulture("en-US")),
                            LabelTextHeb =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_CustomField_Link),
                                    CultureInfo.CreateSpecificCulture("he-IL")),
                            LabelTextUk =
                                Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_CustomField_Link),
                                    CultureInfo.CreateSpecificCulture("uk-UA")),
                            IsRequired = false,
                            IsReadOnly = false,
                            CanBeDisabled = true,
                            CanBeRequired = false,
                            CanBeRemoved = true,
                            HasOptions = false,
                            Culture = getCulture()
                        });

                default:
                    return null;
            }
        }

        private string CreateDefaultActivityForm(Activity activity)
        {
            var stringBuilder = this.GetDefaultForm(activity);

            if (activity.CustomPricesEnabled)
            {
                foreach (var activityCustomPrice in activity.ActivityCustomPrices)
                {
                    stringBuilder.Append(this.GetControlTemplate(ActivityFormControlType.CustomPrice,
                        new ActivityFormControlTemplateModel
                        {
                            PropertyName = $"customPrice-{activityCustomPrice.Id}",
                            LabelTextEn = "custom price",
                            LabelTextHeb = "custom price",
                            LabelTextUk = "custom price",
                            Culture = getCulture(),
                            CanBeDisabled = true,
                            CustomPrice = new CustomPriceItem
                            {
                                Id = activityCustomPrice.Id,
                                TitleEng = activityCustomPrice.TitleEng,
                                TitleHeb = activityCustomPrice.TitleHeb,
                                TitleUk = activityCustomPrice.TitleUk,
                                Price = activityCustomPrice.Price,
                                MaxQuantity = activityCustomPrice.MaxQuantity
                            }
                        }));
                }
            }

            stringBuilder.Append(this.GetControlTemplate(ActivityFormControlType.BasicTextArea,
                new ActivityFormControlTemplateModel
                {
                    PropertyName = "comments",
                    LabelTextEn =
                        Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_Comments),
                            CultureInfo.CreateSpecificCulture("en-US")),
                    LabelTextHeb =
                        Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_Comments),
                            CultureInfo.CreateSpecificCulture("he-IL")),
                    LabelTextUk = 
                        Messages.ResourceManager.GetString(nameof(Messages.Activity_BuildForm_Comments),
                            CultureInfo.CreateSpecificCulture("uk-UA")),
                    CanBeRequired = true,
                    Culture = getCulture()
                }));

            return stringBuilder.ToString();
        }

        private List<DropdownItem> GetLeaguesDropdown(Activity activity)
        {
            return activity.Union.Leagues
                .Select(x => new DropdownItem { Value = x.LeagueId.ToString(), Caption = x.Name })
                .ToList();
        }

        private List<DropdownItem> GetTeamsDropdown(Activity activity)
        {
            var result = new List<DropdownItem>();
            foreach (var activityLeague in activity.Union.Leagues)
            {
                result.AddRange(activityLeague.LeagueTeams.Select(
                    x => new DropdownItem { Value = x.Teams.TeamId.ToString(), Caption = x.Teams.Title }));
            }
            return result;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetActivityFormReadOnlyFiles(int id)
        {
            var activity = activityRepo.GetById(id);

            if (activity == null || !activity.ActivityForms.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var files = activity.ActivityFormsFiles.Select(x => new ActivityFormReadOnlyFile
            {
                PropertyName = x.PropertyName,
                FilePath = $"{GlobVars.ContentPath}/activityforms/{x.FileName}"
            });

            return Json(files, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveActivityFormReadOnlyFile(int id, string propertyName)
        {
            var activity = activityRepo.GetById(id);

            if (activity == null || !activity.ActivityForms.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var maxFileSize = GlobVars.MaxFileSize * 1000;
            var file = Request.Files["file"];

            if (file == null || file.ContentLength > maxFileSize)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string ext = Path.GetExtension(file.FileName)?.ToLower();

            if (!GlobVars.ValidImages.Contains(ext) &&
                !string.Equals(ext, ".xls", StringComparison.CurrentCultureIgnoreCase) &&
                !string.Equals(ext, ".xlsx", StringComparison.CurrentCultureIgnoreCase))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var newName = $"ActivityForm_{id}_ReadOnly_{AppFunc.GetUniqName()}{ext}";

            var savePath = Server.MapPath(GlobVars.ContentPath + "/activityforms/");

            var di = new DirectoryInfo(savePath);
            if (!di.Exists)
                di.Create();

            byte[] fileData;
            using (var reader = new BinaryReader(file.InputStream))
            {
                fileData = reader.ReadBytes(file.ContentLength);
            }
            System.IO.File.WriteAllBytes(savePath + newName, fileData);

            var activityFile = activity.ActivityFormsFiles.FirstOrDefault(x => x.PropertyName == propertyName);
            if (activityFile == null)
            {
                activity.ActivityFormsFiles.Add(new ActivityFormsFile
                {
                    ActivityId = activity.ActivityId,
                    PropertyName = propertyName,
                    FileName = newName
                });
            }
            else
            {
                activityFile.FileName = newName;
            }

            activityRepo.Save();

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public ActionResult SaveActivityFormImage(int id)
        {
            var activity = activityRepo.GetById(id);

            if (activity == null || !activity.ActivityForms.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var maxFileSize = GlobVars.MaxFileSize * 1000;
            var file = Request.Files["formImage"];

            if (file == null || file.ContentLength > maxFileSize)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string ext = Path.GetExtension(file.FileName)?.ToLower();

            if (!GlobVars.ValidImages.Contains(ext))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var newName = $"ActivityForm_{id}_{AppFunc.GetUniqName()}{ext}";

            var savePath = Server.MapPath(GlobVars.ContentPath + "/activityforms/");

            var di = new DirectoryInfo(savePath);
            if (!di.Exists)
                di.Create();

            byte[] imgData;
            using (var reader = new BinaryReader(file.InputStream))
            {
                imgData = reader.ReadBytes(file.ContentLength);
            }
            System.IO.File.WriteAllBytes(savePath + newName, imgData);

            activity.ActivityForms.First().ImageFile = newName;

            activityRepo.Save();

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult GetActivityFormImage(int id)
        {
            var activity = activityRepo.GetById(id);

            if (activity == null)
            {
                return HttpNotFound("activity not found");
            }

            if (!activity.ActivityForms.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.NoContent);
            }

            var image = activity.ActivityForms.First().ImageFile;

            return Content($"{GlobVars.ContentPath}/activityforms/{image}");
        }

        [HttpPost]
        public ActionResult SaveActivityForm(int id, SaveActivityFormModel model)
        {
            var activity = activityRepo.GetById(id);

            if (activity == null)
            {
                return Content("Activity not found");
            }

            var form = activity.ActivityForms.FirstOrDefault();

            if (form == null)
            {
                form = new ActivityForm
                {
                    ActivityId = activity.ActivityId,
                    Name = model.FormName,
                    Description = model.FormDescription,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    UpdatedBy = Convert.ToInt32(User.Identity.Name)
                };

                activity.ActivityForms.Add(form);

                ActivityFormsRepo.Save();
            }

            if (model.Fields != null && model.Fields.Any())
            {
                ActivityFormsRepo.RemoveFormDetails(form.Id);

                form.Name = model.FormName ?? activity.Name;
                form.Description = model.FormDescription;

                form.ActivityFormsDetails = model.Fields.Select(x => new ActivityFormsDetail
                {
                    CanBeDisabled = x.CanBeDisabled,
                    CanBeRequired = x.CanBeRequired,
                    FormId = form.Id,
                    IsDisabled = x.IsDisabled,
                    IsRequired = x.IsRequired,
                    LabelTextEn = x.LabelTextEn,
                    LabelTextHeb = x.LabelTextHeb,
                    LabelTextUk = x.LabelTextUk,
                    PropertyName = x.PropertyName,
                    Type = x.Type.ToString(),
                    IsReadOnly = x.IsReadOnly,
                    FieldNote = x.FieldNote,
                    CustomDropdownValues = x.CustomDropdownValues,
                    CanBeRemoved = x.CanBeRemoved,
                    HasOptions = x.HasOptions,
                    CustomPriceId = x.CustomPriceId
                })
                    .ToList();
            }

            ActivityFormsRepo.Save();

            return Content("Success");
        }

        [HttpPost]
        public ActionResult ExportRegStatusToExcel(int id)
        {
            var activity = activityRepo.GetById(id);

            var formType = activity.GetFormType();

            if (activity != null)
            {
                //var data = activity.ActivityFormsSubmittedDatas;
                var data = db.ActivityFormsSubmittedDatas
                    .Where(x => x.ActivityId == id)
                    .Include(x => x.User)
                    .Include(x => x.User.PlayerFiles)
                    .Include(x => x.User.MedicalCertApprovements)
                    .ToList();

                var allBuiltInRegistrations = activityRepo.GetCollection<ActivityFormsSubmittedData>(
                        x => x.Activity.IsAutomatic == true &&
                             x.Activity.Type == ActivityType.Personal &&
                             x.Activity.SeasonId == activity.SeasonId &&
                             x.Activity.UnionId == activity.UnionId &&
                             x.Activity.ClubId == activity.ClubId &&
                             x.IsActive)
                    .ToList();

                using (var workbook =
                    new XLWorkbook(XLEventTracking.Disabled) {RightToLeft = getCulture() == CultEnum.He_IL})
                {
                    var ws = workbook.Worksheets.Add("Sheet1");

                    var columnCount = 1;
                    var rowCount = 2;

                    Action<string> addColumn = name =>
                    {
                        ws.Cell(1, columnCount).Value = name;

                        columnCount++;
                    };
                    Action<string> addRow = value =>
                    {
                        ws.Cell(rowCount, columnCount).SetValue(value);

                        columnCount++;
                    };
                    Action<string, XLDataType> addRowWithType = (value, type) =>
                    {
                        ws.Cell(rowCount, columnCount).SetValue(value);
                        ws.Cell(rowCount, columnCount).SetDataType(type);

                        columnCount++;
                    };

                    switch (formType)
                    {
                        case ActivityFormType.PlayerRegistration:

                            #region Columns

                            var playerColumnCount = 1;
                            Action<string> addPlayerColumn = name =>
                            {
                                ws.Cell(1, playerColumnCount).Value = name;

                                playerColumnCount++;
                            };

                            addPlayerColumn(Messages.Team);
                            addPlayerColumn(Messages.League);
                            addPlayerColumn(Messages.TeamPlayers_ProfilePicture);
                            addPlayerColumn(Messages.IdentNum);
                            addPlayerColumn(Messages.FirstName);
                            addPlayerColumn(Messages.LastName);
                            addPlayerColumn(Messages.MiddleName);
                            addPlayerColumn(Messages.BirthDay);
                            addPlayerColumn(Messages.City);
                            addPlayerColumn(Messages.Email);
                            addPlayerColumn(Messages.Phone);
                            if (activity.RegistrationPrice)
                            {
                                addPlayerColumn(Messages.Activity_BuildForm_RegistrationPrice);
                                addPlayerColumn(Messages.Activity_BuildForm_RegistrationPaid);
                            }

                            //addPlayerColumn(Messages.Activity_BuildForm_RegistrationPaid);
                            if (activity.InsurancePrice)
                            {
                                addPlayerColumn(Messages.Activity_BuildForm_InsurancePrice);
                                //addPlayerColumn(Messages.Activity_BuildForm_InsurancePaid);
                                addPlayerColumn(Messages.Activity_BuildForm_InsurancePaid);
                            }

                            if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                            {
                                foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                {
                                    addColumn(!IsHebrew
                                        ? activityCustomPrice.TitleEng
                                        : activityCustomPrice.TitleHeb);
                                    addColumn(
                                        $"{(!IsHebrew ? activityCustomPrice.TitleEng : activityCustomPrice.TitleHeb)} {Messages.Paid}");
                                }
                            }

                            addPlayerColumn(Messages.Activity_BuildForm_RemainForPayment);
                            addPlayerColumn(Messages.Activity_BuildForm_ApproveMedicalCert);
                            //addPlayerColumn(Messages.Activity_BuildForm_ByBenefactor);
                            //addPlayerColumn(Messages.Activity_BuildForm_AmountPayedByBenefactor);
                            addPlayerColumn(Messages.Activity_BuildForm_Comments);
                            if (data.Any(x => x.CardComInvoiceNumber != null))
                            {
                                addPlayerColumn(Messages.Activity_Status_CardComInvoiceNumber);
                            }
                            if (data.Any(x => x.CardComNumberOfPayments != null))
                            {
                                addPlayerColumn(Messages.Activity_Status_CardComNumberOfPayments);
                            }
                            foreach (var customField in activity.ActivityForms.First()
                                .ActivityFormsDetails
                                .Where(x => x.Type.StartsWith("Custom") &&
                                            x.Type != ActivityFormControlType.CustomText.ToString() &&
                                            x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                            x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                            x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                            !x.IsDisabled))
                            {
                                addPlayerColumn(!IsHebrew
                                    ? customField.LabelTextEn
                                    : customField.LabelTextHeb);
                            }

                            addPlayerColumn(Messages.Activity_BuildForm_DateSubmitted);
                            addPlayerColumn(Messages.Activity_BuildForm_UnionComment);
                            addPlayerColumn(Messages.Activity_BuildForm_RegistrationStatusActive);

                            #endregion

                            var playerRowCount = 2;
                            Action<string> addPlayerRow = value =>
                            {
                                ws.Cell(playerRowCount, playerColumnCount).SetValue(value);

                                playerColumnCount++;
                            };

                            foreach (var submittedData in data.OrderByDescending(x => x.DateSubmitted))
                            {
                                playerColumnCount = 1;
                                var submittedCustomFields = new List<ActivityFormCustomField>();
                                if (submittedData.CustomFields != null)
                                {
                                    submittedCustomFields =
                                        JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(submittedData
                                            .CustomFields);
                                }

                                var submittedCustomPrices = new List<ActivityCustomPriceModel>();
                                if (submittedData.CustomPrices != null)
                                {
                                    submittedCustomPrices =
                                        JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(submittedData
                                            .CustomPrices);
                                }

                                addPlayerRow(submittedData.Team.TeamsDetails
                                                 .FirstOrDefault(x => x.SeasonId == activity.SeasonId)
                                                 ?.TeamName ?? submittedData.Team.Title);
                                addPlayerRow(submittedData.League.Name);
                                addPlayerRow(!string.IsNullOrWhiteSpace(submittedData.User.Image) ||
                                             submittedData.User.PlayerFiles.Any(
                                                 x => x.SeasonId == activity.SeasonId &&
                                                      x.FileType == (int) PlayerFileType.PlayerImage)
                                    ? Messages.Yes
                                    : Messages.No);
                                addPlayerRow(submittedData.User.IdentNum);
                                addPlayerRow(submittedData.User.FirstName);
                                addPlayerRow(submittedData.User.LastName);
                                addPlayerRow(submittedData.User.MiddleName);
                                addPlayerRow(submittedData.User.BirthDay?.ToString("d"));
                                addPlayerRow(submittedData.User.City);
                                addPlayerRow(submittedData.User.Email);
                                addPlayerRow(submittedData.User.Telephone);
                                if (activity.RegistrationPrice)
                                {
                                    addPlayerRow(submittedData.RegistrationPrice.ToString());
                                    addPlayerRow(submittedData.Paid > 0 && submittedData.RegistrationPaid == 0
                                        ? (submittedData.Paid - submittedData.InsurancePrice).ToString()
                                        : submittedData.RegistrationPaid.ToString());
                                }

                                //addPlayerRow(submittedData.RegistrationPaid.ToString());
                                if (activity.InsurancePrice)
                                {
                                    addPlayerRow(submittedData.InsurancePrice.ToString());
                                    //addPlayerRow(submittedData.InsurancePaid.ToString());
                                    addPlayerRow(submittedData.Paid > 0 && submittedData.InsurancePaid == 0
                                        ? (submittedData.Paid - submittedData.RegistrationPrice).ToString()
                                        : submittedData.InsurancePaid.ToString());
                                }

                                if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                                {
                                    foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                    {
                                        var regCustomPrice =
                                            submittedCustomPrices.FirstOrDefault(
                                                x => x.PropertyName == $"customPrice-{activityCustomPrice.Id}");
                                        addRow(regCustomPrice?.TotalPrice.ToString());
                                        addRow(regCustomPrice?.Paid.ToString());
                                    }
                                }

                                addPlayerRow((submittedData.RegistrationPrice + submittedData.InsurancePrice -
                                              ((submittedData.Paid > 0 && submittedData.RegistrationPaid == 0
                                                   ? submittedData.Paid - submittedData.InsurancePrice
                                                   : submittedData.RegistrationPaid) +
                                               (submittedData.Paid > 0 && submittedData.InsurancePaid == 0
                                                   ? submittedData.Paid - submittedData.RegistrationPrice
                                                   : submittedData.InsurancePaid)) +
                                              (submittedCustomPrices.Sum(x => x.TotalPrice) -
                                               submittedCustomPrices.Sum(x => x.Paid))).ToString());
                                addPlayerRow(submittedData.User.MedicalCertApprovements
                                                 .FirstOrDefault(c => c.SeasonId == activity.SeasonId)?.Approved == true
                                    ? Messages.Yes
                                    : Messages.No);
                                //addPlayerRow(submittedData.PaymentByBenefactor);
                                //addPlayerRow(submittedData.IsPaymentByBenefactor ? submittedData.Paid.ToString() : "");
                                addPlayerRow(submittedData.Comments);

                                if (data.Any(x => x.CardComInvoiceNumber != null))
                                {
                                    addPlayerRow(submittedData.CardComInvoiceNumber?.ToString());
                                }
                                if (data.Any(x => x.CardComNumberOfPayments != null))
                                {
                                    addPlayerRow(submittedData.CardComNumberOfPayments?.ToString());
                                }

                                foreach (var customField in activity.ActivityForms.First()
                                    .ActivityFormsDetails
                                    .Where(x => x.Type.StartsWith("Custom") &&
                                                x.Type != ActivityFormControlType.CustomText.ToString() &&
                                                x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                                x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                                !x.IsDisabled))
                                {
                                    addPlayerRow(submittedCustomFields
                                                     .FirstOrDefault(x => x.PropertyName == customField.PropertyName)
                                                     ?.Value ?? "");
                                }

                                addPlayerRow(submittedData.DateSubmitted.ToString("d"));
                                addPlayerRow(submittedData.UnionComment);
                                addPlayerRow(submittedData.IsActive ? Messages.Yes : Messages.No);

                                playerRowCount++;
                            }

                            break;

                        case ActivityFormType.TeamRegistration:

                            #region Columns

                            var teamColumnCount = 1;
                            Action<string> addTeamColumn = name =>
                            {
                                ws.Cell(1, teamColumnCount).Value = name;

                                teamColumnCount++;
                            };

                            addTeamColumn(Messages.Team);
                            addTeamColumn(Messages.League);
                            addTeamColumn(Messages.FirstName);
                            addTeamColumn(Messages.LastName);
                            addTeamColumn(Messages.MiddleName);
                            addTeamColumn(Messages.Email);
                            addTeamColumn(Messages.Phone);
                            addTeamColumn(Messages.Activity_BuildForm_AmountPaid);
                            addTeamColumn(Messages.Activity_BuildForm_RemainForPayment);
                            addTeamColumn(Messages.Activity_BuildForm_ByBenefactor);
                            addTeamColumn(Messages.Activity_BuildForm_AmountPayedByBenefactor);
                            addTeamColumn(Messages.Activity_BuildForm_SelfInsurance);
                            addTeamColumn(Messages.NeedShirts);
                            addTeamColumn(Messages.Activity_BuildForm_Comments);

                            if (data.Any(x => x.CardComInvoiceNumber != null))
                            {
                                addTeamColumn(Messages.Activity_Status_CardComInvoiceNumber);
                            }
                            if (data.Any(x => x.CardComNumberOfPayments != null))
                            {
                                addTeamColumn(Messages.Activity_Status_CardComNumberOfPayments);
                            }

                            foreach (var customField in activity.ActivityForms.First()
                                .ActivityFormsDetails
                                .Where(x => x.Type.StartsWith("Custom") &&
                                            x.Type != ActivityFormControlType.CustomText.ToString() &&
                                            x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                            x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                            !x.IsDisabled))
                            {
                                addTeamColumn(!IsHebrew
                                    ? customField.LabelTextEn
                                    : customField.LabelTextHeb);
                            }

                            addTeamColumn(Messages.Activity_BuildForm_DateSubmitted);
                            addTeamColumn(Messages.Activity_BuildForm_UnionComment);
                            addTeamColumn(Messages.Activity_ActivityRegistrationActive);

                            #endregion

                            var teamRowCount = 2;
                            Action<string> addTeamRow = value =>
                            {
                                ws.Cell(teamRowCount, teamColumnCount).SetValue(value);

                                teamColumnCount++;
                            };

                            foreach (var submittedData in data.OrderByDescending(x => x.DateSubmitted))
                            {
                                teamColumnCount = 1;
                                var submittedCustomFields = new List<ActivityFormCustomField>();
                                if (submittedData.CustomFields != null)
                                {
                                    submittedCustomFields =
                                        JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(submittedData
                                            .CustomFields);
                                }

                                var regPrice = submittedData.League.LeaguesPrices.FirstOrDefault(
                                                       lp => lp.PriceType == (int?) LeaguePriceType
                                                                 .TeamRegistrationPrice &&
                                                             lp.StartDate <= submittedData.DateSubmitted &&
                                                             lp.EndDate >= submittedData.DateSubmitted)
                                                   ?.Price ?? 0;

                                addTeamRow(submittedData.Team.TeamsDetails
                                               .FirstOrDefault(x => x.SeasonId == activity.SeasonId)
                                               ?.TeamName ?? submittedData.Team.Title);
                                addTeamRow(submittedData.League.Name);
                                addTeamRow(submittedData.User.FirstName);
                                addTeamRow(submittedData.User.LastName);
                                addTeamRow(submittedData.User.MiddleName);
                                addTeamRow(submittedData.User.Email);
                                addTeamRow(submittedData.User.Telephone);
                                addTeamRow(submittedData.RegistrationPaid.ToString());
                                if (submittedData.IsPaymentByBenefactor)
                                {
                                    addTeamRow("0");
                                }
                                else
                                {
                                    addTeamRow((regPrice - submittedData.Paid).ToString());
                                }

                                addTeamRow(submittedData.PaymentByBenefactor);
                                addTeamRow(submittedData.IsPaymentByBenefactor ? submittedData.Paid.ToString() : "");
                                addTeamRow(submittedData.SelfInsurance ? Messages.Yes : Messages.No);
                                addTeamRow(submittedData.Team.NeedShirts == true ? Messages.Yes : Messages.No);
                                addTeamRow(submittedData.Comments);

                                if (data.Any(x => x.CardComInvoiceNumber != null))
                                {
                                    addTeamRow(submittedData.CardComInvoiceNumber?.ToString());
                                }
                                if (data.Any(x => x.CardComNumberOfPayments != null))
                                {
                                    addTeamRow(submittedData.CardComNumberOfPayments?.ToString());
                                }

                                foreach (var customField in activity.ActivityForms.First()
                                    .ActivityFormsDetails
                                    .Where(x => x.Type.StartsWith("Custom") &&
                                                x.Type != ActivityFormControlType.CustomText.ToString() &&
                                                x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                                x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                                !x.IsDisabled))
                                {
                                    addTeamRow(submittedCustomFields
                                                   .FirstOrDefault(x => x.PropertyName == customField.PropertyName)
                                                   ?.Value ?? "");
                                }

                                addTeamRow(submittedData.DateSubmitted.ToString("d"));
                                addTeamRow(submittedData.UnionComment);
                                addTeamRow(submittedData.IsActive ? Messages.Yes : Messages.No);

                                teamRowCount++;
                            }

                            break;

                        case ActivityFormType.CustomGroup:

                            #region Columns

                            addColumn(Messages.Team);
                            addColumn(Messages.League);
                            addColumn(Messages.IdentNum);
                            addColumn(Messages.FirstName);
                            addColumn(Messages.LastName);
                            addColumn(Messages.MiddleName);
                            addColumn(Messages.Email);
                            addColumn(Messages.Phone);
                            addColumn(Messages.Address);
                            addColumn(Messages.BirthDay);
                            addColumn(Messages.Activity_BuildForm_RegistrationPrice);
                            addColumn(Messages.Activity_BuildForm_RegistrationPaid);

                            if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                            {
                                foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                {
                                    addColumn(!IsHebrew
                                        ? activityCustomPrice.TitleEng
                                        : activityCustomPrice.TitleHeb);
                                    addColumn(
                                        $"{(!IsHebrew ? activityCustomPrice.TitleEng : activityCustomPrice.TitleHeb)} {Messages.Paid}");
                                }
                            }

                            addColumn(Messages.Activity_BuildForm_RemainForPayment);
                            addColumn(Messages.Activity_BuildForm_ByBenefactor);
                            addColumn(Messages.Activity_BuildForm_AmountPayedByBenefactor);
                            addColumn(Messages.Activity_BuildForm_SelfInsurance);
                            addColumn(Messages.NeedShirts);
                            addColumn(Messages.Activity_BuildForm_Comments);

                            if (data.Any(x => x.CardComInvoiceNumber != null))
                            {
                                addColumn(Messages.Activity_Status_CardComInvoiceNumber);
                            }
                            if (data.Any(x => x.CardComNumberOfPayments != null))
                            {
                                addColumn(Messages.Activity_Status_CardComNumberOfPayments);
                            }

                            foreach (var customField in activity.ActivityForms.First()
                                .ActivityFormsDetails
                                .Where(x => x.Type.StartsWith("Custom") &&
                                            x.Type != ActivityFormControlType.CustomText.ToString() &&
                                            x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                            x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                            !x.IsDisabled))
                            {
                                addColumn(!IsHebrew
                                    ? customField.LabelTextEn
                                    : customField.LabelTextHeb);
                            }

                            addColumn(Messages.Activity_BuildForm_DateSubmitted);
                            addColumn(Messages.Activity_BuildForm_UnionComment);
                            addColumn(Messages.Activity_ActivityRegistrationActive);

                            #endregion

                            foreach (var submittedData in data.OrderByDescending(x => x.DateSubmitted))
                            {
                                columnCount = 1;
                                var submittedCustomFields = new List<ActivityFormCustomField>();
                                if (submittedData.CustomFields != null)
                                {
                                    submittedCustomFields =
                                        JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(submittedData
                                            .CustomFields);
                                }

                                var submittedCustomPrices = new List<ActivityCustomPriceModel>();
                                if (submittedData.CustomPrices != null)
                                {
                                    submittedCustomPrices =
                                        JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(submittedData
                                            .CustomPrices);
                                }

                                addRow(submittedData.Team?.TeamsDetails
                                           .FirstOrDefault(x => x.SeasonId == activity.SeasonId)
                                           ?.TeamName ?? submittedData.Team?.Title ??
                                       Messages.Activity_NoTeamPlaceholder);
                                addRow(submittedData.League.Name);
                                addRow(submittedData.User.IdentNum);
                                addRow(submittedData.User.FirstName);
                                addRow(submittedData.User.LastName);
                                addRow(submittedData.User.MiddleName);
                                addRow(submittedData.User.Email);
                                addRow(submittedData.User.Telephone);
                                addRow(submittedData.User.City);
                                addRow(submittedData.User.BirthDay?.ToString("d"));
                                addRow(submittedData.RegistrationPrice.ToString());
                                addRow(submittedData.RegistrationPaid.ToString());

                                if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                                {
                                    foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                    {
                                        var regCustomPrice =
                                            submittedCustomPrices.FirstOrDefault(
                                                x => x.PropertyName == $"customPrice-{activityCustomPrice.Id}");
                                        addRow(regCustomPrice?.TotalPrice.ToString());
                                        addRow(regCustomPrice?.Paid.ToString());
                                    }
                                }

                                if (submittedData.IsPaymentByBenefactor)
                                {
                                    addRow("0");
                                }
                                else
                                {
                                    addRow((
                                        (submittedData.RegistrationPrice) +
                                        (submittedData.Activity.CustomPricesEnabled
                                            ? submittedCustomPrices.Sum(x => x.TotalPrice)
                                            : 0)
                                        - (
                                            (submittedData.RegistrationPaid) +
                                            (submittedData.Activity.CustomPricesEnabled
                                                ? submittedCustomPrices.Sum(x => x.Paid)
                                                : 0)
                                        )).ToString());
                                }

                                addRow(submittedData.PaymentByBenefactor);
                                addRow(submittedData.IsPaymentByBenefactor
                                    ? ((submittedData.RegistrationPaid) +
                                       (submittedData.Activity.CustomPricesEnabled
                                           ? submittedCustomPrices.Sum(x => x.Paid)
                                           : 0)).ToString()
                                    : "");
                                addRow(submittedData.SelfInsurance ? Messages.Yes : Messages.No);
                                addRow(submittedData.Team?.NeedShirts == true ? Messages.Yes : Messages.No);
                                addRow(submittedData.Comments);

                                if (data.Any(x => x.CardComInvoiceNumber != null))
                                {
                                    addRow(submittedData.CardComInvoiceNumber?.ToString());
                                }
                                if (data.Any(x => x.CardComNumberOfPayments != null))
                                {
                                    addRow(submittedData.CardComNumberOfPayments?.ToString());
                                }

                                foreach (var customField in activity.ActivityForms.First()
                                    .ActivityFormsDetails
                                    .Where(x => x.Type.StartsWith("Custom") &&
                                                x.Type != ActivityFormControlType.CustomText.ToString() &&
                                                x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                                x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                                !x.IsDisabled))
                                {
                                    addRow(submittedCustomFields
                                               .FirstOrDefault(x => x.PropertyName == customField.PropertyName)
                                               ?.Value ?? "");
                                }

                                addRow(submittedData.DateSubmitted.ToString("d"));
                                addRow(submittedData.UnionComment);
                                addRow(submittedData.IsActive ? Messages.Yes : Messages.No);

                                rowCount++;
                            }

                            break;

                        case ActivityFormType.CustomPersonal:

                            #region Columns

                            if (!activity.NoTeamRegistration)
                            {
                                addColumn(Messages.Team);
                                addColumn(Messages.League);
                            }

                            addColumn(Messages.TeamPlayers_ProfilePicture);
                            addColumn(Messages.IdentNum);
                            addColumn(Messages.FirstName);
                            addColumn(Messages.LastName);
                            addColumn(Messages.MiddleName);
                            addColumn(Messages.BirthDay);
                            addColumn(Messages.Gender);
                            addColumn(Messages.Address);
                            addColumn(Messages.City);
                            addColumn(Messages.Email);
                            addColumn(Messages.Phone);
                            if (activity.RegistrationPrice)
                            {
                                addColumn(Messages.Activity_BuildForm_RegistrationPrice);
                                addColumn(Messages.Activity_BuildForm_RegistrationPaid);
                            }

                            if (activity.InsurancePrice)
                            {
                                addColumn(Messages.Activity_BuildForm_InsurancePrice);
                                addColumn(Messages.Activity_BuildForm_InsurancePaid);
                                addColumn(Messages.Activity_DoNotPayInsurance);
                            }

                            if (activity.MembersFee)
                            {
                                addColumn(Messages.LeagueDetail_MemberFees);
                                addColumn(Messages.Activity_MembersFeePaid);
                                addColumn(Messages.Activity_DoNotPayMembersFee);
                            }

                            if (activity.HandlingFee)
                            {
                                addColumn(Messages.LeagueDetail_HandlingFee);
                                addColumn(Messages.Activity_HandlingFeePaid);
                                addColumn(Messages.Activity_DoNotPayHandlingFee);
                            }

                            if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                            {
                                foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                {
                                    addColumn(!IsHebrew
                                        ? activityCustomPrice.TitleEng
                                        : activityCustomPrice.TitleHeb);
                                    addColumn(
                                        $"{(!IsHebrew ? activityCustomPrice.TitleEng : activityCustomPrice.TitleHeb)} {Messages.Paid}");
                                }
                            }

                            addColumn(Messages.Activity_BuildForm_RemainForPayment);
                            addColumn(Messages.Activity_BuildForm_ApproveMedicalCert);
                            addColumn(Messages.Activity_IsUnionPlayer);
                            addColumn(Messages.Activity_BuildForm_Comments);

                            if (data.Any(x => x.CardComInvoiceNumber != null))
                            {
                                addColumn(Messages.Activity_Status_CardComInvoiceNumber);
                            }
                            if (data.Any(x => x.CardComNumberOfPayments != null))
                            {
                                addColumn(Messages.Activity_Status_CardComNumberOfPayments);
                            }

                            foreach (var customField in activity.ActivityForms.First()
                                .ActivityFormsDetails
                                .Where(x => x.Type.StartsWith("Custom") &&
                                            x.Type != ActivityFormControlType.CustomText.ToString() &&
                                            x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                            x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                            !x.IsDisabled))
                            {
                                addColumn(!IsHebrew
                                    ? customField.LabelTextEn
                                    : customField.LabelTextHeb);
                            }

                            addColumn(Messages.Activity_BuildForm_DateSubmitted);
                            addColumn(Messages.Activity_BuildForm_UnionComment);
                            addColumn(Messages.Activity_BuildForm_RegistrationStatusActive);

                            #endregion

                            foreach (var submittedData in data.OrderByDescending(x => x.DateSubmitted))
                            {
                                columnCount = 1;
                                var submittedCustomFields = new List<ActivityFormCustomField>();
                                if (submittedData.CustomFields != null)
                                {
                                    submittedCustomFields =
                                        JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(submittedData
                                            .CustomFields);
                                }

                                var submittedCustomPrices = new List<ActivityCustomPriceModel>();
                                if (submittedData.CustomPrices != null)
                                {
                                    submittedCustomPrices =
                                        JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(submittedData
                                            .CustomPrices);
                                }

                                var isEscortPlayer =
                                    submittedData.User.TeamsPlayers
                                        .FirstOrDefault(
                                            x => x.TeamId == submittedData.TeamId && x.SeasonId == activity.SeasonId)
                                        ?.IsEscortPlayer == true;
                                var isUnionApprovedPlayer =
                                    allBuiltInRegistrations.Any(x => x.PlayerId == submittedData.PlayerId);

                                if (!activity.NoTeamRegistration)
                                {
                                    addRow(submittedData.Team?.TeamsDetails
                                               .FirstOrDefault(x => x.SeasonId == activity.SeasonId)
                                               ?.TeamName ?? submittedData.Team?.Title);
                                    addRow(submittedData.League?.Name);
                                }

                                addRow(!string.IsNullOrWhiteSpace(submittedData.User.Image) ||
                                       submittedData.User.PlayerFiles.Any(
                                           x => x.SeasonId == activity.SeasonId &&
                                                x.FileType == (int) PlayerFileType.PlayerImage)
                                    ? Messages.Yes
                                    : Messages.No);
                                addRow(submittedData.User.IdentNum);
                                addRow(
                                    $"{(isEscortPlayer ? Messages.Player_EscortIndicator : string.Empty)}{submittedData.User.FirstName}");
                                addRow(submittedData.User.LastName);
                                addRow(submittedData.User.MiddleName);
                                addRow(submittedData.User.BirthDay?.ToString("d"));
                                addRow(submittedData.User.GenderId == 0 ? Messages.Female :
                                    submittedData.User.GenderId == 1 ? Messages.Male : string.Empty);
                                addRow(submittedData.User.Address);
                                addRow(submittedData.User.City);
                                addRow(submittedData.User.Email);
                                addRow(submittedData.User.Telephone);

                                if (activity.RegistrationPrice)
                                {
                                    addRow(submittedData.RegistrationPrice.ToString());
                                    addRow(submittedData.RegistrationPaid.ToString());
                                }

                                if (activity.InsurancePrice)
                                {
                                    if (activity.UnionApprovedPlayerNoInsurance && isUnionApprovedPlayer)
                                    {
                                        addRow(string.Empty);
                                        addRow(string.Empty);
                                    }
                                    else
                                    {
                                        addRow(submittedData.InsurancePrice.ToString());
                                        addRow(submittedData.InsurancePaid.ToString());
                                    }

                                    addRow(submittedData.DisableInsurancePayment ? Messages.Yes : Messages.No);
                                }

                                if (activity.MembersFee)
                                {
                                    addRow(submittedData.MembersFee.ToString());
                                    addRow(submittedData.MembersFeePaid.ToString());
                                    addRow(submittedData.DisableMembersFeePayment ? Messages.Yes : Messages.No);
                                }

                                if (activity.HandlingFee)
                                {
                                    addRow(submittedData.HandlingFee.ToString());
                                    addRow(submittedData.HandlingFeePaid.ToString());
                                    addRow(submittedData.DisableHandlingFeePayment ? Messages.Yes : Messages.No);
                                }

                                if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                                {
                                    foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                    {
                                        var regCustomPrice =
                                            submittedCustomPrices.FirstOrDefault(
                                                x => x.PropertyName == $"customPrice-{activityCustomPrice.Id}");
                                        addRow(regCustomPrice?.TotalPrice.ToString());
                                        addRow(regCustomPrice?.Paid.ToString());
                                    }
                                }

                                addRow((
                                    (submittedData.Activity.RegistrationPrice ? submittedData.RegistrationPrice : 0) +
                                    (submittedData.Activity.InsurancePrice
                                        ? (submittedData.DisableInsurancePayment ? 0 : submittedData.InsurancePrice)
                                        : 0) +
                                    (submittedData.Activity.MembersFee
                                        ? (submittedData.DisableMembersFeePayment ? 0 : submittedData.MembersFee)
                                        : 0) +
                                    (submittedData.Activity.HandlingFee
                                        ? (submittedData.DisableHandlingFeePayment ? 0 : submittedData.HandlingFee)
                                        : 0) +
                                    (submittedData.Activity.CustomPricesEnabled
                                        ? submittedCustomPrices.Sum(x => x.TotalPrice)
                                        : 0)
                                    - (
                                        (submittedData.Activity.RegistrationPrice
                                            ? submittedData.RegistrationPaid
                                            : 0) +
                                        (submittedData.Activity.InsurancePrice ? submittedData.InsurancePaid : 0) +
                                        (submittedData.Activity.MembersFee ? submittedData.MembersFeePaid : 0) +
                                        (submittedData.Activity.HandlingFee ? submittedData.HandlingFeePaid : 0) +
                                        (submittedData.Activity.CustomPricesEnabled
                                            ? submittedCustomPrices.Sum(x => x.Paid)
                                            : 0)
                                    )).ToString());

                                addRow(submittedData.User.MedicalCertApprovements
                                           .FirstOrDefault(c => c.SeasonId == activity.SeasonId)?.Approved == true
                                    ? Messages.Yes
                                    : Messages.No);
                                addRow(isUnionApprovedPlayer ? Messages.Yes : Messages.No);
                                addRow(submittedData.Comments);

                                if (data.Any(x => x.CardComInvoiceNumber != null))
                                {
                                    addRow(submittedData.CardComInvoiceNumber?.ToString());
                                }
                                if (data.Any(x => x.CardComNumberOfPayments != null))
                                {
                                    addRow(submittedData.CardComNumberOfPayments?.ToString());
                                }

                                foreach (var customField in activity.ActivityForms.First()
                                    .ActivityFormsDetails
                                    .Where(x => x.Type.StartsWith("Custom") &&
                                                x.Type != ActivityFormControlType.CustomText.ToString() &&
                                                x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                                x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                                !x.IsDisabled))
                                {
                                    addRow(submittedCustomFields
                                               .FirstOrDefault(x => x.PropertyName == customField.PropertyName)
                                               ?.Value ?? "");
                                }

                                addRow(submittedData.DateSubmitted.ToString("d"));
                                addRow(submittedData.UnionComment);
                                addRow(submittedData.IsActive ? Messages.Yes : Messages.No);

                                rowCount++;
                            }

                            break;

                        case ActivityFormType.ClubPlayerRegistration:

                            #region Columns

                            var clubPlayerRegColumnCount = 1;
                            Action<string> addClubPlayerRegColumn = name =>
                            {
                                ws.Cell(1, clubPlayerRegColumnCount).Value = name;

                                clubPlayerRegColumnCount++;
                            };

                            addClubPlayerRegColumn(Messages.Team);
                            addClubPlayerRegColumn(Messages.SchoolName);
                            addClubPlayerRegColumn(Messages.TeamPlayers_ProfilePicture);
                            addClubPlayerRegColumn(Messages.IdentNum);
                            addClubPlayerRegColumn(Messages.BrotherIdNum);
                            addClubPlayerRegColumn(Messages.FirstName);
                            addClubPlayerRegColumn(Messages.LastName);
                            addClubPlayerRegColumn(Messages.MiddleName);
                            addClubPlayerRegColumn(Messages.Email);
                            addClubPlayerRegColumn(Messages.Phone);
                            addClubPlayerRegColumn(Messages.Address);
                            addClubPlayerRegColumn(Messages.BirthDay);
                            if (activity.AllowToEnterDateToAdjustPrices)
                            {
                                addClubPlayerRegColumn(Messages.Activity_BuildForm_StartDateToAdjustPrices);
                            }
                            addClubPlayerRegColumn(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice);
                            addClubPlayerRegColumn(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice_Paid);
                            addClubPlayerRegColumn(Messages.TeamDetails_ParticipationPrice);
                            addClubPlayerRegColumn(Messages.TeamDetails_ParticipationPrice_Paid);
                            addClubPlayerRegColumn(Messages.TeamDetails_PlayerInsurancePrice);
                            addClubPlayerRegColumn(Messages.Activity_BuildForm_InsurancePaid);
                            if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                            {
                                foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                {
                                    addClubPlayerRegColumn(!IsHebrew
                                        ? activityCustomPrice.TitleEng
                                        : activityCustomPrice.TitleHeb);
                                    addClubPlayerRegColumn(
                                        $"{(!IsHebrew ? activityCustomPrice.TitleEng : activityCustomPrice.TitleHeb)} {Messages.Paid}");
                                }
                            }

                            addClubPlayerRegColumn(Messages.Activity_BuildForm_RemainForPayment);
                            addClubPlayerRegColumn(Messages.Activity_BuildForm_ApproveMedicalCert);
                            addClubPlayerRegColumn(Messages.Activity_BuildForm_Comments);

                            if (data.Any(x => x.CardComInvoiceNumber != null))
                            {
                                addClubPlayerRegColumn(Messages.Activity_Status_CardComInvoiceNumber);
                            }
                            if (data.Any(x => x.CardComNumberOfPayments != null))
                            {
                                addClubPlayerRegColumn(Messages.Activity_Status_CardComNumberOfPayments);
                            }

                            foreach (var customField in activity.ActivityForms.First()
                                .ActivityFormsDetails
                                .Where(x => x.Type.StartsWith("Custom") &&
                                            x.Type != ActivityFormControlType.CustomText.ToString() &&
                                            x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                            x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                            !x.IsDisabled))
                            {
                                addClubPlayerRegColumn(!IsHebrew
                                    ? customField.LabelTextEn
                                    : customField.LabelTextHeb);
                            }

                            addClubPlayerRegColumn(Messages.Activity_BuildForm_DateSubmitted);
                            addClubPlayerRegColumn(Messages.Activity_BuildForm_ClubComment);
                            addClubPlayerRegColumn(Messages.Activity_BuildForm_RegistrationStatusActive);

                            #endregion

                            var addClubPlayerRegRowCount = 2;
                            Action<string, XLDataType> addClubPlayerRegRow = (value, type) =>
                            {
                                ws.Cell(addClubPlayerRegRowCount, clubPlayerRegColumnCount).SetValue(value);
                                ws.Cell(addClubPlayerRegRowCount, clubPlayerRegColumnCount).SetDataType(type);

                                clubPlayerRegColumnCount++;
                            };

                            foreach (var submittedData in data.OrderByDescending(x => x.DateSubmitted))
                            {
                                clubPlayerRegColumnCount = 1;
                                var submittedCustomFields = new List<ActivityFormCustomField>();
                                if (submittedData.CustomFields != null)
                                {
                                    submittedCustomFields =
                                        JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(submittedData
                                            .CustomFields);
                                }

                                var submittedCustomPrices = new List<ActivityCustomPriceModel>();
                                if (submittedData.CustomPrices != null)
                                {
                                    submittedCustomPrices =
                                        JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(submittedData
                                            .CustomPrices);
                                }

                                addClubPlayerRegRow(submittedData.Team.TeamsDetails
                                                        .FirstOrDefault(x => x.SeasonId == activity.SeasonId)
                                                        ?.TeamName ?? submittedData.Team.Title, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.Team?.SchoolTeams
                                    ?.FirstOrDefault(x => x.School.SeasonId == activity.SeasonId)
                                    ?.School?.Name, XLDataType.Text);
                                addClubPlayerRegRow(!string.IsNullOrWhiteSpace(submittedData.User.Image) ||
                                                    submittedData.User.PlayerFiles.Any(
                                                        x => x.SeasonId == activity.SeasonId &&
                                                             x.FileType == (int) PlayerFileType.PlayerImage)
                                    ? Messages.Yes
                                    : Messages.No, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.IdentNum, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User1?.IdentNum, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.FirstName, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.LastName, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.MiddleName, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.Email, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.Telephone, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.City, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.BirthDay?.ToString("d"), XLDataType.DateTime);
                                if (activity.AllowToEnterDateToAdjustPrices)
                                {
                                    addClubPlayerRegRow(submittedData.PlayerStartDate?.ToString("d"), XLDataType.DateTime);
                                }
                                addClubPlayerRegRow(submittedData.RegistrationPrice.ToString(), XLDataType.Number);
                                addClubPlayerRegRow(submittedData.RegistrationPaid.ToString(), XLDataType.Number);
                                addClubPlayerRegRow(submittedData.ParticipationPrice.ToString(), XLDataType.Number);
                                addClubPlayerRegRow(submittedData.ParticipationPaid.ToString(), XLDataType.Number);
                                addClubPlayerRegRow(submittedData.InsurancePrice.ToString(), XLDataType.Number);
                                addClubPlayerRegRow(submittedData.InsurancePaid.ToString(), XLDataType.Number);
                                if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                                {
                                    foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                    {
                                        var regCustomPrice =
                                            submittedCustomPrices.FirstOrDefault(
                                                x => x.PropertyName == $"customPrice-{activityCustomPrice.Id}");
                                        addClubPlayerRegRow(regCustomPrice?.TotalPrice.ToString(), XLDataType.Number);
                                        addClubPlayerRegRow(regCustomPrice?.Paid.ToString(), XLDataType.Number);
                                    }
                                }

                                addClubPlayerRegRow(
                                    ((submittedData.RegistrationPrice +
                                      submittedData.ParticipationPrice +
                                      submittedData.InsurancePrice +
                                      submittedCustomPrices.Sum(x => x.TotalPrice))
                                     -
                                     (submittedData.RegistrationPaid +
                                      submittedData.ParticipationPaid +
                                      submittedData.InsurancePaid +
                                      submittedCustomPrices.Sum(x => x.Paid)))
                                    .ToString(), XLDataType.Number);
                                addClubPlayerRegRow(
                                    submittedData.User.MedicalCertApprovements
                                        .FirstOrDefault(c => c.SeasonId == activity.SeasonId)?.Approved == true
                                        ? Messages.Yes
                                        : Messages.No, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.Comments, XLDataType.Text);

                                if (data.Any(x => x.CardComInvoiceNumber != null))
                                {
                                    addClubPlayerRegRow(submittedData.CardComInvoiceNumber?.ToString(), XLDataType.Text);
                                }
                                if (data.Any(x => x.CardComNumberOfPayments != null))
                                {
                                    addClubPlayerRegRow(submittedData.CardComNumberOfPayments?.ToString(), XLDataType.Text);
                                }

                                foreach (var customField in activity.ActivityForms.First()
                                    .ActivityFormsDetails
                                    .Where(x => x.Type.StartsWith("Custom") &&
                                                x.Type != ActivityFormControlType.CustomText.ToString() &&
                                                x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                                x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                                !x.IsDisabled))
                                {
                                    addClubPlayerRegRow(submittedCustomFields
                                                            .FirstOrDefault(
                                                                x => x.PropertyName == customField.PropertyName)
                                                            ?.Value ?? "", XLDataType.Text);
                                }

                                addClubPlayerRegRow(submittedData.DateSubmitted.ToString("d"), XLDataType.DateTime);
                                addClubPlayerRegRow(submittedData.ClubComment, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.IsActive ? Messages.Yes : Messages.No,
                                    XLDataType.Text);

                                addClubPlayerRegRowCount++;
                            }

                            break;

                        case ActivityFormType.ClubCustomPersonal:

                            #region Columns

                            if (!activity.NoTeamRegistration)
                            {
                                addColumn(Messages.Team);
                                addColumn(Messages.SchoolName);
                            }

                            addColumn(Messages.TeamPlayers_ProfilePicture);
                            addColumn(Messages.IdentNum);
                            addColumn(Messages.BrotherIdNum);
                            addColumn(Messages.FirstName);
                            addColumn(Messages.LastName);
                            addColumn(Messages.MiddleName);
                            addColumn(Messages.Email);
                            addColumn(Messages.Phone);
                            addColumn(Messages.Address);
                            addColumn(Messages.BirthDay);
                            if (activity.AllowToEnterDateToAdjustPrices)
                            {
                                addColumn(Messages.Activity_BuildForm_StartDateToAdjustPrices);
                            }
                            addColumn(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice);
                            addColumn(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice_Paid);
                            addColumn(Messages.TeamDetails_ParticipationPrice);
                            addColumn(Messages.TeamDetails_ParticipationPrice_Paid);
                            addColumn(Messages.TeamDetails_PlayerInsurancePrice);
                            addColumn(Messages.Activity_BuildForm_InsurancePaid);
                            if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                            {
                                foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                {
                                    addColumn(!IsHebrew
                                        ? activityCustomPrice.TitleEng
                                        : activityCustomPrice.TitleHeb);
                                    addColumn(
                                        $"{(!IsHebrew ? activityCustomPrice.TitleEng : activityCustomPrice.TitleHeb)} {Messages.Paid}");
                                }
                            }

                            addColumn(Messages.Activity_BuildForm_RemainForPayment);
                            addColumn(Messages.Activity_BuildForm_ApproveMedicalCert);
                            addColumn(Messages.Activity_BuildForm_Comments);

                            if (data.Any(x => x.CardComInvoiceNumber != null))
                            {
                                addColumn(Messages.Activity_Status_CardComInvoiceNumber);
                            }
                            if (data.Any(x => x.CardComNumberOfPayments != null))
                            {
                                addColumn(Messages.Activity_Status_CardComNumberOfPayments);
                            }

                            foreach (var customField in activity.ActivityForms.First()
                                .ActivityFormsDetails
                                .Where(x => x.Type.StartsWith("Custom") &&
                                            x.Type != ActivityFormControlType.CustomText.ToString() &&
                                            x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                            x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                            !x.IsDisabled))
                            {
                                addColumn(!IsHebrew
                                    ? customField.LabelTextEn
                                    : customField.LabelTextHeb);
                            }

                            addColumn(Messages.Activity_BuildForm_DateSubmitted);
                            addColumn(Messages.Activity_BuildForm_ClubComment);
                            addColumn(Messages.Activity_BuildForm_RegistrationStatusActive);

                            #endregion

                            foreach (var submittedData in data.OrderByDescending(x => x.DateSubmitted))
                            {
                                columnCount = 1;
                                var submittedCustomFields = new List<ActivityFormCustomField>();
                                if (submittedData.CustomFields != null)
                                {
                                    submittedCustomFields =
                                        JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(submittedData
                                            .CustomFields);
                                }

                                var submittedCustomPrices = new List<ActivityCustomPriceModel>();
                                if (submittedData.CustomPrices != null)
                                {
                                    submittedCustomPrices =
                                        JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(submittedData
                                            .CustomPrices);
                                }

                                if (!activity.NoTeamRegistration)
                                {
                                    addRowWithType(submittedData.Team.TeamsDetails
                                                       .FirstOrDefault(x => x.SeasonId == activity.SeasonId)
                                                       ?.TeamName ?? submittedData.Team.Title, XLDataType.Text);
                                    addRowWithType(submittedData.Team?.SchoolTeams
                                        ?.FirstOrDefault(x => x.School.SeasonId == activity.SeasonId)
                                        ?.School?.Name, XLDataType.Text);
                                }

                                addRowWithType(!string.IsNullOrWhiteSpace(submittedData.User.Image) ||
                                               submittedData.User.PlayerFiles.Any(
                                                   x => x.SeasonId == activity.SeasonId &&
                                                        x.FileType == (int) PlayerFileType.PlayerImage)
                                    ? Messages.Yes
                                    : Messages.No, XLDataType.Text);
                                addRowWithType(submittedData.User.IdentNum, XLDataType.Text);
                                addRowWithType(submittedData.User1?.IdentNum, XLDataType.Text);
                                addRowWithType(submittedData.User.FirstName, XLDataType.Text);
                                addRowWithType(submittedData.User.LastName, XLDataType.Text);
                                addRowWithType(submittedData.User.MiddleName, XLDataType.Text);
                                addRowWithType(submittedData.User.Email, XLDataType.Text);
                                addRowWithType(submittedData.User.Telephone, XLDataType.Text);
                                addRowWithType(submittedData.User.City, XLDataType.Text);
                                addRowWithType(submittedData.User.BirthDay?.ToString("d"), XLDataType.DateTime);
                                if (activity.AllowToEnterDateToAdjustPrices)
                                {
                                    addRowWithType(submittedData.PlayerStartDate?.ToString("d"), XLDataType.DateTime);
                                }
                                addRowWithType(submittedData.RegistrationPrice.ToString(), XLDataType.Number);
                                addRowWithType(submittedData.RegistrationPaid.ToString(), XLDataType.Number);
                                addRowWithType(submittedData.ParticipationPrice.ToString(), XLDataType.Number);
                                addRowWithType(submittedData.ParticipationPaid.ToString(), XLDataType.Number);
                                addRowWithType(submittedData.InsurancePrice.ToString(), XLDataType.Number);
                                addRowWithType(submittedData.InsurancePaid.ToString(), XLDataType.Number);
                                if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                                {
                                    foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                    {
                                        var regCustomPrice =
                                            submittedCustomPrices.FirstOrDefault(
                                                x => x.PropertyName == $"customPrice-{activityCustomPrice.Id}");
                                        addRowWithType(regCustomPrice?.TotalPrice.ToString(), XLDataType.Number);
                                        addRowWithType(regCustomPrice?.Paid.ToString(), XLDataType.Number);
                                    }
                                }

                                addRowWithType(
                                    ((submittedData.RegistrationPrice +
                                      submittedData.ParticipationPrice +
                                      submittedData.InsurancePrice +
                                      submittedCustomPrices.Sum(x => x.TotalPrice))
                                     -
                                     (submittedData.RegistrationPaid +
                                      submittedData.ParticipationPaid +
                                      submittedData.InsurancePaid +
                                      submittedCustomPrices.Sum(x => x.Paid)))
                                    .ToString(), XLDataType.Number);
                                addRowWithType(submittedData.User.MedicalCertApprovements
                                                   .FirstOrDefault(c => c.SeasonId == activity.SeasonId)?.Approved ==
                                               true
                                    ? Messages.Yes
                                    : Messages.No, XLDataType.Text);
                                addRowWithType(submittedData.Comments, XLDataType.Text);

                                if (data.Any(x => x.CardComInvoiceNumber != null))
                                {
                                    addRowWithType(submittedData.CardComInvoiceNumber?.ToString(), XLDataType.Text);
                                }
                                if (data.Any(x => x.CardComNumberOfPayments != null))
                                {
                                    addRowWithType(submittedData.CardComNumberOfPayments?.ToString(), XLDataType.Text);
                                }

                                foreach (var customField in activity.ActivityForms.First()
                                    .ActivityFormsDetails
                                    .Where(x => x.Type.StartsWith("Custom") &&
                                                x.Type != ActivityFormControlType.CustomText.ToString() &&
                                                x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                                x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                                !x.IsDisabled))
                                {
                                    addRowWithType(submittedCustomFields
                                                       .FirstOrDefault(
                                                           x => x.PropertyName == customField.PropertyName)
                                                       ?.Value ?? "", XLDataType.Text);
                                }

                                addRowWithType(submittedData.DateSubmitted.ToString("d"), XLDataType.DateTime);
                                addRowWithType(submittedData.ClubComment, XLDataType.Text);
                                addRowWithType(submittedData.IsActive ? Messages.Yes : Messages.No, XLDataType.Text);

                                rowCount++;
                            }

                            break;

                        case ActivityFormType.DepartmentClubPlayerRegistration:

                            #region Columns

                            addColumn(Messages.Team);
                            addColumn(Messages.SchoolName);
                            addColumn(Messages.TeamPlayers_ProfilePicture);
                            addColumn(Messages.IdentNum);
                            addColumn(Messages.FirstName);
                            addColumn(Messages.LastName);
                            addColumn(Messages.MiddleName);
                            addColumn(Messages.Email);
                            addColumn(Messages.Phone);
                            addColumn(Messages.Address);
                            addColumn(Messages.BirthDay);
                            addColumn(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice);
                            addColumn(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice_Paid);
                            addColumn(Messages.TeamDetails_ParticipationPrice);
                            addColumn(Messages.TeamDetails_ParticipationPrice_Paid);
                            addColumn(Messages.TeamDetails_PlayerInsurancePrice);
                            addColumn(Messages.Activity_BuildForm_InsurancePaid);
                            addColumn(Messages.Activity_BuildForm_ApproveMedicalCert);
                            addColumn(Messages.Activity_BuildForm_Comments);

                            if (data.Any(x => x.CardComInvoiceNumber != null))
                            {
                                addColumn(Messages.Activity_Status_CardComInvoiceNumber);
                            }
                            if (data.Any(x => x.CardComNumberOfPayments != null))
                            {
                                addColumn(Messages.Activity_Status_CardComNumberOfPayments);
                            }

                            foreach (var customField in activity.ActivityForms.First()
                                .ActivityFormsDetails
                                .Where(x => x.Type.StartsWith("Custom") &&
                                            x.Type != ActivityFormControlType.CustomText.ToString() &&
                                            x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                            x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                            !x.IsDisabled))
                            {
                                addColumn(!IsHebrew
                                    ? customField.LabelTextEn
                                    : customField.LabelTextHeb);
                            }

                            addColumn(Messages.Activity_BuildForm_DateSubmitted);
                            addColumn(Messages.Activity_BuildForm_ClubComment);
                            addColumn(Messages.Activity_BuildForm_RegistrationStatusActive);

                            #endregion

                            foreach (var submittedData in data.OrderByDescending(x => x.DateSubmitted))
                            {
                                columnCount = 1;
                                var submittedCustomFields = new List<ActivityFormCustomField>();
                                if (submittedData.CustomFields != null)
                                {
                                    submittedCustomFields =
                                        JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(submittedData
                                            .CustomFields);
                                }

                                addRow(submittedData.Team.TeamsDetails
                                           .FirstOrDefault(x => x.SeasonId == activity.SeasonId)
                                           ?.TeamName ?? submittedData.Team.Title);
                                addRow(submittedData.Team?.SchoolTeams
                                    ?.FirstOrDefault(x => x.School.SeasonId == activity.SeasonId)
                                    ?.School?.Name);
                                addRow(!string.IsNullOrWhiteSpace(submittedData.User.Image) ||
                                       submittedData.User.PlayerFiles.Any(
                                           x => x.SeasonId == activity.SeasonId &&
                                                x.FileType == (int) PlayerFileType.PlayerImage)
                                    ? Messages.Yes
                                    : Messages.No);
                                addRow(submittedData.User.IdentNum);
                                addRow(submittedData.User.FirstName);
                                addRow(submittedData.User.LastName);
                                addRow(submittedData.User.MiddleName);
                                addRow(submittedData.User.Email);
                                addRow(submittedData.User.Telephone);
                                addRow(submittedData.User.City);
                                addRow(submittedData.User.BirthDay?.ToString("d"));
                                addRow(submittedData.RegistrationPrice.ToString());
                                addRow(submittedData.RegistrationPaid.ToString());
                                addRow(submittedData.ParticipationPrice.ToString());
                                addRow(submittedData.ParticipationPaid.ToString());
                                addRow(submittedData.InsurancePrice.ToString());
                                addRow(submittedData.InsurancePaid.ToString());
                                addRow(submittedData.User.MedicalCertApprovements
                                           .FirstOrDefault(c => c.SeasonId == activity.SeasonId)?.Approved == true
                                    ? Messages.Yes
                                    : Messages.No);
                                addRow(submittedData.Comments);

                                if (data.Any(x => x.CardComInvoiceNumber != null))
                                {
                                    addRow(submittedData.CardComInvoiceNumber?.ToString());
                                }
                                if (data.Any(x => x.CardComNumberOfPayments != null))
                                {
                                    addRow(submittedData.CardComNumberOfPayments?.ToString());
                                }

                                foreach (var customField in activity.ActivityForms.First()
                                    .ActivityFormsDetails
                                    .Where(x => x.Type.StartsWith("Custom") &&
                                                x.Type != ActivityFormControlType.CustomText.ToString() &&
                                                x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                                x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                                !x.IsDisabled))
                                {
                                    addRow(submittedCustomFields
                                               .FirstOrDefault(
                                                   x => x.PropertyName == customField.PropertyName)
                                               ?.Value ?? "");
                                }

                                addRow(submittedData.DateSubmitted.ToString("d"));
                                addRow(submittedData.ClubComment);
                                addRow(submittedData.IsActive ? Messages.Yes : Messages.No);

                                rowCount++;
                            }

                            break;

                        case ActivityFormType.DepartmentClubCustomPersonal:

                            #region Columns

                            if (!activity.NoTeamRegistration)
                            {
                                addColumn(Messages.Team);
                                addColumn(Messages.SchoolName);
                            }

                            addColumn(Messages.TeamPlayers_ProfilePicture);
                            addColumn(Messages.IdentNum);
                            addColumn(Messages.FirstName);
                            addColumn(Messages.LastName);
                            addColumn(Messages.MiddleName);
                            addColumn(Messages.Email);
                            addColumn(Messages.Phone);
                            addColumn(Messages.Address);
                            addColumn(Messages.BirthDay);
                            addColumn(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice);
                            addColumn(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice_Paid);
                            addColumn(Messages.TeamDetails_ParticipationPrice);
                            addColumn(Messages.TeamDetails_ParticipationPrice_Paid);
                            addColumn(Messages.TeamDetails_PlayerInsurancePrice);
                            addColumn(Messages.Activity_BuildForm_InsurancePaid);
                            addColumn(Messages.Activity_BuildForm_ApproveMedicalCert);
                            addColumn(Messages.Activity_BuildForm_Comments);

                            if (data.Any(x => x.CardComInvoiceNumber != null))
                            {
                                addColumn(Messages.Activity_Status_CardComInvoiceNumber);
                            }
                            if (data.Any(x => x.CardComNumberOfPayments != null))
                            {
                                addColumn(Messages.Activity_Status_CardComNumberOfPayments);
                            }

                            foreach (var customField in activity.ActivityForms.First()
                                .ActivityFormsDetails
                                .Where(x => x.Type.StartsWith("Custom") &&
                                            x.Type != ActivityFormControlType.CustomText.ToString() &&
                                            x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                            x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                            !x.IsDisabled))
                            {
                                addColumn(!IsHebrew
                                    ? customField.LabelTextEn
                                    : customField.LabelTextHeb);
                            }

                            addColumn(Messages.Activity_BuildForm_DateSubmitted);
                            addColumn(Messages.Activity_BuildForm_ClubComment);
                            addColumn(Messages.Activity_BuildForm_RegistrationStatusActive);

                            #endregion

                            foreach (var submittedData in data.OrderByDescending(x => x.DateSubmitted))
                            {
                                columnCount = 1;
                                var submittedCustomFields = new List<ActivityFormCustomField>();
                                if (submittedData.CustomFields != null)
                                {
                                    submittedCustomFields =
                                        JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(submittedData
                                            .CustomFields);
                                }

                                if (!activity.NoTeamRegistration)
                                {
                                    addRow(submittedData.Team.TeamsDetails
                                               .FirstOrDefault(x => x.SeasonId == activity.SeasonId)
                                               ?.TeamName ?? submittedData.Team.Title);
                                    addRow(submittedData.Team?.SchoolTeams
                                        ?.FirstOrDefault(x => x.School.SeasonId == activity.SeasonId)
                                        ?.School?.Name);
                                }

                                addRow(!string.IsNullOrWhiteSpace(submittedData.User.Image) ||
                                       submittedData.User.PlayerFiles.Any(
                                           x => x.SeasonId == activity.SeasonId &&
                                                x.FileType == (int) PlayerFileType.PlayerImage)
                                    ? Messages.Yes
                                    : Messages.No);
                                addRow(submittedData.User.IdentNum);
                                addRow(submittedData.User.FirstName);
                                addRow(submittedData.User.LastName);
                                addRow(submittedData.User.MiddleName);
                                addRow(submittedData.User.Email);
                                addRow(submittedData.User.Telephone);
                                addRow(submittedData.User.City);
                                addRow(submittedData.User.BirthDay?.ToString("d"));
                                addRow(submittedData.RegistrationPrice.ToString());
                                addRow(submittedData.RegistrationPaid.ToString());
                                addRow(submittedData.ParticipationPrice.ToString());
                                addRow(submittedData.ParticipationPaid.ToString());
                                addRow(submittedData.InsurancePrice.ToString());
                                addRow(submittedData.InsurancePaid.ToString());
                                addRow(submittedData.User.MedicalCertApprovements
                                           .FirstOrDefault(c => c.SeasonId == activity.SeasonId)?.Approved == true
                                    ? Messages.Yes
                                    : Messages.No);
                                addRow(submittedData.Comments);

                                if (data.Any(x => x.CardComInvoiceNumber != null))
                                {
                                    addRow(submittedData.CardComInvoiceNumber?.ToString());
                                }
                                if (data.Any(x => x.CardComNumberOfPayments != null))
                                {
                                    addRow(submittedData.CardComNumberOfPayments?.ToString());
                                }

                                foreach (var customField in activity.ActivityForms.First()
                                    .ActivityFormsDetails
                                    .Where(x => x.Type.StartsWith("Custom") &&
                                                x.Type != ActivityFormControlType.CustomText.ToString() &&
                                                x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                                x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                                !x.IsDisabled))
                                {
                                    addRow(submittedCustomFields
                                               .FirstOrDefault(
                                                   x => x.PropertyName == customField.PropertyName)
                                               ?.Value ?? "");
                                }

                                addRow(submittedData.DateSubmitted.ToString("d"));
                                addRow(submittedData.ClubComment);
                                addRow(submittedData.IsActive ? Messages.Yes : Messages.No);

                                rowCount++;
                            }

                            break;

                        case ActivityFormType.UnionClub:

                            #region Columns

                            addColumn(Messages.Club);
                            addColumn(Messages.Club_ClubNumberOfCourts);
                            addColumn(Messages.ClubNGONumber);
                            addColumn(Messages.ClubNameSportsCenter);
                            addColumn(Messages.ClubAddress);
                            addColumn(Messages.ClubPhone);
                            addColumn(Messages.ClubEmail);
                            addColumn(Messages.IdentNum);
                            addColumn(Messages.FirstName);
                            addColumn(Messages.LastName);
                            addColumn(Messages.MiddleName);
                            addColumn(Messages.Email);
                            addColumn(Messages.Phone);
                            addColumn(Messages.Address);
                            addColumn(Messages.BirthDay);
                            addColumn(Messages.Activity_BuildForm_Comments);
                            if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                            {
                                foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                {
                                    addColumn(!IsHebrew
                                        ? activityCustomPrice.TitleEng
                                        : activityCustomPrice.TitleHeb);
                                    addColumn(
                                        $"{(!IsHebrew ? activityCustomPrice.TitleEng : activityCustomPrice.TitleHeb)} {Messages.Paid}");
                                }
                            }

                            addColumn(Messages.Activity_BuildForm_RemainForPayment);

                            addColumn(Messages.Activity_BuildForm_ByBenefactor);
                            addColumn(Messages.Activity_BuildForm_AmountPayedByBenefactor);

                            if (data.Any(x => x.CardComInvoiceNumber != null))
                            {
                                addColumn(Messages.Activity_Status_CardComInvoiceNumber);
                            }
                            if (data.Any(x => x.CardComNumberOfPayments != null))
                            {
                                addColumn(Messages.Activity_Status_CardComNumberOfPayments);
                            }

                            foreach (var customField in activity.ActivityForms.First()
                                .ActivityFormsDetails
                                .Where(x => x.Type.StartsWith("Custom") &&
                                            x.Type != ActivityFormControlType.CustomText.ToString() &&
                                            x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                            x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                            !x.IsDisabled))
                            {
                                addColumn(!IsHebrew
                                    ? customField.LabelTextEn
                                    : customField.LabelTextHeb);
                            }

                            addColumn(Messages.Activity_BuildForm_DateSubmitted);
                            addColumn(Messages.Activity_BuildForm_UnionComment);
                            addColumn(Messages.Activity_BuildForm_RegistrationStatusActive);

                            #endregion

                            foreach (var submittedData in data.OrderByDescending(x => x.DateSubmitted))
                            {
                                columnCount = 1;
                                var submittedCustomFields = new List<ActivityFormCustomField>();
                                if (submittedData.CustomFields != null)
                                {
                                    submittedCustomFields =
                                        JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(submittedData
                                            .CustomFields);
                                }

                                var submittedCustomPrices = new List<ActivityCustomPriceModel>();
                                if (submittedData.CustomPrices != null)
                                {
                                    submittedCustomPrices =
                                        JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(submittedData
                                            .CustomPrices);
                                }

                                addRow(submittedData.Club.Name);
                                addRow(submittedData.Club.NumberOfCourts.ToString());
                                addRow(submittedData.Club.NGO_Number?.ToString() ?? string.Empty);
                                addRow(!IsHebrew
                                    ? submittedData.Club.SportCenter?.Eng
                                    : submittedData.Club.SportCenter?.Heb);
                                addRow(submittedData.Club.Address);
                                addRow(submittedData.Club.ContactPhone);
                                addRow(submittedData.Club.Email);
                                addRow(submittedData.User.IdentNum);
                                addRow(submittedData.User.FirstName);
                                addRow(submittedData.User.LastName);
                                addRow(submittedData.User.MiddleName);
                                addRow(submittedData.User.Email);
                                addRow(submittedData.User.Telephone);
                                addRow(submittedData.User.City);
                                addRow(submittedData.User.BirthDay?.ToString("d"));
                                addRow(submittedData.Comments);
                                if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                                {
                                    foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                    {
                                        var regCustomPrice =
                                            submittedCustomPrices.FirstOrDefault(
                                                x => x.PropertyName == $"customPrice-{activityCustomPrice.Id}");
                                        addRow(regCustomPrice?.TotalPrice.ToString());
                                        addRow(regCustomPrice?.Paid.ToString());
                                    }
                                }

                                addRow((
                                    (submittedData.Activity.CustomPricesEnabled
                                        ? submittedCustomPrices.Sum(x => x.TotalPrice)
                                        : 0)
                                    - (
                                        (submittedData.Activity.CustomPricesEnabled
                                            ? submittedCustomPrices.Sum(x => x.Paid)
                                            : 0)
                                    )).ToString());

                                addRow(submittedData.PaymentByBenefactor);
                                addRow(submittedData.IsPaymentByBenefactor ? submittedData.Paid.ToString() : "");

                                if (data.Any(x => x.CardComInvoiceNumber != null))
                                {
                                    addRow(submittedData.CardComInvoiceNumber?.ToString());
                                }
                                if (data.Any(x => x.CardComNumberOfPayments != null))
                                {
                                    addRow(submittedData.CardComNumberOfPayments?.ToString());
                                }

                                foreach (var customField in activity.ActivityForms.First()
                                    .ActivityFormsDetails
                                    .Where(x => x.Type.StartsWith("Custom") &&
                                                x.Type != ActivityFormControlType.CustomText.ToString() &&
                                                x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                                x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                                !x.IsDisabled))
                                {
                                    addRow(submittedCustomFields
                                               .FirstOrDefault(
                                                   x => x.PropertyName == customField.PropertyName)
                                               ?.Value ?? "");
                                }

                                addRow(submittedData.DateSubmitted.ToString("d"));
                                addRow(submittedData.UnionComment);
                                addRow(submittedData.IsActive ? Messages.Yes : Messages.No);

                                rowCount++;
                            }

                            break;

                        case ActivityFormType.UnionPlayerToClub:

                            #region Columns

                            addColumn(Messages.Club);
                            addColumn(Messages.Team);
                            addColumn(Messages.IdentNum);
                            addColumn(Messages.PassportNum);
                            addColumn(Messages.FirstName);
                            addColumn(Messages.LastName);
                            addColumn(Messages.MiddleName);
                            addColumn(Messages.ForeignFirstName);
                            addColumn(Messages.ForeignLastName);
                            addColumn(Messages.Gender);
                            addColumn(Messages.BirthDay);
                            addColumn(Messages.City);
                            addColumn(Messages.Address);
                            addColumn(Messages.PostalCode);
                            addColumn(Messages.Email);
                            addColumn(Messages.Phone);

                            addColumn(Messages.FatherName);
                            addColumn(Messages.MotherName);
                            addColumn(Messages.ParentPhone);
                            addColumn(Messages.ParentEmail);

                            addColumn(Messages.MedExamDate);
                            addColumn(Messages.DateOfInsuranceValidity);

                            addColumn(Messages.Player_CompetitiveMember);

                            addColumn(Messages.Activity_BuildForm_UnionPlayerToClub_RegistrationPrice);
                            addColumn(Messages.Activity_BuildForm_UnionPlayerToClub_RegistrationPaid);

                            addColumn(Messages.Activity_BuildForm_InsurancePrice);
                            addColumn(Messages.Activity_BuildForm_InsurancePaid);

                            addColumn(Messages.Activity_SchoolInsurance);

                            if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                            {
                                foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                {
                                    addColumn(!IsHebrew
                                        ? activityCustomPrice.TitleEng
                                        : activityCustomPrice.TitleHeb);
                                    addColumn(
                                        $"{(!IsHebrew ? activityCustomPrice.TitleEng : activityCustomPrice.TitleHeb)} {Messages.Paid}");
                                }
                            }

                            addColumn(Messages.Activity_BuildForm_RemainForPayment);

                            addColumn(Messages.Activity_BuildForm_ByBenefactor);
                            addColumn(Messages.Activity_BuildForm_AmountPayedByBenefactor);

                            addColumn(Messages.Activity_BuildForm_ApproveMedicalCert);
                            addColumn(Messages.Activity_BuildForm_Comments);

                            if (data.Any(x => x.CardComInvoiceNumber != null))
                            {
                                addColumn(Messages.Activity_Status_CardComInvoiceNumber);
                            }
                            if (data.Any(x => x.CardComNumberOfPayments != null))
                            {
                                addColumn(Messages.Activity_Status_CardComNumberOfPayments);
                            }

                            foreach (var customField in activity.ActivityForms.First()
                                .ActivityFormsDetails
                                .Where(x => x.Type.StartsWith("Custom") &&
                                            x.Type != ActivityFormControlType.CustomText.ToString() &&
                                            x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                            x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                            !x.IsDisabled))
                            {
                                addColumn(!IsHebrew
                                    ? customField.LabelTextEn
                                    : customField.LabelTextHeb);
                            }

                            addColumn(Messages.Activity_BuildForm_DateSubmitted);
                            addColumn(Messages.Activity_BuildForm_UnionComment);
                            addColumn(Messages.Activity_BuildForm_RegistrationStatusActive);

                            #endregion

                            foreach (var submittedData in data.OrderByDescending(x => x.DateSubmitted))
                            {
                                columnCount = 1;
                                var submittedCustomFields = new List<ActivityFormCustomField>();
                                if (submittedData.CustomFields != null)
                                {
                                    submittedCustomFields =
                                        JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(submittedData
                                            .CustomFields);
                                }

                                var submittedCustomPrices = new List<ActivityCustomPriceModel>();
                                if (submittedData.CustomPrices != null)
                                {
                                    submittedCustomPrices =
                                        JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(submittedData
                                            .CustomPrices);
                                }

                                addRow(submittedData.Club?.Name);
                                addRow(submittedData.Team?.TeamsDetails
                                           .FirstOrDefault(x => x.SeasonId == activity.SeasonId)
                                           ?.TeamName ?? submittedData.Team?.Title);

                                addRow(submittedData.User.IdentNum);
                                addRow(submittedData.User.PassportNum);
                                addRow(submittedData.User.FirstName);
                                addRow(submittedData.User.LastName);
                                addRow(submittedData.User.MiddleName);
                                addRow(submittedData.User.ForeignFirstName);
                                addRow(submittedData.User.ForeignLastName);
                                addRow(submittedData.User.GenderId == 0 ? Messages.Female :
                                    submittedData.User.GenderId == 1 ? Messages.Male : string.Empty);
                                addRow(submittedData.User.BirthDay?.ToString("d"));
                                addRow(submittedData.User.City);
                                addRow(submittedData.User.Address);
                                addRow(submittedData.User.PostalCode);
                                addRow(submittedData.User.Email);
                                addRow(submittedData.User.Telephone);

                                addRow(submittedData.User.ParentName);
                                addRow(submittedData.User.MotherName);
                                addRow(submittedData.User.ParentPhone);
                                addRow(submittedData.User.ParentEmail);

                                addRow(submittedData.User.MedExamDate?.ToString("d"));
                                addRow(submittedData.User.DateOfInsurance?.ToString("d"));

                                addRow(submittedData.User.IsCompetitiveMember ? Messages.Yes : Messages.No);

                                addRow(submittedData.RegistrationPrice.ToString());
                                addRow(submittedData.RegistrationPaid.ToString());

                                addRow(submittedData.InsurancePrice.ToString());
                                addRow(submittedData.InsurancePaid.ToString());
                                addRow(submittedData.IsSchoolInsurance ? Messages.Yes : Messages.No);

                                if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                                {
                                    foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                    {
                                        var regCustomPrice =
                                            submittedCustomPrices.FirstOrDefault(
                                                x => x.PropertyName == $"customPrice-{activityCustomPrice.Id}");
                                        addRow(regCustomPrice?.TotalPrice.ToString());
                                        addRow(regCustomPrice?.Paid.ToString());
                                    }
                                }

                                addRow((
                                    (submittedData.RegistrationPrice +
                                     submittedData.InsurancePrice +
                                     (submittedData.Activity.CustomPricesEnabled
                                         ? submittedCustomPrices.Sum(x => x.TotalPrice)
                                         : 0))
                                    - (
                                        submittedData.RegistrationPaid +
                                        submittedData.InsurancePaid +
                                        (submittedData.Activity.CustomPricesEnabled
                                            ? submittedCustomPrices.Sum(x => x.Paid)
                                            : 0)
                                    )).ToString());

                                addRow(submittedData.PaymentByBenefactor);
                                addRow(submittedData.IsPaymentByBenefactor ? submittedData.Paid.ToString() : "");

                                addRow(submittedData.User.MedicalCertApprovements
                                           .FirstOrDefault(c => c.SeasonId == activity.SeasonId)?.Approved == true
                                    ? Messages.Yes
                                    : Messages.No);
                                addRow(submittedData.Comments);

                                if (data.Any(x => x.CardComInvoiceNumber != null))
                                {
                                    addRow(submittedData.CardComInvoiceNumber?.ToString());
                                }
                                if (data.Any(x => x.CardComNumberOfPayments != null))
                                {
                                    addRow(submittedData?.CardComNumberOfPayments?.ToString());
                                }

                                foreach (var customField in activity.ActivityForms.First()
                                    .ActivityFormsDetails
                                    .Where(x => x.Type.StartsWith("Custom") &&
                                                x.Type != ActivityFormControlType.CustomText.ToString() &&
                                                x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                                x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                                !x.IsDisabled))
                                {
                                    addRow(submittedCustomFields
                                               .FirstOrDefault(x => x.PropertyName == customField.PropertyName)
                                               ?.Value ?? "");
                                }

                                addRow(submittedData.DateSubmitted.ToString("d"));
                                addRow(submittedData.UnionComment);
                                addRow(submittedData.IsActive ? Messages.Yes : Messages.No);

                                rowCount++;
                            }

                            break;
                    }

                    ws.Columns(1, 23).AdjustToContents();

                    var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ExportActivityRegistrationStatus-" + DateTime.Now.ToLongDateString() + ".xlsx");
                }
            }

            return null;
        }

        [NonAction]
        private int GetMonthsDiff(DateTime start, DateTime end)
        {
            if (start > end)
                return GetMonthsDiff(end, start);

            int months = 0;
            do
            {
                start = start.AddMonths(1);
                if (start > end)
                    return months;

                months++;
            }
            while (true);
        }




        [HttpPost]
        public ActionResult ExportExcelMonthlyPayments(int id)
        {
            var activity = activityRepo.GetById(id);
            var formType = activity.GetFormType();

            if (activity != null)
            {
                var data = db.ActivityFormsSubmittedDatas
                    .Where(x => x.ActivityId == id)
                    .Include(x => x.User)
                    .Include(x => x.User.PlayerFiles)
                    .Include(x => x.User.MedicalCertApprovements)
                    .ToList();

                using (var workbook =
                    new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
                {
                    var ws = workbook.Worksheets.Add(activity.Name);

                    var columnCount = 1;
                    var rowCount = 2;

                    Action<string> addColumn = name =>
                    {
                        ws.Cell(1, columnCount).Value = name;

                        columnCount++;
                    };
                    Action<string> addRow = value =>
                    {
                        ws.Cell(rowCount, columnCount).SetValue(value);

                        columnCount++;
                    };
                    Action<string, XLDataType> addRowWithType = (value, type) =>
                    {
                        ws.Cell(rowCount, columnCount).SetValue(value);
                        ws.Cell(rowCount, columnCount).SetDataType(type);

                        columnCount++;
                    };
                    //var months = new List<int>();
                    var monthCalculator = activity.StartDate.Value;

                    var startMonth = activity.StartDate.Value.Month;
                    var monthsCount = GetMonthsDiff(activity.StartDate.Value, activity.EndDate.Value);

                    switch (formType)
                    {
                        
                        case ActivityFormType.ClubPlayerRegistration:

                            #region Columns

                            var clubPlayerRegColumnCount = 1;
                            Action<string> addClubPlayerRegColumn = name =>
                            {
                                ws.Cell(1, clubPlayerRegColumnCount).Value = name;

                                clubPlayerRegColumnCount++;
                            };



                            addClubPlayerRegColumn(Messages.Team);
                            addClubPlayerRegColumn(Messages.SchoolName);
                            addClubPlayerRegColumn(Messages.TeamPlayers_ProfilePicture);
                            addClubPlayerRegColumn(Messages.IdentNum);
                            addClubPlayerRegColumn(Messages.BrotherIdNum);
                            addClubPlayerRegColumn(Messages.FirstName);
                            addClubPlayerRegColumn(Messages.LastName);
                            addClubPlayerRegColumn(Messages.MiddleName);
                            addClubPlayerRegColumn(Messages.Email);
                            addClubPlayerRegColumn(Messages.Phone);
                            addClubPlayerRegColumn(Messages.Address);
                            addClubPlayerRegColumn(Messages.BirthDay);
                            if (activity.AllowToEnterDateToAdjustPrices)
                            {
                                addClubPlayerRegColumn(Messages.Activity_BuildForm_StartDateToAdjustPrices);
                            }
                            addClubPlayerRegColumn(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice);
                            addClubPlayerRegColumn(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice_Paid);
                            addClubPlayerRegColumn(Messages.TeamDetails_ParticipationPrice);
                            addClubPlayerRegColumn(Messages.TeamDetails_ParticipationPrice_Paid);
                            for (int i = 0; i < monthsCount; i++)
                            {
                                addClubPlayerRegColumn((((startMonth + i)%12)+1).ToString());
                            }
                            addClubPlayerRegColumn(Messages.Security);


                            addClubPlayerRegColumn(Messages.TeamDetails_PlayerInsurancePrice);
                            addClubPlayerRegColumn(Messages.Activity_BuildForm_InsurancePaid);
                            if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                            {
                                foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                {
                                    addClubPlayerRegColumn(!IsHebrew
                                        ? activityCustomPrice.TitleEng
                                        : activityCustomPrice.TitleHeb);
                                    addClubPlayerRegColumn(
                                        $"{(!IsHebrew ? activityCustomPrice.TitleEng : activityCustomPrice.TitleHeb)} {Messages.Paid}");
                                }
                            }

                            addClubPlayerRegColumn(Messages.Activity_BuildForm_RemainForPayment);
                            addClubPlayerRegColumn(Messages.Activity_BuildForm_ApproveMedicalCert);
                            addClubPlayerRegColumn(Messages.Activity_BuildForm_Comments);

                            if (data.Any(x => x.CardComInvoiceNumber != null))
                            {
                                addClubPlayerRegColumn(Messages.Activity_Status_CardComInvoiceNumber);
                            }
                            if (data.Any(x => x.CardComNumberOfPayments != null))
                            {
                                addClubPlayerRegColumn(Messages.Activity_Status_CardComNumberOfPayments);
                            }

                            foreach (var customField in activity.ActivityForms.First()
                                .ActivityFormsDetails
                                .Where(x => x.Type.StartsWith("Custom") &&
                                            x.Type != ActivityFormControlType.CustomText.ToString() &&
                                            x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                            x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                            !x.IsDisabled))
                            {
                                addClubPlayerRegColumn(!IsHebrew
                                    ? customField.LabelTextEn
                                    : customField.LabelTextHeb);
                            }

                            addClubPlayerRegColumn(Messages.Activity_BuildForm_DateSubmitted);
                            addClubPlayerRegColumn(Messages.Activity_BuildForm_ClubComment);
                            addClubPlayerRegColumn(Messages.Activity_BuildForm_RegistrationStatusActive);

                            #endregion

                            var addClubPlayerRegRowCount = 2;
                            Action<string, XLDataType> addClubPlayerRegRow = (value, type) =>
                            {
                                ws.Cell(addClubPlayerRegRowCount, clubPlayerRegColumnCount).SetValue(value);
                                ws.Cell(addClubPlayerRegRowCount, clubPlayerRegColumnCount).SetDataType(type);

                                clubPlayerRegColumnCount++;
                            };

                            foreach (var submittedData in data.OrderByDescending(x => x.DateSubmitted))
                            {
                                clubPlayerRegColumnCount = 1;
                                var submittedCustomFields = new List<ActivityFormCustomField>();
                                if (submittedData.CustomFields != null)
                                {
                                    submittedCustomFields =
                                        JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(submittedData
                                            .CustomFields);
                                }

                                var submittedCustomPrices = new List<ActivityCustomPriceModel>();
                                if (submittedData.CustomPrices != null)
                                {
                                    submittedCustomPrices =
                                        JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(submittedData
                                            .CustomPrices);
                                }

                                addClubPlayerRegRow(submittedData.Team.TeamsDetails
                                                        .FirstOrDefault(x => x.SeasonId == activity.SeasonId)
                                                        ?.TeamName ?? submittedData.Team.Title, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.Team?.SchoolTeams
                                    ?.FirstOrDefault(x => x.School.SeasonId == activity.SeasonId)
                                    ?.School?.Name, XLDataType.Text);
                                addClubPlayerRegRow(!string.IsNullOrWhiteSpace(submittedData.User.Image) ||
                                                    submittedData.User.PlayerFiles.Any(
                                                        x => x.SeasonId == activity.SeasonId &&
                                                             x.FileType == (int)PlayerFileType.PlayerImage)
                                    ? Messages.Yes
                                    : Messages.No, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.IdentNum, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User1?.IdentNum, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.FirstName, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.LastName, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.MiddleName, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.Email, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.Telephone, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.City, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.User.BirthDay?.ToString("d"), XLDataType.DateTime);
                                if (activity.AllowToEnterDateToAdjustPrices)
                                {
                                    addClubPlayerRegRow(submittedData.PlayerStartDate?.ToString("d"), XLDataType.DateTime);
                                }
                                addClubPlayerRegRow(submittedData.RegistrationPrice.ToString(), XLDataType.Number);
                                addClubPlayerRegRow(submittedData.RegistrationPaid.ToString(), XLDataType.Number);
                                addClubPlayerRegRow(submittedData.ParticipationPrice.ToString(), XLDataType.Number);
                                addClubPlayerRegRow(submittedData.ParticipationPaid.ToString(), XLDataType.Number);
                                var playerStartDate = submittedData.PlayerStartDate.HasValue ? submittedData.PlayerStartDate.Value.Date : activity.StartDate.Value.Date;
                                var daysParticipated = (long)(activity.EndDate.Value.Date.Ticks - playerStartDate.Ticks) / (60 * 60 * 24 * (long)10000000);
                                var lastMonthPaid = activity.EndDate.Value.Date;
                                var monthsPayments = new List<decimal>();
                                var teamParticipationFee = submittedData.Team.ClubTeamPrices.FirstOrDefault(p => p.PriceType == (int)ClubTeamPriceType.ParticipationPrice)?.Price ?? 0;
                                var fullMonthlyFee = teamParticipationFee > 0 ? teamParticipationFee / (monthsCount + 1) : 0;
                                for (int i = 0; i <= monthsCount; i++)
                                {
                                    int totalMonthDays = DateTime.DaysInMonth(lastMonthPaid.Year, lastMonthPaid.Month);
                                    var lastDayOfCurrentMonth = new DateTime(lastMonthPaid.Year, lastMonthPaid.Month, totalMonthDays, 23, 59, 59);
                                    if (lastDayOfCurrentMonth.Ticks - playerStartDate.Ticks > totalMonthDays * 60 * 60 * 24 * (long)10000000)
                                    {
                                        monthsPayments.Add(fullMonthlyFee);
                                    }
                                    else if (lastDayOfCurrentMonth.Ticks - playerStartDate.Ticks > 0)
                                    {
                                        var monthCost = submittedData.ParticipationPaid - (fullMonthlyFee * monthsPayments.Count());
                                        monthsPayments.Add(monthCost);
                                    }
                                    else
                                    {
                                        monthsPayments.Add(0);
                                    }
                                    lastMonthPaid = lastMonthPaid.AddMonths(-1);
                                }
                                monthsPayments.Reverse();
                                var securityPay = submittedData.Team.MonthlyProvisionForSecurity ?? 0;
                                decimal currentSecurityPayAccomulator = 0;
                                foreach (var payment in monthsPayments)
                                {
                                    var calculated = payment - securityPay;
                                    if (calculated < 0)
                                    {
                                        if (payment >= 0)
                                        {
                                            calculated = 0;
                                            currentSecurityPayAccomulator = currentSecurityPayAccomulator + payment;
                                            addClubPlayerRegRow(Math.Round(payment, 2).ToString(), XLDataType.Number);
                                        }
                                        else
                                        {
                                            addClubPlayerRegRow(Math.Round(calculated, 2).ToString(), XLDataType.Number);
                                        }
                                    }
                                    else
                                    {
                                        currentSecurityPayAccomulator = currentSecurityPayAccomulator + securityPay;
                                        addClubPlayerRegRow(Math.Round(calculated, 2).ToString(), XLDataType.Number);
                                    }
                                }

                                addClubPlayerRegRow(Math.Round(currentSecurityPayAccomulator, 2).ToString(), XLDataType.Number);

                                addClubPlayerRegRow(submittedData.InsurancePrice.ToString(), XLDataType.Number);
                                addClubPlayerRegRow(submittedData.InsurancePaid.ToString(), XLDataType.Number);
                                if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                                {
                                    foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                    {
                                        var regCustomPrice =
                                            submittedCustomPrices.FirstOrDefault(
                                                x => x.PropertyName == $"customPrice-{activityCustomPrice.Id}");
                                        addClubPlayerRegRow(regCustomPrice?.TotalPrice.ToString(), XLDataType.Number);
                                        addClubPlayerRegRow(regCustomPrice?.Paid.ToString(), XLDataType.Number);
                                    }
                                }

                                addClubPlayerRegRow(
                                    ((submittedData.RegistrationPrice +
                                      submittedData.ParticipationPrice +
                                      submittedData.InsurancePrice +
                                      submittedCustomPrices.Sum(x => x.TotalPrice))
                                     -
                                     (submittedData.RegistrationPaid +
                                      submittedData.ParticipationPaid +
                                      submittedData.InsurancePaid +
                                      submittedCustomPrices.Sum(x => x.Paid)))
                                    .ToString(), XLDataType.Number);
                                addClubPlayerRegRow(
                                    submittedData.User.MedicalCertApprovements
                                        .FirstOrDefault(c => c.SeasonId == activity.SeasonId)?.Approved == true
                                        ? Messages.Yes
                                        : Messages.No, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.Comments, XLDataType.Text);

                                if (data.Any(x => x.CardComInvoiceNumber != null))
                                {
                                    addClubPlayerRegRow(submittedData.CardComInvoiceNumber?.ToString(), XLDataType.Text);
                                }
                                if (data.Any(x => x.CardComNumberOfPayments != null))
                                {
                                    addClubPlayerRegRow(submittedData.CardComNumberOfPayments?.ToString(), XLDataType.Text);
                                }

                                foreach (var customField in activity.ActivityForms.First()
                                    .ActivityFormsDetails
                                    .Where(x => x.Type.StartsWith("Custom") &&
                                                x.Type != ActivityFormControlType.CustomText.ToString() &&
                                                x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                                x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                                !x.IsDisabled))
                                {
                                    addClubPlayerRegRow(submittedCustomFields
                                                            .FirstOrDefault(
                                                                x => x.PropertyName == customField.PropertyName)
                                                            ?.Value ?? "", XLDataType.Text);
                                }

                                addClubPlayerRegRow(submittedData.DateSubmitted.ToString("d"), XLDataType.DateTime);
                                addClubPlayerRegRow(submittedData.ClubComment, XLDataType.Text);
                                addClubPlayerRegRow(submittedData.IsActive ? Messages.Yes : Messages.No,
                                    XLDataType.Text);

                                addClubPlayerRegRowCount++;
                            }

                            break;

                        case ActivityFormType.ClubCustomPersonal:

                            #region Columns

                            if (!activity.NoTeamRegistration)
                            {
                                addColumn(Messages.Team);
                                addColumn(Messages.SchoolName);
                            }

                            addColumn(Messages.TeamPlayers_ProfilePicture);
                            addColumn(Messages.IdentNum);
                            addColumn(Messages.BrotherIdNum);
                            addColumn(Messages.FirstName);
                            addColumn(Messages.LastName);
                            addColumn(Messages.MiddleName);
                            addColumn(Messages.Email);
                            addColumn(Messages.Phone);
                            addColumn(Messages.Address);
                            addColumn(Messages.BirthDay);
                            if (activity.AllowToEnterDateToAdjustPrices)
                            {
                                addColumn(Messages.Activity_BuildForm_StartDateToAdjustPrices);
                            }
                            addColumn(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice);
                            addColumn(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice_Paid);
                            addColumn(Messages.TeamDetails_ParticipationPrice);
                            addColumn(Messages.TeamDetails_ParticipationPrice_Paid);
                            for (int i = 0; i <= monthsCount; i++)
                            {
                                int monthNum = startMonth-1 + i;
                                if (monthNum > 12)
                                {
                                    monthNum = monthNum - 12;
                                }
                                addColumn(((monthNum%12)+1).ToString());
                            }
                            addColumn(Messages.Security);
                            addColumn(Messages.TeamDetails_PlayerInsurancePrice);
                            addColumn(Messages.Activity_BuildForm_InsurancePaid);
                            if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                            {
                                foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                {
                                    addColumn(!IsHebrew
                                        ? activityCustomPrice.TitleEng
                                        : activityCustomPrice.TitleHeb);
                                    addColumn(
                                        $"{(!IsHebrew ? activityCustomPrice.TitleEng : activityCustomPrice.TitleHeb)} {Messages.Paid}");
                                }
                            }

                            addColumn(Messages.Activity_BuildForm_RemainForPayment);
                            addColumn(Messages.Activity_BuildForm_ApproveMedicalCert);
                            addColumn(Messages.Activity_BuildForm_Comments);

                            if (data.Any(x => x.CardComInvoiceNumber != null))
                            {
                                addColumn(Messages.Activity_Status_CardComInvoiceNumber);
                            }
                            if (data.Any(x => x.CardComNumberOfPayments != null))
                            {
                                addColumn(Messages.Activity_Status_CardComNumberOfPayments);
                            }

                            foreach (var customField in activity.ActivityForms.First()
                                .ActivityFormsDetails
                                .Where(x => x.Type.StartsWith("Custom") &&
                                            x.Type != ActivityFormControlType.CustomText.ToString() &&
                                            x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                            x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                            !x.IsDisabled))
                            {
                                addColumn(!IsHebrew
                                    ? customField.LabelTextEn
                                    : customField.LabelTextHeb);
                            }

                            addColumn(Messages.Activity_BuildForm_DateSubmitted);
                            addColumn(Messages.Activity_BuildForm_ClubComment);
                            addColumn(Messages.Activity_BuildForm_RegistrationStatusActive);

                            #endregion

                            foreach (var submittedData in data.OrderByDescending(x => x.DateSubmitted))
                            {
                                columnCount = 1;
                                var submittedCustomFields = new List<ActivityFormCustomField>();
                                if (submittedData.CustomFields != null)
                                {
                                    submittedCustomFields =
                                        JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(submittedData
                                            .CustomFields);
                                }

                                var submittedCustomPrices = new List<ActivityCustomPriceModel>();
                                if (submittedData.CustomPrices != null)
                                {
                                    submittedCustomPrices =
                                        JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(submittedData
                                            .CustomPrices);
                                }

                                if (!activity.NoTeamRegistration)
                                {
                                    addRowWithType(submittedData.Team.TeamsDetails
                                                       .FirstOrDefault(x => x.SeasonId == activity.SeasonId)
                                                       ?.TeamName ?? submittedData.Team.Title, XLDataType.Text);
                                    addRowWithType(submittedData.Team?.SchoolTeams
                                        ?.FirstOrDefault(x => x.School.SeasonId == activity.SeasonId)
                                        ?.School?.Name, XLDataType.Text);
                                }

                                addRowWithType(!string.IsNullOrWhiteSpace(submittedData.User.Image) ||
                                               submittedData.User.PlayerFiles.Any(
                                                   x => x.SeasonId == activity.SeasonId &&
                                                        x.FileType == (int)PlayerFileType.PlayerImage)
                                    ? Messages.Yes
                                    : Messages.No, XLDataType.Text);
                                addRowWithType(submittedData.User.IdentNum, XLDataType.Text);
                                addRowWithType(submittedData.User1?.IdentNum, XLDataType.Text);
                                addRowWithType(submittedData.User.FirstName, XLDataType.Text);
                                addRowWithType(submittedData.User.LastName, XLDataType.Text);
                                addRowWithType(submittedData.User.MiddleName, XLDataType.Text);
                                addRowWithType(submittedData.User.Email, XLDataType.Text);
                                addRowWithType(submittedData.User.Telephone, XLDataType.Text);
                                addRowWithType(submittedData.User.City, XLDataType.Text);
                                addRowWithType(submittedData.User.BirthDay?.ToString("d"), XLDataType.DateTime);
                                if (activity.AllowToEnterDateToAdjustPrices)
                                {
                                    addRowWithType(submittedData.PlayerStartDate?.ToString("d"), XLDataType.DateTime);
                                }
                                addRowWithType(submittedData.RegistrationPrice.ToString(), XLDataType.Number);
                                addRowWithType(submittedData.RegistrationPaid.ToString(), XLDataType.Number);
                                addRowWithType(submittedData.ParticipationPrice.ToString(), XLDataType.Number);
                                addRowWithType(submittedData.ParticipationPaid.ToString(), XLDataType.Number);

                                var playerStartDate = submittedData.PlayerStartDate.HasValue ? submittedData.PlayerStartDate.Value.Date : activity.StartDate.Value.Date;
                                var daysParticipated = (long)(activity.EndDate.Value.Date.Ticks - playerStartDate.Ticks) / (60 * 60 * 24 * (long)10000000);
                                //var dayCost = submittedData.ParticipationPrice / daysParticipated;
                                var lastMonthPaid = activity.EndDate.Value.Date;
                                var monthsPayments = new List<decimal>();
                                var teamParticipationFee = submittedData.Team.ClubTeamPrices.FirstOrDefault(p => p.PriceType == (int)ClubTeamPriceType.ParticipationPrice)?.Price ?? 0;
                                var fullMonthlyFee = teamParticipationFee > 0 ? teamParticipationFee / (monthsCount+1) : 0;
                                for (int i = 0; i <= monthsCount; i++)
                                {
                                    int totalMonthDays = DateTime.DaysInMonth(lastMonthPaid.Year, lastMonthPaid.Month);
                                    var lastDayOfCurrentMonth = new DateTime(lastMonthPaid.Year, lastMonthPaid.Month, totalMonthDays,23,59,59);
                                    if (lastDayOfCurrentMonth.Ticks - playerStartDate.Ticks >= (totalMonthDays-1) * 60 * 60 * 24 * (long)10000000)
                                    {
                                        monthsPayments.Add(fullMonthlyFee);
                                    }
                                    else if (lastDayOfCurrentMonth.Ticks - playerStartDate.Ticks > 0)
                                    {
                                        var monthCost = submittedData.ParticipationPaid - (fullMonthlyFee * monthsPayments.Count());
                                        monthsPayments.Add(monthCost);
                                    }
                                    else
                                    {
                                        monthsPayments.Add(0);
                                    }
                                    lastMonthPaid = lastMonthPaid.AddMonths(-1);
                                }
                                monthsPayments.Reverse();
                                var securityPay = submittedData.Team.MonthlyProvisionForSecurity ?? 0;
                                decimal currentSecurityPayAccomulator = 0;
                                foreach (var payment in monthsPayments)
                                {
                                    var calculated = payment - securityPay;
                                    if (calculated < 0)
                                    {
                                        if (payment >= 0)
                                        {
                                            calculated = 0;
                                            currentSecurityPayAccomulator = currentSecurityPayAccomulator + payment;
                                            addRowWithType(Math.Round(payment, 2).ToString(), XLDataType.Number);
                                        }
                                        else
                                        {
                                            addRowWithType(Math.Round(calculated, 2).ToString(), XLDataType.Number);
                                        }
                                    }
                                    else 
                                    {
                                        currentSecurityPayAccomulator = currentSecurityPayAccomulator + securityPay;
                                        addRowWithType(Math.Round(calculated, 2).ToString(), XLDataType.Number);
                                    }

                                }

                                addRowWithType(Math.Round(currentSecurityPayAccomulator, 2).ToString(), XLDataType.Number);
                                addRowWithType(submittedData.InsurancePrice.ToString(), XLDataType.Number);
                                addRowWithType(submittedData.InsurancePaid.ToString(), XLDataType.Number);
                                if (activity.CustomPricesEnabled && activity.ActivityCustomPrices.Any())
                                {
                                    foreach (var activityCustomPrice in activity.ActivityCustomPrices.ToList())
                                    {
                                        var regCustomPrice =
                                            submittedCustomPrices.FirstOrDefault(
                                                x => x.PropertyName == $"customPrice-{activityCustomPrice.Id}");
                                        addRowWithType(regCustomPrice?.TotalPrice.ToString(), XLDataType.Number);
                                        addRowWithType(regCustomPrice?.Paid.ToString(), XLDataType.Number);
                                    }
                                }

                                addRowWithType(
                                    ((submittedData.RegistrationPrice +
                                      submittedData.ParticipationPrice +
                                      submittedData.InsurancePrice +
                                      submittedCustomPrices.Sum(x => x.TotalPrice))
                                     -
                                     (submittedData.RegistrationPaid +
                                      submittedData.ParticipationPaid +
                                      submittedData.InsurancePaid +
                                      submittedCustomPrices.Sum(x => x.Paid)))
                                    .ToString(), XLDataType.Number);
                                addRowWithType(submittedData.User.MedicalCertApprovements
                                                   .FirstOrDefault(c => c.SeasonId == activity.SeasonId)?.Approved ==
                                               true
                                    ? Messages.Yes
                                    : Messages.No, XLDataType.Text);
                                addRowWithType(submittedData.Comments, XLDataType.Text);

                                if (data.Any(x => x.CardComInvoiceNumber != null))
                                {
                                    addRowWithType(submittedData.CardComInvoiceNumber?.ToString(), XLDataType.Text);
                                }
                                if (data.Any(x => x.CardComNumberOfPayments != null))
                                {
                                    addRowWithType(submittedData.CardComNumberOfPayments?.ToString(), XLDataType.Text);
                                }

                                foreach (var customField in activity.ActivityForms.First()
                                    .ActivityFormsDetails
                                    .Where(x => x.Type.StartsWith("Custom") &&
                                                x.Type != ActivityFormControlType.CustomText.ToString() &&
                                                x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                                x.Type != ActivityFormControlType.CustomPrice.ToString() &&
                                                !x.IsDisabled))
                                {
                                    addRowWithType(submittedCustomFields
                                                       .FirstOrDefault(
                                                           x => x.PropertyName == customField.PropertyName)
                                                       ?.Value ?? "", XLDataType.Text);
                                }

                                addRowWithType(submittedData.DateSubmitted.ToString("d"), XLDataType.DateTime);
                                addRowWithType(submittedData.ClubComment, XLDataType.Text);
                                addRowWithType(submittedData.IsActive ? Messages.Yes : Messages.No, XLDataType.Text);

                                rowCount++;
                            }

                            break;
                    }

                    ws.Columns(1, 23).AdjustToContents();

                    var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ExportExcelMonthlyPayments-" + DateTime.Now.ToLongDateString() + ".xlsx");
                }
            }

            return null;
        }

        [HttpPost]
        public ActionResult Publish(int id, bool value)
        {
            var activity = activityRepo.GetById(id);

            if (activity == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "activity not found");
            }

            activity.IsPublished = value;

            activityRepo.Save();

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public void SaveStatusColumnsOrder(int id, int[] columns)
        {
            var activity = activityRepo.GetById(id);

            int userId;
            if (activity != null && int.TryParse(User.Identity.Name, out userId))
            {
                var existingSetting = activity.ActivityStatusColumnsOrders.FirstOrDefault(x => x.UserId == userId);
                if (existingSetting == null)
                {
                    activity.ActivityStatusColumnsOrders.Add(new ActivityStatusColumnsOrder
                    {
                        UserId = userId,
                        Columns = JsonConvert.SerializeObject(columns)
                    });
                }
                else
                {
                    existingSetting.Columns = JsonConvert.SerializeObject(columns);
                }

                activityRepo.Save();
            }
        }

        [HttpPost]
        public void EditStatusColumnName(int id, int pk, string value)
        {
            var activity = activityRepo.GetById(id);

            int userId;
            if (activity != null && int.TryParse(User.Identity.Name, out userId))
            {
                var culture = getCulture();

                var existingData =
                    activity.ActivityStatusColumnNames.FirstOrDefault(x =>
                        x.ColumnIndex == pk && x.Language == culture.ToString() && x.UserId == userId);
                if (existingData == null)
                {
                    activity.ActivityStatusColumnNames.Add(new ActivityStatusColumnName
                    {
                        ColumnIndex = pk,
                        ColumnName = value,
                        Language = culture.ToString(),
                        UserId = userId
                    });
                }
                else
                {
                    existingData.ColumnName = value;
                }

                activityRepo.Save();
            }
        }

        [HttpPost]
        public void ResetStatusColumnsNames(int id)
        {
            var activity = activityRepo.GetById(id);
            var culture = getCulture();

            int userId;
            if (activity != null && int.TryParse(User.Identity.Name, out userId))
            {
                db.ActivityStatusColumnNames.RemoveRange(
                    activity.ActivityStatusColumnNames.Where(
                        x => x.UserId == userId && x.Language == culture.ToString()));

                db.SaveChanges();
            }
        }

        [HttpPost]
        public void SaveStatusSorting(int id, 
            string value) //JSON
        {
            var activity = activityRepo.GetById(id);

            int userId;
            if (activity != null && int.TryParse(User.Identity.Name, out userId))
            {
                var existingData = activity.ActivityStatusColumnsSortings.FirstOrDefault(x => x.UserId == userId);
                if (existingData == null)
                {
                    activity.ActivityStatusColumnsSortings.Add(new ActivityStatusColumnsSorting
                    {
                        UserId = userId,
                        Sorting = value
                    });
                }
                else
                {
                    existingData.Sorting = value;
                }

                activityRepo.Save();
            }
        }

        [HttpPost]
        public void SetBranchState(int branchId, bool collapsed)
        {
            var branch = db.ActivityBranches.Find(branchId);
            int userId;

            if (branch != null && int.TryParse(User.Identity.Name, out userId))
            {
                var existingData = branch.ActivityBranchesStates.FirstOrDefault(x => x.UserId == userId);

                if (existingData == null)
                {
                    branch.ActivityBranchesStates.Add(new ActivityBranchesState
                    {
                        UserId = userId,
                        Collapsed = collapsed
                    });
                }
                else
                {
                    existingData.Collapsed = collapsed;
                }

                activityRepo.Save();
            }
        }

        [HttpPost]
        public void EditPlayerData(int pk, string name, string value)
        {
            var user = usersRepo.GetById(pk);

            if (user != null)
            {
                switch (name)
                {
                    //case nameof(ActivityRegistrationItem.PlayerFullName):
                    //    user.FullName = value;
                    //    break;
                    case nameof(ActivityRegistrationItem.PlayerFirstName):
                        user.FirstName = value;
                        break;
                    case nameof(ActivityRegistrationItem.PlayerLastName):
                        user.LastName = value;
                        break;
                    case nameof(ActivityRegistrationItem.PlayerMiddleName):
                        user.MiddleName = value;
                        break;
                    case nameof(ActivityRegistrationItem.PlayerEmail):
                        user.Email = value;
                        break;
                    case nameof(ActivityRegistrationItem.PlayerPhone):
                        user.Telephone = value;
                        break;
                    case nameof(ActivityRegistrationItem.PlayerCity):
                        user.City = value;
                        break;
                    case nameof(ActivityRegistrationItem.PlayerAddress):
                        user.Address = value;
                        break;
                    case nameof(ActivityRegistrationItem.PlayerBirthDate):
                        user.BirthDay = DateTime.ParseExact(value, "d", CultureInfo.CurrentCulture);
                        break;
                    case nameof(ActivityRegistrationItem.UserIdNum):
                        user.IdentNum = value;
                        break;
                }

                usersRepo.Save();
            }
        }

        [HttpPost]
        public void EditTeam(int pk, string name, string value, int seasonId)
        {
            var team = teamRepo.GetById(pk);

            if (team != null)
            {
                switch (name)
                {
                    case nameof(ActivityRegistrationItem.Team):
                        var seasonalDetails = team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId);
                        if (seasonalDetails != null)
                        {
                            seasonalDetails.TeamName = value;
                        }
                        else
                        {
                            team.Title = value;
                        }
                        break;
                }

                teamRepo.Save();
            }
        }

        [HttpPost]
        public void EditCustomField(int pk, string name, string[] value)
        {
            var registration = activityRepo.GetCollection<ActivityFormsSubmittedData>(x => x.Id == pk).FirstOrDefault();

            if (registration != null && value.Length >= 1)
            {
                var regCustomFields = string.IsNullOrWhiteSpace(registration.CustomFields)
                    ? new List<ActivityFormCustomField>()
                    : JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(registration.CustomFields);

                var fieldToUpdate = regCustomFields.FirstOrDefault(x => x.PropertyName == name);
                if (fieldToUpdate != null)
                {
                    switch (fieldToUpdate.Type)
                    {
                        case ActivityFormControlType.CustomTextBox:
                            fieldToUpdate.Value = value[0];
                            break;

                        case ActivityFormControlType.CustomTextArea:
                            fieldToUpdate.Value = value[0];
                            break;

                        case ActivityFormControlType.CustomDropdown:
                            fieldToUpdate.Value = value[0];
                            break;

                        case ActivityFormControlType.CustomDropdownMultiselect:
                            fieldToUpdate.Value = string.Join(",", value);
                            break;

                        case ActivityFormControlType.CustomCheckBox:
                            fieldToUpdate.Value = value[0] == Messages.Yes
                                ? true.ToString()
                                : false.ToString();
                            break;
                    }

                    registration.CustomFields = JsonConvert.SerializeObject(regCustomFields);
                    activityRepo.Save();
                }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public void CardComIndicator(int terminalNumber, int operation, Guid lowProfileCode)
        {
            var registrations = ActivityFormsRepo.GetRegistrationByCardComLpc(lowProfileCode);
            
            var activity = registrations?.FirstOrDefault()?.Activity;

            var apiUserName = CardComHelper.GetApiUserNameByActivity(activity);
            var indicatorInformation = CardComHelper.GetIndicatorResult(apiUserName, terminalNumber, lowProfileCode);

            db.CardComIndicators.Add(new CardComIndicator
            {
                LowProfileCode = lowProfileCode,
                TerminalNumber = terminalNumber,
                Operation = operation,
                ActivityId = activity?.ActivityId,
                ApiUserName = apiUserName,
                CardComIndicatorInfo = indicatorInformation.OriginalResponse,
                DateCreated = DateTime.Now
            });
            db.SaveChanges();

            if (activity == null)
            {
                return;
            }

            if (!indicatorInformation.Success)
            {
                return;
            }

            foreach (var registration in registrations)
            {
                var fullPrice = FillPaidValues(registration);

                registration.CardComPaymentCompleted = true;
                registration.CardComPaymentDate = DateTime.Now;
                registration.Paid = fullPrice;

                registration.PostponeParticipationPayment = false;

                registration.CardComInvoiceNumber = indicatorInformation.InvoiceNumber;
                registration.CardComNumberOfPayments = indicatorInformation.NumOfPayments;
            }

            ActivityFormsRepo.Save();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult CheckBrotherRegistration(int activityId, string brotherIdNum)
        {
            var brotherRegistered =
                db.ActivityFormsSubmittedDatas.Any(x => x.ActivityId == activityId && x.User.IdentNum == brotherIdNum);

            return brotherRegistered
                ? new HttpStatusCodeResult(HttpStatusCode.OK)
                : new HttpNotFoundResult();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult PayPalSuccess(Guid llPaymentId, string paymentId, string token, string payerId)
        {
            //Activity/PayPalSuccess?paymentId=PAYID-LWBWA5I7J825526Y2487511H&token=EC-01397435XS2277747&PayerID=YTUPMEUGDX2XA 
            var registrations =
                db.ActivityFormsSubmittedDatas.Where(x =>
                        x.PayPalLogLigId == llPaymentId &&
                        x.PayPalPaymentId == paymentId)
                    .ToList();

            if (!registrations.Any())
            {
                return Content("No registrations found");
            }

            PayPalHelper.ExecutePayment(paymentId, payerId);

            foreach (var registration in registrations)
            {
                var fullPrice = FillPaidValues(registration);

                registration.PayPalPayerId = payerId;

                registration.Paid = fullPrice;

                registration.PostponeParticipationPayment = false;
            }

            db.SaveChanges();

            return PartialView("ActivityFormPublishedSubmitResult",
                new ActivityFormSuccessResultModel
                {
                    Activity = registrations.FirstOrDefault()?.Activity,
                    PaymentIdentifier = llPaymentId
                });
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult PayPalCancel()
        {
            return Content("Payment cancelled");
        }

        [HttpPost]
        public ActionResult StatusReplaceFile(int regId, string propertyName)
        {
            var file = Request.Files["file"];

            if (!ValidateFileSize(file))
            {
                return Json(new {error = Messages.FileSizeError});
            }

            if (!ValidateFileType(file))
            {
                return Json(new {error = Messages.FileType});
            }

            var registration = db.ActivityFormsSubmittedDatas.Find(regId);

            if (registration == null)
            {
                return Json(new {error = string.Format(Messages.Activity_RegistrationNotFound, regId)});
            }

            var customFields = !string.IsNullOrWhiteSpace(registration.CustomFields)
                ? JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(registration.CustomFields)
                : new List<ActivityFormCustomField>();

            var customFile = customFields?.FirstOrDefault(x => x.PropertyName == propertyName);
            if (customFile == null)
            {
                return Json(new {error = Messages.FileError});
            }

            customFile.Value = SaveFormFile(registration.ActivityId ?? 0, "file");

            registration.CustomFields = JsonConvert.SerializeObject(customFields);

            db.SaveChanges();

            return Json(new {result = $"{GlobVars.ContentPath}/publishedactivityforms/{customFile.Value}"});
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetCompetitionCategories(int activityId, string playerId, DateTime? birthDate, int? gender)
        {
            var activity = activityRepo.GetById(activityId);
            if (activity == null)
            {
                return Json(new {error = "Activity not found"}, JsonRequestBehavior.AllowGet);
            }

            var user = db.Users.FirstOrDefault(x => x.IdentNum == playerId);
            var userId = user?.UserId ?? 0;

            birthDate = birthDate ?? user?.BirthDay;

            gender = gender ?? user?.GenderId;

            var competitions = db.Leagues
                .Where(x => x.UnionId == activity.UnionId &&
                            x.SeasonId == activity.SeasonId &&
                            !x.IsArchive &&
                            x.EilatTournament == true)
                .ToList();

            var competitionsIds = competitions
                .Select(x => x.LeagueId)
                .ToArray();

            var competitionsPrices = db.LeaguesPrices
                .Where(x => competitionsIds.Contains(x.LeagueId ?? 0) &&
                            x.PriceType == (int) LeaguePriceType.PlayerRegistrationPrice &&
                            x.StartDate <= DateTime.Now &&
                            x.EndDate > DateTime.Now)
                .ToList();

            var competitionsDisciplines = db.CompetitionDisciplines
                .Where(x => competitionsIds.Contains(x.CompetitionId) &&
                            !x.IsDeleted)
                .ToList();

            var competitionsDisciplinesIds = competitionsDisciplines
                .Select(x => x.Id)
                .ToArray();

            var competitionsRegistrations = db.ActivityFormsSubmittedDatas
                .Where(x => x.ActivityId == activityId &&
                            competitionsDisciplinesIds.Contains(x.CompetitionDisciplineId ?? 0))
                .ToList();

            var categoriesIds = competitionsDisciplines
                .Select(x => x.CategoryId)
                .ToArray();

            var competitionAges = db.CompetitionAges
                .Where(x => categoriesIds.Contains(x.id) &&
                            x.from_birth <= birthDate &&
                            x.to_birth > birthDate &&
                            (gender == null || x.gender == gender || x.gender == 3/*all*/))
                .ToList();

            var birthDateThresholdAsChild = new DateTime(2009, 01, 01);
            var minimumCategoriesSelection = birthDate > birthDateThresholdAsChild
                ? 2
                : 4;

            var userRegistrations = db.ActivityFormsSubmittedDatas
                .Where(x => x.ActivityId == activityId &&
                            x.PlayerId == userId)
                .ToList();

            var result = new ActivityFormGetCompetitionCategoriesModel
            {
                Categories = new List<ActivityFormGetCompetitionCategoriesItemModel>(),
                MinimumSelection = minimumCategoriesSelection
            };

            foreach (var competition in competitions)
            {
                var competitionDisciplines = competitionsDisciplines
                    .Where(x => x.CompetitionId == competition.LeagueId)
                    .ToList();

                if (!competitionDisciplines.Any())
                {
                    continue;
                }

                foreach (var competitionDiscipline in competitionDisciplines)
                {
                    var competitionAge = competitionAges
                        .FirstOrDefault(x => x.id == competitionDiscipline?.CategoryId);

                    if (competitionAge == null)
                    {
                        continue;
                    }

                    var registrationPrice = competitionsPrices
                        .FirstOrDefault(x => x.LeagueId == competition.LeagueId);

                    var existingRegistration = userRegistrations
                        .FirstOrDefault(x => x.CompetitionDisciplineId == competitionDiscipline.Id);

                    if (existingRegistration == null &&
                        (competition.StartRegistrationDate > DateTime.Now ||
                         (competition.EndRegistrationDate != null && competition.EndRegistrationDate < DateTime.Now)))
                    {
                        //Skip because competition's registration dates are not met
                        continue;
                    }

                    var registeredAndPaidPlayersCount = competitionsRegistrations
                        .Where(x => x.CompetitionDisciplineId == competitionDiscipline.Id &&
                                    x.RegistrationPaid > 0)
                        .GroupBy(x => x.PlayerId)
                        .Count();

                    if (competitionDiscipline.MaxSportsmen >= 0 &&
                        registeredAndPaidPlayersCount >= competitionDiscipline.MaxSportsmen)
                    {
                        //skip because maximum registrations count reached
                        continue;
                    }

                    var selectionDisabled = false;

                    if (existingRegistration != null &&
                        competition.EndRegistrationDate != null && competition.EndRegistrationDate < DateTime.Now)
                    {
                        selectionDisabled = true;
                    }

                    var alreadyPaid = false;

                    if (existingRegistration != null &&
                        existingRegistration.RegistrationPaid > 0)
                    {
                        alreadyPaid = true;
                    }

                    result.Categories.Add(new ActivityFormGetCompetitionCategoriesItemModel
                    {
                        Id = competitionDiscipline.Id,
                        Name = $"{competition.Name} - {competitionAge.age_name}",
                        RegistrationPrice = existingRegistration == null
                            ? registrationPrice?.Price ?? 0
                            : 0,

                        AlreadyRegistered = existingRegistration != null,
                        RegistrationId = existingRegistration?.Id ?? 0,
                        SelectionDisabled = selectionDisabled,

                        AlreadyPaid = alreadyPaid
                    });
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FixCustomFieldsTypes()
        {
            var sb = new StringBuilder();
            var sw = new Stopwatch();
            sw.Start();

            var registrations = db.ActivityFormsSubmittedDatas
                .Where(x => x.CustomFields != null && x.CustomFields.Length > 2).ToList();

            var customFieldsProcessed = 0;
            var foundBrokenFields = 0;
            var fixedBrokenFields = 0;

            var fixedFields = new List<string>();

            var regTimers = new List<long>();
            var cfTimers = new List<long>();

            var regSw = new Stopwatch();
            var cfSw = new Stopwatch();
            foreach (var reg in registrations)
            {
                regSw.Restart();

                var customFields = JsonConvert.DeserializeObject<List<ActivityFormCustomField>>(reg.CustomFields);

                customFieldsProcessed += customFields.Count;

                foreach (var customField in customFields)
                {
                    cfSw.Restart();

                    var customFieldType = customField.Type;
                    var customFieldTypeByName = Regex.Replace(customField.PropertyName, @"\-[0-9]+$", string.Empty);

                    if (!string.Equals(customFieldType.ToString(), customFieldTypeByName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        foundBrokenFields++;

                        ActivityFormControlType actualType;
                        if (Enum.TryParse(customFieldTypeByName, true, out actualType))
                        {
                            fixedFields.Add($"Broken field: {customField.PropertyName} Type: <b>{customFieldType} -> {actualType}</b>");
                            customField.Type = actualType;

                            fixedBrokenFields++;
                        }
                    }

                    cfSw.Stop();
                    cfTimers.Add(cfSw.ElapsedMilliseconds);
                }

                reg.CustomFields = JsonConvert.SerializeObject(customFields);

                regSw.Stop();
                regTimers.Add(regSw.ElapsedMilliseconds);
            }

            db.SaveChanges();
            sw.Stop();

            sb.AppendLine($"<pre>");
            sb.AppendLine($"Hi!");
            sb.AppendLine($"I have processed {registrations.Count} registrations");
            sb.AppendLine($"Those registrations had {customFieldsProcessed} custom fields");
            sb.AppendLine($"I have found {foundBrokenFields} broken custom fields");
            sb.AppendLine($"{fixedBrokenFields} of them were fixed. Yay! Magic.");
            sb.AppendLine(string.Empty);
            sb.AppendLine($"That took about {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds).TotalSeconds}seconds (includes the database queries round trips)");
            sb.AppendLine($"Each registration took {regTimers.Average()}ms (min: {regTimers.Min()}ms, max: {regTimers.Max()}ms)");
            sb.AppendLine($"Each custom field took {cfTimers.Average()}ms (min: {cfTimers.Min()}ms, max: {cfTimers.Max()}ms)");
            sb.AppendLine(string.Empty);
            sb.AppendLine(string.Empty);
            sb.AppendLine("Below comes the list of changed fields:");
            foreach (var fixedField in fixedFields)
            {
                sb.AppendLine(fixedField);
            }
            sb.AppendLine($"</pre>");

            return Content(sb.ToString());
        }

        [NonAction]
        private void CreateExportFile(System.IO.Stream stream, string fileName)
        {
            byte[] resultFileBytes = null;
            using (stream)
            {
                resultFileBytes = new byte[stream.Length];
                stream.Read(resultFileBytes, 0, resultFileBytes.Length);
            }
            Session.Remove(ExportActivitiesResultSessionKey);
            Session.Remove(ExportActivitiesResultFileNameSessionKey);
            Session.Add(ExportActivitiesResultSessionKey, resultFileBytes);
            Session.Add(ExportActivitiesResultFileNameSessionKey, fileName);
        }

        private List<ActivityBranchViewModel> LoadActivities(int? unionId, int? seasonId, int? clubId, string jobRole,
            bool IsActivityManager, bool IsActivityViewer, string type = "all")
        {
            var activitiesQuery = db.Activities
                .Include(x => x.ActivityBranch)
                .Include(x => x.ActivitiesUsers)
                .Where(p => type == "all" || type == p.Type);
            if (unionId.HasValue)
            {
                activitiesQuery = activitiesQuery.Where(p => p.UnionId == unionId);
            }
            if (clubId.HasValue)
            {
                activitiesQuery = activitiesQuery.Where(p => p.ClubId == clubId);
            }

            if (jobRole != JobRole.Referee && !IsActivityViewer && !IsActivityManager)
            {
                seasonId = seasonId ?? GetUnionCurrentSeasonFromSession();
            }

            if (seasonId.HasValue)
            {
                activitiesQuery = activitiesQuery.Where(p => p.SeasonId == seasonId.Value);
            }

            if (!User.IsInAnyRole(AppRole.Admins) && jobRole != JobRole.UnionManager)
            {
                if (IsActivityManager || IsActivityViewer)
                {
                    //activitiesQuery = activitiesQuery.Where(p => p.ActivitiesUsers.Where(pp => pp.UserId == AdminId && pp.UserGroup == 0).Count() > 0);
                    activitiesQuery = activitiesQuery.Where(p => p.ActivitiesUsers.Any(pp => pp.UserId == AdminId));
                }
                //if (IsActivityViewer)
                //{
                //    activitiesQuery = activitiesQuery.Where(p => p.ActivitiesUsers.Where(pp => pp.UserId == AdminId && pp.UserGroup == 1).Count() > 0);
                //}
            }

            var readOnly = !User.IsInAnyRole(AppRole.Admins) &&
                           !User.HasTopLevelJob(JobRole.UnionManager) &&
                           !User.HasTopLevelJob(JobRole.ClubManager) && 
                           !User.HasTopLevelJob(JobRole.ClubSecretary) &&
                           !User.HasTopLevelJob(JobRole.DepartmentManager);

            var activities = activitiesQuery.Select(p => new ActivityViewModel
            {
                ActivityId = p.ActivityId,
                Date = p.Date,
                Description = p.Description,
                EndDate = p.EndDate,
                IsPublished = p.IsPublished,
                Name = p.Name,
                StartDate = p.StartDate,
                Type = p.Type,
                SeasonName = p.Season == null ? "" : p.Season.Name,
                ActivityBranchId = p.ActivityBranch.AtivityBranchId,
                IsReadOnly = readOnly && !p.ActivitiesUsers.Any(q => q.UserId == AdminId && q.UserGroup == 0),
                IsAutomatic = p.IsAutomatic ?? false,

                ByBenefactor = p.ByBenefactor,
                CanAttachDocument = p.AttachDocuments,
                CanAttachInsuranceCert = p.InsuranceCertificate,
                CanAttachMedicalCert = p.MedicalCertificate,

                RegistationsCount = p.ActivityFormsSubmittedDatas.Count,
                InactiveRegistrationsCount = p.ActivityFormsSubmittedDatas.Count(x => !x.IsActive)
            })
                .ToList();

            var branchesQuery = db.ActivityBranches.AsQueryable();
            if (unionId.HasValue)
            {
                branchesQuery = branchesQuery.Where(p => p.UnionId == unionId.Value);
            }
            if (clubId.HasValue)
            {
                branchesQuery = branchesQuery.Where(p => p.ClubId == clubId.Value);
            }

            if (seasonId.HasValue)
            {
                branchesQuery = branchesQuery.Where(p => p.SeasonId == seasonId.Value);
            }

            var branches = branchesQuery.OrderBy(p => p.BranchName.ToLower().Trim()).ToList();

            var branchesViewModel = new List<ActivityBranchViewModel>();

            foreach (var branch in branches)
            {
                var branchViewModel = new ActivityBranchViewModel
                {
                    ActivityBranchId = branch.AtivityBranchId,
                    ActivityBranchName = branch.BranchName
                };

                branchViewModel.Activities = activities.Where(p => p.ActivityBranchId == branch.AtivityBranchId)
                    .ToList();

                int userId;
                if (int.TryParse(User.Identity.Name, out userId))
                {
                    branchViewModel.Collapsed =
                        branch.ActivityBranchesStates.FirstOrDefault(x => x.UserId == userId)?.Collapsed ?? false;
                }

                branchesViewModel.Add(branchViewModel);
            }

            //var result = activities.GroupBy(p => p.ActivityBranchId)
            //    .Select(p => new ActivityBranchViewModel
            //    {
            //        ActivityBranchId = p.Key,
            //        ActivityBranchName = p.First().ActivityBranchName,
            //        Activities = p.ToList()
            //    }).ToList();

            return branchesViewModel;
        }
    }
}