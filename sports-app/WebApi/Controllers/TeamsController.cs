using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using AppModel;
using DataService;
using DataService.DTO;
using WebApi.Models;
using WebApi.Services;
using System.Configuration;
using System.IO;
using DataService.Utils;
using DataService.LeagueRank;
using WebApi.Helpers;

namespace WebApi.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/Teams")]
    public class TeamsController : BaseLogLigApiController
    {
        private SeasonsRepo _seasonsRepo;
        private TeamsService _teamsService = null;
        private SectionsRepo _sectionsRepo = null;
        private LeagueRepo _leagueRepo = null;
        private TeamsRepo _teamRepo = new TeamsRepo();
        private GamesRepo _gamesRepo;

        protected SeasonsRepo seasonsRepo
        {
            get
            {
                if (_seasonsRepo == null)
                {
                    _seasonsRepo = new SeasonsRepo(db);
                }
                return _seasonsRepo;
            }
        }

        protected TeamsService teamsService
        {
            get
            {
                if (_teamsService == null)
                {
                    _teamsService = new TeamsService(db);
                }
                return _teamsService;
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

        protected LeagueRepo leagueRepo
        {
            get
            {
                if (_leagueRepo == null)
                {
                    _leagueRepo = new LeagueRepo(db);
                }
                return _leagueRepo;
            }
        }

        /// <summary>
        /// מחזיר קיר הודעות של קבוצה
        /// </summary>
        /// <param name="teamId">ID קבוצה</param>
        /// <returns></returns>
        [ResponseType(typeof(List<WallThreadViewModel>))]
        [Route("Messages/{teamId}")]
        public IHttpActionResult GetTeamMessages(int teamId)
        {
            var MessageThreads = MessagesService.GetTeamMessages(teamId);
            return Ok(MessageThreads);
        }

        /// <summary>
        /// מחזיר דף קבוצה לפי ליגה
        /// </summary>
        /// <param name="teamId">ID קבוצה</param>
        /// <param name="leagueId">ID ליגה</param>
        /// <returns></returns>
        [ResponseType(typeof(TeamPageViewModel))]
        [Route("{teamId}/League/{leagueId}")]
        public IHttpActionResult GetTeam(int teamId, int leagueId)
        {
            try
            {
                var team = teamsService.GetTeamById(teamId);
                if (team == null)
                {
                    return NotFound();
                }
                
                var vm = new TeamPageViewModel();
                var section = sectionsRepo.GetByLeagueId(leagueId);
                var sRepo = new SectionsRepo();
                var sectionName = sRepo.GetSectionByTeamId(teamId)?.Alias;
                vm.SectionName = sectionName;
                var isTennis = sectionName != null ? sectionName.Equals(GamesAlias.Tennis) : false;

                var currentSeasonId = seasonsRepo.GetLastSeasonByLeagueId(leagueId);
                if (currentSeasonId == null)
                {
                    var ll = leagueRepo.GetById(leagueId);
                    currentSeasonId = ll.SeasonId;
                }

                vm.TeamInfo = TeamsService.GetTeamInfo(team, leagueId, currentSeasonId, isTennis);
                if (vm.TeamInfo == null)
                {
                    return NotFound();
                }
                vm.TeamInfo.SectionName = sectionName;

                var teamGames = team.GuestTeamGamesCycles
                                        .Concat(team.HomeTeamGamesCycles)
                                        .Where(tg => tg.Stage.LeagueId == leagueId && tg.IsPublished).ToList();
                var currentUserId = Convert.ToInt32(User.Identity.Name);

                GamesService.UpdateGameSets(teamGames, section: section?.Alias);
                //Next Game
                vm.NextGame = GamesService.GetNextGame(teamGames, currentUserId, leagueId, currentSeasonId);
                //List of all next games
                vm.NextGames = GamesService.GetTeamNextGames(leagueId, teamId, DateTime.Now, currentSeasonId, currentUserId);
                //Last Game
                vm.LastGame = GamesService.GetLastGame(teamGames, currentSeasonId);
                //Last Games
                vm.LastGames = GamesService.GetLastGames(teamGames, currentSeasonId).OrderBy(x => x.StartDate);
                //League Info
                if(!isTennis)
                {
                    var leagues = leagueRepo.GetLastSeasonLeaguesBySection(section.SectionId)
                        .Where(l => db.LeagueTeams.Where(lt => lt.TeamId == team.TeamId).Select(lt => lt.LeagueId).Contains(l.LeagueId))
                        .ToList();
                    vm.Leagues = leagues.Select(l => new LeagueInfoVeiwModel(l)).ToList();
                }
                else
                {
                    var leagues = leagueRepo.GetLastSeasonLeaguesBySection(section.SectionId)
                        .Where(l => db.TeamRegistrations.Where(lt => lt.TeamId == team.TeamId).Select(lt => lt.LeagueId).Contains(l.LeagueId))
                        .ToList();
                    vm.Leagues = leagues.Select(l => new LeagueInfoVeiwModel(l)).ToList();

                }
                //Fans
                vm.Fans = TeamsService.GetTeamFans(team.TeamId, leagueId, CurrUserId);
                //Game Cycles
                vm.GameCycles = teamGames.Select(gc => gc.CycleNum).Distinct().OrderBy(c => c).ToList();
                //Players
                 vm.Players = currentSeasonId != null ? PlayerService.GetActivePlayersByTeamId(teamId, currentSeasonId.Value, leagueId) :
                                                       new List<CompactPlayerViewModel>();
                vm.Players = vm.Players.OrderBy(t => t.TennisPositionOrder ?? int.MaxValue).ThenBy(c => c.FullName).ToList();

                // Set friends status for each of the players
                FriendsService.AreFriends(vm.Players, currentUserId);
                //Jobs
                vm.Jobs = TeamsService.GetTeamJobsByTeamId(teamId, currentUserId, currentSeasonId);

                vm.MessageThreads = MessagesService.GetTeamMessages(teamId);

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
                vm.LeagueTableStages = vm.LeagueTableStages.Where(x => x.Groups.All(y => !y.IsAdvanced)).ToList();

                // get team score
                if(vm.LeagueTableStages.Count() > 0)
                {
                    foreach(var rs in vm.LeagueTableStages.OrderByDescending(l => l.StageId))
                    {
                        var group = rs.Groups.FirstOrDefault(gr => gr.Teams.Any(t => t.Id == team.TeamId));
                        if(group != null)
                        {
                            var rTeam = group.Teams.FirstOrDefault(t => t.Id == team.TeamId);
                            if(rTeam != null)
                            {
                                if (!isTennis)
                                {
                                    vm.TeamInfo.Place = rTeam.PositionNumber;
                                    vm.TeamInfo.Ratio = rTeam.SetsLost + ":" + rTeam.SetsWon;

                                    if (rTeam.Games == 0)
                                    {
                                        vm.TeamInfo.SuccsessLevel = 0;
                                    }
                                    else
                                    {
                                        double wins = rTeam.Wins;
                                        double games = rTeam.Games;
                                        var ratio = (wins / games) * 100;
                                        vm.TeamInfo.SuccsessLevel = Convert.ToInt32(ratio);
                                    }
                                }
                                else
                                {
                                    var index = group.Teams.FindIndex(gt => gt.Id == team.TeamId);
                                    vm.TeamInfo.Place = index + 1;
                                    vm.TeamInfo.Ratio = rTeam.TennisInfo.PlayersSetsLost + "-" + rTeam.TennisInfo.PlayersSetsWon;
                                    if (rTeam.TennisInfo.Matches == 0)
                                    {
                                        vm.TeamInfo.SuccsessLevel = 0;
                                    }
                                    else
                                    {
                                        double wins = rTeam.TennisInfo.Wins;
                                        double games = rTeam.TennisInfo.Matches;
                                        var ratio = (wins / games) * 100;
                                        vm.TeamInfo.SuccsessLevel = Convert.ToInt32(ratio);
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
                vm.LeagueId = leagueId;
                vm.LeagueTableStages.Reverse();
                return Ok(vm);
            }
            catch (Exception ex)
            {

                return InternalServerError(ex);
            }

        }

        [ResponseType(typeof(TeamPageViewModel))]
        [Route("{teamId}/ClubByNew/{clubId}")]
        public IHttpActionResult GetTeamClubByNew(int teamId, int clubId)
        {
            var isSchoolTeam = false;
            try
            {
                var sRepo = new SectionsRepo();
                var vm = new ClubTeamPageViewModel();
                vm.SectionName = sRepo.GetSectionByTeamId(teamId)?.Alias;
                var isTennis = vm.SectionName != null ? vm.SectionName.Equals(GamesAlias.Tennis) : false;

                var team = teamsService.GetTeamById(teamId);

                var seasonId = teamRepo.GetSeasonIdByTeamId(teamId);

                if (team == null)
                {
                    return NotFound();
                }

                vm.ClubId = clubId;
                vm.Logo = team.Logo;
                vm.Title = team.Title;
                vm.TeamId = team.TeamId;
                vm.Image = team.PersonnelPic;
                vm.Description = team.Description;
                vm.TeamStandings = getTeamStandings(teamId, clubId);

                // Check if the team is school team.
                var schoolTeam = db.SchoolTeams.Where(st => st.TeamId == teamId).FirstOrDefault();
                if (schoolTeam != null)
                {
                    isSchoolTeam = true;
                }

                {
                    if (vm.TeamStandings != null && vm.TeamStandings.Count > 0)
                    {
                        var externalTeamName = db.TeamStandingGames.Where(ts => ts.TeamId == teamId).FirstOrDefault().ExternalTeamName;

                        foreach (var ts in vm.TeamStandings)
                        {
                            if (ts.Team == team.Title || externalTeamName == ts.Team)
                            {
                                if (vm.SectionName == "basketball")
                                {
                                    vm.Ratio = ts.Papf.Replace("/", ":");
                                    vm.Place = ts.Rank;
                                    if (ts.Games == 0)
                                    {
                                        vm.SuccsessLevel = 0;
                                    }
                                    else
                                    {
                                        double wins = ts.Wins;
                                        double games = ts.Games;
                                        var ratio = (wins / games) * 100;
                                        vm.SuccsessLevel = Convert.ToInt32(ratio);
                                    }
                                }
                                else if (vm.SectionName == "Soccer")
                                {
                                    vm.Ratio = ts.Papf.Replace("-", ":");
                                    vm.Place = ts.Rank;
                                    if (ts.Games == 0)
                                    {
                                        vm.SuccsessLevel = 0;
                                    }
                                    else
                                    {
                                        double wins = ts.Wins;
                                        double games = ts.Games;
                                        var ratio = (wins / games) * 100;
                                        vm.SuccsessLevel = Convert.ToInt32(ratio);
                                    }
                                }
                                else
                                {
                                    vm.Ratio = ts.Papf.Replace("/", ":");
                                    vm.Place = ts.Rank;
                                    if (ts.Games == 0)
                                    {
                                        vm.SuccsessLevel = 0;
                                    }
                                    else
                                    {
                                        double wins = ts.Wins;
                                        double games = ts.Games;
                                        var ratio = (wins / games) * 100;
                                        vm.SuccsessLevel = Convert.ToInt32(ratio);
                                    }
                                }

                                break;
                            }
                        }
                    }

                    vm.Fans = new List<UserBaseViewModel>();
                    foreach (var fan in team.TeamsFans)
                    {

                        var fanModel = new UserBaseViewModel
                        {
                            UserName = fan.User.UserName,
                            Id = fan.User.UserId,
                            Image = fan.User.Image == null && fan.User.UsersType.TypeRole == "players" ? PlayerService.GetPlayerProfile(fan.User, seasonId).Image : fan.User.Image,
                            UserRole = fan.User.UsersType.TypeRole,
                            FriendshipStatus = FriendsService.AreFriends(fan.User.UserId, CurrUserId),
                            CanRcvMsg = fan.UserId != CurrUserId ? true : false
                        };
                        vm.Fans.Add(fanModel);
                    }

                    int? currentSeasonId = seasonsRepo.GetLastSeasonIdByCurrentClubId(clubId);
                    var currentUserId = Convert.ToInt32(User.Identity.Name);

                    vm.Players = currentSeasonId != null ? PlayerService.GetActivePlayersByClubTeam(teamId, currentSeasonId.Value, CurrUserId, clubId) :
                                                              new List<CompactPlayerViewModel>();
                    vm.Players = vm.Players.OrderBy(x => x.FullName).ToList();

                    //Jobs
                    vm.Jobs = TeamsService.GetTeamJobsByTeamId(teamId, currentUserId, seasonId);

                    vm.MessageThreads = MessagesService.GetTeamMessages(teamId);

                    vm.NextGames = new List<ClubGame>();
                    vm.LastGames = new List<ClubGame>();


                    TeamScheduleScrapper next = null;

                    IEnumerable<TeamScheduleScrapper> schedules = _teamRepo.GetTeamGamesFromScrapper(clubId, teamId).OrderBy(x => x.StartDate);
                    foreach (var game in schedules)
                    {

                        var userGoing = game.Users.Where(x => x.UserId == currentUserId);
                        var IsGoing = 0;
                        var status = "waiting";
                        if (userGoing.Count() != 0)
                        {
                            IsGoing = 1;
                        }
                        if (game.StartDate > DateTime.Now)
                        {
                            if (next == null)
                                next = game;
                            vm.NextGames.Add(new ClubGame
                            {
                                GameId = game.Id,
                                StartDate = game.StartDate,
                                HomeTeam = game.HomeTeam,
                                GuestTeam = game.GuestTeam,
                                Score = game.Score,
                                Auditorium = game.Auditorium,
                                IsGoing = IsGoing,
                                Status = status,
                                SchedulerScrapperGamesId = game.SchedulerScrapperGamesId,
                            });
                        }
                        else
                        {
                            status = "ended";
                            vm.LastGames.Add(new ClubGame
                            {
                                GameId = game.Id,
                                StartDate = game.StartDate,
                                HomeTeam = game.HomeTeam,
                                GuestTeam = game.GuestTeam,
                                Score = game.Score,
                                Auditorium = game.Auditorium,
                                IsGoing = IsGoing,
                                Status = status,
                                SchedulerScrapperGamesId = game.SchedulerScrapperGamesId
                            });
                        }
                    }

                    vm.NextGame = GamesService.ParseNextClubGame(currentUserId, next, (int)currentSeasonId);

                    // Set friends status for each of the players
                    FriendsService.AreFriends(vm.Players, currentUserId);
                }
                return Ok(vm);
            }
            catch (Exception ex)
            {

                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// מחזיר דף קבוצה לפי ליגה
        /// </summary>
        /// <param name="teamId">ID קבוצה</param>
        /// <param name="clubId">ID ליגה</param>
        /// <returns></returns>
        [ResponseType(typeof(ClubTeamPageViewModel))]
        [Route("{teamId}/byclub/{clubId}")]
        public IHttpActionResult GetTeamByClubId(int teamId, int clubId)
        {
            var seasonId = teamRepo.GetSeasonIdByTeamId(teamId);
            var leagueTeam = db.LeagueTeams.FirstOrDefault(lt => lt.TeamId == teamId && lt.SeasonId == seasonId);
            if (leagueTeam != null)
            {
                return GetTeam(teamId, leagueTeam.LeagueId);
            }
            else
            {
                return GetClubTeam(teamId, clubId);
            }
        }
        /// <summary>
        /// מחזיר דף קבוצה לפי ליגה
        /// </summary>
        /// <param name="teamId">ID קבוצה</param>
        /// <param name="clubId">ID ליגה</param>
        /// <returns></returns>
        [ResponseType(typeof(ClubTeamPageViewModel))]
        [Route("{teamId}/Club/{clubId}")]
        public IHttpActionResult GetClubTeam(int teamId, int clubId)
        {
            var isSchoolTeam = false;
            try
            {
                var sRepo = new SectionsRepo();
                var vm = new ClubTeamPageViewModel();
                vm.SectionName = sRepo.GetSectionByTeamId(teamId)?.Alias;
                var isTennis = vm.SectionName != null ? vm.SectionName.Equals(GamesAlias.Tennis) : false;

                var team = teamsService.GetTeamById(teamId);

                var seasonId = teamRepo.GetSeasonIdByTeamId(teamId);

                if (team == null)
                {
                    return NotFound();
                }

                vm.ClubId = clubId;
                vm.Logo = team.Logo;
                vm.Title = team.Title;
                vm.TeamId = team.TeamId;
                vm.Image = team.PersonnelPic;
                vm.Description = team.Description;
                vm.TeamStandings = getTeamStandings(teamId, clubId);

                // Check if the team is school team.
                var schoolTeam = db.SchoolTeams.Where(st => st.TeamId == teamId).FirstOrDefault();
                if(schoolTeam != null)
                {
                    isSchoolTeam = true;
                }
                
                {
                    if (vm.TeamStandings != null && vm.TeamStandings.Count > 0)
                    {
                        var externalTeamName = db.TeamStandingGames.Where(ts => ts.TeamId == teamId).FirstOrDefault().ExternalTeamName;

                        foreach (var ts in vm.TeamStandings)
                        {
                            if (ts.Team == team.Title || externalTeamName == ts.Team)
                            {
                                if (vm.SectionName == "basketball")
                                {
                                    vm.Ratio = ts.Papf.Replace("/", ":");
                                    vm.Place = ts.Rank;
                                    if (ts.Games == 0)
                                    {
                                        vm.SuccsessLevel = 0;
                                    }
                                    else
                                    {
                                        double wins = ts.Wins;
                                        double games = ts.Games;
                                        var ratio = (wins / games) * 100;
                                        vm.SuccsessLevel = Convert.ToInt32(ratio);
                                    }
                                }
                                else if (vm.SectionName == "Soccer")
                                {
                                    vm.Ratio = ts.Papf.Replace("-", ":");
                                    vm.Place = ts.Rank;
                                    if (ts.Games == 0)
                                    {
                                        vm.SuccsessLevel = 0;
                                    }
                                    else
                                    {
                                        double wins = ts.Wins;
                                        double games = ts.Games;
                                        var ratio = (wins / games) * 100;
                                        vm.SuccsessLevel = Convert.ToInt32(ratio);
                                    }
                                }
                                else
                                {
                                    vm.Ratio = ts.Papf.Replace("/", ":");
                                    vm.Place = ts.Rank;
                                    if (ts.Games == 0)
                                    {
                                        vm.SuccsessLevel = 0;
                                    }
                                    else
                                    {
                                        double wins = ts.Wins;
                                        double games = ts.Games;
                                        var ratio = (wins / games) * 100;
                                        vm.SuccsessLevel = Convert.ToInt32(ratio);
                                    }
                                }

                                break;
                            }
                        }
                    }

                    vm.Fans = new List<UserBaseViewModel>();
                    foreach (var fan in team.TeamsFans)
                    {

                        var fanModel = new UserBaseViewModel
                        {
                            UserName = fan.User.UserName,
                            Id = fan.User.UserId,
                            Image = fan.User.Image == null && fan.User.UsersType.TypeRole == "players" ? PlayerService.GetPlayerProfile(fan.User, seasonId).Image : fan.User.Image,
                            UserRole = fan.User.UsersType.TypeRole,
                            FriendshipStatus = FriendsService.AreFriends(fan.User.UserId, CurrUserId)
                        };
                        vm.Fans.Add(fanModel);
                    }

                    int? currentSeasonId = seasonsRepo.GetLastSeasonIdByCurrentClubId(clubId);
                    var currentUserId = Convert.ToInt32(User.Identity.Name);

                    vm.Players = currentSeasonId != null ? PlayerService.GetActivePlayersByClubTeam(teamId, currentSeasonId.Value, CurrUserId, clubId) :
                                                              new List<CompactPlayerViewModel>();
                    vm.Players = vm.Players.OrderBy(x => x.FullName).ToList();

                    //Jobs
                    vm.Jobs = TeamsService.GetTeamJobsByTeamId(teamId, currentUserId, seasonId);

                    vm.MessageThreads = MessagesService.GetTeamMessages(teamId);

                    vm.NextGames = new List<ClubGame>();
                    vm.LastGames = new List<ClubGame>();


                    TeamScheduleScrapper next = null;

                    IEnumerable<TeamScheduleScrapper> schedules = _teamRepo.GetTeamGamesFromScrapper(clubId, teamId).OrderBy(x => x.StartDate);
                    foreach (var game in schedules)
                    {

                        var userGoing = game.Users.Where(x => x.UserId == currentUserId);
                        var IsGoing = 0;
                        var status = "waiting";
                        if (userGoing.Count() != 0)
                        {
                            IsGoing = 1;
                        }
                        if (game.StartDate > DateTime.Now)
                        {
                            if (next == null)
                                next = game;
                            vm.NextGames.Add(new ClubGame
                            {
                                GameId = game.Id,
                                StartDate = game.StartDate,
                                HomeTeam = game.HomeTeam,
                                GuestTeam = game.GuestTeam,
                                Score = game.Score,
                                Auditorium = game.Auditorium,
                                IsGoing = IsGoing,
                                Status = status,
                                SchedulerScrapperGamesId = game.SchedulerScrapperGamesId,
                            });
                        }
                        else
                        {
                            status = "ended";
                            vm.LastGames.Add(new ClubGame
                            {
                                GameId = game.Id,
                                StartDate = game.StartDate,
                                HomeTeam = game.HomeTeam,
                                GuestTeam = game.GuestTeam,
                                Score = game.Score,
                                Auditorium = game.Auditorium,
                                IsGoing = IsGoing,
                                Status = status,
                                SchedulerScrapperGamesId = game.SchedulerScrapperGamesId
                            });
                        }
                    }

                    vm.NextGame = GamesService.ParseNextClubGame(currentUserId, next, (int)currentSeasonId);

                    // Set friends status for each of the players
                    FriendsService.AreFriends(vm.Players, currentUserId);
                }
                return Ok(vm);
            }
            catch (Exception ex)
            {

                return InternalServerError(ex);
            }

        }

        public List<TeamStandingsForm> getTeamStandings(int teamId, int clubId)
        {
            var team = db.Teams.Find(teamId);
            if (team == null)
            {
                return null;
            }

            if (!team.ClubTeams.Any(l => l.ClubId == clubId))
            {
                return null;
            }

            var listtsf = new List<TeamStandingsForm>();
            foreach (var model in _teamRepo.GetTeamStandings(clubId, teamId).ToList())
            {
                var tsf = new TeamStandingsForm
                {
                    Id = model.Id,
                    Team = model.Team,
                    Games = model.Games,
                    Home = model.Home,
                    Last5 = model.Last5,
                    Lost = model.Lost,
                    Papf = model.Papf,
                    Pts = model.Pts,
                    Rank = model.Rank,
                    Road = model.Road,
                    ScoreRoad = model.ScoreRoad,
                    ScoreHome = model.ScoreHome,
                    Wins = model.Wins,
                    PlusMinusField = model.PlusMinusField
                };
                listtsf.Add(tsf);
            }

            return listtsf;
        }

        /// <summary>
        /// מחזיר רשימת ליגות והקבוצות מתחתם לפי שם ענף
        /// </summary>
        /// <param name="section">שם ענף</param>
        /// <returns></returns>
        /// // GET: api/Teams/Section/{section}
        [ResponseType(typeof(List<LeagueTeamsViewModel>))]
        [Route("Section/{section}")]
        public IHttpActionResult GetTeams(string section)
        {
            var sectionObj = db.Sections.Where(s => s.Alias == section)
                .Include(s => s.Unions)
                .FirstOrDefault();

            var unions = sectionObj.Unions.Where(u => u.Leagues.Count > 0 && u.IsArchive == false);
            var allLeagues = new List<League>();

            foreach (var union in unions)
            {
                allLeagues.AddRange(union.Leagues.Where(l => l.LeagueTeams.Count > 0 && l.IsArchive == false && l.SeasonId == (int)seasonsRepo.GetLasSeasonByUnionId(union.UnionId)));
            }

            // WARNING: The code below gets the "global" last season for all section and unions, this will NOT return the correct value for the current union or section.
            //var lastSeason = _seasonsRepository.GetLastSeason();

            var result = allLeagues.Select(l =>
                new LeagueTeamsViewModel
                {
                    LeagueId = l.LeagueId,
                    Name = l.Name,
                    Teams = l.LeagueTeams.Where(t => t.Teams.IsArchive == false && t.SeasonId == (int)seasonsRepo.GetLasSeasonByUnionId((int)l.UnionId))
                    .Select(t => new TeamCompactViewModel(t.Teams, l.LeagueId, t.SeasonId))
                }).ToList();

            return Ok(result);
        }

        [ResponseType(typeof(List<ClubTeamInfoViewModel>))]
        [Route("Clubs/{unionId}")]
        public IHttpActionResult GetClubTeams(int unionId)
        {
            var allClubs = new List<Club>();

            var seasonId = seasonsRepo.GetLasSeasonByUnionId(unionId).Value;
            allClubs = db.Clubs.Where(l => l.ClubTeams.Count > 0 && l.IsArchive == false && l.SeasonId == seasonId).ToList();


            // WARNING: The code below gets the "global" last season for all section and unions, this will NOT return the correct value for the current union or section.
            //var lastSeason = _seasonsRepository.GetLastSeason();

            var result = allClubs.Select(l =>
                new ClubTeamInfoViewModel
                {
                    Id = l.ClubId,
                    Title = l.Name,
                    Teams = l.ClubTeams.Where(t => t.IsBlocked == false && t.SeasonId == (int)seasonsRepo.GetLasSeasonByUnionId((int)l.UnionId))
                    .Select(t => new ClubTeamViewModel
                    {
                        TeamId = t.TeamId,
                        Title = t.Team.Title,
                        ParentId = l.ClubId,
                        Logo = t.Team.Logo
                    }).ToList(),
                }).ToList();

            return Ok(result);
        }
        [ResponseType(typeof(List<ClubTeamInfoViewModel>))]
        [Route("AthleticClubs/{unionId}")]
        public IHttpActionResult GetAthleticClubTeams(int unionId)
        {
            var allClubs = new List<Club>();

            var seasonId = seasonsRepo.GetLasSeasonByUnionId(unionId).Value;
            allClubs = db.Clubs.Where(l => l.ClubTeams.Count > 0 && l.IsArchive == false && l.SeasonId == seasonId && l.UnionId == unionId).ToList();


            // WARNING: The code below gets the "global" last season for all section and unions, this will NOT return the correct value for the current union or section.
            //var lastSeason = _seasonsRepository.GetLastSeason();

            var result = allClubs.Select(l =>
                new ClubTeamInfoViewModel
                {
                    Id = l.ClubId,
                    Title = l.Name,
                    Teams = l.ClubTeams.Where(t => t.IsBlocked == false && t.SeasonId == (int)seasonsRepo.GetLasSeasonByUnionId((int)l.UnionId))
                    .Select(t => new ClubTeamViewModel
                    {
                        TeamId = t.TeamId,
                        Title = t.Team.Title,
                        ParentId = l.ClubId,
                        Logo = t.Team.Logo
                    }).ToList(),
                }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// מחזיר רשימת ליגות והקבוצות מתחתם לפי שם ענף
        /// </summary>
        /// <param name="section">שם ענף</param>
        /// <param name="unionId">id of the union to determine last season</param>
        /// <returns></returns>
        /// // GET: api/Teams/Section/{section}
        [ResponseType(typeof(List<LeagueTeamsViewModel>))]
        [Route("Section/{section}/{unionId}")]
        public IHttpActionResult GetTeams(string section, int unionId)
        {
            var sectionObj = db.Sections.Include(s => s.Unions).FirstOrDefault(s => s.Alias == section);

            if (sectionObj == null)
            {
                return NotFound();
            }

            var unions = sectionObj.Unions.Where(u => u.Leagues.Count > 0 && u.IsArchive == false);
            var allLeagues = new List<League>();

            foreach (var union in unions)
            {
                allLeagues.AddRange(union.Leagues.Where(l => l.LeagueTeams.Count > 0 && l.IsArchive == false));
            }

            var lastSeasonId = seasonsRepo.GetLasSeasonByUnionId(unionId);
            if (lastSeasonId.HasValue)
            {
                var result = allLeagues.Select(l =>
                    new LeagueTeamsDto()
                    {
                        LeagueId = l.LeagueId,
                        Name = l.Name,
                        Teams = l.LeagueTeams.Where(t => t.Teams.IsArchive == false && t.SeasonId == lastSeasonId)
                            .Select(t => new TeamInformationDto()
                            {
                                Team = new TeamDto()
                                {
                                    TeamId = t.TeamId,
                                    Title = t.Teams.Title,
                                    LeagueId = l.LeagueId,
                                    Logo = t.Teams.Logo
                                },
                                TeamInformation =
                                    t.Teams != null &&
                                    t.Teams.TeamsDetails.FirstOrDefault(td => td.SeasonId == lastSeasonId) != null
                                        ? new TeamDetailsDto
                                        {
                                            TeamName =
                                                t.Teams.TeamsDetails.FirstOrDefault(_td => _td.SeasonId == lastSeasonId)
                                                    .TeamName
                                        }
                                        : null
                            })
                    })
                    .ToList()
                    .Select(x => new LeagueTeamsViewModel
                    {
                        LeagueId = x.LeagueId,
                        Name = x.Name,
                        Teams = x.Teams.Select(t => new TeamCompactViewModel()
                        {
                            TeamId = t.Team.TeamId,
                            Title = t.TeamInformation != null ? t.TeamInformation.TeamName : t.Team.Title,
                            LeagueId = x.LeagueId,
                            Logo = t.Team.Logo
                        })
                    })
                    .ToList();

                return Ok(result);
            }

            return Ok(new List<LeagueTeamsViewModel>());
        }


        [ResponseType(typeof(List<ImageGalleryViewModel>))]
        [Route("ImageGallery/{teamId}")]
        public IHttpActionResult GetImageGallery(int teamId)
        {
            var result = new List<ImageGalleryViewModel>();
            var dirPath = ConfigurationManager.AppSettings["TeamUrl"] + "\\" + teamId;
            if (!Directory.Exists(dirPath))
            {
                return Ok(result);
            }

            var usersRepo = new UsersRepo();
            var allfiles = System.IO.Directory.EnumerateFiles(dirPath, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".jpeg") || s.EndsWith(".jpg") || s.EndsWith(".png"));
            
            foreach (var file in allfiles)
            {
                try
                {

                    var info = new FileInfo(file);
                    var uid = int.Parse(info.Name.Substring(0, info.Name.IndexOf("__")));
                    var user = usersRepo.GetById(uid);
                    if (user != null)
                    {
                        var elem = new ImageGalleryViewModel();
                        elem.Created = info.CreationTime;
                        elem.url = teamId + "/" + info.Name;
                        elem.User = new UserModel
                        {
                            Id = user.UserId,
                            Name = user.FullName ?? user.UserName,
                            Image = user.Image == null && user.UsersType.TypeRole == "players" ? PlayerService.GetPlayerProfile(user, null).Image : user.Image,
                            UserRole = user.UsersType.TypeRole
                        };
                        result.Add(elem);
                    }
                    
                } catch(Exception e)
                {
                    continue;
                }

                // Do something with the Folder or just add them to a list via nameoflist.add();
            }

            result = result.OrderByDescending(x => x.Created).ToList();

            return Ok(result);
        }

        [Route("DeleteGallery/{teamId}/{galleryName}")]
        [HttpGet]
        public IHttpActionResult DeleteImageGallery(int teamId, string galleryName)
        {
            var filePath = ConfigurationManager.AppSettings["TeamUrl"] + "\\" + teamId + "\\" + galleryName;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Ok();
            } else
            {
                return null;
            }
        }

        [Route("TimerScrap")]
        public IHttpActionResult GetRunScrapping()
        {
            var db = new DataEntities();
            var teamGames = db.TeamScheduleScrapperGames.ToList();
            var controller = new TeamsController();
            var existedSchedulerGame = db.TeamScheduleScrappers.ToList();

            if (existedSchedulerGame.Count > 0)
            {
                db.TeamScheduleScrappers.RemoveRange(existedSchedulerGame);
                db.SaveChanges();
            }
            for (var i = 0; i < teamGames.Count; i++)
            {
                var gameDetail = teamGames[i];
                controller.SaveGamesUrl(gameDetail.TeamId.Value, gameDetail.GameUrl, gameDetail.ExternalTeamName, gameDetail.ClubId.Value, gameDetail.SeasonId ?? 0, true);
            }

            var teamStandings = db.TeamStandingGames.ToList();
            for (var j = 0; j < teamStandings.Count; j++)
            {
                var standingDetail = teamStandings[j];
                controller.SaveTeamStandingGameUrl(standingDetail.TeamId, standingDetail.ClubId.Value, standingDetail.GamesUrl, standingDetail.ExternalTeamName, standingDetail.SeasonId ?? 0);
            }
            return Ok();
        }

        public void SaveGamesUrl(int teamId, string url, string teamName, int clubId, int seasonId, bool isScraper = false)
        {
            try
            {
                var service = new DataService.Services.ScrapperService();

                var repo = new BaseRepo();
                var sectionId = repo.GetSectionByTeamId(teamId).SectionId;
                if (sectionId == 1)
                {
                    var result = service.SchedulerScraper(url);

                    service.Quit();

                    var isTeamExist = teamRepo.GetById(teamId) != null;


                    if (!isTeamExist && result.Any(t => t.HomeTeam != teamName || t.GuestTeam != teamName))
                    {
                        return;
                    }

                    var scheduleId = gamesRepo.SaveTeamGameUrl(teamId, url, clubId, teamName, seasonId);

                    gamesRepo.UpdateGamesSchedulesFromDto(result, clubId, scheduleId, url, isScraper);

                    ProcessHelper.ClosePhantomJSProcess();

                    return;
                }
                else if (sectionId == 7)
                {
                    var result = service.FootballSchedulerScraper(url);

                    service.Quit();

                    var isTeamExist = teamRepo.GetById(teamId) != null;


                    if (!isTeamExist && result.Any(t => t.HomeTeam != teamName || t.GuestTeam != teamName))
                    {
                        return;
                    }

                    var scheduleId = gamesRepo.SaveTeamGameUrl(teamId, url, clubId, teamName, seasonId);

                    gamesRepo.UpdateGamesSchedulesFromDto(result, clubId, scheduleId, url, isScraper);

                    ProcessHelper.ClosePhantomJSProcess();

                    return;
                }
                else
                {
                    return;
                }


            }
            catch (Exception e)
            {
                ProcessHelper.ClosePhantomJSProcess();

                return;
            }

        }
        
        public void SaveTeamStandingGameUrl(int teamId, int clubId, string url, string teamName, int seasonId)
        {
            try
            {
                var service = new DataService.Services.ScrapperService();

                var repo = new BaseRepo();
                var sectionId = repo.GetSectionByTeamId(teamId).SectionId;

                if (sectionId == 1)
                {
                    var isTeamExist = teamRepo.GetById(teamId) != null;
                    var result = service.StandingScraper(url);

                    if (!isTeamExist && result.Any(t => t.Team != teamName))
                    {
                        ProcessHelper.ClosePhantomJSProcess();
                        return;
                    }

                    var standingGameId = teamRepo.SaveTeamStandingUrl(teamId, clubId, url, teamName, seasonId);

                    var isSuccess = standingGameId > 0;

                    if (isSuccess)
                    {
                        gamesRepo.UpdateTeamStandingsFromModel(result, standingGameId, url);

                        service.Quit();
                    }
                    ProcessHelper.ClosePhantomJSProcess();
                    return;
                }
                else if (sectionId == 7)
                {
                    var isTeamExist = teamRepo.GetById(teamId) != null;
                    var result = service.SoccerStandingScraper(url);

                    if (!isTeamExist && result.Any(t => t.Team != teamName))
                    {
                        ProcessHelper.ClosePhantomJSProcess();
                        return;
                    }

                    var standingGameId = teamRepo.SaveTeamStandingUrl(teamId, clubId, url, teamName, seasonId);

                    var isSuccess = standingGameId > 0;

                    if (isSuccess)
                    {
                        gamesRepo.UpdateTeamStandingsFromModel(result, standingGameId, url);

                        service.Quit();
                    }
                    ProcessHelper.ClosePhantomJSProcess();
                    return;
                }
                else
                {
                    return;
                }

            }
            catch (Exception e)
            {
                ProcessHelper.ClosePhantomJSProcess();
                return;
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        protected TeamsRepo teamRepo
        {
            get
            {
                if (_teamRepo == null)
                {
                    _teamRepo = new TeamsRepo(db);
                }
                return _teamRepo;
            }
        }

        protected GamesRepo gamesRepo
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

        private bool TeamExists(int id)
        {
            return db.Teams.Count(e => e.TeamId == id) > 0;
        }
    }
}