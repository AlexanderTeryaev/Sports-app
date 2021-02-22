using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using DataService;
using DataService.DTO;
using CmsApp.Models;
using System;
using AppModel;
using AutoMapper;
using Resources;

namespace CmsApp.Controllers
{
    public class SectionsController : AdminController
    {
        private const string leaguesKey = "Leagues";
        private const string auditoriumsKey = "Auditoriums";

        public ActionResult Index(int? langId)
        {
            ViewBag.LangId = new SelectList(secRepo.GetLanguages(), "LangId", "Name");
            var resList = secRepo.GetSections(langId);
            return View(resList);
        }

        [HttpPost]
        public ActionResult Update(int sectionId, string name)
        {
            var item = secRepo.GetById(sectionId);
            item.Name = name;
            secRepo.Save();
            TempData["SavedId"] = sectionId;

            return RedirectToAction("Index", new { langId = item.LangId });
        }

        public ActionResult Edit(int id)
        {
            gamesRepo.UpdateGameStatistics();
            var sect = secRepo.GetById(id);
            ViewBag.SectionName = sect.Name;
            ViewBag.SectionAlias = sect.Alias;
            SaveCurrentSectionAliasIntoSession(sect);
            return View();
        }

        public ActionResult List(Schedules model, int seasonId, int? idUnion = null, int? disciplineId = null)
        {
            if (model == null && idUnion != null)
                model = new Schedules()
                {
                    UnionId = (int)idUnion,
                };
            bool shouldClearSelection = false;
            if (model.Leagues == null)
            {
                shouldClearSelection = true;
            }
            if (TempData.ContainsKey(leaguesKey) && model.Leagues == null)
            {
                model.Leagues = (LeagueShort[])TempData[leaguesKey];
            }
            if (TempData.ContainsKey(auditoriumsKey) && model.Auditoriums == null)
            {
                model.Auditoriums = (List<AuditoriumShort>)TempData[auditoriumsKey];
            }

            var res = new Schedules();
            res.UnionId = model.UnionId;
            res.SeasonId = seasonId;
            res.Games = new List<GameInLeague>();
            var leagueShortList = leagueRepo.GetLeaguesFilterList(model.UnionId ?? 0, seasonId, disciplineId);
            if (model.Leagues != null && !shouldClearSelection)
            {
                foreach (var league in leagueShortList)
                {
                    if (model.Leagues[0].Check || model.Leagues
                            .Where(l => l.Id == league.Id).Select(l => l.Check).SingleOrDefault())
                        league.Check = true;
                }
            }
            //  Fill in auditoriums list
            var auditoriumShortList = auditoriumsRepo.GetAuditoriumsFilterList(model.UnionId ?? 0, seasonId);
            if (model.Auditoriums != null)
            {
                foreach (var aud in auditoriumShortList)
                {
                    if (model.Auditoriums[0].Check || model.Auditoriums
                            .Where(a => a.Id == aud.Id).Select(a => a.Check).SingleOrDefault())
                        aud.Check = true;
                }
            }

            //  Fill in games list
            var userIsEditor = User.IsInAnyRole(AppRole.Admins, AppRole.Editors, AppRole.Workers);
            var cond = new GamesRepo.GameFilterConditions
            {
                seasonId = seasonId,
                leagues = leagueShortList,
                auditoriums = auditoriumShortList,
                onlyNotIsArchive = true
            };
            if (model.dateFilterType == Schedules.DateFilterPeriod.BeginningOfMonth)
            {
                cond.dateFrom = Schedules.FirstDayOfMonth;
                cond.dateTo = null;
            }
            else if (model.dateFilterType == Schedules.DateFilterPeriod.Ranged)
            {
                cond.dateFrom = model.dateFrom;
                cond.dateTo = model.dateTo;
            }
            else if(model.dateFilterType == Schedules.DateFilterPeriod.FromToday)
            {
                cond.dateFrom = Schedules.Today;
                cond.dateTo = null;
            } 
            else
            {
                cond.dateFrom = null;
                cond.dateTo = null;
            }
            var result = gamesRepo.GetCyclesByFilterConditions(cond, userIsEditor, true, model.OnlyPublished, model.OnlyUnpublished);
            var someLeaguesChecked = cond.leagues.Any(l => l.Check);
            var allLeaguesChecked = cond.leagues.All(l => l.Check);
            var someAuditoriumsChecked = cond.auditoriums.Any(a => a.Check);
            var allAuditoriumsChecked = cond.auditoriums.All(a => a.Check);
            var gamesSelected = result.Count() > 0;
            if (gamesSelected)
            {
                res.Games = result.Select(gc => new GameInLeague(gc)
                {
                    LeagueId = gc.Stage.LeagueId,
                    LeagueName = gc.Stage.League.Name,
                    IsPublished = gc.IsPublished,
                    IsDateUpdated = gc.IsDateUpdated,
                    Note = gc.Note,
                    Remark = gc.Remark
                }).ToList();
                var uRepo = new UsersRepo();
                var referees = uRepo.GetUnionWorkers(model.UnionId ?? 0, "referee")
                    .ToDictionary(u => u.UserId, u => u);
                var spectators = uRepo.GetUnionWorkers(model.UnionId ?? 0, "spectator")
                    .ToDictionary(u => u.UserId, u => u);
                var desks = uRepo.GetUnionWorkers(model.UnionId ?? 0, "desk")
                   .ToDictionary(u => u.UserId, u => u);
                res.Referees = referees.Union(spectators).OrderBy(x => x.Value.FullName).ToDictionary(k => k.Key, v => v.Value);
                res.Spectators = spectators.Union(referees).ToDictionary(k => k.Key, v => v.Value);
                res.Desks = desks.Union(spectators).Union(referees).ToDictionary(k => k.Key, v => v.Value);
                // If all games for selected leagues are published
                if (res.Games.All(gc => gc.IsPublished))
                    res.IsPublished = true;
                // If all games for selected leagues aren't published
                else if (res.Games.All(gc => !gc.IsPublished))
                    res.IsPublished = false;
                // If there are published and unpublished games for selected leagues
                else
                    res.IsPublished = null;
            }
            else
                res.IsPublished = false;

            res.Leagues = leagueShortList.ToArray();
            res.Auditoriums = auditoriumShortList.ToList();
            res.dateFilterType = model?.dateFilterType ?? 0;
            res.dateFrom = model?.dateFrom ?? Schedules.FirstDayOfMonth;
            res.dateTo = model?.dateTo ?? Schedules.Tomorrow;
            int[] leagueIdArray;
            if (res.Leagues.Any(l => l.Check))
            {
                leagueIdArray = res.Leagues.Where(l => l.Id > 0 && l.Check).Select(l => l.Id).ToArray();
            }
            else
            {
                leagueIdArray = res.Games.Select(g => g.LeagueId).Distinct().ToArray();
            }

            if (model != null)
            {
                res.Sort = model.Sort;

                if (res.Sort == 1)
                {
                    res.Games = res.Games.OrderBy(x => x.StartDate).ToList();
                }
                if (res.Sort == 2)
                {
                    res.Games = res.Games.OrderBy(x => x.Auditorium != null ? x.Auditorium.Name : "").ToList();
                }
                if (res.Sort == 0)
                {
                    res.Sort = Session["GamesSort"] == null ? 1 : (int)Session["GamesSort"];

                    if (res.Sort == 1)
                    {
                        res.Games = res.Games.OrderBy(x => x.StartDate).ToList();
                    }
                    if (res.Sort == 2)
                    {
                        res.Games = res.Games.OrderBy(x => x.Auditorium != null ? x.Auditorium.Name : "").ToList();
                    }
                }
            }
            Session["GamesSort"] = res.Sort;
            TempData[leaguesKey] = res.Leagues;
            TempData[auditoriumsKey] = res.Auditoriums;
            int? unionSection = null;
            if (idUnion.HasValue)
            {
                unionSection = idUnion.Value;
            }
            else if (res.UnionId.HasValue)
            {
                unionSection = res.UnionId.Value;
            }

            ViewBag.IsCatchBall = false;
            ViewBag.RoleName = usersRepo.GetTopLevelJob(AdminId);
            ViewBag.IsUnionViewer = AuthSvc.AuthorizeUnionViewerByManagerId(model.UnionId ?? 0, AdminId);
            if (unionSection.HasValue)
            {
                var section = unionsRepo.GetSectionByUnionId(unionSection.Value);
                ViewBag.IsCatchBall = string.Equals(section.Alias, GamesAlias.NetBall, StringComparison.OrdinalIgnoreCase);
                ViewBag.IsPenaltySection = string.Equals(section.Alias, GamesAlias.WaterPolo, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(section.Alias, GamesAlias.Soccer, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(section.Alias, GamesAlias.Rugby, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(section.Alias, GamesAlias.Handball, StringComparison.OrdinalIgnoreCase);

                res.Section = section.Alias;
                res.teamsByGroups = section.Alias.Equals(GamesAlias.Tennis)
                    ? teamRepo.GetGroupTeamsBySeasonAndLeaguesForTennis(seasonId, leagueIdArray, gamesSelected && ((allLeaguesChecked || !someLeaguesChecked) && (allAuditoriumsChecked || !someAuditoriumsChecked)))
                    : teamRepo.GetGroupTeamsBySeasonAndLeagues(seasonId, leagueIdArray, gamesSelected && ((allLeaguesChecked || !someLeaguesChecked) && (allAuditoriumsChecked || !someAuditoriumsChecked)));

                switch (section.Alias)
                {
                    case GamesAlias.WaterPolo:
                    case GamesAlias.Soccer:
                    case GamesAlias.Rugby:
                    case GamesAlias.BasketBall:
                        return PartialView("BasketBallWaterpolo/_List", res);

                    default:
                        return PartialView("_List", res);
                }

            }

            res.teamsByGroups = teamRepo.GetGroupTeamsBySeasonAndLeagues(seasonId, leagueIdArray,
                gamesSelected && ((allLeaguesChecked || !someLeaguesChecked)
                               && (allAuditoriumsChecked || !someAuditoriumsChecked)));

            return PartialView("_List", res);
        }

        [HttpPost]
        public ActionResult Publish(int[] games, int seasonId, int unionId, bool isPublished)
        {
            if (games != null && games.Any())
            {
                var gameCycles = gamesRepo.GetGamesQuery().Where(g => games.Contains(g.CycleId)).ToList();
                gameCycles.ForEach(g => g.IsPublished = isPublished);
                gameCycles.ForEach(g => gamesRepo.Update(g));
            }

            return RedirectToAction("List", new { idUnion = unionId, seasonId });
        }

        public ActionResult Delete(int id, int seasonId)
        {
            var gc = gamesRepo.GetGameCycleById(id);
            int leagueId = gc.Stage.LeagueId;
            var league = leagueRepo.GetAll().FirstOrDefault(x => x.LeagueId == leagueId);
            gamesRepo.RemoveCycle(gc);
            gamesRepo.Save();
            var modelShedule = new Schedules()
            {
                UnionId = league.UnionId
            };
            return RedirectToAction("List", new { idUnion = modelShedule.UnionId, seasonId });
        }

        public ActionResult UpdateGame(GameCycleForm frm)
        {
            try
            {
                frm.RefereeIds = Request.Params["RefereeIds"];
                var spectators = frm.SpectatorIds != null && frm.SpectatorIds.Any() ? String.Join(",", frm.SpectatorIds) : string.Empty;
                bool isChanged = false;
                var gc = gamesRepo.GetGameCycleById(frm.CycleId);
                if (gc.AuditoriumId != frm.AuditoriumId)
                {
                    isChanged = true;
                    gc.AuditoriumId = frm.AuditoriumId;
                }
                if (!gc.StartDate.Equals(frm.StartDate) && gc.IsPublished)
                {
                    isChanged = true;
                    gc.StartDate = frm.StartDate;
                    gc.IsDateUpdated = true;
                }
                else if (!gc.StartDate.Equals(frm.StartDate) && !gc.IsPublished)
                {
                    isChanged = true;
                    gc.StartDate = frm.StartDate;
                }

                //If we check for Any()
                //the 'select' (no referee) option from dropdown will not be saved
                //if (frm.RefereeIds != null && frm.RefereeIds.Any())
                if (frm.RefereeIds != null)
                {
                    gc.RefereeIds = string.Join(",", frm.RefereeIds);
                }

                if (!string.Equals(gc.SpectatorIds, spectators, StringComparison.OrdinalIgnoreCase))
                {
                    gc.SpectatorIds = spectators;
                }
                //  Teams could be replaced only before game starts
                if (gc.GameStatus != GameStatus.Started && gc.GameStatus != GameStatus.Ended && gc.Group.TypeId == 1)
                {
                    if (frm.HomeTeamId != null)
                    {
                        gc.HomeTeamId = frm.HomeTeamId;
                    }

                    if (frm.GuestTeamId != null)
                    {
                        gc.GuestTeamId = frm.GuestTeamId;
                    }
                }

                if (!string.Equals(gc.Remark, frm.Remark, StringComparison.OrdinalIgnoreCase))
                {
                    gc.Remark = frm.Remark;
                }
                gamesRepo.Update(gc);

                if (isChanged && gc.IsPublished)
                {
                    NotesMessagesRepo notesRep = new NotesMessagesRepo();
                    if (gc.Stage != null && gc.Stage.League != null && gc.Stage.League.SeasonId != null)
                    {
                        String message = String.Format("Game details has been updated: {0} vs {1}", gc.HomeTeam != null ? gc.HomeTeam.Title : "", gc.GuestTeam != null ? gc.GuestTeam.Title : "");

                        if (gc.HomeTeamId != null)
                        {
                            notesRep.SendToTeam((int)gc.Stage.League.SeasonId, (int)gc.HomeTeamId, message);
                        }
                        if (gc.GuestTeamId != null)
                        {
                            notesRep.SendToTeam((int)gc.Stage.League.SeasonId, (int)gc.GuestTeamId, message);
                        }
                        if (!string.IsNullOrEmpty(gc.RefereeIds))
                        {
                            var ids = gc.RefereeIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                            notesRep.SendToUsers(ids, message);
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
            return Json(new { stat = "ok", id = frm.CycleId });
        }

        public ActionResult Toggle(int id, int unionId, int seasonId)
        {
            TeamShortDTO hTeam;
            TeamShortDTO gTeam;
            GamesCycle cycle;
            int homeTeamScore, guestTeamScore;
            var gameAlias = unionsRepo.GetById(unionId)?.Section?.Alias;
            try
            {
                gamesRepo.ToggleTeams(id);
                cycle = gamesRepo.GetGameCycleById(id);
                var homeTeam = teamRepo.GetById(cycle.HomeTeamId, seasonId);
                hTeam = new TeamShortDTO
                {
                    TeamId = homeTeam.TeamId,
                    Title = homeTeam.Title
                };
                var guestTeam = teamRepo.GetById(cycle.GuestTeamId, seasonId);
                gTeam = new TeamShortDTO
                {
                    TeamId = guestTeam.TeamId,
                    Title = guestTeam.Title
                };
                if (gameAlias == GamesAlias.BasketBall || gameAlias == GamesAlias.WaterPolo || gameAlias == GamesAlias.Soccer || gameAlias == GamesAlias.Rugby || gameAlias == GamesAlias.Softball)
                {
                    homeTeamScore = cycle.GameSets.Sum(s => s.HomeTeamScore);
                    guestTeamScore = cycle.GameSets.Sum(s => s.GuestTeamScore);
                }
                else
                {
                    homeTeamScore = cycle.HomeTeamScore;
                    guestTeamScore = cycle.GuestTeamScore;
                }
            }
            catch (Exception e)
            {
                return Json(new { stat = "error", message = e.ToString() });
            }
            return Json(new
            {
                stat = "ok",
                id = id,
                homeTeam = hTeam,
                guestTeam = gTeam,
                homeTeamScore = homeTeamScore,
                guestTeamScore = guestTeamScore,
                arenaId = cycle.AuditoriumId
            });
        }

        public ActionResult CreateSection(SectionModel section)
        {
            if (section == null)
                return RedirectToAction("Index", new { langId = section.LangId });
            secRepo.CreateSection(new Section
            {
                LangId = section.LangId,
                Name = section.Name,
                Alias = section.Name,
                IsIndividual = section.IsIndividual
            });
            db.SaveChanges();
            return RedirectToAction("Index", new { langId = section.LangId });
        }

        [HttpPost]
        public void ChangeIndividualStatus(int id, bool isIndividual)
        {
            secRepo.SectionIndividualStatus(id, isIndividual);
        }
    }
}
