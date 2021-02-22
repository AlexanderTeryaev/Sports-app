using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AppModel;
using DataService;
using WebApi.Models;
using WebApi.Services;
using System.Collections.Generic;
using DataService.LeagueRank;
using WebApi.Helpers;

namespace WebApi.Controllers
{
    [RoutePrefix("api/Fans")]
    public class FansController : BaseLogLigApiController
    {
        private GamesRepo _gamesRepo = null;
        readonly SeasonsRepo _seasonsRepo = new SeasonsRepo();
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
        /// <summary>
        /// מחזיר דף הבית של אוהד בהתייחסות לקבוצה בליגה
        /// </summary>
        /// <param name="teamId">קבוצה ID</param>
        /// <param name="leagueId">ליגה ID</param>
        /// <returns></returns>
        // GET: api/Fans/Home/Team/{teamId}/League/{leagueId}
        [Route("Home/Team/{teamId}/League/{leagueId}")]
        [Authorize]
        //[ResponseType(typeof(FanOwnPrfileViewModel))]
        public IHttpActionResult GetFanHomePage(int teamId, int leagueId)
        {
            var user = CurrentUser;
            if (user == null)
            {
                return NotFound();
            }

            var team = db.Teams.Find(teamId);
            if (team == null)
            {
                return NotFound();
            }

            var seasonId = _seasonsRepo.GetLastSeasonByLeagueId(leagueId);

            var vm = new FanOwnPrfileViewModel();

            var sRepo = new SectionsRepo();
            var sectionName = sRepo.GetSectionByTeamId(teamId)?.Alias;
            var isTennis = sectionName != null ? sectionName.Equals(GamesAlias.Tennis) : false;

            vm.TeamInfo = TeamsService.GetTeamInfo(team, leagueId, seasonId, isTennis);

            var teamGames = team.GuestTeamGamesCycles
                                    .Concat(team.HomeTeamGamesCycles)
                                            .Where(tg => tg.Stage.LeagueId == leagueId && tg.IsPublished)
                                            .ToList();

            //Next Game
            vm.NextGame = GamesService.GetNextGame(teamGames, Convert.ToInt32(User.Identity.Name), leagueId, seasonId);
            //Last Game
            vm.LastGame = GamesService.GetLastGame(teamGames, seasonId);
            //Team Fans
            vm.TeamFans = TeamsService.GetTeamFans(team.TeamId, leagueId, CurrentUser.UserId);
            //Friends
            vm.Friends = FriendsService.GetAllConfirmedFriendsAsUsers(user)
                .Select(u =>
                    new FanFriendViewModel
                    {
                        Id = u.UserId,
                        UserName = u.UserName,
                        FullName = u.FullName,
                        UserRole = u.UsersType.TypeRole,
                        //Image = u.Image,
                        Image = u.Image == null && u.UsersType.TypeRole == "players"
                            ? PlayerService.GetPlayerProfile(u, seasonId).Image
                            : u.Image,
                        CanRcvMsg = true,
                        FriendshipStatus = FriendshipStatus.Yes,
                        Teams = TeamsService.GetFanTeams(u)
                    })
                .Where(u => u.Id != this.CurrUserId)
                .ToList();

            return Ok(vm);
        }


        [Route("Home/Team/{teamId}/Club/{clubId}")]
        //[Authorize]
        //[ResponseType(typeof(FanFriendViewModel))]
        public IHttpActionResult GetFriends(int teamId, int clubId)
        {
            var user = CurrentUser;
            if (user == null)
            {
                return NotFound();
            }

            var vm = new FanOwnPrfileViewModel
            {
                TeamFans = TeamsService.GetTeamFans(teamId, clubId, CurrentUser.UserId),
                Friends = FriendsService.GetAllConfirmedFriendsAsUsers(user)
                    .Where(items => items.UserId != this.CurrUserId)
                    .Select(items => new FanFriendViewModel
                    {
                        Id = items.UserId,
                        UserName = items.UserName,
                        FullName = items.FullName,
                        UserRole = items.UsersType.TypeRole,
                        Image = items.Image == null && items.UsersType.TypeRole == "players"
                            ? PlayerService.GetPlayerProfile(items, null).Image
                            : items.Image,
                        CanRcvMsg = true,
                        FriendshipStatus = FriendshipStatus.Yes,
                        Teams = TeamsService.GetFanTeams(items)
                    })
                    .ToList()
            };

            //Friends

            return Ok(vm);
        }

        // GET: api/Fans/Standing/Team/{teamId}/Club/{clubId}}
        [Route("Standing/Team/{teamId}/Club/{clubId}")]
        [Authorize]
        //[ResponseType(typeof(TeamStandingsForm))]
        public IHttpActionResult GetTeamStanding(int teamId, int clubId)
        {
            var user = CurrentUser;
            if (user == null)
            {
                return NotFound();
            }

            var team = db.Teams.Find(teamId);
            if (team == null)
            {
                return NotFound();
            }

            if (!team.ClubTeams.Any(l => l.ClubId == clubId))
            {
                return NotFound();
            }

            var listtsf = new List<TeamStandingsForm>();
            var teamRepo = new TeamsRepo();
            foreach (var model in teamRepo.GetTeamStandings(clubId, teamId).ToList())
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

            return Ok(listtsf);
        }

        [Route("Game/Team/{teamId}/League/{leagueId}/Union/{unionId}")]
        [Authorize]
        public IHttpActionResult GetUpComingGameForPlayer(int teamId, int leagueId, int unionId)
        {
            var user = CurrentUser;
            if (user == null)
            {
                return NotFound();
            }

            var vm = new FanOwnPrfileViewModel();
            var nextgames = new List<NextGameViewModel>();
            var lastgames = new List<GameViewModel>();
            var gamecycles = new List<int>();

            var teamsPlayers = db.TeamsPlayers.Where(tp => tp.UserId == user.UserId).ToList();
            var seasonId = unionId != null ? _seasonsRepo.GetLastSeasonByCurrentUnionId(unionId) : -1;

            var leagueGames = db.GamesCycles.Where(gc => gc.HomeTeamId == teamId || gc.GuestTeamId == teamId).ToList();
            if (leagueGames != null && leagueGames.Count() > 0)
            {
                vm.NextGame = GamesService.GetNextGame(leagueGames, user.UserId, leagueId, seasonId);
                vm.LastGame = GamesService.GetLastGame(leagueGames, seasonId);
                if(vm.NextGame != null)
                    vm.NextGames = GamesService.GetNextGames(leagueGames, vm.NextGame.StartDate, seasonId);
                vm.LastGames = GamesService.GetLastGames(leagueGames, seasonId);
                vm.GameCycles = leagueGames.Select(gc => gc.CycleNum).Distinct().OrderBy(c => c).ToList();
            }
            /**
            foreach (var teamsPlayer in teamsPlayers)
            {
                var leagueGames = db.GamesCycles.Where(gc => gc.Stage.LeagueId == teamsPlayer.LeagueId && gc.IsPublished).ToList();
                if (leagueGames != null && leagueGames.Count() > 0)
                {
                    var nextgame = GamesService.GetNextGame(leagueGames, user.UserId, -1, seasonId);
                    if (nextgame != null)
                    {
                        nextgame.gameType = 0;
                        nextgames.Add(nextgame);
                    }
                                
                    var lastgame = GamesService.GetLastGame(leagueGames, seasonId);
                    if (lastgame != null)
                    {
                        lastgame.gameType = 0;
                        lastgames.Add(lastgame);
                    }
                    gamecycles.AddRange(leagueGames.Select(gc => gc.CycleNum).Distinct().OrderBy(c => c).ToList());
                    if(nextgame != null)
                        vm.NextGames = GamesService.GetNextGames(leagueGames, nextgame.StartDate, seasonId);
                    vm.LastGames = GamesService.GetLastGames(leagueGames, seasonId);
                    break;

                }

                var competitionGames = db.TennisGameCycles.Where(
                    tgc => tgc.FirstPlayerId == teamsPlayer.Id || tgc.FirstPlayerPairId == teamsPlayer.Id|| tgc.SecondPlayerId == teamsPlayer.Id || tgc.SecondPlayerPairId == teamsPlayer.Id).ToList();
                if (competitionGames != null && competitionGames.Count() > 0)
                {
                    var nextgame = GamesService.GetNextTennisGame(competitionGames, user.UserId, -1, seasonId);
                    if(nextgame != null) {
                        nextgame.gameType = 1;
                        nextgames.Add(nextgame);
                    }
                    var lastgame = GamesService.GetLastTennisGame(competitionGames, seasonId);
                    if (lastgame != null) {
                        lastgame.gameType = 1;
                        lastgames.Add(lastgame);
                    }
                    gamecycles.AddRange(competitionGames.Select(gc => gc.CycleNum).Distinct().OrderBy(c => c).ToList());
                    vm.NextGames = GamesService.GetNextTennisGames(competitionGames, DateTime.Now, seasonId);
                    vm.LastGames = GamesService.GetLastTennisGames(competitionGames, seasonId).OrderBy(x => x.StartDate);
                    break;
                }

            }
            if(nextgames.Count() > 0 )
                vm.NextGame = nextgames.OrderBy(g => g.StartDate).LastOrDefault();
            if(lastgames.Count() > 0)
                vm.LastGame = lastgames.OrderBy(g => g.StartDate).FirstOrDefault();
            if (gamecycles.Count() > 0)
                vm.GameCycles = gamecycles;
            **/

            return Ok(vm);
        }
        [Route("GameTennis/Team/{teamId}/League/{leagueId}/Union/{unionId}")]
        [Authorize]
        public IHttpActionResult GetUpComingGameForPlayerOfTennis(int teamId, int leagueId, int unionId)
        {
            var user = CurrentUser;
            if (user == null)
            {
                return NotFound();
            }

            var vm = new FanOwnPrfileViewModel();
            var nextgames = new List<NextGameViewModel>();
            var lastgames = new List<GameViewModel>();
            var gamecycles = new List<int>();

            var teamsPlayers = db.TeamsPlayers.Where(tp => tp.UserId == user.UserId);
            var seasonId = unionId != null ? _seasonsRepo.GetLastSeasonByCurrentUnionId(unionId) : -1;
            var leagueGames = (from tgcs in db.TennisGameCycles
                               from tp in teamsPlayers
                               where (tgcs.FirstPlayerId == tp.Id || tgcs.SecondPlayerId == tp.Id ||
                               tgcs.FirstPlayerPairId == tp.Id || tgcs.SecondPlayerPairId == tp.Id)&& tp.UserId==CurrUserId
                               select new
                               {
                                   results = tgcs
                               }).ToList();
            if (leagueGames != null && leagueGames.Count() > 0)
            {
                vm.NextGame = GamesService.GetNextGameTennis(leagueGames.Select(u=>u.results), user.UserId, leagueId, seasonId);
                vm.LastGame = GamesService.GetLastGameTennis(leagueGames.Select(u => u.results), seasonId);
                if (vm.NextGame != null)
                    vm.NextGames = GamesService.GetNextTennisGames(leagueGames.Select(u => u.results), vm.NextGame.StartDate, seasonId);
                vm.LastGames = GamesService.GetLastTennisGames(leagueGames.Select(u => u.results), seasonId);
                vm.GameCycles = leagueGames.Select(gc => gc.results.CycleNum).Distinct().OrderBy(c => c).ToList();
            }
            return Ok(vm);
        }
        [Route("MyClubInfo/Club/{clubId}")]
        [Authorize]
        public IHttpActionResult GetClubByClubId(int clubId)
        {
            var clubInfo = new ClubListViewModel();
            var club = db.Clubs.Where(ct => ct.ClubId == clubId).FirstOrDefault() ?? null;
            if (club != null)
            {
                clubInfo.Id = club.ClubId;
                clubInfo.Name = club.Name;
                clubInfo.Logo = club.Logo;
                clubInfo.TotalPlayers = club.TeamsPlayers.Count;
            }
            return Ok(clubInfo);
        }

        [Route("MyClubInfo/Team/{teamId}")]
        [Authorize]
        public IHttpActionResult GetClubByTeamId(int teamId)
        {
            var clubInfo = new ClubListViewModel();
            var club = db.ClubTeams.Where(ct => ct.TeamId == teamId).Select(ct => ct.Club).FirstOrDefault() ?? null;
            if(club != null)
            {
                clubInfo.Id = club.ClubId;
                clubInfo.Name = club.Name;
                clubInfo.Logo = club.Logo;
                clubInfo.TotalPlayers = club.TeamsPlayers.Count;
            }
            return Ok(clubInfo);
        }
        [Route("MyClubInfo/User/{UserId}")]
        //[Authorize]
        public IHttpActionResult getClubInfoByUserId(int UserId)
        {
            var clubInfo = new List<ClubListViewModel>();
            try
            {
                clubInfo = (from teamsfans in db.TeamsFans
                              from clubteams in db.ClubTeams
                              from clubs in db.Clubs
                              from ss in db.Seasons
                            where ((UserId == teamsfans.UserId) && (teamsfans.TeamId == clubteams.TeamId )
                              && clubteams.ClubId == clubs.ClubId && clubteams.IsTrainingTeam == true && clubs.SeasonId == ss.Id
                              && ss.StartDate < DateTime.Now && ss.EndDate > DateTime.Now)
                            select new ClubListViewModel
                              {
                                  Id = clubs.ClubId,
                                  Name = clubs.Name,
                                  Logo = clubs.Logo,
                                  TotalPlayers = clubs.TeamsPlayers.Where(x=>x.IsApprovedByManager==true).Distinct().Count(),
                                  seasonid = clubs.SeasonId
                              }).GroupBy(x=>x.seasonid).Select(x=>x.FirstOrDefault()).Concat(
                    (from teamsplayers in db.TeamsPlayers
                     from clubteams in db.ClubTeams
                     from clubs in db.Clubs
                     from ss in db.Seasons
                     where (UserId == teamsplayers.UserId && teamsplayers.TeamId == clubteams.TeamId
                     && clubteams.ClubId == clubs.ClubId && clubs.ClubId == teamsplayers.ClubId && clubteams.IsTrainingTeam == true&& clubs.SeasonId == ss.Id
                     && ss.StartDate<DateTime.Now && ss.EndDate>DateTime.Now)
                     select new ClubListViewModel
                     {
                         Id = clubs.ClubId,
                         Name = clubs.Name,
                         Logo = clubs.Logo,
                         TotalPlayers = clubs.TeamsPlayers.Where(x=>x.IsApprovedByManager==true).Select(x=>x.UserId).Distinct()
                         .Count(),
                         seasonid = clubs.SeasonId
                     }).GroupBy(x => x.seasonid).Select(x => x.FirstOrDefault())
                    ).ToList();


                


                var fans = 0;
                // Cheng Li. Get total Players : 20180808:6. club list: please change count of teams to count of sportsmans. show first sportsmans and then fans
                var players = 0;
                //foreach (var info in clubInfo)
                //{
                //    foreach( var clubTeam in db.Clubs.Where(x=>x.ClubId == info.Id).FirstOrDefault().ClubTeams)
                //    {
                //        fans += clubTeam.Team.TeamsFans.Count;
                //        players += clubTeam.Team.TeamsPlayers.Where(tp => tp.SeasonId == seasonId && tp.IsApprovedByManager == true).Select(tp => tp.UserId).Distinct().Count();
                //    }
                //}
            }
            catch (Exception ex)
            {
                return Ok(clubInfo);
                throw ex;
            }

            return Ok(clubInfo);
        }
        // GET: api/Fans/Standing/Team/{teamId}
        [Route("Archievements/Team/{teamId}/Union/{unionId}")]
        [Authorize]
        //[ResponseType(typeof(TeamStandingsForm))]
        public IHttpActionResult GetArchievementsForPlayer(int teamId, int unionId)
        {
            var user = CurrentUser;
            if (user == null)
            {
                return NotFound();
            }
            var vm = new TennisAchievement1();
           // return Ok(vm);
            var seasonId = unionId != null ? _seasonsRepo.GetLastSeasonByCurrentUnionId(unionId) :-1;
            //var currentTennisPoints = db.TennisRanks.Where(t => t.UserId == user.UserId);

            var svc = new CategoryRankService(teamId);
            var currentTennisPoints = svc.GetTennisRanksForUser(user.UserId, seasonId);
            vm.tennisRankModel = currentTennisPoints?.Select(x => new TennisRankModel
            {
                Points = x.Points != null ? (int)x.PointsToAverage : 0,
                Rank = x.Rank != null ? (int)x.Rank : 0,
                AgeName = x.AgeId == null ? "" : db.CompetitionAges.Where(ca => ca.id == x.AgeId).FirstOrDefault().age_name
            }).ToList().Distinct();
            vm.tennisRankModel =  vm.tennisRankModel.Take(1);

            vm.tennisLeagueGameModel = CacheService.GetAllTennisGamesForUser(gamesRepo, user.UserId, teamId, seasonId, true)?.OrderByDescending(t => t.DateOfGame).Select(x => new TennisLeagueGameModel
            {
                Id = x.Id,
                CompetionType = x.CompetionType,
                DateOfGame = x.DateOfGame,
                CompetitionName = x.CompetitionName,
                OpponentName = x.OpponentName,
                PartnerName = x.PartnerName,
                ResultScore = x.ResultScore,
                ResultTypeValue = x.ResultType
            }).ToList();

            foreach(var t in vm.tennisLeagueGameModel)
            {
                switch(t.ResultTypeValue)
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

        /// <summary>
        /// מחזיר דף אוהד כפי שמשתמש אחר רואה אותו
        /// </summary>
        /// <param name="id">ID משתמש</param>
        /// <param name="unionId"></param>
        /// <returns></returns>
        /// // GET: api/Fans/5
        [AllowAnonymous]
        [Route("{id}")]
        [ResponseType(typeof(FanPrfileViewModel))]
        public IHttpActionResult GetFan(int id, int? unionId = null)
        {
            var fan = db.Users.FirstOrDefault(u => u.UserId == id &&
                                                    u.IsArchive == false &&
                                                    u.UsersType.TypeRole == AppRole.Fans);

            if (fan == null) return NotFound();

            var user = CurrentUser;

            var seasonId = unionId != null ? _seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) :
                                              (int?)null;

            var vm = user == null ?
                                    FansService.GetFanProfileAsAnonymousUser(fan, seasonId) :
                                    FansService.GetFanProfileAsLoggedInUser(user, fan, seasonId);

            return Ok(vm);
        }

        /// <summary>
        /// עריכת פרופיל אוהד
        /// </summary>
        /// <param name="id">ID אוהד</param>
        /// <param name="bm">Fan Edit Profile Binding Model</param>
        /// <returns></returns>
        // Post: api/Fans/Edit
        [Route("Edit/{id}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutFanProfile(int id, FanEditProfileBindingModel bm)
        {

            if (User == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != CurrentUser.UserId)
            {
                return BadRequest();
            }

            var usr = db.Users.Find(id);

            if (usr == null)
            {
                return BadRequest();
            }

            if (!string.IsNullOrEmpty(bm.UserName))
                usr.UserName = bm.UserName;
            if (!string.IsNullOrEmpty(bm.Email))
                usr.Email = bm.Email;
            if (!string.IsNullOrEmpty(bm.Password))
                usr.Password = Protector.Encrypt(bm.Password);
            if (bm.Teams != null)
            {
                usr.TeamsFans.Clear();
                foreach (var t in bm.Teams)
                {
                    usr.TeamsFans.Add(new TeamsFan
                    {
                        TeamId = t.TeamId,
                        UserId = usr.UserId,
                        LeageId = t.LeagueId
                    });
                }
            }
            db.Entry(usr).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.UserId == id) > 0;
        }

        // POST: api/Fans/Going
        [Route("Going")]
        public IHttpActionResult PostFanGoing(GoingDTO item)
        {
            var user = CurrentUser;
            var game = db.GamesCycles.Find(item.Id);
            var fan = game.Users.FirstOrDefault(t => t.UserId == user.UserId);

            if (item.IsGoing == 1)
            {
                if (fan == null)
                {
                    game.Users.Add(user);
                    db.SaveChanges();
                }
            }
            else
            {
                if (fan != null)
                {
                    game.Users.Remove(fan);
                    db.SaveChanges();
                }
            }

            return Ok();
        }

        // POST: api/Fans/Going
        [Route("GoingForTennis")]
        public IHttpActionResult PostTennisFanGoing(GoingDTO item)
        {
            var user = CurrentUser;
            var game = db.GamesCycles.Find(item.Id);
            var fan = game.Users.FirstOrDefault(t => t.UserId == user.UserId);

            if (item.IsGoing == 1)
            {
                if (fan == null)
                {
                    game.Users.Add(user);
                    db.SaveChanges();
                }
            }
            else
            {
                if (fan != null)
                {
                    game.Users.Remove(fan);
                    db.SaveChanges();
                }
            }

            return Ok();
        }

        // POST: api/Fans/Going1
        [Route("Going1")]
        public IHttpActionResult PostFanGoing1(GoingDTO item)
        {
            var user = CurrentUser;

            var list = db.LeaguesFans.Where(df => df.UserId == user.UserId && df.LeagueId == item.Id).ToList();
            var lf = new LeaguesFan();
            lf.LeagueId = item.Id;
            lf.UserId = user.UserId;

            if (item.IsGoing == 1)
            {
                if (list == null || list.Count == 0)
                {
                    db.LeaguesFans.Add(lf);
                    db.SaveChanges();
                }
            }
            else
            {
                if (list != null)
                {
                    var fan = db.LeaguesFans.Where(df => df.UserId == user.UserId && df.LeagueId == item.Id).FirstOrDefault();
                    db.LeaguesFans.Remove(fan);
                    db.SaveChanges();
                }
            }
            return Ok();
        }

        // POST: api/Fans/Club/Going
        [Route("Club/Going")]
        public IHttpActionResult PostClubFanGoing(GoingDTO item)
        {
            var user = CurrentUser;
            var game = db.TeamScheduleScrappers.Find(item.Id);
            var fan = game.Users.FirstOrDefault(t => t.UserId == user.UserId);

            if (item.IsGoing == 1)
            {
                if (fan == null)
                {
                    game.Users.Add(user);
                    db.SaveChanges();
                }
            }
            else
            {
                if (fan != null)
                {
                    game.Users.Remove(fan);
                    db.SaveChanges();
                }
            }

            return Ok();
        }
    }
}