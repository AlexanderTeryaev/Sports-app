using AppModel;
using DataService;
using DataService.DTO;
using DataService.LeagueRank;
using LogLigFront.Helpers;
using LogLigFront.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace LogLigFront.Controllers
{
    public class LeagueRankController : Controller
    {

        private readonly DataEntities db = new DataEntities();
        private readonly PlayersRepo _playersRepo;
        private readonly ClubsRepo _clubsRepo;
        private readonly LeagueRepo _leagueRepo;
        private readonly UnionsRepo _unionsRepo;
        private readonly TeamsRepo _teamsRepo;
        private readonly DisciplinesRepo _disciplinesRepo;


        public LeagueRankController()
        {
            _playersRepo = new PlayersRepo(db);
            _clubsRepo = new ClubsRepo(db);
            _leagueRepo = new LeagueRepo(db);
            _unionsRepo = new UnionsRepo(db);
            _teamsRepo = new TeamsRepo(db);
            _disciplinesRepo = new DisciplinesRepo(db);
        }

        public ActionResult TennisUnionRanks(int? unionId, int? ageId, int seasonId, int? clubId)
        {
            if (ageId.HasValue)
            {
                var rankList = _teamsRepo.GetTennisUnionRanks(unionId, ageId, clubId, seasonId);
                UnionTennisRankForm result = new UnionTennisRankForm();
                result.RankList = rankList;
                result.UnionId = unionId;
                result.ClubId = clubId;
                result.SeasonId = seasonId;
                result.CompetitionAgeId = ageId;
                result.ListAges = new SelectList(_teamsRepo.GetCompetitionAges(unionId.Value, seasonId), "id", "age_name", ageId);

                return View(result);
            }
            else
            {
                UnionTennisRankForm result = new UnionTennisRankForm();
                result.RankList = new List<UnionTennisRankDto>();
                result.UnionId = unionId;
                result.ClubId = clubId;
                result.SeasonId = seasonId;
                result.CompetitionAgeId = ageId;
                result.ListAges = new SelectList(_teamsRepo.GetCompetitionAges(unionId.Value, seasonId), "id", "age_name", ageId);

                return View(result);
            }
        }

        private RankCategory UpdateRankCategory(int categoryId, int seasonId)
        {
            CategoryRankService svc = new CategoryRankService(categoryId);
            RankCategory rCategory = svc.CreateCategoryRankTable(seasonId);

            if (rCategory == null)
            {
                rCategory = new RankCategory();
            }
            else if (rCategory.Stages.Count == 0)
            {
                rCategory = svc.CreateEmptyRankTable(seasonId);
                rCategory.IsEmptyRankTable = true;

                if (rCategory.Stages.Count == 0)
                {
                    rCategory.Players = db.TeamsPlayers.Where(x => x.TeamId == categoryId && x.SeasonId == seasonId).ToList();
                }
            }


            return rCategory;
        }

        public ActionResult AthleticsCupStanding(int id, int seasonId, bool isModal = false)
        {
            var rankings = Caching.GetObjectFromCache<List<List<CompetitionClubRankedStanding>>>(CacheKeys.CompetitionClubsRankingsKey + id.ToString() + seasonId.ToString());
            if (rankings == null)
            {
                rankings = _disciplinesRepo.CupClubRanks(id, seasonId);

                for (int i = 0; i < rankings.Count; i++)
                {
                    var table = rankings[i];
                    foreach (var rank in table)
                    {
                        rank.Points += rank.Correction;
                    }
                    table = table.OrderByDescending(t => t.Points).ToList();
                    rankings[i] = table;
                }
                Caching.SetObjectInCache(CacheKeys.CompetitionClubsRankingsKey + id.ToString() + seasonId.ToString(), 30, rankings);
            }

            var competition = _leagueRepo.GetById(id);


            ViewBag.CompetitionName = competition.Name;
            ViewBag.CompetitionDate = competition.LeagueStartDate;
            string logo = competition.Union.Logo;
            ViewBag.Logo = logo;

            if (isModal)
            {
                return PartialView("_AthleticsRankTable", rankings);
            }
            else
                return View("_AthleticsRankTable", rankings);

        }

        public ActionResult AthleticsLeagueCompetitionRanking(int id, int seasonId, bool isField = false, bool isModal = false)
        {
            List<IGrouping<Tuple<int?, int>, CompetitionClubsCorrection>> grouped = new List<IGrouping<Tuple<int?, int>, CompetitionClubsCorrection>>();
            ViewBag.IsFieldCompetition = isField;
            var competition = _leagueRepo.GetById(id);
            if (competition.IsFieldCompetition)
            {
                ViewBag.FieldRaceTables = _leagueRepo.GetFieldRaceTables(competition);
            }
            var isGoldenSpike = false;
            if (!isField)
            {
                var data = db.CompetitionClubsCorrections.Where(c => c.LeagueId == id && c.SeasonId == seasonId).ToList();
                grouped = data
                        .OrderBy(x => x.TypeId)
                        .ThenByDescending(x => x.GenderId)
                        .ThenByDescending(x => x.FinalScore)
                        .GroupBy(c => new Tuple<int?, int>(c.TypeId, c.GenderId))
                        .ToList();
                var competitionDisciplines = _disciplinesRepo.GetCompetitionDisciplines(id).Select(c => new CompetitionDisciplineDto
                {
                    SectionAlias = c.League.Union.Section.Alias,
                    DisciplineId = c.DisciplineId

                }).ToList();
                if (competitionDisciplines.FirstOrDefault()?.SectionAlias == GamesAlias.Athletics)
                {
                    foreach (var competitionDiscipline in competitionDisciplines)
                    {
                        if (competitionDiscipline.DisciplineId.HasValue)
                        {
                            var discipline = _disciplinesRepo.GetById(competitionDiscipline.DisciplineId.Value);
                            if (!string.IsNullOrWhiteSpace(discipline.DisciplineType) && (discipline.DisciplineType == "GoldenSpikesU14" || discipline.DisciplineType == "GoldenSpikesU16"))
                            {
                                isGoldenSpike = true;
                                break;
                            }
                        }
                    }
                }
            }

            ViewBag.CompetitionName = competition.Name;
            ViewBag.CompetitionDate = competition.LeagueStartDate;
            string logo = competition.Union.Logo;
            ViewBag.Logo = logo;
            ViewBag.isGoldenSpike = isGoldenSpike;
            ViewBag.LeagueId = id;
            ViewBag.SeasonId = competition.SeasonId;

            if (isModal)
            {
                return PartialView("_AthleticsLeagueCompetitionRanking", grouped);
            }

            return View("_AthleticsLeagueCompetitionRanking", grouped);
        }



        public ActionResult AthleticsCombinedRanking(int id, int seasonId, bool isModal = false)
        {
            var competition = _leagueRepo.GetById(id);

            var competitionDisciplines = _disciplinesRepo.GetCompetitionDisciplines(id).Select(c => new CompetitionDisciplineDto
            {
                SectionAlias = c.League.Union.Section.Alias,
                DisciplineId = c.DisciplineId
            }).ToList();
            var isGoldenSpike = 0;
            if (competitionDisciplines.FirstOrDefault()?.SectionAlias == GamesAlias.Athletics)
            {
                foreach (var competitionDiscipline in competitionDisciplines)
                {
                    if (competitionDiscipline.DisciplineId.HasValue)
                    {
                        var discipline = _disciplinesRepo.GetById(competitionDiscipline.DisciplineId.Value);
                        if (!string.IsNullOrWhiteSpace(discipline.DisciplineType) && discipline.DisciplineType == "GoldenSpikesU14")
                        {
                            isGoldenSpike = 1;
                            break;
                        }
                        if (!string.IsNullOrWhiteSpace(discipline.DisciplineType) && discipline.DisciplineType == "GoldenSpikesU16")
                        {
                            isGoldenSpike = 2;
                            break;
                        }
                    }
                }
            }

            var combinedPlayerRanks = _leagueRepo.GetAthleticCombinedRanking(competition, isGoldenSpike);

            var competitionsDisciplines = competition.CompetitionDisciplines.Where(cd => cd.IsMultiBattle && !cd.IsDeleted);
            var multiBattleCompetitionsDisciplines = competitionsDisciplines.OrderBy(a => db.Disciplines.FirstOrDefault(t => t.DisciplineId == a.DisciplineId), new LeagueRepo.CombinedDisciplineComparer(isGoldenSpike == 0 ? competitionsDisciplines.Count() : -1));
            var disciplinesNameListForMen = new List<string>();
            var disciplinesNameListForWomen = new List<string>();
            foreach (var multiBattleCompetitionsDiscipline in multiBattleCompetitionsDisciplines)
            {
                var discipline = db.Disciplines.FirstOrDefault(d => d.DisciplineId == multiBattleCompetitionsDiscipline.DisciplineId);
                var name = discipline.Name;
                if (multiBattleCompetitionsDiscipline.CompetitionAge.gender == 1)
                {
                    disciplinesNameListForMen.Add(name);
                }
                else
                {
                    disciplinesNameListForWomen.Add(name);
                }
            }
            ViewBag.MultiBattleDisciplinesNameListForMen = disciplinesNameListForMen;
            ViewBag.MultiBattleDisciplinesNameListForWomen = disciplinesNameListForWomen;

            ViewBag.CompetitionName = competition.Name;
            ViewBag.CompetitionDate = competition.LeagueStartDate;
            string logo = competition.Union.Logo;
            ViewBag.Logo = logo;
            ViewBag.SeasonId = seasonId;
            
            if (isModal)
            {
                return PartialView("_AthleticsCombinedRanking", combinedPlayerRanks);
            }

            return View("_AthleticsCombinedRanking", combinedPlayerRanks);
        }

        public ActionResult AthleticsLeagueStandings(int id, bool isModal = false)
        {
            var athleticLeague = db.AthleticLeagues.Find(id);
            var competitionsOfLeague = db.Leagues.Where(x => x.AthleticLeagueId == id).ToList();

            if (athleticLeague == null || !competitionsOfLeague.Any())
            {
                return new EmptyResult();
            }

            ViewBag.Header = athleticLeague.Name;
            ViewBag.SubHeader = string.Empty;

            var competitionsIds = competitionsOfLeague.Select(x => x.LeagueId).ToArray();

            var results = new List<AthleticsLeagueStandingModel>();

            var data = db.CompetitionClubsCorrections
                .Include(x => x.Club)
                .Where(x => x.TypeId != null &&
                            competitionsIds.Contains(x.LeagueId) &&
                            x.SeasonId == athleticLeague.SeasonId)
                .AsNoTracking()
                .ToList();

            foreach (var competitionData in data)
            {
                var existing = results.FirstOrDefault(x => x.TypeId == competitionData.TypeId &&
                                                           x.GenderId == competitionData.GenderId &&
                                                           x.ClubId == competitionData.ClubId);

                if (existing == null)
                {
                    results.Add(new AthleticsLeagueStandingModel
                    {
                        ClubId = competitionData.ClubId,
                        ClubName = competitionData.Club.Name,
                        TypeId = competitionData.TypeId,
                        GenderId = competitionData.GenderId,
                        FinalScore = competitionData.FinalScore
                    });
                }
                else
                {
                    existing.FinalScore += competitionData.FinalScore;
                }
            }

            var viewModel = results
                .OrderBy(x => x.TypeId)
                .ThenByDescending(x => x.GenderId)
                .ThenByDescending(x => x.FinalScore)
                .GroupBy(c => new Tuple<int?, int>(c.TypeId, c.GenderId))
                .ToList();

            if (isModal)
            {
                return PartialView("_AthleticsLeagueStandings", viewModel);
            }

            return View("_AthleticsLeagueStandings", viewModel);
        }

        private IEnumerable<TennisPlayoffRank> UpdateTennisPlayoffRank(int categoryId, int seasonId)
        {
            var svc = new CategoryRankService(categoryId);
            return svc.CreatePlayoffEmptyTable(seasonId, out bool hasPlayoff).OrderBy(c => c.Rank);
        }
    }
}