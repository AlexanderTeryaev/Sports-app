using AppModel;
using DataService;
using DataService.LeagueRank;
using DataService.DTO;
using Resources;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services;
using CmsApp.Helpers;
using LogLigFront.Helpers;
namespace WebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Leauges")]
    public class LeaugesController : BaseLogLigApiController
    {
        private TeamsRepo _teamsRepo = null;
        private SeasonsRepo _seasonsRepo = null;
        private LeagueRepo _leagueRepo = null;
        private SectionsRepo _sectionsRepo = null;
        private GamesRepo _gamesRepo = null;

        public TeamsRepo teamsRepo
        {
            get
            {
                if (_teamsRepo == null)
                {
                    _teamsRepo = new TeamsRepo();
                }
                return _teamsRepo;
            }
        }

        public SeasonsRepo seasonsRepo
        {
            get
            {
                if (_seasonsRepo == null)
                {
                    _seasonsRepo = new SeasonsRepo();
                }
                return _seasonsRepo;
            }
        }

        public LeagueRepo leagueRepo
        {
            get
            {
                if (_leagueRepo == null)
                {
                    _leagueRepo = new LeagueRepo();
                }
                return _leagueRepo;
            }
        }

        public SectionsRepo sectionsRepo
        {
            get
            {
                if (_sectionsRepo == null)
                {
                    _sectionsRepo = new SectionsRepo(db);
                }
                return _sectionsRepo;
            }
        }

        public GamesRepo gamesRepo
        {
            get
            {
                if (_gamesRepo == null)
                {
                    _gamesRepo = new GamesRepo(db);
                }
                return _gamesRepo;
            }
        }
        public bool IsKarateSection(int id, LogicaName logicalName)
        {
            string sportName = string.Empty;
            switch (logicalName)
            {
                case LogicaName.Union:
                    var union = db.Unions.FirstOrDefault(u => u.UnionId == id);
                    sportName = union?.Sport?.Name ?? string.Empty;
                    break;
                case LogicaName.League:
                    var league = db.Leagues.FirstOrDefault(l => l.LeagueId == id);
                    sportName = league?.Union?.Sport?.Name ?? league?.Club?.Sport?.Name ?? string.Empty;
                    break;
                case LogicaName.Club:
                    var club = db.Clubs.FirstOrDefault(c => c.ClubId == id);
                    sportName = club?.Sport?.Name ?? club?.Union?.Sport?.Name ?? string.Empty;
                    break;
            }
            return string.Equals(sportName, "Karate", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Cheng Li. Add: GetCompetitionRegistration API
        /// </summary>
        /// <param name="leagureId">leagure Id</param>
        /// <param name="categoryId">category Id</param>
        /// <returns></returns>
        [AllowAnonymous]
        [ResponseType(typeof(List<RegistrationModel>))]
        [Route("GetRankTennisCompetition/{leagureId}/{categoryId}")]
        public IHttpActionResult GetRankingTableForTennis(int leagureId, int categoryId)
        {
            League league = db.Leagues.Find(leagureId);
            if (league == null)
            {
                return NotFound();
            }
            List<TennisPlayoffRankModel> rmList = new List<TennisPlayoffRankModel>();
            IEnumerable<LeagueTeams> leagueTeams = leagueRepo.GetLeagueTeam(league.LeagueId, league.SeasonId != null ? (int)league.SeasonId : -1);

            CategoryRankService svc = new CategoryRankService(categoryId);
            rmList = svc.CreatePlayoffEmptyTable(league.SeasonId != null ? (int)league.SeasonId : -1, out bool hasPlayoff).Select(t => new TennisPlayoffRankModel
            {
                UserId = t.PlayerId,
                Points = t.Points + t.Correction,
                PlayerName = t.PlayerName
            }).ToList();
            var index = 1;
            foreach (var item in rmList)
            {
                item.Rank = index;
                index++;
            }
            rmList = rmList.OrderBy(c => c.Rank).ToList();
            return Ok(rmList);
        }
        [AllowAnonymous]
        [ResponseType(typeof(List<RegistrationModel>))]
        [Route("GetRankTennisCompetitionNew/{leagureId}/{categoryId}")]
        public IHttpActionResult GetRankTennisCompetitionNew(int leagureId, int categoryId)
        {
            League league = db.Leagues.Find(leagureId);
            if (league == null)
            {
                return NotFound();
            }
            List<TennisPlayoffRankModel> rmList = new List<TennisPlayoffRankModel>();
            rmList = db.TennisCategoryPlayoffRanks.Where(tcpr => tcpr.LeagueId == leagureId && tcpr.CategoryId == categoryId && tcpr.SeasonId == league.SeasonId).ToList().Select(tcpr => new TennisPlayoffRankModel { UserId = tcpr.TeamsPlayer.UserId, Points = tcpr.Points + tcpr.Correction, PlayerName = tcpr.TeamsPlayer.User.FullName })
            .GroupBy(x => x.UserId)
            .Select(x => x.FirstOrDefault())
            .OrderByDescending(x => x.Points)
            .ToList();

            var index = 1;
            foreach (var item in rmList)
            {
                item.Rank = index;
                index++;
            }

            return Ok(rmList);
        }
        /// <summary>
        /// Cheng Li. Add: Get Routes Name List API
        /// </summary>
        /// <param name="leagureId">leagure Id</param>
        /// <returns></returns>
        [AllowAnonymous]
        [ResponseType(typeof(List<string>))]
        [Route("GetTennisCategoryNameList/{leagureId}")]
        public IHttpActionResult GetTennisCategoryNameList(int leagureId)
        {
            League league = db.Leagues.Find(leagureId);
            if (league == null)
            {
                return NotFound();
            }
            List<BaseNameModel> cateogyrNameList = new List<BaseNameModel>();
            IEnumerable<LeagueTeams> leagueTeams = leagueRepo.GetLeagueTeam(league.LeagueId, league.SeasonId != null ? (int)league.SeasonId : -1);
            foreach (LeagueTeams leagueTeam in leagueTeams)
            {
                BaseNameModel vm = new BaseNameModel();
                vm.Id = leagueTeam.TeamId;
                int? competitionAgeId = db.Teams.FirstOrDefault(t => t.TeamId == leagueTeam.TeamId).CompetitionAgeId;
                vm.Name = "";
                if (competitionAgeId != null)
                {
                    vm.Name = db.CompetitionAges.Where(x => x.id == competitionAgeId).FirstOrDefault().age_name;
                }
                if(vm.Name.Length > 0)
                    cateogyrNameList.Add(vm);
            }
            return Ok(cateogyrNameList);
        }
        /// <summary>
        /// Cheng Li. Add: GetCompetitionRegistration API
        /// </summary>
        /// <param name="leagureId">leagure Id</param>
        /// <param name="routeName">route name</param>
        /// <param name="rankName">rank name</param>
        /// <returns></returns>
        [AllowAnonymous]
        [ResponseType(typeof(List<RegistrationModel>))]
        [Route("CompetitionRegistration/{leagureId}/{routeName}/{rankName}")]
        public IHttpActionResult GetCompetitionRegistration(int leagureId, string routeName, string rankName)
        {
            League league = db.Leagues.Find(leagureId);
            if (league == null)
            {
                return NotFound();
            }
            if (routeName.CompareTo("ShowAll") == 0) routeName = "";
            if (rankName.CompareTo("ShowAll") == 0) rankName = "";

            UnionsRepo sRepo = new UnionsRepo();
            String sectionName = "";
            if (league.UnionId != null)
            {
                sectionName = sRepo.GetSectionByUnionId(league.UnionId.Value).Alias;
            }
            else
            {
                sectionName = db.Sections.Where(s => s.SectionId == league.Club.SectionId).FirstOrDefault().Alias;
            }

            IEnumerable<CompetitionRegistration> crList;
            IEnumerable<RegistrationModel> rmList = Enumerable.Empty<RegistrationModel>();


            if (sectionName.Equals(GamesAlias.Gymnastic) == true)
            {
                if (routeName.Length > 0 && rankName.Length > 0)
                    crList = league.CompetitionRegistrations.Where(cr => cr.CompetitionRoute.DisciplineRoute.Route == routeName && cr.CompetitionRoute.RouteRank.Rank == rankName && cr.IsActive && !cr.IsRegisteredByExcel);
                else if (routeName.Length > 0)
                    crList = league.CompetitionRegistrations.Where(cr => cr.CompetitionRoute.DisciplineRoute.Route == routeName && cr.IsActive && !cr.IsRegisteredByExcel);
                else if (rankName.Length > 0)
                    crList = league.CompetitionRegistrations.Where(cr => cr.CompetitionRoute.RouteRank.Rank == rankName && cr.IsActive && !cr.IsRegisteredByExcel);
                else
                    crList = league.CompetitionRegistrations.Where(cr => cr.IsActive && !cr.IsRegisteredByExcel);

                rmList = crList.GroupBy(c => c.UserId).Select(crt => new RegistrationModel
                {
                    FinalScore = crt.First().FinalScore?.ToString(),
                    ClubName = crt.First().Club.Name,
                    FullName = crt.First().User.FullName,
                    UserId = crt.First().UserId,
                    ClubId = crt.First().ClubId,
                    Rank = crt.First().Position
                });
            }
            else if(league.UnionId != null && IsKarateSection((int)league.LeagueId, LogicaName.League) == true)
            {
                rmList = league.SportsRegistrations.Select(sr => new RegistrationModel
                {
                    FinalScore = sr.FinalScore?.ToString(),
                    ClubName = sr.Club.Name,
                    FullName = sr.User.FullName,
                    UserId = sr.UserId,
                    ClubId = sr.ClubId,
                    Rank = sr.Position
                });
            }
            else
            {
                var rLeague = UpdateRankLeague(league.LeagueId, (int)league.SeasonId);
                rLeague.Stages.Reverse();
            }
            if (rmList != null)
                rmList = rmList.OrderBy(cr => cr.Rank).ToList();

            return Ok(rmList);
        }

        private RankLeague UpdateRankLeague(int id, int seasonId, bool isTennisLeague = false)
        {
            var usr = CurrentUser;
            //LeagueRankService svc = new LeagueRankService(id);
            RankLeague rLeague = CacheService.CreateLeagueRankTable(id, seasonId, isTennisLeague);

            if (rLeague == null)
            {
                rLeague = new RankLeague();
            }
            else if (rLeague.Stages.Count == 0)
            {
                rLeague = CacheService.CreateEmptyRankTable(id, seasonId);
                rLeague.IsEmptyRankTable = true;

                if (rLeague.Stages.Count == 0)
                {
                    var teamRepo = new TeamsRepo();
                    rLeague.Teams = teamRepo.GetTeams(seasonId, id).ToList();
                }
            }
            return rLeague;
        }

        /// <summary>
        /// Cheng Li. Add: Get Routes Name List API
        /// </summary>
        /// <param name="leagureId">leagure Id</param>
        /// <returns></returns>
        [AllowAnonymous]
        [ResponseType(typeof(List<string>))]
        [Route("GetRoutesNameList/{leagureId}")]
        public IHttpActionResult GetRoutesNameList(int leagureId)
        {
            League league = db.Leagues.Find(leagureId);
            if (league == null)
            {
                return NotFound();
            }
            int id = 0;
            IEnumerable<BaseNameModel> routeNameList = league.CompetitionRoutes.Any()
                ? league.CompetitionRoutes.GroupBy(cr => cr.DisciplineRoute.Route)
                    .Select(cr => new BaseNameModel
                    {
                        Id = ++id,
                        Name = cr.First().DisciplineRoute.Route
                    })
                : Enumerable.Empty<BaseNameModel>();
            return Ok(routeNameList);
        }

        /// <summary>
        /// Cheng Li. Add: Get Ranks Name List API
        /// </summary>
        /// <param name="leagureId">leagure Id</param>
        /// <returns></returns>
        [AllowAnonymous]
        [ResponseType(typeof(List<string>))]
        [Route("GetRanksNameList/{leagureId}")]
        public IHttpActionResult GetRanksNameList(int leagureId)
        {
            League league = db.Leagues.Find(leagureId);
            if (league == null)
            {
                return NotFound();
            }
            int id = 0;
            IEnumerable<BaseNameModel> rankNameList = league.CompetitionRoutes.Any()
                ? league.CompetitionRoutes.GroupBy(cr => cr.RouteRank.Rank)
                    .Select(cr => new BaseNameModel
                    {
                        Id = ++id,
                        Name = cr.First().RouteRank.Rank
                    })
                : Enumerable.Empty<BaseNameModel>();
            return Ok(rankNameList);
        }


        /// <summary>
        /// Cheng Li. Add: Get Competition for Tennis
        /// </summary>
        /// <param name="leagueId">leagure Id</param>
        /// <param name="teamId">team Id</param>
        /// <param name="ln"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [ResponseType(typeof(LeaguePageVeiwModel))]
        [Route("competitionForTennis/{leagueId}/{teamId}")]
        public IHttpActionResult GetCompetitionForTennis(int leagueId, int teamId, string ln)
        {
            CultureModel.ChangeCulture(ln);

            League league = db.Leagues.Find(leagueId);
            if (league == null) {
                return NotFound();
            }

            LeaguePageVeiwModel vm = new LeaguePageVeiwModel();
            int? currentSeasonId = league.SeasonId;
            int currentUserId = base.CurrUserId;

            var teamRepo = new TeamsRepo();
            var section = sectionsRepo.GetByLeagueId(leagueId);

            //LeagueRankService lrsvc = new LeagueRankService(leagueId);
            //League Info 
            vm.LeagueInfo = new LeagueInfoVeiwModel(league);
            vm.LeagueInfo.Type = 1;

            // Category Info
          var team = teamRepo.GetTeams((int)currentSeasonId, leagueId).Where(lt => lt.TeamId == teamId).FirstOrDefault();
            if (team != null)
            {
                vm.Team = new CompetitionTeamViewModel(db, team, 38/*tennis union id*/, leagueId, currentUserId, currentSeasonId);
                if(team.CompetitionAgeId != null)
                {
                    var teamDetails = team.TeamsDetails.FirstOrDefault(x => x.SeasonId == currentSeasonId.Value);
                    if (teamDetails != null)
                    {
                        vm.LeagueInfo.Title += (" - " + teamDetails.TeamName);
                        //vm.LeagueInfo.Image = team.Logo;
                        //vm.LeagueInfo.Logo = team.Logo;
                    }
                }

                //League Other Info 
                vm.LevelDateSetting = new LevelDateSetting();
                vm.LevelDateSetting.QualificationStartDate = team.CategoriesPlaceDates?.FirstOrDefault()?.QualificationStartDate;
                vm.LevelDateSetting.QualificationEndDate = team.CategoriesPlaceDates?.FirstOrDefault()?.QualificationEndDate;
                vm.LevelDateSetting.FinalStartDate = team.CategoriesPlaceDates?.FirstOrDefault()?.FinalStartDate;
                vm.LevelDateSetting.FinalEndDate = team.CategoriesPlaceDates?.FirstOrDefault()?.FinalEndDate;

                var games = gamesRepo.GetTennisGroupsCycles(teamId);
                //var leagueGames = (from tgcs in db.TennisGameCycles
                //                   from tp in teamsPlayers
                //                   where (tgcs.FirstPlayerId == tp.Id || tgcs.SecondPlayerId == tp.Id ||
                //                   tgcs.FirstPlayerPairId == tp.Id || tgcs.SecondPlayerPairId == tp.Id) && tp.UserId == CurrUserId
                //                   select new
                //                   {
                //                       results = tgcs
                //                   }).ToList();
                //Next Game
                vm.NextGame = GamesService.GetNextTennisGame(games, currentUserId, leagueId, currentSeasonId);
                //List of all next games
                vm.NextGames = GamesService.GetNextTennisGames(games, DateTime.Now, currentSeasonId);
                //Last Games
                vm.LastGames = GamesService.GetLastTennisGames(games, currentSeasonId).OrderBy(x => x.StartDate);
                //Game Cycles
                vm.GameCycles = games.Select(gc => gc.CycleNum).Distinct().OrderBy(c => c).ToList();
            }
            return Ok(vm);
        }

        [AllowAnonymous]
        //[ResponseType(typeof(LeaguePageVeiwModel))]
        [Route("GetGameBracketUrl/{leagueId}/{teamId}")]
        public IHttpActionResult GetGameBracketUrl(int leagueId, int teamId, string ln)
        {

            League league = db.Leagues.Find(leagueId);
            if (league == null)
            {
                return NotFound();
            }

            LeaguePageVeiwModel vm = new LeaguePageVeiwModel();
            int? currentSeasonId = league.SeasonId;
            var result = "http://loglig.com:8088/LeagueTable/SchedulesForTennisCompetition/" + teamId + "?seasonId=" + currentSeasonId;
            return Ok(result);
        }
        /// <summary>
        /// מחזיר דף ליגה
        /// </summary>
        /// <param name="id">ID ליגה</param>
        /// <param name="ln"></param>
        /// <returns></returns>
        public class AthleticsLeagueStandingModel
        {
            public int? TypeId { get; set; }
            public int GenderId { get; set; }
            public int ClubId { get; set; }
            public string ClubName { get; set; }
            public decimal FinalScore { get; set; }
        }
        [AllowAnonymous]
        [ResponseType(typeof(LeaguePageVeiwModel))]
        [Route("{id}")]
        public IHttpActionResult GetLeague(int id, string ln)
        {
            CultureModel.ChangeCulture(ln);

            League league = db.Leagues.Find(id);
            if (league == null)
            {
                return NotFound();
            }

            int? currentSeasonId = league.SeasonId;

            LeaguePageVeiwModel vm = new LeaguePageVeiwModel();
            //League Info
            vm.LeagueInfo = new LeagueInfoVeiwModel(league);
            var disciplinesRepo = new DisciplinesRepo();
            var leagueRepo = new LeagueRepo();
            vm.LeagueInfo.docId = leagueRepo.GetTermsDoc(id)==null?0: leagueRepo.GetTermsDoc(id).DocId;
            //get discipline info for a competition_id = id
            var competitionDisciplines = disciplinesRepo.GetCompetitionDisciplines(id)
                .Select(c => new CompetitionDisciplineDto
                {
                    Id = c.Id,
                    DisciplineId = c.DisciplineId,
                    CompetitionId = c.CompetitionId,
                    CategoryId = c.CategoryId,
                    MaxSportsmen = c.MaxSportsmen,
                    MinResult = c.MinResult,
                    StartTime = c.StartTime,
                    IsMultiBattle = c.IsMultiBattle,
                    IsForScore = c.IsForScore,
                    SectionAlias = c.League.Union.Section.Alias,
                    RegistrationsCount = c.CompetitionDisciplineRegistrations.Count,
                    IsResultsManualyRanked = c.IsResultsManualyRanked,
                    CategoryName = c.CompetitionAge?.age_name,
                    DistanceName = c.RowingDistance?.Name
                }).ToList();
            var resultCompetitionDispciplines = new List<CompetitionDisciplineDto>();
            
            if (competitionDisciplines.FirstOrDefault()?.SectionAlias == GamesAlias.Athletics || competitionDisciplines.FirstOrDefault()?.SectionAlias == GamesAlias.Swimming || competitionDisciplines.FirstOrDefault()?.SectionAlias == GamesAlias.Rowing)
            {
                foreach (var competitionDiscipline in competitionDisciplines)
                {
                    if (competitionDiscipline.DisciplineId.HasValue)
                    {
                        var discipline = disciplinesRepo.GetById(competitionDiscipline.DisciplineId.Value);
                        //user must have permissions set
                        competitionDiscipline.DisciplineName = $"{discipline.Name} {LangHelper.GetDisciplineClassById(discipline.Class ?? 0)}";
                        competitionDiscipline.Format = discipline.Format;
                        competitionDiscipline.PlayersCount = db.CompetitionDisciplineRegistrations.Where(x => x.CompetitionDisciplineId == competitionDiscipline.Id && x.CompetitionResult.Count() > 0 && !(x.CompetitionResult.FirstOrDefault() == null || x.CompetitionResult.FirstOrDefault().Result == "")).Count();
                        resultCompetitionDispciplines.Add(competitionDiscipline);
                    }
                }
                //resultCompetitionDispciplines = resultCompetitionDispciplines.OrderBy(d => d.CategoryName).ToList();
            }
            else
            {
                //resultCompetitionDispciplines = competitionDisciplines.OrderBy(d => d.CategoryName).ToList();
            }
            vm.CompetitionDisciplines = resultCompetitionDispciplines;
            //end get discipline
            //start get competition_ranks data
            vm.CompetitionRanks = leagueRepo.AthleticsLeagueStandings(db.Leagues.Where(x => x.LeagueId == id).FirstOrDefault()?.AthleticLeagueId??0);
            //end 
            //start to get league ranks data
            vm.LeagueRanks = leagueRepo.AthleticsLeagueCompetitionRanking(id, currentSeasonId??0);
            //end
            // Cheng Li : Get competition routes
            vm.Routes = league.CompetitionRoutes.Any()
                ? league.CompetitionRoutes.Select(cr => new BasicRouteViewModel
                {
                    Id = cr.Id,
                    RouteId = cr.RouteId,
                    RouteName = cr.DisciplineRoute.Route,
                    RankName = cr.RouteRank.Rank,
                    Composition = cr.Composition,
                    SecondComposition = cr.SecondComposition,
                    InstrumentName = leagueRepo.GetInstrumentsNames(cr.InstrumentIds)
                })
                : Enumerable.Empty<BasicRouteViewModel>();

            //var teamWithMostFans = league.Teams.OrderByDescending(t => t.TeamsFans.Where(tf => tf.LeageId == id).Count()).FirstOrDefault();

            //Team with the most fans
            var teamRepo = new TeamsRepo();
            var topTeamTuple = teamRepo.GetByMostFans(league.LeagueId);
            var topTeam = topTeamTuple == null ? null : teamRepo.GetById(topTeamTuple.Item1);
            if (topTeam != null)
            {
                vm.TeamWithMostFans = new TeamCompactViewModel
                {
                    FanNumber = topTeamTuple.Item2,
                    TeamId = topTeam.TeamId,
                    Logo = topTeam.Logo,
                    Title = topTeam.Title,
                    LeagueId = league.LeagueId
                };
            }

            UnionsRepo sRepo = new UnionsRepo();
            String sectionName = "";
            if(league.UnionId != null)
            {
                sectionName = sRepo.GetSectionByUnionId(league.UnionId.Value).Alias;
            } else
            {
                sectionName = db.Sections.Where(s => s.SectionId == league.Club.SectionId).FirstOrDefault().Alias;
            }
            vm.SectionName = sectionName;
            bool isTennis = sectionName.Equals(GamesAlias.Tennis);
            var leagueGames = db.GamesCycles.Include(t => t.Auditorium)
                .Include(t => t.GuestTeam)
                .Include(t => t.HomeTeam)
                .Where(gc => gc.Stage.LeagueId == id && gc.IsPublished).ToList();

            // Cheng Li. Add : get fans of coming for karate, gymanstics
            if (sectionName.Equals(GamesAlias.Gymnastic) == true || sectionName.Equals(GamesAlias.Athletics) == true || (league.UnionId != null && IsKarateSection((int)league.LeagueId, LogicaName.League) == true)) {

                if (leagueGames == null || leagueGames.Count == 0)
                {
                    StagesRepo _stagesRepo = new StagesRepo(db);
                    {
                        _stagesRepo.Create(vm.LeagueInfo.Id);
                    }
                    Auditorium aud = new Auditorium();
                    {
                        aud.UnionId = league.UnionId;
                        aud.SeasonId = currentSeasonId;
                        aud.Name = league.PlaceOfCompetition;

                        //AuditoriumsRepo aRepo = new AuditoriumsRepo(db);
                        //aRepo.Create(aud);
                    }
                    
                    var gc_item = new GamesCycle();
                    /**
                     * if time is 1/1/1, error :  Conversion of a datetime2 data type to a datetime data type results out-of-range value.
                     * Reason : have to ensure that Start is greater than or equal to SqlDateTime.MinValue (January 1, 1753) - by default Start equals DateTime.MinValue (January 1, 0001).
                     */
                    System.DateTime timeMinValue = new DateTime(1753, 1, 1);
                    if(timeMinValue > vm.LeagueInfo.LeagueStartDate)
                        gc_item.StartDate = timeMinValue;
                    else
                        gc_item.StartDate = vm.LeagueInfo.LeagueStartDate;

                    gc_item.IsPublished = true;
                    gc_item.Auditorium = aud;

                    int currentUserId = base.CurrUserId;
                    vm.NextGame = GamesService.GetNextGameForSpecial(gc_item, vm.LeagueInfo, currentUserId, id, currentSeasonId);
                    List<LeaguesFan> list = db.LeaguesFans.Where(df => df.UserId == currentUserId && df.LeagueId == league.LeagueId).ToList();

                    if(vm.NextGame != null)
                    {
                        if (list == null || list.Count == 0)
                            vm.NextGame.IsGoing = 0;
                        else
                            vm.NextGame.IsGoing = 1;
                        vm.NextGame.FansList = GamesService.GetGoingFansForSpecial(league.LeagueId, currentUserId);
                    }
                }
            }
            else
            {
                GamesService.UpdateGameSets(leagueGames, sectionName);
                //Next Game
                vm.NextGame = GamesService.GetNextGame(leagueGames, base.CurrUserId, id, currentSeasonId);
            }

            //List of all next games
            if (vm.NextGame != null) {
                vm.NextGames = GamesService.GetNextGames(leagueGames, vm.NextGame.StartDate, currentSeasonId);
            }

            //List of all last games
            vm.LastGames = GamesService.GetLastGames(leagueGames, currentSeasonId);

            //League table
            var rLeague = CacheService.CreateLeagueRankTable(id, currentSeasonId, isTennis);
            if (rLeague != null)
            {
                vm.LeagueTableStages = rLeague.Stages;
                LeaugesController.MakeGroupStages(vm.LeagueTableStages, isEmpty: false);
            }

            if (vm.LeagueTableStages == null || vm.LeagueTableStages.Count == 0)
            {
                vm.LeagueTableStages = CacheService.CreateEmptyRankTable(id, currentSeasonId).Stages;
                LeaugesController.MakeGroupStages(vm.LeagueTableStages, isEmpty: true);
            }

            vm.GameCycles = leagueGames.Select(gc => gc.CycleNum).Distinct().OrderBy(c => c).ToList();
            vm.LeagueTableStages = vm.LeagueTableStages.Where(x => x.Groups.All(y => !y.IsAdvanced)).ToList();
            vm.LeagueTableStages.Reverse();
            return Ok(vm);
        }
        [HttpGet]
        [AllowAnonymous]
        [ResponseType(typeof(RegisteredCompetitionModel))]
        [Route("RegisteredCompetitionAthletes/{disciplineId}")]
        public IHttpActionResult RegisteredCompetitionAthletes(int disciplineId)
        {
            var vm = new RegisteredCompetitionModel();
            var _disciplinesRepo = new DisciplinesRepo();
            var Model = _disciplinesRepo.GetCompetitionDisciplineNoTracking(disciplineId);
            var SeasonId = Model.League.SeasonId;
            vm.Logo = Model.League.Union.Logo;
            vm.Header1 = Model.League.Union.Name;
            vm.Header2 = $"רשימת משתתפים: {Model.League.Name}";
            vm.Header3 = Model.StartTime.HasValue ? ($" במקצוע: {CmsApp.Helpers.@UIHelpers.GetCompetitionDisciplineName(Model.DisciplineId.Value)} בתאריך: {Model.StartTime.Value.ToString("dd-MM-yyy")}") :
                ($" במקצוע: {CmsApp.Helpers.@UIHelpers.GetCompetitionDisciplineName(Model.DisciplineId.Value)}");
            vm.items = new List<SItem>();
            foreach (var participant in Model.CompetitionDisciplineRegistrations.OrderBy(r => r.User.FullName))
            {
                var i = new SItem();
                i.AthleteNumber = participant.User?.AthleteNumbers.FirstOrDefault(x => x.SeasonId == SeasonId)?.AthleteNumber1?.ToString();
                i.FullName = participant.User.FullName;
                i.ClubName = participant.Club.Name;
                i.UserId = participant.UserId;
                i.SeasonId = SeasonId??0;
                if (participant.User.BirthDay.HasValue)
                {
                    i.BirthDay = participant.User.BirthDay?.Year.ToString();
                }
                vm.items.Add(i);
            }
            return Ok(vm);
        }
        [HttpGet]
        [AllowAnonymous]
        [ResponseType(typeof(RegisteredCompetitionModel))]
        [Route("StartList/{disciplineId}")]
        public IHttpActionResult StartList(int disciplineId)
        {
            var _disciplinesRepo = new DisciplinesRepo();
            var disciplineCompetition = _disciplinesRepo.GetCompetitionDisciplineById(disciplineId);
            var discipline = _disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);

            var CompetitionId = disciplineCompetition.CompetitionId;
            var SeasonId = disciplineCompetition.League.SeasonId;
            var DisciplineName = discipline.Name;
            var Format = discipline.Format;
            var CompetitionDisciplineId = disciplineId;
            var CompetitionName = disciplineCompetition.League.Name;
            var CompetitionDate = disciplineCompetition.League.LeagueStartDate.HasValue ? disciplineCompetition.League.LeagueStartDate.Value.ToShortDateString() : "";
            var registrationsWithHeat = disciplineCompetition.CompetitionDisciplineRegistrations.Where(r => r.CompetitionResult.FirstOrDefault() != null);
            var PlayersCount = registrationsWithHeat.Count();
            var groupedByHeat = registrationsWithHeat.OrderBy(r => r.CompetitionResult.FirstOrDefault().Lane ?? int.MaxValue).GroupBy(r => r.CompetitionResult.FirstOrDefault().Heat ?? "").OrderBy(group => group.Key).ToList();


            var numericValues = groupedByHeat.Where(r => int.TryParse(r.Key, out _)).ToList();
            numericValues = numericValues.OrderBy(group => int.Parse(group.Key)).ToList();
            var nonNumericValues = groupedByHeat.Where(r => !int.TryParse(r.Key, out _)).ToList();
            groupedByHeat = numericValues.Concat(nonNumericValues).ToList();

            var userIds = disciplineCompetition.CompetitionDisciplineRegistrations.Select(r => r.UserId).ToList();
            var usersRegsGroupedByUserId = gamesRepo.GetBulkAthleteCompetitionsAchievements(userIds, disciplineCompetition.League.SeasonId.Value);
            var isAsc = LangHelper.IsOrderByFormatAsc(discipline.Format);
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
                        userTopResultByRecordIds = userRegs?.Where(r => r.RecordId?.FirstOrDefault() != null && disciplineRecordId.HasValue && r.RecordId?.FirstOrDefault() == disciplineRecordId.Value && r.AlternativeResult == 0).OrderBy(r => r.SortValue).FirstOrDefault()?.Result ?? string.Empty;
                    }
                    else
                    {
                        userTopResultByRecordIds = userRegs?.Where(r => r.RecordId?.FirstOrDefault() != null && disciplineRecordId.HasValue && r.RecordId?.FirstOrDefault() == disciplineRecordId.Value && r.AlternativeResult == 0).OrderByDescending(r => r.SortValue).FirstOrDefault()?.Result ?? string.Empty;
                    }
                    reg.SeasonalBest = userTopResultByRecordIds;
                }
                else
                {
                    reg.SeasonalBest = string.Empty;
                }
            }
            //}

            DisciplineRecord DisciplineRecord = null ;
            if (disciplineCompetition.IncludeRecordInStartList)
            {
                if (!disciplineRecordId.HasValue)
                {
                    DisciplineRecord = new DisciplineRecord { CompetitionRecord = disciplineCompetition.CompetitionRecord };
                }
                else
                {
                    var disciplineRecordR = _disciplinesRepo.GetDisciplineRecordById(disciplineRecordId.Value);
                    disciplineRecordR.CompetitionRecord = disciplineCompetition.CompetitionRecord;
                    DisciplineRecord = disciplineRecordR;
                }
            }
            var Model = groupedByHeat;
            var vm = new RegisteredCompetitionModel();
            var Model1 = _disciplinesRepo.GetCompetitionDisciplineNoTracking(disciplineId);
            vm.Logo = Model1.League.Union.Logo;
            vm.Header1 = Model1.League.Union.Name;
            vm.Header2 = "StartList-" + DisciplineName +"-"+ CompetitionName +"-"+ CompetitionDate;
            if (!string.IsNullOrEmpty(DisciplineRecord?.CompetitionRecord))
            {
                vm.isCompetitionRecord = true;
            }
            else
            {
                vm.isCompetitionRecord = false;
            }
            vm.isDisciplineRecord = false;
            if (DisciplineRecord!=null)
            {
                var disciplineRecordR = _disciplinesRepo.GetDisciplineRecordById(disciplineRecordId.Value);
                vm.IsraeliRecord = DisciplineRecord.IsraeliRecord;
                vm.CompetitionRecord = DisciplineRecord?.CompetitionRecord;
                vm.IntentionalIsraeliRecord = DisciplineRecord.IntentionalIsraeliRecord;
                if (disciplineRecordR.SeasonRecords.FirstOrDefault(x =>
                            x.SeasonId == disciplineCompetition.League.SeasonId) != null)
                {
                    vm.SeasonRecord =
                        disciplineRecordR.SeasonRecords.FirstOrDefault(x =>
                            x.SeasonId == disciplineCompetition.League.SeasonId).SeasonRecord1;
                }
                vm.isDisciplineRecord = true;
            }
            vm.Format = Format??0;
            vm.stitems = new List<STItem>();
            foreach (var group in groupedByHeat)
            {
                var i = new STItem();
                i.key = group.Key;
                i.sitems = new List<SItem>();
                foreach (var reg in group)
                {
                    var result = reg.CompetitionResult.FirstOrDefault();
                    var j = new SItem();
                    j.AthleteNumber = reg.User?.AthleteNumbers.FirstOrDefault(x => x.SeasonId == SeasonId)?.AthleteNumber1?.ToString();
                    j.Heat = (result?.Heat ?? "");
                    j.Lane = (result?.Lane.ToString() ?? "");
                    j.FullName = reg.User.FullName;
                    j.ClubName = reg.Club.Name;
                    j.SB = LogLigFront.Helpers.UIHelpers.GetCompetitionDisciplineResultString(reg.SeasonalBest, Format);
                    j.UserId = reg.UserId;
                    //j.playerDiscipline = db.PlayerDisciplines.Where(x => x.ClubId == (reg.ClubId??0) && x.SeasonId == SeasonId && x.PlayerId == reg.UserId).Select(x => x.Discipline.Name).FirstOrDefault() ?? null;
                    i.sitems.Add(j);
                }
                i.sitems.OrderBy(t => t.Lane);
                vm.stitems.Add(i);
            }
            return Ok(vm);
        }
        [HttpGet]
        [AllowAnonymous]
        [ResponseType(typeof(RegisteredCompetitionModel))]
        [Route("AthleticsDisciplineResultsByHeat/{disciplineId}")]
        public IHttpActionResult AthleticsDisciplineResultsByHeat(int disciplineId)
        {
            var _disciplinesRepo = new DisciplinesRepo();
            bool wasCached = true;
            var disciplineCompetition = LogLigFront.Helpers.Caching.GetObjectFromCache<CompetitionDiscipline>(LogLigFront.Helpers.CacheKeys.DisciplineCompetitionKey + disciplineId.ToString());
            if (disciplineCompetition == null)
            {
                wasCached = false;
                disciplineCompetition = _disciplinesRepo.GetCompetitionDisciplineById(disciplineId);
                LogLigFront.Helpers.Caching.SetObjectInCache(LogLigFront.Helpers.CacheKeys.DisciplineCompetitionKey + disciplineId.ToString(), 30, disciplineCompetition);
            }


            var discipline = _disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);

            var  CompetitionId = disciplineCompetition.CompetitionId;
            int? unionId = disciplineCompetition.League.UnionId;
            int? sectionId = disciplineCompetition.League.SeasonId;
            var  Format = discipline.Format;
            var  SeasonId = disciplineCompetition.League.SeasonId;
            var  IsResultsManualyRanked = disciplineCompetition.IsResultsManualyRanked;
            if (!wasCached)
            {
                if (disciplineCompetition.League.IsCompetitionLeague)
                {
                    var rankings = LogLigFront.Helpers.Caching.GetObjectFromCache<List<List<CompetitionClubRankedStanding>>>(LogLigFront.Helpers.CacheKeys.CompetitionClubsRankingsKey + disciplineId.ToString() + disciplineCompetition.League.SeasonId.Value.ToString());
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
                        LogLigFront.Helpers.Caching.SetObjectInCache(LogLigFront.Helpers.CacheKeys.CompetitionClubsRankingsKey + disciplineId.ToString() + disciplineCompetition.League.SeasonId.Value.ToString(), 30, rankings);
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
            var  DisciplineName = discipline.Name;
            var  IsCombinedDiscipline = !string.IsNullOrWhiteSpace(discipline.DisciplineType) && (discipline.DisciplineType == "decathlon" || discipline.DisciplineType == "heptathlon");
            var  GenderId = LangHelper.GetGenderCharById(disciplineCompetition.CompetitionAge.gender.Value);
            string logo = disciplineCompetition.League.Union.Logo;
            var  Logo = logo;
            var  IsMultiBattle = disciplineCompetition.IsMultiBattle;
            disciplineCompetition.CompetitionDisciplineRegistrations = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && !string.IsNullOrWhiteSpace(x.CompetitionResult.FirstOrDefault().Result)).ToList();
            disciplineCompetition.CompetitionDisciplineRegistrationsByHeat = disciplineCompetition.CompetitionDisciplineRegistrations.GroupBy(r => r.CompetitionResult.FirstOrDefault().Heat ?? "").ToList();

            var numericValues = disciplineCompetition.CompetitionDisciplineRegistrationsByHeat.Where(r => int.TryParse(r.Key, out _)).ToList();
            numericValues = numericValues.OrderBy(group => int.Parse(group.Key)).ToList();
            var nonNumericValues = disciplineCompetition.CompetitionDisciplineRegistrationsByHeat.Where(r => !int.TryParse(r.Key, out _)).ToList();
            disciplineCompetition.CompetitionDisciplineRegistrationsByHeat = numericValues.Concat(nonNumericValues).ToList();


            var Model = disciplineCompetition;
            var vm = new RegisteredCompetitionModel();
            vm.Logo = Logo;
            vm.Header1 = Model.League.Union.Name;
            if (Model.StartTime.HasValue)
            {
                vm.Header2 = Model.League.Name +"-"+ Model.StartTime.Value.Date.ToString("dd/MM/yyyy");
            }
            else
            {
                vm.Header2 = Model.League.Name;
            }
            vm.Header3 = DisciplineName +"-" +GenderId;
            vm.IsCombinedDiscipline = IsCombinedDiscipline;
            vm.Format = Format??0;
            vm.stitems = new List<STItem>();
            string[] alternativeResultArray = new string[] { "", "DNF", "DQ", "DNS", "NM" };
            foreach (var group in Model.CompetitionDisciplineRegistrationsByHeat)
            {
                var rank = 0;
                var i = new STItem();
                i.key = group.Key;
                i.sitems = new List<SItem>();
                foreach (var reg in group)
                {
                    rank++;
                    var results = reg.CompetitionResult.FirstOrDefault();
                    var j = new SItem();
                    j.Rank = IsResultsManualyRanked ? results.Rank??0 : rank;
                    j.AthleteNumber = reg.User?.AthleteNumbers.FirstOrDefault(x => x.SeasonId == SeasonId)?.AthleteNumber1?.ToString();
                    j.FullName = reg.User.FullName;
                    j.BirthDay = reg.User.BirthDay.HasValue ==true ? reg.User.BirthDay.Value.Year.ToString() : "";
                    j.ClubName = reg.Club.Name;
                    j.Heat = (results?.Heat ?? "");
                    j.Lane = (results?.Lane.ToString() ?? "");
                    if (results?.AlternativeResult > 0)
                    {
                        j.Result =  alternativeResultArray[results.AlternativeResult];
                    }
                    else if (!string.IsNullOrWhiteSpace(results?.Result))
                    {
                        j.Result = LogLigFront.Helpers.@UIHelpers.GetCompetitionDisciplineResultString(results?.Result, Format);
                    }
                    j.Wind = String.Format("{0:0.0}", results.Wind);
                    if (results != null && results.AlternativeResult == 0)
                    {
                        if (IsMultiBattle)
                        {
                            j.Points = results.CombinedPoint?.ToString();
                                            }
                        else if ((Model.League.AthleticLeagueId.HasValue && Model.League.AthleticLeagueId.Value > -1) || !Model.IsForScore)
                        {
                            j.Points = LogLigFront.Helpers.UIHelpers.RemoveRightSidedZeros(results.ClubPoints?.ToString());
                        }
                        else
                        {
                            j.Points = results.ClubPoints?.ToString();
                        }
                    }
                    j.UserId = reg.UserId;
                    //j.playerDiscipline = db.PlayerDisciplines.Where(x => x.ClubId == (reg.ClubId??0) && x.SeasonId == SeasonId && x.PlayerId == reg.UserId).Select(x => x.Discipline.Name).FirstOrDefault() ?? null;
                    i.sitems.Add(j);
                }
                vm.stitems.Add(i);
            }
            return Ok(vm);
        }
        [HttpGet]
        [AllowAnonymous]
        [ResponseType(typeof(RegisteredCompetitionModel))]
        [Route("AthleticsDisciplineResults/{disciplineId}")]
        public IHttpActionResult AthleticsDisciplineResults(int disciplineId)
        {
            var _disciplinesRepo = new DisciplinesRepo();
            bool wasCached = true;
            var disciplineCompetition =LogLigFront.Helpers.Caching.GetObjectFromCache<CompetitionDiscipline>(LogLigFront.Helpers.CacheKeys.DisciplineCompetitionKey + disciplineId.ToString());
            if (disciplineCompetition == null)
            {
                wasCached = false;
                disciplineCompetition = _disciplinesRepo.GetCompetitionDisciplineById(disciplineId);
                LogLigFront.Helpers.Caching.SetObjectInCache(LogLigFront.Helpers.CacheKeys.DisciplineCompetitionKey + disciplineId.ToString(), 30, disciplineCompetition);
                if (disciplineCompetition.LastResultUpdate != null)
                {
                    LogLigFront.Helpers.Caching.SetObjectInCache(LogLigFront.Helpers.CacheKeys.DisciplineCompetitionKeyLastUpdate + disciplineId.ToString(), 30, disciplineCompetition.LastResultUpdate);
                }
            }


            var discipline = _disciplinesRepo.GetById(disciplineCompetition.DisciplineId.Value);

            var  CompetitionId = disciplineCompetition.CompetitionId;
            int? unionId = disciplineCompetition.League.UnionId;
            int? sectionId = disciplineCompetition.League.SeasonId;
            var  Format = discipline.Format;
            var  SeasonId = disciplineCompetition.League.SeasonId;
            var  IsResultsManualyRanked = disciplineCompetition.IsResultsManualyRanked;
            if (!wasCached)
            {
                if (disciplineCompetition.League.IsCompetitionLeague)
                {
                    var rankings = LogLigFront.Helpers.Caching.GetObjectFromCache<List<List<CompetitionClubRankedStanding>>>(LogLigFront.Helpers.CacheKeys.CompetitionClubsRankingsKey + disciplineId.ToString() + disciplineCompetition.League.SeasonId.Value.ToString());
                    if (rankings == null)
                    {
                        rankings = _disciplinesRepo.CupClubRanks(disciplineCompetition.CompetitionId, disciplineCompetition.League.SeasonId.Value);

                        for (int i = 0; i < rankings.Count; i++)
                        {
                            var table = rankings[i];
                            foreach (var rank1 in table)
                            {
                                rank1.Points += rank1.Correction;
                            }
                            table = table.OrderByDescending(t => t.Points).ToList();
                            rankings[i] = table;
                        }
                        LogLigFront.Helpers.Caching.SetObjectInCache(LogLigFront.Helpers.CacheKeys.CompetitionClubsRankingsKey + disciplineId.ToString() + disciplineCompetition.League.SeasonId.Value.ToString(), 30, rankings);
                    }
                }

                if (disciplineCompetition.IsResultsManualyRanked)
                {
                    var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().Rank ?? int.MaxValue).ToList();
                    disciplineCompetition.ResultedCompetitionDisciplineRegistrations = resulted;
                }
                else if (discipline.Format != null && ((discipline.Format.Value >= 6 && discipline.Format.Value <= 8) || discipline.Format.Value == 10 || discipline.Format.Value == 11))
                {
                    var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().SortValue).ThenByDescending(r => r.CompetitionResult.FirstOrDefault()?.LiveSplitOrder ?? int.MinValue);
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
            var  DisciplineName = discipline.Name;
            var  IsCombinedDiscipline = !string.IsNullOrWhiteSpace(discipline.DisciplineType) && (discipline.DisciplineType == "decathlon" || discipline.DisciplineType == "heptathlon");
            var  GenderId = LogLigFront.Helpers.UIHelpers.GetGenderCharById(disciplineCompetition.CompetitionAge.gender.Value);
            string logo = disciplineCompetition.League.Union.Logo;
            var  Logo = logo;
            var  IsMultiBattle = disciplineCompetition.IsMultiBattle;
            disciplineCompetition.ResultedCompetitionDisciplineRegistrations = disciplineCompetition.ResultedCompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && !string.IsNullOrWhiteSpace(x?.CompetitionResult?.FirstOrDefault()?.Result)).ToList();
            var  IsOneRecordHasValue = disciplineCompetition.CompetitionDisciplineRegistrations.Any(r => r.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault()?.Records));
            var  LastFirstStageColumn = -1;
            var  IsAnyAttempt = false;
            if (discipline.Format == 7 || discipline.Format == 10 || discipline.Format == 11)
            {
                IsAnyAttempt = disciplineCompetition.CompetitionDisciplineRegistrations.Any(r => r.CompetitionResult.FirstOrDefault() != null && (
                    !string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault().Attempt1) ||
                    r.CompetitionResult.FirstOrDefault().Attempt1Wind.HasValue ||
                    (r.CompetitionResult.FirstOrDefault().Alternative1.HasValue && r.CompetitionResult.FirstOrDefault().Alternative1.Value > 0)
                ));

            }
            else if (discipline.Format == 6)
            {
                IsAnyAttempt = disciplineCompetition.CompetitionDisciplineRegistrations.Any(r => r.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(r.CompetitionResult.FirstOrDefault().CustomFields));
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
                    if (result != null)
                    {
                        if (!string.IsNullOrWhiteSpace(result.Attempt6) && columnLimit < 6)
                        {
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
                LastFirstStageColumn = columnLimit;
            }

            var disciplineRecordId = _disciplinesRepo.GetCompetitonDisciplineRecord(disciplineCompetition.DisciplineId.Value, disciplineCompetition.CategoryId);
            DisciplineRecord DisciplineRecord = null;
            if (disciplineCompetition.IncludeRecordInStartList)
            {
                if (!disciplineRecordId.HasValue)
                {
                    DisciplineRecord = new DisciplineRecord { CompetitionRecord = disciplineCompetition.CompetitionRecord };
                }
                else
                {
                    var disciplineRecordR = _disciplinesRepo.GetDisciplineRecordById(disciplineRecordId.Value);
                    disciplineRecordR.CompetitionRecord = disciplineCompetition.CompetitionRecord;
                    DisciplineRecord = disciplineRecordR;
                }
            }
            var vm = new RegisteredCompetitionModel();
            var Model = disciplineCompetition;
            vm.Logo = Logo;
            vm.Header1 = Model.League.Union.Name;
            if (Model.StartTime.HasValue)
            {
                vm.Header2 = Model.League.Name + "- " + Model.StartTime.Value.Date.ToString("dd/MM/yyyy");
            }
            else
            {
                vm.Header2 = Model.League.Name;
            }
            vm.Header3 = DisciplineName +"-" +GenderId;
            vm.isDisciplineRecord = DisciplineRecord == null ? false:true;
            if (vm.isDisciplineRecord)
            {
                var disciplineRecordR = _disciplinesRepo.GetDisciplineRecordById(disciplineRecordId.Value);
                vm.IsraeliRecord = DisciplineRecord.IsraeliRecord;
                vm.CompetitionRecord = DisciplineRecord.CompetitionRecord;
                vm.IntentionalIsraeliRecord = DisciplineRecord.IntentionalIsraeliRecord;
                if (disciplineRecordR.SeasonRecords.FirstOrDefault(x =>
                            x.SeasonId == disciplineCompetition.League.SeasonId) != null)
                {
                    vm.SeasonRecord =
                        disciplineRecordR.SeasonRecords.FirstOrDefault(x =>
                            x.SeasonId == disciplineCompetition.League.SeasonId).SeasonRecord1;
                }

            }
            vm.IsCombinedDiscipline = IsCombinedDiscipline;
            vm.IsOneRecordHasValue = IsOneRecordHasValue;
            var rank = 0;
            vm.items = new List<SItem>();
            foreach (var reg in Model.ResultedCompetitionDisciplineRegistrations)
            {
                var i = new SItem();
                CompetitionResult results = null;
                if (reg.CompetitionResult.Count() > 0)
                {
                    results = reg.CompetitionResult.FirstOrDefault();
                }
                rank += 1;
                i.Rank = IsResultsManualyRanked ? results.Rank??0: rank;
                i.AthleteNumber = reg.User?.AthleteNumbers.FirstOrDefault(x => x.SeasonId == SeasonId)?.AthleteNumber1?.ToString();
                i.FullName = reg.User.FullName;
                i.BirthDay = (reg.User.BirthDay.HasValue ? reg.User.BirthDay.Value.Year.ToString() : "");
                i.ClubName = reg.Club.Name;
                i.Heat = results?.Heat;
                i.Lane = results?.Lane?.ToString();
                string[] alternativeResultArray = new string[] { "", "DNF", "DQ", "DNS", "NM" };
                if (results != null)
                {
                    if (results.AlternativeResult > 0)
                    {
                        i.Result = alternativeResultArray[results.AlternativeResult];
                    }
                    else if (!string.IsNullOrWhiteSpace(results.Result))
                    {
                        i.Result = LogLigFront.Helpers.UIHelpers.GetCompetitionDisciplineResultString(results.Result, Format);
                    }
                }
                if (results != null)
                {
                    i.Wind = String.Format("{0:0.0}", results?.Wind);
                }
                if (results != null && results.AlternativeResult == 0)
                {
                    if (IsMultiBattle)
                    {
                        i.Points = results.CombinedPoint.ToString();
                    }
                    else if ((Model.League.AthleticLeagueId.HasValue && Model.League.AthleticLeagueId.Value > -1) || !Model.IsForScore)
                    {
                        i.Points = LogLigFront.Helpers.UIHelpers.RemoveRightSidedZeros(results.ClubPoints.ToString());
                    }
                    else
                    {
                        i.Points = results.ClubPoints?.ToString();
                    }
                }
                i.Records = (!string.IsNullOrWhiteSpace(results.Records) ? results.Records : string.Empty);
                vm.items.Add(i);
            }
            vm.IsAnyAttempt = IsAnyAttempt;
            vm.Format = Format ?? 0;
            vm.items1 = new List<SItem>();
            vm.Cols = Model.GetFormat6CustomFields();
            foreach (var reg in Model.CompetitionDisciplineRegistrations)
            {
                var i = new SItem();
                CompetitionResult result = null;
                if (reg.CompetitionResult.Count() > 0)
                {
                    result = reg.CompetitionResult.FirstOrDefault();
                }
                i.AthleteNumber = reg.User?.AthleteNumbers.FirstOrDefault(x => x.SeasonId == SeasonId)?.AthleteNumber1?.ToString();
                i.FullName = reg.User.FullName;
                i.ClubName = reg.Club.Name;
                i.Result1 = (result != null ? (result.Alternative1.HasValue && result.Alternative1 == 1 ? "-" : (result.Alternative1.HasValue && result.Alternative1 == 2 ? "X" : (string.IsNullOrWhiteSpace(result.Attempt1) ? string.Empty : LogLigFront.Helpers.UIHelpers.removeStringLeft2Zeros(result.Attempt1.ToString())))) : "");
                i.Wind1 = (result != null && result.Attempt1Wind.HasValue ? Math.Round(result.Attempt1Wind.Value, 2).ToString() : "");
                i.Result2 = (result != null ? (result.Alternative2.HasValue && result.Alternative2 == 1 ? "-" : (result.Alternative2.HasValue && result.Alternative2 == 2 ? "X" : (string.IsNullOrWhiteSpace(result.Attempt2) ? string.Empty : LogLigFront.Helpers.UIHelpers.removeStringLeft2Zeros(result.Attempt2.ToString())))) : "");
                i.Wind2 = (result != null && result.Attempt2Wind.HasValue ? Math.Round(result.Attempt2Wind.Value, 2).ToString() : "");
                i.Result3 = (result != null ? (result.Alternative3.HasValue && result.Alternative3 == 1 ? "-" : (result.Alternative3.HasValue && result.Alternative3 == 2 ? "X" : (string.IsNullOrWhiteSpace(result.Attempt3) ? string.Empty : LogLigFront.Helpers.UIHelpers.removeStringLeft2Zeros(result.Attempt3.ToString())))) : "");
                i.Wind3 = (result != null && result.Attempt3Wind.HasValue ? Math.Round(result.Attempt3Wind.Value, 2).ToString() : "");
                i.Result4 = (result != null ? (result.Alternative4.HasValue && result.Alternative4 == 1 ? "-" : (result.Alternative4.HasValue && result.Alternative4 == 2 ? "X" : (string.IsNullOrWhiteSpace(result.Attempt4) ? string.Empty : LogLigFront.Helpers.UIHelpers.removeStringLeft2Zeros(result.Attempt4.ToString())))) : "");
                i.Wind4 = (result != null && result.Attempt4Wind.HasValue ? Math.Round(result.Attempt4Wind.Value, 2).ToString() : "");
                i.Result5 = (result != null ? (result.Alternative5.HasValue && result.Alternative5 == 1 ? "-" : (result.Alternative5.HasValue && result.Alternative5 == 2 ? "X" : (string.IsNullOrWhiteSpace(result.Attempt5) ? string.Empty : LogLigFront.Helpers.UIHelpers.removeStringLeft2Zeros(result.Attempt5.ToString())))) : "");
                i.Wind5 = (result != null && result.Attempt5Wind.HasValue ? Math.Round(result.Attempt5Wind.Value, 2).ToString() : "");
                i.Result6 = (result != null ? (result.Alternative6.HasValue && result.Alternative6 == 1 ? "-" : (result.Alternative6.HasValue && result.Alternative6 == 2 ? "X" : (string.IsNullOrWhiteSpace(result.Attempt6) ? string.Empty : LogLigFront.Helpers.UIHelpers.removeStringLeft2Zeros(result.Attempt6.ToString())))) : "");
                i.Wind6 = (result != null && result.Attempt6Wind.HasValue ? Math.Round(result.Attempt6Wind.Value, 2).ToString() : "");
                
                var alternative = LogLigFront.Helpers.UIHelpers.GetAlternativeResultStringByValue(result?.AlternativeResult ?? 0);
                if (!string.IsNullOrWhiteSpace(alternative))
                {
                    i.FinalResult1 = alternative;
                }
                else
                {
                    i.FinalResult1 = (result != null ? LogLigFront.Helpers.UIHelpers.removeStringLeft2Zeros(result.Result) : "");
                    if(Format == 10 || Format == 11)
                    {
                        i.FinalResult2 = (result != null && result.Wind.HasValue && !string.IsNullOrWhiteSpace(LogLigFront.Helpers.UIHelpers.removeStringLeft2Zeros(result.Result)) ? Math.Round(result.Wind.Value, 2).ToString() : "");
                    }
                    else
                    {
                        //i.FinalResult2 = (result != null ? LogLigFront.Helpers.UIHelpers.removeStringLeft2Zeros(result.Result) : "");
                    }

                }
                if (IsAnyAttempt && Format == 6)
                {
                    i.subitems = new List<SubItem>();
                    for (var k = 0; k < vm.Cols.Count; k++)
                    {
                        var temp = new SubItem();
                        var field = result.GetFormat6CustomField(k, 0);
                        var successIndex = result.GetSuccessIndex6(vm.Cols.Count);
                        var columnSuccess = successIndex >= 0 ? successIndex / 3 : -1;
                        if (reg.CompetitionResult.Count() > 0)
                        {
                            result = reg.CompetitionResult.FirstOrDefault();
                        }
                        temp.item1 = result.GetFormat6CustomField(k, 0);
                        temp.item2 = result.GetFormat6CustomField(k, 1);
                        temp.item3 = result.GetFormat6CustomField(k, 2);
                        i.subitems.Add(temp);
                    }
                }
                vm.items1.Add(i);
            }
            return Ok(vm);
        }

        [AllowAnonymous]
        [ResponseType(typeof(LeaguePageVeiwModel))]
        [Route("Tennis/{leagueId}")]
        public IHttpActionResult GetLeagueTennis(int leagueId, string ln)
        {
            {
                CultureModel.ChangeCulture(ln);

                League league = db.Leagues.Find(leagueId);
                if (league == null)
                {
                    return NotFound();
                }

                int? currentSeasonId = league.SeasonId;

                LeaguePageVeiwModel vm = new LeaguePageVeiwModel();
                //League Info
                vm.LeagueInfo = new LeagueInfoVeiwModel(league);

                // Cheng Li : Get competition routes
                vm.Routes = league.CompetitionRoutes.Any()
                    ? league.CompetitionRoutes.Select(cr => new BasicRouteViewModel
                    {
                        Id = cr.Id,
                        RouteId = cr.RouteId,
                        RouteName = cr.DisciplineRoute.Route,
                        RankName = cr.RouteRank.Rank,
                        Composition = cr.Composition,
                        SecondComposition = cr.SecondComposition,
                        InstrumentName = leagueRepo.GetInstrumentsNames(cr.InstrumentIds)
                    })
                    : Enumerable.Empty<BasicRouteViewModel>();

                //var teamWithMostFans = league.Teams.OrderByDescending(t => t.TeamsFans.Where(tf => tf.LeageId == id).Count()).FirstOrDefault();

                //Team with the most fans
                var teamRepo = new TeamsRepo();
                var topTeamTuple = teamRepo.GetByMostFans(league.LeagueId);
                var topTeam = topTeamTuple == null ? null : teamRepo.GetById(topTeamTuple.Item1);
                if (topTeam != null)
                {
                    vm.TeamWithMostFans = new TeamCompactViewModel
                    {
                        FanNumber = topTeamTuple.Item2,
                        TeamId = topTeam.TeamId,
                        Logo = topTeam.Logo,
                        Title = topTeam.Title,
                        LeagueId = league.LeagueId
                    };
                }

                UnionsRepo sRepo = new UnionsRepo();
                String sectionName = "";
                if (league.UnionId != null)
                {
                    sectionName = sRepo.GetSectionByUnionId(league.UnionId.Value).Alias;
                }
                else
                {
                    sectionName = db.Sections.Where(s => s.SectionId == league.Club.SectionId).FirstOrDefault().Alias;
                }
                vm.SectionName = sectionName;
                bool isTennis = sectionName.Equals(GamesAlias.Tennis);
                var leagueGames = db.GamesCycles.Include(t => t.Auditorium)
                    .Include(t => t.GuestTeam)
                    .Include(t => t.HomeTeam)
                    .Where(gc => gc.Stage.LeagueId == leagueId && gc.IsPublished).ToList();

                // Cheng Li. Add : get fans of coming for karate, gymanstics
                if (sectionName.Equals(GamesAlias.Gymnastic) == true || (league.UnionId != null && IsKarateSection((int)league.LeagueId, LogicaName.League) == true))
                {

                    if (leagueGames == null || leagueGames.Count == 0)
                    {
                        StagesRepo _stagesRepo = new StagesRepo(db);
                        {
                            _stagesRepo.Create(vm.LeagueInfo.Id);
                        }
                        Auditorium aud = new Auditorium();
                        {
                            aud.UnionId = league.UnionId;
                            aud.SeasonId = currentSeasonId;
                            aud.Name = league.PlaceOfCompetition;

                            //AuditoriumsRepo aRepo = new AuditoriumsRepo(db);
                            //aRepo.Create(aud);
                        }

                        var gc_item = new GamesCycle();
                        /**
                         * if time is 1/1/1, error :  Conversion of a datetime2 data type to a datetime data type results out-of-range value.
                         * Reason : have to ensure that Start is greater than or equal to SqlDateTime.MinValue (January 1, 1753) - by default Start equals DateTime.MinValue (January 1, 0001).
                         */
                        System.DateTime timeMinValue = new DateTime(1753, 1, 1);
                        if (timeMinValue > vm.LeagueInfo.LeagueStartDate)
                            gc_item.StartDate = timeMinValue;
                        else
                            gc_item.StartDate = vm.LeagueInfo.LeagueStartDate;

                        gc_item.IsPublished = true;
                        gc_item.Auditorium = aud;

                        int currentUserId = base.CurrUserId;
                        vm.NextGame = GamesService.GetNextGameForSpecial(gc_item, vm.LeagueInfo, currentUserId, leagueId, currentSeasonId);
                        List<LeaguesFan> list = db.LeaguesFans.Where(df => df.UserId == currentUserId && df.LeagueId == league.LeagueId).ToList();

                        if (vm.NextGame != null)
                        {
                            if (list == null || list.Count == 0)
                                vm.NextGame.IsGoing = 0;
                            else
                                vm.NextGame.IsGoing = 1;
                            vm.NextGame.FansList = GamesService.GetGoingFansForSpecial(league.LeagueId, currentUserId);
                        }
                    }
                }
                else
                {
                    GamesService.UpdateGameSets(leagueGames, sectionName);
                    //Next Game
                    vm.NextGame = GamesService.GetNextGame(leagueGames, base.CurrUserId, leagueId, currentSeasonId);
                    if(vm.NextGame!=null)
                        vm.NextGame.gameType = 0;
                }

                //List of all next games
                if (vm.NextGame != null)
                {
                    vm.NextGames = GamesService.GetNextGames(leagueGames, vm.NextGame.StartDate, currentSeasonId);
                }

                //List of all last games
                vm.LastGames = GamesService.GetLastGames(leagueGames, currentSeasonId);

                //League table
                var rLeague = CacheService.CreateLeagueRankTable(leagueId, currentSeasonId, isTennis);
                if (rLeague != null)
                {
                    vm.LeagueTableStages = rLeague.Stages;
                    LeaugesController.MakeGroupStages(vm.LeagueTableStages, isEmpty: false);
                }

                if (vm.LeagueTableStages == null || vm.LeagueTableStages.Count == 0)
                {
                    vm.LeagueTableStages = CacheService.CreateEmptyRankTable(leagueId, currentSeasonId).Stages;
                    LeaugesController.MakeGroupStages(vm.LeagueTableStages, isEmpty: true);
                }

                vm.GameCycles = leagueGames.Select(gc => gc.CycleNum).Distinct().OrderBy(c => c).ToList();
                vm.LeagueTableStages = vm.LeagueTableStages.Where(x => x.Groups.All(y => !y.IsAdvanced)).ToList();
                vm.LeagueTableStages.Reverse();
                return Ok(vm);
            }
        }

        public static void MakeGroupStages(List<RankStage> stages, bool isEmpty)
        {
            if (isEmpty)
            {
                foreach (var team in stages.SelectMany(x => x.Groups).SelectMany(t => t.Teams))
                {
                    team.Points = 0;
                    team.Draw = 0;
                    team.Address = string.Empty;
                    team.Position = "";
                    team.PositionNumber = 0;
                    team.Games = 0;
                    team.Wins = 0;
                    team.Loses = 0;
                    team.SetsWon = 0;
                    team.SetsLost = 0;
                    team.HomeTeamFinalScore = 0;
                    team.GuestTeamScore = 0;
                    team.TotalPointsScored = 0;
                    team.TotalHomeTeamPoints = 0;
                    team.TotalGuesTeamPoints = 0;
                    team.TotalPointsLost = 0;
                    team.HomeTeamScore = 0;
                    team.GuestTeamScore = 0;
                    team.GuesTeamFinalScore = 0;
                    team.Logo = string.Empty;
                }
            }

            foreach (var stage in stages)
            {
                var nameStage = "";
                if (stage.Groups.Count() > 0 && stage.Groups.All(g => g.IsAdvanced))
                {
                    var firstGroup = stage.Groups.FirstOrDefault();
                    if (firstGroup != null && firstGroup.PlayoffBrackets != null)
                    {
                        int numOfBrackets = (int)firstGroup.PlayoffBrackets;
                        switch (numOfBrackets)
                        {
                            case 1:
                                nameStage = Messages.Final; break;
                            case 2:
                                nameStage = Messages.Semifinals; break;
                            case 4:
                                nameStage = Messages.Quarter_finals;
                                break;
                            case 8:
                                nameStage = Messages.Final_Eighth;
                                break;
                            default:
                                nameStage = (numOfBrackets * 2) + Messages.FinalNumber;
                                break;
                        }
                    }
                }
                else
                {
                    stage.Playoff = false;
                    foreach (var group in stage.Groups)
                    {
                        var teamsPositions = group.Teams.Select(x => x.Position).ToArray();
                        for (var i = 0; i < group.Teams.Count; i++)
                        {
                            int numOfBrackets = (int)group.PlayoffBrackets;
                            if (i != 0 && teamsPositions[i] == teamsPositions[i - 1])
                            {
                                group.Teams[i].Position = "-";
                                group.Teams[i].PositionNumber = i;
                            }
                            else
                            {
                                group.Teams[i].Position = (i + 1).ToString();
                                group.Teams[i].PositionNumber = (i + 1);
                            }
                        }
                    }
                    continue;
                }
                stage.Playoff = true;
                stage.Name = nameStage;
                foreach (var group in stage.Groups)
                {
                    for (var i = 0; i < group.Teams.Count; i++)
                    {
                        int numOfBrackets = (int)group.PlayoffBrackets;
                        if (i % ((numOfBrackets)) == 0)
                        {
                            group.Teams[i].Position = (i + 1).ToString();
                            group.Teams[i].PositionNumber = (i + 1);
                        }
                        else
                        {
                            group.Teams[i].Position = "-";
                            group.Teams[i].PositionNumber = i;
                        }
                    }

                }
            }
        }

        [AllowAnonymous]
        [ResponseType(typeof(List<LeaguesListItemViewModel>))]
        [Route("AthleticsCompetitions/{unionId}")]
        public IHttpActionResult GetAthleticCompetitions(int unionId)
        {
            SeasonsRepo seasonrepo = new SeasonsRepo();
            int seasonId = seasonrepo.GetCurrentByUnionId(unionId) ?? 0;

            var result = leagueRepo.GetByUnion(unionId, seasonId)
                        .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                        .Select(l => new LeaguesListItemViewModel
                        {
                            Id = l.LeagueId,
                            AthleticId = l.AthleticLeagueId ?? 0,
                            Title = l.Name,
                            TotalTeams = l.LeagueTeams.Where(t => t.Teams.IsArchive == false).Count(),
                            TotalDisciplines = l.CompetitionDisciplines.Count(cdr => !cdr.IsDeleted),
                            TotalFans = l.LeagueTeams.Join(db.TeamsFans, tf => tf.TeamId, lt => lt.TeamId, (lt, tf) => tf.UserId).Distinct().Count(),
                            TotalPlayers = l.CompetitionDisciplines.Sum(t => t.CompetitionDisciplineRegistrations
                                                 .Count(cdr => !cdr.CompetitionDiscipline.IsDeleted)),
                            FansIds = l.LeagueTeams.Join(db.TeamsFans, tf => tf.TeamId, lt => lt.TeamId, (lt, tf) => tf.UserId).Distinct(),
                            Logo = l.Logo,
                            StartDate = l.LeagueStartDate == null?DateTime.MinValue.ToString("dd/MM/yyyy") :l.LeagueStartDate.Value.ToString("dd/MM/yyyy"),
                            StartDate_datetime = l.LeagueStartDate == null ? DateTime.MinValue : l.LeagueStartDate.Value,
                            Address = l.PlaceOfCompetition
                        }).OrderByDescending(t=>t.StartDate_datetime).ToList();

            return Ok(result);
        }

        /// <summary>
        /// מחזירת רשימת ליגות בלי קבוצות
        /// </summary>
        /// <param name="section">שם ענף</param>
        /// <returns></returns>
        /// // GET: api/Leauges/Section/{section}
        [AllowAnonymous]
        [ResponseType(typeof(List<LeaguesListItemViewModel>))]
        [Route("Section/{section}")]
        public IHttpActionResult GetLeagues(string section)
        {
            var sectionObj = db.Sections
                .Include(s => s.Unions)
                .FirstOrDefault(s => s.Alias == section);

            if (sectionObj == null)
                return Ok(Enumerable.Empty<LeaguesListItemViewModel>());

            var unions = sectionObj.Unions.Where(u => u.Leagues.Count > 0 && u.IsArchive == false && u.UnionId != 56 && u.UnionId != 71);
            // for app only some unions have support
            var allLeagues = new List<League>();

            foreach (var union in unions)
            {
                if (string.Equals(section, GamesAlias.BasketBall, StringComparison.CurrentCultureIgnoreCase) && union.UnionId != 31)
                    continue;
                if (string.Equals(section, GamesAlias.NetBall, StringComparison.CurrentCultureIgnoreCase) && union.UnionId != 15)
                    continue;
                if (string.Equals(section, GamesAlias.Gymnastic, StringComparison.CurrentCultureIgnoreCase) && union.UnionId != 36)
                    continue;

                var lastSeason = db.Seasons
                    .OrderByDescending(x => x.Id)
                    .First(l => l.UnionId == union.UnionId);

                allLeagues.AddRange(union.Leagues.Where(l => l.EilatTournament == null && !l.IsArchive && l.SeasonId == lastSeason.Id));
            }

            var result = allLeagues.OrderBy(l => l.SortOrder)
                .Select(l => new LeaguesListItemViewModel
                {
                    Id = l.LeagueId,
                    Title = l.Name,
                    TotalTeams = l.LeagueTeams.Count(t => t.Teams.IsArchive == false),
                    TotalFans = l.LeagueTeams.Join(db.TeamsFans, tf => tf.TeamId, lt => lt.TeamId, (lt, tf) => tf.UserId).Distinct().Count(),
                    FansIds = l.LeagueTeams.Join(db.TeamsFans, tf => tf.TeamId, lt => lt.TeamId, (lt, tf) => tf.UserId).Distinct(),
                    Logo = l.Logo
                }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// מחזירת רשימת ליגות בלי קבוצות
        /// </summary>
        /// <param name="section">שם ענף</param>
        /// <returns></returns>
        /// // GET: api/Leauges/Section/{section}
        [AllowAnonymous]
        [ResponseType(typeof(List<LeaguesListItemViewModel>))]
        [Route("SectionET/{section}")]
        public IHttpActionResult GetLeaguesET(string section)
        {
            var sectionId = db.Sections
                .Include(s => s.Unions)
                .FirstOrDefault(s => s.Alias == section)
                ?.SectionId ?? 0;

            var allLeagues = leagueRepo.GetLastSeasonLeaguesBySection(sectionId)
                .Where(l => l.EilatTournament == true &&
                            !l.IsArchive)
                .ToList();

            if (string.Equals(section, SectionAliases.Netball, StringComparison.CurrentCultureIgnoreCase))
            {
                allLeagues = allLeagues.Where(x => x.UnionId == 15).ToList();
            }

            var result = allLeagues
                .OrderByDescending(l => l.Place.HasValue)
                .ThenBy(l => l.Place)
                .Select(l => new LeaguesListItemViewModel
                {
                    Id = l.LeagueId,
                    Title = l.Name,
                    TotalTeams = l.LeagueTeams.Count(t => t.Teams.IsArchive == false),
                    TotalFans = l.TeamsFans.Count,
                    Logo = l.Logo
                })
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Get tournaments from club
        /// </summary>
        /// <param name="clubid">שם ענף</param>
        /// <returns></returns>
        /// // GET: api/Leauges/tournament/{clubid}
        [AllowAnonymous]
        [ResponseType(typeof(List<LeaguesListItemViewModel>))]
        [Route("Tournament/{clubid}")]
        public IHttpActionResult GetLeaguesET(int clubid)
        {
            SeasonsRepo seasonrepo = new SeasonsRepo();
            if(clubid==0|| clubid == null)
            {
                clubid = (int)CurrentUser.UsersJobs.Where(x => x.ClubId.HasValue == true).FirstOrDefault().ClubId;
            }
            var seasonId = seasonrepo.GetLastSeasonIdByCurrentClubId(clubid);

            var allLeagues = leagueRepo.GetByClub(clubid, seasonId)
                    .Where(l => l.EilatTournament ?? false && !l.IsArchive && l.EilatTournament != null && ((bool)l.EilatTournament) == true)
                .OrderByDescending(l => l.Place.HasValue).ThenBy(l => l.Place)
                .Select(l => new LeaguesListItemViewModel
                {
                    Id = l.LeagueId,
                    Title = l.Name,
                    TotalTeams = l.LeagueTeams.Where(t => t.Teams.IsArchive == false).Count(),
                    TotalFans = l.TeamsFans.Count,
                    Logo = l.Logo
                }).ToList();

            return Ok(allLeagues);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ln"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [ResponseType(typeof(LeagueRank))]
        [Route("Rank/{id}")]
        public IHttpActionResult Rank(int id, string ln)
        {
            CultureModel.ChangeCulture(ln);
            var leagueRank = new LeagueRank
            {
                Stages = new List<StageRank>()
            };

            int? seasonId = seasonsRepo.GetLastSeasonByLeagueId(id);
            RankLeague rLeague = CacheService.CreateLeagueRankTable(id, seasonId) ?? new RankLeague();

            foreach (var stage in rLeague.Stages)
            {
                if (stage.Groups.All(g => g.IsAdvanced))
                    continue;
                var rankStage = new StageRank();
                var nameStage = "";
                if (stage.Groups.Any() && stage.Groups.All(g => g.IsAdvanced))
                {
                    var firstGroup = stage.Groups.FirstOrDefault();
                    if (firstGroup != null && firstGroup.PlayoffBrackets != null)
                    {
                        int numOfBrackets = (int)firstGroup.PlayoffBrackets;
                        switch (numOfBrackets)
                        {
                            case 1:
                                nameStage = Messages.Final; break;
                            case 2:
                                nameStage = Messages.Semifinals; break;
                            case 4:
                                nameStage = Messages.Quarter_finals;
                                break;
                            case 8:
                                nameStage = Messages.Final_Eighth;
                                break;
                            default:
                                nameStage = (numOfBrackets * 2) + Messages.FinalNumber;
                                break;
                        }
                    }
                }
                else
                {
                    nameStage = Messages.Stage + stage.Number;
                }

                rankStage.NameStage = nameStage;
                rankStage.Groups = new List<GroupRank>();

                foreach (var group in stage.Groups)
                {
                    var rankGroup = new GroupRank
                    {
                        NameGroup = @group.Title,
                        Teams = new List<TeamRank>()
                    };

                    for (var i = 0; i < group.Teams.Count(); i++)
                    {
                        var rankTeam = new TeamRank
                        {
                            Team = @group.Teams[i].Title,
                            Logo = @group.Teams[i].Logo
                        };

                        int numOfBrackets = (int)group.PlayoffBrackets;
                        rankTeam.Position = i % (numOfBrackets) == 0 ? (i + 1).ToString() : "-";

                        rankGroup.Teams.Add(rankTeam);
                    }
                    rankStage.Groups.Add(rankGroup);
                }
                leagueRank.Stages.Add(rankStage);
            }
            leagueRank.Stages.Reverse();
            return Ok(leagueRank);
        }

        [ResponseType(typeof(List<ImageGalleryViewModel>))]
        [Route("ImageGallery/{legueId}")]
        public IHttpActionResult GetImageGallery(int legueId)
        {
            List<ImageGalleryViewModel> result = new List<ImageGalleryViewModel>();
            string dirPath = ConfigurationManager.AppSettings["LeagueUrl"] + "\\" + legueId;
            if (!Directory.Exists(dirPath))
            {
                return Ok(result);
            }

            UsersRepo usersRepo = new UsersRepo();
            var allfiles = System.IO.Directory.EnumerateFiles(dirPath, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".jpeg") || s.EndsWith(".jpg") || s.EndsWith(".png"));

            foreach (var file in allfiles)
            {
                try
                {

                    FileInfo info = new FileInfo(file);
                    var uid = int.Parse(info.Name.Substring(0, info.Name.IndexOf("__")));
                    User user = usersRepo.GetById(uid);
                    if (user != null)
                    {
                        var elem = new ImageGalleryViewModel();
                        elem.Created = info.CreationTime;
                        elem.url = legueId + "/" + info.Name;
                        elem.User = new UserModel
                        {
                            Id = user.UserId,
                            Name = user.FullName != null ? user.FullName : user.UserName,
                            Image = user.Image == null && user.UsersType.TypeRole == "players" ? PlayerService.GetPlayerProfile(user, null).Image : user.Image,
                            UserRole = user.UsersType.TypeRole
                        };
                        result.Add(elem);
                    }

                }
                catch (Exception e)
                {
                    continue;
                }

                // Do something with the Folder or just add them to a list via nameoflist.add();
            }

            result = result.OrderByDescending(x => x.Created).ToList();

            return Ok(result);
        }

        [ResponseType(typeof(List<ImageGalleryViewModel>))]
        [Route("ImageGalleryOfClub/{legueId}")]
        public IHttpActionResult GetImageGalleryOfClub(int legueId)
        {
            List<ImageGalleryViewModel> result = new List<ImageGalleryViewModel>();
            string dirPath = ConfigurationManager.AppSettings["LeagueUrl"] + "\\" + legueId;
            if (!Directory.Exists(dirPath))
            {
                return Ok(result);
            }

            UsersRepo usersRepo = new UsersRepo();
            var allfiles = System.IO.Directory.EnumerateFiles(dirPath, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".jpeg") || s.EndsWith(".jpg") || s.EndsWith(".png"));

            foreach (var file in allfiles)
            {
                try
                {

                    FileInfo info = new FileInfo(file);
                    var uid = int.Parse(info.Name.Substring(0, info.Name.IndexOf("__")));
                    User user = usersRepo.GetById(uid);
                    if (user != null)
                    {
                        var elem = new ImageGalleryViewModel();
                        elem.Created = info.CreationTime;
                        elem.url = legueId + "/" + info.Name;
                        elem.User = new UserModel
                        {
                            Id = user.UserId,
                            Name = user.FullName ?? user.UserName,
                            Image = user.Image == null && user.UsersType.TypeRole == "players" ? PlayerService.GetPlayerProfile(user, null).Image : user.Image,
                            UserRole = user.UsersType.TypeRole
                        };
                        result.Add(elem);
                    }

                }
                catch (Exception e)
                {
                    continue;
                }

                // Do something with the Folder or just add them to a list via nameoflist.add();
            }

            result = result.OrderByDescending(x => x.Created).ToList();

            return Ok(result);
        }

        [Route("DeleteGallery/{leagueId}/{galleryName}")]
        [AllowAnonymous]
        [HttpGet]
        public IHttpActionResult DeleteImageGallery(int leagueId, string galleryName)
        {
            string filePath = ConfigurationManager.AppSettings["LeagueUrl"] + "\\" + leagueId + "\\" + galleryName;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Ok();
            }
            else
            {
                return null;
            }
        }
        public class ImageGalleryViewModel
        {
            public string url { get; set; }
            public DateTime Created { get; set; }
            public UserModel User { get; set; }
        }

        public class UserModel
        {
            public int Id { get; set; } // User id
            public String Name { get; set; } // User name
            public String Image { get; set; } // User image
            public String UserRole { get; set; }
        }
    }
}
