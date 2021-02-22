using AppModel;
using DataService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataService.DTO;
using DataService.Services;
using CmsApp.Helpers;
using CmsApp.Models;
using System.Threading.Tasks;
using System.IO;

namespace CmsApp.Controllers
{
    public class GameCycleController : AdminController
    {
        GamesService _gamesService = new GamesService();
        UsersRepo uRepo = new UsersRepo();

        // GET: GameCycle/Edit/5
        public ActionResult Edit(int id, bool global = false, int? departmentId = null)
        {
            gamesRepo.UpdateGameStatistics();
            var gc = gamesRepo.GetGameCycleById(id);
            if (global == true)
            {
                var league = leagueRepo.GetById(gc.Stage.LeagueId);
                Session["UnionId"] = league.UnionId;
                var checks = (bool[])Session["Checks"];
            }
            Session["global"] = global;

            //var gameAlias = departmentId == null ? gc.Stage?.League?.Union?.Section?.Alias : gc?.Group?.Stage?.League?.Club?.Section?.Alias ?? "";
            var gameAlias = gc?.Stage?.League?.Union?.Section?.Alias ?? gc?.Group?.Stage?.League?.Club?.Section?.Alias ?? "";
            var unionId = gc.Stage?.League?.UnionId ?? gc?.Group?.Stage?.League?.Club?.ParentClub?.Union?.UnionId;
            var leagueId = gc.Stage?.LeagueId ?? gc?.Group?.Stage?.League?.LeagueId;
            var referees = unionId.HasValue ? uRepo.GetUnionAndLeagueReferees(unionId.Value, leagueId.Value) : new List<User>();
            var spectators = unionId.HasValue ? uRepo.GetUnionAndLeagueSpectators(unionId.Value, leagueId.Value) : new List<User>();

            ViewBag.Referees = referees.Union(spectators);
            ViewBag.Spectators = spectators.Union(referees);

            ViewBag.IsDepartment = departmentId.HasValue ? true : false;
            ViewBag.DepartmentId = departmentId;
            ViewBag.IsBasketball = gameAlias == GamesAlias.BasketBall;
            ViewBag.IsWaterPolo = gameAlias == GamesAlias.WaterPolo;
            ViewBag.IsSoftBall = gameAlias == GamesAlias.Softball;
            ViewBag.NeedPenalties = gameAlias.Equals(GamesAlias.WaterPolo, StringComparison.OrdinalIgnoreCase)
                || gameAlias.Equals(GamesAlias.Soccer, StringComparison.OrdinalIgnoreCase)
                || gameAlias.Equals(GamesAlias.Rugby, StringComparison.OrdinalIgnoreCase)
                || gameAlias.Equals(GamesAlias.Handball, StringComparison.OrdinalIgnoreCase);

            switch (gameAlias)
            {
                case GamesAlias.WaterPolo:
                case GamesAlias.Soccer:
                case GamesAlias.Rugby:
                case GamesAlias.BasketBall:
                case GamesAlias.Softball:
                    return PartialView("BasketBallWaterPolo/_EditGamePartial", gc);
                default:
                    return PartialView("_EditGamePartial", gc);
            }
        }

        // POST: GameCycle/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, GamesCycle gc)
        {
            return RedirectToAction("Edit", new { id = id });
        }

        // GET: GameCycle/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: GameCycle/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult StartGame(int id, int? departmentId = null)
        {
            GamesCycle editGc = gamesRepo.StartGame(id);
            return RedirectToAction(nameof(Edit), new { id, departmentId });
        }

        public ActionResult EndGame(int id, int? departmentId = null)
        {
            GamesCycle editGc = gamesRepo.EndGame(id);
            //temporary comment. In notification panel of samsung show swapped.
            /*
            if (editGc.GameSets.Count > 0 && editGc.IsPublished &&
                (editGc.Stage?.League?.Union?.Section?.Alias == GamesAlias.BasketBall || 
                    editGc.Stage?.League?.Union?.Section?.Alias == GamesAlias.WaterPolo ||
                    editGc.Stage?.League?.Union?.Section?.Alias == GamesAlias.Rugby || 
                    editGc.Stage?.League?.Union?.Section?.Alias == GamesAlias.Soccer))
            {
                NotesMessagesRepo notesRep = new NotesMessagesRepo();
                int homeScore = editGc.GameSets.Sum(x => x.HomeTeamScore);
                int guestScore = editGc.GameSets.Sum(x => x.GuestTeamScore);

                String message = editGc.HomeTeam.Title + homeScore + " - " + guestScore + editGc.GuestTeam.Title;
                if (editGc.HomeTeamId != null)
                {
                    notesRep.SendToTeam((int)editGc.Stage.League.SeasonId, (int)editGc.HomeTeamId, message, true);
                }
                if (editGc.GuestTeamId != null)
                {
                    notesRep.SendToTeam((int)editGc.Stage.League.SeasonId, (int)editGc.GuestTeamId, message, true);
                }

                var notsServ = new GamesNotificationsService();
                notsServ.SendPushToDevices(GlobVars.IsTest);
            }*/

            return RedirectToAction(nameof(Edit), new { id, departmentId });
        }

        public ActionResult UpdateGame(GamesCycle gc)
        {
            GamesCycle editGc = gamesRepo.GetGameCycleById(gc.CycleId);
            try
            {
                bool isChanged = false;
                if (gc.AuditoriumId != editGc.AuditoriumId)
                {
                    isChanged = true;
                    editGc.AuditoriumId = gc.AuditoriumId;
                }
                if (!editGc.StartDate.Equals(gc.StartDate))
                {
                    isChanged = true;
                    editGc.StartDate = gc.StartDate;
                }

                gamesRepo.Update(editGc);
                TempData["SavedId"] = editGc.CycleId;

                if (isChanged && editGc.IsPublished)
                {
                    NotesMessagesRepo notesRep = new NotesMessagesRepo();
                    if (editGc.Stage != null && editGc.Stage.League != null && editGc.Stage.League.SeasonId != null)
                    {
                        String message = String.Format("Game details has been updated: {0} vs {1}", editGc.HomeTeam != null ? editGc.HomeTeam.Title : "", editGc.GuestTeam != null ? editGc.GuestTeam.Title : "");

                        if (editGc.HomeTeamId != null)
                        {
                            notesRep.SendToTeam((int)editGc.Stage.League.SeasonId, (int)editGc.HomeTeamId, message, true);
                        }
                        if (editGc.GuestTeamId != null)
                        {
                            notesRep.SendToTeam((int)editGc.Stage.League.SeasonId, (int)editGc.GuestTeamId, message, true);
                        }
                        if (!string.IsNullOrEmpty(gc.RefereeIds))
                        {
                            var ids = editGc.RefereeIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                            notesRep.SendToUsers(ids, message);
                        }
                    }

                    var notsServ = new GamesNotificationsService();
                    notsServ.SendPushToDevices(GlobVars.IsTest);
                }
            }
            catch (Exception e) { }
            return RedirectToAction("Game", editGc);
        }
        public void SaveSpectators(List<string> spectatorIds, int cycleId)
        {
            string spectatorIdsString = "";

            if (spectatorIds.Count != 0 || spectatorIds != null)
                spectatorIdsString = String.Join(",", spectatorIds.ToArray());

            gamesRepo.SaveSpectatorsByCycleId(spectatorIdsString, cycleId);
        }

        public ActionResult Game(GamesCycle gc)
        {
            GamesCycle editGc = gamesRepo.GetGameCycleById(gc.CycleId);

            var leagueId = editGc.Stage.LeagueId;
            var unionId = editGc.Stage.League.UnionId;
            var seasonId = editGc.Stage.League.SeasonId;
            if (unionId != null)
            {
                var referees = unionId.HasValue ? uRepo.GetUnionAndLeagueReferees(unionId.Value, leagueId) : new List<User>();
                var spectators = unionId.HasValue ? uRepo.GetUnionAndLeagueSpectators(unionId.Value, leagueId) : new List<User>();

                ViewBag.Referees = referees.Union(spectators);
                ViewBag.Spectators = spectators.Union(referees);

                var aRepo = new AuditoriumsRepo();
                if (seasonId.HasValue)
                {
                    //if season id has value get all arena's by season id
                    ViewBag.Auditoriums = aRepo.GetByUnionAndSeason(unionId.Value, seasonId.Value);
                }
                else
                {
                    ViewBag.Auditoriums = aRepo.GetAll(unionId.Value);
                }

            }
            return PartialView("_Game", editGc);
        }

        public ActionResult AddWaterPoloBasketBallSet(GameSet set)
        {
            gamesRepo.AddGameSet(set);
            gamesRepo.UpdateBasketBallWaterPoloScore(set.GameCycleId);
            return GameSetList(set.GameCycleId);
        }

        public ActionResult AddGameSet(GameSet set)
        {
            gamesRepo.AddGameSet(set);
            return GameSetList(set.GameCycleId);
        }

        public ActionResult UpdateGameSet(GameSet set, bool needPenalties = false)
        {
            gamesRepo.UpdateGameSet(set);
            return RedirectToAction("GameSetList", new { id = set.GameCycleId });
        }

        public PartialViewResult DeleteLastGameSet(int id)
        {
            GamesCycle gc = gamesRepo.GetGameCycleById(id);
            var lastSet = gc.GameSets.OrderBy(c => c.SetNumber).LastOrDefault();
            gamesRepo.DeleteSet(lastSet);
            return GameSetList(id);
        }

        public ActionResult UpdateGameResults(int id, bool isWaterpoloOrBasketball, int? departmentId = null)
        {
            GamesCycle gc = gamesRepo.GetGameCycleById(id);
            if (gc.GameStatus != GameStatus.Ended || gc.Group.GamesType.TypeId == 1 /* Division */)
            {
                var gameAlias = gamesRepo.GetSectionAlias(gc);
                var isPenaltySections = gameAlias.Equals(GamesAlias.WaterPolo, StringComparison.OrdinalIgnoreCase)
                    || gameAlias.Equals(GamesAlias.Soccer, StringComparison.OrdinalIgnoreCase)
                    || gameAlias.Equals(GamesAlias.Rugby, StringComparison.OrdinalIgnoreCase)
                    || gameAlias.Equals(GamesAlias.Handball, StringComparison.OrdinalIgnoreCase);

                if (isPenaltySections)
                {
                    gamesRepo.UpdateGameScoreForPenaltySections(gc);
                    gamesRepo.Save();
                }
                else if (isWaterpoloOrBasketball && !isPenaltySections)
                {
                    gamesRepo.UpdateBasketBallWaterPoloScore(id);
                }
                else
                {
                    gamesRepo.UpdateGameScore(gc);
                }
            }
            else
            {   //  If game status need to be reset after game has been ended
                //  then further games in knockout or playoff group need to be
                //  rescheduled
                gamesRepo.EndGame(gc);
            }
            return RedirectToAction(nameof(Edit), new { id, departmentId });
        }

        public PartialViewResult GameSetList(int id)
        {
            GamesCycle gc = gamesRepo.GetGameCycleById(id);
            List<GameSet> list = gc.GameSets.ToList();

            var alias = gc.Stage?.League?.Union?.Section?.Alias ?? gc.Stage?.League?.Club?.Section?.Alias ?? gc.Stage?.League?.Union?.Section?.Alias;
            ViewBag.NeedPenalties = alias.Equals(GamesAlias.WaterPolo, StringComparison.OrdinalIgnoreCase)
                || alias.Equals(GamesAlias.Soccer, StringComparison.OrdinalIgnoreCase)
                || alias.Equals(GamesAlias.Rugby, StringComparison.OrdinalIgnoreCase)
                || alias.Equals(GamesAlias.Handball, StringComparison.OrdinalIgnoreCase);
            ViewBag.IsSoftBall = alias == GamesAlias.Softball;
            switch (alias)
            {
                case GamesAlias.WaterPolo:
                case GamesAlias.Soccer:
                case GamesAlias.Rugby:
                case GamesAlias.BasketBall:
                case GamesAlias.Softball:
                    return PartialView("BasketBallWaterPolo/_GameSetList", list);
                default:
                    return PartialView("_GameSetList", list);
            }
        }

        public ActionResult TechnicalWin(int id, int teamId, int athleteId, int? departmentId = null)
        {
            if (athleteId == 0)
                _gamesService.SetTechnicalWinForGame(id, teamId);
            else
                _gamesService.SetTechnicalWinForAthletesGame(id, athleteId);

            return RedirectToAction(nameof(Edit), new { id, departmentId });
        }

        public ActionResult GameCycleComment(int id, String comments, int? departmentId = null)
        {
           _gamesService.SetGameCycleComments(id, comments);
            return RedirectToAction(nameof(Edit), new { id, departmentId });
        }


        public ActionResult ResetGame(int id, int? departmentId = null)
        {
            gamesRepo.ResetGame(id);
            return RedirectToAction(nameof(Edit), new { id, departmentId });
        }

        public ActionResult PotentialTeams(int id, int index)
        {
            BracketsRepo repo = new BracketsRepo();
            IEnumerable<Titled> list = repo.GetAllPotintialTeams(id, index);
            return PartialView("_PotentialTeams", list);
        }

        public ActionResult GetStatistics(int cycleId)
        {
            var gamesStatHelper = new GamesStaticticsService();

            ViewBag.HomeGameStatistic = gamesStatHelper.GetGamesStatistics(cycleId, "home");
            ViewBag.GuestGamesStatistic = gamesStatHelper.GetGamesStatistics(cycleId, "guest");

            return PartialView("BasketBallWaterPolo/_GameStatistics");
        }

        public ActionResult GetWaterpoloStatistics(int cycleId)
        {
            var gamesStatHelper = new GamesStaticticsService();

            ViewBag.HomeGameStatistic = gamesStatHelper.GetWGamesStatistics(cycleId, "home");
            ViewBag.GuestGamesStatistic = gamesStatHelper.GetWGamesStatistics(cycleId, "guest");

            return PartialView("BasketBallWaterPolo/_WaterpoloStatistics");
        }

        [HttpPost]
        public ActionResult UpdateWStatistics(PlayersStatisticsDTO stat)
        {
            var newGameStat = gamesRepo.UpdatePlayersWGameStat(stat);
            return PartialView("_PlayersWStatisticsRow", newGameStat);
        }

        [HttpPost]
        public ActionResult UpdateStatistics(PlayersStatisticsDTO stat)
        {
            var newGameStat = gamesRepo.UpdatePlayersGameStat(stat);
            return PartialView("_PlayersStatisticsRow", newGameStat);
        }

        public ActionResult EditTennisLeague(int id, int seasonId, bool isFromTeamPage = false)
        {
            var helper = new TennisLeagueHelper(db, id);
            var viewModel = helper.GetGames(seasonId);

            ViewBag.SeasonId = seasonId;
            ViewBag.IsFromTeam = isFromTeamPage;
            return PartialView("_EditTennisLeague", viewModel);
        }

        [HttpPost]
        public JsonResult RemoveGameReportFile(int id)
        {
            gamesRepo.RemoveGameCycleReportFile(id);
            return Json("Report File was deleted");
        }

        [HttpPost]
        public JsonResult UploadGameReportFile(int id)
        {
            var savePath = Server.MapPath(GlobVars.ContentPath + "/gamecycles/");

            var di = new DirectoryInfo(savePath);
            if (!di.Exists)
                di.Create();

            var gc = gamesRepo.GetGameCycleById(id);
            var gameAlias = gc?.Stage?.League?.Union?.Section?.Alias ?? gc?.Group?.Stage?.League?.Club?.Section?.Alias ?? "";

            HttpPostedFileBase file = Request.Files[0]; //Uploaded file
                                                        //Use the following properties to get file's name, size and MIMEType
            int fileSize = file.ContentLength;
            string fileName = "GameReport_" + id.ToString() + "_" + DateTime.Now.ToFileTime() + "." + file.FileName.Split('.')[1];
            string mimeType = file.ContentType;
            Stream fileContent = file.InputStream;
            //To save file, use SaveAs method
            file.SaveAs(savePath + fileName); //File will be saved in application root

            gamesRepo.SaveGameCycleReportFile(id, fileName);
            return Json(fileName);
        }

        [HttpPost]
        public ActionResult SaveTennisLeagueScores(TennisGameViewModel viewModel)
        {
            var helper = new TennisLeagueHelper(db, viewModel.GameCycleId);
            helper.CreateGame(viewModel);
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult EndTennisGame(int gameCycleId, int gameNumber)
        {
            var helper = new TennisLeagueHelper(db, gameCycleId);
            helper.EndGame(gameNumber);
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult ResetTennisGame(int gameCycleId, int gameNumber)
        {
            var helper = new TennisLeagueHelper(db, gameCycleId);
            helper.ResetTennisGame(gameNumber);
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult EndAndPublishGames(int gameCycleId)
        {
            var helper = new TennisLeagueHelper(db, gameCycleId);
            helper.EndAndPublishGames();
            helper.SaveChanges();
            return Json(new { CycleId = gameCycleId, homeScore = helper.GameCycle.HomeTeamScore, guestScore = helper.GameCycle.GuestTeamScore, gameStatus = helper.GameCycle.GameStatus });
            /*
            return RedirectToAction("List", "Schedules", new
            {
                id = helper.League.LeagueId,
                seasonId = helper.League.SeasonId,
                isChronological = TempData["IsChrono"],
                desOrder = false
            });
            */
        }

        [HttpPost]
        public ActionResult ResetAllTennisGames(int id, int seasonId)
        {
            var helper = new TennisLeagueHelper(db, id);
            helper.ResetAllGames();
            helper.SaveChanges();
            return RedirectToAction("List", "Schedules", new
            {
                id = helper.League.LeagueId,
                seasonId = helper.League.SeasonId,
                isChronological = TempData["IsChrono"],
                desOrder = false
            });
        }
    }
}