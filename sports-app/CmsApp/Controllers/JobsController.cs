using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Resources;
using CmsApp.Helpers;
using CmsApp.Models;
using AppModel;
using DataService;
using DataService.DTO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using Ionic.Zip;
using System.Web;
using ClosedXML.Excel;

namespace CmsApp.Controllers
{
    public class JobsController : AdminController
    {
        private const string ExportPlayersResultSessionKey = "ExportPlayers_Result";
        private const string ExportPlayersResultFileNameSessionKey = "ExportPlayers_ResultFileName";
        private const string ImportPlayersErrorResultSessionKey = "ImportPlayers_ErrorResult";
        private const string ImportPlayersErrorResultFileNameSessionKey = "ImportPlayers_ErrorResultFileName";

        TeamsRepo teamsRepo = new TeamsRepo();
        JobsRepo jobsRepo = new JobsRepo();


        //Jobs
        public ActionResult Index(int id)
        {
            var sectionAlias = secRepo.GetById(id)?.Alias;
            var rolesList = sectionAlias != SectionAliases.MultiSport ? jobsRepo.GetRoles(r => r.RoleName != JobRole.DepartmentManager) : jobsRepo.GetRoles();

            var rolesListItems = rolesList.Select(r => new SelectListItem
            {
                Value = r.RoleId.ToString(),
                Text = LangHelper.GetJobName(r.RoleName)
            }).ToList();

            var vm = new JobForm
            {
                SectionId = id,
                JobsList = jobsRepo.GetBySection(id),
                Roles = new SelectList(rolesListItems, "Value", "Text")
            };

            return PartialView("_List", vm);
        }

        [HttpPost]
        public ActionResult Edit(JobForm frm)
        {
            var job = new Job();

            if (frm.JobId == 0)
                jobsRepo.Add(job);
            else
                job = jobsRepo.GetById(frm.JobId);

            UpdateModel(job);

            jobsRepo.Save();

            return RedirectToAction("Index", new { id = frm.SectionId });
        }

        public ActionResult RefreshTravelResults(RefreshTravelResultsModel request)
        {
            var travelInformations = db.TravelInformations.Where(x => x.UserJobId == request.JobId).ToList();
            var dateTime = DateTime.ParseExact(request.SelectedDate, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture);
            if (travelInformations.Count > 0)
            {
                var travelInformation = travelInformations.FirstOrDefault(x => x.FromHour.Value.Date == dateTime);
                TravelSectionModel model;
                if (travelInformation!=null)
                {
                    model = new TravelSectionModel
                    {
                        TravelInformationDto = new TravelInformationDto
                        {
                            NoTravel = travelInformation.NoTravel ?? false,
                            FromHour = travelInformation.FromHour,
                            ToHour = travelInformation.ToHour,
                            IsUnionTravel = travelInformation.IsUnionTravel
                        },
                        IsIndividualSection = request.IsIndividualSection,
                        JobId = request.JobId,
                        IsAthleticsLeague = request.IsAthleticsLeague,
                        SelectedDate = request.SelectedDate
                    };
                    
                }
                else
                {
                    model = new TravelSectionModel
                    {
                        TravelInformationDto = new TravelInformationDto
                        {
                            NoTravel = false,
                            FromHour = dateTime,
                            ToHour = dateTime,
                            IsUnionTravel = false
                        },
                        IsIndividualSection = request.IsIndividualSection,
                        JobId = request.JobId,
                        IsAthleticsLeague = request.IsAthleticsLeague,
                        SelectedDate = request.SelectedDate
                    };
                }
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            return null;
        }
        

        public ActionResult Delete(int id)
        {
            var item = jobsRepo.GetById(id);
            item.IsArchive = true;
            jobsRepo.Save();

            return RedirectToAction("Index", new { id = item.SectionId });
        }

        [HttpPost]
        public void BlockRefereeCompetition(int[] ids, bool value)
        {
            if (ids?.Any() == true)
            {
                var userJobs = db.UsersJobs.Where(x => ids.Contains(x.Id));

                var refereeRegistrations = db.RefereeRegistrations
                    .Where(x => ids.Contains(x.RefereeId) && !x.IsArchive);

                foreach (var userJob in userJobs)
                {
                    userJob.IsCompetitionRegistrationBlocked = value;

                    if (!value)
                    {
                        var refereeRegistration = refereeRegistrations.FirstOrDefault(x => x.RefereeId == userJob.Id);
                        if (refereeRegistration != null)
                        {
                            refereeRegistration.IsArchive = true;
                        }
                    }
                }

                db.SaveChanges();
            }
        }

        //Worker List
        public ActionResult WorkerList(int id, LogicaName logicalName, int seasonId, int? leagueId, int? departmentId = null, bool showAll = false,
            IEnumerable<int> filteredOfficialsIds = null, bool onlyReferees = false, int unionId = 0, string dateSelected = null, bool showInactive = false)
        {
            if (User.HasTopLevelJob(JobRole.CommitteeOfReferees))
            {
                onlyReferees = true;
            }
            var vm = GetWorkersByRelevantEntity(id, logicalName, seasonId, leagueId, departmentId, showAll, onlyReferees);
            vm.UnionId = unionId;
            vm.UsersList = vm.UsersList ?? new List<UserJobDto>();
            if (onlyReferees && logicalName == LogicaName.Union)
            {
                vm.UsersList = vm.UsersList.Where(ul => string.Equals(ul.RoleName, JobRole.Spectator, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(ul.RoleName, JobRole.Referee, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(ul.RoleName, JobRole.CommitteeOfReferees, StringComparison.OrdinalIgnoreCase))?.ToList();
            }
            if (unionId == 37 && !onlyReferees && logicalName == LogicaName.Union)
            {
                vm.UsersList = vm.UsersList.Where(ul => !string.Equals(ul.RoleName, JobRole.Referee, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(ul.RoleName, JobRole.Spectator, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(ul.RoleName, JobRole.CommitteeOfReferees, StringComparison.OrdinalIgnoreCase))?.ToList();
            }
            if (User.HasTopLevelJob(JobRole.CommitteeOfReferees))
            {
                vm.UsersList.Insert(0, jobsRepo.GetUserJobDtoItem(AdminId, seasonId));
            }
            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            vm.JobsRoles = GetCurrentOfficialsJobs(vm.UsersList) ?? new Dictionary<int, JobsRole>();

            if (filteredOfficialsIds != null && filteredOfficialsIds.Any())
            {
                vm.UsersList = vm.UsersList.Where(c => filteredOfficialsIds.Contains(c.RoleId))?.ToList();
            }
            vm.SelectedValues = filteredOfficialsIds?.Select(c => c.ToString()) ?? Enumerable.Empty<string>();

            ViewBag.IsClubOrUnion = logicalName == LogicaName.Union || logicalName == LogicaName.Club;
            ViewBag.SeasonId = seasonId;
            ViewBag.ShowAll = showAll;

            var distanceSettings = logicalName == LogicaName.Union ? db.Unions.FirstOrDefault(u => u.UnionId == id)?.DistanceSettings
                : db.Clubs.FirstOrDefault(c => c.ClubId == id)?.DistanceSettings;

            var reportSettings = logicalName == LogicaName.Union ? db.Unions.FirstOrDefault(u => u.UnionId == id)?.ReportSettings
                : db.Clubs.FirstOrDefault(c => c.ClubId == id)?.ReportSettings;

            ViewBag.DistanceSettings = distanceSettings ?? string.Empty;
            ViewBag.ReportSettings = reportSettings ?? string.Empty;
            var union = unionsRepo.GetById(unionId);
            if (union != null)
            {
                ViewBag.DontShowKarate = union.SectionId != 8;
            }
            ViewBag.IsRegionalManager = (logicalName == LogicaName.RegionalFederation);
            ViewBag.UserIds = vm.UsersList.Select(x => x.UserId);
            ViewBag.DateTimeFrom = DateTime.MinValue;
            ViewBag.DateTimeTo = DateTime.MinValue;
            ViewBag.IsLeague = logicalName == LogicaName.League;
            if (string.IsNullOrEmpty(dateSelected))
            {
                ViewBag.DateSelected = (vm.UsersList.FirstOrDefault() != null && vm.UsersList.First().LeagueDates != null && vm.UsersList.First().LeagueDates.Any()) ? vm.UsersList.First().LeagueDates.Min().Date.ToShortDateString() : DateTime.Now.Date.ToShortDateString();
                foreach (var user in vm.UsersList)
                {
                    user.CurrentDateInformation = (vm.UsersList.FirstOrDefault() != null && vm.UsersList.First().LeagueDates != null && vm.UsersList.First().LeagueDates.Any()) ? vm.UsersList.First().LeagueDates.Min().Date : DateTime.Now.Date;
                }
            } else
            {
                ViewBag.DateSelected = dateSelected;
                foreach (var user in vm.UsersList)
                {
                    var dateTime = DateTime.ParseExact(dateSelected, "dd/MM/yyyy",
                   System.Globalization.CultureInfo.InvariantCulture);
                    user.CurrentDateInformation = dateTime;
                }
            }
            //Active filter
            if (!showInactive)
            {                
                vm.UsersList = vm.UsersList.Where(x => x.Active).ToList();
            }

            return onlyReferees ? PartialView("_RefereesList", vm) : PartialView("_WorkerList", vm);
        }

        private IDictionary<int, JobsRole> GetCurrentOfficialsJobs(IEnumerable<UserJobDto> usersList)
        {
            var jobsIds = usersList?.Select(c => c.Id);
            return jobsRepo.GetOfficialTypesByJobsIds(jobsIds);
        }

        private Workers GetWorkersByRelevantEntity(int id, LogicaName logicalName, int seasonId, int? leagueId, int? departmentId = null, bool showAll = false, bool onlyReferees = false)
        {
            var jobRole = usersRepo.GetTopLevelJob(AdminId);

            var result = new Workers
            {
                RelevantEntityId = id,
                RelevantEntityLogicalName = logicalName,
                SeasonId = seasonId,
                LeagueId = leagueId ?? 0,
                IsReportsEnabled = jobsRepo.CheckForReportsEnabled(id, logicalName),
                StartReportDate = DateTime.Now.AddMonths(-1).ToShortDateString(),
                EndReportDate = DateTime.Now.ToShortDateString(),
                IsUnionManager = usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager,
                IsIndividualSection = CheckForIndividualSection(id, logicalName),
            };

            switch (logicalName)
            {
                case LogicaName.Union:
                    var union = unionsRepo.GetById(id);
                    if (showAll)
                    {
                        result.UsersList = jobsRepo.GetAllUsersJobs(id, LogicaName.Union, seasonId)
                            .GroupBy(elem => new { elem.Id, elem.UserId, elem.FullName }).Select(group => group.First())?.ToList();
                        result.JobsList = new SelectList(jobsRepo.GetByUnion(id), "JobId", "JobName");
                        result.SaturdaysTariff = union.SaturdaysTariff ?? false;
                        result.FromTime = union.SaturdaysTariffFromTime;
                        result.ToTime = union.SaturdaysTariffToTime;
                    }
                    else
                    {
                        result.UsersList = base.jobsRepo.GetUnionUsersJobs(id, seasonId);
                        result.JobsList = new SelectList(base.jobsRepo.GetByUnion(id), "JobId", "JobName");
                        result.SaturdaysTariff = union.SaturdaysTariff ?? false;
                        result.FromTime = union.SaturdaysTariffFromTime;
                        result.ToTime = union.SaturdaysTariffToTime;
                    }

                    if (onlyReferees && id == 37)
                    {
                        result.JobsList = new SelectList(jobsRepo.GetUnionRefereesJobList(id), "JobId", "JobName");
                    }
                    if (!onlyReferees && id == 37)
                    {
                        result.JobsList = new SelectList(jobsRepo.GetAllExceptRefereesJobList(id), "JobId", "JobName");
                    }


                    if (jobRole == JobRole.RefereeAssignment)
                    {
                        var list = jobsRepo.GetUnionRefereesJobList(id);

                        list = ((List<Job>)list).Where(p => p.JobsRole.RoleName != JobRole.RefereeAssignment);

                        result.JobsList = new SelectList(list, "JobId", "JobName");
                    }

                    
                    result.SectionAlias = unionsRepo.GetSectionByUnionId(id)?.Alias ?? string.Empty;
                    result.ReportRemoveDistance = union?.ReportRemoveTravelDistance == true;
                    break;
                case LogicaName.League:
                    result.UsersList = base.jobsRepo.GetLeagueUsersJobs(id, seasonId);

                    var jobs = base.jobsRepo.GetByLeague(id, jobRole == JobRole.RefereeAssignment).ToList();

                    if (User.HasTopLevelJob(JobRole.RefereeAssignment)) //select referee role by default for RefereeAssignment
                    {
                        var refereeJob = jobs.FirstOrDefault(x => x.JobsRole.RoleName == JobRole.Referee);
                        if (refereeJob != null)
                        {
                            result.DefaultJobSelected = refereeJob.JobId;
                        }
                    }

                    result.JobsList = new SelectList(jobs, "JobId", "JobName");

                    result.SectionAlias = leagueRepo.GetSectionAlias(id) ?? string.Empty;
                    break;
                case LogicaName.Team:
                    result.UsersList = base.jobsRepo.GetTeamUsersJobs(id, seasonId);
                    result.JobsList = new SelectList(base.jobsRepo.GetByTeam(id, departmentId), "JobId", "JobName");
                    result.SectionAlias = teamsRepo.GetSectionByTeamId(id)?.Alias ?? string.Empty;
                    break;
                case LogicaName.Club:
                    var club = clubsRepo.GetById(id);
                    if (showAll)
                    {
                        result.UsersList = jobsRepo.GetAllUsersJobs(id, LogicaName.Club, seasonId)
                            .GroupBy(elem => new { elem.Id, elem.UserId, elem.FullName }).Select(group => group.First())?.ToList();
                        result.JobsList = new SelectList(jobsRepo.GetClubJobs(id), "JobId", "JobName");
                    }
                    else
                    {
                        result.UsersList = base.jobsRepo.GetClubOfficials(id, seasonId);
                        result.JobsList = new SelectList(base.jobsRepo.GetClubJobs(id), "JobId", "JobName");
                    }
                    result.SectionAlias = clubsRepo.GetSectionByClubId(id);
                    result.ReportRemoveDistance = club?.ReportRemoveTravelDistance == true;
                    break;
                case LogicaName.Discipline:
                    result.UsersList = base.jobsRepo.GetDisciplineUsersJobs(id)?.ToList();
                    result.JobsList = new SelectList(base.jobsRepo.GetByDiscipline(id), "JobId", "JobName");
                    result.SectionAlias = disciplinesRepo.GetSectionByTeamId(id)?.Alias ?? string.Empty;
                    break;

                case LogicaName.RegionalFederation:
                    //  var itemq = regionalsRepo.GetRegionalById(id);
                    //  int sectionIDq = unionsRepo.GetById(itemq.UnionId.Value).SectionId;

                    var itemInfo = regionalsRepo.GetById(id);
                    var sectionId = itemInfo.Union?.SectionId ?? 0;

                    if (showAll)
                    {
                        result.UsersList = jobsRepo.GetAllUsersJobs(id, LogicaName.RegionalFederation, seasonId)
                            .GroupBy(elem => new { elem.Id, elem.UserId, elem.FullName })
                            .Select(group => group.First())?.ToList();

                        result.JobsList = new SelectList(jobsRepo.GetRegionalManagerJobs(id, sectionId), "JobId", "JobName");
                    }
                    else
                    {
                        result.UsersList = base.jobsRepo.GetRegionalFedOfficials(id, seasonId);
                        result.JobsList = new SelectList(base.jobsRepo.GetRegionalManagerJobs(id, sectionId), "JobId", "JobName");
                    }

                    // result.SectionAlias = new SectionsRepo().GetById(itemInfo.SectionId)?.Alias;
                    result.SectionAlias = itemInfo.Union?.Section?.Alias;

                    break;
            }

            if (result.UsersList != null && result.UsersList.Count > 0)
            {
                ViewBag.UserIds = result.UsersList?.Select(x => x.UserId);
            }
            result.ReportOfficials = jobsRepo.GetCurrentOfficialsForReports(result.UsersList, seasonId);




            return result;
        }

        private bool CheckForIndividualSection(int id, LogicaName logicalName)
        {
            switch (logicalName)
            {
                case LogicaName.Union:
                    var union = unionsRepo.GetById(id);
                    return union?.Section?.IsIndividual ?? false;
                case LogicaName.League:
                    var league = leagueRepo.GetById(id);
                    return league?.Union?.Section?.IsIndividual ?? league?.Club?.Section?.IsIndividual ?? league?.Club?.Union?.Section?.IsIndividual ?? false;
                case LogicaName.Team:
                    var team = teamsRepo.GetById(id);
                    return teamsRepo.GetSectionByTeamId(id)?.IsIndividual ?? false;
                case LogicaName.Club:
                    var club = clubsRepo.GetById(id);
                    return club?.Section?.IsIndividual ?? club?.Union?.Section?.IsIndividual ?? false;
                default:
                    return false;
            }
        }

        public ActionResult ExportWorkersToExcel(int id, LogicaName logicalName, int seasonId, bool showAll = false, IEnumerable<int> filteredOfficialsIds = null,
        bool isReferee = false, int unionId = 37)
        {
            var union = logicalName == LogicaName.Union ? unionsRepo.GetById(id) : null;
            var club = logicalName == LogicaName.Club ? clubsRepo.GetById(id) : null;

            var name = logicalName == LogicaName.Union
                ? union?.Name
                : club?.Name ?? "";
            var sectionAlias = secRepo.GetAlias(
                logicalName == LogicaName.Union ? (int?)id : null,
                logicalName == LogicaName.Club ? (int?)id : null,
                null);
            var isMultiDisciplineSection = sectionAlias?.Equals(GamesAlias.Gymnastic) ?? sectionAlias?.Equals(GamesAlias.Motorsport)
                ?? sectionAlias?.Equals(GamesAlias.Athletics) == true;
            var helper = new ImportExportPlayersHelper();
            var vm = GetWorkersByRelevantEntity(id, logicalName, seasonId, null, null, showAll);
            if (isReferee && logicalName == LogicaName.Union)
            {
                vm.UsersList = vm.UsersList.Where(ul =>
                        string.Equals(ul.RoleName, JobRole.Referee, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(ul.RoleName, JobRole.Spectator, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(ul.RoleName, JobRole.CommitteeOfReferees, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (unionId == 37 && !isReferee && logicalName == LogicaName.Union)
            {
                vm.UsersList = vm.UsersList.Where(ul =>
                        !string.Equals(ul.RoleName, JobRole.Referee, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(ul.RoleName, JobRole.Spectator, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (filteredOfficialsIds != null && filteredOfficialsIds.Any())
            {
                vm.UsersList = vm.UsersList.Where(c => filteredOfficialsIds.Contains(c.RoleId))?.ToList();
            }

            var exportResult = helper.ExportAllWorkers(vm.UsersList, isMultiDisciplineSection, union?.Sport?.Name, getCulture());
            CreateExportFile(exportResult, $"{name}_{Messages.ExportOfficials_FilePostfix}.xlsx");
            ViewBag.ExportResult = true;
            ViewBag.FromExcel = true;
            if (vm.UsersList != null && vm.UsersList.Count > 0)
            {
                ViewBag.UserIds = vm.UsersList?.Select(x => x.UserId);
            }
            return PartialView("_ExportList");
        }

        private void CreateExportFile(Stream stream, string fileName)
        {
            byte[] resultFileBytes = null;
            using (stream)
            {
                resultFileBytes = new byte[stream.Length];
                stream.Read(resultFileBytes, 0, resultFileBytes.Length);
            }
            Session.Remove(ExportPlayersResultSessionKey);
            Session.Remove(ExportPlayersResultFileNameSessionKey);
            Session.Add(ExportPlayersResultSessionKey, resultFileBytes);
            Session.Add(ExportPlayersResultFileNameSessionKey, fileName);
        }

        [HttpGet]
        public ActionResult DownloadExportFile()
        {
            var fileByteObj = Session[ExportPlayersResultSessionKey];
            if (fileByteObj == null)
            {
                throw new FileNotFoundException();
            }

            var fileBytes = (byte[])fileByteObj;
            var fileName = Session[ExportPlayersResultFileNameSessionKey] as string;

            return File(fileBytes, "application/octet-stream", fileName);
        }

        // CRUD Worker
        public ActionResult EditWorker(int id, int relevantEntityId, LogicaName logicalName, int seasonId, int? leagueId, string SectionAlias,
            bool showAll = false, bool onlyReferees = false)
        {
            var jobRole = usersRepo.GetTopLevelJob(AdminId);
            var season = seasonsRepository.GetById(seasonId);

            ViewBag.IsAthletics = GamesAlias.Athletics == SectionAlias;

            var model = new CreateWorkerForm
            {
                AlternativeId = season?.UnionId == GlobVars.UkraineGymnasticUnionId,
                RelevantEntityId = relevantEntityId,
                RelevantEntityLogicalName = logicalName,
            };

            if (!showAll)
            {
                switch (model.RelevantEntityLogicalName)
                {
                    case LogicaName.Union:
                        if (relevantEntityId == 37 && onlyReferees)
                            model.JobsList = new SelectList(jobsRepo.GetUnionRefereesJobList(model.RelevantEntityId), "JobId", "JobName");
                        else if (relevantEntityId == 37 && !onlyReferees)
                            model.JobsList = new SelectList(jobsRepo.GetAllExceptRefereesJobList(model.RelevantEntityId), "JobId", "JobName");
                        else
                            model.JobsList = new SelectList(jobsRepo.GetByUnion(model.RelevantEntityId), "JobId", "JobName");

                        model.UnionClubs = clubsRepo.GetByUnion(relevantEntityId, seasonId)?.Select(uc => new ClubShort { Id = uc.ClubId, Name = uc.Name })
                            ?? Enumerable.Empty<ClubShort>();
                        model.UnionDisciplines = disciplinesRepo.GetAllByUnionId(relevantEntityId)
                            ?? Enumerable.Empty<DisciplineDTO>();
                        break;
                    case LogicaName.League:
                        model.JobsList = new SelectList(jobsRepo.GetByLeague(model.RelevantEntityId, jobRole == JobRole.RefereeAssignment), "JobId", "JobName");
                        break;
                    case LogicaName.Team:
                        model.JobsList = new SelectList(jobsRepo.GetByTeam(model.RelevantEntityId), "JobId", "JobName");
                        break;
                    case LogicaName.Club:
                        model.JobsList = new SelectList(jobsRepo.GetClubJobs(model.RelevantEntityId), "JobId", "JobName");
                        break;
                    case LogicaName.RegionalFederation:
                        model.JobsList = new SelectList(jobsRepo.GetRegionalManagerJobs(model.RelevantEntityId), "JobId", "JobName");
                        break;
                    default:
                        model.JobsList = new List<SelectListItem>();
                        break;
                }
            }
            else
            {
                model.JobsList = new List<SelectListItem>();
            }

            var userJob = jobsRepo.GetUsersJobById(id);

            model.JobId = userJob.JobId;
            if (logicalName == LogicaName.Union && relevantEntityId == 37)
            {
                if (!model.JobsList.Select(c => (int.Parse(c.Value))).Contains(model.JobId))
                {
                    var necessaryJobItem = jobsRepo.GetJobItem(model.JobId);
                    if (necessaryJobItem != null)
                    {
                        var selectListItem = new SelectListItem { Value = necessaryJobItem.JobId.ToString(), Text = necessaryJobItem.JobName };
                        var jobLists = model.JobsList.ToList();
                        jobLists.Add(selectListItem);
                        model.JobsList = jobLists;
                    }
                }
            }

            model.UserJobId = userJob.Id;
            model.SeasonId = seasonId;
            model.WithholdingTax = userJob.WithhodlingTax;

            model.Phone = userJob.User.Telephone;
            model.Address = userJob.User.Address;
            model.City = userJob.User.City;
            model.FirstName = userJob.User.FirstName;
            model.LastName = userJob.User.LastName;
            model.MiddleName = userJob.User.MiddleName;
            model.FullNameFormatted = userJob.User.FullName;
            model.IdentNum = userJob.User.IdentNum;
            model.Email = userJob.User.Email;
            model.BirthDate = userJob.User.BirthDay;
            model.IsActive = userJob.User.IsActive;
            model.UserId = userJob.UserId;

            if(userJob.FormatPermissions != null && userJob.FormatPermissions.Length > 0)
                model.FormatPermissions = userJob.FormatPermissions.Split(',').ToList();

            model.IsKarateReferee = secRepo.IsKarateSection(model.RelevantEntityId, logicalName) && string.Equals(userJob?.Job?.JobsRole?.RoleName, JobRole.Referee);
            model.IsKarate = secRepo.IsKarateSection(model.RelevantEntityId, logicalName);

            model.IsReferee = string.Equals(userJob?.Job?.JobsRole?.RoleName, JobRole.Referee);
            model.IsRefereeCommittee = string.Equals(userJob?.Job?.JobsRole?.RoleName, JobRole.CommitteeOfReferees);

            model.IsRelevantUser = string.Equals(usersRepo.GetTopLevelJob(AdminId), JobRole.UnionManager, StringComparison.OrdinalIgnoreCase) || User.IsInRole(AppRole.Admins);
            model.CanConnectClubs =
                string.Equals(usersRepo.GetTopLevelJob(AdminId), JobRole.UnionManager,
                    StringComparison.OrdinalIgnoreCase) ||
                User.IsInRole(AppRole.Admins) ||
                model.IsReferee ||
                model.IsRefereeCommittee;
            model.ConnectedClubId = userJob?.ConnectedClubId;
            model.SelectedDisciplinesIds = userJob?.ConnectedDisciplineIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            model.PaymentRateType = userJob?.RateType;

            model.JobRole = usersRepo.GetTopLevelJob(AdminId);
            model.IsRefereeRole = string.Equals(userJob?.Job?.JobsRole?.RoleName, JobRole.Referee, StringComparison.OrdinalIgnoreCase);
            var education = usersRepo.GetEducationByUserId(userJob.UserId);

            if (education != null)
            {
                model.Education = education.Education;
                model.PlaceOfEducation = education.PlaceOfEducation;
                model.DateOfEdIssue = education.DateOfEdIssue;
                model.EducationCert = education.EducationCert;
            }

            if (model.IsKarateReferee || (model.IsKarate && model.IsRefereeCommittee))
            {
                var refereesRanks = jobsRepo.GetAllRefereesRanks(model.UserJobId);
                if (refereesRanks.Any())
                {
                    var helper = GetRefereesRanksHelperInstance();
                    helper.SetRefereesRanks(model, refereesRanks);
                }
            }
            if (userJob.Job.RoleId == 15)
            {
                if (!string.IsNullOrEmpty(userJob.User.RefereeCertificate))
                {
                    model.CoachCertificate = userJob.User.RefereeCertificate;
                }
            }


            if (!string.IsNullOrEmpty(userJob.User.Password))
            {
                model.Password = Protector.Decrypt(userJob.User.Password);
            }

            ViewBag.ShowAll = showAll;
            ViewBag.OnlyReferees = onlyReferees;
            model.IsUnionCoach = userJob.Job.RoleId == 15;
            model.Function = userJob.Function;

            ViewBag.IsRegionalManager = logicalName == LogicaName.RegionalFederation;

            return PartialView("_EditWorker", model);
        }

        [HttpPost]
        public ActionResult EditWorker(CreateWorkerForm model, bool showAll = false, bool onlyReferees = false)
        {
            var user = usersRepo.GetById(model.UserId);
            var userJob = jobsRepo.GetUsersJobById(model.UserJobId);
            var jobRole = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.IsAthletics = false;

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

            var isUserInJob = jobsRepo.IsUserInJob(model.RelevantEntityLogicalName, model.RelevantEntityId, model.JobId, userJob.UserId, model.SeasonId);

            if (isUserInJob && userJob.JobId != model.JobId)
            {
                ModelState.AddModelError("JobId", Messages.UserAlreadyHasThisRole);
            }

            if (model.Address == null)
            {
                ModelState.AddModelError("Address",
                    Messages.AddressModelStateError.Replace("{0}", Messages.Workers.ToLowerInvariant()));
            }
            if (!showAll)
            {
                switch (model.RelevantEntityLogicalName)
                {
                    case LogicaName.Union:
                        if (model.RelevantEntityId == 37 && onlyReferees)
                            model.JobsList = new SelectList(jobsRepo.GetUnionRefereesJobList(model.RelevantEntityId), "JobId", "JobName");
                        else if (model.RelevantEntityId == 37 && !onlyReferees)
                            model.JobsList = new SelectList(jobsRepo.GetAllExceptRefereesJobList(model.RelevantEntityId), "JobId", "JobName");
                        else
                            model.JobsList = new SelectList(jobsRepo.GetByUnion(model.RelevantEntityId), "JobId", "JobName");
                        ViewBag.IsAthletics = GamesAlias.Athletics == unionsRepo.GetById(model.RelevantEntityId).Section.Alias;
                        break;
                    case LogicaName.League:
                        model.JobsList = new SelectList(jobsRepo.GetByLeague(model.RelevantEntityId, jobRole == JobRole.RefereeAssignment), "JobId", "JobName");
                        break;
                    case LogicaName.Team:
                        model.JobsList = new SelectList(jobsRepo.GetByTeam(model.RelevantEntityId), "JobId", "JobName");
                        break;
                    case LogicaName.Club:
                        model.JobsList = new SelectList(jobsRepo.GetClubJobs(model.RelevantEntityId), "JobId", "JobName");
                        break;
                    case LogicaName.RegionalFederation:
                        model.JobsList = new SelectList(jobsRepo.GetRegionalManagerJobs(model.RelevantEntityId), "JobId", "JobName");
                        break;
                    default:
                        model.JobsList = new List<SelectListItem>();
                        break;
                }
            }


            if (ModelState.IsValid)
            {
                if (userJob.Job.RoleId == 15)
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
                }

                user.Address = model.Address;
                user.BirthDay = model.BirthDate;
                user.City = model.City;
                user.Telephone = model.Phone;
                user.IsActive = model.IsActive;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.MiddleName = model.MiddleName;
                user.Email = model.Email;
                userJob.WithhodlingTax = model.WithholdingTax;

                //set format permission for entire season
                jobsRepo.GetAllUsersJobs(model.UserId).Where(x => x.SeasonId == model.SeasonId).ForEach(job =>
                
                    job.FormatPermissions = string.Join(",", model.FormatPermissions)
                );
                //userJob.FormatPermissions = string.Join(",", model.FormatPermissions);
                user.Password = Protector.Encrypt(model.Password);
                user.IdentNum = model.IdentNum;
                usersRepo.UpdateEducationInfo(user.UserId, model.Education, model.PlaceOfEducation, model.DateOfEdIssue);
                ProcessUserFiles(user, PlayerFileType.EducationCert, model.RemoveEducationCert);
                usersRepo.Save();

                if (User.HasTopLevelJob(JobRole.CommitteeOfReferees))
                {
                    var userJobs = jobsRepo.GetAllUsersJobs(model.UserId).Where(c => c.UnionId == model.RelevantEntityId).FirstOrDefault();
                    model.JobId = userJobs.JobId;
                }

                userJob.JobId = model.JobId;
                userJob.SeasonId = model.SeasonId;
                userJob.LeagueId = (userJob.LeagueId == 0 ? null : userJob.LeagueId);
                userJob.RateType = model.PaymentRateType;
                userJob.ConnectedClubId = model.ConnectedClubId;
                userJob.ConnectedDisciplineIds = model.ConnectedDisciplineIds != null ? string.Join(",", model.ConnectedDisciplineIds) : null;
                userJob.Function = model.Function;

                var isKarateReferee = Request.Form["IsKarateReferee"] == "True";
                if (isKarateReferee || (model.IsKarate && model.IsRefereeCommittee))
                {
                    jobsRepo.DeleteCurrentRanks(userJob);
                    var helper = GetRefereesRanksHelperInstance();
                    helper.AlterRefereesRanks(jobsRepo, model, userJob);
                }


                jobsRepo.Save();

                ViewBag.OnlyReferees = onlyReferees;
                //RedirectToAction(nameof(WorkerList), new { id = frm.RelevantEntityId, logicalName = frm.RelevantEntityLogicalName, seasonId = frm.SeasonId, showAll = showAll });
                return Json(new { Success = true });
            }

            ViewBag.ShowAll = showAll;
            ViewBag.OnlyReferees = onlyReferees;

            ViewBag.IsRegionalManager = (model.RelevantEntityLogicalName == LogicaName.RegionalFederation);

            return PartialView("_EditWorker", model);
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

        public ActionResult CreateWorker(int relevantEntityId, LogicaName logicalName, int seasonId, bool onlyReferees = false,
            int unionId = 0)
        {
            var season = seasonsRepository.GetById(seasonId);

            var model = new CreateWorkerForm
            {
                AlternativeId = season?.UnionId == GlobVars.UkraineGymnasticUnionId,
                RelevantEntityId = relevantEntityId,
                RelevantEntityLogicalName = logicalName,
                SeasonId = seasonId,
                OnlyReferees = onlyReferees
            };

            var jobRole = usersRepo.GetTopLevelJob(AdminId);

            switch (model.RelevantEntityLogicalName)
            {
                case LogicaName.Union:
                    if (relevantEntityId == 37 && onlyReferees)
                        model.JobsList = new SelectList(jobsRepo.GetUnionRefereesJobList(model.RelevantEntityId), "JobId", "JobName");
                    else if (relevantEntityId == 37 && !onlyReferees)
                        model.JobsList = new SelectList(jobsRepo.GetAllExceptRefereesJobList(model.RelevantEntityId), "JobId", "JobName");
                    else
                        model.JobsList = new SelectList(jobsRepo.GetByUnion(model.RelevantEntityId), "JobId", "JobName");

                    if (jobRole == JobRole.RefereeAssignment)
                    {
                        var list = jobsRepo.GetUnionRefereesJobList(model.RelevantEntityId);

                        list = ((List<Job>)list).Where(p => p.JobsRole.RoleName != JobRole.RefereeAssignment);

                        model.JobsList = new SelectList(list, "JobId", "JobName");
                    }

                    break;
                case LogicaName.League:
                    model.JobsList = new SelectList(jobsRepo.GetByLeague(model.RelevantEntityId, jobRole == JobRole.RefereeAssignment), "JobId", "JobName");
                    break;
                case LogicaName.Team:
                    model.JobsList = new SelectList(jobsRepo.GetByTeam(model.RelevantEntityId), "JobId", "JobName");
                    break;
                case LogicaName.Club:
                    model.JobsList = new SelectList(jobsRepo.GetClubJobs(model.RelevantEntityId), "JobId", "JobName");
                    break;
                case LogicaName.Discipline:
                    model.JobsList = new SelectList(jobsRepo.GetByDiscipline(model.RelevantEntityId), "JobId", "JobName");
                    break;
                case LogicaName.RegionalFederation:
                    model.JobsList = new SelectList(jobsRepo.GetRegionalManagerJobs(model.RelevantEntityId), "JobId", "JobName");
                    break;
                default:
                    model.JobsList = new List<SelectListItem>();
                    break;
            }
            ViewBag.OnlyReferees = onlyReferees;

            ViewBag.IsRegionalManager = logicalName == LogicaName.RegionalFederation;

            model.IsActive = true;
            return PartialView("_CreateWorker", model);
        }

        [HttpPost]
        public ActionResult CreateWorker(CreateWorkerForm model, bool onlyReferees = false)
        {
            if (usersRepo.GetByIdentityNumber(model.IdentNum) != null)
            {
                ModelState.AddModelError("IdentNum", string.Format(Messages.IdIsAlreadyExists, model.IdentNum));
            }

            var jobRole = usersRepo.GetTopLevelJob(AdminId);

            switch (model.RelevantEntityLogicalName)
            {
                case LogicaName.Union:
                    model.JobsList = new SelectList(jobsRepo.GetByUnion(model.RelevantEntityId), "JobId", "JobName");
                    break;
                case LogicaName.Discipline:
                    model.JobsList = new SelectList(jobsRepo.GetByDiscipline(model.RelevantEntityId), "JobId", "JobName");
                    break;
                case LogicaName.League:
                    model.JobsList = new SelectList(jobsRepo.GetByLeague(model.RelevantEntityId, jobRole == JobRole.RefereeAssignment), "JobId", "JobName");
                    break;
                case LogicaName.Team:
                    model.JobsList = new SelectList(jobsRepo.GetByTeam(model.RelevantEntityId), "JobId", "JobName");
                    break;
                case LogicaName.Club:
                    model.JobsList = new SelectList(jobsRepo.GetClubJobs(model.RelevantEntityId), "JobId", "JobName");
                    break;
                case LogicaName.RegionalFederation:
                    model.JobsList = new SelectList(jobsRepo.GetRegionalManagerJobs(model.RelevantEntityId), "JobId", "JobName");
                    break;
                default:
                    model.JobsList = new List<SelectListItem>();
                    break;
            }

            ViewBag.IsRegionalManager = (model.RelevantEntityLogicalName == LogicaName.RegionalFederation);

            if (ModelState.IsValid)
            {
                var user = new User();
                UpdateModel(user);
                user.Password = Protector.Encrypt(model.Password);
                user.TypeId = 3;
                user.Address = model.Address;
                user.City = model.City;
                user.Telephone = model.Phone;
                user.BirthDay = model.BirthDate;
                usersRepo.Create(user);
                if (User.HasTopLevelJob(JobRole.CommitteeOfReferees))
                {
                    var alljobs = jobsRepo.GetByUnion(model.RelevantEntityId).Where(c => c.JobsRole.RoleName == JobRole.Referee).FirstOrDefault();
                    model.JobId = alljobs.JobId;
                }
                var uJob = new UsersJob
                {
                    JobId = model.JobId,
                    UserId = user.UserId,
                    SeasonId = model.SeasonId,
                    WithhodlingTax = model.WithholdingTax, 
                    Active = true,
                    Function = model.Function
                };

                switch (model.RelevantEntityLogicalName)
                {
                    case LogicaName.Union:
                        uJob.UnionId = model.RelevantEntityId;
                        break;
                    case LogicaName.Discipline:
                        uJob.DisciplineId = model.RelevantEntityId;
                        break;
                    case LogicaName.League:
                        uJob.LeagueId = model.RelevantEntityId;
                        break;
                    case LogicaName.Team:
                        uJob.TeamId = model.RelevantEntityId;
                        break;
                    case LogicaName.Club:
                        uJob.ClubId = model.RelevantEntityId;
                        break;
                    case LogicaName.RegionalFederation:
                        uJob.RegionalId = model.RelevantEntityId;
                        break;
                }

                var jRepo = new JobsRepo();
                jRepo.AddUsersJob(uJob);
                jRepo.Save();

                ViewBag.SeasonId = model.SeasonId;
                ViewBag.OnlyReferees = onlyReferees;
                TempData["WorkerAddedSuccessfully"] = true;
            }

            return PartialView("_CreateWorker", model);
        }

        public ActionResult DeleteWorker(int id, int relevantEntityId, LogicaName logicalName, int seasonId, bool showAll, bool onlyReferees = false)
        {
            jobsRepo.RemoveUsersJob(id);

            return RedirectToAction("WorkerList", new { id = relevantEntityId, logicalName, seasonId, showAll, onlyReferees });
        }

        [HttpPost]
        public ActionResult UpdateWorkTimes(int id, DateTime? fromHour, DateTime? toHour, string dateTimeString, bool isUnionTravel, bool noTravel)
        {
            var dateTime = DateTime.ParseExact(dateTimeString, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture);
            jobsRepo.UpdateWorkTimes(id, fromHour, toHour, dateTime, isUnionTravel, noTravel);
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult UpdateWorkTimesAll(List<int> userJobsIds, DateTime? fromHour, DateTime? toHour, string dateTimeString)
        {
            var dateTime = fromHour.Value.Date;
            if (!string.IsNullOrWhiteSpace(dateTimeString))
            {
                dateTime = DateTime.ParseExact(dateTimeString, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }

            jobsRepo.UpdateWorkTimesAll(userJobsIds, fromHour, toHour, dateTime);
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult Search(string term, int id, LogicaName logicalName, int? seasonId)
        {

            var result = new List<ListItemDto>();

            var unionId = 0;

            switch (logicalName)
            {
                case LogicaName.Union:
                    unionId = id;
                    break;
                //case LogicaName.Discipline:
                //    sectionId = secRepo.GetByDisciplineId(id).SectionId;
                //    break;
                case LogicaName.League:
                    unionId = leagueRepo.GetById(id)?.UnionId ?? 0;
                    break;
                //case LogicaName.Team:
                //    var team = teamRepo.GetById(id);
                //    result = GetAllPlayersFromUnionOrClubByTeam(team, term);
                //    break;
                case LogicaName.Club:
                    unionId = clubsRepo.GetById(id)?.UnionId ?? 0;
                    break;

                    //case LogicaName.Player:
                    //    break;

                    //default:
                    //    throw new ArgumentOutOfRangeException(nameof(logicalName), logicalName, null);
            }
            //result.AddRange(usersRepo.SearchSectionUser(sectionId, AppRole.Workers, term, 999));

            if(seasonId.HasValue && unionId == 0)
            {
                var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId.Value);
                if(season != null)
                {
                    unionId = season.UnionId ?? 0;
                }
            }



            var isDigitsOnly = term.All(char.IsDigit);
            var isUnionManager = usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager;
            List<ListItemDto> results;
            if (isDigitsOnly)
            {
                results = usersRepo.SearchUsersByIdentNum(term, 999, seasonId.Value, isUnionManager, unionId);

            }
            else
            {
                results = User.HasTopLevelJob(JobRole.RefereeAssignment)
                    ? usersRepo.SearchAllReferees(term, 999, unionId, seasonId ?? 0)
                    : usersRepo.SearchAllUsers(term, 999, seasonId.Value, isUnionManager, unionId);
            }

            result.AddRange(results);

            result = result.OrderBy(x => x.Name).ToList();

            return Json(result);
        }

        private List<ListItemDto> GetAllPlayersFromUnionOrClubByTeam(Team team, string searchTerm)
        {
            var result = new List<ListItemDto>();

            if (team.LeagueTeams.Any())
            {
                //by union
                var union = team.LeagueTeams.First().Leagues.Union;
                if (union != null)
                {
                    var allLeagues = union.Leagues;
                    foreach (var league in allLeagues)
                    {
                        foreach (var leagueTeam in league.LeagueTeams)
                        {
                            result.AddRange(leagueTeam.Teams.TeamsPlayers.Where(x => x.User.FullName.Contains(searchTerm)).Select(x => new ListItemDto { Id = x.User.UserId, Name = x.User.FullName }));
                        }
                    }
                }
            }
            else if (team.ClubTeams.Any())
            {
                //by club
                var club = team.ClubTeams.First().Club;
                if (club != null)
                {
                    var allTeams = club.ClubTeams;
                    foreach (var clubTeam in allTeams)
                    {
                        result.AddRange(clubTeam.Team.TeamsPlayers.Where(x => x.User.FullName.Contains(searchTerm)).Select(x => new ListItemDto { Id = x.User.UserId, Name = x.User.FullName }));
                    }
                }
            }

            return result.DistinctBy(x => x.Id).ToList();
        }

        public ActionResult AddExistingUser(Workers model, bool showAll = false, bool isReferee = false, int unionId = 0, bool onlyReferees = false)
        {
            var uRepo = new UsersRepo();
            var userId = model.UserId == 0 ? model.RefereeId : model.UserId;
            var user = uRepo.GetById(userId);
            if (User.HasTopLevelJob(JobRole.CommitteeOfReferees))
            {
                var alljobs = jobsRepo.GetByUnion(model.RelevantEntityId).Where(c => c.JobsRole.RoleName == JobRole.Referee).FirstOrDefault();
                model.JobId = alljobs.JobId;
                onlyReferees = true;
            }
            ViewBag.SeasonId = model.SeasonId;
            ViewBag.IsClubOrUnion = model.RelevantEntityLogicalName == LogicaName.Union || model.RelevantEntityLogicalName == LogicaName.Club;
            ViewBag.ShowAll = showAll;

            var vm = new Workers();

            if (user == null || model.FullName != $"{user.FirstName} {user.LastName}")
            {
                ModelState.AddModelError("FullName", Messages.UserNotExists);
                vm = GetWorkersByRelevantEntity(model.RelevantEntityId, model.RelevantEntityLogicalName, model.SeasonId, model.LeagueId, null, showAll);
                vm.JobsRoles = showAll ? GetCurrentOfficialsJobs(vm.UsersList) : vm.JobsRoles = new Dictionary<int, JobsRole>();
                vm.UnionId = unionId;
                if (User.HasTopLevelJob(JobRole.CommitteeOfReferees))
                {
                    vm.UsersList = vm.UsersList
                        .Where(x =>
                            string.Equals(x.RoleName, JobRole.Spectator, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(x.RoleName, JobRole.Referee, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    if (vm.UsersList != null && vm.UsersList.Count > 0)
                    {
                        ViewBag.UserIds = vm.UsersList?.Select(x => x.UserId);
                    }
                }
                ViewBag.DateSelected = vm.UsersList.FirstOrDefault() != null 
                    ? vm.UsersList.First().LeagueDates?.Min().Date.ToShortDateString() 
                    : null;
                foreach (var u in vm.UsersList)
                {
                    u.CurrentDateInformation = vm.UsersList.Any()
                        ? vm.UsersList.First().LeagueDates?.Min().Date ?? DateTime.Now.Date
                        : DateTime.Now.Date;
                }
                return onlyReferees ? PartialView("_RefereesList", vm) : PartialView("_WorkerList", vm);
            }

            var uJob = new UsersJob
            {
                JobId = model.JobId,
                UserId = user.UserId,
                SeasonId = model.SeasonId,
                LeagueId = (model.LeagueId == 0 ? (int?)null : model.LeagueId),
                Active = true
            };

            switch (model.RelevantEntityLogicalName)
            {
                case LogicaName.Union:
                    uJob.UnionId = model.RelevantEntityId;
                    break;
                case LogicaName.Discipline:
                    uJob.DisciplineId = model.RelevantEntityId;
                    break;
                case LogicaName.League:
                    uJob.LeagueId = model.RelevantEntityId;
                    break;
                case LogicaName.Team:
                    uJob.TeamId = model.RelevantEntityId;
                    break;
                case LogicaName.Club:
                    uJob.ClubId = model.RelevantEntityId;
                    break;
                case LogicaName.RegionalFederation:
                    uJob.RegionalId = model.RelevantEntityId;
                    break;
            }

            if (jobsRepo.IsUserInJob(model.RelevantEntityLogicalName, model.RelevantEntityId, uJob.JobId, user.UserId, model.SeasonId))
            {
                ModelState.AddModelError("FullName", Messages.UserAlreadyHasThisRole);
                vm = GetWorkersByRelevantEntity(model.RelevantEntityId, model.RelevantEntityLogicalName, model.SeasonId, model.LeagueId, null, showAll);
                vm.JobsRoles = showAll ? GetCurrentOfficialsJobs(vm.UsersList) : vm.JobsRoles = new Dictionary<int, JobsRole>();
                if (User.HasTopLevelJob(JobRole.CommitteeOfReferees))
                {
                    vm.UsersList = vm.UsersList.Where(ul => string.Equals(ul.RoleName, JobRole.Spectator, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(ul.RoleName, JobRole.Referee, StringComparison.OrdinalIgnoreCase))?.ToList();
                    if (vm.UsersList != null && vm.UsersList.Count > 0)
                    {
                        ViewBag.UserIds = vm.UsersList?.Select(x => x.UserId);
                    }
                }
                ViewBag.DateSelected = vm.UsersList.FirstOrDefault() != null ? vm.UsersList.First().LeagueDates.Min().Date.ToShortDateString() : null;
                foreach (var u in vm.UsersList)
                {
                    u.CurrentDateInformation = vm.UsersList.FirstOrDefault() != null ? vm.UsersList.First().LeagueDates.Min().Date : DateTime.Now.Date;
                }
                return onlyReferees ? PartialView("_RefereesList", vm) : PartialView("_WorkerList", vm);
            }

            // have to test this code...
            if (model.JobId <= 0)
            {
                ModelState.AddModelError("JobId", "Please select");
                vm = GetWorkersByRelevantEntity(model.RelevantEntityId, model.RelevantEntityLogicalName, model.SeasonId, model.LeagueId, null, showAll);
                vm.JobsRoles = showAll ? GetCurrentOfficialsJobs(vm.UsersList) : vm.JobsRoles = new Dictionary<int, JobsRole>();
                vm.UnionId = unionId;
                if (User.HasTopLevelJob(JobRole.CommitteeOfReferees))
                {
                    vm.UsersList = vm.UsersList.Where(ul => string.Equals(ul.RoleName, JobRole.Spectator, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(ul.RoleName, JobRole.Referee, StringComparison.OrdinalIgnoreCase))?.ToList();
                    if (vm.UsersList != null && vm.UsersList.Count > 0)
                    {
                        ViewBag.UserIds = vm.UsersList?.Select(x => x.UserId);
                    }
                }
                ViewBag.DateSelected = vm.UsersList.FirstOrDefault() != null ? vm.UsersList.First().LeagueDates.Min().Date.ToShortDateString() : null;
                foreach (var u in vm.UsersList)
                {
                    u.CurrentDateInformation = vm.UsersList.FirstOrDefault() != null ? vm.UsersList.First().LeagueDates.Min().Date : DateTime.Now.Date;
                }
                return onlyReferees ? PartialView("_RefereesList", vm) : PartialView("_WorkerList", vm);
            }

            jobsRepo.AddUsersJob(uJob);
            jobsRepo.Save();
            TempData["SavedId"] = uJob.UserId;

            vm = GetWorkersByRelevantEntity(model.RelevantEntityId, model.RelevantEntityLogicalName, model.SeasonId, model.LeagueId, null, showAll);
            vm.JobsRoles = showAll ? GetCurrentOfficialsJobs(vm.UsersList) : vm.JobsRoles = new Dictionary<int, JobsRole>();
            vm.StartReportDate = DateTime.Now.AddMonths(-1).ToShortDateString();
            vm.EndReportDate = DateTime.Now.ToShortDateString();
            vm.ReportOfficials = jobsRepo.GetCurrentOfficialsForReports(model.UsersList, model.SeasonId);

            return RedirectToAction(nameof(WorkerList), new
            {
                id = model.RelevantEntityId,
                logicalName = model.RelevantEntityLogicalName,
                seasonId = model.SeasonId,
                leagueId = model.LeagueId,
                showAll,
                onlyReferees,
                unionId = vm.RelevantEntityLogicalName == LogicaName.Union ? vm.RelevantEntityId : 0
            });
        }

        public ActionResult DistanceEdit(int id, LogicaName logicalName, int? seasonId)
        {
            var distances = jobsRepo.GetDistances(id, logicalName, seasonId);
            ViewBag.Id = id;
            ViewBag.LogicalName = logicalName;
            ViewBag.SeasonId = seasonId;
            return PartialView("_DistanceForm", distances);
        }

        public ActionResult AddNewDistance(DistanceTableDto frm, int relevantId, LogicaName logicalName, int? seasonId)
        {
            if (frm.Distance == 0 || string.IsNullOrEmpty(frm.CityFromName) || string.IsNullOrEmpty(frm.CityToName))
            {
                return Json(new { Message = Messages.DistanceTable_EmptyFields });
            }
            else
            {
                jobsRepo.AddNewDistance(frm, relevantId, logicalName, seasonId);
                return RedirectToAction("DistanceEdit", new { id = relevantId, logicalName = logicalName, seasonId = seasonId });
            }
        }

        [HttpPost]
        public ActionResult UpdateDistance(DistanceTableDto frm, int relevantId, LogicaName logicalName, int? seasonId)
        {
            var success = jobsRepo.UpdateDistance(frm, out var message);
            return success
                ? (ActionResult)RedirectToAction("DistanceEdit", new { id = relevantId, logicalName = logicalName, seasonId = seasonId })
                : Json(new { IsSuccess = false, Message = message });
        }

        [HttpPost]
        public ActionResult DeleteDistance(int id, int relevantId, LogicaName logicalName, int? seasonId)
        {
            var success = jobsRepo.DeleteDistance(id, out var message);
            return success
                ? (ActionResult)RedirectToAction("DistanceEdit", new { id = relevantId, logicalName = logicalName, seasonId = seasonId })
                : Json(new { IsSuccess = false, Message = message });
        }

        public ActionResult GetDistanceTable(int id, LogicaName logicalName, int? seasonId)
        {
            var vm = new DistanceTableForm
            {
                RelevantId = id,
                RelevantLogicalName = logicalName,
                Cities = jobsRepo.GetUniqueCitiesForDistanceTable(id, logicalName, seasonId),
                SeasonId = seasonId
            };
            return PartialView("_DistanceTableInstance", vm);
        }

        public void ExportDistanceTable(int id, LogicaName logicalName, int? seasonId)
        {
            var cities = jobsRepo.GetUniqueCitiesForDistanceTable(id, logicalName, seasonId)?.ToList();
            var enitityName = logicalName == LogicaName.Union
                ? unionsRepo.GetById(id)?.Name
                : clubsRepo.GetById(id)?.Name;
            var distanceHelper = new DistanceHelper();

            if (cities.Count > 0)
            {
                using (var workbook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
                {
                    var ws = workbook.AddWorksheet(Messages.DistanceTable);

                    var columnCounter = 1;
                    var rowCounter = 1;
                    var addCell = new Action<string>(value =>
                    {
                        ws.Cell(rowCounter, columnCounter).Value = value;
                        columnCounter++;
                    });


                    for (var row = 0; row <= cities.Count; row++)
                    {
                        for (var column = 0; column <= cities.Count; column++)
                        {
                            if (row == 0)
                            {
                                if (column == 0)
                                {
                                    addCell($"{Messages.City} / {Messages.Distance.ToLowerInvariant()}");
                                    continue;
                                }
                                addCell(cities[column - 1]);
                            }
                            else
                            {
                                if (column == 0)
                                {
                                    addCell(cities[row - 1]);
                                }
                                else
                                {
                                    var city1Name = cities[column - 1];
                                    var city2Name = cities[row - 1];
                                    var distance = distanceHelper.GetDistanceBetweenCities(id, logicalName, seasonId, city1Name, city2Name);
                                    addCell(distance);
                                }
                            }
                        }
                        columnCounter = 1;
                        rowCounter++;
                    }

                    ws.Columns().AdjustToContents();

                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", $"attachment;" +
                        $"filename= {enitityName?.Replace(' ', '_')}_{Messages.DistanceTable.ToLower().Replace(" ", "_")}.xlsx");

                    using (var myMemoryStream = new MemoryStream())
                    {
                        workbook.SaveAs(myMemoryStream);
                        myMemoryStream.WriteTo(Response.OutputStream);
                        Response.Flush();
                        Response.End();
                    }
                }
            }
        }

        public ActionResult ShowReport(int userId, string officialType, int seasonId, int relevantId,
            LogicaName logicalName,
            DateTime? startDate, DateTime? endDate, string distanceSettings, int jobId, bool isSaturdayTariff = false)
        {
            ViewBag.SectionAlias = GetSectionAlias(logicalName, relevantId);
            var user = usersRepo.GetById(userId);
            if (string.IsNullOrEmpty(user?.Address))
                return Json(new { Message = Messages.NoAddressForOfficial });
            //var users = GetWorkerVMByRelevantEntity(relevantId, logicalName, seasonId, null);
            var vm = new SalaryReportForm
            {
                ReportStartDate = (startDate ?? DateTime.Now.AddMonths(-1)).Date,
                ReportEndDate = (endDate ?? DateTime.Now).Date.AddDays(1).AddTicks(-1),
                OfficialType = officialType,
                JobId = jobId
            };
            if (ViewBag.SectionAlias == GamesAlias.Athletics)
            {
                vm.WorkerReportInfo = gamesRepo.GetWorkerReportInfoDetails(userId, relevantId, logicalName,
                    vm.ReportStartDate,
                    vm.ReportEndDate,
                    officialType, seasonId, distanceSettings, jobId);
                vm.WorkerReportInfo = gamesRepo.GetWorkerReportInfo(user, vm.WorkerReportInfo, vm.ReportStartDate,
                    vm.ReportEndDate, distanceSettings, isSaturdayTariff);
            }
            else
                vm.WorkerReportInfo = gamesRepo.GetWorkerGames(userId, relevantId, logicalName,
                    vm.ReportStartDate,
                    vm.ReportEndDate,
                    officialType, seasonId, distanceSettings, jobId, isSaturdayTariff);

            ViewBag.IsReferee = string.Equals(officialType, JobRole.Referee, StringComparison.OrdinalIgnoreCase);
            ViewBag.ReportType = GetReportType(logicalName, relevantId);
            return PartialView("_OfficialsReport", vm);
        }

        [HttpPost]
        public void UpdateOfficialGameReportDetail(int id, int gameId, int? travelDistance, string comment)
        {
            var userJob = db.UsersJobs.Find(id);
            if (gameId == 0)
            {
                if (userJob != null)
                {
                    var reportDetail = userJob.OfficialGameReportDetails
                        .FirstOrDefault(x => x.GameCycleId == null);
                    if (reportDetail == null)
                    {
                        userJob.OfficialGameReportDetails.Add(
                            new OfficialGameReportDetail
                            {
                                TravelDistance = travelDistance,
                                Comment = comment
                            });
                    }
                    else
                    {
                        reportDetail.TravelDistance = travelDistance;
                        reportDetail.Comment = comment;
                    }

                    db.SaveChanges();
                }
            }
            else
            {
                var gameCycle = db.GamesCycles.Find(gameId);

                if (userJob != null && gameCycle != null)
                {
                    var reportDetail = userJob.OfficialGameReportDetails
                        .FirstOrDefault(x => x.GameCycleId == gameId);
                    if (reportDetail == null)
                    {
                        userJob.OfficialGameReportDetails.Add(
                            new OfficialGameReportDetail
                            {
                                GameCycleId = gameId,
                                TravelDistance = travelDistance,
                                Comment = comment
                            });
                    }
                    else
                    {
                        reportDetail.TravelDistance = travelDistance;
                        reportDetail.Comment = comment;
                    }

                    db.SaveChanges();
                }
            }
        }

        private string GetReportType(LogicaName logicalName, int relevantId)
        {
            switch (logicalName)
            {
                case LogicaName.Union:
                    var union = unionsRepo.GetById(relevantId);
                    return union?.ReportSettings;
                case LogicaName.Club:
                    var club = clubsRepo.GetById(relevantId);
                    return club?.ReportSettings;
                default:
                    throw new NotImplementedException();
            }
        }

        private string GetSectionAlias(LogicaName logicalName, int relevantId)
        {
            switch (logicalName)
            {
                case LogicaName.Union:
                    var union = unionsRepo.GetById(relevantId);
                    return union?.Section?.Alias ?? string.Empty;
                case LogicaName.Club:
                    var club = clubsRepo.GetById(relevantId);
                    return club?.Section?.Alias ?? club?.Union?.Section?.Alias ?? string.Empty;
                default:
                    throw new NotImplementedException();
            }
        }

        public ActionResult ExportChoosenJobs(int id, LogicaName logicalName, int seasonId, IEnumerable<int> jobsIds,
            DateTime? startReportDate, DateTime? endReportDate, string distanceSettings, int jobId, bool saveToOfficials, bool isSaturdayTariff = false)
        {
            var workers = jobsRepo.GetWorkersByIds(jobsIds);
            var outputStream = new MemoryStream();
            using (var zip = new ZipFile())
            {
                foreach (var worker in workers)
                {
                    var pdfFile = CreatePdfFile(worker.UserId, startReportDate, endReportDate, worker.OfficialType, id,
                        logicalName, seasonId, distanceSettings, out var fileName, worker.UserJobId, saveToOfficials, false, isSaturdayTariff);

                    if (pdfFile == null)
                    {
                        //report contains no data, skipping
                        continue;
                    }

                    zip.AddEntry(fileName, pdfFile);
                }

                Response.ClearContent();
                Response.ClearHeaders();

                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                zip.Save(outputStream);
            }

            outputStream.Flush();
            outputStream.Position = 0;

            try
            {
                CreateExportFileByBytes(outputStream.ToArray(), $"reports_{DateTime.Now.ToFileTimeUtc()}.zip");
                ViewBag.ExportResult = true;
                ViewBag.FromExcel = false;
                return PartialView("_ExportList");

            }
            catch (Exception ex)
            {
                ViewBag.ExportResult = false;
                ViewBag.FromExcel = false;
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("_ExportList");
            }
        }

        public ActionResult ExportReferees(int id, int relevantId, LogicaName logicalName, int seasonId, DateTime? startReportDate, DateTime? endReportDate, string distanceSettings, List<int> userIds, bool isSaturdayTariff = false)
        {
            MemoryStream exportResult = new MemoryStream();
            startReportDate = (startReportDate ?? DateTime.Now.AddMonths(-1)).Date;
            endReportDate = (endReportDate ?? DateTime.Now).Date.AddDays(1).AddTicks(-1);
            if (logicalName == LogicaName.Union)
            {
                var union = unionsRepo.GetById(id);
                var contentPath = Server.MapPath(GlobVars.ContentPath);
                List<WorkerReportDTO> workerReportInfos = new List<WorkerReportDTO>();
                var users = usersRepo.GetByIds(userIds.ToArray());
                if (union.Section.Name == GamesAlias.Athletics)
                {
                    foreach (var user in users)
                    {
                        var userJob = jobsRepo.GetUserJobDtoItem(user.UserId, getCurrentSeason().Id);
                        WorkerReportDTO workerReportInfo = gamesRepo.GetWorkerReportInfoDetails(user.UserId, relevantId, logicalName,
                            startReportDate,
                            endReportDate,
                            userJob.RoleName, seasonId, distanceSettings, userJob.Id);
                        workerReportInfo = gamesRepo.GetWorkerReportInfo(user, workerReportInfo, startReportDate.Value,
                            endReportDate.Value, distanceSettings, isSaturdayTariff);

                        if (workerReportInfo.GamesCount > 0)
                        {
                            workerReportInfos.Add(workerReportInfo);
                        }
                    }
                }
                else
                {
                    foreach (var user in users)
                    {
                        var userJob = jobsRepo.GetUserJobDtoItem(user.UserId, getCurrentSeason().Id);

                        WorkerReportDTO workerReportInfo = gamesRepo.GetWorkerGames(user.UserId, relevantId, logicalName,
                            startReportDate,
                            endReportDate,
                            userJob.RoleName, seasonId, distanceSettings, userJob.Id, isSaturdayTariff);
                        if (workerReportInfo.GamesCount > 0)
                        {
                            workerReportInfos.Add(workerReportInfo);
                        }
                    }
                }

                ExportRefereesHelper refereeFormPdfHelper = new ExportRefereesHelper(union, startReportDate, endReportDate, seasonId, IsHebrew, contentPath, users, workerReportInfos);
                exportResult = refereeFormPdfHelper.GetDocumentStream();
                exportResult.Flush();
                exportResult.Position = 0;
            }

            try
            {
                CreateExportFileByBytes(exportResult.ToArray(), $"referees_summary{DateTime.Now.ToFileTimeUtc()}.pdf");
                ViewBag.ExportResult = true;
                ViewBag.FromExcel = false;
                return PartialView("_ExportList");
            }
            catch (Exception ex)
            {
                ViewBag.ExportResult = false;
                ViewBag.FromExcel = false;
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("_ExportList");
            }
        }

        public ActionResult ExportRefereesCompetition(int id, int relevantId, LogicaName logicalName, string distanceSettings, List<string> userJobIds, bool isSaturdayTariff = false)
        {
            MemoryStream exportResult = new MemoryStream();
            if (logicalName == LogicaName.League)
            {
                var league = leagueRepo.GetById(id);
                var contentPath = Server.MapPath(GlobVars.ContentPath);
                List<UsersJob> usersJobs = new List<UsersJob>();
                List<UsersJob> usersJobsOnUnionLevel = new List<UsersJob>();
                List<User> users = new List<User>();
                foreach (int userJobId in userJobIds.Select(x => Convert.ToInt32(x)))
                {
                    var userJob = jobsRepo.GetUsersJobById(userJobId);
                    usersJobs.Add(userJob);
                    var usersJobOnUnionLevel = jobsRepo.GetUnionUserJob(userJob.UserId);
                    usersJobsOnUnionLevel.Add(usersJobOnUnionLevel);
                    var user = usersRepo.GetById(userJob.UserId);
                    users.Add(user);
                }

                ExportRefereesCompetitionHelper refereeFormPdfHelper = new ExportRefereesCompetitionHelper(league, IsHebrew, contentPath, users, usersJobs, usersJobsOnUnionLevel);
                exportResult = refereeFormPdfHelper.GetDocumentStream();
                exportResult.Flush();
                exportResult.Position = 0;
            }

            try
            {
                CreateExportFileByBytes(exportResult.ToArray(), $"referees_summary{DateTime.Now.ToFileTimeUtc()}.pdf");
                ViewBag.ExportResult = true;
                ViewBag.FromExcel = false;
                return PartialView("_ExportList");
            }
            catch (Exception ex)
            {
                ViewBag.ExportResult = false;
                ViewBag.FromExcel = false;
                ViewBag.ErrorMessage = ex.Message;
                return PartialView("_ExportList");
            }
        }

        public ActionResult CreatePdf(int userId, string officialType, int seasonId, int relevantId, LogicaName logicalName,
            DateTime? startDate, DateTime? endDate, string distanceSettings, int jobId, bool saveToOfficials, bool isSaturdayTariff = false)
        {
            var usersPdfFile = CreatePdfFile(userId, startDate, endDate, officialType, relevantId, logicalName,
                seasonId, distanceSettings, out var fileName, jobId, saveToOfficials, true, isSaturdayTariff);

            CreateExportFileByBytes(usersPdfFile, fileName);

            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        private byte[] CreatePdfFile(int userId, DateTime? startDate, DateTime? endDate, string officialType,
            int relevantId, LogicaName logicalName,
            int seasonId, string distanceSettings, out string fileName, int userJobId, bool saveToOfficials,
            bool exportEmptyReports, bool isSaturdayTariff = false)
        {
            var sectionAlias = GetSectionAlias(logicalName, relevantId);
            var user = usersRepo.GetById(userId);

            var vm = new SalaryReportForm
            {
                ReportStartDate = (startDate ?? DateTime.Now.AddMonths(-1)).Date, //.Date for start of day, so TimeOfDay=00:00:00
                ReportEndDate = (endDate ?? DateTime.Now).Date.AddDays(1).AddTicks(-1), //End of day
                OfficialType = LangHelper.GetJobName(officialType)?.ToLower() ?? ""
            };

            if (sectionAlias == GamesAlias.Athletics)
            {
                vm.WorkerReportInfo = gamesRepo.GetWorkerReportInfoDetails(userId, relevantId, logicalName,
                    vm.ReportStartDate,
                    vm.ReportEndDate,
                    officialType, seasonId, distanceSettings, userJobId);
                vm.WorkerReportInfo = gamesRepo.GetWorkerReportInfo(user, vm.WorkerReportInfo, vm.ReportStartDate, vm.ReportEndDate, distanceSettings, isSaturdayTariff);
            }
            else
            {
                vm.WorkerReportInfo = gamesRepo.GetWorkerGames(userId, relevantId, logicalName,
                    vm.ReportStartDate,
                    vm.ReportEndDate,
                    LangHelper.GetOfficialName(officialType), seasonId, distanceSettings, userJobId, isSaturdayTariff);
            }

            if (!exportEmptyReports && vm.WorkerReportInfo.GamesAssigned?.Any() != true)
            {
                //report contains no data, skipping
                fileName = null;
                return null;
            }

            fileName = $"Report_{DateTime.Now.ToFileTimeUtc()}.pdf";

            var exportHelper = new ExportReportToPdfHelper();

            var widthFormat = new Rectangle(PageSize.A4.Rotate());
            var stream = new MemoryStream();
            var document = new Document(widthFormat);
            var writer = PdfWriter.GetInstance(document, stream);

            writer.CloseStream = false;

            document.Open();

            var isHebrew = IsHebrew;
            var isReferee = string.Equals(officialType, JobRole.Referee, StringComparison.OrdinalIgnoreCase);
            var reportType = GetReportType(logicalName, relevantId);

            exportHelper.CreateTable(document, vm, isHebrew, isReferee, sectionAlias, reportType);

            document.Close();

            stream.Flush();
            stream.Position = 0;

            if (saveToOfficials && vm.WorkerReportInfo.GamesAssigned?.Any() == true)
            {
                var personalReportFileName = FileUtil.SaveSalaryReportFile(stream, userId);
                var existingRefereeSalaryReport = user.RefereeSalaryReports
                    .FirstOrDefault(x => x.SeasonId == seasonId &&
                                         x.StartDate == vm.ReportStartDate &&
                                         x.EndDate == vm.ReportEndDate);
                if (existingRefereeSalaryReport == null)
                {
                    user.RefereeSalaryReports.Add(new RefereeSalaryReport
                    {
                        SeasonId = seasonId,
                        StartDate = vm.ReportStartDate,
                        EndDate = vm.ReportEndDate,
                        FileName = personalReportFileName
                    });
                }
                else
                {
                    existingRefereeSalaryReport.FileName = personalReportFileName;
                }

                db.SaveChanges();
            }

            return stream.ToArray();
        }

        public void CreateExportFileByBytes(byte[] resultFileBytes, string fileName)
        {
            Session.Remove(ExportPlayersResultSessionKey);
            Session.Remove(ExportPlayersResultFileNameSessionKey);
            Session.Add(ExportPlayersResultSessionKey, resultFileBytes);
            Session.Add(ExportPlayersResultFileNameSessionKey, fileName);
        }

        public void ChangeDistanceSettings(int id, LogicaName logicalName, string type)
        {
            switch (logicalName)
            {
                case LogicaName.Union:
                    unionsRepo.ChangeDistanceSettings(id, type);
                    break;
                case LogicaName.Club:
                    clubsRepo.ChangeDistanceSettings(id, type);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void ChangeReportSettings(int id, LogicaName logicalName, string type, bool removeTravel)
        {
            switch (logicalName)
            {
                case LogicaName.Union:
                    unionsRepo.ChangeReportSettings(id, type, removeTravel);
                    break;
                case LogicaName.Club:
                    clubsRepo.ChangeReportSettings(id, type, removeTravel);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }


        public void ChangeTariffSettings(int id, LogicaName logicalName, bool enableTariff, double? fromTime, double? toTime)
        {
            DateTime? fromTimeD = null;
            DateTime? toTimeD = null;
            if (fromTime.HasValue)
            {
                fromTimeD = (new DateTime(1970, 1, 1)).AddMilliseconds(fromTime.Value);
            }
            if (toTime.HasValue)
            {
                toTimeD = (new DateTime(1970, 1, 1)).AddMilliseconds(toTime.Value);
            }
            switch (logicalName)
            {
                case LogicaName.Union:
                    unionsRepo.ChangeTariffSettings(id, enableTariff, fromTimeD, toTimeD);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        [HttpPost]
        public void BlockOfficial(int id, bool isBlocked)
        {
            jobsRepo.ChangeBlockStatus(id, isBlocked);
            jobsRepo.Save();
        }

        private KarateRefereeRanksHelper GetRefereesRanksHelperInstance()
        {
            return new KarateRefereeRanksHelper();
        }

        [HttpPost]
        public ActionResult UpdateUsersJobActiveStatus(int id)
        {
            jobsRepo.ChangeActiveStatus(id);
            jobsRepo.Save();

            return Json(new { Message = Messages.ActiveStatusChanged});            
        }
    }
}