using System.Collections.Generic;
using System.Linq;
using AppModel;
using CmsApp.Models;
using DataService;

namespace CmsApp.Helpers
{
    internal enum TeamType
    {
        HomeTeam,
        GuestTeam
    }

    public class TennisLeagueHelper
    {
        private readonly int cycleId;
        private readonly DataEntities db;
        private GamesRepo gamesRepo;
        private TeamsRepo teamsRepo;
        private PlayersRepo playersRepo;

        public TennisLeagueHelper(DataEntities db, int cycleId)
        {
            this.cycleId = cycleId;
            this.db = db;
            this.gamesRepo = new GamesRepo(db);
            this.playersRepo = new PlayersRepo(db);
            this.teamsRepo = new TeamsRepo(db);
        }

        public GamesCycle GameCycle => gamesRepo.GetGameCycleById(cycleId);
        public League League => GameCycle.Group.Stage.League;

        public TennisLeagueViewModel GetGames(int seasonId)
        {
            var gameCycle = gamesRepo.GetGameCycleById(cycleId);

            return new TennisLeagueViewModel
            {
                GameCycleId = gameCycle.CycleId,
                LeagueId = gameCycle.Group.Stage.LeagueId,
                HomeTeam = GetTeamValue(gameCycle.HomeTeam, seasonId),
                GuestTeam = GetTeamValue(gameCycle.GuestTeam, seasonId),
                GameSettings = GetGameSettings(gameCycle.StageId),
                Games = GetAllGames()
            };
        }

        private List<TennisGameViewModel> GetAllGames()
        {
            var result = new List<TennisGameViewModel>();

            var tennisGames = GameCycle.TennisLeagueGames;
            if (tennisGames.Count > 0)
            {
                foreach (var game in tennisGames)
                {
                    result.Add(new TennisGameViewModel
                    {
                        GameId = game.Id,
                        GameCycleId = GameCycle.CycleId,
                        GameNumber = game.GameNumber,
                        HomeInformation = new TennisPlayerInformation
                        {
                            PlayerId = game.HomePlayerId,
                            IsTechnicalWinner = game.HomePlayerId == game.TechnicalWinnerId,
                            PairPlayerId = game.HomePairPlayerId
                        },
                        GuestInformation = new TennisPlayerInformation
                        {
                            PlayerId = game.GuestPlayerId,
                            IsTechnicalWinner = game.GuestPlayerId == game.TechnicalWinnerId,
                            PairPlayerId = game.GuestPairPlayerId
                        },
                        Sets = game.TennisLeagueGameScores.Select(s => new Set { HomeScore = s.HomeScore, GuestScore = s.GuestScore, IsPairScores = s.IsPairScores, IsTieBreak = s.IsTieBreak })?
                            .ToList(),
                        IsEnded = game.IsEnded
                    });
                }
            }

            return result.OrderBy(t => t.GameNumber)?.ToList();
        }

        private TennisGameSettings GetGameSettings(int stageId)
        {
            var gameSettings = gamesRepo.GetGameSettings(stageId, League.LeagueId);
            return new TennisGameSettings
            {
                BestOfSets = gameSettings?.BestOfSets ?? 3,
                NumberOfGames = gameSettings?.NumberOfGames ?? 3,
                PairsAsLastGame = gameSettings?.PairsAsLastGame ?? false,
                TechWinGuestPoints = gameSettings?.TechWinGuestPoints,
                TechWinHomePoints = gameSettings?.TechWinHomePoints
            };
        }

        private TennisTeam GetTeamValue(Team team, int seasonId)
        {
            var players = playersRepo.GetPlayersForTennisRegistrations(team.TeamId, seasonId, League.EndRegistrationDate)?
                .OrderBy(r => r.TennisPositionOrder ?? int.MaxValue).ThenBy(r => r.User.FullName)?.Select(tp => tp.User)?.ToList();
            var teamName = teamsRepo.GetCurrentTeamName(team.TeamId, seasonId);
            return new TennisTeam
            {
                TeamId = team.TeamId,
                TeamName = teamName,
                RegisteredPlayersList = players,
            };

        }

        public void CreateGame(TennisGameViewModel viewModel)
        {
            var technicalWinner = GetTechnicalWinnerId(viewModel.HomeInformation, viewModel.GuestInformation);
            var gameId = gamesRepo.CreateOrUpdateTennisLeagueGame(cycleId, viewModel.GameNumber, viewModel.HomeInformation.PlayerId, viewModel.GuestInformation.PlayerId, technicalWinner,
                viewModel.HomeInformation.PairPlayerId, viewModel.GuestInformation.PairPlayerId);
            if (gameId.HasValue)
            {
                if (viewModel.Sets.Count > 0)
                {
                    foreach (var set in viewModel.Sets)
                        gamesRepo.AddTennisLeagueGameSetResult(gameId.Value, set.HomeScore, set.GuestScore,
                            viewModel.HomeInformation.PairPlayerId.HasValue && viewModel.GuestInformation.PairPlayerId.HasValue, set.IsTieBreak);
                    gamesRepo.Save();
                }
            }
        }


        private int? GetTechnicalWinnerId(TennisPlayerInformation homeInformation, TennisPlayerInformation guestInformation)
        {
            int? technicalWinnerId = null;

            if (homeInformation.IsTechnicalWinner) technicalWinnerId = homeInformation.PlayerId;
            else if (guestInformation.IsTechnicalWinner) technicalWinnerId = guestInformation.PlayerId;

            return technicalWinnerId;
        }

        public void EndGame(int gameNumber)
        {
            var game = GameCycle.TennisLeagueGames.FirstOrDefault(g => g.GameNumber == gameNumber);
            if (game != null)
            {
                game.IsEnded = true;
            }
            gamesRepo.Save();
        }

        public void ResetTennisGame(int gameNumber)
        {
            var game = GameCycle.TennisLeagueGames.FirstOrDefault(g => g.GameNumber == gameNumber);
            if (game != null)
            {
                game.IsEnded = false;
                game.TechnicalWinnerId = null;
                gamesRepo.RemoveTennisSets(game);
                gamesRepo.Save();
            }
        }

        private void checkAllLeagueGamesExcludedDone(PenaltyForExclusion penaltyForExclusion)
        {
            var prevPenalisedGames = db.GamesCycles.Where(g =>
                (g.GuestTeam.TeamsPlayers.Select(c => c.UserId).Contains(penaltyForExclusion.UserId) ||
                 g.HomeTeam.TeamsPlayers.Select(c => c.UserId).Contains(penaltyForExclusion.UserId)) &&
                 g.AppliedExclusionId.HasValue && g.AppliedExclusionId.Value == penaltyForExclusion.Id && g.GameStatus == GameStatus.Ended).ToList();
            if (prevPenalisedGames.Count() >= penaltyForExclusion.ExclusionNumber)
            {
                penaltyForExclusion.IsEnded = true;
            }
        }
        public void EndAndPublishGames()
        {
            var settings = gamesRepo.GetGameSettings(GameCycle.StageId, League.LeagueId);

            GameCycle.GameStatus = GameStatus.Ended;

            if (GameCycle.AppliedExclusionId.HasValue)
            {
                checkAllLeagueGamesExcludedDone(GameCycle.PenaltyForExclusion);
            }

            if (GameCycle.TennisLeagueGames.Any())
            {
                gamesRepo.CalculateTennisGameScores(settings, GameCycle.TennisLeagueGames.ToList(), out int homeScore, out int guestScore,
                    out int homeSetsWon, out int guestSetsWon, out int homeGaming, out int guestGaming);

                GameCycle.HomeTeamScore = homeScore;
                GameCycle.GuestTeamScore = guestScore;

                if (GameCycle.Group.TypeId == GameTypeId.Playoff || GameCycle.Group.TypeId == GameTypeId.Knockout)
                {
                    var bracket = GameCycle.PlayoffBracket;
                    if (homeScore > guestScore)
                    {
                        GameCycle.PlayoffBracket.WinnerId = bracket.FirstTeam.TeamId;
                        GameCycle.PlayoffBracket.LoserId = bracket.SecondTeam.TeamId;
                    }
                    else if (homeScore < guestScore)
                    {
                        GameCycle.PlayoffBracket.WinnerId = bracket.SecondTeam.TeamId;
                        GameCycle.PlayoffBracket.LoserId = bracket.FirstTeam.TeamId;
                    }
                    else
                    {
                        if (homeSetsWon > guestSetsWon)
                        {
                            GameCycle.PlayoffBracket.WinnerId = bracket.FirstTeam.TeamId;
                            GameCycle.PlayoffBracket.LoserId = bracket.SecondTeam.TeamId;
                        }
                        else if (homeSetsWon < guestSetsWon)
                        {
                            GameCycle.PlayoffBracket.WinnerId = bracket.SecondTeam.TeamId;
                            GameCycle.PlayoffBracket.LoserId = bracket.FirstTeam.TeamId;
                        }
                        else
                        {
                            if (homeGaming > guestGaming)
                            {
                                GameCycle.PlayoffBracket.WinnerId = bracket.FirstTeam.TeamId;
                                GameCycle.PlayoffBracket.LoserId = bracket.SecondTeam.TeamId;
                            }
                            else if (homeGaming < guestGaming)
                            {
                                GameCycle.PlayoffBracket.WinnerId = bracket.SecondTeam.TeamId;
                                GameCycle.PlayoffBracket.LoserId = bracket.FirstTeam.TeamId;
                            }
                        }
                    }

                    MovePlayersToAnotherRound(bracket);
                }
            }
            SaveChanges();
        }

        private void MovePlayersToAnotherRound(PlayoffBracket bracket)
        {
            if (bracket != null)
            {
                foreach (var child in bracket.ChildrenSide1)
                {
                    if (child.Type == (int)PlayoffBracketType.Winner)
                    {
                        child.Team1Id = bracket.WinnerId;
                    }
                    else if (child.Type == (int)PlayoffBracketType.Loseer)
                    {
                        child.Team1Id = bracket.LoserId;
                    }

                    for (int i = 0; i < child.GamesCycles.Count; i++)
                    {
                        GamesCycle game = child.GamesCycles.ElementAt(i);
                        gamesRepo.ResetGame(game, true, false);
                        if (i % 2 == 0)
                        {
                            game.HomeTeamId = child.Team1Id;
                            game.GuestTeamId = child.Team2Id;
                        }
                        else
                        {
                            game.HomeTeamId = child.Team2Id;
                            game.GuestTeamId = child.Team1Id;
                        }
                    }
                }


                foreach (var child in bracket.ChildrenSide2)
                {
                    if (child.Type == (int)PlayoffBracketType.Winner)
                    {
                        child.Team2Id = bracket.WinnerId;
                    }
                    else if (child.Type == (int)PlayoffBracketType.Loseer)
                    {
                        child.Team2Id = bracket.LoserId;
                    }
                    for (int i = 0; i < child.GamesCycles.Count; i++)
                    {
                        GamesCycle game = child.GamesCycles.ElementAt(i);
                        gamesRepo.ResetGame(game, true, false);
                        if (i % 2 == 0)
                        {
                            game.HomeTeamId = child.Team1Id;
                            game.GuestTeamId = child.Team2Id;
                        }
                        else
                        {
                            game.HomeTeamId = child.Team2Id;
                            game.GuestTeamId = child.Team1Id;
                        }
                    }
                }
            }
        }

        public void ResetAllGames()
        {
            var games = GameCycle.TennisLeagueGames?.ToList();

            if (games.Count > 0)
            {
                foreach (var game in games)
                {
                    if (game.TennisLeagueGameScores.Any()) db.TennisLeagueGameScores.RemoveRange(game.TennisLeagueGameScores);
                    db.TennisLeagueGames.Remove(game);
                }
            }
            GameCycle.GameStatus = null;
            GameCycle.HomeTeamScore = 0;
            GameCycle.GuestTeamScore = 0;
            if (GameCycle.PlayoffBracket != null)
            {
                GameCycle.PlayoffBracket.WinnerId = null;
                GameCycle.PlayoffBracket.LoserId = null;
            }

            MovePlayersToAnotherRound(GameCycle.PlayoffBracket);

        }

        public void SaveChanges()
        {
            db.SaveChanges();
        }
    }
}