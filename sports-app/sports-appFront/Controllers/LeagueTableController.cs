using AppModel;
using DataService;
using DataService.LeagueRank;
using LogLigFront.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DataService.DTO;
using LogLigFront.Helpers;
using System.Web;
using Resources;

namespace LogLigFront.Controllers
{
    public class LeagueTableController : Controller
    {
        #region Fields & constructor
        private readonly TeamsRepo _teamsRepo;
        private readonly UnionsRepo _unionsRepo;
        private readonly SectionsRepo _sectionsRepo;
        private readonly JobsRepo _jobsRepo;
        private readonly PlayersRepo _playersRepo;
        private readonly SeasonsRepo _seasonsRepo;
        private GamesRepo _gamesRepo = null;
        private LeagueRepo _leagueRepo;
        private ClubsRepo _clubRepo;
        private DisciplinesRepo _disciplinesRepo;

        public GamesRepo gamesRepo
        {
            get
            {
                if (_gamesRepo == null)
                {
                    _gamesRepo = new GamesRepo();
                }
                return _gamesRepo;
            }
        }

        public LeagueTableController()
        {
            _teamsRepo = new TeamsRepo();
            _unionsRepo = new UnionsRepo();
            _sectionsRepo = new SectionsRepo();
            _leagueRepo = new LeagueRepo();
            _jobsRepo = new JobsRepo();
            _playersRepo = new PlayersRepo();
            _disciplinesRepo = new DisciplinesRepo();
            _clubRepo = new ClubsRepo();
            _seasonsRepo = new SeasonsRepo();
        }
        #endregion

        // GET: LeagueTable
        public ActionResult Index(int id, int seasonId, int? union = null)
        {
            string sectionAlias;

            if (union.HasValue)
            {
                var section = _unionsRepo.GetSectionByUnionId(union.Value);
                sectionAlias = section.Alias;
            }
            else
            {
                var section = _sectionsRepo.GetByLeagueId(id);
                sectionAlias = section?.Alias;
            }

            var svc = new LeagueRankService(id);
            var rLeague = svc.CreateLeagueRankTable(seasonId);

            if (rLeague.Stages.Count == 0)
            {
                rLeague = svc.CreateEmptyRankTable(seasonId);
                rLeague.IsEmptyRankTable = true;
            }

            if (rLeague.Stages.Count == 0)
            {
                rLeague.Teams = _teamsRepo.GetTeams(seasonId, id).ToList();
            }
            rLeague.SeasonId = seasonId;

            rLeague.RankedStanding = svc.GetRankedStanding(seasonId);

            rLeague.Stages.Reverse(); 


            ViewBag.PlayoffTable = UpdatePlayoffRank(id, seasonId);
            ViewBag.SectionAlias = sectionAlias;

            switch (sectionAlias)
            {
                case GamesAlias.WaterPolo:
                case GamesAlias.Soccer:
                case GamesAlias.Rugby:
                case GamesAlias.Softball:
                    return View("Waterpolo/Index", rLeague);
                case GamesAlias.BasketBall:
                    return View("Basketball/Index", rLeague);

                case GamesAlias.NetBall:
                case GamesAlias.VolleyBall:
                    return View("Netball_VolleyBall/Index", rLeague);

                default:
                    return View(rLeague);

            }
        }

        private IEnumerable<PlayoffRank> UpdatePlayoffRank(int id, int seasonId)
        {
            var svc = new LeagueRankService(id);
            var result = svc.CreatePlayoffEmptyTable(seasonId, out bool hasPlayoff);

            if (TempData["IsPlayoff"] != null)
                TempData["IsPlayoff"] = null;

            TempData["IsPlayoff"] = hasPlayoff;
            return result;
        }

        public ActionResult Schedules(int id, string gameIds, int? seasonId = null)
        {
            var games = string.IsNullOrWhiteSpace(gameIds)
                ? new int[] { }
                : gameIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.Parse(s))
                    .ToArray();

            var resList = gamesRepo.GetSchedulesForExternalLink(id, seasonId, games);
            var lRepo = new LeagueRepo();
            var league = lRepo.GetById(id);

            ViewBag.SeasonId = seasonId;
            if (league != null)
            {
                ViewBag.LeagueId = league.LeagueId;
                ViewBag.Logo = UIHelper.GetLeagueLogo(league.Logo);
                ViewBag.ResTitle = league.Name.Trim();

                var alias = league.Union?.Section?.Alias ?? league.Club?.Section?.Alias;
                ViewBag.IsTennisLeagueGame = (league.EilatTournament == null || league.EilatTournament == false)
                    && alias.Equals(GamesAlias.Tennis);
                var showCycles = gamesRepo.GetGameSettings(id)?.ShowCyclesOnExternal ??
                    (alias.Equals(GamesAlias.NetBall) ? 0 : 1);
                resList.NeedShowCycles = showCycles == 1;
                resList.RoundStartCycle = gamesRepo.GetGameSettings(id)?.RoundStartCycle;
                ViewBag.ShouldShowRemarkColumn = resList.GameCycles.Any(x=>x.IsPublished && !string.IsNullOrEmpty(x.Remark));
                if (ViewBag.IsTennisLeagueGame == true)
                {
                    _teamsRepo.ChangeTeamNamesForTheTennis(resList);
                }
                switch (alias)
                {
                    case GamesAlias.WaterPolo:
                    case GamesAlias.Soccer:
                    case GamesAlias.Rugby:
                    case GamesAlias.BasketBall:
                    case GamesAlias.Softball:
                        return View("WaterPoloBasketBall/Schedules", resList);
                    default:
                        return View(resList);
                }
            }

            return View(resList);
        }

        public ActionResult SchedulesForTennisCompetition(int id, int seasonId)
        {
            var resList = gamesRepo.GetCompetitionSchedulesForExternalLink(id, seasonId);
            ViewBag.SeasonId = seasonId;
            var needShowCycles = gamesRepo.GetTennisGameSettings(id)?.ShowCyclesOnExternal ?? 1;
            var league = _leagueRepo.GetLeagueForCategory(id, seasonId);
            if (league != null)
            {
                var category = _teamsRepo.GetById(id, seasonId);
                var categoryName = category?.TeamsDetails?.OrderByDescending(t => t.Id)?.FirstOrDefault()?.TeamName
                    ?? category.Title;
                ViewBag.LeagueId = league.LeagueId;
                ViewBag.Logo = UIHelper.GetLeagueLogo(league.Logo);
                ViewBag.ResTitle = $"{league.Name.Trim()} / {categoryName}";
            }
            resList.NeedShowCycles = needShowCycles == 1;
            resList.RoundStartCycle = gamesRepo.GetTennisGameSettings(id)?.RoundStartCycle;
            return View("Tennis/CompetitionSchedules", resList);
        }

        public ActionResult AuditoriumSchedules(int id, int? seasonId = null)
        {
            var resList = gamesRepo.GetCyclesByAuditorium(id, seasonId);
            var aRepo = new AuditoriumsRepo();
            var aud = aRepo.GetById(id);
            ViewBag.AudTitle = aud.Name;
            ViewBag.AudAddress = aud.Address;
            ViewBag.SeasonId = seasonId;
            return View(resList);
        }

        public ActionResult TeamSchedulesLink(int id, int leagueId, int seasonId)
        {
            var resList = gamesRepo.GetCyclesByTeam(id).Where(g => g.LeagueId == leagueId);

            var team = _teamsRepo.GetById(id);
            ViewBag.TeamId = id;
            ViewBag.LeagueId = leagueId;
            ViewBag.SeasonId = seasonId;
            if (team != null)
            {
                ViewBag.Logo = UIHelper.GetTeamLogo(team.Logo);
                ViewBag.ResTitle = team.Title.Trim();
                if (team.LeagueTeams.Count > 1)
                {
                    var leagues = team.LeagueTeams.Where(l => (l.Leagues.Stages.Any(s => s.GamesCycles.Any(gc => gc.HomeTeamId == id && gc.IsPublished)) || l.Leagues.Stages.Any(s => s.GamesCycles.Any(gc => gc.GuestTeamId == id && gc.IsPublished))) && l.SeasonId == seasonId);
                    List<SelectListItem> items = new List<SelectListItem>();
                    foreach (var lg in leagues)
                    {
                        items.Add(new SelectListItem { Text = lg.Leagues.Name, Value = lg.LeagueId.ToString(), Selected = lg.LeagueId == leagueId });
                    }
                    ViewBag.Leagues = items;
                }

            }
            var lRepo = new LeagueRepo();
            var league = lRepo.GetById(leagueId);
            var alias = league.Union?.Section?.Alias ?? league?.Club?.Section?.Alias;

            ViewBag.IsTennisLeagueGame = (league.EilatTournament == null || league.EilatTournament == false)
                && alias.Equals(GamesAlias.Tennis);
            if (ViewBag.IsTennisLeagueGame == true)
            {
                _teamsRepo.ChangeTeamNamesForTheTennis(resList);
                ViewBag.ResTitle = _teamsRepo.GetTeamNameWithoutLeagueName(team.TeamId, league.LeagueId);
            }

            return View("SchedulesLink", new SchedulesDto
            {
                GameCycles = resList,
                gameAlias = alias
            });
        }

        public ActionResult TennisTeamRegistrationList(int id, int leagueId, int seasonId)
        {
            var registrations = _leagueRepo.GetAllTennisRegistrations(leagueId, seasonId, false).Where(c => c.TeamId == id)
                .OrderBy(t => t.TeamPositionOrder ?? int.MaxValue).ThenBy(c => c.FullName).ToList();

            ViewBag.LeagueId = leagueId;
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueName = _leagueRepo.GetById(id)?.Name;
            ViewBag.TeamId = id;
            ViewBag.TeamManager = _jobsRepo.GetTeamManagerInfo(id, seasonId);

            return PartialView("Tennis/_TennisTeamRegistrationTable", registrations);
        }

        public ActionResult GameSet(int id)
        {
            var gc = gamesRepo.GetGameCycleById(id);
            var alias = gc.Stage?.League?.Union?.Section?.Alias ?? gc.Stage?.League?.Club?.Section?.Alias;

            var resList = gc.GameSets.ToList();
            if (gc.Group.IsIndividual)
            {
                var homeTeamName = gc.TeamsPlayer1 != null
                    ? $"{gc.TeamsPlayer1.User.FullName} /{gc.TeamsPlayer1.Team.Title}"
                    : $"Competitor #{gc.HomeTeamPos}";
                var guestTeamName = gc.TeamsPlayer != null
                    ? $"{gc.TeamsPlayer.User.FullName} /{gc.TeamsPlayer.Team.Title}"
                    : $"Competitor #{gc.GuestTeamPos}";
                ViewBag.HomeTeam = homeTeamName;
                ViewBag.GuestTeam = guestTeamName;
            }
            else
            {
                ViewBag.HomeTeam = gc.HomeTeam.Title;
                ViewBag.GuestTeam = gc.GuestTeam.Title;
            }
            ViewBag.SectionAlias = alias;
            switch (alias)
            {
                case GamesAlias.WaterPolo:
                case GamesAlias.Soccer:
                case GamesAlias.Rugby:
                case GamesAlias.BasketBall:
                case GamesAlias.Softball:
                    return PartialView("WaterPoloBasketBall/_GameSet", resList);
                default:
                    return PartialView("_GameSet", resList);
            }
        }

        public ActionResult TennisGameSets(int id)
        {
            var tennisGame = gamesRepo.GetGameCycleById(id);
            var vm = GetTennisGameSets(tennisGame.TennisLeagueGames.OrderBy(g => g.GameNumber));
            ViewBag.HomeTeamName = _teamsRepo.GetTeamNameWithoutLeagueName(tennisGame.HomeTeamId, tennisGame.Stage.LeagueId);
            ViewBag.GuestTeamName = _teamsRepo.GetTeamNameWithoutLeagueName(tennisGame.GuestTeamId, tennisGame.Stage.LeagueId);
            return PartialView("Tennis/_GameSet", vm);
        }

        private IEnumerable<TennisGameViewModel> GetTennisGameSets(IEnumerable<TennisLeagueGame> tennisLeagueGames)
        {
            foreach (var game in tennisLeagueGames)
            {
                yield return new TennisGameViewModel
                {
                    GameId = game.Id,
                    GameNumber = game.GameNumber,
                    HomePlayerName = game.HomePlayer?.FullName,
                    HomePlayerId = game.HomePlayerId,
                    GuestPlayerName = game.GuestPlayer?.FullName,
                    GuestPlayerId = game.GuestPlayerId,
                    HomePlayerPairName = game.HomePairPlayer?.FullName,
                    GuestPlayerPairName = game.GuestPairPlayer?.FullName,
                    Sets = game.TennisLeagueGameScores
                        .Where(g => !(g.HomeScore == 0 && g.GuestScore == 0)),
                    TechnicalWinnerId = game.TechnicalWinnerId
                };
            }
        }

        public ActionResult PotentialTeams(int id, int index)
        {
            BracketsRepo repo = new BracketsRepo();
            IEnumerable<Titled> list = repo.GetAllPotintialTeams(id, index);
            return PartialView("_PotentialTeams", list);
        }

        [HttpGet]
        public ActionResult TennisLeagueDetails(int id, int seasonId)
        {
            LeagueRankService svc = new LeagueRankService(id);
            RankLeague rLeague = svc.CreateLeagueRankTable(seasonId, true);
            var league = _leagueRepo.GetById(id);

            if (rLeague.Stages.Count == 0)
            {
                rLeague = svc.CreateEmptyRankTable(seasonId);
                rLeague.IsEmptyRankTable = true;
            }

            if (rLeague.Stages.Count == 0)
            {
                rLeague.Teams = _teamsRepo.GetTeams(seasonId, id).ToList();
            }

            rLeague.SeasonId = seasonId;
            ViewBag.PlayoffTable = UpdatePlayoffRank(id, seasonId);
            return View("Tennis/Index", rLeague);
        }

        public ActionResult CompetitionGameSets(int id)
        {
            var tennisGame = gamesRepo.GetTennisGameCycleById(id);
            ViewBag.FirstPlayerName = tennisGame?.FirstPlayerPairId.HasValue == true
                ? $"{tennisGame?.FirstPlayer?.User?.FullName} / {tennisGame?.FirstPairPlayer?.User?.FullName}"
                : tennisGame?.FirstPlayer?.User?.FullName;
            ViewBag.SecondPlayerName = tennisGame?.SecondPlayerPairId.HasValue == true
                ? $"{tennisGame?.SecondPlayer?.User?.FullName} / {tennisGame?.SecondPairPlayer?.User?.FullName}"
                : tennisGame?.SecondPlayer?.User?.FullName;
            return PartialView("Tennis/_CompetitionGameSet", tennisGame);
        }


        public ActionResult WeightLiftingCompetitions(int id, int seasonId, bool? isPast)
        {
            return AthleticCompetitions(id, seasonId, isPast);
        }

        public ActionResult AthleticCompetitions(int id, int seasonId, bool? isPast, int? type = null) {
            var union = _unionsRepo.GetById(id);
            var result = _leagueRepo.GetByUnion(id, seasonId).Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true && (type == null || type == x.CompetitionType));
            if(isPast.HasValue && isPast == true)
            {
                result = result.Where(x => x.LeagueStartDate.HasValue && x.LeagueStartDate.Value < DateTime.Now);
                ViewBag.IsPast = true;
            }
            else if(isPast.HasValue && isPast == false)
            {
                ViewBag.IsPast = false;
                result = result.Where(x => !x.LeagueStartDate.HasValue || x.LeagueStartDate.Value > DateTime.Now);
            }
            ViewBag.Seasons = _seasonsRepo.GetSeasonsByUnion(id, false).ToList();
            ViewBag.SeasonId = seasonId;
            ViewBag.UnionName = _unionsRepo.GetById(id).Name;
            ViewBag.CompetitionsId = id;
            ViewBag.SectionAlias = union.Section.Alias;
            ViewBag.isPast = isPast;
            ViewBag.CompetitionsType = type;
            return View("_AthleticsCompetitions", result.ToList());
        }

        public ActionResult AthleticsDisciplines(int id, int CompetitionsId = 0)
        {

            var competitionDisciplines = Caching.GetObjectFromCache<IEnumerable<CompetitionDiscipline>>(CacheKeys.CompetitionKey + id.ToString());
            if (competitionDisciplines == null)
            {
                competitionDisciplines = _disciplinesRepo.GetCompetitionDisciplines(id).ToList();

                foreach (var competitionDiscipline in competitionDisciplines)
                {

                    var discipline = _disciplinesRepo.GetById(competitionDiscipline.DisciplineId.Value);
                    competitionDiscipline.CategoryName = competitionDiscipline.CompetitionAge.age_name;
                    competitionDiscipline.DisciplineName = discipline?.Name ?? string.Empty;
                }
                Caching.SetObjectInCache(CacheKeys.CompetitionKey + id.ToString(), 120, competitionDisciplines);
            }

            List<int> playersCount = new List<int>();
            foreach (var competition in competitionDisciplines)
            {
                playersCount.Add(competition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && !string.IsNullOrWhiteSpace(x.CompetitionResult.FirstOrDefault().Result)).Count());
            }
            var leagueData = _leagueRepo.GetById(id);
            ViewBag.leagueData = leagueData;
            ViewBag.PlayersWithResultsCount = playersCount;
            ViewBag.CanGoBack = false;
            ViewBag.IsFieldRace = leagueData.IsFieldCompetition;
            ViewBag.IsMultiBattleCompetition = leagueData.CompetitionDisciplines.Where(cd => cd.IsMultiBattle && !cd.IsDeleted).Count() > 0;
            var doc = _leagueRepo.GetTermsDoc(id);
            ViewBag.TermsFile = doc;

            if (CompetitionsId > 0)
            {
                ViewBag.CanGoBack = true;
                return View("_AthleticsDisciplines", competitionDisciplines);
            }
            else
                return View("_AthleticsDisciplines", competitionDisciplines);
        }

        public ActionResult ShowLeagueDoc(int id)
        {
            var doc = _leagueRepo.GetDocById(id);

            Response.AddHeader("content-disposition", "inline;filename=" + doc.FileName + ".pdf");

            return this.File(doc.DocFile, "application/pdf");
        }

        public ActionResult WeightDeclarationResultsBySession(String id, bool isModal = false)
        {
            String[] idList = id.Split('_');
            int sessionId = Convert.ToInt32(idList[0]);
            int leagueId = Convert.ToInt32(idList[1]);

            var competitionDisciplines = Caching.GetObjectFromCache<List<CompetitionDiscipline>>(CacheKeys.CompetitionResultsKey + id.ToString());
            if (competitionDisciplines == null)
            {
                competitionDisciplines = _disciplinesRepo.GetCompetitionDisciplines(leagueId).ToList();
                competitionDisciplines = competitionDisciplines.OrderBy(cd => cd.CompetitionAge.age_name, new LeagueRepo.WeightLiftingCategoryComparer()).ToList();
                Caching.SetObjectInCache(CacheKeys.CompetitionResultsKey + id.ToString(), 5, competitionDisciplines);
            }

            int comp_count = competitionDisciplines.ToArray().Count();
            CompetitionDiscipline[] model = new CompetitionDiscipline[comp_count];
            var session = _disciplinesRepo.GetCompetitionSession(sessionId);
            League league = _leagueRepo.GetById(leagueId);
            ViewBag.isTeam = "false";
            if (league.IsTeam)
                ViewBag.isTeam = "true";
            ViewBag.isCup = "false";
            if (league.IsCompetitionLeague)
                ViewBag.isCup = "true";
            int i = 0;
            int reg_count = 0;
            ViewBag.CompetitionId = new int[comp_count];
            ViewBag.CategoryName = new string[comp_count];
            ViewBag.CompetitionName = new string[comp_count];
            ViewBag.CompetitionDisciplineId = new int[comp_count];
            ViewBag.SessionId = sessionId;
            ViewBag.SessionNum = session?.SessionNum;
            foreach (CompetitionDiscipline cd in competitionDisciplines)
            {
                model[i] = cd;
                model[i].CompetitionDisciplineRegistrations = cd.CompetitionDisciplineRegistrations.Where(r => r.WeightliftingSessionId == sessionId).ToList();

                var regs = cd.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().LiftingResult);
                int index = 1;
                foreach (var reg in regs)
                {
                    reg.CompetitionResult.FirstOrDefault().LiftingRank = index;
                    index += 1;
                }
                index = 1;
                regs = cd.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().PushResult);
                foreach (var reg in regs)
                {
                    reg.CompetitionResult.FirstOrDefault().PushRank = index;
                    index += 1;
                }

                ViewBag.CompetitionId[i] = cd.CompetitionId;
                int? unionId = cd.League.UnionId;
                int? sectionId = cd.League.SeasonId;
                ViewBag.CategoryName[i] = cd.CompetitionAge.age_name;
                ViewBag.CompetitionName[i] = cd.League.Name;
                ViewBag.CompetitionDisciplineId[i] = cd.Id;
                reg_count += model[i].CompetitionDisciplineRegistrations.Count();
                i++;
            }
            ViewBag.compCount = comp_count;
            ViewBag.reg_count = reg_count;

            if (isModal)
            {
                return PartialView("_WeighLiftingResultsBySession", model);
            }
            else
                return View("_WeighLiftingResultsBySession", model);
        }




        public ActionResult IsWeightDeclarationResultsRequireUpdate(int competitionDisciplineId, long? lastRecieveValue)
        {
            var lastResultUpdate = _disciplinesRepo.GetCompetitionDisciplineLastResultUpdateById(competitionDisciplineId);
            bool isStillSame = false;
            if (!lastResultUpdate.HasValue && !lastRecieveValue.HasValue)
            {
                isStillSame = true;
            }
            long? nextTicks = null;
            if (lastResultUpdate.HasValue && lastRecieveValue.HasValue)
            {
                long diff = lastResultUpdate.Value.Ticks - lastRecieveValue.Value;
               
                if (diff == 0)
                {
                    isStillSame = true;
                }
                nextTicks = lastResultUpdate.Value.Ticks;
                var lastDisciplineCompetitionUpdateCache = Caching.GetObjectFromCache<DateTime?>(CacheKeys.DisciplineCompetitionKeyLastUpdate + competitionDisciplineId.ToString());
                if (lastDisciplineCompetitionUpdateCache.HasValue && lastDisciplineCompetitionUpdateCache.Value.Ticks != lastResultUpdate.Value.Ticks)
                {
                    Caching.ClearCacheValue(CacheKeys.DisciplineCompetitionKey + competitionDisciplineId.ToString());
                }
            }
            return Json(new { IsStillSame = isStillSame, lastUpdate = nextTicks });
        }


        public ActionResult WeightDeclarationResults(int id, bool isModal = false)
        {

            var disciplineCompetition = Caching.GetObjectFromCache<CompetitionDiscipline>(CacheKeys.CompetitionResultsKey + id.ToString());
            if (disciplineCompetition == null)
            {
                disciplineCompetition = _disciplinesRepo.GetCompetitionDisciplineById(id);
                Caching.SetObjectInCache(CacheKeys.CompetitionResultsKey + id.ToString(), 5, disciplineCompetition);
            }

            ViewBag.CompetitionId = disciplineCompetition.CompetitionId;
            int? unionId = disciplineCompetition.League.UnionId;
            int? sectionId = disciplineCompetition.League.SeasonId;
            ViewBag.CategoryName = disciplineCompetition.CompetitionAge.age_name;
            ViewBag.CompetitionName = disciplineCompetition.League.Name;
            ViewBag.CompetitionDisciplineId = id;
            long? nextTicks = null;
            if (disciplineCompetition.LastResultUpdate.HasValue)
            {
                nextTicks = disciplineCompetition.LastResultUpdate.Value.Ticks;
            }
            ViewBag.LastResultsUpdate = nextTicks;

            //disciplineCompetition.CompetitionDisciplineRegistrations = disciplineCompetition.CompetitionDisciplineRegistrations.OrderBy(r => r.CompetitionResult).ToList();

            var regs = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().LiftingResult);
            int index = 1;
            foreach (var reg in regs)
            {
                reg.CompetitionResult.FirstOrDefault().LiftingRank = index;
                index += 1;
            }
            index = 1;
            regs = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().PushResult);
            foreach (var reg in regs)
            {
                reg.CompetitionResult.FirstOrDefault().PushRank = index;
                index += 1;
            }
            var resulted = regs.Where(x => x.CompetitionResult.Count() > 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().FinalResult);
            var res = resulted.Union(disciplineCompetition.CompetitionDisciplineRegistrations).ToList();
            disciplineCompetition.CompetitionDisciplineRegistrations = res;

            var league = _leagueRepo.GetById(disciplineCompetition.CompetitionId);
            ViewBag.isTeam = "false";
            if (league.IsTeam)
                ViewBag.isTeam = "true";
            ViewBag.isCup = "false";
            if (league.IsCompetitionLeague)
                ViewBag.isCup = "true";

            if (isModal)
            {
                return PartialView("_WeightDeclarationResults", disciplineCompetition);
            }
            else
                return View("_WeightDeclarationResults", disciplineCompetition);
        }






        public ActionResult WeightliftingAllCategoryResults(int id, bool isModal = false)
        {
            var league = _leagueRepo.GetById(id);
            ViewBag.isTeam = "false";
            if (league.IsTeam)
                ViewBag.isTeam = "true";
            ViewBag.isCup = "false";
            if (league.IsCompetitionLeague)
                ViewBag.isCup = "true";

            foreach (var disciplineCompetition in league.CompetitionDisciplines)
            {
                var regs = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().LiftingResult);
                int index = 1;
                foreach (var reg in regs)
                {
                    reg.CompetitionResult.FirstOrDefault().LiftingRank = index;
                    index += 1;
                }
                index = 1;
                regs = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().PushResult);
                foreach (var reg in regs)
                {
                    reg.CompetitionResult.FirstOrDefault().PushRank = index;
                    index += 1;
                }
                var resulted = regs.Where(x => x.CompetitionResult.Count() > 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().FinalResult);
                var res = resulted.Union(disciplineCompetition.CompetitionDisciplineRegistrations).ToList();
                disciplineCompetition.CompetitionDisciplineRegistrations = res;

            }
            ViewBag.CompetitionName = league.Name;

            /*
            int? unionId = disciplineCompetition.League.UnionId;
            int? sectionId = disciplineCompetition.League.SeasonId;
            ViewBag.CategoryName = disciplineCompetition.CompetitionAge.age_name;
            */








            if (isModal)
            {
                return PartialView("_WeightliftingAllCategoryResults", league);
            }
            else
                return View("_WeightliftingAllCategoryResults", league);
        }

        
        public ActionResult AthleticsLeagueClubResults(int id, int leagueId, int seasonId, int? GenderId, int? LeagueType, bool isModal = false)
        {
            var league = _leagueRepo.GetById(leagueId);
            var club = _clubRepo.GetById(id);
            ViewBag.CompetitionName = league.Name;
            ViewBag.UnionName = league.Union?.Name;
            ViewBag.CompetitionDate = league.LeagueStartDate;
            ViewBag.LeagueId = id;
            ViewBag.SeasonId = seasonId;
            ViewBag.ClubName = club.Name;
            ViewBag.GenderLetter = UIHelpers.GetGenderCharById(GenderId ?? 3);
            ViewBag.LeagueType = UIHelpers.GetAthleticLeagueTypeById(LeagueType);
            var data = _leagueRepo.GetAthleticsLeagueClubResults(id, leagueId, seasonId, GenderId, LeagueType);
            
            if (isModal)
            {
                return PartialView("_AthleticsLeagueClubResults", data);
            }

            return View("_AthleticsLeagueClubResults", data);
        }

        public ActionResult AthleticsLeagueClubs(int id, int seasonId, bool isModal = false)
        {
            var league = _leagueRepo.GetById(id);
            ViewBag.CompetitionName = league.Name;
            ViewBag.CompetitionDate = league.LeagueStartDate;
            ViewBag.LeagueId = id;
            ViewBag.SeasonId = seasonId;
            var data = _teamsRepo.GetClubsByCorrection(id, seasonId);
            if (isModal)
            {
                return PartialView("_AthleticsLeagueClubs", data);
            }

            return View("_AthleticsLeagueClubs", data);
        }

        public ActionResult StartList(int id, bool isModal = false)
        {
            var disciplineCompetition = _disciplinesRepo.GetCompetitionDisciplineById(id);
            var discipline = _disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);

            ViewBag.CompetitionId = disciplineCompetition.CompetitionId;
            ViewBag.SeasonId = disciplineCompetition.League.SeasonId;
            ViewBag.DisciplineName = discipline.Name;
            ViewBag.Format = discipline.Format;
            ViewBag.CompetitionDisciplineId = id;
            ViewBag.CompetitionName = disciplineCompetition.League.Name;
            ViewBag.CompetitionDate = disciplineCompetition.League.LeagueStartDate.HasValue ? disciplineCompetition.League.LeagueStartDate.Value.ToShortDateString() : "";
            var registrationsWithHeat = disciplineCompetition.CompetitionDisciplineRegistrations.Where(r => r.CompetitionResult.FirstOrDefault() != null );
            ViewBag.PlayersCount = registrationsWithHeat.Count();
            var groupedByHeat = registrationsWithHeat.OrderBy(r => r.CompetitionResult.FirstOrDefault().Lane ?? int.MaxValue).GroupBy(r => r.CompetitionResult.FirstOrDefault().Heat ?? Messages.None).OrderBy(group => group.Key).ToList();


            var numericValues = groupedByHeat.Where(r => int.TryParse(r.Key, out _)).ToList();
            numericValues = numericValues.OrderBy(group => int.Parse(group.Key)).ToList();
            var nonNumericValues = groupedByHeat.Where(r => !int.TryParse(r.Key, out _)).ToList();
            groupedByHeat = numericValues.Concat(nonNumericValues).ToList();

            var userIds = disciplineCompetition.CompetitionDisciplineRegistrations.Select(r => r.UserId).ToList();
            var usersRegsGroupedByUserId = gamesRepo.GetBulkAthleteCompetitionsAchievements(userIds, disciplineCompetition.League.SeasonId.Value);
            var isAsc = Utils.IsOrderByFormatAsc(discipline.Format);
            var disciplineRecordId = _disciplinesRepo.GetCompetitonDisciplineRecord(disciplineCompetition.DisciplineId.Value, disciplineCompetition.CategoryId);

            //foreach (var heatGroup in groupedByHeat)
            //{
            foreach (var reg in disciplineCompetition.CompetitionDisciplineRegistrations)
                {
                    var userId = reg.UserId;
                    if (disciplineRecordId.HasValue)
                    {
                        var userRegs = usersRegsGroupedByUserId.FirstOrDefault(g => g.Key == userId);
                        var userTopResultByRecordIds = string.Empty;
                        if (isAsc)
                        {
                            userTopResultByRecordIds = userRegs?.Where(r => r.CompetitionStartDate <= (disciplineCompetition.League.LeagueStartDate ?? DateTime.MaxValue) && disciplineRecordId.HasValue && r.RecordId.Contains(disciplineRecordId.Value) && r.AlternativeResult == 0).OrderBy(r => r.SortValue).FirstOrDefault()?.Result ?? string.Empty;
                        }
                        else
                        {
                            userTopResultByRecordIds = userRegs?.Where(r => r.CompetitionStartDate <= (disciplineCompetition.League.LeagueStartDate ?? DateTime.MaxValue) && disciplineRecordId.HasValue && r.RecordId.Contains(disciplineRecordId.Value) && r.AlternativeResult == 0).OrderByDescending(r => r.SortValue).FirstOrDefault()?.Result ?? string.Empty;
                        }
                        reg.SeasonalBest = userTopResultByRecordIds;
                    }
                    else
                    {
                        reg.SeasonalBest = string.Empty;
                    }
                }
            //}


            if (disciplineCompetition.IncludeRecordInStartList)
            {
                if (!disciplineRecordId.HasValue)
                {
                    ViewBag.DisciplineRecord = new DisciplineRecord { CompetitionRecord = disciplineCompetition.CompetitionRecord };
                }
                else
                {
                    var disciplineRecordR = _disciplinesRepo.GetDisciplineRecordById(disciplineRecordId.Value);
                    disciplineRecordR.CompetitionRecord = disciplineCompetition.CompetitionRecord;
                    ViewBag.DisciplineRecord = disciplineRecordR;
                    if (disciplineRecordR.SeasonRecords.FirstOrDefault(x =>
                            x.SeasonId == disciplineCompetition.League.SeasonId) != null)
                    {
                        ViewBag.SeasonRecord =
                            disciplineRecordR.SeasonRecords.FirstOrDefault(x =>
                                x.SeasonId == disciplineCompetition.League.SeasonId).SeasonRecord1;
                    }
                }
            }
            if (isModal)
            {
                return PartialView("_Startlist", groupedByHeat);
            }
            else
                return View("_Startlist", groupedByHeat);
        }

        public ActionResult AthleticsDisciplineResults(int id, bool isModal = false)
        {
            bool wasCached = true;
            var disciplineCompetition = Caching.GetObjectFromCache<CompetitionDiscipline>(CacheKeys.DisciplineCompetitionKey + id.ToString());
            if (disciplineCompetition == null)
            {
                wasCached = false;
                disciplineCompetition = _disciplinesRepo.GetCompetitionDisciplineById(id);
                Caching.SetObjectInCache(CacheKeys.DisciplineCompetitionKey + id.ToString(), 30, disciplineCompetition);
                if (disciplineCompetition.LastResultUpdate != null)
                {
                    Caching.SetObjectInCache(CacheKeys.DisciplineCompetitionKeyLastUpdate + id.ToString(), 30, disciplineCompetition.LastResultUpdate);
                }
            }


            var discipline = _disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);

            ViewBag.CompetitionId = disciplineCompetition.CompetitionId;
            int? unionId = disciplineCompetition.League.UnionId;
            int? seasonId = disciplineCompetition.League.SeasonId;
            ViewBag.Format = discipline.Format;
            ViewBag.SeasonId = disciplineCompetition.League.SeasonId;
            ViewBag.IsResultsManualyRanked = disciplineCompetition.IsResultsManualyRanked;
            if (!wasCached)
            {
                if (disciplineCompetition.League.IsCompetitionLeague)
                {
                    var rankings = Caching.GetObjectFromCache<List<List<CompetitionClubRankedStanding>>>(CacheKeys.CompetitionClubsRankingsKey + id.ToString() + disciplineCompetition.League.SeasonId.Value.ToString());
                    if (rankings == null)
                    {
                        rankings = _disciplinesRepo.CupClubRanks(disciplineCompetition.CompetitionId, disciplineCompetition.League.SeasonId.Value);

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
                        Caching.SetObjectInCache(CacheKeys.CompetitionClubsRankingsKey + id.ToString() + disciplineCompetition.League.SeasonId.Value.ToString(), 30, rankings);
                    }
                }

                if (disciplineCompetition.IsResultsManualyRanked)
                {
                    var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().Rank ?? int.MaxValue).ToList();
                    disciplineCompetition.ResultedCompetitionDisciplineRegistrations = resulted;
                }
                else if (discipline.Format != null && ((discipline.Format.Value >= 6 && discipline.Format.Value <= 8) || discipline.Format.Value == 10 || discipline.Format.Value == 11))
                {
                    var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().SortValue);
                    if (discipline.Format.Value == 7 || discipline.Format.Value == 10 || discipline.Format.Value == 11) {
                        resulted = resulted.ThenByDescending(r => r.GetThrowingsOrderPower());
                    }
                    if (discipline.Format.Value == 6){
                        resulted = resulted.ThenByDescending(r => r.CompetitionResult.FirstOrDefault()?.LiveSplitOrder ?? int.MinValue);
                    }
                    var res = resulted.Union(disciplineCompetition.CompetitionDisciplineRegistrations).ToList();
                    disciplineCompetition.ResultedCompetitionDisciplineRegistrations = res;
                }
                else
                {
                    var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().SortValue).ThenByDescending(r => r.CompetitionResult.FirstOrDefault()?.LiveSplitOrder ?? int.MinValue);
                    var res = resulted.Union(disciplineCompetition.CompetitionDisciplineRegistrations).ToList();
                    disciplineCompetition.ResultedCompetitionDisciplineRegistrations = res;
                }
            }
            ViewBag.DisciplineName = discipline.Name;
            ViewBag.IsCombinedDiscipline = !string.IsNullOrWhiteSpace(discipline.DisciplineType) && (discipline.DisciplineType == "decathlon" || discipline.DisciplineType == "heptathlon");
            ViewBag.GenderId = Helpers.UIHelpers.GetGenderCharById(disciplineCompetition.CompetitionAge.gender.Value);
            string logo = disciplineCompetition.League.Union.Logo;
            ViewBag.Logo = logo;
            ViewBag.IsMultiBattle = disciplineCompetition.IsMultiBattle;
            disciplineCompetition.ResultedCompetitionDisciplineRegistrations = disciplineCompetition.ResultedCompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && !string.IsNullOrWhiteSpace(x.CompetitionResult.FirstOrDefault().Result)).ToList();
            ViewBag.IsOneRecordHasValue = disciplineCompetition.CompetitionDisciplineRegistrations.Any(r => r.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault().Records));
            ViewBag.LastFirstStageColumn = -1;
            ViewBag.IsAnyAttempt = false;
            if (discipline.Format == 7 || discipline.Format == 10 || discipline.Format == 11)
            {
                ViewBag.IsAnyAttempt = disciplineCompetition.CompetitionDisciplineRegistrations.Any(r => r.CompetitionResult.FirstOrDefault() != null && (
                    !string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault().Attempt1) ||
                    r.CompetitionResult.FirstOrDefault().Attempt1Wind.HasValue ||
                    (r.CompetitionResult.FirstOrDefault().Alternative1.HasValue && r.CompetitionResult.FirstOrDefault().Alternative1.Value > 0)
                ));

            }
            else if (discipline.Format == 6)
            {
                 ViewBag.IsAnyAttempt = disciplineCompetition.CompetitionDisciplineRegistrations.Any(r => r.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault().CustomFields));
            }
            disciplineCompetition.CompetitionDisciplineRegistrations = disciplineCompetition.CompetitionDisciplineRegistrations.OrderBy(r => r.CompetitionResult.FirstOrDefault()?.Lane ?? int.MaxValue).ToList();
            if (disciplineCompetition.NumberOfWhoPassesToNextStage.HasValue && discipline.Format.HasValue && (discipline.Format.Value == 7 || discipline.Format.Value == 10 || discipline.Format.Value == 11))
            {
                disciplineCompetition.CompetitionDisciplineRegistrations = disciplineCompetition.CompetitionDisciplineRegistrations.OrderBy(r => r.CompetitionResult.FirstOrDefault()?.LiveSplitOrder ?? int.MaxValue).ToList();
                var noSecondStagePassers = disciplineCompetition.CompetitionDisciplineRegistrations.Skip(disciplineCompetition.NumberOfWhoPassesToNextStage.Value);
                var columnLimit = -1;
                foreach (var noSecondStagePasser in noSecondStagePassers)
                {
                    var result = noSecondStagePasser.CompetitionResult.FirstOrDefault();
                    if(result != null)
                    {
                        if (!string.IsNullOrWhiteSpace(result.Attempt6) && columnLimit < 6) {
                            columnLimit = 6;
                        }
                        if (!string.IsNullOrWhiteSpace(result.Attempt5) && columnLimit < 5)
                        {
                            columnLimit = 5;
                        }
                        if (!string.IsNullOrWhiteSpace(result.Attempt4) && columnLimit < 4)
                        {
                            columnLimit = 4;
                        }
                        if (!string.IsNullOrWhiteSpace(result.Attempt3) && columnLimit < 3)
                        {
                            columnLimit = 3;
                        }
                        if (!string.IsNullOrWhiteSpace(result.Attempt2) && columnLimit < 2)
                        {
                            columnLimit = 2;
                        }
                        if (!string.IsNullOrWhiteSpace(result.Attempt1) && columnLimit < 1)
                        {
                            columnLimit = 1;
                        }
                    }
                }
                ViewBag.LastFirstStageColumn = columnLimit;
            }

            var disciplineRecordId = _disciplinesRepo.GetCompetitonDisciplineRecord(disciplineCompetition.DisciplineId.Value, disciplineCompetition.CategoryId);

            if (disciplineCompetition.IncludeRecordInStartList)
            {
                if (!disciplineRecordId.HasValue)
                {
                    ViewBag.DisciplineRecord = new DisciplineRecord { CompetitionRecord = disciplineCompetition.CompetitionRecord };
                }
                else
                {
                    var disciplineRecordR = _disciplinesRepo.GetDisciplineRecordById(disciplineRecordId.Value);
                    disciplineRecordR.CompetitionRecord = disciplineCompetition.CompetitionRecord;
                    ViewBag.DisciplineRecord = disciplineRecordR;
                }
            }

            ViewBag.SeasonId = seasonId;
            if (isModal)
            {
                return PartialView("_AthleticsDisciplineResults", disciplineCompetition);
            }else
                return View("_AthleticsDisciplineResults", disciplineCompetition);

        }

        public void LeagueInfo(int id, int seasonId )
        {

        }
        



        public ActionResult AthleticsDisciplineResultsByHeat(int id, bool isModal = false)
        {
            bool wasCached = true;
            var disciplineCompetition = Caching.GetObjectFromCache<CompetitionDiscipline>(CacheKeys.DisciplineCompetitionKey + id.ToString());
            if (disciplineCompetition == null)
            {
                wasCached = false;
                disciplineCompetition = _disciplinesRepo.GetCompetitionDisciplineById(id);
                Caching.SetObjectInCache(CacheKeys.DisciplineCompetitionKey + id.ToString(), 30, disciplineCompetition);
            }


            var discipline = _disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);

            ViewBag.CompetitionId = disciplineCompetition.CompetitionId;
            int? unionId = disciplineCompetition.League.UnionId;
            int? sectionId = disciplineCompetition.League.SeasonId;
            ViewBag.Format = discipline.Format;
            ViewBag.SeasonId = disciplineCompetition.League.SeasonId;
            ViewBag.IsResultsManualyRanked = disciplineCompetition.IsResultsManualyRanked;
            if (!wasCached)
            {
                if (disciplineCompetition.League.IsCompetitionLeague)
                {
                    var rankings = Caching.GetObjectFromCache<List<List<CompetitionClubRankedStanding>>>(CacheKeys.CompetitionClubsRankingsKey + id.ToString() + disciplineCompetition.League.SeasonId.Value.ToString());
                    if (rankings == null)
                    {
                        rankings = _disciplinesRepo.CupClubRanks(disciplineCompetition.CompetitionId, disciplineCompetition.League.SeasonId.Value);

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
                        Caching.SetObjectInCache(CacheKeys.CompetitionClubsRankingsKey + id.ToString() + disciplineCompetition.League.SeasonId.Value.ToString(), 30, rankings);
                    }
                }

                if (disciplineCompetition.IsResultsManualyRanked)
                {
                    var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().Rank ?? int.MaxValue).ToList();
                    disciplineCompetition.CompetitionDisciplineRegistrations = resulted;
                }
                else if (discipline.Format != null && ((discipline.Format.Value >= 6 && discipline.Format.Value <= 8) || discipline.Format.Value == 10 || discipline.Format.Value == 11))
                {
                    var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().SortValue);
                    if (discipline.Format.Value == 7 || discipline.Format.Value == 10 || discipline.Format.Value == 11)
                    {
                        resulted = resulted.ThenByDescending(r => r.GetThrowingsOrderPower());
                    }
                    if (discipline.Format.Value == 6)
                    {
                        resulted = resulted.ThenByDescending(r => r.CompetitionResult.FirstOrDefault()?.LiveSplitOrder ?? int.MinValue);
                    }
                    var res = resulted.Union(disciplineCompetition.CompetitionDisciplineRegistrations).ToList();
                    disciplineCompetition.CompetitionDisciplineRegistrations = res;
                }
                else
                {
                    var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().SortValue);
                    var res = resulted.Union(disciplineCompetition.CompetitionDisciplineRegistrations).ToList();
                    disciplineCompetition.CompetitionDisciplineRegistrations = res;
                }
            }
            ViewBag.DisciplineName = discipline.Name;
            ViewBag.IsCombinedDiscipline = !string.IsNullOrWhiteSpace(discipline.DisciplineType) && (discipline.DisciplineType == "decathlon" || discipline.DisciplineType == "heptathlon");
            ViewBag.GenderId = Helpers.UIHelpers.GetGenderCharById(disciplineCompetition.CompetitionAge.gender.Value);
            string logo = disciplineCompetition.League.Union.Logo;
            ViewBag.Logo = logo;
            ViewBag.IsMultiBattle = disciplineCompetition.IsMultiBattle;
            disciplineCompetition.CompetitionDisciplineRegistrations = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && !string.IsNullOrWhiteSpace(x.CompetitionResult.FirstOrDefault().Result)).ToList();
            disciplineCompetition.CompetitionDisciplineRegistrationsByHeat = disciplineCompetition.CompetitionDisciplineRegistrations.GroupBy(r => r.CompetitionResult.FirstOrDefault().Heat ?? Messages.None).ToList();

            var numericValues = disciplineCompetition.CompetitionDisciplineRegistrationsByHeat.Where(r => int.TryParse(r.Key, out _)).ToList();
            numericValues = numericValues.OrderBy(group => int.Parse(group.Key)).ToList();
            var nonNumericValues = disciplineCompetition.CompetitionDisciplineRegistrationsByHeat.Where(r => !int.TryParse(r.Key, out _)).ToList();
            disciplineCompetition.CompetitionDisciplineRegistrationsByHeat = numericValues.Concat(nonNumericValues).ToList();

            ViewBag.SeasonId = disciplineCompetition.League.SeasonId;
            if (isModal)
            {
                return PartialView("_AthleticsDisciplineResultsByHeat", disciplineCompetition);
            }
            else
                return View("_AthleticsDisciplineResultsByHeat", disciplineCompetition);

        }
        public ActionResult WeightliftingSessionAppointments(int id, int LeagueId, bool isModal = false)
        {
            var playersInSession = _playersRepo.GetPlayersDisciplineRegistrationsBySession(LeagueId, id).ToList();
            ViewBag.SessionId = id;
            ViewBag.CompetitionId = LeagueId;
            var league = _leagueRepo.GetById(LeagueId);

            var session = _disciplinesRepo.GetCompetitionSession(id);
            ViewBag.WSession = session;
            ViewBag.SessionNum = "-";
            if(session?.SessionNum != null)
            {
                ViewBag.SessionNum = session?.SessionNum;
            }
            ViewBag.CompetitionName = league.Name;
            if (isModal)
            {
                ViewBag.IsModal = true;
                return PartialView("_WeightLiftingSessionAppointments", playersInSession);
            }
            else
            {
                ViewBag.IsModal = false;
                return View("_WeightLiftingSessionAppointments", playersInSession);
            }
        }

        public ActionResult WeightliftingSessions(int id)
        {
            var sessions = _disciplinesRepo.GetCompetitionSessions(id).ToList();
            var league = _leagueRepo.GetById(id);
            ViewBag.CompetitionId = id;
            ViewBag.SeasonId = league.SeasonId;
            ViewBag.CompetitionName = league.Name;
            ViewBag.CategoriesList = _disciplinesRepo.GetCompetitionDisciplines(id)
                .Select(c => new CompetitionDisciplineDto
                {
                    Id = c.Id,
                    DisciplineId = c.DisciplineId,
                    CompetitionId = c.CompetitionId,
                    CategoryId = c.CategoryId,
                    MaxSportsmen = c.MaxSportsmen,
                    SectionAlias = c.League.Union.Section.Alias,
                    RegistrationsCount = c.CompetitionDisciplineRegistrations.Count,
                    CategoryName = c.CompetitionAge?.age_name
                }).ToList();
            return View("_WeightLiftingSessions", sessions);
        }


        public ActionResult CategoryRegistration(int leagueId, int disciplineId, int seasonId)
        {
            var league = _leagueRepo.GetById(leagueId);
            var competitionDiscipline = _disciplinesRepo.GetCompetitionDisciplineById(disciplineId);
            List<AthleticRegDto> sportsmen = _playersRepo.GetAthleticsRegistrations(null, leagueId, disciplineId, seasonId)?.OrderBy(t => t.DisciplineName).ToList();
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueId = leagueId;
            ViewBag.CompetitionDisiplineId = disciplineId;
            ViewBag.CompetitionName = league.Name;
            ViewBag.UnionName = league.Union.Name;
            ViewBag.CategoryName = competitionDiscipline.CompetitionAge.age_name;
            return PartialView("_CategoryRegistration", sportsmen);
        }







        public ActionResult RegisteredCompetitionAthletes(int id)
        {
            var competitionDiscipline = _disciplinesRepo.GetCompetitionDisciplineNoTracking(id);
            ViewBag.SeasonId = competitionDiscipline.League.SeasonId;
            return PartialView("_DisciplineRegistration", competitionDiscipline);
        }

        public ActionResult CategoryRankDetails(int categoryId, int seasonId, int? unionId)
        {
            var category = _teamsRepo.GetById(categoryId);
            var league = _leagueRepo.GetLeagueForCategory(categoryId, seasonId);
            if (league != null)
            {
                var categoryName = category?.TeamsDetails?.OrderByDescending(t => t.Id)?.FirstOrDefault()?.TeamName
                    ?? category.Title;
                ViewBag.LeagueId = league.LeagueId;
                ViewBag.Logo = UIHelper.GetLeagueLogo(league.Logo);
                ViewBag.ResTitle = $"{league.Name.Trim()} / {categoryName}";
            }

            var listLevels = category?.CompetitionLevel?.LevelPointsSettings?.OrderBy(x => x.Rank).Where(t => t.SeasonId == seasonId).ToList();
            RankCategory rCategory = new RankCategory();
            if (listLevels != null)
            {
                ViewBag.PlayerRankGroupList = gamesRepo.GetTennisPlayoffRanksGroup(categoryId, category.LeagueTeams.FirstOrDefault(t => t.SeasonId == seasonId)?.LeagueId ?? 0, seasonId, true);
            }
            ViewBag.PlayoffTable = UpdateTennisPlayoffRank(categoryId, seasonId);
            return View("Tennis/CategoryDetails", rCategory);
        }

        private IEnumerable<TennisPlayoffRank> UpdateTennisPlayoffRank(int categoryId, int seasonId)
        {
            var svc = new CategoryRankService(categoryId);
            return svc.CreatePlayoffEmptyTable(seasonId, out bool hasPlayoff).OrderBy(c => c.Rank);
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
                    rCategory.Players = _teamsRepo.GetCategoryPlayers(categoryId, seasonId);
                }
            }

            return rCategory;
        }

    }
}
