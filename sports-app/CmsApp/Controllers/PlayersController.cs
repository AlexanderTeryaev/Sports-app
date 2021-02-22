using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsApp.Models;
using AppModel;
using Omu.ValueInjecter;
using System.IO;
using System.Threading;
using CmsApp.Helpers;
using CmsApp.Helpers.Injections;
using CmsApp.Models.Mappers;
using CmsApp.Services;
using Resources;
using DataService.DTO;
using log4net;
using Microsoft.Ajax.Utilities;
using DataService;
using Ionic.Zip;
using System.Web.Hosting;
using System.Text;
using CmsApp.Helpers.ModelStateAttributes;
using Newtonsoft.Json;
using ClosedXML.Excel;
using DataService.LeagueRank;
using DataService.Services;
using CmsApp.Helpers.ActivityHelpers;
using DataService.Utils;

namespace CmsApp.Controllers
{
    public class PlayersController : AdminController
    {
        private const string ExportPlayersResultSessionKey = "ExportPlayers_Result";
        private const string ExportPlayersResultFileNameSessionKey = "ExportPlayers_ResultFileName";

        private static readonly ILog log = LogManager.GetLogger(typeof(PlayersController));

        private const string ImportPlayersErrorResultSessionKey = "ImportPlayers_ErrorResult";
        private const string ImportPlayersErrorResultFileNameSessionKey = "ImportPlayers_ErrorResultFileName";

        private const string ImportPlayersImageErrorResultSessionKey = "ImportPlayersImage_ErrorResult";

        private const string ImportAthletesErrorResultSessionKey = "ImportGymnastics_ErrorResult";
        private const string ImportAthletesErrorResultFileNameSessionKey = "ImportGymnastics_ErrorResultFileName";

        private readonly List<string> _disciplineSections = new List<string> { "Athletics", "Gymnastic", "Motorsport" };
        // GET: Players
        public ActionResult Index()
        {
            //var query = repo.GetQuery(false);
            return View();
        }

        public ActionResult List(int id, LogicaName logicalName, int? seasonId = null, bool isUnionViewer = false)
        {
            Union union = null;
            Club club = null;
            Dictionary<int, Club> clubs = null;
            List<PlayerStatusViewModel> players;
            List<Position> positions;

            var section = logicalName == LogicaName.Club
                ? clubsRepo.GetById(id)?.Union?.Section
                : unionsRepo.GetById(id)?.Section;

            switch (logicalName)
            {
                case LogicaName.Club:
                    club = clubsRepo.GetById(id);
                    union = club?.Union;
                    positions = posRepo.GetBySection(union?.SectionId ?? club?.SectionId ?? 0).ToList();
                    players = playersRepo.GetPlayersStatusesForCountByClubId(id, seasonId).ToList();
                    break;

                case LogicaName.Union:
                    union = unionsRepo.GetById(id);
                    clubs = clubsRepo.GetByUnion(union.UnionId, seasonId).ToDictionary(c => c.ClubId);
                    positions = posRepo.GetBySection(union.SectionId).ToList();
                    players = playersRepo.GetPlayersStatusesForCountByUnionId(id, seasonId);
                    break;
                default: throw new Exception("LogicalName is not acceptable");
            }

            var isGrouped = union?.Section?.Alias == GamesAlias.Gymnastic || union?.Section?.Alias == GamesAlias.Tennis;
            var isAthletics = section?.Alias == GamesAlias.Athletics;
            var isKarate = section?.Alias == GamesAlias.MartialArts && union?.UnionId == 37;
            var isTennis = section?.Alias == GamesAlias.Tennis;

            ViewBag.PositionsNames = string.Join(",", positions.Select(c => c.Title));
            ViewBag.PositionsIds = string.Join(",", positions.Select(c => c.PosId));
            ViewBag.SectionName = union?.Section?.Alias;
            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);

            var playersCount = isGrouped ? players.GroupBy(x => x.UserId).Count() : players.Count;
            var playersCompletedCount = isGrouped || isKarate
                ? players.GroupBy(x => x.UserId).Select(x => x.First()).Count(p => p.IsActive == true && !p.IsApproveChecked && !p.IsNotApproveChecked)
                : players.Count(p => p.IsActive == true && !p.IsApproveChecked && !p.IsNotApproveChecked);
            var playersApprovedCount = isGrouped || isKarate
                ? players.Where(c => c.IsApproveChecked == true && c.IsActive == true).GroupBy(x => x.UserId).Select(x => x.First())?.Count()
                : players.Count(c => c.IsApproveChecked);
            var playersNotApprovedCount = isGrouped || isKarate
                ? players.GroupBy(x => x.UserId).Select(x => x.First()).Count(c => c.IsNotApproveChecked)
                : players.Count(c => c.IsNotApproveChecked);

            var playersWaitingForApprovalCount = isGrouped || isKarate
                ? players
                    .GroupBy(x => x.UserId)
                    .Select(x => x.First())
                    .Count(c => c.IsActive == true && !c.IsApproveChecked && !c.IsNotApproveChecked && (!isAthletics || !c.HasMedicalCert))
                : players.Count(c => c.IsActive == true && !c.IsApproveChecked && !c.IsNotApproveChecked && (!isAthletics || !c.HasMedicalCert));

            var playersWaitingWithMedicalCertificateCount = isGrouped || isKarate
                ? players.Where(c => c.IsActive == true && !c.IsApproveChecked && !c.IsNotApproveChecked && c.HasMedicalCert && (!isTennis || (c.RawTenicardValidity != null && c.RawTenicardValidity > DateTime.Now && (c.MedExamDate == null || c.MedExamDate.Value > DateTime.Now)))).GroupBy(x => x.UserId).Select(x => x.First()).Count()
                : players.Count(c => c.IsActive == true && !c.IsApproveChecked && !c.IsNotApproveChecked && c.HasMedicalCert);

            var palyersUnactiveCount = isGrouped || isKarate
                ? players.GroupBy(x => x.UserId).Select(x => x.First()).Count(c => c.IsActive == false)
                : players.Count(c => c.IsActive == false);
            Session["PlayersCount"] = playersCount;

            var hasDisciplines = _disciplineSections.Contains(union?.Section?.Alias);
            var discilpines = hasDisciplines
                ? disciplinesRepo.GetAllByUnionId(union?.UnionId ?? 0).ToDictionary(c => c.DisciplineId)
                : new Dictionary<int, DisciplineDTO>();
            var role = usersRepo.GetTopLevelJob(AdminId);
            var model = new PlayersListForm
            {
                ClubId = logicalName == LogicaName.Club ? id : 0,
                UnionId = logicalName == LogicaName.Union ? id : clubsRepo.GetById(id)?.UnionId ?? 0,
                SeasonId = seasonId,
                TotalPlayersCount = playersCount,
                CompletedPlayersCount = playersCompletedCount,
                ApprovedPlayersCount = playersApprovedCount ?? 0,
                NotApprovedPlayersCount = playersNotApprovedCount,
                WaitingForApproval = playersWaitingForApprovalCount,
                WaitingWithMedicalCert = playersWaitingWithMedicalCertificateCount,
                UnactivePlayers = palyersUnactiveCount,
                LogicalName = logicalName,
                CanApprove = User.IsInAnyRole(AppRole.Admins) ||
                                            User.HasTopLevelJob(JobRole.UnionManager) ||
                                            User.HasTopLevelJob(JobRole.ClubSecretary) ||
                                            User.HasTopLevelJob(JobRole.ClubManager),
                CanBlockade = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager),
                HiddenColumns = playersRepo.GetHiddenColumns(AdminId),
                ClubsList = clubs ?? new Dictionary<int, Club>(),
                DisciplinesList = discilpines,
                HasDisciplines = hasDisciplines,
                IsHandicapEnabled = union?.IsHadicapEnabled ?? false,
                IsGymnastic = section?.Alias == GamesAlias.Gymnastic,
                IsCatchball = section?.Alias == GamesAlias.NetBall,
                IsWaterpolo = section?.Alias == GamesAlias.WaterPolo,
                IsBasketball = section?.Alias == GamesAlias.BasketBall,
                IsTennis = section?.Alias == GamesAlias.Tennis,
                IsWeightLifting = section?.Alias == GamesAlias.WeightLifting,
                IsClubManager = (User.HasTopLevelJob(JobRole.ClubManager) || User.HasTopLevelJob(JobRole.ClubSecretary)),
                IsUnionViewer = isUnionViewer,
                IsMotorsport = string.Equals(section?.Alias, GamesAlias.Motorsport, StringComparison.OrdinalIgnoreCase),
                IsIndividual = section?.IsIndividual == true,
                CantChangeIfAccepted = ((union != null || club != null && club.IsUnionClub == true)
                    && !(User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager))),
                PlayerTypeString = UIHelpers.GetPlayerCaption(section?.Alias, false)?.ToLower(),
                IsAthletics = section?.Alias == GamesAlias.Athletics,
                IsSwimming = section?.Alias == GamesAlias.Swimming,
                IsMartialArts = section?.Alias == GamesAlias.MartialArts,
                IsRowing = section?.Alias == GamesAlias.Rowing,
                IsSurfing = section?.Alias == GamesAlias.WaveSurfing,
                IsBicycle = section?.Alias == GamesAlias.Bicycle,
                IsClimbing = section?.Alias == GamesAlias.Climbing,
                BlockRegistration = logicalName == LogicaName.Club
                    && union?.IsClubsBlocked == true
                    && (role?.Equals(JobRole.ClubManager) == true || role?.Equals(JobRole.ClubSecretary) == true),
                ApprovePlayerByClubManagerFirst = union?.ApprovePlayerByClubManagerFirst ?? false
            };

            return PartialView("_List", model);
        }

        [HttpPost]
        public ActionResult LoadData(int id, LogicaName logicalName, int draw, int start, int length, int? seasonId = null,
            string filterByClubs = null, string filterByDisciplines = null, string filterByPlayersStatus = null, string filterSearchByColumns = null)
        {
            var searchText = Request.Form["search[value]"];
            var sortColumnId = Request.Form.GetValues("order[0][column]").FirstOrDefault();
            var sortBy = Request.Form.GetValues($"columns[{sortColumnId}][data]").FirstOrDefault();
            var sortDirection = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var cacheKey =
                $"LoadData_{id}{logicalName}{start}{length}{seasonId}{filterByClubs}{filterByDisciplines}{filterByPlayersStatus}{filterSearchByColumns}{searchText}{sortBy}{sortDirection}";
            var playerViewModels = Caching.GetObjectFromCache<List<PlayerViewModel>>(cacheKey);
            var totalRecords = 0;
            var playersCountStored = Session["PlayersCount"];
            if (null != playersCountStored)
                int.TryParse(playersCountStored.ToString(), out totalRecords);
            var recordsFiltered = totalRecords;
            if (playerViewModels == null)
            {
                var sectionAlias = logicalName == LogicaName.Union
                    ? secRepo.GetByUnionId(id)?.Alias
                    : secRepo.GetByClubId(id)?.Alias;
                var isGrouped = sectionAlias.Equals(GamesAlias.Gymnastic) | sectionAlias.Equals(GamesAlias.Tennis);
                var isFiltered = !string.IsNullOrEmpty(filterByClubs) || !string.IsNullOrEmpty(filterByDisciplines) || !string.IsNullOrEmpty(filterByPlayersStatus);
                
                if (length == -1)
                    length = int.MaxValue;

                //get Sort columns values



                var clubsIds = string.IsNullOrEmpty(filterByClubs) ? new List<int>() : filterByClubs.Split(',').Select(int.Parse).ToList();
                var disciplinesIds = string.IsNullOrEmpty(filterByDisciplines) ? new List<int>() : filterByDisciplines.Split(',').Select(int.Parse).ToList();
                var playersStatusIds = string.IsNullOrEmpty(filterByPlayersStatus) ? new List<int>() : filterByPlayersStatus.Split(',').Select(int.Parse).ToList();
                var searchColumnIds = string.IsNullOrEmpty(filterSearchByColumns) ? new List<string>() : filterSearchByColumns.Split(',').ToList();

                var playersCount = 0;

                var players = logicalName == LogicaName.Union
                    ? playersRepo.GetTeamPlayersByUnionId(id, seasonId, searchText, clubsIds, disciplinesIds, playersStatusIds, searchColumnIds, sortBy, sortDirection, start, length, sectionAlias, out playersCount)
                    : playersRepo.GetTeamPlayersByClubId(id, seasonId, searchText, disciplinesIds, playersStatusIds, searchColumnIds, sortBy, sortDirection, start, length, sectionAlias, out playersCount);



                //TODO: Check this part of code! Why we need this? Filtering is not working with it. 
                //if (isFiltered)
                //{
                //    var testedPlayers = playersRepo.GetTeamPlayersByClubIdFiltered(id, seasonId, searchText, clubsIds, disciplinesIds, playersStatusIds, sortBy,
                //    sortDirection, start, length, sectionAlias, filterByClubs, filterByDisciplines, filterByPlayersStatus, out recordsFiltered);
                //    return Json(new { draw = draw, recordsFiltered, recordsTotal = totalRecords, data = testedPlayers },
                //    JsonRequestBehavior.AllowGet);
                //}


                if (isGrouped) // Display one row per player for Gymnastics union
                {
                    var result = new List<PlayerViewModel>();
                    var groupedPlayers = players.GroupBy(x => x.UserId);
                    foreach (var groupedPlayer in groupedPlayers)
                    {
                        var resultPlayer = players.First(x => x.UserId == groupedPlayer.Key);
                        //resultPlayer.TeamId = 0;
                        resultPlayer.TeamName = string.Join(", ", groupedPlayer.Select(x => x.TeamName));

                        result.Add(resultPlayer);
                    }
                    players = result;
                }
                // seems that this is redundant code below and is not needed, so its commented for now to make sure all filtering systems work fine with all possible use cases for LoadData
                /*
                if (isFiltered)
                {
                    var playersLite = logicalName == LogicaName.Union
                       ? playersRepo.GetPlayersStatusesByUnionId(id, seasonId, sectionAlias)
                       : playersRepo.GetPlayersStatusesByClubId(id, seasonId);

                    playersLite = isGrouped ? playersLite.GroupBy(x => x.UserId).Select(x => x.First()) : playersLite;
                    players = playersRepo.FilterPlayers(players, filterByClubs, filterByDisciplines, filterByPlayersStatus,
                        playersLite, ref recordsFiltered);
                }
                */

                if (sectionAlias == SectionAliases.Bicycle && (logicalName == LogicaName.Club || logicalName == LogicaName.Union))
                {
                    foreach (var p in players)
                    {
                        p.FriendshipPriceTypeName = CustomPriceHelper.GetFriendshipPriceTypeName(p.FriendshipPriceTypeId);
                        p.KitStatusName = PlayerKitHelper.GetKitName(p.KitStatusId);
                    }
                }

                playerViewModels = players.ToList();
                Caching.SetObjectInCache(cacheKey, 20, playerViewModels);

                //if (clubsIds.Count > 0)
                //    recordsFiltered = players.Count;
                recordsFiltered = playersCount;
                if (playersCount > totalRecords) totalRecords = playersCount;
            }

            int unionId;
            if (logicalName == LogicaName.Union)
            {
                unionId = id;
            }
            else
            {
                unionId = (int)clubsRepo.GetClubById(id).UnionId;
            }


            if (unionId == 31)
            {
                // players under 19 years old or started to play on the last 3 years or years between 19 and 24
                var playerVm = playerViewModels
                    .Where(x =>
                        (DateTime.Today - x.Birthday.GetValueOrDefault()).Days / 365.25m < 19
                        || (DateTime.Today - x.StartPlaying.GetValueOrDefault()).Days / 365 <= 3
                        || x.IsYoung);
                if (playerVm != null && playerVm.Any())
                {
                    foreach (var player in playerVm)
                    {
                        player.FullName = Messages.Player_YoungIndicator + " " + player.FullName;
                    }
                }

            }

            
            
            return Json(new { draw = draw, recordsFiltered, recordsTotal = totalRecords, data = playerViewModels.ToList() },
                JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult ApprovePlayerRegistration(int id)
        {
            var date = playersRepo.ChangeTeamsPlayerRegistrationStatus(id, AdminId);
            return Json(new { DateOfApprove = date.ToString() });
        }
        
        [HttpPost]
        public ActionResult UpdateAllPlayers(List<PlayerViewModel> model, LogicaName logicalName, int? seasonId)
        {
            var t = Request.Form;
            return Json(new { ok = "ok" });
        }


        [HttpPost]
        public ActionResult UpdatePlayerApproval(int userId, int? seasonId, int approval)
        {
            playersRepo.ApproveAndActiveAllTeamplayersOfPlayerThisSeason(userId, seasonId, approval);
            return Json(new { });
        }


        [HttpPost]
        public ActionResult UpdatePlayer(PlayerViewModel model, LogicaName logicalName, int? seasonId)
        {
            var isSuccess = true;
            var sectionAlias = logicalName == LogicaName.Club
                ? secRepo.GetByClubId(model.ClubId ?? 0)?.Alias
                : secRepo.GetSectionByTeamId(model.TeamId)?.Alias;

            var individualSections = secRepo.CheckSectionAliasesWithIndividualStatus();

            var hiddenColumns = (playersRepo.GetHiddenColumns(AdminId)?.Split(',').Select(int.Parse) ?? Enumerable.Empty<int>()).ToList();
            var errorMessage = string.Empty;
            try
            {
                model.MedicalCertificate = Request.Form["MedicalCertificate"] != null;
                model.IsActive = (Request.Form["IsActive"]?.ToLower() == "on");
                model.IsApproveChecked = Request.Form["IsApproveChecked"] != null ;
                model.IsNotApproveChecked = Request.Form["IsNotApproveChecked"] != null;
                model.ToWaitingStatus = Request.Form["ToWaitingStatus"] != null;
                model.IsBlockaded = Request.Form["IsBlockaded"] != null;
                model.IsApprovedByClubManager = Request.Form["IsApprovedByClubManagerChecked"] != null;

                if (string.Equals(usersRepo.GetTopLevelJob(AdminId), JobRole.UnionManager) || User.IsInRole(AppRole.Admins))
                {
                    model.IsApproveChecked = Request.Form["IsApproveChecked"] != null && Request.Form["IsApproveChecked"] == "on";
                }

                if (!string.IsNullOrEmpty(Request.Form["EndBlockadeDate"]))
                {
                    model.EndBlockadeDate = DateTime.Parse(Request.Form["EndBlockadeDate"]);
                }

                var requestTeamPlayerItem = individualSections.Contains(sectionAlias) ? playersRepo.GetTeamPlayersByUserIdAndSeasonId(model.UserId, seasonId ?? 0) : new List<TeamsPlayer> { playersRepo.GetTeamPlayerBySeasonId(model.Id, seasonId) };

                var teamPlayerItemsToUpdate = sectionAlias == GamesAlias.Gymnastic
                    ? requestTeamPlayerItem
                    : requestTeamPlayerItem;

                foreach (var teamPlayerItem in teamPlayerItemsToUpdate)
                {
                    if (sectionAlias != GamesAlias.Gymnastic && sectionAlias != GamesAlias.WaterPolo &&
                        sectionAlias != GamesAlias.Tennis)
                    {
                        if (teamPlayerItem.ShirtNum != model.ShirtNum && !hiddenColumns.Contains(7))
                        {
                            //if (playersRepo.ShirtNumberExists(frm.TeamId, frm.ShirtNum, frm.SeasonId ?? 0, frm.LeagueId, frm.ClubId))
                            //{
                            //    isSuccess = false;
                            //    errorMessage += Messages.ImportPlayers_PlayerWithJerseyNumberExist;
                            //}
                            //else
                            //{
                            teamPlayerItem.ShirtNum = model.ShirtNum;
                            //}
                        }
                    }

                    var medExamDate = Request.Form["MedExamDate"];
                    if (string.IsNullOrEmpty(medExamDate))
                    {
                        teamPlayerItem.User.MedExamDate = null;
                    }
                    else
                    {
                        teamPlayerItem.User.MedExamDate = DateTime.Parse(medExamDate);
                    }
                    if(sectionAlias == SectionAliases.Basketball)
                    {
                        var dateOfInsurance = Request.Form["DateOfInsurance"];
                        if (string.IsNullOrEmpty(dateOfInsurance))
                        {
                            teamPlayerItem.User.DateOfInsurance = null;
                        }
                        else
                        {
                            teamPlayerItem.User.DateOfInsurance = DateTime.Parse(dateOfInsurance);
                        }
                    }


                    if (string.Equals(usersRepo.GetTopLevelJob(AdminId), JobRole.UnionManager) || User.IsInRole(AppRole.Admins))
                    {
                        var tenicardValidity = Request.Form["TenicardValidity"];
                        if (string.IsNullOrEmpty(tenicardValidity))
                        {
                            teamPlayerItem.User.TenicardValidity = null;
                        }
                        else
                        {
                            teamPlayerItem.User.TenicardValidity = DateTime.Parse(tenicardValidity);
                        }
                    }
                    var initialApprovalDateString = Request.Form["InitialApprovalDate"];
                    var currentInitialApprovalDate = playersRepo.GetInitialApprovalDate(teamPlayerItem.UserId);
                    if (seasonId.HasValue && !string.IsNullOrEmpty(initialApprovalDateString) && currentInitialApprovalDate == null)
                    {
                        var season = seasonsRepository.GetById(seasonId.Value);
                        playersRepo.CreateInitialApprovalDate(teamPlayerItem.UserId,
                            DateTime.Parse(initialApprovalDateString), season.UnionId ?? 36);
                    }
                    else if (!string.IsNullOrEmpty(initialApprovalDateString) && currentInitialApprovalDate != null)
                    {
                        currentInitialApprovalDate.InitialApprovalDate1 = DateTime.Parse(initialApprovalDateString);
                    }
                    else if (string.IsNullOrEmpty(initialApprovalDateString) && currentInitialApprovalDate != null)
                    {
                        playersRepo.RemoveInitialApprovalDate(currentInitialApprovalDate);
                    }

                    var medCertApprovement =
                        teamPlayerItem.User.MedicalCertApprovements.FirstOrDefault(x => x.SeasonId == seasonId);
                    if (medCertApprovement != null)
                    {
                        medCertApprovement.Approved = model.MedicalCertificate == true;
                    }
                    else
                    {
                        teamPlayerItem.User.MedicalCertApprovements.Add(new MedicalCertApprovement
                        {
                            SeasonId = seasonId ?? 0,
                            Approved = model.MedicalCertificate == true
                        });
                    }

                    teamPlayerItem.User.ShirtSize = model.ShirtSize;
                    teamPlayerItem.PosId = model.PosId;

                    var startPlaying = Request.Form["StartPlaying"];
                    if (string.IsNullOrEmpty(startPlaying))
                    {
                        teamPlayerItem.StartPlaying = null;
                    }
                    else
                    {
                        teamPlayerItem.StartPlaying = DateTime.Parse(startPlaying);
                    }

                    var baseHandicap = Request.Form["BaseHandicap"];
                    if (!string.IsNullOrEmpty(baseHandicap))
                    {
                        decimal baseHandicapLevelUpdated;
                        var baseHandicapLevelNow = teamPlayerItem.HandicapLevel;
                        decimal.TryParse(baseHandicap, out baseHandicapLevelUpdated);
                        if (baseHandicapLevelNow != baseHandicapLevelUpdated)
                        {
                            if (!individualSections.Contains(sectionAlias))
                            {
                                //var tps = playersRepo.GetTeamPlayersByUserIdAndSeasonId(model.UserId, seasonId ?? 0);
                                //foreach (var tp in tps)
                                //{
                                //    tp.HandicapLevel = baseHandicapLevelUpdated;
                                //} 
                                playersRepo.SetPlayerHandicapLevel(model.UserId, seasonId ?? 0, baseHandicapLevelUpdated);
                            }
                        }
                    }

                    var topLevelJob = usersRepo.GetTopLevelJob(AdminId);
                    if ((string.Equals(topLevelJob, JobRole.UnionManager) || string.Equals(topLevelJob, JobRole.ClubManager) ||
                        string.Equals(topLevelJob, JobRole.ClubSecretary) ||
                        User.IsInRole(AppRole.Admins)) && logicalName == LogicaName.Club)
                    {
                        var u = clubsRepo.GetUnionByClub(model.ClubId ?? 0, model.SeasonId ?? 0);
                        if(u != null && u.ApprovePlayerByClubManagerFirst == true)
                            teamPlayerItem.IsApprovedByClubManager = model.IsApprovedByClubManager;
                    }
                    
                    if (string.Equals(usersRepo.GetTopLevelJob(AdminId), JobRole.UnionManager) ||
                        User.IsInRole(AppRole.Admins))
                    {
                        if (model.IsApproveChecked)
                        {
                            var foundExpiredValidity = false;
                            if (((!teamPlayerItem.User.MedExamDate.HasValue ||
                                  teamPlayerItem.User.MedExamDate.Value < DateTime.Now) ||
                                 (!teamPlayerItem.User.TenicardValidity.HasValue ||
                                  teamPlayerItem.User.TenicardValidity.Value < DateTime.Now)) &&
                                teamPlayerItem.Season.Union.Section.Alias == GamesAlias.Tennis)
                            {
                                foundExpiredValidity = true;
                            }

                            if (teamPlayerItem.Season.Union.Section.Alias == GamesAlias.Gymnastic)
                            {
                                if (teamPlayerItem.User.MedExamDate.HasValue && teamPlayerItem.User.MedExamDate.Value.AddMonths(12) < DateTime.Now)
                                {
                                    foundExpiredValidity = true;
                                }
                                if (teamPlayerItem.User.MedExamDate.HasValue && teamPlayerItem.User.MedExamDate.Value.AddMonths(11) < DateTime.Now && teamPlayerItem.User.MedExamDate.Value.AddMonths(12) > DateTime.Now)
                                {
                                    foundExpiredValidity = true;
                                }

                            }
                            if(sectionAlias == GamesAlias.Bicycle)
                            {
                                if (!teamPlayerItem.User.MedExamDate.HasValue || teamPlayerItem.User.MedExamDate.Value < DateTime.Now)
                                {
                                    foundExpiredValidity = true;
                                }
                            }

                            if (foundExpiredValidity)
                            {
                                return Json(new { IsSuccess = false, ErrorMessage = $"{Messages.Error} : {Messages.ValidMedicalCertRequired}" });
                            }


                            if (sectionAlias == GamesAlias.Tennis)
                            {
                                playersRepo.ApproveAllPlayers(teamPlayerItem, AdminId);

                                try
                                {
                                    new EmailService().SendAsync(teamPlayerItem?.User?.Email,
                                        Messages.Player_ApprovedByManager_Tennis);
                                }
                                catch
                                {
                                    // ignored
                                }
                            }
                            else
                            {
                                teamPlayerItem.ActionUserId = AdminId;
                                var dateOfApprove = DateTime.Now;
                                if (teamPlayerItem.ApprovalDate == null)
                                {
                                    teamPlayerItem.ApprovalDate = dateOfApprove;
                                }
                            }

                            var unionId = teamPlayerItem.Season?.UnionId;
                            if (unionId > 0 && teamPlayerItem.ApprovalDate > DateTime.MinValue)
                            {
                                var hasInitialApproveDate =
                                    teamPlayerItem.User.InitialApprovalDates
                                        .Any(x => x.UserId == unionId);

                                if (!hasInitialApproveDate)
                                {
                                    db.InitialApprovalDates.Add(new InitialApprovalDate
                                    {
                                        UserId = teamPlayerItem.UserId,
                                        UnionId = unionId.Value,
                                        InitialApprovalDate1 = teamPlayerItem.ApprovalDate.Value
                                    });
                                }
                            }

                            teamPlayerItem.IsApprovedByManager = true;
                        }
                        else if (model.IsNotApproveChecked && !model.ToWaitingStatus)
                        {
                            teamPlayerItem.IsApprovedByManager = false;
                            teamPlayerItem.ApprovalDate = null;
                        }
                        else
                        {
                            teamPlayerItem.IsApprovedByManager = null;
                            teamPlayerItem.ApprovalDate = null;
                        }
                    }

                    if (model.IsActive == true &&
                        (model.IsNotApproveChecked || teamPlayerItem.IsApprovedByManager == false) && model.ToWaitingStatus)
                    {
                        teamPlayerItem.IsApprovedByManager = null;
                        teamPlayerItem.ApprovalDate = null;
                    }

                    if (model.IsActive.HasValue)
                    {
                        if (sectionAlias == GamesAlias.Tennis)
                        {
                            var allTeamPlayersForThisUser = db.TeamsPlayers
                                .Where(t =>
                                    t.UserId == model.UserId &&
                                    t.SeasonId == model.SeasonId);

                            foreach (var teamPlayer in allTeamPlayersForThisUser)
                            {
                                teamPlayer.IsActive = model.IsActive == true;
                            }
                        }
                        else
                        {
                            //findG check for gymnastic rank, route and age, if player need to be set as active(from inactive status)
                            var unionId = 0;
                            if (seasonId.HasValue) unionId = seasonsRepository.GetById(seasonId.Value).UnionId ?? 0;
                            if (sectionAlias == GamesAlias.Gymnastic && unionId == 36 && !teamPlayerItem.IsActive && model.IsActive == true)
                            {
                                var playerDisciplinesIds = teamPlayerItem.User.PlayerDisciplines.Where(x => x.SeasonId == seasonId.Value).Select(x => x.DisciplineId);

                                //var individualRoutes = teamPlayerItem.User.UsersRoutes.Where(x => playerDisciplinesIds.Contains(x.DisciplineRoute.DisciplineId)).ToList();
                                var individualRanks = teamPlayerItem.User.UsersRanks.Where(x => playerDisciplinesIds.Contains(x.UsersRoute.DisciplineRoute.DisciplineId)).ToList();
                                var teamRanks = teamPlayerItem.User.TeamsRanks.Where(x => playerDisciplinesIds.Contains(x.TeamsRoute.DisciplineTeamRoute.DisciplineId)).ToList();
                                      
                                if(individualRanks.Count == 0 && teamRanks.Count == 0)
                                {
                                    return Json(new { IsSuccess = false, ClimbingCheck = true, ErrorMessage = $"{Messages.Error} : {Messages.PleaseSetRouteAndRank}" });
                                }

                                foreach (var ir in individualRanks)
                                {
                                    if (ir.RouteRank.FromAge > teamPlayerItem.User.BirthDay && ir.RouteRank.ToAge < teamPlayerItem.User.BirthDay)
                                    {
                                        return Json(new { IsSuccess = false, ClimbingCheck = true, ErrorMessage = $"{Messages.Error} : {Messages.AgeInvalid_ChangeRouteAndRank}" });
                                    }
                                }

                                foreach (var tr in teamRanks)
                                {
                                    if (tr.RouteTeamRank.FromAge > teamPlayerItem.User.BirthDay && tr.RouteTeamRank.ToAge < teamPlayerItem.User.BirthDay)
                                    {
                                        return Json(new { IsSuccess = false, ClimbingCheck = true, ErrorMessage = $"{Messages.Error} : {Messages.AgeInvalid_ChangeRouteAndRank}" });
                                    }
                                }

                            }

                            teamPlayerItem.IsActive = model.IsActive == true;
                            
                        }
                    }

                    if (logicalName == LogicaName.Union)
                    {
                        teamPlayerItem.UnionComment = model.UnionComment;
                    }
                    else if (logicalName == LogicaName.Club)
                    {
                        teamPlayerItem.ClubComment = model.ClubComment;
                    }
                }

                if ((string.Equals(usersRepo.GetTopLevelJob(AdminId), JobRole.UnionManager) || User.IsInRole(AppRole.Admins)) && model.IsApproveChecked && sectionAlias == GamesAlias.WaterPolo)
                {
                    int currentSeasonId = getCurrentSeason().Id;
                    var teamPlayersToApprove =
                        teamRepo.GetCollection<TeamsPlayer>(x =>
                            x.TeamId == model.TeamId && x.SeasonId == currentSeasonId && x.UserId == model.UserId && x.LeagueId != model.LeagueId);

                    foreach (var teamPlayerToApprove in teamPlayersToApprove)
                    {
                        teamPlayerToApprove.IsApprovedByManager = true;
                        teamPlayerToApprove.ApprovalDate = DateTime.Now;
                    }
                }

                if (isSuccess)
                {
                    playersRepo.Save();
                    Caching.DeleteAllCache();
                }

                return Json(new { IsSuccess = isSuccess, ErrorMessage = errorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { IsSuccess = false, ErrorMessage = $"{Messages.SystemError} : {ex.Message}" });
            }
        }

        [HttpPost]
        public ActionResult RefreshTennisPlayersCompetitionCount(int unionId, int? seasonId)
        {
            var competitions = leagueRepo.GetFinishedTennisCompeitions(unionId, seasonId ?? 0, 16);
            playersRepo.UpdateTennisCompetitionParticipations(competitions, seasonId ?? 0);
            return Json(new { Success = true });
        }


        public ActionResult CheckApprovedStatus(int userId, int teamId, int? seasonId, int? leagueId)
        {
            var approvedPlayersTeams = playersRepo.GetAllApprovedTeams(userId, teamId, leagueId, seasonId);
            if (approvedPlayersTeams.Any())
            {
                var message = GenerateMessageForWaterpolo(approvedPlayersTeams);
                return Json(new { IsApprovedInAnotherTeam = true, Message = message });

            }
            return Json(new { IsApprovedInAnotherTeam = false });
        }

        public ActionResult EditAthleteNumberProducedStatus(int userId, bool isProduced, int? seasonId) {
            playersRepo.SetAthleteNumberProduced(userId, isProduced, seasonId);
            return Json(new { success = true });
        }


        private string GenerateMessageForWaterpolo(IEnumerable<KeyValuePair<string, string>> approvedPlayersTeams)
        {
            var stringBuilder = new StringBuilder();
            foreach (var player in approvedPlayersTeams)
            {
                stringBuilder.Append("<p>");
                stringBuilder.Append(Messages.WaterpoloApprovedAlert.Replace("[team]", player.Key).Replace("[league]", player.Value));
                stringBuilder.Append("</p>");
            }
            stringBuilder.Append(Messages.WaterpoloApprovedAlertConfirm);
            return stringBuilder.ToString();
        }

        [HttpPost]
        public ActionResult DeletePlayer(int id, int? seasonId)
        {
            var p = playersRepo.GetTeamPlayerBySeasonId(id, seasonId);
            if(p == null)
            {
                return Json(new { ErrorMessage = Messages.PlayerNotExists });
            }
            playersRepo.RemoveFromTeam(p);
            playersRepo.Save();

            return Json(new { ErrorMessage = string.Empty });
        }

        [HttpPost]
        public ActionResult ExportPlayersList(int id, LogicaName logicalName, bool isFiltered, int? seasonId,
            IEnumerable<int> clubsIds, IEnumerable<int> disciplinesIds, IEnumerable<int> statusesIds)
        {
            var isGrouped = logicalName == LogicaName.Union
                ? string.Equals(secRepo.GetByUnionId(id)?.Alias, GamesAlias.Gymnastic,
                    StringComparison.InvariantCultureIgnoreCase) || string.Equals(secRepo.GetByUnionId(id)?.Alias, GamesAlias.Tennis)
                : string.Equals(secRepo.GetByClubId(id)?.Alias, GamesAlias.Gymnastic,
                    StringComparison.InvariantCultureIgnoreCase) || string.Equals(secRepo.GetByClubId(id)?.Alias, GamesAlias.Tennis);

            var name = logicalName == LogicaName.Union ? unionsRepo.GetById(id)?.Name : clubsRepo.GetById(id)?.Name ?? "";
            var isIndividual = logicalName == LogicaName.Union
                ? unionsRepo.GetById(id)?.Section?.IsIndividual == true
                : clubsRepo.GetById(id)?.Union?.Section?.IsIndividual == true;

            List<PlayerViewModel> players;
            var helper = new ImportExportPlayersHelper();
            var section = string.Empty;
            int playersCount = 0;
            switch (logicalName)
            {
                case LogicaName.Club:
                    section = clubsRepo.GetById(id)?.Union?.Section?.Alias ?? clubsRepo.GetById(id)?.Section?.Alias;
                    players = playersRepo.GetTeamPlayersByClubId(id, seasonId, null,
                        disciplinesIds?.Where(x => x != 0)?.ToList(),
                        statusesIds?.Where(x => x != 0)?.ToList(),
                        null,null, null, null, null, section, out playersCount);
                    break;
                case LogicaName.Union:
                    section = unionsRepo.GetById(id)?.Section?.Alias;
                    players = playersRepo.GetTeamPlayersByUnionId(id, seasonId, null,
                        clubsIds?.Where(x => x != 0).ToList(),
                        disciplinesIds?.Where(x => x != 0)?.ToList(),
                        statusesIds?.Where(x => x != 0)?.ToList(),
                        null,null, null, null, null, section, out playersCount);
                    break;
                default: throw new Exception("LogicalName is not acceptable");
            }
            if (isGrouped) // Display one row per player for Gymnastics union
            {
                var result = new List<PlayerViewModel>();
                var groupedPlayers = players.GroupBy(x => x.UserId);
                foreach (var groupedPlayer in groupedPlayers)
                {
                    var resultPlayer = players.First(x => x.UserId == groupedPlayer.Key);

                    resultPlayer.TeamId = 0;
                    resultPlayer.TeamName = string.Join(", ", groupedPlayer.Select(x => x.TeamName));

                    result.Add(resultPlayer);
                }

                players = result;
            }
            var exportResult = helper.ExportAllPlayers(players, section, getCulture(), isIndividual);

            var fileName = $"{name}_{Messages.Export}{UIHelpers.GetPlayerCaption(section)}.xlsx";

            CreateExportFile(exportResult, fileName);
            ViewBag.ExportResult = true;

            return PartialView("_ExportList");
        }

        public void ExportActivesList(int id, LogicaName logicalName, int? seasonId)
        {
            var isGrouped = logicalName == LogicaName.Union
                ? string.Equals(secRepo.GetByUnionId(id)?.Alias, GamesAlias.Gymnastic,
                    StringComparison.InvariantCultureIgnoreCase) || string.Equals(secRepo.GetByUnionId(id)?.Alias, GamesAlias.Tennis)
                : string.Equals(secRepo.GetByClubId(id)?.Alias, GamesAlias.Gymnastic,
                    StringComparison.InvariantCultureIgnoreCase) || string.Equals(secRepo.GetByClubId(id)?.Alias, GamesAlias.Tennis);

            var name = logicalName == LogicaName.Union ? unionsRepo.GetById(id)?.Name : clubsRepo.GetById(id)?.Name ?? "";
            var isIndividual = logicalName == LogicaName.Union ? unionsRepo.GetById(id)?.Section?.IsIndividual == true && unionsRepo.GetById(id)?.Section?.Alias != GamesAlias.Athletics : clubsRepo.GetById(id)?.Union?.Section?.IsIndividual == true && clubsRepo.GetById(id)?.Union?.Section?.Alias != GamesAlias.Athletics;

            List<PlayerStatusViewModel> players;
            var helper = new ImportExportPlayersHelper();
            var section = string.Empty;
            Club club = null;
            Union union;
            switch (logicalName)
            {
                case LogicaName.Club:
                    section = clubsRepo.GetById(id)?.Union?.Section?.Alias ?? clubsRepo.GetById(id)?.Section?.Alias;
                    players = playersRepo.GetPlayersStatusesByClubId(id, seasonId, true).ToList();
                    club = clubsRepo.GetById(id);
                    union = club.Union;
                    break;
                case LogicaName.Union:
                    union = unionsRepo.GetById(id);
                    section = unionsRepo.GetById(id)?.Section?.Alias;
                    players = playersRepo.GetPlayersStatusesByUnionId(id, seasonId, section, true);
                    break;
                default: throw new Exception("LogicalName is not acceptable");
            }

            var activePlayers = players.Where(p => p.IsActive.HasValue && p.IsActive.Value).DistinctBy(p => p.UserId).ToList();
            var isHebrew = IsHebrew;
            var contentPath = Server.MapPath(GlobVars.ContentPath);
            var playersName = UIHelpers.GetPlayerCaption(union.Section.Alias, true); //union.Section.Alias == SectionAliases.Athletics ? Messages.Athletes.ToLower() : union.Section.Alias == SectionAliases.Athletics ? Messages.Gymnastics.ToLower() : Messages.Players;
            var activesExportPdfHelper = new ActivesExportPdfHelper(activePlayers, union, club, isHebrew, contentPath, playersName, isIndividual);
            var exportResult = activesExportPdfHelper.GetDocumentStream();

            var firstFilenamePart = union.Name;
            if (club != null) {
                firstFilenamePart = club.Name;
            }
            string mainName = Messages.ActivesTable;
            if (section == GamesAlias.Athletics)
            {
                mainName = Messages.ExportActives;
                mainName = mainName.Replace(Messages.Players, UIHelpers.GetPlayerCaption(section, true));
            }
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename={firstFilenamePart}_{mainName}_{DateTime.Now.ToShortDateString()}.pdf");

            Response.OutputStream.Write(exportResult.GetBuffer(), 0, exportResult.GetBuffer().Length);
            Response.Flush();
            Response.End();
        }

        public void ExportAthleticActivesList(int id, LogicaName logicalName, int? seasonId)
        {
            var isGrouped = logicalName == LogicaName.Union
                ? string.Equals(secRepo.GetByUnionId(id)?.Alias, GamesAlias.Gymnastic,
                    StringComparison.InvariantCultureIgnoreCase) || string.Equals(secRepo.GetByUnionId(id)?.Alias, GamesAlias.Tennis)
                : string.Equals(secRepo.GetByClubId(id)?.Alias, GamesAlias.Gymnastic,
                    StringComparison.InvariantCultureIgnoreCase) || string.Equals(secRepo.GetByClubId(id)?.Alias, GamesAlias.Tennis);

            var name = logicalName == LogicaName.Union ? unionsRepo.GetById(id)?.Name : clubsRepo.GetById(id)?.Name ?? "";

            List<PlayerStatusViewModel> players;
            var helper = new ImportExportPlayersHelper();
            var section = string.Empty;
            Club club = null;
            Union union;
            switch (logicalName)
            {
                case LogicaName.Club:
                    section = clubsRepo.GetById(id)?.Union?.Section?.Alias ?? clubsRepo.GetById(id)?.Section?.Alias;
                    players = playersRepo.GetPlayersStatusesByClubId(id, seasonId, true).ToList();
                    club = clubsRepo.GetById(id);
                    union = club.Union;
                    break;
                case LogicaName.Union:
                    union = unionsRepo.GetById(id);
                    section = unionsRepo.GetById(id)?.Section?.Alias;
                    players = playersRepo.GetPlayersStatusesByUnionId(id, seasonId, section, true);
                    break;
                default: throw new Exception("LogicalName is not acceptable");
            }

            var activePlayers = players.Where(p => p.IsActive.HasValue && p.IsActive.Value && p.IsApproveChecked && p.CompetitionCount > 0).DistinctBy(p => p.UserId).ToList();

            var isHebrew = IsHebrew;
            var contentPath = Server.MapPath(GlobVars.ContentPath);
            var playersName = UIHelpers.GetPlayerCaption(union.Section.Alias, true); //union.Section.Alias == SectionAliases.Athletics ? Messages.Athletes.ToLower() : union.Section.Alias == SectionAliases.Athletics ? Messages.Gymnastics.ToLower() : Messages.Players;
            var activesExportPdfHelper = new ActivesExportPdfHelper(activePlayers, union, club, isHebrew, contentPath, playersName, true, true);
            var exportResult = activesExportPdfHelper.GetDocumentStream();

            var firstFilenamePart = union.Name;
            if (club != null)
            {
                firstFilenamePart = club.Name;
            }
            string mainName = Messages.FourCompetitionsReport;

            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename={firstFilenamePart}_{mainName.Replace(Messages.Players, UIHelpers.GetPlayerCaption(section, true))}_{DateTime.Now.ToShortDateString()}.pdf");

            Response.OutputStream.Write(exportResult.GetBuffer(), 0, exportResult.GetBuffer().Length);
            Response.Flush();
            Response.End();
        }

        public ActionResult ProduceReportsForLeague(int[] leagues, int seasonId)
        {
            Dictionary<int, List<TeamRegistration>> teamRegistrationsPerClub = new Dictionary<int, List<TeamRegistration>>();
            Dictionary<int, List<TeamsPlayer>> playersPerClub = new Dictionary<int, List<TeamsPlayer>>();
            List<int> allClubIds = new List<int>();
            foreach (var leagueId in leagues)
            {
                var league = leagueRepo.GetById(leagueId);
                var clubIds = !string.IsNullOrEmpty(league.AllowedCLubsIds)
                    ? league.AllowedCLubsIds.Split(',').Select(int.Parse).AsEnumerable()
                    : Enumerable.Empty<int>();
                allClubIds = allClubIds.Union(clubIds).ToList();
                foreach (var clubId in clubIds)
                {
                    
                    var clubBalances = db.ClubBalances.Where(c => c.ClubId == clubId && c.SeasonId == seasonId && c.IsPdfReport.HasValue && c.IsPdfReport.Value && (!c.IsPaid.HasValue || !c.IsPaid.Value));
                    foreach (var clubBalance in clubBalances)
                    {
                        db.TeamPlayersPayments.RemoveRange(clubBalance.TeamPlayersPayments);
                        db.TeamRegistrationPayments.RemoveRange(clubBalance.TeamRegistrationPayments);
                    }
                    db.ClubBalances.RemoveRange(clubBalances);

                    var club = db.Clubs.Include(i => i.TeamRegistrations).Where(x => x.ClubId == clubId).FirstOrDefault();
                    var contentPath = Server.MapPath(GlobVars.ContentPath);


                    var teams = teamRepo.GetClubTeamsByClubAndSeasonId(clubId, seasonId);
                    var players = playersRepo.GetTeamPlayersWithInvalidTeniCards(teams, clubId, seasonId).Where(r => r.Team.TeamRegistrations.Where(tr => tr.LeagueId == leagueId && tr.SeasonId == seasonId && !tr.IsDeleted).Count() > 0).ToList();
                    if (!playersPerClub.ContainsKey(clubId))
                    {
                        playersPerClub.Add(clubId, players);
                    }
                    else
                    {
                        List<TeamsPlayer> previousTeamsPlayers;
                        playersPerClub.TryGetValue(clubId, out previousTeamsPlayers);
                        previousTeamsPlayers = previousTeamsPlayers.Union(players).ToList();
                        playersPerClub.Remove(clubId);
                        playersPerClub.Add(clubId, previousTeamsPlayers);
                    }

                    var teamRegisrations = club.TeamRegistrations.Where(tr => tr.SeasonId == seasonId && tr.LeagueId == leagueId && !tr.IsDeleted && !tr.Team.IsArchive && !tr.League.IsArchive && (tr.TeamRegistrationPayments.Count == 0 || tr.TeamRegistrationPayments.Any(t => !t.ClubBalances.IsPaid.HasValue || !t.ClubBalances.IsPaid.Value))).ToList();
                    if (!teamRegistrationsPerClub.ContainsKey(clubId))
                    {
                        teamRegistrationsPerClub.Add(clubId, teamRegisrations);
                    }
                    else
                    {
                        List<TeamRegistration> previousTeamsRegistrations;
                        teamRegistrationsPerClub.TryGetValue(clubId, out previousTeamsRegistrations);
                        previousTeamsRegistrations = previousTeamsRegistrations.Union(teamRegisrations).ToList();
                        teamRegistrationsPerClub.Remove(clubId);
                        teamRegistrationsPerClub.Add(clubId, previousTeamsRegistrations);
                    }
                }
            }
            foreach (var clubId in allClubIds)
            {
                List<TeamRegistration> teamsRegistrations = new List<TeamRegistration>();
                List<TeamsPlayer> players = new List<TeamsPlayer>();

                teamRegistrationsPerClub.TryGetValue(clubId, out teamsRegistrations);
                playersPerClub.TryGetValue(clubId, out players);
                players = players.OrderBy(p => p.Team.TeamRegistrations.Where(tr => tr.SeasonId == seasonId&& !tr.IsDeleted).FirstOrDefault().League.Name ?? "zzzzzzzzzzz").ThenBy(p => p.User.FullName).ToList();
                if (players.Count() > 0 || teamsRegistrations.Count() > 0)
                {
                    var balance = new ClubBalanceDto
                    {
                        Income = 0.0m,
                        Expense = 0.0m,
                        Comment = Messages.ClubPaymentReport,
                        SeasonId = seasonId,
                        IsPdfReport = true,
                        IsPaid = false
                    };
                    balance.ActionUser.UserId = AdminId;

                    var cbsServ = new ClubBalanceService(db);
                    var clubBalance = cbsServ.CreateBalanceRecordForReport(clubId, balance, players, teamsRegistrations);
                }
            }
            return Json(new { Success = true});
        }
        

        public ActionResult ProducePaymentReport(int clubId, int seasonId)
        {
            var clubBalances = db.ClubBalances.Where(c => c.ClubId == clubId && c.SeasonId == seasonId && c.IsPdfReport.HasValue && c.IsPdfReport.Value && (!c.IsPaid.HasValue || !c.IsPaid.Value));
            foreach (var clubBalance in clubBalances)
            {
                db.TeamPlayersPayments.RemoveRange(clubBalance.TeamPlayersPayments);
                db.TeamRegistrationPayments.RemoveRange(clubBalance.TeamRegistrationPayments);
            }
            db.ClubBalances.RemoveRange(clubBalances);
            var club = clubsRepo.GetById(clubId);
            var contentPath = Server.MapPath(GlobVars.ContentPath);
            var teams = teamRepo.GetClubTeamsByClubAndSeasonId(clubId, seasonId);
            var players = playersRepo.GetTeamPlayersWithInvalidTeniCards(teams, clubId, seasonId).Where(r => r.Team.TeamRegistrations.Where(tr => tr.SeasonId == seasonId && !tr.IsDeleted).Count() > 0).OrderBy(p => p.Team.TeamRegistrations.Where(tr => tr.SeasonId == seasonId && !tr.IsDeleted).FirstOrDefault().League.Name ?? "zzzzzzzzzzz").ThenBy(p => p.User.FullName).ToList();
            var teamRegisrations = club.TeamRegistrations.Where(tr => tr.SeasonId == seasonId && !tr.IsDeleted && !tr.Team.IsArchive && !tr.League.IsArchive && (tr.TeamRegistrationPayments.Count == 0 || tr.TeamRegistrationPayments.Any(t => !t.ClubBalances.IsPaid.HasValue || !t.ClubBalances.IsPaid.Value))).ToList();
            if (players.Count > 0 || teamRegisrations.Count() > 0)
            {

                var balance = new ClubBalanceDto
                {
                    Income = 0.0m,
                    Expense = 0.0m,
                    Comment = Messages.ClubPaymentReport,
                    SeasonId = seasonId,
                    IsPdfReport = true,
                    IsPaid = false
                };
                balance.ActionUser.UserId = AdminId;

                var cbsServ = new ClubBalanceService(db);

                var clubBalance = cbsServ.CreateBalanceRecordForReport(clubId, balance, players, teamRegisrations);
                var sumPlayers = playersRepo.CreatePlayersPaymentsForITenniCard(players, clubBalance.Id);
                return Json(new { Success = true });
            }
            else
                return Json(new { Success = false});
        }


        public void GeneratePaymentReport(int clubId, int seasonId)
        {
            var clubBalances = db.ClubBalances.Where(c => c.ClubId == clubId && c.SeasonId == seasonId && c.IsPdfReport.HasValue && c.IsPdfReport.Value && (!c.IsPaid.HasValue || !c.IsPaid.Value));
            foreach (var clubBalance in clubBalances)
            {
                db.TeamPlayersPayments.RemoveRange(clubBalance.TeamPlayersPayments);
                db.TeamRegistrationPayments.RemoveRange(clubBalance.TeamRegistrationPayments);
            }
            db.ClubBalances.RemoveRange(clubBalances);
            var club = clubsRepo.GetById(clubId);
            var contentPath = Server.MapPath(GlobVars.ContentPath);
            var teams = teamRepo.GetClubTeamsByClubAndSeasonId(clubId, seasonId);
            var players = playersRepo.GetTeamPlayersWithInvalidTeniCards(teams, clubId, seasonId).Where(r => r.Team.TeamRegistrations.Where(tr => tr.SeasonId == seasonId && !tr.IsDeleted).Count() > 0).OrderBy(p => p.Team.TeamRegistrations.Where(tr => tr.SeasonId == seasonId && !tr.IsDeleted).FirstOrDefault().League.Name ?? "zzzzzzzzzzz").ThenBy(p => p.User.FullName).ToList();
            var teamRegisrations = club.TeamRegistrations.Where(tr => tr.SeasonId == seasonId && !tr.IsDeleted && !tr.Team.IsArchive && !tr.League.IsArchive && (tr.TeamRegistrationPayments.Count == 0 || tr.TeamRegistrationPayments.Any(t => !t.ClubBalances.IsPaid.HasValue || !t.ClubBalances.IsPaid.Value))).ToList();

            var paymentReportPdfExportHelper = new PaymentReportPdfExportHelper(players, teamRegisrations, club, IsHebrew, contentPath);
            var exportResult = paymentReportPdfExportHelper.GetDocumentStream();

            var firstFilenamePart = club.Name;

            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename={firstFilenamePart}_{Messages.ClubPaymentReport}_{DateTime.Now.ToShortDateString()}.pdf");

            Response.OutputStream.Write(exportResult.GetBuffer(), 0, exportResult.GetBuffer().Length);
            Response.Flush();
            Response.End();
        }


        public void ReadPaymentReport(int clubBalanceId, int clubId)
        {
            var club = clubsRepo.GetById(clubId);
            var contentPath = Server.MapPath(GlobVars.ContentPath);

            var players = playersRepo.GetTeamPlayersInPaymentReport(clubBalanceId).OrderBy(p => p.TeamPlayers.Team.TeamRegistrations.Where(tr => tr.SeasonId == p.ClubBalances.SeasonId && !tr.IsDeleted).FirstOrDefault().League.Name ?? "zzzzzzzzzzz").ThenBy(p => p.TeamPlayers.User.FullName).ToList();
            var teams = playersRepo.GetTeamRegistrationsInPaymentReport(clubBalanceId);
            var clubBalance = db.ClubBalances.Where(t => t.Id == clubBalanceId).FirstOrDefault();
            var paymentReportPdfExportHelper = new PaymentReportPdfExportHelper(players, teams, club, clubBalance.TimeOfAction, IsHebrew, contentPath);
            var exportResult = paymentReportPdfExportHelper.GetDocumentStream();

            var firstFilenamePart = club.Name;

            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename={firstFilenamePart}_{Messages.ClubPaymentReport}_{DateTime.Now.ToShortDateString()}.pdf");

            Response.OutputStream.Write(exportResult.GetBuffer(), 0, exportResult.GetBuffer().Length);
            Response.Flush();
            Response.End();
        }




        public void ExportTeamsActivesList(int id, int? clubId, int? unionId, int? seasonId)
        {

            List<TeamPlayerItem> players = null;
            List<TeamPlayerItem> activePlayers = null;

            ImportExportPlayersHelper helper = new ImportExportPlayersHelper();
            Team team = teamRepo.GetById(id, seasonId);
            players = playersRepo.GetTeamPlayers(id, clubId ?? 0, 0, seasonId ?? 0).ToList();
            Club club = null;
            bool isIndividual = false;
            if (clubId != null)
                club = clubsRepo.GetById(clubId.Value);
            Union union = null;
            if (club != null) {
                union = club.Union;
            }
            else if(unionId != null) {
                union = unionsRepo.GetById(unionId.Value);
            }
            bool isCatchball = false;
            bool isAthletics = false;
            if (union != null)
            {
                isCatchball = union.Section.Alias == GamesAlias.NetBall;
                isAthletics = union.Section.Alias == GamesAlias.Athletics;
                isIndividual = union.Section.IsIndividual && union.Section.Alias != GamesAlias.Athletics;
            }
            bool isWaterpolo = false;
            if (union != null)
            {
                isWaterpolo = union.Section.Alias == GamesAlias.WaterPolo;
                isAthletics = union.Section.Alias == GamesAlias.Athletics;
            }
            if (players != null) {
                activePlayers = players.Where(p => p.IsActive || isWaterpolo).DistinctBy(p => p.UserId).ToList();
            }


            var isHebrew = IsHebrew;
            var contentPath = Server.MapPath(GlobVars.ContentPath);
            var playersName = Messages.Players;
            if (union != null)
            {
                playersName = UIHelpers.GetPlayerCaption(union.Section.Alias, true);
            }
            ActivesExportPdfHelper activesExportPdfHelper = null;
            activesExportPdfHelper = new ActivesExportPdfHelper(activePlayers, union, club, team, isHebrew, contentPath, playersName, isIndividual);

            var exportResult = activesExportPdfHelper.GetDocumentStream();
            string firstFilenamePart = string.Empty;
            if (union != null)
            {
                firstFilenamePart = union.Name;
            }
            if (club != null)
            {
                firstFilenamePart = club.Name;
            }
            firstFilenamePart = team.Title + " " + firstFilenamePart;
            string mainName = Messages.ActivesTable;
            if (isAthletics)
            {
                mainName = Messages.ExportActives;
                mainName = mainName.Replace(Messages.Players, UIHelpers.GetPlayerCaption(GamesAlias.Athletics, true));
            }
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename={firstFilenamePart}_{mainName}_{DateTime.Now.ToShortDateString()}.pdf");

            Response.OutputStream.Write(exportResult.GetBuffer(), 0, exportResult.GetBuffer().Length);
            Response.Flush();
            Response.End();
        }

        public void ExportAthleticTeamActivesList(int id, int? clubId, int? unionId, int? seasonId)
        {

            List<TeamPlayerItem> players = null;
            List<TeamPlayerItem> activePlayers = null;

            ImportExportPlayersHelper helper = new ImportExportPlayersHelper();
            Team team = teamRepo.GetById(id, seasonId);
            players = playersRepo.GetTeamPlayers(id, clubId ?? 0, 0, seasonId ?? 0).ToList();
            Club club = null;
            if (clubId != null)
                club = clubsRepo.GetById(clubId.Value);
            Union union = null;
            if (club != null)
            {
                union = club.Union;
            }
            else if (unionId != null)
            {
                union = unionsRepo.GetById(unionId.Value);
            }
            bool isCatchball = false;
            bool isWaterpolo = false;
            var section = string.Empty;
            if (union != null)
            {
                isCatchball = union.Section.Alias == GamesAlias.NetBall;
                isWaterpolo = union.Section.Alias == GamesAlias.WaterPolo;
                section = union.Section.Alias;
            }

            if (players != null)
            {
                activePlayers = players.Where(p => p.IsActive && p.IsApprovedByManager.HasValue && p.IsApprovedByManager.Value && p.CompetitionCount > 0).DistinctBy(p => p.UserId).ToList();
            }

            var isHebrew = IsHebrew;
            var contentPath = Server.MapPath(GlobVars.ContentPath);
            var playersName = Messages.Players;
            if (union != null)
            {
                playersName = UIHelpers.GetPlayerCaption(union.Section.Alias, true);
            }
            ActivesExportPdfHelper activesExportPdfHelper = null;
            activesExportPdfHelper = new ActivesExportPdfHelper(activePlayers, union, club, team, isHebrew, contentPath, playersName, true, true);

            var exportResult = activesExportPdfHelper.GetDocumentStream();
            string firstFilenamePart = string.Empty;
            if (union != null)
            {
                firstFilenamePart = union.Name;
            }
            if (club != null)
            {
                firstFilenamePart = club.Name;
            }
            firstFilenamePart = team.Title + " " + firstFilenamePart;

            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename={firstFilenamePart}_{Messages.ExportActivesRaw.Replace(Messages.Players, UIHelpers.GetPlayerCaption(section, true))}_{DateTime.Now.ToShortDateString()}.pdf");

            Response.OutputStream.Write(exportResult.GetBuffer(), 0, exportResult.GetBuffer().Length);
            Response.Flush();
            Response.End();
        }
        public void ProduceAthleteNumberForm(int id, LogicaName logicalName, int? seasonId = null, bool isUnionViewer = false)
        {
            Union union = null;
            Club club = null;
            List<PlayerStatusViewModel> players;
            var section = logicalName == LogicaName.Club
                ? clubsRepo.GetById(id)?.Union?.Section
                : unionsRepo.GetById(id)?.Section;
            var sectionAlias = section?.Alias;
            switch (logicalName)
            {
                case LogicaName.Club:
                    club = clubsRepo.GetById(id);
                    union = club?.Union;
                    players = playersRepo.GetPlayersStatusesByClubId(id, seasonId).Where(p => !p.IsAthleteNumberProduced && p.IsApproveChecked && p.IsActive.HasValue && p.IsActive.Value && p.AthletesNumbers.HasValue).ToList();
                    break;

                case LogicaName.Union:
                    union = unionsRepo.GetById(id);
                    players = playersRepo.GetPlayersStatusesByUnionId(id, seasonId, sectionAlias).Where(p => !p.IsAthleteNumberProduced && p.IsApproveChecked && p.IsActive.HasValue && p.IsActive.Value && p.AthletesNumbers.HasValue).ToList();
                    break;
                default: throw new Exception("LogicalName is not acceptable");
            }
            if (players.Count() > 0)
            {
                var isHebrew = IsHebrew;
                var exportHelper = new AthleteNumberProductionPdfExportHelper(isHebrew);

                var season = seasonsRepository.GetById(seasonId.Value);
                var clubsList = clubsRepo.GetByUnion(union.UnionId, seasonId.Value).ToList();
                exportHelper.ProduceAthleteNumbersForPlayersList(players, union.Name, season.EndDate, clubsList);
                playersRepo.SetAthleteNumberProduced(players, seasonId.Value);

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", $"attachment;filename={Messages.Athletes.ToLower()}_{DateTime.Now.ToShortDateString()}.pdf");

                var _stream = exportHelper.GetDocumentStream();
                Response.OutputStream.Write(_stream.GetBuffer(), 0, _stream.GetBuffer().Length);
                Response.Flush();
                Response.End();
            }
            else {
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/text";
                Response.AddHeader("content-disposition", $"attachment;filename=NoPlayersForAthleteNumberProduction");
                Response.OutputStream.Write(new byte[0], 0, 0);
                Response.Flush();
                Response.End();
                //Response.Redirect(Request.RawUrl);

            }

        }

        public void DownloadAthletesNumbersForm(int id, LogicaName logicalName, int? seasonId = null, bool isUnionViewer = false)
        {
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                    Union union = null;
                    Club club = null;
                    List<PlayerStatusViewModel> players;
                    var section = logicalName == LogicaName.Club
                        ? clubsRepo.GetById(id)?.Union?.Section
                        : unionsRepo.GetById(id)?.Section;
                    var sectionAlias = section?.Alias;
                    switch (logicalName)
                    {
                        case LogicaName.Club:
                            club = clubsRepo.GetById(id);
                            union = club?.Union;
                            players = playersRepo.GetPlayersStatusesByClubId(id, seasonId).ToList();
                            break;

                        case LogicaName.Union:
                            union = unionsRepo.GetById(id);
                            players = playersRepo.GetPlayersStatusesByUnionId(id, seasonId, sectionAlias);
                            break;
                        default: throw new Exception("LogicalName is not acceptable");
                    }

                    var isGrouped = union?.Section?.Alias == GamesAlias.Gymnastic || union?.Section?.Alias == GamesAlias.Tennis;
                    var isAthletics = section?.Alias == GamesAlias.Athletics;
                    var ws = workbook.AddWorksheet(Messages.AthleteNumber);



                    

                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });
                var addTextCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    ws.Cell(rowCounter, columnCounter).DataType = XLDataType.Text;
                    columnCounter++;
                });

                #region Excel header

                addCell(Messages.FullName);
                addCell($" {Messages.ClubName}");
                addCell($"* {Messages.IdentNum}/{Messages.PassportNum}");
                addCell($" {Messages.BirthDay}");
                addCell($"* {Messages.AthleteNumber}");
                rowCounter++;
                columnCounter = 1;
                ws.Column(3).Style.NumberFormat.SetFormat("@");

                if (isAthletics)
                {
                    var approvedPlayers = players.Where(o => o.IsApproveChecked && (bool)o.IsActive).ToList(); // && o.IsPlayerRegistered
                    foreach (var player in approvedPlayers)
                    {
                        var playerClub = clubsRepo.GetClubById((int)player.ClubId);
                        var user = db.Users.Include("AthleteNumbers").First(u => u.UserId == player.UserId);
                        addCell(user.FullName);
                        addCell(playerClub.Name);
                        addTextCell(user.IdentNum.IsNullOrWhiteSpace() ? user.PassportNum : user.IdentNum );
                        addCell(user.BirthDay.ToString());
                        addCell(user.AthleteNumbers.FirstOrDefault(x => x.SeasonId == seasonId)?.AthleteNumber1.ToString());
                        rowCounter++;
                        columnCounter = 1;
                    }

                }

                ws.Column(3).Style.NumberFormat.SetFormat("@");
                ws.Columns().AdjustToContents();

                #endregion

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", $"attachment;filename={Messages.ExcelForm} {Messages.AthleteNumbers}.xlsx");

                using (var myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
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
            if (fileName.Contains(".pdf")) {
                Response.ContentType = "application/pdf";
                return File(fileBytes, "application/pdf", fileName);
            }
            else
                return File(fileBytes, "application/octet-stream", fileName);
        }

        [HttpPost]
        public ActionResult ColumnVisibility(int item, bool value, string columnName)
        {
            //var columnId = ColumnsHelper.GetColumnId(columnName);
            //playersRepo.SetVisibility(columnId != -1 ? columnId : item, value, AdminId);
            playersRepo.SetVisibility(item, value, AdminId);
            return null;
        }

        public ActionResult Delete(int id)
        {
            var pl = usersRepo.GetById(id);
            pl.IsArchive = true;
            usersRepo.Save();

            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult TeamRetirement(int id = 0, int seasonId = 0, int leagueId = 0, int clubId = 0, int teamId = 0)
        {
            return PartialView("_TeamRetirement",
                new PlayerTeamRetirementForm
                {
                    SeasonId = seasonId,
                    ClubId = clubId,
                    LeagueId = leagueId,
                    TeamId = teamId
                });
        }
        public ActionResult ApproveRetirement(int id = 0, int seasonId = 0, int leagueId = 0, int clubId = 0, int teamId = 0)
        {
            return PartialView("_TeamApproveRetirement",
                new PlayerTeamApproveRetirementForm
                {
                    SeasonId = seasonId,
                    ClubId = clubId,
                    LeagueId = leagueId,
                    TeamId = teamId
                });
        }

        [HttpPost]
        public ActionResult ApproveRetirement(int id, PlayerTeamApproveRetirementForm request)
        {
            if (!ModelState.IsValid || !(User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.ClubManager) || User.HasTopLevelJob(JobRole.ClubSecretary) || User.HasTopLevelJob(JobRole.UnionManager)))
            {
                return RedirectToAction("Edit", new { id, request.SeasonId, request.LeagueId, request.ClubId, request.TeamId });
            }

            var player = usersRepo.GetById(id);

            if (player != null)
            {
                var retirementRequest = player.RetirementRequests.FirstOrDefault(x => x.TeamId == request.TeamId);
                var playerTeam =
                    player.TeamsPlayers.FirstOrDefault(
                        x => x.TeamId == request.TeamId && x.SeasonId == request.SeasonId);

                if (retirementRequest != null && playerTeam != null)
                {
                    retirementRequest.Approved = true;
                    retirementRequest.ApproveText = request.ApproveText;
                    retirementRequest.DateApproved = request.ApproveDate;
                    retirementRequest.RefundAmount = request.ApproveAmount ?? 0;
                    retirementRequest.ApprovedBy = Convert.ToInt32(User.Identity.Name);

                    var approvedBy = usersRepo.GetById(retirementRequest.ApprovedBy ?? 0);

                    playerTeam.IsActive = false;

                    usersRepo.Save();

                    var teamManagers =
                        jobsRepo.GetUsersJobCollection(x => x.TeamId == request.TeamId &&
                                                            x.SeasonId == request.SeasonId &&
                                                            x.Job.JobsRole.RoleName == JobRole.TeamManager &&
                                                            !x.Job.IsArchive &&
                                                            !x.User.IsArchive &&
                                                            x.User.IsActive);

                    var sendToEmails = new List<string> { player.Email };
                    sendToEmails.AddRange(teamManagers.Select(x => x.User.Email));

                    var emailService = new EmailService();

                    var sb = new StringBuilder();
                    sb.AppendLine(string.Format(Messages.RetirementApproveEmail,
                        player.FullName,
                        request.ApproveDate?.ToString("d")));
                    sb.AppendLine(string.Format(Messages.RetirementApprovedByName, approvedBy?.FullName));

                    foreach (var sendToEmail in sendToEmails.Distinct())
                    {
                        var email = sendToEmail;
#if DEBUG
                        email = "info@loglig.com";
#endif
                        emailService.SendAsync(email, sb.ToString());
                    }
                }
            }

            return RedirectToAction("Edit", new { id, request.SeasonId, request.LeagueId, request.ClubId, request.TeamId });
        }

        [HttpPost]
        public ActionResult TeamRetirement(int id, PlayerTeamRetirementForm request)
        {
            var player = usersRepo.GetById(id);

            if (player != null)
            {
                var fileName = string.Empty;
                var postedFile = GetPostedFile("RetirementDocument");

                if (postedFile != null)
                {
                    if (postedFile.ContentLength > GlobVars.MaxFileSize * 1000)
                    {
                        return RedirectToAction("Edit",
                            new { id, request.SeasonId, request.LeagueId, request.ClubId, request.TeamId });
                    }

                    fileName = SaveFile(postedFile, id, PlayerFileType.TeamRetirement);
                }

                player.RetirementRequests.Add(new RetirementRequest
                {
                    TeamId = request.RetirementTeamId,
                    UserId = id,
                    DocumentFileName = fileName,
                    Reason = request.Reason,
                    RequestDate = DateTime.Now
                });
                usersRepo.Save();

                var managersEmailsToNotify = new List<string>();
                //var currentSeasonId = getCurrentSeason()?.Id;
                var currentSeasonId = request.SeasonId;
                var currentTeam = teamRepo.GetById(request.RetirementTeamId, currentSeasonId);

                var leagueId = request.LeagueId;
                if (leagueId == 0)
                {
                    leagueId = currentTeam.LeagueTeams.FirstOrDefault(x => x.SeasonId == currentSeasonId)?.LeagueId ?? 0;
                }

                var league = leagueRepo.GetById(leagueId);
                if (league != null)
                {
                    if (league.Union != null || league.Club?.Union != null)
                    {
                        var unionId = league.Union?.UnionId ?? (int)league.Club?.Union?.UnionId;

                        var unionManagers =
                            jobsRepo.GetUsersJobCollection(x => x.UnionId == unionId &&
                                                                x.SeasonId == currentSeasonId &&
                                                                x.Job.JobsRole.RoleName == JobRole.UnionManager &&
                                                                !x.Job.IsArchive &&
                                                                !x.User.IsArchive &&
                                                                x.User.IsActive);
                        managersEmailsToNotify.AddRange(unionManagers.Select(x => x.User.Email));
                    }
                    else if (league.Club?.Section != null)
                    {
                        var clubManagers =
                            jobsRepo.GetUsersJobCollection(x => x.ClubId == league.Club.ClubId &&
                                                                x.SeasonId == currentSeasonId &&
                                                                (x.Job.JobsRole.RoleName == JobRole.ClubManager || x.Job.JobsRole.RoleName == JobRole.ClubSecretary) &&
                                                                !x.Job.IsArchive &&
                                                                !x.User.IsArchive &&
                                                                x.User.IsActive);
                        managersEmailsToNotify.AddRange(clubManagers.Select(x => x.User.Email));
                    }

                    var teamManagers =
                        jobsRepo.GetUsersJobCollection(x => x.TeamId == request.RetirementTeamId &&
                                                            x.SeasonId == currentSeasonId &&
                                                            x.Job.JobsRole.RoleName == JobRole.TeamManager &&
                                                            !x.Job.IsArchive &&
                                                            !x.User.IsArchive &&
                                                            x.User.IsActive);
                    managersEmailsToNotify.AddRange(teamManagers.Select(x => x.User.Email));

                    var emailService = new EmailService();

                    foreach (var managerEmail in managersEmailsToNotify.Distinct())
                    {
                        var email = managerEmail;
#if DEBUG
                        email = "info@loglig.com";
#endif

                        #region Player Link

                        //var hostBaseUrl = System.Configuration.ConfigurationManager.AppSettings["SiteUrl"];

                        //var playerUrl = string.Format("{0}/Players/Edit/{1}",
                        //    hostBaseUrl,
                        //    player.UserId);

                        //var playerUrlParams = string.Empty;

                        ////if (currentSeasonId.HasValue)
                        ////{
                        ////    playerUrlParams += "?seasonId=" + currentSeasonId.Value;
                        ////}
                        //playerUrlParams += "?seasonId=" + currentSeasonId;

                        //if (request.LeagueId > 0)
                        //{
                        //    if (playerUrlParams.Length == 0)
                        //    {
                        //        playerUrlParams += "?";
                        //    }
                        //    else
                        //    {
                        //        playerUrlParams += "&";
                        //    }

                        //    playerUrlParams += "leagueId=" + request.LeagueId;
                        //}

                        //if (request.ClubId > 0)
                        //{
                        //    if (playerUrlParams.Length == 0)
                        //    {
                        //        playerUrlParams += "?";
                        //    }
                        //    else
                        //    {
                        //        playerUrlParams += "&";
                        //    }

                        //    playerUrlParams += "clubId=" + request.ClubId;
                        //}

                        //if (request.TeamId > 0)
                        //{
                        //    if (playerUrlParams.Length == 0)
                        //    {
                        //        playerUrlParams += "?";
                        //    }
                        //    else
                        //    {
                        //        playerUrlParams += "&";
                        //    }

                        //    playerUrlParams += "teamId=" + request.TeamId;
                        //}

                        //playerUrl += playerUrlParams;

                        #endregion

                        var playerUrl = new UrlHelper(Request.RequestContext)
                            .Action("Edit", "Players",
                            new { id, request.SeasonId, request.LeagueId, request.ClubId, request.TeamId },
                                Request?.Url?.Scheme ?? "http");

                        var emailBody =
                            $"{string.Format(Messages.RetirementRequestEmailHeader, DateTime.Now, player.FullName)}" +
                            "<br>" +
                            $"{Messages.IdentNum}: {player.IdentNum}" +
                            "<br>" +
                            $"{Messages.Team}: {currentTeam.TeamsDetails.FirstOrDefault(x => x.SeasonId == currentSeasonId)?.TeamName ?? currentTeam.Title}" +
                            "<br>" +
                            $"{Messages.League}: {league.Name}" +
                            "<br>" +
                            $"{string.Format(Messages.RetirementRequestEmailRequestText, request.Reason)}" +
                            "<br>" +
                            $"{string.Format(Messages.RetirementRequestEmailLinkToPlayer, playerUrl, Messages.RetirementRequestEmailLinkToPlayerClickHere)}";
                            //$"{string.Format(Messages.RetirementRequestEmailLinkToPlayer, playerUrl, WebUtility.HtmlEncode(playerUrl))}";

                        emailService.SendAsync(email, emailBody);
                    }
                }
            }


            return RedirectToAction("Edit",
                new { id, request.SeasonId, request.LeagueId, request.ClubId, request.TeamId });
        }

        [HttpGet]
        public void SaveRank(int playerId, int rank)
        {
            var tennisRank = db.TennisRanks.Where(x => x.UserId == playerId).FirstOrDefault();
            if (tennisRank == null)
            {
                var newRank = new TennisRank();
                newRank.UserId = playerId;
                newRank.Rank = rank;
                db.TennisRanks.Add(newRank);
            }
            else
            {
                tennisRank.Rank = rank;
            }
            db.SaveChanges();
        }

        [HttpGet]
        public void SavePoints(int playerId, int point)
        {
            var tennisRank = db.TennisRanks.Where(x => x.UserId == playerId).FirstOrDefault();
            if (tennisRank == null)
            {
                var newRank = new TennisRank();
                newRank.UserId = playerId;
                newRank.Points = point;
                db.TennisRanks.Add(newRank);
            }
            else
            {
                tennisRank.Points = point;
            }
            db.SaveChanges();
        }

        public void UpdateTennisRank(int id, int? rank, int? points)
        {
            var tennisRank = db.TennisRanks.Find(id);
            if (tennisRank != null)
            {
                tennisRank.Rank = rank;
                tennisRank.Points = points;
                tennisRank.IsUpdated = true;
                db.SaveChanges();
            }
        }

        public ActionResult Achievements(int id = 0, int seasonId = 0, int leagueId = 0, int clubId = 0, int teamId = 0)
        {
            var pl = usersRepo.GetById(id);

            var isWorker = User.IsInAnyRole(AppRole.Workers);

            var section = isWorker
                ? usersRepo.GetManagersSection(AdminId, seasonId) 
                : secRepo.GetSectionByTeamId(teamId) ??
                  secRepo.GetSectionByClubId(clubId) ??
                  secRepo.GetByLeagueId(leagueId) ?? 
                  playersRepo.GetSectionByUserId(id, seasonId);

            var isBasketaball = section?.Alias == GamesAlias.BasketBall;
            var isTennis = section?.Alias == GamesAlias.Tennis;
            var isMartialArts = section?.Alias == GamesAlias.MartialArts;
            var isAthletics = section?.Alias == GamesAlias.Athletics;

            ViewBag.IsIndividual = section?.IsIndividual == true;
            var jobRole = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.JobRole = jobRole;
            var hasCompetitions = pl.CompetitionRegistrations.Any(cr => !cr.League.IsArchive && cr.FinalScore.HasValue || cr.Position.HasValue)
                || pl.SportsRegistrations.Any(sr => !sr.League.IsArchive && sr.FinalScore.HasValue || sr.Position.HasValue);
            Sport sport = null;

            var currentSeasonId = seasonId != 0 ? seasonId : getCurrentSeason()?.Id;
            var playerFirstTeamInSeason = pl.TeamsPlayers
                .FirstOrDefault(x => x.SeasonId == currentSeasonId)
                ?.Team;
            var competitionList = leagueRepo.GetCompetionsDtos(pl, section?.Alias);

            var season = seasonsRepository.GetById(seasonId);

            ViewBag.UnionImage = "";
            if(season != null && season.UnionId.HasValue)
            {
                ViewBag.UnionImage = unionsRepo.GetById(season.UnionId.Value).Logo;
            }

            if (playerFirstTeamInSeason != null)
            {
                if (playerFirstTeamInSeason.LeagueTeams.Any())
                {
                    var league = playerFirstTeamInSeason.LeagueTeams.FirstOrDefault(x => x.SeasonId == currentSeasonId)?.Leagues;
                    if (league != null)
                    {
                        sport = league.Club?.Sport ?? league.Union?.Sport;
                    }

                }
                else if (playerFirstTeamInSeason.ClubTeams.Any())
                {
                    //player in club
                    var club = playerFirstTeamInSeason.ClubTeams.FirstOrDefault(x => x.SeasonId == currentSeasonId)
                        ?.Club;
                    if (club != null)
                    {
                        sport = club.Sport;
                    }
                }
            }
            if (isTennis)
            {
                var svc = new CategoryRankService(teamId);
                var vm = new PlayerAchievementsViewModel()
                {
                    Culture = getCulture(),
                    IsEditAllowed = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.ClubManager) || User.HasTopLevelJob(JobRole.ClubSecretary) || User.HasTopLevelJob(JobRole.TeamManager),
                    IsBasketball = isBasketaball,
                    PlayersStatistic = playersRepo.GetPlayersStatistics(id, teamId, seasonId, leagueId),
                    SeasonId = seasonId,
                    PlayerId = id,
                    SectionAlias = section.Alias,
                    TennisCompetitionsGames = gamesRepo.GetAllTennisGamesForUser(id, teamId, seasonId, getCulture() == CultEnum.He_IL)
                        ?.OrderByDescending(t => t.DateOfGame),
                    IsTennis = true,
                    PointsAndRanks = svc.GetTennisRanksForUser(id, seasonId),

                };
                
                FillPrintHeaderInfo(vm, seasonId, pl, isAthletics);
                return PartialView("_Achievements", vm);
            }
            if (isMartialArts)
            {
                List<MartialArtsStatsBySeason> martialArtsStatsBySeasons = new List<MartialArtsStatsBySeason>();
                var martialArtsStats = playersRepo.GetMartialArtsPlayerStats(id);
                foreach (var martialArtsStat in martialArtsStats)
                {
                    var martialArtsStatsBySeason = martialArtsStatsBySeasons.FirstOrDefault(x => x.SeasonId == martialArtsStat.SeasonId);
                    if (martialArtsStatsBySeason == null)
                    {
                        martialArtsStatsBySeason = new MartialArtsStatsBySeason
                        {
                            SeasonId = martialArtsStat.SeasonId,
                            SeasonName = seasonsRepository.GetById(martialArtsStat.SeasonId)?.Name,
                            MartialArtsStats = new List<MartialArtsCompetitionDto>()
                        };
                        martialArtsStatsBySeason.MartialArtsStats.Add(martialArtsStat);
                        martialArtsStatsBySeasons.Add(martialArtsStatsBySeason);
                    }
                    else
                    {
                        martialArtsStatsBySeason.MartialArtsStats.Add(martialArtsStat);
                    }
                }

                foreach (var martialArtStatBySeasons in martialArtsStatsBySeasons)
                {
                    martialArtStatBySeasons.MartialArtsStats = martialArtStatBySeasons.MartialArtsStats
                        .OrderByDescending(x => x.EndDate).ToList();
                }
                var vm = new PlayerAchievementsViewModel()
                {
                    Culture = getCulture(),
                    IsEditAllowed = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.ClubManager) || User.HasTopLevelJob(JobRole.ClubSecretary) || User.HasTopLevelJob(JobRole.TeamManager),
                    IsMartialArts = isMartialArts,
                    MartialArtsStatsBySeason = martialArtsStatsBySeasons.OrderByDescending(x => x.SeasonId).ToList(),
                    SeasonId = seasonId,
                    PlayerId = id,
                    SectionAlias = section.Alias
                };
                FillPrintHeaderInfo(vm, seasonId, pl, isAthletics);
                return PartialView("_Achievements", vm);
            }

            if (sport == null && !isBasketaball && !hasCompetitions && !isTennis && ViewBag.IsIndividual == false) return Content("Unable to determine sport of current player");
            else if (sport == null && isBasketaball)
            {
                var vm = new PlayerAchievementsViewModel()
                {
                    Culture = getCulture(),
                    IsEditAllowed = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.ClubManager) || User.HasTopLevelJob(JobRole.ClubSecretary) || User.HasTopLevelJob(JobRole.TeamManager),
                    IsBasketball = isBasketaball,
                    PlayersStatistic = playersRepo.GetPlayersStatistics(id, teamId, seasonId, leagueId),
                    SeasonId = seasonId,
                    PlayerId = id,
                    SectionAlias = section.Alias
                };
                FillPrintHeaderInfo(vm, seasonId, pl, isAthletics);
                return PartialView("_Achievements", vm);
            }
            else if(isAthletics)
            {
                var vm = new PlayerAchievementsViewModel();
                vm.HasActiveCompetitions = false;
                vm.SectionAlias = GamesAlias.Athletics;
                var athleteCompetitions = gamesRepo.GetAthleteCompetitionsAchievements(id);
                foreach (var athleteCompetition in athleteCompetitions)
                {
                    athleteCompetition.Achievements = athleteCompetition.Achievements.OrderByDescending(x => x.CompetitionStartDate).ToList();
                }
                ViewBag.AthleteAchievementsBySeason = athleteCompetitions;
                FillPrintHeaderInfo(vm, seasonId, pl, isAthletics);
                return PartialView("_Achievements", vm);
                
            }
            else if (hasCompetitions && ViewBag.IsIndividual == true)
            {
                List<CompetitionAchievementBySeason> competitionListBySeasons = new List<CompetitionAchievementBySeason>();
                foreach (var competitionAchievement in competitionList)
                {
                    var competitionAchievementBySeason = competitionListBySeasons.FirstOrDefault(x => x.SeasonId == competitionAchievement.SeasonId);
                    if (competitionAchievementBySeason == null)
                    {
                        competitionAchievementBySeason = new CompetitionAchievementBySeason
                        {
                            SeasonId = competitionAchievement.SeasonId,
                            SeasonName = seasonsRepository.GetById(competitionAchievement.SeasonId)?.Name,
                            CompetitionAchievements = new List<CompetitionAchievement>()
                        };
                        competitionAchievementBySeason.CompetitionAchievements.Add(competitionAchievement);
                        competitionListBySeasons.Add(competitionAchievementBySeason);
                    }
                    else
                    {
                        competitionAchievementBySeason.CompetitionAchievements.Add(competitionAchievement);
                    }
                }

                foreach (var competitionListBySeason in competitionListBySeasons)
                {
                    competitionListBySeason.CompetitionAchievements = competitionListBySeason.CompetitionAchievements
                        .OrderByDescending(x => x.EndDateDateTime).ToList();
                }
                var vm = new PlayerAchievementsViewModel()
                {
                    Culture = getCulture(),
                    IsEditAllowed = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.ClubManager) || User.HasTopLevelJob(JobRole.ClubSecretary) || User.HasTopLevelJob(JobRole.TeamManager),
                    HasActiveCompetitions = competitionList.Any(),
                    CompetitionsList = competitionListBySeasons.OrderByDescending(x => x.SeasonId).ToList(),
                    SeasonId = seasonId,
                    SectionAlias = section?.Alias
                };
                FillPrintHeaderInfo(vm, seasonId, pl, isAthletics);
                return PartialView("_Achievements", vm);
            }
            else if (!hasCompetitions && ViewBag.IsIndividual == true)
            {
                return Content(Messages.NoCompetitions);
            }
            var sportRankIds = sport?.SportRanks?.Select(x => x.Id)?.ToList()?? new List<int>(); //explicitly create list of rank ids because entityframework only allows primitives and enumerables in context

            var playerAchievements =
                PlayerAchievementsRepo.GetCollection(x => x.PlayerId == id &&
                                                          sportRankIds.Contains(x.RankId)).ToList();

            if (sport != null && !playerAchievements.Any())
            {
                var achievements = sport.SportRanks.Select(sportRank => new PlayerAchievement
                {
                    PlayerId = id,
                    RankId = sportRank.Id
                });

                PlayerAchievementsRepo.Add(achievements);
                playerAchievements =
                    PlayerAchievementsRepo.GetCollection(x => x.PlayerId == id &&
                                                              sportRankIds.Contains(x.RankId))
                        .ToList();
            }

            var viewModel = new PlayerAchievementsViewModel
            {
                Culture = getCulture(),
                AchievementsBySeasonList = new List<AchievementsBySeason>(),
                IsEditAllowed = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.ClubManager) || User.HasTopLevelJob(JobRole.ClubSecretary) || User.HasTopLevelJob(JobRole.TeamManager),
                SectionAlias = section?.Alias
            };
            viewModel.AchievementsBySeasonList = new List<AchievementsBySeason>();
            AchievementsBySeason achievementsBySeason = new AchievementsBySeason
            {
                SeasonId = currentSeasonId ?? 0,
                SeasonName = leagueRepo.GetById(currentSeasonId ?? 0)?.Name,
                Achievements = new List<PlayerAchievementViewItem>()
            };
            foreach (var playerAchievement in playerAchievements)
            {
                var achievementViewModel = new PlayerAchievementViewItem();
                achievementViewModel.InjectFrom<CloneInjection>(playerAchievement);

                achievementsBySeason.Achievements.Add(achievementViewModel);
            }

            achievementsBySeason.Achievements.OrderByDescending(x => x.DateCompleted).ToList();
            viewModel.AchievementsBySeasonList.Add(achievementsBySeason);
            viewModel.IsBasketball = isBasketaball;
            viewModel.PlayerId = id;

            if (secRepo.GetSectionByTeamId(teamId)?.Alias == GamesAlias.Tennis)
            {
                var tennisRank = db.TennisRanks.Where(x => x.UserId == id).FirstOrDefault();
                var rank = 0;
                var points = 0;
                if (tennisRank != null)
                {
                    rank = tennisRank.Rank.HasValue ? tennisRank.Rank.Value : 0;
                    points = tennisRank.Points.HasValue ? tennisRank.Points.Value : 0;
                }
                viewModel.Rank = rank;
                viewModel.Points = points;
            }
            viewModel.SectionAlias = section?.Alias;
            FillPrintHeaderInfo(viewModel, seasonId, pl, isAthletics);
            return PartialView("_Achievements", viewModel);
        }

        private static void FillPrintHeaderInfo(PlayerAchievementsViewModel vm, int seasonId, User pl, bool isAthletics)
        {
            vm.PlayerName = pl.FullName;
            vm.PlayerClub = pl.TeamsPlayers.FirstOrDefault(x => x.SeasonId == seasonId).Club?.Name;
            vm.BirthDay = pl.BirthDay.HasValue ? pl.BirthDay.Value.Date.ToShortDateString() : null;
            vm.IdentNum = pl.IdentNum;
            if (isAthletics)
            {
                vm.AthleteNumber = pl.AthleteNumbers.FirstOrDefault(x => x.SeasonId == seasonId)?.AthleteNumber1;
            }
        }

        [HttpPost]
        public ActionResult Achievements(int id, PlayerAchievementsViewModel model)
        {
            if (!ModelState.IsValid || !(User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.ClubManager) || User.HasTopLevelJob(JobRole.ClubSecretary) || User.HasTopLevelJob(JobRole.TeamManager)))
            {
                return RedirectToAction("Edit", new { id, model.SeasonId, model.LeagueId, model.ClubId, model.TeamId });
            }

            var achIds = model.AchievementsBySeasonList.FirstOrDefault()?.Achievements.Select(x => x.Id).ToList();
            var playerAchievements = PlayerAchievementsRepo.GetCollection(x => x.PlayerId == id && achIds.Contains(x.Id)).ToList();

            if (playerAchievements.Any() && model.AchievementsBySeasonList != null && model.AchievementsBySeasonList.Any())
            {
                foreach (var achievementsBySeason in model.AchievementsBySeasonList)
                {
                    foreach (var achievement in achievementsBySeason.Achievements)
                    {
                        var dbAchievement = playerAchievements.FirstOrDefault(x => x.Id == achievement.Id);
                        if (dbAchievement == null) continue;

                        dbAchievement.DueDate = achievement.DueDate;
                        dbAchievement.DateCompleted = achievement.DateCompleted;
                        dbAchievement.Score = achievement.Score;
                    }
                }
                

                PlayerAchievementsRepo.Save();
                TempData["Saved"] = true;
            }

            return RedirectToAction("Edit", new { id, model.SeasonId, model.LeagueId, model.ClubId, model.TeamId });
        }

        [HttpGet]
        [RestoreModelStateFromTempData]
        public ActionResult Edit(DepartmentSettings departmentSettings, int id = 0, int seasonId = 0, int leagueId = 0, int clubId = 0, int teamId = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("NotFound", "Error");
            }
            playersRepo.CheckForExclusions();
            gamesRepo.UpdateGameStatistics();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("he-IL");

            var department = clubsRepo.GetById(clubId);
            var isDepartment = department?.ParentClub != null;
            var isTennisLeagueTeam = teamRepo.CheckIfTeamIsLeagueRegistrationTeam(teamId, seasonId, clubId);
            ViewBag.KarateMesssage = TempData["KarateClubMessage"]?.ToString();
            var sectionAlias = secRepo.GetSectionByTeamId(teamId)?.Alias;
            ViewBag.SectionAlias = sectionAlias;
            ViewBag.IsSpecificUnionTopLevelJob = !User.IsInAnyRole(AppRole.Admins) && (User.HasTopLevelJob(JobRole.ClubManager) || User.HasTopLevelJob(JobRole.ClubSecretary) ||
                                               User.HasTopLevelJob(JobRole.UnionManager));
            var isAllPotentUser = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager);
            var vm = new PlayerFormView
            {
                LeagueId = leagueId,
                Genders = new SelectList(usersRepo.GetGenders(), "GenderId", "Title"),

                CanApproveRetirementRequests = User.IsInAnyRole(AppRole.Admins) ||
                                               User.HasTopLevelJob(JobRole.ClubManager) ||
                                               User.HasTopLevelJob(JobRole.ClubSecretary) ||
                                               User.HasTopLevelJob(JobRole.UnionManager),
                CanApproveMedicalCertificate = User.IsInAnyRole(AppRole.Admins) ||
                                               User.HasTopLevelJob(JobRole.ClubManager) ||
                                               User.HasTopLevelJob(JobRole.ClubSecretary) ||
                                               User.HasTopLevelJob(JobRole.UnionManager),
                UnionDisciplines = new List<Discipline>(),

                IsReadOnly = User.IsInAnyRole(AppRole.Players) &&
                             !string.Equals(usersRepo.GetTopLevelJob(AdminId),
                                 JobRole.UnionManager,
                                 StringComparison.OrdinalIgnoreCase) &&
                             !string.Equals(usersRepo.GetTopLevelJob(AdminId),
                                 JobRole.ClubManager,
                                 StringComparison.OrdinalIgnoreCase) &&
                             CheckReadonlyStatus(id, seasonId, clubId, teamId,
                                 leagueId),
                Culture = getCulture(),
                EnableIDCorrectionCheck =seasonsRepository.GetById(seasonId)?.Union?.EnableIDCorrectionCheck ?? false
            };

            Union union = null;
            if (leagueId != 0)
            {
                var league = leagueRepo.GetById(leagueId);
                if (league.Union != null)
                {
                    if (league.UnionId.HasValue)
                    {
                        union = unionsRepo.GetById(league.UnionId.Value);
                        if (union.Disciplines != null && union.Disciplines.Count > 0)
                        {
                            vm.IsSectionTeam = true;
                            vm.UnionDisciplines = union.Disciplines;
                        }
                        else
                        {
                            vm.UnionDisciplines = new List<Discipline>();
                        }
                        vm.IsIndividualSection = union?.Section?.IsIndividual == true;
                    }
                }
                else if (league.Club?.Union != null)
                {
                    union = unionsRepo.GetById(league.Club.UnionId.Value);
                    if (union.Disciplines != null && union.Disciplines.Count > 0)
                    {
                        vm.IsSectionTeam = true;
                        vm.UnionDisciplines = union.Disciplines;
                    }
                    vm.IsIndividualSection = union?.Section?.IsIndividual == true;
                }
            }
            if (clubId != 0)
            {
                var club = clubsRepo.GetById(clubId);
                if (club.UnionId.HasValue)
                {
                    union = unionsRepo.GetById(club.UnionId.Value);
                    if (union.Disciplines != null && union.Disciplines.Count > 0)
                    {
                        vm.IsSectionTeam = true;
                        var clubDisciplines = club.ClubDisciplines.Where(x => x.SeasonId == seasonId);
                        var clubTeamsDisciplinesIds = clubDisciplines.SelectMany(x =>
                            x.Club?.ClubTeams.Where(ct => ct.SeasonId == seasonId).SelectMany(ct =>
                                ct.Team?.TeamDisciplines.Where(td => td.SeasonId == seasonId)
                                    .Select(td => td.DisciplineId)));
                        vm.UnionDisciplines = union.Disciplines.Where(x => clubTeamsDisciplinesIds.Contains(x.DisciplineId));
                    }
                    vm.IsIndividualSection = union?.Section?.IsIndividual == true;
                }
            }

            var is31Union = union?.UnionId == 31;
            var is38Union = union?.UnionId == 38;
            vm.IsAlternativeId = union?.UnionId == GlobVars.UkraineGymnasticUnionId;
            vm.IsUkraineGymnasticUnion = union?.UnionId == GlobVars.UkraineGymnasticUnionId;

            if (id != 0)
            {
                vm.ClubId = clubId;
                vm.CurrentTeamId = teamId;
                vm.SeasonId = seasonId;

                var player = usersRepo.GetById(id);
                if (player == null || player.IsArchive)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                var teamPlayer = player.TeamsPlayers.FirstOrDefault(x => x.SeasonId == seasonId);

                if (sectionAlias == SectionAliases.Bicycle || sectionAlias == SectionAliases.Rowing) 
                {
                    var season = seasonsRepository.GetById(seasonId);
                    if (season != null)
                    {
                        vm.PlayerSeasonAge = season.SeasonForAge.HasValue && player.BirthDay.HasValue ? season.SeasonForAge.Value - player.BirthDay.Value.Year : new Nullable<int>();
                    }
                }

                vm.PassportValidity = player.PassportValidity;
                vm.IsReligious = player.IsReligious ?? false;
                if (sectionAlias == SectionAliases.Bicycle)
                {
                    var flist = friendshipTypesRepo.GetAllByUnionId(union.UnionId).Where(x =>
                        x.CompetitionAges.Any(c =>
                            c.from_age <= vm.PlayerSeasonAge && c.to_age >= vm.PlayerSeasonAge &&
                            (c.gender == player.GenderId || c.gender == 3))).ToList();
                    vm.FriendshipsList = new SelectList(flist, nameof(FriendshipsType.FriendshipsTypesId),
                        nameof(FriendshipsType.Name), teamPlayer.FriendshipTypeId);
                    vm.FriendshipTypeId = teamPlayer.FriendshipTypeId;

                    if (teamPlayer.FriendshipTypeId.HasValue)
                    {
                        var ftlist = CustomPriceHelper.GetFriendshipPriceTypesSelectList(teamPlayer.FriendshipPriceType,
                            teamPlayer.FriendshipsType.FriendshipPrices
                                .Where(x => x.FromAge <= vm.PlayerSeasonAge && x.ToAge >= vm.PlayerSeasonAge &&
                                            (x.GenderId == player.GenderId || x.GenderId == 3))
                                .Select(x => x.FriendshipPriceType).ToList());
                        ftlist.RemoveAt(0);
                        vm.FriendshipsTypeList = new SelectList(ftlist, nameof(SelectListItem.Value),
                            nameof(SelectListItem.Text), teamPlayer.FriendshipPriceType);
                    }
                    else
                    {
                        var ftlist = CustomPriceHelper.GetFriendshipPriceTypesSelectList(null, new List<int?>());
                        ftlist.RemoveAt(0);
                        vm.FriendshipsTypeList = new SelectList(ftlist, nameof(SelectListItem.Value),
                            nameof(SelectListItem.Text), teamPlayer.FriendshipPriceType);
                    }

                    if (teamPlayer.FriendshipTypeId.HasValue)
                    {
                        var roadList = disciplinesRepo.GetAllByUnionIdWithRoad(union.UnionId);
                        roadList = roadList.Where(x => x.CompetitionAges.Any(c =>
                            c.from_age <= vm.PlayerSeasonAge && c.to_age >= vm.PlayerSeasonAge &&
                            (c.gender == player.GenderId || c.gender == 3) &&
                            c.FriendshipTypeId == teamPlayer.FriendshipTypeId)).ToList();
                        vm.RoadDisciplines = new SelectList(roadList, nameof(Discipline.DisciplineId),
                            nameof(Discipline.Name), teamPlayer.RoadDisciplineId);

                        var mountainList = disciplinesRepo.GetAllByUnionIdWithMountain(union.UnionId);
                        mountainList = mountainList.Where(x => x.CompetitionAges.Any(c =>
                            c.from_age <= vm.PlayerSeasonAge && c.to_age >= vm.PlayerSeasonAge &&
                            (c.gender == player.GenderId || c.gender == 3) &&
                            c.FriendshipTypeId == teamPlayer.FriendshipTypeId)).ToList();
                        vm.MountainDisciplines = new SelectList(mountainList, nameof(Discipline.DisciplineId),
                            nameof(Discipline.Name), teamPlayer.MountaintDisciplineId);
                    }

                    var mountCategorie = "";
                    var roadCategorie = "";
                    var isrChampCategorie = "";
                    var competitionAges = disciplinesRepo.GetCompetitionCategoriesByUnionId(union.UnionId);
                    if (competitionAges != null && competitionAges.Count > 0)
                    {
                        var cat = competitionAges
                            .Where(c => c.from_age <= vm.PlayerSeasonAge &&
                                        c.to_age >= vm.PlayerSeasonAge &&
                                        c.FriendshipTypeId == teamPlayer.FriendshipTypeId)
                            .ToList();

                        mountCategorie = cat.FirstOrDefault(x => x.DisciplineId == teamPlayer.MountaintDisciplineId)?.age_name;
                        var roadCat = cat.FirstOrDefault(x => x.DisciplineId == teamPlayer.RoadDisciplineId);
                        if(roadCat != null)
                        {
                            roadCategorie = roadCat.age_name;
                            if (roadCat.IsIsraelChampionship == true) isrChampCategorie = roadCat.age_name;
                        }
                        
                    }
                    vm.BicycleMountaintCategory = mountCategorie;
                    vm.BicycleRoadCategory = roadCategorie;
                    vm.BicycleIsrChampCategory = isrChampCategorie;

                    //Bicycle Friendship Price
                    var bicycleFriendshipPriceHelper = new BicycleFriendshipPriceHelper();

                    var payments = db.BicycleFriendshipPayments
                        .Where(x => x.UserId == id &&
                                    x.ClubId == clubId &&
                                    x.TeamId == teamId &&
                                    x.SeasonId == seasonId)
                        .OrderByDescending(x => x.DateCreated)
                        .ToList();

                    vm.FriendshipPayment = payments.FirstOrDefault(x => x.IsPaid && !x.Discarded);

                    if (vm.FriendshipPayment != null)
                    {
                        //payment already initiated and paid, show price that was stored with payment
                        var friendshipPrice = new BicycleFriendshipPriceHelper.BicycleFriendshipPrice(
                            vm.FriendshipPayment.FriendshipPrice,
                            vm.FriendshipPayment.ChipPrice,
                            vm.FriendshipPayment.UciPrice);

                        vm.FriendshipTotalPrice = friendshipPrice.Total;
                    }
                    else
                    {
                        var friendshipPrice = bicycleFriendshipPriceHelper.GetFriendshipPrice(teamPlayer);

                        vm.FriendshipTotalPrice = friendshipPrice.Total;
                    }

                    vm.FriendshipTeamPlayerId = playersRepo.GetTeamPlayerId(id, vm.SeasonId, vm.LeagueId, vm.ClubId, vm.CurrentTeamId, false) ?? 0;
                }

                vm.InsuranceTypesList = UIHelpers.PopulateInsuranceTypeList(db.InsuranceTypes.ToList(), player.InsuranceTypeId);
                
                vm.FullNameFormatted = player.FullName;

                Sport sport = null;
                vm.Section = new SectionModel();

                var playerDisciplines = player.PlayerDisciplines.Where(x => x.ClubId == clubId && x.SeasonId == seasonId).ToList();
                vm.PlayerDisciplineIds = playerDisciplines.Select(x => x.DisciplineId);

                var playerFirstTeamInSeason = teamPlayer?.Team;
                if (playerFirstTeamInSeason != null)
                {
                    if (playerFirstTeamInSeason.LeagueTeams.Any())
                    {
                        var league = playerFirstTeamInSeason.LeagueTeams.FirstOrDefault(x => x.SeasonId == seasonId)?.Leagues;
                        if (league != null)
                        {
                            var section = league.Club?.Section ?? league.Club?.Union?.Section ?? league.Union?.Section;

                            if (!isDepartment)
                            {
                                vm.Section.InjectFrom(section);
                                sport = league.Club?.Sport ?? league.Union?.Sport;
                            }
                            else
                            {
                                vm.Section.InjectFrom(department.SportSection);
                                sport = league.Club?.Sport ?? league.Union?.Sport;
                            }
                        }
                        else
                        {
                            //get section from seasonId
                            var currSection = seasonsRepository.GetById(seasonId)?.Union?.Section;
                            vm.Section.InjectFrom(currSection);
                        }
                    }
                    else if (playerFirstTeamInSeason.ClubTeams.Any())
                    {
                        //player in club
                        var club = playerFirstTeamInSeason.ClubTeams
                            .FirstOrDefault(x => x.SeasonId == seasonId &&
                                                 (!is38Union || //Tennis specific: filter teams by TeamRegistrations
                                                  !x.Team.TeamRegistrations.Any() ||
                                                  x.Team.TeamRegistrations.Any(tr => tr.ClubId == clubId &&
                                                                                     tr.SeasonId == seasonId &&
                                                                                     !tr.IsDeleted &&
                                                                                     !tr.League.IsArchive))
                            )
                            ?.Club;
                        //is38Union = club?.Union?.UnionId == 38;
                        if (club != null)
                        {
                            var section = club.Section ?? club.Union?.Section;
                            if (!isDepartment)
                            {
                                vm.Section.InjectFrom(section);
                                sport = club.Sport;
                            }
                            else
                            {
                                vm.Section.InjectFrom(department.SportSection);
                                sport = club.Sport;
                            }
                        }
                    }
                }
                if (union == null)
                {
                    vm.IsIndividualSection = vm.Section?.IsIndividual == true;
                }
                if (vm.Section?.Alias?.Equals(GamesAlias.Motorsport, StringComparison.OrdinalIgnoreCase) == true)
                {
                    int driverLicenseTypeId;
                    var vehicleService = new VehicleService(db);
                    int.TryParse(player.LicenseLevel, out driverLicenseTypeId);
                    vm.DriverLicenceTypeList = vehicleService.GetDriversLicenseTypesList(null, driverLicenseTypeId);
                }


                if (leagueId > 0)
                {
                    var league = leagueRepo.GetById(leagueId);
                    if (league != null)
                    {
                        vm.IsHadicapEnabled = league?.Union?.IsHadicapEnabled ?? false;
                        vm.SeasonId = league.SeasonId ?? vm.SeasonId;
                    }
                }
                else
                {
                    if (clubId > 0 && seasonId > 0)
                    {
                        vm.IsHadicapEnabled = clubsRepo.GetUnionByClub(clubId, seasonId)?.IsHadicapEnabled ?? false;
                    }
                }

                vm.TeamPlayerId = playersRepo.GetTeamPlayerId(id, vm.SeasonId, vm.LeagueId, vm.ClubId, vm.CurrentTeamId) ?? 0;

                var athleteNumber = player.AthleteNumbers.FirstOrDefault(x => x.SeasonId == seasonId)?.AthleteNumber1;
                vm.InjectFrom<IgnoreNulls>(player);
                vm.AthleteNumber = athleteNumber;
                if (teamId > 0)
                {
                    var teamsPlayer = player.TeamsPlayers
                        .FirstOrDefault(x => x.UserId == player.UserId &&
                                             x.SeasonId == vm.SeasonId &&
                                             x.TeamId == teamId &&
                                             (leagueId > 0 ? x.LeagueId == leagueId : x.LeagueId == null) &&
                                             (clubId > 0 && leagueId <= 0 ? x.ClubId == clubId : x.ClubId == null));
                    if (teamsPlayer == null)
                    {
                        return RedirectToAction("NotFound", "Error");
                    }

                    vm.HandicapLevel = teamsPlayer.HandicapLevel;
                    vm.StartPlaying = teamsPlayer.StartPlaying;
                    vm.MedExamDate = teamsPlayer.User.MedExamDate;
                    vm.NumberOfTeamsUserPlays = player.TeamsPlayers.Count;
                    vm.MountainIronNumber = teamsPlayer.MountainIronNumber;
                    vm.RoadIronNumber = teamsPlayer.RoadIronNumber;
                    vm.VelodromeIronNumber = teamsPlayer.VelodromeIronNumber;
                    vm.KitStatus = teamsPlayer.KitStatus;
                    vm.UciId = player.UciId;
                    vm.ChipNumber = player.ChipNumber;
                    vm.TeamForUci = teamsPlayer.TeamForUci;
                }

                vm.MedicalCertificate =
                    player.MedicalCertApprovements.FirstOrDefault(x => x.SeasonId == vm.SeasonId)?.Approved == true;

                vm.PlayerFiles = new List<PlayerFileModel>();
                foreach (var playerFile in player.PlayerFiles.Where(x => x.SeasonId == seasonId))
                {
                    var pfModel = new PlayerFileModel();
                    pfModel.InjectFrom(playerFile);
                    vm.PlayerFiles.Add(pfModel);
                }
                vm.IDFileName = player.IDFile ?? vm.PlayerFiles.FirstOrDefault(x => x.FileType == (int)PlayerFileType.IDFile)?.FileName;

                if (!string.IsNullOrEmpty(player.Password))
                    vm.Password = Protector.Decrypt(player.Password);
                vm.IsValidUser = User.IsInAnyRole(AppRole.Admins, AppRole.Editors) || usersRepo.GetTopLevelJob(AdminId) == JobRole.DepartmentManager
                    || usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager || usersRepo.GetTopLevelJob(AdminId) == JobRole.Unionviewer 
                    || usersRepo.GetTopLevelJob(AdminId) == JobRole.RefereeAssignment || usersRepo.GetTopLevelJob(AdminId) == JobRole.ClubSecretary
                    || usersRepo.GetTopLevelJob(AdminId) == JobRole.ClubManager || usersRepo.GetTopLevelJob(AdminId) == JobRole.TeamManager;
                vm.ManagerTeams = new List<TeamDto>();
                vm.PlayerTeams = new List<TeamDto>();
                vm.PlayerHistories = new List<PlayerHistoryFormView>();

                if (sport != null)
                {
                    var currentSportRanks = sport.SportRanks.Select(x => x.Id).ToList();
                    var playerAchievements = player.PlayerAchievements
                        .Where(x => currentSportRanks.Contains(x.RankId) && x.DateCompleted != null)
                        .ToList();
                    if (playerAchievements.Any())
                    {
                        var playerRank = playerAchievements
                            .Aggregate((x, z) => z.DateCompleted > x.DateCompleted ? z : x)
                            ?.SportRank;
                        vm.SportRank = !IsHebrew
                            ? playerRank?.RankName
                            : playerRank?.RankNameHeb;
                    }
                }

                var currDate = DateTime.Now;
                if (player.UsersType.TypeRole == AppRole.Players)
                {
                    //var teams = player.TeamsPlayers.Where(t => !t.Team.IsArchive && seasonsRepository.GetAllCurrent().Select(s => s.Id).Contains(t.SeasonId ?? 0))
                    var teams = player.TeamsPlayers
                        .Where(x => !x.Team.IsArchive &&
                                    x.SeasonId == seasonId);

                    if (!is38Union)
                    {
                        teams = teams.Where(x => (leagueId > 0 ? x.LeagueId == leagueId : x.LeagueId == null) &&
                                                 (clubId > 0 && leagueId <= 0 ? x.ClubId == clubId : x.ClubId == null));
                    }

                    if (is38Union && clubId > 0 && leagueId <= 0)
                    {
                        teams = teams.Where(x =>
                            !x.Team.TeamRegistrations.Any() ||
                            x.Team.TeamRegistrations.Any(tr => tr.ClubId == clubId &&
                                                               tr.SeasonId == seasonId &&
                                                               !tr.IsDeleted && 
                                                               !tr.League.IsArchive));
                    }

                    var teamsDto = teams
                        .Select(t => new TeamDto
                        {
                            TeamId = t.TeamId,
                            Title = t.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == t.SeasonId)?.TeamName ??
                                    t.Team.Title,
                            SeasonId = t.SeasonId,
                            //ClubId = t.Team.ClubTeams.FirstOrDefault(ct => ct.SeasonId == t.SeasonId)?.ClubId ?? 0,
                            ClubId = t.ClubId ?? 0,
                            //LeagueId = t.Team.LeagueTeams
                            //               .FirstOrDefault(lt => lt.SeasonId == t.SeasonId)
                            //               ?.LeagueId ?? 0,
                            LeagueId = t.LeagueId ?? 0,
                            IsActive = t.IsActive,
                            IsTrainerPlayer = t.IsTrainerPlayer,
                            RetirementRequest = player.RetirementRequests
                                .Where(x => x.TeamId == t.TeamId)
                                .Select(x => new RetirementRequestDto
                                {
                                    Reason = x.Reason,
                                    RequestDate = x.RequestDate,
                                    DocumentFile = x.DocumentFileName,
                                    Approved = x.Approved,
                                    ApprovedBy = x.ApproveUser?.FullName,
                                    ApproveText = x.ApproveText,
                                    DateApproved = x.DateApproved,
                                    RefundAmount = x.RefundAmount
                                })
                                .FirstOrDefault(),

                            IsApprovedByManager = t.IsApprovedByManager == true,
                            ApprovalDate = t.ApprovalDate,
                            UserActionName = t.User1?.FullName
                        })
                        .ToList();

                    vm.PlayerTeams = teamsDto;

                    if (!vm.IsValidUser)
                    {
                        var managerTeams = AuthSvc.FindTeamsByManagerId(base.AdminId)
                            .Select(t => new
                            {
                                TeamId = t.TeamId,
                                Title = t.Title,
                                Club = teamRepo.GetClubByTeamId(t.TeamId, currDate),
                                League = teamRepo.GetLeagueByTeamId(t.TeamId, currDate)
                            })
                            .Select(t => new TeamDto
                            {
                                TeamId = t.TeamId,
                                Title = t.Title,
                                SeasonId = t.League?.SeasonId ?? t.Club?.SeasonId,
                                ClubId = t.Club?.ClubId ?? 0,
                                LeagueId = t.League?.LeagueId ?? 0
                            });
                        vm.ManagerTeams = managerTeams;
                    }
                }

                vm.PlayerHistories = usersRepo.GetPlayerHistory(id, seasonId).ToViewModel();
                vm.PassportFileName = player.PassportFile;
                var teamIds = vm.PlayerTeams.Where(x => x.ClubId > 0).Select(x => x.ClubId);
                vm.PlayerClubs = clubsRepo.GetCollection<Club>(c => teamIds.Contains(c.ClubId)).DistinctBy(x => x.ClubId).ToList();
                vm.PlayersPenaltiesHistory = playersRepo.GetPenaltyHistory(id);

                var seasonible = player.ActivityFormsSubmittedDatas.Where(d => d.Activity?.Season?.Union?.Section?.Alias == ViewBag.SectionAlias);
                var seasonless = player.ActivityFormsSubmittedDatas.Where(d => d.Activity?.SeasonId == 0 || d.Activity?.Season?.UnionId == 0);
                var merged = seasonless.Union(seasonible);

                vm.RegistrationsHistory = new List<RegistrationsHistory>();
                vm.RegistrationsHistory.AddRange(merged
                    .Select(x => new RegistrationsHistory
                    {
                        SeasonId = x.Activity.SeasonId ?? 0,
                        SeasonName = x.Activity.Season.Name,

                        ApprovalDate = x.ApprovalDate,
                        ActivityName = x.Activity.Name,
                        TeamId = x.Team?.TeamId ?? 0,
                        TeamName = x.Team?.TeamsDetails.FirstOrDefault(td => td.SeasonId == vm.SeasonId)?.TeamName ??
                                   x.Team?.Title,
                        RegistrationPaid = x.RegistrationPaid,
                        InsurancePaid = x.InsurancePaid,
                        TenicardPaid = x.TenicardPaid,
                        HandlingFeePaid = x.HandlingFeePaid,
                        MembersFeePaid = x.MembersFeePaid,
                        ParticipationPaid = x.ParticipationPaid,
                        CustomPrices = !string.IsNullOrWhiteSpace(x.CustomPrices)
                            ? JsonConvert.DeserializeObject<List<ActivityCustomPriceModel>>(x.CustomPrices)
                            : new List<ActivityCustomPriceModel>(),
                        CardComInvoiceNumber = x.CardComInvoiceNumber,
                        CardComNumberOfPayments = x.CardComNumberOfPayments
                    }));

                var teamsRegistrations = player.TeamsPlayers
                    .Where(x => x.IsApprovedByManager == true && (!x.SeasonId.HasValue || x.SeasonId.Value == 0 || x.Season.Union?.Section.Alias == ViewBag.SectionAlias) &&
                                vm.RegistrationsHistory.All(r => r.TeamId != x.TeamId) &&
                                !x.Team.IsArchive);

                if (is38Union && clubId > 0 && leagueId <= 0)
                {
                    teamsRegistrations = teamsRegistrations.Where(x =>
                        !x.Team.TeamRegistrations.Any() ||
                        x.Team.TeamRegistrations.Any(tr => tr.ClubId == clubId &&
                                                           tr.SeasonId == seasonId &&
                                                           !tr.IsDeleted &&
                                                           !tr.League.IsArchive));
                }

                vm.RegistrationsHistory.AddRange(teamsRegistrations
                    .Select(x =>
                    {
                        var leaguesNames = new List<string>();

                        leaguesNames.AddRange(x.Team?.TeamRegistrations?
                            .Where(lt => lt.SeasonId == x.SeasonId && !lt.IsDeleted)
                            .Select(lt => lt.League?.Name)
                            .Where(lt => lt != null)
                            .ToList() ?? new List<string>());
                        leaguesNames.AddRange(x.Team?.LeagueTeams?
                            .Where(lt => lt.SeasonId == x.SeasonId)
                            .Select(lt => lt.Leagues?.Name)
                            .Where(lt => lt != null)
                            .ToList() ?? new List<string>());

                        return new RegistrationsHistory
                        {
                            ApprovalDate = x.ApprovalDate,
                            TeamName = x.Team?.TeamsDetails.FirstOrDefault(td => td.SeasonId == x.SeasonId)?.TeamName ??
                                       x.Team?.Title,
                            LeagueName = string.Join(", ", leaguesNames),
                            UserActionName = x.User1?.FullName,
                            SeasonId = x.SeasonId ?? 0,
                            SeasonName = x.Season?.Name
                        };
                    }));
                vm.RegistrationsHistory.AddRange(playersRepo.GetAllApprovalHistory(id, seasonId)
                    .Select(x => new RegistrationsHistory
                    {
                        ApprovalDate = x?.ManagerApprovalDate,
                        TeamName = x?.TeamName,
                        UserActionName = x?.User1?.FullName,
                        SeasonId = x.SeasonId,
                        SeasonName = x.Season?.Name
                    }));

                if (string.Equals(vm.Section?.Alias, GamesAlias.Swimming, StringComparison.CurrentCultureIgnoreCase))
                {
                    vm.ClassSList = new SelectList(Enumerable.Range(1,15), player.ClassS);
                    vm.ClassSBList = new SelectList(Enumerable.Range(1, 15).Where(t => t != 10), player.ClassSB);
                    vm.ClassSMList = new SelectList(Enumerable.Range(1, 15), player.ClassSM);
                    vm.Masters = teamPlayer.Masters;
                }

                vm.RegistrationsHistory = vm.RegistrationsHistory.OrderBy(x => x.SeasonId).ThenBy(x => x.ApprovalDate).ToList();

                vm.TeamRetirements = player.RetirementRequests.OrderBy(x => x.DateApproved).ToList();

                vm.AthleticTeams = GetAthleticTeams(union?.UnionId, clubId, seasonId, !isAllPotentUser)
                    .Select(x => new SelectListItem
                    {
                        Value = x.TeamId.ToString(),
                        Text = vm.IsIndividualSection ? $"{x.Title}-{x.ClubName}" : x.Title,
                        Selected = x.TeamId == teamId
                    })
                    .ToList();


                if(ViewBag.SectionAlias == GamesAlias.Climbing)
                {
                    ViewBag.Auditoriums = auditoriumsRepo.GetAuditoriumsFilterList(union.UnionId, seasonId, false);

                    vm.IsNationalSportsman = player.IsNationalSportsman ?? false;
                    vm.ShoesSize = player.ShoesSize;
                    vm.ArmyDraftDate = player.ArmyDraftDate;
                    vm.MedicalInformation = player.MedicalInformation;
                    vm.AuditoriumId = player.AuditoriumId;
                }
            }
            var teamsPlayerValue = playersRepo.GetTeamPlayer(teamId, id, seasonId);
            if (ViewBag.SectionAlias == GamesAlias.Swimming)
            {
                ViewBag.MedicalInstitutes = unionsRepo.GetMedicalInstitutes(union.UnionId, seasonId);
                vm.MedicalInstitutesId = teamsPlayerValue.MedicalInstituteId ?? 0;
            }
            vm.PersonalCoachId = teamsPlayerValue?.PersonalCoachId;
            var coaches = usersRepo.GetClubTeamAndUnionCoaches(seasonId, union?.UnionId, clubId, teamId);
            vm.PersonalCoachesList = new List<SelectListItem>();
            vm.PersonalCoachesList.Add(new SelectListItem
            {
                Value = "0",
                Text = Messages.Select,
                Selected = vm.PersonalCoachId == null
            });
            vm.PersonalCoachesList.AddRange(coaches.Select(x => new SelectListItem
            {
                Value = x.UserId.ToString(),
                Text = x.FullName,
                Selected = x.UserId == vm.PersonalCoachId
            }));
            vm.NationalTeamInvitements = teamsPlayerValue != null
                ? GetNationalTeamInvitements(teamsPlayerValue.Id, seasonId)
                : new List<NationalTeamInvitementModel>();

            if (teamsPlayerValue?.User?.BlockadeId != null)
            {
                vm.IsBlockade = true;
                vm.BlockadeEndDate = teamsPlayerValue?.User?.PlayersBlockade?.EndDate;
            }


            if (teamsPlayerValue?.User?.PenaltyForExclusions?.Where(c => !c.IsCanceled)?.Any(c => !c.IsEnded) == true)
            {
                vm.IsUnderPenalty = true;
            }
            if (teamsPlayerValue != null)
            {
                vm.BlockadeHistory =
                    playersRepo.GetAllBlockadesForPlayer(teamsPlayerValue.UserId)
                    ?? Enumerable.Empty<BlockadeHistoryDTO>();
            }

            if (is31Union)
            {
                vm.BasketballFiveLevelReduction = leagueRepo.GetById(leagueId)?.FiveHandicapReduction;
            }

            if (TempData["ViewData"] != null)
            {
                ViewData = (ViewDataDictionary)TempData["ViewData"];
            }

            ViewBag.IsUnionManager = User.IsInRole(AppRole.Workers) &&
                                     usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager;
            ViewBag.IsUnionviewer = AuthSvc.AuthorizeUnionViewerByManagerId(union?.UnionId ?? 0, AdminId);
            ViewBag.IsAdmin = User.IsInAnyRole(AppRole.Admins);
            ViewBag.IsClubManager = User.IsInRole(AppRole.Workers) &&
                                    (usersRepo.GetTopLevelJob(AdminId) == JobRole.ClubManager || usersRepo.GetTopLevelJob(AdminId) == JobRole.ClubSecretary);
            ViewBag.IsPlayer = User.IsInRole(AppRole.Players);

            ViewBag.IsWaterpolo = secRepo.GetSectionByTeamId(teamId)?.Alias == "waterpolo" ? true : false;
            ViewBag.NetBall = secRepo.GetSectionByTeamId(teamId)?.Alias == "netball" ? true : false;
            ViewBag.IsGymnastic = secRepo.GetSectionByTeamId(teamId)?.Alias == GamesAlias.Gymnastic;
            ViewBag.IsWeightLifting = secRepo.GetSectionByTeamId(teamId)?.Alias == GamesAlias.WeightLifting;

            ViewBag.Is31Union = is31Union;
            ViewBag.Is38Union = is38Union;
            ViewBag.IsTennisRegistration = isTennisLeagueTeam;

            ViewBag.LeaguesOfSeason = playersRepo.GetTeamPlayersByUserIdAndSeasonId(id, seasonId).Where(tp => tp.LeagueId.HasValue && tp.League.LeagueTeams.FirstOrDefault(lt => lt.SeasonId == seasonId) != null).Select(tp => new SelectListItem { Text = tp.League.Name, Value = tp.League.LeagueId.ToString() }).Distinct().ToList();

            ViewBag.CountryList = new SelectList(getCountries(), "Key", "Value", vm.Nationality);
            ViewBag.CountryOfBirthList = new SelectList(getCountries(), "Key", "Value", vm.CountryOfBirth);

            var clubValue = clubsRepo.GetByUserId(id);
            vm.IsApproved = playersRepo.CheckApproveStatus(teamsPlayerValue) ?? playersRepo.CheckApproveStatus(id);
            vm.IsApprovedByManager = teamsPlayerValue != null ? teamsPlayerValue.IsApprovedByManager : playersRepo.CheckIfAllTeamPlayersApproved(id, seasonId);

            ViewBag.CantChangeIfAccepted = CantChangeIfAccepted(vm.IsApproved, union, clubValue);
            var isClubUnderUnionManager = (usersRepo.GetTopLevelJob(AdminId)?.Equals(JobRole.ClubManager) == true || usersRepo.GetTopLevelJob(AdminId)?.Equals(JobRole.ClubSecretary) == true)
                && clubValue?.UnionId.HasValue == true;
            ViewBag.IsUnionClubManagerUnderPastSeason = isClubUnderUnionManager && CheckIfIsUnderPastSeason(clubValue.ClubId, seasonId);
            if (departmentSettings.DepartmentId.HasValue)
            {
                vm.DepartmentId = departmentSettings.DepartmentId;
                vm.DepartmentSeasonId = departmentSettings.DepartmentSeasonId;
                vm.SportId = departmentSettings.SportId;
            }

            if (ViewBag.IsGymnastic == true || ViewBag.IsWeightLifting == true)
            {
                vm.InitialApprovalDate = db.Users.FirstOrDefault(c => c.UserId == id)?.InitialApprovalDates?.FirstOrDefault()?.InitialApprovalDate1;
            }

            if (is38Union)
            {
                var trainingTeams = unionsRepo.GetAllTrainingTeams(38, seasonId);
                var playerTrainingTeams = playersRepo.GetTrainingTeamsOfPlayer(id, 38, seasonId);

                if (trainingTeams?.Any() == true)
                {
                    vm.ListOfTrainingTeams = trainingTeams
                        .Select(c => new SelectListItem
                        {
                            Value = c.TeamId.ToString(),
                            Text = c.Team.TeamsDetails.FirstOrDefault(td => td.SeasonId == seasonId)?.TeamName
                                   ?? c.Team.Title,
                            Selected = playerTrainingTeams.Any(x => x.TeamId == c.TeamId)
                        })
                        .OrderBy(c => c.Text);
                }
                else
                {
                    vm.ListOfTrainingTeams = new List<SelectListItem>();
                }
            }

            //if (string.IsNullOrEmpty(vm.FirstName) && !string.IsNullOrEmpty(vm.FullName))
            //    vm.FirstName = playersRepo.GetFirstNameByFullName(vm.FullName);

            //if (string.IsNullOrEmpty(vm.LastName) && !string.IsNullOrEmpty(vm.FullName))
            //    vm.LastName = playersRepo.GetLastNameByFullName(vm.FullName);

            return View(vm);
        }

        private List<TeamDto> GetAthleticTeams(int? unionId, int clubId, int seasonId, bool isOnlySameClubTeams = false)
        {
            var result = new List<TeamDto>();

            List<Team> teams = new List<Team>();
            if (unionId > 0 && 
                (User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager)))
            {
                teams = teamRepo.GetByUnion(unionId.Value, seasonId).ToList();
            }

            if (clubId > 0 && (User.HasTopLevelJob(JobRole.ClubManager) || User.HasTopLevelJob(JobRole.ClubSecretary)))
            {
                teams = teamRepo.GetTeamsByClubAndSeasonId(clubId, seasonId);
            }
            List<Team> filteredTeams = new List<Team>();
            foreach (var t in teams)
            {
                var club = t.ClubTeams.FirstOrDefault(ct => ct.SeasonId == seasonId)?.Club;
                if ((club == null || !club.IsUnionArchive || (clubId != 0 && club.ClubId == clubId)) && (!isOnlySameClubTeams || (club != null && club.ClubId == clubId)))
                {
                    filteredTeams.Add(t);
                }
            }


            return filteredTeams?.Any() == true
                ? filteredTeams.Select(x => new TeamDto
                    {
                        TeamId = x.TeamId,
                        Title = x.TeamsDetails.FirstOrDefault(td => td.SeasonId == seasonId)?.TeamName
                                ?? x.Title,
                        ClubName = x.ClubTeams.FirstOrDefault(ct => ct.SeasonId == seasonId)?.Club?.Name ?? string.Empty
                })
                    .OrderBy(x => x.Title)
                    .ToList()
                : new List<TeamDto>();
        }

        private bool CheckIfIsUnderPastSeason(int clubId, int seasonId)
        {
            var isUnderPastSeason = false;
            var currentClubSeason = seasonsRepository.GetById(seasonId);
            var lastClubSeason = seasonsRepository.GetById(seasonsRepository.GetLastSeasonIdByCurrentClubId(clubId));
            if (currentClubSeason.EndDate.Year <= lastClubSeason.StartDate.Year || !currentClubSeason.IsActive)
                isUnderPastSeason = true;
            return isUnderPastSeason;
        }

        private bool CantChangeIfAccepted(bool isApproved, Union union, Club club)
        {
            var cantChange = false;

            if (isApproved)
            {
                if (club != null)
                {
                    cantChange = club.IsSectionClub == true &&
                        !(User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager) || User.HasTopLevelJob(JobRole.ClubManager) || User.HasTopLevelJob(JobRole.ClubSecretary))
                        || club.IsUnionClub == true && !(User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager));
                }
                else if (union != null)
                {
                    cantChange = !(User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager));
                }
                else
                {
                    cantChange = User.IsInAnyRole(AppRole.Players) && !(User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager));
                }
            }

            return cantChange;
        }

        public ActionResult CreatePenalty(int userId, int seasonId, int exclusionNumber, int[] leagueForExclusionIds)
        {
            playersRepo.CreatePenalty(userId, exclusionNumber, leagueForExclusionIds, AdminId);
            return RedirectToAction(nameof(PenaltyTable), new { id = userId, seasonId });
        }

        public ActionResult PenaltyTable(int id, int seasonId)
        {
            var penalties = playersRepo.GetPenaltiesForPlayer(id, seasonId);
            ViewBag.PlayerId = id;
            ViewBag.SeasonId = seasonId;
            return PartialView("_Penalties", penalties);
        }

        [HttpPost]
        public ActionResult UpdatePenalty(int id, int exclusionNumber)
        {
            var penalty = playersRepo.UpdatePenalty(id, exclusionNumber);
            return PartialView("_PenaltyItem", penalty);
        }

        [HttpPost]
        public void DeletePenalty(int id)
        {
            playersRepo.DeletePenalty(id);
        }


        private bool CheckReadonlyStatus(int userId, int seasonId, int clubId, int teamId, int leagueId)
        {
            if (usersRepo.GetTopLevelJob(AdminId) == JobRole.TeamManager || usersRepo.GetTopLevelJob(AdminId) == JobRole.ClubManager || usersRepo.GetTopLevelJob(AdminId) == JobRole.ClubSecretary)
            {
                //var isUnionTeam = teamRepo.IsUnionTeam(teamId, 
                //        clubsRepo.GetById(clubId)?.UnionId ?? leagueRepo.GetById(leagueId)?.Club?.UnionId ?? leagueRepo.GetById(leagueId)?.UnionId
                //        ,seasonId);
                var playerIsApproved = playersRepo
                    .GetTeamPlayerBySeasonId(teamId, userId, leagueId, clubId, seasonId)?.IsApprovedByManager ?? false;
                if (playerIsApproved) return true;
            }
            return false;
        }

        [HttpPost]
        [SetTempDataModelState]
        public ActionResult Edit(PlayerFormView model)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("he-IL");
            var savePath = Server.MapPath(GlobVars.ContentPath + "/players/");
            var maxFileSize = GlobVars.MaxFileSize * 1000;
            var isAdminRole = User.IsInRole(AppRole.Workers) &&
                                     usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager || User.IsInAnyRole(AppRole.Admins);
            //for bicycle 
            var isClubMng = false;

            var sectionAlias = secRepo.GetSectionByTeamId(model.CurrentTeamId)?.Alias ?? string.Empty;
            if(sectionAlias != null && sectionAlias.Equals(SectionAliases.Bicycle, StringComparison.OrdinalIgnoreCase))
            {
                isClubMng = User.HasTopLevelJob(JobRole.ClubManager);
                if (!isAdminRole) isAdminRole = isClubMng;
                else isClubMng = false;
            }
            ViewBag.IsSpecificUnionTopLevelJob = !User.IsInAnyRole(AppRole.Admins) && (User.HasTopLevelJob(JobRole.ClubManager) ||
                                   User.HasTopLevelJob(JobRole.ClubSecretary) ||
                                   User.HasTopLevelJob(JobRole.UnionManager));
            if (sectionAlias?.Equals(GamesAlias.Athletics) == true)
            {
                if (!string.IsNullOrEmpty(model.AthleteNumber.ToString()) && playersRepo.IsAthleteNumberUsed(model.AthleteNumber, model.UserId, model.SeasonId)) {
                    ModelState.AddModelError("AthleteNumber", Messages.AthleteNumberAlreadyExists);
                }
                if (!model.Insurance)
                {
                    if (!User.IsInAnyRole(AppRole.Admins) && usersRepo.GetTopLevelJob(AdminId) != JobRole.UnionManager)
                    {
                        ModelState.AddModelError("Insurance", Messages.Insurance_Required);
                    }
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

            if (!string.IsNullOrWhiteSpace(model.IdentNum))
            {
                var idNumAlreadyExist = db.Users.Any(x => x.IdentNum == model.IdentNum && x.UserId != model.UserId);
                if (idNumAlreadyExist)
                {
                    ModelState.AddModelError(nameof(model.IdentNum), string.Format(Messages.IdIsAlreadyExists, model.IdentNum));
                }
            }
            if (!string.IsNullOrWhiteSpace(model.PassportNum))
            {
                var passportAlreadyExist = db.Users.Any(x => x.PassportNum == model.PassportNum && x.UserId != model.UserId);
                if (passportAlreadyExist)
                {
                    ModelState.AddModelError(nameof(model.PassportNum), string.Format(Messages.PassportAlreadyExists, model.PassportNum));
                }
            }

            if (sectionAlias != null && sectionAlias.Equals(SectionAliases.Bicycle, StringComparison.OrdinalIgnoreCase))
            {
                if (!model.SaveAsDraft)
                {
                    if (string.IsNullOrEmpty(model.Email))
                    {
                        ModelState.AddModelError("Email", Messages.FieldIsRequired);
                    }
                    else
                    {
                        //var season = seasonsRepository.GetById(model.SeasonId);
                        //if (!usersRepo.CheckEmailWithinTheSameUnion(model.Email, season.UnionId.Value, model.SeasonId, model.UserId))
                        //{
                        //    ModelState.AddModelError("Email", string.Format(Messages.EmailAlreadyExists, model.Email));
                        //}

                    }

                    if (string.IsNullOrEmpty(model.ForeignFirstName))
                    {
                        ModelState.AddModelError("ForeignFirstName", Messages.FieldIsRequired);
                    }
                    if (string.IsNullOrEmpty(model.ForeignLastName))
                    {
                        ModelState.AddModelError("ForeignLastName", Messages.FieldIsRequired);
                    }

                    if ((model.FriendshipTypeId == null || model.FriendshipTypeId == -1 || model.FriendshipPriceType == null || model.FriendshipPriceType == -1) && isAdminRole)
                    {
                        ModelState.AddModelError("FriendshipTypeId", Messages.FieldIsRequired);
                        ModelState.AddModelError("FriendshipPriceType", Messages.FieldIsRequired);
                        ModelState.AddModelError("RoadDisciplineId", Messages.FieldIsRequired);
                        ModelState.AddModelError("MountaintDisciplineId", Messages.FieldIsRequired);
                    }

                    if (model.InsuranceTypeId == null)
                    {
                        ModelState.AddModelError("InsuranceTypeId", Messages.FieldIsRequired);
                    }
                    else
                        if (model.InsuranceTypeId == 5)
                    {
                        var fcheck = GetPostedFile("InsuranceFile");
                        if (fcheck == null || fcheck.ContentLength == 0)
                        {
                            var fileExists = playersRepo.CheckIfFileExits(PlayerFileType.Insurance, model.UserId);
                            if (!fileExists)
                            {
                                ModelState.AddModelError("InsuranceFile", Messages.FieldIsRequired);
                            }

                        }

                    }
                }
                else
                {
                    if (!ModelState.IsValid) ModelState.Remove("BirthDay");
                }
            }

            if (!ModelState.IsValid)
            {
                if (model.LeagueId != 0)
                {
                    return RedirectToAction("Edit",
                        new
                        {
                            id = model.UserId,
                            seasonId = model.SeasonId,
                            leagueId = model.LeagueId,
                            teamId = model.CurrentTeamId
                        });
                }
                if (!model.DepartmentId.HasValue)
                    return RedirectToAction("Edit", new { id = model.UserId, seasonId = model.SeasonId, clubId = model.ClubId, teamId = model.CurrentTeamId });
                else
                    return RedirectToAction("Edit", new
                    {
                        id = model.UserId,
                        seasonId = model.SeasonId,
                        clubId = model.ClubId,
                        teamId = model.CurrentTeamId,
                        departmentId = model.DepartmentId,
                        departmentSeasonId = model.DepartmentSeasonId,
                        sportId = model.SportId
                    });
            }

            var player = new User();
            var unionId = 0;
            var isClimbing = sectionAlias.Equals(SectionAliases.Climbing, StringComparison.OrdinalIgnoreCase);
            if (model.UserId != 0)
            {
                player = usersRepo.GetById(model.UserId);
                if ((model.UserId != AdminId || sectionAlias == GamesAlias.Bicycle) && model.CurrentTeamId > 0)
                {
                    var teamsPlayer = player.TeamsPlayers
                        .FirstOrDefault(x => x.UserId == player.UserId &&
                                             x.SeasonId == model.SeasonId &&
                                             (model.LeagueId > 0 ? x.LeagueId == model.LeagueId : x.LeagueId == null) &&
                                             (model.ClubId > 0 && model.LeagueId <= 0 ? x.ClubId == model.ClubId : x.ClubId == null) &&
                                             x.TeamId == model.CurrentTeamId);
                    if (teamsPlayer == null)
                    {
                        return RedirectToAction("NotFound", "Error");
                    }

                    var startPlayning = Request.Form["StartPlaying"];
                    teamsPlayer.StartPlaying = string.IsNullOrEmpty(startPlayning) ? null : (DateTime?)DateTime.Parse(startPlayning);

                    //teamsPlayer.HandicapLevel = model.HandicapLevel;
                    playersRepo.SetPlayerHandicapLevel(player.UserId, model.SeasonId, model.HandicapLevel);

                    teamsPlayer.User.MedExamDate = model.MedExamDate;
                    if (model.PersonalCoachId == 0)
                    {
                        model.PersonalCoachId = null;
                    }
                    teamsPlayer.PersonalCoachId = model.PersonalCoachId;
                    if (isAdminRole && !isClubMng){
                        teamsPlayer.MountainIronNumber = model.MountainIronNumber;
                        teamsPlayer.RoadIronNumber = model.RoadIronNumber;
                        teamsPlayer.VelodromeIronNumber = model.VelodromeIronNumber;
                        teamsPlayer.KitStatus = model.KitStatus;
                        teamsPlayer.TeamForUci = model.TeamForUci;
                    }
                    unionId = teamsPlayer?.Club?.Union?.UnionId ?? teamsPlayer?.League?.Club?.UnionId ?? teamsPlayer?.League?.UnionId ?? 0;
                    sectionAlias = teamsPlayer?.Club?.Union?.Section?.Alias ?? teamsPlayer?.Club?.Section?.Alias
                        ?? teamsPlayer?.League?.Club?.Union?.Section?.Alias ?? teamsPlayer?.League?.Club?.Section?.Alias
                        ?? teamsPlayer?.League?.Union?.Section?.Alias ?? string.Empty;
                    if (sectionAlias.Equals(GamesAlias.Tennis))
                    {
                        if (!User.IsInAnyRole(AppRole.Admins) && !(usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager))
                        {
                            BackToWaitingStatus(player);
                        }
                    }
                    if (sectionAlias == GamesAlias.Swimming)
                    {
                        teamsPlayer.User.ClassS = model.ClassS;
                        teamsPlayer.User.ClassSB = model.ClassSB;
                        teamsPlayer.User.ClassSM = model.ClassSM;
                        teamsPlayer.Masters = model.Masters;
                        if (model.MedicalInstitutesId == 0)
                        {
                            teamsPlayer.MedicalInstituteId = null;
                        }
                        else
                        {
                            teamsPlayer.MedicalInstituteId = model.MedicalInstitutesId;
                        }
                    }
                    if(sectionAlias == GamesAlias.Bicycle && isAdminRole)
                    {
                        teamsPlayer.FriendshipTypeId = model.FriendshipTypeId;
                        if(model.FriendshipTypeId == null)
                        {
                            model.FriendshipPriceType = null;
                            model.RoadDisciplineId = null;
                            model.MountaintDisciplineId = null;
                        }
                        teamsPlayer.FriendshipPriceType = model.FriendshipPriceType != null && model.FriendshipPriceType < 0 ? null : model.FriendshipPriceType;
                        teamsPlayer.RoadDisciplineId = model.RoadDisciplineId != null && model.RoadDisciplineId < 0 ? null : model.RoadDisciplineId;
                        teamsPlayer.MountaintDisciplineId = model.MountaintDisciplineId != null && model.MountaintDisciplineId < 0 ? null : model.MountaintDisciplineId;
                        
                    }

                    if(sectionAlias == GamesAlias.Bicycle && !model.SaveAsDraft)
                    {
                        teamsPlayer.IsActive = true;
                    }
                }
            }
            else
            {
                usersRepo.Create(player);
            }

            var seasonAthleteNumber = model.AthleteNumber;
            var seasonAthleteNumberFromDb =
                player.AthleteNumbers.FirstOrDefault(x => x.SeasonId == model.SeasonId);
            if (seasonAthleteNumberFromDb != null)
            {
                seasonAthleteNumberFromDb.AthleteNumber1 = seasonAthleteNumber;
            }
            else
            {
                db.AthleteNumbers.Add(new AthleteNumber
                {
                    AthleteNumber1 = seasonAthleteNumber,
                    SeasonId = model.SeasonId,
                    UserId = player.UserId
                });
            }

            if (sectionAlias == SectionAliases.Bicycle) {
                if(isAdminRole && !isClubMng)
                {
                    player.PaymentForChipNumber = model.PaymentForChipNumber;
                    player.PaymentForUciId = model.PaymentForUciId;

                    player.UciId = model.UciId;
                    player.ChipNumber = model.ChipNumber;
                }
                else
                {
                    model.UciId = player.UciId;
                    model.ChipNumber = player.ChipNumber;
                    model.HeatTypeForUciCard = player.HeatTypeForUciCard;
                }
            }

            player.PassportValidity = model.PassportValidity;
            player.IsReligious = model.IsReligious;
            UpdateModel(player);

            if (isClimbing || sectionAlias == GamesAlias.Swimming)
            {
                if (isAdminRole)
                {
                    player.IsNationalSportsman = model.IsNationalSportsman;
                }
                else
                {
                    model.IsNationalSportsman = player.IsNationalSportsman == true;
                }

                if (model.IsNationalSportsman)
                {
                    player.ShoesSize = model.ShoesSize;
                    player.ArmyDraftDate = model.ArmyDraftDate;
                    player.MedicalInformation = model.MedicalInformation;
                }

                player.AuditoriumId = model.AuditoriumId;

                if (sectionAlias == GamesAlias.Swimming)
                {
                    var season = seasonsRepository.GetById(model.SeasonId);
                    var unionIdValue = season?.UnionId.Value;
                    var nationalClub = db.Clubs.FirstOrDefault(x =>
                        x.SeasonId == model.SeasonId && x.UnionId == unionIdValue && x.IsNationalTeam);
                    if (nationalClub != null)
                    {
                        var nationalTeamsIds = nationalClub.ClubTeams
                            .Where(x => x.SeasonId == model.SeasonId && !x.IsBlocked).Select(x => x.TeamId).ToList();
                        foreach (var nationalTeamId in nationalTeamsIds)
                        {
                            db.TeamsPlayers.Add(new TeamsPlayer
                            {
                                SeasonId = model.SeasonId,
                                UserId = player.UserId,
                                TeamId = nationalTeamId,
                                ClubId = nationalClub.ClubId
                            });
                        }
                    }
                }
            }            

            player.LicenseLevel = model.LicenseLevelId;

            if (sectionAlias.Equals(SectionAliases.Gymnastic, StringComparison.OrdinalIgnoreCase) && isAdminRole)
            {
                if (player.InitialApprovalDates != null && player.InitialApprovalDates.Any())
                {
                    var userApproval = player.InitialApprovalDates.FirstOrDefault();
                    if (model.InitialApprovalDate.HasValue)
                        userApproval.InitialApprovalDate1 = model.InitialApprovalDate.Value;
                    else
                        player.InitialApprovalDates.Remove(userApproval);
                }
                else
                {
                    if (model.InitialApprovalDate.HasValue)
                        player.InitialApprovalDates.Add(new InitialApprovalDate
                        {
                            UserId = player.UserId,
                            InitialApprovalDate1 = model.InitialApprovalDate.Value,
                            UnionId = unionId
                        });
                }
            }

            int? newClubId = null;
            int? newTeamId = null;
            
            if (string.Equals(sectionAlias, SectionAliases.Tennis, StringComparison.CurrentCultureIgnoreCase))
            {
                var currentTrainingTeams = playersRepo.GetTrainingTeamsOfPlayer(model.UserId, unionId, model.SeasonId);

                if (model.TrainingTeamsIds?.Any() == true)
                {
                    foreach (var trainingTeamId in model.TrainingTeamsIds)
                    {
                        //process new training teams, add player to it
                        if (currentTrainingTeams.All(x => x.TeamId != trainingTeamId))
                        {
                            newClubId = teamRepo
                                .GetById(trainingTeamId)
                                ?.ClubTeams
                                ?.FirstOrDefault(x => !x.Club.IsArchive && x.IsTrainingTeam && x.SeasonId == model.SeasonId)
                                ?.ClubId;

                            if (newClubId.HasValue)
                            {
                                player.TeamsPlayers.Add(new TeamsPlayer
                                {
                                    TeamId = trainingTeamId,
                                    UserId = player.UserId,
                                    IsActive = true,
                                    SeasonId = model.SeasonId,
                                    ClubId = newClubId
                                });

                                newTeamId = trainingTeamId;
                            }
                        }
                    }

                    //process removed teams
                    foreach (var currentTrainingTeam in currentTrainingTeams.Where(x => !model.TrainingTeamsIds.Contains(x.TeamId)))
                    {
                        playersRepo.RemoveFromTeam(currentTrainingTeam);
                    }
                    if (currentTrainingTeams.Any(x => !model.TrainingTeamsIds.Contains(x.TeamId)))
                    {
                        //if player was removed from team, get first team where player still exist to set new club id and team id
                        var trainingTeam = currentTrainingTeams.FirstOrDefault(x => model.TrainingTeamsIds.Contains(x.TeamId));

                        if (trainingTeam != null)
                        {
                            newClubId = trainingTeam.ClubId;
                            newTeamId = trainingTeam.TeamId;
                        }
                        else if(!newClubId.HasValue && !newTeamId.HasValue) //no teams left and no new teams were assigned
                        {
                            newClubId = model.ClubId;
                            newTeamId = 0;
                        }
                    }
                }

                if (model.TrainingTeamsIds?.Any() != true && currentTrainingTeams.Any()) //all teams removed
                {
                    currentTrainingTeams.ForEach(x => player.TeamsPlayers.Remove(x));

                    newClubId = 0;
                    newTeamId = 0;
                }
            }
            
            if (sectionAlias.Equals(SectionAliases.Athletics) &&
                model.AthleticTeamId.HasValue &&
                model.AthleticTeamId != model.CurrentTeamId)
            {
                newClubId = teamRepo.GetById(model.AthleticTeamId.Value)
                    ?.ClubTeams
                    ?.FirstOrDefault(x => !x.Club.IsArchive && x.SeasonId == model.SeasonId)
                    ?.ClubId;
                if (newClubId.HasValue)
                {
                    playersRepo.MovePlayersToTeam(model.AthleticTeamId.Value, 0, newClubId.Value, new[] {model.UserId},
                        model.CurrentTeamId, model.SeasonId, model.LeagueId, model.ClubId, AdminId);

                    newTeamId = model.AthleticTeamId.Value;
                }
            }

            if (newClubId.HasValue)
            {
                //Why model state modified manually: https://stackoverflow.com/questions/47871015/
                ModelState.Remove(nameof(model.ClubId));
                ModelState.Remove(nameof(model.CurrentTeamId));
                ModelState.Remove(nameof(model.ListOfTrainingTeams));
                ModelState.Remove(nameof(model.TrainingTeamsIds));

                if (model.LeagueId <= 0)
                {
                    model.ClubId = newClubId.Value;
                    model.CurrentTeamId = newTeamId.Value;
                }
            }

            // images will be processed separately
            //pl.MedicalCertificateFile = medicalCertificateFile;
            //pl.InsuranceFile = insuranceFile;

            player.Password = Protector.Encrypt(model.Password);
            player.TypeId = 4;

            var medCertApprovement = player.MedicalCertApprovements.FirstOrDefault(x => x.SeasonId == model.SeasonId);
            if (medCertApprovement != null)
            {
                if (sectionAlias == GamesAlias.Tennis) {
                    if (User.IsInAnyRole(AppRole.Admins) || User.IsInRole(AppRole.Workers) && usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager)
                    {
                        medCertApprovement.Approved = model.MedicalCertificate;
                    }
                    else
                    {
                        medCertApprovement.Approved = false;
                    }
                }
            }
            else
            {               
                player.MedicalCertApprovements.Add(new MedicalCertApprovement
                {
                    Approved = model.MedicalCertificate,
                    SeasonId = model.SeasonId
                });              
            }

            if (sectionAlias == GamesAlias.Tennis)
            {
                if (!(User.IsInAnyRole(AppRole.Admins) || User.IsInRole(AppRole.Workers) && usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager))
                {
                    player.MedExamDate = null;
                }
            }



            if (ModelState.IsValid)
            {
                usersRepo.Save();
            }

            var secAlisas = sectionAlias == GamesAlias.Bicycle ? GamesAlias.Bicycle : "";

            if (User.IsInAnyRole(AppRole.Admins) || User.IsInRole(AppRole.Workers) && usersRepo.GetTopLevelJob(AdminId) == JobRole.UnionManager)
            {
                playersRepo.SetActive(model.PlayerTeams, model.UserId, model.SeasonId, sectionAlias);            }
           

            ProcessUserFile(player, PlayerFileType.PlayerImage, model.RemoveImage, model.SeasonId);
            ProcessUserFile(player, PlayerFileType.Insurance, model.RemoveInsuranceFile, model.SeasonId);
            ProcessUserFile(player, PlayerFileType.MedicalCertificate, model.RemoveMedicalCertificateFile, model.SeasonId, secAlisas);
            ProcessUserFile(player, PlayerFileType.DriverLicense, model.RemoveDriverLicenseFile, model.SeasonId);
            ProcessUserFile(player, PlayerFileType.ParentStatement, model.RemoveParentStatementFile, model.SeasonId);
            ProcessUserFile(player, PlayerFileType.ParentApproval, model.RemoveParentApprovalFile, model.SeasonId);
            if (sectionAlias == GamesAlias.Bicycle)
            {
                ProcessUserFile(player, PlayerFileType.SpecialClassificationFile, model.RemoveSpecialClassificationFile, model.SeasonId);
            }

            var teamPlayer = playersRepo.GetTeamPlayer(model.CurrentTeamId, model.UserId, model.SeasonId);
            var idFile = GetPostedFile("IDFilePost");
            if (model.RemoveIDFile && idFile == null)
            {
                player.IDFile = null;
                if(sectionAlias == GamesAlias.Bicycle) ModelState.AddModelError("IDFilePost", Messages.FieldIsRequired);
            }

            if (idFile != null)
            {
                if (idFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("IDFilePost", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(idFile, player.UserId, PlayerFileType.IDFile);
                    if (newName == null)
                    {
                        ModelState.AddModelError("IDFilePost", Messages.FileError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(player.IDFile))
                            FileUtil.DeleteFile(savePath + player.IDFile);

                        player.IDFile = newName;
                    }
                    //add bicycle/martial art check and update player for medical file
                    if (sectionAlias.ToLower() == SectionAliases.MartialArts.ToLower())
                    {                        
                        if (teamPlayer != null && teamPlayer.IsApprovedByManager == true)
                        {
                            teamPlayer.IsApprovedByManager = null;
                            teamPlayer.ApprovalDate = null;
                        }
                    }
                }
            }

            var passportFile = GetPostedFile("PassportFilePost");
            if (model.RemovePassportFile == true && passportFile == null)
            {
                player.PassportFile = null;
            }

            if (passportFile != null)
            {
                if (passportFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("PassportFilePost", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(passportFile, player.UserId, PlayerFileType.PassportFile);
                    if (newName == null)
                    {
                        ModelState.AddModelError("PassportFilePost", Messages.FileError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(player.PassportFile))
                            FileUtil.DeleteFile(savePath + player.PassportFile);

                        player.PassportFile = newName;
                    }
                }
            }

            var playerDisciplines = player.PlayerDisciplines.Where(x => x.ClubId == model.ClubId && x.SeasonId == model.SeasonId).ToList();

            if (model.DisciplinesIds != null && !model.DisciplinesIds.SequenceEqual(playerDisciplines.Select(x => x.DisciplineId)))
            {
                foreach (var disciplineId in model.DisciplinesIds)
                {
                    var discipline = playerDisciplines.FirstOrDefault(x => x.DisciplineId == disciplineId);

                    if (discipline == null)
                    {
                        player.PlayerDisciplines.Add(new PlayerDiscipline
                        {
                            SeasonId = model.SeasonId,
                            ClubId = model.ClubId,
                            DisciplineId = disciplineId
                        });
                    }
                }

                var disciplinesToRemove = playerDisciplines.Where(x => !model.DisciplinesIds.Contains(x.DisciplineId)).ToList();

                var routesOfRemovedDisciplines = disciplinesToRemove
                    .SelectMany(x => x.Discipline.DisciplineRoutes.Select(d => d.Id))
                    .ToArray();
                var userRanksToRemove = player.UsersRanks
                    .Where(x => routesOfRemovedDisciplines.Contains(x.UsersRoute.RouteId))
                    .ToList();

                foreach (var disciplineToRemove in disciplinesToRemove)
                {
                    db.Entry(disciplineToRemove).State = EntityState.Deleted;
                }

                foreach (var userRankToRemove in userRanksToRemove)
                {
                    db.UsersRoutes.Remove(userRankToRemove.UsersRoute);
                    db.UsersRanks.Remove(userRankToRemove);
                }

                var teamsByDisciplines =
                    teamRepo.GetCollection<TeamDiscipline>(
                            x => x.ClubId == model.ClubId &&
                                 x.SeasonId == model.SeasonId &&
                                 model.DisciplinesIds.Contains(x.DisciplineId))
                        .ToList();

                var removeFromTeams =
                    player.TeamsPlayers.Where(x => x.ClubId == model.ClubId &&
                                               x.SeasonId == model.SeasonId &&
                                               x.IsActive &&
                                               !teamsByDisciplines.Select(td => td.TeamId).Contains(x.TeamId))
                        .ToList();

                foreach (var removeFromTeam in removeFromTeams)
                {
                    if (removeFromTeam.TeamId == model.CurrentTeamId)
                    {
                        model.CurrentTeamId = 0;
                    }
                    db.Entry(removeFromTeam).State = EntityState.Deleted;
                }

                foreach (var teamsByDiscipline in teamsByDisciplines)
                {
                    if (!player.TeamsPlayers.Any(x => x.ClubId == model.ClubId && x.SeasonId == model.SeasonId && x.TeamId == teamsByDiscipline.TeamId))
                    {
                        player.TeamsPlayers.Add(new TeamsPlayer
                        {
                            TeamId = teamsByDiscipline.TeamId,
                            ShirtNum = 0,
                            IsActive = true,
                            SeasonId = model.SeasonId,
                            ClubId = model.ClubId
                        });
                    }
                }
            }

            
            if (teamPlayer != null)
            {
                SaveNationalInvitements(model.NationalTeamInvitements, teamPlayer.Id, model.SeasonId);
            }
            ViewBag.LeaguesOfSeason = playersRepo.GetTeamPlayersByUserIdAndSeasonId(model.UserId, model.SeasonId).Where(tp => tp.LeagueId.HasValue && tp.League.LeagueTeams.FirstOrDefault(lt => lt.SeasonId == model.SeasonId) != null).Select(tp => new SelectListItem { Text = tp.League.Name, Value = tp.League.LeagueId.ToString() }).Distinct().ToList();

            player.IdentNum = model.IdentNum;
            player.PassportNum = model.PassportNum;
            player.Email = model.Email;

            player.FirstName = model.FirstName;
            player.LastName = model.LastName;
            player.MiddleName = model.MiddleName;
            player.Nationality = model.Nationality;
            player.CountryOfBirth = model.CountryOfBirth;
            model.FullNameFormatted = player.FullName;

            if (!User.IsInAnyRole(AppRole.Admins) && !User.HasTopLevelJob(JobRole.UnionManager) && model.CurrentTeamId != 0)
            {
                if ((teamPlayer.Club?.Union?.Section?.Alias == GamesAlias.Tennis || teamPlayer.Season?.Union?.Section?.Alias == GamesAlias.MartialArts) && teamPlayer.IsApprovedByManager.HasValue && teamPlayer.IsApprovedByManager.Value && model.MedicalCertificateFile != null)
                {
                    teamPlayer.IsApprovedByManager = null;
                }
            }
            if (model.IsBlockade)
            {
                playersRepo.BlockadePlayer(new List<int> { player.UserId }, model.BlockadeEndDate, model.SeasonId, AdminId);
            }
            else
            {
                if (player.PlayersBlockade != null)
                {
                    player.BlockadeId = null;
                    player.PlayersBlockade.IsActive = false;
                }
            }
            if (ModelState.IsValid)
            {
                usersRepo.Save();
                TempData["Saved"] = true;
            }
            else
            {
                TempData["ViewData"] = ViewData;
            }

            return RedirectToAction("Edit",
                new
                {
                    id = model.UserId,
                    seasonId = model.SeasonId,
                    leagueId = model.LeagueId,
                    clubId = model.ClubId,
                    teamId = model.CurrentTeamId
                });
            //return Redirect(Request.UrlReferrer.PathAndQuery);
        }

        [HttpGet]
        public ActionResult FriendshipCard(int seasonId, int userId)
        {

            var user = usersRepo.GetById(userId);
            var season = seasonsRepository.GetById(seasonId);
            var union = season.Union;

            var vm = new FriendshipCardModel()
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirthDay = user.BirthDay,
                GenderId = user.GenderId,
                //GenderName = user.Gender?.Title,
                IdentNum = user.IdentNum,
                PlayerImage = user.PlayerFiles.FirstOrDefault(x => x.FileType == (int)PlayerFileType.PlayerImage)?.FileName,
                UciId = user.UciId,
                UnionLogo = union.Logo,
                UnionName = union.Name,
                ContentForFriendshipCard = union.ContentForFriendshipCard,
                SeasonId = seasonId,
                SeasonName = season.Name,
                IsHebrew = IsHebrew,
                UnionForeignName = union.UnionForeignName
            };

            if (vm.GenderId == 0)
                vm.GenderName = Messages.Female;
            else
                vm.GenderName = Messages.Male;

            var teamsPlayer = user.TeamsPlayers.FirstOrDefault(x => x.SeasonId == seasonId);
            if(teamsPlayer != null)
            {
                vm.ClubName = teamsPlayer.Club.Name;
                vm.RoadHeatName = teamsPlayer.Discipline?.Name;
                vm.MountainHeatName = teamsPlayer.Discipline1?.Name;
            }

            return View(vm);
        }

        [HttpGet]
        public ActionResult UciCard(int seasonId, int userId)
        {

            var user = usersRepo.GetById(userId);
            var season = seasonsRepository.GetById(seasonId);
            var union = season.Union;
            var teamsPlayer = user.TeamsPlayers.FirstOrDefault(x => x.SeasonId == seasonId);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Locales.En_US);

            var playerAge = Convert.ToInt32(season.Name) - user.BirthDay.Value.Year;
            var competitionages = db.CompetitionAges.Where(x => x.UnionId == season.UnionId && x.from_age <= playerAge && x.to_age >= playerAge && (x.gender == user.GenderId || x.gender == 3) && x.FriendshipTypeId == teamsPlayer.FriendshipTypeId).ToList();

            var uciCategory = competitionages.FirstOrDefault(x => x.IsUCICategory == true);
            var userJob = user.UsersJobs.FirstOrDefault(x => x.SeasonId == seasonId);

            var vm = new UciCardModel()
            {
                UserId = user.UserId,
                FirstName = user.ForeignFirstName,
                LastName = user.ForeignLastName,
                BirthDay = user.BirthDay,
                GenderId = user.GenderId,
                //GenderName = user.Gender?.Title,
                IdentNum = user.IdentNum,
                PlayerImage = user.PlayerFiles.FirstOrDefault(x => x.FileType == (int)PlayerFileType.PlayerImage)?.FileName,
                UciId = user.UciId,
                UnionLogo = union.Logo,
                UnionName = union.Name,
                ContentForFriendshipCard = union.ContentForFriendshipCard,
                SeasonId = seasonId,
                SeasonName = season.SeasonForAge?.ToString(),
                IsHebrew = IsHebrew,
                UnionForeignName = union.UnionForeignName,
                Nationality = getCountryCode(user.Nationality),
                Function = userJob?.Function, //check this
                UciCategory = uciCategory?.age_name,
                UnionAddress = union.ForeignAddress,
                UnionWebSite = union.UnionWebSite,
                UnionEmail = union.Email,
                UnionPhone = union.ContactPhone,
                EmergencyContact = user.ParentName + ", " + user.ParentPhone
            };

            if (vm.GenderId == 0)
                vm.GenderName = Messages.Female;
            else
                vm.GenderName = Messages.Male;

            if(user.HeatTypeForUciCard == 1)
            {
                vm.NationalCategory = competitionages.FirstOrDefault(x => x.IsUCICategory == true && x.Discipline.RoadHeat == true)?.age_foreign_name;
            }
            else 
                if(user.HeatTypeForUciCard == 2)
                {
                    vm.NationalCategory = competitionages.FirstOrDefault(x => x.IsUCICategory == true && x.Discipline.MountainHeat == true)?.age_foreign_name;
                }


            if (user.UsersType.TypeRole == AppRole.Players)
            {
                vm.Role = UIHelpers.GetPlayerCaption(SectionAliases.Bicycle, false);
            }
            else
            {
                vm.Role = Messages.TeamStaff;
            }

            
            if (teamsPlayer != null)
            {
                vm.ClubName = teamsPlayer.Club.ForeignName;
                vm.RoadHeatName = teamsPlayer.Discipline?.Name;
                vm.MountainHeatName = teamsPlayer.Discipline1?.Name;
                vm.TeamForUci = teamsPlayer.TeamForUci;
            }

            return View("_UciCard", vm);
        }

        private void BackToWaitingStatus(User user)
        {
            var insurance = GetPostedFile("InsuranceFile");
            var certificate = GetPostedFile("MedicalCertificateFile");
            if (insurance != null || certificate != null)
            {
                var teamPlayers = user.TeamsPlayers;
                foreach (var teamsPlayer in teamPlayers)
                {
                    teamsPlayer.IsApprovedByManager = null;
                    teamsPlayer.ApprovalDate = null;
                }
            }
        }

        private IEnumerable<NationalTeamInvitementModel> GetNationalTeamInvitements(int id, int? seasonId)
        {
            var nationalTeamInvs = playersRepo.GetAllNatTeamInvitements(id, seasonId);
            var result = new List<NationalTeamInvitementModel>();
            if (nationalTeamInvs != null)
            {
                foreach (var natTeamInv in nationalTeamInvs)
                {
                    result.Add(new NationalTeamInvitementModel
                    {
                        StartDate = natTeamInv.StartDate,
                        EndDate = natTeamInv.EndDate
                    });
                }
            }
            return result;
        }

        private void SaveNationalInvitements(IEnumerable<NationalTeamInvitementModel> newInvitementsValues, int teamPlayerId, int? seasonId)
        {
            try
            {
                var currentInvitementsValues = GetNationalTeamInvitements(teamPlayerId, seasonId).ToList();
                if (newInvitementsValues != null)
                {
                    var nationalTeamDto = new List<NationalTeamInvitementDTO>();
                    foreach (var newInvitementsValue in newInvitementsValues)
                    {
                        nationalTeamDto.Add(new NationalTeamInvitementDTO
                        {
                            StartDate = newInvitementsValue.StartDate,
                            EndDate = newInvitementsValue.EndDate
                        });
                    }

                    if (currentInvitementsValues != null && currentInvitementsValues.Count > 0)
                    {
                        playersRepo.UpdateInvitementValue(nationalTeamDto, teamPlayerId, seasonId);
                    }
                    else
                    {
                        if (newInvitementsValues != null)
                        {
                            playersRepo.SaveInvitementValue(nationalTeamDto, teamPlayerId, seasonId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public ActionResult ImportPlayers(string formName, int? clubId = null, int? leagueId = null, int? teamId = null,
            int? unionId = null, int? seasonId = null, string updateTargetId = null)
        {
            var model = new ImportPlayersViewModel
            {
                ClubId = clubId,
                ImportFile = null,
                LeagueId = leagueId,
                TeamId = teamId,
                UnionId = unionId,
                SeasonId = seasonId,
                FormName = formName,
                UpdateTargetId = updateTargetId ?? "playerimportform_pl"
            };

            var clubManagerUnderSection = false;
            if (clubId != null)
            {
                var club = clubsRepo.GetById(clubId.Value);

                clubManagerUnderSection = club.SectionId != null &&
                                          (User.HasTopLevelJob(JobRole.ClubManager) ||
                                           User.HasTopLevelJob(JobRole.ClubSecretary));
            }

            model.CanApprovePlayers = User.IsInAnyRole(AppRole.Admins) ||
                                      User.HasTopLevelJob(JobRole.UnionManager) ||
                                      clubManagerUnderSection;

            return PartialView("_ImportPlayers", model);
        }

        [HttpPost]
        public ActionResult ImportPlayers(ImportPlayersViewModel model)
        {
            try
            {
                List<ImportPlayerModel> validationErrorRows;
                List<ImportPlayerModel> correctRows;

                if (model.ImportFile != null)
                {
                    var importHelper = new ImportExportPlayersHelper(db);

                    model.IsTennis = false;
                    model.IsSectionClub = false;
                    var sectionAlias = "";

                    if (model.ClubId > 0)
                    {
                        sectionAlias = secRepo.GetAlias(null, model.ClubId.Value, null);

                        model.IsTennis = string.Equals(SectionAliases.Tennis, sectionAlias, StringComparison.CurrentCultureIgnoreCase);
                        model.IsSectionClub = clubsRepo.GetById(model.ClubId.Value)?.IsSectionClub == true;
                    }
                    else if (model.UnionId > 0)
                    {
                        sectionAlias = secRepo.GetAlias(model.UnionId, null, null);

                        model.IsTennis = string.Equals(SectionAliases.Tennis, sectionAlias, StringComparison.CurrentCultureIgnoreCase);
                    }

                    if(sectionAlias == SectionAliases.Bicycle)
                    {
                        importHelper.InitiateFieldsForBicycle(model.UnionId, model.SeasonId.Value);
                    }

                    importHelper.ExtractData(model.ImportFile.InputStream, out correctRows, out validationErrorRows, model.IsTennis, model.IsSectionClub, sectionAlias);

                    if (correctRows.Count > 0)
                    {
                        var suitableTeams = new List<ImportPlayerAllowedTeamModel>();

                        switch (model.FormName)
                        {
                            case "club":
                                if (model.ClubId > 0 && model.SeasonId > 0)
                                {
                                    var clubTeams =
                                        teamRepo.GetClubTeamsByClubAndSeasonId(model.ClubId.Value,
                                            model.SeasonId.Value);
                                    suitableTeams.AddRange(clubTeams.Select(x => new ImportPlayerAllowedTeamModel
                                    {
                                        TeamId = x.TeamId,
                                        ClubId = model.ClubId
                                    }));
                                }

                                break;

                            case "team":
                                if (model.TeamId > 0)
                                {
                                    suitableTeams.Add(new ImportPlayerAllowedTeamModel
                                    {
                                        TeamId = model.TeamId.Value,
                                        LeagueId = model.LeagueId,
                                        ClubId = model.ClubId
                                    });
                                }
                                break;

                            case "union":
                                if (model.UnionId > 0 && model.SeasonId > 0)
                                {
                                    var leagues = leagueRepo.GetByUnion(model.UnionId.Value, model.SeasonId.Value).ToList();
                                    foreach (var league in leagues)
                                    {
                                        var leagueTeams = teamRepo.GetTeamsByLeague(league.LeagueId);
                                        suitableTeams.AddRange(leagueTeams.Select(x => new ImportPlayerAllowedTeamModel
                                        {
                                            TeamId = x.TeamId,
                                            LeagueId = league.LeagueId
                                        }));
                                    }

                                    if (model.IsTennis)
                                    {
                                        var trainingTeams =
                                            unionsRepo.GetAllTrainingTeams(model.UnionId.Value, model.SeasonId.Value);
                                        suitableTeams.AddRange(trainingTeams.Select(x => new ImportPlayerAllowedTeamModel
                                        {
                                            TeamId = x.TeamId,
                                            ClubId = x.ClubId
                                        }));
                                    }

                                    if(sectionAlias == SectionAliases.Bicycle)
                                    {
                                        var teams = unionsRepo.GetAllTeamsByUnionId(model.UnionId.Value, model.SeasonId.Value);
                                        suitableTeams.AddRange(teams.Select(x => new ImportPlayerAllowedTeamModel
                                        {
                                            TeamId = x.TeamId,
                                            ClubId = x.ClubId
                                        }));
                                    }
                                }

                                break;

                            case "league":
                                if (model.LeagueId > 0)
                                {
                                    var teams = teamRepo.GetTeamsByLeague(model.LeagueId.Value);
                                    suitableTeams.AddRange(teams.Select(x => new ImportPlayerAllowedTeamModel
                                    {
                                        TeamId = x.TeamId,
                                        LeagueId = model.LeagueId
                                    }));
                                }
                                break;
                        }

                        List<ImportPlayerModel> importErrorRows = null;
                        List<ImportPlayerModel> duplicatedRows = null;
                        model.SuccessCount = importHelper.ImportPlayers(suitableTeams, model.SeasonId, model.LeagueId,
                            model.ClubId, correctRows,
                            out importErrorRows,
                            out duplicatedRows,
                            model.ApprovePlayersOnImport,
                            model.SetPlayersOnlyAsActiveOnImport,
                            sectionAlias);

                        validationErrorRows.AddRange(importErrorRows);

                        if (validationErrorRows.Count > 0 || duplicatedRows.Count > 0)
                        {
                            CreateErrorImportFile(importHelper, model, validationErrorRows, duplicatedRows, model.IsTennis, model.IsSectionClub, sectionAlias);

                            model.Result = ImportPlayersResult.PartialyImported;

                        }
                        else
                        {
                            model.Result = ImportPlayersResult.Success;
                        }

                        model.ErrorCount = validationErrorRows.Count;
                        model.DuplicateCount = duplicatedRows.Count;
                    }
                    else
                    {
                        model.Result = ImportPlayersResult.PartialyImported;
                        model.ResultMessage = Messages.ImportPlayers_NoRowLoaded;
                        model.SuccessCount = 0;
                        model.DuplicateCount = 0;
                        model.ErrorCount = validationErrorRows.Count;

                        CreateErrorImportFile(importHelper, model, validationErrorRows, null, model.IsTennis, model.IsSectionClub, sectionAlias);
                    }
                }
                else
                {
                    model.Result = ImportPlayersResult.Error;
                    model.ResultMessage = Messages.ImportPlayers_ChooseFile;
                }

                return PartialView("_ImportPlayers", model);
            }
            catch (Exception ex)
            {
                log.Error("Import players exception", ex);
                model.Result = ImportPlayersResult.Error;
                model.ResultMessage = Messages.ImportPlayers_ImportException;

                return PartialView("_ImportPlayers", model);
            }
        }

        [HttpGet]
        public ActionResult ImportPlayersImage(int seasonId)
        {
            var model = new ImportPlayersImageViewModel { SeasonId = seasonId };
            return PartialView("_ImportPlayersImage", model);
        }

        [HttpPost]
        public ActionResult ImportPlayersImage(ImportPlayersImageViewModel model)
        {
            try
            {
                if (model.ImportFile.Any())
                {

                    var maxFileSize = GlobVars.MaxFileSize * 1000;
                    var savePath = Server.MapPath(GlobVars.ContentPath + "/players/");

                    var importResult = new List<string>();

                    foreach (var imageFile in model.ImportFile)
                    {
                        if (imageFile != null)
                        {
                            var fileName = imageFile.FileName;
                            var fi = new FileInfo(fileName);


                            var identNumber = fi.Name.Replace(fi.Extension, "");

                            while (identNumber.Length < 9)
                            {
                                identNumber = "0" + identNumber;
                            }

                            var user = usersRepo.GetByIdentityNumber(identNumber);
                            if (user != null)
                            {

                                if (imageFile.ContentLength > maxFileSize)
                                {
                                    importResult.Add(string.Format("{0} - {1}", imageFile.FileName, Messages.FileSizeError));
                                }
                                else
                                {
                                    var newFileName = SaveFile(imageFile, user.UserId, PlayerFileType.PlayerImage);
                                    if (newFileName == null)
                                    {
                                        importResult.Add(string.Format("{0} - {1}", imageFile.FileName, Messages.FileError));
                                    }
                                    else
                                    {
                                        var existingImage = user.PlayerFiles.FirstOrDefault(x =>
                                            x.FileType == (int)PlayerFileType.PlayerImage &&
                                            x.SeasonId == model.SeasonId);

                                        if (!string.IsNullOrEmpty(existingImage?.FileName))
                                        {
                                            FileUtil.DeleteFile(savePath + existingImage.FileName);

                                            existingImage.FileName = newFileName;
                                        }
                                        else
                                        {
                                            user.PlayerFiles.Add(new PlayerFile
                                            {
                                                DateCreated = DateTime.Now,
                                                FileName = newFileName,
                                                FileType = (int)PlayerFileType.PlayerImage,
                                                SeasonId = model.SeasonId
                                            });
                                        }

                                        importResult.Add(string.Format("{0} - {1}", imageFile.FileName, Messages.ImportPlayersImage_Success));
                                    }
                                }
                            }
                            else
                            {
                                importResult.Add(string.Format("{0} - {1}", imageFile.FileName, Messages.UserNotExists));
                            }
                        }


                    }

                    usersRepo.Save();

                    Session.Remove(ImportPlayersImageErrorResultSessionKey);

                    byte[] fileContent = null;

                    using (var ms = new MemoryStream())
                    {
                        using (var sw = new StreamWriter(ms))
                        {
                            for (var i = 0; i < importResult.Count; i++)
                            {
                                sw.WriteLine(importResult[i]);
                            }

                            sw.Flush();

                            fileContent = new byte[ms.Length];
                            ms.Position = 0;
                            ms.Read(fileContent, 0, fileContent.Length);
                        }
                        //sw.Close();
                        //sw.Dispose();
                    }

                    Session.Add(ImportPlayersImageErrorResultSessionKey, fileContent);

                    model.ImportResult = Models.ImportPlayersImage.Completed;
                }
                else
                {
                    model.ImportResult = Models.ImportPlayersImage.NoFiles;
                }

                return PartialView("_ImportPlayersImage", model);
            }
            catch (Exception ex)
            {
                log.Error("Import players image exception", ex);

                model.ImportResult = Models.ImportPlayersImage.Error;
                return PartialView("_ImportPlayersImage", model);
            }
        }

        [HttpGet]
        public ActionResult DownloadPartiallyImport()
        {
            var fileByteObj = Session[ImportPlayersErrorResultSessionKey];
            if (fileByteObj == null)
            {
                throw new FileNotFoundException();
            }

            var fileBytes = (byte[])fileByteObj;
            var fileName = Session[ImportPlayersErrorResultFileNameSessionKey] as string;

            var fi = new FileInfo(fileName);

            return File(fileBytes, "application/octet-stream", string.Format("{0}-{2}{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension, Messages.ImportPlayers_OutputFilePrefix));
        }

        [HttpGet]
        public ActionResult DownloadPictureImportResult()
        {
            var fileByteObj = Session[ImportPlayersImageErrorResultSessionKey];
            if (fileByteObj == null)
            {
                throw new FileNotFoundException();
            }

            var fileBytes = (byte[])fileByteObj;
            var fileName = Messages.ImportPlayersImage_ResultFileName;

            return File(fileBytes, "application/octet-stream", fileName);
        }

        [NonAction]
        private void ProcessUserFile(User player, PlayerFileType fileType, bool removeFile, int seasonId, string sectionAlias = "")
        {
            var isIndividual = seasonsRepository.GetById(seasonId)?.Union?.Section?.IsIndividual ?? false;
            if (removeFile)
            {
                var playerFile = player.PlayerFiles.FirstOrDefault(x => x.SeasonId == seasonId &&
                                                                        x.FileType == (int)fileType);
                if (playerFile != null)
                {
                    db.Entry(playerFile).State = EntityState.Deleted;
                    var filePath = Server.MapPath(GlobVars.ContentPath + "/players/" + playerFile.FileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                return;
            }

            var maxFileSize = GlobVars.MaxFileSize * 1000;

            HttpPostedFileBase postedFile = null;
            switch (fileType)
            {
                case PlayerFileType.Insurance:
                    postedFile = GetPostedFile("InsuranceFile");
                    break;
                case PlayerFileType.MedicalCertificate:
                    postedFile = GetPostedFile("MedicalCertificateFile");
                    break;
                case PlayerFileType.PlayerImage:
                    postedFile = GetPostedFile("ImageFile");
                    break;
                case PlayerFileType.DriverLicense:
                    postedFile = GetPostedFile("DriverLicenseFile");
                    break;
                case PlayerFileType.ParentStatement:
                    postedFile = GetPostedFile("ParentStatementFile");
                    break;
                case PlayerFileType.ParentApproval:
                    postedFile = GetPostedFile("ParentApprovalFile");
                    break;
                case PlayerFileType.SpecialClassificationFile:
                    postedFile = GetPostedFile("SpecialClassificationFile");
                    break;
                case PlayerFileType.Unknown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null);
            }
            if (postedFile == null || postedFile.ContentLength == 0) return;

            if (postedFile.ContentLength > maxFileSize)
            {
                ModelState.AddModelError(fileType.ToString(), Messages.FileSizeError);
            }
            else
            {
                var newName = SaveFile(postedFile, player.UserId, fileType);
                if (newName == null)
                {
                    ModelState.AddModelError(fileType.ToString(), Messages.FileError);
                }
                else
                {
                    var playerFile =
                        player.PlayerFiles.FirstOrDefault(x =>
                            x.SeasonId == seasonId &&
                            x.FileType == (int)fileType && (fileType != PlayerFileType.MedicalCertificate || !x.IsArchive));
                    if (playerFile != null)
                    {
                        if (!isIndividual || fileType != PlayerFileType.MedicalCertificate)
                        {
                            var filePath = Server.MapPath(GlobVars.ContentPath + "/players/" + playerFile.FileName);
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                            db.Entry(playerFile).State = EntityState.Deleted;
                        }
                        else
                        {
                            playerFile.IsArchive = true;
                        }
                    }

                    player.PlayerFiles.Add(new PlayerFile
                    {
                        DateCreated = DateTime.Now,
                        FileName = newName,
                        FileType = (int)fileType,
                        PlayerId = player.UserId,
                        SeasonId = seasonId
                    });

                    //add bicycle/martial art check and update player for medical file
                    if((sectionAlias.ToLower() == SectionAliases.MartialArts.ToLower() || sectionAlias == SectionAliases.Bicycle) && fileType == PlayerFileType.MedicalCertificate)
                    {
                        var teamPlayer = player.TeamsPlayers.FirstOrDefault(x => x.SeasonId == seasonId);
                        if(teamPlayer != null && teamPlayer.IsApprovedByManager == true)
                        {
                            teamPlayer.IsApprovedByManager = null;
                            teamPlayer.ApprovalDate = null;
                            player.MedExamDate = null;
                        }
                    }
                   
                }
            }
        }

        [NonAction]
        private void CreateErrorImportFile(ImportExportPlayersHelper importHelper, ImportPlayersViewModel model,
            List<ImportPlayerModel> validationErrorRows, List<ImportPlayerModel> duplicatedRows, bool isTennis, bool isClubSection, string sectionAlias = "")
        {
            byte[] errorFileBytes = null;
            using (var errorFile = importHelper.BuildErrorFile(getCulture(), validationErrorRows, duplicatedRows, isTennis, isClubSection, sectionAlias))
            {
                errorFileBytes = new byte[errorFile.Length];
                errorFile.Read(errorFileBytes, 0, errorFileBytes.Length);
            }

            Session.Remove(ImportPlayersErrorResultSessionKey);
            Session.Remove(ImportPlayersErrorResultFileNameSessionKey);
            Session.Add(ImportPlayersErrorResultSessionKey, errorFileBytes);
            Session.Add(ImportPlayersErrorResultFileNameSessionKey, model.ImportFile.FileName);
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
        private string SaveFile(HttpPostedFileBase file, int id, PlayerFileType fileType = PlayerFileType.Unknown)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!GlobVars.ValidImages.Contains(ext))
            {
                return null;
            }

            var newName = $"{fileType}_{id}_{AppFunc.GetUniqName()}{ext}";

            var savePath = Server.MapPath(GlobVars.ContentPath + "/players/");

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

        public ActionResult ExportPlayersImages(int? unionId, int? leagueId, int? clubId, int? teamId,
            int? seasonId, string leaguesIds = null, PageType pageType = PageType.Default)
        {
            var outputStream = new MemoryStream();
            var fileType = "application/octet-stream";
            ActionResult fileResult = null;
            switch (pageType)
            {
                case PageType.Union:
                    var leaguesIdsArray = Array.ConvertAll(leaguesIds.Split(','), s => int.Parse(s));
                    fileResult = ExportFromUnion(unionId, seasonId, leaguesIdsArray, outputStream, fileType, PageType.Union);
                    break;
                case PageType.League:
                    fileResult = ExportFromLeagues(leagueId, seasonId, outputStream, fileType);
                    break;
                case PageType.Club:
                    fileResult = ExportFromClub(clubId, seasonId, outputStream, fileType);
                    break;
                case PageType.UnionClubs:
                    fileResult = ExportFromUnionClubs(unionId, seasonId, outputStream, fileType);
                    break;
                case PageType.Team:
                    fileResult = ExportFromTeams(teamId, clubId, leagueId, seasonId, outputStream, fileType);
                    break;
                case PageType.Default:
                    throw new NotImplementedException();
            }

            return fileResult;
        }

        private void SaveZipFile(List<TeamPlayerItem> players, ZipFile zipFile, MemoryStream outputStream, string name)
        {
            var files = Directory.GetFiles(HostingEnvironment.MapPath("/assets/players"));
            foreach (var player in players)
            {
                foreach (var file in files)
                {
                    var fileExtension = Path.GetExtension(file);
                    var playerFileName = Path.GetFileName(file);
                    if (player.PlayerImage == playerFileName)
                    {
                        var zipEntryName = $"{player.IdentNum}{fileExtension}";
                        var duplicationsCount = 0;

                        while (zipFile.ContainsEntry(zipEntryName))
                        {
                            duplicationsCount++;

                            zipEntryName = $"{player.IdentNum}_{duplicationsCount}{fileExtension}";
                        }

                        zipFile.AddFile(file).FileName = zipEntryName;
                    }
                }
            }

            Response.ClearContent();
            Response.ClearHeaders();

            Response.AppendHeader("content-disposition", $"attachment; filename={name}_imgs.zip");
            zipFile.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
            zipFile.Save(outputStream);

        }

        #region Export images types

        private ActionResult ExportFromTeams(int? teamId, int? clubId, int? leagueId, int? seasonId, MemoryStream outputStream, string fileType)
        {
            var players = new List<TeamPlayerItem>();
            var teamName = Messages.Team;
            if (teamId.HasValue)
            {
                players = playersRepo.GetTeamPlayers(teamId.Value, clubId ?? 0, leagueId ?? 0, seasonId ?? 0).ToList();
                teamName = teamRepo.GetById(teamId, seasonId)?.Title ?? Messages.Team;
            }
            using (var zipFile = new ZipFile())
            {
                SaveZipFile(players.Where(p => p.PlayerImage != null).ToList(), zipFile, outputStream, teamName);
            }
            outputStream.Position = 0;
            return new FileStreamResult(outputStream, fileType);
        }

        private ActionResult ExportFromClub(int? clubId, int? seasonId, MemoryStream outputStream, string fileType)
        {
            var players = new List<TeamPlayerItem>();
            var clubName = Messages.Club;
            if (clubId.HasValue)
            {
                var clubTeams = teamRepo.GetClubTeamsByClubAndSeasonId(clubId.Value, seasonId ?? 0);
                clubName = clubsRepo.GetClubById(clubId.Value)?.Name ?? Messages.Club;

                foreach (var clubTeam in clubTeams)
                {
                    var playersIds = clubTeam.TeamsPlayers.Select(c => c.Id).ToArray();
                    players.AddRange(playersRepo.GetTeamPlayersDTOByIds(playersIds, seasonId));
                }
            }
            using (var zipFile = new ZipFile())
            {
                SaveZipFile(players.Where(p => p.PlayerImage != null).ToList(), zipFile, outputStream, clubName);
            }

            outputStream.Position = 0;
            return new FileStreamResult(outputStream, fileType);
        }

        private ActionResult ExportFromUnionClubs(int? unionId, int? seasonId, MemoryStream outputStream, string fileType)
        {
            var players = new List<TeamPlayerItem>();
            var unionName = unionsRepo.GetById(unionId.Value)?.Name ?? Messages.Union;
            var clubs = clubsRepo.GetByUnion(unionId.Value, seasonId.Value);
            foreach(var club in clubs)
            { 
                var clubTeams = teamRepo.GetClubTeamsByClubAndSeasonId(club.ClubId, seasonId ?? 0);                

                foreach (var clubTeam in clubTeams)
                {
                    var playersIds = clubTeam.TeamsPlayers.Select(c => c.Id).ToArray();
                    players.AddRange(playersRepo.GetTeamPlayersDTOByIds(playersIds, seasonId));
                }
            }
            using (var zipFile = new ZipFile())
            {
                SaveZipFile(players.Where(p => p.PlayerImage != null).ToList(), zipFile, outputStream, unionName);
            }

            outputStream.Position = 0;
            return new FileStreamResult(outputStream, fileType);
        }

        private ActionResult ExportFromLeagues(int? leagueId, int? seasonId, MemoryStream outputStream, string fileType)
        {
            var players = new List<TeamPlayerItem>();
            var leagueName = Messages.League;
            var league = leagueRepo.GetById(leagueId.Value);
            if (leagueId.HasValue)
            {
                var leagueTeams = teamRepo.GetAllLeagueTeams(leagueId.Value, seasonId ?? 0);
                leagueName = leagueRepo.GetById(leagueId.Value)?.Name ?? Messages.League;

                foreach (var leagueTeam in leagueTeams.Where(lt => !lt.Teams.IsArchive))
                {
                    var playersIds = leagueTeam.Teams.TeamsPlayers.Where(tp => tp.SeasonId == seasonId && !tp.User.IsArchive && tp.League?.UnionId == league.UnionId).Select(c => c.Id).ToArray();
                    players.AddRange(playersRepo.GetTeamPlayersDTOByIds(playersIds, seasonId));
                }
            }

            using (var zipFile = new ZipFile())
            {
                SaveZipFile(players.DistinctBy(c => c.UserId).Where(p => p.PlayerImage != null).ToList(), zipFile, outputStream, leagueName);
            }

            outputStream.Position = 0;

            return new FileStreamResult(outputStream, fileType);
        }

        private ActionResult ExportFromUnion(int? unionId, int? seasonId, IEnumerable<int> leaguesIds, MemoryStream outputStream,
            string fileType, PageType pageType)
        {
            var players = new List<TeamPlayerItem>();
            var unionName = Messages.Union;
            if (unionId.HasValue)
            {
                unionName = unionsRepo.GetById(unionId.Value)?.Name ?? Messages.Union;
                foreach (var id in leaguesIds)
                {
                    var league = leagueRepo.GetById(id);
                    var leagueTeams = teamRepo.GetAllLeagueTeams(id, seasonId);
                    foreach (var leagueTeam in leagueTeams.Where(lt => !lt.Teams.IsArchive))
                    {
                        var playersIds = leagueTeam.Teams.TeamsPlayers.Where(tp => tp.SeasonId == seasonId && !tp.User.IsArchive && tp.League?.UnionId == league.UnionId).Select(c => c.Id).ToArray();
                        players.AddRange(playersRepo.GetTeamPlayersDTOByIds(playersIds, seasonId));
                    }
                }
            }
            using (var zipFile = new ZipFile())
            {
                SaveZipFile(players.DistinctBy(c => c.UserId).Where(p => p.PlayerImage != null).ToList(), zipFile, outputStream, unionName);
            }
            outputStream.Position = 0;

            return new FileStreamResult(outputStream, fileType);
        }

        #endregion

        private string BuildErrorMessage(PageType pageType, string name, IEnumerable<int> leaguesIds = null)
        {
            var message = "";
            if (pageType == PageType.Union)
            {
                var messageBuilder = new StringBuilder("");

                var leagueStrings = new List<string>();
                foreach (var leagueId in leaguesIds)
                {
                    leagueStrings.Add(leagueRepo.GetById(leagueId)?.Name ?? "");
                }
                if (leagueStrings.Count > 0)
                {
                    messageBuilder.Append("(");
                    messageBuilder.Append(string.Join(",", leagueStrings));
                    messageBuilder.Append(")");
                }

                message = string.Format(Messages.ExportImgs_ErrorMessage, Messages.Union.ToLower(), name, messageBuilder.ToString());
            }
            else
            {
                switch (pageType)
                {
                    case PageType.League:
                        message = Messages.ExportImgs_ErrorMessage.Replace("{0}", Messages.League.ToLower()).Replace("{1}", name).Replace("{2}", string.Empty);
                        break;
                    case PageType.Club:
                        message = Messages.ExportImgs_ErrorMessage.Replace("{0}", Messages.Club.ToLower()).Replace("{1}", name).Replace("{2}", string.Empty);
                        break;
                    case PageType.UnionClubs:
                        message = Messages.ExportImgs_ErrorMessage.Replace("{0}", Messages.Union.ToLower()).Replace("{1}", name).Replace("{2}", string.Empty);
                        break;
                    case PageType.Team:
                        message = Messages.ExportImgs_ErrorMessage.Replace("{0}", Messages.Team.ToLower()).Replace("{1}", name).Replace("{2}", string.Empty);
                        break;
                }
            }
            return message;
        }

        public ActionResult ImgsServerCheck(int? unionId, int? leagueId, int? clubId, int? teamId,
            int? seasonId, int[] leaguesIds = null, PageType pageType = PageType.Default)
        {
            ExportImageModel viewModel = null;
            switch (pageType)
            {
                case PageType.Union:
                    {
                        var unionName = unionsRepo.GetById(unionId.Value).Name;
                        var leaguesTeams = leagueRepo.GetLeaguesTeamsByIds(leaguesIds, seasonId.Value);
                        var leaguesPlayers = new List<TeamPlayerItem>();
                        var unionPlayers = new List<TeamPlayerItem>();
                        if (unionId.HasValue)
                        {
                            foreach (var id in leaguesIds)
                            {
                                var league = leagueRepo.GetById(id);
                                var leagueTeams = teamRepo.GetAllLeagueTeams(id, seasonId);

                                foreach (var leagueTeam in leagueTeams.Where(lt => !lt.Teams.IsArchive))
                                {
                                    var playersIds = leagueTeam.Teams.TeamsPlayers.Where(tp => tp.SeasonId == seasonId && !tp.User.IsArchive && tp.League?.UnionId == league.UnionId).Select(c => c.Id).ToArray();
                                    leaguesPlayers.AddRange(playersRepo.GetTeamPlayersDTOByIds(playersIds, seasonId));
                                }
                            }
                        }

                        CheckForPlayerImages(leaguesPlayers, out unionPlayers);

                        viewModel = new ExportImageModel
                        {
                            UnionId = unionId,
                            SeasonId = seasonId,
                            LeaguesIds = leaguesIds,
                            Count = unionPlayers.Count,
                            HasPictures = unionPlayers.Count > 0 ? true : false,
                            ErrorMessage = BuildErrorMessage(PageType.Union, unionName, leaguesIds),
                            Page = PageType.Union
                        };

                        break;
                    }

                case PageType.League:
                    {
                        var league = leagueRepo.GetById(leagueId.Value);
                        var leagueName = league.Name;
                        var players = new List<TeamPlayerItem>();
                        var currentLeaguePlayers = new List<TeamPlayerItem>();

                        if (leagueId.HasValue)
                        {
                            var leagueTeams = teamRepo.GetAllLeagueTeams(leagueId.Value, seasonId ?? 0);
                            leagueName = leagueRepo.GetById(leagueId.Value)?.Name ?? Messages.League;

                            foreach (var leagueTeam in leagueTeams.Where(lt => !lt.Teams.IsArchive))
                            {
                                var playersIds = leagueTeam.Teams.TeamsPlayers.Where(tp => tp.SeasonId == seasonId && !tp.User.IsArchive && tp.League?.UnionId == league.UnionId).Select(c => c.Id).ToArray();
                                players.AddRange(playersRepo.GetTeamPlayersDTOByIds(playersIds, seasonId));
                            }
                        }
                        players = players.GroupBy(p => p.UserId).Select(ug => ug.First()).ToList();
                        CheckForPlayerImages(players, out currentLeaguePlayers);
                        viewModel = new ExportImageModel
                        {
                            LeagueId = leagueId,
                            SeasonId = seasonId,
                            Count = currentLeaguePlayers.Count,
                            HasPictures = currentLeaguePlayers.Count > 0 ? true : false,
                            ErrorMessage = BuildErrorMessage(PageType.League, leagueName),
                            Page = PageType.League
                        };
                        break;
                    }

                case PageType.Club:
                    {
                        var clubName = clubsRepo.GetById(clubId.Value).Name;
                        var players = new List<TeamPlayerItem>();
                        var clubPlayers = new List<TeamPlayerItem>();

                        if (clubId.HasValue)
                        {
                            var clubTeams = teamRepo.GetClubTeamsByClubAndSeasonId(clubId.Value, seasonId ?? 0);

                            foreach (var clubTeam in clubTeams)
                            {
                                var playersIds = clubTeam.TeamsPlayers.Select(c => c.Id).ToArray();
                                players.AddRange(playersRepo.GetTeamPlayersDTOByIds(playersIds, seasonId));
                            }
                        }

                        CheckForPlayerImages(players, out clubPlayers);
                        viewModel = new ExportImageModel
                        {
                            ClubId = clubId,
                            SeasonId = seasonId,
                            Count = clubPlayers.Count,
                            HasPictures = clubPlayers.Count > 0 ? true : false,
                            ErrorMessage = BuildErrorMessage(PageType.Club, clubName),
                            Page = PageType.Club
                        };
                        break;
                    }
                case PageType.UnionClubs:
                    {
                        var clubs = clubsRepo.GetByUnion(unionId.Value, seasonId);
                        var players = new List<TeamPlayerItem>();
                        var clubPlayers = new List<TeamPlayerItem>();
                        var unionName = unionsRepo.GetById(unionId.Value).Name;

                        foreach(var club in clubs)
                        {
                            var clubTeams = teamRepo.GetClubTeamsByClubAndSeasonId(club.ClubId, seasonId ?? 0);

                            foreach (var clubTeam in clubTeams)
                            {
                                var playersIds = clubTeam.TeamsPlayers.Select(c => c.Id).ToArray();
                                players.AddRange(playersRepo.GetTeamPlayersDTOByIds(playersIds, seasonId));
                            }
                        }

                        CheckForPlayerImages(players, out clubPlayers);
                        viewModel = new ExportImageModel
                        {
                            UnionId = unionId,
                            SeasonId = seasonId,
                            Count = clubPlayers.Count,
                            HasPictures = clubPlayers.Count > 0 ? true : false,
                            ErrorMessage = BuildErrorMessage(PageType.UnionClubs, unionName),
                            Page = PageType.UnionClubs
                        };
                        break;
                    }
                case PageType.Team:
                    {
                        var team = teamRepo.GetById(teamId.Value);
                        var currTeamPlayers = new List<TeamPlayerItem>();
                        var teamsPlayers = new List<TeamPlayerItem>();
                        if (teamId.HasValue)
                        {
                            currTeamPlayers = playersRepo.GetTeamPlayers(teamId.Value, clubId ?? 0, leagueId ?? 0, seasonId ?? 0).ToList();
                        }
                        CheckForPlayerImages(currTeamPlayers, out teamsPlayers);
                        viewModel = new ExportImageModel
                        {
                            TeamId = teamId,
                            ClubId = clubId,
                            LeagueId = leagueId,
                            SeasonId = seasonId,
                            Count = teamsPlayers.Count,
                            HasPictures = teamsPlayers.Count > 0 ? true : false,
                            ErrorMessage = BuildErrorMessage(PageType.Team, team.Title),
                            Page = PageType.Team
                        };
                        break;
                    }
                case PageType.Default:
                    throw new NotImplementedException();
            }

            return PartialView("_ExportPlayersImages", viewModel);
        }

        private void CheckForPlayerImages(List<TeamPlayerItem> players, out List<TeamPlayerItem> checkedPlayers)
        {
            var allPlayers = new List<TeamPlayerItem>();
            var files = Directory.GetFiles(HostingEnvironment.MapPath("/assets/players"));
            foreach (var player in players)
            {
                foreach (var file in files)
                {
                    var fileExtension = Path.GetExtension(file);
                    var playerFileName = Path.GetFileName(file);
                    if (player.PlayerImage == playerFileName)
                    {
                        allPlayers.Add(player);
                    }
                }
            }
            checkedPlayers = allPlayers;
        }

        public ActionResult GetPlayersRanksTable(int? userId, IEnumerable<int> disciplineIds, string dispsString, string ranksString, string routesString)
        {
            var usersDisciplineRoutes = playersRepo.GetRoutesByDisciplinesIds(disciplineIds);
            ViewBag.RouteType = "_player_";
            ViewBag.UserId = userId;
            ViewBag.IsFromAdd = userId == null || userId == 0;
            if (!string.IsNullOrEmpty(ranksString) && !string.IsNullOrEmpty(routesString))
            {
                var SelectedValues = new List<SelectedRoutesDto>();
                var disps = dispsString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
                var routes = routesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
                var ranks = ranksString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
                for (var i = 0; i < routes.Count(); i++)
                {
                    var routeId = routes.ElementAtOrDefault(i);
                    if (routeId != 0)
                    {
                        var rankId = ranks.ElementAtOrDefault(i);
                        if (rankId != 0 )
                        {
                            var r = new SelectedRoutesDto
                            {
                                DisciplineId = disps.ElementAt(i),
                                UsersRouteId = routeId,
                                UsersRankId = rankId
                            };
                            SelectedValues.Add(r);
                        }
                    }
                }
                ViewBag.SelectedValues = SelectedValues;
            }
            else
            {
                ViewBag.SelectedValues = userId.HasValue && userId != 0 ? playersRepo.GetSelectedRoutes(userId.Value) : Enumerable.Empty<SelectedRoutesDto>();
            }
            return PartialView("_DisicplinesRanks", usersDisciplineRoutes);
        }

        public ActionResult GetTeamsRanksTable(int? teamId, int? userId, IEnumerable<int> disciplineIds, string dispsString, string ranksString, string routesString)
        {
            var teamsDisciplineRoutes = playersRepo.GetTeamRoutesByDisciplinesIds(disciplineIds);
            ViewBag.RouteType = "_team_";
            ViewBag.UserId = userId;
            ViewBag.TeamId = teamId;
            ViewBag.IsFromAdd = teamId == null || teamId == 0;
            if (!string.IsNullOrEmpty(ranksString) && !string.IsNullOrEmpty(routesString))
            {
                var SelectedValues = new List<SelectedRoutesDto>();
                var disps = dispsString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
                var routes = routesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
                var ranks = ranksString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
                for (var i = 0; i < routes.Count(); i++)
                {
                    var routeId = routes.ElementAtOrDefault(i);
                    if (routeId != 0)
                    {
                        var rankId = ranks.ElementAtOrDefault(i);
                        if (rankId != 0)
                        {
                            var r = new SelectedRoutesDto
                            {
                                DisciplineId = disps.ElementAt(i),
                                UsersRouteId = routeId,
                                UsersRankId = rankId
                            };
                            SelectedValues.Add(r);
                        }
                    }
                }
                ViewBag.SelectedValues = SelectedValues;
            }
            else
            {
                ViewBag.SelectedValues = teamId.HasValue && teamId != 0 ? teamRepo.GetSelectedRoutes(teamId.Value, userId.Value) : Enumerable.Empty<SelectedRoutesDto>();
            }

            var result = new Dictionary<Discipline, IEnumerable<DisciplineTeamRoute>>();

            foreach (var d in teamsDisciplineRoutes)
            {
                if (d.Value.Count() > 0)
                {
                    result.Add(d.Key, d.Value);
                }
            }

            return PartialView("_DisicplinesTeamRanks", result);
        }

        [HttpPost]
        public JsonResult EditRankForUser(int userId, int? routeId, int? rankId)
        {
            string errorMessage;
            playersRepo.UpdateUsersRoute(userId, routeId, rankId, out errorMessage);
            return Json(new { Message = errorMessage });
        }

        [HttpPost]
        public JsonResult EditRankForTeam(int teamId, int userId, int? routeId, int? rankId)
        {
            string errorMessage;
            teamRepo.UpdateTeamsRoute(teamId, userId, routeId, rankId, out errorMessage);
            return Json(new { Message = errorMessage });
        }

        public ActionResult RegisteredPlayers(int leagueId, int seasonId)
        {
            var playersRegistered = playersRepo.GetAllRegisteredPlayers(leagueId, seasonId);
            ViewBag.LeagueId = leagueId;
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueName = leagueRepo.GetById(leagueId)?.Name;

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);

            return PartialView("_RegisteredPlayers", playersRegistered);
        }

        public ActionResult DeleteSportsmenRegistration(int id, int leagueId, int seasonId)
        {
            playersRepo.DeleteSportsmenRegistration(id);
            return RedirectToAction(nameof(RegisteredPlayers), new { leagueId, seasonId });
        }

        [HttpPost]
        public void ChangeRegistrationStatus(int id, bool isApproved)
        {
            
            playersRepo.ChangeRegistrationStatus(id, isApproved);
        }

        [HttpPost]
        public void ChangeRegistrationStatusForAllPlayers(int leagueId, int seasonId, bool isApproved)
        {    
            playersRepo.ChangeRegistrationStatusForAllPlayers(leagueId, seasonId, isApproved);
        }


        [HttpPost]
        public void ChangeToChargeForAllPlayers(int leagueId, int seasonId, int? disciplineId, bool isCharged)
        {
            playersRepo.ChangeToChargeForAllPlayers(leagueId, seasonId, disciplineId, isCharged);
        }

        [HttpPost]
        public void ChangeToApproveForAllPlayers(int leagueId, int seasonId, int? disciplineId, bool isApproved)
        {
            playersRepo.ChangeToApproveForAllPlayers(leagueId, seasonId, disciplineId, isApproved);
        }

        [HttpPost]
        public void ChargeRegistrationStatus(int id, int leagueId, bool isCharged)
        {
            playersRepo.ChargeRegistrationStatus(id, leagueId, AdminId, isCharged);
        }

        [HttpPost]
        public JsonResult ApproveRegistrationStatus(int id, int leagueId, bool isApproved)
        {
            
            playersRepo.ApproveRegistrationStatus(id, leagueId, AdminId, isApproved);
            return Json(new { Success = true});
        }

        



        [HttpPost]
        public void ExportSportsmenRegistrationsToExcel(int leagueId, int seasonId, bool onlyHeaders)
        {
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var leagueName = leagueRepo.GetById(leagueId)?.Name;

                var ws = workbook.AddWorksheet($"{Messages.Sportsmans} {Messages.RegistrationList.ToLowerInvariant()}");

                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                addCell("*" + Messages.ClubNumber);
                addCell(Messages.ClubName);
                addCell("*" + Messages.FullName);
                addCell("*" + Messages.IdentNum);
                addCell("*" + Messages.BirthDay);
                addCell(Messages.FinalScore);
                addCell(Messages.PlayerInfoRank);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();
                if (!onlyHeaders)
                {
                    var playersRegistered = playersRepo.GetAllRegisteredPlayers(leagueId, seasonId);

                    foreach (var row in playersRegistered)
                    {
                        addCell(row.ClubNumber?.ToString());
                        addCell(row.ClubName);
                        addCell(row.FullName);
                        addCell(row.IdentNum);
                        addCell(row.Birthday?.ToShortDateString());
                        addCell(row.FinalScore?.ToString());
                        addCell(row.Rank?.ToString());

                        rowCounter++;
                        columnCounter = 1;
                    }
                    ws.Columns().AdjustToContents();
                }

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                if (!onlyHeaders)
                {
                    Response.AddHeader("content-disposition", $"attachment;filename={leagueName.Replace(' ', '_')}" +
                    $"_{Messages.RegistrationCaption.Replace(Messages.Gymnastics.ToLower(), Messages.Sportsmans.ToLower())}.xlsx");
                }
                else
                {
                    Response.AddHeader("content-disposition", $"attachment;filename={Messages.ImportSportsmen.Replace(' ', '_').ToLower()}.xlsx");
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
        public ActionResult ImportSportsmenRegistrations(int leagueId, int seasonId)
        {
            var model = new ImportSportsmanViewModel
            {
                LeagueId = leagueId,
                SeasonId = seasonId
            };

            return PartialView("_ImportSportsmen", model);
        }

        [HttpPost]
        public ActionResult ImportSportsmenRegistrations(ImportSportsmanViewModel model)
        {
            try
            {
                if (model.ImportFile != null)
                {
                    var importHelper = new ImportExportPlayersHelper(db);
                    List<ImportSportsmanRegistrationModel> correctRows = null;
                    List<ImportSportsmanRegistrationModel> validationErrorRows = null;

                    importHelper.ExtractSportsmanData(model.ImportFile.InputStream, out correctRows, out validationErrorRows);

                    if (correctRows.Count > 0)
                    {
                        List<ImportSportsmanRegistrationModel> importErrorRows = null;
                        List<ImportSportsmanRegistrationModel> duplicatedRows = null;

                        model.SuccessCount = importHelper.ImportSportsmen(model.SeasonId, model.LeagueId,
                            correctRows, out importErrorRows, out duplicatedRows);

                        validationErrorRows.AddRange(importErrorRows);

                        if (validationErrorRows.Count > 0 || duplicatedRows.Count > 0 && model.SuccessCount > 0)
                        {
                            CreateErrorSportsmenImportFile(importHelper, model, validationErrorRows, duplicatedRows);
                            model.Result = ImportPlayersResult.PartialyImported;
                            model.ResultMessage = Messages.ImportPlayers_NoRowLoaded.Replace(Messages.Players, Messages.Sportsmans);
                        }
                        else if (validationErrorRows.Count == 0 || duplicatedRows.Count == 0 && model.SuccessCount > 0)
                        {
                            model.Result = ImportPlayersResult.Success;
                        }
                        else
                        {
                            model.Result = ImportPlayersResult.Error;
                        }

                        model.ErrorCount = validationErrorRows.Count;
                        model.DuplicateCount = duplicatedRows.Count;
                    }
                    else
                    {
                        model.Result = ImportPlayersResult.Error;
                        model.ResultMessage = Messages.ImportPlayers_NoRowLoaded.Replace(Messages.Players, Messages.Gymnastics);
                        model.SuccessCount = 0;
                        model.DuplicateCount = 0;
                        model.ErrorCount = validationErrorRows.Count;

                        CreateErrorSportsmenImportFile(importHelper, model, validationErrorRows, null);
                    }
                }
                else
                {
                    model.Result = ImportPlayersResult.Error;
                    model.ResultMessage = Messages.ImportPlayers_ChooseFile;
                }

                return PartialView("_ImportSportsmen", model);
            }
            catch (Exception ex)
            {
                model.Result = ImportPlayersResult.Error;
                model.ResultMessage = Messages.ImportPlayers_ImportException;

                return PartialView("_ImportSportsmen", model);
            }
        }

        [NonAction]
        private void CreateErrorSportsmenImportFile(ImportExportPlayersHelper importHelper, ImportSportsmanViewModel model,
            List<ImportSportsmanRegistrationModel> validationErrorRows, List<ImportSportsmanRegistrationModel> duplicatedRows)
        {

            var culture = getCulture();
            byte[] errorFileBytes = null;
            using (var errorFile = importHelper.BuildErrorFileForSportsmen(validationErrorRows, duplicatedRows, culture))
            {
                errorFileBytes = new byte[errorFile.Length];
                errorFile.Read(errorFileBytes, 0, errorFileBytes.Length);
            }

            Session.Remove(ImportPlayersErrorResultSessionKey);
            Session.Remove(ImportPlayersErrorResultFileNameSessionKey);
            Session.Add(ImportPlayersErrorResultSessionKey, errorFileBytes);
            Session.Add(ImportPlayersErrorResultFileNameSessionKey, model.ImportFile.FileName);
        }

        public ActionResult ImportInitialApprovalDate()
        {
            return View();
        }

        [HttpPost]
        public void ImportInitialApprovalDate(HttpPostedFileBase file)
        {
            if (file != null)
            {
                var users = new List<InitialDateForm>();
                var errorRows = 0;
                var isFirstRow = true;
                using (var workBook = new XLWorkbook(file.InputStream))
                {
                    var workSheet = workBook.Worksheet(1);
                    foreach (var row in workSheet.Rows())
                    {
                        if (!isFirstRow && !row.IsEmpty())
                        {
                            var identNumStr = row.Cell(1).Value?.ToString();

                            var date = DateTime.MinValue;

                            if (row.Cell(2).DataType == XLDataType.DateTime)
                            {
                                date = row.Cell(2).GetDateTime();
                            }
                            else
                            {
                                var birthDateStr = row.Cell(2).Value.ToString();

                                if (!string.IsNullOrEmpty(birthDateStr))
                                {
                                    DateTime.TryParseExact(birthDateStr, "dd.MM.yyyy", CultureInfo.GetCultureInfoByIetfLanguageTag("en-GB"), DateTimeStyles.None, out date);
                                }
                            }

                            if (!string.IsNullOrEmpty(identNumStr) && date != DateTime.MinValue)
                            {
                                users.Add(new InitialDateForm
                                {
                                    IdentNum = identNumStr,
                                    InitialApproveDate = date
                                });
                            }
                            else
                            {
                                errorRows++;
                            }
                        }
                        else
                        {
                            isFirstRow = false;
                        }
                    }
                }

                if (users.Any())
                {
                    foreach (var item in users)
                        usersRepo.UdateGymnasticsValue(item.IdentNum, item.InitialApproveDate);

                    usersRepo.Save();
                }
            }
        }

        [HttpGet]
        public ActionResult ImportAthletesNumbers(int UnionId, int SeasonId)
        {
            ViewBag.UnionId = UnionId;
            ViewBag.SeasonId = SeasonId;
            return PartialView("_ImportAthletesNumbers", new ImportViewModelBasic());
        }

        [HttpPost]
        public ActionResult ImportAthletesNumbers(ImportViewModelBasic model, int UnionId, int SeasonId)
        {
            try
            {
                if (model.ImportFile != null)
                {
                    var importHelper = new ImportExportPlayersHelper(db);
                    List<ImportAthletesNumbersModel> correctRows = null;
                    List<ImportAthletesNumbersModel> validationErrorRows = null;

                    importHelper.ExtractAthletesNumbersData(model.ImportFile.InputStream, out correctRows, out validationErrorRows);

                    if (correctRows.Count > 0)
                    {
                        List<ImportAthletesNumbersModel> importErrorRows = null;
                        var playersAlreadyIn = playersRepo.GetPlayersStatusesByUnionId(UnionId, SeasonId, string.Empty).AsEnumerable();

                        model.SuccessCount = importHelper.ImportAthletesNumbers(correctRows, out importErrorRows, playersAlreadyIn, SeasonId);

                        validationErrorRows.AddRange(importErrorRows);

                        if (validationErrorRows.Count > 0 && model.SuccessCount > 0)
                        {
                            CreateErrorImportFile(importHelper, model, validationErrorRows);
                            model.Result = ImportPlayersResult.PartialyImported;
                            model.ErrorCount = validationErrorRows.Count;
                            model.ResultMessage = Messages.ImportPlayers_NoRowLoaded.Replace(Messages.Players, $"{Messages.Athletes} {Messages.Numbers.ToLower()}");
                        }
                        else if (validationErrorRows.Count == 0 && model.SuccessCount > 0)
                        {
                            model.Result = ImportPlayersResult.Success;
                        }
                        else
                        {
                            model.Result = ImportPlayersResult.Error;
                        }

                        model.ErrorCount = validationErrorRows.Count;
                    }
                    else
                    {
                        model.Result = ImportPlayersResult.Error;
                        model.ResultMessage = Messages.ImportPlayers_NoRowLoaded.Replace(Messages.Players, Messages.Athletes);
                        model.SuccessCount = 0;
                        model.DuplicateCount = 0;
                        model.ErrorCount = correctRows.Count - model.SuccessCount;

                        CreateErrorImportFile(importHelper, model, null);
                    }
                }
                else
                {
                    model.Result = ImportPlayersResult.Error;
                    model.ResultMessage = Messages.ImportPlayers_ChooseFile;
                }

                return PartialView("_ImportAthletesNumbers", model);
            }
            catch (Exception ex)
            {
                model.Result = ImportPlayersResult.Error;
                model.ResultMessage = Messages.ImportPlayers_ImportException;
                model.ExceptionMessage = ex.Message;

                return PartialView("_ImportAthletesNumbers", model);
            }
        }

        private void CreateErrorImportFile(ImportExportPlayersHelper importHelper, ImportViewModelBasic model,
            List<ImportAthletesNumbersModel> validationErrorRows)
        {
            var culture = getCulture();
            byte[] errorFileBytes = null;
            using (var errorFile = importHelper.BuildErrorFileForAthletesNumber(validationErrorRows, culture))
            {
                errorFileBytes = new byte[errorFile.Length];
                errorFile.Read(errorFileBytes, 0, errorFileBytes.Length);
            }
            Session.Remove(ImportAthletesErrorResultSessionKey);
            Session.Remove(ImportAthletesErrorResultFileNameSessionKey);
            Session.Add(ImportAthletesErrorResultSessionKey, errorFileBytes);
            Session.Add(ImportAthletesErrorResultFileNameSessionKey, model.ImportFile.FileName);
        }

        [HttpGet]
        public ActionResult DownloadPartiallyImportAthletes()
        {
            var fileByteObj = Session[ImportAthletesErrorResultSessionKey];
            if (fileByteObj == null)
            {
                throw new FileNotFoundException();
            }

            var fileBytes = (byte[])fileByteObj;
            var fileName = Session[ImportAthletesErrorResultFileNameSessionKey] as string;

            var fi = new FileInfo(fileName);

            return File(fileBytes, "application/octet-stream", string.Format("{0}-{2}{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension, Messages.ImportPlayers_OutputFilePrefix));
        }

        public void ExportPlayerUciCard(int seasonId, int userId)
        {
            var season = seasonsRepository.GetById(seasonId);
            var unionname = season.Union?.Name;
            var currCult = getCulture();
            
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Locales.En_US);
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var ws = workbook.AddWorksheet($"{Messages.Sportsman} {Messages.UciCard.ToLowerInvariant()}");

                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                addCell(Messages.FirstName);
                addCell(Messages.LastName);
                addCell(Messages.IdentNum);
                addCell(Messages.UciCategory);
                addCell(Messages.Team);
                addCell(Messages.Club);
                addCell(Messages.Nationality);
                addCell(Messages.UciId);
                addCell(Messages.BirthDay);
                addCell(Messages.Gender);
                addCell(Messages.UserRole);
                addCell(Messages.Function);
                addCell(Messages.Photo);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                var teamPlayer = playersRepo.GetTeamPlayerByUserIdAndSeasonId(userId, seasonId);

                var user = teamPlayer.User;

                var playerAge = Convert.ToInt32(season.Name) - user.BirthDay.Value.Year;
                var competitionage = db.CompetitionAges.FirstOrDefault(x => x.UnionId == season.UnionId && x.from_age <= playerAge && x.to_age >= playerAge && (x.gender == user.GenderId || x.gender == 3) && x.FriendshipTypeId == teamPlayer.FriendshipTypeId && x.IsUCICategory == true);

                addCell(user.ForeignFirstName);
                addCell(user.ForeignLastName);
                addCell(user.IdentNum);
                addCell(competitionage?.age_name);
                var teamName = db.TeamsDetails.FirstOrDefault(x => x.TeamId == teamPlayer.TeamId && x.SeasonId == seasonId)?.TeamForeignName;
                if (string.IsNullOrEmpty(teamName)) teamName = teamRepo.GetById(teamPlayer.TeamId)?.Title;
                addCell(teamName);
                addCell(teamPlayer.Club?.ForeignName);
                addCell(user.Nationality);
                addCell(user.UciId?.ToString());
                addCell(user.BirthDay?.ToString());
                if (user.GenderId == 0)
                    addCell(Messages.Female);
                else
                    addCell(Messages.Male);
                if (user.UsersType.TypeRole == AppRole.Players)
                {
                    addCell(UIHelpers.GetPlayerCaption(SectionAliases.Bicycle, false));
                }
                else
                {
                    addCell(Messages.TeamStaff);
                }
                addCell(""); //Function empty for now
                addCell(user.PlayerFiles.FirstOrDefault(x => x.FileType == 3)?.FileName);

                rowCounter++;
                columnCounter = 1;
                
                ws.Columns().AdjustToContents();
                

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                Response.AddHeader("content-disposition", $"attachment;filename=" + unionname + "_" + Messages.UciCard + DateTime.Now.ToString() + ".xlsx");

                
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
        public ActionResult GetFriendshipPayments(int userId)
        {
            var model = new BicycleFriendshipPaymentsModel();

            var friendshipPayments = db.BicycleFriendshipPayments
                .Include(x => x.Club)
                .Include(x => x.Team)
                .Include(x => x.Team.TeamsDetails)
                .Include(x => x.Season)
                .Where(x => x.UserId == userId)
                .ToList();

            model.Payments = friendshipPayments
                .Select(x => new BicycleFriendshipPaymentItem
                {
                    ClubName = x.Club.Name,
                    TeamName = x.Team.TeamsDetails.FirstOrDefault(td => td.SeasonId == x.SeasonId)?.TeamName
                               ?? x.Team.Title,
                    SeasonName = x.Season?.Name,
                    Prices = new BicycleFriendshipPriceHelper.BicycleFriendshipPrice(x.FriendshipPrice, x.ChipPrice,
                        x.UciPrice),
                    LogLigPaymentId = x.LogLigPaymentId,
                    OfficeGuyCustomerId = x.OfficeGuyCustomerId,
                    OfficeGuyPaymentId = x.OfficeGuyPaymentId,
                    OfficeGuyDocumentNumber = x.OfficeGuyDocumentNumber,

                    CreatedByName = x.CreatedByUser?.FullName,
                    DateCreated = x.DateCreated,

                    DatePaid = x.DatePaid,

                    Discarded = x.Discarded,
                    DiscardedByName = x.DiscardUser?.FullName,
                    DiscardDate = x.DiscardDate,

                    Comment = x.Comment,
                    IsManual = x.IsManual
                })
                .ToList();

            return PartialView("_FriendshipPayments", model);
        }
    }

    public class InitialDateForm
    {
        public string IdentNum { get; set; }
        public DateTime InitialApproveDate { get; set; }
    }
}


