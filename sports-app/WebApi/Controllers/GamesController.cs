using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using AppModel;
using DataService;
using WebApi.Models;
using WebApi.Services;
using DataService.DTO;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace WebApi.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/Games")]
    public class GamesController : BaseLogLigApiController
    {
        readonly GamesRepo _gamesRepo = new GamesRepo();
        private static readonly PlayersRepo _playersRepo = new PlayersRepo();
        private static readonly TeamsRepo _teamsRepo = new TeamsRepo();
        readonly DataService.Services.GamesService _gamesService = new DataService.Services.GamesService();
        readonly SeasonsRepo _seasonsRepo = new SeasonsRepo();
        readonly SectionsRepo _sectionRepo = new SectionsRepo();
        readonly UsersRepo _usersRepo = new UsersRepo();

        /// <summary>
        /// מחזיר קיר הודעות של משחק
        /// </summary>
        /// <param name="teamId">ID משחק</param>
        /// <returns></returns>
        [ResponseType(typeof(List<WallThreadViewModel>))]
        [Route("Messages/{gameId}")]
        public IHttpActionResult GetGameMessages(int gameId)
        {
            List<WallThreadViewModel> MessageThreads = MessagesService.GetGameMessages(gameId);
            return Ok(MessageThreads);
        }


        /// <summary>
        /// מחזיר קיר הודעות של משחק
        /// </summary>
        /// <param name="teamId">ID משחק</param>
        /// <param name="leagueId">ID משחק</param>
        /// <returns></returns>
        [ResponseType(typeof(List<GamesCycleDto>))]
        [Route("Schedule/Team/{teamId}/League/{leagueId}")]
        public IHttpActionResult GetTeamSchedule(int teamId, int leagueId)
        {
            int? seasonId = _seasonsRepo.GetLastSeasonByLeagueId(leagueId);
            var gamesCycles = db.GamesCycles
                .Where(game => (game.HomeTeamId == teamId || game.GuestTeamId == teamId) &&
                               game.IsPublished &&
                               game.HomeTeam.LeagueTeams.Any(
                                   t => t.SeasonId == seasonId && leagueId == t.LeagueId) &&
                               game.GuestTeam.LeagueTeams.Any(
                                   t => t.SeasonId == seasonId && leagueId == t.LeagueId) &&
                               leagueId == game.Stage.LeagueId
                )
                .Select(t => new GamesCycleDto
                {
                    //AuditoriumId = t.Auditorium?.AuditoriumId ?? 0,
                    Auditorium = t.Auditorium.Name,
                    AuditoriumAddress = t.Auditorium.Address,
                    CycleId = t.CycleId,
                    GameType = t.Group.TypeId,
                    LeagueId = t.Stage.LeagueId,
                    LeagueName = t.Stage.League.Name,
                    LeagueLogo = t.Stage.League.Logo,
                    SectionAlias = t.Stage.League.Union.Section.Alias,
                    GameStatus = t.GameStatus,
                    HomeTeamId = t.HomeTeamId,
                    HomeAthleteId = t.HomeAthleteId,
                    HomeTeam = t.HomeTeam.Title,
                    GuestTeam = t.GuestTeam.Title,
                    //HomeLogo = t.HomeTeam?.Logo,
                    //IsHomeTeamKnown = t.HomeTeam != null,
                    HomeTeamScore = t.IsPublished ? t.HomeTeamScore : -1,
                    GuestTeamId = t.GuestTeamId,
                    GuestAthleteId = t.GuestAthleteId,
                    //GuesLogo = t.GuestTeam?.Logo,
                    //IsGuestTeamKnown = t.GuestTeam != null,
                    GuestTeamScore = t.IsPublished ? t.GuestTeamScore : -1,
                    StageId = t.StageId,
                    //StageName = "שלב " + t.Stage.Number,
                    GroupId = t.GroupId.Value,
                    GroupName = t.Group.Name,
                    StartDate = t.StartDate,
                    //MaxPlayoffPos = t.MaxPlayoffPos,
                    //MinPlayoffPos = t.MinPlayoffPos,
                    IsPublished = t.IsPublished
                    //EventRef = evnts == null ? null :
                    //    evnts.Where(ev => ev.LeagueId == t.Stage.LeagueId
                    //                      && ev.EventTime <= t.StartDate)
                    //.OrderByDescending(ev => ev.EventTime).FirstOrDefault()
                }).ToList();

            return Ok(gamesCycles);
        }

        /// <summary>
        /// מחזיר קיר הודעות של משחק
        /// </summary>
        /// <param name="teamId">ID משחק</param>
        /// <param name="clubId">ID משחק</param>
        /// <returns></returns>
        [ResponseType(typeof(List<WallThreadViewModel>))]
        [Route("Schedule/Team/{teamId}/Club/{clubId}")]
        public IHttpActionResult GetClubTeamSchedule(int teamId, int clubId)
        {

            return Ok();
        }

        /// <summary>
        /// מחזיר דף משחק
        /// </summary>
        /// <param name="id">משחק ID</param>
        /// <param name="unionId"></param>
        /// <returns></returns>
        // GET: api/Games/5
        [ResponseType(typeof(GamePageViewModel))]
        public IHttpActionResult Get(int id, int? unionId = null)
        {
            int? seasonId = unionId != null ? _seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) :
                                             (int?)null;

            var game = GamesService.GetGameById(id, seasonId);

            if (game == null)
            {
                return NotFound();
            }

            var vm = GamePageViewModel(id, game);
            // Cheng Li : Get Tennis Game Info 
            var gamesScore = db.GamesCycles
                .FirstOrDefault(gc => gc.CycleId == game.GameId)
                ?.TennisLeagueGames
                ?.ToList();
            if (gamesScore?.Any() == true)
            {
                game.TennisLeagueGamesScore = gamesScore.Select(tg => new TennisLeagueGameViewModel
                {
                    FirstPlayerName = tg.HomePlayer != null ? tg.HomePlayer.FullName : "",
                    FirstPairPlayerName = tg.HomePairPlayer?.FullName,
                    SecondPlayerName = tg.GuestPlayer != null ? tg.GuestPlayer.FullName : "",
                    SecondPairPlayerName = tg.GuestPairPlayer?.FullName,
                    TennisLeagueGameScore = tg.TennisLeagueGameScores.Select(tlgs => new TennisLeagueGameScoreViewModel
                    {
                        HomeScore = tlgs.HomeScore,
                        GuestScore = tlgs.GuestScore,
                        IsPairScores = tlgs.IsPairScores
                    }).ToList()
                }).ToList();
            }
            
            //
            /** add cheng for comment of EditGamePartial */
            vm.Comments = db.GamesCycles.Where(gc => gc.CycleId == game.GameId).FirstOrDefault().Note;
            return Ok(vm);
        }

        [Authorize]
        [Route("SetComments/Game/{gameId}/Comments/{comments}")]
        [HttpGet]
        public IHttpActionResult SetComments(int gameId, string comments)
        {
            var game = db.GamesCycles.Find(gameId);

            if (game != null)
            {
                game.Note = comments;
                db.SaveChanges();
            }
            return Ok();
        }

        private GamePageViewModel GamePageViewModel(int id, GameViewModel game, bool updateSetsAndHistory = true)
        {
            var section = _sectionRepo.GetByLeagueId(game.LeagueId)?.Alias;

            GamesService.UpdateGameSet(game, section);

            var vm = new GamePageViewModel { GameInfo = game };

            if (updateSetsAndHistory)
            {
                if (User.Identity.IsAuthenticated)
                {
                    vm.GoingFriends = GetTeamsFansList(id);
                }

                vm.Sets = GamesService.GetGameSets(id).ToList();
                vm.History = GamesService.GetGameHistory(game.GuestTeamId, game.HomeTeamId);
                GamesService.UpdateGameSets(vm.History);
                GamesService.UpdateGameSet(vm.GameInfo, section);
            }

            return vm;
        }

        /// <param name="gameId">ID משחק</param>
        /// <returns></returns>
        /// GET: api/Games/TennisCompetition/{gameId}
        [Authorize]
        [Route("TennisCompetition/{gameId}")]
        [ResponseType(typeof(GamePageViewModel))]
        public IHttpActionResult GetTennisCompetitionGames(int gameId)
        {

            var game = GamesService.GetTennisGameById(gameId);
            var seasonId = game.SeasonId;
            if (game == null)
            {
                return NotFound();
            }

            var vm = TennisGamePageViewModel(gameId, game);

            var tennisLeagueGameViewModel = new TennisLeagueGameViewModel();

            var gamesScore = db.TennisGameCycles.FirstOrDefault(gc => gc.CycleId == game.GameId);
            if (gamesScore != null)
            {
                seasonId = gamesScore.FirstPlayer.Team.LeagueTeams
                    .FirstOrDefault(x =>
                        x.Leagues.Season.StartDate < DateTime.Now &&
                        x.Leagues.Season.EndDate > DateTime.Now)
                    ?.SeasonId;
            }
            
            if(gamesScore.TeamsPlayer != null)
            {
                var firstPlayer = gamesScore.TeamsPlayer.User;
                tennisLeagueGameViewModel.FirstPlayerName = firstPlayer != null ? firstPlayer.FullName : null;
                game.HomeTeamLogo = gamesScore.FirstPlayer.User.Image;
                if(game.HomeTeamLogo == null)
                {
                    
                    var ppv = PlayerService.GetPlayerProfile(gamesScore.FirstPlayer.User, seasonId);
                    game.HomeTeamLogo = ppv.Image;
                }
            }
            else
            {
                tennisLeagueGameViewModel.FirstPlayerName = "";
            }
            if (gamesScore.FirstPairPlayer != null)
            {
                var firstPairePlayer = gamesScore.FirstPairPlayer.User;
                tennisLeagueGameViewModel.FirstPairPlayerName = firstPairePlayer != null ? firstPairePlayer.FullName : null;
                game.HomeTeamLogoPair = gamesScore.FirstPairPlayer.User.Image;
                if (game.HomeTeamLogoPair == null)
                {

                    var ppv = PlayerService.GetPlayerProfile(gamesScore.FirstPairPlayer.User, seasonId);
                    game.HomeTeamLogoPair = ppv.Image;
                }
            }
            else
            {
                tennisLeagueGameViewModel.FirstPairPlayerName = "";
            }

            if (gamesScore.TeamsPlayer1 != null)
            {
                var secondPlayer = gamesScore.TeamsPlayer1.User;
                var secondPairPlayer = gamesScore.TeamsPlayer1.User1;
                tennisLeagueGameViewModel.SecondPlayerName = secondPlayer != null ? secondPlayer.FullName : null;
                tennisLeagueGameViewModel.SecondPairPlayerName = secondPairPlayer != null ? secondPairPlayer.FullName : null;
                game.GuestTeamLogo = gamesScore.SecondPlayer.User.Image;
                if(game.GuestTeamLogo == null)
                {
                    var ppv = PlayerService.GetPlayerProfile(gamesScore.SecondPlayer.User, seasonId);
                    game.GuestTeamLogo = ppv.Image;
                }
            }
            else
            {
                tennisLeagueGameViewModel.SecondPlayerName = "";
                tennisLeagueGameViewModel.SecondPairPlayerName = "";
            }
            if (gamesScore.SecondPairPlayer != null)
            {
                var secondPairPlayer = gamesScore.SecondPairPlayer.User;
                tennisLeagueGameViewModel.SecondPairPlayerName = secondPairPlayer != null ? secondPairPlayer.FullName : null;
                game.GuestTeamLogoPair = gamesScore.SecondPairPlayer.User.Image;
                if (game.GuestTeamLogoPair == null)
                {
                    var ppv = PlayerService.GetPlayerProfile(gamesScore.SecondPairPlayer.User, seasonId);
                    game.GuestTeamLogoPair = ppv.Image;
                }
            }
            else
            {
                tennisLeagueGameViewModel.SecondPairPlayerName = "";
            }

            tennisLeagueGameViewModel.TennisLeagueGameScore = gamesScore.TennisGameSets.Select(tgs => new TennisLeagueGameScoreViewModel
            {
                HomeScore = tgs.FirstPlayerScore,
                GuestScore = tgs.SecondPlayerScore,
                IsPairScores = false
            }).ToList();
            game.TennisLeagueGamesScore = new List<TennisLeagueGameViewModel>();
            game.TennisLeagueGamesScore.Add(tennisLeagueGameViewModel);
            return Ok(vm);
        }

        private GamePageViewModel TennisGamePageViewModel(int id, GameViewModel game, bool updateSetsAndHistory = true)
        {
            var section = _sectionRepo.GetByLeagueId(game.LeagueId)?.Alias;

            GamesService.UpdateGameSet(game, section);

            var vm = new GamePageViewModel { GameInfo = game };

            if (updateSetsAndHistory)
            {                if (User.Identity.IsAuthenticated)
                {
                    vm.GoingFriends = GetTeamsFansList(id);
                }

                vm.Sets = GamesService.GetGameSets(id).ToList();
                vm.History = GamesService.GetTennisGameHistory(game.GuestTeamId, game.HomeTeamId);
            }

            return vm;
        }

        /// <summary>
        /// מחזיר רשימת אוהדם שבאים למשחק
        /// </summary>
        /// <param name="gameId">ID משחק</param>
        /// <returns></returns>
        /// GET: api/Games/Fans/{teamId}
        [Authorize]
        [Route("Fans/{gameId}")]
        public IHttpActionResult GetTeams(int gameId)
        {
           var resList = GetTeamsFansList(gameId);
           return Ok(resList);
        }

        private IEnumerable<UserBaseViewModel> GetTeamsFansList(int gameId)
        {
            var user = base.CurrentUser;
            if (user == null)
                return new List<UserBaseViewModel>();

            IEnumerable<UserBaseViewModel> teamFans = GamesService.GetGoingFriends(gameId, user)
                     .Select(u => new UserBaseViewModel
                     {
                         Id = u.UserId,
                         UserName = u.UserName,
                         FullName = u.FullName,
                         UserRole = u.UsersType.TypeRole,
                         Image = u.Image == null && u.UsersType.TypeRole == "players" ? PlayerService.GetPlayerProfile(u, null).Image : u.Image,
                         FriendshipStatus = FriendshipStatus.Yes,
                         CanRcvMsg = true
                     });

            return teamFans.GroupBy(x => x.Id).Select(g => g.First());
        }

        [Authorize]
        [Route("Fans/Club/{gameId}")]
        public IHttpActionResult GetClubTeamsFans(int gameId)
        {
            var user = base.CurrentUser;
            var resList = GamesService.GetClubGoingFriends(gameId, user)
                     .Select(u => new UserBaseViewModel
                     {
                         Id = u.UserId,
                         UserName = u.UserName,
                         FullName = u.FullName,
                         UserRole = u.UsersType.TypeRole,
                         Image = u.Image == null && u.UsersType.TypeRole == "players" ? PlayerService.GetPlayerProfile(u, null).Image : u.Image,
                         FriendshipStatus = FriendshipStatus.Yes,
                         CanRcvMsg = true
                     });

            return Ok(resList.GroupBy(x => x.Id).Select(g => g.First()));
        }

        /// <summary>
        /// Start game by game Id
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        [Authorize]
        [Route("{gameId}/Actions/StartGame")]
        [HttpPost]
        public IHttpActionResult StartGame(int gameId)
        {
            GamesCycle game = _gamesRepo.GetGameCycleById(gameId);
            if (game == null)
            {
                return NotFound();
            }

            _gamesRepo.StartGame(gameId);

            return Ok();
        }

        /// <summary>
        /// End game by game Id
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        [Authorize]
        [Route("{gameId}/Actions/EndGame")]
        [HttpPost]
        public IHttpActionResult EndGame(int gameId, [FromBody]GameDetails gameDetails)
        {
            GamesCycle game = _gamesRepo.GetGameCycleById(gameId);
            if (game == null)
            {
                return NotFound();
            }

            var sectionAlias = _gamesRepo.GetSectionAlias(game);
            if (string.Equals(sectionAlias, SectionAliases.Basketball, StringComparison.OrdinalIgnoreCase))
            {
                _gamesRepo.EndGameForBasketballApp(gameId);
            }
            else
            {
                _gamesRepo.EndGame(gameId);
            }
            UpdateGameDetails(gameDetails, game);

            return Ok();
        }

        /// <summary>
        /// Save comment by game Id
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [Authorize]
        [Route("{gameId}/{comment}/Actions/SaveComment")]
        [HttpPost]
        public IHttpActionResult SaveComment(int gameId, string comment)
        {
            byte[] decode = Convert.FromBase64String(comment);
            string decodeComments = System.Text.Encoding.UTF8.GetString(decode);

            GamesCycle game = _gamesRepo.GetGameCycleById(gameId);
            if (game == null)
            {
                return NotFound();
            }
            game.Note = decodeComments;
            _gamesRepo.Save();
            return Ok();
        }

        private void UpdateGameDetails(GameDetails gameDetails, GamesCycle game)
        {
            if (gameDetails != null && !string.IsNullOrEmpty(gameDetails.Note))
            {
                game.Note = gameDetails.Note;
                _gamesRepo.Save();
            }
            else
            {
                if (!string.IsNullOrEmpty(game.Note))
                {
                    game.Note = null;
                    _gamesRepo.Save();
                }
            }
        }

        /// <summary>
        /// Set technical win for game
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="teamId"></param>
        /// <returns></returns>
        [Authorize]
        [Route("{gameId}/{teamId}/Actions/TechnicalWin")]
        [HttpPost]
        public IHttpActionResult SetTechnicalWin(int gameId, int teamId)
        {
            GamesCycle game = _gamesRepo.GetGameCycleById(gameId);
            if (game == null)
            {
                return NotFound();
            }

            _gamesService.SetTechnicalWinForGame(gameId, teamId);

            return Ok();
        }

        /// <summary>
        /// Get games for app user
        /// </summary>
        /// <returns></returns>§
        [Authorize]
        [Route("app")]
        [HttpGet]
        public IHttpActionResult GetGamesForApp()
        {
            if (CurrentUser == null) return BadRequest("Authorization issue");
            if (CurrentUser.TypeId != 6) return BadRequest("User doesn't has access to this api");
            IEnumerable<GameDto> games;
            try
            {
                var entity = CurrentUser.FullName.Split('.');
                var monthAhead = DateTime.Now.AddMonths(1);
                switch (entity[0])
                {
                    case "club":
                    case " club":
                        games = _gamesRepo.GetByClubId(int.Parse(entity[1]), monthAhead);
                        break;
                    case "union":
                    case " union":
                        games = _gamesRepo.GetByUnionId(int.Parse(entity[1]), monthAhead);
                        break;
                    default:
                        throw new Exception("User saved in wrong format");
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            var result = games.Select(g => new { GameInfo = g }).ToList();
            return Ok(result);
        }

        /// <summary>
        /// Get games for waterpolo app user
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("app/waterpolo")]
        [HttpGet]
        public IHttpActionResult GetGamesForWaterpoloApp()
        {
            if (CurrentUser == null) return BadRequest("Authorization issue");

            IEnumerable<BaseGameDto> refereeGames = Enumerable.Empty<BaseGameDto>();
            try
            {
                var isWaterpoloReferee = _usersRepo.IsSectionWorker(CurrentUser.UserId, GamesAlias.WaterPolo, JobRole.Referee);
                if (!isWaterpoloReferee) return BadRequest("User doesn't has access to this api");
                refereeGames = _gamesRepo.GetRefereeGames(CurrentUser);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            var result = refereeGames.Select(g => new { GameInfo = g }).ToList();
            return Ok(result);
        }

        [Authorize]
        [Route("Statistic")]
        [HttpPost]
        public IHttpActionResult Statistic(StatisticBindingModel statistics)
        {
            try
            {
                //statistics.Id = Guid.NewGuid().ToString();
                _gamesService.AddStatistics(statistics);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        [Authorize]
        [Route("Statistics")]
        [HttpPost]
        public IHttpActionResult Statistics(IEnumerable<StatisticBindingModel> statistics)
        {
            try
            {
                if (statistics != null && statistics.Any())
                {
                    foreach (var statistic in statistics)
                    {
                        //statistic.Id = Guid.NewGuid().ToString();
                        _gamesService.AddStatistics(statistic);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Get games for app user
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("{gameId}/officials")]
        [HttpGet]
        public IHttpActionResult GetOfficialsForGame(int gameId)
        {
            var officials = _gamesRepo.GetOfficials(gameId);
            return Ok(officials);
        }

        /// <summary>
        /// Get games for app user
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("{gameId}/players")]
        [HttpGet]
        public IHttpActionResult GetPlayersForGame(int gameId)
        {
            try
            {
                GameViewModel gameVm = GamesService.GetGameById(gameId);
                List<Team> result = new List<Team>();
                var sectionAlias = _gamesRepo.GetSectionAlias(gameId);
                var homeTeamId = gameVm.HomeTeamId;
                if (homeTeamId != null)
                {
                    var homeTeamName = _teamsRepo.GetTeamNameById(homeTeamId ?? 0);
                    var playerList = GetPlayersList(homeTeamId.Value, gameVm.SeasonId, gameVm.HomeTeam, gameVm.UnionId, gameVm.LeagueId);

                    if (sectionAlias.Equals(GamesAlias.WaterPolo))
                    {
                        playerList = playerList.Where(p => p.IsApprovedByManager).ToList();
                    }

                    var team1 = new Team
                    {
                        teamID = homeTeamId.Value,
                        teamName = homeTeamName,
                        playersList = playerList,
                        teamIdentifier = 0
                    };

                    result.Add(team1);
                }
                var guestTeamId = gameVm.GuestTeamId;
                if (guestTeamId != null)
                {
                    var guestTeamName = _teamsRepo.GetTeamNameById(guestTeamId ?? 0);
                    var playerList = GetPlayersList(guestTeamId.Value, gameVm.SeasonId, gameVm.GuestTeam, gameVm.UnionId, gameVm.LeagueId);

                    if (sectionAlias.Equals(GamesAlias.WaterPolo))
                    {
                        playerList = playerList.Where(p => p.IsApprovedByManager).ToList();
                    }

                    var team1 = new Team
                    {
                        teamID = guestTeamId.Value,
                        teamName = guestTeamName,
                        playersList = playerList,
                        teamIdentifier = TeamEnum.Guest
                    };

                    result.Add(team1);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + "\r\n" + ex.StackTrace);
            }
        }


        /// <summary>
        /// Excepts pdf file
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("{gameId}/pdf")]
        [HttpPost]
        public async Task<IHttpActionResult> GetPdfFile(int gameId)
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                var game = _gamesRepo.GetGameCycleById(gameId);
                if (game == null)
                {
                    return BadRequest($"Game with id {gameId} doesn't exist.");
                }

                var savePath = Path.Combine(@"C:\Publish\ProductionCMS\assets\gamereports\");
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (var file in provider.Contents)
                {
                    var newFileName = await SaveFile(file, gameId, savePath);
                    _gamesRepo.SaveNewPdfGameReport(game, newFileName, savePath);
                }

                return Ok("Pdf game report uploaded");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + "\r\n" + ex.StackTrace);
            }
        }


        /// <summary>
        /// Returns all referees for game
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("{id}/allreferees")]
        [HttpGet]
        public IHttpActionResult GetRefereesForGame(int id)
        {
            try
            {
                var unionId = _gamesRepo.GetUnionIdByGameCycle(id);
                var referees = new List<GameReferee>();

                if (unionId.HasValue)
                {
                    var refereesValues = _usersRepo.GetUnionAndLeagueReferees(unionId.Value, id)?
                        .Select(u => new GameReferee
                        {
                            Id = u.UserId,
                            Name = u.FullName
                        });

                    var spectatorsValues = _usersRepo.GetUnionAndLeagueSpectators(unionId.Value, id)?
                        .Select(u => new GameReferee
                        {
                            Id = u.UserId,
                            Name = u.FullName
                        });

                    if (refereesValues?.Any() == true) referees.AddRange(refereesValues);
                    if (spectatorsValues?.Any() == true) referees.AddRange(spectatorsValues);
                }

                var selectedReferees = _gamesRepo.GetSelectedRefereesForGame(id);
                var result = new RefereeItems
                {
                    AllReferees = referees,
                    SelectedReferees = selectedReferees
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Save selected referees to database
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("{id}/referees")]
        [HttpPost]
        public IHttpActionResult Referees(int id, IEnumerable<int> ids)
        {
            try
            {
                _gamesRepo.SaveReferees(id, ids ?? Enumerable.Empty<int>());
                _gamesRepo.Save();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        [NonAction]
        private async Task<string> SaveFile(HttpContent file, int gameId, string savePath)
        {
            string ext = Path.GetExtension(file.Headers.ContentDisposition.FileName.Trim('\"'));

            if (!string.Equals(".pdf", ext, StringComparison.OrdinalIgnoreCase))
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var newName = $"pdfgamereport_{gameId}_{DateTime.Now.ToString("ddMMyyyyHHmmssfff")}{ext}";
            var di = new DirectoryInfo(savePath);
            if (!di.Exists)
            {
                di.Create();
            }

            var buffer = await file.ReadAsByteArrayAsync();
            File.WriteAllBytes(savePath + newName, buffer);
            return newName;
        }


        private static List<Player> GetPlayersList(int teamId, int? seasonId, string teamName, int? unionId, int? leagueId = null)
        {
            List<Player> players = new List<Player>();
            if (teamId == -1)
            {
                players.Add(new Player
                {
                    id = int.MaxValue,
                    name = teamName,
                    shirtNumber = 1,
                    teamId = -1
                });
            }
            else
            {
                //HashSet<int> shirtNums = new HashSet<int>();
                foreach (var tp in _playersRepo.GetTeamPlayers(teamId, seasonId, false, leagueId == 0 ? null : leagueId))
                {
                    //if (shirtNums.Contains(tp.ShirtNum)) continue;
                    //shirtNums.Add(tp.ShirtNum);
                    decimal handicap = _playersRepo.GetFinalHandicap(tp, out decimal reduction, seasonId, null, tp.LeagueId);
                    players.Add(new Player
                    {
                        id = tp.Id,
                        name = tp.User.FullName,
                        shirtNumber = tp.ShirtNum,
                        imgUrl = tp.User.PlayerFiles.FirstOrDefault(x => x.FileType == (int)PlayerFileType.PlayerImage)
                                     ?.FileName
                                 ?? "PlayerImage_" + tp.User.Image,
                        teamId = teamId,
                        BaseHandicap = tp.HandicapLevel,
                        HandicapLevel = handicap,
                        HandicapReduction = reduction,
                        IsApprovedByManager = tp.IsActive && tp.User.IsActive && tp.IsApprovedByManager == true
                    });
                }
            }

            return players
                .OrderBy(p => p.shirtNumber.ToString().Length)
                .ThenBy(p => p.shirtNumber)
                .ToList();
        }

        class Team
        {
            public int teamID { get; set; }
            public String teamName { get; set; }
            public Player teamManager { get; set; }
            public IEnumerable<Player> playersList { get; set; }
            public String teamLogoUri { get; set; }
            public TeamEnum teamIdentifier { get; set; }
        }

        class Player
        {
            public int id { get; set; }
            public String name { get; set; }
            public int shirtNumber { get; set; }
            public String imgUrl { get; set; }
            public String foulType { get; set; }        //Offence / Defense.
            public int foul_counter { get; set; }
            public int steal_ball_counter { get; set; }
            public bool onField { get; set; }
            public int onFieldPosition { get; set; }
            public int teamId { get; set; }
            public decimal HandicapLevel { get; internal set; }
            public decimal HandicapReduction { get; set; }
            public decimal BaseHandicap { get; set; }
            public bool IsApprovedByManager { get; set; }
        }

        internal enum TeamEnum
        {
            Home = 0,
            Guest = 1
        }
        public class GameDetails
        {
            public string Note { get; set; }
        }

        private class RefereeItems
        {
            public List<GameReferee> AllReferees { get; set; }
            public List<GameReferee> SelectedReferees { get; set; }
        }

    }
}

