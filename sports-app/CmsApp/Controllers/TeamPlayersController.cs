using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Resources;
using CmsApp.Models;
using AppModel;
using Omu.ValueInjecter;
using CmsApp.Helpers;
using DataService.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using DataService;
using DataService.Utils;
using System.Web;
using ClosedXML.Excel;
using Microsoft.Ajax.Utilities;
using CmsApp.Services;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using Amazon.Runtime.Internal;
using DataService.LeagueRank;
using Newtonsoft.Json;
using CmsApp.Helpers.ActivityHelpers;

namespace CmsApp.Controllers
{
    public class TeamPlayersController : AdminController
    {
        private const string ExportPlayersResultSessionKey = "ExportPlayers_Result";
        private const string ExportPlayersResultFileNameSessionKey = "ExportPlayers_ResultFileName";
        NotesMessagesRepo notesRep = new NotesMessagesRepo();
        private readonly TeamsRepo _teamsRepo;

        public TeamPlayersController()
        {
            _teamsRepo = new TeamsRepo();
        }

        public ActionResult Edit(int id, int? currentLeagueId, int seasonId, int? clubId)
        {
            var vm = new TeamPlayerForm
            {
                IsActive = true,
                TeamId = id,
                SeasonId = seasonId,
                Positions = new SelectList(posRepo.GetByTeam(id), "PosId", "Title"),
                ClubId = clubId,
                LeagueId = currentLeagueId,
                IsCurrentUserUnionManager =
                    usersRepo.GetCurrentJob(AdminId, getCurrentSeason().Id) == JobRole.UnionManager
            };

            return PartialView("_Edit", vm);
        }

        [HttpPost]
        public ActionResult Edit(TeamPlayerForm frm)
        {
            var user = playersRepo.GetUserByIdentNum(frm.IdentNum);
            if (user == null)
            {
                ModelState.AddModelError("IdentNum", Messages.PlayerNotExists);
            }
            else
            {
                var tu = playersRepo.GetTeamPlayer(frm.TeamId, user.UserId, frm.SeasonId, frm.PosId);
                if (tu != null)
                {
                    ModelState.AddModelError("PosId", Messages.PlayerAlreadyOnThisPosition);
                }

                //if (playersRepo.ShirtNumberExists(frm.TeamId, frm.ShirtNum, frm.SeasonId, frm.LeagueId, frm.ClubId))
                //{
                //    ModelState.AddModelError("ShirtNum", Messages.ShirtAlreadyExists);
                //}
            }

            if (!ModelState.IsValid)
            {
                frm.Positions = new SelectList(posRepo.GetByTeam(frm.TeamId), "PosId", "Title");
                return PartialView("_Edit", frm);
            }

            var item = new TeamsPlayer();
            item.InjectFrom(frm);
            item.UserId = user.UserId;
            playersRepo.AddToTeam(item);
            playersRepo.Save();

            return RedirectToAction("Edit", new {id = frm.TeamId});
        }


        public ActionResult List(DepartmentSettings departmentSettings, int id, int seasonId,
            int? currentLeagueId = null, int? clubId = null, int? unionId = null,
            int? isTennisCompetition = null, bool IsRegistrationBlockedForNonTopLevel = false)
        {
            var team = teamRepo.GetById(id);
            var alias = secRepo.GetSectionByTeamId(id)?.Alias;
            var isGymnastics = string.Equals(alias, GamesAlias.Gymnastic, StringComparison.InvariantCultureIgnoreCase);
            ViewBag.NeedShowRanks = string.Equals(secRepo.GetSectionByTeamId(id)?.Alias, GamesAlias.Tennis) &&
                                    isTennisCompetition != 1 &&
                                    !teamRepo.GetClubByTeamId(id).IsTrainingTeam; // && !currentLeagueId.HasValue
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId);
            var union = db.Unions.FirstOrDefault(u => u.UnionId == unionId);

            var resList = LoadPlayers(id, seasonId, currentLeagueId, clubId);

            ViewBag.IsUnionConnected = union != null || club?.UnionId > 0;

            if (isTennisCompetition.HasValue && isTennisCompetition.Value == 1)
            {
                var isTeamHasActivityThatRequiresPayment = false;
                if (currentLeagueId.HasValue)
                {
                    var league = leagueRepo.GetById(currentLeagueId.Value);
                    var activityForLeague = league.ActivitiesLeagues.Select(l => l.Activity)
                        .FirstOrDefault(a => a.Type == "personal");
                    if (activityForLeague != null)
                    {
                        /*
                        var customPrices = !string.IsNullOrWhiteSpace(activityForLeague.CustomPrices)
                                            ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(activityForLeague.CustomPrices)
                                            : new List<ActivityCustomPriceModel>();
                        */
                        if (activityForLeague.RegistrationPrice || activityForLeague.InsurancePrice ||
                            activityForLeague.MembersFee || activityForLeague.HandlingFee ||
                            activityForLeague.CustomPricesEnabled)
                        {
                            isTeamHasActivityThatRequiresPayment = true;
                        }
                    }
                }


                var playersPaid = new List<TeamPlayerItem>();
                foreach (var playerItem in resList)
                {
                    var player = playersRepo.GetTeamsPlayerById(playerItem.Id);
                    var isAtleastOnePaid = false;
                    foreach (var activityForm in player.User.ActivityFormsSubmittedDatas.Where(a =>
                        a.TeamId == team.TeamId))
                    {
                        //hasCondition = true;
                        var activity = db.Activities.FirstOrDefault(a => a.ActivityId == activityForm.ActivityId);
                        activityForm.Activity = activity;
                        decimal balance = 0;
                        balance += activityForm.Activity.RegistrationPrice
                            ? (!activityForm.IsPaymentByBenefactor ? activityForm.RegistrationPrice : 0)
                            : 0;
                        balance += activityForm.Activity.InsurancePrice
                            ? (activityForm.DisableInsurancePayment == true ? 0 : activityForm.InsurancePrice)
                            : 0;
                        balance += activityForm.Activity.MembersFee
                            ? (activityForm.DisableMembersFeePayment == true ? 0 : activityForm.MembersFee)
                            : 0;
                        balance += activityForm.Activity.HandlingFee
                            ? (activityForm.DisableHandlingFeePayment == true ? 0 : activityForm.HandlingFee)
                            : 0;

                        var customPrices = !string.IsNullOrWhiteSpace(activityForm.CustomPrices)
                            ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(activityForm.CustomPrices)
                            : new List<ActivityCustomPriceModel>();

                        balance += activityForm.Activity.CustomPricesEnabled ? customPrices.Sum(x => x.TotalPrice) : 0;

                        balance -= activityForm.Activity.RegistrationPrice
                            ? (!activityForm.IsPaymentByBenefactor ? activityForm.RegistrationPaid : 0)
                            : 0;
                        balance -= activityForm.Activity.InsurancePrice
                            ? (activityForm.DisableInsurancePayment ? activityForm.InsurancePaid : 0)
                            : 0;
                        balance -= activityForm.Activity.MembersFee
                            ? (activityForm.DisableMembersFeePayment == true ? 0 : activityForm.MembersFeePaid)
                            : 0;
                        balance -= activityForm.Activity.HandlingFee
                            ? (activityForm.DisableHandlingFeePayment == true ? 0 : activityForm.HandlingFeePaid)
                            : 0;
                        balance -= activityForm.Activity.CustomPricesEnabled ? customPrices.Sum(x => x.Paid) : 0;


                        if (balance <= 0)
                        {
                            isAtleastOnePaid = true;
                            break;
                        }
                    }

                    if (!isAtleastOnePaid && currentLeagueId > 0)
                    {
                        //hasCondition = true;
                        foreach (var activityForm in player.User.ActivityFormsSubmittedDatas.Where(a =>
                            a.LeagueId == currentLeagueId))
                        {
                            var activity = db.Activities.FirstOrDefault(a => a.ActivityId == activityForm.ActivityId);
                            activityForm.Activity = activity;
                            decimal balance = 0;
                            balance += activityForm.Activity.RegistrationPrice
                                ? (!activityForm.IsPaymentByBenefactor ? activityForm.RegistrationPrice : 0)
                                : 0;
                            balance += activityForm.Activity.InsurancePrice
                                ? (activityForm.DisableInsurancePayment == true ? 0 : activityForm.InsurancePrice)
                                : 0;
                            balance += activityForm.Activity.MembersFee
                                ? (activityForm.DisableMembersFeePayment == true ? 0 : activityForm.MembersFee)
                                : 0;
                            balance += activityForm.Activity.HandlingFee
                                ? (activityForm.DisableHandlingFeePayment == true ? 0 : activityForm.HandlingFee)
                                : 0;

                            var customPrices = !string.IsNullOrWhiteSpace(activityForm.CustomPrices)
                                ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(activityForm
                                    .CustomPrices)
                                : new List<ActivityCustomPriceModel>();

                            balance += activityForm.Activity.CustomPricesEnabled
                                ? customPrices.Sum(x => x.TotalPrice)
                                : 0;

                            var totalFee = balance;
                            decimal totalPaid = 0;
                            totalPaid += activityForm.Activity.RegistrationPrice
                                ? (!activityForm.IsPaymentByBenefactor ? activityForm.RegistrationPaid : 0)
                                : 0;
                            totalPaid += activityForm.Activity.InsurancePrice
                                ? (activityForm.DisableInsurancePayment ? activityForm.InsurancePaid : 0)
                                : 0;
                            totalPaid += activityForm.Activity.MembersFee
                                ? (activityForm.DisableMembersFeePayment == true ? 0 : activityForm.MembersFeePaid)
                                : 0;
                            totalPaid += activityForm.Activity.HandlingFee
                                ? (activityForm.DisableHandlingFeePayment == true ? 0 : activityForm.HandlingFeePaid)
                                : 0;
                            totalPaid += activityForm.Activity.CustomPricesEnabled ? customPrices.Sum(x => x.Paid) : 0;


                            if (totalFee - totalPaid <= 0 && totalFee > 0)
                            {
                                isAtleastOnePaid = true;
                                break;
                            }
                        }
                    }

                    if (isAtleastOnePaid || !isTeamHasActivityThatRequiresPayment)
                    {
                        playersPaid.Add(playerItem);
                    }
                }

                resList = playersPaid;
            }

            if (alias.Equals(GamesAlias.Tennis) && isTennisCompetition != 1)
            {
                resList = resList.OrderBy(r => r.TennisPositionOrder ?? int.MaxValue).ThenBy(r => r.FullName)?.ToList();
            }

            var userJob = usersRepo.GetTopLevelJob(AdminId);
            if (clubId.HasValue && ViewBag.LeaguesDetail == null ||
                !((List<LeagueDetailsForm>) ViewBag.LeaguesDetail).Any())
            {
                ViewBag.CanSetParticipationDiscount = User.IsInAnyRole(AppRole.Admins) ||
                                                      userJob == JobRole.ClubManager;
                ViewBag.CanUpdateComment = User.IsInAnyRole(AppRole.Admins) ||
                                           userJob == JobRole.ClubManager ||
                                           userJob == JobRole.ClubSecretary;
            }
            else
            {
                ViewBag.CanSetRegistrationDiscount = User.IsInRole(AppRole.Workers) &&
                                                     userJob == JobRole.UnionManager ||
                                                     User.IsInAnyRole(AppRole.Admins);
                ViewBag.CanUpdateComment = User.IsInRole(AppRole.Workers) &&
                                           userJob == JobRole.UnionManager ||
                                           userJob == JobRole.ClubSecretary ||
                                           User.IsInAnyRole(AppRole.Admins);
            }

            ViewBag.CanSetNoInsurance = User.IsInAnyRole(AppRole.Admins) ||
                                        userJob == JobRole.ClubManager ||
                                        userJob == JobRole.ClubSecretary ||
                                        userJob == JobRole.UnionManager;
            ViewBag.CanRemovePlayer = User.IsInAnyRole(AppRole.Admins) ||
                                      userJob == JobRole.UnionManager ||
                                      unionId != 36;

            if (ViewBag.LeagueId == null)
            {
                ViewBag.LeagueId = currentLeagueId;
            }

            if (ViewBag.UnionId == null)
            {
                ViewBag.UnionId = unionId;
            }

            ViewBag.IsRegistrationBlockedForNonTopLevel = IsRegistrationBlockedForNonTopLevel;
            ViewBag.SeasonId = seasonId;
            ViewBag.ClubId = clubId;
            if (unionId.HasValue)
                ViewBag.Is31Union = unionId.Value == 31;
            else if (clubId.HasValue)
                ViewBag.Is31Union = clubsRepo.GetById(clubId.Value)?.Union?.UnionId == 31;
            else if (currentLeagueId.HasValue)
                ViewBag.Is31Union = leagueRepo.GetById(currentLeagueId.Value)?.Union?.UnionId == 31;
            else
                ViewBag.Is31Union = false;
            var role = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.IsGymnastics = isGymnastics;
            ViewBag.IsCatchball = string.Equals(secRepo.GetSectionByTeamId(id)?.Alias, GamesAlias.NetBall,
                StringComparison.InvariantCultureIgnoreCase);
            ViewBag.IsWaterpolo = string.Equals(secRepo.GetSectionByTeamId(id)?.Alias, GamesAlias.WaterPolo,
                StringComparison.InvariantCultureIgnoreCase);
            ViewBag.IsAthletics = string.Equals(secRepo.GetSectionByTeamId(id)?.Alias, GamesAlias.Athletics,
                StringComparison.InvariantCultureIgnoreCase);
            ViewBag.IsTennis = string.Equals(secRepo.GetSectionByTeamId(id)?.Alias, GamesAlias.Tennis,
                StringComparison.InvariantCultureIgnoreCase);
            ViewBag.IsRugby = string.Equals(secRepo.GetSectionByTeamId(id)?.Alias, GamesAlias.Rugby,
                StringComparison.InvariantCultureIgnoreCase);
            ViewBag.DepartmentSettings = departmentSettings.IsDepartmentTeam == true ? departmentSettings : null;
            ViewBag.Culture = getCulture();
            ViewBag.IsMotorsport = string.Equals(secRepo.GetSectionByTeamId(id)?.Alias, GamesAlias.Motorsport,
                StringComparison.InvariantCultureIgnoreCase);
            ViewBag.CantChangeIfAccepted = ((union != null || club != null && club.IsUnionClub == true)
                                            && !(User.IsInAnyRole(AppRole.Admins) ||
                                                 User.HasTopLevelJob(JobRole.UnionManager)));
            ViewBag.IsLeagueTeam = false;
            ViewBag.BlockApprovements = union?.IsClubsBlocked == true &&
                                        (role?.Equals(JobRole.TeamManager) == true ||
                                         role?.Equals(JobRole.ClubSecretary) == true ||
                                         role?.Equals(JobRole.ClubManager) == true);
            var clubTeam = db.ClubTeams.FirstOrDefault(c => c.TeamId == id);
            ViewBag.IsLeagueTeam = !clubTeam?.IsTrainingTeam ?? false;
            ViewBag.IsAdmin = User.IsInAnyRole(AppRole.Admins);
            ViewBag.IsUnionManager = User.HasTopLevelJob(JobRole.UnionManager);
            ViewBag.IsClubSecretary = User.HasTopLevelJob(JobRole.ClubSecretary);
            ViewBag.IsTeamManager = User.HasTopLevelJob(JobRole.TeamManager);
            ViewBag.IsTennisCompetition = isTennisCompetition.HasValue && isTennisCompetition.Value == 1;
            ViewBag.TeamId = id;
            var sectionAlias = secRepo.GetSectionByTeamId(id)?.Alias;
            var leagueId = teamRepo.GetLeagueIdByTeamId(id);
            var players = playersRepo.GetTeamPlayersByTeamIdsShort(id, leagueId, seasonId, sectionAlias);
            clubsRepo.CountPlayersRegistrations(players, sectionAlias.Equals(GamesAlias.Gymnastic),
                out var approvedPlayers, out var completedPlayers,
                out var notApprovedPlayers, out var playersCount, out var waitingForApproval, out var activePlayers,
                out var notActive,
                out var registered);
            ViewBag.NumberOfApprovedPlayers = approvedPlayers;
            return PartialView("_List", resList);
        }

        public ActionResult Delete(int id, int? seasonId, bool IsTennisCompetition = false, int? leagueId = null,
            int? clubId = null, int? unionId = null)
        {
            var teamPlayer = playersRepo.GetTeamPlayerBySeasonId(id, seasonId);
            playersRepo.RemoveFromTeam(teamPlayer);
            playersRepo.Save();

            return RedirectToAction("List",
                new
                {
                    id = teamPlayer.TeamId, seasonId, currentLeagueId = leagueId, clubId, unionId,
                    isTennisCompetition = IsTennisCompetition == true ? 1 : 0
                });
        }

        public ActionResult ExportRoster(int? clubId, int? leagueId, int teamId, int seasonId)
        {
            var hebrew = IsHebrew;
            var exportHelper = new TournamentRosterPdfExportHelper(hebrew);

            var club = clubsRepo.GetById(clubId ?? 0);
            var league = leagueRepo.GetById(leagueId ?? 0);
            var teamName = teamRepo.GetCurrentTeamName(teamId, seasonId);
            var teamOfficials = jobsRepo.GetTeamUsersJobs(teamId, seasonId);

            var teamPlayers = db.TeamsPlayers
                .Include(x => x.User)
                .Include(x => x.Position)
                .Where(x =>
                    x.SeasonId == seasonId &&
                    x.NextTournamentRoster &&
                    (x.ClubId == clubId || x.LeagueId == leagueId) &&
                    x.TeamId == teamId);

            //teamPlayers = club != null
            //    ? teamPlayers.Where(x => x.ClubId == clubId)
            //    : teamPlayers.Where(x => x.LeagueId == leagueId);

            teamPlayers = teamPlayers.OrderBy(x => x.User.FullName);

            exportHelper.AddHeader(club?.Name, league?.Name, teamName, teamOfficials);
            exportHelper.AddBody(teamPlayers.ToList());

            var fileName = $"RosterExport_{DateTime.Now.ToShortDateString()}.pdf";
            Response.AddHeader("content-disposition", $"inline;filename={fileName}");
            return File(exportHelper.GetDocumentContent(), "application/pdf");
        }

        [HttpPost]
        public ActionResult Update(int? shirtNum, string shirtSize, int? posId, int updateId, bool? isActive,
            decimal? managerRegistrationDiscount, decimal? managerParticipationDiscount, bool? noInsurancePayment,
            int teamId, int? leagueId, int? clubId, int seasonId, int? unionId,
            string comment, decimal? teamPlayerPaid, bool? toWaitingStatus, bool? nextTournamentRoster,
            bool? PreviousIsActive, DateTime? tenicardValidity, bool? isTennisCompetition)
        {
            var teamPlayer = playersRepo.GetTeamsPlayerById(updateId);
            var sectionAlias = secRepo.GetSectionByTeamId(teamPlayer?.TeamId ?? 0)?.Alias;
            //if (!sectionAlias.Equals(GamesAlias.NetBall) && shirtNum != null)
            //{
            //    if (playersRepo.ShirtNumberExists(teamId, shirtNum.Value, updateId, seasonId, leagueId, clubId))
            //    {
            //        HttpContext.Response.StatusCode = (int)HttpStatusCode.Ambiguous;
            //        var oldNum = playersRepo.GetTeamsPlayerById(updateId).ShirtNum;
            //        return Json(new { statusText = Messages.ShirtAlreadyExists, Id = updateId, oldShirtNum = oldNum },
            //            JsonRequestBehavior.DenyGet);
            //    }
            //}

            teamPlayer.NextTournamentRoster = nextTournamentRoster ?? false;
            teamPlayer.ShirtNum = shirtNum ?? 0;
            teamPlayer.PosId = posId;

            var role = usersRepo.GetTopLevelJob(AdminId);
            //var union = db.Unions.FirstOrDefault(u => u.UnionId == unionId);
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId);
            var union = club?.Union;
            var BlockApprovements = false;
            if (union != null)
            {
                BlockApprovements = union?.IsClubsBlocked == true &&
                                    (role?.Equals(JobRole.TeamManager) == true ||
                                     role?.Equals(JobRole.ClubSecretary) == true ||
                                     role?.Equals(JobRole.ClubManager) == true);
            }

            var canChangeStatus = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager) ||
                                  !BlockApprovements;
            if (canChangeStatus)
            {
                isActive = (isActive.HasValue && isActive.Value) || Request.Form["IsActive"]?.ToLower() != null;
            }
            else
            {
                isActive = PreviousIsActive;
            }

            teamPlayer.IsActive = isActive == true;

            if (teamPlayer.IsActive && teamPlayer.IsApprovedByManager == false && toWaitingStatus == true)
            {
                teamPlayer.IsApprovedByManager = null;
                teamPlayer.ApprovalDate = null;
            }

            teamPlayer.Comment = comment;
            if (teamPlayerPaid.HasValue && teamPlayer.Paid != teamPlayerPaid.Value)
            {
                teamPlayer.Paid = teamPlayerPaid.Value;
            }

            if (managerRegistrationDiscount.HasValue)
            {
                var managerDiscount =
                    teamPlayer.User.PlayerDiscounts.FirstOrDefault(x => x.TeamId == teamId &&
                                                                        x.LeagueId == leagueId &&
                                                                        x.ClubId == clubId &&
                                                                        x.SeasonId == seasonId &&
                                                                        x.DiscountType == (int) PlayerDiscountTypes
                                                                            .ManagerRegistrationDiscount);

                if (managerDiscount != null)
                {
                    managerDiscount.Amount = managerRegistrationDiscount.Value;
                    managerDiscount.UpdateUserId = Convert.ToInt32(User.Identity.Name);
                    managerDiscount.DateUpdated = DateTime.Now;
                }
                else
                {
                    teamPlayer.User.PlayerDiscounts.Add(new PlayerDiscount
                    {
                        PlayerId = teamPlayer.UserId,
                        TeamId = teamId,
                        LeagueId = leagueId,
                        ClubId = clubId,
                        SeasonId = seasonId,
                        DiscountType = (int) PlayerDiscountTypes.ManagerRegistrationDiscount,
                        Amount = managerRegistrationDiscount.Value,
                        UpdateUserId = Convert.ToInt32(User.Identity.Name),
                        DateUpdated = DateTime.Now
                    });
                }
            }

            if (managerParticipationDiscount.HasValue)
            {
                var managerDiscount =
                    teamPlayer.User.PlayerDiscounts.FirstOrDefault(x => x.TeamId == teamId &&
                                                                        x.LeagueId == leagueId &&
                                                                        x.ClubId == clubId &&
                                                                        x.SeasonId == seasonId &&
                                                                        x.DiscountType == (int) PlayerDiscountTypes
                                                                            .ManagerParticipationDiscount);

                if (managerDiscount != null)
                {
                    managerDiscount.Amount = managerParticipationDiscount.Value;
                    managerDiscount.UpdateUserId = Convert.ToInt32(User.Identity.Name);
                    managerDiscount.DateUpdated = DateTime.Now;
                }
                else
                {
                    teamPlayer.User.PlayerDiscounts.Add(new PlayerDiscount
                    {
                        PlayerId = teamPlayer.UserId,
                        TeamId = teamId,
                        LeagueId = leagueId,
                        ClubId = clubId,
                        SeasonId = seasonId,
                        DiscountType = (int) PlayerDiscountTypes.ManagerParticipationDiscount,
                        Amount = managerParticipationDiscount.Value,
                        UpdateUserId = Convert.ToInt32(User.Identity.Name),
                        DateUpdated = DateTime.Now
                    });
                }
            }

            var user = usersRepo.GetById(teamPlayer.UserId);
            user.ShirtSize = shirtSize;
            user.NoInsurancePayment = noInsurancePayment == true;
            user.TenicardValidity = tenicardValidity ?? user.TenicardValidity;

            playersRepo.Save();
            usersRepo.Save();

            TempData["SavedId"] = updateId;

            playersRepo.UpdateUserActive(teamPlayer.UserId);

            var tennisCompetitionAsNumeric = isTennisCompetition == true ? 1 : 0;

            if (leagueId.HasValue)
            {
                return RedirectToAction("List", new { id = teamId, seasonId, currentLeagueId = leagueId, unionId, isTennisCompetition = tennisCompetitionAsNumeric });
            }

            return RedirectToAction("List", new { id = teamId, seasonId, unionId, isTennisCompetition = tennisCompetitionAsNumeric });
        }

        [HttpPost]
        public ActionResult Search(string term, int teamId, bool isPassport = false,
            int? departmentSportId = null, int? clubId = null)
        {
            var sectionId = departmentSportId ?? secRepo.GetSectionByTeamId(teamId)?.SectionId
                            ?? secRepo.GetSectionByClubId(clubId ?? 0)?.SectionId
                            ?? 0;

            var resList = !isPassport
                ? usersRepo.SearchUserByIdent(sectionId, null, AppRole.Players, term, 8)
                : usersRepo.SearchUserByPassport(sectionId, null, AppRole.Players, term, 8);

            return Json(resList);
        }

        public ActionResult CreatePlayer(int? leagueId, int? clubId, int teamId, int seasonId, int? departmentSportId)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("he-IL");
            var vm = new TeamPlayerForm
            {
                ClubId = clubId,
                LeagueId = leagueId,
                Genders = new SelectList(playersRepo.GetGenders(), "GenderId", "Title"),
                IsActive = true,
                TeamId = teamId,
                SeasonId = seasonId,
                Positions = new SelectList(posRepo.GetByTeam(teamId), "PosId", "Title"),
                UnionDisciplines = new List<Discipline>(),
                DepartmentSportId = departmentSportId
            };
            var team = teamRepo.GetById(teamId, seasonId);
            //if (team.TeamRegistrations.Any(r => r.SeasonId == seasonId && r.League.EndRegistrationDate > DateTime.Now))
            //{
            //    ViewBag.ErrorMessage = Messages.CannotAddPlayerToTennisTeam;
            //    return PartialView("_Error");
            //}

            Union union = null;

            if (leagueId != null)
            {
                var league = leagueRepo.GetById(leagueId.Value);
                if (league.Union != null)
                {
                    if (league.UnionId.HasValue)
                    {
                        union = unionsRepo.GetById(league.UnionId.Value);

                        if (union.Disciplines != null)
                        {
                            vm.IsSectionTeam = true;
                            vm.UnionDisciplines = union.Disciplines;
                        }
                        else
                        {
                            vm.UnionDisciplines = new List<Discipline>();
                        }
                    }

                    vm.IsHadicapEnabled = (bool) league.Union?.IsHadicapEnabled;
                }
                else if (league.Club?.Union != null)
                {
                    union = unionsRepo.GetById(league.Club.UnionId.Value);

                    if (union.Disciplines != null && union.Disciplines.Count > 0)
                    {
                        vm.IsSectionTeam = true;
                        vm.UnionDisciplines = union.Disciplines;
                    }
                    else
                    {
                        vm.UnionDisciplines = new List<Discipline>();
                    }
                }
            }

            vm.IsGymnastic = union?.Section?.Alias == GamesAlias.Gymnastic;
            vm.IsAthletics = union?.Section?.Alias == GamesAlias.Athletics;
            vm.IsWaterpolo = union?.Section?.Alias == GamesAlias.WaterPolo;
            vm.IsNetBall = union?.Section?.Alias == GamesAlias.NetBall;
            vm.IsBicycle = union?.Section?.Alias == GamesAlias.Bicycle;
            vm.IsSwimming = union?.Section?.Alias == GamesAlias.Swimming;

            vm.IsMotorsport = string.Equals(union?.Section?.Alias, GamesAlias.Motorsport,
                StringComparison.CurrentCultureIgnoreCase);

            if (clubId != null && clubId > 0)
            {
                var club = clubsRepo.GetById(clubId.Value);
                if (club.UnionId.HasValue)
                {
                    union = unionsRepo.GetById(club.UnionId.Value);

                    if (union.Disciplines != null && union.Disciplines.Count > 0)
                    {
                        vm.IsSectionTeam = true;
                        var clubDisciplinesIds = club.ClubDisciplines.Where(x => x.SeasonId == seasonId)
                            .Select(x => x.DisciplineId);
                        var teamsWithDisciplines = club.TeamDisciplines.Where(x => x.SeasonId == seasonId)
                            .Select(x => x.DisciplineId);
                        vm.UnionDisciplines = union.Disciplines.Where(x =>
                            clubDisciplinesIds.Contains(x.DisciplineId) &&
                            teamsWithDisciplines.Contains(x.DisciplineId));
                    }

                    var season = seasonsRepository.GetById(seasonId);
                    if (season?.Union?.Section?.Alias == GamesAlias.Rowing)
                    {
                        vm.IsRowing = true;
                    }
                }
            }

            if (!vm.IsGymnastic)
            {
                vm.IsGymnastic = union?.Section?.Alias == GamesAlias.Gymnastic;
                vm.IsAthletics = union?.Section?.Alias == GamesAlias.Athletics;
                vm.IsWaterpolo = union?.Section?.Alias == GamesAlias.WaterPolo;
                vm.IsBicycle = union?.Section?.Alias == GamesAlias.Bicycle;
                vm.IsMotorsport = string.Equals(union?.Section?.Alias, GamesAlias.Motorsport,
                    StringComparison.CurrentCultureIgnoreCase);
                vm.IsSwimming = union?.Section?.Alias == GamesAlias.Swimming;
            }

            vm.IsUkraineGymnasticUnion = union?.UnionId == GlobVars.UkraineGymnasticUnionId;

            if (vm.IsMotorsport)
            {
                var vehicleService = new VehicleService(db);
                vm.DriverLicenceTypeList = vehicleService.GetDriversLicenseTypesList();
            }

            if ((vm.IsAthletics || vm.IsRowing || vm.IsWaterpolo || vm.IsBicycle || vm.IsUkraineGymnasticUnion || vm.IsSwimming) &&
                clubId > 0)
            {
                vm.ClubTeams = GetClubTeamsSelectList(clubId.Value, seasonId);
            }

            if(vm.IsBicycle)
            {                            
                vm.FriendshipsList = new SelectList(Enumerable.Empty<SelectListItem>());
                vm.FriendshipsTypeList = new SelectList(Enumerable.Empty<SelectListItem>());
                vm.RoadDisciplines = new SelectList(Enumerable.Empty<SelectListItem>());
                vm.MountainDisciplines = new SelectList(Enumerable.Empty<SelectListItem>());
                vm.InsuranceTypesList = UIHelpers.PopulateInsuranceTypeList(db.InsuranceTypes.ToList(), null);
            }

            vm.AlternativeId = union?.UnionId == GlobVars.UkraineGymnasticUnionId;
            
            return PartialView("_EditPlayer", vm);
        }

        public ActionResult GetFriendships(DateTime? birthDay, int seasonId, int? genderId)
        {
            var season = seasonsRepository.GetById(seasonId);

            var flist = new List<FriendshipsType>();
            if(birthDay.HasValue && genderId.HasValue)
            {
                genderId = genderId - 10; //This type of handling genderId I have found in this controller (?) 
                var age = Convert.ToInt32(season.Name) - birthDay.Value.Year;
                flist = friendshipTypesRepo.GetAllByUnionId(season.UnionId.Value);
                flist = flist.Where(x => x.CompetitionAges.Any(c => c.from_age <= age && c.to_age >= age && (c.gender == genderId || c.gender == 3))).ToList();
            }

            var result = new SelectList(flist, nameof(FriendshipsType.FriendshipsTypesId), nameof(FriendshipsType.Name)).ToList();
            result.Insert(0, (new SelectListItem { Text = Messages.Select, Value = null }));

            return Json(new { Data = result });

        }

        public ActionResult GetRoadHeatsByUserId(int? friendshipTypeId, int userId, int seasonId)
        {
            var user = usersRepo.GetById(userId);
            return GetRoadHeats(user.BirthDay, seasonId, user.GenderId + 10, friendshipTypeId);
        }

        public ActionResult GetRoadHeats(DateTime? birthDay, int seasonId, int? genderId, int? friendshipTypeId)
        {
            var season = seasonsRepository.GetById(seasonId);

            var roadList = new List<Discipline>();

            if (birthDay.HasValue && genderId.HasValue && friendshipTypeId.HasValue)
            {
                genderId = genderId - 10; //This type of handling genderId I have found in this controller (?) 
                var age = Convert.ToInt32(season.Name) - birthDay.Value.Year;
                roadList = disciplinesRepo.GetAllByUnionIdWithRoad(season.UnionId.Value);
                roadList = roadList.Where(x => x.CompetitionAges.Any(c => c.from_age <= age && c.to_age >= age && (c.gender == genderId || c.gender == 3) && c.FriendshipTypeId == friendshipTypeId)).ToList();
            }

            var result = new SelectList(roadList, nameof(Discipline.DisciplineId), nameof(Discipline.Name)).ToList();
            result.Insert(0, (new SelectListItem { Text = Messages.Select, Value = null }));

            return Json(new { Data = result });

        }

        public ActionResult GetMountainHeatsByUserId(int? friendshipTypeId, int userId, int seasonId)
        {
            var user = usersRepo.GetById(userId);
            return GetMountainHeats(user.BirthDay, seasonId, user.GenderId + 10, friendshipTypeId);
        }

        public ActionResult GetMountainHeats(DateTime? birthDay, int seasonId, int? genderId, int? friendshipTypeId)
        {
            var season = seasonsRepository.GetById(seasonId);

            var mountainList = new List<Discipline>();

            if (birthDay.HasValue && genderId.HasValue && friendshipTypeId.HasValue)
            {
                genderId = genderId - 10; //This type of handling genderId I have found in this controller (?) 
                var age = Convert.ToInt32(season.Name) - birthDay.Value.Year;
                mountainList = disciplinesRepo.GetAllByUnionIdWithMountain(season.UnionId.Value);
                mountainList = mountainList.Where(x => x.CompetitionAges.Any(c => c.from_age <= age && c.to_age >= age && (c.gender == genderId || c.gender == 3) && c.FriendshipTypeId == friendshipTypeId)).ToList();
            }
            var result = new SelectList(mountainList, nameof(Discipline.DisciplineId), nameof(Discipline.Name)).ToList();
            result.Insert(0, (new SelectListItem { Text = Messages.Select, Value = null }));

            return Json(new { Data = result });

        }

        private List<SelectListItem> GetClubTeamsSelectList(int clubId, int seasonId)
        {
            var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
            var isGrouped = season.Union?.Section.IsIndividual == false;
            var clubTeams = db.ClubTeams
                .Include(x => x.Team)
                .Include(x => x.Team.TeamsDetails)
                .Where(x => x.SeasonId == seasonId && x.ClubId == clubId)
                .AsNoTracking()
                .ToList();

            var result = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "0",
                    Text = Messages.Select,
                    Disabled = true,
                    Selected = true
                }
            };

            result.AddRange(clubTeams.Select(x => new SelectListItem
            {
                Value = x.TeamId.ToString(),
                Text = $"{x.Team.TeamsDetails.FirstOrDefault(td => td.SeasonId == seasonId)?.TeamName ?? x.Team.Title}{(isGrouped ? ($" - {x.Team.LeagueTeams.FirstOrDefault(lt => lt.SeasonId == seasonId)?.Leagues?.Name ?? string.Empty}") : string.Empty)}"
            }).ToList());

            return result;
        }

        public ActionResult MovePlayerToTeam(int teamId, int? leagueId, int seasonId, int? clubId, int? unionId)
        {
            var vm = new MovePlayerForm();
            var userJob = usersRepo.GetTopLevelJob(AdminId);
            var union = unionId.HasValue ? unionsRepo.GetById(unionId.Value) : null;
            var isTennis = union?.Section?.Alias == GamesAlias.Tennis;
            var team = teamRepo.GetById(teamId);
            var clubTeam = team.ClubTeams.FirstOrDefault(tr =>
                tr.SeasonId == seasonId && !tr.IsTrainingTeam && tr.ClubId == clubId);

            if (User.IsInAnyRole(AppRole.Admins) || userJob == JobRole.UnionManager)
            {
                if (clubId.HasValue)
                {

                    var allTeams = teamRepo.GetTeamsByClubAndSeasonId(clubId.Value, seasonId, unionId);
                    var filteredTeams = new List<Team>();

                    foreach (var tteam in allTeams)
                    {
                        var club = tteam.ClubTeams.FirstOrDefault(ct => ct.SeasonId == seasonId)?.Club;
                        if (club == null || !club.IsUnionArchive)
                        {
                            filteredTeams.Add(tteam);
                        }
                    }

                    vm.Teams = filteredTeams
                        .Select(x => new TeamDto
                        {
                            ClubId = x.ClubTeams.FirstOrDefault(ct => ct.SeasonId == seasonId)?.ClubId ?? 0,
                            TeamId = x.TeamId,
                            Title = x.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId)?.TeamName ?? x.Title,
                            SchoolName = x.SchoolTeams.Where(st => st.TeamId == x.TeamId).FirstOrDefault()?.School?.Name,
                            LeagueId = unionId != null
                                ? x.LeagueTeams
                                      .FirstOrDefault(l => l.SeasonId == seasonId &&
                                                           l.Leagues.UnionId == unionId)
                                      ?.LeagueId ?? 0
                                : x.LeagueTeams
                                      .FirstOrDefault(l => l.SeasonId == seasonId &&
                                                           l.Leagues.UnionId ==
                                                           x.ClubTeams.FirstOrDefault(c => c.SeasonId == seasonId)
                                                               ?.Club?.UnionId)
                                      ?.LeagueId ?? 0,

                            LeagueName = unionId != null
                                ? x.LeagueTeams
                                    .FirstOrDefault(l => l.SeasonId == seasonId &&
                                                         l.Leagues.UnionId == unionId)
                                    ?.Leagues?.Name
                                : x.LeagueTeams
                                    .FirstOrDefault(l => l.SeasonId == seasonId &&
                                                         l.Leagues.UnionId ==
                                                         x.ClubTeams.FirstOrDefault(c => c.SeasonId == seasonId)
                                                             ?.Club?.UnionId)
                                    ?.Leagues?.Name,
                            ClubName = x.ClubTeams.FirstOrDefault(ct => ct.SeasonId == seasonId)?.Club?.Name ?? string.Empty
                        })
                        .ToList();
                    if (!isTennis || clubTeam == null)
                    {
                        vm.Teams.AddRange(teamRepo.GetSchoolTeamsByClubAndSeason(clubId.Value, seasonId)
                            .Where(x => x.TeamId != teamId)
                            .Select(x => new TeamDto
                            {
                                TeamId = x.TeamId, Title = x.Title,
                                SchoolName = x.SchoolTeams.Where(st => st.TeamId == x.TeamId).FirstOrDefault()?.School?.Name,
                                ClubId = x.SchoolTeams.FirstOrDefault(ct => ct.School.SeasonId == seasonId)?.School
                                             ?.ClubId ?? 0,
                                ClubName = x.ClubTeams.FirstOrDefault(ct => ct.SeasonId == seasonId)?.Club?.Name ?? string.Empty
                            }));
                    }
                }
                else
                {
                    vm.Teams = teamRepo.GetAllExceptCurrent(teamId, seasonId, unionId, false);
                }
            }
            else if (User.IsInAnyRole(AppRole.Workers))
            {
                switch (userJob)
                {
                    case JobRole.LeagueManager:
                        //League manager is able to move players/teams at the league that he is managing.
                        //except current team. No sense move player to team where he is presented at current time.
                        var teamsInLeague = leagueRepo.GetTeamsByManager(AdminId, teamId, seasonId, unionId);
                        vm.Teams = teamsInLeague.ToList();
                        break;
                    //Association(Union) manager is capable to move players/teams at the level of all leagues / association.
                    //case JobRole.UnionManager:
                    //    var teamsAllLeaguesAssociation = teamRepo.GetAllExceptCurrent(teamId, seasonId, unionId);
                    //    vm.Teams = teamsAllLeaguesAssociation;
                    //    break;
                    case JobRole.ClubSecretary:
                    case JobRole.ClubManager:
                        var teamsInClub = teamRepo.GetTeamsByClubSeasonIdExceptCurrent(teamId, clubId ?? 0, seasonId, unionId);
                        var teamsInOtherClubsButAsClubManagerToThem = teamRepo.GetTeamsByExceptClubBySeasonId(clubId ?? 0, seasonId, User.GetClubsUserManagingBySeason(seasonId), unionId);
                        vm.Teams = unionId == 31 ? teamsInClub.Union(teamsInOtherClubsButAsClubManagerToThem).ToList() : teamsInClub;
                        break;
                }
            }

            vm.Teams = vm.Teams.OrderBy(x => x.Title).ToList();

            vm.CurrentTeamId = teamId;
            vm.CurrentLeagueId = leagueId;
            vm.SeasonId = seasonId;
            vm.ClubId = clubId;
            vm.UnionId = unionId;
            vm.HasAccess = User.IsInAnyRole(AppRole.Admins, AppRole.Editors) ||
                           usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager;
            vm.IsIndividual = union?.Section?.IsIndividual;

            return PartialView("_MovePlayerToTeam", vm);
        }

        [HttpPost]
        public ActionResult DisplayErrorMessage(string message)
        {
            ViewBag.ErrorMessage = message;
            return PartialView("_Error");
        }

        [HttpPost]
        public ActionResult MovePlayerToTeam(MovePlayerForm model)
        {
            var parts = model.TeamLeagueClub.Split(new[] {"-"}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                return Content("League, team or club is not found");
            }

            int teamId;
            int leagueId;
            int clubId;
            if (!int.TryParse(parts[0], out teamId) || !int.TryParse(parts[1], out leagueId) ||
                !int.TryParse(parts[2], out clubId))
            {
                return Content("Unable to parse league, team or club");
            }

            var club = clubsRepo.GetById(clubId);
            var sectionAlias = club?.Union?.Section?.Alias ?? "";

            var isHighLevelUser = User.IsInRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager);


            var team = db.Teams
                .Include(x => x.TeamRegistrations)
                .Include(x => x.TeamsPlayers)
                .FirstOrDefault(x => x.TeamId == teamId);
            if (!model.IsExeptional)
            {
                if (team?.TeamRegistrations?.Any(r =>
                        r.SeasonId == model.SeasonId && !r.IsDeleted && r.League.EndRegistrationDate < DateTime.Now) ==
                    true)
                {
                    if (isHighLevelUser)
                    {
                        return JavaScript(
                            $"onConfirmAdmin(\"{Messages.CannotMovePlayerToTennisTeam}, {Messages.AreYouSureYouWantToProceed}\");");
                    }
                    else
                    {
                        return JavaScript($"onFailure(\"{Messages.CannotMovePlayerToTennisTeam}\");");
                    }
                }
            }

            if (model.Players?.Any(x => team?.TeamsPlayers.Any(tp =>
                                            tp.UserId == x &&
                                            tp.TeamId == teamId &&
                                            (leagueId == 0 || tp.LeagueId == leagueId) &&
                                            (leagueId > 0 ? tp.ClubId == null : (clubId == 0 || tp.ClubId == clubId)) &&
                                            tp.SeasonId == model.SeasonId) == true) == true)
            {
                return JavaScript($"onFailure(\"{Messages.MovePlayerToTeam_AlreadyInTeam}\");");
            }

            if (!model.IsExeptional && sectionAlias.Equals(GamesAlias.Tennis))
            {
                var teamReg = db.TeamRegistrations.FirstOrDefault(t =>
                    !t.IsDeleted && t.ClubId == clubId && t.TeamId == teamId && !t.League.IsArchive);
                if (teamReg != null)
                {
                    foreach (var playerId in model.Players)
                    {
                        var player = playersRepo.GetUserByUserId(playerId);
                        var genderId = player.GenderId ?? 0;
                        var birthday = player.BirthDay;
                        var isLeagueConditionValid =
                            CheckLeagueConditionForTheTennis(teamReg, genderId, birthday.Value);
                        if (!isLeagueConditionValid)
                        {
                            if (isHighLevelUser)
                            {
                                return JavaScript(
                                    $"onConfirmAdmin(\"{Messages.ErrorTennisAddNewPlayer}, {Messages.AreYouSureYouWantToProceed}\");");
                            }
                            else
                            {
                                return JavaScript($"onFailure(\"{Messages.ErrorTennisAddNewPlayer}\");");
                            }
                        }
                    }
                }
            }

            if (string.Equals(sectionAlias, SectionAliases.Gymnastic, StringComparison.CurrentCultureIgnoreCase) &&
                !model.IgnoreDifferentClubs &&
                model.ClubId > 0 &&
                model.Players?.Any() == true)
            {
                var currentClubPlayers = db.TeamsPlayers
                    .Include(x => x.User)
                    .Where(x => x.SeasonId == model.SeasonId &&
                                x.ClubId == model.ClubId)
                    .ToList();

                var playersToBeInDifferentClubs = new List<User>();
                foreach (var player in model.Players)
                {
                    var anotherTeamOfPlayer =
                        currentClubPlayers.FirstOrDefault(x => x.UserId == player && x.TeamId != model.CurrentTeamId);

                    if (anotherTeamOfPlayer != null)
                    {
                        playersToBeInDifferentClubs.Add(anotherTeamOfPlayer.User);
                    }
                }

                if (playersToBeInDifferentClubs.Any())
                {
                    var playersNames = string.Join(", ", playersToBeInDifferentClubs.Select(x => x.FullName));

                    return JavaScript(
                        $"ConfirmMovingToDifferentClub(\"{string.Format(Messages.ConfirmPlayersToBeInDifferentClubs, playersNames)}\");");
                }
            }

            playersRepo.MovePlayersToTeam(teamId, leagueId, clubId,
                model.Players,
                model.CurrentTeamId,
                model.SeasonId,
                model.CurrentLeagueId ?? 0,
                model.ClubId ?? 0,
                AdminId,
                model.CopyPlayers);

            if (model.IsBlockade)
            {
                playersRepo.BlockadePlayer(model.Players, model.BlockadeEndDate, model.SeasonId, AdminId);
            }

            return RedirectToAction("List",
                new
                {
                    id = model.CurrentTeamId, seasonId = model.SeasonId, currentLeagueId = model.CurrentLeagueId,
                    model.ClubId, model.UnionId
                });
        }

        [HttpPost]
        public ActionResult CreatePlayer(TeamPlayerForm model)
        {
            var season = seasonsRepository.GetById(model.SeasonId);       
            var isFromTeamPage = model.TeamId != 0;
            if ((model.IsAthletics || model.IsRowing || model.IsWaterpolo || model.IsBicycle || model.IsUkraineGymnasticUnion ||
                 model.IsSwimming) && model.TeamId <= 0)
            {
                model.ClubTeams = GetClubTeamsSelectList(model.ClubId ?? 0, model.SeasonId);
                if (model.ClubTeamId <= 0 || !model.ClubTeamId.HasValue)
                {
                    ModelState.AddModelError(nameof(model.ClubTeamId), Messages.FieldIsRequired);
                }
                else
                {
                    model.TeamId = model.ClubTeamId.Value;
                }
            }

            if ((model.IsAthletics || model.IsRowing || model.IsBicycle || model.IsWaterpolo || model.IsUkraineGymnasticUnion ||
                 model.IsSwimming) && model.ClubId > 0)
            {
                model.ClubTeams = GetClubTeamsSelectList(model.ClubId.Value, model.SeasonId);
            }

            var section = secRepo.GetSectionByTeamId(model.TeamId);
            var isGroupedUnion = section?.IsIndividual == null || section?.IsIndividual == false;
            var sectionAlias = section?.Alias ?? string.Empty;
            if (model.IsBicycle)
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    ModelState.AddModelError("Email", Messages.FieldIsRequired);
                }
                //if (model.BirthDay == null) ModelState.Remove("BirthDay");
                //if (!ModelState.IsValid) ModelState.Remove("GenderId");
                //if email exist moved after union is loaded and before modelstate.isvalid

                //if (string.IsNullOrEmpty(model.ForeignFirstName))
                //{
                //    ModelState.AddModelError("ForeignFirstName", Messages.FieldIsRequired);
                //}

                //if (string.IsNullOrEmpty(model.ForeignLastName))
                //{
                //    ModelState.AddModelError("ForeignLastName", Messages.FieldIsRequired);
                //}

                if (!model.FriendshipTypeId.HasValue || model.FriendshipTypeId == -1)
                {
                    ModelState.AddModelError("FriendshipTypeId", Messages.FieldIsRequired);
                }
                if (!model.FriendshipPriceType.HasValue || model.FriendshipPriceType == -1)
                {
                    ModelState.AddModelError("FriendshipPriceType", Messages.FieldIsRequired);
                }
                //if (!model.RoadDisciplineId.HasValue || model.RoadDisciplineId == -1)
                //{
                //    ModelState.AddModelError("RoadDisciplineId", Messages.FieldIsRequired);
                //}
                //if (!model.MountaintDisciplineId.HasValue || model.MountaintDisciplineId == -1)
                //{
                //    ModelState.AddModelError("MountaintDisciplineId", Messages.FieldIsRequired);
                //}
                //if (!model.InsuranceTypeId.HasValue)
                //{
                //    ModelState.AddModelError("InsuranceTypeId", Messages.FieldIsRequired);
                //}
                //else
                //{
                //    if (model.InsuranceTypeId == 5)
                //    {
                //        var fcheck = Request.Files.Get("InsuranceFile");
                //        if (fcheck == null || fcheck.ContentLength == 0)
                //        {

                //            ModelState.AddModelError("InsuranceFile", Messages.FieldIsRequired);                          

                //        }

                //    }
                //}
            }

            User user = null;
            if (string.Equals(model.IdType, "id", StringComparison.CurrentCultureIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(model.IdentNum))
                {
                    ModelState.AddModelError(nameof(model.IdentNum), Messages.PropertyValueRequired);
                }
                else
                {
                    var needCheckIdValidity = season?.Union?.EnableIDCorrectionCheck ?? false;
                    if (needCheckIdValidity && !CheckIDValidity(model.IdentNum))
                    {
                        ModelState.AddModelError(nameof(model.IdentNum), Messages.Invalid);
                    }
                    user = playersRepo.GetUserByIdentNum(model.IdentNum);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.PassportNum))
                {
                    ModelState.AddModelError(nameof(model.PassportNum), Messages.PropertyValueRequired);
                }
                else
                {
                    user = playersRepo.GetUserByPassportNum(model.PassportNum);
                }
            }

            if (model.IsAthletics)
            {
                if (!model.Insurance)
                {
                    if (!User.IsInAnyRole(AppRole.Admins) && usersRepo.GetTopLevelJob(AdminId) != JobRole.UnionManager)
                    {
                        ModelState.AddModelError("Insurance", Messages.Insurance_Required);
                    }
                }
            }

            var isWaterpolo = string.Equals(sectionAlias, GamesAlias.WaterPolo,
                StringComparison.CurrentCultureIgnoreCase);
            var isRugby = string.Equals(sectionAlias, GamesAlias.Rugby,
                StringComparison.CurrentCultureIgnoreCase);

            var isTennis = string.Equals(sectionAlias, GamesAlias.Tennis,
                StringComparison.CurrentCultureIgnoreCase);
            var isAdmin = User.IsInAnyRole(AppRole.Admins);
            var isUnionManager = User.HasTopLevelJob(JobRole.UnionManager);
            var isClubManager = (User.HasTopLevelJob(JobRole.ClubManager) || User.HasTopLevelJob(JobRole.ClubSecretary));

            if (user == null && (isRugby || (isTennis && !(isAdmin || isUnionManager || isClubManager))))
            {
                ModelState.AddModelError(model.IdentNum == null
                        ? nameof(model.PassportNum)
                        : nameof(model.IdentNum),
                    Messages.TeamPlayers_AddNewPlayer_NotAllowed);
            }

            if (isWaterpolo && user != null)
            {
                var isValid = true;
                League newLeague = null;
                if (model.LeagueId == null)
                {
                    var team = teamRepo.GetById(model.TeamId);
                    newLeague = team.LeagueTeams.FirstOrDefault(tl => tl.SeasonId == model.SeasonId).Leagues;
                }
                else
                    newLeague = leagueRepo.GetById(model.LeagueId.Value);
                if (newLeague == null)
                {
                    isValid = false;
                }
                if (isValid)
                {
                    var currentSeasonId = getCurrentSeason().Id;
                    foreach (var teamPlayer in playersRepo.GetCollection<TeamsPlayer>(x =>
                        x.UserId == user.UserId && x.SeasonId == currentSeasonId && x.TeamId != model.TeamId))
                    {
                        if (teamPlayer.LeagueId.HasValue)
                        {
                            var league = leagueRepo.GetById(teamPlayer.LeagueId.Value);
                            if (league.LeagueId != newLeague.LeagueId && newLeague.GenderId == 1 &&
                                newLeague.GenderId == league.GenderId && league.AgeId == newLeague.AgeId)
                            {
                                isValid = false;
                            }
                        }
                    }
                }
                if (!isValid && !model.IsExceptional)
                {
                    TempData["Error"] = "NeedCheckbox39";
                    return PartialView("_EditPlayer", model);
                }
            }

            if (model.IsAthletics)
            {
                if (!string.IsNullOrEmpty(model.AthleteNumber.ToString()) &&
                    playersRepo.IsAthleteNumberUsed(model.AthleteNumber, model.UserId, model.SeasonId))
                {
                    ModelState.AddModelError("AthleteNumber", Messages.AthleteNumberAlreadyExists);
                }

                if (model.ClubId > 0)
                {
                    model.ClubTeams = GetClubTeamsSelectList(model.ClubId.Value, model.SeasonId);
                }
            }

            if (string.IsNullOrEmpty(model.FirstName))
            {
                ModelState.AddModelError("FirstName", Messages.FieldIsRequired);
            }

            if (string.IsNullOrEmpty(model.LastName))
            {
                ModelState.AddModelError("LastName", Messages.FieldIsRequired);
            }

            //if (!string.IsNullOrEmpty(model.FirstName) && !string.IsNullOrEmpty(model.LastName)) {
            //    model.FullName = $"{model.FirstName} {model.LastName}";
            //}

            var isNetBall = string.Equals(sectionAlias, GamesAlias.NetBall, StringComparison.CurrentCultureIgnoreCase);
            if (!isNetBall)
            {

                if (model.GenderId == 0)
                {
                    if(!model.IsBicycle)                   
                    {
                        ModelState.AddModelError("GenderId", Messages.FieldIsRequired);
                    }
                    else
                    {
                        model.GenderId = -1;
                    }
                   
                }
                else
                {
                    model.GenderId = model.GenderId - 10;
                }
            }
            //added as fix for adding existing player in net ball - do not know why previous if is there, but didn't want to remove it             
            if(isNetBall && user != null)
            {
                model.GenderId = model.GenderId - 10;
            }


            Union union = null;

            model.Genders = new SelectList(playersRepo.GetGenders(), "GenderId", "Title");
            model.Positions = new SelectList(posRepo.GetByTeam(model.TeamId), "PosId", "Title");
            if (model.LeagueId != null)
            {
                var league = leagueRepo.GetById(model.LeagueId.Value);
                if (league.Union != null)
                {
                    if (league.UnionId.HasValue)
                    {
                        union = unionsRepo.GetById(league.UnionId.Value);

                        if (union.Disciplines != null && union.Disciplines.Count > 0)
                        {
                            model.IsSectionTeam = true;
                            model.UnionDisciplines = union.Disciplines;
                        }
                        else
                        {
                            model.UnionDisciplines = new List<Discipline>();
                        }
                    }
                }
                else if (league.Club?.Union != null)
                {
                    union = unionsRepo.GetById(league.Club.UnionId.Value);

                    if (union.Disciplines != null && union.Disciplines.Count > 0)
                    {
                        model.IsSectionTeam = true;
                        model.UnionDisciplines = union.Disciplines;
                    }
                    else
                    {
                        model.UnionDisciplines = new List<Discipline>();
                    }
                }
            }

            if (model.ClubId != null && model.ClubId > 0)
            {
                var club = clubsRepo.GetById(model.ClubId.Value);
                if (club.UnionId.HasValue)
                {
                    union = unionsRepo.GetById(club.UnionId.Value);

                    if (union.Disciplines != null && union.Disciplines.Count > 0)
                    {
                        model.IsSectionTeam = true;
                        //var clubDisciplinesIds = club.ClubDisciplines.Where(x => x.SeasonId == model.SeasonId).Select(x => x.DisciplineId);
                        //model.UnionDisciplines = union.Disciplines.Where(x => clubDisciplinesIds.Contains(x.DisciplineId));

                        var clubDisciplinesIds = club.ClubDisciplines.Where(x => x.SeasonId == model.SeasonId)
                            .Select(x => x.DisciplineId);
                        var teamsWithDisciplines = club.TeamDisciplines.Where(x => x.SeasonId == model.SeasonId)
                            .Select(x => x.DisciplineId);
                        model.UnionDisciplines = union.Disciplines.Where(x =>
                            clubDisciplinesIds.Contains(x.DisciplineId) &&
                            teamsWithDisciplines.Contains(x.DisciplineId));
                    }
                }
            }

            model.Is31Union = union?.UnionId == 31;

            model.PlayerDisciplineIds = model.DisciplinesIds ?? new List<int>();

            var clubValue = db.Clubs.FirstOrDefault(t => t.ClubId == model.ClubId);
            var sectionAliasClub = clubValue?.Section?.Alias ??
                                   clubValue?.Union?.Section?.Alias ?? union?.Section?.Alias ?? string.Empty;

            var genderId = model.GenderId;
            var birthday = model.BirthDay;
            if (user != null)
            {
                genderId = user.GenderId ?? genderId;
                birthday = user.BirthDay;
            }

            if (user != null)
            {
                if (playersRepo.PlayerExistsInTeam(model.TeamId, user.UserId, model.SeasonId, model.LeagueId,
                    model.ClubId))
                {
                    //Player exists in team
                    ModelState.AddModelError(model.IdentNum != null ? "IdentNum" : "PasswordNum",
                        Messages.PlayerAlreadyInTeam);
                }

                if (model.IsGymnastic && !model.IsUkraineGymnasticUnion)
                {
                    if (user.TeamsPlayers.Any(x => x.ClubId != model.ClubId && x.SeasonId == model.SeasonId) &&
                        !(User.HasTopLevelJob(JobRole.UnionManager) || User.IsInAnyRole(AppRole.Admins)))
                    {
                        ModelState.AddModelError(model.IdentNum != null ? "IdentNum" : "PasswordNum",
                            Messages.Club_Gymnastics_PlayerFromAnotherClub);
                    }

                    if (model.DisciplinesIds?.Any() == true)
                    {
                        var disciplinesTeams = teamRepo
                            .GetCollection<TeamDiscipline>(x => model.DisciplinesIds.Contains(x.DisciplineId) &&
                                                                x.SeasonId == model.SeasonId &&
                                                                x.ClubId == model.ClubId)
                            .DistinctBy(x => x.TeamId)
                            .Select(x => x.TeamId)
                            .ToArray();

                        if (playersRepo.PlayerExistsInAnyTeam(disciplinesTeams, user.UserId, model.SeasonId,
                            model.LeagueId, model.ClubId))
                        {
                            ModelState.AddModelError(model.IdentNum != null ? "IdentNum" : "PasswordNum",
                                Messages.PlayerAlreadyInTeam);
                        }
                    }
                }

                //if (union?.UnionId == 31)
                //{
                var player =
                    playersRepo.GetTeamPlayerByIdentNumAndSeasonIdConnectedToClub(user.IdentNum, model.SeasonId);

                //if player exist
                if (player != null && model.ClubId != null &&
                    (union?.UnionId == 31 || (player.ClubId != model.ClubId && model.SeasonId == player.SeasonId)))
                {
                    var playerInCurrentTeam = playersRepo.GetTeamPlayerByIdentNumAndTeamId(user.IdentNum, model.TeamId);
                    if (playerInCurrentTeam == null)
                    {
                        var startPlaying = playersRepo.GetTeamPlayerWithStartPlaying(user.IdentNum, model.SeasonId)
                            ?.StartPlaying;
                        // check if player not young
                        if (union?.UnionId != 31 || ((DateTime.Today - birthday.GetValueOrDefault()).Days / 365.25m > 19
                                                     && (DateTime.Today - startPlaying.GetValueOrDefault()).Days / 365 >
                                                     3))
                        {
                            if (!model.IsExceptional)
                            {
                                TempData["Error"] = (User.IsInAnyRole(AppRole.Admins) ||
                                                     usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager)
                                    ? "NeedCheckbox31"
                                    : Messages.ErrorPlayerOnAnotherTeam;
                                return PartialView("_EditPlayer", model);
                            }
                        }
                    }
                }
                //}
                //if (union?.UnionId == 38)
                //{
                //    var playerInCurrentTeam = playersRepo.GetTeamPlayerByIdentNum(user.IdentNum);

                //    if (playerInCurrentTeam != null && playerInCurrentTeam.ClubId != frm.ClubId)
                //    {
                //        if (!frm.IsTennisExceptional)
                //        {
                //            TempData["Error"] = union?.UnionId == 38 && (User.IsInAnyRole(AppRole.Admins) || usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager)
                //                                ? "NeedCheckbox38"
                //                                : Messages.TennisRelatedAlert;
                //            return PartialView("_EditPlayer", frm);
                //        }
                //    }
                //}             
            }

            if (!model.IsGymnastic && ModelState.ContainsKey(nameof(model.DisciplinesIds)))
            {
                ModelState[nameof(model.DisciplinesIds)].Errors.Clear();
            }

            if (model.IsGymnastic && !model.IsUkraineGymnasticUnion &&
                (string.IsNullOrEmpty(model.RanksString) && string.IsNullOrEmpty(model.TeamRanksString) &&
                 string.IsNullOrEmpty(model.RoutesString) && string.IsNullOrEmpty(model.TeamRoutesString)))
            {
                ModelState.AddModelError("SelectRoute", Messages.SelectRoute);
            }
            else if (model.IsGymnastic && !model.IsUkraineGymnasticUnion)
            {
                if (model.DisciplinesIds == null)
                {
                    ModelState.AddModelError("SelectRoute", Messages.ChooseDiscipline);
                }
                else
                {
                    /*
                    var teamD = playersRepo.GetTeamRoutesByDisciplinesIds(model.DisciplinesIds);

                    var teamRoutesCount = teamD.SelectMany(p => p.Value).ToList().Count;

                    if (teamRoutesCount > 0 && (string.IsNullOrEmpty(model.TeamRanksString) ||
                        string.IsNullOrEmpty(model.TeamRoutesString)))
                    {
                        ModelState.AddModelError("SelectRoute", Messages.SelectRoute);
                    }
                    */
                }
            }

            if (!string.IsNullOrEmpty(model.RanksString) && !string.IsNullOrEmpty(model.RoutesString))
            {
                var routes = model.RoutesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse);
                var ranks = model.RanksString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse);

                if (routes.Count() != ranks.Count())
                {
                    ModelState.AddModelError("SelectRoute", Messages.SelectRankForEachRoute);
                }

                for (var i = 0; i < routes.Count(); i++)
                {
                    var routeId = routes.ElementAtOrDefault(i);
                    if (routeId != 0)
                    {
                        var rankId = ranks.ElementAtOrDefault(i);
                        if (rankId != 0 && birthday.HasValue)
                        {
                            var eMessage = string.Empty;
                            if (!ValidateAge_User(birthday.Value, rankId, routeId, out eMessage))
                            {
                                ModelState.AddModelError("SelectRoute", eMessage);
                                break;
                            }
                        }
                    }
                }
            }

            ModelState.Remove("FullName");
            if (ModelState.IsValid && !string.IsNullOrEmpty(model.TeamRanksString) &&
                !string.IsNullOrEmpty(model.TeamRoutesString))
            {
                var routes = model.TeamRoutesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse);
                var ranks = model.TeamRanksString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse);

                if (routes.Count() != ranks.Count())
                {
                    ModelState.AddModelError("SelectRoute", Messages.SelectRankForEachRoute);
                }

                for (var i = 0; i < routes.Count(); i++)
                {
                    var routeId = routes.ElementAtOrDefault(i);
                    if (routeId != 0)
                    {
                        var rankId = ranks.ElementAtOrDefault(i);
                        if (rankId != 0 && birthday.HasValue)
                        {
                            var eMessage = string.Empty;
                            if (!ValidateAge_Team(birthday.Value, model.TeamId, rankId, routeId, out eMessage))
                            {
                                ModelState.AddModelError("SelectRoute", eMessage);
                                break;
                            }
                        }
                    }
                }
            }

            //validate bicycle email and file
            var ParentStatementFile = Request.Files.Get("ParentStatementFile");
            var idFile = Request.Files.Get(PlayerFileType.IDFile.ToString());
            var imageFile = Request.Files.Get(PlayerFileType.PlayerImage.ToString());
            var medicalCertFile = Request.Files.Get("MedicalCertificateFile");
            if (model.IsBicycle)
            {
                //if (!string.IsNullOrEmpty(model.Email))
                //{
                //    if (!usersRepo.CheckEmailWithinTheSameUnion(model.Email, union.UnionId, model.SeasonId))
                //    {
                //        ModelState.AddModelError("Email", string.Format(Messages.EmailAlreadyExists, model.Email));
                //    }
                //}
                //if (ParentStatementFile == null || ParentStatementFile.ContentLength == 0)
                //{
                //    ModelState.AddModelError("ParentStatementFile", Messages.FieldIsRequired);
                //}
                if (idFile == null || idFile.ContentLength == 0)
                {
                    ModelState.AddModelError("IDFile", Messages.FieldIsRequired);
                }
                //if (imageFile == null || imageFile.ContentLength == 0)
                //{
                //    ModelState.AddModelError(PlayerFileType.PlayerImage.ToString(), Messages.FieldIsRequired);
                //}
                //if(medicalCertFile == null || medicalCertFile.ContentLength == 0)
                //{
                //    ModelState.AddModelError("MedicalCertificateFile", Messages.FieldIsRequired);
                //}
            }
            if(model.IsWaterpolo && user == null)
            {
                if (ParentStatementFile == null || ParentStatementFile.ContentLength == 0)
                {
                    ModelState.AddModelError("ParentStatementFile", Messages.FieldIsRequired);
                }
            }


            if (!ModelState.IsValid)
            {
                if (model.IsBicycle && model.BirthDay.HasValue)
                {
                    model = PopulateBicycleFields(model, union.UnionId, model.SeasonId);
                }
                if(model.IsBicycle)
                {
                    model.InsuranceTypesList = UIHelpers.PopulateInsuranceTypeList(db.InsuranceTypes.ToList(), null);
                }

                return PartialView("_EditPlayer", model);
            }

            if (sectionAliasClub.Equals(GamesAlias.Tennis))
            {
                var teamReg = db.TeamRegistrations.FirstOrDefault(t =>
                    !t.IsDeleted && t.ClubId == model.ClubId && t.TeamId == model.TeamId && !t.League.IsArchive);
                if (teamReg != null)
                {
                    var isLeagueConditionValid = CheckLeagueConditionForTheTennis(teamReg, genderId, birthday.Value);
                    if (!isAdmin && !isUnionManager && !isLeagueConditionValid)
                    {
                        TempData["Error"] = Messages.ErrorTennisAddNewPlayer;
                        return PartialView("_EditPlayer", model);
                    }

                    if (!isAdmin && !isUnionManager &&
                        (teamReg.League.EndRegistrationDate.HasValue &&
                         teamReg.League.EndRegistrationDate.Value < DateTime.Now ||
                         teamReg.League.StartRegistrationDate.HasValue &&
                         teamReg.League.StartRegistrationDate.Value > DateTime.Now))
                    {
                        TempData["Error"] = Messages.ErrorCantRegisterWhenItsNotRegisteringPeriod;
                        return PartialView("_EditPlayer", model);
                    }
                }
            }

            if (user == null)
            {
                //New User
                user = new User();
                UpdateModel(user);

                user.IdentNum = model.IdentNum;
                user.PassportNum = model.PassportNum;
                user.SeasonIdOfCreation = model.SeasonId;
                user.Password = Protector.Encrypt("123abc12");
                user.TypeId = 4; // player
                user.LicenseLevel = model.LicenseLevelId;
                user.GenderId = model.GenderId == -1 ? (int?)null : model.GenderId;

                user.PaymentForUciId = sectionAlias == SectionAliases.Bicycle ? model.PaymentForUciId : (bool?)null;
               
                usersRepo.Create(user);
            }
            else
            {
                if (model.BirthDay.HasValue)
                {
                    user.BirthDay = model.BirthDay;
                }

                user.GenderId = model.GenderId == -1 ? (int?)null : model.GenderId;

                if (season != null && season.Union?.Section?.Alias == GamesAlias.Athletics)
                {
                    var oldSeasonIdOfCreation = user.SeasonIdOfCreation;
                    if (oldSeasonIdOfCreation.HasValue)
                    {
                        var oldSeason = seasonsRepository.GetById(oldSeasonIdOfCreation.Value);
                        if (oldSeason.Union?.Section?.Alias != GamesAlias.Athletics)
                        {
                            user.SeasonIdOfCreation = model.SeasonId;
                            usersRepo.Save();
                        }
                    }
                    else
                    {
                        user.SeasonIdOfCreation = model.SeasonId;
                        usersRepo.Save();
                    }
                }
            }

            if (sectionAlias == SectionAliases.Rowing)
            {
                user.MedExamDate = model.MedExamDate;
            }

            if (model.DisciplinesIds != null)
            {
                var playerDisciplines = user.PlayerDisciplines
                    .Where(x => x.ClubId == model.ClubId && x.SeasonId == model.SeasonId).ToList();

                foreach (var disciplineId in model.DisciplinesIds)
                {
                    var discipline = playerDisciplines.FirstOrDefault(x => x.DisciplineId == disciplineId);

                    if (discipline == null)
                    {
                        user.PlayerDisciplines.Add(new PlayerDiscipline
                        {
                            SeasonId = model.SeasonId,
                            ClubId = model.ClubId ?? 0,
                            DisciplineId = disciplineId
                        });
                    }
                }

                foreach (var disciplineToRemove in playerDisciplines.Where(x =>
                    !model.DisciplinesIds.Contains(x.DisciplineId)))
                {
                    db.Entry(disciplineToRemove).State = EntityState.Deleted;
                }
            }
            else
            {
                if (model.IsGymnastic && !model.IsUkraineGymnasticUnion)
                {
                    playersRepo.SetDisciplinesForTheGymnastic(model.TeamId, model.ClubId, model.SeasonId, user);
                }
            }

            model.PlayerDisciplineIds = user.PlayerDisciplines
                .Where(x => x.ClubId == model.ClubId && x.SeasonId == model.SeasonId)
                .Select(x => x.DisciplineId);

            TeamsPlayer tennisTeamPlayer = null;

            var isNewPlayerInUnion = false;

            var currentUnionSeasons = season.Union
                ?.Seasons
                ?.Select(x => x.Id)
                ?.ToArray();

            if (currentUnionSeasons?.Any() == true)
            {
                isNewPlayerInUnion = !db.TeamsPlayers.Any(x => x.UserId == user.UserId &&
                                                              currentUnionSeasons.Contains(x.SeasonId ?? 0));
            }

            var handicapLevelInSeason = db.TeamsPlayers
                .Where(x => x.UserId == user.UserId &&
                            x.SeasonId == season.Id)
                .Max(x => (int?)x.HandicapLevel) ?? 0;

            if (model.DisciplinesIds != null)
            {
                //add to teams by disciplines
                var teams = teamRepo
                    .GetCollection<TeamDiscipline>(x => model.DisciplinesIds.Contains(x.DisciplineId) &&
                                                        x.SeasonId == model.SeasonId &&
                                                        x.ClubId == model.ClubId)
                    .DistinctBy(x => x.TeamId)
                    .ToList();

                //if (teams.Any() &&
                //    teams.All(x => playersRepo.PlayerExistsInTeam(x.TeamId, user.UserId, frm.SeasonId, frm.LeagueId,
                //        frm.ClubId)))
                //{
                //    //Player exists in team
                //    ModelState.AddModelError(frm.IdentNum != null ? "IdentNum" : "PasswordNum", Messages.PlayerAlreadyInTeam);

                //    return PartialView("_EditPlayer", frm);
                //}

                foreach (var team in teams)
                {
                    user.TeamsPlayers.Add(new TeamsPlayer
                    {
                        TeamId = team.TeamId,
                        UserId = user.UserId,
                        PosId = model.PosId,
                        ShirtNum = model.ShirtNum,
                        IsActive = model.IsActive,
                        SeasonId = model.SeasonId,
                        ClubId = model.ClubId,
                        IsNewPlayerInUnion = isNewPlayerInUnion,
                        HandicapLevel = handicapLevelInSeason
                    });
                }
            }
            else
            {
                //Tennis specific: if player is already approved, he should be added to a new team as approved also
                if (isTennis)
                {
                    tennisTeamPlayer = user.TeamsPlayers?.FirstOrDefault(x =>
                        x.SeasonId == model.SeasonId && x.IsApprovedByManager != null);
                    if (!teamRepo.IsInTrainingTeam(user.UserId, model.SeasonId))
                    {
                        var trainingTeam = teamRepo.GetTrainingTeam(model.ClubId, model.SeasonId);
                        if (trainingTeam != null && trainingTeam.TeamId != model.TeamId)
                        {
                            user.TeamsPlayers.Add(new TeamsPlayer
                            {
                                TeamId = trainingTeam.TeamId,
                                UserId = user.UserId,
                                PosId = model.PosId,
                                ShirtNum = model.ShirtNum,
                                IsActive = model.IsActive,
                                SeasonId = model.SeasonId,
                                LeagueId = model.LeagueId,
                                ClubId = model.ClubId,
                                IsApprovedByManager = tennisTeamPlayer?.IsApprovedByManager,
                                ApprovalDate = tennisTeamPlayer?.ApprovalDate,
                                IsExceptionalMoved = model.IsExceptional,
                                DateOfCreate = model.IsTennisExceptional ? (DateTime?) DateTime.Now : null,
                                IsNewPlayerInUnion = isNewPlayerInUnion,
                                HandicapLevel = handicapLevelInSeason
                            });
                        }
                    }
                }

                var addRegularTeamPlayer = true;
                if(isGroupedUnion && !isFromTeamPage && model.IsWaterpolo)
                {
                    addRegularTeamPlayer = false;
                }

                if (addRegularTeamPlayer)
                {
                    user.TeamsPlayers.Add(new TeamsPlayer
                    {
                        TeamId = model.TeamId,
                        UserId = user.UserId,
                        PosId = model.PosId,
                        ShirtNum = model.ShirtNum,
                        IsActive = model.IsActive,
                        SeasonId = model.SeasonId,
                        LeagueId = model.LeagueId,
                        ClubId = model.LeagueId != null ? null : model.ClubId,
                        IsApprovedByManager = tennisTeamPlayer?.IsApprovedByManager,
                        ApprovalDate = tennisTeamPlayer?.ApprovalDate,
                        IsExceptionalMoved = model.IsExceptional,
                        DateOfCreate = model.IsTennisExceptional ? (DateTime?)DateTime.Now : null,
                        FriendshipTypeId = model.IsBicycle ? (model.FriendshipTypeId == -1 ? null : model.FriendshipTypeId) : null,
                        FriendshipPriceType = model.IsBicycle ? (model.FriendshipPriceType == -1 ? null : model.FriendshipPriceType) : null,
                        RoadDisciplineId = model.IsBicycle ? (model.RoadDisciplineId == -1 ? null : model.RoadDisciplineId) : null,
                        MountaintDisciplineId = model.IsBicycle ? (model.MountaintDisciplineId == -1 ? null : model.MountaintDisciplineId) : null,
                        IsNewPlayerInUnion = isNewPlayerInUnion,
                        HandicapLevel = handicapLevelInSeason
                    });
                }

                if (isWaterpolo)
                {
                    foreach (var leagueId in leagueRepo.GetByTeamAndSeason(model.TeamId, getCurrentSeason().Id)
                        .Select(x => x.LeagueId))
                    {
                        if (leagueId != model.LeagueId)
                        {
                            user.TeamsPlayers.Add(new TeamsPlayer
                            {
                                TeamId = model.TeamId,
                                UserId = user.UserId,
                                PosId = model.PosId,
                                ShirtNum = model.ShirtNum,
                                IsActive = model.IsActive,
                                SeasonId = model.SeasonId,
                                LeagueId = leagueId,
                                ClubId = null,
                                IsApprovedByManager = tennisTeamPlayer?.IsApprovedByManager,
                                ApprovalDate = tennisTeamPlayer?.ApprovalDate,
                                IsExceptionalMoved = model.IsExceptional,
                                DateOfCreate = model.IsTennisExceptional ? (DateTime?) DateTime.Now : null,
                                IsNewPlayerInUnion = isNewPlayerInUnion,
                                HandicapLevel = handicapLevelInSeason
                            });
                        }
                    }
                }
            }

            playersRepo.Save();

            if (!string.IsNullOrEmpty(model.RanksString) && !string.IsNullOrEmpty(model.RoutesString))
            {
                var routes = model.RoutesString.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse);
                var ranks = model.RanksString.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse);
                for (var i = 0; i < routes.Count(); i++)
                {
                    var routeId = routes.ElementAtOrDefault(i);
                    if (routeId != 0)
                    {
                        var rankId = ranks.ElementAtOrDefault(i);
                        if (rankId != 0)
                        {
                            playersRepo.UpdateUsersRoute(user.UserId, routeId, rankId, out var errorMessage);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(model.TeamRanksString) && !string.IsNullOrEmpty(model.TeamRoutesString))
            {
                var routes = model.TeamRoutesString.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse);
                var ranks = model.TeamRanksString.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse);
                for (var i = 0; i < routes.Count(); i++)
                {
                    var routeId = routes.ElementAtOrDefault(i);
                    if (routeId != 0)
                    {
                        var rankId = ranks.ElementAtOrDefault(i);
                        if (rankId != 0)
                        {
                            //playersRepo.UpdateUsersRoute(user.UserId, routeId, rankId, out string errorMessage);

                            if (model.DisciplinesIds != null)
                            {
                                //add to teams by disciplines
                                var teams = teamRepo
                                    .GetCollection<TeamDiscipline>(x => model.DisciplinesIds.Contains(x.DisciplineId) &&
                                                                        x.SeasonId == model.SeasonId &&
                                                                        x.ClubId == model.ClubId)
                                    .DistinctBy(x => x.TeamId)
                                    .ToList();
                                foreach (var t in teams)
                                {
                                    teamRepo.UpdateTeamsRoute(t.TeamId, user.UserId, routeId, rankId,
                                        out var errorMessage);
                                }
                            }
                            else
                            {
                                teamRepo.UpdateTeamsRoute(model.TeamId, user.UserId, routeId, rankId,
                                    out var errorMessage);
                            }
                        }
                    }
                }
            }

            var savePath = Server.MapPath(GlobVars.ContentPath + "/players/");
            var maxFileSize = GlobVars.MaxFileSize * 1000;

            if (imageFile != null)
            {
                if (imageFile.ContentLength > maxFileSize)
                {
                    TempData["Error"] = $"{PlayerFileType.PlayerImage}: {Messages.FileSizeError}";
                    return PartialView("_EditPlayer", model);
                }

                var fileName = FileUtil.SaveFile(imageFile, savePath, user.UserId.ToString(),
                    PlayerFileType.PlayerImage);

                if (fileName != null)
                {
                    var playerFile =
                        user.PlayerFiles.FirstOrDefault(x =>
                            x.SeasonId == model.SeasonId &&
                            x.FileType == (int) PlayerFileType.PlayerImage);
                    if (playerFile != null)
                    {
                        db.Entry(playerFile).State = EntityState.Deleted;
                    }

                    user.PlayerFiles.Add(new PlayerFile
                    {
                        SeasonId = model.SeasonId,
                        DateCreated = DateTime.Now,
                        FileName = fileName,
                        FileType = (int) PlayerFileType.PlayerImage,
                        PlayerId = user.UserId
                    });
                }
            }

            
            if (idFile != null)
            {
                if (imageFile.ContentLength > maxFileSize)
                {
                    TempData["Error"] = $"{PlayerFileType.IDFile}: {Messages.FileSizeError}";
                    return PartialView("_EditPlayer", model);
                }

                var fileName = FileUtil.SaveFile(idFile, savePath, user.UserId.ToString(), PlayerFileType.IDFile);

                if (fileName != null)
                {
                    user.IDFile = fileName;
                }
            }
            
            if (medicalCertFile != null)
            {
                if (medicalCertFile.ContentLength > maxFileSize)
                {
                    TempData["Error"] = $"MedicalCertificateFile: {Messages.FileSizeError}";
                    return PartialView("_EditPlayer", model);
                }

                var fileName = FileUtil.SaveFile(medicalCertFile, savePath, user.UserId.ToString(),
                    PlayerFileType.MedicalCertificate);

                if (fileName != null)
                {
                    var playerFile =
                        user.PlayerFiles.FirstOrDefault(x =>
                            x.SeasonId == model.SeasonId &&
                            x.FileType == (int) PlayerFileType.MedicalCertificate && !x.IsArchive);
                    if (playerFile != null)
                    {
                        db.Entry(playerFile).State = EntityState.Deleted;
                    }

                    user.PlayerFiles.Add(new PlayerFile
                    {
                        SeasonId = model.SeasonId,
                        DateCreated = DateTime.Now,
                        FileName = fileName,
                        FileType = (int) PlayerFileType.MedicalCertificate,
                        PlayerId = user.UserId
                    });
                }
            }

            var driverLicenseFile = Request.Files.Get("DriverLicenseFile");
            if (driverLicenseFile != null)
            {
                if (driverLicenseFile.ContentLength > maxFileSize)
                {
                    TempData["Error"] = $"DriverLicenseFile: {Messages.FileSizeError}";
                    return PartialView("_EditPlayer", model);
                }

                var fileName = FileUtil.SaveFile(driverLicenseFile, savePath, user.UserId.ToString(),
                    PlayerFileType.DriverLicense);

                if (fileName != null)
                {
                    var playerFile =
                        user.PlayerFiles.FirstOrDefault(x =>
                            x.SeasonId == model.SeasonId &&
                            x.FileType == (int) PlayerFileType.DriverLicense);
                    if (playerFile != null)
                    {
                        db.Entry(playerFile).State = EntityState.Deleted;
                    }

                    user.PlayerFiles.Add(new PlayerFile
                    {
                        SeasonId = model.SeasonId,
                        DateCreated = DateTime.Now,
                        FileName = fileName,
                        FileType = (int) PlayerFileType.DriverLicense,
                        PlayerId = user.UserId
                    });
                }
            }

            
            if (ParentStatementFile != null)
            {
                if (ParentStatementFile.ContentLength > maxFileSize)
                {
                    TempData["Error"] = $"ParentStatementFile: {Messages.FileSizeError}";
                    return PartialView("_EditPlayer", model);
                }

                var fileName = FileUtil.SaveFile(ParentStatementFile, savePath, user.UserId.ToString(),
                    PlayerFileType.ParentStatement);

                if (fileName != null)
                {
                    var playerFile =
                        user.PlayerFiles.FirstOrDefault(x =>
                            x.SeasonId == model.SeasonId &&
                            x.FileType == (int) PlayerFileType.ParentStatement);
                    if (playerFile != null)
                    {
                        db.Entry(playerFile).State = EntityState.Deleted;
                    }

                    user.PlayerFiles.Add(new PlayerFile
                    {
                        SeasonId = model.SeasonId,
                        DateCreated = DateTime.Now,
                        FileName = fileName,
                        FileType = (int) PlayerFileType.ParentStatement,
                        PlayerId = user.UserId
                    });
                }
            }

            var specialClassficationFile = Request.Files.Get(PlayerFileType.SpecialClassificationFile.ToString());
            if (specialClassficationFile != null)
            {
                if (specialClassficationFile.ContentLength > maxFileSize)
                {
                    TempData["Error"] = $"SpecialClassificationFile: {Messages.FileSizeError}";
                    return PartialView("_EditPlayer", model);
                }

                var fileName = FileUtil.SaveFile(specialClassficationFile, savePath, user.UserId.ToString(),
                    PlayerFileType.SpecialClassificationFile);

                if (fileName != null)
                {
                    var playerFile =
                        user.PlayerFiles.FirstOrDefault(x =>
                            x.SeasonId == model.SeasonId &&
                            x.FileType == (int)PlayerFileType.SpecialClassificationFile);
                    if (playerFile != null)
                    {
                        db.Entry(playerFile).State = EntityState.Deleted;
                    }

                    user.PlayerFiles.Add(new PlayerFile
                    {
                        SeasonId = model.SeasonId,
                        DateCreated = DateTime.Now,
                        FileName = fileName,
                        FileType = (int)PlayerFileType.SpecialClassificationFile,
                        PlayerId = user.UserId
                    });
                }
            }

            var insuranceFile = Request.Files.Get("InsuranceFile");
            if (insuranceFile != null)
            {
                if (insuranceFile.ContentLength > maxFileSize)
                {
                    TempData["Error"] = $"SpecialClassificationFile: {Messages.FileSizeError}";
                    return PartialView("_EditPlayer", model);
                }

                var fileName = FileUtil.SaveFile(insuranceFile, savePath, user.UserId.ToString(),
                    PlayerFileType.Insurance);

                if (fileName != null)
                {
                    var playerFile =
                        user.PlayerFiles.FirstOrDefault(x =>
                            x.SeasonId == model.SeasonId &&
                            x.FileType == (int)PlayerFileType.Insurance);
                    if (playerFile != null)
                    {
                        db.Entry(playerFile).State = EntityState.Deleted;
                    }

                    user.PlayerFiles.Add(new PlayerFile
                    {
                        SeasonId = model.SeasonId,
                        DateCreated = DateTime.Now,
                        FileName = fileName,
                        FileType = (int)PlayerFileType.Insurance,
                        PlayerId = user.UserId
                    });
                }
            }

            usersRepo.Save();

            if(sectionAlias == SectionAliases.Bicycle)
            {
                model.InsuranceTypesList = UIHelpers.PopulateInsuranceTypeList(db.InsuranceTypes.ToList(), user.InsuranceTypeId);
            }

            TempData["Success"] = true;
            return PartialView("_EditPlayer", model);
        }

        private bool CheckIDValidity(string tz)
        {
            var tot = 0;
            var isNum = int.TryParse(tz, out _);
            if (tz.Length < 9 || !isNum)
            {
                return false;
            }
            for (var i = 0; i < 8; i++)
            {
                var x = ((i % 2) + 1) * int.Parse(tz.ToCharArray()[i].ToString());
                if (x > 9)
                {
                    var xStr = x.ToString();
                    x = int.Parse(xStr.ToCharArray()[0].ToString()) + int.Parse(xStr.ToCharArray()[1].ToString());
                }
                tot += x;
            }

            if ((tot + int.Parse(tz.ToCharArray()[8].ToString())) % 10 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ValidateAge_User(DateTime birthDate, int rankId, int routeId, out string message)
        {
            message = string.Empty;

            var rank = db.RouteRanks.FirstOrDefault(p => p.Id == rankId && p.RouteId == routeId);
            if (rank != null)
            {
                if ((rank.FromAge == null || (rank.FromAge != null && birthDate >= rank.FromAge.Value))
                    && (rank.ToAge == null || (rank.ToAge != null && birthDate <= rank.ToAge.Value)))
                {
                    return true;
                }
                else
                {
                    message = rank.Rank + ": " + Messages.PlayerRankValidationMessage;
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private bool ValidateAge_Team(DateTime birthDate, int teamId, int rankId, int routeId, out string message)
        {
            message = string.Empty;

            var rank = db.RouteTeamRanks.FirstOrDefault(p => p.Id == rankId && p.TeamRouteId == routeId);
            if (rank != null)
            {
                if ((rank.FromAge == null || (rank.FromAge != null && birthDate >= rank.FromAge.Value))
                    && (rank.ToAge == null || (rank.ToAge != null && birthDate <= rank.ToAge.Value)))
                {
                    return true;
                }
                else
                {
                    message = rank.Rank + ": " + Messages.PlayerRankValidationMessage;
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private bool CheckLeagueConditionForTheTennis(TeamRegistration teamReg, int genderId, DateTime birthdate)
        {
            var isValid = false;
            var leagueGenderId = teamReg.League.GenderId;
            var isMinBirthdayValid =
                !teamReg.League.MinimumAge.HasValue || teamReg.League.MinimumAge.Value >= birthdate;
            var isMaxBirthdayValid =
                !teamReg.League.MaximumAge.HasValue || teamReg.League.MaximumAge.Value <= birthdate;
            var isBirthdayValid = isMinBirthdayValid && isMaxBirthdayValid;

            var maxPlayers = teamReg.League.MaximumPlayersTeam;

            isValid = (leagueGenderId == 3 || genderId == leagueGenderId)
                      && (isBirthdayValid)
                      && (maxPlayers == null
                          || teamReg.Team.TeamsPlayers.Count(t => !t.User.IsArchive) < maxPlayers);

            return isValid;
        }

        [HttpPost]
        public ActionResult ExistingPlayer(TeamPlayerForm frm)
        {
            var user = playersRepo.GetUserByIdentNum(frm.IdentNum);
            var tp = playersRepo.GetTeamPlayerByIdentNumAndTeamId(frm.IdentNum, frm.TeamId);
            frm.Genders = new SelectList(playersRepo.GetGenders(), "GenderId", "Title");
            frm.Positions = new SelectList(posRepo.GetByTeam(frm.TeamId), "PosId", "Title");
            var season = seasonsRepository.GetById(frm.SeasonId);
            Union union = null;

            if (user == null)
            {
                ModelState.AddModelError("IdentNum", Messages.PlayerNotExists);
                return PartialView("_EditPlayerFormBody", frm);
            }


            frm.IsGymnastic = season.Union?.Section?.Alias == GamesAlias.Gymnastic;
            frm.IsAthletics = season.Union?.Section?.Alias == GamesAlias.Athletics;
            frm.IsWaterpolo = season.Union?.Section?.Alias == GamesAlias.WaterPolo;
            frm.IsMotorsport = string.Equals(season.Union?.Section?.Alias, GamesAlias.Motorsport,
                StringComparison.CurrentCultureIgnoreCase);
            frm.IsBicycle = season.Union?.Section?.Alias == GamesAlias.Bicycle;

            if (frm.ClubId != null)
            {
                var club = clubsRepo.GetById(frm.ClubId.Value);
                if (club.UnionId.HasValue)
                {
                    union = unionsRepo.GetById(club.UnionId.Value);

                    if (union.Disciplines != null && union.Disciplines.Count > 0)
                    {
                        frm.IsSectionTeam = true;
                        var clubDisciplinesIds = club.ClubDisciplines.Where(x => x.SeasonId == frm.SeasonId)
                            .Select(x => x.DisciplineId);
                        var teamsWithDisciplines = club.TeamDisciplines.Where(x => x.SeasonId == frm.SeasonId)
                            .Select(x => x.DisciplineId);
                        frm.UnionDisciplines = union.Disciplines.Where(x =>
                            clubDisciplinesIds.Contains(x.DisciplineId) &&
                            teamsWithDisciplines.Contains(x.DisciplineId));
                    }
                }
            }

            if (frm.IsMotorsport)
            {
                var vehicleService = new VehicleService(db);
                frm.DriverLicenceTypeList = vehicleService.GetDriversLicenseTypesList();
            }

            if ((frm.IsAthletics || frm.IsWaterpolo || frm.IsBicycle) && frm.ClubId > 0)
            {
                frm.ClubTeams = GetClubTeamsSelectList(frm.ClubId.Value, frm.SeasonId);
            }

            user.GenderId = user.GenderId + 10;
            frm.InjectFrom<NullableInjection>(user);
            if (tp != null)
            {
                frm.InjectFrom<NullableInjection>(tp);
            } 
            else
            {
                if(frm.IsBicycle)
                {
                    tp = user.TeamsPlayers.FirstOrDefault(x => x.SeasonId == frm.SeasonId);
                    if(tp!= null) frm.InjectFrom<NullableInjection>(tp);
                }
            }

            if (string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.FullName))
            {
                var ssize = user.FullName.Split(new char[0]);
                if (ssize.Length > 1)
                {
                    frm.FirstName = ssize[0];
                    for (var i = 1; i < ssize.Length; i++)
                    {
                        if (i > 1)
                        {
                            frm.LastName = frm.LastName + " ";
                        }

                        frm.LastName = frm.LastName + ssize[i];
                    }
                }
            }

            if ((frm.IsAthletics || frm.IsRowing || frm.IsBicycle || frm.IsWaterpolo || frm.IsUkraineGymnasticUnion || frm.IsSwimming) && frm.ClubId > 0)
            {
                frm.ClubTeams = GetClubTeamsSelectList(frm.ClubId.Value, frm.SeasonId);
            }

            if (frm.IsBicycle)
            {
                frm = PopulateBicycleFields(frm, union != null ? union.UnionId : season.UnionId, frm.SeasonId);
                frm.InsuranceTypesList = UIHelpers.PopulateInsuranceTypeList(db.InsuranceTypes.ToList(), null);
            }
            return PartialView("_EditPlayerFormBody", frm);
        }


        [HttpPost]
        public ActionResult ExistingPlayerByPassport(TeamPlayerForm frm)
        {
            var user = playersRepo.GetUserByPassportNum(frm.PassportNum);
            //var tp = playersRepo.GetTeamPlayerByPassportAndTeamId(frm.PassportNum, frm.TeamId);
            frm.Genders = new SelectList(playersRepo.GetGenders(), "GenderId", "Title");
            frm.Positions = new SelectList(posRepo.GetByTeam(frm.TeamId), "PosId", "Title");
            var season = seasonsRepository.GetById(frm.SeasonId);
            Union union = null;

            if (user == null)
            {
                ModelState.AddModelError("PassportNum", Messages.PlayerNotExists);
                return PartialView("_EditPlayerFormBody", frm);
            }

            frm.IsGymnastic = season.Union?.Section?.Alias == GamesAlias.Gymnastic;
            frm.IsAthletics = season.Union?.Section?.Alias == GamesAlias.Athletics;
            frm.IsWaterpolo = season.Union?.Section?.Alias == GamesAlias.WaterPolo;
            frm.IsMotorsport = string.Equals(season.Union?.Section?.Alias, GamesAlias.Motorsport,
                StringComparison.CurrentCultureIgnoreCase);
            frm.IsBicycle = season.Union?.Section?.Alias == GamesAlias.Bicycle;

            if (frm.ClubId != null)
            {
                var club = clubsRepo.GetById(frm.ClubId.Value);
                if (club.UnionId.HasValue)
                {
                    union = unionsRepo.GetById(club.UnionId.Value);

                    if (union.Disciplines != null && union.Disciplines.Count > 0)
                    {
                        frm.IsSectionTeam = true;
                        var clubDisciplinesIds = club.ClubDisciplines.Where(x => x.SeasonId == frm.SeasonId)
                            .Select(x => x.DisciplineId);
                        var teamsWithDisciplines = club.TeamDisciplines.Where(x => x.SeasonId == frm.SeasonId)
                            .Select(x => x.DisciplineId);
                        frm.UnionDisciplines = union.Disciplines.Where(x =>
                            clubDisciplinesIds.Contains(x.DisciplineId) &&
                            teamsWithDisciplines.Contains(x.DisciplineId));
                    }
                }
            }

            if (frm.IsMotorsport)
            {
                var vehicleService = new VehicleService(db);
                frm.DriverLicenceTypeList = vehicleService.GetDriversLicenseTypesList();
            }

            if ((frm.IsAthletics || frm.IsWaterpolo || frm.IsBicycle) && frm.ClubId > 0)
            {
                frm.ClubTeams = GetClubTeamsSelectList(frm.ClubId.Value, frm.SeasonId);
            }

            user.GenderId = user.GenderId + 10;
            frm.InjectFrom<NullableInjection>(user);
            if (frm.IsBicycle)
            {
                frm = PopulateBicycleFields(frm, union != null ? union.UnionId : season.UnionId, frm.SeasonId);
                frm.InsuranceTypesList = UIHelpers.PopulateInsuranceTypeList(db.InsuranceTypes.ToList(), null);
            }
            return PartialView("_EditPlayerFormBody", frm);
        }

        [HttpPost]
        public JsonResult CreateTeam(int? leagueId, int? clubId, int seasonId, TeamDto model)
        {
            var team = new Team {Title = model.Title};

            teamRepo.Create(team);
            if (leagueId.HasValue)
            {
                var league = leagueRepo.GetByLeagueSeasonId(leagueId.Value, seasonId);
                var leagueTeam = new LeagueTeams
                {
                    TeamId = team.TeamId,
                    LeagueId = leagueId.Value,
                    SeasonId = seasonId
                };

                league.LeagueTeams.Add(leagueTeam);
                leagueRepo.Save();
            }
            else if (clubId.HasValue)
            {
                var club = clubsRepo.GetById(clubId.Value);
                if (club != null)
                {
                    club.ClubTeams.Add(new ClubTeam
                    {
                        ClubId = clubId.Value,
                        SeasonId = seasonId,
                        TeamId = team.TeamId
                    });

                    clubsRepo.Save();
                }
            }

            var data = new TeamDto
            {
                Title = team.Title,
                TeamId = team.TeamId
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SendNotification(int id, int teamId, int? unionId, int? clubId, int seasonId)
        {
            var hasPermission = User.IsInAnyRole(AppRole.Admins) ||
                                usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager;
            GetEmailsForCurrentTeam(teamId, seasonId, unionId, clubId, out var unionEmail, out var clubEmail);

            if (!hasPermission)
                unionEmail = string.Empty;
            else
                clubEmail = string.Empty;

            var model = new NotificationsForm
            {
                EntityId = id,
                RelevantEntityLogicalName = LogicaName.Player,
                SeasonId = seasonId,
                ClubEmail = clubEmail,
                UnionEmail = unionEmail,
                IsUnionManager = hasPermission,
                TeamId = teamId
            };
            return PartialView("_Notification", model);
        }

        private TeamPlayerForm PopulateBicycleFields(TeamPlayerForm model, int? unionId, int seasonId)
        {
            if (!model.BirthDay.HasValue)
            {
                return model;
            }

            var season = seasonsRepository.GetById(model.SeasonId);
            var pAge = Convert.ToInt32(season.Name) - model.BirthDay.Value.Year;

            var genderId = model.GenderId > 9 ? model.GenderId - 10 : model.GenderId;

            var flist = friendshipTypesRepo.GetAllByUnionId(unionId.Value).Where(x => x.CompetitionAges.Any(c => c.from_age <= pAge && c.to_age >= pAge && (c.gender == genderId || c.gender == 3))).ToList();
            model.FriendshipsList = new SelectList(flist, nameof(FriendshipsType.FriendshipsTypesId), nameof(FriendshipsType.Name), model.FriendshipTypeId);

            if (model.FriendshipTypeId.HasValue && model.FriendshipTypeId != -1)
            {
                var ftlist = CustomPriceHelper.GetFriendshipPriceTypesSelectList(model.FriendshipPriceType, flist.FirstOrDefault(x => x.FriendshipsTypesId == model.FriendshipTypeId)?.FriendshipPrices.Where(x => x.FromAge <= pAge && x.ToAge >= pAge && (x.GenderId == genderId || x.GenderId == 3)).Select(x => x.FriendshipPriceType).ToList());
                ftlist.RemoveAt(0);
                model.FriendshipsTypeList = new SelectList(ftlist, nameof(SelectListItem.Value), nameof(SelectListItem.Text), model.FriendshipPriceType);
            }
            else
            {
                var ftlist = CustomPriceHelper.GetFriendshipPriceTypesSelectList(null, new List<int?>());
                ftlist.RemoveAt(0);
                model.FriendshipsTypeList = new SelectList(ftlist, nameof(SelectListItem.Value), nameof(SelectListItem.Text), model.FriendshipPriceType);
            }

            var roadList = disciplinesRepo.GetAllByUnionIdWithRoad(unionId.Value);
            roadList = roadList.Where(x => x.CompetitionAges.Any(c => c.from_age <= pAge && c.to_age >= pAge && (c.gender == genderId || c.gender == 3) && c.FriendshipTypeId == model.FriendshipTypeId)).ToList();
            model.RoadDisciplines = new SelectList(roadList, nameof(Discipline.DisciplineId), nameof(Discipline.Name), model.RoadDisciplineId);

            var mountainList = disciplinesRepo.GetAllByUnionIdWithMountain(unionId.Value);
            mountainList = mountainList.Where(x => x.CompetitionAges.Any(c => c.from_age <= pAge && c.to_age >= pAge && (c.gender == genderId || c.gender == 3) && c.FriendshipTypeId == model.FriendshipTypeId)).ToList();
            model.MountainDisciplines = new SelectList(mountainList, nameof(Discipline.DisciplineId), nameof(Discipline.Name), model.MountaintDisciplineId);

            return model;
        }

        private void GetEmailsForCurrentTeam(int id, int seasonId, int? unionId, int? clubId, out string unionEmail,
            out string clubEmail)
        {
            unionEmail = string.Empty;
            clubEmail = string.Empty;

            var team = teamRepo.GetById(id, seasonId);

            if (team != null && !unionId.HasValue && !clubId.HasValue)
            {
                var league = team.LeagueTeams.LastOrDefault()?.Leagues;
                var club = team.ClubTeams.LastOrDefault()?.Club;
                unionEmail = league?.Union?.Email ?? league?.Club?.Union?.Email ?? string.Empty;
                clubEmail = club?.Email ?? league?.Club?.Email ?? string.Empty;
            }

            if (unionId.HasValue && unionId != 0)
            {
                var union = unionsRepo.GetById(unionId.Value);
                if (union != null)
                {
                    unionEmail = union.Email;
                }
            }

            if (clubId.HasValue && clubId != 0)
            {
                var club = clubsRepo.GetById(clubId.Value);
                if (club != null)
                {
                    clubEmail = club.Email;
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> SendNotification(NotificationsForm model)
        {
            var success = false;
            if (ModelState.IsValid)
            {
                var messageId = notesRep.SendToPlayer(model.SeasonId.Value, model.EntityId, model.Message,
                    model.SendByEmail, model.Subject);
                var notsServ = new GamesNotificationsService();
                notsServ.SendPushToDevices(GlobVars.IsTest);

                if (model.SendByEmail)
                {
                    if (messageId.HasValue)
                    {
                        var hasPermission = User.IsInAnyRole(AppRole.Admins) ||
                                            usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager;
                        var email = GetSenderMailForTeam(model.ClubEmail, model.UnionEmail, model.IsClubEmailChecked);
                        await SendEmailsToUserAsync(messageId.Value, email);
                    }
                }

                success = true;
                ViewBag.SendeSuccess = true;
            }

            return Json(new {Success = success});
        }

        [HttpPost]
        public ActionResult GetNotificationsForm(int seasonId, int[] playersIds, int activityId)
        {
            var activity = db.Activities.FirstOrDefault(a => a.ActivityId == activityId);
            var hasPermission = User.IsInAnyRole(AppRole.Admins) ||
                                usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager;
            var clubEmail = activity?.Club?.Email;
            var unionEmail = activity?.Union?.Email;
            if (!hasPermission) unionEmail = null;
            var model = new NotificationsMultipleForm
            {
                PlayersIds = playersIds,
                SeasonId = seasonId,
                ActivityId = activityId,
                ClubEmail = clubEmail,
                UnionEmail = unionEmail,
                IsUnionManager = hasPermission
            };

            return PartialView("_NotificationMultiple", model);
        }

        [HttpPost]
        public async Task<ActionResult> SendNotificationMultiple(NotificationsMultipleForm model)
        {
            if (ModelState.IsValid)
            {
                foreach (var playerId in model.PlayersIds)
                {
                    var messageId = notesRep.SendToPlayer(model.SeasonId.Value, playerId, model.Message,
                        model.SendByEmail, model.Subject);

                    var notsServ = new GamesNotificationsService();
                    notsServ.SendPushToDevices(GlobVars.IsTest);

                    if (model.SendByEmail)
                    {
                        if (messageId.HasValue)
                        {
                            var activity = db.Activities.FirstOrDefault(a => a.ActivityId == model.ActivityId);
                            var emailAddress = GetSenderMailForActivity(activity, model.IsClubEmailChecked);
                            await SendEmailsToUserAsync(messageId.Value, emailAddress);
                        }
                    }
                }

                ViewBag.SendeSuccess = true;
            }

            return Redirect(Request.UrlReferrer.PathAndQuery);
        }

        private string GetSenderMailForActivity(Activity activity, bool isClubEmailChecked)
        {
            var email = ConfigurationManager.AppSettings["MailServerSenderAdress"];
            if (isClubEmailChecked)
            {
                var unionEmail = activity?.Union?.Email;
                var clubEmail = activity?.Club?.Email;
                if (!string.IsNullOrEmpty(unionEmail) && User.IsInAnyRole(AppRole.Admins) ||
                    usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager)
                {
                    email = unionEmail;
                }
                else if (!string.IsNullOrEmpty(clubEmail))
                {
                    email = clubEmail;
                }
            }

            return email;
        }

        private string GetSenderMailForTeam(string clubEmail, string unionEmail, bool isClubEmailChecked)
        {
            var email = ConfigurationManager.AppSettings["MailServerSenderAdress"];
            if (isClubEmailChecked)
            {
                if (!string.IsNullOrEmpty(unionEmail) && User.IsInAnyRole(AppRole.Admins) ||
                    usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager)
                {
                    email = unionEmail;
                }
                else if (!string.IsNullOrEmpty(clubEmail))
                {
                    email = clubEmail;
                }
            }

            return email;
        }


        private async Task SendEmailsToUserAsync(int messageId, string emailAddress)
        {
            var notesRecipients = notesRep.GetAllRecipients(messageId);
            var message = notesRep.GetMessageById(messageId);
            var emailService = new EmailService();
            var emailFile = Request.Files["EmailFile"];
            if (!string.IsNullOrEmpty(message.Message))
            {
                if (notesRecipients != null && notesRecipients.Any())
                {
                    var userEmails = string.Join(",",
                        notesRecipients.Select(c => usersRepo.GetById(c.UserId)?.Email)
                            .Where(c => !string.IsNullOrEmpty(c)));
                    if (!string.IsNullOrEmpty(userEmails))
                    {
                        if (emailFile != null && emailFile.ContentLength > 0)
                        {
                            ProcessNotificationFile(message?.MsgId, emailFile);
                            await emailService.SendWithFileAsync(userEmails, message.Message, message.Subject,
                                emailFile, emailAddress);
                        }
                        else
                            await emailService.SendAsync(userEmails, message.Message, message.Subject, emailAddress);
                    }

                    notesRep.SetEmailSendStatus(notesRecipients);
                    notesRep.Save();
                }
            }
        }

        private void ProcessNotificationFile(int? messageId, HttpPostedFileBase postedFile)
        {
            if (messageId.HasValue)
            {
                var newName = SaveFile(postedFile, messageId.Value);
                db.NotesAttachedFiles.Add(new NotesAttachedFile
                {
                    NoteMessageId = messageId.Value,
                    FilePath = newName
                });
                db.SaveChanges();
            }
        }

        private string SaveFile(HttpPostedFileBase file, int msgId)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();

            var newName = $"{AppFunc.GetUniqName()}{ext}";

            var savePath = Server.MapPath(GlobVars.ContentPath + "/notifications/");

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

        public ActionResult ExportTennisList(int seasonId, int? unionId = null, IEnumerable<int> leaguesIds = null)
        {
            var unionName = unionsRepo.GetById(unionId.Value).Name;
            var result = new MemoryStream();
            var contentPath = Server.MapPath(GlobVars.ContentPath);
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled)
                {RightToLeft = getCulture() == CultEnum.He_IL})
            {
                var ws = workbook.AddWorksheet($"{Messages.RegistrationList}");
                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                #region header

                if (leaguesIds.Count() > 1)
                {
                    addCell(Messages.LeagueName);
                }

                addCell(Messages.ClubName);
                addCell(Messages.TeamName);
                addCell(Messages.FullName);
                addCell(Messages.IdentNum);
                addCell(Messages.BirthDay);
                addCell(Messages.Phone);
                addCell(Messages.TenicardValidity);
                addCell(Messages.InsuranceDate);
                addCell(Messages.ValidityOfMedicalExamination);
                rowCounter++;
                columnCounter = 1;
                ws.Columns().AdjustToContents();

                #endregion

                var allClubIds = new List<int>();
                foreach (var leagueId in leaguesIds)
                {
                    var league = leagueRepo.GetById(leagueId);
                    var clubIds = !string.IsNullOrEmpty(league.AllowedCLubsIds)
                        ? league.AllowedCLubsIds.Split(',').Select(int.Parse).AsEnumerable()
                        : Enumerable.Empty<int>();
                    allClubIds.AddRange(clubIds);

                    foreach (var clubId in clubIds)
                    {
                        var club = db.Clubs.Include(i => i.TeamRegistrations)
                            .Where(x => x.ClubId == clubId && x.SeasonId == seasonId).FirstOrDefault();

                        var allPlayersRegistrations = leagueRepo
                            .GetAllTennisRegistrationsForClub(leagueId, clubId, seasonId, false)
                            .OrderBy(c => c.TeamName).ThenBy(c => c.FullName);

                        foreach (var row in allPlayersRegistrations)
                        {
                            if (leaguesIds.Count() > 1)
                            {
                                addCell(league.Name);
                            }

                            addCell(row.ClubName);
                            addCell(row.TeamName);
                            addCell(row.FullName);
                            addCell(row.IdentNum);
                            addCell(row.BirthDay?.ToShortDateString() ?? string.Empty);
                            addCell(row.Phone);
                            addCell(row.TennicardValidity?.ToShortDateString() ?? string.Empty);
                            addCell(row.InsuranceValidity?.ToShortDateString());
                            addCell(row.MedicalValidity?.ToShortDateString());

                            rowCounter++;
                            columnCounter = 1;
                        }
                    }
                }

                ws.Columns().AdjustToContents();
                workbook.SaveAs(result);
                result.Position = 0;
            }

            CreateExportFile(result, string.Format("{0}_{1}.xlsx", unionName, Messages.ExportPlayers_FilePostfix));
            ViewBag.ExportResult = true;
            return PartialView("_ExportList");
        }

        public ActionResult ExportTennisTeamsList(int seasonId, int? unionId = null, IEnumerable<int> leaguesIds = null)
        {
            var unionName = unionsRepo.GetById(unionId.Value).Name;
            var result = new MemoryStream();
            var contentPath = Server.MapPath(GlobVars.ContentPath);
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled)
                {RightToLeft = getCulture() == CultEnum.He_IL})
            {
                var ws = workbook.AddWorksheet($"{Messages.RegistrationList}");
                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                #region header

                addCell(Messages.LeagueName);
                addCell(Messages.ClubName);
                addCell(Messages.TeamName);
                rowCounter++;
                columnCounter = 1;
                ws.Columns().AdjustToContents();

                #endregion

                foreach (var leagueId in leaguesIds)
                {
                    var league = leagueRepo.GetById(leagueId);
                    var clubIds = !string.IsNullOrEmpty(league.AllowedCLubsIds)
                        ? league.AllowedCLubsIds.Split(',').Select(int.Parse).AsEnumerable()
                        : Enumerable.Empty<int>();
                    foreach (var clubId in clubIds)
                    {
                        var club = db.Clubs.Include(i => i.TeamRegistrations)
                            .Where(x => x.ClubId == clubId && x.SeasonId == seasonId && !x.IsArchive).FirstOrDefault();
                        if (club != null)
                        {
                            var teamRegisrations = club.TeamRegistrations.Where(tr =>
                                tr.SeasonId == seasonId && tr.LeagueId == leagueId && !tr.IsDeleted &&
                                !tr.Team.IsArchive && !tr.League.IsArchive).ToList();

                            foreach (var row in teamRegisrations)
                            {
                                addCell(row.League.Name);
                                addCell(row.Club.Name);
                                addCell(row.Team.TeamsDetails.FirstOrDefault()?.TeamName ?? row.Team.Title);
                                rowCounter++;
                                columnCounter = 1;
                            }
                        }
                    }

                    ws.Columns().AdjustToContents();
                    workbook.SaveAs(result);
                    result.Position = 0;
                }
            }

            CreateExportFile(result,
                string.Format("{0}_{1}_{2}.xlsx", unionName, Messages.ExportToExcel, Messages.Teams));
            ViewBag.ExportResult = true;
            return PartialView("_ExportList");
        }

        public ActionResult ExportTeamsList(int seasonId, int? unionId = null, IEnumerable<int> leaguesIds = null)
        {
            var unionName = unionsRepo.GetById(unionId.Value).Name;
            var result = new MemoryStream();
            var contentPath = Server.MapPath(GlobVars.ContentPath);
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled)
                {RightToLeft = getCulture() == CultEnum.He_IL})
            {
                var ws = workbook.AddWorksheet($"{Messages.RegistrationList}");
                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                #region header

                addCell(Messages.LeagueName);
                addCell(Messages.TeamName);
                rowCounter++;
                columnCounter = 1;
                ws.Columns().AdjustToContents();

                #endregion

                foreach (var leagueId in leaguesIds)
                {
                    var league = leagueRepo.GetById(leagueId);
                    foreach (var row in league.LeagueTeams.Where(t => !t.Teams.IsArchive && t.SeasonId == seasonId))
                    {
                        addCell(league.Name);
                        addCell(row.Teams.TeamsDetails.FirstOrDefault()?.TeamName ?? row.Teams.Title);
                        rowCounter++;
                        columnCounter = 1;
                    }

                    ws.Columns().AdjustToContents();
                    workbook.SaveAs(result);
                    result.Position = 0;
                }
            }

            CreateExportFile(result,
                string.Format("{0}_{1}_{2}.xlsx", unionName, Messages.ExportToExcel, Messages.Teams));
            ViewBag.ExportResult = true;
            return PartialView("_ExportList");
        }

        public ActionResult ExportList(int? teamId, int seasonId, int? leagueId = null, int? clubId = null,
            int? unionId = null, bool check = false, string tab = null,
            IEnumerable<int> leaguesIds = null)
        {
            var resList = new List<TeamPlayerItem>();
            var positions = new List<Position>();

            var club = clubsRepo.GetById(clubId ?? 0);
            var league = leagueRepo.GetById(leagueId ?? 0);

            var helper = new ImportExportPlayersHelper(db);
            switch (tab)
            {
                case "team":
                {
                    var leagueName = leagueRepo.GetById(leagueId ?? 0)?.Name ?? "";
                    resList = LoadPlayers((int) teamId, seasonId, leagueId, clubId).Select(x =>
                    {
                        x.LeagueName = leagueName;
                        return x;
                    }).ToList();
                    positions = posRepo.GetByTeam((int) teamId).ToList();
                    var team = teamRepo.GetTeamByTeamSeasonId(teamId.Value, seasonId);
                    var exportTeamResult = helper.ExportPlayers(resList, positions, club, league, check);
                    var partName = Messages.ExportPlayers_FilePostfix;
                    if (club?.Union?.Section.Alias == GamesAlias.Athletics)
                    {
                        partName = partName.Replace(Messages.Players, Messages.Athletes);
                    }

                    CreateExportFile(exportTeamResult, string.Format("{0}_{1}.xlsx", team.Title, partName));
                    break;
                }

                case "club":
                {
                    var clubTeams = clubsRepo.GetTeamClubs((int) clubId, seasonId);
                    foreach (var team in clubTeams)
                    {
                        var leagueNames =
                            String.Join(",",
                                team?.Team?.LeagueTeams?.Where(l => l.SeasonId == seasonId)
                                    .Select(t => t.Leagues?.Name)) ?? Messages.NoRelatedLeagues;

                        resList.AddRange(LoadPlayers(team.TeamId, seasonId, leagueId, clubId)
                            .Select(x =>
                            {
                                x.LeagueName = leagueNames;
                                return x;
                            }).ToList());

                        positions.AddRange(posRepo.GetByTeam(team.TeamId));
                    }

                    var exportTeamResult = helper.ExportPlayers(
                        resList.OrderBy(c => c.LeagueName).ThenBy(c => c.TeamName).ToList(),
                        positions, club, league, check);
                    var partName = Messages.ExportPlayers_FilePostfix;
                    if (club?.Union?.Section.Alias == GamesAlias.Athletics)
                    {
                        partName = partName.Replace(Messages.Players, Messages.Athletes);
                    }

                    CreateExportFile(exportTeamResult, string.Format("{0}_{1}.xlsx", club?.Name ?? "Club", partName));
                    break;
                }

                case "league":
                {
                    var leagueName = leagueRepo.GetById(leagueId ?? 0)?.Name ?? "";
                    var leagueTeams = teamRepo.GetTeamsByLeague(leagueId ?? 0);
                    foreach (var team in leagueTeams)
                    {
                        resList.AddRange(LoadPlayers(team.TeamId, seasonId, leagueId, clubId).Select(x =>
                            {
                                x.LeagueName = leagueName;
                                return x;
                            })
                            .ToList());
                        positions.AddRange(posRepo.GetByTeam(team.TeamId));
                    }

                    var exportTeamResult = helper.ExportPlayers(
                        resList.OrderBy(c => c.LeagueName).ThenBy(c => c.TeamName).ToList(),
                        positions, club, league, check);
                    var partName = Messages.ExportPlayers_FilePostfix;
                    if (club?.Union?.Section.Alias == GamesAlias.Athletics)
                    {
                        partName = partName.Replace(Messages.Players, Messages.Athletes);
                    }

                    CreateExportFile(exportTeamResult,
                        string.Format("{0}_{1}.xlsx", String.IsNullOrEmpty(leagueName) ? Messages.League : leagueName,
                            partName));
                    break;
                }

                case "union":
                {
                    var unionName = unionsRepo.GetById(unionId ?? 0).Name;
                    var leagueTeams = leagueRepo.GetLeaguesTeamsByIds(leaguesIds, seasonId);
                    foreach (var team in leagueTeams)
                    {
                        var leagueName = leagueRepo.GetById(team.LeagueId)?.Name ?? "";
                        resList.AddRange(LoadPlayers(team.TeamId, seasonId, leagueId, clubId)
                            .Select(x =>
                            {
                                x.LeagueName = leagueName;
                                return x;
                            }).ToList());
                        positions.AddRange(posRepo.GetByTeam(team.TeamId));
                    }

                    var exportTeamResult = helper.ExportPlayers(
                        resList.OrderBy(c => c.LeagueName).ThenBy(c => c.TeamName).ToList(),
                        positions, club, league, check);
                    var partName = Messages.ExportPlayers_FilePostfix;
                    if (club?.Union?.Section.Alias == GamesAlias.Athletics)
                    {
                        partName = partName.Replace(Messages.Players, Messages.Athletes);
                    }

                    CreateExportFile(exportTeamResult,
                        string.Format("{0}_{1}.xlsx", String.IsNullOrEmpty(unionName) ? Messages.Union : unionName,
                            partName));
                    break;
                }
            }


            ViewBag.ExportResult = true;

            return PartialView("_ExportList");
        }

        public ActionResult ExportSummaryReport(int seasonId, int? unionId = null, IEnumerable<int> leaguesIds = null)
        {
            var unionName = unionsRepo.GetById(unionId.Value).Name;
            var result = new MemoryStream();
            var contentPath = Server.MapPath(GlobVars.ContentPath);
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled)
                {RightToLeft = getCulture() == CultEnum.He_IL})
            {
                var ws = workbook.AddWorksheet($"{Messages.SummaryReport}");
                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                #region header

                addCell(Messages.ClubNumber);
                addCell(Messages.ClubName);
                addCell(Messages.TeamName);
                addCell(Messages.Category);
                addCell(Messages.League);
                addCell(Messages.Group);
                addCell(Messages.FinalRank);

                rowCounter++;
                columnCounter = 1;
                ws.Columns().AdjustToContents();

                #endregion

                var summaryReportRows = new List<SummaryReportRow>();
                var isTennis = false;
                if (leaguesIds != null && leaguesIds.Any())
                {
                    isTennis = leagueRepo.GetSectionAlias(leaguesIds.FirstOrDefault())
                        .Equals(SectionAliases.Tennis, StringComparison.OrdinalIgnoreCase);
                }

                if (isTennis)
                {
                    summaryReportRows = GetSummaryReportRowsTennis(seasonId, leaguesIds);
                }
                else
                {
                    summaryReportRows = GetSummaryReportRows(seasonId, leaguesIds);
                }

                foreach (var row in summaryReportRows.OrderBy(x => x.LeagueName).ThenBy(x => x.Group)
                    .ThenBy(x => x.FinalRank))
                {
                    addCell(row.ClubNumber.HasValue ? row.ClubNumber.Value.ToString() : "0");
                    addCell(row.ClubName);
                    addCell(row.TeamName);
                    addCell(row.CategoryName);
                    addCell(row.LeagueName);
                    addCell(row.Group);
                    addCell(row.FinalRank.ToString());

                    rowCounter++;
                    columnCounter = 1;
                }

                ws.Columns().AdjustToContents();
                workbook.SaveAs(result);
                result.Position = 0;
            }

            CreateExportFile(result, string.Format("{0}_{1}.xlsx", unionName, Messages.SummaryReport));
            ViewBag.ExportResult = true;
            return PartialView("_ExportList");
        }

        private List<SummaryReportRow> GetSummaryReportRows(int seasonId, IEnumerable<int> leaguesIds)
        {
            var summaryReportRows = new List<SummaryReportRow>();
            foreach (var leagueId in leaguesIds)
            {
                var league = leagueRepo.GetById(leagueId);
                var rankLeague = GetRankLeague(leagueId, seasonId);
                foreach (var leagueTeam in league.LeagueTeams.Where(t => !t.Teams.IsArchive && t.SeasonId == seasonId))
                {
                    var groupTeams = leagueTeam?.Teams?.GroupsTeams.Where(x =>
                            x.Group.TypeId == GameTypeId.Division && (x.SeasonId == seasonId || !x.SeasonId.HasValue))
                        .ToList();
                    foreach (var groupTeam in groupTeams)
                    {
                        var row = new SummaryReportRow();
                        var club = leagueTeam?.Teams?.ClubTeams?.FirstOrDefault(x => x.SeasonId == seasonId)?.Club;
                        row.ClubNumber = club?.ClubNumber;
                        row.ClubName = club?.Name;
                        row.TeamName = leagueTeam?.Teams.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)
                                           ?.TeamName ??
                                       leagueTeam.Teams.Title;
                        row.CategoryName = league.Age.Title;
                        row.LeagueName = league.Name;
                        row.Group = groupTeam.Group.Name;
                        foreach (var stage in rankLeague.Stages)
                        {
                            var group = stage.Groups.FirstOrDefault(x => x.GroupId == groupTeam.GroupId);
                            if (@group != null)
                            {
                                var rankTeam = @group.Teams.FirstOrDefault(x => x.Id == groupTeam.TeamId);
                                if (rankTeam != null)
                                {
                                    row.FinalRank = rankTeam.TeamPosition;
                                    summaryReportRows.Add(row);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return summaryReportRows;
        }

        private List<SummaryReportRow> GetSummaryReportRowsTennis(int seasonId, IEnumerable<int> leaguesIds)
        {
            List<SummaryReportRow> summaryReportRows = new AutoConstructedList<SummaryReportRow>();
            var isTennis = true;
            foreach (var leagueId in leaguesIds)
            {
                var teams = teamRepo.GetRegisteredTeamsByLeagueId(leagueId, seasonId);
                var league = leagueRepo.GetById(leagueId);
                var rankLeague = GetRankLeague(leagueId, seasonId, isTennis);
                foreach (var team in teams)
                {
                    var groupTeams = team?.GroupsTeams.Where(x =>
                            x.Group.TypeId == GameTypeId.Division && (x.SeasonId == seasonId || !x.SeasonId.HasValue))
                        .ToList();
                    foreach (var groupTeam in groupTeams)
                    {
                        var row = new SummaryReportRow();
                        var club = team?.ClubTeams?.FirstOrDefault(x => x.SeasonId == seasonId)?.Club;
                        row.ClubNumber = club?.ClubNumber;
                        row.ClubName = club?.Name;
                        row.TeamName = team?.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)?.TeamName ??
                                       team?.Title;
                        row.CategoryName = league.Age.Title;
                        row.LeagueName = league.Name;
                        row.Group = groupTeam.Group.Name;
                        foreach (var stage in rankLeague.Stages)
                        {
                            var group = stage.Groups.FirstOrDefault(x => x.GroupId == groupTeam.GroupId);
                            if (@group != null)
                            {
                                var rankTeam = @group.Teams.FirstOrDefault(x => x.Id == groupTeam.TeamId);
                                if (rankTeam != null)
                                {
                                    row.FinalRank = rankTeam.TeamPosition;
                                    summaryReportRows.Add(row);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return summaryReportRows;
        }

        public RankLeague GetRankLeague(int id, int seasonId, bool isTennisLeague = false)
        {
            var svc = new LeagueRankService(id);
            var rLeague = svc.CreateLeagueRankTable(seasonId, isTennisLeague);

            if (rLeague == null)
            {
                rLeague = new RankLeague();
            }
            else if (rLeague.Stages.Count == 0)
            {
                rLeague = svc.CreateEmptyRankTable(seasonId);
                rLeague.IsEmptyRankTable = true;

                if (rLeague.Stages.Count == 0)
                {
                    rLeague.Teams = _teamsRepo.GetTeams(seasonId, id).ToList();
                }
            }

            if (User != null)
            {
                rLeague.CanEditPenalties =
                    User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager);
            }

            return rLeague;
        }

        [HttpGet]
        public ActionResult DownloadExportFile()
        {
            var fileByteObj = Session[ExportPlayersResultSessionKey];
            if (fileByteObj == null)
            {
                throw new System.IO.FileNotFoundException();
            }

            var fileBytes = (byte[]) fileByteObj;
            var fileName = Session[ExportPlayersResultFileNameSessionKey] as string;

            return File(fileBytes, "application/octet-stream", fileName);
        }

        [HttpGet]
        public ActionResult UnlockPlayers(int id)
        {
            var player = playersRepo.GetTeamsPlayerById(id);

            player.IsLocked = !player.IsLocked ?? false;

            playersRepo.Save();

            return new JsonResult()
            {
                Data = new {result = true, message = Messages.DataSavedSuccess, value = player.IsLocked.Value},
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
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

            Session.Remove(ExportPlayersResultSessionKey);
            Session.Remove(ExportPlayersResultFileNameSessionKey);
            Session.Add(ExportPlayersResultSessionKey, resultFileBytes);
            Session.Add(ExportPlayersResultFileNameSessionKey, fileName);
        }

        [NonAction]
        private List<TeamPlayerItem> LoadPlayers(int id, int seasonId, int? leagueId = null, int? clubId = null)
        {
            var seasonIdReal = seasonId;
            int? unionId = null;
            if (clubId.HasValue)
            {
                var club = clubsRepo.GetById(clubId.Value);
                //if (club.IsSectionClub ?? true)
                //{
                //    seasonIdReal = teamRepo.GetSeasonIdByTeamId(id, DateTime.Now) ?? seasonId;
                //}

                unionId = club.UnionId;
            }

            var leagues = leagueRepo.GetByTeamAndSeason(id, seasonId).ToList();
            if (leagueId == null && leagues.Any())
            {
                var league = leagues.First();

                leagueId = league.LeagueId;

                ViewBag.UnionId = league.UnionId;
            }

            if (leagueId != null)
            {
                var league = leagueRepo.GetById(leagueId.GetValueOrDefault());
                unionId = league.UnionId.GetValueOrDefault();
            }

            ViewBag.Positions = posRepo.GetByTeam(id);
            ViewBag.TeamId = id;
            ViewBag.SeasonId = seasonIdReal;
            ViewBag.LeagueId = leagueId;
            ViewBag.ClubId = clubId;

            var resList = playersRepo.GetTeamPlayers(
                id,
                leagueId != null ? 0 : clubId ?? 0,
                leagueId ?? 0,
                seasonIdReal);

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.CanApproveTeamRegistration = User.IsInAnyRole(AppRole.Admins) ||
                                                 User.HasTopLevelJob(JobRole.UnionManager) ||
                                                 User.HasTopLevelJob(JobRole.ClubSecretary) ||
                                                 User.HasTopLevelJob(JobRole.ClubManager);
            var team = teamRepo.GetTeamByTeamSeasonId(id, seasonId);

            ViewBag.TeamName = team.Title;

            var leaguesDetail = new List<LeagueDetailsForm>();

            DateTime? minOfMinDate = null;
            DateTime? maxOfMaxDate = null;

            //string PlayerRegistrationPriceStr = string.Empty;
            //string PlayerInsurancePriceStr = string.Empty;


            decimal? PlayerRegistrationPrice = null;
            decimal? PlayerInsurancePrice = null;

            foreach (var league in leagues)
            {
                if (minOfMinDate == null && league.MinimumAge != null)
                {
                    minOfMinDate = league.MinimumAge;
                }
                else if (league.MinimumAge != null)
                {
                    if (league.MinimumAge < minOfMinDate)
                    {
                        minOfMinDate = league.MinimumAge;
                    }
                }

                if (maxOfMaxDate == null && league.MaximumAge != null)
                {
                    maxOfMaxDate = league.MaximumAge;
                }
                else if (league.MaximumAge != null)
                {
                    if (league.MaximumAge > maxOfMaxDate)
                    {
                        maxOfMaxDate = league.MaximumAge;
                    }
                }

                var ld = new LeagueDetailsForm();
                ld.InjectFrom(league);
                leaguesDetail.Add(ld);

                ld.Registration = team.ActivityFormsSubmittedDatas
                    .FirstOrDefault(x =>
                        x.LeagueId == league.LeagueId &&
                        x.Activity.IsAutomatic.HasValue && x.Activity.IsAutomatic.Value &&
                        x.Activity.Type == ActivityType.Group &&
                        x.Activity.SeasonId == seasonId);

                ld.LeaguesPlayerInsurancePrice = league.LeaguesPrices
                    .Where(p => p.PriceType == (int) LeaguePriceType.PlayerInsurancePrice)
                    .Select(p => new LeaguesPricesForm
                    {
                        EndDate = p.EndDate,
                        Price = p.Price,
                        StartDate = p.StartDate
                    }).ToList();
                ld.LeaguesPlayerRegistrationPrice = league.LeaguesPrices
                    .Where(p => p.PriceType == (int) LeaguePriceType.PlayerRegistrationPrice)
                    .Select(p => new LeaguesPricesForm
                    {
                        EndDate = p.EndDate,
                        Price = p.Price,
                        StartDate = p.StartDate
                    }).ToList();
                ld.LeaguesTeamRegistrationPrice = league.LeaguesPrices
                    .Where(p => p.PriceType == (int) LeaguePriceType.TeamRegistrationPrice)
                    .Select(p => new LeaguesPricesForm
                    {
                        EndDate = p.EndDate,
                        Price = p.Price,
                        StartDate = p.StartDate
                    }).ToList();

                ld.MemberFees = league.MemberFees.Select(x => new MemberFeeModel
                {
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Price = x.Amount
                }).ToList();
                ld.HandlingFees = league.HandlingFees.Select(x => new HandlingFeeModel
                {
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Price = x.Amount
                }).ToList();

                var playerInsurancePrice = ld.GetPrice(LeaguePriceType.PlayerInsurancePrice);
                if (playerInsurancePrice != null)
                {
                    PlayerInsurancePrice = Math.Min(playerInsurancePrice.Value,
                        PlayerInsurancePrice.GetValueOrDefault(int.MaxValue));
                }

                var playerRegistrationPrice = ld.GetPrice(LeaguePriceType.PlayerRegistrationPrice);
                if (playerRegistrationPrice != null)
                {
                    PlayerRegistrationPrice = Math.Min(playerRegistrationPrice.Value,
                        PlayerRegistrationPrice.GetValueOrDefault(int.MaxValue));
                }
            }

            ViewBag.LeaguesDetail = leaguesDetail;
            var sectionAlias = secRepo.GetSectionByTeamId(id)?.Alias;
            foreach (var player in resList)
            {
                if (player.IsLocked == null)
                {
                    if (player.Birthday != null)
                    {
                        if (minOfMinDate != null && player.Birthday > minOfMinDate.Value)
                        {
                            player.IsLocked = true;
                        }

                        if (maxOfMaxDate != null && player.Birthday < maxOfMaxDate)
                        {
                            player.IsLocked = true;
                        }
                    }
                }

                if (team.CompetitionAgeId.HasValue && sectionAlias == SectionAliases.Tennis)
                {
                    player.TennisRank =
                        playersRepo.GetTennisPlayerRank(player.UserId, team.CompetitionAgeId.Value, seasonId) ?? 0;
                }

                var playerPriceHelper = new PlayerPriceHelper();

                if (clubId.HasValue && !leaguesDetail.Any())
                {
                    var price =
                        playerPriceHelper.GetTeamPlayerPrice(player.UserId, team.TeamId, clubId.Value, seasonIdReal,
                            null, false);

                    player.PlayerInsurancePrice = price.PlayerInsurancePrice;
                    player.PlayerRegistrationAndEquipmentPrice = price.PlayerRegistrationAndEquipmentPrice;
                    player.ParticipationPrice = price.ParticipationPrice;
                    player.FinalParticipationPrice = playersRepo.GetFinalParticipationPrice(player.UserId,
                        player.ParticipationPrice, player.ManagerParticipationDiscount, seasonId, clubId.Value);
                }
                else
                {
                    var price = playerPriceHelper.GetPlayerPrices(player.UserId, team.TeamId, leagueId ?? 0,
                        seasonIdReal);

                    player.PlayerInsurancePrice = price.InsurancePrice;
                    player.PlayerRegistrationPrice = price.RegistrationPrice;
                    player.MembersFee = price.MembersFee;
                    player.HandlingFee = price.HandlingFee;
                }
            }

            if (unionId == 31)
            {
                // players under 19 years old or started to play on the last 3 years

                foreach (var player in resList)
                {
                    if ((DateTime.Today - player.Birthday.GetValueOrDefault()).Days / 365.25m < 24
                        || (DateTime.Today - player.StartPlaying.GetValueOrDefault()).Days / 365 <= 3)
                        player.IsYoungPlayer = true;
                }
            }

            return resList.OrderByDescending(x => x.IsActive).ThenBy(x => x.FullName).ToList();
        }

        [HttpPost]
        public ActionResult ImportFromExcel(HttpPostedFileBase importedExcel, int seasonId, int? clubId, int teamId)
        {
            try
            {
                if (importedExcel != null && importedExcel.ContentLength > 0)
                {
                    var dto = new List<ExcelPlayerDto>();
                    using (var workBook = new XLWorkbook(importedExcel.InputStream))
                    {
                        var sheet = workBook.Worksheet(1);
                        var valueRows = sheet.RowsUsed().Skip(1).ToList();
                        var i = 0;
                        foreach (var row in valueRows)
                        {
                            var localCulture = CultureInfo.CurrentCulture.ToString();

                            int.TryParse(sheet.Cell(i + 2, 1).Value.ToString(), out var outUserId);
                            decimal.TryParse(sheet.Cell(i + 2, 22).Value.ToString(), out var outParticipationDiscount);
                            decimal.TryParse(sheet.Cell(i + 2, 24).Value.ToString(), out var outPaid);
                            var commentField = sheet.Cell(i + 2, 25).Value.ToString();
                            dto.Add(new ExcelPlayerDto
                            {
                                UserId = outUserId,
                                TeamId = teamId,
                                ClubId = clubId,
                                SeasonId = seasonId,
                                Paid = outPaid,
                                ParticipationDiscount = outParticipationDiscount,
                                Comments = commentField
                            });

                            i++;
                        }
                    }

                    playersRepo.UpdatePlayersFromExcelImport(dto, AdminId);

                    return Redirect(Request.UrlReferrer.ToString());
                }

                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (Exception ex)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
        }
    }
}