using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using AppModel;
using WebApi.Models;
using System.Linq.Expressions;
using DataService;
using Omu.ValueInjecter;
using Resources;
using WebApi.Exceptions;
using DataService.DTO;

namespace WebApi.Services
{
    public static class GamesService
    {
        private static readonly GamesRepo _gamesRepo = new GamesRepo();

        public static GameSetViewModel CreateGameSet(CreateGameSetViewModel viewModel)
        {
            var gamesCycle = _gamesRepo.GetGameCycleById(viewModel.GameCycleId);

            if (gamesCycle == null) throw new NotFoundEntityException("Game cycle with this Id doesn't exist");
            if (!gamesCycle.IsPublished) throw new NotFoundEntityException("Game cycle with this Id doesn't published");

            var gameSet = new GameSet();
            gameSet.InjectFrom(viewModel);
            _gamesRepo.AddGameSet(gameSet);

            var gameSetViewModel = new GameSetViewModel();
            gameSetViewModel.InjectFrom(gameSet);

            return gameSetViewModel;
        }

        public static void UpdateGameSet(int gameSetId, CreateGameSetViewModel viewModel)
        {
            viewModel.GameSetId = gameSetId;

            var gameSet = _gamesRepo.GetGameSetById(gameSetId);
            if (gameSet == null) throw new NotFoundEntityException("Game set with this Id doesn't exist");

            var gamesCycle = _gamesRepo.GetGameCycleById(viewModel.GameCycleId);
            if (gamesCycle == null) throw new NotFoundEntityException("Game cycle with this Id doesn't exist");
            if (!gamesCycle.IsPublished) throw new NotFoundEntityException("Game cycle with this Id doesn't published");

            gameSet.InjectFrom(viewModel);
            _gamesRepo.UpdateGameSet(gameSet);
        }

        public static List<UserBaseViewModel> GetGoingFans(int cycleId, int currentUserId)
        {
            using (var db = new DataEntities())
            {
                var result = new List<UserBaseViewModel>();
                var games = db.GamesCycles.Where(x => x.IsPublished).ToList();
                foreach (var g in games.Where(x => x.CycleId == cycleId))
                {
                    foreach (var gu in g.Users)
                    {
                        foreach (var us in gu.Friends.Where(t => t.FriendId == currentUserId && t.UserId == gu.UserId).DefaultIfEmpty())
                        {
                            foreach (var usf in gu.UsersFriends.Where(t => t.UserId == currentUserId && t.FriendId == gu.UserId).DefaultIfEmpty())
                            {
                                if (gu.UsersType.TypeRole == "players")
                                {

                                    var ppv = PlayerService.GetPlayerProfile(gu, null);
                                    if (gu.Image == null)
                                    {
                                        gu.Image = ppv.Image;
                                    }
                                }

                                result.Add(new UserBaseViewModel
                                    {
                                        Id = gu.UserId,
                                        UserName = gu.UserName,
                                        FullName = gu.FullName,
                                        Image = gu.Image,
                                        CanRcvMsg = true,
                                        UserRole = gu.UsersType.TypeRole,
                                        FriendshipStatus = (us == null && usf == null)
                                            ? FriendshipStatus.No
                                            :
                                            ((us != null && us.IsConfirmed) || (usf != null && usf.IsConfirmed))
                                                ?
                                                FriendshipStatus.Yes
                                                :
                                                FriendshipStatus.Pending
                                    }
                                );
                            }
                        }
                    }
                }
                return result;
                //return (from g in db.GamesCycles
                //        from gu in g.Users
                //        from us in gu.Friends.Where(t => t.FriendId == currentUserId && t.UserId == gu.UserId).DefaultIfEmpty()
                //        from usf in gu.UsersFriends.Where(t => t.UserId == currentUserId && t.FriendId == gu.UserId).DefaultIfEmpty()
                //        where g.CycleId == cycleId
                //        select new UserBaseViewModel
                //        {
                //            Id = gu.UserId,
                //            UserName = gu.UserName,
                //            FullName = gu.FullName,
                //            Image = gu.Image,
                //            CanRcvMsg = true,
                //            UserRole = gu.UsersType.TypeRole,
                //            FriendshipStatus = (us == null && usf == null) ? FriendshipStatus.No :
                //            ((us != null && us.IsConfirmed) || (usf != null && usf.IsConfirmed)) ? FriendshipStatus.Yes :
                //            FriendshipStatus.Pending
                //        }).ToList();
            }

            //DataEntities db = new DataEntities();
            //if (currentUserId == 0)
            //{
            //    return db.GamesCycles.Find(cycleId).Users
            //        .Where(u => u.IsActive == true && u.IsArchive == false)
            //        .Select(u =>
            //            new UserBaseViewModel
            //            {
            //                Id = u.UserId,
            //                UserName = u.UserName,
            //                Image = u.Image,
            //                IsFriend = false
            //            });
            //}
            //else
            //{
            //    var fans = db.GamesCycles.Find(cycleId).Users
            //        .Where(u => u.IsActive == true && u.IsArchive == false).
            //        Select(u =>
            //            new UserBaseViewModel
            //            {
            //                Id = u.UserId,
            //                UserName = u.UserName,
            //                Image = u.Image
            //            }).ToList();

            //    FriendsService.AreFansFriends(fans, currentUserId);
            //    return fans;
            //}
        }

        // Add: Cheng Li For Gymnastics and Karate
        public static List<UserBaseViewModel> GetGoingFansForSpecial(int leagueId, int currentUserId)
        {
            using (var db = new DataEntities())
            {
                var result = new List<UserBaseViewModel>();
                var list = db.LeaguesFans.Where(df => df.LeagueId == leagueId).ToList();

                foreach (var l in list)
                {
                    var gu = db.Users.FirstOrDefault(u => u.UserId == l.UserId);
                    foreach (var us in gu.Friends.Where(t => t.FriendId == currentUserId && t.UserId == gu.UserId).DefaultIfEmpty())
                    {
                        foreach (var usf in gu.UsersFriends.Where(t => t.UserId == currentUserId && t.FriendId == gu.UserId).DefaultIfEmpty())
                        {
                            if (gu.UsersType.TypeRole == "players")
                            {

                                var ppv = PlayerService.GetPlayerProfile(gu, null);
                                if (gu.Image == null)
                                {
                                    gu.Image = ppv.Image;
                                }
                            }
                            result.Add(new UserBaseViewModel
                            {
                                Id = gu.UserId,
                                UserName = gu.UserName,
                                FullName = gu.FullName,
                                Image = gu.Image,
                                CanRcvMsg = true,
                                UserRole = gu.UsersType.TypeRole,
                                FriendshipStatus = (us == null && usf == null) ? FriendshipStatus.No :
                                ((us != null && us.IsConfirmed) || (usf != null && usf.IsConfirmed)) ? FriendshipStatus.Yes :
                                FriendshipStatus.Pending
                            });
                        }
                    }
                }
                return result;
            }
        }
        public static List<UserBaseViewModel> GetGoingClubFans(int currentUserId, TeamScheduleScrapper scrapper, int seasonId)
        {
            var result = new List<UserBaseViewModel>();
            foreach (var gu in scrapper.Users)
            {
                foreach (var us in gu.Friends.Where(t => t.FriendId == currentUserId && t.UserId == gu.UserId).DefaultIfEmpty())
                {
                    foreach (var usf in gu.UsersFriends.Where(t => t.UserId == currentUserId && t.FriendId == gu.UserId).DefaultIfEmpty())
                    {
                        if (gu.UsersType.TypeRole == "players")
                        {

                            var ppv = PlayerService.GetPlayerProfile(gu, null);
                            if (gu.Image == null)
                            {
                                gu.Image = ppv.Image;
                            }
                        }

                        result.Add(new UserBaseViewModel
                            {
                                Id = gu.UserId,
                                UserName = gu.UserName,
                                FullName = gu.FullName,
                                Image = gu.Image,
                                CanRcvMsg = true,
                                UserRole = gu.UsersType.TypeRole,
                                FriendshipStatus = (us == null && usf == null)
                                    ? FriendshipStatus.No
                                    :
                                    ((us != null && us.IsConfirmed) || (usf != null && usf.IsConfirmed))
                                        ?
                                        FriendshipStatus.Yes
                                        :
                                        FriendshipStatus.Pending
                            }
                        );
                    }
                }
            }
            return result;
        }

        public static NextGameViewModel GetNextGame(IEnumerable<GamesCycle> games, int currentUserId, int leagueId, int? seasonId = null)
        {
            var gameCycle = games.Where(g => /*(g.GameStatus == GameStatus.Started || g.GameStatus == GameStatus.Next) && */g.StartDate >= DateTime.Now && g.IsPublished).OrderBy(g => g.StartDate).FirstOrDefault();
            /*if (gameCycle != null)
            {
                gameCycle = games.Where(g => g.StartDate >= DateTime.Now && g.IsPublished).OrderBy(g => g.StartDate).FirstOrDefault();
            }*/

            if (gameCycle == null)
            {
                return null;
            }

            return ParseNextGameCycle(currentUserId, gameCycle, leagueId, seasonId);
        }
        public static NextGameViewModel GetNextGameTennis(IEnumerable<TennisGameCycle> games, int currentUserId, int leagueId, int? seasonId = null)
        {
            var gameCycle = games.Where(g => g.StartDate >= DateTime.Now && g.IsPublished).OrderBy(g => g.StartDate).FirstOrDefault();
            /*if (gameCycle != null)
            {
                gameCycle = games.Where(g => g.StartDate >= DateTime.Now && g.IsPublished).OrderBy(g => g.StartDate).FirstOrDefault();
            }*/

            if (gameCycle == null)
            {
                return null;
            }

            return ParseNextGameCycleTennis(currentUserId, gameCycle, leagueId, seasonId);
        }

        public static NextGameViewModel GetNextTennisGame(IEnumerable<TennisGameCycle> games, int currentUserId, int leagueId, int? seasonId = null)
        {
            //TennisGameCycle gameCycle = games.Where(g => g.StartDate >= DateTime.Now && g.IsPublished).OrderBy(g => g.StartDate).FirstOrDefault();
            var gameCycle = games.Where(g => g.StartDate >= DateTime.Now&& g.IsPublished).OrderBy(g => g.StartDate).FirstOrDefault();
            if (gameCycle == null)
            {
                return null;
            }

            return ParseNextTennisGameCycle(currentUserId, gameCycle, leagueId, seasonId);
        }

        public static NextGameViewModel GetNextGameForSpecial(GamesCycle gameCycle, LeagueInfoVeiwModel leauge, int currentUserId, int leagueId, int? seasonId = null)
        {
            if (leauge.LeagueStartDate < DateTime.Now)
                return null;

            if (gameCycle == null)
                return null;

            gameCycle.StartDate = leauge.LeagueStartDate;
            gameCycle.Auditorium.Name = leauge.PlaceOfCompetition;
            return ParseNextGameCycle(currentUserId, gameCycle, leagueId, seasonId);
        }

        public static GameViewModel GetLastGame(IEnumerable<GamesCycle> games, int? seasonId = null)
        {
            var gameCycle = games.Where(g => g.GameStatus == GameStatus.Ended && g.IsPublished).OrderBy(g => g.StartDate).LastOrDefault();
            if (gameCycle == null)
            {
                return null;
            }
            var vm = ParseGameCycle(gameCycle, seasonId);
            return vm;
        }
        public static GameViewModel GetLastGameTennis(IEnumerable<TennisGameCycle> games, int? seasonId = null)
        {
            var gameCycle = games.Where(g => g.GameStatus == GameStatus.Ended && g.IsPublished).OrderBy(g => g.StartDate).LastOrDefault();
            if (gameCycle == null)
            {
                return null;
            }
            var vm = ParseTennisGameCycle(gameCycle, seasonId);
            return vm;
        }
        public static GameViewModel GetLastTennisGame(IEnumerable<TennisGameCycle> games, int? seasonId = null)
        {
            var gameCycle = games.Where(g => g.GameStatus == GameStatus.Ended && g.IsPublished).OrderBy(g => g.StartDate).LastOrDefault();
            if (gameCycle == null)
            {
                return null;
            }
            var vm = ParseTennisGameCycle(gameCycle, seasonId);
            return vm;
        }
        public static IEnumerable<GameViewModel> GetNextGames(IEnumerable<GamesCycle> leagueGames, DateTime fromDate, int? seasonId = null)
        {
            var temp = leagueGames.Where(g => g.IsPublished && (g.GameStatus == GameStatus.Next || g.GameStatus == null) && g.StartDate >= fromDate)
                .OrderBy(g => g.StartDate)
                .ToList()
                .Select(g => ParseGameCycle(g, seasonId));
            return temp;
        }
        public static IEnumerable<GameViewModel> GetNextTennisGames(IEnumerable<TennisGameCycle> leagueGames, DateTime fromDate, int? seasonId = null)
        {
            //var temp = leagueGames.Where(g => g.IsPublished && (g.GameStatus == GameStatus.Next || g.GameStatus == null) && g.StartDate >= fromDate)
            //    .OrderBy(g => g.StartDate)
            //    .ToList()
            //    .Select(g => ParseTennisGameCycle(g, seasonId));
            var temp = leagueGames.Where(g => g.IsPublished && g.StartDate >= fromDate&&g.IsPublished)
                .OrderBy(g => g.StartDate)
                .ToList()
                .Select(g => ParseTennisGameCycle(g, seasonId));
            return temp;
        }
        public static IEnumerable<GameViewModel> GetTeamNextGames(int leagueId, int teamId, DateTime fromDate, int? seasonId, int currentUserId)
        {
            using (var db = new DataEntities())
            {
                Func<GameDto, bool> predicate = g => g.StartDate > fromDate &&
                                                    g.LeagueId == leagueId &&
                                                    (g.HomeTeamId == teamId || g.GuestTeamId == teamId) && g.IsPublished;
                var teamNextGames = GetGamesQuery(db)
                                                     .Where(predicate)
                                                     .Select(x => CycleToGame(x, seasonId))
                                                     .OrderBy(g => g.StartDate)
                                                     .ToList();
                for(var i = 0; i < teamNextGames.Count; i++)
                {
                    var game = teamNextGames[i];
                    var cycle = db.GamesCycles.Where(g => g.CycleId == game.GameId).FirstOrDefault();
                    teamNextGames[i].IsGoing = ParseNextGameCycle(currentUserId, cycle, leagueId, seasonId).IsGoing;
                }
                return teamNextGames;
            }
        }

        public static IEnumerable<GameViewModel> GetLastGames(IEnumerable<GamesCycle> leagueGames, int? seasonId)
        {
            return leagueGames.Where(g => g.StartDate < DateTime.Now)
                .Where(g => g.GuestTeamId.HasValue && g.HomeTeamId.HasValue && g.IsPublished)
                .OrderByDescending(g => g.StartDate)
                .ToList()
                .Select(g => ParseGameCycle(g, seasonId));
        }

        public static IEnumerable<GameViewModel> GetLastTennisGames(IEnumerable<TennisGameCycle> leagueGames, int? seasonId)
        {
            return leagueGames.Where(g => g.StartDate < DateTime.Now && g.GameStatus == "ended")
                .Where(g => g.FirstPlayerId.HasValue && g.SecondPlayerId.HasValue && g.IsPublished)
                .OrderByDescending(g => g.StartDate)
                .ToList()
                .Select(g => ParseTennisGameCycle(g, seasonId));
        }

        public static List<User> GetGoingFriends(int gameId, User currentUser)
        {
            using (var db = new DataEntities())
            {
                var usersList = (from c in db.GamesCycles
                                 from u in c.Users
                                 where c.CycleId == gameId && c.IsPublished
                                 select u.UserId).ToList();

                var friends = FriendsService.GetAllConfirmedFriendsAsUsers(currentUser);
                friends.Add(currentUser); // Cheng Li. contain me
                return friends.Where(t => usersList.Contains(t.UserId)).ToList();
            }
        }

        public static List<User> GetClubGoingFriends(int gameId, User currentUser)
        {
            using (var db = new DataEntities())
            {
                var usersList = (from c in db.TeamScheduleScrappers
                                 from u in c.Users
                                 where c.Id == gameId
                                 select u.UserId).ToList();

                var friends = FriendsService.GetAllConfirmedFriendsAsUsers(currentUser);
                friends.Add(currentUser); // Cheng Li. contain me
                return friends.Where(t => usersList.Contains(t.UserId)).ToList();
            }
        }

        internal static GameViewModel ParseGameCycleTennis(int gameId)
        {
            using (var db = new DataEntities())
            {
                var game = db.GamesCycles.Include(t => t.Auditorium).FirstOrDefault(t => t.CycleId == gameId && t.IsPublished);
                if (game != null)
                {
                    return ParseGameCycle(game);
                }
            }
            return null;
        }

        public static Expression<Func<GamesCycle, GameViewModel>> CycleToGame()
        {
            return c => new GameViewModel
            {
                GameId = c.CycleId,
                GameCycleStatus = c.GameStatus,
                StartDate = c.StartDate,
                HomeTeamId = c.HomeTeamId,
                HomeTeam = c.HomeTeam.Title,
                HomeTeamScore = c.HomeTeamScore,
                GuestTeam = c.GuestTeam.Title,
                GuestTeamId = c.GuestTeamId,
                GuestTeamScore = c.GuestTeamScore,
                Auditorium = c.Auditorium.Name,
                AuditoriumAddress = c.Auditorium.Address,
                HomeTeamLogo = c.HomeTeam.Logo,
                GuestTeamLogo = c.GuestTeam.Logo,
                CycleNumber = c.CycleNum,
                LeagueId = c.Stage.LeagueId,
                LeagueName = c.Stage.League.Name
            };
        }

        public static GameViewModel CycleToGame(GamesCycle gamesCycle)
        {
            var vm = new GameViewModel
            {
                GameId = gamesCycle.CycleId,
                GameCycleStatus = gamesCycle.GameStatus,
                StartDate = gamesCycle.StartDate,
                HomeTeamId = gamesCycle.HomeTeamId,
                HomeTeam = gamesCycle.HomeTeam.Title,
                HomeTeamScore = gamesCycle.HomeTeamScore,
                GuestTeam = gamesCycle.GuestTeam.Title,
                GuestTeamId = gamesCycle.GuestTeamId,
                GuestTeamScore = gamesCycle.GuestTeamScore,
                Auditorium = gamesCycle.Auditorium.Name,
                AuditoriumAddress = gamesCycle.Auditorium.Address,
                HomeTeamLogo = gamesCycle.HomeTeam.Logo,
                GuestTeamLogo = gamesCycle.GuestTeam.Logo,
                CycleNumber = gamesCycle.CycleNum,
                LeagueId = gamesCycle.Stage.LeagueId,
                LeagueName = gamesCycle.Stage.League.Name
            };

            return vm;
        }

        internal static NextGameViewModel ParseNextClubGame(int curUserId, TeamScheduleScrapper scrapper, int seasonId)
        {
            if (scrapper != null)
            {
                IEnumerable<UserBaseViewModel> FansList = GetGoingClubFans(curUserId, scrapper, seasonId);
                var me = FansList.FirstOrDefault(t => t.Id == curUserId);
                var result = new NextGameViewModel
                {
                    GameId = scrapper.Id,
                    StartDate = scrapper.StartDate,
                    Auditorium = scrapper.Auditorium,
                    AuditoriumAddress = scrapper.Auditorium,
                    HomeTeam = scrapper.HomeTeam,
                    GuestTeam = scrapper.GuestTeam,
                    FansList = FansList.ToList(),
                    FriendsGoing = FansList.Count(t => t.FriendshipStatus == FriendshipStatus.Yes),
                    FansGoing = FansList.Count(t => t.FriendshipStatus != FriendshipStatus.Yes),
                    IsGoing = me != null ? 1 : 0
                };
                return result;
            }
            return null;
        }

        internal static GameViewModel ParseGameCycle(GamesCycle gameCycle, int? seasondId = null)
        {
            if (gameCycle != null)
            {

                var typePlayOff = 2;
                if (gameCycle.PlayoffBracket != null)
                    typePlayOff = gameCycle.PlayoffBracket.Type;

                var gameModel = new GameViewModel
                {
                    HouseName = gameCycle.Group != null ? gameCycle.Group.Name : "",
                    GroupName = gameCycle.Auditorium != null ? gameCycle.Auditorium.Name : "",
                    GameId = gameCycle.CycleId,
                    GameCycleStatus = gameCycle.GameStatus ?? "next",
                    StartDate = gameCycle.StartDate,
                    HomeTeamId = gameCycle.HomeTeamId,
                    HomeTeam = !gameCycle.HomeTeamId.HasValue ? (typePlayOff == 2 ? "Winner" : "Loser") : gameCycle.HomeTeam.Title,
                    HomeTeamScore = gameCycle.HomeTeamScore,
                    GuestTeam = !gameCycle.GuestTeamId.HasValue ? (typePlayOff == 2 ? "Winner" : "Loser") : gameCycle.GuestTeam.Title,
                    GuestTeamId = gameCycle.GuestTeamId,
                    GuestTeamScore = gameCycle.GuestTeamScore,
                    Auditorium = gameCycle.Auditorium != null ? gameCycle.Auditorium.Name : null,
                    AuditoriumAddress = gameCycle.Auditorium != null ? gameCycle.Auditorium.Address : null,
                    HomeTeamLogo = gameCycle.HomeTeam != null ? gameCycle.HomeTeam.Logo : string.Empty,
                    GuestTeamLogo = gameCycle.GuestTeam != null ? gameCycle.GuestTeam.Logo : string.Empty,
                    CycleNumber = gameCycle.CycleNum,
                    MaxPlayoffPos = gameCycle.MaxPlayoffPos,
                    MinPlayoffPos = gameCycle.MinPlayoffPos
                };

                var alias = gameCycle?.Stage?.League?.Union?.Section?.Alias;
                switch (alias)
                {
                    case GamesAlias.WaterPolo:
                    case GamesAlias.Soccer:
                    case GamesAlias.Rugby:
                    case GamesAlias.BasketBall:
                    case GamesAlias.Softball:
                        if (gameCycle.GameSets.Any())
                        {
                            gameModel.HomeTeamScore = gameCycle.GameSets.Sum(x => x.HomeTeamScore);
                            gameModel.GuestTeamScore = gameCycle.GameSets.Sum(x => x.GuestTeamScore);
                        }
                        break;
                }


                if (gameCycle.Group != null && gameCycle.Group.IsAdvanced && gameCycle.Group.PlayoffBrackets != null)
                {
                    var numOfBrackets = gameCycle.Group.PlayoffBrackets.Count;
                    switch (numOfBrackets)
                    {
                        case 1:
                            gameModel.PlayOffType = Messages.Final;
                            break;
                        case 2:
                            gameModel.PlayOffType = Messages.Semifinals;
                            break;
                        case 4:
                            gameModel.PlayOffType = Messages.Quarter_finals;
                            break;
                        case 8:
                            gameModel.PlayOffType = Messages.Final_Eighth;
                            break;
                        default:
                            gameModel.PlayOffType = (numOfBrackets * 2) + " " + Messages.FinalNumber;
                            break;
                    }
                }

                if (seasondId.HasValue && gameCycle.GuestTeam != null && gameCycle.HomeTeam != null)
                {
                    var homeTeamsDetails = gameCycle.HomeTeam.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasondId);
                    gameModel.HomeTeam = homeTeamsDetails != null ? homeTeamsDetails.TeamName : gameCycle.HomeTeam.Title;

                    var guestTeamsDetails = gameCycle.GuestTeam.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasondId);
                    gameModel.GuestTeam = guestTeamsDetails != null ? guestTeamsDetails.TeamName : gameCycle.GuestTeam.Title;
                }
                else
                {
                    gameModel.HomeTeam = !gameCycle.HomeTeamId.HasValue ? (typePlayOff == 2 ? "Winner" : "Loser") : gameCycle.HomeTeam.Title;
                    gameModel.GuestTeam = !gameCycle.GuestTeamId.HasValue ? (typePlayOff == 2 ? "Winner" : "Loser") : gameCycle.GuestTeam.Title;
                }

                return gameModel;
            }
            return null;
        }
        internal static GameViewModel ParseTennisGameCycle(TennisGameCycle gameCycle, int? seasondId = null)
        {
            if (gameCycle != null)
            {

                var typePlayOff = 2;
                if (gameCycle.TennisPlayoffBracket != null)
                    typePlayOff = gameCycle.TennisPlayoffBracket.Type;

                var gameModel = new GameViewModel
                {
                    HouseName = gameCycle.TennisGroup != null ? gameCycle.TennisGroup.Name : "",
                    GroupName = gameCycle.Auditorium != null ? gameCycle.Auditorium.Name : "",
                    GameId = gameCycle.CycleId,
                    GameCycleStatus = gameCycle.GameStatus != null ? gameCycle.GameStatus : "next",
                    StartDate = gameCycle.StartDate,
                    HomeTeamId = gameCycle.FirstPlayer != null ? gameCycle.FirstPlayer.UserId : 0,
                    GuestTeamId = gameCycle.SecondPlayer != null ? gameCycle.SecondPlayer.UserId : 0,
                    HomeTeam = gameCycle.FirstPlayer != null ? gameCycle.FirstPlayer.User.FullName !=null? gameCycle.FirstPlayer.User.FullName:gameCycle.FirstPlayer.User.UserName : string.Empty,
                    HomeTeamPair = gameCycle.FirstPairPlayer != null ? gameCycle.FirstPairPlayer.User.FullName != null ? gameCycle.FirstPairPlayer.User.FullName : gameCycle.FirstPairPlayer.User.UserName : string.Empty,
                    HomeTeamScore = gameCycle.FirstPlayerScore,
                    GuestTeam = gameCycle.SecondPlayer != null ? gameCycle.SecondPlayer.User.FullName!=null?gameCycle.SecondPlayer.User.FullName:gameCycle.SecondPlayer.User.UserName : string.Empty,
                    GuestTeamPair = gameCycle.SecondPairPlayer != null ? gameCycle.SecondPairPlayer.User.FullName != null ? gameCycle.SecondPairPlayer.User.FullName : gameCycle.SecondPairPlayer.User.UserName : string.Empty,
                    GuestTeamScore = gameCycle.SecondPlayerScore,
                    Auditorium = gameCycle.Auditorium != null ? gameCycle.Auditorium.Name : null,
                    AuditoriumAddress = gameCycle.Auditorium != null ? gameCycle.Auditorium.Address : null,
                    HomeTeamLogo = gameCycle.FirstPlayer != null ? gameCycle.FirstPlayer.User.Image : string.Empty,
                    GuestTeamLogo = gameCycle.SecondPlayer != null ? gameCycle.SecondPlayer.User.Image : string.Empty,
                    CycleNumber = gameCycle.CycleNum,
                    MaxPlayoffPos = gameCycle.MaxPlayoffPos,
                    MinPlayoffPos = gameCycle.MinPlayoffPos,
                    TimeInitial = gameCycle.TimeInitial
                };

                if (gameModel.HomeTeamLogo == string.Empty && gameCycle.FirstPlayer != null )
                {
                    var ppv = PlayerService.GetPlayerProfile(gameCycle.FirstPlayer.User, seasondId);
                    gameModel.HomeTeamLogo = ppv.Image;
                    
                }
                if (gameModel.GuestTeamLogo == string.Empty && gameCycle.SecondPlayer != null)
                {
                    var ppv = PlayerService.GetPlayerProfile(gameCycle.SecondPlayer.User, seasondId);
                    gameModel.GuestTeamLogo = ppv.Image;
                }

                /**
                                if (gameCycle.TennisGameSets.Any())
                                {
                                    gameModel.HomeTeamScore = gameCycle.TennisGameSets.Sum(x => x.FirstPlayerScore);
                                    gameModel.GuestTeamScore = gameCycle.TennisGameSets.Sum(x => x.SecondPlayerScore);
                                }
*/
                if (gameCycle.TennisGroup != null && gameCycle.TennisGroup.IsAdvanced && gameCycle.TennisGroup.TennisPlayoffBrackets != null)
                {
                    var numOfBrackets = gameCycle.TennisGroup.TennisPlayoffBrackets.Count;
                    switch (numOfBrackets)
                    {
                        case 1:
                            gameModel.PlayOffType = Messages.Final;
                            break;
                        case 2:
                            gameModel.PlayOffType = Messages.Semifinals;
                            break;
                        case 4:
                            gameModel.PlayOffType = Messages.Quarter_finals;
                            break;
                        case 8:
                            gameModel.PlayOffType = Messages.Final_Eighth;
                            break;
                        default:
                            gameModel.PlayOffType = (numOfBrackets * 2) + " " + Messages.FinalNumber;
                            break;
                    }
                }
                return gameModel;
            }
            return null;
        }

        internal static List<GameViewModel> GetGameHistory(int? guestId, int? homeId, int? seasonId = null)
        {
            using (var db = new DataEntities())
            {
                Func<GameDto, bool> predicate = g => ((g.HomeTeamId == homeId && g.GuestTeamId == guestId) ||
                                                      (g.HomeTeamId == guestId && g.GuestTeamId == homeId)) &&
                                                       g.GameCycleStatus == GameStatus.Ended && g.IsPublished;
                var game = GetGamesQuery(db)
                                        .Where(predicate)
                                        .Select(x => CycleToGame(x, seasonId))
                                        .OrderByDescending(g => g.StartDate)
                                        .Take(5)
                                        .ToList();
                return game;
            }
        }

        internal static List<GameViewModel> GetTennisGameHistory(int? guestId, int? homeId, int? seasonId = null)
        {
            using (var db = new DataEntities())
            {
                Func<TennisGameCycle, bool> predicate = g =>
                    ((g.TeamsPlayer?.UserId == homeId || g.FirstPlayerId == homeId) &&
                     (g.TeamsPlayer1?.UserId == guestId || g.SecondPlayerId == guestId) ||
                     (g.TeamsPlayer?.UserId == guestId || g.FirstPlayerId == guestId) &&
                     (g.TeamsPlayer1?.UserId == homeId || g.SecondPlayerId == homeId)) &&
                    g.GameStatus == GameStatus.Ended &&
                    g.IsPublished;

                var game = GetTennisGames(db, predicate)
                    .Select(x => CycleToTennisGame(x, seasonId))
                    .OrderByDescending(g => g.StartDate)
                    .Take(5)
                    .ToList();

                return game;
            }
        }

        private static NextGameViewModel ParseNextGameCycle(int currentUserId, GamesCycle gameCycle, int leagueId, int? seasonId = null)
        {
            var gvm = new NextGameViewModel();
            if (gameCycle != null)
            {
                IEnumerable<UserBaseViewModel> FansList = GetGoingFans(gameCycle.CycleId, currentUserId).OrderByDescending(t => t.FriendshipStatus);
                gvm = new NextGameViewModel
                {
                    LeagueId = leagueId,
                    GameId = gameCycle.CycleId,
                    GameCycleStatus = gameCycle.GameStatus,
                    StartDate = gameCycle.StartDate,
                    HomeTeamId = gameCycle.HomeTeamId,
                    HomeTeamScore = gameCycle.HomeTeamScore,
                    GuestTeamId = gameCycle.GuestTeamId,
                    GuestTeamScore = gameCycle.GuestTeamScore,
                    Auditorium = gameCycle.Auditorium != null ? gameCycle.Auditorium.Name : null,
                    AuditoriumAddress = gameCycle.Auditorium != null ? gameCycle.Auditorium.Address : null,
                    FansList = FansList.ToList(),
                    FriendsGoing = FansList.Count(t => t.FriendshipStatus == FriendshipStatus.Yes),
                    FansGoing = FansList.Count(t => t.FriendshipStatus != FriendshipStatus.Yes),
                    HomeTeamLogo = gameCycle != null && gameCycle.HomeTeam != null ? gameCycle.HomeTeam.Logo : string.Empty,
                    GuestTeamLogo = gameCycle != null && gameCycle.GuestTeam != null ? gameCycle.GuestTeam.Logo : string.Empty,
                    CycleNumber = gameCycle.CycleNum
                };

                var alias = gameCycle?.Stage?.League?.Union?.Section?.Alias;
                switch (alias)
                {
                    case GamesAlias.WaterPolo:
                    case GamesAlias.Soccer:
                    case GamesAlias.Rugby:
                    case GamesAlias.BasketBall:
                    case GamesAlias.Softball:
                        if (gameCycle.GameSets.Any())
                        {
                            gvm.HomeTeamScore = gameCycle.GameSets.Sum(x => x.HomeTeamScore);
                            gvm.GuestTeamScore = gameCycle.GameSets.Sum(x => x.GuestTeamScore);
                        }
                        break;
                }


                if (gameCycle.Group != null && gameCycle.Group.IsAdvanced && gameCycle.Group.PlayoffBrackets != null)
                {
                    var numOfBrackets = gameCycle.Group.PlayoffBrackets.Count;
                    switch (numOfBrackets)
                    {
                        case 1:
                            gvm.PlayOffType = Messages.Final;
                            break;
                        case 2:
                            gvm.PlayOffType = Messages.Semifinals;
                            break;
                        case 4:
                            gvm.PlayOffType = Messages.Quarter_finals;
                            break;
                        case 8:
                            gvm.PlayOffType = Messages.Final_Eighth;
                            break;
                        default:
                            gvm.PlayOffType = (numOfBrackets * 2) + " " + Messages.FinalNumber;
                            break;
                    }
                }

                var me = gvm.FansList.FirstOrDefault(t => t.Id == currentUserId);
                if (me != null)
                {
                    gvm.IsGoing = 1;
                }
                else
                {
                    gvm.IsGoing = 0;
                }

                if (seasonId.HasValue && gameCycle.HomeTeam != null && gameCycle.GuestTeam != null)
                {
                    var homeTeamDetails = gameCycle.HomeTeam.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId.Value);
                    gvm.HomeTeam = homeTeamDetails != null ? homeTeamDetails.TeamName : gameCycle.HomeTeam.Title;

                    var guestTeamDetails = gameCycle.GuestTeam.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId.Value);
                    gvm.GuestTeam = guestTeamDetails != null ? guestTeamDetails.TeamName : gameCycle.GuestTeam.Title;
                }
                else
                {
                    gvm.HomeTeam = gameCycle.HomeTeam != null ? gameCycle.HomeTeam.Title : string.Empty;
                    gvm.GuestTeam = gameCycle.GuestTeam != null ? gameCycle.GuestTeam.Title : string.Empty;
                }
            }
            return gvm;
        }
        private static NextGameViewModel ParseNextGameCycleTennis(int currentUserId, TennisGameCycle gameCycle, int leagueId, int? seasonId = null)
        {
            var gvm = new NextGameViewModel();
            if (gameCycle != null)
            {
                IEnumerable<UserBaseViewModel> FansList = GetGoingFans(gameCycle.CycleId, currentUserId).OrderByDescending(t => t.FriendshipStatus);
                gvm = new NextGameViewModel
                {
                    LeagueId = leagueId,
                    GameId = gameCycle.CycleId,
                    GameCycleStatus = gameCycle.GameStatus,
                    StartDate = gameCycle.StartDate,
                    HomeTeamId = gameCycle.FirstPlayer.UserId,
                    HomeTeam = gameCycle.FirstPlayer.User.FullName != null ? gameCycle.FirstPlayer.User.FullName : gameCycle.FirstPlayer.User.UserName,
                    HomeTeamScore = gameCycle.FirstPlayerScore,
                    GuestTeamId = gameCycle.SecondPlayer.UserId,
                    GuestTeam = gameCycle.SecondPlayer.User.FullName != null ? gameCycle.SecondPlayer.User.FullName : gameCycle.SecondPlayer.User.UserName,
                    GuestTeamScore = gameCycle.SecondPlayerScore,
                    Auditorium = gameCycle.Auditorium != null ? gameCycle.Auditorium.Name : null,
                    AuditoriumAddress = gameCycle.Auditorium != null ? gameCycle.Auditorium.Address : null,
                    FansList = FansList.ToList(),
                    FriendsGoing = FansList.Count(t => t.FriendshipStatus == FriendshipStatus.Yes),
                    FansGoing = FansList.Count(t => t.FriendshipStatus != FriendshipStatus.Yes),
                    HomeTeamLogo = gameCycle != null && gameCycle.FirstPlayer != null ? gameCycle.FirstPlayer.User.Image!=null?
                    gameCycle.FirstPlayer.User.Image: string.Empty:"",
                    GuestTeamLogo = gameCycle != null && gameCycle.SecondPlayer != null ? gameCycle.SecondPlayer.User.Image != null ?
                    gameCycle.SecondPlayer.User.Image : string.Empty:"",
                    CycleNumber = gameCycle.CycleNum,
                    gameType = 1,
                    TimeInitial = gameCycle.TimeInitial
                };
                if (gvm.HomeTeamLogo == string.Empty && gameCycle.FirstPlayer != null)
                {
                    var ppv = PlayerService.GetPlayerProfile(gameCycle.FirstPlayer.User, seasonId);
                    gvm.HomeTeamLogo = ppv.Image;

                }
                if (gvm.GuestTeamLogo == string.Empty && gameCycle.SecondPlayer != null)
                {
                    var ppv = PlayerService.GetPlayerProfile(gameCycle.SecondPlayer.User, seasonId);
                    gvm.GuestTeamLogo = ppv.Image;
                }
                //if (gameCycle.TennisGameSets.Any())
                //{
                //    gvm.HomeTeamScore = gameCycle.TennisGameSets.Sum(x => x.FirstPlayerScore);
                //    gvm.GuestTeamScore = gameCycle.TennisGameSets.Sum(x => x.SecondPlayerScore);
                //}


                if (gameCycle.TennisGroup != null && gameCycle.TennisGroup.IsAdvanced && gameCycle.TennisGroup.TennisPlayoffBrackets != null)
                {
                    var numOfBrackets = gameCycle.TennisGroup.TennisPlayoffBrackets.Count;
                    switch (numOfBrackets)
                    {
                        case 1:
                            gvm.PlayOffType = Messages.Final;
                            break;
                        case 2:
                            gvm.PlayOffType = Messages.Semifinals;
                            break;
                        case 4:
                            gvm.PlayOffType = Messages.Quarter_finals;
                            break;
                        case 8:
                            gvm.PlayOffType = Messages.Final_Eighth;
                            break;
                        default:
                            gvm.PlayOffType = (numOfBrackets * 2) + " " + Messages.FinalNumber;
                            break;
                    }
                }

                var me = gvm.FansList.FirstOrDefault(t => t.Id == currentUserId);
                if (me != null)
                {
                    gvm.IsGoing = 1;
                }
                if(gvm.HomeTeam ==null ||gvm.GuestTeam == null)
                {
                    var homeTeamDetails = gameCycle.FirstPlayer.User;
                    gvm.HomeTeam = homeTeamDetails != null ? homeTeamDetails.UserName : string.Empty;

                    var guestTeamDetails = gameCycle.SecondPlayer.User;
                    gvm.GuestTeam = guestTeamDetails != null ? guestTeamDetails.UserName : string.Empty;
                }
            }
            return gvm;
        }

        private static NextGameViewModel ParseNextTennisGameCycle(int currentUserId, TennisGameCycle gameCycle, int leagueId, int? seasonId = null)
        {
            var gvm = new NextGameViewModel();
            using (var db = new DataEntities())
            {
                if (gameCycle != null)
                {
                    /** note implement for tennis fas */
                    //IEnumerable<UserBaseViewModel> FansList = GetGoingFans(gameCycle.CycleId, currentUserId).OrderByDescending(t => t.FriendshipStatus);
                    IEnumerable<UserBaseViewModel> FansList =  null;
                    gvm = new NextGameViewModel
                    {
                        LeagueId = leagueId,
                        GameId = gameCycle.CycleId,
                        GameCycleStatus = gameCycle.GameStatus,
                        StartDate = gameCycle.StartDate,
                        HomeTeamId = gameCycle.FirstPlayer != null ? gameCycle.FirstPlayer.UserId : 0,
                        HomeTeamPairId = gameCycle.FirstPairPlayer!=null? gameCycle.FirstPairPlayer.UserId : 0,
                        GuestTeamId = gameCycle.SecondPlayer != null ? gameCycle.SecondPlayer.UserId : 0,
                        GuestTeamPairId = gameCycle.SecondPairPlayer != null ? gameCycle.SecondPairPlayer.UserId : 0,
                        HomeTeamScore = gameCycle.FirstPlayerScore,
                        GuestTeamScore = gameCycle.SecondPlayerScore,
                        HomeTeam = gameCycle.FirstPlayer!=null&&gameCycle.FirstPlayer.User!=null? gameCycle.FirstPlayer.User.FullName:string.Empty,
                        HomeTeamPair = gameCycle.FirstPairPlayer!=null && gameCycle.FirstPairPlayer.User != null ? gameCycle.FirstPairPlayer.User.FullName : string.Empty,
                        GuestTeam = gameCycle.SecondPlayer!=null && gameCycle.SecondPlayer.User != null ? gameCycle.SecondPlayer.User.FullName : string.Empty,
                        GuestTeamPair = gameCycle.SecondPairPlayer!=null && gameCycle.SecondPairPlayer.User != null ? gameCycle.SecondPairPlayer.User.FullName : string.Empty,
                        Auditorium = gameCycle.Auditorium != null ? gameCycle.Auditorium.Name : string.Empty,
                        AuditoriumAddress = gameCycle.Auditorium != null ? gameCycle.Auditorium.Address : string.Empty,
                        FansList = FansList != null ?FansList.ToList() : null,
                        FriendsGoing = FansList != null ? FansList.Count(t => t.FriendshipStatus == FriendshipStatus.Yes) : 0,
                        FansGoing = FansList != null ? FansList.Count(t => t.FriendshipStatus != FriendshipStatus.Yes) : 0,
                        HomeTeamLogo = gameCycle != null && gameCycle.FirstPlayerId != null ? db.TeamsPlayers.Where(x => x.Id == gameCycle.FirstPlayerId).FirstOrDefault().User.Image : string.Empty,
                        GuestTeamLogo = gameCycle != null && gameCycle.SecondPlayerId != null ? db.TeamsPlayers.Where(x => x.Id == gameCycle.SecondPlayerId).FirstOrDefault().User.Image : string.Empty,
                        HomeTeamLogoPair = gameCycle != null && gameCycle.FirstPlayerPairId != null ? db.TeamsPlayers.Where(x => x.Id == gameCycle.FirstPlayerPairId).FirstOrDefault().User.Image : string.Empty,
                        GuestTeamLogoPair = gameCycle != null && gameCycle.SecondPlayerPairId != null ? db.TeamsPlayers.Where(x => x.Id == gameCycle.SecondPlayerPairId).FirstOrDefault().User.Image : string.Empty,
                        CycleNumber = gameCycle.CycleNum,
                        TimeInitial = gameCycle.TimeInitial
                    };
                    if (gvm.HomeTeamLogo == string.Empty || gvm.HomeTeamLogo == null)
                    {
                        if(gameCycle.FirstPlayer != null)
                        {
                            var ppv = PlayerService.GetPlayerProfile(gameCycle.FirstPlayer.User, seasonId);
                            gvm.HomeTeamLogo = ppv.Image;
                        }
                    }
                    if (gvm.HomeTeamLogoPair == string.Empty || gvm.HomeTeamLogoPair == null)
                    {
                        if (gameCycle.FirstPairPlayer != null)
                        {
                            var ppv = PlayerService.GetPlayerProfile(gameCycle.FirstPairPlayer.User, seasonId);
                            gvm.HomeTeamLogoPair = ppv.Image;
                        }
                    }
                    if (gvm.GuestTeamLogo == string.Empty|| gvm.GuestTeamLogo == null)
                    {
                        if (gameCycle.SecondPlayer != null)
                        {
                            var ppv = PlayerService.GetPlayerProfile(gameCycle.SecondPlayer.User, seasonId);
                            gvm.GuestTeamLogo = ppv.Image;
                        }
                    }
                    if (gvm.GuestTeamLogoPair == string.Empty || gvm.GuestTeamLogoPair == null)
                    {
                        if (gameCycle.SecondPairPlayer != null)
                        {
                            var ppv = PlayerService.GetPlayerProfile(gameCycle.SecondPairPlayer.User, seasonId);
                            gvm.GuestTeamLogoPair = ppv.Image;
                        }
                    }
                    /**
                                        if (gameCycle.TennisGameSets.Any())
                                        {
                                            gvm.HomeTeamScore = gameCycle.TennisGameSets.Sum(x => x.FirstPlayerScore);
                                            gvm.GuestTeamScore = gameCycle.TennisGameSets.Sum(x => x.SecondPlayerScore);
                                        }
*/

                    if (gameCycle.TennisGroup != null && gameCycle.TennisGroup.IsAdvanced && gameCycle.TennisGroup.TennisPlayoffBrackets != null)
                    {
                        var numOfBrackets = gameCycle.TennisGroup.TennisPlayoffBrackets.Count;
                        switch (numOfBrackets)
                        {
                            case 1:
                                gvm.PlayOffType = Messages.Final;
                                break;
                            case 2:
                                gvm.PlayOffType = Messages.Semifinals;
                                break;
                            case 4:
                                gvm.PlayOffType = Messages.Quarter_finals;
                                break;
                            case 8:
                                gvm.PlayOffType = Messages.Final_Eighth;
                                break;
                            default:
                                gvm.PlayOffType = (numOfBrackets * 2) + " " + Messages.FinalNumber;
                                break;
                        }
                    }

                    if(gvm.FansList != null)
                    {
                        var me = gvm.FansList.FirstOrDefault(t => t.Id == currentUserId);
                        if (me != null)
                        {
                            gvm.IsGoing = 1;
                        }
                    }
                    //gvm.HomeTeam = db.TeamsPlayers.Where(x => x.Id == gvm.HomeTeamId).FirstOrDefault().User.FullName;
                    //gvm.GuestTeam = db.TeamsPlayers.Where(x => x.Id == gvm.GuestTeamId).FirstOrDefault().User.FullName;
                    if(gameCycle.FirstPlayer != null && gvm.HomeTeam == string.Empty)
                        gvm.HomeTeam = gameCycle.FirstPlayer.User.FullName;
                    if (gameCycle.SecondPlayer != null && gvm.GuestTeam == string.Empty)
                        gvm.GuestTeam = gameCycle.SecondPlayer.User.FullName;
                }
            }
            if ((gvm.HomeTeam == string.Empty || gvm.HomeTeam == null)&&gameCycle.FirstPlayer!=null)
                gvm.HomeTeam = gameCycle.FirstPlayer.User.FullName;
            if ((gvm.GuestTeam == string.Empty|| gvm.GuestTeam == null) && gameCycle.SecondPlayer!=null)
                gvm.GuestTeam = gameCycle.SecondPlayer.User.FullName;
            return gvm;
        }
        public static IList<GameViewModel> GetPlayerLastGames(int teamId, int? seasonId)
        {
            using (var db = new DataEntities())
            {
                var resList = new GameViewModel[3];

                var query = (from gameCycle in db.GamesCycles
                             join homeTeam in db.Teams on gameCycle.HomeTeamId equals homeTeam.TeamId
                             join stage in db.Stages on gameCycle.StageId equals stage.StageId
                             join guestTeam in db.Teams on gameCycle.GuestTeamId equals guestTeam.TeamId
                             join auditorium in db.Auditoriums on gameCycle.AuditoriumId equals auditorium.AuditoriumId into aud
                             from gameCycleAuditorium in aud.DefaultIfEmpty()

                             let homeTeamDetails = homeTeam.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)
                             let guestTeamDetails = guestTeam.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)

                             where gameCycle.IsPublished && (gameCycle.GuestTeamId == teamId || gameCycle.HomeTeamId == teamId)
                             select new GameViewModel
                             {
                                 GameId = gameCycle.CycleId,
                                 GameCycleStatus = gameCycle.GameStatus,
                                 StartDate = gameCycle.StartDate,
                                 HomeTeamId = homeTeam.TeamId,
                                 HomeTeam = homeTeamDetails != null ? homeTeamDetails.TeamName : homeTeam.Title,
                                 HomeTeamScore = gameCycle.HomeTeamScore,
                                 GuestTeam = guestTeamDetails != null ? guestTeamDetails.TeamName : guestTeam.Title,
                                 GuestTeamId = guestTeam.TeamId,
                                 GuestTeamScore = gameCycle.GuestTeamScore,
                                 Auditorium = gameCycle.Auditorium != null ? gameCycle.Auditorium.Name : null,
                                 AuditoriumAddress = gameCycle.Auditorium != null ? gameCycle.Auditorium.Address : null,
                                 HomeTeamLogo = homeTeam.Logo,
                                 GuestTeamLogo = guestTeam.Logo,
                                 CycleNumber = gameCycle.CycleNum,
                                 LeagueId = gameCycle.Stage.LeagueId
                             });
                if(query == null || query.Count() == 0)
                {
                    query = (from gameCycle in db.TeamScheduleScrappers
                             join homeTeam in db.Teams on gameCycle.HomeTeam equals homeTeam.Title
                             join stage in db.TeamScheduleScrapperGames on gameCycle.SchedulerScrapperGamesId equals stage.Id
                             join guestTeam in db.Teams on gameCycle.GuestTeam equals guestTeam.Title
                             join auditorium in db.Auditoriums on gameCycle.Auditorium equals auditorium.Name into aud
                             from gameCycleAuditorium in aud.DefaultIfEmpty()

                             let homeTeamDetails = homeTeam.TeamsDetails.OrderByDescending(x => x.SeasonId).FirstOrDefault() ?? null
                             let guestTeamDetails = guestTeam.TeamsDetails.OrderByDescending(x => x.SeasonId).FirstOrDefault() ?? null

                             where (guestTeam.TeamId == teamId || homeTeam.TeamId == teamId)
                             select new GameViewModel
                             {
                                 GameId = gameCycle.Id,
                                 GameCycleStatus = GameStatus.Closetodate,
                                 StartDate = gameCycle.StartDate,
                                 HomeTeamId = homeTeam.TeamId,
                                 HomeTeam = homeTeamDetails != null ? homeTeamDetails.TeamName : homeTeam.Title,
                                 HomeTeamPair = gameCycle.Score,
                                 GuestTeam = guestTeamDetails != null ? guestTeamDetails.TeamName : guestTeam.Title,
                                 GuestTeamId = guestTeam.TeamId,
                                 //GuestTeamScore = gameCycle.Score,
                                 Auditorium = gameCycle.Auditorium != null ? gameCycle.Auditorium : null,
                                 AuditoriumAddress = stage.GameUrl ?? "",
                                 HomeTeamLogo = homeTeam.Logo,
                                 GuestTeamLogo = guestTeam.Logo,
                                 CycleNumber = gameCycle.SchedulerScrapperGamesId,
                                 //LeagueId = homeTeam.LeagueTeams !=null.FirstOrDefault().LeagueId
                                 });
                    foreach( var q in query)
                    {
                        q.HomeTeamScore = int.Parse(q.HomeTeamPair.Split(':').ElementAt(0));
                        q.GuestTeamScore = int.Parse(q.HomeTeamPair.Split(':').ElementAt(1));
                        //q.GameCycleStatus = GameStatus.Closetodate;
                    }
                }

                var lastGame = query.Where(g => g.GameCycleStatus == GameStatus.Ended)
                    .OrderByDescending(g => g.StartDate)
                    .FirstOrDefault();

                if (lastGame == null)
                {
                    lastGame = query.Where(g => g.StartDate <= DateTime.Now)
                        .OrderByDescending(g => g.StartDate)
                        .FirstOrDefault();
                }

                resList[1] = lastGame;
                //checkmeacer
                if (lastGame != null)
                {
                    var prevGame = query.Where(g => g.StartDate < lastGame.StartDate)
                        .OrderByDescending(g => g.StartDate)
                        .FirstOrDefault();

                    resList[0] = prevGame;
                }

                var nextGame = query.Where(g => g.StartDate >= DateTime.Now)
                    .OrderBy(g => g.StartDate)
                    .FirstOrDefault();

                resList[2] = nextGame;

                return resList;
            }
        }

        public static GameViewModel GetGameById(int gameId, int? seasonId = null)
        {
            using (var db = new DataEntities())
            {
                // game from scrapper
                if (gameId < 0)
                {
                    var scrapperGame = db.TeamScheduleScrappers.Find(-gameId);
                    var team = scrapperGame.TeamScheduleScrapperGame;
                    var vm = new GameViewModel
                    {
                        StartDate = scrapperGame.StartDate,
                        Auditorium = scrapperGame.Auditorium,
                        AuditoriumAddress = scrapperGame.Auditorium,
                        HomeTeam = scrapperGame.HomeTeam,
                        GuestTeam = scrapperGame.GuestTeam,
                        HomeTeamId = scrapperGame.HomeTeam == team.ExternalTeamName ? team.TeamId : -1,
                        GuestTeamId = scrapperGame.GuestTeam == team.ExternalTeamName ? team.TeamId : -1,
                    };

                    return vm;
                }

                Func<GameDto, bool> predicate = gameCycle => gameCycle.GameId == gameId && gameCycle.IsPublished;
                var game = GetGamesQuery(db)
                                        .Where(predicate)
                                        .Select(x => CycleToGame(x, seasonId))
                                        .ToList()
                                        .FirstOrDefault();

                return game;
            }
        }

        public static GameViewModel GetTennisGameById(int gameId, int? seasonId = null)
        {
            using (var db = new DataEntities())
            {
                Func<TennisGameCycle, bool> predicate = gameCycle => gameCycle.CycleId == gameId &&
                                                                     gameCycle.IsPublished;
                var game = GetTennisGames(db, predicate)
                    .Select(x => CycleToTennisGame(x, seasonId))
                    .ToList()
                    .FirstOrDefault();

                return game;
            }
        }

        public static IQueryable<GameDto> GetGamesQuery(DataEntities db)
        {
            var gamesQuery = (from gameCycle in db.GamesCycles
                                              join homeTeam in db.Teams on gameCycle.HomeTeamId equals homeTeam.TeamId
                                              join guestTeam in db.Teams on gameCycle.GuestTeamId equals guestTeam.TeamId
                                              join auditorium in db.Auditoriums on gameCycle.AuditoriumId equals auditorium.AuditoriumId into aud
                                              join stage in db.Stages on gameCycle.StageId equals stage.StageId
                                              join league in db.Leagues on stage.LeagueId equals league.LeagueId

                                              from gameCycleAuditorion in aud.DefaultIfEmpty()

                                            let homeTeamDetails = homeTeam.TeamsDetails.FirstOrDefault(x => x.SeasonId == league.SeasonId)
                                            let guestTeamDetails = guestTeam.TeamsDetails.FirstOrDefault(x => x.SeasonId == league.SeasonId)

                                              select new GameDto()
                                              {
                                                  GameId = gameCycle.CycleId,
                                                  GameCycleStatus = gameCycle.GameStatus,
                                                  StartDate = gameCycle.StartDate,
                                                  HomeTeamId = homeTeam.TeamId,
                                                  HomeTeamTitle = homeTeamDetails != null ? homeTeamDetails.TeamName : homeTeam.Title,
                                                  HomeTeamScore = gameCycle.HomeTeamScore,
                                                  GuestTeamTitle = guestTeamDetails != null ? guestTeamDetails.TeamName : guestTeam.Title,
                                                  GuestTeamId = guestTeam.TeamId,
                                                  GuestTeamScore = gameCycle.GuestTeamScore,
                                                  Auditorium = gameCycle.Auditorium.Name != null ? gameCycle.Auditorium.Name : string.Empty,
                                                  AuditoriumAddress = gameCycle.Auditorium.Address != null? gameCycle.Auditorium.Address : string.Empty,
                                                  HomeTeamLogo = homeTeam.Logo,
                                                  GuestTeamLogo = guestTeam.Logo,
                                                  CycleNumber = gameCycle.CycleNum,
                                                  LeagueId = stage.LeagueId,
                                                  LeagueName = league.Name,
                                                  IsPublished = gameCycle.IsPublished,
                                                  SeasonId = league.SeasonId,
                                                  UnionId = gameCycle.Stage.League.UnionId,
                                                  HomeTeamDetails = homeTeam.TeamsDetails.Select(x => new TeamDetailsDto()
                                                  {
                                                      TeamId = x.TeamId,
                                                      TeamName = x.TeamName,
                                                      SeasonId = x.SeasonId
                                                  }),
                                                  GuestTeamDetails = guestTeam.TeamsDetails.Select(x => new TeamDetailsDto()
                                                  {
                                                      TeamId = x.TeamId,
                                                      TeamName = x.TeamName,
                                                      SeasonId = x.SeasonId
                                                  }),
                                              });

            return gamesQuery;
        }

        public static List<GameDto> GetTennisGames(DataEntities db, Func<TennisGameCycle, bool> predicate)
        {
            var gamesQuery = db.TennisGameCycles
                .Where(predicate)
                .ToList()
                .Select(gameCycle => new GameDto
                {
                    GameId = gameCycle.CycleId,
                    GameCycleStatus = gameCycle.GameStatus,
                    StartDate = gameCycle.StartDate,
                    HomeTeamId = gameCycle.TeamsPlayer?.UserId ?? gameCycle.FirstPlayerId,
                    GuestTeamId = gameCycle.TeamsPlayer1?.UserId ?? gameCycle.SecondPlayerId,
                    HomeTeamTitle = gameCycle.TeamsPlayer?.User.FullName,
                    HomeTeamTitlePair = gameCycle.TeamsPlayer11 != null ? gameCycle.TeamsPlayer11.User.FullName : "",
                    GuestTeamTitlePair = gameCycle.TeamsPlayer3 != null ? gameCycle.TeamsPlayer3.User.FullName : "",
                    HomeTeamScore = gameCycle.FirstPlayerScore,
                    GuestTeamTitle = gameCycle.TeamsPlayer1?.User.FullName,
                    GuestTeamScore = gameCycle.SecondPlayerScore,
                    Auditorium = gameCycle.Auditorium != null ? gameCycle.Auditorium.Name : string.Empty,
                    AuditoriumAddress = gameCycle.Auditorium != null ? gameCycle.Auditorium.Address : string.Empty,
                    HomeTeamLogo =
                        gameCycle.FirstPlayerId != null
                            ? db.TeamsPlayers.FirstOrDefault(x => x.Id == gameCycle.FirstPlayerId)?.User.Image
                            : string.Empty,
                    GuestTeamLogo = gameCycle.SecondPlayerId != null
                        ? db.TeamsPlayers.FirstOrDefault(x => x.Id == gameCycle.SecondPlayerId)?.User.Image
                        : string.Empty,
                    CycleNumber = gameCycle.CycleNum,
                    IsPublished = gameCycle.IsPublished
                })
                .ToList();

            //foreach (var gvm in gamesQuery)
            //{
            //    if (gvm.HomeTeamLogo == null || gvm.HomeTeamLogo == string.Empty)
            //    {
            //        gvm.HomeTeamLogo =
            //            db.PlayerFiles
            //                .Where(x => x.PlayerId == gvm.HomeTeamId && x.FileType == (int) PlayerFileType.PlayerImage)
            //                .Select(x => x.FileName).FirstOrDefault() ?? null;
            //    }

            //    if (gvm.GuestTeamLogo == null || gvm.GuestTeamLogo == string.Empty)
            //    {
            //        gvm.GuestTeamLogo =
            //            db.PlayerFiles
            //                .Where(x => x.PlayerId == gvm.GuestTeamId && x.FileType == (int) PlayerFileType.PlayerImage)
            //                .Select(x => x.FileName).FirstOrDefault() ?? null;
            //    }
            //}

            return gamesQuery;
        }

        public static GameViewModel CycleToGame(GameDto gameDto, int? seasonId = null)
        {
            var vm = new GameViewModel()
            {
                GameId = gameDto.GameId,
                GameCycleStatus = gameDto.GameCycleStatus,
                StartDate = gameDto.StartDate,
                HomeTeamId = gameDto.HomeTeamId,
                HomeTeamScore = gameDto.HomeTeamScore,
                GuestTeamId = gameDto.GuestTeamId,
                GuestTeamScore = gameDto.GuestTeamScore,
                Auditorium = gameDto.Auditorium,
                AuditoriumAddress = gameDto.AuditoriumAddress,
                HomeTeamLogo = gameDto.HomeTeamLogo,
                GuestTeamLogo = gameDto.GuestTeamLogo,
                CycleNumber = gameDto.CycleNumber,
                LeagueId = gameDto.LeagueId,
                LeagueName = gameDto.LeagueName,
                SeasonId = gameDto.SeasonId,
                UnionId = gameDto.UnionId
            };

            var _repo = new GamesRepo();
            if (_repo.IsBasketBallOrWaterPoloGameCycle(vm.GameId))
            {
                var gc = _repo.GetGameCycleById(vm.GameId);
                vm.HomeTeamScore = gc.GameSets.Sum(x => x.HomeTeamScore);
                vm.GuestTeamScore = gc.GameSets.Sum(x => x.GuestTeamScore);
            }

            if (seasonId.HasValue)
            {
                var homeTeamDetails = gameDto.HomeTeamDetails.FirstOrDefault(x => x.SeasonId == seasonId);
                vm.HomeTeam = homeTeamDetails != null ? homeTeamDetails.TeamName : gameDto.HomeTeamTitle;

                var guestTeamDetails = gameDto.GuestTeamDetails.FirstOrDefault(x => x.SeasonId == seasonId);
                vm.GuestTeam = guestTeamDetails != null ? guestTeamDetails.TeamName : gameDto.GuestTeamTitle;
            }
            else
            {
                vm.HomeTeam = gameDto.HomeTeamTitle;
                vm.GuestTeam = gameDto.GuestTeamTitle;
            }

            return vm;
        }

        public static GameViewModel CycleToTennisGame(GameDto gameDto, int? seasonId = null)
        {
            var vm = new GameViewModel()
            {
                GameId = gameDto.GameId,
                GameCycleStatus = gameDto.GameCycleStatus,
                StartDate = gameDto.StartDate,
                HomeTeamId = gameDto.HomeTeamId,
                HomeTeamScore = gameDto.HomeTeamScore,
                GuestTeamId = gameDto.GuestTeamId,
                GuestTeamScore = gameDto.GuestTeamScore,
                Auditorium = gameDto.Auditorium,
                AuditoriumAddress = gameDto.AuditoriumAddress,
                HomeTeamLogo = gameDto.HomeTeamLogo,
                GuestTeamLogo = gameDto.GuestTeamLogo,
                CycleNumber = gameDto.CycleNumber,
                LeagueId = gameDto.LeagueId,
                LeagueName = gameDto.LeagueName,
                SeasonId = gameDto.SeasonId,
                UnionId = gameDto.UnionId,
                HomeTeam = gameDto.HomeTeamTitle,
                HomeTeamPair = gameDto.HomeTeamTitlePair,
                GuestTeam = gameDto.GuestTeamTitle,
                GuestTeamPair = gameDto.GuestTeamTitlePair
            };
            return vm;
        }
        public static IEnumerable<GameSetViewModel> GetGameSets(int gameId)
        {
            using (var db = new DataEntities())
            {
                return db.GameSets.Where(t => t.GameCycleId == gameId && t.GamesCycle.IsPublished).Select(s =>
                    new GameSetViewModel
                    {
                        GameSetId = s.GameSetId,
                        GameCycleId = s.GameCycleId,
                        HomeTeamScore = s.HomeTeamScore,
                        GuestTeamScore = s.GuestTeamScore,
                        SetNumber = s.SetNumber,
                        IsGoldenSet = s.IsGoldenSet,
                        IsHomeX = s.IsHomeX,
                        IsGeustX = s.IsGuestX
                    }).OrderBy(s => s.SetNumber).ToList();
            }
        }

        public static void UpdateGameSets(List<GamesCycle> gameCycles, string section)
        {
            switch (section)
            {
                case GamesAlias.WaterPolo:
                case GamesAlias.Soccer:
                case GamesAlias.Rugby:
                case GamesAlias.BasketBall:
                case GamesAlias.Softball:
                    foreach (var game in gameCycles)
                    {
                        game.HomeTeamScore = game.GameSets.Sum(x => x.HomeTeamScore);
                        game.GuestTeamScore = game.GameSets.Sum(x => x.GuestTeamScore);

                    }
                    break;
            }
        }

        public static void UpdateGameSet(GameViewModel game, string section)
        {
            switch (section)
            {
                case GamesAlias.WaterPolo:
                case GamesAlias.BasketBall:
                    using (var db = new DataEntities())
                    {
                        var searchedGameSet = db.GameSets.Where(x => x.GameCycleId == game.GameId).ToList();
                        if (searchedGameSet.Count > 0)
                        {
                            game.HomeTeamScore = searchedGameSet.Sum(x => x.HomeTeamScore);
                            game.GuestTeamScore = searchedGameSet.Sum(x => x.GuestTeamScore);
                        }
                        
                    }

                    break;
            }
        }

        public static void UpdateGameSets(IEnumerable<GameViewModel> games)
        {
            using (var db = new DataEntities())
            {
                foreach (var game in games)
                {
                    if (game != null)
                    {
                        var section = db.Leagues.FirstOrDefault(x => x.LeagueId == game.LeagueId)?.Union?.Section?.Alias;
                        if (section == null)
                        {
                            section = db.Leagues.FirstOrDefault(x => x.LeagueId == game.LeagueId)?.Club?.Section?.Alias??"";
                        }
                        switch (section)
                        {
                            case GamesAlias.WaterPolo:
                            case GamesAlias.Soccer:
                            case GamesAlias.Rugby:
                            case GamesAlias.BasketBall:
                            case GamesAlias.Softball:
                                var searchedGameSet = db.GameSets.Where(x => x.GameCycleId == game.GameId).ToList();
                                if (searchedGameSet.Count > 0)
                                {
                                    game.HomeTeamScore = searchedGameSet.Sum(x => x.HomeTeamScore);
                                    game.GuestTeamScore = searchedGameSet.Sum(x => x.GuestTeamScore);
                                }

                                break;
                        }
                    }
                }
            }
        }

        public static IList<GameViewModel> GetPlayerGames(int playerId, int? seasonId)
        {
            using (var db = new DataEntities())
            {
                var now = DateTime.Now;
                var playerGames = (from gameCycle in db.GamesCycles
                                   join homeTeam in db.Teams on gameCycle.HomeTeamId equals homeTeam.TeamId
                                   join stage in db.Stages on gameCycle.StageId equals stage.StageId
                                   join guestTeam in db.Teams on gameCycle.GuestTeamId equals guestTeam.TeamId
                                   join auditorium in db.Auditoriums on gameCycle.AuditoriumId equals auditorium.AuditoriumId into aud
                                   from gameCycleAuditorium in aud.DefaultIfEmpty()
                                   let homeTeamDetails = (from ht in homeTeam.TeamsDetails
                                                          from season in db.Seasons
                                                          where (ht.SeasonId == season.Id && season.Id == seasonId)
                                                          select new
                                                          {
                                                              detail =ht
                                                          }).FirstOrDefault().detail

                                   let guestTeamDetails = (from ht in guestTeam.TeamsDetails
                                                           from season in db.Seasons
                                                           where (ht.SeasonId == season.Id && season.Id == seasonId)
                                                           select new
                                                          {
                                                              detail = ht
                                                          }).FirstOrDefault().detail
                                   //let homeTeamDetails = homeTeam.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)
                                   //let guestTeamDetails = guestTeam.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId)
                                   where gameCycle.IsPublished && (gameCycle.Users.Any(t => t.UserId == playerId))
                                   select new GameViewModel
                                   {
                                       GameId = gameCycle.CycleId,
                                       GameCycleStatus = gameCycle.GameStatus,
                                       StartDate = gameCycle.StartDate,
                                       HomeTeamId = homeTeam.TeamId,
                                       HomeTeam = homeTeamDetails != null ? homeTeamDetails.TeamName : homeTeam.Title,
                                       HomeTeamScore = gameCycle.HomeTeamScore,
                                       GuestTeam = guestTeamDetails != null ? guestTeamDetails.TeamName : guestTeam.Title,
                                       GuestTeamId = guestTeam.TeamId,
                                       GuestTeamScore = gameCycle.GuestTeamScore,
                                       Auditorium = gameCycleAuditorium != null ? gameCycleAuditorium.Name : null,
                                       AuditoriumAddress = gameCycleAuditorium != null ? gameCycleAuditorium.Address : null,
                                       HomeTeamLogo = homeTeam.Logo,
                                       GuestTeamLogo = guestTeam.Logo,
                                       CycleNumber = gameCycle.CycleNum,
                                       LeagueId = stage.LeagueId
                                   }).Take(4).ToList();
                return playerGames;
            }
        }

        public static IList<GameViewModel> GetTennisCompetitionPlayerGames(int playerId, int? seasonId)
        {
            using (var db = new DataEntities())
            {
                var playerGames = db.TennisGameCycles
                    .Where(gameCycle =>
                        gameCycle.IsPublished && (gameCycle.TeamsPlayer.UserId == playerId ||
                                                  gameCycle.TeamsPlayer1.UserId == playerId))
                    .Join(db.TennisStages, gameCycle => gameCycle.StageId, stage => stage.StageId,
                        (gameCycle, stage) => new GameViewModel
                        {
                            GameId = gameCycle.CycleId,
                            GameCycleStatus = gameCycle.GameStatus,
                            StartDate = gameCycle.StartDate,
                            HomeTeamId =
                                gameCycle.TeamsPlayer != null
                                    ? gameCycle.TeamsPlayer.UserId
                                    : gameCycle.FirstPlayerId,
                            HomeTeam = gameCycle.TeamsPlayer.User.FullName,
                            HomeTeamScore = gameCycle.FirstPlayerScore,
                            GuestTeamId =
                                gameCycle.TeamsPlayer1 != null
                                    ? gameCycle.TeamsPlayer1.UserId
                                    : gameCycle.SecondPlayerId,
                            GuestTeam = gameCycle.TeamsPlayer1.User.FullName,
                            GuestTeamScore = gameCycle.SecondPlayerScore,
                            Auditorium =
                                gameCycle.Auditorium != null ? gameCycle.Auditorium.Name : string.Empty,
                            AuditoriumAddress =
                                gameCycle.Auditorium != null ? gameCycle.Auditorium.Address : string.Empty,
                            HomeTeamLogo =
                                gameCycle != null && gameCycle.FirstPlayerId != null
                                    ? db.TeamsPlayers
                                        .FirstOrDefault(x => x.Id == gameCycle.FirstPlayerId)
                                        .User.Image
                                    : string.Empty,
                            GuestTeamLogo =
                                gameCycle != null && gameCycle.SecondPlayerId != null
                                    ? db.TeamsPlayers
                                        .FirstOrDefault(x => x.Id == gameCycle.SecondPlayerId)
                                        .User.Image
                                    : string.Empty,
                            CycleNumber = gameCycle.CycleNum
                        })
                    .ToList();

                /*
                            foreach(var gvm in playerGames)
                            {
                                if (gvm.HomeTeamLogo == null || gvm.HomeTeamLogo == string.Empty)
                                {
                                    gvm.HomeTeamLogo = db.PlayerFiles.Where(x => x.PlayerId == gvm.HomeTeamId && x.FileType == (int)PlayerFileType.PlayerImage).Select(x => x.FileName).FirstOrDefault() ?? null;
                                }
                                if (gvm.GuestTeamLogo == null || gvm.GuestTeamLogo == string.Empty)
                                {
                                    gvm.GuestTeamLogo = db.PlayerFiles.Where(x => x.PlayerId == gvm.GuestTeamId && x.FileType == (int)PlayerFileType.PlayerImage).Select(x => x.FileName).FirstOrDefault() ?? null;
                                }

                            }
                */

                return playerGames;
            }
        }

    }
}