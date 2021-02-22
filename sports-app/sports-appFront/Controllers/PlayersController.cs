using DataService;
using DataService.DTO;
using LogLigFront.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LogLigFront.Controllers
{
    public class PlayersController : CommonController
    {

        #region Fields & constructor
        private readonly TeamsRepo _teamsRepo;
        private readonly UnionsRepo _unionsRepo;
        private readonly SectionsRepo _sectionsRepo;
        private readonly JobsRepo _jobsRepo;
        private readonly PlayersRepo _playersRepo;
        private GamesRepo _gamesRepo;
        private LeagueRepo _leagueRepo;
        private ClubsRepo _clubRepo;
        private DisciplinesRepo _disciplinesRepo;
        private SeasonsRepo _seasonsRepo;
        public PlayersController()
        {
            _teamsRepo = new TeamsRepo();
            _unionsRepo = new UnionsRepo();
            _sectionsRepo = new SectionsRepo();
            _leagueRepo = new LeagueRepo();
            _jobsRepo = new JobsRepo();
            _playersRepo = new PlayersRepo();
            _disciplinesRepo = new DisciplinesRepo();
            _clubRepo = new ClubsRepo();
            _gamesRepo = new GamesRepo();
            _seasonsRepo = new SeasonsRepo();
        }
        #endregion

        public ActionResult Details(int id, int? seasonId, string tab)
        {
            var user = _playersRepo.GetUserByUserId(id);
            var player = new ExternalPlayerModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Gender = user.GenderId ?? 4,
                BirthDate = user.BirthDay,
                ProfileImage = user.Image
            };

            if (seasonId.HasValue)
            {
                var season = _seasonsRepo.GetById(seasonId.Value);
                var union = season.Union;
                player.SectionAlias = union?.Section?.Alias;
                var athleteCompetitions = _gamesRepo.GetAthleteCompetitionsAchievements(id)
                    ?.FirstOrDefault(x => x.SeasonId == seasonId.Value)?.Achievements;
                var athleteSeasonalBests = new List<AthleteCompetitionAchievementViewItem>();
                var teamPlayers = _playersRepo.GetTeamPlayerByUserIdAndSeasonIdClubCheck(id, seasonId.Value);
                if (teamPlayers?.Club != null)
                {
                    player.ClubId = teamPlayers.ClubId;
                    player.ClubName = teamPlayers.Club.Name;
                }
                if (athleteCompetitions != null)
                {
                    var disiplineGroupedRegs = athleteCompetitions.OrderBy(r => r.CompetitionStartDate ?? DateTime.MaxValue)
                        .GroupBy(r => r.DisciplineName).ToList();
                    foreach (var disiplineGroupedReg in disiplineGroupedRegs)
                    {
                        var format = disiplineGroupedReg.First().Format;
                        var registrations = disiplineGroupedReg.Where(r =>
                                (!r.Wind.HasValue || r.Wind.Value <= 2.00) &&
                                !(!r.Wind.HasValue && GamesRepo.disciplineTypesThatUsesWinds.Contains(r.DisciplineType)))
                            .ToList();

                        if (format != null && ((format.Value >= 6 && format.Value <= 8) || format.Value == 10 ||
                                               format.Value == 11))
                        {
                            var resulted = registrations
                                .Where(x => !string.IsNullOrWhiteSpace(x.Result) && x.AlternativeResult == 0)
                                .OrderByDescending(x => x.SortValue);
                            var top = resulted.Union(disiplineGroupedReg).ToList().FirstOrDefault();
                            if (top != null)
                            {
                                athleteSeasonalBests.Add(top);
                            }
                        }
                        else
                        {
                            var resulted = registrations
                                .Where(x => !string.IsNullOrWhiteSpace(x.Result) && x.AlternativeResult == 0)
                                .OrderBy(x => x.SortValue);
                            var res = resulted.Union(disiplineGroupedReg).ToList();
                            var top = resulted.Union(disiplineGroupedReg).ToList().FirstOrDefault();
                            if (top != null)
                            {
                                athleteSeasonalBests.Add(top);
                            }
                        }
                    }
                    ViewBag.AthleteAchievements = disiplineGroupedRegs;
                }

                athleteSeasonalBests = athleteSeasonalBests.OrderBy(x => x.DisciplineName).ToList();
                ViewBag.AthleteSeasonalBests = athleteSeasonalBests;
                ViewBag.Tab = tab;
                ViewBag.PlayerId = id;
                ViewBag.Seasons = _seasonsRepo.GetSeasonsByUnion(union?.UnionId ?? 0, false).ToList();
            }

            return View(player);
        }


        // GET: Players
        public ActionResult Index()
        {
            return View();
        }
    }
}