using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using WebApi.Models;
using WebApi.Services;
using AppModel;
using DataService;
using DataService.DTO;
using System.Collections.Generic;
using System;
using DataService.LeagueRank;

namespace WebApi.Controllers
{
    [RoutePrefix("api/Player")]
    public class PlayerController : BaseLogLigApiController
    {
        readonly SeasonsRepo _seasonsRepo = new SeasonsRepo();
        private GamesRepo _gamesRepo = null;
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

        private PlayersRepo _playersRepo;
        protected PlayersRepo playersRepo
        {
            get
            {
                if (_playersRepo == null)
                {
                    _playersRepo = new PlayersRepo(db);
                }
                return _playersRepo;
            }
        }
        private LeagueRepo _leagueRepo = null;
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

        private UsersRepo _usersRepo = null;
        protected UsersRepo usersRepo
        {
            get
            {
                if (_usersRepo == null)
                {
                    _usersRepo = new UsersRepo(db);
                }
                return _usersRepo;
            }
        }

        // GET: api/Player/5
        /// <summary>
        /// Get player by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="unionId"></param>
        /// <param name="leagueId"></param>
        /// <param name="clubId"></param>
        /// <returns></returns>
        [ResponseType(typeof(PlayerProfileViewModel))]
        public IHttpActionResult Get(int id, int? unionId = null, int? leagueId = null, int? clubId = null)
        {
            // what is IsActive?
            //User player = db.Users.Include(u => u.PlayerFiles).FirstOrDefault(u => u.UserId == id && u.IsArchive == false && u.IsActive); 
            User player = db.Users.Include(u => u.PlayerFiles).FirstOrDefault(u => u.UserId == id);
            //if(leagueId == null)
            //{
            //    var league = db.LeagueTeams.Join(db.TeamsPlayers, first => first.TeamId, second => second.TeamId, (first, second) => (new
            //    {
            //        id = first.LeagueId,
            //        userid = second.UserId,
            //    })).Where(x => x.userid == id).LastOrDefault();
            //    leagueId = league != null ? league.id : 0;
            //}
            if (player == null)
            {
                return NotFound();
            }
            int? seasonId = unionId != null ? _seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) :
            leagueId != null ? _seasonsRepo.GetLastSeasonByLeagueId(leagueId.Value) : (int?)null;

            PlayerProfileViewModel vm = PlayerService.GetPlayerProfile(player, seasonId);
            var teamsRepo = new TeamsRepo();

            if (unionId == null && leagueId != null)
            {
                LeagueRepo leagueRepo = new LeagueRepo();
                League league = leagueRepo.GetById((int)leagueId);
                unionId = league != null ? league.UnionId : null;
                seasonId = unionId != null ? _seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) : (int?)null;
            }

            if (unionId == null && clubId != null)
            {
                SeasonsRepo _seasonRepo = new SeasonsRepo();
                seasonId = _seasonRepo.GetLastSeasonIdByCurrentClubId((int)clubId);
                vm.Teams = teamsRepo.GetClubPlayerTeams(id, seasonId, clubId);
            }
            else
            {
                vm.Teams = teamsRepo.GetPlayerPositions(id, seasonId, leagueId);
            }
            
            vm.FriendshipStatus = FriendsService.AreFriends(id, CurrUserId);

            if (User.Identity.IsAuthenticated)
            {
                vm.Friends = FriendsService.GetAllFanFriends(id, base.CurrUserId);
            }

            vm.Games = GamesService.GetPlayerGames(player.UserId, seasonId);
            // Cheng Li need for Assaf 9/19
            //vm.Competitions = GamesService.GetTennisCompetitionPlayerGames(player.UserId, seasonId);
            GamesService.UpdateGameSets(vm.Games);
            return Ok(vm);
        }

        [ResponseType(typeof(PlayerProfileViewModel))]
        [Route("Players/{id}/clubId/{clubId}/leagueId/{leagueId}")]
        public IHttpActionResult GetFromClubAndLeague(int id, int? clubId, int? leagueId )
        {
            // what is IsActive?
            //User player = db.Users.Include(u => u.PlayerFiles).FirstOrDefault(u => u.UserId == id && u.IsArchive == false && u.IsActive); 
            int? unionId = null;
            User player = db.Users.Include(u => u.PlayerFiles).FirstOrDefault(u => u.UserId == id);
            if (player == null)
            {
                return NotFound();
            }
            int? seasonId = unionId != null ? _seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) :
            leagueId != null ? _seasonsRepo.GetLastSeasonByLeagueId(leagueId??0) :null;
            if(seasonId == null)
            {
                seasonId = _seasonsRepo.GetLastSeasonIdByCurrentClubId(clubId??0);
                if(clubId == 0)
                {
                    var temp = db.TeamsPlayers.Where(x => x.UserId == id);
                    var temp1 = temp!=null?temp.OrderByDescending(g => g.SeasonId)?.FirstOrDefault():null;
                    clubId = temp1?.ClubId;
                }
                if(leagueId == 0 && clubId ==0)
                {
                    var temp = db.TeamsPlayers.Where(x => x.UserId == id)?.OrderByDescending(g => g.SeasonId)?.FirstOrDefault();
                    leagueId = temp != null ? (int)temp.LeagueId : 0; 
                }
            }
            PlayerProfileViewModel vm = PlayerService.GetPlayerProfileClubApp(player, seasonId,leagueId,clubId);
            var teamsRepo = new TeamsRepo();

            if (unionId == null && leagueId>0)
            {
                LeagueRepo leagueRepo = new LeagueRepo();
                League league = leagueRepo.GetById((int)leagueId);
                unionId = league != null ? league.UnionId : null;
                seasonId = unionId != null ? _seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) : (int?)null;
            }

            if (unionId == null && clubId>0)
            {
                SeasonsRepo _seasonRepo = new SeasonsRepo();
                seasonId = _seasonRepo.GetLastSeasonIdByCurrentClubId((int)clubId);
                vm.Teams = teamsRepo.GetClubPlayerTeams(id, seasonId, clubId);
            }
            else
            {
                vm.Teams = teamsRepo.GetPlayerPositions(id, seasonId, leagueId);
            }

            vm.FriendshipStatus = FriendsService.AreFriends(id, CurrUserId);

            if (User.Identity.IsAuthenticated)
            {
                vm.Friends = FriendsService.GetAllFanFriends(id, base.CurrUserId);
            }

            vm.Games = GamesService.GetPlayerGames(player.UserId, seasonId);
            // Cheng Li need for Assaf 9/19
            //vm.Competitions = GamesService.GetTennisCompetitionPlayerGames(player.UserId, seasonId);
            GamesService.UpdateGameSets(vm.Games);
            return Ok(vm);
        }

        /// <summary>
        /// Get player games
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="unionId"></param>
        /// <returns></returns>
        [Route("Games/{teamId}")]
        public IHttpActionResult GetPlayerGames(int teamId, int? unionId = null)
        {
            //var season = db.TeamsDetails.Where(x => x.TeamId == teamId && x.Season.StartDate < DateTime.Now &&
            //x.Season.EndDate > DateTime.Now).FirstOrDefault();
            //int? seasonId = season != null ? season.SeasonId : 0;

            int? seasonId = unionId != null ? _seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) :
                                  (int?)null;
            var gamesList = GamesService.GetPlayerLastGames(teamId, seasonId);
            GamesService.UpdateGameSets(gamesList);
            return Ok(gamesList);
        }

        /// <summary>
        /// Get ranked teams
        /// </summary>
        /// <param name="leagueId"></param>
        /// <param name="teamId"></param>
        /// <returns></returns>
        [Route("Ranked/{leagueId}/{teamId}")]
        public IHttpActionResult GetRankedTeams(int leagueId, int teamId)
        {
            League league = db.Leagues.Find(leagueId);
            if (league == null)
            {
                return NotFound();
            }

            Team team = db.Teams.Find(teamId);
            if (team == null)
            {
                return NotFound();
            }

            int? seasonId = league.SeasonId;

            var resTeams = TeamsService.GetRankedTeams(leagueId, teamId, seasonId);
            return Ok(resTeams);
        }

        // GET: api/Fans/Standing/Team/{teamId}
        [Route("TennisGamesHistory/Player/{playerId}/Team/{teamId}/Union/{unionId}/Lang/{ishebrew}")]
        [Authorize]
        //[ResponseType(typeof(TeamStandingsForm))]
        public IHttpActionResult GetTennisGamesHistoryForPlayer(int playerId,int teamId, int unionId,bool ishebrew = false)
        {
            //start new api
            TennisAchievement1 vm = new TennisAchievement1();
            Sport sport = null;
            GamesRepo gamesRepo = new GamesRepo();
            PlayerAchievementsRepo PlayerAchievementsRepo = new PlayerAchievementsRepo();
            SeasonsRepo u_repo = new SeasonsRepo();
            UsersRepo usersRepo = new UsersRepo();
            var pl = usersRepo.GetById(playerId);
            var currentSeasonId = unionId != null ? _seasonsRepo.GetLastSeasonByCurrentUnionId(unionId) : 0;
            teamId = CurrentUser.TeamsPlayers.Where(r=>r.UserId == playerId && r.SeasonId == currentSeasonId)?.FirstOrDefault()?.TeamId??0;
            var playerFirstTeamInSeason = pl.TeamsPlayers
                .FirstOrDefault(x => x.SeasonId == currentSeasonId)
                ?.Team;
            if (playerFirstTeamInSeason != null)
            {
                if (playerFirstTeamInSeason.LeagueTeams.Any())
                {
                    var league = playerFirstTeamInSeason.LeagueTeams.FirstOrDefault(x => x.SeasonId == currentSeasonId)?.Leagues;
                    if (league != null)
                    {
                        sport = league.Club?.Sport ?? league.Union?.Sport;
                    }

                }
                else if (playerFirstTeamInSeason.ClubTeams.Any())
                {
                    //player in club
                    var club = playerFirstTeamInSeason.ClubTeams.FirstOrDefault(x => x.SeasonId == currentSeasonId)
                        ?.Club;
                    if (club != null)
                    {
                        sport = club.Sport;
                    }
                }
            }
            var sportRankIds = sport?.SportRanks?.Select(x => x.Id)?.ToList() ?? new List<int>(); //explicitly create list of rank ids because entityframework only allows primitives and enumerables in context
            var playerAchievements = PlayerAchievementsRepo.GetCollection(x => x.PlayerId == playerId && sportRankIds.Contains(x.RankId)).ToList();
            if (sport != null && !playerAchievements.Any())
            {
                var achievements = sport.SportRanks.Select(sportRank => new PlayerAchievement
                {
                    PlayerId = playerId,
                    RankId = sportRank.Id
                });

                PlayerAchievementsRepo.Add(achievements);
                playerAchievements =
                    PlayerAchievementsRepo.GetCollection(x => x.PlayerId == playerId &&
                                                              sportRankIds.Contains(x.RankId))
                        .ToList();
            }
            var svc = new CategoryRankService(teamId);
            var PointsAndRanks = svc.GetTennisRanksForUser(playerId, currentSeasonId);
            var ranks = new List<TennisRankModel>();
            foreach (var PointsAndRank in PointsAndRanks)
            {

                var item = new TennisRankModel();
                item.Rank = PointsAndRank.Rank??-1;
                item.AgeName = PointsAndRank.CompetitionAge?.age_name;
                item.Points = PointsAndRank.Points??-1;
                ranks.Add(item);
            }
            vm.tennisRankModel = (IEnumerable<TennisRankModel>)ranks;
            //
            var TennisCompetitionsGames = gamesRepo.GetAllTennisGamesForUser(playerId, teamId, currentSeasonId, ishebrew)?.OrderByDescending(t => t.DateOfGame);
            var tennisLeagueGameModel = new List<TennisLeagueGameModel>();
            foreach (var playerAchievement in TennisCompetitionsGames)
            {
                var item = new TennisLeagueGameModel();
                item.Id = playerAchievement.Id;
                item.CompetionType = playerAchievement.CompetionType;
                item.DateOfGame = playerAchievement.DateOfGame;
                item.CompetitionName = playerAchievement.CompetitionName;
                item.OpponentName = playerAchievement.OpponentName;
                item.PartnerName = playerAchievement.PartnerName;
                item.ResultScore = playerAchievement.ResultScore;
                item.ResultTypeValue = playerAchievement.ResultType;
                tennisLeagueGameModel.Add(item);
            }
            vm.tennisLeagueGameModel = (IEnumerable<TennisLeagueGameModel>)tennisLeagueGameModel;
            foreach (var t in vm.tennisLeagueGameModel)
            {
                switch (t.ResultTypeValue)
                {
                    case ResultType.None:
                        t.ResultType = "N";
                        break;
                    case ResultType.Win:
                        t.ResultType = "W";
                        break;
                    case ResultType.Lose:
                        t.ResultType = "L";
                        break;
                    case ResultType.Draw:
                        t.ResultType = "D";
                        break;
                    default:
                        break;
                }
            }
            return Ok(vm);
        }

        [Route("GamesHistory/Player/{playerId}/Union/{unionId}")]
        [Authorize]
        //[ResponseType(typeof(TeamStandingsForm))]
        public IHttpActionResult GetGamesHistoryForPlayer(int playerId, int unionId)
        {
            var sectRepo = new SectionsRepo();
            var isAthletic = (GamesAlias.Athletics == sectRepo.GetAlias(unionId,null,null));
            var pl = usersRepo.GetById(playerId);
            int seasonId = unionId != null ? _seasonsRepo.GetLastSeasonByCurrentUnionId(unionId) : -1;
            var hasCompetitions = pl.CompetitionRegistrations.Any(cr => !cr.League.IsArchive && cr.FinalScore.HasValue || cr.Position.HasValue)
                            || pl.SportsRegistrations.Any(sr => !sr.League.IsArchive && sr.FinalScore.HasValue || sr.Position.HasValue);

            UnionsRepo sRepo = new UnionsRepo();
            var section = sRepo.GetSectionByUnionId(unionId).Alias;
            var isBasketaball = section == GamesAlias.BasketBall;
            var isMartialArts = section == GamesAlias.MartialArts;


            if (isMartialArts)
            {
                IEnumerable<MartialArtsCompetitionDto> MartialArtsStats = playersRepo.GetMartialArtsPlayerStats(playerId).Where(x => x.SeasonId == seasonId).ToList();
                return Ok(MartialArtsStats);
            }
            if (isAthletic)
            {
                var athleteCompetitions = gamesRepo.GetAthleteCompetitionsAchievements(playerId)?.FirstOrDefault(x=>x.SeasonId == seasonId)?.Achievements;
                return Ok(athleteCompetitions);
            }
            if (hasCompetitions)
            {
                IEnumerable<CompetitionAchievement> competitionAchievement = leagueRepo.GetCompetionsDtos(pl, section);
                return Ok(competitionAchievement);
            }

            return NotFound();
        }

    }
}
