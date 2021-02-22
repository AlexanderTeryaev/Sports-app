using System;
using System.Web.Mvc;

using Omu.ValueInjecter;
using AppModel;
using CmsApp.Models;
using CmsApp.Models.Mappers;
using Resources;
using System.Linq;
using System.Collections.Generic;

namespace CmsApp.Controllers
{
    public class GamesController : AdminController
    {
        // GET: Games
        public ActionResult Edit(int idLeague, int idStage, int? departmentId = null)
        {
            var sectionAlias = leagueRepo.GetSectionAlias(idLeague);
            var league = leagueRepo.GetById(idLeague);
            var isTennisLeague = leagueRepo.CheckIfIsTennisLeague(idLeague);
            var stage = stagesRepo.GetByIdExtended(idStage);
            var firstGroup = stage?.Groups?.FirstOrDefault();
            var numOfCycles = firstGroup != null ? stage?.Groups?.FirstOrDefault().GroupsTeams.Where(gt => !(gt?.Team?.IsArchive ?? false)).Count() - 1 : 0;
            var model = new GameForm
            {
                StartDate = DateTime.Now,
                LeagueId = idLeague,
                DaysList = Messages.WeekDays.Split(','),
                StageId = idStage,
                IsTennisLeague = isTennisLeague,
                CreateCrossesStage = stage?.CreateCrossesStage ?? false,
                IsCrossesStageApplicable = stage?.Groups?.Count == 2,
                NumberOfCycles = numOfCycles ?? 0
            };

            Session["idLeague"] = idLeague;
            Game game;
            if (sectionAlias.Equals(SectionAliases.Tennis))
            {
                game = gamesRepo.GetByLeagueStage(idLeague, idStage) ?? new Game
                {
                    GamesInterval = "00:30",
                    ActiveWeeksNumber = 1,
                    BreakWeeksNumber = 0,
                    StartDate = DateTime.Now,
                    PointsWin = 1,
                    PointsTechWin = 1,
                    PointsTechLoss = 0,
                    PointsLoss = 0,
                    PointsDraw = 0,
                    SortDescriptors = "1",
                    BestOfSets = 3,
                    NumberOfGames = 3,
                    CyclesStartDate = ""
                };
            }
            else if (sectionAlias.Equals(SectionAliases.Waterpolo))
            {
                game = gamesRepo.GetByLeagueStage(idLeague, idStage) ?? new Game
                {
                    GamesInterval = "00:30",
                    ActiveWeeksNumber = 1,
                    BreakWeeksNumber = 0,
                    StartDate = DateTime.Now,
                    PointsWin = 2,
                    PointsTechWin = 2,
                    PointsTechLoss = 0,
                    PointsLoss = 0,
                    PointsDraw = 1,
                    SortDescriptors = "1"
                };
            }
            else if (sectionAlias.Equals(SectionAliases.Rugby))
            {
                game = gamesRepo.GetByLeagueStage(idLeague, idStage) ?? new Game
                {
                    GamesInterval = "00:30",
                    ActiveWeeksNumber = 1,
                    BreakWeeksNumber = 0,
                    StartDate = DateTime.Now,
                    PointsWin = 4,
                    PointsTechWin = 4,
                    PointsTechLoss = 0,
                    PointsLoss = 1,
                    PointsDraw = 2,
                    SortDescriptors = "1"
                };
            }
            else if (sectionAlias.Equals(SectionAliases.Softball))
            {
                game = gamesRepo.GetByLeagueStage(idLeague, idStage) ?? new Game
                {
                    GamesInterval = "00:30",
                    ActiveWeeksNumber = 1,
                    BreakWeeksNumber = 0,
                    StartDate = DateTime.Now,
                    PointsWin = 2,
                    PointsTechWin = 2,
                    PointsTechLoss = 0,
                    PointsLoss = 0,
                    PointsDraw = 1,
                    SortDescriptors = "1"
                };
            }
            else
            {
                game = gamesRepo.GetByLeagueStage(idLeague, idStage) ?? new Game
                {
                    GamesInterval = "00:30",
                    ActiveWeeksNumber = 1,
                    BreakWeeksNumber = 0,
                    StartDate = DateTime.Now,
                    PointsWin = 2,
                    PointsTechWin = 2,
                    PointsTechLoss = 0,
                    PointsLoss = 1,
                    PointsDraw = 0,
                    SortDescriptors = "1"
                };

            }

            game.StageId = idStage;

            model.InjectFrom(game);
            model.TechWinHomePoints = game.TechWinHomePoints ?? 6;
            model.TechWinGuestPoints = game.TechWinGuestPoints ?? 0;
            model.ShowCyclesOnExternal = game.ShowCyclesOnExternal ?? GetValueForExternalLinkCyclesEnabling(game);
            model.IsDivision = groupRepo.GetGroupByLeagueStageId(idStage)?.TypeId == GameTypeId.Division;
            ViewBag.IsDepartmentLeague = departmentId.HasValue;
            ViewBag.DepartmentId = departmentId;

            return PartialView("_Edit", model);
        }

        [NonAction]
        private int GetValueForExternalLinkCyclesEnabling(Game game)
        {
            var league = game?.League;
            var sectionAlias = league?.Club?.Section?.Alias ?? league?.Club?.Union?.Section?.Alias ?? league?.Union?.Section?.Alias;
            if (sectionAlias == GamesAlias.NetBall && !game.ShowCyclesOnExternal.HasValue) return 0;
            else return 1;
        }

        public ActionResult EditTennis(int categoryId, int idStage, int? departmentId = null)
        {
            var frm = new GameForm
            {
                StartDate = DateTime.Now,
                LeagueId = categoryId,
                DaysList = Messages.WeekDays.Split(','),
                StageId = idStage
            };

            Session["idCategory"] = categoryId;

            TennisGame game = gamesRepo.GetTennisByCategoryStage(categoryId, idStage) ?? new TennisGame
            {
                GamesInterval = "00:30",
                ActiveWeeksNumber = 1,
                BreakWeeksNumber = 0,
                StartDate = DateTime.Now,
                PointsWin = 2,
                PointTechWin = 2,
                PointsTechLoss = 0,
                PointsLoss = 1,
                PointsDraw = 0,
                SortDescriptions = "1",
                IsDateForFirstCycleOnly = true
            };

            game.StageId = idStage;

            frm.InjectFrom(game);
            frm.IsDateForFirstCycleOnly = game.IsDateForFirstCycleOnly ?? false;
            frm.ShowCyclesOnExternal = game.ShowCyclesOnExternal ?? 1;
            frm.IsDivision = groupRepo.GetTennisCompetitionGroupByStageId(idStage)?.TypeId == GameTypeId.Division;
            ViewBag.IsDepartmentLeague = departmentId.HasValue;
            ViewBag.DepartmentId = departmentId;

            return PartialView("_TennisGamesEdit", frm);
        }

        public ActionResult GamesUrl(int clubId, int teamId, int? seasonId)
        {
            var team = teamRepo.GetScheduleScrapperById(teamId, clubId, seasonId ?? 0);
            var teamGames = teamRepo.GetTeamGamesFromScrapper(clubId, teamId).ToViewModelSorted();
            var section = secRepo.GetSectionByTeamId(teamId).Alias;
            ViewBag.Section = section;
            var viewModel = new GamesUrlForm
            {
                ClubId = clubId,
                TeamId = teamId,
                GamesUrl = team?.GameUrl ?? string.Empty,
                TeamSchedule = teamGames,
                TeamName = team?.ExternalTeamName,
                SeasonId = seasonId ?? 0
            };


            return PartialView("~/Views/Schedules/GamesUrls/Index.cshtml", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(GameForm model, string[] daysArr, int? departmentId = null)
        {
            var idLeague = model.LeagueId;
            if (!string.IsNullOrWhiteSpace(model.CyclesStartDate))
            {
                var cyclesStartDate = model.CyclesStartDate ?? "";
                var cyclesStartDateList = cyclesStartDate.Split(',').ToList();
                var FirstEmptyIndex = cyclesStartDateList.IndexOf("");
                var isHaveDateInList = false;
                if (FirstEmptyIndex > -1)
                {
                    var fromFirstEmptyList = cyclesStartDateList.Skip(FirstEmptyIndex + 1).ToList();
                    isHaveDateInList = fromFirstEmptyList.Where(x => !string.IsNullOrWhiteSpace(x)).Count() > 0;
                }

                if (isHaveDateInList)
                {
                    TempData["Error_Empty"] = FirstEmptyIndex;
                    if (departmentId.HasValue)
                    {
                        return RedirectToAction("Edit", new { idLeague, idStage = model.StageId, departmentId });
                    }
                    return RedirectToAction("Edit", new { idLeague, idStage = model.StageId });
                }

            }



            if (idLeague == 0)
            {
                var url = Request.RawUrl;
                var session = Session["idLeague"];
                var startPosition = url.LastIndexOf('/') + 1;
                idLeague = (int?)session ?? int.Parse(url.Substring(startPosition,
                               Request.RawUrl.IndexOf('?') - startPosition));
            }

            var stage = stagesRepo.GetById(model.StageId);
            stage.CreateCrossesStage = model.CreateCrossesStage;
            var game = gamesRepo.GetByLeagueStage(model.LeagueId, model.StageId);
            if (game == null)
            {
                game = new Game
                {
                    StageId = model.StageId,
                    SortDescriptors = "0,1,2"
                };
                gamesRepo.Create(game);
            }



            UpdateModel(game);
            game.GameDays = string.Join(",", daysArr);
            game.LeagueId = idLeague;
            gamesRepo.Save();

            ViewBag.IsDepartmentLeague = departmentId.HasValue;
            ViewBag.DepartmentId = departmentId;
            TempData["Saved"] = true;

            if (departmentId.HasValue)
            {
                return RedirectToAction("Edit", new { idLeague, idStage = model.StageId, departmentId });
            }

            return RedirectToAction("Edit", new { idLeague, idStage = model.StageId });
        }

        [HttpPost]
        public void SetAllAuditouriums(int stageId, int cycleNum, int? auditoriumId)
        {
            gamesRepo.SetAuditoriums(stageId, cycleNum, auditoriumId);
            gamesRepo.Save();
        }

        [HttpPost]
        public void SetAllTennisAuditouriums(int stageId, int cycleNum, int? auditoriumId)
        {
            gamesRepo.SetTennisAuditoriums(stageId, cycleNum, auditoriumId);
            gamesRepo.Save();
        }

        [HttpPost]
        public void SetAllTennisTimeInitials(int stageId, int cycleNum, string timeInitial)
        {
            gamesRepo.SetTennisTimeInitials(stageId, cycleNum, timeInitial);
            gamesRepo.Save();
        }

        [HttpPost]
        public ActionResult SetAllReferees(int stageId, int[] cycleIds, int[] refereeIds)
        {
            var refereeNames = new List<string>();
            var btns = gamesRepo.SetAllReferees(stageId, cycleIds, refereeIds);
            for (int i = 0; i < btns.Count; i++)
            {
                var title = btns[i];
                if (string.IsNullOrWhiteSpace(title))
                {
                    btns[i] = Messages.ChooseReferees;
                }
            }
            return Json(btns);
        }

        [HttpPost]
        public ActionResult UpdateRoundReferees(int stageId, int[] cycleIds, int refereeId, bool isChecked)
        {
            var btns = gamesRepo.UpdateRoundReferees(stageId, cycleIds, refereeId, isChecked);
            for (int i = 0; i < btns.Count; i++)
            {
                var title = btns[i];
                if (string.IsNullOrWhiteSpace(title))
                {
                    btns[i] = Messages.ChooseReferees;
                }
            }
            return Json(btns);
        }

        [HttpPost]
        public ActionResult UpdateRoundAllReferees(int stageId, int[] cycleIds, int[] refereeIds, bool isChecked)
        {
            var btns = gamesRepo.UpdateRoundAllReferees(stageId, cycleIds, refereeIds, isChecked);
            for (int i = 0; i < btns.Count; i++)
            {
                var title = btns[i];
                if (string.IsNullOrWhiteSpace(title))
                {
                    btns[i] = Messages.ChooseReferees;
                }
            }
            return Json(btns);
        }




        [HttpPost]
        public ActionResult EditTennis(GameForm frm, string[] daysArr, int? departmentId = null)
        {
            int idCategory = frm.LeagueId;
            if (idCategory == 0)
            {
                var url = Request.RawUrl;
                var session = Session["idCategory"];
                int startPosition = url.LastIndexOf('/') + 1;
                idCategory = (int?)session ?? int.Parse(url.Substring(startPosition,
                               Request.RawUrl.IndexOf('?') - startPosition));
            }

            var item = gamesRepo.GetTennisByCategoryStage(frm.LeagueId, frm.StageId);
            if (item == null)
            {
                item = new TennisGame();
                item.StageId = frm.StageId;
                item.SortDescriptions = "0,1,2";
                item.IsDateForFirstCycleOnly = true;
                gamesRepo.CreateTennis(item);
            }

            UpdateModel(item);
            item.GameDays = string.Join(",", daysArr);
            item.CategoryId = idCategory;
            item.RoundStartCycle = frm.RoundStartCycle;
            item.IsDateForFirstCycleOnly = frm.IsDateForFirstCycleOnly;
            gamesRepo.Save();

            ViewBag.IsDepartmentLeague = departmentId.HasValue;
            ViewBag.DepartmentId = departmentId;
            TempData["Saved"] = true;
            if (departmentId.HasValue)
            {
                return RedirectToAction("EditTennis", new { idCategory, idStage = frm.StageId, departmentId });
            }
            else
            {
                return RedirectToAction("EditTennis", new { categoryId = idCategory, idStage = frm.StageId });
            }
        }
    }
}