using System;
using System.Linq;
using System.Web.Mvc;
using AppModel;
using CmsApp.Helpers;
using CmsApp.Models;
using DataService;

namespace CmsApp.Controllers
{
    public class StagesController : AdminController
    {
        private readonly StagesRepo _stageRepo = new StagesRepo();
        private readonly GamesRepo _gamesRepo = new GamesRepo();

        [HttpPost]
        public ActionResult UpdateStage(UpdateStageModel model)
        {
            var stage = stagesRepo.GetById(model.StageId);

            if (stage != null)
            {
                stage.Name = model.StageName;

                stagesRepo.Save();
            }

            return new EmptyResult();
        }

        [HttpPost]
        public void EnableRankedStanding(int id, bool value)
        {
            var stage = stagesRepo.GetById(id);
            if (stage != null)
            {
                stage.RankedStandingsEnabled = value;

                stagesRepo.Save();
            }
        }

        // GET: Stages
        public ActionResult Index(int id, int? seasonId, int? departmentId = null)
        {
            ViewBag.SeasonId = seasonId;
            ViewBag.IsDepartmentLeague = departmentId.HasValue;
            ViewBag.DepartmentId = departmentId;
            var resList = _stageRepo.GetAll(id);

            var season = seasonsRepository.GetById(seasonId ?? 0);
            var sectionAlias = season?.Union?.Section?.Alias;

            ViewBag.IsRugby = string.Equals(sectionAlias, SectionAliases.Rugby,
                StringComparison.CurrentCultureIgnoreCase);

            CrossesStageHelper.SetNameForCrossesStages(resList);

            return PartialView("_List", resList.OrderBy(x => x.Number));
        }

        public ActionResult TennisIndex(int leagueId, int id, int? seasonId, int? departmentId = null)
        {
            ViewBag.SeasonId = seasonId;
            ViewBag.IsDepartmentLeague = departmentId.HasValue ? true : false;
            ViewBag.DepartmentId = departmentId;
            ViewBag.LeagueId = leagueId;
            ViewBag.CategoryId = id;
            var resList = _stageRepo.GetAllTennisStages(id, seasonId);
            return PartialView("_TennisList", resList.ToList());
        }

        public ActionResult Create(int id, int seasonId, int? departmentId = null)
        {
            var stage = _stageRepo.Create(id);
            
            _stageRepo.Save();

            if (departmentId.HasValue)
            {
                return RedirectToAction("Index", new { id = id, seasonId, departmentId = departmentId });
            }
            else
            {
                return RedirectToAction("Index", new { id = id, seasonId });
            }

            //var model = new GameForm();
            //model.DaysList = Messages.WeekDays.Split(',');
            //model.StartDate = DateTime.Now;
            //model.StageId = stage.StageId;
            //model.LeagueId = stage.LeagueId;
            //return PartialView("_Settings", model);
        }

        public ActionResult CreateTennisStage(int leagueId, int id, int seasonId, int? departmentId = null)
        {
            var stage = _stageRepo.CreateTennisStage(id, seasonId);

            _stageRepo.Save();

            if (departmentId.HasValue)
            {
                return RedirectToAction("TennisIndex", new { leagueId = leagueId, id = id, seasonId, departmentId = departmentId });
            }
            else
            {
                return RedirectToAction("TennisIndex", new { leagueId = leagueId, id = id, seasonId });
            }

            //var model = new GameForm();
            //model.DaysList = Messages.WeekDays.Split(',');
            //model.StartDate = DateTime.Now;
            //model.StageId = stage.StageId;
            //model.LeagueId = stage.LeagueId;
            //return PartialView("_Settings", model);
        }

        public ActionResult Delete(int id, int? seasonId, int? departmentId = null)
        {

            _stageRepo.DeleteAllGameCycles(id);
            _stageRepo.DeleteAllGroups(id);
            var st = _stageRepo.GetById(id);
            st.IsArchive = true;
            _stageRepo.Save();

            ViewBag.DepartmentId = departmentId;
            ViewBag.IsDepartmentLeague = departmentId.HasValue ? true : false;

            if (departmentId.HasValue)
            {
                return RedirectToAction("Index", new { id = st.LeagueId, seasonId, departmentId = departmentId });

            }
            else
            {
                return RedirectToAction("Index", new { id = st.LeagueId, seasonId });
            }
        }

        public ActionResult DeleteTennis(int id, int? seasonId, int? departmentId = null)
        {

            _stageRepo.DeleteAllTennisGameCycles(id);
            _stageRepo.DeleteAllTennisGroups(id);
            var st = _stageRepo.GetTennisStageById(id);
            st.IsArchive = true;
            _stageRepo.Save();

            ViewBag.DepartmentId = departmentId;
            ViewBag.IsDepartmentLeague = departmentId.HasValue ? true : false;
            int leagueId = leagueRepo.GetByTeamAndSeason(st.CategoryId, seasonId.Value).FirstOrDefault().LeagueId;

            if (departmentId.HasValue)
            {
                return RedirectToAction("TennisIndex", new { leagueId = leagueId, id = st.CategoryId, seasonId, departmentId = departmentId });
            }
            else
            {
                return RedirectToAction("TennisIndex", new { leagueId = leagueId, id = st.CategoryId, seasonId });
            }
        }

        public ActionResult CreateSetting(GameForm frm, string[] daysArr)
        {
            var item = new Game();
            item.StageId = frm.StageId;
            item.SortDescriptors = "0,1,2";
            item.GameDays = string.Join(",", daysArr);
            item.StartDate = frm.StartDate;
            _gamesRepo.Create(item);
            UpdateModel(item);
            _gamesRepo.Save();
            TempData["Id"] = frm.LeagueId;
            TempData["Close"] = "close";
            return PartialView("_Settings", frm);
        }
    }
}