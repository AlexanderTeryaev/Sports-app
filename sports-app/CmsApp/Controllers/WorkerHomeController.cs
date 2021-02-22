using System;
using CmsApp.Models;
using DataService;
using Resources;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AppModel;
using DataService.DTO;
using CmsApp.Helpers;
using System.Security.Policy;
using System.Web;
using Microsoft.Ajax.Utilities;

namespace CmsApp.Controllers
{
    public class WorkerHomeController : AdminController
    {
        private DepartmentRepo departmentRepo;

        public WorkerHomeController()
        {
            this.departmentRepo = new DepartmentRepo();
        }
        // GET: WorkerHome
        public ActionResult Index()
        {
            playersRepo.CheckForExclusions();
            //var isMultisection = usersRepo.IsMultiSectionPlayer(AdminId); //TODO: Perfomance
            var hasMultipleJobs = usersRepo.CheckIfHasMultipleJobs(AdminId);
            var topJobName = usersRepo.IfHasSingleJobGetName(AdminId);
            var roleName = hasMultipleJobs ? JobRole.Multiple : usersRepo.GetTopLevelJob(AdminId);

            var tree = new List<ManagedItemViewModel>();

            ViewBag.UserId = AdminId;

            switch (roleName)
            {
                case JobRole.UnionCoach:
                    ViewBag.Title = "Union coach";
                    var userUnion = unionsRepo.GetByManagerId(AdminId);
                    if (userUnion.Count == 1)
                    {
                        var seasons = userUnion.First().Seasons.Where(x => x.IsActive).ToList();
                        var season = 1;
                        if (seasons.Count > 0)
                        {
                            season = seasons.Last().Id;
                        }

                        var unblockaded =
                            GetAllUnblockadedPlayers(userUnion.FirstOrDefault().UnionId, season); //TODO: Perfomance
                        Session["UnblockadedPlayers"] = unblockaded;

                        SetUnionCurrentSeason(season);


                        return RedirectToAction("Index", "Coach", new { id = AdminId, seasonId = season});
                    }
                    
                    break;
                case JobRole.CommitteeOfReferees:
                    ViewBag.Title = "Referees";
                    var userUnions = unionsRepo.GetByManagerId(AdminId);
                    if (userUnions.Count == 1)
                    {
                        var seasons = userUnions.First().Seasons.Where(x => x.IsActive).ToList();
                        var season = 1;
                        if (seasons.Count > 0)
                        {
                            season = seasons.Last().Id;
                        }

                        var unblockaded =
                            GetAllUnblockadedPlayers(userUnions.FirstOrDefault().UnionId, season); //TODO: Perfomance
                        Session["UnblockadedPlayers"] = unblockaded;

                        SetUnionCurrentSeason(season);

                        return RedirectToAction("EditReferees", "Unions", new { id = userUnions.First().UnionId, seasonId = season });
                    }
                    break;

                case JobRole.ClubSecretary:
                case JobRole.ClubManager:
                    var clubIds = clubsRepo.GetByManagerId(AdminId); //TODO: add relationship to db & appmodel
                    var clubs = clubsRepo.GetCollection<Club>(x => clubIds.Contains(x.ClubId)).ToList();
                    var c_user = usersRepo.GetById(AdminId);
                    var c_currentSeasons = c_user.TeamsPlayers.Where(x => x.Season?.StartDate <= DateTime.Now && x.Season?.EndDate > DateTime.Now).ToList();
                    var c_latestSeason = c_currentSeasons.FirstOrDefault(x => x.SeasonId == c_currentSeasons.Max(s => s.SeasonId));
                    
                    ViewBag.IsPlayer = User.IsInAnyRole(AppRole.Players);
                    ViewBag.LatestSeasonId = c_latestSeason?.SeasonId;
                    if (ViewBag.LatestSeasonId == null || ViewBag.LatestSeasonId == 0)
                    {
                        var club = clubs.FirstOrDefault();
                        var clubSeason = club?.UnionId != null
                            ? club.Union.Seasons.Where(x => x.IsActive).OrderBy(x => x.Id).LastOrDefault()
                            : club?.Seasons.Where(x => x.IsActive).OrderBy(x => x.Id).LastOrDefault();
                        ViewBag.LatestSeasonId = clubSeason?.Id;
                    }
                    if (clubs.Count == 1)
                    {
                        var club = clubs.FirstOrDefault();

                        var clubSeason = club?.UnionId != null
                            ? club.Union.Seasons.Where(x => x.IsActive).OrderBy(x => x.Id).LastOrDefault()
                            : club?.Seasons.Where(x => x.IsActive).OrderBy(x => x.Id).LastOrDefault();

                        if (club?.UnionId == null)
                        {
                            SetIsSectionClubLevel(true);
                            SetClubCurrentSeason(clubSeason?.Id ?? 0);
                        }
                        else
                        {
                            SetIsSectionClubLevel(false);
                            SetUnionCurrentSeason(clubSeason?.Id ?? 0);
                        }
                        var lastSeasonId = seasonsRepository.GetLastClubSeason();
                        TempData["KarateClubMessage"] = club != null && club.UnionId.HasValue && club.UnionId == 37
                            ? FormMessageForKarateClub(club.ClubId, club.SeasonId ?? 0)
                            : string.Empty;
                        return RedirectToAction("Edit", "Clubs",
                            new
                            {
                                id = club?.ClubId,
                                seasonId = clubSeason?.Id,
                                sectionId = club?.SectionId,
                                unionId = club?.UnionId,
                                showAlerts = true
                            });
                    }
                    ViewBag.IsMultiple = true;
                    var c_pageHelper = new PageHelper();
                    tree.AddRange(clubs.Select(x =>
                    {
                        var season = x.Seasons.FirstOrDefault(s => s.Id == x.SeasonId) ??
                                     x.Seasons.OrderBy(s => s.Id).LastOrDefault() ??
                                     x.Union?.Seasons?.FirstOrDefault(s => s.Id == x.SeasonId);

                        return new ManagedItemViewModel
                        {
                            Id = x.ClubId,
                            Name = x.Name,
                            Controller = "Clubs",
                            Url = c_pageHelper.GenerateUrl(roleName, x.Union?.Section.Alias, x.UnionId, x.ClubId, null,
                                null, null, season?.Id, null, AdminId),
                            SeasonId = season?.Id,
                            SeasonName = season?.Name,
                            SeasonEndDate = season?.EndDate,
                            ClubId = x.ClubId,
                            UnionId = x.UnionId,
                            ClubName = x.Name,
                            UnionName = x.Union?.Name,
                            JobName = topJobName,
                            KarateClubMessage = x.UnionId.HasValue && x.UnionId.Value == 37
                                ? FormMessageForKarateClub(x.ClubId, x.SeasonId ?? 0)
                                : string.Empty
                        };
                    }));
                    tree = tree.OrderByDescending(x => x.SeasonEndDate).ToList();
                    break;

                case JobRole.UnionManager:
                case JobRole.Unionviewer:
                case JobRole.RefereeAssignment:

                    ViewBag.Title = Messages.Unions;

                    SetIsSectionClubLevel(false);

                    var unions = unionsRepo.GetByManagerId(AdminId);
                    if (unions.Count == 1)
                    {
                        var seasons = unions.First().Seasons.Where(x => x.IsActive).ToList();
                        var season = 1;
                        if (seasons.Count > 0)
                        {
                            season = seasons.Last().Id;
                        }

                        var unblockaded =
                            GetAllUnblockadedPlayers(unions.FirstOrDefault().UnionId, season); //TODO: Perfomance
                        Session["UnblockadedPlayers"] = unblockaded;

                        SetUnionCurrentSeason(season);

                        return RedirectToAction("Edit", "Unions", new {id = unions.First().UnionId, seasonId = season});
                    }


                    tree.AddRange(unions.Select(item => new ManagedItemViewModel
                    {
                        Id = item.UnionId,
                        Name = item.Name,
                        Controller = "Unions",
                        SeasonId = item.Seasons.Last().Id,
                        Unblockaded = GetAllUnblockadedPlayers(item.UnionId, item.Seasons.LastOrDefault().Id)
                    }));

                    break;

                case JobRole.DisciplineManager:
                    ViewBag.Title = Messages.Disciplines;
                    var disciplines = disciplinesRepo.GetByManagerId(AdminId);
                    if (disciplines.Count == 1)
                    {
                        return RedirectToAction("Edit", "Disciplines", new {id = disciplines.First().DisciplineId});
                    }

                    tree.AddRange(disciplines.Select(item => new ManagedItemViewModel
                    {
                        Id = item.DisciplineId,
                        Name = item.Name,
                        Controller = "Disciplines"
                    }));
                    break;

                case JobRole.DepartmentManager:
                    var departmentIds =
                        departmentRepo.GetByManagerId(AdminId); //TODO: add relationship to db & appmodel
                    var departments = clubsRepo.GetCollection<Club>(x => departmentIds.Contains(x.ClubId)).ToList();

                    if (departments.Count == 1)
                    {
                        var department = departments.First();

                        var departmentSeason = seasonsRepository.GetLastSeasonByCurrentClub(department.ParentClub);

                        if (department.UnionId == null)
                        {
                            SetIsSectionClubLevel(true);
                            SetClubCurrentSeason(departmentSeason?.Id ?? 0);
                        }
                        else
                        {
                            SetIsSectionClubLevel(false);
                            SetUnionCurrentSeason(departmentSeason?.Id ?? 0);
                        }

                        return RedirectToAction("Edit", "Clubs",
                            new
                            {
                                id = department.ClubId,
                                seasonId = departmentSeason?.Id,
                                unionId = department.UnionId,
                                isDepartment = true
                            });
                    }

                    tree.AddRange(departments.Select(x => new ManagedItemViewModel
                    {
                        Id = x.ClubId,
                        Name = x.Name,
                        Controller = "Clubs",
                        SeasonId = x.Seasons.OrderBy(s => s.Id).LastOrDefault()?.Id,
                        ClubId = x.ClubId,
                        UnionId = x.UnionId
                    }));

                    break;

                case JobRole.LeagueManager:
                    ViewBag.Title = Messages.Leagues;

                    SetIsSectionClubLevel(false);

                    var leagues = leagueRepo.GetByManagerId(AdminId, null);
                    if (leagues.Count == 1)
                    {
                        var league = leagues.First();
                        var season = league.Season;

                        var section = season?.Union?.Section ??
                                      season?.Club?.Section;

                        var isTennis = string.Equals(section?.Alias, SectionAliases.Tennis,
                            StringComparison.CurrentCultureIgnoreCase);

                        SetUnionCurrentSeason(season?.Id ?? 0);

                        return RedirectToAction("Edit", "Leagues",
                            new
                            {
                                id = league.LeagueId,
                                seasonId = season?.Id ?? 0,
                                isTennisCompetition = isTennis && league.EilatTournament == true ? 1 : 0
                            });
                    }

                    tree.AddRange(leagues.Select(item => new ManagedItemViewModel
                    {
                        Id = item.LeagueId,
                        Name = item.Name,
                        SeasonId = item.SeasonId,
                        SeasonEndDate = seasonsRepository.GetById(item.SeasonId ?? 0)?.EndDate,
                        SeasonName = item.Season?.Name,
                        Controller = "Leagues"
                    }));
                    tree = tree.OrderByDescending(x => x.SeasonEndDate).ToList();
                    break;

                case JobRole.TeamManager:
                case JobRole.TeamViewer:
                    ViewBag.Title = Messages.Teams;
                    var teamManagers = teamRepo.GetByTeamManagerId(AdminId);
                    if (teamManagers.Count == 1)
                    {
                        var teamManager = teamManagers.First();

                        if (teamManager.LeagueId != null)
                        {
                            SetIsSectionClubLevel(false);
                            SetUnionCurrentSeason(teamManager.SeasonId ?? 0);

                            return RedirectToAction("Edit", "Teams",
                                new
                                {
                                    id = teamManager.TeamId,
                                    currentLeagueId = teamManager.LeagueId,
                                    seasonId = teamManager.SeasonId,
                                    unionId = teamManager.UnionId
                                });
                        }

                        if (teamManager.ClubId != null)
                        {
                            var teamManagerClub = clubsRepo.GetById(teamManager.ClubId ?? 0);
                            if (teamManagerClub != null && teamManagerClub.UnionId == null)
                            {
                                SetIsSectionClubLevel(true);
                                SetClubCurrentSeason(teamManager.SeasonId ?? 0);
                            }
                            else if (teamManagerClub != null)
                            {
                                SetIsSectionClubLevel(false);
                                SetUnionCurrentSeason(teamManager.SeasonId ?? 0);
                                var unblockaded = GetAllUnblockadedPlayers(teamManagerClub.UnionId.Value, teamManagerClub.SeasonId.Value, teamManagerClub.ClubId, teamManager.TeamId, BlockadeType.MedicalExpiration); //TODO: Perfomance
                                Session["UnblockadedPlayers"] = unblockaded;
                            }

                            return RedirectToAction("Edit", "Teams",
                                new
                                {
                                    id = teamManager.TeamId,
                                    seasonId = teamManager.SeasonId,
                                    unionId = teamManager.UnionId,
                                    clubId = teamManager.ClubId.Value
                                });
                        }
                    }

                    var items = teamManagers.Select(teamManager => new {teamManager, teamId = teamManager.TeamId})
                        .Where(t => t.teamId != null && (t.teamManager.LeagueId != null || t.teamManager.ClubId != null))
                        .Select(t => new ManagedItemViewModel
                        {
                            Id = t.teamId.Value,
                            Name = t.teamManager.Title,
                            LeagueName = t.teamManager.LeagueName,
                            Controller = "Teams",
                            SeasonId = t.teamManager.SeasonId,
                            SeasonEndDate = seasonsRepository.GetById(t.teamManager.SeasonId ?? 0)?.EndDate,
                            LeagueId = t.teamManager.LeagueId,
                            UnionId = t.teamManager.UnionId,
                            ClubId = t.teamManager.ClubId
                        });
                    tree.AddRange(items);
                    tree = tree.OrderByDescending(x => x.SeasonEndDate).ToList();
                    break;

                case JobRole.Referee:
                case JobRole.Activityviewer:
                case JobRole.Activitymanager:
                case JobRole.ActivityRegistrationActive:

                    var activitiesAllowed = jobsRepo.IsActivityManager(AdminId) ||
                                            jobsRepo.IsActivityViewer(AdminId) ||
                                            jobsRepo.IsActivityRegistrationActive(AdminId);
                    int userId;
                    if (int.TryParse(User.Identity.Name, out userId) && activitiesAllowed)
                    {
                        var userJobs = jobsRepo.GetUsersJobCollection(x => x.UserId == userId &&
                                                                           (x.Job.JobsRole.RoleName ==
                                                                            JobRole.Activitymanager ||
                                                                            x.Job.JobsRole.RoleName ==
                                                                            JobRole.Activityviewer ||
                                                                            x.Job.JobsRole.RoleName ==
                                                                            JobRole.ActivityRegistrationActive ||
                                                                            x.Job.JobsRole.RoleName ==
                                                                            JobRole.Referee &&
                                                                            x.Season.IsActive)
                        );

                        //var applicableSeasons =
                        //    userJobs.Where(x => x.Season?.StartDate <= DateTime.Now && x.Season?.EndDate > DateTime.Now);

                        //var lastSeason = applicableSeasons.OrderBy(x => x.SeasonId).LastOrDefault();
                        var lastSeason = userJobs.OrderBy(x => x.SeasonId).LastOrDefault();
                        if (lastSeason != null)
                        {
                            return RedirectToAction("ActivityList", "Activity", new {seasonId = lastSeason.SeasonId});
                        }
                    }

                    if (activitiesAllowed)
                    {
                        return RedirectToAction("ActivityList", "Activity", new { });
                    }

                    if (roleName == JobRole.Referee)
                    {
                        var tennisRefereeView = GetTennisRefereeLeaguesView();
                        if (tennisRefereeView != null)
                        {
                            return tennisRefereeView;
                        }
                    }

                    break;
                case JobRole.RegionalManager:
                    ViewBag.Title = Messages.RegionalList;
                    var myJobs = db.UsersJobs.Where(u => u.UserId == AdminId && u.RegionalId != null);
                    if (myJobs.Count() == 1)
                    {
                        int regionalid = myJobs.FirstOrDefault().RegionalId.GetValueOrDefault(0);
                        var regional = regionalsRepo.GetById(regionalid);

                        return RedirectToAction("Edit", "Regional", new { id = regionalid, seasonId = regional.SeasonId, unionId = regional.UnionId });
                    }
                    else if (myJobs.Count() > 1)
                    {
                        int regionalid = myJobs.FirstOrDefault().RegionalId.GetValueOrDefault(0);
                        var regional = regionalsRepo.GetById(regionalid);

                        return RedirectToAction("Edit", "Regional", new { id = regionalid, seasonId = regional.SeasonId, unionId = regional.UnionId });
                    }
                    break;
                case JobRole.CallRoomManager:
                    var job = db.UsersJobs.Where(u => u.UserId == AdminId && u.LeagueId.HasValue && u.Job.JobsRole.RoleName == JobRole.CallRoomManager);
                    var oneJob = job.FirstOrDefault();
                    return RedirectToAction("Edit", "Leagues", new { id = oneJob.LeagueId, seasonId = oneJob.SeasonId });
                    break;
                case JobRole.Multiple:
                    var jobs = jobsRepo.GetAllUsersJobs(AdminId).ToList();
                    var pageHelper = new PageHelper();
                    tree.AddRange(jobs.Where(x => x != null && x.Season?.IsActive == true && (!x.LeagueId.HasValue || !x.League.IsArchive) && (!x.ClubId.HasValue || !x.Club.IsArchive)).Select(j =>
                    {
                        var section = j.Union?.Section 
                                      ?? j.Club?.Union?.Section
                                      ?? j.Club?.Section 
                                      ?? j.League?.Club?.Union?.Section
                                      ?? j.League?.Club?.Union?.Section
                                      ?? j.Job?.Section;

                        Regional region = null;
                        if (j.RegionalId > 0)
                        {
                            region = db.Regionals.Find(j.RegionalId);
                        }

                        return new ManagedItemViewModel
                        {
                            Url = pageHelper.GenerateUrl(j.Job?.JobsRole?.RoleName, section?.Alias, j.UnionId, j.ClubId, j.LeagueId,
                                j.DisciplineId, j.TeamId, j.SeasonId, region, AdminId),

                            LeagueName = j.League?.Name
                                         ?? string.Join(", ", j.Team?.LeagueTeams.Where(x => x.SeasonId == j.SeasonId)
                                                                  .Select(x => x.Leagues.Name)
                                                              ?? new List<string>()),
                            LeagueStartDate = j.League != null && j.League.LeagueStartDate != null ? j.League.LeagueStartDate.Value.ToString("dd/mm/yyyy") : string.Empty,
                            LeagueEndDate   = j.League != null && j.League.LeagueEndDate != null   ? j.League.LeagueEndDate.Value.ToString("dd/mm/yyyy")   : string.Empty,
                            LeagueId = j.LeagueId,
                            SeasonEndDate = j.Season.EndDate,
                            TeamName =
                                j.Team?.TeamsDetails?.OrderByDescending(c => c.SeasonId).FirstOrDefault()?.TeamName ??
                                j.Team?.Title,
                            TeamId = j.TeamId,

                            UnionName = j.Union?.Name ??
                                        j.Club?.Union?.Name ??
                                        j.League?.Union?.Name ??
                                        j.Discipline?.Union?.Name ??
                                        region?.Union?.Name ??
                                        string.Join(", ",
                                            j.Team?.LeagueTeams.Where(x => x.SeasonId == j.SeasonId)
                                                .Select(x => x.Leagues.Union.Name) ?? new List<string>()),

                            RegionName = region?.Name,

                            SeasonId = j.Season?.Id,
                            SeasonName = j.Season?.Name,
                            ClubName = j.Club?.Name,
                            JobName = j?.Job?.JobName,
                            JobRole = j?.Job?.JobsRole?.RoleName,

                            Section = section,
                            SectionName = section?.Name,

                            KarateClubMessage =
                                j.ClubId.HasValue
                                    ? FormMessageForKarateClub(j.ClubId.Value, j.Club.SeasonId ?? 0)
                                    : string.Empty
                        };
                    }));

                    db.SaveChanges();

                    var user = usersRepo.GetById(AdminId);
                    var currentSeasons = user.TeamsPlayers.Where(x => x.Season?.StartDate <= DateTime.Now && x.Season?.EndDate > DateTime.Now).ToList();
                    var latestSeason = currentSeasons.FirstOrDefault(x => x.SeasonId == currentSeasons.Max(s => s.SeasonId));

                    ViewBag.IsPlayer = User.IsInAnyRole(AppRole.Players);
                    ViewBag.LatestSeasonId = latestSeason?.SeasonId;
                    ViewBag.IsMultiple = true;

                    var jobsToRemove = tree.Where(x =>
                            x.JobRole == JobRole.TeamManager ||
                            (x.Section?.Alias == SectionAliases.Netball && x.JobRole == JobRole.LeagueManager))
                        .ToList();
                    if (jobsToRemove.Any())
                    {
                        var jobsToRemoveUnions = jobsToRemove.GroupBy(x => x.UnionName);

                        foreach (var unionGroup in jobsToRemoveUnions)
                        {
                            var groupLatestSeason = unionGroup.Max(x => x.SeasonId);

                            foreach (var jobToRemove in jobsToRemove.Where(x =>
                                x.UnionName == unionGroup.Key &&
                                x.SeasonId != groupLatestSeason))
                            {
                                tree.Remove(jobToRemove);
                            }
                        }
                    }

                    if (tree.All(x =>
                        string.Equals(x.SectionName, SectionAliases.Tennis, StringComparison.CurrentCultureIgnoreCase) &&
                        string.Equals(x.JobRole, JobRole.Referee)))
                    {
                        var tennisRefereeView = GetTennisRefereeLeaguesView();
                        if (tennisRefereeView != null)
                        {
                            return tennisRefereeView;
                        }
                    }

                    //if (tree.Any())
                    //{
                    //    var groupedByUnion = tree.GroupBy(x => x.UnionName);
                    //    foreach (var group in groupedByUnion)
                    //    {
                    //        var groupLatestSeason = group.Max(x => x.SeasonId);

                    //        var notLastSeasonJobs = group.Where(x => x.SeasonId != groupLatestSeason);

                    //        notLastSeasonJobs.ForEach(x => tree.Remove(x));
                    //    }
                    //}
                    tree = tree.OrderByDescending(x => x.SeasonEndDate).ToList();
                    if (ViewBag.LatestSeasonId == null)
                    {
                        ViewBag.LatestSeasonId = tree.FirstOrDefault()?.SeasonId;
                    }
                    break;
                
            }

            return View(tree);
        }

        private ActionResult GetTennisRefereeLeaguesView()
        {
            var tennisLeagues = jobsRepo.GetAllTennisLeagues(AdminId);
            if (tennisLeagues.Count > 0)
            {
                if (tennisLeagues.Count == 1)
                {
                    var league = tennisLeagues.First();
                    return RedirectToAction("Edit", "Leagues", new
                    {
                        id = league.LeagueId,
                        seasonId = league.SeasonId
                    });
                }

                return View("RefereeLeagues", tennisLeagues);
            }

            return null;
        }

        public ActionResult SalaryReports(int? userId = null)
        {
            var result = new RefereeSalaryReportModel
            {
                Reports = new List<RefereeSalaryReportItemModel>(),
                CanRemoveReports = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager)
            };

            if (!userId.HasValue || userId <= 0)
            {
                return PartialView("_SalaryReports", result);
            }

            var user = usersRepo.GetById(userId.Value);
            if (user != null)
            {
                var userReports = db.RefereeSalaryReports
                    .Where(x => x.UserId == user.UserId)
                    .Include(x => x.Season)
                    .AsNoTracking()
                    .ToList();

                result.Reports.AddRange(userReports.Select(x => new RefereeSalaryReportItemModel
                {
                    Id = x.Id,
                    Name = user.FullName,
                    SeasonName = x.Season.Name,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate
                }));
            }

            return PartialView("_SalaryReports", result);
        }

        [HttpPost]
        public void RemoveSalaryReport(int id)
        {
            var actionPermitted = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager);
            if (!actionPermitted)
            {
                return;
            }

            var report = db.RefereeSalaryReports.Find(id);
            if (report == null)
            {
                return;
            }

            db.RefereeSalaryReports.Remove(report);
            db.SaveChanges();
        }

        public ActionResult DownloadSalaryReport(int id)
        {
            var report = db.RefereeSalaryReports.Find(id);

            if (report != null)
            {
                var filePath = HttpContext.Server.MapPath($"{GlobVars.ContentPath}/salaryreports/{report.FileName}");

                if (System.IO.File.Exists(filePath))
                {
                    return File(filePath, MimeMapping.GetMimeMapping(report.FileName), report.FileName);
                }
            }

            return new EmptyResult();
        }

        private string FormMessageForKarateClub(int clubId, int seasonId)
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
                        message = GetClubPaymentMessage(unionPayments, clubApprovedPlayersCount, club.Name);
                    }
                }
            }
            return message;
        }

        private string GetClubPaymentMessage(ICollection<KarateUnionPayment> unionPayments, int clubApprovedPlayersCount, string clubName)
        {
            string message = string.Empty;
            var necessaryPaymentType = unionPayments.FirstOrDefault(p => p.FromNumber <= clubApprovedPlayersCount && p.ToNumber >= clubApprovedPlayersCount);

            var isShown = db.DisplayedPaymentMessages.FirstOrDefault(c => c.PaymentId == necessaryPaymentType.Id && c.UserId == AdminId) != null;

            if (necessaryPaymentType != null && !isShown)
            {
                if (necessaryPaymentType != null) { unionsRepo.SetIsShownPaymentStatus(necessaryPaymentType.Id, AdminId); }
                if (clubApprovedPlayersCount == necessaryPaymentType.ToNumber)
                {
                    var nextValue = unionPayments.OrderBy(c => c.FromNumber).FirstOrDefault(c => c.FromNumber >= clubApprovedPlayersCount);
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

        private IList<PlayersBlockadeShortDTO> GetAllUnblockadedPlayers(int unionId, int seasonId, int? clubId = null, int? teamId = null, int BType = BlockadeType.All)
        {
            IList<PlayersBlockadeShortDTO> unionsBlockades = playersRepo.GetAllBlockadedPlayersForUnion(unionId, seasonId, clubId, teamId, BType);
            IList<PlayersBlockadeShortDTO> unblockaded = new List<PlayersBlockadeShortDTO>();
            if (unionsBlockades.Count > 0)
            {
                foreach (var unionBlockade in unionsBlockades)
                {
                    bool isShown = playersRepo.CheckIfBlockadeWasShown(AdminId, unionBlockade.BlockadeId);
                    if (!isShown)
                    {
                        playersRepo.AddBlockadeNotification(AdminId, unionBlockade.BlockadeId);
                        unblockaded.Add(unionBlockade);
                    }
                }
            }
            return unblockaded;
        }

    }
}