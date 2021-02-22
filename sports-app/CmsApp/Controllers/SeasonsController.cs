using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AppModel;
using CmsApp.Models;
using DataService;
using Resources;

namespace CmsApp.Controllers
{
    public class SeasonsController : AdminController
    {
        private readonly SeasonsRepo _seasonsRepository;
        private readonly LeagueRepo _leagueRepo;
        private readonly ActivityRepo _activityRepo;

        public SeasonsController()
        {
            _seasonsRepository = new SeasonsRepo();
            _leagueRepo = new LeagueRepo();
            _activityRepo = new ActivityRepo();
        }

        [HttpGet]
        public ActionResult List(int entityId, LogicaName logicalName, int? seasonId)
        {
            var viewModel = new SeasonViewModel
            {
                LogicalName = logicalName,
                EntityId = entityId,
                SeasonId = seasonId
            };

            switch (logicalName)
            {
                case LogicaName.Union:
                    viewModel.Seasons = _seasonsRepository.GetSeasonsByUnion(entityId, User.IsInAnyRole(AppRole.Admins));
                    break;
                case LogicaName.Club:
                    viewModel.Seasons = _seasonsRepository.GetClubsSeasons(entityId, User.IsInAnyRole(AppRole.Admins));
                    break;
            }

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);

            return PartialView("_List", viewModel);
        }

        [HttpGet]
        public ActionResult Create(int entityId, LogicaName logicalName, int? seasonId)
        {
            var currentSeason = getCurrentSeason();

            if ((logicalName == LogicaName.Union || logicalName == LogicaName.Club) &&
                currentSeason != null)
            {
                ViewBag.Leagues = _leagueRepo.GetLeaguesBySesonUnion(entityId, currentSeason.Id);
            }

            var model = new CreateSeason
            {
                EntityId = entityId,
                RelevantEntityLogicalName = logicalName
            };

            return PartialView("_CreateSeason", model);
        }

        [HttpPost]
        public ActionResult SetActive(int id, bool value)
        {
            var season = seasonsRepository.GetCollection<Season>(x => x.Id == id).FirstOrDefault();

            if (season != null)
            {
                var entityId = season.UnionId ?? season.ClubId;
                var entityName = season.UnionId != null ? LogicaName.Union : LogicaName.Club;
                var previousSeason = season.Union?.Seasons?.Where(x => x.IsActive)?.OrderBy(x => x.Id)?.LastOrDefault(x => x.Id != id) ??
                                     season.Club?.Seasons?.Where(x => x.IsActive)?.OrderBy(x => x.Id)?.LastOrDefault(x => x.Id != id);

                if (entityName == LogicaName.Union)
                {
                    SetUnionCurrentSeason(previousSeason?.Id ?? 0);
                }
                else
                {
                    SetClubCurrentSeason(previousSeason?.Id ?? 0);
                }

                season.IsActive = value;

                seasonsRepository.Save();

                return RedirectToAction("List", new { entityId, logicalName = entityName, seasonId = previousSeason?.Id });
            }

            return Content("Season not found");
        }

        [HttpPost]
        public ActionResult Create(CreateSeason model)
        {
            if (ModelState.IsValid)
            {
                var newSeason = new Season
                {
                    Name = model.Name,
                    StartDate = model.StartDate.Value,
                    EndDate = model.EndDate.Value,
                    Description = model.Description,
                    IsActive = true,
                    SeasonForAge = model.SeasonForAge
                };

                switch (model.RelevantEntityLogicalName)
                {
                    case LogicaName.Union:
                        newSeason.UnionId = model.EntityId;

                        var lastSeasonId = _seasonsRepository.GetLastSeasonByCurrentUnionId(model.EntityId);
                        newSeason.PreviousSeasonId = lastSeasonId <= 0
                            ? (int?) null
                            : lastSeasonId;
                        _seasonsRepository.Create(newSeason);
                        _seasonsRepository.Save();

                        var leaguesToDuplicate = model.Leagues;
                        if (model.IsDuplicate == true)
                        {
                            _seasonsRepository.Duplicate(model.EntityId, leaguesToDuplicate ?? new int[0], lastSeasonId, newSeason.Id);
                        }

                        _activityRepo.CreateActivityForUnionSeason(model.EntityId, newSeason.Id, model.IsDuplicate.GetValueOrDefault(false), leaguesToDuplicate);

                        var union = unionsRepo.GetById(model.EntityId);

                        foreach (var unionClub in union.Clubs)
                        {
                            _activityRepo.CreateActivityForClubSeason(unionClub.ClubId, newSeason.Id);
                        }

                        return RedirectToAction("Edit", "Unions", new { id = model.EntityId });

                    case LogicaName.Club:
                        newSeason.ClubId = model.EntityId;
                        var lastClubSeason = _seasonsRepository.GetLastSeasonIdByCurrentClubId(model.EntityId);

                        _seasonsRepository.Create(newSeason);
                        _seasonsRepository.Save();

                        if (model.IsDuplicate == true)
                        {
                            _seasonsRepository.DuplicateClubEntities(model.EntityId, lastClubSeason, newSeason.Id);
                        }

                        _activityRepo.CreateActivityForClubSeason(model.EntityId, newSeason.Id);

                        var club = clubsRepo.GetById(model.EntityId);
                        var clubsDepartaments = club?.RelatedClubs;
                        if (clubsDepartaments != null && clubsDepartaments.Count > 0)
                        {
                            foreach (var departament in clubsDepartaments)
                            {
                                _activityRepo.CreateActivityForClubSeason(departament.ClubId, newSeason.Id);
                            }
                        }

                        var section = secRepo.GetByClubId(model.EntityId);

                        if (section != null && section.Alias == SectionAliases.MultiSport)
                        {
                            return RedirectToAction("Edit", "Clubs", new { id = model.EntityId, sectionId = section.SectionId, seasonId = newSeason.Id });
                        }
                        else
                        {
                            return RedirectToAction("Edit", "Clubs", new { id = model.EntityId, seasonId = newSeason.Id });
                        }
                }
            }

            return PartialView("_CreateSeason", model);
        }

        [HttpGet]
        public ActionResult GetSeason(int seasonId, int unionId)
        {
            var season = _seasonsRepository.Get(seasonId, unionId);

            var mappedSeason = new Season
            {
                Description = season.Description,
                StartDate = season.StartDate,
                EndDate = season.EndDate
            };

            return Json(mappedSeason, JsonRequestBehavior.AllowGet);
        }
    }
}
