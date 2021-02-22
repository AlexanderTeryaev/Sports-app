using AppModel;
using CmsApp.Models;
using Omu.ValueInjecter;
using Resources;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsApp.Helpers;
using System.Collections.Generic;
using CmsApp.Helpers.Injections;
using DataService;
using System;
using System.Data.Entity;
using DataService.DTO;
using System.Globalization;
using Microsoft.Ajax.Utilities;
using System.Text;
using ClosedXML.Excel;
using Newtonsoft.Json;
using System.Threading.Tasks;
using DataService.Services;
using DataService.Utils;

namespace CmsApp.Controllers
{
    public class ClubsController : AdminController
    {
        private readonly ClubTeamTrainingsRepo _objClubTrainingsRepo;
        private readonly TeamTrainingsRepo _objTeamTrainingsRepo;
        private readonly List<string> _disciplineSections = new List<string> { "Athletics", "Gymnastic", "Motorsport" };
        private const string ImportGymnasticsErrorResultSessionKey = "ImportGymnastics_ErrorResult";
        private const string ImportGymnasticsErrorResultFileNameSessionKey = "ImportGymnastics_ErrorResultFileName";
        private readonly ClubBalanceService cbsServ;
        public static string ExportTotoSessionKey = "ExportTotoSessionKey";
        public static string ExportTotoFileNameSessionKey = "ExportTotoFileNameSessionKey";
        public static string ExportTotoSessionKeyTimeOut = "ExportTotoSessionTimeOut";

        public ClubsController()
        {
            _objClubTrainingsRepo = new ClubTeamTrainingsRepo();
            _objTeamTrainingsRepo = new TeamTrainingsRepo();
            cbsServ = new ClubBalanceService(db);
        }

        [HttpPost]
        public ActionResult AddTeamToSchool(AddTeamToSchoolModel model)
        {
            var school = SchoolRepo.GetCollection(x => x.Id == model.SchoolId).FirstOrDefault();

            if (school != null)
            {
                if (!string.IsNullOrWhiteSpace(model.NewTeamName))
                {
                    var team = new Team { Title = model.NewTeamName.Trim() };
                    teamRepo.Create(team);

                    model.TeamId = team.TeamId;
                    //return RedirectToAction("Edit", new { id = school.ClubId, seasonId = school.SeasonId });
                }

                school.SchoolTeams.Add(new SchoolTeam
                {
                    SchoolId = school.Id,
                    TeamId = model.TeamId
                });

                SchoolRepo.Save();
                var isDepartment = SchoolRepo.GetById(school.Id)?.Club?.ParentClub != null ? true : false;
                if (isDepartment)
                {
                    return RedirectToAction("Edit",
                        new { id = school.ClubId, seasonId = school.SeasonId, isDepartment = isDepartment });
                }

                return RedirectToAction("Edit", new { id = school.ClubId, seasonId = school.SeasonId });
            }

            return null;
        }

        [HttpPost]
        public ActionResult RemoveSchool(int id)
        {
            var school = SchoolRepo.GetCollection(x => x.Id == id).FirstOrDefault();

            if (school != null)
            {
                db.SchoolTeams.RemoveRange(school.SchoolTeams);
                db.Schools.Remove(school);

                SchoolRepo.Save();

                return RedirectToAction("Edit", new { id = school.ClubId, seasonId = school.SeasonId });
            }

            return null;
        }

        [HttpPost]
        public ActionResult RemoveFromSchool(int clubId, int schoolId, int teamId, int seasonId, bool isCamp = false)
        {
            var club = clubsRepo.GetById(clubId);
            var school = club.Schools.FirstOrDefault(x => x.Id == schoolId);
            var teamToRemove = school?.SchoolTeams.FirstOrDefault(x => x.TeamId == teamId);
            if (teamToRemove != null)
            {
                db.SchoolTeams.Remove(teamToRemove);

                clubsRepo.Save();
            }

            return RedirectToAction("SchoolTeams", new { id = clubId, seasonId, isCamp });
        }

        [HttpPost]
        public ActionResult AddSchool(AddSchoolModel model)
        {
            var club = clubsRepo.GetById(model.ClubId);
            var isDepartment = club.ParentClub == null ? false : true;
            club.Schools.Add(new School
            {
                ClubId = club.ClubId,
                SeasonId = model.SeasonId,
                Name = model.Name,
                CreatedBy = Convert.ToInt32(User.Identity.Name),
                DateCreated = DateTime.Now,
                IsCamp = model.IsCamp
            });

            clubsRepo.Save();

            return RedirectToAction("Edit",
                new { id = club.ClubId, seasonId = model.SeasonId, isDepartment = isDepartment });
        }

        public ActionResult SchoolTeams(int id, int? seasonId, bool isCamp = false)
        {
            if (!seasonId.HasValue)
            {
                return Content(Messages.NoSeasonSelected);
            }

            var club = clubsRepo.GetById(id);
            var schools = club.Schools.Where(x => x.SeasonId == seasonId && x.IsCamp == isCamp).ToList();

            var teamsInSchools = new List<Team>();
            foreach (var school in schools)
            {
                teamsInSchools.AddRange(school.SchoolTeams.Select(x => x.Team));
            }

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.ClubId = id;
            ViewBag.SeasonId = seasonId;
            return PartialView("_SchoolTeams",
                new SchoolTeamsModel
                {
                    ClubId = id,
                    SeasonId = seasonId.Value,
                    CanManageSchools = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.ClubManager) ||
                                       User.HasTopLevelJob(JobRole.ClubSecretary) || User.HasTopLevelJob(JobRole.DepartmentManager),
                    Schools = schools.OrderBy(x => x.SortOrder).ToList(),
                    AvailableTeams =
                        club.ClubTeams
                            .Where(x => x.SeasonId == seasonId &&
                                        !teamsInSchools.Select(ts => ts.TeamId).Contains(x.TeamId)).Select(x => x.Team)
                            .OrderBy(x => x.SortOrder).ToList(),
                    IsCamp = isCamp
                });
        }

        // GET: Positions
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListBySection(int id)
        {
            var vm = new ClubsForm
            {
                SectionId = id,
                isEditorOrUnionManager = User.IsInAnyRole(AppRole.Admins, AppRole.Editors)
                                         || User.HasTopLevelJob(JobRole.UnionManager),
                SeasonId = GetClubCurrentSeasonFromSession(),
                Clubs = clubsRepo.GetBySection(id)
            };
            return PartialView("_List", vm);
        }

        public ActionResult ListByUnion(int id, int seasonId, string sortingOrder, bool isFlowersOfSport = false)
        {
            var union = unionsRepo.GetById(id);
            var section = union?.Section;
            var vm = new ClubsForm
            {
                UnionId = id,
                SeasonId = seasonId,
                Clubs = clubsRepo.GetByUnion(id, seasonId, isFlowersOfSport),
                isEditorOrUnionManager = User.IsInAnyRole(AppRole.Admins, AppRole.Editors)
                                         || User.HasTopLevelJob(JobRole.UnionManager),
                ClubsRegistrations = clubsRepo.GetUnionClubActivityRegistrations(id, seasonId),
                IsIndividualSection = section?.IsIndividual == true,
                SectionAlias = section?.Name,
                IsFlowerOfSport = isFlowersOfSport,

                IsGymnasticUnion = string.Equals(section?.Name, SectionAliases.Gymnastic,
                    StringComparison.CurrentCultureIgnoreCase),
                CanApproveClubs = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager),
                IsRegionalLevelEnabled = union?.IsRegionallevelEnabled == true
            };
            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.FromUnion = true;
            ViewBag.SortingOrder = sortingOrder;
            vm.Clubs = SortClubs(vm.Clubs, sortingOrder);
            return !isFlowersOfSport ? PartialView("_List", vm) : PartialView("_FlowersList", vm);
        }

        private List<Club> SortClubs(List<Club> clubs, string sortingOrder)
        {
            switch (sortingOrder)
            {
                case "clubId_desc":
                    return clubs.OrderByDescending(x => x.ClubId).ToList();
                case "clubNumber":
                    return clubs.OrderBy(x => x.ClubNumber).ToList();
                case "clubNumber_desc":
                    return clubs.OrderByDescending(x => x.ClubNumber).ToList();
                case "clubId":
                    return clubs.OrderBy(x => x.ClubId).ToList();
                case "clubName_desc":
                    return clubs.OrderByDescending(x => x.Name).ToList();
                case "sportCenter":
                    if (IsHebrew)
                    {
                        return clubs.OrderBy(x => x.SportCenter?.Heb).ToList();
                    }
                    else
                    {
                        return clubs.OrderBy(x => x.SportCenter?.Eng).ToList();
                    }
                case "sportCenter_desc":
                    if (IsHebrew)
                    {
                        return clubs.OrderByDescending(x => x.SportCenter?.Heb).ToList();
                    }
                    else
                    {
                        return clubs.OrderByDescending(x => x.SportCenter?.Eng).ToList();
                    }
                default:
                    return clubs.OrderBy(x => x.Name).ToList();
            }
        }

        [HttpPost]
        public void ExportClubsList(int id, int seasonId, bool isFlowersOfSport = false)
        {
            var clubs = clubsRepo.GetByUnion(id, seasonId, isFlowersOfSport).ToList();
            var clubsInfo = clubsRepo.GetPlayersInformation(clubs, seasonId);

            // Dictionary values of clubsInfo:  int - clubId, int[] - 
            //first value - waiting for approval, 
            //second value - approved, 
            //third value - active players ( has more than 4 active competitions)

            var clubsRegistrations = clubsRepo.GetCollection<ActivityFormsSubmittedData>(x =>
                    x.Activity.UnionId == id &&
                    x.Activity.SeasonId == seasonId &&
                    x.Activity.Type == ActivityType.Club &&
                    x.Activity.IsAutomatic == true)
                .ToList();

            var unionName = unionsRepo.GetById(id)?.Name ?? string.Empty;

            using (var workbook =
                new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var ws = workbook.AddWorksheet(isFlowersOfSport ? Messages.FlowersOfSport : Messages.ClubList);

                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });
                addCell("#");
                addCell(Messages.ClubNumber);
                addCell(Messages.Name);
                addCell(Messages.RegistrationStatus);
                addCell(Messages.ClubManager);
                addCell(Messages.Email);
                addCell(Messages.Waiting);
                addCell(Messages.Approved);
                addCell(Messages.ActivePlayers);
                addCell(Messages.DateOfClubApproval);
                addCell(Messages.DateOfInitialClubApproval);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                foreach (var row in clubs)
                {
                    var clubManagersList = row.UsersJobs
                        .Where(j =>
                            JobRole.ClubManager.Equals(j.Job.JobsRole.RoleName, StringComparison.OrdinalIgnoreCase))?
                        .Select(uj => uj.User.FullName)
                        .ToList();

                    var clubManager = clubManagersList.Any() ? string.Join(",", clubManagersList) : string.Empty;

                    var clubManagersEmail = row.UsersJobs
                        .Where(j => JobRole.ClubManager.Equals(j.Job.JobsRole.RoleName,
                            StringComparison.OrdinalIgnoreCase))
                        .Select(uj => uj.User.Email)
                        .ToList();
                    var clubManagersEmailList =
                        clubManagersEmail.Any() ? string.Join(",", clubManagersEmail) : string.Empty;

                    var isClubRegistered = row.IsClubApproved == true ||
                                           clubsRegistrations.Any(x => x.ClubId == row.ClubId && x.IsActive)
                        ? Messages.Yes
                        : Messages.No;

                    var waitingCount = clubsInfo[row.ClubId][0];
                    var approvedCount = clubsInfo[row.ClubId][1];
                    var activeCount = clubsInfo[row.ClubId][2];

                    addCell(row.ClubId.ToString());
                    addCell(row.ClubNumber.HasValue ? row.ClubNumber.ToString() : string.Empty);
                    addCell(row.Name);
                    addCell(isClubRegistered);
                    addCell(clubManager);
                    addCell(clubManagersEmailList);
                    addCell(waitingCount.ToString());
                    addCell(approvedCount.ToString());
                    addCell(activeCount.ToString());
                    addCell(row.DateOfClubApproval?.ToShortDateString());
                    addCell(row.InitialDateOfClubApproval?.ToShortDateString());


                    rowCounter++;
                    columnCounter = 1;
                }

                ws.Columns().AdjustToContents();

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                if (isFlowersOfSport)
                {
                    Response.AddHeader("content-disposition",
                        $"attachment;filename= {unionName?.Replace(' ', '_')}_{Messages.FlowersOfSport.ToLower().Replace(" ", "_")}.xlsx");
                }
                else
                {
                    Response.AddHeader("content-disposition",
                        $"attachment;filename= {unionName?.Replace(' ', '_')}_{Messages.ClubList.ToLower().Replace(" ", "_")}.xlsx");
                }


                using (var myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        [HttpGet]
        public ActionResult DownloadExportToto()
        {
            var fileByteObj = Session[ExportTotoSessionKey];
            if (fileByteObj == null)
            {
                throw new FileNotFoundException();
            }

            var fileBytes = (byte[])fileByteObj;
            var fileName = Session[ExportTotoFileNameSessionKey] as string;

            var fi = new FileInfo(fileName);

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                string.Format("{0}-{2}{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension,
                    Messages.ImportPlayers_OutputFilePrefix));
        }

        [HttpPost]
        public JsonResult GetUnionClubsPlayersInformation(int id, int seasonId)
        {
            var clubs = clubsRepo.GetByUnion(id, seasonId)?.ToList();
            var clubsInfo = clubsRepo.GetPlayersInformation(clubs, seasonId);
            return Json(JsonConvert.SerializeObject(clubsInfo));
        }

        [HttpPost]
        public JsonResult GetClubPlayersInfo(int id, int seasonId)
        {
            var club = clubsRepo.GetById(id);
            var sectionAlias = club?.Section?.Alias ?? club?.Union?.Section?.Alias ?? string.Empty;

            var players = playersRepo.GetTeamPlayersShortByClubId(id, seasonId, sectionAlias);

            if (sectionAlias.Equals(GamesAlias.Gymnastic))
            {
                var result = new List<PlayerViewModel>();
                var groupedPlayers = players.GroupBy(x => x.UserId);
                foreach (var groupedPlayer in groupedPlayers)
                {
                    var resultPlayer = players.First(x => x.UserId == groupedPlayer.Key);
                    resultPlayer.TeamName = string.Join(", ", groupedPlayer.Select(x => x.TeamName));
                    result.Add(resultPlayer);
                }

                players = result;
            }

            clubsRepo.CountPlayersRegistrations(players, sectionAlias.Equals(GamesAlias.Gymnastic),
                out var approvedPlayers, out var completedPlayers, out var notApprovedPlayers,
                out var playersCount, out var waitingForApproval, out var activePlayers, out var notActivePlayers,
                out var registerediTennis);
            int iTennis = clubsRepo.GetItennisNumber(club.ClubId, seasonId);
            return Json(string.Join(",", new int[] { waitingForApproval, approvedPlayers, activePlayers, iTennis }));
        }

        [HttpPost]
        public ActionResult Save(ClubsForm model)
        {
            var club = new Club();

            if (model.ClubId.HasValue)
            {
                club = clubsRepo.GetById(model.ClubId.Value);
                UpdateModel(club);
            }
            else if (model.UnionId.HasValue)
            {
                UpdateModel(club);
                clubsRepo.Create(club);
                var isIndividualSection = secRepo.GetByUnionId(model.UnionId.Value)?.IsIndividual;
                if (isIndividualSection == true)
                {
                    club.ClubTeams.Add(new ClubTeam
                    {
                        Team = new Team
                        {
                            Title = club.Name,
                            CreateDate = DateTime.Now
                        },
                        SeasonId = model.SeasonId ?? 0
                    });
                }
            }
            else
            {
                UpdateModel(club);
                club.SeasonId = null;
                clubsRepo.Create(club);
            }

            clubsRepo.Save();

            if (model.SeasonId != null)
            {
                if (model.UnionId != null)
                {
                    activityRepo.CreateActivityForClubSeason(club.ClubId, model.SeasonId.Value);
                }
                else if (model.SectionId != null)
                {
                    activityRepo.CreateActivityForSectionClubSeason(club.ClubId,
                        club.Seasons.FirstOrDefault()?.Id ?? club.SeasonId ?? 0);
                }
            }

            return RedirectToClubList(club);
        }

        private ActionResult RedirectToClubList(Club pos)
        {
            if (pos.SectionId.HasValue)
            {
                return RedirectToAction(nameof(ListBySection), new { id = pos.SectionId });
            }
            else
            {
                return RedirectToAction(nameof(ListByUnion),
                    new { id = pos.UnionId, seasonId = pos.SeasonId, isFlowersOfSport = pos.IsFlowerOfSport });
            }
        }

        public ActionResult Delete(int id)
        {
            var item = clubsRepo.GetById(id);
            item.IsArchive = true;

            clubsRepo.Save();

            return RedirectToClubList(item);
        }

        [HttpPost]
        public ActionResult Update(int clubId, string name, bool? approved, int? accountingKeyNumber)
        {
            var club = clubsRepo.GetById(clubId);
            club.Name = name;
            if (club?.UnionId == 36)
            {
                club.AccountingKeyNumber = accountingKeyNumber;
            }

            if (approved.HasValue)
            {
                club.IsClubApproved = approved.Value;
                if (club.UnionId != null)
                {
                    club.DateOfClubApproval = approved.Value
                        ? (DateTime?)DateTime.Now
                        : null;
                    if (!club.InitialDateOfClubApproval.HasValue && approved.Value && club.UnionId == 52)
                    {
                        club.InitialDateOfClubApproval = DateTime.Now;
                    }
                }
            }

            clubsRepo.Save();

            TempData["SavedId"] = clubId;

            return RedirectToClubList(club);
        }

        public ActionResult Edit(int id, int? seasonId, int? sectionId, int? unionId, bool isDepartment = false,
            int? sportId = null, string roleType = null, bool showAlerts = false)
        {

            if (!string.IsNullOrWhiteSpace(roleType))
            {
                this.SetWorkerSession(roleType);
            }

            var club = clubsRepo.GetById(id);
            SetIsSectionClubLevel(club.UnionId == null);
            SetCurrentClubId(id);
            playersRepo.CheckForExclusions();
            leagueRepo.SetRegistrationsOrders(id);
            ViewBag.KarateMesssage = TempData["KarateClubMessage"]?.ToString();
            var roleName = usersRepo.GetTopLevelJob(AdminId);

            var isUnionClubManager = usersRepo.GetTopLevelJob(AdminId)?.Equals(JobRole.ClubManager) == true
                && club.UnionId.HasValue;

            if (seasonId != null)
            {
                if (club.UnionId == null)
                {
                    SetClubCurrentSeason(seasonId.Value);
                }
                else
                {
                    SetUnionCurrentSeason(seasonId.Value);
                }
            }

            if (club.IsArchive)
            {
                return RedirectToAction("NotFound", "Error");
            }

            if (!isDepartment)
            {
                if (User.IsInAnyRole(AppRole.Workers) &&
                    !(User.CurrentTopClubLevelJob(id) == JobRole.ClubManager ||
                      User.CurrentTopClubLevelJob(id) == JobRole.ClubSecretary ||
                      User.CurrentTopClubLevelJob(id) == JobRole.DepartmentManager ||
                      User.HasTopLevelJob(JobRole.RegionalManager) ||
                      club.IsUnionClub == true &&
                      (User.HasTopLevelJob(JobRole.UnionManager) ||
                       AuthSvc.AuthorizeUnionViewerByManagerId(unionId ?? 0, AdminId))))
                {
                    return RedirectToAction("Index", "NotAuthorized");
                }
            }
            else
            {
                if (User.IsInAnyRole(AppRole.Workers) &&
                    !(User.HasTopLevelJob(JobRole.DepartmentManager) ||
                      club.IsUnionClub == true &&
                      User.HasTopLevelJob(JobRole.UnionManager) ||
                      User.HasTopLevelJob(JobRole.ClubSecretary) ||
                      User.HasTopLevelJob(JobRole.ClubManager)))
                {
                    return RedirectToAction("Index", "NotAuthorized");
                }
            }
            if (showAlerts)
            {
                IList<PlayersBlockadeShortDTO> unblockaded = new List<PlayersBlockadeShortDTO>();
                if (!club.UnionId.HasValue && !club.SeasonId.HasValue)
                {
                    unblockaded = GetAllUnblockadedPlayers(club.ClubId, null, BlockadeType.MedicalExpiration);
                }
                else
                {
                    unblockaded = GetAllUnblockadedPlayers(club.UnionId.Value, club.SeasonId.Value, club.ClubId, null, BlockadeType.MedicalExpiration); //TODO: Perfomance

                }
                if (club?.Section?.Alias == SectionAliases.Basketball || club?.Union?.Section?.Alias == SectionAliases.Basketball)
                {
                    if (!club.UnionId.HasValue && !club.SeasonId.HasValue)
                    {
                        unblockaded = unblockaded.Concat(GetAllUnblockadedPlayers(club.ClubId, null, BlockadeType.InsuranceExpiration)).ToList();
                    }
                    else
                    {
                        unblockaded = unblockaded.Concat(GetAllUnblockadedPlayers(club.UnionId.Value, club.SeasonId.Value, club.ClubId, null, BlockadeType.InsuranceExpiration)).ToList(); ; //TODO: Perfomance

                    }
                }
                Session["UnblockadedPlayers"] = unblockaded;

                if ((club?.Section?.Alias == SectionAliases.Climbing || club?.Union?.Section?.Alias == SectionAliases.Climbing) && (roleName == JobRole.ClubManager || roleName == JobRole.ClubSecretary))
                {
                    var cm = usersRepo.GetById(AdminId);

                    var playersToClub = activityRepo.GetUnionPlayerToClubByPrevClub(SectionAliases.Climbing, club.UnionId, club.ClubId, seasonId);
                    Session["UnionPlayerToClub"] = new PlayerToClubModalDataDTO()
                    {
                        ClubManagerName = String.Join(" ", cm.FirstName, cm.LastName),
                        PlayersToClubs = playersToClub
                    };
                }
            }
            var viewModel = new EditClubViewModel
            {
                Id = id,
                Name = club.Name,
                SectionId = club?.SectionId ?? club?.SportSectionId ?? null,
                SectionName = secRepo.GetByClubId(club.ClubId).Alias,
                UnionId = club.UnionId,
                ClubId = club.ClubId,
                UnionName = club?.Union?.Name,
                SeasonId = seasonId,
                CurrentSeasonId = seasonId ?? seasonsRepository.GetLastSeasonByCurrentClub(club).Id,
                CurrentSeasonName = seasonId.HasValue ? seasonsRepository.GetById(seasonId.Value).Name : "",
                Seasons = club.IsSectionClub ?? true ? seasonsRepository.GetClubsSeasons(id, false) : new List<Season>(),
                IsClubTrainingEnabled = _objClubTrainingsRepo.GetClubById(id).IsTrainingEnabled,
                SectionIsIndividual = club.IsUnionClub.Value ? club.Union.Section.IsIndividual : club.Section.IsIndividual,
                IsUnionClub = club?.IsUnionClub,
                IsCatchBall = club?.SectionId == 2 || club?.Union?.SectionId == 2,
                IsGymnastics = club?.Section?.Alias == "Gymnastic" || club?.Union?.Section?.Alias == "Gymnastic",
                IsTennis = club?.Section?.Alias == SectionAliases.Tennis || club?.Union?.Section?.Alias == SectionAliases.Tennis,
                IsMartialArts = club?.Section?.Alias.Equals(SectionAliases.MartialArts, StringComparison.OrdinalIgnoreCase) == true || club?.Union?.Section?.Alias.Equals(SectionAliases.MartialArts, StringComparison.OrdinalIgnoreCase) == true,
                SportId = sportId,
                IsUnionClubManagerUnderPastSeason = isUnionClubManager && CheckIfIsUnderPastSeason(id, seasonId)
            };

            ViewBag.SectionAlias = secRepo.GetByClubId(club.ClubId).Alias;


            if (club.UnionId != null)
            {
                var registration =
                    clubsRepo.GetUnionClubActivityRegistrations(club.UnionId.Value, seasonId ?? 0, club.ClubId);

                club.IsClubApproved = club.IsClubApproved ?? registration.FirstOrDefault()?.IsActive;
            }
            viewModel.CanEditClub = !viewModel.IsTennis && !viewModel.IsGymnastics && !viewModel.IsBicycle && !viewModel.IsWaterPolo ||
                                    !(club.IsClubApproved != true &&
                                      (roleName == JobRole.ClubManager || roleName == JobRole.ClubSecretary || roleName == JobRole.TeamManager));
            var userCurrentRole = User.GetSessionWorkerValueOrTopLevelSeasonJob(seasonId ?? GetUnionCurrentSeasonFromSession());
            if ((userCurrentRole == JobRole.ClubManager || userCurrentRole == JobRole.ClubSecretary) && !club.UnionId.HasValue)
            {
                viewModel.CanEditClub = true;
            }

            if (isDepartment)
            {
                var parentClub = clubsRepo.GetById(id).ParentClub;
                viewModel.ParentClubId = parentClub.ClubId;
                viewModel.ParentClubTitle = parentClub.Name;
                viewModel.ParentClubSectionId = parentClub.SectionId;
                viewModel.Seasons = club.IsSectionClub ?? true
                    ? seasonsRepository.GetClubsSeasons(parentClub.ClubId, false)
                    : new List<Season>();
            }

            ViewBag.IsDepartMgr = roleName == JobRole.DepartmentManager;
            ViewBag.IsClubManager = (roleName == JobRole.ClubManager || roleName == JobRole.ClubSecretary);
            ViewBag.IsActivityManager = jobsRepo.IsActivityManager(AdminId);
            ViewBag.IsActivityViewer = jobsRepo.IsActivityViewer(AdminId);
            ViewBag.IsActivityRegistrationActive = jobsRepo.IsActivityRegistrationActive(AdminId);
            var sectionAlias = club?.Section?.Alias ?? club?.Union?.Section?.Alias ?? string.Empty;
            var isMultiSport = !(string.IsNullOrEmpty(sectionAlias)) && sectionAlias == SectionAliases.MultiSport;
            ViewBag.IsMultiSport = isMultiSport;
            var jobRole = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.JobRole = jobRole;
            ViewBag.IsDepartment = isDepartment;
            ViewBag.IsUnionViewer = AuthSvc.AuthorizeUnionViewerByManagerId(unionId ?? 0, AdminId);
            var view = View(viewModel);
            return view;
        }

        private bool CheckIfIsUnderPastSeason(int clubId, int? seasonId)
        {
            var club = clubsRepo.GetById(clubId);
            var isUnderPastSeason = false;
            var clubsSeason = seasonId ?? club?.SeasonId;
            if (clubsSeason.HasValue)
            {
                var currentClubSeason = seasonsRepository.GetById(clubsSeason.Value);
                var lastClubSeason = seasonsRepository.GetById(seasonsRepository.GetLastSeasonIdByCurrentClubId(clubId));
                if (!currentClubSeason.IsActive ||
                    currentClubSeason.Id != lastClubSeason.Id) //currentClubSeason.EndDate.Year <= lastClubSeason.StartDate.Year
                {
                    isUnderPastSeason = true;
                }
            }
            return isUnderPastSeason;
        }

        [HttpPost]
        public ActionResult MarkPlayerToClubActivityAsSeen(int? unionId, int clubId, int? seasonId)
        {
            activityRepo.MarkPlayerToClubActivityAsSeen(SectionAliases.Climbing, unionId, clubId, seasonId);
            return Json(new { Success = true });
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

        private IList<PlayersBlockadeShortDTO> GetAllUnblockadedPlayers(int clubId, int? teamId = null, int BType = BlockadeType.All)
        {
            IList<PlayersBlockadeShortDTO> unionsBlockades = playersRepo.GetAllBlockadedPlayersForClub(clubId, teamId, BType);
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

        public ActionResult Details(int id, int seasonId)
        {
            var club = clubsRepo.GetById(id);
            Section section = null;

            var vm = new ClubDetailsForm();
            vm.InjectFrom<CloneInjection>(club);
            vm.MedicalCertificateFile = club.MedicalSertificateFile;

            if (club.Section != null)
            {
                section = club.ParentClubId != null ? club?.SportSection : club?.Section;
            }
            else if (club.Union?.Section != null)
            {
                section = club.Union.Section;

                vm.Section = new SectionModel();
                vm.Section.InjectFrom(section);
            }
            vm.ClubDisplayName = club.ClubDisplayName;
            vm.SportCenterList = db.SportCenters.OrderBy(sc => sc.Id).ToList();
            vm.Culture = getCulture();

            vm.AvailableSports = new List<SelectListItem>();
            vm.ShowAppCredentials = section?.Alias == SectionAliases.Basketball;
            if (section != null)
            {
                var availableSports = SportsRepo.GetBySectionId(section.SectionId).ToList().OrderBy(x => x.Name);
                if (availableSports.Any())
                {
                    vm.AvailableSports.AddRange(availableSports.Select(x => new SelectListItem
                    {
                        Text = !IsHebrew ? x.Name : x.NameHeb,
                        Value = x.Id.ToString(),
                        Selected = x.Id == club.SportType
                    }));
                }
            }

            var user = usersRepo.GetByUnionName("club." + vm.ClubId);
            if (user != null)
            {
                vm.AppLogin = user.UserName;
                vm.AppPassword = Protector.Decrypt(user.Password);
            }

            if (vm.UnionId.HasValue)
            {
                var unionForms = unionsRepo.GetUnionForms(vm.UnionId.Value, club.SeasonId);
                vm.UnionForms = unionForms.Any()
                    ? unionForms.Select(uf => new UnionFormModel
                    {
                        FormId = uf.FormId,
                        SeasonId = uf.SeasonId,
                        Title = uf.Title,
                        UnionId = uf.UnionId,
                        Path = uf.FilePath
                    })
                    : new List<UnionFormModel>();
            }

            vm.UnionDisciplines = club.Union?.Disciplines ?? new List<Discipline>();
            vm.ClubDisciplinesIds = club.ClubDisciplines.Where(x => x.SeasonId == seasonId).Select(x => x.DisciplineId)
                .ToList();

            if (TempData["ViewData"] != null)
            {
                ViewData = (ViewDataDictionary)TempData["ViewData"];
            }
            ViewBag.IsUnionManager = usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager;
            var roleName = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.IsDepartMgr = roleName == JobRole.DepartmentManager;
            ViewBag.IsClubManager = (roleName == JobRole.ClubManager || roleName == JobRole.ClubSecretary);
            ViewBag.IsRegionallevelEnabled = club.Union?.IsRegionallevelEnabled;

            ViewBag.IsMartialArts = false;
            ViewBag.ContainsDisciplines = _disciplineSections.Contains(section?.Alias ?? "");

            if (String.Equals(section?.Alias, SectionAliases.MartialArts, StringComparison.CurrentCultureIgnoreCase))
            {
                ViewBag.IsMartialArts = true;
            }

            vm.IsAthletics = section?.Alias == GamesAlias.Athletics;
            vm.IsRowing = section?.Alias == GamesAlias.Rowing;
            vm.IsBicycle = section?.Alias == GamesAlias.Bicycle;
            vm.Statement = club.Union?.StatementOfClub;
            vm.IsGymnastics = section?.Alias == GamesAlias.Gymnastic;
            if (vm.IsGymnastics && (usersRepo.GetTopLevelJob(AdminId) == JobRole.ClubManager || usersRepo.GetTopLevelJob(AdminId) == JobRole.ClubSecretary))
                ViewBag.IsClubManagerUnderGymnastics =
                    club.UnionId == 36 && club.IsUnionClub == true && roleName == JobRole.ClubManager;

            if (vm.IsGymnastics && (usersRepo.GetTopLevelJob(AdminId) == JobRole.ClubManager || usersRepo.GetTopLevelJob(AdminId) == JobRole.ClubSecretary))
            {
                vm.DisciplinesString = String.Join(",", vm.UnionDisciplines
                    .Where(d => vm.ClubDisciplinesIds.Contains(d.DisciplineId))?
                    .Select(c => c.Name)
                    .AsEnumerable());
            }

            var clubRegistration = club.ActivityFormsSubmittedDatas.FirstOrDefault(x =>
                x.Activity.UnionId == club.UnionId &&
                x.Activity.IsAutomatic == true &&
                x.Activity.Type == ActivityType.Club &&
                x.Activity.SeasonId == seasonId);
            if (clubRegistration != null)
            {
                vm.Registration = new ClubRegistration
                {
                    IsActive = clubRegistration.IsActive,
                    ActivityName = clubRegistration.Activity.Name,
                    IsAutomatic = clubRegistration.Activity.IsAutomatic == true,
                    FormControls = clubRegistration.Activity.ActivityForms.FirstOrDefault()?.ActivityFormsDetails
                        ?.Where(x => !x.IsDisabled &&
                                     x.Type != ActivityFormControlType.CustomText.ToString() &&
                                     x.Type != ActivityFormControlType.CustomTextMultiline.ToString() &&
                                     x.Type != ActivityFormControlType.CustomFileReadonly.ToString() &&
                                     x.Type != ActivityFormControlType.CustomPrice.ToString())
                        .ToList(),
                    CustomFields = ActivityCustomFieldsHelper.DeserializeFields(clubRegistration.CustomFields)?.Where(
                            f => f.Type != ActivityFormControlType.CustomText &&
                                 f.Type != ActivityFormControlType.CustomTextMultiline &&
                                 f.Type != ActivityFormControlType.CustomFileReadonly &&
                                 f.Type != ActivityFormControlType.CustomPrice)
                        .ToList(),
                    Culture = getCulture()
                };
            }

            var customRegistrations = club.ActivityFormsSubmittedDatas.Where(x =>
                    x.Activity.UnionId == club.UnionId &&
                    x.Activity.IsAutomatic != true &&
                    x.Activity.Type == ActivityType.Club &&
                    x.Activity.SeasonId == seasonId)
                .ToList();
            vm.CustomRegistrations = customRegistrations.Select(x => new ClubRegistration
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
                CustomFields = ActivityCustomFieldsHelper.DeserializeFields(x.CustomFields)?.Where(
                            f => f.Type != ActivityFormControlType.CustomText &&
                                 f.Type != ActivityFormControlType.CustomTextMultiline &&
                                 f.Type != ActivityFormControlType.CustomFileReadonly &&
                                 f.Type != ActivityFormControlType.CustomPrice)
                        .ToList(),
                Culture = getCulture()
            })
                .ToList();

            return PartialView("_Details", vm);
        }

        [HttpPost]
        public ActionResult Details(ClubDetailsForm frm)
        {
            if (!frm.StatementApproved && !string.IsNullOrEmpty(frm.Statement) &&
                !(User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager)))
            {
                ModelState.AddModelError("StatementApproved", Messages.StatementNotApproved);
            }

            var maxFileSize = GlobVars.MaxFileSize * 1000;
            var savePath = Server.MapPath(GlobVars.ClubContentPath);
            var club = clubsRepo.GetById(frm.ClubId);
            var sectionAlias = club?.Section?.Alias ?? club?.Union?.Section?.Alias ?? string.Empty;

            if (club == null)
            {
                return HttpNotFound($"club {frm.ClubId} not found");
            }

            club.ClubNumber = frm.ClubNumber;
            club.NumberOfCourts = frm.NumberOfCourts;
            club.DateOfClubApproval = frm.DateOfClubApproval;
            club.InitialDateOfClubApproval = frm.InitialDateOfClubApproval;
            club.Name = frm.Name ?? club.Name;
            club.NGO_Number = frm.NGO_Number;
            club.SportCenterId = frm.SportCenterId;
            club.ContactPhone = frm.ContactPhone;
            club.Email = frm.Email;
            club.TermsCondition = frm.TermsCondition;
            club.Description = frm.Description;
            club.IndexAbout = frm.IndexAbout;
            club.IsReportsEnabled = frm.IsReportsEnabled;
            club.StatementApproved = frm.StatementApproved;
            club.Address = frm.Address;
            club.IsTrainingEnabled = frm.IsTrainingEnabled;
            club.ClubDisplayName = frm.ClubDisplayName;
            club.IsUnionArchive = frm.IsUnionArchive;
            club.IsNationalTeam = frm.IsNationalTeam;


            if (club?.UnionId.HasValue == true && (User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager)))
            {
                var isAccountingKeyNumberUsed = db.Clubs.Any(c => c.SeasonId == frm.SeasonId && c.AccountingKeyNumber.HasValue && c.AccountingKeyNumber == frm.AccountingKeyNumber && c.ClubId != frm.ClubId);
                if (isAccountingKeyNumberUsed)
                {
                    ModelState.AddModelError("AccountingKeyNumber", Messages.AccountingKeyNumberDuplication);
                }
                else
                    club.AccountingKeyNumber = frm.AccountingKeyNumber;
            }

            if (string.Equals(sectionAlias, GamesAlias.MartialArts, StringComparison.OrdinalIgnoreCase))
            {
                club.SportType = frm.SportType;
            }

            club.ForeignName = frm.ForeignName;


            var error = usersRepo.AddAppCredentials("club", frm.ClubId, frm.AppLogin, frm.AppPassword);

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
                        if (!string.IsNullOrEmpty(club.PrimaryImage))
                            FileUtil.DeleteFile(savePath + club.PrimaryImage);

                        club.PrimaryImage = newName;
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
                        if (!string.IsNullOrEmpty(club.Logo))
                            FileUtil.DeleteFile(savePath + club.Logo);

                        club.Logo = newName;
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
                        if (!string.IsNullOrEmpty(club.IndexImage))
                            FileUtil.DeleteFile(savePath + club.IndexImage);

                        club.IndexImage = newName;
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

                var isValid = SaveDocument(docFile, frm.ClubId);
                if (!isValid)
                {
                    ModelState.AddModelError("DocFile", Messages.FileError);
                }
            }

            var medicalFile = GetPostedFile("MedicalCertificateFile");

            if (frm.RemoveMedicalCertificateFile == true && medicalFile == null)
            {
                club.MedicalSertificateFile = null;
            }

            if (medicalFile != null)
            {
                if (medicalFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("MedicalCertificateFile", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(medicalFile, "medical");
                    if (newName == null)
                    {
                        ModelState.AddModelError("MedicalCertificateFile", Messages.FileError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(club.MedicalSertificateFile))
                            FileUtil.DeleteFile(savePath + club.MedicalSertificateFile);

                        club.MedicalSertificateFile = newName;
                    }
                }
            }

            if (club.MedicalSertificateFile == null && sectionAlias == GamesAlias.Bicycle)
            {
                ModelState.AddModelError("MedicalCertificateFile", Messages.FieldIsRequired);
            }

            var clubInsuranceFile = GetPostedFile("ClubInsuranceFile");
            if (frm.RemoveInsuranceFile == true && clubInsuranceFile == null)
            {
                club.ClubInsurance = null;
            }

            if (clubInsuranceFile != null)
            {
                if (clubInsuranceFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("ClubInsurance", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(clubInsuranceFile, "insurance");
                    if (newName == null)
                    {
                        ModelState.AddModelError("ClubInsurance", Messages.FileError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(club.ClubInsurance))
                            FileUtil.DeleteFile(savePath + club.ClubInsurance);

                        club.ClubInsurance = newName;
                    }
                }
            }


            #region Documents

            var certificateOfIncorporation = GetPostedFile("CertificateOfIncorporation");
            if (frm.RemoveCertificateOfIncorporation == true && certificateOfIncorporation == null)
            {
                club.CertificateOfIncorporation = null;
            }

            if (certificateOfIncorporation != null)
            {
                if (certificateOfIncorporation.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("CertificateOfIncorporation", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(certificateOfIncorporation, "certificateOfIncorporation");
                    if (newName == null)
                    {
                        ModelState.AddModelError("CertificateOfIncorporation", Messages.FileError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(club.CertificateOfIncorporation))
                            FileUtil.DeleteFile(savePath + club.CertificateOfIncorporation);

                        club.CertificateOfIncorporation = newName;
                    }
                }
            }

            var approvalOfInsuranceCover = GetPostedFile("ApprovalOfInsuranceCover");
            if (frm.RemoveApprovalOfInsuranceCover == true && approvalOfInsuranceCover == null)
            {
                club.ApprovalOfInsuranceCover = null;
            }

            if (approvalOfInsuranceCover != null)
            {
                if (approvalOfInsuranceCover.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("ApprovalOfInsuranceCover", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(approvalOfInsuranceCover, "approvalOfInsuranceCover");
                    if (newName == null)
                    {
                        ModelState.AddModelError("approvalOfInsuranceCover", Messages.FileError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(club.ApprovalOfInsuranceCover))
                            FileUtil.DeleteFile(savePath + club.ApprovalOfInsuranceCover);

                        club.ApprovalOfInsuranceCover = newName;
                    }
                }
            }

            var authorizedSignatories = GetPostedFile("AuthorizedSignatories");
            club.AuthorizedSignPersonName = frm.AuthorizedSignPersonName;
            club.SignEachSeparately = frm.SignEachSeparately;
            club.SignTogether = frm.SignTogether;

            if (authorizedSignatories != null)
            {
                if (CheckAuthorizedSignatureFields(frm) && !(club.IsUnionClub == false) && club.UnionId != 52)
                {
                    ModelState.AddModelError("AuthorizedSignatories", Messages.AuthorizedSignatureInvalidFields);
                }
                else
                {
                    if (authorizedSignatories.ContentLength > maxFileSize)
                    {
                        ModelState.AddModelError("AuthorizedSignatories", Messages.FileSizeError);
                    }
                    else
                    {
                        var newName = SaveFile(authorizedSignatories, "AuthorizedSignatories");
                        if (newName == null)
                        {
                            ModelState.AddModelError("AuthorizedSignatories", Messages.FileError);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(club.AuthorizedSignatories))
                                FileUtil.DeleteFile(savePath + club.AuthorizedSignatories);

                            club.AuthorizedSignatories = newName;
                        }
                    }
                }
            }
            else
            {
                if (frm.RemoveAuthorizedSignatories == true)
                {
                    club.AuthorizedSignatories = null;
                }
            }

            #endregion

            if (frm.DisciplinesIds != null)
            {
                var clubDisciplines = club.ClubDisciplines.Where(x => x.SeasonId == frm.SeasonId).ToList();

                foreach (var frmDisciplinesId in frm.DisciplinesIds)
                {
                    var existingDiscipline =
                        clubDisciplines.FirstOrDefault(
                            x => x.DisciplineId == frmDisciplinesId && x.SeasonId == frm.SeasonId);

                    if (existingDiscipline == null)
                    {
                        club.ClubDisciplines.Add(new ClubDiscipline
                        {
                            DisciplineId = frmDisciplinesId,
                            ClubId = club.ClubId,
                            SeasonId = frm.SeasonId
                        });
                    }
                }

                foreach (var disciplineToRemove in clubDisciplines.Where(x =>
                    !frm.DisciplinesIds.Contains(x.DisciplineId)))
                {
                    db.Entry(disciplineToRemove).State = EntityState.Deleted;
                }
            }

            if (club.IsCertificateApproved != frm.IsCertificateApproved)
                club.IsCertificateApproved = frm.IsCertificateApproved;
            if (club.IsInsuranceCoverApproved != frm.IsInsuranceCoverApproved)
                club.IsInsuranceCoverApproved = frm.IsInsuranceCoverApproved;
            if (club.IsAuthorizedSignatoriesApproved != frm.IsAuthorizedSignatoriesApproved)
                club.IsAuthorizedSignatoriesApproved = frm.IsAuthorizedSignatoriesApproved;

            if (ModelState.IsValid)
            {
                clubsRepo.Save();

                TempData["Saved"] = true;
            }
            else
            {
                TempData["ViewData"] = ViewData;
            }

            return RedirectToAction("Details", new { id = club.ClubId, seasonId = frm.SeasonId });
        }

        private bool CheckAuthorizedSignatureFields(ClubDetailsForm frm)
        {
            return string.IsNullOrEmpty(frm.AuthorizedSignPersonName) || (!frm.SignEachSeparately && !frm.SignTogether);
        }


        [NonAction]
        private bool SaveDocument(HttpPostedFileBase file, int unionId)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (ext != ".pdf")
            {
                return false;
            }

            return false;
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

        public ActionResult MainTab(int id, int seasonId = 0)
        {
            var club = clubsRepo.GetById(id);
            var section = club?.Section ?? club?.Union?.Section;

            var clubsRegistrations = clubsRepo.GetCollection<ActivityFormsSubmittedData>(x =>
                x.Activity.UnionId == club.UnionId &&
                x.Activity.SeasonId == seasonId &&
                x.Activity.Type == ActivityType.Club &&
                x.Activity.IsAutomatic == true).ToList();

            var schools = new List<School>();
            if (club?.RelatedClubs != null && club.RelatedClubs.Count > 0)
            {
                foreach (var department in club.RelatedClubs)
                {
                    schools.AddRange(department.Schools);
                }
            }
            else
            {
                schools = SchoolRepo.GetCollection(sc => sc.ClubId == id && sc.SeasonId == seasonId).ToList();
            }

            var isIndividual = club?.Union?.Section?.IsIndividual == true || club?.Section?.IsIndividual == true;
            var sectionAlias = club?.Union?.Section?.Alias ?? club?.Section?.Alias;
            var teams = GetAllCurrentClubTeams(club, schools, seasonId);
            var hashedTeams = new HashSet<Team>(teams);

            var players = playersRepo.GetTeamPlayersShortByClubId(id, seasonId, sectionAlias);
            var schoolPlayers = playersRepo.GetSchoolTeamPlayersByClubId(id, seasonId, sectionAlias);

            var isGymnastic = sectionAlias != null && sectionAlias.Equals(GamesAlias.Gymnastic);
            clubsRepo.CountPlayersRegistrations(players, isGymnastic,
                out var approvedPlayers, out var completedPlayers, out var notApprovedPlayers, out var playersCount,
                out var waitingForApproval, out var activePlayers, out var notActivePlayers, out var registered);
            int iTennis = clubsRepo.GetItennisNumber(club.ClubId, seasonId);
            clubsRepo.CountSchoolPlayersRegistrations(schoolPlayers, isGymnastic, out var schoolApprovedPlayers,
                out var schoolCompletedPlayers, out var schoolNotApprovedPlayers, out var schoolPlayersCount,
                out var schoolWaitingForApproval, out var activeSchoolPlayers);

            var uniquePlayers = players.GroupBy(c => c.UserId).Select(g => g.First()).ToList();
            var uniqueShoolPlayers = schoolPlayers.GroupBy(c => c.UserId).Select(g => g.First()).ToList();

            CountPlayersRegistrationsUnique(uniquePlayers, seasonId, out var approvedPlayersUnique,
                out var completedPlayersUnique, out var notApprovedPlayersUnique, out var playersCountUnique,
                out var activeUniquePlayers);

            CountPlayersRegistrationsUnique(uniqueShoolPlayers, seasonId, out var approvedSchoolPlayersUnique,
                out var completedSchoolPlayersUnique, out var notApprovedSchoolPlayersUnique,
                out var playersShoolCountUnique, out var activeSchoolUniquePlayers);

            var approvedTeamsCount = 0;
            var completedTeamsCount = 0;

            foreach (var team in hashedTeams)
            {
                var registrationActive = team.ActivityFormsSubmittedDatas.FirstOrDefault(x =>
                    x.Activity.IsAutomatic.HasValue && x.Activity.IsAutomatic.Value &&
                    x.Activity.Type == ActivityType.Group &&
                    x.Activity.SeasonId == seasonId)?.IsActive;

                if (registrationActive == true)
                    approvedTeamsCount += 1;
                else if (registrationActive == false)
                    completedTeamsCount += 1;
            }

            List<Team> teamsInfo = GetCurrentClubLeagueTeams(club, seasonId);
            List<PlayerCountsDetail> CountDetail = new List<PlayerCountsDetail>();
            foreach (Team t in teamsInfo)
            {
                List<PlayerViewModel> teamPlayers = playersRepo.GetTeamPlayersShortByTeamId(t.TeamId, seasonId, sectionAlias);
                clubsRepo.CountPlayersRegistrations(teamPlayers, isGymnastic,
                    out var approvedCnt, out var completedCnt, out var notApprovedCnt, out var playersCnt,
                    out var waitingCnt, out var activeCnt, out var notActiveCnt, out var registeredCnt);
                CountDetail.Add(new PlayerCountsDetail
                {
                    TeamName = t.Title,
                    TotalCount = teamPlayers.Count(),
                    WaitingCount = waitingCnt,
                    ApprovedCount = approvedCnt,
                    NotApprovedCount = notApprovedCnt,
                    ActiveCount = activeCnt,
                });
            }

            var totalTeams = teamsInfo;
            var isUnionClub = false;
            isUnionClub = clubsRepo.GetById(id).IsUnionClub ?? true;
            ViewBag.Section = secRepo.GetByClubId(club.ClubId)?.Alias;
            var vm = new ClubMainTabForm
            {
                TotalTeams = totalTeams.Count(),
                ClubId = id,
                SeasonId = seasonId,
                PlayersCount = playersCount + schoolPlayersCount,
                SchoolPlayersCount = schoolPlayersCount,
                UniquePlayersCount = playersCountUnique,
                UniqueSchoolPlayersCount = playersShoolCountUnique,
                PlayersCompletedRegistrations = completedPlayers,
                SchoolPlayersCompletedRegistrations = schoolCompletedPlayers,
                PlayersCompletedRegistrationsUnique = completedPlayersUnique,
                SchoolPlayersCompletedRegistrationsUnique = completedSchoolPlayersUnique,
                PlayersApproved = approvedPlayers,
                SchoolPlayersApproved = schoolApprovedPlayers,
                PlayersApprovedUnique = approvedPlayersUnique,
                SchoolPlayersApprovedUnique = approvedSchoolPlayersUnique,
                PlayersNotApproved = notApprovedPlayers,
                SchoolPlayersNotApproved = schoolNotApprovedPlayers,
                PlayersNotApprovedUnique = notApprovedPlayersUnique,
                SchoolPlayersNotApprovedUnique = notApprovedSchoolPlayersUnique,
                WaitingForApproval = waitingForApproval,
                SchoolWaitingForApproval = schoolWaitingForApproval,
                OfficialsCount = jobsRepo.CountOfficialsInClub(id, seasonId),
                TeamsCount = hashedTeams.Count,
                TeamsApproved = approvedTeamsCount,
                TeamsCompletedRegistrations = completedTeamsCount,
                SectionIsIndividual = section?.IsIndividual ?? false,
                IsGymnastics = section?.Alias == "Gymnastic",
                IsRowing = section?.Alias == "Rowing",
                IsBicycle = section?.Alias == "Bicycle",
                IsIndividual = isIndividual,
                ActivePlayers = activePlayers,
                ActiveSchoolPlayers = activeSchoolPlayers,
                ActiveUniquePlayers = activeUniquePlayers,
                ActiveUniqueSchoolPlayers = activeSchoolUniquePlayers,
                SectionAlias = section?.Alias,
                TotalWaitingAndApprovedCount = waitingForApproval + approvedPlayers,
                Comment = club?.Comment,
                IsClubApproved = club?.IsClubApproved ??
                                 clubsRegistrations?.Any(x => x.ClubId == club.ClubId && x.IsActive) == true,
                HasPermission = User.IsInAnyRole(AppRole.Admins) ||
                                JobRole.UnionManager.Equals(usersRepo.GetTopLevelJob(AdminId)),
                CountDetail = CountDetail,
                IsUnionClub = isUnionClub,
                Itennis = iTennis,
                UnionId = club.UnionId
            };

            if (TempData["ViewData"] != null)
            {
                ViewData = (ViewDataDictionary)TempData["ViewData"];
            }

            //todo: refactor - move to vm
            var trainingPlayersCount = 0;
            /*
            foreach (var team in teams)
            {
                if (team.ClubTeams.FirstOrDefault(c => c.TeamId == team.TeamId)?.IsTrainingTeam == true)
                {
                    trainingPlayersCount += team.TeamsPlayers.Count;
                }
            }
            */
            ViewBag.IsTennisClub = sectionAlias == "Tennis";
            ViewBag.TotalTrainingPlayers = trainingPlayersCount;
            return PartialView("_MainTab", vm);
        }

        private void CountPlayersRegistrationsUnique(List<PlayerViewModel> uniquePlayers, int? seasonId,
            out int approvedPlayersUnique, out int completedPlayersUnique, out int notApprovedPlayersUnique,
            out int playersCountUnique, out int activeCountUnique)
        {
            approvedPlayersUnique = 0;
            completedPlayersUnique = 0;
            notApprovedPlayersUnique = 0;
            activeCountUnique = 0;
            playersCountUnique = uniquePlayers.Count;

            var playerIds = uniquePlayers.Select(x => x.UserId).ToArray();
            var teamsIds = uniquePlayers.Select(x => x.TeamId).ToArray();
            var playersTeamsPlayers = db.TeamsPlayers
                .Where(x => playerIds.Contains(x.UserId) && teamsIds.Contains(x.TeamId) && x.SeasonId == seasonId)
                .ToList();

            foreach (var player in uniquePlayers)
            {
                var hasCompleted = false;
                var hasNotApproved = false;
                var teamsOfPlayer = playersTeamsPlayers.Where(x =>
                    x.TeamId == player.TeamId && x.UserId == player.UserId && x.SeasonId == seasonId);

                if (player.IsActive == true)
                    completedPlayersUnique += 1;
                if (player.IsActivePlayer)
                    activeCountUnique += 1;
                if (teamsOfPlayer != null)
                {
                    foreach (var teamPlayer in teamsOfPlayer)
                    {
                        if (!hasCompleted)
                        {
                            if (teamPlayer.IsApprovedByManager != null && teamPlayer.IsApprovedByManager == true)
                            {
                                approvedPlayersUnique += 1;
                                hasCompleted = true;
                            }
                        }

                        if (!hasNotApproved)
                        {
                            if (teamPlayer.IsApprovedByManager != null && teamPlayer.IsApprovedByManager == false)
                            {
                                notApprovedPlayersUnique += 1;
                                hasNotApproved = true;
                            }
                        }
                    }
                }
            }
        }

        private IEnumerable<TeamsPlayer> GetAllCurrentClubPlayers(IEnumerable<Team> teams, int seasonId)
        {
            var allPlayers = new List<TeamsPlayer>();
            foreach (var team in teams)
            {
                allPlayers.AddRange(team.TeamsPlayers.Where(c => c.SeasonId == seasonId && c.User.IsArchive == false));
            }

            return allPlayers;
        }

        private List<Team> GetCurrentClubLeagueTeams(Club club, int seasonId)
        {
            var clubTeams = new List<Team>();
            if (club?.RelatedClubs != null && club.RelatedClubs.Count > 0)
            {
                foreach (var departament in club.RelatedClubs)
                {
                    clubTeams.AddRange(departament.ClubTeams
                        .Where(c => c.SeasonId == seasonId && c.IsTrainingTeam == false).Select(c => c.Team));
                }
            }
            else
            {
                clubTeams.AddRange(club.ClubTeams.Where(c => c.SeasonId == seasonId && c.IsTrainingTeam == false)
                    .Select(c => c.Team));
            }

            return clubTeams;
        }

        private List<Team> GetAllCurrentClubTeams(Club club, IEnumerable<School> schools, int seasonId)
        {
            var allTeams = new List<Team>();
            if (club?.RelatedClubs != null && club.RelatedClubs.Count > 0)
            {
                foreach (var departament in club.RelatedClubs)
                {
                    allTeams.AddRange(departament.ClubTeams.Where(c => c.SeasonId == seasonId).Select(c => c.Team));
                }
            }
            else
            {
                if (club != null)
                    allTeams.AddRange(club.ClubTeams.Where(c => c.SeasonId == seasonId).Select(c => c.Team));
            }

            foreach (var school in schools)
            {
                allTeams.AddRange(school.SchoolTeams.Where(sc => sc.School.SeasonId == seasonId).Select(s => s.Team));
            }

            return allTeams;
        }


        public ActionResult ShowPaymentReportToClubManager(int clubId, int seasonId, bool isShow)
        {
            clubsRepo.UpdateClubShowPaymentToClubManager(clubId, seasonId, isShow);
            return Json(new { Success = true });
        }

        public ActionResult CheckClubPaymentDone(int clubBalanceId, bool isPaid)
        {
            cbsServ.UpdateBalancePaymentStatus(clubBalanceId, isPaid);
            return Json(new { Success = true });
        }

        [HttpPost]
        public void ExportTeamsList(int clubId, int seasonId)
        {
            var club = clubsRepo.GetById(clubId);
            var sectionAlias = club?.Union?.Section?.Alias ?? club?.Section?.Alias;
            var isGymnastic = sectionAlias != null && sectionAlias.Equals(GamesAlias.Gymnastic);

            List<Team> teams = teamRepo.GetClubTeamsByClubAndSeasonId(clubId, seasonId).OrderBy(t => t.Title).ToList();

            using (var workbook =
                new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var ws = workbook.AddWorksheet(Messages.Players);

                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });
                addCell("#");
                addCell(Messages.Id);
                addCell(Messages.TeamName);
                addCell(Messages.NumberOfInactivePlayers);
                addCell(Messages.NumberOfWaitingPlayers);
                addCell(Messages.NumberOfApprovedPlayers);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                foreach (var t in teams)
                {
                    List<PlayerViewModel> teamPlayers = playersRepo.GetTeamPlayersShortByTeamId(t.TeamId, seasonId, sectionAlias);
                    clubsRepo.CountPlayersRegistrations(teamPlayers, isGymnastic,
                        out var approvedCnt, out var completedCnt, out var notApprovedCnt, out var playersCnt,
                        out var waitingCnt, out var activeCnt, out var notActiveCnt, out var registeredCnt);

                    addCell((rowCounter - 1).ToString());
                    addCell(t.TeamId.ToString()); // id
                    addCell(t.Title); // team name
                    addCell(notActiveCnt.ToString()); // unactive players
                    addCell(waitingCnt.ToString()); // waiting players
                    addCell(approvedCnt.ToString()); // approved players

                    rowCounter++;
                    columnCounter = 1;
                }

                ws.Columns().AdjustToContents();

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition",
                    $"attachment;filename= {Messages.ExportTeamsToExcel}.xlsx");

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
        public void ExportSchoolTeamsList(int clubId, int seasonId)
        {
            ExportSchoolsTeamsList(clubId, seasonId, false);
        }

        [HttpPost]
        public void ExportCampTeamsList(int clubId, int seasonId)
        {
            ExportSchoolsTeamsList(clubId, seasonId, true);
        }

        public void ExportSchoolsTeamsList(int clubId, int seasonId, bool isCamp)
        {
            var club = clubsRepo.GetById(clubId);
            var sectionAlias = club?.Union?.Section?.Alias ?? club?.Section?.Alias;
            var isGymnastic = sectionAlias != null && sectionAlias.Equals(GamesAlias.Gymnastic);
            var schools = club.Schools.Where(x => x.SeasonId == seasonId && x.IsCamp == isCamp).OrderBy(x => x.Name).ToList();

            using (var workbook =
                new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var ws = workbook.AddWorksheet(Messages.Players);

                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });
                addCell("#");
                addCell(Messages.Id);
                addCell(Messages.TeamName);
                addCell(Messages.NumberOfInactivePlayers);
                addCell(Messages.NumberOfWaitingPlayers);
                addCell(Messages.NumberOfApprovedPlayers);
                if (isCamp == true)
                    addCell(Messages.CampName);
                else
                    addCell(Messages.SchoolName);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                foreach (var school in schools)
                {
                    var teams = school.SchoolTeams.Select(x => x.Team).OrderBy(x => x.Title).ToList();
                    // List<Team> teams = club.ClubTeams.Where(x => x.SeasonId == seasonId && !teamsInSchools.Select(ts => ts.TeamId).Contains(x.TeamId)).Select(x => x.Team)
                    //    .OrderBy(x => x.Title).ToList();

                    foreach (var t in teams)
                    {
                        List<PlayerViewModel> teamPlayers = playersRepo.GetTeamPlayersShortByTeamId(t.TeamId, seasonId, sectionAlias);
                        clubsRepo.CountPlayersRegistrations(teamPlayers, isGymnastic,
                            out var approvedCnt, out var completedCnt, out var notApprovedCnt, out var playersCnt,
                            out var waitingCnt, out var activeCnt, out var notActiveCnt, out var registeredCnt);

                        addCell((rowCounter - 1).ToString());
                        addCell(t.TeamId.ToString()); // id
                        addCell(t.Title); // team name
                        addCell(notActiveCnt.ToString()); // unactive players
                        addCell(waitingCnt.ToString()); // waiting players
                        addCell(approvedCnt.ToString()); // approved players
                        addCell(school.Name);

                        rowCounter++;
                        columnCounter = 1;
                    }
                }

                ws.Columns().AdjustToContents();

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition",
                    $"attachment;filename= {Messages.ExportTeamsToExcel}.xlsx");

                using (var myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        public ActionResult ClubTeams(int clubId, int seasonId, int? sportId = null)
        {
            var club = clubsRepo.GetClubById(clubId);
            var isClubsBlocked = club?.Union?.IsClubsBlocked ?? false;
            var section = club?.Union?.Section?.Alias ?? club?.Section?.Alias ?? string.Empty;
            bool isTennis = section == GamesAlias.Tennis;
            var isDepartment = club.ParentClub == null ? false : true;
            if (!isDepartment)
            {
                if ((User.IsInAnyRole(AppRole.Workers) && !((User.CurrentTopClubLevelJob(clubId) == JobRole.ClubManager || User.CurrentTopClubLevelJob(clubId) == JobRole.ClubSecretary)
                                                            || (club.IsUnionClub.Value &&
                                                                (User.HasTopLevelJob(JobRole.UnionManager)
                                                                 || AuthSvc.AuthorizeUnionViewerByManagerId(
                                                                     club?.UnionId ?? 0, AdminId))))))
                {
                    return RedirectToAction("Index", "NotAuthorized");
                }
            }
            else
            {
                if ((User.IsInAnyRole(AppRole.Workers) && !(User.HasTopLevelJob(JobRole.DepartmentManager)
                                                            || (club.IsUnionClub.Value &&
                                                                User.HasTopLevelJob(JobRole.UnionManager))
                                                            || (isDepartment && (User.HasTopLevelJob(JobRole.ClubManager) || User.HasTopLevelJob(JobRole.ClubSecretary))
                                                            ))))
                {
                    return RedirectToAction("Index", "NotAuthorized");
                }
            }

            var teams = teamRepo.GetClubTeamsByClubAndSeasonId(clubId, seasonId);
            var teamsVm = new List<TeamViewModel>();
            var gameAlias = secRepo.GetByClubId(clubId)?.Alias;
            foreach (var team in teams)
            {
                var registrationActive = team.ActivityFormsSubmittedDatas.FirstOrDefault(x =>
                                             x.Activity.IsAutomatic.HasValue && x.Activity.IsAutomatic.Value &&
                                             x.Activity.Type == ActivityType.Group &&
                                             x.Activity.SeasonId == seasonId)?.IsActive ?? false;
                var department = isDepartment
                    ? team.ClubTeams.Where(c => c.DepartmentId.HasValue).FirstOrDefault()?.Department
                    : null;
                int? departmentSeasonId = null;
                if (department != null)
                {
                    departmentSeasonId = seasonsRepository.GetLastSeasonByCurrentClub(department)?.Id;
                }

                var tennisTeamLeagues = leagueRepo.GetTennisTeamLeagues(team.TeamId, departmentSeasonId ?? seasonId).ToList();
                var registrationEndedLeaguesCount = tennisTeamLeagues.Where(l => l.EndTeamRegistrationDate.HasValue && l.EndTeamRegistrationDate < DateTime.Now).Count();

                teamsVm.Add(new TeamViewModel
                {
                    Id = team.TeamId,
                    Title = team.Title,
                    LeagueNames = section == SectionAliases.Tennis
                        ? String.Join(",",
                           tennisTeamLeagues
                                .Select(c => c.Name))
                        : String.Join(",",
                            leagueRepo.GetByTeamAndSeason(team.TeamId, departmentSeasonId ?? seasonId)
                                .Select(c => c.Name)),
                    IsActive = registrationActive,
                    DepartmentId = department?.ClubId,
                    DepartmentSeasonId = departmentSeasonId,
                    RegistredLeagueName = team.TeamRegistrations
                                              .FirstOrDefault(c => c.ClubId == clubId && c.SeasonId == seasonId)?.League
                                              ?.Name
                                          ?? string.Empty,
                    IsClubInsurance = team.IsClubInsurance,
                    IsTeamPossibleToDelete = registrationEndedLeaguesCount == 0,
                    IsUnionInsurance = team.IsUnionInsurance,
                    PlayersCount = team.TeamsPlayers
                        .Count(tp => !tp.User.IsArchive &&
                                     tp.SeasonId == seasonId &&
                                     (isTennis || tp.IsApprovedByManager == true || tp.IsActive || (gameAlias == GamesAlias.Climbing && !isClubsBlocked)))
                });
            }


            var model = new ClubTeamsForm
            {
                ClubId = clubId,
                Teams = teamsVm,
                SeasonId = seasonId,
                CurrentSeasonId = seasonId,
                SectionId = club.IsSectionClub.Value ? club.SectionId.Value : club.Union.SectionId,
                IsGymnastic = string.Equals(gameAlias, GamesAlias.Gymnastic,
                    StringComparison.InvariantCultureIgnoreCase),
                IsAthletic = string.Equals(gameAlias, GamesAlias.Athletics,
                    StringComparison.InvariantCultureIgnoreCase),
                Section = clubsRepo.GetById(clubId)?.Union?.Section?.Alias ?? clubsRepo.GetById(clubId)?.Section?.Alias,
                IsDepartment = isDepartment,
                SportId = sportId,
                IsClubFeesPaid = club.IsClubFeesPaid,
                IsClubManagerCanSeePayReport = club.IsClubManagerCanSeePayReport
            };
            var topLevelJob = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.IsClubManagerUnderGymnastics =
                club.UnionId == 36 && club.IsUnionClub == true && (topLevelJob == JobRole.ClubManager || topLevelJob == JobRole.ClubSecretary);
            ViewBag.IsClubManagerUnderWaterpolo =
                string.Equals(gameAlias, GamesAlias.WaterPolo,
                    StringComparison.InvariantCultureIgnoreCase) && club.IsUnionClub == true && (topLevelJob == JobRole.ClubManager || topLevelJob == JobRole.ClubSecretary);
            ViewBag.JobRole = topLevelJob;
            return PartialView("_ClubTeams", model);
        }

        [HttpPost]
        public ActionResult CreateClubTeam(ClubTeamsForm model)
        {
            var team = new Team();

            if (model.IsNew)
            {
                team.Title = model.TeamName.Trim();
                teamRepo.Create(team);
            }

            else if (model.TeamId != 0 && !model.IsNew)
            {
                team = teamRepo.GetById(model.TeamId, model.SeasonId);
            }
            else
            {
                TempData["ErrExist"] = Messages.TeamNotFound;
                return RedirectToAction(nameof(ClubTeams),
                    new { clubId = model.ClubId, seasonId = model.CurrentSeasonId, sectionId = model.SectionId });
            }

            var clubTeam = new ClubTeam
            {
                ClubId = model.ClubId,
                TeamId = team.TeamId,
                DepartmentId = model.IsDepartment && model.ClubId !=
                               team.ClubTeams.FirstOrDefault(ct => ct.SeasonId == model.TeamSeasonId)?.ClubId
                    ? team.ClubTeams.FirstOrDefault(ct => ct.SeasonId == model.TeamSeasonId)?.ClubId
                    : null,
                SeasonId = model.CurrentSeasonId
            };
            var isExistClubTeam =
                clubsRepo.IsExistClubTeamForCurrentSeason(clubTeam.ClubId, clubTeam.TeamId, clubTeam.SeasonId);
            if (isExistClubTeam)
            {
                TempData["ErrExist"] = Messages.TeamExists;
                return RedirectToAction(nameof(ClubTeams),
                    new { clubId = clubTeam.ClubId, seasonId = model.CurrentSeasonId, sectionId = model.SectionId });
            }

            clubsRepo.CreateTeamClub(clubTeam);

            clubsRepo.Save();

            return RedirectToAction(nameof(ClubTeams),
                new
                {
                    clubId = clubTeam.ClubId,
                    seasonId = model.CurrentSeasonId,
                    sectionId = model.SectionId,
                    sportId = model.SportId
                });
        }

        public ActionResult DeleteTemClub(int clubId, int teamId, int seasonId, int sectionId)
        {
            var clubTeam = clubsRepo.GetTeamClub(clubId, teamId, seasonId);
            if (clubTeam != null)
            {
                if (teamRepo.GetNumberOfLeaguesAndClubs(teamId) == 0 || clubTeam.Club?.Union?.Section?.Alias == GamesAlias.Tennis)
                {
                    var team = teamRepo.GetById(teamId);
                    team.IsArchive = true;
                }
                teamRepo.Save();
                clubsRepo.RemoveTemClub(clubTeam);
                clubsRepo.Save();
            }

            return RedirectToAction("ClubTeams", new { clubId, seasonId, sectionId });
        }

        public ActionResult EditTeam(int teamId, int clubId, int seasonId, int sectionId)
        {
            if (User.IsInRole(AppRole.Workers) && AuthSvc.AuthorizeTeamByIdAndManagerId(teamId, base.AdminId))
                return RedirectToAction("Index", "NotAuthorized");
            else
            {
                var team = teamRepo.GetById(teamId);
                if (team.IsArchive)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                var vm = new TeamNavView
                {
                    SeasonId = seasonId,
                    TeamName = team.Title,
                    IsValidUser = User.IsInAnyRole(AppRole.Admins, AppRole.Editors),
                    TeamId = team.TeamId,
                    TeamLeagues = leagueRepo.GetByTeamAndSeasonShort(teamId, seasonId),
                    clubs = clubsRepo.GetByTeamAndSeasonShort(teamId, seasonId),
                    SectionId = sectionId
                };
                ViewBag.ClubId = clubId;
                return View("EditTeamClub", vm);
            }
        }

        [NonAction]
        private string SaveFile(HttpPostedFileBase file, string name, bool isTeam = false)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!GlobVars.ValidImages.Contains(ext))
                return null;

            var newName = name + "_" + AppFunc.GetUniqName() + ext;

            var savePath = Server.MapPath(GlobVars.ClubContentPath);
            if (isTeam)
            {
                savePath = Server.MapPath(GlobVars.TeamContentPath);
            }
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

        public ActionResult TeamTrainings(int clubId, int seasonId)
        {
            var clubTrainings = _objClubTrainingsRepo.GetAllClubTeams(clubId, seasonId);
            var clubTrainingsViewModel = new List<ClubTrainingsViewModel>();
            ViewBag.IsAllBlocked = true;

            var section = "";
            if (clubTrainings.Any())
            {
                section = secRepo.GetByClubId(clubTrainings[0].ClubId).Alias;
            }

            ViewBag.Section = section;

            foreach (var training in clubTrainings)
            {
                if (training.IsBlocked == false)
                {
                    ViewBag.IsAllBlocked = false;
                }

                var clubTraining = new ClubTrainingsViewModel
                {
                    TeamId = training.TeamId,
                    TeamTitle = training.Team.Title,
                    IsBlocked = training.IsBlocked,
                    TeamPosition = training.TeamPosition
                };
                clubTrainingsViewModel.Add(clubTraining);
            }

            var clubTtraining = clubTrainings.FirstOrDefault(c => c.SeasonId == seasonId && c.ClubId == clubId);
            if (clubTtraining != null && clubTtraining.TeamPosition == 0)
            {
                _objClubTrainingsRepo.AddPositlionsToTeam(clubId, seasonId);
            }

            #region Auditoriums option

            ViewBag.Auditoriums = _objClubTrainingsRepo.GetAllClubNonArchiveAuditoriums(clubId);
            var clubTrainingDays = _objClubTrainingsRepo.GetClubTrainingDays(clubId);
            var clubTrainingDaysViewModel = new List<ClubTrainingDaysViewModel>();

            foreach (var day in clubTrainingDays)
            {
                var clubTrainingDay = new ClubTrainingDaysViewModel
                {
                    Id = day.Id,
                    Auditorium = day.Auditorium.Name,
                    ClubId = day.ClubId,
                    TrainingDay = day.TrainingDay,
                    TrainingStartTime = day.TrainingStartTime,
                    TrainingEndTime = day.TrainingEndTime
                };
                clubTrainingDaysViewModel.Add(clubTrainingDay);
            }

            #endregion

            var viewModel = new ClubsViewModel
            {
                ClubId = clubId,
                SeasonId = seasonId,
                ClubTrainings = clubTrainingsViewModel.OrderBy(c => c.TeamPosition),
                ClubTrainingDays = clubTrainingDaysViewModel
            };

            ViewBag.Section = clubsRepo.GetById(clubId)?.Union?.Section?.Alias ??
                              clubsRepo.GetById(clubId)?.Section?.Alias;
            return PartialView(viewModel);
        }

        [HttpPost]
        public void ChangeOrder(int clubId, int seasonId, int[] ids)
        {
            var teamsToUpdate = _objClubTrainingsRepo.GetAllClubTeams(clubId, seasonId);
            var teamsDto = new List<ClubTeamsDTO>();
            for (var i = 0; i < ids.Length; i++)
            {
                var teamDto = new ClubTeamsDTO
                {
                    TeamId = ids[i],
                    TeamPosition = i + 1
                };
                teamsDto.Add(teamDto);
            }

            _objClubTrainingsRepo.UpdateTeamPositions(clubId, seasonId, teamsDto);
        }

        [HttpPost]
        public void ChangeSchoolsOrder(int clubId, short[] schoolIds, int seasonId, bool isCamp = false)
        {
            var club = clubsRepo.GetById(clubId);
            var schools = club.Schools.Where(x => x.SeasonId == seasonId && x.IsCamp == isCamp).ToList();

            short sortOrder = 0;
            if (schools.Any())
            {
                foreach (var schoolId in schoolIds)
                {
                    var school = schools.FirstOrDefault(x => x.Id == schoolId);
                    if (school != null)
                    {
                        school.SortOrder = sortOrder;
                    }
                    sortOrder++;

                }
                clubsRepo.Save();
            }
        }

        [HttpPost]
        public void ChangeTeamInSchoolsOrder(int schoolId, short[] schoolTeamIds)
        {
            var school = SchoolRepo.GetById(schoolId);
            var teamsInSchools = new Dictionary<int, Team>();
            foreach (var schoolTeam in school.SchoolTeams)
            {
                teamsInSchools.Add(schoolTeam.Id, schoolTeam.Team);

            }
            short sortOrder = 0;
            if (teamsInSchools.Any())
            {
                foreach (var schoolTeamId in schoolTeamIds)
                {
                    var teamsInSchool = teamsInSchools.FirstOrDefault(x => x.Key == schoolTeamId).Value;
                    if (teamsInSchool != null)
                    {
                        teamsInSchool.SortOrder = sortOrder;
                    }
                    sortOrder++;

                }
                clubsRepo.Save();
            }
        }

        public void UpdateIsBlockedValue(int clubId, int seasonId, int teamId, bool isBlockedValue)
        {
            _objClubTrainingsRepo.UpdateBlockedItem(clubId, teamId, seasonId, isBlockedValue);
        }

        public void UpdateAllBlockedValues(int clubId, int seasonId, bool blockValue)
        {
            _objClubTrainingsRepo.UpdateAllBlockedItems(clubId, seasonId, blockValue);
        }

        [HttpPost]
        public ActionResult SetClubTrainingDays(int clubId, int auditoriumId, string trainingDay,
            string trainingStartTime, string trainingEndTime)
        {
            var trainingsDb = _objClubTrainingsRepo.GetAllDaysTrainings(clubId);
            var idOfTraining = 0;

            _objClubTrainingsRepo.AddClubTrainingDays(clubId, auditoriumId, trainingDay, trainingStartTime,
                trainingEndTime);
            idOfTraining = _objClubTrainingsRepo.GetLastTrainingDayAdded().Id;
            return Json(new { Message = "Save", Id = $"{idOfTraining}" });
        }

        [HttpPost]
        public ActionResult DeleteTrainingDay(int trainingDayId)
        {
            _objClubTrainingsRepo.SetTrainingDayInArchieve(trainingDayId);
            return Json(new { Message = $"Training day deleted." });
        }

        [HttpPost]
        public ActionResult GenerateClubTrainings(int clubId, int sesonId, DateTime startDate, DateTime endDate)
        {
            var clubTeams = _objClubTrainingsRepo.GetAllClubTeams(clubId, sesonId).Where(t => t.IsBlocked)
                .OrderBy(c => c.TeamPosition).ToList();
            var trainingDaySettings = _objClubTrainingsRepo.GetAllDaysTrainings(clubId);
            var allTrainings = new List<TeamTrainingViewModel>();
            var trainingDates = DateTimeHelper.GetDaysInterval(startDate, endDate);
            var listOfTeamNames = new List<string>();
            foreach (var clubTeam in clubTeams)
            {
                var teamTrainingSettings = _objClubTrainingsRepo.GetTrainingSettingsOfTheTeam(clubTeam.TeamId);
                if (teamTrainingSettings == null)
                {
                    listOfTeamNames.Add(clubTeam.Team.Title);
                }
            }

            if (listOfTeamNames.Count > 0)
            {
                return Json(new
                {
                    Success = false,
                    Message = $"{Messages.AlertTeamsWithoutTrSet}:",
                    TeamList = listOfTeamNames
                });
            }


            #region Generate all trainings for all club teams

            #region Generate trainings for each checked training for one week

            var clubTrainingService = new ClubTrainingService(trainingDaySettings);

            foreach (var clubTeam in clubTeams)
            {
                var teamsCount = _objTeamTrainingsRepo.GetAllTeamTrainingsByTeamId(clubTeam.TeamId, sesonId).Count();
                if (teamsCount != 0)
                {
                    _objTeamTrainingsRepo.RemoveTeamTrainingByTeamId(clubTeam.TeamId);
                }

                var trainingSettings = _objTeamTrainingsRepo.GetTrainingSettingForTeam(clubTeam.TeamId);
                var teamArenas = _objTeamTrainingsRepo.GetTeamArenas(clubTeam.TeamId);

                foreach (var trainingDay in trainingDaySettings.GroupBy(s => s.TrainingDay))
                {
                    var training =
                        clubTrainingService.GetTraining(trainingDay.Key, clubTeam, trainingSettings, teamArenas);
                    if (training != null)
                        allTrainings.Add(training);
                }
            }

            var finalVmTrainings = new List<TeamTrainingViewModel>();
            foreach (var team in clubTeams)
            {
                var teamTrainings = allTrainings.Where(t => t.TeamId == team.TeamId).DistinctBy(t => t.TrainingDay)
                    .OrderByDescending(d => d.TrainingDay)
                    .ToList();
                var countOfTrainingsPerWeek = Convert.ToInt32(_objTeamTrainingsRepo
                    .GetTrainingSettingForTeam(team.TeamId)
                    .MinNumTrainingDays);
                var finalTrainings = finalVmTrainings.Where(t => t.TeamId == team.TeamId).ToList();
                var noTrainingDayAfterDayForTeam =
                    _objTeamTrainingsRepo.GetTrainingSettingForTeam(team.TeamId).NoDayAfterDayTrainings;
                var firstValue = true;
                if (noTrainingDayAfterDayForTeam)
                {
                    for (var i = teamTrainings.Count - 1; i >= 0; i--)
                    {
                        if (finalTrainings.Count == 0 || finalTrainings.Count < countOfTrainingsPerWeek)
                        {
                            if (firstValue)
                            {
                                firstValue = false;
                                finalVmTrainings.Add(teamTrainings[i]);
                                finalTrainings.Add(teamTrainings[i]);
                                teamTrainings.RemoveAll(t => t.TrainingDay == (teamTrainings[i].TrainingDay + 1));
                                teamTrainings.Remove(teamTrainings[i - 1]);
                            }
                            else
                            {
                                if (teamTrainings.Count - 1 > 0)
                                {
                                    finalVmTrainings.Add(teamTrainings[teamTrainings.Count - 1]);
                                    finalTrainings.Add(teamTrainings[teamTrainings.Count - 1]);
                                    teamTrainings.RemoveAll(t =>
                                        t.TrainingDay == (teamTrainings[teamTrainings.Count - 1].TrainingDay + 1));
                                    if (teamTrainings.Count - 1 >= 0)
                                        teamTrainings.Remove(teamTrainings[teamTrainings.Count - 1]);
                                }
                                else if (teamTrainings.Count == 1)
                                {
                                    finalVmTrainings.Add(teamTrainings[0]);
                                    finalTrainings.Add(teamTrainings[0]);
                                    teamTrainings.Remove(teamTrainings[0]);
                                }
                                else
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var teamTraining in teamTrainings.ToList())
                    {
                        if (finalTrainings.Count == 0 || finalTrainings.Count < countOfTrainingsPerWeek)
                        {
                            finalVmTrainings.Add(teamTraining);
                            finalTrainings.Add(teamTraining);
                            teamTrainings.Remove(teamTraining);
                        }
                    }
                }
            }

            foreach (var team in clubTeams)
            {
                var teamTrainings = finalVmTrainings.Where(t => t.TeamId == team.TeamId).DistinctBy(t => t.TrainingDay)
                    .ToList();
                var countOfTrainingsPerWeek = Convert.ToInt32(_objTeamTrainingsRepo
                    .GetTrainingSettingForTeam(team.TeamId)
                    .MinNumTrainingDays);
                if (teamTrainings.Count < countOfTrainingsPerWeek)
                {
                    listOfTeamNames.Add(team.Team.Title);
                }
            }

            if (listOfTeamNames.Count > 0)
            {
                return Json(new
                {
                    Success = false,
                    Message = $"{Messages.AlertAvailability}:",
                    TeamList = listOfTeamNames
                });
            }

            #endregion

            var finalTrainingVm = new List<TeamTrainingViewModel>();
            foreach (var training in finalVmTrainings)
            {
                foreach (var date in trainingDates)
                {
                    if (training.TrainingDay == date.DayOfWeek)
                    {
                        var trainingDateString =
                            $"{date.ToString("yyyy-MM-dd")} {training.TrainingStartTime.ToString()}";
                        var trainingDate = DateTime.ParseExact(trainingDateString, "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture);
                        var trainingVm = new TeamTrainingViewModel
                        {
                            TeamId = training.TeamId,
                            AuditoriumId = training.AuditoriumId,
                            TrainingDate = trainingDate
                        };
                        finalTrainingVm.Add(trainingVm);
                    }
                }
            }

            #endregion

            var filteredTrainings = new List<TeamTrainingViewModel>();

            #region Filter trainings

            foreach (var team in clubTeams)
            {
                var trainingSettings = _objTeamTrainingsRepo.GetTrainingSettingForTeam(team.TeamId);
                var isConsiderationHolidays = trainingSettings.ConsiderationHolidays;
                var dontAddTrainingSameDay = trainingSettings.TrainingSameDay;
                var dontAddTrainingOnFollowDay = trainingSettings.TrainingFollowDay;
                var addTrainingBeforeGame = trainingSettings.TrainingBeforeGame;
                var dontLet2Trainings = trainingSettings.NoTwoTraining;
                var coachTeams = _objTeamTrainingsRepo.GetCoachTeams(team.TeamId);
                var teamTrainings = finalTrainingVm.Where(t => t.TeamId == team.TeamId)
                    .OrderBy(t => t.TrainingDate).ToList();
                var gamesOfTeam = _objTeamTrainingsRepo.GetDatesOfGames(team.TeamId, startDate, endDate);

                //Add a day before game
                if (addTrainingBeforeGame)
                {
                    foreach (var gameDate in gamesOfTeam)
                    {
                        var dayBeforeGame = gameDate.AddDays(-1);
                        teamTrainings.Add(new TeamTrainingViewModel
                        {
                            TeamId = team.TeamId,
                            AuditoriumId = null,
                            TrainingDate = dayBeforeGame.Date
                        });
                    }
                }

                //dont add training on the follow day as a game
                if (dontAddTrainingOnFollowDay)
                {
                    foreach (var teamTraining in teamTrainings.ToList())
                    {
                        foreach (var game in gamesOfTeam.ToList())
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

                //Dont add training on the same day as a game
                if (dontAddTrainingSameDay)
                {
                    foreach (var teamTraining in teamTrainings.ToList())
                    {
                        foreach (var game in gamesOfTeam.ToList())
                        {
                            if (teamTraining.TrainingDate.Date == game.Date)
                            {
                                teamTrainings.Remove(teamTraining);
                            }
                        }
                    }
                }

                //Check if coach train more than one team and not add trainings if there are conflicts in time
                if (coachTeams.Count > 1)
                {
                    var listOfTeamDates = new List<DateTime>();
                    for (var i = 0; i < coachTeams.Count; i++)
                    {
                        var teamsTrainingDates = _objTeamTrainingsRepo.GetAllTeamTrainingsByTeamId(coachTeams[i].TeamId, sesonId)
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

                //Delete all holidays
                if (isConsiderationHolidays)
                {
                    var holidays = new Holidays();
                    var holidaysList = holidays.GetAllHolidaysDates();
                    teamTrainings.RemoveHolidays(holidaysList);
                }

                //Leave only one training at the same day
                if (dontLet2Trainings)
                {
                    var distinctTrainings = teamTrainings
                        .DistinctBy(t => t.TrainingDate.Date).ToList();
                    filteredTrainings.AddRange(distinctTrainings);
                }
                else
                {
                    filteredTrainings.AddRange(teamTrainings);
                }
            }

            #endregion

            foreach (var training in filteredTrainings)
            {
                var model = new TeamTraining
                {
                    Title = Messages.Training,
                    TeamId = training.TeamId,
                    AuditoriumId = training.AuditoriumId,
                    Content = string.Empty,
                    TrainingDate = training.TrainingDate,
                    SeasonId = sesonId
                };
                _objTeamTrainingsRepo.InsertTeamTrainings(model);
            }

            TempData["ShowTrainings"] = true;
            return Json(new { Success = true });
        }

        private List<TeamTrainingViewModel> GetTrainingsViewModel(List<TeamTraining> clubTrainings, int seasonId)
        {
            var clubTrainingsViewModel = new List<TeamTrainingViewModel>();
            var teamsIds = clubTrainings.Select(x => x.TeamId).ToArray();
            var allPlayers = _objTeamTrainingsRepo.GetPlayersByTeamId(seasonId, teamsIds).ToList();
            var playersAttendanceDictionary = _objTeamTrainingsRepo.GetAttendances(teamsIds);
            foreach (var clubTraining in clubTrainings)
            {
                var teamArenas = _objTeamTrainingsRepo.GetTeamArenas(clubTraining.TeamId);
                var teamName = clubTraining.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)?.TeamName;
                var teamTrainingVm = new TeamTrainingViewModel
                {
                    Id = clubTraining.Id,
                    Title = clubTraining.Title,
                    TrainingReport = clubTraining.TrainingReport,
                    TeamId = clubTraining.TeamId,
                    TeamName = string.IsNullOrEmpty(teamName) ? clubTraining.Team.Title : teamName,
                    AuditoriumId = clubTraining.AuditoriumId,
                    TeamArenas = teamArenas,
                    TrainingDate = clubTraining.TrainingDate,
                    Content = clubTraining.Content,
                    isPublished = clubTraining.isPublished,
                    Players = allPlayers.Where(x => x.TeamId == clubTraining.TeamId),
                    PlayerAttendance = playersAttendanceDictionary[clubTraining.TeamId]
                };
                clubTrainingsViewModel.Add(teamTrainingVm);
            }

            return clubTrainingsViewModel;
        }

        private IEnumerable<TeamTrainingViewModel> GetTrainingsViewModel(int clubId, int seasonId)
        {
            var clubTrainingsModel = _objClubTrainingsRepo.GetAllClubTrainings(clubId, seasonId);

            var teamsIds = clubTrainingsModel.Select(x => x.TeamId).ToArray();

            var allPlayers = _objTeamTrainingsRepo.GetPlayersByTeamId(seasonId, teamsIds).ToList();
            var allArenas = _objTeamTrainingsRepo.GetTeamArenas(teamsIds);
            var attendances = _objTeamTrainingsRepo.GetAttendances(teamsIds);

            return clubTrainingsModel.Select(clubTraining => new TeamTrainingViewModel
            {
                Id = clubTraining.Id,
                Title = clubTraining.Title,
                TeamId = clubTraining.TeamId,
                TeamName = clubTraining.Team.Title,
                TrainingReport = clubTraining.TrainingReport,
                AuditoriumId = clubTraining.AuditoriumId,
                TeamArenas = allArenas.Where(x => x.TeamId == clubTraining.TeamId).ToList(),
                TrainingDate = clubTraining.TrainingDate,
                Content = clubTraining.Content,
                isPublished = clubTraining.isPublished,
                Players = allPlayers.Where(x => x.TeamId == clubTraining.TeamId),
                PlayerAttendance = attendances[clubTraining.TeamId]
            }).ToList();
        }

        public ActionResult ClubTrainings(int clubId, int seasonId, int pageNumber = 1, int pageSize = 10)
        {
            TrainingsPageModel model = new TrainingsPageModel();

            var vm = GetTrainingsViewModel(_objClubTrainingsRepo.GetTrainingsOfFirstDaysOfMonth(clubId, seasonId, true), seasonId).OrderBy(d => d.TrainingDate);
            ViewBag.ClubId = clubId;
            ViewBag.SeasonId = seasonId;

            ViewBag.Section = clubsRepo.GetById(clubId)?.Union?.Section?.Alias ??
                              clubsRepo.GetById(clubId)?.Section?.Alias;
            model.TeamTrainings = vm.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
            var pager = new Pager(vm.Count(), pageNumber, pageSize);
            model.Pager = pager;
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteTrainings(List<int> trainingIds)
        {
            _objClubTrainingsRepo.DeleteTrainings(trainingIds);
            return Json(new { Ok = true });
        }

        [HttpPost]
        public ActionResult Filter(int clubId, int seasonId, DateTime? startFilterDate, DateTime? endFilterDate,
            string filterValue,
            string sortValue,
            int pageNumber = 1,
            int pageSize = 10)
        {
            TrainingsPageModel model = new TrainingsPageModel();
            var vm = new List<TeamTrainingViewModel>();
            ViewBag.FilterStartDate = startFilterDate;
            ViewBag.FilterEndDate = endFilterDate;
            ViewBag.FilterValue = filterValue;
            ViewBag.SeasonId = seasonId;
            ViewBag.ClubId = clubId;
            ViewBag.SortValue = sortValue;
            var section = _objClubTrainingsRepo.GetClubById(clubId).Section.Alias;
            ViewBag.Section = section;

            switch (filterValue)
            {
                case "fltr_bom":
                    vm = GetTrainingsViewModel(
                        _objClubTrainingsRepo.GetTrainingsOfFirstDaysOfMonth(clubId, seasonId, true), seasonId);
                    break;
                case "fltr_ranged":
                    var startDate = startFilterDate ?? DateTime.Now;
                    var endDate = endFilterDate ?? DateTime.Now;
                    vm = GetTrainingsViewModel(
                        _objClubTrainingsRepo.GetTrainingsInDateRange(clubId, seasonId, startDate, endDate, true), seasonId);
                    break;
                case "fltr_all":
                    vm = GetTrainingsViewModel(
                        _objClubTrainingsRepo.GetAllClubTrainings(clubId, seasonId, true), seasonId);
                    break;
                default:
                    throw new FormatException("Unknown type of select value!");
            }

            var sortedListOfTrainings = new List<TeamTrainingViewModel>();
            switch (sortValue)
            {
                case "sortByDate":
                    sortedListOfTrainings = vm.OrderBy(d => d.TrainingDate).ToList();
                    break;
                case "sortByArena":
                    sortedListOfTrainings = vm.OrderBy(a => a.AuditoriumId).ToList();
                    break;
                case "sortByTeam":
                    sortedListOfTrainings = vm.OrderBy(a => a.TeamName).ToList();
                    break;
                default:
                    throw new FormatException("Unknown type of select value!");
            }
            if (pageSize > 100)
            {
                model.TeamTrainings = sortedListOfTrainings;
            }
            else
            {
                model.TeamTrainings = sortedListOfTrainings.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
            }
            var pager = new Pager(vm.Count(), pageNumber, pageSize);
            model.Pager = pager;
            return PartialView("ClubTrainings", model);
        }

        [HttpGet]
        public ActionResult AddTraining(int? clubId, int? teamId, int seasonId)
        {
            List<SelectListItem> teamSelectList = new List<SelectListItem>();
            List<SelectListItem> arenasSelectList = new List<SelectListItem>();
            bool isTeam = false;
            if (teamId.HasValue)
            {
                arenasSelectList = new SelectList(_objTeamTrainingsRepo.GetTeamArenas(teamId.Value), "AuditoriumId", "Auditorium.Name").ToList();
                isTeam = true;
            }
            else if (clubId.HasValue)
            {
                teamSelectList = new SelectList(teamRepo.GetClubTeamsByClubAndSeasonId(clubId.Value, seasonId, null, true).Distinct(), nameof(Team.TeamId), nameof(Team.Title)).ToList();
                var allArenas = _objTeamTrainingsRepo.GetActiveClubArenas(seasonId, clubId.Value);
                arenasSelectList = new SelectList(allArenas, "AuditoriumId", "Name").ToList();
            }

            var trainingForm = new TrainingForm
            {
                SeasonId = seasonId,
                ClubId = clubId,
                TeamsSelectListItems = teamSelectList,
                AuditoriumSelectListItems = arenasSelectList,
                SelectedTeamId = teamId,
                IsTeam = isTeam
            };

            return PartialView("_AddTraining", trainingForm);
        }

        [HttpPost]
        public ActionResult AddTraining(TrainingForm trainingForm)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AddTraining", trainingForm);
            }

            TeamTraining teamTraining = new TeamTraining
            {
                SeasonId = trainingForm.SeasonId,
                AuditoriumId = trainingForm.AuditoriumId,
                Content = trainingForm.Content,
                Title = trainingForm.Title,
                TrainingDate = trainingForm.TrainingTime,
                TeamId = trainingForm.SelectedTeamId ?? 0,
                isPublished = true
            };
            _objClubTrainingsRepo.SaveTeamTraining(teamTraining);
            //upload image and add to training
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
                    var newName = SaveFile(postedFile, $"trainingReport_{teamTraining.Id}_", true);
                    if (newName == null)
                    {
                        ModelState.AddModelError("ImageFile", Messages.FileError);
                    }
                    else
                    {
                        _objClubTrainingsRepo.UpdateTeamTrainingFileName(teamTraining.Id, newName);
                    }
                }
            }
            return Json(trainingForm.IsTeam);
        }



        [HttpPost]
        public ActionResult PublishToApp(int clubId, int seasonId)
        {
            var clubTeams = _objClubTrainingsRepo.GetAllClubTeams(clubId, seasonId);
            var allPublished = true;
            foreach (var team in clubTeams)
            {
                var trainings = _objTeamTrainingsRepo.GetAllTeamTrainingsByTeamId(team.TeamId, seasonId);
                foreach (var isPublishedValue in trainings.Select(t => t.isPublished))
                {
                    if (isPublishedValue == false)
                    {
                        allPublished = false;
                    }
                }
            }

            _objClubTrainingsRepo.PublishToApp(clubId, seasonId, allPublished);
            if (allPublished)
            {
                return Json(new { Message = "Unchecked" });
            }

            return Json(new { Message = "Checked" });
        }

        public ActionResult Calendar(int clubId, int seasonId)
        {
            ViewBag.SeasonId = seasonId;
            ViewBag.ClubId = clubId;
            return PartialView();
        }

        [HttpPost]
        public ActionResult NewCalendarObject(int clubId, int seasonId)
        {
            var trainings = _objClubTrainingsRepo.GetAllClubTrainings(clubId, seasonId, true, true)
                .OrderBy(d => d.TrainingDate).ToList();
            var dates = new List<ClubCalendarViewModel>();
            List<int> schoolTeamIds = new List<int>();
            List<int> campTeamIds = new List<int>();
            var club = db.Clubs.Find(clubId);
            var schools = club.Schools.Where(x => x.SeasonId == seasonId);
            foreach (var school in schools)
            {
                if (school.IsCamp)
                {
                    campTeamIds.AddRange(school.SchoolTeams.Select(y => y.TeamId).ToList());
                }
                else
                {
                    schoolTeamIds.AddRange(school.SchoolTeams.Select(y => y.TeamId).ToList());
                }
            }

            foreach (var training in trainings)
            {
                var attendanceList = new List<string>();
                foreach (var attendance in training.TrainingAttendances)
                {
                    var user = attendance.TeamsPlayer?.User;
                    if (user != null)
                    {
                        attendanceList.Add($"{user.FirstName} {user.LastName}");
                    }
                }

                ColorModel color = new ColorModel { Id = 1, ColorName = "Navy", HexCode = "#000080" };
                if (schoolTeamIds.Contains(training.TeamId))
                {
                    color = new ColorModel { Id = 2, ColorName = "Crimson", HexCode = "#DC143C" };
                }
                else if (campTeamIds.Contains(training.TeamId))
                {
                    color = new ColorModel { Id = 5, ColorName = "Olive", HexCode = "#808000" };
                }

                var teamName = training.Team?.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)?.TeamName;
                var calendarDate = new ClubCalendarViewModel
                {
                    id = training.Id,
                    title = training.Title,
                    color = color.HexCode,
                    description = training.Content,
                    start = ConvertDateTimeToStringForCalendar(training.TrainingDate),
                    end = null,
                    attendanceList = attendanceList,
                    auditorium = training?.Auditorium?.Name,
                    hasImage = !string.IsNullOrEmpty(training?.TrainingReport),
                    teamName = !string.IsNullOrEmpty(teamName) ? teamName : (training.Team != null ? training.Team.Title : "")
                };
                dates.Add(calendarDate);
            }

            ViewBag.SeasonId = seasonId;
            ViewBag.ClubId = clubId;
            return Json(dates, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTrainingReport(int trainingId)
        {
            var trainingReport = db.TeamTrainings.FirstOrDefault(x => x.Id == trainingId)?.TrainingReport;
            return Json(GlobVars.ContentPath + "/Teams/" + trainingReport, JsonRequestBehavior.AllowGet);
        }

        private string ConvertDateTimeToStringForCalendar(DateTime dateTime)
        {
            var date = dateTime.ToString("yyyy-MM-dd");
            var time = dateTime.ToString("HH:mm:ss");
            return $"{date}T{time}";
        }

        [HttpPost]
        public ActionResult CalendarObject(int clubId, int? seasonId)
        {
            var calendarHelper = new UnionCalendarHelper();
            var model = calendarHelper.GetDepartmentCalendarObject(clubId);
            return Json(model, JsonRequestBehavior.AllowGet);
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

        private void ExportClubTrainingsToExcel(List<TeamTraining> clubTrainings, int clubId, int seasonId)
        {
            using (var workBook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var clubName = _objClubTrainingsRepo.GetClubName(clubId);
                var ws = workBook.AddWorksheet($"{Messages.TrainingSchedule}");
                ws.Cell(1, 1).Value = "Id";
                ws.Cell(1, 2).Value = Messages.Title;
                ws.Cell(1, 3).Value = Messages.Name;
                ws.Cell(1, 4).Value = Messages.Date;
                ws.Cell(1, 5).Value = Messages.Time;
                ws.Cell(1, 6).Value = Messages.Auditorium;
                ws.Cell(1, 7).Value = Messages.Content;
                ws.Cell(1, 8).Value = Messages.Attendance;
                ws.Cell(1, 9).Value = "PlayersIds";
                ws.Column(1).AdjustToContents(5D, 7D);
                ws.Columns(2, 9).AdjustToContents(2, 9, 12D, 20D);
                ws.Column(3).AdjustToContents(18D, 18D);

                var rowNum = 2;
                foreach (var row in clubTrainings)
                {
                    ws.Cell(rowNum, 1).SetValue(row.Id);
                    ws.Cell(rowNum, 2).SetValue(row.Title);
                    ws.Cell(rowNum, 3).SetValue(row.Team.Title);
                    ws.Cell(rowNum, 4).SetValue(row.TrainingDate.ToShortDateString());
                    ws.Cell(rowNum, 5).SetValue(row.TrainingDate.ToString("HH:mm"));
                    var auditorium = row.Auditorium == null ? "" : row.Auditorium.Name;
                    ws.Cell(rowNum, 6).SetValue(auditorium);
                    ws.Cell(rowNum, 7).SetValue(row.Content);
                    var playersForTeamName = row.TrainingAttendances.Select(t => t.TeamsPlayer.User.FullName);
                    var playersForTeamIds = row.TrainingAttendances.Select(t => t.PlayerId);
                    var playersString = GetPlayersNameString(playersForTeamName);
                    var playersIds = GetPlayersIdsString(playersForTeamIds);
                    ws.Cell(rowNum, 8).SetValue(playersString);
                    ws.Cell(rowNum, 9).SetValue(playersIds);
                    rowNum++;
                }

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition",
                    $"attachment;filename= {clubName}-{Messages.TrainingSchedule}-{DateTime.Now}.xlsx");

                using (var myMemoryStream = new MemoryStream())
                {
                    workBook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        [HttpPost]
        public void ExportClubTrainingsToXML(int clubId, int seasonId,
            DateTime startFilterDate, DateTime endFilterDate,
            string filterValue, string sortValue)
        {
            try
            {
                var vm = new List<TeamTraining>();
                switch (filterValue)
                {
                    case "fltr_bom":
                        vm = _objClubTrainingsRepo.GetTrainingsOfFirstDaysOfMonth(clubId, seasonId);
                        break;
                    case "fltr_ranged":
                        var startDate = startFilterDate;
                        var endDate = endFilterDate;
                        vm = _objClubTrainingsRepo.GetTrainingsInDateRange(clubId, seasonId, startDate, endDate);
                        break;
                    case "fltr_all":
                        vm = _objClubTrainingsRepo.GetAllClubTrainings(clubId, seasonId);
                        break;
                    default:
                        vm = _objClubTrainingsRepo.GetAllClubTrainings(clubId, seasonId);
                        break;
                }

                var sortedListOfTrainings = new List<TeamTraining>();
                switch (sortValue)
                {
                    case "sortByDate":
                        sortedListOfTrainings = vm.OrderBy(d => d.TrainingDate).ToList();
                        break;
                    case "sortByArena":
                        sortedListOfTrainings = vm.OrderBy(a => a.AuditoriumId).ToList();
                        break;
                    case "sortByTeam":
                        sortedListOfTrainings = vm.OrderBy(a => a.Team.Title).ToList();
                        break;
                    default:
                        sortedListOfTrainings = vm;
                        break;
                }

                ExportClubTrainingsToExcel(sortedListOfTrainings, clubId, seasonId);
            }
            catch (Exception e)
            {
            }
        }

        [HttpPost]
        public ActionResult ImportTeamTrainingsDateFromXML(HttpPostedFileBase importFile, int clubId, int seasonId)
        {
            if (importFile != null)
            {
                using (var workBook = new XLWorkbook(importFile.InputStream))
                {
                    //Read the first Sheet from Excel file.
                    var workSheet = workBook.Worksheet(1);
                    var firstRow = true;

                    var excelTeamTrainings = new List<ExcelTeamTrainingDto>();

                    var localCulture = CultureInfo.CurrentCulture.ToString();
                    const int teamTrainingIdColumn = 1;
                    const int dateColumn = 4;
                    const int timeColumn = 5;

                    try
                    {
                        foreach (var row in workSheet.Rows())
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

                        _objClubTrainingsRepo.UpdateDatesInTable(excelTeamTrainings, clubId, seasonId);
                    }
                    catch (Exception ex)
                    {
                        // ignored
                    }
                }
            }

            #region GetUpdated values

            var clubTrainings = _objClubTrainingsRepo.GetAllClubTrainings(clubId, seasonId);
            var vm = new List<TeamTrainingViewModel>();
            var model = GetTrainingsViewModel(clubId, seasonId);
            ViewBag.ClubId = clubId;
            ViewBag.SeasonId = seasonId;

            #endregion

            return RedirectToAction("Edit", new
            {
                id = clubId,
                seasonId = seasonId
            });
        }

        public ActionResult CompetitionsList(int clubId, int unionId, int seasonId, int? teamId = null)
        {
            var sectionAlias = clubsRepo.GetSectionByClubId(clubId);
            var clubDisciplinesIds =
                db.ClubDisciplines
                    .Where(x => x.ClubId == clubId)
                    .Select(c => c.DisciplineId)
                    .Distinct()
                    .ToList();

            var teamDisciplinesIds =
                db.TeamDisciplines
                    .Where(x => x.TeamId == teamId)
                    .Select(c => c.DisciplineId)
                    .Distinct()
                    .ToList();

            var model = new UnionCompetitionForm
            {
                ClubId = clubId,
                UnionId = unionId,
                SeasonId = seasonId,
                Instruments = leagueRepo.GetAllInstrumentsDto(),
                SelectedRefereesIds = leagueRepo.GetAllRegisteredRefereesIds(clubId, seasonId, unionId),
                ClubReferees = leagueRepo.GetAllClubRelatedReferees(clubId, false),
                SectionAlias = sectionAlias
            };

            if (GamesAlias.Athletics == sectionAlias)
            {
                var loadData = User.IsInAnyRole(AppRole.Admins) || JobRole.UnionManager.Equals(usersRepo.GetTopLevelJob(AdminId));


                model.Competitions =
                    leagueRepo.GetByUnion(unionId, seasonId)
                    .Where(l => l.EilatTournament == true && l.LeagueEndDate > DateTime.Now.AddDays(-1) && l.StartRegistrationDate != null)
                    .OrderBy(x => x.LeagueStartDate ?? DateTime.MaxValue)
                    .Select(l => new CompetitionDto
                    {
                        LeagueId = l.LeagueId,
                        CompetitionName = l.Name,
                        StartDate = l.LeagueStartDate,
                        EndDate = l.LeagueEndDate,
                        EndRegistrationDate = l.EndRegistrationDate,
                        StartRegistrationDate = l.StartRegistrationDate,
                        Place = l.PlaceOfCompetition,
                        SeasonId = seasonId,
                        IsMaxed = l.MaxRegistrations.HasValue && l.MaxRegistrations <= l.CompetitionRegistrations.Count(),
                        IsEnded = l.EndRegistrationDate.HasValue && l.EndRegistrationDate.Value <= DateTime.Now,
                        IsStarted = !l.StartRegistrationDate.HasValue || l.StartRegistrationDate.Value <= DateTime.Now,
                        CompetitionDisciplines = (loadData || (DateTime.Compare(l.StartRegistrationDate.Value, DateTime.Now) <= 0 && DateTime.Compare(l.EndRegistrationDate.Value, DateTime.Now) >= 0)) ? disciplinesRepo.GetCompetitionDisciplines(l, clubId, seasonId, sectionAlias) : new List<CompetitionDisciplineDto>(),
                        IsClubCompetition = l.IsClubCompetition
                    })
                    .ToList();
            }
            else if (sectionAlias == GamesAlias.Rowing)
            {
                var loadData = User.IsInAnyRole(AppRole.Admins) || JobRole.UnionManager.Equals(usersRepo.GetTopLevelJob(AdminId));
                ViewBag.IsAdmin = loadData;

                model.Competitions =
                    leagueRepo.GetByUnion(unionId, seasonId)
                    .Where(l => l.EilatTournament == true)
                    .Select(l => new CompetitionDto
                    {
                        LeagueId = l.LeagueId,
                        CompetitionName = l.Name,
                        StartDate = l.LeagueStartDate,
                        EndDate = l.LeagueEndDate,
                        EndRegistrationDate = l.EndRegistrationDate,
                        StartRegistrationDate = l.StartRegistrationDate,
                        StartTeamRegistrationDate = l.StartTeamRegistrationDate,
                        EndTeamRegistrationDate = l.EndTeamRegistrationDate,
                        Place = l.PlaceOfCompetition,
                        SeasonId = seasonId,
                        IsMaxed = l.MaxRegistrations.HasValue && l.MaxRegistrations <= l.CompetitionRegistrations.Count(),
                        IsEnded = l.EndRegistrationDate.HasValue && l.EndRegistrationDate.Value <= DateTime.Now,
                        IsStarted = !l.StartRegistrationDate.HasValue || l.StartRegistrationDate.Value <= DateTime.Now,
                        CompetitionDisciplines = (loadData || (l.LeagueEndDate.HasValue && DateTime.Compare(l.LeagueEndDate.Value, DateTime.Now) >= 0)) ? disciplinesRepo.GetCompetitionDisciplines(l, clubId, seasonId, sectionAlias) : new List<CompetitionDisciplineDto>(),
                        IsClubCompetition = l.IsClubCompetition,
                        NoTeamRegistration = l.NoTeamRegistration ?? false,
                    })
                    .ToList();
            }
            else if (sectionAlias == GamesAlias.Swimming)
            {
                model.Competitions =
                    leagueRepo.GetByUnion(unionId, seasonId)
                    ?.Where(l => l.EilatTournament == true)
                    ?.Select(l => new CompetitionDto
                    {
                        LeagueId = l.LeagueId,
                        CompetitionName = l.Name,
                        StartDate = l.LeagueStartDate,
                        EndDate = l.LeagueEndDate,
                        EndRegistrationDate = l.EndRegistrationDate,
                        StartRegistrationDate = l.StartRegistrationDate,
                        Place = l.PlaceOfCompetition,
                        SeasonId = seasonId,
                        IsMaxed = l.MaxRegistrations.HasValue && l.MaxRegistrations <= l.CompetitionRegistrations.Count(),
                        IsEnded = l.EndRegistrationDate.HasValue && l.EndRegistrationDate.Value <= DateTime.Now,
                        IsStarted = !l.StartRegistrationDate.HasValue || l.StartRegistrationDate.Value <= DateTime.Now,
                        CompetitionDisciplines = disciplinesRepo.GetCompetitionDisciplines(l, clubId, seasonId, sectionAlias),
                        IsClubCompetition = l.IsClubCompetition
                    })
                    .ToList();
            }
            else if (sectionAlias == GamesAlias.Bicycle || GamesAlias.Climbing == sectionAlias)
            {
                model.Competitions =
                   leagueRepo.GetByUnion(unionId, seasonId)
                   ?.Select(l => new CompetitionDto
                   {
                       LeagueId = l.LeagueId,
                       CompetitionName = l.Name,
                       StartDate = l.LeagueStartDate,
                       EndDate = l.LeagueEndDate,
                       EndRegistrationDate = l.EndRegistrationDate,
                       StartRegistrationDate = l.StartRegistrationDate,
                       Place = l.PlaceOfCompetition,
                       SeasonId = seasonId,
                       IsMaxed = l.MaxRegistrations.HasValue && l.MaxRegistrations <= l.CompetitionRegistrations.Count(),
                       IsEnded = l.EndRegistrationDate.HasValue && l.EndRegistrationDate.Value <= DateTime.Now,
                       IsStarted = !l.StartRegistrationDate.HasValue || l.StartRegistrationDate.Value <= DateTime.Now,
                       CompetitionDisciplines = disciplinesRepo.GetCompetitionDisciplines(l, clubId, seasonId, sectionAlias),
                       IsClubCompetition = l.IsClubCompetition,
                       RegistrationLink = l.RegistrationLink
                   })
                   .ToList();
            }
            else
            {
                if (clubDisciplinesIds.Any() && !teamId.HasValue)
                {
                    model.Competitions = disciplinesRepo.GetDisciplineCompetitions(clubDisciplinesIds, seasonId);
                    model.ClubPlayersIds = playersRepo.GetActiveClubPlayersIds(clubId, seasonId);
                }
                else if (teamDisciplinesIds.Any() && teamId.HasValue)
                {
                    model.Competitions = disciplinesRepo.GetDisciplineCompetitions(teamDisciplinesIds, seasonId);
                    model.ClubPlayersIds = playersRepo.GetActiveTeamPlayersIds(teamId.Value, seasonId);
                }
            }

            return PartialView("_CompetitionsList", model);
        }

        public ActionResult MartialArtsCompetitionsList(int clubId, int unionId, int seasonId)
        {
            var competitions = leagueRepo.GetAllMartialArtsCompetitions(clubId, unionId, seasonId);
            var season = seasonsRepository.GetById(seasonId);
            var alias = season != null ? season.Union?.Section?.Alias : null;
            int playersCount = 0;
            var vm = new UnionCompetitionForm
            {
                ClubId = clubId,
                UnionId = unionId,
                SeasonId = seasonId,
                Competitions = leagueRepo.GetAllMartialArtsCompetitions(clubId, unionId, seasonId),
                ClubSportsmen =
                    playersRepo.GetTeamPlayersByClubId(clubId, seasonId, null, null, null, null, null, null, null, null, alias, out playersCount)
                        ?.Where(c => c.IsApproveChecked == true),
                ClubReferees = leagueRepo.GetAllClubRelatedReferees(clubId, false),
                SelectedRefereesIds = leagueRepo.GetAllRegisteredRefereesIds(clubId, seasonId, unionId),
                SelectedSportsmenIds = leagueRepo.GetAllRegisteredSportsmenIds(clubId, seasonId, unionId),
                SectionAlias = GamesAlias.MartialArts
            };

            return PartialView("_MartialArtsCompetitionsList", vm);
        }

        [HttpPost]
        public ActionResult CompetitionDetails(int competitionId, int clubId, Sections section = Sections.None)
        {
            var league = leagueRepo.GetById(competitionId);
            var sectionAlias = leagueRepo.GetSectionAlias(competitionId);
            var vm = new CompetitionForm();
            if (league != null)
            {
                vm.CompetitionId = league.LeagueId;
                vm.DisciplineName = league.Discipline?.Name;
                vm.NameOfCompetition = league.Name;
                vm.AboutCompetition = league.AboutLeague;
                vm.CompetitionStructure = league.LeagueStructure;
                vm.Logo = league.Logo;
                vm.MaxRegistrations = league.MaxRegistrations;
                vm.Image = league.Image;
                vm.StartDate = league.LeagueStartDate;
                vm.EndDate = league.LeagueEndDate;
                vm.StartRegistrationDate = league.StartRegistrationDate;
                vm.EndRegistrationDate = league.EndRegistrationDate;
                vm.StartTeamRegistrationDate = league.StartTeamRegistrationDate;
                vm.EndTeamRegistrationDate = league.EndTeamRegistrationDate;
                vm.RoutesRanks = league.CompetitionRoutes.Any()
                    ? league.CompetitionRoutes.Select(cr => new BasicRouteForm
                    {
                        RouteName = cr.DisciplineRoute.Route,
                        RankName = cr.RouteRank.Rank,
                        Composition = cr.Composition,
                        MaxRegistrationsAllowed = cr.CompetitionRouteClub.FirstOrDefault(cc => cc.ClubId == clubId)?.MaximumRegistrationsAllowed
                    })
                    : Enumerable.Empty<BasicRouteForm>();
                vm.RoutesTeamRanks = league.CompetitionTeamRoutes.Any()
                    ? league.CompetitionTeamRoutes.Select(cr => new BasicRouteForm
                    {
                        RouteName = cr.DisciplineTeamRoute.Route,
                        RankName = cr.RouteTeamRank.Rank,
                        Composition = cr.Composition,
                        MaxRegistrationsAllowed = cr.CompetitionTeamRouteClub.FirstOrDefault(cc => cc.ClubId == clubId)?.MaximumRegistrationsAllowed
                    })
                    : Enumerable.Empty<BasicRouteForm>();
                vm.MinimumPlayers = league.MinimumPlayersTeam;
                vm.MaximumPlayers = league.MaximumPlayersTeam;
                vm.MaximumAge = league.MaximumAge;
                vm.MinimumAge = league.MinimumAge;
                vm.TypeId = league.Type;
                vm.Place = league.PlaceOfCompetition;
                vm.CompetitionDisciplines = sectionAlias != SectionAliases.Bicycle ? disciplinesRepo.GetCompetitionDisciplines(league.LeagueId) : null;
                vm.CompetitionExperties = sectionAlias == SectionAliases.Bicycle ? disciplinesRepo.GetCompetitionExperties(league.LeagueId) : null;
                vm.LevelName = sectionAlias == SectionAliases.Bicycle ? league.CompetitionLevel?.level_name : "";
                vm.TypeName = sectionAlias == SectionAliases.Bicycle ? league.BicycleCompetitionDiscipline?.Name : "";
                vm.RegistrationLink = sectionAlias == SectionAliases.Bicycle ? league.RegistrationLink : "";
            }

            ViewBag.FromTennis = section == Sections.Tennis;
            ViewBag.FromMartialArts = section == Sections.MartialArts;
            ViewBag.FromAthletics = sectionAlias.Equals(GamesAlias.Athletics);
            ViewBag.FromSwimming = sectionAlias.Equals(GamesAlias.Swimming);
            ViewBag.FromWeightLifting = sectionAlias == GamesAlias.WeightLifting;
            ViewBag.FromRowing = sectionAlias == GamesAlias.Rowing;
            ViewBag.FromBicycle = sectionAlias == GamesAlias.Bicycle;
            ViewBag.FromClimbing = sectionAlias == GamesAlias.Climbing;
            return PartialView("_CompetitionDetails", vm);
        }

        [HttpPost]
        public ActionResult UpdateTeamRegistrationNumber(int disciplineId, int clubId, int teamRegistrationNumber)
        {
            disciplinesRepo.UpdateCompetitionDisciplineTeamRegistration(disciplineId, clubId, teamRegistrationNumber);

            return Json(new { Success = true });
        }

        //Used currently only for ROWING
        [HttpPost]
        public ActionResult GetPlayersForRegistration(int disciplineId, int clubId, int seasonId, string sectionAlias = SectionAliases.Rowing)
        {
            var competitionDiscipline = disciplinesRepo.GetCompetitionDisciplineById(disciplineId);
            var result = disciplinesRepo.GetCompetitionDisciplinePlayers(competitionDiscipline, clubId, seasonId, sectionAlias);
            var alreadyRegistered = competitionDiscipline.League.CompetitionDisciplines.SelectMany(x => x.CompetitionDisciplineRegistrations).Select(x => x.UserId).ToList();

            return Json(new { players = result.GroupBy(x => x.TeamTitle), playersCount = result.Count(), alreadyRegistered });
        }

        [HttpPost]
        public ActionResult CreateTeamForCompetitionDiscipline(int disciplineId, int clubId)
        {
            try
            {
                var isClubManager = (usersRepo.GetCurrentClubJob(AdminId, clubId) == JobRole.ClubManager || usersRepo.GetCurrentClubJob(AdminId, clubId) == JobRole.ClubSecretary);
                var teamNum = disciplinesRepo.AddTeamForCompetitionDiscipline(disciplineId, clubId, isClubManager);
                return Json(new { Success = true, teamNum.TeamNumber, TeamId = teamNum.Id });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = Messages.MaximumTeamRegistrationsAllowed });
            }

        }


        public ActionResult GetTeamNumberPerClub(int disciplineId)
        {
            var vm = new ClubTeamsNumberList();
            var discipline = disciplinesRepo.GetCompetitionDisciplineById(disciplineId);
            var disc = disciplinesRepo.GetById(discipline.DisciplineId.Value);
            var teamNumbers = discipline.CompetitionDisciplineClubsRegistrations.ToList();

            vm.Title = disc.Name + " / " + discipline.RowingDistance.Name + " / " + disciplinesRepo.GetCompetitionCategoryById(discipline.CategoryId)?.age_name;

            foreach (var team in teamNumbers)
            {
                var club = team.Club;
                vm.ClubTeamsNumbers.Add(new ClubTeamsNumber()
                {
                    ClubId = club.ClubId,
                    ClubName = club.Name,
                    TeamNumber = team.TeamRegistrations.Value
                });
            }

            return View("_TeamNumberPerClub", vm);
        }

        public ActionResult GetTeamNumberPerClubForAllBoats(int competitionId)
        {
            var vm = GetInfoForAllBoats(competitionId);

            return View("_TeamNumberPerClubForAllBoats", vm);
        }

        private ClubTeamsInfoWithBoatsList GetInfoForAllBoats(int competitionId)
        {
            var vm = new ClubTeamsInfoWithBoatsList();
            var discipline = disciplinesRepo.GetCompetitionDisciplines(competitionId);
            vm.CompetitionId = competitionId;
            vm.Title = discipline.FirstOrDefault()?.League.Name;
            foreach (var d in discipline)
            {
                var model = new ClubTeamsInfoWithBoats();
                var teamNumbers = d.CompetitionDisciplineClubsRegistrations.ToList();

                foreach (var t in teamNumbers)
                {
                    vm.ClubTeamsWithBoats.Add(new ClubTeamsInfoWithBoats()
                    {
                        ClubId = t.ClubId,
                        ClubName = t.Club.Name,
                        Boat = disciplinesRepo.GetById(d.DisciplineId.Value).Name,
                        Distance = d.RowingDistance.Name,
                        Category = d.CompetitionAge?.age_name,
                        TeamNumber = t.TeamRegistrations.Value
                    });
                }

            }

            return vm;
        }

        [HttpPost]
        public ActionResult DisciplineCompetitionRegistration(int disciplineId, int clubId, IEnumerable<int> sportsmenIds, int? teamId = null, IEnumerable<int> coxwainIds = null)
        {

            var competitionDecipline = disciplinesRepo.GetCompetitionDisciplineById(disciplineId);
            var sectionAlias = competitionDecipline.League?.Union?.Section?.Alias;
            var isAllowed = sectionAlias != GamesAlias.Rowing && (User.IsInAnyRole(AppRole.Admins) || JobRole.UnionManager.Equals(usersRepo.GetTopLevelJob(AdminId)));

            var currentRegistrationCount = sectionAlias == GamesAlias.Rowing ? competitionDecipline.CompetitionDisciplineRegistrations.Count() : competitionDecipline.League.CompetitionDisciplines.Sum(x => x.CompetitionDisciplineRegistrations.Count());
            var numberOfSportsmen = disciplinesRepo.GetById(competitionDecipline.DisciplineId.Value).NumberOfSportsmen;
            int? maxRegistrations = null;
            if (numberOfSportsmen.HasValue || competitionDecipline.League.MaxRegistrations.HasValue)
            {
                if (!competitionDecipline.League.MaxRegistrations.HasValue)
                {
                    maxRegistrations = numberOfSportsmen.Value;
                }
                else if (!numberOfSportsmen.HasValue)
                {
                    maxRegistrations = competitionDecipline.League.MaxRegistrations.Value;
                }
                else
                {
                    maxRegistrations = Math.Min(competitionDecipline.League.MaxRegistrations.Value, numberOfSportsmen.Value);
                }
            }
            try
            {
                disciplinesRepo.RegisterSportsmenUnderCompetitionDiscipline(disciplineId, clubId, sportsmenIds ?? Enumerable.Empty<int>(), isAllowed, maxRegistrations, currentRegistrationCount, sectionAlias, teamId, coxwainIds ?? Enumerable.Empty<int>());
            }
            catch (MaximumRequiredException ex)
            {
                if (ex.NumOfRegistrationsLeft > 0)
                {
                    return Json(new { Success = false, Message = Messages.NumberOfRegistrationsLeft.Replace("_", ex.NumOfRegistrationsLeft.ToString()) });
                }
                else
                    return Json(new { Success = false, Message = Messages.NoRegistrationSpotsLeft });
            }
            catch (PlayerAlreadyRegisteredException ex)
            {
                return Json(new { Success = false, Message = Messages.PlayerIsRegistered + ": " + Messages.UserName + ": " + ex.UserName, ex.UserId, ex.IsCoxwain });
            }
            catch (AverageAgeException ex)
            {
                return Json(new { Success = false, Message = Messages.AverageAgeError + " " + ex.AverageAgeValue, AverageAge = true, IsCoxwain = true });
            }
            catch (MaxParticipationAllowedForSportsmanReachedException ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = Messages.MaximumParticipationAllowedForSportsman + ": " + ex.MaxParticipationAllowed + ". " + Messages.UserName + ": " + ex.UserName,
                    ex.UserId,
                    ex.IsCoxwain
                });
            }
            catch (CoxwainAlsoPartOfTeamException ex)
            {
                return Json(new { Success = false, Message = Messages.CoxwainAlsoPartOfTeam, ex.UserId, ex.IsCoxwain });
            }
            catch (UnRegisteredPlayersException ex)
            {
                return Json(new { Success = false, Message = Messages.UnregisteredPlayersAtAll, ex.UserId, ex.UserName });
            }
            catch (MixedCategoryException ex)
            {
                return Json(new { Success = false, Message = Messages.MixedCategoryException });
            }
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult RegisterPlayers(IEnumerable<CompetitionRegistrationForm> playersRegistrations, int clubId,
            int competitionId, int seasonId)
        {
            var isAllowed = User.IsInAnyRole(AppRole.Admins) || JobRole.UnionManager.Equals(usersRepo.GetTopLevelJob(AdminId));
            var maxRegistrations = leagueRepo.GetById(competitionId).MaxRegistrations;
            var creator = db.Users.Where(u => u.UserId == AdminId).FirstOrDefault();
            var created = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            var creator_name = creator.FirstName + " " + creator.LastName;
            try
            {
                playersRepo.RegisterPlayersInCompetition(playersRegistrations, clubId, competitionId, seasonId, isAllowed, maxRegistrations, created, creator_name);
            }
            catch (MaximumRequiredPerClubException ex)
            {
                StringBuilder ErrorMessage = new StringBuilder();
                foreach (KeyValuePair<string, int> entry in ex.GetClubsMaxErrorMap())
                {
                    ErrorMessage.Append($"{entry.Key} {Messages.CanHaveUpTo} {entry.Value} {Messages.OnlyGymnasts}");
                    ErrorMessage.AppendLine();
                }
                return Json(new { Success = false, Message = ErrorMessage.ToString() });
            }
            catch (MaximumRequiredException ex)
            {
                if (ex.NumOfRegistrationsLeft > 0)
                {
                    return Json(new { Success = false, Message = Messages.NumberOfRegistrationsLeft.Replace("_", ex.NumOfRegistrationsLeft.ToString()) });
                }
                else
                    return Json(new { Success = false, Message = Messages.NoRegistrationSpotsLeft });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }

            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult RegisterSportsmen(int clubId, int leagueId, int seasonId, IEnumerable<int> sportsmenIds)
        {
            try
            {
                playersRepo.RegisterSportsmenInCompetition(sportsmenIds, clubId, leagueId, seasonId);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }

            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult RegisterSportsmenForDiscipline(int clubId, int competitionDisciplineId,
            IEnumerable<int> sportsmenIds)
        {
            var isAllowed = User.IsInAnyRole(AppRole.Admins) || JobRole.UnionManager.Equals(usersRepo.GetTopLevelJob(AdminId));
            var competitionDecipline = disciplinesRepo.GetCompetitionDisciplineById(competitionDisciplineId);
            var currentRegistrationCount = competitionDecipline.League.CompetitionDisciplines.Sum(x => x.CompetitionDisciplineRegistrations.Count());
            var maxRegistrations = competitionDecipline.League.MaxRegistrations;
            try
            {
                playersRepo.RegisterSportsmenInCompetitionDiscipline(sportsmenIds, clubId, competitionDisciplineId, isAllowed, maxRegistrations, currentRegistrationCount);
            }
            catch (MaximumRequiredException ex)
            {
                if (ex.NumOfRegistrationsLeft > 0)
                {
                    return Json(new { Success = false, Message = Messages.NumberOfRegistrationsLeft.Replace("_", ex.NumOfRegistrationsLeft.ToString()) });
                }
                else
                    return Json(new { Success = false, Message = Messages.NoRegistrationSpotsLeft });
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = e.Message });
            }

            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult UpdateSportsmanRegistrationForDiscipline(int clubId, int competitionDisciplineId,
            int sportsmanId, int weightDeclaration)
        {
            try
            {
                playersRepo.UpdateSportsmanRegistrationForDiscipline(clubId, competitionDisciplineId, sportsmanId, weightDeclaration);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = e.Message });
            }
            return Json(new { Success = true });
        }

        public void RegisterReferees(int clubId, int seasonId, int leagueId, IEnumerable<int> refereeIds)
        {
            var isSuccess = leagueRepo.RegisterReferees(clubId, seasonId, leagueId, refereeIds);
            if (isSuccess) leagueRepo.Save();
        }


        public ActionResult AllRoutesPlayersRegistrationList(int? clubId, int leagueId, int seasonId,
            int? competitionRouteId = null)
        {
            leagueRepo.SetRegistrationsOrders();

            var playersRegistrationsWithoutInstruments =
                playersRepo.GetPlayersRegistrationsWithoutInstruments(clubId, leagueId, seasonId, competitionRouteId);
            var playersRegistrationsWithInstruments =
                playersRepo.GetPlayersRegistrationsWithInstruments(clubId, leagueId, seasonId, competitionRouteId);
            var teamPlayersRegistrationsWithoutInstruments =
                playersRepo.GetTeamPlayersRegistrationsWithoutInstruments(clubId, leagueId, seasonId, competitionRouteId);
            var teamPlayersRegistrationsWithInstruments =
                playersRepo.GetTeamPlayersRegistrationsWithInstruments(clubId, leagueId, seasonId, competitionRouteId);


            var playersAll = playersRegistrationsWithoutInstruments.Union(playersRegistrationsWithInstruments)
                .OrderBy(c => c.Route)
                .ThenBy(c => c.Rank)
                .ThenBy(c => c.FullName);

            var teamPlayersAll = teamPlayersRegistrationsWithoutInstruments.Union(teamPlayersRegistrationsWithInstruments)
                .OrderBy(c => c.Route)
                .ThenBy(c => c.Rank)
                .ThenBy(c => c.FullName);

            var together = playersAll.Concat(teamPlayersAll);


            ViewBag.InstrumentsList = GetInstrumenList(playersRegistrationsWithInstruments);
            ViewBag.ClubId = clubId;
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueId = leagueId;
            ViewBag.RoutesType = 3;
            ViewBag.ClubName = clubId.HasValue ? clubsRepo.GetById(clubId.Value)?.Name : string.Empty;
            ViewBag.CompetitionName = leagueRepo.GetById(leagueId)?.Name;
            ViewBag.CompetitionRouteId = competitionRouteId;
            ViewBag.IsGymnastics = (leagueRepo.GetSectionAlias(leagueId) ??
                                    clubsRepo.GetSectionByClubId(clubId ?? 0) ?? string.Empty)?
                                   .Equals(GamesAlias.Gymnastic) == true;
            return PartialView("_PlayersRegistrationList", together);


        }



        public ActionResult PlayersRegistrationList(int? clubId, int leagueId, int seasonId,
            int? competitionRouteId = null)
        {
            leagueRepo.SetRegistrationsOrders();

            var playersRegistrationsWithoutInstruments =
                playersRepo.GetPlayersRegistrationsWithoutInstruments(clubId, leagueId, seasonId, competitionRouteId);
            var playersRegistrationsWithInstruments =
                playersRepo.GetPlayersRegistrationsWithInstruments(clubId, leagueId, seasonId, competitionRouteId);

            var playersAll = playersRegistrationsWithoutInstruments.Union(playersRegistrationsWithInstruments)
                .OrderBy(c => c.Route)
                .ThenBy(c => c.Rank)
                .ThenBy(c => c.FullName);
            ViewBag.InstrumentsList = GetInstrumenList(playersRegistrationsWithInstruments);
            ViewBag.ClubId = clubId;
            ViewBag.RoutesType = 1;
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueId = leagueId;
            ViewBag.ClubName = clubId.HasValue ? clubsRepo.GetById(clubId.Value)?.Name : string.Empty;
            ViewBag.CompetitionName = leagueRepo.GetById(leagueId)?.Name;
            ViewBag.CompetitionRouteId = competitionRouteId;
            ViewBag.IsGymnastics = (leagueRepo.GetSectionAlias(leagueId) ??
                                    clubsRepo.GetSectionByClubId(clubId ?? 0) ?? string.Empty)?
                                   .Equals(GamesAlias.Gymnastic) == true;
            return PartialView("_PlayersRegistrationList", playersAll);
        }

        public ActionResult PlayersTeamRegistrationList(int? clubId, int leagueId, int seasonId,
    int? competitionRouteId = null)
        {
            leagueRepo.SetRegistrationsOrders();
            var playersRegistrationsWithoutInstruments =
                playersRepo.GetTeamPlayersRegistrationsWithoutInstruments(clubId, leagueId, seasonId, competitionRouteId);
            var playersRegistrationsWithInstruments =
                playersRepo.GetTeamPlayersRegistrationsWithInstruments(clubId, leagueId, seasonId, competitionRouteId);
            var playersAll = playersRegistrationsWithoutInstruments.Union(playersRegistrationsWithInstruments)
                .OrderBy(c => c.Route)
                .ThenBy(c => c.Rank)
                .ThenBy(c => c.FullName);
            ViewBag.InstrumentsList = GetInstrumenList(playersRegistrationsWithInstruments);
            ViewBag.ClubId = clubId;
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueId = leagueId;
            ViewBag.RoutesType = 2;
            ViewBag.ClubName = clubId.HasValue ? clubsRepo.GetById(clubId.Value)?.Name : string.Empty;
            ViewBag.CompetitionName = leagueRepo.GetById(leagueId)?.Name;
            ViewBag.CompetitionRouteId = competitionRouteId;
            ViewBag.IsGymnastics = (leagueRepo.GetSectionAlias(leagueId) ??
                                    clubsRepo.GetSectionByClubId(clubId ?? 0) ?? string.Empty)?
                                   .Equals(GamesAlias.Gymnastic) == true;
            return PartialView("_PlayersRegistrationList", playersAll);
        }

        public ActionResult AthleticsRegistrationList(int? clubId, int? leagueId, int? disciplineId, int seasonId)
        {
            var sessionAlias = leagueId.HasValue
                ? secRepo.GetByLeagueId(leagueId.Value).Alias
                : secRepo.GetByClubId(clubId.Value).Alias;
            ViewBag.LeagueId = leagueId;

            List<AthleticRegDto> sportsmen;
            sportsmen = playersRepo.GetAthleticsRegistrations(clubId, leagueId, disciplineId, seasonId)?.ToList();
            if (sessionAlias == SectionAliases.Rowing)
            {
                sportsmen = sportsmen?.OrderBy(x => x.DisciplineName).ThenBy(x => x.CategoryName).ThenBy(x => Convert.ToInt32(x.RowingDistance)).ThenBy(x => x.TeamNumber).ToList();
            }
            else
            {
                sportsmen = sportsmen?.OrderBy(t => t.DisciplineName).ToList();
            }
            ViewBag.ClubId = clubId;
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueId = leagueId;
            ViewBag.ClubName = clubId.HasValue ? clubsRepo.GetById(clubId.Value)?.Name : string.Empty;
            ViewBag.DisciplineId = disciplineId;
            var league = leagueRepo.GetById(leagueId ?? 0);
            ViewBag.CompetitionName = league?.Name;
            ViewBag.CompetitionDate = league?.LeagueStartDate?.ToShortDateString() ?? "";
            var competitionDiscipline = disciplinesRepo.GetCompetitionDisciplineById(disciplineId ?? 0);
            ViewBag.CategoryName = competitionDiscipline?.CompetitionAge?.age_name;
            if (competitionDiscipline != null && competitionDiscipline.DisciplineId.HasValue)
            {
                var discipline = disciplinesRepo.GetById(competitionDiscipline.DisciplineId.Value);
                ViewBag.DisciplineName = disciplineId == null || !competitionDiscipline.DisciplineId.HasValue ? "" : discipline.Name;
                ViewBag.Format = discipline.Format;
            }
            else
            {
                //ViewBag.DisciplineName = disciplineId == null || !competitionDiscipline.DisciplineId.HasValue ? "" : discipline.Name;
                //ViewBag.Format = n;
            }
            return PartialView("_AthleticsRegistrationList", new AthleticRegListModel
            {
                SectionName = sessionAlias,
                AthleticRegistrations = sportsmen
            });
        }

        public ActionResult BicycleRegistrationList(int? clubId, int? leagueId, int? compExpId, int seasonId)
        {
            var sessionAlias = leagueId.HasValue
                ? secRepo.GetByLeagueId(leagueId.Value).Alias
                : secRepo.GetByClubId(clubId.Value).Alias;
            ViewBag.LeagueId = leagueId;

            List<AthleticRegDto> sportsmen;
            sportsmen = playersRepo.GetBicycleRegistrations(clubId, leagueId, compExpId, seasonId)?.ToList();
            //order sportsmen by smth

            var competitionExpeHeat = disciplinesRepo.GetCompetitionExpertiesHeatByExpId(compExpId);
            ViewBag.ClubId = clubId;
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueId = leagueId;
            ViewBag.ClubName = clubId.HasValue ? clubsRepo.GetById(clubId.Value)?.Name : string.Empty;
            var league = leagueRepo.GetById(leagueId ?? 0);
            ViewBag.CompetitionName = league?.Name;
            ViewBag.CompetitionDate = league?.LeagueStartDate?.ToShortDateString() ?? "";
            ViewBag.CategoryName = "Expertise name";
            ViewBag.DisciplineId = compExpId;

            var title = league?.Name + " / " + ViewBag.CompetitionDate + " / " + league.PlaceOfCompetition;
            if (compExpId.HasValue) title += " / " + competitionExpeHeat.CompetitionExperty.DisciplineExpertise.Name + " - " + competitionExpeHeat.BicycleCompetitionHeat.Name;

            return PartialView("_AthleticsRegistrationList", new AthleticRegListModel
            {
                SectionName = sessionAlias,
                AthleticRegistrations = sportsmen,
                ModalTitle = title
            });
        }


        public ActionResult WeightLiftingRegistrationList(int clubId, int unionId, int seasonId)
        {
            int playersCount = 0;
            var vm = new UnionCompetitionForm
            {
                ClubId = clubId,
                UnionId = unionId,
                SeasonId = seasonId,
                Competitions = leagueRepo.GetByUnion(unionId, seasonId)
                    ?.Where(l => l.EilatTournament == true)
                    .Select(l => new CompetitionDto
                    {
                        LeagueId = l.LeagueId,
                        CompetitionName = l.Name,
                        StartDate = l.LeagueStartDate,
                        EndDate = l.LeagueEndDate,
                        EndRegistrationDate = l.EndRegistrationDate,
                        StartRegistrationDate = l.StartRegistrationDate,
                        Place = l.PlaceOfCompetition,
                        SeasonId = seasonId,
                        IsMaxed = l.MaxRegistrations.HasValue && l.MaxRegistrations <= l.CompetitionRegistrations.Count(),
                        IsEnded = l.EndRegistrationDate.HasValue && l.EndRegistrationDate.Value <= DateTime.Now,
                        IsStarted = !l.StartRegistrationDate.HasValue || l.StartRegistrationDate.Value <= DateTime.Now,
                        CompetitionDisciplines =
                            disciplinesRepo.GetCompetitionDisciplines(l, clubId, seasonId, GamesAlias.WeightLifting),
                        Heats = disciplinesRepo.GetCompetitionSessions(l.LeagueId)
                    })
                    .ToList(),
                ClubSportsmen = playersRepo
                    .GetTeamPlayersByClubId(clubId, seasonId, null, null, null, null, null, null, null, null, GamesAlias.WeightLifting, out playersCount)
                    ?.Where(c => c.IsApproveChecked).ToList(),
                ClubReferees = leagueRepo.GetAllClubRelatedReferees(clubId),
                SelectedRefereesIds = leagueRepo.GetAllRegisteredRefereesIds(clubId, seasonId, unionId),
                SelectedSportsmenIds = leagueRepo.GetAllRegisteredSportsmenIds(clubId, seasonId, unionId),
                SectionAlias = GamesAlias.WeightLifting
            };
            return PartialView("_CompetitionsList", vm);
        }

        public void DeleteDisciplineRegistration(int id)
        {
            playersRepo.DeleteDisciplineRegistration(id);
        }

        public void DeleteBicycleRegistration(int id)
        {
            playersRepo.DeleteBicycleRegistration(id);
        }


        public void ResetDisciplineRegistrationResult(int id)
        {
            playersRepo.ResetDisciplineRegistration(id);
        }




        public ActionResult SportsmenRegistrationList(int? clubId, int leagueId, int seasonId)
        {
            leagueRepo.SetRegistrationsOrders();
            var sportsmenRegistrations = playersRepo.GetSportsmenRegs(clubId, leagueId, seasonId)
                .OrderBy(c => c.Route)
                .ThenBy(c => c.FullName);
            ViewBag.ClubId = clubId;
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueId = leagueId;
            ViewBag.ClubName = clubId.HasValue ? clubsRepo.GetById(clubId.Value)?.Name : string.Empty;
            ViewBag.CompetitionName = leagueRepo.GetById(leagueId)?.Name;
            ViewBag.IsMartialArts = true;
            return PartialView("_PlayersRegistrationList", sportsmenRegistrations);
        }

        private List<RegistrationInstrument> GetInstrumenList(
            IEnumerable<GymnasticDto> playersRegistrationsWithInstruments)
        {
            var instrumentsIds = new List<int>();

            foreach (var reg in playersRegistrationsWithInstruments)
            {
                var ids = reg.RegistrationInstruments?.Select(c => c.InstrumentId).ToList();
                if (ids?.Any() == true)
                {
                    instrumentsIds.AddRange(ids);
                }
            }

            return leagueRepo.GetInstrumentsByIds(instrumentsIds?.Distinct());
        }

        public ActionResult DeleteRegistration(int id, int? clubId, int leagueId, int seasonId,
            int? competitionRouteId = null, int? deleteType = null)
        {
            playersRepo.DeleteRegistration(id);

            if (deleteType == null || deleteType == 3)
            {
                return RedirectToAction(nameof(AllRoutesPlayersRegistrationList),
                    new { clubId, leagueId, seasonId, competitionRouteId });
            }
            else if (deleteType == 1)
            {
                return RedirectToAction(nameof(PlayersRegistrationList),
                    new { clubId, leagueId, seasonId, competitionRouteId });
            }
            else if (deleteType == 2)
            {
                return RedirectToAction(nameof(PlayersTeamRegistrationList),
                    new { clubId, leagueId, seasonId, competitionRouteId });
            }
            return RedirectToAction(nameof(PlayersRegistrationList),
                new { clubId, leagueId, seasonId, competitionRouteId });
        }






        public ActionResult DeleteAdditionalRegistration(int id, int? clubId, int leagueId, int seasonId, bool isTeam,
            int? competitionRouteId = null, int? deleteType = null)
        {
            if (isTeam)
            {
                playersRepo.DeleteAdditionalTeamRegistration(id);
            }
            else
                playersRepo.DeleteAdditionalRegistration(id);

            if (deleteType == null || deleteType == 3)
            {
                return RedirectToAction(nameof(AllRoutesPlayersRegistrationList),
                    new { clubId, leagueId, seasonId, competitionRouteId });
            }
            else if (deleteType == 1)
            {
                return RedirectToAction(nameof(PlayersRegistrationList),
                    new { clubId, leagueId, seasonId, competitionRouteId });
            }
            else if (deleteType == 2)
            {
                return RedirectToAction(nameof(PlayersTeamRegistrationList),
                    new { clubId, leagueId, seasonId, competitionRouteId });
            }
            return RedirectToAction(nameof(PlayersRegistrationList),
                new { clubId, leagueId, seasonId, competitionRouteId });
        }



        public ActionResult DeleteSporsmanRegistration(int id, int? clubId, int leagueId, int seasonId)
        {
            playersRepo.DeleteSportsmanRegistration(id);
            return RedirectToAction(nameof(SportsmenRegistrationList), new { clubId, leagueId, seasonId });
        }


        public ActionResult SearchForCompRegistration(string id, int leagueId, int disciplineId, int seasonId)

        {
            var league = leagueRepo.GetById(leagueId);
            int sectionId = league.Season.Union.SectionId;
            string sectionAlias = league.Season.Union.Section.Alias;
            var descipline = disciplinesRepo.GetCompetitionDisciplineById(disciplineId);
            bool canAddUnAuthorizedAthletes = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager);
            var idResList = usersRepo.SearchUserByIdent(sectionId, seasonId, AppRole.Players, id, 8, !canAddUnAuthorizedAthletes).ToList();
            var passportResList = usersRepo.SearchUserByPassport(sectionId, seasonId, AppRole.Players, id, 8, !canAddUnAuthorizedAthletes);
            var nameResList = usersRepo.SearchUserByFullName(sectionId, seasonId, AppRole.Players, id, 8, !canAddUnAuthorizedAthletes);

            int athleteNum;
            var isParsed = int.TryParse(id, out athleteNum);
            idResList = idResList.Union(passportResList).Union(nameResList).ToList();
            if (isParsed)
            {
                var athleteResList = usersRepo.SearchUserByAthleteNumber(sectionId, seasonId, AppRole.Players, athleteNum, 8, !canAddUnAuthorizedAthletes);
                if (athleteResList != null && athleteResList.Count() > 0)
                {
                    idResList = athleteResList.Union(idResList).ToList();
                }
            }
            var allAllowedPlayersToRegister = disciplinesRepo.GetCompetitionAllDisciplinePlayers(descipline, seasonId, sectionAlias);
            IEnumerable<CompDiscRegDTO> RegisteredPlayers = null;
            if (sectionAlias == SectionAliases.Bicycle)
            {
                RegisteredPlayers = db.BicycleDisciplineRegistrations.Where(x => x.CompetitionExpertiesHeatId == disciplineId).Select(cdr => new CompDiscRegDTO
                {
                    UserId = cdr.UserId.Value
                });
            }
            else
            {
                RegisteredPlayers = descipline.CompetitionDisciplineRegistrations.Select(cdr => new CompDiscRegDTO
                {
                    UserId = cdr.UserId
                });
            }
            var registeredInCompetition = new List<CompetitionDisciplineRegistration>();
            if (sectionAlias == GamesAlias.WeightLifting)
            {
                league.CompetitionDisciplines.ForEach(cd => registeredInCompetition.AddRange(cd.CompetitionDisciplineRegistrations.Where(r => !r.IsArchive).ToList()));
            }


            foreach (var player in idResList)
            {
                if (!player.AuthorizedToRegister)
                {
                    player.Status = 4;
                }
                foreach (var allowed in allAllowedPlayersToRegister)
                {
                    if (allowed.UserId == player.UserId)
                    {
                        player.Status = 1;
                        break;
                    }
                }

                foreach (var registered in RegisteredPlayers)
                {
                    if (registered.UserId == player.UserId)
                    {

                        player.Status = 2;
                        break;
                    }
                }
                foreach (var registered in registeredInCompetition)
                {
                    if (registered.UserId == player.UserId)
                    {

                        player.Status = 3;

                    }
                }
            }
            return Json(idResList, JsonRequestBehavior.AllowGet);
        }




        [HttpPost]
        public void ExportRegistrationsToExcel(int? clubId, int leagueId, int seasonId, int? competitionRouteId = null)
        {
            using (var workbook =
                new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var clubName = clubId.HasValue ? _objClubTrainingsRepo.GetClubName(clubId.Value) : string.Empty;
                var leagueName = leagueRepo.GetById(leagueId)?.Name;


                var playersRegistrationsWithoutInstruments =
                    playersRepo.GetPlayersRegistrationsWithoutInstruments(clubId, leagueId, seasonId,
                        competitionRouteId);
                var playersRegistrationsWithInstruments =
                    playersRepo.GetPlayersRegistrationsWithInstruments(clubId, leagueId, seasonId, competitionRouteId);
                var indvidualPlayers = playersRegistrationsWithoutInstruments.Union(playersRegistrationsWithInstruments)
                    .OrderBy(c => c.Route)
                    .ThenBy(c => c.Rank)
                    .ThenBy(c => c.FullName);

                var teamPlayersRegistrationsWithoutInstruments =
                    playersRepo.GetTeamPlayersRegistrationsWithoutInstruments(clubId, leagueId, seasonId, competitionRouteId);
                var teamPlayersRegistrationsWithInstruments =
                    playersRepo.GetTeamPlayersRegistrationsWithInstruments(clubId, leagueId, seasonId, competitionRouteId);


                var teamPlayersAll = teamPlayersRegistrationsWithoutInstruments.Union(teamPlayersRegistrationsWithInstruments)
                    .OrderBy(c => c.Route)
                    .ThenBy(c => c.Rank)
                    .ThenBy(c => c.FullName);


                var instumentalizedPlayers = playersRegistrationsWithInstruments.Concat(teamPlayersRegistrationsWithInstruments);
                var playersAll = indvidualPlayers.Concat(teamPlayersAll);
                playersAll = playersAll.OrderBy(p => p.FullName);
                var instrumentsList = GetInstrumenList(instumentalizedPlayers);

                var ws = workbook.AddWorksheet($"{Messages.RegistrationList}");

                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                if (competitionRouteId.HasValue || !competitionRouteId.HasValue && !clubId.HasValue)
                {
                    addCell(Messages.ClubName);
                }

                addCell(Messages.FirstName);
                addCell(Messages.LastName);
                addCell(Messages.IdentNum);
                addCell(Messages.BirthDay);
                addCell(Messages.IsReligious);
                addCell(Messages.Route);
                addCell(Messages.PlayerInfoRank);
                if (instrumentsList != null)
                {
                    foreach (var instrument in instrumentsList)
                    {
                        addCell(Messages.Instrument + " " + instrument.Name);
                        addCell(Messages.Order);
                    }
                }

                addCell($"{Messages.Composition}");
                addCell($"{Messages.Composition} #2");
                addCell($"{Messages.Composition} #3");
                addCell($"{Messages.Composition} #4");
                addCell($"{Messages.Composition} #5");
                addCell($"{Messages.Composition} #6");
                addCell($"{Messages.Composition} #7");
                addCell($"{Messages.Composition} #8");
                addCell($"{Messages.Composition} #9");
                addCell($"{Messages.Composition} #10");
                addCell($"{Messages.CompostionAmount}");
                addCell($"{Messages.Type} {Messages.Route}");
                addCell($"{Messages.Reserved}");
                addCell(Messages.FinalScore);
                addCell(Messages.Rank);


                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();
                playersAll = playersAll.OrderBy(g => g.IsTeam).ThenBy(u => u.ClubName).ThenBy(u => u.Route).ThenBy(u => u.Rank).ThenBy(u => u.CompositionNumber).ThenBy(u => u.InstrumentName);
                foreach (var row in playersAll)
                {
                    if (competitionRouteId.HasValue || !competitionRouteId.HasValue && !clubId.HasValue)
                    {
                        addCell(row.ClubName);
                    }
                    string[] ssize = row.FullName.Split(new char[0]);
                    if (ssize.Length > 1)
                    {
                        addCell(ssize[0]);
                        string lastName = "";
                        for (int i = 1; i < ssize.Length; i++)
                        {
                            if (i > 1)
                            {
                                lastName = lastName + " ";
                            }
                            lastName = lastName + ssize[i];
                        }
                        addCell(lastName);
                    }
                    else
                    {
                        addCell("");
                        addCell("");
                    }
                    //addCell(row.FullName);
                    addCell(row.IdentNum);
                    addCell(row.BirthDate.HasValue ? row.BirthDate.Value.ToShortDateString() : string.Empty);
                    addCell(row.IsReligious == true ? Messages.Yes : Messages.No);
                    addCell(row.Route);
                    addCell(row.Rank);
                    if (instrumentsList != null)
                    {
                        if (row.RegistrationInstruments != null && row.RegistrationInstruments.Any())
                        {
                            var neededCountOfTd = instrumentsList.Count - row.RegistrationInstruments.Count;

                            foreach (var instrument in instrumentsList)
                            {
                                var instrumentGym =
                                    row.RegistrationInstruments.FirstOrDefault(x =>
                                        x.InstrumentId == instrument.InstrumentId);
                                if (instrumentGym != null)
                                {
                                    addCell(Messages.Yes);
                                    addCell(instrumentGym.OrderNumber?.ToString());
                                }
                                else
                                {
                                    addCell(string.Empty);
                                    addCell(string.Empty);
                                }
                            }
                        }
                    }

                    addCell(row.CompositionNumber == 0 && row.Composition.HasValue ? "v" : string.Empty);
                    addCell(row.CompositionNumber == 1 && row.SecondComposition.HasValue ? "v" : string.Empty);
                    addCell(row.CompositionNumber == 2 && row.ThirdComposition.HasValue ? "v" : string.Empty);
                    addCell(row.CompositionNumber == 3 && row.FourthComposition.HasValue ? "v" : string.Empty);
                    addCell(row.CompositionNumber == 4 && row.FifthComposition.HasValue ? "v" : string.Empty);
                    addCell(row.CompositionNumber == 5 && row.SixthComposition.HasValue ? "v" : string.Empty);
                    addCell(row.CompositionNumber == 6 && row.SeventhComposition.HasValue ? "v" : string.Empty);
                    addCell(row.CompositionNumber == 7 && row.EighthComposition.HasValue ? "v" : string.Empty);
                    addCell(row.CompositionNumber == 8 && row.NinthComposition.HasValue ? "v" : string.Empty);
                    addCell(row.CompositionNumber == 9 && row.TenthComposition.HasValue ? "v" : string.Empty);
                    addCell(row.Composition.HasValue ? row.Composition.Value.ToString() : string.Empty);
                    addCell(row.IsTeam ? Messages.Teams : Messages.Individual);
                    addCell(row.IsAdditional ? "v" : string.Empty);
                    addCell(row.FinalScore.HasValue ? row.FinalScore.ToString() : string.Empty);
                    addCell(row.Position.HasValue ? row.Position.ToString() : string.Empty);

                    rowCounter++;
                    columnCounter = 1;
                }

                ws.Columns().AdjustToContents();

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                if (clubId.HasValue)
                {
                    Response.AddHeader("content-disposition",
                        $"attachment;filename= {clubName.Replace(' ', '_')}_{leagueName.Replace(' ', '_')}_{Messages.RegistrationCaption}.xlsx");
                }
                else
                {
                    Response.AddHeader("content-disposition",
                        $"attachment;filename={leagueName.Replace(' ', '_')}_{Messages.RegistrationCaption}.xlsx");
                }

                using (var myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }


        public void GenerateEntryList(int? clubId, int? disciplineId, int? leagueId, int seasonId)
        {
            var sessionAlias = leagueId.HasValue ? secRepo.GetByLeagueId(leagueId.Value).Alias : secRepo.GetByClubId(clubId.Value).Alias;
            var clubName = clubId.HasValue ? _objClubTrainingsRepo.GetClubName(clubId.Value) : string.Empty;
            var league = leagueRepo.GetById(leagueId ?? 0);
            var leagueName = league?.Name;
            var contentPath = Server.MapPath(GlobVars.ContentPath);
            var firstFilenamePart = "";
            Club club = null;
            if (clubId.HasValue)
            {
                club = clubsRepo.GetById(clubId.Value);
            }

            EntryListPdfHelper entryListPdfHelper = null;
            if (sessionAlias != SectionAliases.Bicycle)
            {
                var discipline = disciplinesRepo.GetCompetitionDisciplineById(disciplineId ?? 0);
                var disciplineName = discipline?.DisciplineId != null ? disciplinesRepo.GetById(discipline.DisciplineId.Value).Name : "";
                var categoryName = discipline?.CompetitionAge?.age_name;

                firstFilenamePart = disciplineName.Replace(".", "").Replace(",", "");
                if (sessionAlias == SectionAliases.Climbing) firstFilenamePart = categoryName?.Replace(".", "").Replace(",", "");

                if (club != null)
                {
                    firstFilenamePart = firstFilenamePart + "_" + club.Name;
                    List<AthleticRegDto> sportsmen = playersRepo.GetAthleticsRegistrations(clubId, leagueId, disciplineId, seasonId)?.OrderBy(s => s.LastName).ToList();
                    entryListPdfHelper = new EntryListPdfHelper(sportsmen, club, league, disciplineName, IsHebrew, contentPath, sessionAlias);
                }
                else
                {

                    var discCatName = sessionAlias == SectionAliases.Climbing ? categoryName : $"{disciplineName} - {categoryName}";
                    List<AthleticRegDto> sportsmen = playersRepo.GetAthleticsRegistrations(clubId, leagueId, disciplineId, seasonId)?.OrderBy(s => s.LastName).ToList();
                    entryListPdfHelper = new EntryListPdfHelper(sportsmen, league, discCatName, IsHebrew, contentPath, sessionAlias);

                }
            }
            else
            {
                var expCompHeat = disciplinesRepo.GetCompetitionExpertiesHeatByExpId(disciplineId ?? 0);
                var disciplineName = expCompHeat?.CompetitionExperty.DisciplineExpertise.Name ?? "";
                var categoryName = expCompHeat?.BicycleCompetitionHeat.Name;
                firstFilenamePart = disciplineName.Replace(".", "").Replace(",", "");

                if (club != null)
                {
                    firstFilenamePart = !string.IsNullOrEmpty(firstFilenamePart) ? (firstFilenamePart + "_" + club.Name) : (club.Name);
                    List<AthleticRegDto> sportsmen = playersRepo.GetBicycleRegistrations(clubId, leagueId, disciplineId, seasonId)?.OrderBy(s => s.LastName).ToList();
                    entryListPdfHelper = new EntryListPdfHelper(sportsmen, club, league, $"{disciplineName} - {categoryName}", IsHebrew, contentPath, sessionAlias);
                }
                else
                {
                    List<AthleticRegDto> sportsmen = playersRepo.GetBicycleRegistrations(clubId, leagueId, disciplineId, seasonId)?.OrderBy(s => s.LastName).ToList();
                    entryListPdfHelper = new EntryListPdfHelper(sportsmen, league, $"{disciplineName} - {categoryName}", IsHebrew, contentPath, sessionAlias);
                }
            }


            var exportResult = entryListPdfHelper.GetDocumentStream();

            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename={firstFilenamePart}_{Messages.EntryList}_{DateTime.Now.ToShortDateString()}.pdf");

            Response.OutputStream.Write(exportResult.GetBuffer(), 0, exportResult.GetBuffer().Length);
            Response.Flush();
            Response.End();
        }

        [HttpPost]
        public void ExportDisciplineRegistrationsToExcel(int? clubId, int? disciplineId, int? leagueId, int seasonId)
        {
            var sessionAlias = leagueId.HasValue
                ? secRepo.GetByLeagueId(leagueId.Value).Alias
                : secRepo.GetByClubId(clubId.Value).Alias;
            IOrderedEnumerable<AthleticRegDto> sportsmen;
            if (sessionAlias != SectionAliases.Bicycle)
                sportsmen = playersRepo.GetAthleticsRegistrations(clubId, leagueId, disciplineId, seasonId)?.OrderBy(t => t.DisciplineName);
            else
                sportsmen = playersRepo.GetBicycleRegistrations(clubId, leagueId, disciplineId, seasonId)?.OrderBy(t => t.CategoryName);
            var clubName = clubId.HasValue ? _objClubTrainingsRepo.GetClubName(clubId.Value) : string.Empty;
            var leagueName = leagueRepo.GetById(leagueId ?? 0)?.Name;
            var discipline = disciplinesRepo.GetCompetitionDisciplineById(disciplineId ?? 0);
            var disciplineName = discipline?.DisciplineId != null
                ? disciplinesRepo.GetById(discipline.DisciplineId.Value).Name
                : "";
            var categoryName = discipline?.CompetitionAge?.age_name;
            var rowingDistance = discipline?.RowingDistance?.Name;
            if (sessionAlias == GamesAlias.Athletics)
            {
                sportsmen = sportsmen.OrderBy(s => s.LastName).ThenBy(s => s.FirstName);
            }
            else
            {
                if (sessionAlias == GamesAlias.Rowing)
                {
                    sportsmen = sportsmen.OrderBy(x => x.DisciplineName).ThenBy(x => x.CategoryName).ThenBy(x => Convert.ToInt32(x.RowingDistance)).ThenBy(x => x.TeamNumber);
                }
                else
                {
                    sportsmen = sportsmen.OrderBy(s => s.FullName);
                }
            }

            using (var workbook =
                new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var ws = workbook.AddWorksheet($"{Messages.RegistrationList}");
                var systemList = workbook.AddWorksheet("ValidationData");
                systemList.Cell(1, 1).Value = Messages.H_or_F;
                systemList.Cell(1, 2).Value = Messages.H_or_F_H;
                systemList.Cell(1, 3).Value = Messages.H_or_F_F;
                systemList.Hide();

                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Func<string, int>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    return columnCounter++; // not a typo: return previous one, then increase
                });
                var addCellFormat = new Func<string, XLDataType, int>((value, type) =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    ws.Cell(rowCounter, columnCounter).DataType = type;

                    //the code below causes the export to largely fail due to type cast errors
                    //switch (type)
                    //{
                    //	case XLDataType.DateTime:
                    //		ws.Cell(rowCounter, columnCounter).Style.DateFormat.Format = Messages.DateFormat;
                    //		break;
                    //	case XLDataType.TimeSpan:
                    //		ws.Cell(rowCounter, columnCounter).Style.DateFormat.Format = "HH:mm:ss.ff";
                    //		break;
                    //	case XLDataType.Number:
                    //		ws.Cell(rowCounter, columnCounter).Style.NumberFormat.Format = "#";
                    //		break;
                    //	default:
                    //		break;
                    //}
                    return columnCounter++; // not a typo: return previous one, then increase
                });

                if (sessionAlias == GamesAlias.WeightLifting)
                {
                    if (!clubId.HasValue)
                    {
                        addCell(Messages.ClubName);
                    }
                    addCell(Messages.Category);
                }
                else
                {
                    /*
                    if(sessionAlias == GamesAlias.Rowing && !clubId.HasValue)
                    {
                        addCell(Messages.ClubName);
                    }
                    else
                    */
                    if (disciplineId.HasValue && sessionAlias != GamesAlias.Athletics)
                    {
                        if (sessionAlias == GamesAlias.Rowing)
                        {
                            addCell(Messages.StartDate);
                            addCell(Messages.NumberSign);
                            addCell(Messages.H_or_F);
                            addCell(Messages.Leap);
                            addCell(Messages.LeapTime);
                            addCell(Messages.ClubName);
                            addCell(Messages.RelatedClubName);
                        }
                        else if (sessionAlias != GamesAlias.Bicycle)
                        {
                            addCell(Messages.ClubName);
                        }
                    }
                    else
                    {
                        if (sessionAlias == GamesAlias.Rowing)
                        {
                            addCell(Messages.ClubName);
                            addCell(Messages.RelatedClubName);
                        }
                        if (sessionAlias != GamesAlias.Bicycle && sessionAlias != GamesAlias.Climbing)
                        {
                            if (sessionAlias == GamesAlias.Rowing) addCell(Messages.Distance);
                            addCell(sessionAlias == GamesAlias.Rowing ? Messages.Boat : Messages.Discipline);
                            addCell(Messages.Category);
                        }
                        if (sessionAlias == GamesAlias.Climbing)
                        {
                            addCell(Messages.ClubName);
                        }
                    }
                }
                if (sessionAlias == GamesAlias.Rowing || sessionAlias == GamesAlias.Bicycle)
                {
                    addCellFormat(Messages.IdentNum, XLDataType.Text);
                }
                if (sessionAlias == GamesAlias.Athletics)
                {
                    addCell(Messages.AthleteNumbers);
                }
                if (sessionAlias == GamesAlias.Athletics || sessionAlias == GamesAlias.Rowing)
                {
                    addCell(Messages.LastName);
                    addCell(Messages.FirstName);
                }
                else
                {
                    addCell(Messages.FullName);
                }
                if (sessionAlias == GamesAlias.Athletics && !clubId.HasValue)
                {
                    addCell(Messages.ClubName);
                }

                if (sessionAlias != GamesAlias.Rowing) addCell($"{Messages.IdentNum}/{Messages.PassportNum}");
                addCell(Messages.BirthDay);
                if (sessionAlias == GamesAlias.Bicycle)
                {
                    addCell(Messages.ClubName);
                    addCell(Messages.Expertise);
                    addCell(Messages.Heat);
                }
                if (sessionAlias == GamesAlias.Climbing)
                {
                    addCell(Messages.Category);
                    addCell(Messages.Heat);
                }
                if (sessionAlias == GamesAlias.Rowing)
                {
                    addCell(Messages.Team);
                    addCell(Messages.Coxwain);
                }
                if (disciplineId.HasValue)
                {
                    if (sessionAlias == GamesAlias.Rowing)
                    { 
                    addCell(Messages.StartTimeLeap);
                    addCell(Messages.FinishTime);
                    addCell(Messages.Result);
                    addCell(Messages.Rank);
                    }
                }
                else
                { 
                if (sessionAlias == GamesAlias.Rowing)
                {
                    addCell(Messages.Result);
                    addCell(Messages.Rank);
                }
                }
                if (sessionAlias == GamesAlias.WeightLifting)
                {
                    addCell(Messages.WeightDeclaration);
                }
                else
                {
                    if (sessionAlias != GamesAlias.Rowing && sessionAlias != GamesAlias.Bicycle)
                        addCell(Messages.BestResult);
                }
                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                foreach (var row in sportsmen)
                {
                    if (sessionAlias == GamesAlias.WeightLifting)
                    {
                        if (!clubId.HasValue)
                        {
                            addCell(row.ClubName);
                        }
                        addCell(row.CategoryName);
                    }
                    else
                    {
                        if (disciplineId.HasValue && sessionAlias != GamesAlias.Athletics)
                        {
                            if (sessionAlias == GamesAlias.Rowing)
                            {
                                addCellFormat(row.CompetitionStartDate.ToString(), XLDataType.DateTime);
                                addCellFormat(null, XLDataType.Number);
                                addCellFormat(null, XLDataType.TimeSpan);
                                //ws.Column(hfnum).SetDataValidation().List(systemList.Range("$B$1:$C$1"), true);
                                addCellFormat(null, XLDataType.Number);
                                addCellFormat(null, XLDataType.TimeSpan);
                                addCell(row.ClubName);
                                addCell(row.RelatedClubName);
                            }
                            else if (sessionAlias != GamesAlias.Bicycle)
                            {
                                addCell(row.ClubName);
                            }
                        }
                        else
                        {
                            if (sessionAlias == GamesAlias.Rowing)
                            {
                                addCell(row.ClubName);
                                addCell(row.RelatedClubName);
                            }
                            if (sessionAlias != GamesAlias.Bicycle && sessionAlias != GamesAlias.Climbing)
                            {
                                if (sessionAlias == GamesAlias.Rowing) addCell(row.RowingDistance);
                                addCell(row.DisciplineName);
                                addCell(row.CategoryName);
                            }
                            if (sessionAlias == GamesAlias.Climbing)
                            {
                                addCell(row.ClubName);
                            }
                        }
                    }
                    if (sessionAlias == GamesAlias.Rowing || sessionAlias == GamesAlias.Bicycle)
                    {
                        addCellFormat("'" + row.IdentNum, XLDataType.Text);
                    }
                    if (sessionAlias == GamesAlias.Athletics)
                    {
                        addCell(row.AthleteNumber.ToString());
                    }
                    if (sessionAlias == GamesAlias.Athletics || sessionAlias == GamesAlias.Rowing)
                    {
                        addCell(row.LastName);
                        addCell(row.FirstName);
                    }
                    else
                    {
                        addCell(row.FullName);
                    }
                    if (sessionAlias == GamesAlias.Athletics && !clubId.HasValue)
                    {
                        addCell(row.ClubName);
                    }
                    if (sessionAlias != GamesAlias.Rowing) addCell(!string.IsNullOrWhiteSpace(row.IdentNum) ? row.IdentNum : row.PassportNum);
                    addCell(row.BirthDay?.ToShortDateString());
                    if (sessionAlias == GamesAlias.Bicycle)
                    {
                        addCell(row.ClubName);
                        addCell(row.CategoryName);
                        addCell(row.Heat);
                    }
                    if (sessionAlias == GamesAlias.Climbing)
                    {
                        addCell(row.CategoryName);
                        addCell(row.Heat);
                    }
                    if (sessionAlias == GamesAlias.Rowing)
                    {
                        addCell(Messages.Team + " " + row.TeamNumber);
                        if (row.CoxwainFullName == row.FullName)
                            addCell("V");
                        else
                            addCell("");
                    }
                    if (disciplineId.HasValue && sessionAlias == GamesAlias.Rowing)
                    {
                        addCellFormat(null, XLDataType.TimeSpan);
                        addCellFormat(null, XLDataType.TimeSpan);
                        var diffcell = addCell(null);
                        ws.Cell(rowCounter, diffcell).FormulaR1C1 = "=RC[-1]-RC[-2]";
                        addCellFormat(null, XLDataType.Number);
                    }
                    if (sessionAlias == GamesAlias.Rowing)
                    {
                        addCellFormat(null, XLDataType.Number);
                        addCellFormat(null, XLDataType.Number);
                    }
                    if (sessionAlias == GamesAlias.WeightLifting)
                    {
                        addCell(row.WeightDeclaration.ToString());
                    }
                    else if (sessionAlias != GamesAlias.Bicycle)
                    {
                        addCell(row.BestResult?.ToString());
                    }


                    rowCounter++;
                    columnCounter = 1;
                }

                ws.Columns().AdjustToContents();

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                if (sessionAlias == SectionAliases.Rowing)
                {
                    if (!disciplineId.HasValue)
                    {
                        Response.AddHeader("content-disposition",
                            $"attachment;filename={leagueName.Replace(' ', '_').Replace(",", "")}_{Messages.SportmansWithApostrophe.ToLower()}_{Messages.RegistrationStatus.ToLower().Replace(' ', '_')}.xlsx");
                    }
                    else
                    {
                        Response.AddHeader("content-disposition",
                                    $"attachment;filename={leagueName.Replace(' ', '_').Replace(",", "")}_{categoryName.Replace(' ', '_').Replace(",", "")}_{disciplineName.Replace(' ', '_').Replace(",", "")}_{rowingDistance.Replace(' ', '_').Replace(",", "")}_{Messages.SportmansWithApostrophe.ToLower()}_{Messages.RegistrationStatus.ToLower().Replace(' ', '_')}.xlsx");

                    }
                }
                else if (sessionAlias == SectionAliases.Bicycle)
                {

                    if (!disciplineId.HasValue)
                    {
                        Response.AddHeader("content-disposition",
                            $"attachment;filename={leagueName.Replace(' ', '_').Replace(",", "")}_{Messages.SportmansWithApostrophe.ToLower()}_{Messages.RegistrationStatus.ToLower().Replace(' ', '_')}.xlsx");
                    }
                    else
                    {
                        var exp = disciplinesRepo.GetCompetitionExpertiesHeatByExpId(disciplineId);
                        var catName = exp.CompetitionExperty.DisciplineExpertise.Name;
                        var discName = exp.BicycleCompetitionHeat.Name;
                        Response.AddHeader("content-disposition",
                                    $"attachment;filename={leagueName.Replace(' ', '_').Replace(",", "")}_{catName.Replace(' ', '_').Replace(",", "")}_{discName.Replace(' ', '_').Replace(",", "")}_{Messages.SportmansWithApostrophe.ToLower()}_{Messages.RegistrationStatus.ToLower().Replace(' ', '_')}.xlsx");

                    }
                }
                else
                if (sessionAlias == SectionAliases.Climbing)
                {
                    if (clubId.HasValue)
                    {
                        Response.AddHeader("content-disposition",
                            $"attachment;filename= {clubName.Replace(' ', '_').Replace(",", "")}_{leagueName.Replace(' ', '_').Replace(",", "")}_{Messages.RegistrationStatus.ToLower().Replace(' ', '_')}.xlsx");
                    }
                    else
                    {
                        Response.AddHeader("content-disposition",
                                   $"attachment;filename={leagueName.Replace(' ', '_').Replace(",", "")}_{categoryName.Replace(' ', '_').Replace(",", "")}_{Messages.RegistrationStatus.ToLower().Replace(' ', '_')}.xlsx");

                    }

                }
                else
                {
                    if (clubId.HasValue)
                    {
                        Response.AddHeader("content-disposition",
                            $"attachment;filename= {clubName.Replace(' ', '_').Replace(",", "")}_{leagueName.Replace(' ', '_').Replace(",", "")}_{Messages.AthleticsCaption}.xlsx");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(disciplineName))
                        {
                            Response.AddHeader("content-disposition",
                                $"attachment;filename={disciplineName.Replace(' ', '_').Replace(",", "")}_{leagueName.Replace(' ', '_').Replace(",", "")}_{categoryName.Replace(' ', '_').Replace(",", "")}_{Messages.AthleticsCaption}.xlsx");
                        }
                        else
                            Response.AddHeader("content-disposition",
                                $"attachment;filename={leagueName.Replace(' ', '_').Replace(",", "")}_{Messages.AthleticsCaption}.xlsx");
                    }
                }
                using (var myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        public ActionResult UnionLeagues(int clubId, int seasonId)
        {
            var unionId = 38;
            var union38Leagues = leagueRepo.GetAllUnionLeagues(unionId, seasonId, clubId);

            ViewBag.ClubId = clubId;
            ViewBag.SeasonId = seasonId;
            ViewBag.ClubTeamsList = GetClubTeamsList(clubId, seasonId);
            ViewBag.LeagueSelectedTeamIds = GetSelectedTeamIds(clubId, seasonId);

            return PartialView("_UnionLeagues", union38Leagues);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clubId"></param>
        /// <param name="seasonId"></param>
        /// <returns>Key - leagueId, Value - selected teams of club in league</returns>
        private Dictionary<int, IEnumerable<int>> GetSelectedTeamIds(int clubId, int seasonId)
        {
            var result = new Dictionary<int, IEnumerable<int>>();
            var clubsLeaguesRegistrations = clubsRepo.GetAllTennisRegistrations(clubId, seasonId);

            if (clubsLeaguesRegistrations != null && clubsLeaguesRegistrations.Any())
            {
                var leagueIds = clubsLeaguesRegistrations.Select(x => x.LeagueId).Distinct();
                foreach (var leagueId in leagueIds)
                {
                    result.Add(leagueId,
                        clubsLeaguesRegistrations.Where(x => x.LeagueId == leagueId)?.Select(x => x.TeamId));
                }
            }

            return result;
        }

        private IEnumerable<TeamShortDTO> GetClubTeamsList(int clubId, int seasonId)
        {
            var clubTeams = clubsRepo.GetById(clubId)?.ClubTeams
                .Where(c => c.SeasonId == seasonId && !c.IsTrainingTeam);
            foreach (var clubTeam in clubTeams)
            {
                var isValid = clubTeam.Team.TeamRegistrations.Any(r => r.SeasonId == seasonId) || true; //CheckValidityOfTheTennisTeam(clubTeam);
                if (isValid)
                {
                    yield return new TeamShortDTO
                    {
                        TeamId = clubTeam.TeamId,
                        Title = clubTeam.Team.TeamsDetails?.Where(c => c.SeasonId == seasonId)
                                    .OrderByDescending(c => c.Id)?.FirstOrDefault()?.TeamName ?? clubTeam.Team.Title,
                    };
                }
            }
        }

        private bool CheckValidityOfTheTennisTeam(ClubTeam clubTeam)
        {
            var isValid = true;
            var team = clubTeam.Team;
            if ((team.IsClubInsurance == null && team.IsUnionInsurance == null) ||
                (team.IsClubInsurance == false && team.IsUnionInsurance == false)) isValid = false;
            if (team.IsClubInsurance == true && string.IsNullOrEmpty(clubTeam.Club.ClubInsurance)) isValid = false;
            return isValid;
        }

        [HttpPost]
        public void RegisterTennisTeam(IEnumerable<int> teamIds, int clubId, int leagueId, int? seasonId)
        {
            clubsRepo.RegisterTennisTeam(teamIds, clubId, leagueId, seasonId);
        }

        [HttpGet]
        public ActionResult TennisPlayersRegistrationList(int? clubId, int? seasonId, string teamIdsString,
            int? leagueId)
        {
            var result = new Dictionary<Team, IEnumerable<TeamsPlayer>>();
            var league = leagueRepo.GetById(leagueId ?? 0);
            var teamIds = teamIdsString?.Split(',')?.Select(int.Parse)?.ToList() ?? new List<int>();
            if (teamIds != null && teamIds.Any())
            {
                foreach (var teamId in teamIds)
                {
                    var team = teamRepo.GetById(teamId, seasonId);
                    var teamPlayers = playersRepo
                        .GetPlayersForTennisRegistrations(teamId, seasonId, league.EndRegistrationDate)?
                        .OrderBy(r => r.TennisPositionOrder ?? int.MaxValue).ThenBy(r => r.User.FullName);
                    result.Add(team, teamPlayers);
                }

                if (result != null && result.Any())
                {
                    foreach (var item in result)
                    {
                        playersRepo.CheckLockStatus(item.Value, leagueId);
                    }
                }
            }


            ViewBag.ClubName = clubId.HasValue ? clubsRepo.GetById(clubId.Value)?.Name : string.Empty;
            ViewBag.LeagueName = leagueId.HasValue ? leagueRepo.GetById(leagueId.Value)?.Name : string.Empty;
            ViewBag.ClubId = clubId;
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueId = leagueId;
            ViewBag.TeamsIds = teamIdsString;
            ViewBag.IsPlayersRegistrationEnded = league.EndRegistrationDate.HasValue && league.EndRegistrationDate.Value < DateTime.Now;

            return PartialView("_TennisRegistrationList", result);
        }

        [HttpPost]
        public void ChangeTennisPlayersOrder(List<int> ids)
        {
            var playersList = new List<PlayerOrder>();
            for (var i = 0; i < ids.Count(); i++)
            {
                var teamDto = new PlayerOrder
                {
                    RegistrationId = ids[i],
                    PositionOrder = i + 1
                };
                playersList.Add(teamDto);
            }

            playersRepo.UpdateTennisPlayersOrder(playersList);
            playersRepo.Save();
        }

        [HttpPost]
        public ActionResult DeleteTennisRegistration(int teamPlayerId, int? clubId, int? seasonId, int? leagueId,
            string teamIdsString)
        {
            playersRepo.DeleteTennisRegistration(teamPlayerId);
            return RedirectToAction(nameof(TennisPlayersRegistrationList),
                new { clubId, seasonId, leagueId, teamIdsString });
        }

        [HttpPost]
        public void ExportTennisRegistrationsToExcel(int? clubId, int leagueId, string teamIdsString, int seasonId)
        {
            var teamPlayers = new List<TeamsPlayer>();
            var allPlayersRegistrations = Enumerable.Empty<TennisPlayerRegistrationDto>();
            var teamIds = !string.IsNullOrEmpty(teamIdsString)
                ? teamIdsString.Split(',').Select(int.Parse).ToList()
                : new List<int>();
            var league = leagueRepo.GetById(leagueId);

            using (var workbook =
                new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var clubName = clubId.HasValue ? _objClubTrainingsRepo.GetClubName(clubId.Value) : string.Empty;
                if (teamIds != null && teamIds.Any() && clubId.HasValue)
                {
                    foreach (var teamId in teamIds)
                    {
                        teamPlayers.AddRange(
                            playersRepo.GetPlayersForTennisRegistrations(teamId, seasonId, league.EndRegistrationDate));
                    }

                    if (teamPlayers != null && teamPlayers.Any())
                    {
                        playersRepo.CheckLockStatus(teamPlayers, leagueId);
                    }
                }

                else
                {
                    allPlayersRegistrations = !teamIds.Any()
                        ? leagueRepo.GetAllTennisRegistrations(leagueId, seasonId, false).OrderBy(c => c.TeamName)
                            .ThenBy(c => c.FullName)
                        : leagueRepo.GetAllTennisRegistrations(leagueId, seasonId, false).Where(c => c.TeamId == teamIds[0])
                            .OrderBy(c => c.TeamName).ThenBy(c => c.FullName);
                }

                var ws = workbook.AddWorksheet($"{Messages.RegistrationList}");

                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });
                if (!clubId.HasValue)
                {
                    addCell(Messages.ClubName);
                    addCell(Messages.TeamName);
                }

                addCell(Messages.FullName);
                addCell(Messages.IdentNum);
                addCell(Messages.BirthDay);
                addCell(Messages.TenicardValidity);
                addCell(Messages.InsuranceDate);
                addCell(Messages.ValidityOfMedicalExamination);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();
                if (clubId.HasValue)
                {
                    foreach (var row in teamPlayers)
                    {
                        addCell(row.User.FullName);
                        addCell(row.User.IdentNum);
                        addCell(row.User.BirthDay?.ToShortDateString() ?? string.Empty);
                        addCell(row.User.TenicardValidity?.ToShortDateString() ?? string.Empty);
                        addCell(row.User.DateOfInsurance?.ToShortDateString());
                        addCell(row.User.MedExamDate?.ToShortDateString());

                        rowCounter++;
                        columnCounter = 1;
                    }
                }
                else
                {
                    foreach (var row in allPlayersRegistrations)
                    {
                        addCell(row.ClubName);
                        addCell(row.TeamName);
                        addCell(row.FullName);
                        addCell(row.IdentNum);
                        addCell(row.BirthDay?.ToShortDateString() ?? string.Empty);
                        addCell(row.TennicardValidity?.ToShortDateString() ?? string.Empty);
                        addCell(row.InsuranceValidity?.ToShortDateString());
                        addCell(row.MedicalValidity?.ToShortDateString());

                        rowCounter++;
                        columnCounter = 1;
                    }
                }

                ws.Columns().AdjustToContents();

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                if (league != null && teamIds.Count != 1)
                {
                    Response.AddHeader("content-disposition", $"attachment;filename= {league.Name.Replace(" ", "_")}_" +
                                                              $"{Messages.TennisRegistration.ToLower().Replace(' ', '_')}.xlsx");
                }
                else if (teamIds.Count == 1)
                {
                    var team = teamRepo.GetById(teamIds[0], seasonId);
                    var teamName = team.TeamsDetails?.OrderByDescending(r => r.Id)?.FirstOrDefault()?.TeamName
                                   ?? team?.Title;
                    Response.AddHeader("content-disposition", $"attachment;filename= {teamName.Replace(" ", "_")}_" +
                                                              $"{Messages.TennisRegistration.ToLower().Replace(' ', '_')}.xlsx");
                }

                using (var myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        public ActionResult TrainingTeam(int clubId, int seasonId)
        {
            var trainingTeam = clubsRepo.CheckForTrainingTeam(clubId, seasonId);
            return PartialView("_TrainingTeam", trainingTeam);
        }

        [HttpPost]
        public ActionResult PlayersOrder(int clubId, int competitionId, int seasonId, int? competitionRouteId,
            int? instrumentId)
        {
            var registeredPlayers = playersRepo.GetPlayersRegistrationsByRoute(clubId, competitionId, seasonId,
                    competitionRouteId ?? 0, instrumentId)
                .OrderBy(c => c.PositionOrder).AsEnumerable();
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueId = competitionId;
            ViewBag.ClubId = clubId;
            ViewBag.CompetitionRouteId = competitionRouteId;
            ViewBag.SectionAlias = clubsRepo.GetSectionByClubId(clubId);
            return PartialView("_PlayersOrder", registeredPlayers);
        }

        [HttpPost]
        public void ChangePlayersOrder(int clubId, int leagueId, int seasonId, List<int> ids, int competitionRouteId)
        {
            var playersList = new List<GymnasticShortDto>();
            for (var i = 0; i < ids.Count(); i++)
            {
                var teamDto = new GymnasticShortDto
                {
                    RegistrationId = ids[i],
                    PositionOrder = i + 1
                };
                playersList.Add(teamDto);
            }

            playersRepo.UpdatePlayersOrder(clubId, leagueId, seasonId, competitionRouteId, playersList);
        }

        public void DownloadGymnasticsExcel()
        {
            using (var workbook =
                new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var ws = workbook.AddWorksheet($"{Messages.Registrations}");

                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                addCell($"*{Messages.ClubNumber}");
                addCell(Messages.ClubName);
                addCell($"{Messages.FirstName}");
                addCell($"{Messages.LastName}");
                addCell($"{Messages.FullName}");
                addCell($"*{Messages.Id}/{Messages.PassportNum}");
                addCell($"{Messages.BirthDay}");
                //addCell($"{Messages.Route}");
                //addCell($"{Messages.PlayerInfoRank}");
                addCell($"{Messages.Composition} ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})");
                addCell($"{Messages.Composition} 2 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})");
                addCell($"{Messages.Composition} 3 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})");
                addCell($"{Messages.Composition} 4 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})");
                addCell($"{Messages.Composition} 5 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})");
                addCell($"{Messages.Composition} 6 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})");
                addCell($"{Messages.Composition} 7 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})");
                addCell($"{Messages.Composition} 8 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})");
                addCell($"{Messages.Composition} 9 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})");
                addCell($"{Messages.Composition} 10 ({Messages.Yes.ToLower()}/{Messages.No.ToLower()})");
                addCell($"{Messages.Instrument} #1");
                addCell($"{Messages.Order} #1");
                addCell($"{Messages.Instrument} #2");
                addCell($"{Messages.Order} #2");
                addCell($"{Messages.Instrument} #3");
                addCell($"{Messages.Order} #3");
                addCell($"{Messages.Instrument} #4");
                addCell($"{Messages.Order} #4");
                addCell($"{Messages.Instrument} #5");
                addCell($"{Messages.Order} #5");
                addCell(Messages.FinalScore);
                addCell(Messages.Rank);


                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();


                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", $"attachment;filename=gymnastics_form.xlsx");


                using (var myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }


        #region Import
        #region ImportGymnasticsRegistrations
        [HttpGet]
        public ActionResult ImportGymnasticsRegistrations(int leagueId, int seasonId)
        {
            var model = new ImportSportsmanViewModel
            {
                LeagueId = leagueId,
                SeasonId = seasonId,
            };

            return PartialView("_ImportGymnastics", model);
        }
        [HttpPost]
        public ActionResult ImportGymnasticsRegistrations(ImportSportsmanViewModel model)
        {
            try
            {
                if (model.ImportFile != null)
                {
                    var importHelper = new ImportExportPlayersHelper(db);
                    List<ImportGymnasticRegistrationModel> correctRows = null;
                    List<ImportGymnasticRegistrationModel> validationErrorRows = null;

                    importHelper.ExtractGymnasticsData(model.ImportFile.InputStream, out correctRows,
                        out validationErrorRows);

                    if (correctRows.Count > 0)
                    {
                        List<ImportGymnasticRegistrationModel> importErrorRows = null;
                        List<ImportGymnasticRegistrationModel> duplicatedRows = null;
                        leagueRepo.ResetAllRegistrationsPositionsAndScoresToGymnasticCompetition(model.LeagueId, model.SeasonId);
                        model.SuccessCount = importHelper.ImportGymnasticsRegistrations(model.SeasonId, model.LeagueId,
                            correctRows, out importErrorRows, out duplicatedRows);

                        validationErrorRows.AddRange(importErrorRows);

                        if (validationErrorRows.Count > 0 || duplicatedRows.Count > 0 && model.SuccessCount > 0)
                        {
                            CreateErrorImportFile(importHelper, model, validationErrorRows, duplicatedRows);
                            model.Result = ImportPlayersResult.PartialyImported;
                            model.ResultMessage =
                                Messages.ImportPlayers_NoRowLoaded.Replace(Messages.Players, Messages.Gymnastics);
                        }
                        else if (validationErrorRows.Count == 0 || duplicatedRows.Count == 0 && model.SuccessCount > 0)
                        {
                            model.Result = ImportPlayersResult.Success;
                        }
                        else
                        {
                            model.Result = ImportPlayersResult.Error;
                        }

                        model.ErrorCount = correctRows.Count - model.SuccessCount;
                        model.DuplicateCount = duplicatedRows.Count;
                    }
                    else
                    {
                        model.Result = ImportPlayersResult.Error;
                        model.ResultMessage =
                            Messages.ImportPlayers_NoRowLoaded.Replace(Messages.Players, Messages.Gymnastics);
                        model.SuccessCount = 0;
                        model.DuplicateCount = 0;
                        model.ErrorCount = correctRows.Count - model.SuccessCount;

                        CreateErrorImportFile(importHelper, model, validationErrorRows, null);
                    }
                }
                else
                {
                    model.Result = ImportPlayersResult.Error;
                    model.ResultMessage = Messages.ImportPlayers_ChooseFile;
                }

                return PartialView("_ImportGymnastics", model);
            }
            catch (Exception ex)
            {
                model.Result = ImportPlayersResult.Error;
                model.ResultMessage = Messages.ImportPlayers_ImportException;
                model.ExceptionMessage = ex.Message;

                return PartialView("_ImportGymnastics", model);
            }
        }
        #endregion
        #region ImportAccountingBalances
        /// <summary>
        /// New
        /// </summary>
        /// <param name="UnionId"></param>
        /// <param name="clubId"></param>
        /// <param name="seasonId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ImportAccountingBalances(int UnionId, int seasonId)
        {
            var model = new ImportAccountingBalanceViewModel
            {
                UnionId = UnionId,
                SeasonId = seasonId
            };

            return PartialView("_ImportAccountingBalances", model);
        }
        /// <summary>
        /// New
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ImportAccountingBalances(ImportAccountingBalanceViewModel model)
        {
            int clubId = 0;

            if (model.ImportFile != null)
            {
                using (XLWorkbook workBook = new XLWorkbook(model.ImportFile.InputStream))
                {
                    //Read the first Sheet from Excel file.
                    IXLWorksheet workSheet = workBook.Worksheet(1);
                    bool firstRow = true;
                    int accountingKeyNumber = 0;
                    var balance = new List<ClubBalanceDto>();

                    var localCulture = System.Globalization.CultureInfo.CurrentCulture.ToString();
                    const int idColumn = 2;
                    const int referenceDateColumn = 9;
                    const int reference1Column = 11;
                    const int reference2Column = 12;
                    const int detailsColumn = 13;
                    const int expenseColumn = 15;
                    const int IncomColumn = 16;

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
                                #region Get clubId by AccountingKeyNumber from file
                                if (clubId == 0)
                                {
                                    if (row.RowNumber() > 6 && clubId == 0)
                                    {
                                        return RedirectToAction(nameof(ClubBalanceList), new { id = clubId, seasonId = model.SeasonId });
                                    }
                                    bool isAccountingKeyNumber = int.TryParse(row.Cell(idColumn).Value.ToString(), out accountingKeyNumber);
                                    if (isAccountingKeyNumber)
                                    {
                                        Club club = GetClubByAccountingKeyNumber(accountingKeyNumber, model.SeasonId);
                                        if (club != null)
                                            clubId = club.ClubId;
                                        else
                                        {
                                            //ModelState.AddModelError("AccountingKeyNumber", Messages.ClubNotExistForAccountingKeyNumber);
                                            return RedirectToAction(nameof(ClubBalanceList), new { id = clubId, seasonId = model.SeasonId });
                                        }
                                    }
                                }
                                #endregion

                                DateTime ReferenceDate;
                                int Reference1 = 0;
                                int Reference2 = 0;

                                if (DateTime.TryParse(row.Cell(referenceDateColumn).Value.ToString(), out ReferenceDate)
                                    && (int.TryParse(row.Cell(reference1Column).Value.ToString(), out Reference1) || int.TryParse(row.Cell(reference2Column).Value.ToString(), out Reference2)))
                                {
                                    balance.Add(new ClubBalanceDto
                                    {
                                        ActionUser = new ActionUser() { UserId = AdminId },
                                        ClubId = clubId,
                                        SeasonId = model.SeasonId,
                                        Id = clubId,
                                        TimeOfAction = ReferenceDate,
                                        Reference = Reference1 == 0 ? Reference2 : Reference1,
                                        Comment = row.Cell(detailsColumn).Value.ToString(),
                                        Expense = String.IsNullOrEmpty(row.Cell(expenseColumn).Value.ToString()) ? 0 : Convert.ToDecimal(row.Cell(expenseColumn).Value),
                                        Income = String.IsNullOrEmpty(row.Cell(IncomColumn).Value.ToString()) ? 0 : Convert.ToDecimal(row.Cell(IncomColumn).Value),
                                    });
                                }
                            }
                        }

                        clubsRepo.UpdateClubBalanceFromExcel(balance);
                    }
                    catch (Exception ex)
                    {
                        // ignore
                    }
                }
            }

            return RedirectToAction(nameof(ClubBalanceList), new { id = clubId, seasonId = model.SeasonId });
        }
        #endregion
        #endregion


        [NonAction]
        private void CreateErrorImportFile(ImportExportPlayersHelper importHelper, ImportSportsmanViewModel model,
            List<ImportGymnasticRegistrationModel> validationErrorRows,
            List<ImportGymnasticRegistrationModel> duplicatedRows)
        {
            var culture = getCulture();
            byte[] errorFileBytes = null;
            using (var errorFile =
                importHelper.BuildErrorFileForGymnastics(validationErrorRows, duplicatedRows, culture))
            {
                errorFileBytes = new byte[errorFile.Length];
                errorFile.Read(errorFileBytes, 0, errorFileBytes.Length);
            }

            Session.Remove(ImportGymnasticsErrorResultSessionKey);
            Session.Remove(ImportGymnasticsErrorResultFileNameSessionKey);
            Session.Add(ImportGymnasticsErrorResultSessionKey, errorFileBytes);
            Session.Add(ImportGymnasticsErrorResultFileNameSessionKey, model.ImportFile.FileName);
        }

        [HttpGet]
        public ActionResult DownloadPartiallyImport()
        {
            var fileByteObj = Session[ImportGymnasticsErrorResultSessionKey];
            if (fileByteObj == null)
            {
                throw new FileNotFoundException();
            }

            var fileBytes = (byte[])fileByteObj;
            var fileName = Session[ImportGymnasticsErrorResultFileNameSessionKey] as string;

            var fi = new FileInfo(fileName);

            return File(fileBytes, "application/octet-stream",
                string.Format("{0}-{2}{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension,
                    Messages.ImportPlayers_OutputFilePrefix));
        }

        public void ExportTotoReport(int id, int seasonId, int? clubId)
        {
            var union = db.Unions.Find(id);
            var sectionAlias = union?.Section?.Alias;
            var totoReportMaxBirthYear = union.TotoReportMaxBirthYear;
            Dictionary<int, List<TotoCompetitionUsers>> leagueCycleNums = null;
            Dictionary<string, int> topCompetitionRank = null;

            List<GymnasticTotoValue> sportsmen = null;
            List<TotoCompetition> competitions = null;
            if (sectionAlias == GamesAlias.Tennis)
            {
                competitions = leagueRepo.GetFinishedTennisCompeitions(id, seasonId, 16);
                sportsmen = playersRepo.GetPlayersForTennisTotoReport(competitions, seasonId, clubId, totoReportMaxBirthYear, out leagueCycleNums);
            }
            else
            {
                sportsmen = playersRepo.GetPlayersForTotoReport(id, seasonId, clubId);
                competitions = leagueRepo.GetFinishedCompeitions(id, seasonId, 16);
                if (sectionAlias == GamesAlias.Athletics)
                {
                    topCompetitionRank = gamesRepo.GetTopCompetitionsRanks(sportsmen);
                }
            }

            var isHebrew = IsHebrew;
            var firstCompetitionLetter = GetExcelColumnName(competitions.FirstOrDefault()?.ExcelPosition);
            var lastCompetitionLetter = GetExcelColumnName(competitions.LastOrDefault()?.ExcelPosition);
            var clubName = clubId.HasValue ? "_" + clubsRepo.GetById(clubId.Value).Name : "";
            byte[] data = null;
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = isHebrew })
            {
                var ws = workbook.AddWorksheet($"{Messages.ManagementOfAthletes_Caption}");


                #region Competition information

                var columnCounter = 15;
                var rowCounter = 1;

                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                var addCellWithRow = new Action<object, int>((value, colCounter) =>
                {
                    ws.Cell(rowCounter, colCounter).Value = value;
                });

                addCell(Messages.NameOfCompetition);
                var allcompetitionsColumns = 0;
                foreach (var competition in competitions)
                {
                    if (leagueCycleNums != null)
                    {
                        var isHasValue = leagueCycleNums.TryGetValue(competition.Id.Value, out List<TotoCompetitionUsers> cycleNums);
                        if (isHasValue)
                        {
                            cycleNums = cycleNums.OrderBy(c => c.GroupId).ThenBy(c => c.CycleNum).ToList();

                            foreach (var cycleNum in cycleNums)
                            {
                                if (cycleNums.Count > 1)
                                {
                                    addCell($"{competition.Name} - {cycleNum.CycleNum + 1}");
                                    allcompetitionsColumns++;
                                }
                                else
                                {
                                    addCell($"{competition.Name}");
                                    allcompetitionsColumns++;
                                }
                            }
                        }
                        else
                        {
                            addCell(competition.Name);
                            allcompetitionsColumns++;
                        }
                    }
                    else
                    {
                        addCell(competition.Name);
                        allcompetitionsColumns++;
                    }
                }
                rowCounter++;
                columnCounter = 15;
                if (sectionAlias == GamesAlias.Tennis)
                {
                    lastCompetitionLetter = GetExcelColumnName(allcompetitionsColumns + columnCounter);
                }
                addCell(Messages.DateOfCompetition);

                foreach (var competition in competitions)
                {
                    if (leagueCycleNums != null)
                    {
                        var isHasValue = leagueCycleNums.TryGetValue(competition.Id.Value, out List<TotoCompetitionUsers> cycleNums);
                        if (isHasValue)
                        {
                            cycleNums = cycleNums.OrderBy(c => c.GroupId).ThenBy(c => c.CycleNum).ToList();
                            foreach (var cycleNum in cycleNums)
                            {
                                addCell(competition.StartDate?.ToShortDateString()); // TODO SPECIFY CYCLENUM Date 
                            }
                        }
                        else
                        {
                            addCell(competition.StartDate?.ToShortDateString());
                        }
                    }
                    else
                        addCell(competition.StartDate?.ToShortDateString());
                }

                #endregion

                #region Sportsmen information

                columnCounter = 1;
                rowCounter = 3;

                addCell(Messages.IdentNum);
                addCell(Messages.TypeId);
                addCell(Messages.FirstName);
                addCell(Messages.LastName);
                addCell(Messages.Insurance);
                addCell(Messages.MedicalCertificate);
                addCell(Messages.Gender);
                addCell(Messages.YearOfBirth);
                addCell(Messages.ClubName);
                addCell(Messages.ClubNGONumber);
                addCell(Messages.NameSportCenter);
                addCell(Messages.TeamName);
                addCell(Messages.Category);
                addCell(Messages.Total + " " + Messages.Competitions.ToLower());

                rowCounter++;
                columnCounter = 1;
                ws.Column(1).Style.NumberFormat.SetFormat("@");
                ws.Columns().AdjustToContents();

                foreach (var row in sportsmen)
                {
                    var isPassport = !string.IsNullOrEmpty(row.UsersInformation.PassportNum);
                    addCell(isPassport ? row.UsersInformation.PassportNum : row.UsersInformation.IdentNum);
                    addCell(isPassport ? Messages.PassportNum : Messages.IdentNum);
                    addCell(row.UsersInformation.FirstName);
                    addCell(row.UsersInformation.LastName);
                    addCell(row.UsersInformation.Insurance ? Messages.Yes : Messages.No);
                    addCell(row.UsersInformation.MedicalCertificate ? Messages.Yes : Messages.No);
                    addCell(row.UsersInformation.GenderId == 0 ? Messages.Female : Messages.Male);
                    addCell(row.UsersInformation.YearOfBirth?.ToString());
                    addCell(row.MainInformation.ClubName);
                    addCell(row.MainInformation.ClubNumber?.ToString());
                    addCell(isHebrew ? row.MainInformation.SportCenterNameHeb : row.MainInformation.SportCenterNameEng);
                    addCell(row.MainInformation.TeamName);
                    addCell(row.UsersInformation.Category);
                    ws.Cell(rowCounter, columnCounter).FormulaA1 = sectionAlias.Equals(GamesAlias.Gymnastic)
                        ? $"COUNT({firstCompetitionLetter}{rowCounter}:{lastCompetitionLetter}{rowCounter})"
                        : (sectionAlias.Equals(GamesAlias.Athletics) ? $"COUNTIF({firstCompetitionLetter}{rowCounter}:{lastCompetitionLetter}{rowCounter},\"<>0\")" : $"SUM({firstCompetitionLetter}{rowCounter}:{lastCompetitionLetter}{rowCounter})");

                    var compIndex = 0;
                    foreach (var competition in competitions)
                    {
                        if (sectionAlias.Equals(GamesAlias.Gymnastic))
                        {
                            foreach (var usersCompetition in row.Competitions)
                            {
                                if (competition.Id == usersCompetition.Id)
                                {
                                    addCellWithRow(usersCompetition.Position, competition.ExcelPosition);
                                }
                            }
                        }

                        if (sectionAlias.Equals(GamesAlias.MartialArts))
                        {
                            if (row.Competitions.Any(r => r.Id == competition.Id))
                                addCellWithRow("1", competition.ExcelPosition);
                            else
                                addCellWithRow("0", competition.ExcelPosition);
                        }
                        if (sectionAlias.Equals(GamesAlias.WeightLifting))
                        {
                            if (row.WeightLiftingCompetitionsIds.Any(i => i == competition.Id))
                                addCellWithRow("1", competition.ExcelPosition);
                            else
                                addCellWithRow("0", competition.ExcelPosition);
                        }

                        if (sectionAlias.Equals(GamesAlias.Athletics))
                        {
                            if (row.AthleticsCompetitionsIds.Any(i => i == competition.Id) && topCompetitionRank.ContainsKey($"{row.UsersInformation.UserId}_{competition.Id.Value}"))
                                addCellWithRow(topCompetitionRank[$"{row.UsersInformation.UserId}_{competition.Id.Value}"].ToString(), competition.ExcelPosition);
                            else
                                addCellWithRow("0", competition.ExcelPosition);
                        }

                        if (sectionAlias.Equals(GamesAlias.Tennis))
                        {

                            if (leagueCycleNums != null)
                            {
                                var isHasValue = leagueCycleNums.TryGetValue(competition.Id.Value, out List<TotoCompetitionUsers> cycleNums);
                                if (isHasValue)
                                {
                                    cycleNums = cycleNums.OrderBy(c => c.GroupId).ThenBy(c => c.CycleNum).ToList();
                                    foreach (var cycleNum in cycleNums)
                                    {
                                        compIndex++;
                                        var compParticipated = row.Competitions.FirstOrDefault(r => r.Id == competition.Id && r.CycleNum == cycleNum.CycleNum && r.GroupId == cycleNum.GroupId);
                                        if (compParticipated != null)
                                        {
                                            addCellWithRow(compParticipated.count.ToString(), compIndex + 15);
                                        }
                                        else
                                            addCellWithRow("0", compIndex + 15);
                                    }
                                }
                                else
                                {
                                    compIndex++;
                                    addCellWithRow("0", compIndex + 15);
                                }
                            }
                            else
                            {
                                var compParticipated = row.Competitions.FirstOrDefault(r => r.Id == competition.Id);
                                if (compParticipated != null)
                                    addCellWithRow(compParticipated.count.ToString(), competition.ExcelPosition);
                                else
                                    addCellWithRow("0", competition.ExcelPosition);
                            }
                        }
                    }

                    rowCounter++;
                    columnCounter = 1;
                }
                ws.Column(1).Style.NumberFormat.SetFormat("@");
                ws.Columns().AdjustToContents();

                #endregion

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition",
                    $"attachment;filename={ Messages.TotoReport}{ clubName}.xlsx");
                using (var myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }

            }
        }

        private string GetExcelColumnName(int? columnNumber)
        {
            if (columnNumber.HasValue)
            {
                var dividend = columnNumber.Value;
                var columnName = String.Empty;
                int modulo;

                while (dividend > 0)
                {
                    modulo = (dividend - 1) % 26;
                    columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                    dividend = (int)((dividend - modulo) / 26);
                }

                return columnName;
            }
            else return string.Empty;
        }

        [HttpPost]
        public void ClubActivityReport(int id, int seasonId)
        {
            var clubsActivityService = new ClubActivityReportService(db);
            var sectionAlias = unionsRepo.GetSectionByUnionId(id)?.Alias;
            var clubsInformation = clubsActivityService.GetActivityReport(id, seasonId, sectionAlias);
            var disciplineNames = clubsInformation.FirstOrDefault()?.DisciplinesInformation?.Select(d => d.Name);

            using (var workbook =
                new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var ws = workbook.AddWorksheet($"{Messages.ClubActivityReport}");

                clubsActivityService.GenerateExcelStructure(ws, sectionAlias, clubsInformation, disciplineNames);

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition",
                    $"attachment;filename={Messages.ClubActivityReport.ToLower().Replace(" ", "_")}.xlsx");


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
        public ActionResult ChangeInsuranceStatus(int clubId, int teamId, string type, bool isChecked)
        {
            var club = clubsRepo.GetById(clubId);
            teamRepo.ChangeInsuranceStatus(teamId, type, isChecked);
            teamRepo.Save();
            return Json(new { NeedAlert = isChecked && string.IsNullOrEmpty(club?.ClubInsurance) && type == "club" });
        }

        /// <summary>
        /// This is temporary method which alows to remove all league names from tennis teams
        /// </summary>
        [HttpGet]
        public ActionResult RemoveLeagueNamesFromTennis()
        {
            var count = clubsRepo.RemoveLeagueNamesFromTennis();
            return Content($"Removed league names from <b>{count}</b> teams");
        }

        [HttpPost]
        public ActionResult DisciplineCompetitionRegisterPlayer(RegisterPlayerToCompDTO data)
        {
            var season = seasonsRepository.GetById(data.SeasonId);
            var player = playersRepo.GetTeamPlayerByUserIdAndSeasonId(data.UserId, season.Id);
            int isAdded;

            if (data.SectionAlias == GamesAlias.WeightLifting)
            {
                isAdded = disciplinesRepo.RegisterWeightLifterUnderCompetitionDiscipline(data.DisciplineId, player.ClubId, data.UserId, data.WeightDeclaration);
            }
            else
            {
                if (data.SectionAlias == GamesAlias.Bicycle)
                {
                    isAdded = disciplinesRepo.RegisterBicycleRiderUnderCompetitionExpertise(data.DisciplineId,
                        player.ClubId, data.UserId);
                }
                else
                {
                    isAdded = disciplinesRepo.RegisterAthleteUnderCompetitionDiscipline(data.DisciplineId,
                        player.ClubId, data.UserId);
                }
            }

            if (isAdded > 0)
            {
                if (data.SectionAlias == GamesAlias.Athletics)
                {
                    var competitionDiscipline = disciplinesRepo.GetCompetitionDisciplineById(data.DisciplineId);
                    var discipline = disciplinesRepo.GetById(competitionDiscipline.DisciplineId.Value);
                    return Content($"<tr id='reg_tr_{isAdded}'><td>{player.User.AthleteNumbers.FirstOrDefault(x => x.SeasonId == data.SeasonId)?.AthleteNumber1}</td><td>{discipline.Name}</td><td>{competitionDiscipline.CompetitionAge.age_name}</td><td>{player.User.FullName}</td><td>{player.Club.Name}</td><td>{player.User.BirthDay?.ToShortDateString()}</td><td><input type='text' class='form-control' style='max-width: 120px;' id='heat_id_{isAdded}' value='' onchange='onHeatChange({isAdded});' /><td><input type='number' class='form-control' style='max-width: 120px;' id='lane_id_{isAdded}' value='' onchange='onLaneChange({isAdded});' /></td></td><td></td><td class='remove_print'><a class='{AppCss.Delete}' data-ajax='true' data-ajax-confirm='{Messages.DeleteConfirm}' data-ajax-success='deleteRegistration({isAdded})' href='{Url.Action("DeleteDisciplineRegistration", "Clubs", new { id = isAdded })}'> </a> </td></tr>");
                }
                if (data.SectionAlias == GamesAlias.WeightLifting)
                {
                    if (data.WeightDeclaration == null)
                    {
                        data.WeightDeclaration = 0.0M;
                    }
                    return Content($"<tr id='reg_tr_{isAdded}'><td>{player.Club.Name}</td><td>{player.User.FullName}</td><td>{player.User.BirthDay?.ToShortDateString()}</td><td>{(int)(data.WeightDeclaration)}</td><td><input class='approvereg' data-id='{isAdded} ' data-val='false' id='reg_IsApproved' name='reg.IsApproved' type='checkbox' value='false' onclick='approveReg({isAdded}, this);' ></td><td><input class='chargereg' data-id='{isAdded} ' data-val='false' id='reg_IsCharged' name='reg.IsCharged' type='checkbox' value='false' onclick='chargeReg({isAdded}, this);' ></td><td class='remove_print'><a class='{AppCss.Delete}' data-ajax='true' data-ajax-confirm='{Messages.DeleteConfirm}' data-ajax-success='deleteRegistration({isAdded})' href='{Url.Action("DeleteDisciplineRegistration", "Clubs", new { id = isAdded })}'> </a> </td></tr>");
                }
                if (data.SectionAlias == GamesAlias.Bicycle)
                {
                    var compExpHeat = disciplinesRepo.GetCompetitionExpertiesHeatByExpId(data.DisciplineId);
                    var categoryName = compExpHeat?.CompetitionExperty.DisciplineExpertise.Name;
                    var heatName = compExpHeat?.BicycleCompetitionHeat.Name;
                    return Content($"<tr id='reg_tr_{isAdded}'><td>{player.User.IdentNum}</td><td>{player.User.FullName}</td><td>{player.User.BirthDay?.ToShortDateString()}</td><td>{player.Club?.Name}</td><td>{categoryName}</td><td>{heatName}</td><td class='remove_print'><a class='{AppCss.Delete}' data-ajax='true' data-ajax-confirm='{Messages.DeleteConfirm}' data-ajax-success='deleteRegistration({isAdded})' href='{Url.Action("DeleteBicycleRegistration", "Clubs", new { id = isAdded })}'> </a> </td></tr>");
                }
                if (data.SectionAlias == GamesAlias.Climbing)
                {
                    var competitionDiscipline = disciplinesRepo.GetCompetitionDisciplineById(data.DisciplineId);
                    var categoryName = competitionDiscipline?.CompetitionAge?.age_name;
                    return Content($"<tr id='reg_tr_{isAdded}'><td>{player.User.FullName}</td><td>{player.Club.Name}</td><td>{player.User.BirthDay?.ToShortDateString()}</td><td>{categoryName}</td><td><input type='text' class='form-control' style='max-width: 120px;' id='heat_id_{isAdded}' value='' onchange='onHeatChange({isAdded});' /></td><td></td><td class='remove_print'><a class='{AppCss.Delete}' data-ajax='true' data-ajax-confirm='{Messages.DeleteConfirm}' data-ajax-success='deleteRegistration({isAdded})' href='{Url.Action("DeleteDisciplineRegistration", "Clubs", new { id = isAdded })}'> </a> </td></tr>");

                }
            }
            return Content("");
        }

        [HttpGet]
        public ActionResult SportsmenRegisteredInCompetitionDiscipline(int clubId, int competitionDisciplineId)
        {
            var players = playersRepo.GetPlayersDisciplineRegistrations(clubId, competitionDisciplineId).ToList();
            return Json(JsonConvert.SerializeObject(new { Success = true, players }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task ChangeComment(int clubId, string comment)
        {
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId);
            if (club != null)
            {
                club.Comment = comment;
                await db.SaveChangesAsync();
            }
        }

        [HttpPost]
        public async Task ChangeClubActiveStatus(int clubId, bool isApproved)
        {
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId);
            if (club != null)
            {
                club.IsClubApproved = isApproved;
                await db.SaveChangesAsync();
            }
        }

        [HttpGet]
        public ActionResult ClubPayments(int clubId)
        {
            var clubPayment = clubsRepo.GetById(clubId)?.ClubPayments;
            ViewBag.ClubId = clubId;
            ViewBag.HasPermission = User.IsInAnyRole(AppRole.Admins) ||
                                    JobRole.UnionManager.Equals(usersRepo.GetTopLevelJob(AdminId));
            return PartialView("_MartialArtsPayment", clubPayment);
        }

        public async Task<ActionResult> CreateNewClubPayment(ClubPaymentViewModel vm)
        {
            ViewBag.ClubId = vm.ClubId;
            db.ClubPayments.Add(new ClubPayment
            {
                ClubId = vm.ClubId,
                Paid = vm.Paid,
                DateOfPayment = vm.DateOfPayment
            });
            await db.SaveChangesAsync();

            return Json(new { Success = true, Id = db.ClubPayments.OrderByDescending(p => p.Id)?.FirstOrDefault()?.Id });
        }

        public async Task<ActionResult> DeleteNewClubPayment(int id)
        {
            var clubPayment = db.ClubPayments.Find(id);
            if (clubPayment != null)
            {
                db.ClubPayments.Remove(clubPayment);
                await db.SaveChangesAsync();
            }

            return Json(new { Success = true });
        }

        public async Task<ActionResult> UpdateClubPayment(ClubPaymentViewModel vm)
        {
            if (vm.Id.HasValue)
            {
                var clubPayment = db.ClubPayments.Find(vm.Id.Value);
                if (clubPayment != null)
                {
                    clubPayment.Paid = vm.Paid;
                    clubPayment.DateOfPayment = vm.DateOfPayment;
                    await db.SaveChangesAsync();
                }
            }

            return Json(new { Success = true });
        }

        public ActionResult ClubBalanceList(int id, int seasonId)
        {
            try
            {
                var vm = cbsServ.GetClubBalance(id, seasonId);
                var job = usersRepo.GetTopLevelJob(AdminId);
                var club = clubsRepo.GetById(id);
                ViewBag.IsTennis = false;
                if (club.Union?.Section?.Alias == GamesAlias.Tennis)
                {
                    ViewBag.IsTennis = true;
                }
                ViewBag.HasPermission = User.IsInAnyRole(AppRole.Admins) || job == JobRole.UnionManager
                    || (job == JobRole.ClubManager || job == JobRole.ClubSecretary) && club.UnionId == null;
                ViewBag.HasTopPermission = User.IsInAnyRole(AppRole.Admins) || job == JobRole.UnionManager;
                ViewBag.ClubId = id;
                ViewBag.SeasonId = seasonId;
                return PartialView("_ClubBalance", vm);
            }
            catch
            {
                return new HttpStatusCodeResult(400, "Error11");
            }
        }

        [HttpPost]
        public ActionResult CreateClubBalance(int id, int seasonId, ClubBalanceDto balance)
        {
            balance.ActionUser.UserId = AdminId;
            cbsServ.CreateBalanceRecord(id, balance);
            return RedirectToAction(nameof(ClubBalanceList), new { id, seasonId });
        }

        [HttpPost]
        public ActionResult UpdateClubBalance(int clubId, int seasonId, ClubBalanceDto balance)
        {
            balance.ActionUser.UserId = AdminId;
            cbsServ.UpdateBalanceRecord(clubId, balance);
            return RedirectToAction(nameof(ClubBalanceList), new { id = clubId, seasonId = seasonId });
        }

        public ActionResult DeleteClubBalance(int id, int seasonId, int clubBalanceId)
        {
            cbsServ.DeleteBalanceRecord(clubBalanceId);
            return RedirectToAction(nameof(ClubBalanceList), new { id, seasonId });
        }

        [HttpPost]
        public ActionResult ImportParentStatement(EditClubViewModel model)
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

                var fileName = FileUtil.SaveFile(file, savePath, identNum, PlayerFileType.ParentStatement);

                if (fileName == null) continue;

                var existingFile = player.PlayerFiles.FirstOrDefault(x =>
                   x.SeasonId == model.SeasonId &&
                   x.FileType == (int)PlayerFileType.ParentStatement && !x.IsArchive);

                var latestTeamPlayer = player.TeamsPlayers.LastOrDefault(tp => tp.SeasonId == model.SeasonId);
                var isIndividual = false;
                if (latestTeamPlayer != null)
                {
                    isIndividual = latestTeamPlayer.Season?.Union?.Section?.IsIndividual ?? false;
                }

                if (existingFile == null)
                {
                    player.PlayerFiles.Add(new PlayerFile
                    {
                        SeasonId = model.SeasonId,
                        DateCreated = DateTime.Now,
                        FileName = fileName,
                        FileType = (int)PlayerFileType.ParentStatement
                    });
                }
                else
                {
                    if (!isIndividual)
                    {
                        var filePath = Server.MapPath(GlobVars.ContentPath + "/players/" + existingFile.FileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                        db.Entry(existingFile).State = EntityState.Deleted;
                    }
                    else
                    {
                        existingFile.IsArchive = true;
                    }

                    player.PlayerFiles.Add(new PlayerFile
                    {
                        SeasonId = model.SeasonId,
                        DateCreated = DateTime.Now,
                        FileName = fileName,
                        FileType = (int)PlayerFileType.ParentStatement
                    });

                }

            }

            usersRepo.Save();

            return RedirectToAction("Edit", new { id = model.Id, unionId = model.UnionId, seasonId = model.SeasonId });
        }

        [HttpPost]
        public ActionResult ImportIdFiles(EditClubViewModel model)
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

            return RedirectToAction("Edit", new { id = model.Id, unionId = model.UnionId, seasonId = model.SeasonId });
        }


        [HttpPost]
        public ActionResult ImportMedicalCertificates(EditClubViewModel model)
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

            var playersToImport = db.Users
                .Where(x => identNums.Contains(x.IdentNum))
                .Include(x => x.PlayerFiles)
                .ToList();
            var savePath = Server.MapPath($"{GlobVars.ContentPath}/players/");

            for (var i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];
                if (file == null) continue;

                var identNum = Path.GetFileNameWithoutExtension(file.FileName);

                var player = playersToImport.FirstOrDefault(x => x.IdentNum == identNum);
                if (player == null) continue;

                var fileName = FileUtil.SaveFile(file, savePath, identNum, PlayerFileType.MedicalCertificate);

                if (fileName == null) continue;

                var existingFile = player.PlayerFiles.FirstOrDefault(x =>
                    x.SeasonId == model.SeasonId &&
                    x.FileType == (int)PlayerFileType.MedicalCertificate && !x.IsArchive);


                var latestTeamPlayer = player.TeamsPlayers.LastOrDefault(tp => tp.SeasonId == model.SeasonId);
                var isIndividual = false;
                if (latestTeamPlayer != null)
                {
                    isIndividual = latestTeamPlayer.Season?.Union?.Section?.IsIndividual ?? false;
                }

                if (!User.IsInAnyRole(AppRole.Admins) && !User.HasTopLevelJob(JobRole.UnionManager))
                {
                    var teamPlayersOfThisSeason = player.TeamsPlayers.Where(tp => tp.SeasonId == model.SeasonId).ToList();
                    teamPlayersOfThisSeason.ForEach(tp => {
                        if ((tp.Club?.Union?.Section?.Alias == GamesAlias.Tennis || tp.Season?.Union?.Section?.Alias == GamesAlias.MartialArts) && tp.IsApprovedByManager.HasValue && tp.IsApprovedByManager.Value)
                        {
                            tp.IsApprovedByManager = null;
                        }
                    });
                }

                if (existingFile == null)
                {
                    player.PlayerFiles.Add(new PlayerFile
                    {
                        SeasonId = model.SeasonId,
                        DateCreated = DateTime.Now,
                        FileName = fileName,
                        FileType = (int)PlayerFileType.MedicalCertificate
                    });
                }
                else
                {
                    if (!isIndividual)
                    {
                        var filePath = Server.MapPath(GlobVars.ContentPath + "/players/" + existingFile.FileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                        db.Entry(existingFile).State = EntityState.Deleted;
                    }
                    else
                    {
                        existingFile.IsArchive = true;
                    }

                    player.PlayerFiles.Add(new PlayerFile
                    {
                        SeasonId = model.SeasonId,
                        DateCreated = DateTime.Now,
                        FileName = fileName,
                        FileType = (int)PlayerFileType.MedicalCertificate
                    });

                }
            }

            usersRepo.Save();

            return RedirectToAction("Edit", new { id = model.Id, unionId = model.UnionId, seasonId = model.SeasonId });
        }


        //Created for ROWING section
        [HttpPost]
        public void ExportTeamRegistrationsToExcel(int competitionId)
        {

            var vm = GetInfoForAllBoats(competitionId);

            var orderedBoats = vm.ClubTeamsWithBoats.OrderBy(x => x.Boat).ThenBy(x => Convert.ToInt32(x.Distance)).ThenBy(x => x.Category).ThenBy(x => x.ClubName);
            using (var workbook =
                new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var ws = workbook.AddWorksheet($"{Messages.RegistrationList}");

                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                addCell("#");
                addCell(Messages.ClubName);
                addCell(Messages.Boat);
                addCell(Messages.Distance);
                addCell(Messages.Category);
                addCell(Messages.NumberOfBoats);
                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();



                foreach (var row in orderedBoats)
                {
                    addCell(row.ClubId.ToString());
                    addCell(row.ClubName);
                    addCell(row.Boat);
                    addCell(row.Distance);
                    addCell(row.Category);
                    addCell(row.TeamNumber.ToString());

                    rowCounter++;
                    columnCounter = 1;
                }


                ws.Columns().AdjustToContents();

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                Response.AddHeader("content-disposition",
                                            $"attachment;filename={vm.Title.Replace(' ', '_').Replace(",", "")}_{Messages.Team.ToLower()} {Messages.RegistrationStatus.ToLower()}.xlsx");

                using (var myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        /// <summary>
        /// Export all Clubs Debt for union in season
        /// </summary>
        /// <param name="id"></param>
        /// <param name="logicalName"></param>
        /// <param name="seasonId"></param>
        public void ExportDebtReport(int unionId, int? seasonId)
        {
            Union union = unionsRepo.GetById(unionId);
            ClubsRepo clubsRepo = new ClubsRepo();
            List<Club> clubs = clubsRepo.GetByUnion(unionId, seasonId);
            //Create List of clubs balance
            List<ClubBalanceDto> clubsBalance = new List<ClubBalanceDto>();
            foreach (var club in clubs)
            {
                var res = cbsServ.GetClubBalance(club.ClubId, Convert.ToInt32(seasonId));
                clubsBalance.Add(res.OrderByDescending(p => p.Id).FirstOrDefault());
            }
            //List<ClubBalance> clubsBalance = clubsRepo.GetClubsBalance(seasonId);

            var isHebrew = IsHebrew;
            var contentPath = Server.MapPath(GlobVars.ContentPath);
            var activesExportPdfHelper = new ActivesExportPdfHelper(union, clubs, clubsBalance, isHebrew, contentPath);
            var exportResult = activesExportPdfHelper.GetDocumentStream();

            var firstFilenamePart = union.Name;
            string mainName = Messages.DebtReport;

            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename={firstFilenamePart}_{mainName}_{DateTime.Now.ToShortDateString()}.pdf");

            Response.OutputStream.Write(exportResult.GetBuffer(), 0, exportResult.GetBuffer().Length);
            Response.Flush();
            Response.End();
        }

        public Club GetClubByAccountingKeyNumber(int accountingKeyNumber, int SeasonId)
        {
            return clubsRepo.AccountingKeyNumber(accountingKeyNumber, SeasonId);
        }



        #region HTML
        #endregion

        #region Functions
        #endregion

        #region Exports
        #endregion

        #region Imports
        #endregion

        #region General
        #endregion


    }
}