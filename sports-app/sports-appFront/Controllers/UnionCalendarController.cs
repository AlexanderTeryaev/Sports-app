using System;
using System.Collections.Generic;
using System.Web.Mvc;
using AppModel;
using DataService;
using DataService.DTO;
using DataService.Services;
using LogLigFront.Helpers;

namespace LogLigFront.Controllers
{
    public class UnionCalendarController : Controller
    {
        private readonly DataEntities db = new DataEntities();
        private readonly ClubsRepo _clubsRepo;
        private readonly UnionsRepo _unionsRepo;
        private readonly SectionsRepo _sectionsRepo;


        public UnionCalendarController()
        {
            _clubsRepo = new ClubsRepo(db);
            _unionsRepo = new UnionsRepo(db);
            _sectionsRepo = new SectionsRepo(db);
        }
        public ActionResult Calendar(int? unionId, int? seasonId, int? sectionId = null, int? clubId = null)
        {
            string sectionAlias = _sectionsRepo.GetById(sectionId ?? 0)?.Alias;
            var isMultiSport = !(String.IsNullOrEmpty(sectionAlias)) && sectionAlias == SectionAliases.MultiSport ? true : false;
            var isDepartment = false;
            if (clubId.HasValue)
            {
                var club = _clubsRepo.GetById(clubId.Value);
                isDepartment = club.ParentClub == null ? false : true;
                ViewBag.IsUnionClub = club?.IsUnionClub;
                ViewBag.IsIndividual =
                    club.IsUnionClub.Value ? club.Union.Section.IsIndividual : club.Section.IsIndividual;
            }
            ViewBag.UnionId = unionId;
            ViewBag.SeasonId = seasonId;
            ViewBag.IsMultiSport = isMultiSport;
            ViewBag.IsDepartment = isDepartment;
            ViewBag.ClubId = clubId;
            return PartialView("_Calendar");
        }

        public ActionResult CalendarObject(int unionId, int seasonId, int? clubId)
        {
            if (unionId != 0)
            {
                var unionCalendarService = new UnionGamesService(unionId, seasonId);
                var isIndividual = _unionsRepo.GetById(unionId)?.Section?.IsIndividual;
                IEnumerable<GameDto> games = unionCalendarService.GetAllGames(isIndividual);
                IEnumerable<GameDto> competitions = null;
                var union = _unionsRepo.GetById(unionId);
                if (union.Section.IsIndividual)
                {
                    competitions = unionCalendarService.GetAllCompetitions(union.Section.Alias == SectionAliases.Tennis);
                }

                var unionEvents = _unionsRepo.GetAllUnionEvents(unionId, true);
                var calendarHelper = new UnionCalendarHelper(games, competitions, unionEvents);
                var model = calendarHelper.GetCalendarObject(isIndividual, unionId);
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            else if (clubId.HasValue)
            {
                var unionCalendarService = new UnionGamesService(unionId, seasonId);
                IEnumerable<GameDto> games = unionCalendarService.GetAllClubGames(clubId.Value);
                var calendarHelper = new UnionCalendarHelper(games, null, null);
                var model = calendarHelper.GetClubsCalendarObject();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }
    }
}