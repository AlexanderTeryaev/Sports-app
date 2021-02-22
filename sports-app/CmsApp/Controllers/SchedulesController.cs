using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using AppModel;
using CmsApp.Models;
using DataService;
using ClosedXML.Excel;
using System.Web;
using System.Globalization;
using System.Threading;
using System.Net;
using DataService.DTO;
using DataService.Utils;
using CmsApp.Helpers;
using Resources;
using System.Text;
using System.Data.Entity;

namespace CmsApp.Controllers
{
    public class SchedulesController : AdminController
    {
        // GET: Schedules
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List(int id, bool desOrder, bool isChronological = false,
            int dateFilterType = Schedules.DateFilterPeriod.All,
            DateTime? dateFrom = null, DateTime? dateTo = null, int? seasonId = null, int? departmentId = null)
        {
            ViewBag.IsTennisLeague = leagueRepo.CheckIfIsTennisLeague(id);
            ViewBag.IsTennisLeagueReferee = usersRepo.GetTopLevelJob(AdminId)?.Equals(JobRole.Referee) == true
                && jobsRepo.GetAllTennisLeagues(AdminId).Count > 0;
            ViewBag.UserId = AdminId;
            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.StateDisplay = leagueRepo.GetLeagueScheduleState(id, AdminId);
            var res = new Schedules
            {
                IsDepartmentLeague = departmentId.HasValue ? true : false,
                DepartmentId = departmentId,
                dateFilterType = dateFilterType,
                dateFrom = dateFrom ?? Schedules.FirstDayOfMonth,
                dateTo = dateTo ?? Schedules.Tomorrow,
                RoundStartCycle = gamesRepo.GetGameSettings(id)?.RoundStartCycle
            };
            var league = leagueRepo.GetById(id);
            var unionId = league.UnionId;
            var clubId = league.ClubId;

            var cond = new GamesRepo.GameFilterConditions
            {
                seasonId = seasonId ?? leagueRepo.GetById(id).SeasonId ?? seasonsRepository.GetLastSeason().Id,
                auditoriums = new List<AuditoriumShort>(),
                leagues = new List<LeagueShort>
                {
                    new LeagueShort
                    {
                        Id = id,
                        UnionId = league.UnionId,
                        Check = true,
                        Name = league.Name
                    }
                }
            };
            if (dateFilterType == Schedules.DateFilterPeriod.BeginningOfMonth && isChronological)
            {
                cond.dateFrom = Schedules.FirstDayOfMonth;
                cond.dateTo = null;
            }
            else if (dateFilterType == Schedules.DateFilterPeriod.Ranged && isChronological)
            {
                cond.dateFrom = dateFrom;
                cond.dateTo = dateTo;
            }
            else if (dateFilterType == Schedules.DateFilterPeriod.FromToday)
            {
                cond.dateFrom = Schedules.Today;
                cond.dateTo = null;
            }
            else
            {
                cond.dateFrom = null;
                cond.dateTo = null;
            }
            res.Games = gamesRepo.GetCyclesByFilterConditions(cond,
                User.IsInAnyRole(AppRole.Admins, AppRole.Editors, AppRole.Workers), false).ToList()
                .Select(gc => new GameInLeague(gc)
                {
                    GameTypeId = gc.Group.TypeId,
                    LeagueId = gc.Stage.LeagueId,
                    LeagueName = gc.Stage.League.Name,
                    IsPublished = gc.IsPublished,
                    IsDateUpdated = gc.IsDateUpdated,
                    PdfGameReport = gc.PdfGameReport,
                    IsNotSetYet = gc.IsNotSetYet,
                    Remark = gc.Remark,
                    DeskName = ViewBag.IsTennisLeague ? null : GetMainDeskName(gc.CycleId),
                    DeskNames = ViewBag.IsTennisLeague ? null : GetDeskNamesString(gc.CycleId),
                    MainRefereeName = GetMainRefereeName(gc.CycleId),
                    RefereeNames = ViewBag.IsTennisLeague ? null : GetRefereesNamesString(gc.CycleId),
                    Note = gc.Note,
                    IsGameHasReligiousTeam = (gc.HomeTeam?.IsReligiousTeam ?? false) || (gc.GuestTeam?.IsReligiousTeam ?? false)
                }).ToList();

            foreach (var game in res.Games)
            {
                if (game.PlayoffBracket?.Parent1 != null && game.PlayoffBracket.Parent2 != null &&
                    game.PlayoffBracket.Type == (int)PlayoffBracketType.Winner)
                {
                    if (game.PlayoffBracket.Parent1.Team2GroupPosition == 0 &&
                        game.PlayoffBracket.Parent1.Team1Id.HasValue ||
                        game.PlayoffBracket.Parent1.Team2GroupPosition == 0 &&
                        game.PlayoffBracket.Parent1.Athlete1Id.HasValue)
                    {
                        if (game.Group.IsIndividual)
                        {
                            game.HomeAthleteId = game.PlayoffBracket.Parent1.Athlete1Id.Value;
                            game.TeamsPlayer1 = db.TeamsPlayers.Find(game.HomeAthleteId);
                            gamesRepo.UpdateGameAndBracketForAthlete(game.CycleId, game.HomeAthleteId);
                        }
                        else
                        {
                            game.HomeTeamId = game.PlayoffBracket.Parent1.Team1Id.Value;
                            game.HomeTeam = teamRepo.GetById(game.HomeTeamId);
                            gamesRepo.UpdateGameAndBracket(game.CycleId, game.HomeTeamId);
                        }
                    }


                    if (game.PlayoffBracket.Parent2.Team2GroupPosition == 0 &&
                        game.PlayoffBracket.Parent2.Team1Id.HasValue ||
                        game.PlayoffBracket.Parent2.Team2GroupPosition == 0 &&
                        game.PlayoffBracket.Parent2.Athlete2Id.HasValue)
                    {
                        if (game.Group.IsIndividual)
                        {
                            game.HomeAthleteId = game.PlayoffBracket.Parent2.Athlete1Id.Value;
                            game.TeamsPlayer1 = db.TeamsPlayers.Find(game.HomeAthleteId);
                            gamesRepo.UpdateGameAndBracketForAthlete(game.CycleId, game.HomeAthleteId);
                        }
                        else
                        {
                            gamesRepo.UpdateGameAndBracketGuest(game.CycleId, game.PlayoffBracket.Parent2.Team1Id.Value);
                        }
                    }


                    if (game.PlayoffBracket.Parent1.Team1GroupPosition == 0 &&
                        game.PlayoffBracket.Parent1.Team2Id.HasValue ||
                        game.PlayoffBracket.Parent1.Team1GroupPosition == 0 &&
                        game.PlayoffBracket.Parent1.Athlete2Id.HasValue)
                    {
                        if (game.Group.IsIndividual)
                        {
                            game.HomeAthleteId = game.PlayoffBracket.Parent1.Athlete2Id.Value;
                            game.TeamsPlayer1 = db.TeamsPlayers.Find(game.HomeAthleteId);
                            gamesRepo.UpdateGameAndBracket(game.CycleId, game.HomeAthleteId);
                        }
                        else
                        {
                            game.HomeTeamId = game.PlayoffBracket.Parent1.Team2Id.Value;
                            game.HomeTeam = teamRepo.GetById(game.HomeTeamId);
                            gamesRepo.UpdateGameAndBracket(game.CycleId, game.HomeTeamId);
                        }

                    }


                    if (game.PlayoffBracket.Parent2.Team1GroupPosition == 0 &&
                        game.PlayoffBracket.Parent2.Team2Id.HasValue ||
                        game.PlayoffBracket.Parent2.Team1GroupPosition == 0 &&
                        game.PlayoffBracket.Parent2.Athlete2Id.HasValue)
                    {
                        if (game.Group.IsIndividual)
                        {
                            game.HomeAthleteId = game.PlayoffBracket.Parent2.Athlete2Id.Value;
                            game.TeamsPlayer1 = db.TeamsPlayers.Find(game.HomeAthleteId);
                            gamesRepo.UpdateGameAndBracketForAthlete(game.CycleId, game.HomeAthleteId);
                        }
                        else
                        {
                            gamesRepo.UpdateGameAndBracketGuest(game.CycleId, game.PlayoffBracket.Parent2.Team2Id.Value);
                        }

                    }
                }

                //case (int)PlayoffBracketType.Condolence3rdPlaceBracket:
            }

            res.Groups = res.Games.GroupBy(gc => gc.GameTypeId == GameTypeId.Division ? "Division" : gc.Group.Name)
                .Select(g => new ScheduleGroup
                {
                    GroupName = g.Key,
                    GameTypeId = g.First().GameTypeId,
                    IsIndividual = g.First().Group.IsIndividual,
                    Stages = g.GroupBy(st => st.Stage.StageId).Select(st => new ScheduleStage
                    {
                        StageId = st.Key,
                        StageNumber = st.First().Stage.Number,
                        StageName = st.First().Stage.Name,
                        IsCrossesStage = st.First().Stage.IsCrossesStage,
                        Items = st.ToList()
                    }).OrderBy(s => s.StageNumber).ToList(),
                    BracketsCount = g.GroupBy(st => st.Stage.StageId).Select(st => new ScheduleStage
                    {
                        StageId = st.Key,
                        StageNumber = st.First().Stage.Number,
                        StageName = st.First().Stage.Name,
                        IsCrossesStage = st.First().Stage.IsCrossesStage,
                        Items = st.ToList()
                    }).OrderBy(s => s.StageNumber).FirstOrDefault()?.Items?.Count ?? 0,
                    Rounds = g.First().Group.NumberOfCycles
                }).ToList();
            string alias = string.Empty;

            Session["desOrder"] = desOrder;

            if (unionId.HasValue)
            {
                var referees = usersRepo.GetUnionAndLeagueReferees(unionId.Value, id)
                    .ToDictionary(u => u.UserId, u => u);

                var spectators = usersRepo.GetUnionAndLeagueSpectators(unionId.Value, id)
                    .ToDictionary(u => u.UserId, u => u);

                var desks = usersRepo.GetUnionAndLeageDesks(unionId.Value, id)
                    .ToDictionary(u => u.UserId, u => u);

                res.Auditoriums = auditoriumsRepo.GetByUnionAndSeason(unionId.Value, cond.seasonId)
                    .Select(au => new AuditoriumShort
                    {
                        Id = au.AuditoriumId,
                        Name = cond.seasonId == au.SeasonId ? au.Name : $"{au.Name} - {au.Club.Name}"
                    }).ToList();

                res.Referees = referees.Union(spectators).ToDictionary(k => k.Key, v => v.Value);
                res.Spectators = spectators.Union(referees).ToDictionary(k => k.Key, v => v.Value);
                res.Desks = desks.Union(spectators).Union(referees).ToDictionary(k => k.Key, v => v.Value);

                alias = league.Union.Section?.Alias;
            }
            else if (clubId.HasValue)
            {
                var referees = usersRepo.GetClubAndLeagueReferees(clubId.Value, id)
                    .ToDictionary(u => u.UserId, u => u);

                var spectators = usersRepo.GetClubAndLeagueSpectators(clubId.Value, id)
                    .ToDictionary(u => u.UserId, u => u);

                res.Desks = usersRepo.GetClubAndLeagueDesks(clubId.Value, id)
                    .ToDictionary(u => u.UserId, u => u);

                res.Referees = referees.Union(spectators).ToDictionary(k => k.Key, v => v.Value);
                res.Spectators = spectators.Union(referees).ToDictionary(k => k.Key, v => v.Value);

                res.Auditoriums = auditoriumsRepo.GetByClubAndSeason(clubId.Value, cond.seasonId)
                    .Select(au => new AuditoriumShort
                    {
                        Id = au.AuditoriumId,
                        Name = au.Name
                    }).ToList();

                alias = departmentId == null ? league?.Club?.Section?.Alias : clubsRepo.GetById(departmentId.Value)?.SportSection?.Alias;
            }

            res.Leagues = cond.leagues.ToArray();
            res.SeasonId = cond.seasonId;
            res.UnionId = unionId;
            res.teamsByGroups = alias.Equals(GamesAlias.Tennis)
                   ? teamRepo.GetGroupTeamsBySeasonAndLeaguesForTennis(cond.seasonId, new int[] { league.LeagueId })
                   : teamRepo.GetGroupTeamsBySeasonAndLeagues(cond.seasonId, new int[] { league.LeagueId });
            Session["isChronological"] = isChronological;

            SetRanks(ref res);

            if (desOrder)
            {
                res.Games = res.Games.OrderByDescending(x => x.Stage.Number).ToList();
                foreach (var group in res.Groups)
                {
                    group.Stages = group.Stages.OrderByDescending(st => st.StageNumber).ToList();
                }
            }

            ViewBag.IsCatchball = string.Equals(alias, GamesAlias.NetBall, StringComparison.OrdinalIgnoreCase);
            ViewBag.IsUnionViewer = usersRepo.GetTopLevelJob(AdminId) == JobRole.Unionviewer;
            ViewBag.IsPenaltySection = string.Equals(alias, GamesAlias.WaterPolo, StringComparison.OrdinalIgnoreCase)
                || string.Equals(alias, GamesAlias.Soccer, StringComparison.OrdinalIgnoreCase)
                || string.Equals(alias, GamesAlias.Rugby, StringComparison.OrdinalIgnoreCase)
                || string.Equals(alias, GamesAlias.Handball, StringComparison.OrdinalIgnoreCase);

            res.Section = alias;


            switch (alias)
            {
                case GamesAlias.WaterPolo:
                case GamesAlias.Soccer:
                case GamesAlias.Rugby:
                case GamesAlias.BasketBall:
                case GamesAlias.Softball:
                    if (isChronological)
                    {
                        return PartialView("BasketBallWaterPolo/_ChronologicalList", res);
                    }
                    else
                    {
                        return PartialView("BasketBallWaterPolo/_List", res);
                    }

                default:
                    if (isChronological)
                    {
                        return PartialView("_ChronologicalList", res);
                    }
                    else
                    {
                        return PartialView("_List", res);
                    }
            }
        }



        private string GetRefereesNamesString(int cycleId)
        {
            var listOfRefereesNames = gamesRepo.GetRefereesNames(cycleId);
            if (listOfRefereesNames == null)
                return null;
            var builder = new StringBuilder();
            for (int i = 0; i < listOfRefereesNames.Count; i++)
            {
                var punktuation = (i == (listOfRefereesNames.Count - 1)) ? "." : " ,";
                if (i == 0)
                {
                    builder.Append($"{Messages.MainReferee}: {listOfRefereesNames[i]}");
                    builder.Append(punktuation);
                }
                else
                {
                    builder.Append($"{Messages.Referee} #{i + 1}: {listOfRefereesNames[i]}");
                    builder.Append(punktuation);
                }
            }
            return builder.ToString();
        }

        private string GetDeskNamesString(int cycleId)
        {
            var listOfDesksNames = gamesRepo.GetDeskNames(cycleId);
            if (listOfDesksNames == null)
                return null;
            var builder = new StringBuilder();
            for (int i = 0; i < listOfDesksNames.Count; i++)
            {
                var punktuation = i == listOfDesksNames.Count - 1 ? "." : " ,";
                if (i == 0)
                {
                    builder.Append($"{Messages.MainDesk}: {listOfDesksNames[i]}");
                    builder.Append(punktuation);
                }
                else
                {
                    builder.Append($"{Messages.Desk} #{i + 1}: {listOfDesksNames[i]}");
                    builder.Append(punktuation);
                }
            }
            return builder.ToString();
        }
        private string GetMainRefereeName(int cycleId)
        {
            var refereesNames = gamesRepo.GetRefereesNames(cycleId);

            string mainRefereeName = Messages.NoReferees;
            if (refereesNames != null)
            {
                if (refereesNames.Count == 1)
                    mainRefereeName = refereesNames[0];
                else if (refereesNames.Count > 1)
                {
                    StringBuilder sb = new StringBuilder(refereesNames[0]);
                    for (int i = 1; i < refereesNames.Count(); i++)
                    {
                        var refereeName = refereesNames[i];
                        sb.AppendFormat(", {0}", refereeName);
                    }
                    mainRefereeName = sb.ToString();
                }

            }
            return mainRefereeName;
        }

        private string GetMainDeskName(int cycleId)
        {
            var deskNames = gamesRepo.GetDeskNames(cycleId);

            string mainDeskName = Messages.NoDesks;
            if (deskNames != null)
            {
                if (deskNames.Count == 1)
                    mainDeskName = deskNames[0];
                else if (deskNames.Count > 1)
                    mainDeskName = $"{deskNames[0]}...";
            }
            return mainDeskName;
        }

        public ActionResult TennisList(int categoryId, bool desOrder, bool isChronological = false,
     int dateFilterType = Schedules.DateFilterPeriod.All,
     DateTime? dateFrom = null, DateTime? dateTo = null, int? seasonId = null, int? departmentId = null, string swapError = null)
        {
            var res = new TennisSchedules
            {
                IsDepartmentLeague = departmentId.HasValue ? true : false,
                DepartmentId = departmentId,
                dateFilterType = dateFilterType,
                dateFrom = dateFrom ?? Schedules.FirstDayOfMonth,
                dateTo = dateTo ?? Schedules.Tomorrow,
                RoundStartCycle = gamesRepo.GetTennisGameSettings(categoryId)?.RoundStartCycle
            };

            var category = teamRepo.GetById(categoryId);
            var league = teamRepo.GetLeagueByTeamId(categoryId);
            var unionId = league?.UnionId;
            var clubId = league?.ClubId;
            if (swapError != null && !string.IsNullOrWhiteSpace(swapError))
            {
                ViewBag.SwapError = swapError;
            }

            var cond = new GamesRepo.GameFilterConditions
            {
                seasonId = seasonId ?? league.SeasonId ?? seasonsRepository.GetLastSeason().Id,
                auditoriums = new List<AuditoriumShort>(),
                leagueId = league?.LeagueId
            };
            if (dateFilterType == Schedules.DateFilterPeriod.BeginningOfMonth && isChronological)
            {
                cond.dateFrom = Schedules.FirstDayOfMonth;
                cond.dateTo = null;
            }
            else if (dateFilterType == Schedules.DateFilterPeriod.Ranged && isChronological)
            {
                cond.dateFrom = dateFrom;
                cond.dateTo = dateTo;
            }
            else if (dateFilterType == Schedules.DateFilterPeriod.FromToday)
            {
                cond.dateFrom = Schedules.Today;
                cond.dateTo = null;
            }
            else
            {
                cond.dateFrom = null;
                cond.dateTo = null;
            }
            var shouldSave = false;
            var playersInCategory = db.TeamsPlayers.Where(tp => tp.TeamId == categoryId).ToList();
            var playersInGames = new List<TeamsPlayer>();


            var dbLazyLoad = db.Configuration.LazyLoadingEnabled;
            var dbDetectChanges = db.Configuration.AutoDetectChangesEnabled;

            var tennisStages = db.TennisStages.Where(ts => ts.CategoryId == categoryId).Select(ts => new TennisStageShort { StageId = ts.StageId, Number = ts.Number, CategoryId = categoryId }).ToList();
            var tennisStageIds = tennisStages.Select(ts => ts.StageId).ToList();

            res.Games = gamesRepo.GetTennisCyclesByFilterConditions(cond, User.IsInAnyRole(AppRole.Admins, AppRole.Editors, AppRole.Workers), false).Where(x => tennisStageIds.Contains(x.StageId)).ToList();

            var tennisGameCycleIds = res.Games.Select(gc => gc.CycleId).ToList();

            db.Configuration.LazyLoadingEnabled = false;
            db.Configuration.AutoDetectChangesEnabled = false;


            var teamPlayers1 = res.Games.Select(gc => gc.FirstPlayerId).ToList();
            var teamPlayers11 = res.Games.Select(gc => gc.SecondPlayerId).ToList();
            var teamPlayers111 = res.Games.Select(gc => gc.FirstPlayerPairId).ToList();
            var teamPlayers1111 = res.Games.Select(gc => gc.SecondPlayerPairId).ToList();
            var teamPlayerIds = teamPlayers1.Union(teamPlayers11).Union(teamPlayers111).Union(teamPlayers1111).ToList();
            db.TeamsPlayers.Include(x => x.User).Where(x => teamPlayerIds.Contains(x.Id)).Load();

            var tennisstageIds = res.Games.Select(gc => gc.StageId).ToList();
            db.TennisStages.Where(x => tennisstageIds.Contains(x.StageId)).Load();

            var tennisGroupIds = res.Games.Select(gc => gc.GroupId).ToList();
            db.TennisGroups.Where(x => tennisGroupIds.Contains(x.GroupId)).Load();

            var tennisPlayoffBracketIds = res.Games.Select(gc => gc.BracketId).ToList();
            db.TennisPlayoffBrackets.Where(x => tennisPlayoffBracketIds.Contains(x.Id)).Load();

            var tennisCycleIdsIds = res.Games.Select(gc => gc.CycleId).ToList();
            db.TennisGameSets.Where(x => tennisCycleIdsIds.Contains(x.GameCycleId)).Load();
            

            foreach (var tennisGameCycle in res.Games)
            {
                tennisGameCycle.GameTypeId = tennisGameCycle.TennisGroup.TypeId;
                tennisGameCycle.CategoryId = tennisStageIds.Contains(tennisGameCycle.StageId) ? categoryId : 0;
            }



            db.Configuration.LazyLoadingEnabled = dbLazyLoad;
            db.Configuration.AutoDetectChangesEnabled = dbDetectChanges;
            foreach (var game in res.Games)
            {
                if (game.FirstPlayerId > 0)
                {
                    playersInGames.Add(game.TeamsPlayer);
                }
                if (game.SecondPlayerId > 0)
                {
                    playersInGames.Add(game.TeamsPlayer1);
                }
                if (game.FirstPlayerPairId > 0)
                {
                    playersInGames.Add(game.FirstPairPlayer);
                }
                if (game.SecondPlayerPairId > 0)
                {
                    playersInGames.Add(game.SecondPairPlayer);
                }
                if (game.GameTypeId == GameTypeId.Playoff || game.GameTypeId == GameTypeId.Knockout || game.GameTypeId == GameTypeId.Knockout34 || game.GameTypeId == GameTypeId.Knockout34Consolences1Round || game.GameTypeId == GameTypeId.Knockout34ConsolencesQuarterRound)
                {
                    if (game.TennisPlayoffBracket.Parent1 != null && game.TennisPlayoffBracket.Parent2 != null)
                    {
                        switch (game.TennisPlayoffBracket.Type)
                        {
                            case (int)PlayoffBracketType.Winner:
                            case (int)PlayoffBracketType.CondolenceWinner:
                                {
                                    if (game.TennisPlayoffBracket.Parent1.WinnerId.HasValue)
                                    {
                                        game.FirstPlayerId = game.TennisPlayoffBracket.Parent1.WinnerId.Value;
                                        game.FirstPlayerPairId = game.TennisPlayoffBracket.Parent1.WinnerPlayerPairId;
                                        if (game.FirstPlayer != null)
                                        {
                                            game.TeamsPlayer = game.FirstPlayer;
                                        }
                                        else
                                        {
                                            var player = playersInCategory.FirstOrDefault(tp => tp.Id == game.FirstPlayerId);
                                            if (player == null)
                                            {
                                                player = db.TeamsPlayers.Find(game.FirstPlayerId);
                                            }
                                            game.TeamsPlayer = player;
                                        }
                                        game.FirstPairPlayer = game.FirstPlayerPairId.HasValue ? db.TeamsPlayers.Find(game.FirstPlayerPairId.Value) : null;
                                        shouldSave = gamesRepo.UpdateTennisGameAndBracketForFirstPlayer(game, game.FirstPlayerId, game.FirstPlayerPairId) || shouldSave;
                                    }
                                    if (game.TennisPlayoffBracket.Parent2.WinnerId.HasValue)
                                    {
                                        game.SecondPlayerId = game.TennisPlayoffBracket.Parent2.WinnerId.Value;
                                        game.SecondPlayerPairId = game.TennisPlayoffBracket.Parent2.WinnerPlayerPairId;

                                        if (game.SecondPlayer != null)
                                        {
                                            game.TeamsPlayer1 = game.SecondPlayer;
                                        }
                                        else
                                        {
                                            var player = playersInCategory.FirstOrDefault(tp => tp.Id == game.SecondPlayerId);
                                            if (player == null)
                                            {
                                                player = db.TeamsPlayers.Find(game.SecondPlayerId);
                                            }
                                            game.TeamsPlayer1 = player;
                                        }


                                        game.SecondPairPlayer = game.SecondPlayerPairId.HasValue ? db.TeamsPlayers.Find(game.SecondPlayerPairId.Value) : null;
                                        shouldSave = gamesRepo.UpdateTennisGameAndBracketForSecondPlayer(game, game.SecondPlayerId, game.SecondPlayerPairId) || shouldSave;
                                    }
                                    break;
                                }
                            case (int)PlayoffBracketType.Loseer:
                            case (int)PlayoffBracketType.Condolence3rdPlaceBracket:
                                {
                                    var isAutoMoveNext = false;
                                    if (game.TennisPlayoffBracket.Parent1.LoserId.HasValue)
                                    {
                                        game.FirstPlayerId = game.TennisPlayoffBracket.Parent1.LoserId.Value;
                                        game.FirstPlayerPairId = game.TennisPlayoffBracket.Parent1.LoserPlayerPairId;

                                        if (game.FirstPlayer != null)
                                        {
                                            game.TeamsPlayer = game.FirstPlayer;
                                        }
                                        else
                                        {
                                            var player = playersInCategory.FirstOrDefault(tp => tp.Id == game.FirstPlayerId);
                                            if (player == null)
                                            {
                                                player = db.TeamsPlayers.Find(game.FirstPlayerId);
                                            }
                                            game.TeamsPlayer = player;
                                        }


                                        game.FirstPairPlayer = game.FirstPlayerPairId.HasValue ? db.TeamsPlayers.Find(game.FirstPlayerPairId.Value) : null;
                                        shouldSave = gamesRepo.UpdateTennisGameAndBracketForFirstPlayer(game, game.FirstPlayerId, game.FirstPlayerPairId) || shouldSave;
                                    }
                                    else if (game.TennisPlayoffBracket.Parent1.WinnerId.HasValue)
                                    {
                                        isAutoMoveNext = true;
                                    }

                                    if (game.TennisPlayoffBracket.Parent2.LoserId.HasValue)
                                    {
                                        game.SecondPlayerId = game.TennisPlayoffBracket.Parent2.LoserId.Value;
                                        game.SecondPlayerPairId = game.TennisPlayoffBracket.Parent2.LoserPlayerPairId;

                                        if (game.SecondPlayer != null)
                                        {
                                            game.TeamsPlayer1 = game.SecondPlayer;
                                        }
                                        else
                                        {
                                            var player = playersInCategory.FirstOrDefault(tp => tp.Id == game.SecondPlayerId);
                                            if (player == null)
                                            {
                                                player = db.TeamsPlayers.Find(game.SecondPlayerId);
                                            }
                                            game.TeamsPlayer1 = player;
                                        }

                                        game.SecondPairPlayer = game.SecondPlayerPairId.HasValue ? db.TeamsPlayers.Find(game.SecondPlayerPairId.Value) : null;
                                        shouldSave = gamesRepo.UpdateTennisGameAndBracketForSecondPlayer(game, game.SecondPlayerId, game.SecondPlayerPairId) || shouldSave;
                                    }
                                    else if (game.TennisPlayoffBracket.Parent2.WinnerId.HasValue)
                                    {
                                        isAutoMoveNext = true;
                                    }
                                    if (isAutoMoveNext)
                                    {
                                        var bracket = db.TennisPlayoffBrackets.SingleOrDefault(b => b.Id == game.TennisPlayoffBracket.Id);
                                        if (bracket != null)
                                        {
                                            if (bracket.FirstPlayerId != null)
                                            {
                                                bracket.WinnerId = bracket.FirstPlayerId;
                                            }
                                            if (bracket.SecondPlayerId != null)
                                            {
                                                bracket.WinnerId = bracket.SecondPlayerId;
                                            }
                                            if (bracket.FirstPlayerPairId != null)
                                            {
                                                bracket.WinnerPlayerPairId = bracket.FirstPlayerPairId;
                                            }
                                            if (bracket.SecondPlayerPairId != null)
                                            {
                                                bracket.WinnerPlayerPairId = bracket.SecondPlayerPairId;
                                            }
                                        }
                                        shouldSave = true;
                                    }
                                    break;
                                }
                            case (int)PlayoffBracketType.CondolenceWinnerLooser:
                                {
                                    if (game.TennisPlayoffBracket.Parent1.WinnerId.HasValue)
                                    {
                                        game.FirstPlayerId = game.TennisPlayoffBracket.Parent1.WinnerId.Value;
                                        game.FirstPlayerPairId = game.TennisPlayoffBracket.Parent1.WinnerPlayerPairId;

                                        if (game.FirstPlayer != null)
                                        {
                                            game.TeamsPlayer = game.FirstPlayer;
                                        }
                                        else
                                        {
                                            var player = playersInCategory.FirstOrDefault(tp => tp.Id == game.FirstPlayerId);
                                            if (player == null)
                                            {
                                                player = db.TeamsPlayers.Find(game.FirstPlayerId);
                                            }
                                            game.TeamsPlayer = player;
                                        }

                                        game.FirstPairPlayer = game.FirstPlayerPairId.HasValue ? db.TeamsPlayers.Find(game.FirstPlayerPairId.Value) : null;
                                        shouldSave = gamesRepo.UpdateTennisGameAndBracketForFirstPlayer(game, game.FirstPlayerId, game.FirstPlayerPairId) || shouldSave;
                                    }
                                    if (game.TennisPlayoffBracket.Parent2.LoserId.HasValue)
                                    {
                                        game.SecondPlayerId = game.TennisPlayoffBracket.Parent2.LoserId.Value;
                                        game.SecondPlayerPairId = game.TennisPlayoffBracket.Parent2.LoserPlayerPairId;

                                        if (game.SecondPlayer != null)
                                        {
                                            game.TeamsPlayer1 = game.SecondPlayer;
                                        }
                                        else
                                        {
                                            var player = playersInCategory.FirstOrDefault(tp => tp.Id == game.SecondPlayerId);
                                            if (player == null)
                                            {
                                                player = db.TeamsPlayers.Find(game.SecondPlayerId);
                                            }
                                            game.TeamsPlayer1 = player;
                                        }


                                        game.SecondPairPlayer = game.SecondPlayerPairId.HasValue ? db.TeamsPlayers.Find(game.SecondPlayerPairId.Value) : null;
                                        shouldSave = gamesRepo.UpdateTennisGameAndBracketForSecondPlayer(game, game.SecondPlayerId, game.SecondPlayerPairId) || shouldSave;
                                    }

                                    break;
                                }
                            case (int)PlayoffBracketType.CondolenceLooserWinner:
                                {
                                    if (game.TennisPlayoffBracket.Parent1.LoserId.HasValue)
                                    {
                                        game.FirstPlayerId = game.TennisPlayoffBracket.Parent1.LoserId.Value;
                                        game.FirstPlayerPairId = game.TennisPlayoffBracket.Parent1.LoserPlayerPairId;

                                        if (game.FirstPlayer != null)
                                        {
                                            game.TeamsPlayer = game.FirstPlayer;
                                        }
                                        else
                                        {
                                            var player = playersInCategory.FirstOrDefault(tp => tp.Id == game.FirstPlayerId);
                                            if (player == null)
                                            {
                                                player = db.TeamsPlayers.Find(game.FirstPlayerId);
                                            }
                                            game.TeamsPlayer = player;
                                        }

                                        game.FirstPairPlayer = game.FirstPlayerPairId.HasValue ? db.TeamsPlayers.Find(game.FirstPlayerPairId.Value) : null;
                                        shouldSave = gamesRepo.UpdateTennisGameAndBracketForFirstPlayer(game, game.FirstPlayerId, game.FirstPlayerPairId) || shouldSave;
                                    }
                                    if (game.TennisPlayoffBracket.Parent2.WinnerId.HasValue)
                                    {
                                        game.SecondPlayerId = game.TennisPlayoffBracket.Parent2.WinnerId.Value;
                                        game.SecondPlayerPairId = game.TennisPlayoffBracket.Parent2.WinnerPlayerPairId;

                                        if (game.SecondPlayer != null)
                                        {
                                            game.TeamsPlayer1 = game.SecondPlayer;
                                        }
                                        else
                                        {
                                            var player = playersInCategory.FirstOrDefault(tp => tp.Id == game.SecondPlayerId);
                                            if (player == null)
                                            {
                                                player = db.TeamsPlayers.Find(game.SecondPlayerId);
                                            }
                                            game.TeamsPlayer1 = player;
                                        }

                                        game.SecondPairPlayer = game.SecondPlayerPairId.HasValue ? db.TeamsPlayers.Find(game.SecondPlayerPairId.Value) : null;
                                        shouldSave = gamesRepo.UpdateTennisGameAndBracketForSecondPlayer(game, game.SecondPlayerId, game.SecondPlayerPairId) || shouldSave;
                                    }
                                    break;
                                }
                        }
                    }

                    if ((game.TennisPlayoffBracket.WinnerId == null && game.TennisPlayoffBracket.WinnerPlayerPairId == null) && (game.TennisPlayoffBracket.FirstPlayerId != null || game.TennisPlayoffBracket.SecondPlayerId != null) && (game.TennisPlayoffBracket.FirstPlayerId == null || game.TennisPlayoffBracket.SecondPlayerId == null) && (game.TennisPlayoffBracket.Type == (int)PlayoffBracketType.CondolenceWinnerLooser || game.TennisPlayoffBracket.Type == (int)PlayoffBracketType.CondolenceLooserWinner) && game.GameTypeId == GameTypeId.Knockout34ConsolencesQuarterRound)
                    {
                        if (game.TennisPlayoffBracket.Parent2.Type == (int)PlayoffBracketType.Winner && (game.SecondPlayerId > -1 || game.SecondPlayerPairId > -1) && game.TennisPlayoffBracket.Parent1.Type == (int)PlayoffBracketType.Loseer && ((!game.TennisPlayoffBracket.Parent1.FirstPlayerId.HasValue && !game.TennisPlayoffBracket.Parent1.FirstPlayerPairId.HasValue) || (!game.TennisPlayoffBracket.Parent1.SecondPlayerId.HasValue && !game.TennisPlayoffBracket.Parent1.SecondPlayerPairId.HasValue)))
                        {
                            game.TennisPlayoffBracket.WinnerId = game.SecondPlayerId;
                            game.TennisPlayoffBracket.WinnerPlayerPairId = game.SecondPlayerPairId;
                        }
                        if (game.TennisPlayoffBracket.Parent1.Type == (int)PlayoffBracketType.Winner && (game.FirstPlayerId > -1 || game.FirstPlayerPairId > -1) && game.TennisPlayoffBracket.Parent2.Type == (int)PlayoffBracketType.Loseer && ((!game.TennisPlayoffBracket.Parent2.FirstPlayerId.HasValue && !game.TennisPlayoffBracket.Parent2.FirstPlayerPairId.HasValue) || (!game.TennisPlayoffBracket.Parent2.SecondPlayerId.HasValue && !game.TennisPlayoffBracket.Parent2.SecondPlayerPairId.HasValue)))
                        {
                            game.TennisPlayoffBracket.WinnerId = game.FirstPlayerId;
                            game.TennisPlayoffBracket.WinnerPlayerPairId = game.FirstPlayerPairId;
                        }
                        shouldSave = true;
                    }


                    /*
                    if (game.TennisPlayoffBracket.Parent1 != null && game.TennisPlayoffBracket.Parent2 != null &&
                        game.TennisPlayoffBracket.Type == (int)PlayoffBracketType.Winner)
                    {

                    }
                    else if (game.TennisPlayoffBracket.Parent1 != null && game.TennisPlayoffBracket.Parent2 != null &&
                        game.TennisPlayoffBracket.Type == (int)PlayoffBracketType.Loseer)
                    {

                    }
                    */
                }
            }

            ViewBag.PlayersInGames = playersInGames.Distinct().Select(tp => new PlayerViewModel { Id = tp.Id, FullName = tp.User.FullName }).ToList();
            ViewBag.PlayersNotInGames = playersInCategory.Select(tp => new PlayerViewModel { Id = tp.Id, FullName = tp.User.FullName }).ToList();

            if (shouldSave)
            {
                gamesRepo.Save();
            }



            res.Groups = res.Games.GroupBy(gc => gc.GameTypeId == GameTypeId.Division ? "Division" : gc.TennisGroup.Name)
                .Select(g => new TennisScheduleGroup
                {
                    GroupName = g.Key,
                    GameTypeId = g.First().GameTypeId,
                    IsIndividual = g.First().TennisGroup.IsIndividual,
                    Stages = g.OrderBy(gc => gc.TennisPlayoffBracket?.Id ?? 0).GroupBy(st => st.StageId).Select(st => new TennisScheduleStage
                    {
                        StageId = st.Key,
                        StageNumber = tennisStages.FirstOrDefault(s => s.StageId == st.First().StageId).Number,
                        Items = st.First().GameTypeId == GameTypeId.Division ? st.OrderBy(gc => gc.CycleNum).ThenBy(gc => gc.TennisPlayoffBracket?.Id ?? 0).ToList() : st.ToList()
                    }).OrderBy(s => s.StageNumber).ToList(),
                    BracketsCount = g.GroupBy(st => st.StageId).Select(st => new TennisScheduleStage
                    {
                        StageId = st.Key,
                        StageNumber = tennisStages.FirstOrDefault(s => s.StageId == st.First().StageId).Number,
                        Items = st.ToList()
                    }).OrderBy(s => s.StageNumber).FirstOrDefault()?.Items?.Count ?? 0,
                    Rounds = g.First().TennisGroup.NumberOfCycles
                }).ToList();
            string alias = string.Empty;
            Session["desOrder"] = desOrder;

            if (unionId.HasValue)
            {
                var referees = usersRepo.GetUnionReferees(unionId.Value)
                    .ToDictionary(u => u.UserId, u => u);

                var spectators = usersRepo.GetUnionSpectators(unionId.Value)
                    .ToDictionary(u => u.UserId, u => u);

                res.Auditoriums = auditoriumsRepo.GetByUnionAndSeason(unionId.Value, cond.seasonId)
                                    .Select(au => new AuditoriumShort
                                    {
                                        Id = au.AuditoriumId,
                                        Name = au.Name
                                    }).ToList();

                res.Referees = referees.Union(spectators).ToDictionary(k => k.Key, v => v.Value);
                res.Spectators = spectators.Union(referees).ToDictionary(k => k.Key, v => v.Value);

                alias = league.Union.Section?.Alias;
            }
            else if (clubId.HasValue)
            {
                var referees = usersRepo.GetClubReferees(clubId.Value)
                    .ToDictionary(u => u.UserId, u => u);

                var spectators = usersRepo.GetClubSpectators(clubId.Value)
                    .ToDictionary(u => u.UserId, u => u);

                res.Referees = referees.Union(spectators).ToDictionary(k => k.Key, v => v.Value);
                res.Spectators = spectators.Union(referees).ToDictionary(k => k.Key, v => v.Value);

                res.Auditoriums = auditoriumsRepo.GetByClubAndSeason(clubId.Value, cond.seasonId)
                                    .Select(au => new AuditoriumShort
                                    {
                                        Id = au.AuditoriumId,
                                        Name = au.Name
                                    }).ToList();

                alias = departmentId == null ? league?.Club?.Section?.Alias : clubsRepo.GetById(departmentId.Value)?.SportSection?.Alias;
            }

            res.SeasonId = cond.seasonId;
            res.UnionId = unionId;
            res.teamsByGroups = teamRepo.GetGroupTeamsBySeasonAndLeagues(cond.seasonId, new int[] { league.LeagueId });

            res.athletesByGroup = playersRepo.GetAthletesBySeasonAndLeagues(cond.seasonId, new int[] { league.LeagueId });
            Session["isChronological"] = isChronological;

            SetTennisRanks(ref res);

            if (desOrder)
            {
                res.Games = res.Games.OrderByDescending(x => x.TennisStage.Number).ToList();
                foreach (var group in res.Groups)
                {
                    group.Stages = group.Stages.OrderByDescending(st => st.StageNumber).ToList();
                }
            }

            res.CategoryId = categoryId;
            res.Section = alias;

            return PartialView("_TennisList", res);
        }




        public ActionResult TennisCompetitionTeamPlayerSwap(int categoryId, int seasonId, int? swap1, int? swap2)
        {
            string swapError = string.Empty;

            if (swap1 > 0 && swap2 > 0 && swap1 != swap2)
            {
                var playersInCategory = db.TeamsPlayers.Where(tp => tp.TeamId == categoryId).ToList();
                var tennisGameCycles = db.TennisGameCycles.Include("TennisPlayoffBracket").Where(gc => gc.TennisStage.SeasonId == seasonId && gc.TennisStage.CategoryId == categoryId).ToList();
                foreach (var tennisGameCycle in tennisGameCycles)
                {
                    if (tennisGameCycle.FirstPlayerId > 0)
                    {
                        if (tennisGameCycle.FirstPlayerId == swap1)
                        {
                            tennisGameCycle.FirstPlayerId = swap2;
                            tennisGameCycle.TennisPlayoffBracket.FirstPlayerId = swap2;
                        }
                        else if (tennisGameCycle.FirstPlayerId == swap2)
                        {
                            tennisGameCycle.FirstPlayerId = swap1;
                            tennisGameCycle.TennisPlayoffBracket.FirstPlayerId = swap1;
                        }
                    }

                    if (tennisGameCycle.FirstPlayerPairId > 0)
                    {
                        if (tennisGameCycle.FirstPlayerPairId == swap1)
                        {
                            tennisGameCycle.FirstPlayerPairId = swap2;
                            tennisGameCycle.TennisPlayoffBracket.FirstPlayerPairId = swap2;
                        }
                        else if (tennisGameCycle.FirstPlayerPairId == swap2)
                        {
                            tennisGameCycle.FirstPlayerPairId = swap1;
                            tennisGameCycle.TennisPlayoffBracket.FirstPlayerPairId = swap1;
                        }
                    }

                    if (tennisGameCycle.SecondPlayerId > 0)
                    {
                        if (tennisGameCycle.SecondPlayerId == swap1)
                        {
                            tennisGameCycle.SecondPlayerId = swap2;
                            tennisGameCycle.TennisPlayoffBracket.SecondPlayerId = swap2;
                        }
                        else if (tennisGameCycle.SecondPlayerId == swap2)
                        {
                            tennisGameCycle.SecondPlayerId = swap1;
                            tennisGameCycle.TennisPlayoffBracket.SecondPlayerId = swap1;
                        }
                    }

                    if (tennisGameCycle.SecondPlayerPairId > 0)
                    {
                        if (tennisGameCycle.SecondPlayerPairId == swap1)
                        {
                            tennisGameCycle.SecondPlayerPairId = swap2;
                            tennisGameCycle.TennisPlayoffBracket.SecondPlayerPairId = swap2;
                        }
                        else if (tennisGameCycle.SecondPlayerPairId == swap2)
                        {
                            tennisGameCycle.SecondPlayerPairId = swap1;
                            tennisGameCycle.TennisPlayoffBracket.SecondPlayerPairId = swap1;
                        }
                    }

                    if (tennisGameCycle.TechnicalWinnerId > 0)
                    {
                        if (tennisGameCycle.TechnicalWinnerId == swap1)
                        {
                            tennisGameCycle.TechnicalWinnerId = swap2;
                        }
                        else if (tennisGameCycle.TechnicalWinnerId == swap2)
                        {
                            tennisGameCycle.TechnicalWinnerId = swap1;
                        }
                    }

                    if (tennisGameCycle.TennisPlayoffBracket.WinnerId > 0)
                    {
                        if (tennisGameCycle.TennisPlayoffBracket.WinnerId == swap1)
                        {
                            tennisGameCycle.TennisPlayoffBracket.WinnerId = swap2;
                        }
                        else if (tennisGameCycle.TennisPlayoffBracket.WinnerId == swap2)
                        {
                            tennisGameCycle.TennisPlayoffBracket.WinnerId = swap1;
                        }
                    }

                    if (tennisGameCycle.TennisPlayoffBracket.WinnerPlayerPairId > 0)
                    {
                        if (tennisGameCycle.TennisPlayoffBracket.WinnerPlayerPairId == swap1)
                        {
                            tennisGameCycle.TennisPlayoffBracket.WinnerPlayerPairId = swap2;
                        }
                        else if (tennisGameCycle.TennisPlayoffBracket.WinnerPlayerPairId == swap2)
                        {
                            tennisGameCycle.TennisPlayoffBracket.WinnerPlayerPairId = swap1;
                        }
                    }

                    if (tennisGameCycle.TennisPlayoffBracket.LoserId > 0)
                    {
                        if (tennisGameCycle.TennisPlayoffBracket.LoserId == swap1)
                        {
                            tennisGameCycle.TennisPlayoffBracket.LoserId = swap2;
                        }
                        else if (tennisGameCycle.TennisPlayoffBracket.LoserId == swap2)
                        {
                            tennisGameCycle.TennisPlayoffBracket.LoserId = swap1;
                        }
                    }

                    if (tennisGameCycle.TennisPlayoffBracket.LoserPlayerPairId > 0)
                    {
                        if (tennisGameCycle.TennisPlayoffBracket.LoserPlayerPairId == swap1)
                        {
                            tennisGameCycle.TennisPlayoffBracket.LoserPlayerPairId = swap2;
                        }
                        else if (tennisGameCycle.TennisPlayoffBracket.LoserPlayerPairId == swap2)
                        {
                            tennisGameCycle.TennisPlayoffBracket.LoserPlayerPairId = swap1;
                        }
                    }


                    if ((tennisGameCycle.FirstPlayerId > 0 && tennisGameCycle.SecondPlayerId == tennisGameCycle.FirstPlayerId) || (tennisGameCycle.FirstPlayerPairId > 0 && tennisGameCycle.SecondPlayerPairId == tennisGameCycle.FirstPlayerPairId))
                    {
                        if (tennisGameCycle.FirstPlayerId > 0)
                        {
                            swapError = string.Format(Messages.ErrorCantMatchAgainstSelf, tennisGameCycle.TeamsPlayer.User.FullName);
                        }
                        else
                        {
                            swapError = string.Format(Messages.ErrorCantMatchAgainstSelf, tennisGameCycle.TeamsPlayer1.User.FullName);
                        }
                    }


                }
            }
            else
            {
                swapError = Messages.MustChoose2DifferentPlayersForSwap;
            }
            if (string.IsNullOrEmpty(swapError))
            {
                var swap1GroupTeam = db.TennisGroupTeams.FirstOrDefault(tgt => tgt.TennisGroup.TennisStage.SeasonId == seasonId && tgt.TennisGroup.TennisStage.CategoryId == categoryId && tgt.PlayerId == swap1 && swap1 > 0);
                var swap2GroupTeam = db.TennisGroupTeams.FirstOrDefault(tgt => tgt.TennisGroup.TennisStage.SeasonId == seasonId && tgt.TennisGroup.TennisStage.CategoryId == categoryId && tgt.PlayerId == swap2 && swap2 > 0);
                var swap1GroupPTeam = db.TennisGroupTeams.FirstOrDefault(tgt => tgt.TennisGroup.TennisStage.SeasonId == seasonId && tgt.TennisGroup.TennisStage.CategoryId == categoryId && tgt.PairPlayerId == swap1 && swap1 > 0);
                var swap2GroupPTeam = db.TennisGroupTeams.FirstOrDefault(tgt => tgt.TennisGroup.TennisStage.SeasonId == seasonId && tgt.TennisGroup.TennisStage.CategoryId == categoryId && tgt.PairPlayerId == swap2 && swap2 > 0);
                if (swap1GroupTeam != null && swap2 > 0) {
                    swap1GroupTeam.PlayerId = swap2;
                }
                if (swap2GroupTeam != null && swap1 > 0)
                {
                    swap2GroupTeam.PlayerId = swap1;
                }
                if (swap1GroupPTeam != null && swap2 > 0)
                {
                    swap1GroupPTeam.PairPlayerId = swap2;
                }
                if (swap2GroupPTeam != null && swap1 > 0)
                {
                    swap2GroupPTeam.PairPlayerId = swap1;
                }
                db.SaveChanges();
            }
            return Json(new { });
            //return RedirectToAction("TennisList",new { @categoryId = categoryId, DesOrder= false, dateFilterType = 2, @swapError = swapError });
        }


        private void SetRanks(ref Schedules model)
        {
            foreach (var group in model.Groups.Where(g => g.GameTypeId == GameTypeId.Playoff || g.GameTypeId == GameTypeId.Knockout || g.GameTypeId == GameTypeId.Knockout34 || g.GameTypeId == GameTypeId.Knockout34Consolences1Round || g.GameTypeId == GameTypeId.Knockout34ConsolencesQuarterRound))
            {
                var firstStage = group.Stages[0];
                var firstTeamId = firstStage.Items.First().HomeTeamId;
                var firstTeamPos = firstStage.Items.First().HomeTeamPos;
                int rounds = firstTeamId == null
                    ? firstStage.Items.Count(x => x.GuestTeamPos == firstTeamPos || x.HomeTeamPos == firstTeamPos)
                    : firstStage.Items.Count(x => x.GuestTeamId == firstTeamId || x.HomeTeamId == firstTeamId);
                GenerateTeamNumbers teamNumbers;
                try
                {
                    teamNumbers = new GenerateTeamNumbers(firstStage.Items.Count * 2, rounds);
                }
                catch (Exception)
                {
                    //  If it is impossible to generate ranks due to the wrong count of teams in the group,
                    //  then just skip it.
                    continue;
                }
                int StageIndex = 0;
                foreach (var stage in group.Stages.OrderBy(s => s.StageId))
                {
                    var stageFirst = stage.Items.FirstOrDefault();
                    var maxMinStr = $"{stageFirst.MaxPlayoffPos} - {stageFirst.MinPlayoffPos}";
                    StageIndex++;
                    var maxSwapIndex = teamNumbers.MaxSwapIndexForStage(StageIndex - 1);
                    var maxItemsForThisStage = teamNumbers.GetStageTotalMatchesPerRow(StageIndex - 1);
                    var currentSwapIndex = 1;
                    var itmCount = maxItemsForThisStage;
                    foreach (var games in stage.Items.GroupBy(gc => gc.CycleNum))
                    {
                        foreach (var gc in games.OrderBy(gc => gc.CycleId))
                        {
                            if (StageIndex > 1 && gc.GameTypeId == GameTypeId.Playoff)
                            {
                                gc.Rank = teamNumbers.PrintTeamIndex(StageIndex - 2, currentSwapIndex - 1);
                                if (itmCount > 0)
                                {
                                    itmCount--;
                                }
                                if (itmCount == 0)
                                {
                                    if (currentSwapIndex == maxSwapIndex)
                                    {
                                        maxItemsForThisStage = teamNumbers.GetStageTotalMatchesPerRow(StageIndex - 1);
                                        maxSwapIndex = teamNumbers.MaxSwapIndexForStage(StageIndex - 1);
                                        currentSwapIndex = 1;
                                    }
                                    else if (currentSwapIndex < maxSwapIndex)
                                    {
                                        currentSwapIndex++;
                                    }
                                    itmCount = maxItemsForThisStage;
                                }
                            }
                            else if (gc.GameTypeId == GameTypeId.Knockout34 && gc.PlayoffBracket.Type == (int)PlayoffBracketType.Condolence3rdPlaceBracket)
                            {
                                gc.Rank = "3 - 4";
                            }
                            else
                            {
                                gc.Rank = maxMinStr;
                            }
                        }
                    }
                }
            }
        }

        private void SetTennisRanks(ref TennisSchedules model)
        {
            foreach (var group in model.Groups.Where(g => g.GameTypeId == GameTypeId.Playoff || g.GameTypeId == GameTypeId.Knockout || g.GameTypeId == GameTypeId.Knockout34 || g.GameTypeId == GameTypeId.Knockout34Consolences1Round || g.GameTypeId == GameTypeId.Knockout34ConsolencesQuarterRound))
            {
                var firstStage = group.Stages[0];
                var firstPlayerId = firstStage.Items.First().FirstPlayerId;
                var firstPlayerPos = firstStage.Items.First().FirstPlayerPos;
                int rounds = firstPlayerId == null
                    ? firstStage.Items.Count(x => x.SecondPlayerPos == firstPlayerPos || x.FirstPlayerPos == firstPlayerPos)
                    : firstStage.Items.Count(x => x.SecondPlayerId == firstPlayerId || x.FirstPlayerId == firstPlayerId);
                GenerateTeamNumbers teamNumbers;
                try
                {
                    teamNumbers = new GenerateTeamNumbers(firstStage.Items.Count * 2, rounds);
                }
                catch (Exception)
                {
                    //  If it is impossible to generate ranks due to the wrong count of teams in the group,
                    //  then just skip it.
                    continue;
                }
                int StageIndex = 0;
                foreach (var stage in group.Stages.OrderBy(s => s.StageId))
                {
                    var stageFirst = stage.Items.FirstOrDefault();
                    var maxMinStr = $"{stageFirst.MaxPlayoffPos} - {stageFirst.MinPlayoffPos}";
                    StageIndex++;
                    var maxSwapIndex = teamNumbers.MaxSwapIndexForStage(StageIndex - 1);
                    var maxItemsForThisStage = teamNumbers.GetStageTotalMatchesPerRow(StageIndex - 1);
                    var currentSwapIndex = 1;
                    var itmCount = maxItemsForThisStage;
                    foreach (var games in stage.Items.GroupBy(gc => gc.CycleNum))
                    {
                        foreach (var gc in games.OrderBy(gc => gc.CycleId))
                        {
                            if (StageIndex > 1 && gc.GameTypeId == GameTypeId.Playoff)
                            {
                                gc.Rank = teamNumbers.PrintTeamIndex(StageIndex - 2, currentSwapIndex - 1);
                                if (itmCount > 0)
                                {
                                    itmCount--;
                                }
                                if (itmCount == 0)
                                {
                                    if (currentSwapIndex == maxSwapIndex)
                                    {
                                        maxItemsForThisStage = teamNumbers.GetStageTotalMatchesPerRow(StageIndex - 1);
                                        maxSwapIndex = teamNumbers.MaxSwapIndexForStage(StageIndex - 1);
                                        currentSwapIndex = 1;
                                    }
                                    else if (currentSwapIndex < maxSwapIndex)
                                    {
                                        currentSwapIndex++;
                                    }
                                    itmCount = maxItemsForThisStage;
                                }
                            }
                            else if (gc.GameTypeId == GameTypeId.Knockout34Consolences1Round || gc.GameTypeId == GameTypeId.Knockout34ConsolencesQuarterRound)
                            {

                                gc.Rank = $"{gc.MaxPlayoffPos} - {gc.MinPlayoffPos}";
                            }
                            else if (gc.GameTypeId == GameTypeId.Knockout34 && gc.TennisPlayoffBracket.Type == (int)PlayoffBracketType.Condolence3rdPlaceBracket)
                            {
                                gc.Rank = "3 - 4";
                            }
                            else
                            {
                                gc.Rank = maxMinStr;
                            }
                        }
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult PublishAllLeagueGamesCycles(int seasonId, int leagueId, bool isPublished)
        {
            try
            {
                IEnumerable<GamesCycle> games = gamesRepo.GetGroupsCycles(leagueId, seasonId).ToList();
                foreach (var game in games)
                {
                    game.IsPublished = isPublished;
                }

                gamesRepo.Update(games);

                return new HttpStatusCodeResult(200);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(500);
            }
        }

        [HttpPost]
        public ActionResult PublishAllCategoryGamesCycles(int seasonId, int categoryId, bool isPublished)
        {
            try
            {
                IEnumerable<TennisGameCycle> games = gamesRepo.GetTennisGroupsCycles(categoryId, seasonId).ToList();
                foreach (var game in games)
                {
                    game.IsPublished = isPublished;
                }

                gamesRepo.UpdateTennis(games);

                return new HttpStatusCodeResult(200);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(500);
            }
        }

        [HttpPost]
        public ActionResult PublishAllStageGameCycles(int seasonId, int leagueId, int stageId, bool IsPublished)
        {
            try
            {
                var gameCycles = gamesRepo.GetBySeasonLeagueAndStage(seasonId, leagueId, stageId).ToList();
                gameCycles.ForEach(gc => gc.IsPublished = IsPublished);
                gamesRepo.Update(gameCycles);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [HttpPost]
        public ActionResult PublishAllTennisStageGameCycles(int seasonId, int categoryId, int stageId, bool IsPublished)
        {
            try
            {
                var gameCycles = gamesRepo.GetTennisBySeasonCategoryAndStage(seasonId, categoryId, stageId).ToList();
                gameCycles.ForEach(gc => gc.IsPublished = IsPublished);
                gamesRepo.UpdateTennis(gameCycles);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [HttpPost]
        public ActionResult PublishGamesCyclesByCycleNumber(int seasonId, int leagueId, int stageId, int cycleNum, bool isPublished)
        {
            try
            {
                var games = gamesRepo.GetGroupsCycles(leagueId, seasonId)
                                      .Where(g => g.CycleNum == cycleNum && g.StageId == stageId)
                                      .ToList();
                games.ForEach(g => g.IsPublished = isPublished);

                gamesRepo.Update(games);

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [HttpPost]
        public ActionResult PublishTennisGamesCyclesByCycleNumber(int seasonId, int categoryId, int stageId, int cycleNum, bool isPublished)
        {
            try
            {
                var games = gamesRepo.GetTennisGroupsCycles(categoryId, seasonId)
                                      .Where(g => g.CycleNum == cycleNum && g.StageId == stageId)
                                      .ToList();
                games.ForEach(g => g.IsPublished = isPublished);

                gamesRepo.UpdateTennis(games);

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [HttpPost]
        public ActionResult PublishGamesCycle(int gameCycleId, bool isPublished)
        {
            try
            {
                GamesCycle gameCycle = gamesRepo.GetGameCycleById(gameCycleId);
                gameCycle.IsPublished = isPublished;

                gamesRepo.Update(gameCycle);

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [HttpPost]
        public ActionResult PublishTennisGamesCycle(int gameCycleId, bool isPublished)
        {
            try
            {
                TennisGameCycle gameCycle = gamesRepo.GetTennisGameCycleById(gameCycleId);
                gameCycle.IsPublished = isPublished;

                gamesRepo.UpdateTennis(gameCycle);

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        public ActionResult TeamList(int id, int seasonId, int? leagueId, int? departmentId = null)
        {
            HashSet<int> referees;
            var model = new TeamSchedules();
            model.TeamId = id;
            model.SeasonId = seasonId;
            model.Clubs = clubsRepo.GetByTeamAndSeason(id, seasonId).Where(c => c.IsSectionClub.Value).ToList();
            string alias = String.Empty;
            Section section = secRepo.GetSectionByTeamId(id);
            if (departmentId.HasValue && departmentId > 0)
            {
                alias = clubsRepo.GetById(departmentId.Value)?.SportSection?.Alias ?? clubsRepo.GetById(departmentId.Value)?.Union?.Section?.Alias;
            }
            else if (section != null)
            {
                alias = section?.Alias;
            }
            else
            {
                alias = leagueRepo.GetById(leagueId ?? 0)?.Club?.Section?.Alias;
            }

            var teamLeagues = alias == GamesAlias.Tennis ? leagueRepo.GetByTeamAndSeasonForTennisLeaguesShort(id, seasonId).ToList() : leagueRepo.GetByTeamAndSeasonShort(id, seasonId).ToList();
            model.LeaguesWithCycles = teamRepo.GetTeamGames(id, seasonId, teamLeagues, out referees, alias == GamesAlias.Tennis).ToList();

            model.RefereesNames = usersRepo.GetUserNamesByIds(referees);



            model.IsCatchball = alias == GamesAlias.NetBall;
            model.IsWaterpolo = alias == GamesAlias.WaterPolo;
            ViewBag.Section = alias;
            switch (alias)
            {
                case GamesAlias.BasketBall:
                case GamesAlias.Soccer:
                case GamesAlias.Rugby:
                case GamesAlias.WaterPolo:
                case GamesAlias.Softball:
                    return PartialView("BasketBallWaterPolo/_TeamList", model);

                default:
                    return PartialView("_TeamList", model);

            }
        }

        public ActionResult ExportToExcelUnion(List<int> leaguesId, int sortType, int seasonId)
        {
            leaguesId = (List<int>)Session["LeaguesIds"];
            var xlsService = new ExcelGameService();
            bool isBasketBallOrWaterPolo = false;
            var games = new List<ExcelGameDto>();
            foreach (var leagueId in leaguesId)
            {
                games.AddRange(xlsService.GetLeagueGames(leagueId, seasonId));
            }

            if (games?.Count() > 0)
            {
                isBasketBallOrWaterPolo = gamesRepo.IsBasketBallOrWaterPoloGameCycle(games[0].GameId);
            }

            if (sortType == 1)
                games = games.OrderBy(x => x.Date).ToList();
            if (sortType == 2)
                games = games.OrderBy(x => x.Auditorium).ToList();

            return ToExcel(games, isBasketBallOrWaterPolo);
        }

        public ActionResult ExportToExcel(int? leagueId, int? teamId, int? currentLeagueId, int? seasonId,
            int dateFilterType = Schedules.DateFilterPeriod.All,
            DateTime? dateFrom = null, DateTime? dateTo = null, IEnumerable<int> columns = null, LogicaName logicalName = LogicaName.Unspecified)
        {
            var xlsService = new ExcelGameService();
            bool userIsEditor = User.IsInAnyRole(AppRole.Admins, AppRole.Editors, AppRole.Workers);
            var games = new List<ExcelGameDto>();
            bool isBasketBallOrWaterPolo = false;
            if (leagueId.HasValue)
            {
                isBasketBallOrWaterPolo = leagueRepo.IsBasketBallOrWaterPoloLeague(leagueId.Value);
                switch (dateFilterType)
                {
                    case Schedules.DateFilterPeriod.BeginningOfMonth:
                        games = xlsService.GetLeagueGames(leagueId.Value, userIsEditor, Schedules.FirstDayOfMonth, null, seasonId).ToList();
                        break;
                    case Schedules.DateFilterPeriod.Ranged:
                        games = xlsService.GetLeagueGames(leagueId.Value, userIsEditor, dateFrom, dateTo, seasonId).ToList();
                        break;
                    case Schedules.DateFilterPeriod.FromToday:
                        games = xlsService.GetLeagueGames(leagueId.Value, userIsEditor, Schedules.Today, null, seasonId).ToList();
                        break;
                    default:
                        games = xlsService.GetLeagueGames(leagueId.Value, userIsEditor, null, null, seasonId).ToList();
                        break;
                }
            }
            else if (teamId.HasValue)
            {
                if (!seasonId.HasValue)
                {
                    seasonId = teamRepo.GetSeasonIdByTeamId(teamId.Value, DateTime.Now);
                }
                isBasketBallOrWaterPolo = seasonsRepository.IsBasketBallOrWaterPoloSeason(seasonId.Value);
                if (currentLeagueId.HasValue)
                {
                    games = xlsService.GetTeamGames(teamId.Value, currentLeagueId.Value, seasonId).ToList();
                }
                else
                {
                    games = xlsService.GetTeamGames(teamId.Value, seasonId).ToList();
                }
            }
            string leagueName = null;
            if (leagueId.HasValue)
            {
                leagueName = leagueRepo.GetById(leagueId.Value)?.Name;
            }
            return ToExcel(games, isBasketBallOrWaterPolo, false, null, false, leagueName, logicalName);
        }

        public ActionResult ExportToExcelTennisLeague(int? leagueId, int? seasonId,
         int dateFilterType = Schedules.DateFilterPeriod.All,
         DateTime? dateFrom = null, DateTime? dateTo = null, IEnumerable<int> columns = null)
        {
            var xlsService = new ExcelGameService();
            bool userIsEditor = User.IsInAnyRole(AppRole.Admins, AppRole.Editors, AppRole.Workers);
            var games = new List<ExcelGameDto>();
            var leagueName = string.Empty;
            if (leagueId.HasValue)
            {
                leagueName = leagueRepo.GetById(leagueId.Value)?.Name;
                switch (dateFilterType)
                {
                    case Schedules.DateFilterPeriod.BeginningOfMonth:
                        games = xlsService.GetLeagueGames(leagueId.Value, userIsEditor, Schedules.FirstDayOfMonth, null, seasonId).ToList();
                        break;
                    case Schedules.DateFilterPeriod.Ranged:
                        games = xlsService.GetLeagueGames(leagueId.Value, userIsEditor, dateFrom, dateTo, seasonId).ToList();
                        break;
                    case Schedules.DateFilterPeriod.FromToday:
                        games = xlsService.GetLeagueGames(leagueId.Value, userIsEditor, Schedules.Today, null, seasonId).ToList();
                        break;
                    default:
                        games = xlsService.GetLeagueGames(leagueId.Value, userIsEditor, null, null, seasonId).ToList();
                        break;
                }
            }

            return ToExcel(games, false, false, columns, true, leagueName);
        }

        [HttpPost]
        public ActionResult ExportToExcelUnion(List<int> leaguesId, int sortType, FormCollection form, IEnumerable<int> columns = null,
            bool isTennisLeague = false)
        {
            var xlsService = new ExcelGameService();
            var gameIds = string.IsNullOrWhiteSpace(form["gameIds1"])
             ? new int[] { }
               : form["gameIds1"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                 .Select(s => int.Parse(s))
                 .ToArray();

            bool isBacketBallOrWaterPolo = false;

            var games = xlsService.GetGameCyclesByIdSet(gameIds).ToList();
            if (games?.Count > 0)
            {
                isBacketBallOrWaterPolo = gamesRepo.IsBasketBallOrWaterPoloGameCycle(games[0].GameId);
            }
            /*
            if (sortType == 1)
                games = games.OrderBy(x => x.Date).ToList();
            if (sortType == 2)
                games = games.OrderBy(x => x.Auditorium).ToList();
            */
            //var roundStartCycle = games.FirstOrDefault()?.Section == GamesAlias.Tennis && games.FirstOrDefault()?.IsTennisLeagueGame == true
            //    ? gamesRepo.GetTennisGameSettings(teamId ?? 0)?.RoundStartCycle
            //    : gamesRepo.GetGameSettings(leagueId ?? currentLeagueId ?? 0)?.RoundStartCycle;
            return ToExcel(games, isBacketBallOrWaterPolo, false, columns, isTennisLeague, null);

        }

        [HttpPost]
        public void CheckGames(FormCollection form)
        {
            var gameIds = String.IsNullOrWhiteSpace(form["gameIds"])
              ? new int[] { }
            : form["gameIds"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
              .Select(s => int.Parse(s))
              .ToArray();

            Session["GameIds"] = gameIds;
        }

        [HttpPost]
        public ActionResult External(int? leagueId, FormCollection form)
        {
            var gameIds = Session["GameIds"] != null ? (int[])Session["GameIds"] : new int[] { };
            var url = GlobVars.SiteUrl + "/LeagueTable/Schedules/" + leagueId + "?gameIds=" + String.Join(",", gameIds);

            return Redirect(url);
        }

        [HttpPost]
        public ActionResult ExportToExcel(int? leagueId, int? teamId, int? currentLeagueId, int? seasonId, FormCollection form, IEnumerable<int> columns = null)
        {
            var xlsService = new ExcelGameService();
            var gameIds = Session["GameIds"] != null ? (int[])Session["GameIds"] : new int[] { };
            var games = new List<ExcelGameDto>();
            bool isBasketBallOrWaterPolo = false;
            string leagueName = null;
            if (leagueId.HasValue)
            {
                isBasketBallOrWaterPolo = leagueRepo.IsBasketBallOrWaterPoloLeague(leagueId.Value);
                games = xlsService.GetLeagueGames(leagueId.Value, seasonId).OrderBy(g => g.Date).ToList();
                leagueName = leagueRepo.GetById(leagueId.Value).Name;
            }
            else if (teamId.HasValue)
            {
                if (!seasonId.HasValue)
                {
                    seasonId = teamRepo.GetSeasonIdByTeamId(teamId.Value, DateTime.Now);
                }
                isBasketBallOrWaterPolo = seasonsRepository.IsBasketBallOrWaterPoloSeason(seasonId.Value);
                if (currentLeagueId.HasValue)
                {
                    games = xlsService.GetTeamGames(teamId.Value, currentLeagueId.Value, gameIds, seasonId).ToList();
                }
                else
                {
                    games = xlsService.GetTeamGames(teamId.Value, gameIds, seasonId).ToList();
                }
            }
            var isIndividual = games.FirstOrDefault().IsIndividual;

            var roundStartCycle = games.FirstOrDefault()?.Section == GamesAlias.Tennis && games.FirstOrDefault()?.IsTennisLeagueGame == true
                ? gamesRepo.GetTennisGameSettings(teamId ?? 0)?.RoundStartCycle
                : gamesRepo.GetGameSettings(leagueId ?? currentLeagueId ?? 0)?.RoundStartCycle;

            return columns != null && columns.Any() ? ToExcel(games, isBasketBallOrWaterPolo, isIndividual, columns, false, null)
                : ToExcel(games, isBasketBallOrWaterPolo, isIndividual, null, false, null);
        }

        private ActionResult ToExcel(IList<ExcelGameDto> games, bool isBasketBallOrWaterPolo = false, bool isIndividual = false, IEnumerable<int> columns = null,
            bool isTennisLeague = false, string leagueName = null, LogicaName logicalName = LogicaName.Unspecified)
        {
            var section = games.FirstOrDefault()?.Section;


            Thread.CurrentThread.CurrentCulture = new CultureInfo("he-IL");
            int maxNumberOfReferees = 0;
            int maxCountOfTennisGame = section.Equals(GamesAlias.Tennis) ? GetMaxCountOfTheTennisGames(games) : 0;
            for (int i = 0; i < games.Count; i++)
            {
                if (games[i].RefereeIds != null)
                {
                    var currentNumberOfReferees = games[i].RefereeIds.Split(',').ToList().Count;
                    if (i == 0)
                        maxNumberOfReferees = currentNumberOfReferees;
                    else
                        maxNumberOfReferees = currentNumberOfReferees > maxNumberOfReferees ? currentNumberOfReferees : maxNumberOfReferees;
                }
            }
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var ws = workbook.Worksheets.Add("Sheet1");
                workbook.Worksheets.FirstOrDefault().RightToLeft = true;
                Dictionary<int, KeyValuePair<int, string>> selectedColumns = new Dictionary<int, KeyValuePair<int, string>>();
                if (columns != null && columns.Any())
                {
                    #region Only selected columns

                    selectedColumns = GetAllSelectedColumnsDictionary(columns, isIndividual, isBasketBallOrWaterPolo,
                        maxNumberOfReferees, maxCountOfTennisGame);
                    if (selectedColumns.Any())
                    {
                        foreach (var column in selectedColumns)
                        {
                            ws.Cell(1, column.Key).Value = column.Value.Value;
                        }
                    }
                    #endregion
                }
                else
                {
                    #region All columns

                    ws.Cell(1, 1).Value = $"{Messages.GameId}";

                    ws.Cell(1, 2).Value = $"{Messages.LeagueId}";

                    ws.Cell(1, 3).Value = $"{Messages.League}";

                    ws.Cell(1, 4).Value = $"{Messages.Stage}";

                    ws.Cell(1, 5).Value = Messages.Round;

                    ws.Cell(1, 6).Value = $"{Messages.Date}";

                    ws.Cell(1, 7).Value = $"{Messages.Time}";

                    ws.Cell(1, 8).Value = $"{Messages.Day}";

                    ws.Cell(1, 9).Value = isIndividual ? $"{Messages.Home} {Messages.Competitor.ToLowerInvariant()} {Messages.Id.ToLowerInvariant()}"
                        : $"{Messages.Home} {Messages.Team.ToLowerInvariant()} {Messages.Id.ToLowerInvariant()}";

                    ws.Cell(1, 10).Value = isIndividual ? $"{Messages.Home} {Messages.Competitor.ToLowerInvariant()}"
                        : $"{Messages.Home} {Messages.Team.ToLowerInvariant()}";

                    ws.Cell(1, 11).Value = isIndividual ? $"{Messages.Home} {Messages.Competitor.ToLowerInvariant()} {Messages.Score.ToLowerInvariant()}"
                        : $"{Messages.Home} {Messages.Team.ToLowerInvariant()} {Messages.Score.ToLowerInvariant()}";

                    ws.Cell(1, 12).Value = isIndividual ? $"{Messages.Guest} {Messages.Competitor.ToLowerInvariant()} {Messages.Id.ToLowerInvariant()}"
                        : $"{Messages.Guest} {Messages.Team.ToLowerInvariant()} {Messages.Id.ToLowerInvariant()}";

                    ws.Cell(1, 13).Value = isIndividual ? $"{Messages.Guest} {Messages.Competitor.ToLowerInvariant()}"
                        : $"{Messages.Guest} {Messages.Team.ToLowerInvariant()}";

                    ws.Cell(1, 14).Value = isIndividual ? $"{Messages.Guest} {Messages.Competitor.ToLowerInvariant()} {Messages.Score.ToLowerInvariant()}"
                        : $"{Messages.Guest} {Messages.Team.ToLowerInvariant()} {Messages.Score.ToLowerInvariant()}";

                    ws.Cell(1, 15).Value = $"{Messages.Auditorium} {Messages.Id}";

                    ws.Cell(1, 16).Value = $"{Messages.Auditorium}";

                    var tAt = 17;
                    if (section != GamesAlias.NetBall || (LogicaName)logicalName != LogicaName.Team)
                    {

                        for (int i = tAt; i < (tAt + maxNumberOfReferees); i++)
                        {
                            ws.Cell(1, i).Value = (i == tAt) ? $"{Messages.Main} {Messages.Referee.ToLowerInvariant()}" :
                                $"{Messages.Referee} #{(i - tAt) + 1}";
                        }
                        ws.Cell(1, tAt + maxNumberOfReferees).Value = $"{Messages.RefereesIds}";
                        tAt++;

                        ws.Cell(1, tAt + maxNumberOfReferees).Value = $"{Messages.SpectatorsIds}";
                        tAt++;
                        ws.Cell(1, tAt + maxNumberOfReferees).Value = $"{Messages.Spectators}";
                        tAt++;
                        ws.Cell(1, tAt + maxNumberOfReferees).Value = $"{Messages.DesksIds}";
                        tAt++;
                        ws.Cell(1, tAt + maxNumberOfReferees).Value = $"{Messages.DesksNames}";
                        tAt++;
                    }
                    else
                    {
                        maxNumberOfReferees = 0;
                    }
                    ws.Cell(1, tAt + maxNumberOfReferees).Value = $"{Messages.Cycle} {Messages.Number.ToLowerInvariant()}";
                    tAt++;
                    ws.Cell(1, tAt + maxNumberOfReferees).Value = $"{Messages.Group}";
                    tAt++;
                    if (isBasketBallOrWaterPolo)
                    {
                        ws.Cell(1, 24 + maxNumberOfReferees).Value = "Q1";
                        ws.Cell(1, 25 + maxNumberOfReferees).Value = "Q2";
                        ws.Cell(1, 26 + maxNumberOfReferees).Value = "Q3";
                        ws.Cell(1, 27 + maxNumberOfReferees).Value = "Q4";
                    }
                    else if (section.Equals(GamesAlias.Tennis))
                    {
                        for (int k = 0; k < maxCountOfTennisGame; k++)
                        {
                            var coef = k * 3;
                            ws.Cell(1, 24 + maxNumberOfReferees + coef).Value = $"{Messages.Player}{k + 1} {Messages.HomeTeam}";
                            ws.Cell(1, 25 + maxNumberOfReferees + coef).Value = $"{Messages.Game} #{k + 1}";
                            ws.Cell(1, 26 + maxNumberOfReferees + coef).Value = $"{Messages.Player}{k + 1} {Messages.GuestTeam}";
                        }
                    }
                    else
                    {
                        ws.Cell(1, tAt + maxNumberOfReferees).Value = $"{Messages.Set} 1";
                        tAt++;
                        ws.Cell(1, tAt + maxNumberOfReferees).Value = $"{Messages.Set} 2";
                        tAt++;
                        ws.Cell(1, tAt + maxNumberOfReferees).Value = $"{Messages.Set} 3";
                        tAt++;
                        ws.Cell(1, tAt + maxNumberOfReferees).Value = $"{Messages.Set} 4";
                        tAt++;
                    }

                    #endregion
                }

                if (selectedColumns != null && selectedColumns.Any())
                {
                    foreach (var column in selectedColumns)
                    {
                        var rowNumber = 2;
                        for (var i = 0; i < games.Count; i++)
                        {
                            isTennisLeague = games[i].IsTennisLeagueGame;
                            switch (column.Value.Key)
                            {
                                case 1:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Number;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].GameId);
                                    break;
                                case 2:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Number;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].LeagueId);
                                    break;
                                case 3:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].League);
                                    break;
                                case 4:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Number;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].Stage);
                                    break;
                                case 5:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].RoundNum);
                                    break;
                                case 6:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.DateTime;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].Date.ToString("d"));
                                    break;
                                case 7:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.DateTime;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].Time);
                                    break;
                                case 8:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                    ws.Cell(rowNumber, column.Key).SetValue(LangHelper.GetDayOfWeek(games[i].Date));
                                    break;
                                case 9:
                                    if (isIndividual)
                                    {
                                        ws.Cell(rowNumber, column.Key).DataType = XLDataType.Number;
                                        ws.Cell(rowNumber, column.Key).SetValue(games[i].HomeCompetitorId);
                                    }
                                    else
                                    {
                                        ws.Cell(rowNumber, column.Key).DataType = XLDataType.Number;
                                        ws.Cell(rowNumber, column.Key).SetValue(games[i].HomeTeamId);
                                    }
                                    break;
                                case 10:
                                    if (isIndividual)
                                    {
                                        ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                        ws.Cell(rowNumber, column.Key).SetValue(games[i].HomeCompetitor);
                                    }
                                    else
                                    {
                                        ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                        ws.Cell(rowNumber, column.Key).SetValue(games[i].HomeTeam);
                                    }
                                    break;
                                case 11:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].HomeTeamTechnicalWinner ? $"{games[i].HomeTeamScore} ({Messages.TechWin})" : $"{games[i].HomeTeamScore}");
                                    break;
                                case 12:
                                    if (isIndividual)
                                    {
                                        ws.Cell(rowNumber, column.Key).DataType = XLDataType.Number;
                                        ws.Cell(rowNumber, column.Key).SetValue(games[i].GuestCompetitorId);
                                    }
                                    else
                                    {
                                        ws.Cell(rowNumber, column.Key).DataType = XLDataType.Number;
                                        ws.Cell(rowNumber, column.Key).SetValue(games[i].GuestTeamId);
                                    }
                                    break;
                                case 13:
                                    if (isIndividual)
                                    {
                                        ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                        ws.Cell(rowNumber, column.Key).SetValue(games[i].GuestCompetitor);
                                    }
                                    else
                                    {
                                        ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                        ws.Cell(rowNumber, column.Key).SetValue(games[i].GuestTeam);
                                    }
                                    break;
                                case 14:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].GuestTeamTechnicalWinner ? $"{games[i].GuestTeam} ({Messages.TechWin})"
                                        : $"{games[i].GuestTeamScore}");
                                    break;
                                case 15:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Number;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].AuditoriumId);
                                    break;
                                case 16:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].Auditorium);
                                    break;
                                case 17:
                                    var refereesNames = games[i].Referees?.Split(',').ToList();
                                    if (column.Value.Value.ToLowerInvariant() == Messages.MainReferee.ToLowerInvariant())
                                    {
                                        if (refereesNames != null && refereesNames.Any())
                                        {
                                            for (int j = 0; j < refereesNames.Count; j++)
                                            {
                                                ws.Cell(rowNumber, column.Key + j).DataType = XLDataType.Text;
                                                ws.Cell(rowNumber, column.Key + j).SetValue(refereesNames[j]);
                                            }
                                        }
                                        ws.Cell(rowNumber, column.Key + maxNumberOfReferees).DataType = XLDataType.Text;
                                        ws.Cell(rowNumber, column.Key + maxNumberOfReferees).SetValue(games[i].RefereeIds);
                                    }
                                    break;
                                case 18:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].SpectatorIds);
                                    break;
                                case 19:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].Spectators);
                                    break;
                                case 20:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].DesksIds);
                                    break;
                                case 21:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].DesksNames);
                                    break;
                                case 22:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Number;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].CycleNumber);
                                    break;
                                case 23:
                                    ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                    ws.Cell(rowNumber, column.Key).SetValue(games[i].Groupe);
                                    break;
                                case 24:
                                    if (column.Value.Value == "Q1" || column.Value.Value == $"{Messages.Set} 1")
                                    {
                                        ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                        ws.Cell(rowNumber, column.Key).SetValue(games[i].Set1);
                                    }
                                    else if (column.Value.Value == "Q2" || column.Value.Value == $"{Messages.Set} 2")
                                    {
                                        ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                        ws.Cell(rowNumber, column.Key).SetValue(games[i].Set2);
                                    }
                                    else if (column.Value.Value == "Q3" || column.Value.Value == $"{Messages.Set} 3")
                                    {
                                        ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                        ws.Cell(rowNumber, column.Key).SetValue(games[i].Set3);
                                    }
                                    else if (column.Value.Value == "Q4" || column.Value.Value == $"{Messages.Set} 4")
                                    {
                                        ws.Cell(rowNumber, column.Key).DataType = XLDataType.Text;
                                        ws.Cell(rowNumber, column.Key).SetValue(games[i].Set4);
                                    }

                                    break;
                            }
                            rowNumber++;
                        }

                    }
                }
                else
                {
                    var rowNumber = 2;

                    IList<ExcelGameDto> gamesSortedByDate = new List<ExcelGameDto>();

                    // ordering the data by date and time before listing them to ws by loop
                    var gamesCurr = games.OrderBy(g => g.Date).ThenBy(g => g.Time).ToList();
                    for (var i = 0; i < gamesCurr.Count; i++)
                    {

                        isTennisLeague = gamesCurr[i].IsTennisLeagueGame;
                        ws.Cell(rowNumber, 1).DataType = XLDataType.Number;
                        ws.Cell(rowNumber, 1).SetValue(gamesCurr[i].GameId);

                        ws.Cell(rowNumber, 2).DataType = XLDataType.Number;
                        ws.Cell(rowNumber, 2).SetValue(gamesCurr[i].LeagueId);

                        ws.Cell(rowNumber, 3).DataType = XLDataType.Text;
                        ws.Cell(rowNumber, 3).SetValue(gamesCurr[i].League);

                        ws.Cell(rowNumber, 4).DataType = XLDataType.Number;
                        ws.Cell(rowNumber, 4).SetValue(gamesCurr[i].Stage);

                        ws.Cell(rowNumber, 5).DataType = XLDataType.Number;
                        ws.Cell(rowNumber, 5).SetValue(gamesCurr[i].RoundNum);

                        ws.Cell(rowNumber, 6).DataType = XLDataType.DateTime;
                        ws.Cell(rowNumber, 6).SetValue(gamesCurr[i].Date.ToString("d"));

                        ws.Cell(rowNumber, 7).DataType = XLDataType.DateTime;
                        ws.Cell(rowNumber, 7).SetValue(gamesCurr[i].Time);

                        ws.Cell(rowNumber, 8).DataType = XLDataType.Text;
                        ws.Cell(rowNumber, 8).SetValue(LangHelper.GetDayOfWeek(gamesCurr[i].Date));

                        if (isIndividual)
                        {
                            ws.Cell(rowNumber, 9).DataType = XLDataType.Number;
                            ws.Cell(rowNumber, 9).SetValue(gamesCurr[i].HomeCompetitorId);

                            ws.Cell(rowNumber, 10).DataType = XLDataType.Text;
                            ws.Cell(rowNumber, 10).SetValue(gamesCurr[i].HomeCompetitor);

                            ws.Cell(rowNumber, 11).DataType = XLDataType.Number;
                            ws.Cell(rowNumber, 11).SetValue(gamesCurr[i].HomeTeamScore);

                            ws.Cell(rowNumber, 12).DataType = XLDataType.Number;
                            ws.Cell(rowNumber, 12).SetValue(gamesCurr[i].GuestCompetitorId);

                            ws.Cell(rowNumber, 13).DataType = XLDataType.Text;
                            ws.Cell(rowNumber, 13).SetValue(gamesCurr[i].GuestCompetitor);

                            ws.Cell(rowNumber, 14).DataType = XLDataType.Number;
                            ws.Cell(rowNumber, 14).SetValue(gamesCurr[i].GuestTeamScore);
                        }
                        else
                        {
                            ws.Cell(rowNumber, 9).DataType = XLDataType.Number;
                            ws.Cell(rowNumber, 9).SetValue(gamesCurr[i].HomeTeamId);

                            ws.Cell(rowNumber, 10).DataType = XLDataType.Text;
                            ws.Cell(rowNumber, 10).SetValue(gamesCurr[i].HomeTeam);

                            ws.Cell(rowNumber, 11).DataType = XLDataType.Text;
                            ws.Cell(rowNumber, 11).SetValue(gamesCurr[i].HomeTeamTechnicalWinner ? $"{gamesCurr[i].HomeTeamScore} ({Messages.TechWin})"
                                        : $"{gamesCurr[i].HomeTeamScore}");

                            ws.Cell(rowNumber, 12).DataType = XLDataType.Number;
                            ws.Cell(rowNumber, 12).SetValue(gamesCurr[i].GuestTeamId);

                            ws.Cell(rowNumber, 13).DataType = XLDataType.Text;
                            ws.Cell(rowNumber, 13).SetValue(gamesCurr[i].GuestTeam);

                            ws.Cell(rowNumber, 14).DataType = XLDataType.Text;
                            ws.Cell(rowNumber, 14).SetValue(gamesCurr[i].GuestTeamTechnicalWinner ? $"{gamesCurr[i].GuestTeamScore} ({Messages.TechWin})"
                                        : $"{gamesCurr[i].GuestTeamScore}");
                        }


                        ws.Cell(rowNumber, 15).DataType = XLDataType.Number;
                        ws.Cell(rowNumber, 15).SetValue(gamesCurr[i].AuditoriumId);

                        ws.Cell(rowNumber, 16).DataType = XLDataType.Text;
                        ws.Cell(rowNumber, 16).SetValue(gamesCurr[i].Auditorium);



                        int at = 17;
                        if (section != GamesAlias.NetBall || (LogicaName)logicalName != LogicaName.Team)
                        {
                            var refereesNames = gamesCurr[i].Referees?.Split(',').ToList();
                            if (refereesNames != null)
                            {
                                for (int j = 17; j < (17 + maxNumberOfReferees); j++)
                                {
                                    string refereeName = "";
                                    if (j - 17 < refereesNames.Count)
                                    {
                                        refereeName = String.IsNullOrEmpty(refereesNames[j - 17]) ? "" : refereesNames[j - 17];
                                        ws.Cell(rowNumber, j).DataType = XLDataType.Text;
                                        ws.Cell(rowNumber, j).SetValue(refereeName);
                                    }
                                }
                            }

                            ws.Cell(rowNumber, at + maxNumberOfReferees).DataType = XLDataType.Text;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).SetValue(gamesCurr[i].RefereeIds);
                            at++;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).DataType = XLDataType.Text;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).SetValue(gamesCurr[i].SpectatorIds);
                            at++;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).DataType = XLDataType.Text;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).SetValue(gamesCurr[i].Spectators);
                            at++;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).DataType = XLDataType.Text;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).SetValue(gamesCurr[i].DesksIds);
                            at++;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).DataType = XLDataType.Text;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).SetValue(gamesCurr[i].DesksNames);
                            at++;
                        }
                        ws.Cell(rowNumber, at + maxNumberOfReferees).DataType = XLDataType.Number;
                        ws.Cell(rowNumber, at + maxNumberOfReferees).SetValue(gamesCurr[i].CycleNumber);
                        at++;
                        ws.Cell(rowNumber, at + maxNumberOfReferees).DataType = XLDataType.Text;
                        ws.Cell(rowNumber, at + maxNumberOfReferees).SetValue(gamesCurr[i].Groupe);
                        at++;
                        if (section.Equals(GamesAlias.Tennis))
                        {
                            var tennisGames = gamesCurr[i].TennisLeagueGames;
                            foreach (var tennisGame in tennisGames.Select((value, index) => new { value, index }))
                            {
                                var coef = tennisGame.index * 3;
                                ws.Cell(rowNumber, 24 + maxNumberOfReferees + coef).DataType = XLDataType.Text;
                                if (!tennisGame.value.HomePairPlayerId.HasValue)
                                    ws.Cell(rowNumber, 24 + maxNumberOfReferees + coef).SetValue(tennisGame?.value?.GuestPlayer?.FullName);
                                else
                                {
                                    var playerString = !tennisGame.value.GuestPairPlayerId.HasValue
                                        ? tennisGame.value.GuestPlayer?.FullName
                                        : $"{tennisGame?.value?.GuestPlayer?.FullName} / {tennisGame?.value?.GuestPairPlayer?.FullName}";
                                    ws.Cell(rowNumber, 24 + maxNumberOfReferees + coef).SetValue(playerString);

                                }

                                ws.Cell(rowNumber, 25 + maxNumberOfReferees + coef).DataType = XLDataType.Text;
                                ws.Cell(rowNumber, 25 + maxNumberOfReferees + coef).SetValue(GetGameScoresString(tennisGame.value.TennisLeagueGameScores?.ToList()));

                                if (!tennisGame.value.HomePairPlayerId.HasValue)
                                    ws.Cell(rowNumber, 26 + maxNumberOfReferees + coef).SetValue(tennisGame.value.HomePlayer?.FullName);
                                else
                                {
                                    var playerString = !tennisGame.value.HomePairPlayerId.HasValue
                                            ? tennisGame.value.HomePlayer?.FullName
                                            : $"{tennisGame?.value?.HomePlayer?.FullName} / {tennisGame?.value?.HomePairPlayer?.FullName}";
                                    ws.Cell(rowNumber, 26 + maxNumberOfReferees + coef).SetValue(playerString);
                                }
                            }
                        }
                        else
                        {
                            ws.Cell(rowNumber, at + maxNumberOfReferees).DataType = XLDataType.Text;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).SetValue(gamesCurr[i].Set1);
                            at++;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).DataType = XLDataType.Text;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).SetValue(gamesCurr[i].Set2);
                            at++;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).DataType = XLDataType.Text;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).SetValue(gamesCurr[i].Set3);
                            at++;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).DataType = XLDataType.Text;
                            ws.Cell(rowNumber, at + maxNumberOfReferees).SetValue(gamesCurr[i].Set4);
                        }
                        rowNumber++;
                    }
                }
                ws.Columns().AdjustToContents();

                foreach (var cell in ws.CellsUsed(true))
                {
                    cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                }

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                League league = leagueRepo.GetById(games.FirstOrDefault().LeagueId);
                string unionName = null;
                if (league != null && league.UnionId.HasValue)
                {
                    unionName = unionsRepo.GetById(league.UnionId.Value).Name;
                }
                //return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", isTennisLeague ? $"{unionName}_{leagueName}_{Messages.GameScheduleCapture}_{DateTime.Now.ToString("yyyy-MM-dd mm-hh")}.xlsx" : "ExportGamesList-" + DateTime.Now.ToLongDateString() + ".xlsx");
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", leagueName != null ? $"{unionName}_{leagueName}_{Messages.GameScheduleCapture}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm")}.xlsx" : $"{unionName}_{Messages.GameScheduleCapture}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm")}.xlsx");
            }
        }

        private string GetGameScoresString(List<TennisLeagueGameScore> tennisLeagueGameScores)
        {
            var tennisLeagueOrdered = tennisLeagueGameScores.OrderByDescending(r => r.Id).ToList();
            var builder = new StringBuilder();
            foreach (var score in tennisLeagueOrdered)
            {
                if (!(score.HomeScore == 0 && score.GuestScore == 0))
                {
                    builder.Append($"{score.GuestScore} - {score.HomeScore}");
                    if (!tennisLeagueOrdered.IsLast(score)) builder.Append(", ");
                }
            }
            return builder.ToString();
        }

        private int GetMaxCountOfTheTennisGames(IList<ExcelGameDto> games)
        {
            var count = 0;

            for (int i = 0; i < games.Count; i++)
            {
                if (games[i].TennisLeagueGames?.Any() == true)
                {
                    var currentGamesCount = games[i].TennisLeagueGames.Count;
                    if (i == 0)
                        count = currentGamesCount;
                    else
                        count = currentGamesCount > count ? currentGamesCount : count;
                }
            }

            return count;
        }

        private Dictionary<int, KeyValuePair<int, string>> GetAllSelectedColumnsDictionary(IEnumerable<int> columns, bool isIndividual, bool isBasketBallOrWaterPolo, int maxNumberOfReferees,
            int? maxCountOfTennisGames = 0)
        {
            var selectedColumns = new Dictionary<int, KeyValuePair<int, string>>();

            bool isRefereeColumnSelected = columns.Contains(16);

            if (columns != null && columns.Any())
            {
                var listOfColumns = columns.ToList();
                for (int i = 1; i <= listOfColumns.Count; i++)
                {
                    switch (listOfColumns[i - 1])
                    {
                        case 1:
                            selectedColumns.Add(i, new KeyValuePair<int, string>(1, Messages.GameId));
                            break;
                        case 2:
                            selectedColumns.Add(i, new KeyValuePair<int, string>(2, Messages.LeagueId));
                            break;
                        case 3:
                            selectedColumns.Add(i, new KeyValuePair<int, string>(3, Messages.League));
                            break;
                        case 4:
                            selectedColumns.Add(i, new KeyValuePair<int, string>(4, Messages.Stage));
                            break;
                        case 5:
                            selectedColumns.Add(i, new KeyValuePair<int, string>(5, Messages.Round));
                            break;
                        case 6:
                            selectedColumns.Add(i, new KeyValuePair<int, string>(6, Messages.Date));
                            break;
                        case 7:
                            selectedColumns.Add(i, new KeyValuePair<int, string>(7, Messages.Time));
                            break;
                        case 8:
                            selectedColumns.Add(i, new KeyValuePair<int, string>(8, Messages.Day));
                            break;
                        case 9:
                            selectedColumns.Add(i, !isIndividual ? new KeyValuePair<int, string>(9, $"{Messages.HomeTeam} {Messages.Id}")
                                : new KeyValuePair<int, string>(9, $"{Messages.HomeCompetitor} {Messages.Id}"));
                            break;
                        case 10:
                            selectedColumns.Add(i, !isIndividual ? new KeyValuePair<int, string>(10, $"{Messages.HomeTeam}")
                                : new KeyValuePair<int, string>(10, $"{Messages.HomeCompetitor}"));
                            break;
                        case 11:
                            selectedColumns.Add(i, !isIndividual ? new KeyValuePair<int, string>(11, $"{Messages.HomeTeam} {Messages.Score.ToLowerInvariant()}")
                                : new KeyValuePair<int, string>(11, $"{Messages.HomeCompetitor} {Messages.Score.ToLowerInvariant()}"));
                            break;
                        case 12:
                            selectedColumns.Add(i, !isIndividual ? new KeyValuePair<int, string>(12, $"{Messages.GuestTeam} {Messages.Id}")
                                : new KeyValuePair<int, string>(12, $"{Messages.GuestCompetitor} {Messages.Id}"));
                            break;
                        case 13:
                            selectedColumns.Add(i, !isIndividual ? new KeyValuePair<int, string>(13, $"{Messages.GuestTeam}")
                                : new KeyValuePair<int, string>(13, $"{Messages.GuestCompetitor}"));
                            break;
                        case 14:
                            selectedColumns.Add(i, !isIndividual ? new KeyValuePair<int, string>(14, $"{Messages.GuestTeam} {Messages.Score.ToLowerInvariant()}")
                                : new KeyValuePair<int, string>(1, $"{Messages.GuestCompetitor} {Messages.Score.ToLowerInvariant()}"));
                            break;
                        case 15:
                            selectedColumns.Add(i, new KeyValuePair<int, string>(15, $"{Messages.Auditorium} {Messages.Id}"));
                            break;
                        case 16:
                            selectedColumns.Add(i, new KeyValuePair<int, string>(16, Messages.Auditorium));
                            break;
                        case 17:
                            if (isRefereeColumnSelected)
                            {
                                for (int j = i; j < (i + maxNumberOfReferees); j++)
                                {
                                    selectedColumns.Add(j, (j == i) ? new KeyValuePair<int, string>(17, Messages.MainReferee) :
                                        new KeyValuePair<int, string>(0, $"{Messages.Referee} #{j - i + 1}"));
                                }
                                selectedColumns.Add(isRefereeColumnSelected ? i + maxNumberOfReferees : i, new KeyValuePair<int, string>(17, Messages.RefereesIds));
                            }
                            break;
                        case 18:
                            selectedColumns.Add(isRefereeColumnSelected ? i + maxNumberOfReferees : i, new KeyValuePair<int, string>(18, Messages.SpectatorsIds));
                            break;
                        case 19:
                            selectedColumns.Add(isRefereeColumnSelected ? i + maxNumberOfReferees : i, new KeyValuePair<int, string>(19, Messages.Spectators));
                            break;
                        case 20:
                            selectedColumns.Add(isRefereeColumnSelected ? i + maxNumberOfReferees : i, new KeyValuePair<int, string>(20, Messages.DesksIds));
                            break;
                        case 21:
                            selectedColumns.Add(isRefereeColumnSelected ? i + maxNumberOfReferees : i, new KeyValuePair<int, string>(21, Messages.DesksNames));
                            break;
                        case 22:
                            selectedColumns.Add(isRefereeColumnSelected ? i + maxNumberOfReferees : i,
                                new KeyValuePair<int, string>(22, $"{Messages.Cycle} {Messages.Number.ToLowerInvariant()}"));
                            break;
                        case 23:
                            selectedColumns.Add(isRefereeColumnSelected ? i + maxNumberOfReferees : i, new KeyValuePair<int, string>(23, Messages.Group));
                            break;
                        case 24:
                            if (isBasketBallOrWaterPolo)
                            {
                                selectedColumns.Add(isRefereeColumnSelected ? i + maxNumberOfReferees : i, new KeyValuePair<int, string>(24, "Q1"));
                                selectedColumns.Add(isRefereeColumnSelected ? i + 1 + maxNumberOfReferees : i + 1, new KeyValuePair<int, string>(24, "Q2"));
                                selectedColumns.Add(isRefereeColumnSelected ? i + 2 + maxNumberOfReferees : i + 2, new KeyValuePair<int, string>(24, "Q3"));
                                selectedColumns.Add(isRefereeColumnSelected ? i + 3 + maxNumberOfReferees : i + 3, new KeyValuePair<int, string>(24, "Q4"));
                            }
                            else if (maxCountOfTennisGames.HasValue)
                            {
                                for (int k = 1; k <= maxCountOfTennisGames.Value; k++)
                                {
                                    selectedColumns.Add(isRefereeColumnSelected ? i + maxNumberOfReferees : i, new KeyValuePair<int, string>
                                        (24, $"{Messages.Player}{k} {Messages.HomeTeam}"));
                                    selectedColumns.Add(isRefereeColumnSelected ? i + maxNumberOfReferees : i, new KeyValuePair<int, string>
                                        (24, $"{Messages.Game} #{k}"));
                                    selectedColumns.Add(isRefereeColumnSelected ? i + maxNumberOfReferees : i, new KeyValuePair<int, string>
                                        (24, $"{Messages.Player}{k} {Messages.GuestTeam}"));
                                }
                            }
                            else
                            {
                                selectedColumns.Add(isRefereeColumnSelected ? i + maxNumberOfReferees : i, new KeyValuePair<int, string>(24, $"{Messages.Set} 1"));
                                selectedColumns.Add(isRefereeColumnSelected ? i + 1 + maxNumberOfReferees : i + 1, new KeyValuePair<int, string>(24, $"{Messages.Set} 2"));
                                selectedColumns.Add(isRefereeColumnSelected ? i + 2 + maxNumberOfReferees : i + 2, new KeyValuePair<int, string>(24, $"{Messages.Set} 3"));
                                selectedColumns.Add(isRefereeColumnSelected ? i + 3 + maxNumberOfReferees : i + 3, new KeyValuePair<int, string>(24, $"{Messages.Set} 4"));
                            }

                            break;
                    }
                }
            }
            return selectedColumns;
        }

        public ActionResult Create(int id, int seasonId, int? departmentId = null)
        {
            var serv = new SchedulingService(db);
            serv.ScheduleGames(id);

            CrossesStageHelper.SetNameForCrossesStages(stagesRepo.GetAll(id));

            ViewBag.IsDepartmentLeague = departmentId.HasValue ? true : false;
            ViewBag.DepartmentId = departmentId;
            if (departmentId.HasValue)
            {
                return RedirectToAction("List", new { id = id, desOrder = false, inpSeasonId = seasonId, departmentId = departmentId });
            }
            else
            {
                return RedirectToAction("List", new { id = id, desOrder = false, inpSeasonId = seasonId });
            }
        }

        public ActionResult CreateTennisSchedule(int categoryId, int seasonId, int? departmentId = null)
        {
            var serv = new SchedulingService(db);
            var isSettingsAvailable = serv.ScheduleTennisGames(categoryId, seasonId);
            ViewBag.IsDepartmentLeague = departmentId.HasValue ? true : false;
            ViewBag.DepartmentId = departmentId;
            return Json(new { HadSettingsAvailable = isSettingsAvailable });
        }

        public ActionResult Update(GameCycleForm frm)
        {
            try
            {
                bool isChanged = false;
                var gc = gamesRepo.GetGameCycleById(frm.CycleId);
                var sectionAlias = gamesRepo.GetSectionAlias(gc);
                if (gc.AuditoriumId != frm.AuditoriumId)
                {
                    isChanged = true;
                    gc.AuditoriumId = frm.AuditoriumId;
                }
                if (!gc.StartDate.TrimSeconds().Equals(frm.StartDate.TrimSeconds())
                    && gc.IsPublished)
                {
                    isChanged = true;
                    gc.StartDate = frm.StartDate;
                    gc.IsDateUpdated = true;
                }
                else if (!gc.StartDate.TrimSeconds().Equals(frm.StartDate.TrimSeconds())
                    && !gc.IsPublished)
                {
                    isChanged = true;
                    gc.StartDate = frm.StartDate;
                }
                gc.IsNotSetYet = frm.IsNotSetYet;
                if (!sectionAlias.Equals(SectionAliases.Netball))
                {
                    var spectatorIds = !String.IsNullOrEmpty(gc.SpectatorIds) ? gc.SpectatorIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
                    if (!spectatorIds.SequenceEqual(frm.SpectatorIds ?? new List<string>()))
                    {
                        isChanged = true;
                        gc.SpectatorIds = String.Join(",", frm.SpectatorIds?.ToArray() ?? new string[] { });
                    }
                }
                if (sectionAlias.Equals(SectionAliases.Netball))
                {
                    var refereesIds = !String.IsNullOrEmpty(gc.RefereeIds) ? gc.RefereeIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
                    if (!refereesIds.SequenceEqual(frm.RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>()))
                    {
                        isChanged = true;
                        gc.RefereeIds = frm.RefereeIds;
                    }
                }
                //  Teams could be replaced only before game starts
                if (gc.GameStatus != GameStatus.Started && gc.GameStatus != GameStatus.Ended && gc.Group.TypeId == 1)
                {
                    if (frm.HomeTeamId == null && frm.GuestTeamId == null)
                    {
                        gc.HomeAthleteId = frm.HomeAthleteId;
                        gc.GuestAthleteId = frm.GuestAthleteId;
                    }
                    else
                    {
                        gc.HomeTeamId = frm.HomeTeamId;
                        gc.GuestTeamId = frm.GuestTeamId;
                    }

                }

                if (!string.Equals(frm.Remark, gc.Remark))
                {
                    gc.Remark = frm.Remark;
                    isChanged = true;
                }
                gamesRepo.Update(gc);

                if (isChanged && gc.IsPublished)
                {
                    NotesMessagesRepo notesRep = new NotesMessagesRepo();
                    if (gc.Stage != null && gc.Stage.League != null && gc.Stage.League.SeasonId != null)
                    {
                        String message = String.Format("פרטי המשחק עודכנו: {0} vs {1}", gc.HomeTeam != null ? gc.HomeTeam.Title : "", gc.GuestTeam != null ? gc.GuestTeam.Title : "");

                        if (gc.HomeTeamId != null)
                        {
                            notesRep.SendToTeam((int)gc.Stage.League.SeasonId, (int)gc.HomeTeamId, message, true);
                        }
                        if (gc.GuestTeamId != null)
                        {
                            notesRep.SendToTeam((int)gc.Stage.League.SeasonId, (int)gc.GuestTeamId, message, true);
                        }
                        //if (!string.IsNullOrEmpty(gc.RefereeIds))
                        //{
                        //    var ids = gc.RefereeIds.Split(',').Select(int.Parse).ToList();
                        //    notesRep.SendToUsers(ids, message);
                        //}
                    }

                    var notsServ = new GamesNotificationsService();
                    notsServ.SendPushToDevices(GlobVars.IsTest);
                }
            }
            catch (System.Exception e)
            {
                return Json(new { stat = "error", message = e.ToString() });
            }
            return Json(new { stat = "ok", id = frm.CycleId });
        }

        public ActionResult UpdateTennis(TennisGameCycleForm model)
        {
            try
            {
                bool isChanged = false;
                var gc = gamesRepo.GetTennisGameCycleById(model.CycleId);
                if (gc.FieldId != model.AuditoriumId)
                {
                    isChanged = true;
                    gc.FieldId = model.AuditoriumId;
                }
                if (model.StartDate.Ticks != 0)
                {
                    if (!gc.StartDate.TrimSeconds().Equals(model.StartDate.TrimSeconds())
                        && gc.IsPublished)
                    {
                        isChanged = true;
                        gc.StartDate = model.StartDate;
                        gc.IsDateUpdated = true;
                    }
                    else if (!gc.StartDate.TrimSeconds().Equals(model.StartDate.TrimSeconds())
                        && !gc.IsPublished)
                    {
                        isChanged = true;
                        gc.StartDate = model.StartDate;
                    }
                }
                if (!model.IsCathcball)
                {
                    var spectatorIds = !String.IsNullOrEmpty(gc.SpectatorIds) ? gc.SpectatorIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
                    if (!spectatorIds.SequenceEqual(model.SpectatorIds ?? new List<string>()))
                    {
                        isChanged = true;
                        gc.SpectatorIds = String.Join(",", model.SpectatorIds.ToArray());
                    }
                }
                gc.IsNotSetYet = model.IsNotSetYet;
                var refereesIds = !String.IsNullOrEmpty(gc.RefereeIds) ? gc.RefereeIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
                if (!refereesIds.SequenceEqual(model.RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>()))
                {
                    isChanged = true;
                    gc.RefereeIds = model.RefereeIds;
                }
                if (model.FirstScoreOne.HasValue)
                    gc.TennisGameSets.ElementAt(0).FirstPlayerScore = model.FirstScoreOne.Value;
                if (model.SecondScoreOne.HasValue)
                    gc.TennisGameSets.ElementAt(1).FirstPlayerScore = model.SecondScoreOne.Value;
                if (model.ThirdScoreOne.HasValue)
                    gc.TennisGameSets.ElementAt(2).FirstPlayerScore = model.ThirdScoreOne.Value;
                if (model.ForthScoreOne.HasValue)
                    gc.TennisGameSets.ElementAt(3).FirstPlayerScore = model.ForthScoreOne.Value;
                if (model.FifthScoreOne.HasValue)
                    gc.TennisGameSets.ElementAt(4).FirstPlayerScore = model.FifthScoreOne.Value;

                if (model.FirstScoreTwo.HasValue)
                    gc.TennisGameSets.ElementAt(0).SecondPlayerScore = model.FirstScoreTwo.Value;
                if (model.SecondScoreTwo.HasValue)
                    gc.TennisGameSets.ElementAt(1).SecondPlayerScore = model.SecondScoreTwo.Value;
                if (model.ThirdScoreTwo.HasValue)
                    gc.TennisGameSets.ElementAt(2).SecondPlayerScore = model.ThirdScoreTwo.Value;
                if (model.ForthScoreTwo.HasValue)
                    gc.TennisGameSets.ElementAt(3).SecondPlayerScore = model.ForthScoreTwo.Value;
                if (model.FifthScoreTwo.HasValue)
                    gc.TennisGameSets.ElementAt(4).SecondPlayerScore = model.FifthScoreTwo.Value;

                int firstPlayerScore = 0;
                int secondPlayerScore = 0;
                for (int i = 0; i < 5; i++)
                {
                    if (gc.TennisGameSets.ElementAt(i).FirstPlayerScore != 0 || gc.TennisGameSets.ElementAt(i).SecondPlayerScore != 0)
                    {
                        if (gc.TennisGameSets.ElementAt(i).FirstPlayerScore > gc.TennisGameSets.ElementAt(i).SecondPlayerScore)
                        {
                            firstPlayerScore++;
                        }
                        else if (gc.TennisGameSets.ElementAt(i).SecondPlayerScore > gc.TennisGameSets.ElementAt(i).FirstPlayerScore)
                        {
                            secondPlayerScore++;
                        }
                    }
                }

                if (gc.TennisGroup.TypeId == GameTypeId.Playoff || gc.TennisGroup.TypeId == GameTypeId.Knockout || gc.TennisGroup.TypeId == GameTypeId.Knockout34 || gc.TennisGroup.TypeId == GameTypeId.Knockout34Consolences1Round || gc.TennisGroup.TypeId == GameTypeId.Knockout34ConsolencesQuarterRound)
                {
                    if (firstPlayerScore > secondPlayerScore)
                    {
                        gc.TennisPlayoffBracket.WinnerId = gc.FirstPlayerId;
                        gc.TennisPlayoffBracket.WinnerPlayerPairId = gc.FirstPlayerPairId;
                        gc.TennisPlayoffBracket.LoserId = gc.SecondPlayerId;
                        gc.TennisPlayoffBracket.LoserPlayerPairId = gc.SecondPlayerPairId;
                    }
                    else if (firstPlayerScore < secondPlayerScore)
                    {
                        gc.TennisPlayoffBracket.WinnerId = gc.SecondPlayerId;
                        gc.TennisPlayoffBracket.WinnerPlayerPairId = gc.SecondPlayerPairId;
                        gc.TennisPlayoffBracket.LoserId = gc.FirstPlayerId;
                        gc.TennisPlayoffBracket.LoserPlayerPairId = gc.FirstPlayerPairId;
                    }
                }

                gc.FirstPlayerScore = firstPlayerScore;
                gc.SecondPlayerScore = secondPlayerScore;

                gc.TimeInitial = model.TimeInitial;

                //  Teams could be replaced only before game starts
                /*if (gc.GameStatus != GameStatus.Started && gc.GameStatus != GameStatus.Ended && gc.TennisGroup.TypeId == 1)
                {
                    gc.FirstPlayerId = frm.FirstPlayerId;
                    gc.SecondPlayerId = frm.SecondPlayerId;
                }*/
                gamesRepo.UpdateTennis(gc);

                if (isChanged && gc.IsPublished)
                {
                    NotesMessagesRepo notesRep = new NotesMessagesRepo();
                    if (gc.TennisStage != null && gc.TennisStage.Team != null)
                    {
                        String message = String.Format("פרטי המשחק עודכנו: {0} vs {1}", gc.TeamsPlayer != null ? gc.TeamsPlayer.User.FullName : "", gc.TeamsPlayer1 != null ? gc.TeamsPlayer1.User.FullName : "");

                        if (gc.FirstPlayerId != null && gc.FirstPlayerId > 0)
                        {
                            notesRep.SendToPlayer((int)gc.TennisStage.SeasonId, (int)gc.FirstPlayer.UserId, message, true);
                        }
                        if (gc.SecondPlayerId != null && gc.SecondPlayerId > 0)
                        {
                            notesRep.SendToPlayer((int)gc.TennisStage.SeasonId, (int)gc.SecondPlayer.UserId, message, true);
                        }
                    }

                    var notsServ = new GamesNotificationsService();
                    notsServ.SendPushToDevices(GlobVars.IsTest);
                }
            }
            catch (System.Exception e)
            {
                return Json(new { stat = "error", message = e.ToString() });
            }
            return Json(new { stat = "ok", id = model.CycleId });
        }

        public ActionResult Groups(int id, int seasonId, int? departmentId = null)
        {
            ViewBag.LeagueId = id;
            var game = gamesRepo.GetByLeague(id);
            if (game != null)
            {
                ViewBag.GameId = game.GameId;
            }
            else
            {
                ViewBag.GameId = 0;
            }
            ViewBag.SeasonId = seasonId;

            ViewBag.IsDepartmentLeague = departmentId.HasValue ? true : false;
            ViewBag.DepartmentId = departmentId;

            return PartialView("_Groups");
        }

        public ActionResult TennisGroups(int leagueId, int categoryId, int seasonId, int? departmentId = null)
        {
            ViewBag.LeagueId = leagueId;
            var game = gamesRepo.GetByLeague(leagueId);
            if (game != null)
            {
                ViewBag.GameId = game.GameId;
            }
            else
            {
                ViewBag.GameId = 0;
            }
            ViewBag.SeasonId = seasonId;
            ViewBag.CategoryId = categoryId;

            ViewBag.IsDepartmentLeague = departmentId.HasValue ? true : false;
            ViewBag.DepartmentId = departmentId;

            return PartialView("_TennisGroups");
        }

        public ActionResult Toggle(int id, bool isChronological = false, bool isIndividual = false)
        {
            int leagueId = isIndividual ? gamesRepo.ToogleAthletes(id) : gamesRepo.ToggleTeams(id);

            return RedirectToAction("List", new { id = leagueId, desOrder = Session["desOrder"], isChronological = isChronological });
        }

        public ActionResult Delete(int id, bool isChronological = false)
        {
            var gc = gamesRepo.GetGameCycleById(id);
            int leagueId = gc.Stage.LeagueId;

            gamesRepo.RemoveCycle(gc);
            gamesRepo.Save();

            return RedirectToAction("List", new { id = leagueId, desOrder = Session["desOrder"], isChronological = isChronological });
        }

        public ActionResult DeleteTennis(int id, bool isChronological = false)
        {
            var gc = gamesRepo.GetTennisGameCycleById(id);

            int seasonId = gc.TennisStage.SeasonId;
            int categoryId = gc.TennisStage.CategoryId;

            gamesRepo.RemoveTennisCycle(gc);
            gamesRepo.Save();

            return RedirectToAction("TennisList", new { categoryId = categoryId, desOrder = false, inpSeasonId = seasonId });
        }

        private void ResetGameResults(TennisPlayoffBracket bracket, bool isTopParent = true)
        {
            if (bracket != null)
            {

                foreach (var gc in bracket.TennisGameCycles)
                {
                    gc.GameStatus = null;
                    for (int i = 0; i < 5; i++)
                    {
                        gc.TennisGameSets.ElementAt(i).FirstPlayerScore = 0;
                        gc.TennisGameSets.ElementAt(i).SecondPlayerScore = 0;
                    }
                    gc.FirstPlayerScore = 0;
                    gc.SecondPlayerScore = 0;
                    if (!isTopParent)
                    {
                        gc.FirstPlayerId = null;
                        gc.FirstPlayerPairId = null;
                        gc.SecondPlayerId = null;
                        gc.SecondPlayerPairId = null;
                    }
                }
                if (!isTopParent)
                {
                    bracket.FirstPlayerId = null;
                    bracket.FirstPlayerPairId = null;
                    bracket.SecondPlayerId = null;
                    bracket.SecondPlayerPairId = null;
                }
                bracket.WinnerId = null;
                bracket.LoserId = null;
                var bracketsConnectedToRoot = db.TennisPlayoffBrackets.Where(b => b.ParentBracket1Id == bracket.Id || b.ParentBracket2Id == bracket.Id);
                foreach (var childBracket in bracketsConnectedToRoot)
                {
                    ResetGameResults(childBracket, false);
                }
                if (isTopParent)
                {
                    gamesRepo.Save();
                }
            }
        }

        [HttpPost]
        public JsonResult SetTennis(TennisGameCycleForm frm)
        {
            var gc = gamesRepo.GetTennisGameCycleById(frm.CycleId);
            if (gc.GameStatus == GameStatus.Ended)
            {
                if( gc.TennisPlayoffBracket == null)
                {
                    gc.GameStatus = null;
                    for (int i = 0; i < 5; i++)
                    {
                        gc.TennisGameSets.ElementAt(i).FirstPlayerScore = 0;
                        gc.TennisGameSets.ElementAt(i).SecondPlayerScore = 0;
                    }
                    gc.FirstPlayerScore = 0;
                    gc.SecondPlayerScore = 0;
                }
                else
                    ResetGameResults(gc.TennisPlayoffBracket);
            }
            else
            {
                gc.GameStatus = GameStatus.Ended;

                bool isChanged = false;
                if (gc.FieldId != frm.AuditoriumId)
                {
                    isChanged = true;
                    gc.FieldId = frm.AuditoriumId;
                }
                if (!gc.StartDate.TrimSeconds().Equals(frm.StartDate.TrimSeconds()) && !DateTime.MinValue.Equals(frm.StartDate.TrimSeconds()) && gc.IsPublished)
                {
                    isChanged = true;
                    gc.StartDate = frm.StartDate;
                    gc.IsDateUpdated = true;
                }
                else if (!gc.StartDate.TrimSeconds().Equals(frm.StartDate.TrimSeconds()) && !DateTime.MinValue.Equals(frm.StartDate.TrimSeconds()) && !gc.IsPublished)
                {
                    isChanged = true;
                    gc.StartDate = frm.StartDate;
                }
                if (!frm.IsCathcball)
                {
                    var spectatorIds = !String.IsNullOrEmpty(gc.SpectatorIds) ? gc.SpectatorIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
                    if (!spectatorIds.SequenceEqual(frm.SpectatorIds ?? new List<string>()))
                    {
                        isChanged = true;
                        gc.SpectatorIds = String.Join(",", frm.SpectatorIds.ToArray());
                    }
                }

                gc.TennisGameSets.ElementAt(0).FirstPlayerScore = frm.FirstScoreOne ?? 0;
                gc.TennisGameSets.ElementAt(1).FirstPlayerScore = frm.SecondScoreOne ?? 0;
                gc.TennisGameSets.ElementAt(2).FirstPlayerScore = frm.ThirdScoreOne ?? 0;
                gc.TennisGameSets.ElementAt(3).FirstPlayerScore = frm.ForthScoreOne ?? 0;
                gc.TennisGameSets.ElementAt(4).FirstPlayerScore = frm.FifthScoreOne ?? 0;

                gc.TennisGameSets.ElementAt(0).SecondPlayerScore = frm.FirstScoreTwo ?? 0;
                gc.TennisGameSets.ElementAt(1).SecondPlayerScore = frm.SecondScoreTwo ?? 0;
                gc.TennisGameSets.ElementAt(2).SecondPlayerScore = frm.ThirdScoreTwo ?? 0;
                gc.TennisGameSets.ElementAt(3).SecondPlayerScore = frm.ForthScoreTwo ?? 0;
                gc.TennisGameSets.ElementAt(4).SecondPlayerScore = frm.FifthScoreTwo ?? 0;

                int firstPlayerScore = 0;
                int secondPlayerScore = 0;
                for (int i = 0; i < 5; i++)
                {
                    if (gc.TennisGameSets.ElementAt(i).FirstPlayerScore != 0 || gc.TennisGameSets.ElementAt(i).SecondPlayerScore != 0)
                    {
                        if (gc.TennisGameSets.ElementAt(i).FirstPlayerScore > gc.TennisGameSets.ElementAt(i).SecondPlayerScore)
                        {
                            firstPlayerScore++;
                        }
                        if (gc.TennisGameSets.ElementAt(i).SecondPlayerScore > gc.TennisGameSets.ElementAt(i).FirstPlayerScore)
                        {
                            secondPlayerScore++;
                        }
                    }
                }

                if (gc.TennisGroup.TypeId == GameTypeId.Playoff || gc.TennisGroup.TypeId == GameTypeId.Knockout || gc.TennisGroup.TypeId == GameTypeId.Knockout34 || gc.TennisGroup.TypeId == GameTypeId.Knockout34Consolences1Round || gc.TennisGroup.TypeId == GameTypeId.Knockout34ConsolencesQuarterRound)
                {
                    if (firstPlayerScore > secondPlayerScore)
                    {
                        gc.TennisPlayoffBracket.WinnerId = gc.FirstPlayerId;
                        gc.TennisPlayoffBracket.WinnerPlayerPairId = gc.FirstPlayerPairId;
                        gc.TennisPlayoffBracket.LoserId = gc.SecondPlayerId;
                        gc.TennisPlayoffBracket.LoserPlayerPairId = gc.SecondPlayerPairId;
                    }
                    else if (firstPlayerScore < secondPlayerScore)
                    {
                        gc.TennisPlayoffBracket.WinnerId = gc.SecondPlayerId;
                        gc.TennisPlayoffBracket.WinnerPlayerPairId = gc.SecondPlayerPairId;
                        gc.TennisPlayoffBracket.LoserId = gc.FirstPlayerId;
                        gc.TennisPlayoffBracket.LoserPlayerPairId = gc.FirstPlayerPairId;
                    }
                }

                gc.FirstPlayerScore = firstPlayerScore;
                gc.SecondPlayerScore = secondPlayerScore;

                //  Teams could be replaced only before game starts
                /*if (gc.GameStatus != GameStatus.Started && gc.GameStatus != GameStatus.Ended && gc.TennisGroup.TypeId == 1)
                {
                    gc.FirstPlayerId = frm.FirstPlayerId;
                    gc.SecondPlayerId = frm.SecondPlayerId;
                }*/
                gamesRepo.UpdateTennis(gc);

                if (isChanged && gc.IsPublished)
                {
                    NotesMessagesRepo notesRep = new NotesMessagesRepo();
                    if (gc.TennisStage != null && gc.TennisStage.Team != null)
                    {
                        String message = String.Format("פרטי המשחק עודכנו: {0} vs {1}", gc.TeamsPlayer != null ? gc.TeamsPlayer.User.FullName : "", gc.TeamsPlayer1 != null ? gc.TeamsPlayer1.User.FullName : "");

                        if (gc.FirstPlayerId != null && gc.FirstPlayerId > 0)
                        {
                            notesRep.SendToPlayer((int)gc.TennisStage.SeasonId, (int)gc.FirstPlayer.UserId, message, true);
                        }
                        if (gc.SecondPlayerId != null && gc.SecondPlayerId > 0)
                        {
                            notesRep.SendToPlayer((int)gc.TennisStage.SeasonId, (int)gc.SecondPlayer.UserId, message, true);
                        }
                    }

                    var notsServ = new GamesNotificationsService();
                    notsServ.SendPushToDevices(GlobVars.IsTest);
                }
            }

            gamesRepo.Save();

            return Json(new { Success = true });
        }



        [HttpPost]
        public JsonResult EndTennisGame(TennisGameCycleForm frm)
        {
            var isNotZeroes = frm.FirstScoreOne.HasValue && frm.FirstScoreOne != 0 ||
                           frm.SecondScoreOne.HasValue && frm.SecondScoreOne != 0 ||
                           frm.ThirdScoreOne.HasValue && frm.ThirdScoreOne != 0 ||
                           frm.ForthScoreOne.HasValue && frm.ForthScoreOne != 0 ||
                           frm.FifthScoreOne.HasValue && frm.FifthScoreOne != 0 ||
                           frm.FirstScoreTwo.HasValue && frm.FirstScoreTwo != 0 ||
                           frm.SecondScoreTwo.HasValue && frm.SecondScoreTwo != 0 ||
                           frm.ThirdScoreTwo.HasValue && frm.ThirdScoreTwo != 0 ||
                           frm.ForthScoreTwo.HasValue && frm.ForthScoreTwo != 0 ||
                           frm.FifthScoreTwo.HasValue && frm.FifthScoreTwo != 0;

            var gc = gamesRepo.GetTennisGameCycleById(frm.CycleId);
            if (gc.GameStatus != GameStatus.Ended && isNotZeroes && !gc.TennisPlayoffBracket.IsMissingPlayers)
            {
                gc.GameStatus = GameStatus.Ended;
                gc.TennisGameSets.ElementAt(0).FirstPlayerScore = frm.FirstScoreOne ?? 0;
                gc.TennisGameSets.ElementAt(1).FirstPlayerScore = frm.SecondScoreOne ?? 0;
                gc.TennisGameSets.ElementAt(2).FirstPlayerScore = frm.ThirdScoreOne ?? 0;
                gc.TennisGameSets.ElementAt(3).FirstPlayerScore = frm.ForthScoreOne ?? 0;
                gc.TennisGameSets.ElementAt(4).FirstPlayerScore = frm.FifthScoreOne ?? 0;

                gc.TennisGameSets.ElementAt(0).SecondPlayerScore = frm.FirstScoreTwo ?? 0;
                gc.TennisGameSets.ElementAt(1).SecondPlayerScore = frm.SecondScoreTwo ?? 0;
                gc.TennisGameSets.ElementAt(2).SecondPlayerScore = frm.ThirdScoreTwo ?? 0;
                gc.TennisGameSets.ElementAt(3).SecondPlayerScore = frm.ForthScoreTwo ?? 0;
                gc.TennisGameSets.ElementAt(4).SecondPlayerScore = frm.FifthScoreTwo ?? 0;

                int firstPlayerScore = 0;
                int secondPlayerScore = 0;
                for (int i = 0; i < 5; i++)
                {
                    if (gc.TennisGameSets.ElementAt(i).FirstPlayerScore != 0 || gc.TennisGameSets.ElementAt(i).SecondPlayerScore != 0)
                    {
                        if (gc.TennisGameSets.ElementAt(i).FirstPlayerScore > gc.TennisGameSets.ElementAt(i).SecondPlayerScore)
                        {
                            firstPlayerScore++;
                        }
                        if (gc.TennisGameSets.ElementAt(i).SecondPlayerScore > gc.TennisGameSets.ElementAt(i).FirstPlayerScore)
                        {
                            secondPlayerScore++;
                        }
                    }
                }

                if (gc.TennisGroup.TypeId == GameTypeId.Playoff || gc.TennisGroup.TypeId == GameTypeId.Knockout || gc.TennisGroup.TypeId == GameTypeId.Knockout34 || gc.TennisGroup.TypeId == GameTypeId.Knockout34Consolences1Round || gc.TennisGroup.TypeId == GameTypeId.Knockout34ConsolencesQuarterRound)
                {
                    if (firstPlayerScore > secondPlayerScore)
                    {
                        gc.TennisPlayoffBracket.WinnerId = gc.FirstPlayerId;
                        gc.TennisPlayoffBracket.WinnerPlayerPairId = gc.FirstPlayerPairId;
                        gc.TennisPlayoffBracket.LoserId = gc.SecondPlayerId;
                        gc.TennisPlayoffBracket.LoserPlayerPairId = gc.SecondPlayerPairId;
                    }
                    else if (firstPlayerScore < secondPlayerScore)
                    {
                        gc.TennisPlayoffBracket.WinnerId = gc.SecondPlayerId;
                        gc.TennisPlayoffBracket.WinnerPlayerPairId = gc.SecondPlayerPairId;
                        gc.TennisPlayoffBracket.LoserId = gc.FirstPlayerId;
                        gc.TennisPlayoffBracket.LoserPlayerPairId = gc.FirstPlayerPairId;
                    }

                    if (!gc.TennisPlayoffBracket.WinnerId.HasValue && !gc.TennisPlayoffBracket.WinnerPlayerPairId.HasValue && gc.TennisPlayoffBracket.IsHasPlayers)
                    {
                        if (!gc.TennisPlayoffBracket.FirstPlayerId.HasValue && !gc.TennisPlayoffBracket.FirstPlayerPairId.HasValue)
                        {
                            gc.TennisPlayoffBracket.WinnerId = gc.SecondPlayerId;
                            gc.TennisPlayoffBracket.WinnerPlayerPairId = gc.SecondPlayerPairId;
                        }
                        if (!gc.TennisPlayoffBracket.SecondPlayerId.HasValue && !gc.TennisPlayoffBracket.SecondPlayerPairId.HasValue)
                        {
                            gc.TennisPlayoffBracket.WinnerId = gc.FirstPlayerId;
                            gc.TennisPlayoffBracket.WinnerPlayerPairId = gc.FirstPlayerPairId;
                        }
                    }
                }

                gc.FirstPlayerScore = firstPlayerScore;
                gc.SecondPlayerScore = secondPlayerScore;

                gamesRepo.UpdateTennis(gc);

                gamesRepo.Save();
            }
            return Json(new { Success = true });
        }



        public ActionResult AddNewByCycle(int stageId, int num, int roundNum)
        {
            var stage = stagesRepo.GetById(stageId);

            var leagueId = stage.LeagueId;
            var unionId = stage.League.UnionId;
            var clubId = stage.League?.ClubId;

            if (unionId == null && clubId == null)
            {
                throw new Exception("Union and club are unknown");
            }

            var section = stage.League?.Union?.Section?.Alias ?? stage.League?.Club?.Section?.Alias;

            ViewBag.Section = section;

            var vm = new GameCycleFormFull();

            var groups = stage.Groups.Where(gr => !gr.IsArchive).ToList();
            IEnumerable<Auditorium> auditoriums;
            IEnumerable<User> referees;
            IEnumerable<User> spectators;

            if (unionId != null)
            {
                auditoriums = auditoriumsRepo.GetAll(unionId.Value);

                referees = usersRepo.GetUnionAndLeagueReferees(unionId.Value, leagueId);
                spectators = usersRepo.GetUnionAndLeagueSpectators(unionId.Value, leagueId);
            }
            else
            {
                auditoriums = auditoriumsRepo.GetByClubAndSeason(clubId.Value, stage.League?.SeasonId);

                referees = usersRepo.GetClubAndLeagueReferees(clubId.Value, leagueId);
                spectators = usersRepo.GetClubAndLeagueSpectators(clubId.Value, leagueId);
            }

            vm.StageNum = stage.Number;
            vm.LeagueId = leagueId;
            vm.StageId = stageId;
            vm.CycleNum = num;
            vm.RoundNum = roundNum;
            vm.StartDate = DateTime.Now;
            vm.Auditoriums = new SelectList(auditoriums, "AuditoriumId", "Name");
            vm.Referees = new SelectList(referees.Union(spectators), "UserId", "FullName");
            vm.Groups = new SelectList(groups, "GroupId", "Name");

            return PartialView("_AddNewForm", vm);
        }

        public ActionResult AddNew(int stageId, int num)
        {
            var stage = stagesRepo.GetById(stageId);

            var leagueId = stage.LeagueId;
            var unionId = stage.League.UnionId;
            var clubId = stage.League?.ClubId;

            if (unionId == null && clubId == null)
            {
                throw new Exception("Union and club are unknown");
            }

            var section = stage.League?.Union?.Section?.Alias ?? stage.League?.Club?.Section?.Alias;

            ViewBag.Section = section;

            var vm = new GameCycleFormFull();

            var groups = stage.Groups.Where(gr => !gr.IsArchive).ToList();
            IEnumerable<Auditorium> auditoriums;
            IEnumerable<User> referees;
            IEnumerable<User> spectators;

            if (unionId != null)
            {
                auditoriums = auditoriumsRepo.GetAll(unionId.Value);

                referees = usersRepo.GetUnionAndLeagueReferees(unionId.Value, leagueId);
                spectators = usersRepo.GetUnionAndLeagueSpectators(unionId.Value, leagueId);
            }
            else
            {
                auditoriums = auditoriumsRepo.GetByClubAndSeason(clubId.Value, stage.League?.SeasonId);

                referees = usersRepo.GetClubAndLeagueReferees(clubId.Value, leagueId);
                spectators = usersRepo.GetClubAndLeagueSpectators(clubId.Value, leagueId);
            }

            vm.StageNum = stage.Number;
            vm.LeagueId = leagueId;
            vm.StageId = stageId;
            vm.CycleNum = num;
            vm.StartDate = DateTime.Now;
            vm.Auditoriums = new SelectList(auditoriums, "AuditoriumId", "Name");
            vm.Referees = new SelectList(referees.Union(spectators), "UserId", "FullName");
            vm.Groups = new SelectList(groups, "GroupId", "Name");

            return PartialView("_AddNewForm", vm);
        }

        public ActionResult GroupTeamsWithCycle(int gId, int cycleNum)
        {
            var isIndividual = db.Groups.Find(gId).IsIndividual;
            List<Team> leagueTeams = null;
            List<TeamsPlayer> leaguePlayers = null;
            if (isIndividual)
            {
                leaguePlayers = groupRepo.GetById(gId).GroupsTeams
                    .Where(t => t.AthleteId.HasValue)
                    .Select(t => t.TeamsPlayer)
                    .ToList();
            }
            else
            {
                leagueTeams = groupRepo.GetById(gId).GroupsTeams
                    .Where(t => t.Team?.IsArchive == false)
                    .Select(t => t.Team)
                    .ToList();
            }

            ViewBag.TeamsList = isIndividual ? new SelectList(leaguePlayers, "Id", "User.FullName") : new SelectList(leagueTeams, "TeamId", "Title");
            return PartialView("_TeamsView", isIndividual);
        }

        public ActionResult GoupTeams(int id)
        {
            var isIndividual = db.Groups.Find(id).IsIndividual;
            List<Team> leagueTeams = null;
            List<TeamsPlayer> leaguePlayers = null;
            if (isIndividual)
            {
                leaguePlayers = groupRepo.GetById(id).GroupsTeams
                    .Where(t => t.AthleteId.HasValue)
                    .Select(t => t.TeamsPlayer)
                    .ToList();
            }
            else
            {
                leagueTeams = groupRepo.GetById(id).GroupsTeams
                   .Where(t => t.Team?.IsArchive == false)
                   .Select(t => t.Team)
                   .ToList();
            }

            ViewBag.TeamsList = isIndividual ? new SelectList(leaguePlayers, "Id", "User.FullName") : new SelectList(leagueTeams, "TeamId", "Title");
            return PartialView("_TeamsView", isIndividual);
        }

        [HttpPost]
        public ActionResult AddNew(GameCycleFormFull frm)
        {
            var gc = new GamesCycle();
            var isIndividual = db.Groups.Find(frm.GroupId).IsIndividual;
            UpdateModel(gc);
            if (isIndividual)
            {
                frm.HomeTeamId = null;
                frm.GuestTeamId = null;
            }
            var serv = new SchedulingService(db);
            serv.AddGame(gc);

            return RedirectToAction("List", new { id = frm.LeagueId, desOrder = Session["desOrder"] });
        }

        [HttpPost]
        public ActionResult MoveDate(int stageId, int cycleNum, DateTime startDate, bool? isAll)
        {
            var stage = stagesRepo.GetById(stageId);

            var serv = new SchedulingService(db);
            serv.MoveCycles(stageId, cycleNum, startDate, isAll.HasValue);

            return RedirectToAction("List", new { id = stage.LeagueId, desOrder = Session["desOrder"], inpSeasonId = stage.League.SeasonId });
        }

        [HttpPost]
        public ActionResult MoveTennisDate(int stageId, int cycleNum, DateTime startDate, bool? isAll)
        {
            var stage = stagesRepo.GetTennisStageById(stageId);

            var serv = new SchedulingService(db);
            serv.MoveTennisCycles(stageId, cycleNum, startDate, isAll.HasValue);
            return RedirectToAction("TennisList", new { categoryId = stage.CategoryId, desOrder = false, inpSeasonId = stage.SeasonId });
        }


        public ActionResult DeleteCycle(int stageId, int cycleNum, int leagueId)
        {
            gamesRepo.DeleteCycle(stageId, cycleNum);

            return RedirectToAction("List", new { id = leagueId, desOrder = Session["desOrder"] });
        }

        public ActionResult ImportFromExcel(HttpPostedFileBase importedExcel)
        {
            try
            {
                if (importedExcel != null && importedExcel.ContentLength > 0)
                {
                    var dto = new List<ExcelGameDto>();

                    //open xml file from input
                    using (var workBook = new XLWorkbook(importedExcel.InputStream))
                    {
                        var sheet = workBook.Worksheet(1);
                        //skip column names
                        var valueRows = sheet.RowsUsed().Skip(1).ToList();
                        var countOfReferees = sheet.ColumnsUsed().ToList().Count - 35;
                        int i = 0;
                        //iterate over rows in xml file
                        //and getting cell from current row by [i] indexer
                        foreach (var row in valueRows)
                        {
                            int outGameId, outLeagueId, outStage,
                                outHomeTeamId, outHomeTeamScore, outGuestTeamId,
                                outGuestTeamScore, outAuditoriumId,
                                outCycleNumber;

                            var strDate = sheet.Cell(i + 2, 6).Value.ToString();
                            var outDate = DateTime.UtcNow;
                            DateTime parsedTime;

                            //CurrentCulture may not give us what we expect, if the import fails we should revisit this code and for better culture detection.
                            var localCulture = System.Globalization.CultureInfo.CurrentCulture.ToString();

                            outDate = DateTime.Parse(strDate, new CultureInfo(localCulture, false));
                            var time = sheet.Cell(i + 2, 7).Value.ToString();
                            DateTime.TryParse(time, out parsedTime);
                            var date = new DateTime(outDate.Year, outDate.Month, outDate.Day);

                            int.TryParse(sheet.Cell(i + 2, 1).Value.ToString(), out outGameId);
                            int.TryParse(sheet.Cell(i + 2, 2).Value.ToString(), out outLeagueId);
                            int.TryParse(sheet.Cell(i + 2, 4).Value.ToString(), out outStage);
                            int.TryParse(sheet.Cell(i + 2, 9).Value.ToString(), out outHomeTeamId);
                            int.TryParse(sheet.Cell(i + 2, 11).Value.ToString(), out outHomeTeamScore);
                            int.TryParse(sheet.Cell(i + 2, 12).Value.ToString(), out outGuestTeamId);
                            int.TryParse(sheet.Cell(i + 2, 14).Value.ToString(), out outGuestTeamScore);
                            int.TryParse(sheet.Cell(i + 2, 15).Value.ToString(), out outAuditoriumId);
                            int.TryParse(sheet.Cell(i + 2, 22 + countOfReferees).Value.ToString(), out outCycleNumber);

                            var dateWithTime = date.AddHours(parsedTime.Hour).AddMinutes(parsedTime.Minute);
                            var refereeIdsList = (sheet.Cell(i + 2, 17 + countOfReferees).Value.ToString()).Split(',').ToList();
                            var refereeNameList = gamesRepo.GetRefereesNames(outGameId)?.ToList();
                            var refereesNames = refereeNameList == null ? "" : String.Join(",", refereeNameList);
                            dto.Add(new ExcelGameDto
                            {
                                GameId = outGameId,
                                LeagueId = outLeagueId,
                                League = sheet.Cell(i + 2, 3).Value.ToString(),
                                Stage = outStage,
                                Date = dateWithTime,
                                HomeTeamId = outHomeTeamId,
                                HomeTeam = sheet.Cell(i + 2, 10).Value.ToString(),
                                HomeTeamScore = outHomeTeamScore,
                                GuestTeamId = outGuestTeamId,
                                GuestTeam = sheet.Cell(i + 2, 13).Value.ToString(),
                                GuestTeamScore = outGuestTeamScore,
                                AuditoriumId = outAuditoriumId,
                                Auditorium = sheet.Cell(i + 2, 16).Value.ToString(),
                                RefereeIds = sheet.Cell(i + 2, 17 + countOfReferees).Value.ToString(),
                                Referees = refereesNames,
                                SpectatorIds = sheet.Cell(i + 2, 18 + countOfReferees).Value.ToString(),
                                Spectators = sheet.Cell(i + 2, 19 + countOfReferees).Value.ToString(),
                                CycleNumber = outCycleNumber,
                                Groupe = sheet.Cell(i + 2, 23 + countOfReferees).Value.ToString(),
                                Set1 = sheet.Cell(i + 2, 25 + countOfReferees).Value.ToString(),
                                Set2 = sheet.Cell(i + 2, 28 + countOfReferees).Value.ToString(),
                                Set3 = sheet.Cell(i + 2, 31 + countOfReferees).Value.ToString(),
                                Set4 = sheet.Cell(i + 2, 34 + countOfReferees).Value.ToString()
                            });

                            i++;
                        }
                    }

                    gamesRepo.UpdateGroupCyclesFromExcelImport(dto);

                    return Redirect(Request.UrlReferrer.ToString());

                }
                return Redirect(Request.UrlReferrer.ToString());
            }
            catch (Exception e)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
        }

        [HttpGet]
        public ActionResult ShowRefereeModal(int cycleId, int leagueId)
        {
            var cycle = gamesRepo.GetGameCycleById(cycleId);
            //var refereeService = new RefereeService(cycleId, leagueId);

            var unionId = cycle.Group.Stage.League.UnionId ?? 0;

            var referees = usersRepo.GetUnionAndLeagueReferees(unionId, leagueId)
                    .ToDictionary(u => u.UserId, u => u);

            var spectators = usersRepo.GetUnionAndLeagueSpectators(unionId, leagueId)
                .ToDictionary(u => u.UserId, u => u);

            var refereeList = referees.Values.Union(spectators.Values)?.ToList();
            var refereesIds = cycle.RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.ToList();

            var refereeModal = new Referees
            {
                RefereesItems = refereeList,
                RefereeIds = refereesIds,
                CycleId = cycle.CycleId,
                LeagueId = leagueId,
                RefereesNames = GetRefereesNamesString(cycleId) ?? Messages.NoReferees
            };

            return PartialView("_Referee", refereeModal);
        }

        [HttpPost]
        public ActionResult AddReferee(Referees referee)
        {
            var service = new RefereeService(referee.CycleId, referee.LeagueId, referee.RefereeIds);
            var vm = service.AddReferee();
            if (vm == null)
            {
                vm = new Referees
                {
                    CycleId = referee.CycleId,
                    LeagueId = referee.LeagueId,
                    RefereeIds = referee.RefereeIds,
                    RefereesItems = service.GetRefereesInstance(referee.LeagueId).Values,
                    ErrMessage = "Wrong referee settings.",
                    RefereesNames = service.GetRefereesNamesString(referee.CycleId),
                };
            }
            return PartialView("_Referee", vm);
        }

        [HttpPost]
        public ActionResult AddDesk(Desks desk)
        {
            var service = new DeskService(desk.CycleId, desk.LeagueId, desk.DesksIds);
            var vm = service.AddDesk();
            if (vm == null)
            {
                vm = new Desks
                {
                    CycleId = desk.CycleId,
                    LeagueId = desk.LeagueId,
                    DesksIds = desk.DesksIds,
                    DesksItems = service.GetDeskInstance(desk.LeagueId).Values,
                    ErrMessage = "Wrong desk settings.",
                    DesksNames = service.GetDeskNamesString(desk.CycleId)
                };
            }
            return PartialView("_Desk", vm);
        }

        [HttpPost]
        public ActionResult SaveReferees(Referees referee)
        {
            var service = new RefereeService(referee.CycleId, referee.LeagueId, referee.RefereeIds);
            var vm = service.SaveReferees();
            if (vm == null)
            {
                vm = new Referees
                {
                    CycleId = referee.CycleId,
                    LeagueId = referee.LeagueId,
                    RefereeIds = referee.RefereeIds,
                    RefereesItems = service.GetRefereesInstance(referee.LeagueId).Values,
                    ErrMessage = "Wrong referee settings.",
                    RefereesNames = service.GetRefereesNamesString(referee.CycleId),
                };
            }
            return PartialView("_Referee", vm);
        }

        [HttpPost]
        public ActionResult SaveDesks(Desks desk)
        {
            var service = new DeskService(desk.CycleId, desk.LeagueId, desk.DesksIds);
            var vm = service.SaveDesks();
            if (vm == null)
            {
                vm = new Desks
                {
                    CycleId = desk.CycleId,
                    LeagueId = desk.LeagueId,
                    DesksIds = desk.DesksIds,
                    DesksItems = service.GetDeskInstance(desk.LeagueId).Values,
                    ErrMessage = "Wrong desk settings.",
                    DesksNames = service.GetDeskNamesString(desk.CycleId)
                };
            }
            return PartialView("_Desk", vm);
        }


        [HttpPost]
        public ActionResult DeleteReferee(string refereeIdent, int refereeOrder, int cycleId, int leagueId)
        {
            var service = new RefereeService(cycleId, leagueId);
            var vm = service.DeleteReferee(refereeIdent, refereeOrder);
            return PartialView("_Referee", vm);
        }

        [HttpPost]
        public ActionResult DeleteDesk(string deskIdent, int deskOrder, int cycleId, int leagueId)
        {
            var service = new DeskService(cycleId, leagueId);
            var vm = service.DeleteDesk(deskIdent, deskOrder);
            return PartialView("_Desk", vm);
        }

        [HttpPost]
        public ActionResult SaveStateOfSchedule(string elementDivId, bool isHidden, int leagueId)
        {
            leagueRepo.UpdateScheduleState(elementDivId, leagueId, isHidden, AdminId);
            return Json(new { Success = true });
        }
    }
}
