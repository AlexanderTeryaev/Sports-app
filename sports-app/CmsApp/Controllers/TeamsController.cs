using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Resources;
using Omu.ValueInjecter;
using CmsApp.Models;
using DataService;
using AppModel;
using ClosedXML.Excel;
using CmsApp.Helpers;
using CmsApp.Models.Mappers;
using DataService.Services;
using DataService.Utils;
using DataService.DTO;
using CmsApp.Services;
using System.Threading.Tasks;
using log4net;
using Microsoft.Ajax.Utilities;
using System.Data.Entity;
using System.Text;
using System.Configuration;

namespace CmsApp.Controllers
{
    public class TeamsController : AdminController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TeamsController));
        private TrainingSettingsRepo objTrainingSettingsRepo = new TrainingSettingsRepo();
        private TeamTrainingsRepo _objTeamTrainingsRepo = new TeamTrainingsRepo();
        private ClubTeamTrainingsRepo _objClubTrainingsRepo = new ClubTeamTrainingsRepo();

        public ActionResult Edit(int id, int? seasonId, int? unionId, int? currentLeagueId, int? clubId,
            int? departmentId = null, int? departmentSeasonId = null, int? departmentSportId = null, int isTennisCompetition = 0, string roleType = null)
        {
            if (!string.IsNullOrWhiteSpace(roleType))
            {
                this.SetWorkerSession(roleType);
            }
            playersRepo.CheckForExclusions();
            if (seasonId != null)
            {
                if (unionId == null)
                {
                    SetIsSectionClubLevel(true);
                    SetClubCurrentSeason(seasonId.Value);
                }
                else
                {
                    SetIsSectionClubLevel(false);
                    SetUnionCurrentSeason(seasonId.Value);
                }
            }

            var roleName = usersRepo.GetTopLevelJob(AdminId);
            var clubTeams = new List<int>();
            if (roleName == JobRole.ClubManager || roleName == JobRole.ClubSecretary)
            {
                var managerClubId = jobsRepo.GetClubIdByClubManagerId(AdminId);
                ViewBag.IsClubManager = managerClubId.HasValue && jobsRepo.IsClubManager(AdminId, managerClubId.Value);

                var managerInClub = clubsRepo.GetById(managerClubId ?? 0);

                foreach (var clubTeam in managerInClub.ClubTeams)
                {
                    clubTeams.Add(clubTeam.TeamId);
                }

                foreach (var school in managerInClub.Schools)
                {
                    foreach (var schoolTeam in school.SchoolTeams)
                    {
                        clubTeams.Add(schoolTeam.TeamId);
                    }
                }
            }
            else if (roleName == JobRole.TeamManager)
            {
                ViewBag.IsTeamManager = jobsRepo.IsTeamManager(AdminId, id);
            }

            var isHighLevelAdmin = User.IsInRole(AppRole.Admins) || roleName == JobRole.UnionManager;
            //if (User.IsInRole(AppRole.Workers) &&
            //    !clubTeams.Contains(id) &&
            //    !AuthSvc.AuthorizeTeamByIdAndManagerId(id, base.AdminId))
            //{
            //    return RedirectToAction("Index", "NotAuthorized");
            //}

            if (!seasonId.HasValue)
            {
                seasonId = teamRepo.GetSeasonIdByTeamId(id);
            }
            var team = teamRepo.GetById(id, seasonId);


            var season = seasonsRepository.GetById(seasonId ?? 0);
            ViewBag.IsUnionConnected = true;
            if(season != null && !season.UnionId.HasValue)
            {
                ViewBag.IsUnionConnected = false;
            }

            if (team.IsArchive)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var userLeagues = AuthSvc.FindLeaguesByTeamAndManagerId(id, base.AdminId);

            var isDepartmentTeam = false;
            if (clubId.HasValue && !departmentId.HasValue)
            {
                isDepartmentTeam = clubsRepo.GetById(clubId.Value).ParentClub != null;
            }
            else if (departmentId.HasValue)
            {
                isDepartmentTeam = true;
            }
            var clubName = "";
            if (clubId.HasValue)
            {
                clubName = clubsRepo.GetById(clubId.Value)?.Name;
            }
            if (departmentId.HasValue)
            {
                clubName = clubsRepo.GetById(departmentId.Value)?.Name;
                ViewBag.DepartmentSeasonId = departmentSeasonId;
                ViewBag.DepartmentSportId = departmentSportId;
            }

            var teamLeagues = leagueRepo.GetByTeamAndSeasonShort(id, seasonId ?? 0);
            var section = secRepo.GetSectionByTeamId(id);
            var club = clubId.HasValue ? clubsRepo.GetById(clubId.Value) : null;
            var league = currentLeagueId.HasValue ? leagueRepo.GetById(currentLeagueId.Value) : null;
            var isClubUnderUnionManager = (usersRepo.GetTopLevelJob(AdminId)?.Equals(JobRole.ClubManager) == true
                || usersRepo.GetTopLevelJob(AdminId)?.Equals(JobRole.ClubSecretary) == true)
                && (club?.UnionId.HasValue == true || league?.UnionId.HasValue == true || league?.Club?.IsUnionClub == true);

            var validTeamRegistrationsThisSeason = team.TeamRegistrations.Where(r => !r.IsDeleted && !r.League.IsArchive && r.SeasonId == seasonId ).ToList();
            var isPlayerRegistrationClosed = false;
            if (!isHighLevelAdmin)
            {
                isPlayerRegistrationClosed = validTeamRegistrationsThisSeason.FindAll(t => t.League.EndRegistrationDate < DateTime.Now).Count() > 0;
            }
            var vm = new TeamNavView
            {
                TeamId = id,
                TeamName = team.Title,
                IsDepartmentTeam = isDepartmentTeam,
                CurrentClubName = clubName,
                IsValidUser = User.IsInAnyRole(AppRole.Admins, AppRole.Editors),
                TeamLeagues = teamLeagues,
                UserLeagues = userLeagues?.Select(ul => new LeagueShort { Id = ul.LeagueId, Name = ul.Name, UnionId = ul.UnionId, Check = true })?.ToList(),
                SeasonId = seasonId ?? 0,
                clubs = clubsRepo.GetByTeamAndSeasonShort(id, seasonId ?? 0),
                IsBasketball = secRepo.GetSectionByTeamId(id)?.Alias == GamesAlias.BasketBall,
                CurrentClubId = clubId,
                IsRankCalculated = team.IsRankCalculated,
                UnionId = unionId ?? teamLeagues?.FirstOrDefault(x => x.Id == currentLeagueId)?.UnionId ?? clubsRepo.GetById(clubId ?? 0)?.UnionId,
                JobRole = usersRepo.GetTopLevelJob(AdminId),
                Benefactor = team?.TeamBenefactors?.Select(p => new BenefactorViewModel(p))?.FirstOrDefault(),
                IsTrainingEnabled = team.IsTrainingEnabled,
                CurrentLeagueId = currentLeagueId ?? teamLeagues?.FirstOrDefault()?.Id,
                DepartmentId = departmentId ?? clubId,
                IsGymnastic = string.Equals(secRepo.GetSectionByTeamId(id)?.Alias, GamesAlias.Gymnastic, StringComparison.InvariantCultureIgnoreCase),
                IsRugby = string.Equals(secRepo.GetSectionByTeamId(id)?.Alias, GamesAlias.Rugby, StringComparison.InvariantCultureIgnoreCase),
                IsCatchball = string.Equals(secRepo.GetSectionByTeamId(id)?.Alias, GamesAlias.NetBall, StringComparison.InvariantCultureIgnoreCase),
                IsWaterPolo = string.Equals(secRepo.GetSectionByTeamId(id)?.Alias, GamesAlias.WaterPolo, StringComparison.InvariantCultureIgnoreCase),
                IsIndividual = secRepo.CheckSectionIndividualStatus(id, LogicaName.Team),
                Section = section?.Alias,
                SectionId = section?.SectionId ?? 0,
                IsEilatTournament = teamLeagues?.Any(x => x.Id == currentLeagueId && x.IsEilatTournament) == true,
                IsTennisCompetition = isTennisCompetition,
                IsBlockedForRegistrationByTennisCompetition = (section.Alias.Equals(GamesAlias.Tennis) && (team.TeamRegistrations.All(r => r.IsDeleted && r.SeasonId == seasonId) || validTeamRegistrationsThisSeason.Count() == 0 || isPlayerRegistrationClosed)),
                IsTrainingTeam = db.ClubTeams.FirstOrDefault(t => t.TeamId == id && t.ClubId == clubId)?.IsTrainingTeam == true,
                IsUnionClubManagerUnderPastSeason = isClubUnderUnionManager ? CheckIfIsUnderPastSeason(clubId, currentLeagueId, seasonId) : false
            };
            ViewBag.SectionAlias = secRepo.GetSectionByTeamId(id)?.Alias;
            ViewBag.IsDepartMgr = roleName == JobRole.DepartmentManager;
            ViewBag.IsActivityManager = jobsRepo.IsActivityManager(AdminId);
            ViewBag.IsActivityViewer = jobsRepo.IsActivityViewer(AdminId);
            ViewBag.IsActivityRegistrationActive = jobsRepo.IsActivityRegistrationActive(AdminId);
            ViewBag.IsClubTrainingEnabled = _objTeamTrainingsRepo.IsClubTrainingEnabledForTeam(id);
            ViewBag.IsUnionViewer = AuthSvc.AuthorizeUnionViewerByManagerId(vm.UnionId ?? 0, AdminId);
            return View(vm);
        }

        private bool CheckIfIsUnderPastSeason(int? clubId, int? leagueId, int? seasonId)
        {
            var isUnderPastSeason = false;
            if (seasonId.HasValue)
            {
                if (clubId.HasValue)
                {
                    var currentClubSeason = seasonsRepository.GetById(seasonId.Value);
                    var lastClubSeason = seasonsRepository.GetById(seasonsRepository.GetLastSeasonIdByCurrentClubId(clubId.Value));
                    if (!currentClubSeason.IsActive || currentClubSeason.Id != lastClubSeason.Id) //currentClubSeason.EndDate.Year <= lastClubSeason.StartDate.Year
                    {
                        isUnderPastSeason = true;
                    }
                }
                else if (leagueId.HasValue)
                {
                    var currentLeagueSeason = seasonsRepository.GetById(seasonId.Value);
                    var lastLeagueSeason = seasonsRepository.GetLastSeasonIdByCurrentLeagueId(leagueId.Value);
                    if (!currentLeagueSeason.IsActive || currentLeagueSeason.Id != lastLeagueSeason.Id) //currentLeagueSeason.EndDate.Year <= lastLeagueSeason.StartDate.Year
                    {
                        isUnderPastSeason = true;
                    }
                }
            }
            return isUnderPastSeason;
        }

        public ActionResult Details(int id, int seasonId, int? currentLeagueId = null, int? clubId = null, int isTennisCompetition = 0)
        {
            var team = teamRepo.GetTeamByTeamSeasonId(id, seasonId);
            var unionId = teamRepo.GetTeamsUnion(id);
            bool isLeagueRegistrationTeam = teamRepo.CheckIfTeamIsLeagueRegistrationTeam(id, seasonId, clubId);
            var season = seasonsRepository.GetById(seasonId);

            var vm = new TeamInfoForm
            {
                ClubId = clubId,
                Culture = getCulture(),
                CompetitionAgeId = team.CompetitionAgeId.HasValue ? team.CompetitionAgeId.Value : 0,
                GenderId = team.GenderId.HasValue ? team.GenderId.Value : 3,
                MinimumAge = team.MinimumAge,
                MaximumAge = team.MaximumAge,
                CompetitionRegionId = team.RegionId.HasValue ? team.RegionId.Value : 0,
                CompetitionLevelId = team.LevelId.HasValue ? team.LevelId.Value : 0,
                MinRank = team.MinRank,
                MaxRank = team.MaxRank,
                PlaceForQualification = team.PlaceForQualification,
                QualificationStartDate = team.CategoriesPlaceDates?.FirstOrDefault()?.QualificationStartDate,
                QualificationEndDate = team.CategoriesPlaceDates?.FirstOrDefault()?.QualificationEndDate,
                PlaceForFinal = team.PlaceForFinal,
                FinalStartDate = team.CategoriesPlaceDates?.FirstOrDefault()?.FinalStartDate,
                FinalEndDate = team.CategoriesPlaceDates?.FirstOrDefault()?.FinalEndDate,
                IsTennisCompetition = isTennisCompetition,
                SectionAlias = GetSection(currentLeagueId, clubId),
                sectionId = secRepo.GetSectionByTeamId(id).SectionId,
                IsLeagueRegistration = isLeagueRegistrationTeam,
                IsTrainingEnabled = team.ClubTeams.Any(ct => ct.IsTrainingTeam && ct.SeasonId == seasonId),
                IsSchoolTeam = team.SchoolTeams.Any(st => st.School.SeasonId == seasonId),
                IsUnionConnected = unionId>0 || team.ClubTeams.Any(ct => ct.Club.UnionId.HasValue),
                MonthlyProvisionForSecurity = team.MonthlyProvisionForSecurity
            };


            vm.TeamForeignName = team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)?.TeamForeignName;
            

            vm.ListAges = new SelectList(teamRepo.GetCompetitionAges(unionId, seasonId), "id", "age_name", vm.CompetitionAgeId);
            vm.ListGenders = new SelectList(leagueRepo.GetGenders(), "GenderId", "TitleMany", vm.GenderId);
            vm.ListRegions = new SelectList(teamRepo.GetCompetitionRegions(unionId, seasonId), "id", "region_name", vm.CompetitionRegionId);
            vm.ListLevels = new SelectList(teamRepo.GetCompetitionLevels(unionId, seasonId), "id", "level_name", vm.CompetitionLevelId);
            vm.CompetitionAges = db.CompetitionAges.OrderBy(t => t.id).ToList();

            ViewBag.IsUnionClub = season.UnionId.HasValue;

            if (clubId.HasValue)
            {
                var club = clubsRepo.GetById((int)clubId);

                vm.PlayerInsurancePrices = team.ClubTeamPrices
                    .Where(x => x.ClubId == clubId.Value &&
                                x.PriceType == (int?)ClubTeamPriceType.PlayerInsurancePrice)
                    .Select(x => new TeamPrice
                    {
                        Price = x.Price,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        CardComProductId = x.CardComProductId
                    })
                    .ToList();

                vm.ParticipationPrices = team.ClubTeamPrices
                    .Where(x => x.ClubId == clubId.Value &&
                                x.PriceType == (int?)ClubTeamPriceType.ParticipationPrice)
                    .Select(x => new TeamPrice
                    {
                        Price = x.Price,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        CardComProductId = x.CardComProductId
                    })
                    .ToList();

                vm.PlayerRegistrationAndEquipmentPrices = team.ClubTeamPrices
                    .Where(x => x.ClubId == clubId.Value &&
                                x.PriceType == (int?)ClubTeamPriceType.PlayerRegistrationAndEquipmentPrice)
                    .Select(x => new TeamPrice
                    {
                        Price = x.Price,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        CardComProductId = x.CardComProductId
                    })
                    .ToList();

                if (club?.UnionId != null)
                {
                    var clubDisciplinesIds = club.ClubDisciplines.Where(x => x.SeasonId == seasonId).Select(x => x.DisciplineId);
                    vm.UnionDisciplines = club.Union.Disciplines.Where(x => clubDisciplinesIds.Contains(x.DisciplineId));
                    vm.TeamDisciplines = team.TeamDisciplines.Where(x => x.SeasonId == seasonId && x.ClubId == clubId)
                        .Select(x => x.Discipline);
                }
            }

            vm.InjectFrom(team);

            var teamRegistration = team.ActivityFormsSubmittedDatas.FirstOrDefault(x =>
                x.LeagueId == currentLeagueId &&
                x.Activity.IsAutomatic == true &&
                x.Activity.Type == ActivityType.Group &&
                x.Activity.SeasonId == seasonId);
            if (teamRegistration != null && currentLeagueId.HasValue)
            {
                vm.Registration = new TeamRegistrationModel
                {
                    IsActive = teamRegistration.IsActive,
                    ActivityName = teamRegistration.Activity.Name,
                    IsAutomatic = teamRegistration.Activity.IsAutomatic == true,
                    FormControls = teamRegistration.Activity.ActivityForms.FirstOrDefault()?.ActivityFormsDetails
                        ?.Where(x => !x.IsDisabled &&
                                     x.Type != ActivityFormControlType.CustomText.ToString() &&
                                     x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                     x.Type != ActivityFormControlType.CustomFileReadonly.ToString() &&
                                     x.Type != ActivityFormControlType.CustomPrice.ToString())
                        .ToList(),
                    CustomFields = ActivityCustomFieldsHelper.DeserializeFields(teamRegistration.CustomFields),
                    LeagueId = currentLeagueId.Value,
                    Culture = getCulture()
                };
            }
            var customRegistrations = team.ActivityFormsSubmittedDatas.Where(x =>
                    x.LeagueId == currentLeagueId &&
                    x.Activity.IsAutomatic != true &&
                    x.Activity.Type == ActivityType.Group &&
                    x.Activity.SeasonId == seasonId)
                .ToList();
            vm.CustomRegistrations = customRegistrations.Select(x => new TeamRegistrationModel
            {
                IsActive = x.IsActive,
                ActivityName = x.Activity.Name,
                IsAutomatic = x.Activity.IsAutomatic == true,
                FormControls = x.Activity.ActivityForms.FirstOrDefault()?.ActivityFormsDetails
                        ?.Where(r => !r.IsDisabled &&
                                     r.Type != ActivityFormControlType.CustomText.ToString() &&
                                     r.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                     r.Type != ActivityFormControlType.CustomFileReadonly.ToString() &&
                                     r.Type != ActivityFormControlType.CustomPrice.ToString())
                        .ToList(),
                CustomFields = ActivityCustomFieldsHelper.DeserializeFields(x.CustomFields),
                LeagueId = x.LeagueId ?? 0,
                Culture = getCulture()
            })
                .ToList();

            vm.SeasonId = seasonId;
            vm.leagues = leagueRepo.GetByTeamAndSeasonShort(id, seasonId);
            vm.clubs = clubsRepo.GetByTeamAndSeasonShort(id, seasonId);

            ViewBag.LeagueId = currentLeagueId
                ?? team.TeamRegistrations.FirstOrDefault(t=>t.SeasonId == seasonId && !t.IsDeleted && !t.League.IsArchive)?.LeagueId ?? team.TeamRegistrations.LastOrDefault()?.LeagueId;

            if (TempData["ViewData"] != null)
            {
                ViewData = (ViewDataDictionary)TempData["ViewData"];
            }

            ViewBag.IsClubSecretary = false;
            ViewBag.IsTeamManager = false;
            var roleName = usersRepo.GetTopLevelJob(AdminId);
            if (roleName == JobRole.ClubManager || roleName == JobRole.ClubSecretary)
            {
                var managerClubId = jobsRepo.GetClubIdByClubManagerId(AdminId);
                ViewBag.IsClubManager = managerClubId.HasValue && jobsRepo.IsClubManager(AdminId, managerClubId.Value);
                ViewBag.IsClubSecretary = false;
                if (roleName == JobRole.ClubSecretary)
                    ViewBag.IsClubSecretary = true;
            }
            else if (roleName == JobRole.TeamManager)
            {
                ViewBag.IsTeamManager = jobsRepo.IsTeamManager(AdminId, id);
            }
            ViewBag.IsClubTrainingEnabled = _objTeamTrainingsRepo.IsClubTrainingEnabledForTeam(id);
            ViewBag.IsDepartMgr = roleName == JobRole.DepartmentManager ? true : false;
            vm.IsGymnastic = string.Equals(secRepo.GetSectionByTeamId(id)?.Alias, GamesAlias.Gymnastic, StringComparison.InvariantCultureIgnoreCase);
            vm.sectionId = secRepo.GetSectionByTeamId(id).SectionId;
            return PartialView("_Details", vm);
        }

        [HttpGet]
        public ActionResult GetCompetitionAge(int ageId)
        {
            var competitionAge = db.CompetitionAges.Where(x => x.id == ageId).FirstOrDefault();

            return Json(new CompetitionAgeViewModel
            {
                AgeName = competitionAge.age_name,
                FromBirth = competitionAge.from_birth,
                ToBirth = competitionAge.to_birth,
                Gender = UIHelpers.GetGenderTitles(competitionAge.gender)
            }, JsonRequestBehavior.AllowGet);
        }

        private string GetSection(int? currentLeagueId, int? clubId)
        {
            if (currentLeagueId.HasValue)
            {
                var league = leagueRepo.GetById(currentLeagueId.Value);
                return league?.Club?.Section?.Alias ?? league?.Club?.Union?.Section?.Alias ?? league?.Union?.Section?.Alias ?? string.Empty;
            }
            else if (!currentLeagueId.HasValue && clubId.HasValue)
            {
                var club = clubsRepo.GetById(clubId.Value);
                return club?.Section?.Alias ?? club?.Union?.Section?.Alias ?? string.Empty;
            }
            return string.Empty;
        }

        public ActionResult TeamStandings(int teamId, int seasonId, int? departmentId = null)
        {
            var tsm = new TeamStandingsModel();
            tsm.TeamId = teamId;
            tsm.SectionAlias = string.Empty;
            tsm.SeasonId = seasonId;
            Team team = teamRepo.GetById(teamId);
            Season season = seasonsRepository.GetById(seasonId);
            Section section = secRepo.GetSectionByTeamId(teamId) ?? season?.Union?.Section;
            if (departmentId.HasValue && departmentId > 0)
            {
                tsm.SectionAlias = clubsRepo.GetById(departmentId.Value)?.SportSection?.Alias ?? season?.Union?.Section?.Alias;
            }
            else if (section != null)
            {
                tsm.SectionAlias = section?.Alias;
            }
            else
            {
                var club = leagueRepo.GetSectionByTeamId(teamId);
            }
            //tsm.Leagues = leagueRepo.GetByTeamAndSeasonShort(teamId, seasonId);
            tsm.Leagues = tsm.SectionAlias == GamesAlias.Tennis ? leagueRepo.GetByTeamAndSeasonForTennisLeaguesShort(teamId, seasonId).ToList() : leagueRepo.GetByTeamAndSeasonShort(teamId, seasonId).ToList();

            tsm.ClubStandings = new List<TeamStandingsGameForm>();

            var clubs = clubsRepo.GetByTeamAndSeason(teamId, seasonId).Where(c => c.IsSectionClub.Value).ToList();

            foreach (var club in clubs)
            {
                var teamStandings = teamRepo.GetTeamStandings(club.ClubId, teamId);

                var clubStanding = new TeamStandingsGameForm();

                clubStanding.TeamStandings = teamStandings.ToViewModel();
                var teamStandingGame = teamRepo.GetTeamStandingGame(teamId, club.ClubId, seasonId);

                clubStanding.TeamName = teamStandingGame?.ExternalTeamName;
                clubStanding.GamesUrl = teamStandingGame?.GamesUrl;
                clubStanding.TeamId = teamId;
                clubStanding.ClubId = club.ClubId;
                tsm.SeasonId = seasonId;
                tsm.ClubStandings.Add(clubStanding);
            }

            if (section.SectionId == 7)
            {
                return PartialView("SoccerTeamStandings", tsm);
            }
            else
            {
                return PartialView("TeamStandings", tsm);
            }

        }

        [HttpPost]
        public ActionResult DeleteGallery(int teamId, String filename)
        {
            string dirPath = ConfigurationManager.AppSettings["TeamUrl"] + "\\" + teamId + "\\" + filename.Split('/')[1];

            if (System.IO.File.Exists(dirPath))
            {
                System.IO.File.Delete(dirPath);
            }

            return RedirectToAction("TeamGalleries", new { teamId });
        }

        public ActionResult TeamGalleries(int teamId)
        {
            TeamGalleryModel result = new TeamGalleryModel();
            string dirPath = ConfigurationManager.AppSettings["TeamUrl"] + "\\" + teamId;
            if (!Directory.Exists(dirPath))
            {
                return null;
            }

            result.TeamGalleries = new List<TeamGalleriesForm>();

            UsersRepo usersRepo = new UsersRepo();
            var allfiles = System.IO.Directory.EnumerateFiles(dirPath, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".jpeg") || s.EndsWith(".jpg") || s.EndsWith(".png"));

            foreach (var file in allfiles)
            {
                try
                {

                    FileInfo info = new FileInfo(file);
                    var uid = int.Parse(info.Name.Substring(0, info.Name.IndexOf("__")));
                    User user = usersRepo.GetById(uid);
                    if (user != null)
                    {
                        var galleryForm = new TeamGalleriesForm();
                        var elem = new TeamGalleryForm();
                        elem.Created = info.CreationTime;
                        elem.url = teamId + "/" + info.Name;
                        elem.User = new GalleryUserModel
                        {
                            Id = user.UserId,
                            Name = user.FullName != null ? user.FullName : user.UserName,
                            Image = user.Image
                        };
                        galleryForm.TeamGallery = elem;
                        galleryForm.TeamId = teamId;
                        result.TeamGalleries.Add(galleryForm);
                    }

                }
                catch (Exception e)
                {
                    continue;
                }

                // Do something with the Folder or just add them to a list via nameoflist.add();
            }

            return PartialView("TeamGallery", result);
        }

        [HttpGet]
        public ActionResult MaxForClubForRouteDetails(int id, int disciplineId, int seasonId, int leagueId) {
            var league = leagueRepo.GetById(leagueId);
            var competitionRoute = league.CompetitionRoutes.FirstOrDefault(p => p.Id == id);
            List<CompetitionRouteClubDTO> list = league.Discipline.ClubDisciplines.Where(cd => cd.DisciplineId == disciplineId && (cd.Club.IsClubApproved.HasValue && cd.Club.IsClubApproved.Value) && !cd.Club.IsArchive && cd.SeasonId == seasonId).Select(cd => new CompetitionRouteClubDTO {
                CompetitionId = id,
                ClubId = cd.Club.ClubId,
                ClubName = cd.Club.Name,
                MaxRegistrationsAllowed = cd.Club.CompetitionRouteClubs.FirstOrDefault(x => x.CompetitionRouteId == id)?.MaximumRegistrationsAllowed ?? null
            }).OrderBy(p => p.ClubName).ToList();
            ViewBag.Title = competitionRoute.Discipline.Name + " - " + competitionRoute.League.Name + " - " + competitionRoute.DisciplineRoute.Route + " - " + competitionRoute.RouteRank.Rank;
            ViewBag.IsTeam = false;
            return PartialView("_MaxForClubForRouteDetailsList", list);
        }
        [HttpGet]
        public ActionResult MaxForClubForTeamRouteDetails(int id, int disciplineId, int seasonId, int leagueId)
        {
            var league = leagueRepo.GetById(leagueId);
            var competitionRoute = league.CompetitionTeamRoutes.FirstOrDefault(p => p.Id == id);
            List<CompetitionRouteClubDTO> list = league.Discipline.ClubDisciplines.Where(cd => cd.DisciplineId == disciplineId && (cd.Club.IsClubApproved.HasValue && cd.Club.IsClubApproved.Value) && !cd.Club.IsArchive && cd.SeasonId == seasonId).Select(cd => new CompetitionRouteClubDTO
            {
                CompetitionId = id,
                ClubId = cd.Club.ClubId,
                ClubName = cd.Club.Name,
                MaxRegistrationsAllowed = cd.Club.CompetitionTeamRouteClubs.FirstOrDefault(x => x.CompetitionTeamRouteId == id)?.MaximumRegistrationsAllowed ?? null
            }).OrderBy(p => p.ClubName).ToList();
            ViewBag.Title = competitionRoute.Discipline.Name + " - " + competitionRoute.League.Name + " - " + competitionRoute.DisciplineTeamRoute.Route + " - " + competitionRoute.RouteTeamRank.Rank;
            ViewBag.IsTeam = true;
            return PartialView("_MaxForClubForRouteDetailsList", list);
        }

        [HttpPost]
        public JsonResult MaxForClubForRouteDetails(CompetitionRouteClubDTO form)
        {
            disciplinesRepo.UpdateCompetitionRouteClubMaximumRegistrations(form.CompetitionId, form.ClubId, form.MaxRegistrationsAllowed);
            return Json(new { Success = true, ClubId = form.ClubId });
        }
        [HttpPost]
        public JsonResult MaxForClubForTeamRouteDetails(CompetitionRouteClubDTO form)
        {
            disciplinesRepo.UpdateCompetitionTeamRouteClubMaximumRegistrations(form.CompetitionId, form.ClubId, form.MaxRegistrationsAllowed);
            return Json(new { Success = true, ClubId = form.ClubId });
        }

        [HttpPost]
        public ActionResult Details(TeamInfoForm model)
        {
            var savePath = Server.MapPath(GlobVars.ContentPath + "/teams/");
            bool isLeagueRegistrationTeam = teamRepo.CheckIfTeamIsLeagueRegistrationTeam(model.TeamId, model.SeasonId, model.ClubId);
            var maxFileSize = GlobVars.MaxFileSize * 1000;
            Team team;
            if (model?.clubs?.Count > 0)
            {
                var clubTeam = clubsRepo.GetTeamClub(model.clubs.First().Id, model.TeamId, model.SeasonId);
                team = clubTeam != null ? clubTeam.Team : new Team();
            }
            else if (model?.leagues?.Count > 0)
            {
                team = teamRepo.GetLeagueTeam(model.TeamId, model.leagues.First().Id, model.SeasonId)?.Teams;
            }
            else
            {
                team = teamRepo.GetById(model?.TeamId);
            }

            #region Price

            var oldPrices = team.ClubTeamPrices;

            db.ClubTeamPrices.RemoveRange(oldPrices);

            team.ClubTeamPrices.Clear();

            if (model?.PlayerRegistrationAndEquipmentPrices != null)
            {
                foreach (var priceItem in model.PlayerRegistrationAndEquipmentPrices)
                {
                    team.ClubTeamPrices.Add(new ClubTeamPrice
                    {
                        EndDate = priceItem.EndDate,
                        Team = team,
                        ClubId = model.ClubId,
                        Price = priceItem.Price,
                        PriceType = (int)ClubTeamPriceType.PlayerRegistrationAndEquipmentPrice,
                        StartDate = priceItem.StartDate,
                        CardComProductId = priceItem.CardComProductId
                    });
                }
            }

            if (model?.ParticipationPrices != null)
            {
                foreach (var priceItem in model.ParticipationPrices)
                {
                    team.ClubTeamPrices.Add(new ClubTeamPrice
                    {
                        EndDate = priceItem.EndDate,
                        Team = team,
                        ClubId = model.ClubId,
                        Price = priceItem.Price,
                        PriceType = (int)ClubTeamPriceType.ParticipationPrice,
                        StartDate = priceItem.StartDate,
                        CardComProductId = priceItem.CardComProductId
                    });
                }
            }

            if (model?.PlayerInsurancePrices != null)
            {
                foreach (var priceItem in model.PlayerInsurancePrices)
                {
                    team.ClubTeamPrices.Add(new ClubTeamPrice
                    {
                        EndDate = priceItem.EndDate,
                        Team = team,
                        ClubId = model.ClubId,
                        Price = priceItem.Price,
                        PriceType = (int)ClubTeamPriceType.PlayerInsurancePrice,
                        StartDate = priceItem.StartDate,
                        CardComProductId = priceItem.CardComProductId
                    });
                }
            }

            #endregion

            //  UpdateModel(item);
            team.MaximumAge = model?.MaximumAge;
            team.MinimumAge = model?.MinimumAge;
            team.IsReligiousTeam = model.IsReligiousTeam;
            var topJob = User.CurrentTopLevelJob(model.SeasonId);
            if ((User.IsInAnyRole(AppRole.Admins) || topJob == JobRole.ClubManager) && (model.MonthlyProvisionForSecurity.HasValue || team.MonthlyProvisionForSecurity.HasValue))
            {
                team.MonthlyProvisionForSecurity = model.MonthlyProvisionForSecurity;
            }


            if (model.sectionId == 6 && model.IsTennisCompetition == 1 && !isLeagueRegistrationTeam)
            {
                team.CompetitionAgeId = model.CompetitionAgeId;
                team.RegionId = model.CompetitionRegionId;
                team.GenderId = model.GenderId;
                team.LevelId = model.CompetitionLevelId;
                team.MinRank = model.MinRank;
                team.MaxRank = model.MaxRank;
                team.PlaceForQualification = model.PlaceForQualification;
                team.PlaceForFinal = model.PlaceForFinal;
                var categoryDates = team.CategoriesPlaceDates.FirstOrDefault()
                    ?? teamRepo.CreateCategoryPlaceDate(model.TeamId);
                teamRepo.UpdateCategoryPlaceDates(categoryDates, model.QualificationStartDate, model.QualificationEndDate,
                    model.FinalStartDate, model.FinalEndDate);
            }

            var season = seasonsRepository.GetById(model.SeasonId);

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
                        if (!string.IsNullOrEmpty(team.PersonnelPic))
                            FileUtil.DeleteFile(savePath + team.PersonnelPic);

                        team.PersonnelPic = newName;
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
                        if (!string.IsNullOrEmpty(team.Logo))
                            FileUtil.DeleteFile(savePath + team.Logo);

                        team.Logo = newName;
                    }
                }
            }

            var insuranceFile = GetPostedFile("InsuranceApprovalFile");
            if (insuranceFile != null)
            {
                if (insuranceFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("InsuranceApprovalFile", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(insuranceFile, "InsuranceApproval");
                    if (newName == null)
                    {
                        ModelState.AddModelError("InsuranceApprovalFile", Messages.FileError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(team.InsuranceApproval))
                            FileUtil.DeleteFile(savePath + team.InsuranceApproval);

                        team.InsuranceApproval = newName;
                    }
                }
            }

            if (ModelState.IsValid)
            {
                if (model.Registration != null)
                {
                    var teamRegistration = team.ActivityFormsSubmittedDatas.FirstOrDefault(x =>
                        x.LeagueId == model.Registration.LeagueId &&
                        x.Activity.IsAutomatic.HasValue && x.Activity.IsAutomatic.Value &&
                        x.Activity.Type == ActivityType.Group &&
                        x.Activity.SeasonId == model.SeasonId);

                    if (teamRegistration != null)
                    {
                        teamRegistration.IsActive = model.Registration.IsActive;
                    }
                }

                teamRepo.UpdateTeamNameInSeason(team, model.SeasonId, model.Title);

                teamRepo.UpdateForeignTeamNameInSeason(team, model.SeasonId, model.TeamForeignName);

                team.IsReserved = model.IsReserved;
                team.IsUnderAdult = model.IsUnderAdult;
                team.NeedShirts = model.NeedShirts;
                team.Description = model.Description;
                team.IsTrainingEnabled = model.IsTrainingEnabled;

                if (model.SelectedDisciplines != null)
                {
                    var teamDisciplines = team.TeamDisciplines.Where(x => x.ClubId == model.ClubId && x.SeasonId == model.SeasonId).ToList();

                    foreach (var disciplineId in model.SelectedDisciplines)
                    {
                        var discipline = teamDisciplines.FirstOrDefault(x => x.DisciplineId == disciplineId);

                        if (discipline == null)
                        {
                            team.TeamDisciplines.Add(new TeamDiscipline
                            {
                                SeasonId = model.SeasonId,
                                ClubId = model.ClubId ?? 0,
                                DisciplineId = disciplineId
                            });
                        }

                        var playersInTeamWithoutThisDiscipline = team.TeamsPlayers.Where(
                                x => x.ClubId == model.ClubId &&
                                     x.SeasonId == model.SeasonId &&
                                     !x.User.PlayerDisciplines.Any(
                                         d => d.DisciplineId == disciplineId && d.ClubId == model.ClubId &&
                                              d.SeasonId == model.SeasonId))
                            .Select(x => x.User)
                            .ToList();

                        foreach (var player in playersInTeamWithoutThisDiscipline)
                        {
                            player.PlayerDisciplines.Add(new PlayerDiscipline
                            {
                                DisciplineId = disciplineId,
                                SeasonId = model.SeasonId,
                                ClubId = model.ClubId ?? 0
                            });
                        }
                    }

                    foreach (var disciplineToRemove in teamDisciplines.Where(x => !model.SelectedDisciplines.Contains(x.DisciplineId)))
                    {
                        var playersDisciplinesToRemove = disciplineToRemove.Discipline.PlayerDisciplines.Where(
                                x => x.ClubId == disciplineToRemove.ClubId &&
                                     x.SeasonId == disciplineToRemove.SeasonId &&
                                     x.User.TeamsPlayers.Any(
                                         tp => tp.TeamId == team.TeamId && tp.ClubId == model.ClubId &&
                                               tp.SeasonId == model.SeasonId))
                            .ToList();
                        foreach (var playerDiscipline in playersDisciplinesToRemove)
                        {
                            db.Entry(playerDiscipline).State = EntityState.Deleted;
                        }

                        db.Entry(disciplineToRemove).State = EntityState.Deleted;
                    }
                }


                teamRepo.Save();
                TempData["Saved"] = true;
            }
            else
            {
                TempData["ViewData"] = ViewData;
            }

            if (model.IsTennisCompetition == 1)
            {
                return RedirectToAction("Details", new { id = team.TeamId, seasonId = model.SeasonId, clubId = model.ClubId, isTennisCompetition = model.IsTennisCompetition });
            }
            else
            {
                return RedirectToAction("Details", new { id = team.TeamId, seasonId = model.SeasonId, clubId = model.ClubId });
            }

        }

        public ActionResult DeleteImage(int leagueId, int teamId, int seasonId, string image)
        {
            DataEntities db = new DataEntities();
            var item = db.Teams.FirstOrDefault(x => x.TeamId == teamId);
            if (item == null || string.IsNullOrEmpty(image))
                return RedirectToAction("Edit", new { id = leagueId });
            if (image == "Image")
            {
                item.PersonnelPic = null;
            }
            if (image == "Logo")
            {
                item.Logo = null;
            }
            db.SaveChanges();
            return RedirectToAction("Edit", new { id = teamId, currentLeagueId = leagueId, seasonId });
        }

        public ActionResult List(int id, int seasonId, int? unionId, int? departmentId = null, int isTennisCompetition = 0)
        {
            var league = leagueRepo.GetById(id);
            var isIndividualSection = league?.Club?.Section?.IsIndividual
                ?? league?.Union?.Section?.IsIndividual
                ?? league?.Club?.Union?.Section?.IsIndividual
                ?? false;
            var sectionAlias = league?.Club?.Section?.Alias
                ?? league?.Union?.Section?.Alias
                ?? league?.Club?.Union?.Section?.Alias
                ?? string.Empty;
            var club = clubsRepo.GetById(league?.ClubId ?? 0);
            var vm = new TeamForm
            {
                LeagueId = id,
                SeasonId = seasonId,
                DepartmentId = club?.ParentClub != null ? club?.ClubId : 0,
                UnionId = unionId,
                SectionId = secRepo.GetByLeagueId(id)?.SectionId,
                Section = secRepo.GetByLeagueId(id)?.Alias ?? "",
                IsTennisCompetition = isTennisCompetition,
                IsIndividual = isIndividualSection
            };

            if (departmentId != null)
            {
                vm.DepartmentId = departmentId;
            }
            var userRole = usersRepo.GetTopLevelJob(AdminId);
            if (AuthSvc.AuthorizeUnionViewerByManagerId(unionId ?? 0, AdminId)) userRole = JobRole.Unionviewer;
            if (User.IsInAnyRole(AppRole.Workers) && unionId != 38)
            {
                switch (userRole)
                {
                    case JobRole.UnionManager:
                    case JobRole.Unionviewer:
                    case JobRole.RefereeAssignment:
                        // vm.TeamsList = _teamsRepo.GetTeamsByLeague(id);
                        vm.TeamsList = teamRepo.GetTeams(seasonId, id);
                        break;
                    case JobRole.LeagueManager:
                        vm.TeamsList = teamRepo.GetTeams(seasonId, id);
                        ViewBag.IsLeagueManager = true;
                        break;
                    case JobRole.TeamManager:
                        vm.TeamsList = teamRepo.GetByManagerId(base.AdminId, seasonId);
                        break;
                    case JobRole.DisciplineManager:
                        vm.TeamsList = teamRepo.GetTeams(seasonId, id);
                        break;
                    case JobRole.DepartmentManager:
                        vm.TeamsList = teamRepo.GetTeams(seasonId, id);
                        break;
                    case JobRole.ClubManager:
                    case JobRole.ClubSecretary:
                        vm.TeamsList = teamRepo.GetTeams(seasonId, id);
                        break;
                }
            }
            else if (unionId == 38 && isTennisCompetition == 0)
            {
                vm.TeamsList = teamRepo.GetRegisteredTeamsByLeagueId(id, seasonId);
            }
            else
            {
                vm.TeamsList = teamRepo.GetTeams(seasonId, id);
            }

            ViewBag.SeasonId = seasonId;
            ViewBag.UnionId = unionId;
            ViewBag.LeagueName = league?.Name;


            if (!isIndividualSection)
            {
                var teamsIds = vm.TeamsList?.Select(c => c.TeamId);
                if (teamsIds != null && teamsIds.Any())
                {
                    foreach (var teamId in teamsIds)
                    {
                        List<PlayerViewModel> players = playersRepo.GetTeamPlayersByTeamIdsShort(teamId, id, seasonId, sectionAlias);
                        clubsRepo.CountPlayersRegistrations(players, sectionAlias.Equals(GamesAlias.Gymnastic), out int approvedPlayers, out int completedPlayers,
                            out int notApprovedPlayers, out int playersCount, out int waitingForApproval, out int activePlayers, out int notActive,
                            out int registered);
                        vm.StatisticsDictionary.Add(teamId,
                            new PlayerCountStats { Approved = approvedPlayers, NotActive = notActive, Waiting = waitingForApproval, Registered = registered });
                    }
                }
            }

            vm.TeamsList = vm.TeamsList.OrderBy(x => x.Title);

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);

            return PartialView("_List", vm);
        }

        [HttpPost]
        public ActionResult DuplicateRoutesList(int disciplineId, int seasonId, int leagueId, int LeagueIdToDup)
        {
            var disciplineRoutes = disciplinesRepo.GetCompetitionRoutes(disciplineId, seasonId, LeagueIdToDup);
            var disciplineTeamRoutes = disciplinesRepo.GetCompetitionTeamRoutes(disciplineId, seasonId, LeagueIdToDup);
            // duplicate those routes
            var league = disciplinesRepo.GetById(disciplineId).Leagues.FirstOrDefault(l => l.LeagueId == leagueId);
            disciplineRoutes.ForEach(r => db.CompetitionRoutes.Add(new CompetitionRoute() {

                DisciplineId = disciplineId,
                SeasonId = seasonId,
                RouteId = r.RouteId,
                RankId = r.RankId,
                LeagueId = leagueId,
                Composition = r.Composition,
                SecondComposition = r.SecondComposition,
                ThirdComposition = r.ThirdComposition,
                FourthComposition = r.FourthComposition,
                FifthComposition = r.FifthComposition,
                SixthComposition = r.SixthComposition,
                SeventhComposition = r.SeventhComposition,
                EighthComposition = r.EighthComposition,
                NinthComposition = r.NinthComposition,
                TenthComposition = r.TenthComposition,
                InstrumentIds = r.InstrumentIds,
                IsCompetitiveEnabled = r.IsCompetitiveEnabled
                }));
            disciplineTeamRoutes.ForEach(r => db.CompetitionTeamRoutes.Add(new CompetitionTeamRoute()
            {
                DisciplineId = disciplineId,
                SeasonId = seasonId,
                RouteId = r.RouteId,
                RankId = r.RankId,
                LeagueId = leagueId,
                Composition = r.Composition,
                SecondComposition = r.SecondComposition,
                ThirdComposition = r.ThirdComposition,
                FourthComposition = r.FourthComposition,
                FifthComposition = r.FifthComposition,
                SixthComposition = r.SixthComposition,
                SeventhComposition = r.SeventhComposition,
                EighthComposition = r.EighthComposition,
                NinthComposition = r.NinthComposition,
                TenthComposition = r.TenthComposition,
                InstrumentIds = r.InstrumentIds,
                IsCompetitiveEnabled = r.IsCompetitiveEnabled
            }));

            disciplinesRepo.Save();
            return Json(new { Success = true});
        }

        public ActionResult DuplicateRoutes(int disciplineId, int seasonId, int leagueId)
        {

            var leagues = disciplinesRepo.GetById(disciplineId).Leagues.Where(l => l.SeasonId == seasonId && !l.IsArchive && l.LeagueId != leagueId).ToList();
            ViewBag.disciplineId = disciplineId;
            ViewBag.leagueId = leagueId;
            ViewBag.seasonId = seasonId;
            var list = new MultiSelectList(leagues, nameof(League.LeagueId), nameof(League.Name));
            return PartialView("_DuplicateRoutes", list);
        }
        



            public ActionResult RoutesList(int disciplineId, int seasonId, int leagueId)
        {
            var disciplineRoutes = disciplinesRepo.GetCompetitionRoutes(disciplineId, seasonId, leagueId);
            var disciplineTeamRoutes = disciplinesRepo.GetCompetitionTeamRoutes(disciplineId, seasonId, leagueId);
            var vm = new RoutesControlForm
            {
                DisciplineId = disciplineId,
                SeasonId = seasonId,
                LeagueId = leagueId,
                Routes = disciplineRoutes != null && disciplineRoutes.Any()
                        ? disciplineRoutes.Select(dr => new BasicRouteForm
                        {
                            CompetitionRouteId = dr.Id,
                            DiciplineId = dr.DisciplineId,
                            RouteId = dr.DisciplineRoute.Id,
                            RouteName = dr.DisciplineRoute?.Route,
                            RankId = dr.RouteRank.Id,
                            RankName = dr.RouteRank?.Rank,
                            SeasonId = dr.SeasonId,
                            GymnasticsCount = playersRepo.CheckCompetitionRegistrationsCount(null, leagueId, seasonId, dr.Id),
                            Composition = dr.Composition,
                            SecondComposition = dr.SecondComposition,
                            ThirdComposition = dr.ThirdComposition,
                            FourthComposition = dr.FourthComposition,
                            FifthComposition = dr.FifthComposition,
                            SixthComposition = dr.SixthComposition,
                            SeventhComposition = dr.SeventhComposition,
                            EighthComposition = dr.EighthComposition,
                            NinthComposition = dr.NinthComposition,
                            TenthComposition = dr.TenthComposition,
                            InstrumentName = disciplinesRepo.GetInstrumentsNames(dr.InstrumentIds),
                            IsCompetitiveEnabled = dr.IsCompetitiveEnabled
                        })
                        : Enumerable.Empty<RouteForm>(),
                TeamsRoutes = disciplineTeamRoutes != null && disciplineTeamRoutes.Any()
                        ? disciplineTeamRoutes.Select(dr => new BasicRouteForm
                        {
                            CompetitionRouteId = dr.Id,
                            DiciplineId = dr.DisciplineId,
                            RouteId = dr.DisciplineTeamRoute.Id,
                            RouteName = dr.DisciplineTeamRoute?.Route,
                            RankId = dr.RouteTeamRank.Id,
                            RankName = dr.RouteTeamRank?.Rank,
                            SeasonId = dr.SeasonId,
                            GymnasticsCount = playersRepo.CheckCompetitionRegistrationsCount(null, leagueId, seasonId, dr.Id),
                            Composition = dr.Composition,
                            SecondComposition = dr.SecondComposition,
                            ThirdComposition = dr.ThirdComposition,
                            FourthComposition = dr.FourthComposition,
                            FifthComposition = dr.FifthComposition,
                            SixthComposition = dr.SixthComposition,
                            SeventhComposition = dr.SeventhComposition,
                            EighthComposition = dr.EighthComposition,
                            NinthComposition = dr.NinthComposition,
                            TenthComposition = dr.TenthComposition,
                            InstrumentName = disciplinesRepo.GetInstrumentsNames(dr.InstrumentIds),
                            IsCompetitiveEnabled = dr.IsCompetitiveEnabled
                        })
                        : Enumerable.Empty<RouteForm>()
            };

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);

            return PartialView("_RoutesList", vm);
        }

        [HttpPost]
        public void RemoveCompetitionRoutes(string ids)
        {
            leagueRepo.RemoveCompetitionRoutes(ids);
        }

        [HttpPost]
        public void RemoveCompetitionTeamRoutes(string ids)
        {
            leagueRepo.RemoveCompetitionTeamsRoutes(ids);
        }

        [HttpPost]
        public void ChangeCompetitionRouteStatus(int competitionRouteId, bool isEnabled)
        {
            leagueRepo.ChangeCompetitionRouteStatus(competitionRouteId, isEnabled);
        }

        [HttpPost]
        public void ChangeCompetitionTeamRouteStatus(int competitionRouteId, bool isEnabled)
        {
            leagueRepo.ChangeCompetitionTeamRouteStatus(competitionRouteId, isEnabled);
        }

        [HttpGet]
        public ActionResult CreateRoute(int disciplineId, int seasonId, int leagueId)
        {
            var vm = new RouteForm
            {
                DiciplineId = disciplineId,
                SeasonId = seasonId,
                LeagueId = leagueId,
                Routes = (disciplinesRepo.GetDisciplineRoutes(disciplineId) ?? Enumerable.Empty<DisciplineRoute>())
                        .Select(r => new SelectListItem
                        {
                            Value = r?.Id.ToString(),
                            Text = r?.Route
                        }),
                Ranks = disciplinesRepo.GetRanksByDisciplineId(disciplineId),
                Instruments = (disciplinesRepo.GetAllInstruments(disciplineId, seasonId) ?? Enumerable.Empty<InstrumentDto>())
                        .Select(i => new SelectListItem
                        {
                            Value = i?.Id?.ToString(),
                            Text = i?.Name
                        })

            };
            return PartialView("_CreateRoute", vm);
        }

        [HttpGet]
        public ActionResult EditRoute(int competitionRouteId)
        {
            var route = db.CompetitionRoutes.FirstOrDefault(r => r.Id == competitionRouteId);
            
            var vm = new RouteForm
            {
                DiciplineId = route.DisciplineId,
                SeasonId = route.SeasonId,
                LeagueId = route.LeagueId,
                Routes = (disciplinesRepo.GetDisciplineRoutes(route.DisciplineId) ?? Enumerable.Empty<DisciplineRoute>())
                        .Select(r => new SelectListItem
                        {
                            Value = r?.Id.ToString(),
                            Text = r?.Route
                        }),
                Ranks = disciplinesRepo.GetRanksByDisciplineId(route.DisciplineId),
                Instruments = (disciplinesRepo.GetAllInstruments(route.DisciplineId, route.SeasonId) ?? Enumerable.Empty<InstrumentDto>())
                        .Select(i => new SelectListItem
                        {
                            Value = i?.Id?.ToString(),
                            Text = i?.Name
                        })

            };
            ViewBag.RouteToEdit = route;
            return PartialView("_CreateRoute", vm);
        }

        [HttpGet]
        public ActionResult CreateTeamRoute(int disciplineId, int seasonId, int leagueId)
        {
            var vm = new RouteForm
            {
                DiciplineId = disciplineId,
                SeasonId = seasonId,
                LeagueId = leagueId,
                Routes = (disciplinesRepo.GetDisciplineTeamRoutes(disciplineId) ?? Enumerable.Empty<DisciplineTeamRoute>())
                        .Select(r => new SelectListItem
                        {
                            Value = r?.Id.ToString(),
                            Text = r?.Route
                        }),
                Ranks = disciplinesRepo.GetTeamRanksByDisciplineId(disciplineId),
                Instruments = (disciplinesRepo.GetAllInstruments(disciplineId, seasonId) ?? Enumerable.Empty<InstrumentDto>())
                        .Select(i => new SelectListItem
                        {
                            Value = i?.Id?.ToString(),
                            Text = i?.Name
                        })

            };
            return PartialView("_CreateTeamRoute", vm);
        }



        [HttpGet]
        public ActionResult EditTeamRoute(int competitionRouteId)
        {
            var route = db.CompetitionTeamRoutes.FirstOrDefault(r => r.Id == competitionRouteId);

            var vm = new RouteForm
            {
                DiciplineId = route.DisciplineId,
                SeasonId = route.SeasonId,
                LeagueId = route.LeagueId,
                Routes = (disciplinesRepo.GetDisciplineTeamRoutes(route.DisciplineId) ?? Enumerable.Empty<DisciplineTeamRoute>())
                        .Select(r => new SelectListItem
                        {
                            Value = r?.Id.ToString(),
                            Text = r?.Route
                        }),
                Ranks = disciplinesRepo.GetTeamRanksByDisciplineId(route.DisciplineId),
                Instruments = (disciplinesRepo.GetAllInstruments(route.DisciplineId, route.SeasonId) ?? Enumerable.Empty<InstrumentDto>())
                        .Select(i => new SelectListItem
                        {
                            Value = i?.Id?.ToString(),
                            Text = i?.Name
                        })

            };
            ViewBag.RouteToEdit = route;
            return PartialView("_CreateTeamRoute", vm);
        }




        [HttpPost]
        public ActionResult CreateRoute(BasicRouteForm frm)
        {
            if (frm.CompetitionRouteToEdit.HasValue)
            {
                var route = db.CompetitionRoutes.FirstOrDefault(r => r.Id == frm.CompetitionRouteToEdit.Value);
                if (route != null)
                {
                    route.RouteId = frm.RouteId;
                    route.RankId = frm.RankId;
                    route.SeasonId = frm.SeasonId.Value;
                    route.Composition = frm.Composition;
                    route.InstrumentIds = frm.InstrumentsIds != null && frm.InstrumentsIds.Any() ? string.Join(",", frm.InstrumentsIds) : string.Empty;
                    route.SecondComposition = frm.SecondComposition;
                    route.ThirdComposition = frm.ThirdComposition;
                    route.FourthComposition = frm.FourthComposition;
                    route.FifthComposition = frm.FifthComposition;
                    route.SixthComposition = frm.SixthComposition;
                    route.SeventhComposition = frm.SeventhComposition;
                    route.EighthComposition = frm.EighthComposition;
                    route.NinthComposition = frm.NinthComposition;
                    route.TenthComposition = frm.TenthComposition;
                    route.IsCompetitiveEnabled = frm.IsCompetitiveEnabled;
                    disciplinesRepo.Save();
                }
            }
            else
            {
                try
                {
                    var model = new CompetitionRouteDto
                    {
                        DiciplineId = frm.DiciplineId,
                        LeagueId = frm.LeagueId,
                        RouteId = frm.RouteId,
                        RankId = frm.RankId,
                        SeasonId = frm.SeasonId.Value,
                        Composition = frm.Composition,
                        IntrumentsIds = frm.InstrumentsIds != null && frm.InstrumentsIds.Any() ? string.Join(",", frm.InstrumentsIds) : string.Empty,
                        SecondComposition = frm.SecondComposition,
                        ThirdComposition = frm.ThirdComposition,
                        FourthComposition = frm.FourthComposition,
                        FifthComposition = frm.FifthComposition,
                        SixthComposition = frm.SixthComposition,
                        SeventhComposition = frm.SeventhComposition,
                        EighthComposition = frm.EighthComposition,
                        NinthComposition = frm.NinthComposition,
                        TenthComposition = frm.TenthComposition,
                        IsCompetitiveEnabled = frm.IsCompetitiveEnabled
                    };
                    disciplinesRepo.CreateCompetitionRoute(model);
                }
                catch (Exception ex)
                {
                    return Json(new { Error = true, ErrorMessage = ex.Message });
                }
            }
            return RedirectToAction(nameof(RoutesList), new { disciplineId = frm.DiciplineId, seasonId = frm.SeasonId, leagueId = frm.LeagueId });
        }

        
        [HttpPost]
        public ActionResult CreateTeamRoute(BasicRouteForm frm)
        {
            if (frm.CompetitionRouteToEdit.HasValue)
            {
                var route = db.CompetitionTeamRoutes.FirstOrDefault(r => r.Id == frm.CompetitionRouteToEdit.Value);
                if (route != null)
                {
                    route.RouteId = frm.RouteId;
                    route.RankId = frm.RankId;
                    route.SeasonId = frm.SeasonId.Value;
                    route.Composition = frm.Composition;
                    route.InstrumentIds = frm.InstrumentsIds != null && frm.InstrumentsIds.Any() ? string.Join(",", frm.InstrumentsIds) : string.Empty;
                    route.SecondComposition = frm.SecondComposition;
                    route.ThirdComposition = frm.ThirdComposition;
                    route.FourthComposition = frm.FourthComposition;
                    route.FifthComposition = frm.FifthComposition;
                    route.SixthComposition = frm.SixthComposition;
                    route.SeventhComposition = frm.SeventhComposition;
                    route.SeventhComposition = frm.SeventhComposition;
                    route.EighthComposition = frm.EighthComposition;
                    route.NinthComposition = frm.NinthComposition;
                    route.TenthComposition = frm.TenthComposition;
                    route.IsCompetitiveEnabled = frm.IsCompetitiveEnabled;
                    disciplinesRepo.Save();
                }
            }
            else
            {
                try
                {
                    var model = new CompetitionRouteDto
                    {
                        DiciplineId = frm.DiciplineId,
                        LeagueId = frm.LeagueId,
                        RouteId = frm.RouteId,
                        RankId = frm.RankId,
                        SeasonId = frm.SeasonId.Value,
                        Composition = frm.Composition,
                        IntrumentsIds = frm.InstrumentsIds != null && frm.InstrumentsIds.Any() ? string.Join(",", frm.InstrumentsIds) : string.Empty,
                        SecondComposition = frm.SecondComposition,
                        ThirdComposition = frm.ThirdComposition,
                        FourthComposition = frm.FourthComposition,
                        FifthComposition = frm.FifthComposition,
                        SixthComposition = frm.SixthComposition,
                        SeventhComposition = frm.SeventhComposition,
                        EighthComposition = frm.EighthComposition,
                        NinthComposition = frm.NinthComposition,
                        TenthComposition = frm.TenthComposition,
                        IsCompetitiveEnabled = frm.IsCompetitiveEnabled
                    };
                    disciplinesRepo.CreateCompetitionTeamRoute(model);
                }
                catch (Exception ex)
                {
                    return Json(new { Error = true, ErrorMessage = ex.Message });
                }
            }
            return RedirectToAction(nameof(RoutesList), new { disciplineId = frm.DiciplineId, seasonId = frm.SeasonId, leagueId = frm.LeagueId });
        }


        public ActionResult DeleteRoute(int id, int disciplineId, int seasonId, int leagueId)
        {
            disciplinesRepo.DeleteCompetitionRoute(id);
            return RedirectToAction(nameof(RoutesList), new { disciplineId, seasonId, leagueId });
        }


        public ActionResult DeleteTeamRoute(int id, int disciplineId, int seasonId, int leagueId)
        {
            disciplinesRepo.DeleteCompetitionTeamRoute(id);
            return RedirectToAction(nameof(RoutesList), new { disciplineId, seasonId, leagueId });
        }

        public void SetIsRankCalculated(int teamId, bool isChecked)
        {
            teamRepo.SetIsRankCalculated(teamId, isChecked);
        }
        

        [HttpPost]
        public ActionResult Create(TeamForm frm)
        {
            var team = new Team();

            if (frm.IsNew || frm.IsTennisCompetition == 1)
            {
                team.Title = frm.Title.Trim();

                teamRepo.Create(team);
                teamRepo.AddTeamDetailToSeason(team, frm.SeasonId);
            }
            else if (frm.TeamId != 0 && !frm.IsNew)
            {
                team = teamRepo.GetById(frm.TeamId);
            }
            else if (!string.IsNullOrEmpty(frm.Title))
            {
                team = teamRepo.GetByName(frm.Title);
                if (team == null)
                {
                    if (frm.SectionId == 6)
                    {
                        TempData["ErrExists"] = Messages.CategoryNotFound;
                    }
                    else
                    {
                        TempData["ErrExists"] = Messages.TeamNotFound;
                    }

                    return RedirectToAction("List", new { id = frm.LeagueId, seasonId = frm.SeasonId });
                }
            }
            else
            {
                if (frm.SectionId == 6)
                {
                    TempData["ErrExists"] = Messages.CategoryNotFound;
                }
                else
                {
                    TempData["ErrExists"] = Messages.TeamNotFound;
                }

                return RedirectToAction("List", new { id = frm.LeagueId, seasonId = frm.SeasonId });
            }

            var league = teamRepo.GetLeague(frm.LeagueId);
            var isExistsInLeague = league.LeagueTeams.Any(t => t.TeamId == team.TeamId);

            if (!isExistsInLeague)
            {
                var lt = new LeagueTeams
                {
                    TeamId = team.TeamId,
                    LeagueId = league.LeagueId,
                    SeasonId = frm.SeasonId
                };
                league.LeagueTeams.Add(lt);

                if (frm.IsTennisCompetition == 1)
                {
                    var game = new TennisGame
                    {
                        CategoryId = team.TeamId,
                        GameDays = "5,6",
                        StartDate = DateTime.Now,
                        GamesInterval = "01:00",
                        PointsWin = 2,
                        PointsDraw = 0,
                        PointsLoss = 1,
                        PointTechWin = 2,
                        PointsTechLoss = 0,
                        SortDescriptions = "0,1,2",
                        ActiveWeeksNumber = 1,
                        BreakWeeksNumber = 0
                    };

                    db.TennisGames.Add(game);
                    db.SaveChanges();
                }

                var teamPlayersInCurrentSeason = team.TeamsPlayers.Where(x => x.SeasonId == frm.SeasonId).GroupBy(x => x.UserId).Select(x => x.FirstOrDefault()).Where(x => x != null);
                foreach (var teamPlayer in teamPlayersInCurrentSeason)
                {
                    team.TeamsPlayers.Add(new TeamsPlayer
                    {
                        ApprovalDate = teamPlayer.ApprovalDate,
                        ClubComment = teamPlayer.ClubComment,
                        //ClubId = teamPlayer.ClubId,
                        HandicapLevel = teamPlayer.HandicapLevel,
                        IsActive = teamPlayer.IsActive,
                        IsApprovedByManager = teamPlayer.IsApprovedByManager,
                        IsLocked = teamPlayer.IsLocked,
                        IsTrainerPlayer = teamPlayer.IsTrainerPlayer,
                        LeagueId = league.LeagueId,
                        MedExamDate = teamPlayer.User.MedExamDate,
                        PosId = teamPlayer.PosId,
                        SeasonId = frm.SeasonId,
                        ShirtNum = teamPlayer.ShirtNum,
                        StartPlaying = teamPlayer.StartPlaying,
                        TeamId = teamPlayer.TeamId,
                        UserId = teamPlayer.UserId,
                        UnionComment = teamPlayer.UnionComment
                    });
                }

                teamRepo.Save();
            }
            else
            {
                TempData["ErrExists"] = Messages.TeamExists;
            }

            if (frm.IsTennisCompetition == 1)
            {
                return RedirectToAction("List", new { id = frm.LeagueId, seasonId = frm.SeasonId, isTennisCompetition = frm.IsTennisCompetition });
            }
            else
            {
                return RedirectToAction("List", new { id = frm.LeagueId, seasonId = frm.SeasonId });
            }
        }


        public ActionResult Delete(int id, int leagueId, int seasonId, int isTennisCompetition = 0)
        {
            var league = teamRepo.GetLeague(leagueId);
            var teamToRemove = league.LeagueTeams.SingleOrDefault(lt => lt.TeamId == id && lt.SeasonId == seasonId && lt.LeagueId == leagueId);
            if (teamToRemove != null)
            {
                teamRepo.RemoveTeamDetails(teamToRemove.Teams, seasonId);
                league.LeagueTeams.Remove(teamToRemove);

                if (teamRepo.GetNumberOfLeaguesAndClubs(id) == 0)
                {
                    Team team = teamRepo.GetById(id);
                    team.IsArchive = true;
                }

                teamRepo.Save();
            }

            return RedirectToAction("List", new { id = leagueId, seasonId, isTennisCompetition });
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

            var savePath = Server.MapPath(GlobVars.ContentPath + "/teams/");

            var di = new DirectoryInfo(savePath);
            if (!di.Exists)
                di.Create();

            // start security checking
            byte[] imgData;
            using (var reader = new BinaryReader(file.InputStream))
            {
                imgData = reader.ReadBytes(file.ContentLength);
            }


            System.IO.File.WriteAllBytes(savePath + newName, imgData);
            return newName;
        }

        [HttpPost]
        public ActionResult MoveToLeague(int[] teams, int leagueId, int seasonId, bool isTennisLeague = false)
        {
            var vm = new MoveTeamToLeagueViewModel();
            var currentUnionId = leagueRepo.GetById(leagueId).UnionId;

            if (currentUnionId.HasValue)
            {
                if (isTennisLeague)
                {
                    var leagues = leagueRepo.GetByUnion(currentUnionId.Value, seasonId).Where(x => x.LeagueId != leagueId && (x.EilatTournament == null || x.EilatTournament == false)).OrderBy(x => x.SortOrder).ToList();
                    vm.Leagues = leagues.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.LeagueId.ToString()
                    }).ToList();
                    if (leagues.Any())
                    {
                        vm.LeagueId = leagues.Last().LeagueId;
                    }
                }
                else
                {
                var leagues = leagueRepo.GetLeaguesForMoveByUnionSeasonId(currentUnionId.Value, seasonId, leagueId).OrderBy(x => x.SortOrder);
                    vm.Leagues = leagues.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.LeagueId.ToString()
                    }).ToList();

                    if (leagues.Any())
                    {
                        vm.LeagueId = leagues.Last().LeagueId;
                    }
                }

            }
            vm.CurrentLeagueId = leagueId;
            vm.TeamIds = teams;
            vm.SeasonId = seasonId;
            vm.IsTennisLeague = isTennisLeague;
            return PartialView("_MoveTeamToLeaguePartial", vm);
        }

        [HttpPost]
        public ActionResult MoveTeams(MoveTeamToLeagueViewModel model)
        {
            if (model.IsTennisLeague)
            {
                if (model.IsCopy)
                {
                    clubsRepo.CopyTennisTeams(model.TeamIds, model.LeagueId, model.CurrentLeagueId, model.SeasonId);
                }
                else
                {
                    clubsRepo.MoveTennisTeams(model.TeamIds, model.LeagueId, model.CurrentLeagueId, model.SeasonId);
                }
            }
            else
            {
                teamRepo.MoveTeams(model.LeagueId, model.TeamIds, model.CurrentLeagueId, model.SeasonId);
            }
            return Redirect(Request.UrlReferrer.ToString());
        }

        [HttpPost]
        public ActionResult Import(ImportFromExcelViewModel viewModel)
        {
            if (viewModel.ImportFile != null)
            {
                using (XLWorkbook workBook = new XLWorkbook(viewModel.ImportFile.InputStream))
                {
                    //Read the first Sheet from Excel file.
                    IXLWorksheet workSheet = workBook.Worksheet(1);
                    bool firstRow = true;

                    var games = new List<ExcelGameDto>();

                    var localCulture = System.Globalization.CultureInfo.CurrentCulture.ToString();
                    const int gameIdColumn = 1;
                    const int dateColumn = 5;
                    const int timeColumn = 6;

                    try
                    {
                        foreach (IXLRow row in workSheet.Rows())
                        {
                            if (firstRow)
                            {
                                firstRow = false;
                            }
                            else
                            {
                                int gameId;
                                DateTime date;
                                if (int.TryParse(row.Cell(gameIdColumn).Value.ToString(), out gameId) &&
                                    DateTime.TryParseExact(row.Cell(dateColumn).Value.ToString(), "dd/MM/yyyy",
                                        new CultureInfo(localCulture), new DateTimeStyles(), out date))
                                {
                                    games.Add(new ExcelGameDto
                                    {
                                        GameId = gameId,
                                        Date = date,
                                        Time = row.Cell(timeColumn).Value.ToString()
                                    });
                                }
                            }
                        }

                        gamesRepo.UpdateGamesDate(games);
                    }
                    catch (Exception ex)
                    {
                        // ignored
                    }
                }

            }
            if (viewModel.ClubId.HasValue)
            {
                return RedirectToAction("Edit",
                    new { Id = viewModel.TeamId, clubId = viewModel.ClubId, seasonId = viewModel.SeasonId });
            }

            return RedirectToAction("Edit",
                new { Id = viewModel.TeamId, currentLeagueId = viewModel.CurrentLeagueId, seasonId = viewModel.SeasonId });
        }

        [HttpPost]
        public JsonResult SaveGamesUrl(int teamId, string url, string teamName, int clubId, int seasonId, bool isScraper = false)
        {
            ProcessHelper.ClosePhantomJSProcess();
            try
            {
                var service = new ScrapperService();

                BaseRepo repo = new BaseRepo();
                int sectionId = repo.GetSectionByTeamId(teamId).SectionId;

                if (sectionId == 1)
                {
                    var result = service.SchedulerScraper(url);

                    service.Quit();

                    var isTeamExist = teamRepo.GetById(teamId) != null;


                    if (!isTeamExist && result.Any(t => t.HomeTeam != teamName || t.GuestTeam != teamName))
                    {
                        ProcessHelper.ClosePhantomJSProcess();
                        return Json(new { Success = false, Error = "Such team doesn't exist" });
                    }

                    var scheduleId = gamesRepo.SaveTeamGameUrl(teamId, url, clubId, teamName, seasonId);

                    gamesRepo.UpdateGamesSchedulesFromDto(result, clubId, scheduleId, url, isScraper);

                    ProcessHelper.ClosePhantomJSProcess();

                    return Json(new { Success = true, Data = result });
                }
                else if (sectionId == 7)
                {
                    var result = service.FootballSchedulerScraper(url);

                    service.Quit();

                    var isTeamExist = teamRepo.GetById(teamId) != null;


                    if (!isTeamExist && result.Any(t => t.HomeTeam != teamName || t.GuestTeam != teamName))
                    {
                        ProcessHelper.ClosePhantomJSProcess();
                        return Json(new { Success = false, Error = "Such team doesn't exist" });
                    }

                    var scheduleId = gamesRepo.SaveTeamGameUrl(teamId, url, clubId, teamName, seasonId);

                    gamesRepo.UpdateGamesSchedulesFromDto(result, clubId, scheduleId, url, isScraper);

                    ProcessHelper.ClosePhantomJSProcess();

                    return Json(new { Success = true, Data = result });
                }
                else
                {
                    ProcessHelper.ClosePhantomJSProcess();

                    return Json(new { Success = false, Error = "Such team doesn't exist" });
                }


            }
            catch (Exception e)
            {
                ProcessHelper.ClosePhantomJSProcess();

                return Json(new { Success = false, Error = e.Message });
            }

        }

        [HttpPost]
        public JsonResult SaveTeamStandingGameUrl(int teamId, int clubId, string url, string teamName, int seasonId)
        {
            ProcessHelper.ClosePhantomJSProcess();
            try
            {
                var service = new ScrapperService();

                BaseRepo repo = new BaseRepo();
                int sectionId = repo.GetSectionByTeamId(teamId).SectionId;

                if (sectionId == 1)
                {
                    var isTeamExist = teamRepo.GetById(teamId) != null;
                    var result = service.StandingScraper(url);

                    if (!isTeamExist && result.Any(t => t.Team != teamName))
                    {
                        ProcessHelper.ClosePhantomJSProcess();
                        return Json(new { Success = false, Error = "Such team not exist." });
                    }

                    int standingGameId = teamRepo.SaveTeamStandingUrl(teamId, clubId, url, teamName, seasonId);

                    bool isSuccess = standingGameId > 0;

                    if (isSuccess)
                    {
                        gamesRepo.UpdateTeamStandingsFromModel(result, standingGameId, url);

                        service.Quit();
                    }
                    ProcessHelper.ClosePhantomJSProcess();
                    return Json(new { Success = isSuccess, Data = result });
                }
                else if (sectionId == 7)
                {
                    var isTeamExist = teamRepo.GetById(teamId) != null;
                    var result = service.SoccerStandingScraper(url);

                    if (!isTeamExist && result.Any(t => t.Team != teamName))
                    {
                        ProcessHelper.ClosePhantomJSProcess();
                        return Json(new { Success = false, Error = "Such team not exist." });
                    }

                    int standingGameId = teamRepo.SaveTeamStandingUrl(teamId, clubId, url, teamName, seasonId);

                    bool isSuccess = standingGameId > 0;

                    if (isSuccess)
                    {
                        gamesRepo.UpdateTeamStandingsFromModel(result, standingGameId, url);

                        service.Quit();
                    }
                    ProcessHelper.ClosePhantomJSProcess();
                    return Json(new { Success = isSuccess, Data = result });
                }
                else
                {
                    ProcessHelper.ClosePhantomJSProcess();
                    return Json(new { Success = false, Error = "Such team doesn't exist" });
                }

            }
            catch (Exception e)
            {
                ProcessHelper.ClosePhantomJSProcess();
                return Json(new { Sucess = false, Error = e.Message });
            }

        }

        [HttpGet]
        public ActionResult Benefactor(int teamId, int? leagueId, int? seasonId, int? unionId)
        {
            var benefactor = teamRepo.GetBenefactorByTeamId(teamId);
            var league = leagueRepo.GetById(leagueId ?? 0);

            var model = new BenefactorViewModel(benefactor)
            {
                LeagueId = leagueId,
                SeasonId = seasonId,
                UnionId = unionId,
                IsEilatTournament = league?.EilatTournament == true
            };


            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.IsReadonly = model.IsApproved == true;

            return PartialView("_Benefactor", model);
        }

        [HttpPost]
        public async Task<ActionResult> BenefactorSave(BenefactorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isNewBenefactor = false;

                var benefactor = teamRepo.GetBenefactorByTeamId(model.TeamId);
                if (benefactor == null)
                {
                    benefactor = new TeamBenefactor();
                    benefactor.TeamId = model.TeamId;
                    isNewBenefactor = true;
                }
                benefactor.FinancingInsurance = model.FinancingInsurance;
                benefactor.PlayerCreditAmount = model.PlayerCreditAmount;
                benefactor.MaximumPlayersFunded = model.MaximumPlayersFunded;
                benefactor.Name = model.Name;
                benefactor.Comment = model.Comment;
                if (model.IsApproved == true)
                {
                    benefactor.ApprovedDate = DateTime.Now;
                    benefactor.ApprovedUserId = AdminId;
                    benefactor.IsApproved = true;

                    CalculateBenefactorPrices(benefactor, model.LeagueId ?? 0);
                }
                else if (model.IsApproved == false)
                {
                    db.PlayersBenefactorPrices.RemoveRange(benefactor.PlayersBenefactorPrices);
                }

                var emailService = new EmailService();

                var team = teamRepo.GetById(model.TeamId);

                #region Team link

                var hostBaseUrl = System.Configuration.ConfigurationManager.AppSettings["SiteUrl"];

                var teamUrl = string.Format("{0}/Teams/Edit/{1}",
                    hostBaseUrl,
                    model.TeamId);

                var teamUrlParams = string.Empty;

                if (model.SeasonId.HasValue)
                {
                    teamUrlParams += "?seasonId=" + model.SeasonId.Value;
                }

                if (model.LeagueId.HasValue)
                {
                    if (teamUrlParams.Length == 0)
                    {
                        teamUrlParams += "?";
                    }
                    else
                    {
                        teamUrlParams += "&";
                    }

                    teamUrlParams += "currentLeagueId=" + model.LeagueId.Value;
                }

                if (model.UnionId.HasValue)
                {
                    if (teamUrlParams.Length == 0)
                    {
                        teamUrlParams += "?";
                    }
                    else
                    {
                        teamUrlParams += "&";
                    }

                    teamUrlParams += "unionId=" + model.UnionId.Value;
                }

                teamUrl += teamUrlParams;

                #endregion

                var emailBody = $@"
                                    {Messages.TeamBenefactor_FormTitle} '{team.Title}'.
                                    <br />
                                    <br />
                                    {Messages.TeamBenefactor_BenefactorName}: {model.Name}<br />
                                    {Messages.TeamBenefactor_PlayerCreditAmount}: {model.PlayerCreditAmount}<br />
                                    {Messages.TeamBenefactor_MaximumPlayersFunded}: {model.MaximumPlayersFunded}<br />
                                    {Messages.TeamBenefactor_FinancingInsurance}: {(model.FinancingInsurance == true ? Messages.Yes : Messages.No)}<br />
                                    {Messages.TeamBenefactor_TotalAmountFunding}: {(model.MaximumPlayersFunded.GetValueOrDefault(0) * model.PlayerCreditAmount.GetValueOrDefault(0))}<br />
                                    {Messages.Comment}: {model.Comment}<br />
                                    <br /><br />
                                    <a href=""{teamUrl}"">{Messages.TeamBenefactor_OpenTeam}</a>
                                    ";

                var unionId = teamRepo.GetTeamsUnion(model.TeamId);
                var unionManagers = usersRepo.GetUnionWorkers(unionId, JobRole.UnionManager, model.SeasonId);

                foreach (var manager in unionManagers)
                {
                    var email = manager.Email;

#if DEBUG
                    email = "info@loglig.com";
#endif

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        try
                        {
                            //await emailService.SendAsync(email, emailBody);
                            emailService.SendAsync(email, emailBody);
                        }
                        catch (Exception ex)
                        {
                            log.Error(Messages.TeamBenefactor_SendEmailLogException, ex);
                        }
                    }

                }

                if (isNewBenefactor)
                {
                    teamRepo.CreateBenefactor(benefactor);
                }
                else
                {
                    teamRepo.Save();
                }

                ViewBag.Saved = true;
            }

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            return PartialView("_Benefactor", model);
        }

        private void CalculateBenefactorPrices(TeamBenefactor benefactor, int leagueId)
        {
            var oldBenefactorPrices = benefactor.PlayersBenefactorPrices;
            db.PlayersBenefactorPrices.RemoveRange(oldBenefactorPrices);

            var league = leagueRepo.GetById(leagueId);
            if (league == null)
            {
                throw new Exception(Messages.LeagueNotFound);
            }
            var teamPlayers = playersRepo.GetTeamPlayers(benefactor.TeamId, 0, league.LeagueId, getCurrentSeason()?.Id ?? 0);

            foreach (var teamPlayer in teamPlayers.Where(x => x.IsActive && !x.IsPlayerRegistered).Take(benefactor.MaximumPlayersFunded ?? 0))
            {
                var baseInsurancePrice = league.LeaguesPrices
                    .FirstOrDefault(x => x.PriceType == (int?)LeaguePriceType.PlayerInsurancePrice &&
                                         x.StartDate <= DateTime.Now &&
                                         x.EndDate > DateTime.Now)
                    ?.Price;
                var baseRegistrationPrice = league.LeaguesPrices
                    .FirstOrDefault(x => x.PriceType == (int?)LeaguePriceType.PlayerRegistrationPrice &&
                                         x.StartDate <= DateTime.Now &&
                                         x.EndDate > DateTime.Now)
                    ?.Price;

                var newRegistrationPrice = 0m;
                var newInsurancePrice = 0m;

                if (!teamPlayer.IsPlayerRegistered && teamPlayer.IsActive)
                {
                    var amount = benefactor.PlayerCreditAmount.GetValueOrDefault(0);
                    if (benefactor.FinancingInsurance == true)
                    {
                        if (baseInsurancePrice != null)
                        {
                            newInsurancePrice = Math.Max(0, baseInsurancePrice.Value - amount);

                            amount = Math.Max(amount - baseInsurancePrice.Value, 0);

                            newRegistrationPrice = amount > 0
                                ? Math.Max(0, baseRegistrationPrice.GetValueOrDefault(0) - amount)
                                : Math.Max(0, baseRegistrationPrice.GetValueOrDefault(0));
                        }
                        else
                        {
                            newRegistrationPrice = 0m;
                        }
                    }
                    else
                    {
                        newRegistrationPrice = baseRegistrationPrice != null
                            ? Math.Max(0, baseRegistrationPrice.GetValueOrDefault(0) - amount)
                            : 0m;

                        newInsurancePrice = baseInsurancePrice != null
                            ? Math.Max(0, baseInsurancePrice.GetValueOrDefault(0))
                            : 0m;
                    }
                }
                else
                {
                    newInsurancePrice = baseInsurancePrice ?? 0;
                    newRegistrationPrice = baseRegistrationPrice ?? 0;
                }

                benefactor.PlayersBenefactorPrices.Add(new PlayersBenefactorPrice
                {
                    BenefactorId = benefactor.BenefactorId,
                    PlayerId = teamPlayer.UserId,
                    TeamId = teamPlayer.TeamId,
                    LeagueId = league.LeagueId,
                    SeasonId = getCurrentSeason().Id,
                    RegistrationPrice = newRegistrationPrice,
                    InsurancePrice = newInsurancePrice
                });
            }
        }

        [HttpGet]
        public ActionResult ApproveBenefactor(int id, int? leagueId, bool approved)
        {

            var benefactor = teamRepo.GetBenefactorByTeamId(id);

            if (benefactor != null)
            {

                benefactor.ApprovedDate = DateTime.Now;
                benefactor.IsApproved = approved;
                benefactor.ApprovedUserId = AdminId;

                if (approved)
                {
                    CalculateBenefactorPrices(benefactor, leagueId ?? 0);
                }
                else
                {
                    db.PlayersBenefactorPrices.RemoveRange(benefactor.PlayersBenefactorPrices);
                }

                teamRepo.Save();

                return new JsonResult { Data = new { result = true, message = Messages.DataSavedSuccess }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            return new JsonResult { Data = new { result = false, message = Messages.TeamBenefactor_NotFound }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult DeleteBenefactor(int id)
        {
            var benefactor = teamRepo.GetBenefactorByTeamId(id);

            if (benefactor != null)
            {
                db.PlayersBenefactorPrices.RemoveRange(benefactor.PlayersBenefactorPrices);
                db.TeamBenefactors.Remove(benefactor);

                teamRepo.Save();

                return new JsonResult() { Data = new { result = true, message = Messages.DataSavedSuccess }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else
            {
                return new JsonResult() { Data = new { result = false, message = Messages.TeamBenefactor_NotFound }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public ActionResult SaveTrainingSettings(TrainingSettingForm model)
        {
            try
            {
                TrainingSetting viewModel = new TrainingSetting()
                {
                    DurationTraining = model.DurationTraining,
                    ConsiderationHolidays = model.ConsiderationHolidays,
                    TrainingSameDay = model.TrainingSameDay,
                    TrainingBeforeGame = model.TrainingBeforeGame,
                    MinNumTrainingDays = model.MinNumTrainingDays,
                    NoTwoTraining = model.NoTwoTraining,
                    TrainingFollowDay = model.TrainingFollowDay,
                    TeamID = model.TeamID,
                    NoDayAfterDayTrainings = model.DontSetDayAfterDay
                };
                objTrainingSettingsRepo.InsertTrainingSettings(viewModel);
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction("Edit", new { id = model.TeamID, seasonId = model.seasonId });
        }

        [HttpPost]
        public ActionResult SaveTrainingDaySettings(TrainingSettingForm model)
        {
            try
            {
                TrainingDaysSetting viewmodel = new TrainingDaysSetting();

                viewmodel.TeamId = model.TeamID;
                viewmodel.TrainingDay = model.TrainingDays;
                viewmodel.AuditoriumId = Convert.ToInt32(model.ChooseAuditorium);
                viewmodel.TrainingEndTime = model.EndTime;
                viewmodel.TrainingStartTime = model.StartTime;

                objTrainingSettingsRepo.InsertTrainingDaysSettings(viewmodel);
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction("Edit", new { id = model.TeamID, seasonId = model.seasonId });
        }

        private List<SelectListItem> BindAuditoriums(int id, int? clubId, int seasonId)
        {
            int unionId = teamRepo.GetTeamsUnion(id);

            var vm = new TeamsAuditoriumForm();
            vm.TeamId = id;
            vm.TeamAuditoriums = auditoriumsRepo.GetByTeam(id);
            vm.SeasonId = seasonId;


            List<SelectListItem> items = new List<SelectListItem>();
            List<TeamsAuditorium> list = new List<TeamsAuditorium>();

            SelectListItem defaultItem = new SelectListItem();

            defaultItem.Text = $"{Messages.Select}";
            defaultItem.Value = "0";
            defaultItem.Selected = true;

            if (vm.TeamAuditoriums != null)
            {
                foreach (var item in vm.TeamAuditoriums)
                {
                    items.Add(new SelectListItem { Value = item.AuditoriumId.ToString(), Text = item.Auditorium.Name });
                }

                items.Insert(0, defaultItem);
            }
            return items;
        }

        public ActionResult TrainingSettings(int id, int? clubId, int seasonId, string sort, string sortdir)
        {
            string section = secRepo.GetSectionByTeamId(id).Alias;
            var vm = new TrainingSettingForm();
            vm.listdata = new List<TrainingSettingForm>();
            var list = BindAuditoriums(id, clubId, seasonId);
            var newlist = (from t1 in db.TrainingDaysSettings
                           join t2 in db.Auditoriums on t1.AuditoriumId equals t2.AuditoriumId
                           where t1.TeamId == id
                           select new { t1.TrainingSettingsId, t2.Name, t1.TrainingDay, t1.TrainingEndTime, t1.TrainingStartTime }).OrderByDescending(x => x.TrainingSettingsId).ToList();

            string html = $"<table id='tblTrainningSchedule' class='tablesorter'><thead><tr><th>{UIHelpers.GetAuditoriumCaption(section)}</th><th>{Messages.TrainingDays}</th><th>{Messages.StartTime}</th><th>{Messages.EndTime}</th><th></th></tr></thead><tbody>";


            foreach (var item in newlist)
            {

                html += "<tr>";
                html += "<td>" + item.Name + "</td>";
                html += "<td>" + item.TrainingDay + "</td>";
                html += "<td>" + item.TrainingStartTime + "</td>";
                html += "<td>" + item.TrainingEndTime + "</td>";
                html += "<td><input type='button' class='btn' onclick='DeleteRecords(" + item.TrainingSettingsId + ");' value ='" + Messages.Delete + "'/></td>";
                html += "</tr>";

                TrainingSettingForm ts = new TrainingSettingForm();

                ts.Id = item.TrainingSettingsId;
                ts.TrainingDays = item.TrainingDay;
                ts.StartTime = item.TrainingStartTime;
                ts.EndTime = item.TrainingEndTime;
                ts.Name = item.Name;
                vm.listdata.Add(ts);

            }

            html += "</tbody></table>";

            vm.tableHTML = html;

            setTrainingSettingDetails(vm, id);

            vm.auditoriumData = list;
            vm.TeamID = id;
            vm.clubId = clubId;
            vm.seasonId = seasonId;

            ViewBag.IsBlocked = false;
            if (clubId != null)
            {
                ViewBag.IsBlocked = _objTeamTrainingsRepo.CheckIfTeamIsBlocked(id, clubId, seasonId);
            }

            ViewBag.Section = section;
            return PartialView(vm);
        }

        public string ReloadGrid(int teamID)
        {
            var vm = new TrainingSettingForm();
            vm.listdata = new List<TrainingSettingForm>();

            var newlist = (from t1 in db.TrainingDaysSettings
                           join t2 in db.Auditoriums on t1.AuditoriumId equals t2.AuditoriumId
                           where t1.TeamId == teamID

                           select new { t1.TrainingSettingsId, t2.Name, t1.TrainingDay, t1.TrainingEndTime, t1.TrainingStartTime }).OrderByDescending(x => x.TrainingSettingsId).ToList();


            string html = @"<table id='tblTrainningSchedule' class='tablesorter'><thead><tr><th>" + Messages.Auditorium + "</th><th>" + Messages.TrainingDays + "</th><th>" + Messages.StartTime + "</th><th>" + Messages.EndTime + "</th><th></th></tr></thead><tbody>";




            foreach (var item in newlist)
            {

                html += "<tr>";
                html += "<td>" + item.Name + "</td>";
                html += "<td>" + item.TrainingDay + "</td>";
                html += "<td>" + item.TrainingStartTime + "</td>";
                html += "<td>" + item.TrainingEndTime + "</td>";
                html += "<td><input type='button' class='btn' onclick='DeleteRecords(" + item.TrainingSettingsId + ");' value ='" + Messages.Delete + "'/></td>";
                html += "</tr>";

                TrainingSettingForm ts = new TrainingSettingForm();

                ts.Id = item.TrainingSettingsId;
                ts.TrainingDays = item.TrainingDay;
                ts.StartTime = item.TrainingStartTime;
                ts.EndTime = item.TrainingEndTime;
                ts.Name = item.Name;
                vm.listdata.Add(ts);

            }

            html += "</tbody></table>";

            vm.tableHTML = html;

            string jData = "{tbl:" + vm.tableHTML + "}";

            return html;

        }

        [HttpPost]
        public ActionResult DeleteRecord(int id)
        {
            //var id = Convert.ToInt32(myHid);
            var vm = new TrainingSettingForm();

            var rec = db.TrainingDaysSettings.FirstOrDefault(s => s.TrainingSettingsId == id);
            if (rec != null)
            {
                db.TrainingDaysSettings.Remove(rec);
                db.SaveChanges();
            }
            //vm.listdata = new List<TrainingSettingForm>();

            //var newlist = (from t1 in db.TrainingDaysSettings
            //               join t2 in db.Auditoriums on t1.AuditoriumId equals t2.AuditoriumId
            //               select new { t1.TrainingSettingsId, t2.Name, t1.TrainingDay, t1.TrainingEndTime, t1.TrainingStartTime }).OrderByDescending(x => x.TrainingSettingsId).ToList();

            //foreach (var item in newlist)
            //{
            //    TrainingSettingForm ts = new TrainingSettingForm();

            //    ts.Id = item.TrainingSettingsId;
            //    ts.TrainingDays = item.TrainingDay;
            //    ts.StartTime = item.TrainingStartTime;
            //    ts.EndTime = item.TrainingEndTime;
            //    ts.Name = item.Name;
            //    vm.listdata.Add(ts);

            //}
            //return PartialView("_TrainingTeams", vm.listdata);

            return Json(new { Message = "Success" }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult getTrainingDaySettingDetails(int teamID, int audiID, string trainingDay)
        {
            TrainingDaysSetting obj = new TrainingDaysSetting();

            obj = objTrainingSettingsRepo.getTrainingDaysSettingsData_byTeamID(teamID, audiID, trainingDay);

            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        public void setTrainingSettingDetails(TrainingSettingForm form, int teamid)
        {
            TrainingSetting obj = new TrainingSetting();
            obj = objTrainingSettingsRepo.getTrainingSettingsData_byTeamID(teamid);

            if (obj.Id == 0)
            {
                form.Id = 0;
                form.DurationTraining = "0";
                form.TrainingBeforeGame = false;
                form.ConsiderationHolidays = false;
                form.TrainingSameDay = false;
                form.TrainingFollowDay = false;
                form.TrainingBeforeGame = false;
                form.NoTwoTraining = false;
                form.MinNumTrainingDays = obj.MinNumTrainingDays;
            }
            else
            {
                form.Id = obj.Id;
                form.DurationTraining = obj.DurationTraining;
                form.TrainingBeforeGame = obj.TrainingBeforeGame;
                form.ConsiderationHolidays = obj.ConsiderationHolidays;
                form.TrainingSameDay = obj.TrainingSameDay;
                form.TrainingFollowDay = obj.TrainingFollowDay;
                form.TrainingBeforeGame = obj.TrainingBeforeGame;
                form.NoTwoTraining = obj.NoTwoTraining;
                form.MinNumTrainingDays = obj.MinNumTrainingDays;
                form.DontSetDayAfterDay = obj.NoDayAfterDayTrainings;
            }
        }




        [HttpPost]
        public ActionResult GenerateTraining(TrainingSettingForm viewModel)
        {
            try
            {
                var teamId = viewModel.TeamID;
                var startDate = viewModel.StartDate ?? DateTime.Now;
                var endDate = viewModel.EndDate ?? DateTime.Now;

                var teamsCount = _objTeamTrainingsRepo.GetAllTeamTrainingsByTeamId(viewModel.TeamID, viewModel.seasonId).Count();
                if (teamsCount != 0)
                {
                    _objTeamTrainingsRepo.RemoveTeamTrainingByTeamId(viewModel.TeamID);
                }

                var trainingSettings = objTrainingSettingsRepo.getTrainingSettingsData_byTeamID(teamId);
                var trainingDaySettings = _objTeamTrainingsRepo.GetTrainingDaysSettingsForTeam(teamId);
                var gameDates = _objTeamTrainingsRepo.GetDatesOfGames(teamId, startDate, endDate);


                var datesInterval = DateTimeHelper.GetDaysInterval(startDate, endDate);
                var teamTrainings = new List<TeamTrainingViewModel>();

                #region Get all training values

                for (int i = 0; i < datesInterval.Count; i++)
                {
                    foreach (var day in trainingDaySettings)
                    {
                        if (day.TrainingDay == datesInterval[i].DayOfWeek.ToString())
                        {
                            var selectedTeamTraining = trainingDaySettings
                                .Where(d => d.TrainingDay == day.TrainingDay && d.AuditoriumId == day.AuditoriumId)
                                .FirstOrDefault();
                            var trainingHourAvailability = (TimeSpan.Parse(day.TrainingEndTime) - TimeSpan.Parse(day.TrainingStartTime)).TotalMinutes;
                            var trainingDuration = Int32.Parse(trainingSettings.DurationTraining);
                            if (trainingHourAvailability >= trainingDuration)
                            {
                                var trainingDateString = $"{datesInterval[i].Date.ToString("yyyy-MM-dd")} {day.TrainingStartTime}:00";
                                var trainingDate = DateTime.ParseExact(trainingDateString, "yyyy-MM-dd HH:mm:ss",
                                    CultureInfo.InvariantCulture);

                                var teamTraining = new TeamTrainingViewModel()
                                {
                                    AuditoriumId = selectedTeamTraining.AuditoriumId,
                                    TrainingDate = trainingDate
                                };
                                teamTrainings.Add(teamTraining);
                            }
                        }
                    }
                }
                #endregion


                #region Filter trainings
                var isConsiderationHolidays = trainingSettings.ConsiderationHolidays;
                var dontAddTrainingSameDay = trainingSettings.TrainingSameDay;
                var dontAddTrainingOnFollowDay = trainingSettings.TrainingFollowDay;
                var addTrainingBeforeGame = trainingSettings.TrainingBeforeGame;
                var dontLet2Trainings = trainingSettings.NoTwoTraining;
                var coachTeams = _objTeamTrainingsRepo.GetCoachTeams(teamId);
                //Add a day before game
                if (addTrainingBeforeGame)
                {
                    foreach (var gameDate in gameDates)
                    {
                        var dayBeforeGame = gameDate.AddDays(-1);
                        teamTrainings.Add(new TeamTrainingViewModel
                        {
                            AuditoriumId = null,
                            TrainingDate = dayBeforeGame
                        });
                    }
                }

                //dont add training on the same day as a game
                if (dontAddTrainingOnFollowDay)
                {
                    foreach (var teamTraining in teamTrainings.ToList())
                    {
                        foreach (var game in gameDates.ToList())
                        {
                            var followDayOfGame = game.AddDays(1);
                            var followDayTraining = teamTrainings
                                .Where(t => t.TrainingDate.Date == followDayOfGame.Date)
                                .FirstOrDefault();
                            if (teamTraining.TrainingDate.Date == followDayOfGame.Date)
                            {
                                teamTrainings.Remove(followDayTraining);
                            }
                        }
                    }
                }

                //Dont add training on the following day as a game
                if (dontAddTrainingSameDay)
                {
                    foreach (var teamTraining in teamTrainings.ToList())
                    {
                        foreach (var game in gameDates.ToList())
                        {
                            if (teamTraining.TrainingDate.Date == game.Date)
                            {
                                teamTrainings.Remove(teamTraining);
                            }
                        }
                    }
                }

                //Leave only one training at the same day 
                if (dontLet2Trainings)
                {
                    var distinctTrainings = teamTrainings.DistinctBy(t => t.TrainingDate.Date).ToList();
                    teamTrainings = distinctTrainings;
                }

                //Delete all holidays
                if (isConsiderationHolidays)
                {
                    var holidays = new Holidays();
                    var holidaysList = holidays.GetAllHolidaysDates();
                    teamTrainings.RemoveHolidays(holidaysList);
                }

                //Check if coach train more than one team and not add trainings if there are conflicts in time
                if (coachTeams.Count > 1)
                {
                    var listOfTeamDates = new List<DateTime>();
                    for (int i = 0; i < coachTeams.Count; i++)
                    {
                        var teamsTrainingDates = _objTeamTrainingsRepo.GetAllTeamTrainingsByTeamId(coachTeams[i].TeamId, viewModel.seasonId)
                            .Select(training => training.TrainingDate)
                            .ToList();
                        listOfTeamDates.AddRange(teamsTrainingDates);
                    }
                    var repeatedDates = listOfTeamDates.GroupBy(date => date)
                        .Where(date => date.Count() > 1)
                        .Select(group => group.Key)
                        .ToList();

                    foreach (var date in repeatedDates)
                    {
                        teamTrainings.RemoveAll(t => t.TrainingDate == date);
                    }
                }
                #endregion

                foreach (var teamTraining in teamTrainings)
                {
                    var model = new TeamTraining
                    {
                        Title = Messages.Training,
                        TeamId = teamId,
                        AuditoriumId = teamTraining.AuditoriumId,
                        Content = string.Empty,
                        TrainingDate = teamTraining.TrainingDate,
                        SeasonId = viewModel.seasonId

                    };
                    _objTeamTrainingsRepo.InsertTeamTrainings(model);
                }
            }

            catch (Exception ex)
            {
            }

            TempData["ShowTrainings"] = true;
            return RedirectToAction(nameof(Edit), new
            {
                id = viewModel.TeamID,
                seasonId = viewModel.seasonId,
                clubId = viewModel.clubId
            });
        }

        /// <summary>
        /// Add team trainings to the view model
        /// </summary>
        /// <param name="teamTrainings"> table information</param>
        /// <param name="vm">view model</param>
        /// <returns>View model of team training</returns>
        private List<TeamTrainingViewModel> AddTeamTrainingToViewModel(List<TeamTraining> teamTrainings, List<TeamTrainingViewModel> vm)
        {
            foreach (var training in teamTrainings)
            {
                var teamTraining = new TeamTrainingViewModel
                {
                    Id = training.Id,
                    Title = training.Title,
                    TrainingReport = training.TrainingReport,
                    TeamId = training.TeamId,
                    AuditoriumId = training.AuditoriumId,
                    Content = training.Content,
                    TrainingDate = training.TrainingDate,
                    isPublished = training.isPublished
                };
                vm.Add(teamTraining);
            }
            return vm;
        }

        [HttpPost]
        public ActionResult TeamTrainings(int id, int? seasonId, int pageNumber = 1, int pageSize = 10)
        {
            return TeamTrainings(id, 1, seasonId, pageNumber, pageSize);
        }

        [HttpGet]
        public ActionResult TeamTrainings(int id, int? clubId, int? seasonId, int pageNumber = 1, int pageSize = 10)
        {
            TrainingsPageModel model = new TrainingsPageModel();
            var teamsTrainings = _objTeamTrainingsRepo.GetTrainingsOfFirstDaysOfMonth(id, seasonId ?? 0).OrderBy(x=>x.TrainingDate).ToList(); 
            var vm = new List<TeamTrainingViewModel>();
            ViewBag.Players = _objTeamTrainingsRepo.GetPlayersByTeamId(id, seasonId);
            var playersDictionary = _objTeamTrainingsRepo.GetAttendancesByTeamId(id);
            ViewBag.SelectedPlayers = playersDictionary;
            ViewBag.AuditoriumArena = _objTeamTrainingsRepo.GetTeamArenas(id);
            ViewBag.TeamId = id;
            ViewBag.SeasonId = seasonId;
            ViewBag.Section = secRepo.GetSectionByTeamId(id).Alias;

            vm = AddTeamTrainingToViewModel(teamsTrainings, vm);
            model.TeamTrainings = vm.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
            var pager = new Pager(vm.Count(), pageNumber, pageSize);
            model.Pager = pager;
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteTeamTraining(int id)
        {
            _objTeamTrainingsRepo.RemoveTeamTrainingById(id);
            return Json(new { Message = $"Team training with id {id} was deleted!" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateTeamTraining(int teamId, int id, string title, int? auditoriumId, DateTime date, string content,
            IEnumerable<string> playersId, bool isImageDeleted, bool dateApproved = false)
        {
            var reportChanged = false;
            var reportPath = string.Empty;
            var games = _objTeamTrainingsRepo.GetDatesOfGames(teamId, date);
            if (!dateApproved)
            {
                foreach (var gameDate in games)
                {
                    if (gameDate.Date == date.Date)
                    {
                        return Json(new
                        {
                            Message = Messages.TrainingAtTheGameDateAlert.Replace("{0}", date.ToShortDateString()),
                            Date = date.ToString("dd/MM/yyyy HH:mm")
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            int seasonId = getCurrentSeason().Id;

            NotesMessagesRepo notesRep = new NotesMessagesRepo();

            String message = String.Format("Training has been updated: {0}", date);

            notesRep.SendToTeam(seasonId, teamId, message);

            var notsServ = new GamesNotificationsService();
            notsServ.SendPushToDevices(GlobVars.IsTest);

            //upload training report and add to file server
            string trainingReportName = null;
            var maxFileSize = GlobVars.MaxFileSize * 1000;
            HttpPostedFileBase postedFile = GetPostedFile("ImageFile");

            if (postedFile != null && postedFile.ContentLength > 0)
            {
                if (postedFile.ContentLength > maxFileSize)
                {
                    return Json(new { stat = "failed", id = id, Message = Messages.FileSizeError });
                }
                else
                {
                    var newName = SaveFile(postedFile, $"trainingReport_{id}_");
                    if (newName == null)
                    {
                        return Json(new { stat = "failed", id = id, Message = Messages.FileError });
                    }
                    else
                    {
                        reportChanged = true;
                        reportPath = GlobVars.ContentPath + "/teams/" + newName;
                        trainingReportName = newName;
                    }
                }
            }
            else
            {
                if (isImageDeleted)
                {
                    trainingReportName = string.Empty;
                }
            }

            //if (!string.IsNullOrEmpty(gc.RefereeIds))
            //{
            //    var ids = gc.RefereeIds.Split(',').Select(int.Parse).ToList();
            //    notesRep.SendToUsers(ids, message);
            //}

            _objTeamTrainingsRepo.UpdateTeamTrainingById(id, title, auditoriumId, date, content, playersId, trainingReportName);
            return Json(new { Message = "", stat = "ok", reportChanged, isImageDeleted, reportPath }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Filter(int teamId, DateTime? startFilterDate, DateTime? endFilterDate, string filterValue, int seasonId, int pageNumber = 1, int pageSize = 10)
        {
            var vm = new List<TeamTrainingViewModel>();
            TrainingsPageModel model = new TrainingsPageModel();

            ViewBag.Players = _objTeamTrainingsRepo.GetPlayersByTeamId(seasonId, teamId);
            var playersDictionary = _objTeamTrainingsRepo.GetAttendancesByTeamId(teamId);
            ViewBag.SelectedPlayers = playersDictionary;
            ViewBag.AuditoriumArena = _objTeamTrainingsRepo.GetTeamArenas(teamId);
            ViewBag.Section = secRepo.GetSectionByTeamId(teamId).Alias;

            ViewBag.FilterStartDate = startFilterDate;
            ViewBag.FilterEndDate = endFilterDate;
            ViewBag.FilterValue = filterValue;
            ViewBag.TeamId = teamId;
            ViewBag.SeasonId = seasonId;
            switch (filterValue)
            {
                case "fltr_bom":
                    var listBom = _objTeamTrainingsRepo.GetTrainingsOfFirstDaysOfMonth(teamId, seasonId).OrderBy(x => x.TrainingDate).ToList(); ;
                    vm = AddTeamTrainingToViewModel(listBom, vm);
                    break;

                case "fltr_ranged":
                    var startDate = startFilterDate ?? DateTime.Now;
                    var endDate = endFilterDate ?? DateTime.Now;
                    var listRanged = _objTeamTrainingsRepo.GetTrainingsInDateRange(teamId, startDate, endDate, seasonId).OrderBy(x => x.TrainingDate).ToList(); ;
                    vm = AddTeamTrainingToViewModel(listRanged, vm);
                    break;
                case "fltr_all":
                    var listAll = _objTeamTrainingsRepo.GetAllTeamTrainingsByTeamId(teamId, seasonId).OrderBy(x => x.TrainingDate).ToList(); ;
                    vm = AddTeamTrainingToViewModel(listAll, vm);
                    break;
                default:
                    throw new FormatException("Unknown type of select value!");
            }

            if (pageSize > 100)
            {
                model.TeamTrainings = vm;
            }
            else
            {
                model.TeamTrainings = vm.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
            }
            var pager = new Pager(vm.Count(), pageNumber, pageSize);
            model.Pager = pager;
            return PartialView("TeamTrainings", model);
        }

        private string GetPlayersNameString(IEnumerable<string> players)
        {
            var playerBuilder = new StringBuilder();
            foreach (var player in players)
            {
                playerBuilder.Append(player);
                playerBuilder.Append(",");
            }
            return playerBuilder.ToString();
        }
        private string GetPlayersIdsString(IEnumerable<int> players)
        {
            var playerBuilder = new StringBuilder();
            foreach (var player in players)
            {
                playerBuilder.Append(player);
                playerBuilder.Append(",");
            }
            return playerBuilder.ToString();
        }
        private void ExportTeamTrainingsToExcel(List<TeamTraining> teamTrainings, int teamId)
        {
            using (XLWorkbook workBook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var ws = workBook.AddWorksheet($"team_players_{teamId}");
                ws.Cell(1, 1).Value = "Id";
                ws.Cell(1, 2).Value = Messages.Title;
                ws.Cell(1, 3).Value = Messages.Date;
                ws.Cell(1, 4).Value = Messages.Time;
                ws.Cell(1, 5).Value = Messages.Auditorium;
                ws.Cell(1, 6).Value = Messages.Content;
                ws.Cell(1, 7).Value = Messages.Attendance;
                ws.Cell(1, 8).Value = "PlayersIds";
                ws.Columns(1, 8).AdjustToContents();

                int rowNum = 2;
                foreach (var row in teamTrainings)
                {
                    ws.Cell(rowNum, 1).SetValue(row.Id);
                    ws.Cell(rowNum, 2).SetValue(row.Title);
                    ws.Cell(rowNum, 3).SetValue(row.TrainingDate.ToShortDateString());
                    ws.Cell(rowNum, 4).SetValue(row.TrainingDate.ToString("HH:mm"));
                    string auditorium = row.Auditorium == null ? "" : row.Auditorium.Name;
                    ws.Cell(rowNum, 5).SetValue(auditorium);
                    ws.Cell(rowNum, 6).SetValue(row.Content);
                    var playersForTeamName = row.TrainingAttendances.Select(t => t.TeamsPlayer.User.FullName);
                    var playersForTeamIds = row.TrainingAttendances.Select(t => t.PlayerId);
                    var playersString = GetPlayersNameString(playersForTeamName);
                    var playersIds = GetPlayersIdsString(playersForTeamIds);
                    ws.Cell(rowNum, 7).SetValue(playersString);
                    ws.Cell(rowNum, 8).SetValue(playersIds);
                    rowNum++;
                }

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", $"attachment;filename= TeamTrainings-{teamId}.xlsx");

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
        public ActionResult ExportTeamTrainingsToXML(int teamId, DateTime startFilterDate, DateTime endFilterDate, string filterValue, int seasonId)
        {
            try
            {
                var teamTrainings = _objTeamTrainingsRepo.GetAllTeamTrainingsByTeamId(teamId, seasonId);
                var teamAttendances = _objTeamTrainingsRepo.GetAttendancesByTeamId(teamId);

                switch (filterValue)
                {
                    case "fltr_bom":
                        var listBom = _objTeamTrainingsRepo.GetTrainingsOfFirstDaysOfMonth(teamId, seasonId);
                        ExportTeamTrainingsToExcel(listBom, teamId);
                        break;

                    case "fltr_ranged":
                        var listRanged = _objTeamTrainingsRepo.GetTrainingsInDateRange(teamId, startFilterDate, endFilterDate, seasonId);
                        ExportTeamTrainingsToExcel(listRanged, teamId);
                        break;
                    case "fltr_all":
                        var listAll = _objTeamTrainingsRepo.GetAllTeamTrainingsByTeamId(teamId, seasonId);
                        ExportTeamTrainingsToExcel(listAll, teamId);
                        break;
                    default:
                        var listDefaullt = _objTeamTrainingsRepo.GetAllTeamTrainingsByTeamId(teamId, seasonId);
                        ExportTeamTrainingsToExcel(listDefaullt, teamId);
                        break;
                }
            }
            catch (Exception e)
            {

            }
            return Json(new { Message = $"Exported to excel!" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ImportTeamTrainingsDateFromXML(HttpPostedFileBase importFile, int teamId, int seasonId)
        {
            if (importFile != null)
            {
                using (XLWorkbook workBook = new XLWorkbook(importFile.InputStream))
                {
                    //Read the first Sheet from Excel file.
                    IXLWorksheet workSheet = workBook.Worksheet(1);
                    bool firstRow = true;

                    var excelTeamTrainings = new List<ExcelTeamTrainingDto>();

                    var localCulture = CultureInfo.CurrentCulture.ToString();
                    const int teamTrainingIdColumn = 1;
                    const int dateColumn = 3;
                    const int timeColumn = 4;

                    try
                    {
                        foreach (IXLRow row in workSheet.Rows())
                        {
                            if (firstRow)
                            {
                                firstRow = false;
                            }
                            else
                            {

                                int trainingId;
                                if (int.TryParse(row.Cell(teamTrainingIdColumn).Value.ToString(), out trainingId))
                                {
                                    var dateString = row.Cell(dateColumn).Value.ToString().Split(' ')[0];
                                    var timeString = row.Cell(timeColumn).RichText;
                                    var dateTimeString = $"{dateString} {timeString}";
                                    var dateTime = DateTime.ParseExact(dateTimeString, "dd/MM/yyyy HH:mm",
                                        System.Globalization.CultureInfo.InvariantCulture);
                                    excelTeamTrainings.Add(new ExcelTeamTrainingDto
                                    {
                                        TrainingId = trainingId,
                                        TrainingDate = dateTime
                                    });
                                }
                            }
                        }

                        _objTeamTrainingsRepo.UpdateDatesInTable(excelTeamTrainings, teamId);

                    }
                    catch (Exception ex)
                    {
                        // ignored
                    }
                }
            }
            #region GetUpdated values

            //var teamsTrainings = _objTeamTrainingsRepo.GetAllTeamTrainingsByTeamId(teamId);
            //var vm = new List<TeamTrainingViewModel>();
            //ViewBag.Players = _objTeamTrainingsRepo.GetPlayersByTeamId(teamId);
            //var playersDictionary = _objTeamTrainingsRepo.GetAttendancesByTeamId(teamId);
            //ViewBag.SelectedPlayers = playersDictionary;
            //ViewBag.AuditoriumArena = _objTeamTrainingsRepo.GetTeamArenas(teamId);
            //ViewBag.TeamId = teamId;
            //vm = AddTeamTrainingToViewModel(teamsTrainings, vm);

            #endregion

            return RedirectToAction("Edit", new
            {
                id = teamId,
                seasonId = seasonId
            });
        }

        [HttpPost]
        public ActionResult PublishToApp(int teamId)
        {
            var teamTrainings = db.TeamTrainings.Where(t => t.TeamId == teamId);
            var checkedValues = db.TeamTrainings.Where(t => t.TeamId == teamId).
                Select(t => t.isPublished).
                AsEnumerable();
            var isAllChecked = true;
            foreach (var check in checkedValues)
            {
                if (check == false)
                {
                    isAllChecked = false;
                }
            }
            if (isAllChecked)
            {
                foreach (var training in teamTrainings)
                {
                    training.isPublished = false;
                }
                db.SaveChanges();
                return Json(new { Message = $"Unchecked" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                foreach (var training in teamTrainings)
                {
                    training.isPublished = true;
                }
                db.SaveChanges();
                return Json(new { Message = $"Checked" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult PublishTraining(int trainingId, bool publishValue)
        {
            _objTeamTrainingsRepo.PublishTrainingById(trainingId, publishValue);
            return Json(new { Message = $"Training with id {trainingId} was published to app!" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TennisRegistrationStatus(int id, int seasonId, int? teamId)
        {
            var registrations = !teamId.HasValue  //.OrderBy(r => r.TennisPositionOrder ?? int.MaxValue).ThenBy(r => r.FullName)
                ? leagueRepo.GetAllTennisRegistrations(id, seasonId, false).OrderBy(c => c.TeamName).ThenBy(t => t.TeamPositionOrder ?? int.MaxValue).ThenBy(c => c.FullName).ToList()
                : leagueRepo.GetAllTennisRegistrations(id, seasonId, false).Where(c => c.TeamId == teamId.Value).OrderBy(t => t.TeamPositionOrder ?? int.MaxValue).ThenBy(c => c.FullName).ToList();

            ViewBag.LeagueId = id;
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueName = leagueRepo.GetById(id)?.Name;
            ViewBag.TeamId = teamId;
            if (teamId.HasValue)
            {
                ViewBag.TeamManager = jobsRepo.GetTeamManagerInfo(teamId.Value, seasonId);
            }

            return PartialView("_TennisRegistrationList", registrations);
        }

        public ActionResult DeleteTennisTeam(int teamId, int leagueId, int seasonId, int? unionId)
        {
            teamRepo.DeleteTennisTeam(teamId, leagueId, seasonId);
            return RedirectToAction(nameof(List), new { id = leagueId, seasonId, unionId });
        }

        public ActionResult RefereesList(int leagueId, int seasonId)
        {
            var refereeList = leagueRepo.GetLeagueRelatedRefereesByClubs(leagueId, seasonId);
            ViewBag.RefereesRegistered = leagueRepo.CountRegisteredReferees(leagueId, seasonId);
            ViewBag.LeagueId = leagueId;
            ViewBag.SeasonId = seasonId;
            ViewBag.CompetitionName = leagueRepo.GetById(leagueId)?.Name;
            ViewBag.SectionAlias = leagueRepo.GetSectionAlias(leagueId);

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);

            return PartialView("_RoutesRefereeRegistrations", refereeList);
        }

        [HttpPost]
        public async Task ApproveReferee(int id, bool isApproved)
        {
            var refereeReg = db.RefereeRegistrations.Find(id);
            if (refereeReg != null)
            {
                refereeReg.IsApproved = isApproved;
                await db.SaveChangesAsync();
            }
        }

        public void ExportRefereeRegistrationsToExcel(int leagueId, int seasonId)
        {
            var section = leagueRepo.GetSectionAlias(leagueId);
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var leagueName = leagueRepo.GetById(leagueId)?.Name;
                var refereeList = leagueRepo.GetLeagueRelatedRefereesByClubs(leagueId, seasonId);

                var ws = workbook.AddWorksheet($"{Messages.Referee} {Messages.RegistrationList.ToLowerInvariant()}");

                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                addCell(Messages.ClubName);
                addCell(Messages.Referee + Messages.Name);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                foreach (var row in refereeList)
                {
                    addCell(row.Key.Name);
                    addCell(string.Join(", ", row.Value.Select(x => x.UserFullName)));

                    rowCounter++;
                    columnCounter = 1;
                }
                ws.Columns().AdjustToContents();

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = $"{leagueName.Replace(' ', '_')}_{ Messages.Referee.ToLower()}" +
                        $"_{Messages.RegistrationCaption.Replace(Messages.Gymnastics.ToLower(), string.Empty)}.xlsx";

                Response.AddHeader("content-disposition", $"attachment;filename={fileName}");

                using (MemoryStream myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        public ActionResult Statistics(int id, int seasonId)
        {
            var guestStatistics = teamRepo.GetGuestGamesStatistics(id);
            var homeStatistics = teamRepo.GetHomeGamesStatistics(id);
            var seasonsIds = guestStatistics.Keys.Select(c => c.Id).Union(homeStatistics.Keys.Select(c => c.Id)).Distinct();
            var vm = new TeamStatisticsForm
            {
                SeasonId = seasonId,
                TeamId = id,
                GuestStatistics = guestStatistics,
                HomeStatistics = homeStatistics,
                // TODO
                GeneralStatistics = playersRepo.GetGeneralStatistics(guestStatistics, homeStatistics, id),
                Seasons = seasonsRepository.GetSeasons().Where(c => seasonsIds.Contains(c.Id))
            };
            return PartialView("_TeamStatistics", vm);
        }
        public JsonResult GetLevelSettings(int? levelSettingsId, int competitionId)
        {
            if (levelSettingsId.HasValue)
            {
                var levelSettings = db.LevelDateSettings
                    .FirstOrDefault(x =>
                        x.CompetitionLevelId == levelSettingsId.Value && x.CompetitionId == competitionId);

                if (levelSettings != null)
                {
                    return Json(new
                    {
                        levelSettings?.QualificationStartDate,
                        levelSettings?.QualificationEndDate,
                        levelSettings?.FinalStartDate,
                        levelSettings?.FinalEndDate
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
        }
    }


    public class ImportFromExcelViewModel
    {
        public int TeamId { get; set; }
        public int CurrentLeagueId { get; set; }
        public int SeasonId { get; set; }
        public HttpPostedFileBase ImportFile { get; set; }
        public int? ClubId { get; set; }
    }
}
