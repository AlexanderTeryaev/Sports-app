using System;
using AppModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DataService.DTO;

namespace DataService.LeagueRank
{
    public class CategoryRankService : IDisposable
    {
        private int _categoryId;
        DataEntities db = new DataEntities();
        private LeagueRepo leagueRepo;
        private TeamsRepo teamsRepo;
        private SectionsRepo sectionsRepo;
        private List<TennisGame> settings;
        private GamesRepo gamesRepo;
        private readonly UsersRepo usersRepo;
        private readonly UnionsRepo unionsRepo;
        private readonly ClubsRepo clubsRepo;
        private readonly PlayersRepo playersRepo;
        private readonly AgesRepo agesRepo;


        public CategoryRankService(int categoryId)
        {
            _categoryId = categoryId;
            leagueRepo = new LeagueRepo(db);
            teamsRepo = new TeamsRepo(db);
            sectionsRepo = new SectionsRepo(db);
            gamesRepo = new GamesRepo(db);
            usersRepo = new UsersRepo(db);
            unionsRepo = new UnionsRepo(db);
            clubsRepo = new ClubsRepo(db);
            playersRepo = new PlayersRepo(db);
            agesRepo = new AgesRepo(db);
        }

        public RankCategory CreateCategoryRankTable(int? seasonId = null, bool isCategoryStading = false)
        {
            //Get League
            var category = teamsRepo.GetByIdForRanks(_categoryId);
            if (category == null)
            {
                return null;
            }

            //Get game settings
            this.settings = category.TennisGames.ToList();
            if (this.settings == null)
            {
                return null;
            }

            var rCategory = new RankCategory
            {
                CategoryId = _categoryId,
                SeasonId = seasonId.Value,
                Name = category.Title,
                Logo = category.Logo
            };

            var stages = category.TennisStages;
            var index = 0;
            var stagesNotArchive = stages.Where(e => !e.IsArchive).ToList();
            foreach (var stage in stagesNotArchive)
            {
                var rStage = new TennisRankStage { StageId = stage.StageId, Number = stage.Number };

                foreach (var group in stage.TennisGroups.Where(e => !e.IsArchive))
                {
                    if ((group.TypeId == 2 || group.TypeId == 3) && isCategoryStading)
                        continue;

                    var rGroup = CreateGroupRank(group, stagesNotArchive, index, seasonId);
                    rStage.Groups.Add(rGroup);
                    rGroup.GameType = db.GamesTypes.FirstOrDefault(x => x.TypeId == group.TypeId)?.Name;
                    var result = CreateExtendedTable(group, rGroup.Players);

                    rGroup.ExtendedTables.AddRange(result);
                }
                if (rStage.Groups.Any())
                    rCategory.Stages.Add(rStage);
                index++;
            }


            SetPlayerOrderByPosition(rCategory.Stages);

            return rCategory;
        }

        public IEnumerable<TennisPlayoffRank> CreatePlayoffEmptyTable(int seasonId, out bool hasPlayoff)
        {
            var category = teamsRepo.GetByIdForRanks(_categoryId);
            var gamesOfLeague = Enumerable.Empty<TennisPlayoffRank>();

            var categoryStages = category.TennisStages.Where(c => c.IsArchive == false && c.TennisGroups.Any(g => g.TypeId == GameTypeId.Playoff || g.TypeId == GameTypeId.Knockout));
            var isPlayoff = category.TennisStages.Any(c => c.IsArchive == false && c.TennisGroups.All(g => g.TypeId == GameTypeId.Playoff));
            var isKnockout = category.TennisStages.Any(c => c.IsArchive == false && c.TennisGroups.All(g => g.TypeId == GameTypeId.Knockout));
            hasPlayoff = categoryStages.Any();
            if (categoryStages != null && hasPlayoff)
            {
                var allGamesEnded = (categoryStages.LastOrDefault()?.TennisGameCycles.All(gc => gc.GameStatus == GameStatus.Ended)) == true;
                if (allGamesEnded)
                {
                    if (isPlayoff)
                    {
                        gamesOfLeague = UpdatePlayoffTable(categoryStages.LastOrDefault(), seasonId);
                    }
                    else if (isKnockout)
                    {
                        var games = db.TennisGameCycles.Where(t => t.TennisStage.CategoryId == _categoryId);
                        gamesOfLeague = UpdateKnockoutTable(games, seasonId);

                    }
                }
            }
            return gamesOfLeague;
        }


        private IEnumerable<TennisPlayoffRank> UpdatePlayoffTable(TennisStage stage, int seasonId)
        {
            var categoryGames = stage.TennisGameCycles.ToList();

            if (categoryGames == null || !categoryGames.Any()) yield break;

            var category = stage.Team;

            var categoryPlayerIds = categoryGames.Select(lg => lg.FirstPlayerId).Union(categoryGames.Select(lg => lg.SecondPlayerId))
                .Where(c => c.HasValue)
                .Select(c => c.Value)
                .Distinct();

            if (!categoryPlayerIds.Any()) yield break;

            foreach (var playerId in categoryPlayerIds)
            {
                var player = db.TeamsPlayers.Where(x => x.Id == playerId).FirstOrDefault();
                var game = categoryGames.OrderByDescending(c => c.GroupId).Where(c => c.FirstPlayerId == playerId || c.SecondPlayerId == playerId).FirstOrDefault();

                if (player != null)
                {
                    var pairPlayer = GetPaiPlayerForThisGame(game, playerId);
                    var rank = game?.TennisPlayoffBracket.WinnerId == playerId ? game?.MaxPlayoffPos : game?.MinPlayoffPos;
                    var level = category?.CompetitionLevel?.LevelPointsSettings?.Where(t => t.SeasonId == seasonId);
                    var points = pairPlayer != null
                        ? level.FirstOrDefault(t => t.Rank == rank)?.PointsForPairs ?? 0
                        : level.FirstOrDefault(t => t.Rank == rank)?.Points ?? 0;
                    yield return new TennisPlayoffRank
                    {
                        PlayerName = pairPlayer != null ? $"{player.User.FullName} / {pairPlayer.User.FullName}" : player.User.FullName,
                        Rank = rank,
                        Points = points,
                        GamesCount = categoryGames.Count(g => g.FirstPlayerId == playerId || g.SecondPlayerId == playerId),
                        WinsCount = categoryGames.Count(g => g.TennisPlayoffBracket.WinnerId == playerId),
                        FirstPlayerScore = categoryGames.Where(g => g.FirstPlayerId == playerId).Sum(g => g.FirstPlayerScore),
                        SecondPlayerScore = categoryGames.Where(g => g.SecondPlayerId == playerId).Sum(g => g.SecondPlayerScore),
                        FirstPlayerSetsScore = categoryGames.Where(g => g.FirstPlayerId == playerId).Sum(g => g.FirstPlayerScore),
                        SecondPlayerSetsScore = categoryGames.Where(g => g.SecondPlayerId == playerId).Sum(g => g.SecondPlayerScore),
                        HomeSetsMissed = categoryGames.Where(g => g.FirstPlayerId == playerId).Sum(g => g.SecondPlayerScore),
                        GuestSetsMissed = categoryGames.Where(g => g.SecondPlayerId == playerId).Sum(g => g.FirstPlayerScore),
                        GroupName = categoryGames.FirstOrDefault(c => c.FirstPlayerId == playerId || c.SecondPlayerId == playerId)?.TennisGroup.Name,
                        SeasonId = seasonId,
                        CategoryId = stage.CategoryId,
                        PlayerId = player.Id,
                        PairPlayerId = pairPlayer?.Id,
                        PlayerLogo = player.User.Image,
                    };
                }
            }
        }

        private IEnumerable<TennisPlayoffRank> UpdateKnockoutTable(IEnumerable<TennisGameCycle> categoryGames, int seasonId)
        {
            if (categoryGames == null || !categoryGames.Any()) yield break;

            var category = teamsRepo.GetById(_categoryId, seasonId);

            var categoryPlayerIds = categoryGames.Select(lg => lg.FirstPlayerId).Union(categoryGames.Select(lg => lg.SecondPlayerId))
                .Where(c => c.HasValue)
                .Select(c => c.Value)
                .Distinct();

            if (!categoryPlayerIds.Any()) yield break;

            foreach (var playerId in categoryPlayerIds)
            {
                var player = db.TeamsPlayers.Where(x => x.Id == playerId).FirstOrDefault();
                var game = categoryGames.OrderByDescending(c => c.GroupId).Where(c => c.FirstPlayerId == playerId || c.SecondPlayerId == playerId).FirstOrDefault();

                if (player != null)
                {
                    var pairPlayer = GetPaiPlayerForThisGame(game, playerId);
                    var rank = GetCorrectRankForKnockout(game?.TennisPlayoffBracket?.WinnerId == playerId
                        ? game?.MaxPlayoffPos
                        : game?.MinPlayoffPos);
                    var level = category?.CompetitionLevel?.LevelPointsSettings?.Where(t => t.SeasonId == seasonId);
                    var points = pairPlayer != null
                        ? level.FirstOrDefault(t => t.Rank == rank)?.PointsForPairs ?? 0
                        : level.FirstOrDefault(t => t.Rank == rank)?.Points ?? 0;
                    yield return new TennisPlayoffRank
                    {
                        PlayerName = pairPlayer != null ? $"{player.User.FullName} / {pairPlayer.User.FullName}" : player.User.FullName,
                        Rank = rank,
                        Points = points,
                        GamesCount = categoryGames.Count(g => g.FirstPlayerId == playerId || g.SecondPlayerId == playerId),
                        WinsCount = categoryGames.Count(g => g.TennisPlayoffBracket?.WinnerId == playerId),
                        FirstPlayerScore = categoryGames.Where(g => g.FirstPlayerId == playerId).Sum(g => g.FirstPlayerScore),
                        SecondPlayerScore = categoryGames.Where(g => g.SecondPlayerId == playerId).Sum(g => g.SecondPlayerScore),
                        FirstPlayerSetsScore = categoryGames.Where(g => g.FirstPlayerId == playerId).Sum(g => g.FirstPlayerScore),
                        SecondPlayerSetsScore = categoryGames.Where(g => g.SecondPlayerId == playerId).Sum(g => g.SecondPlayerScore),
                        HomeSetsMissed = categoryGames.Where(g => g.FirstPlayerId == playerId).Sum(g => g.SecondPlayerScore),
                        GuestSetsMissed = categoryGames.Where(g => g.SecondPlayerId == playerId).Sum(g => g.FirstPlayerScore),
                        GroupName = categoryGames.FirstOrDefault(c => c.FirstPlayerId == playerId || c.SecondPlayerId == playerId)?.TennisGroup.Name,
                        SeasonId = seasonId,
                        CategoryId = _categoryId,
                        PlayerId = player.Id,
                        PairPlayerId = pairPlayer?.Id,
                        PlayerLogo = player.User.Image,
                    };
                }
            }
        }

        private int? GetCorrectRankForKnockout(int? currentRank)
        {
            switch (currentRank)
            {
                case 1:
                    return 1;
                case 2:
                    return 2;
                case 3:
                case 4:
                    return 3;
                case 5:
                case 6:
                case 7:
                case 8:
                    return 5;
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                    return 9;
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                case 30:
                case 31:
                case 32:
                    return 17;
                default: return null;
            }
        }


        private TeamsPlayer GetPaiPlayerForThisGame(TennisGameCycle game, int playerId)
        {
            if (playerId == game?.FirstPlayerId) return game?.FirstPairPlayer;
            else return game?.SecondPairPlayer;
        }


        //todo: this looks redundant - need to remove it and use position instead of teamPosition
        private void SetPlayerOrderByPosition(List<TennisRankPlayers> players)
        {
            short playerPosition = 1;
            for (var i = 0; i < players.Count; i++)
            {
                players[i].PlayerPosition = playerPosition++;
            }
        }

        private void SetPlayerOrderByPosition(List<TennisRankStage> stages)
        {
            foreach (var rankStage in stages)
            {
                var notAdvancedGroups = rankStage.Groups.Where(x => !x.IsAdvanced);
                foreach (var group in notAdvancedGroups)
                {
                    SetPlayerOrderByPosition(group.Players);
                }
            }
        }

        public RankCategory CreateEmptyRankTable(int? seasonId = null)
        {
            var category = teamsRepo.GetById(this._categoryId);
            if (category == null)
            {
                return null;
            }

            //Get game settings
            this.settings = category.TennisGames.ToList();
            if (this.settings == null)
            {
                return null;
            }
            var rCategory = new RankCategory();
            rCategory.CategoryId = _categoryId;
            rCategory.Name = category.Title;
            rCategory.Logo = category.Logo;
            var stages = category.TennisStages;
            var index = 0;
            var stagesNotArchive = stages.Where(e => e.IsArchive == false).ToList();
            foreach (var stage in stages.Where(e => e.IsArchive == false))
            {
                var rStage = new TennisRankStage();
                rStage.Number = stage.Number;
                foreach (var group in stage.TennisGroups.Where(e => e.IsArchive == false))
                {
                    //LLCMS-191 don't show playoff or knockout type
                    if (group.TypeId == 2 || group.TypeId == 3)
                        continue;

                    var rGroup = CreateGroupRank(group, stagesNotArchive, index, seasonId);

                    SetPlayerOrderByPosition(rGroup.Players);
                    rStage.Groups.Add(rGroup);

                }

                rCategory.Stages.Add(rStage);
                index++;
            }
            return rCategory;
        }

        public List<TennisRankPlayers> GetRankedPlayers(int categoryId, int playerId)
        {
            var resList = new List<TennisRankPlayers>();

            var rCategory = CreateCategoryRankTable();

            if (rCategory != null)
            {
                var stages = rCategory.Stages.OrderByDescending(t => t.Number);
                var stage = stages.Count() > 1 ? stages.ToArray()[1] : null;
                if (stage == null)
                {
                    return null;
                }

                var group = stage.Groups.Where(gr => gr.Players.Any(t => t.Id == playerId)).FirstOrDefault();
                if (group == null)
                {
                    return null;
                }

                var players = group.Players.OrderBy(t => t.Position).ToList();
                for (var i = 0; i < players.Count; i++)
                {
                    if (players[i].Id == playerId)
                    {
                        resList.Add(players[i]);
                    }
                }
            }

            return resList;
        }

        private TennisRankGroup CreateGroupRank(TennisGroup group, List<TennisStage> stages, int index, int? seasonId)
        {
            var rGroup = new TennisRankGroup();
            rGroup.Title = group.Name;
            rGroup.PointsEditType = group.PointEditType;
            var listGroup = db.TennisGroupTeams.Where(x => x.GroupId == group.GroupId).ToList();
            var players = new List<TennisRankPlayers>();
            var categoryLevelId = group?.TennisStage?.Team?.LevelId;
            var categoryPointsSettings = db.LevelPointsSettings.Where(t => t.SeasonId == seasonId && t.LevelId == categoryLevelId)?.ToList();
            foreach (var player in listGroup)
            {
                var points = group.PointEditType == 2 && player.Points != null ? (int)player.Points : 0;
                var res = AddPlayerIfNotExist(player.PlayerId, player.PairPlayerId, players, points, seasonId);
            }
            rGroup.Players = players;
            var rGroupsList = rGroup.Players.ToList();
            GetPoints(rGroupsList, group, stages, index);

            rGroup.Players = @group.TypeId == 3 ? SetPlayersOrderPlayOff(rGroupsList, @group) : SetPlayersOrder(rGroupsList, @group);
            rGroup.IsAdvanced = group.IsAdvanced;
            SetPointsAndRanksForPlayers(rGroup.Players, categoryPointsSettings);
            if (group.TennisPlayoffBrackets.Any())
                rGroup.PlayoffBrackets = group.TennisPlayoffBrackets.OrderBy(x => x.MaxPos).First().MinPos / 2;
            else
                rGroup.PlayoffBrackets = 0;

            return rGroup;
        }

        private void SetPointsAndRanksForPlayers(List<TennisRankPlayers> players, List<LevelPointsSetting> pointsSettings)
        {
            if (players.Any(p => p.Points > 0))
            {
                for (var i = 0; i < players.Count; i++)
                {
                    if (i == 0)
                    {
                        players[i].CategoryPoints = players[i]?.PairPlayerId.HasValue == true
                            ? pointsSettings.FirstOrDefault(p => p.Rank == players[i].PositionNumber)?.PointsForPairs ?? 0
                            : pointsSettings.FirstOrDefault(p => p.Rank == players[i].PositionNumber)?.Points ?? 0;
                    }
                    else
                    {
                        if (players[i].Points == players[i - 1].Points)
                        {
                            players[i].PositionNumber = players[i - 1].PositionNumber;
                            players[i].CategoryPoints = players[i]?.PairPlayerId.HasValue == true
                                ? pointsSettings.FirstOrDefault(p => p.Rank == players[i].PositionNumber)?.PointsForPairs ?? 0
                                : pointsSettings.FirstOrDefault(p => p.Rank == players[i].PositionNumber)?.Points ?? 0;
                        }
                        else
                        {
                            players[i].CategoryPoints = players[i]?.PairPlayerId.HasValue == true
                                ? pointsSettings.FirstOrDefault(p => p.Rank == players[i].PositionNumber)?.PointsForPairs ?? 0
                                : pointsSettings.FirstOrDefault(p => p.Rank == players[i].PositionNumber)?.Points ?? 0;
                        }
                    }
                }
            }
        }

        public void GetPoints(List<TennisRankPlayers> rGroupsList, TennisGroup group, List<TennisStage> stages, int index, bool flag = false, bool sameRecords = false)
        {
            var category = teamsRepo.GetById(_categoryId);
            if (group.PointEditType == 0 || group.PointEditType == 3)
            {
                if ((index - 1) >= 0)
                    foreach (var groupPre in stages[index - 1].TennisGroups)
                    {
                        GetPoints(rGroupsList, groupPre, stages, index - 1, true, group.PointEditType == 3);
                    }
            }
            if (group.TypeId == 3 || group.TypeId == 2)
            {
                if ((index - 1) >= 0 && stages[index - 1].TennisGroups.All(x => x.TypeId == 3 || x.TypeId == 2))
                    foreach (var groupPre in stages[index - 1].TennisGroups)
                    {
                        GetPoints(rGroupsList, groupPre, stages, index - 1, false);
                    }
            }
            var setting = settings.FirstOrDefault(x => x.StageId == @group.StageId) ?? settings.FirstOrDefault();

            if (group.TypeId == 3 || group.TypeId == 2)
            {
                var positions = group.TennisPlayoffBrackets.OrderBy(x => x.MaxPos);
                if (positions.Any())
                {
                    setting.PointsWin = positions.First().MinPos / 2;
                }
                setting.PointsWin = 1;
                setting.PointsTechLoss = 0;
                setting.PointsDraw = 0;
                setting.PointsLoss = 0;
            }

            CalculateTennisPoints(group, flag, rGroupsList, setting, sameRecords);

        }

        private List<TennisExtendedTable> CreateExtendedTable(TennisGroup group, List<TennisRankPlayers> rGroupsList)
        {
            var results = new List<TennisExtendedTable>();
            var alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower().ToCharArray();
            var charIndex = 0;

            foreach (var game in group.TennisGameCycles)
            {

                var homeTeam = rGroupsList.FirstOrDefault(x => x.Id == game.FirstPlayerId);
                var guestTeam = rGroupsList.FirstOrDefault(x => x.Id == game.SecondPlayerId);

                if (guestTeam == null || homeTeam == null)
                    continue;

                SetExtendedTableScoresForFirstPlayer(results, homeTeam, guestTeam, alpha, ref charIndex, game);
                SetExtendedTableScoresForSecondPlayer(results, guestTeam, homeTeam, alpha, ref charIndex, game);
            }

            results = results.OrderBy(x => x.PlayerName).ToList();
            return results;
        }

        private void SetExtendedTableScoresForFirstPlayer(List<TennisExtendedTable> results, TennisRankPlayers firstPlayer, TennisRankPlayers secondPlayer, char[] alpha, ref int charIndex, TennisGameCycle game)
        {
            var firstPlayerForExtended = results.FirstOrDefault(t => t.PlayerId == firstPlayer.Id);
            //scores 
            if (firstPlayerForExtended == null)
            {
                firstPlayerForExtended = new TennisExtendedTable
                {
                    PlayerId = firstPlayer.Id.Value,
                    PlayerName = firstPlayer.Title,
                    Letter = charIndex >= alpha.Length ? alpha[alpha.Length - 1] : alpha[charIndex],
                };

                results.Add(firstPlayerForExtended);

                charIndex++;
            }

            if (!string.IsNullOrEmpty(game.GameStatus) && game.GameStatus.Trim() == GameStatus.Ended)
            {
                firstPlayerForExtended.Scores.Add(new TennisExtendedTableScore
                {
                    OpponentPlayerId = secondPlayer.Id.Value,
                    OpponentScore = game.SecondPlayerScore,
                    PlayerScore = game.FirstPlayerScore
                });
            }
        }

        private void SetExtendedTableScoresForSecondPlayer(List<TennisExtendedTable> results, TennisRankPlayers secondPlayer, TennisRankPlayers firstPlayer, char[] alpha, ref int guestCharIndex, TennisGameCycle game)
        {
            var secondPlayerForExtended = results.FirstOrDefault(t => t.PlayerId == secondPlayer.Id.Value);
            if (secondPlayerForExtended == null)
            {
                secondPlayerForExtended = new TennisExtendedTable
                {
                    PlayerId = secondPlayer.Id.Value,
                    PlayerName = secondPlayer.Title,
                    Letter = alpha[guestCharIndex],
                };

                results.Add(secondPlayerForExtended);

                guestCharIndex++;
            }

            if (!string.IsNullOrEmpty(game.GameStatus) && game.GameStatus.Trim() == GameStatus.Ended)
            {
                secondPlayerForExtended.Scores.Add(new TennisExtendedTableScore
                {
                    OpponentPlayerId = firstPlayer.Id.Value,
                    OpponentScore = game.FirstPlayerScore,
                    PlayerScore = game.SecondPlayerScore
                });
            }

        }

        private void CalculateTennisPoints(TennisGroup @group, bool flag, List<TennisRankPlayers> rGroupsList, TennisGame setting, bool sameRecords)
        {
            foreach (var game in group.TennisGameCycles)
            {
                var firstPlayer = flag ? rGroupsList.FirstOrDefault(x => x.Id == game.FirstPlayerId) : AddPlayerIfNotExist(game.FirstPlayerId, game.FirstPlayerPairId, rGroupsList);
                var secondPlayer = flag ? rGroupsList.FirstOrDefault(x => x.Id == game.SecondPlayerId) : AddPlayerIfNotExist(game.SecondPlayerId, game.FirstPlayerPairId, rGroupsList);

                if (sameRecords)
                {
                    if (firstPlayer == null) firstPlayer = new TennisRankPlayers();
                    if (secondPlayer == null) secondPlayer = new TennisRankPlayers();
                }
                else if (firstPlayer == null || secondPlayer == null)
                    continue;

                if (!string.IsNullOrEmpty(game.GameStatus) && game.GameStatus.Trim() == GameStatus.Ended &&
                    setting != null)
                {

                    firstPlayer.Games++;
                    secondPlayer.Games++;

                    if (game.FirstPlayerScore > game.SecondPlayerScore)
                    {
                        //Home Team wins
                        firstPlayer.Points += setting.PointsWin;
                        secondPlayer.Points += setting.PointsLoss;
                        firstPlayer.Wins++;
                        secondPlayer.Loses++;
                    }
                    else if (game.FirstPlayerScore < game.SecondPlayerScore)
                    {
                        //Guest Team Wins
                        firstPlayer.Points += setting.PointsLoss;
                        secondPlayer.Points += setting.PointsWin;
                        firstPlayer.Loses++;
                        secondPlayer.Wins++;
                    }
                    else
                    {
                        //Drow
                        firstPlayer.Points += setting.PointsDraw;
                        secondPlayer.Points += setting.PointsDraw;

                    }

                    firstPlayer.SetsWon += game.FirstPlayerScore;
                    firstPlayer.SetsLost += game.SecondPlayerScore;
                    secondPlayer.SetsWon += game.FirstPlayerScore;
                    secondPlayer.SetsLost += game.SecondPlayerScore;

                    foreach (var set in game.TennisGameSets)
                    {
                        firstPlayer.TotalPointsScored += set.FirstPlayerScore;
                        secondPlayer.TotalPointsScored += set.SecondPlayerScore;
                        firstPlayer.TotalPointsLost += set.SecondPlayerScore;
                        secondPlayer.TotalPointsLost += set.FirstPlayerScore;
                        firstPlayer.FirstPlayerScore += set.FirstPlayerScore;
                        secondPlayer.SecondPlayerScore += set.SecondPlayerScore;

                        secondPlayer.TotalSecondPlayerPoints += set.SecondPlayerScore;
                        secondPlayer.TotalFirstPlayerPoints += set.FirstPlayerScore;

                        firstPlayer.TotalFirstPlayerPoints += set.FirstPlayerScore;
                        firstPlayer.TotalSecondPlayerPoints += set.SecondPlayerScore;

                        if (set.FirstPlayerScore == set.SecondPlayerScore)
                        {
                            firstPlayer.Draw++;
                            secondPlayer.Draw++;
                        }
                    }
                }
            }
        }

        private List<TennisRankPlayers> SetPlayersOrderPlayOff(List<TennisRankPlayers> players, TennisGroup group)
        {
            IOrderedEnumerable<TennisRankPlayers> ordPlayers = null;


            ordPlayers = players.OrderByDescending(t => t.Points);
            var playerGroups = ordPlayers.GroupBy(t => t.Points);
            var pos = 1;
            var result = new List<TennisRankPlayers>();
            foreach (var tg in playerGroups)
            {
                var groupList = tg.ToList();
                if (groupList.Count() == 1)
                {
                    var t = groupList.ElementAt(0);
                    t.Position = pos.ToString();
                    result.Add(t);
                    pos++;
                }
                else if (groupList.Count() > 1)
                {
                    groupList = SortPlayers(groupList, group);
                    for (var i = 0; i < groupList.Count(); i++)
                    {
                        groupList[i].Position = pos.ToString();
                    }
                    result.AddRange(groupList);
                }
                pos++;
            }
            return result;
        }

        private List<TennisRankPlayers> SetPlayersOrder(List<TennisRankPlayers> players, TennisGroup group)
        {
            var ordTeams = players.OrderByDescending(t => t.Points)
                                                         .ThenByDescending(t => t.SetsRatioNumiric);


            dynamic teamGroups;

            teamGroups = ordTeams.GroupBy(t => new { t.Points, t.SetsRatioNumiric })
                .Select(t => t.ToList()).ToList();

            var result = new List<TennisRankPlayers>();
            var position = 1;

            for (var i = 0; i < teamGroups.Count; i++)
            {
                List<TennisRankPlayers> teamsInGroup = SortPlayers(teamGroups[i], group);

                if (i != 0)
                    position = position + teamGroups[i - 1].Count;
                foreach (var team in teamsInGroup)
                {
                    team.Position = position.ToString();
                    team.PositionNumber = position;
                }

                result.AddRange(teamsInGroup);
            }
            return result;
        }

        public List<TennisRankPlayers> SortPlayers(List<TennisRankPlayers> groupList, TennisGroup group)
        {
            for (var i = 0; i < groupList.Count - 1; i++)
                for (var j = 0; j < groupList.Count - 1; j++)
                    if (GetTheBestOutOfTwoTeams(groupList[j], groupList[j + 1], group) == -1)
                    {
                        var t = groupList[j];
                        groupList[j] = groupList[j + 1];
                        groupList[j + 1] = t;
                    }
            return groupList;
        }

        private int GetTheBestOutOfTwoTeams(TennisRankPlayers player1, TennisRankPlayers player2, TennisGroup group)
        {
            var player1Points = 0;
            var player2Points = 0;

            // Team1 = Guest Team & Team2 = Home Team
            var games = group.TennisGameCycles.Where(gc => gc.SecondPlayerId == player1.Id && gc.FirstPlayerId == player2.Id);
            foreach (var game in games)
            {
                if (game.FirstPlayerScore > game.SecondPlayerScore)
                {
                    player2Points++;
                }
                else if (game.FirstPlayerScore < game.SecondPlayerScore)
                {
                    player1Points++;
                }
            }

            // Team1 = Home Team & Team2 = Guest Team
            games = group.TennisGameCycles.Where(gc => gc.SecondPlayerId == player2.Id && gc.FirstPlayerId == player1.Id);

            foreach (var game in games)
            {
                if (game.FirstPlayerScore > game.SecondPlayerScore)
                {
                    player1Points++;
                }
                else if (game.FirstPlayerScore < game.SecondPlayerScore)
                {
                    player2Points++;
                }
            }

            if (player1Points > player2Points)
            {
                return 1;
            }
            else if (player1Points < player2Points)
            {
                return -1;
            }
            else
            {
                //If the two Teams did not play together or have same score
                return 0;
            }
        }

        public TennisRankPlayers AddPlayerIfNotExist(int? id, int? pairPlayerId, List<TennisRankPlayers> teams, int points = 0, int? seasonId = null)
        {
            var t = teams.FirstOrDefault(tm => tm.Id == id);
            if (t == null)
            {
                var player = db.TeamsPlayers.Where(x => x.Id == id).FirstOrDefault();
                var pairPlayer = db.TeamsPlayers.FirstOrDefault(x => x.Id == pairPlayerId);
                if (player != null)
                {
                    t = new TennisRankPlayers
                    {
                        Id = id,
                        PairPlayerId = pairPlayerId,
                        Points = points,
                        Title = pairPlayer != null ? $"{player.User.FullName} / {pairPlayer.User.FullName}" : player.User.FullName,
                        Games = 0,
                        Wins = 0,
                        Loses = 0,
                        SetsWon = 0,
                        SetsLost = 0,
                        Logo = player.User.Image,
                    };

                    teams.Add(t);
                }
            }
            return t;
        }

        public RankTeam AddTeamIfNotExist(int? id, List<RankTeam> teams, bool isIndividual, int points = 0, int? seasonId = null)
        {
            var t = teams.FirstOrDefault(tm => tm.Id == id);
            if (t == null)
            {

                Team team = null;
                if (isIndividual)
                {
                    var player = id.HasValue ? db.TeamsPlayers.Find(id) : null;
                    team = (player != null) ? new Team() : null;
                    if (team != null)
                    {
                        team.Title = player.User.FullName;
                        team.Logo = player.User.Image;
                    }
                }
                else
                    team = teamsRepo.GetById(id);
                if (team != null)
                {
                    t = new RankTeam
                    {
                        Id = id,
                        Points = points,
                        Title = team.Title,
                        Games = 0,
                        Wins = 0,
                        Loses = 0,
                        SetsWon = 0,
                        SetsLost = 0,
                        Logo = team.Logo
                    };

                    if (seasonId.HasValue && !isIndividual)
                    {
                        var teamDetails = team.TeamsDetails.FirstOrDefault(x => x.SeasonId == seasonId.Value);
                        if (teamDetails != null)
                        {
                            t.Title = teamDetails.TeamName;
                        }
                    }
                    teams.Add(t);
                }
            }
            return t;
        }


        public void Dispose()
        {
            db.Dispose();
        }

        public List<TennisRank> GetTennisRanksForUser(int id, int seasonId)
        {
            /*
            var currentTennisPoints = db.TennisRanks
                .Where(t => t.UserId == id)
                .ToList();

            var competitionPoints = GetRanksAndPointsForUser(id, seasonId);

            if (currentTennisPoints.Any())
            {
                foreach (var competitionPoint in competitionPoints)
                {
                    if (currentTennisPoints.Any(c => c.AgeId == competitionPoint.AgeId))
                    {
                        var point = currentTennisPoints.FirstOrDefault(t => t.AgeId == competitionPoint.AgeId);
                        if (point?.IsUpdated == false)
                        {
                            point.Points = competitionPoint.Points;
                            point.Rank = competitionPoint.Rank;
                            point.IsUpdated = true;
                        }
                    }
                }
            }
            else
            {
                foreach (var competitionPoint in competitionPoints)
                {
                    db.TennisRanks.Add(new TennisRank
                    {
                        AgeId = competitionPoint.AgeId,
                        Points = competitionPoint.Points,
                        Rank = competitionPoint.Rank,
                        UserId = id
                    });
                }
            }

            db.SaveChanges();
            */

            //var previousRanks = db.TennisRanks.Where(t => t.UserId == id).GroupBy(t => t.AgeId).ToList();
            //var newRanks = db.TennisCategoryPlayoffRanks.Where(r => r.SeasonId == seasonId && (r.PlayerId.HasValue || r.PairPlayerId.HasValue)).OrderBy(t => t.LeagueId).ToList();
            return db.TennisRanks.Where(t => t.UserId == id && t.SeasonId == seasonId).ToList();
        }

        private List<TennisRankForm> GetRanksAndPointsForUser(int id, int seasonId)
        {
            var result = new List<TennisRankForm>();

            //var user = usersRepo.GetById(id);
            //var players = user.TeamsPlayers.Where(t => !t.Team.IsArchive).ToList();

            var players = db.TeamsPlayers
                .Where(x => !x.Team.IsArchive && x.UserId == id)
                .Include(x => x.User)
                .Include(x => x.League)
                .Include(x => x.Team)
                .Include(x => x.Team.LeagueTeams)
                .ToList();

            foreach (var player in players)
            {
                var unionId = player.League?.UnionId ?? player?.Team?.LeagueTeams?.FirstOrDefault(t => !t.Leagues.IsArchive)?.Leagues?.UnionId;

                var age = agesRepo.GetCompetitionAge(unionId, player.User.BirthDay, player.User.GenderId);

                var playersInfo = TennisUnionRankDetails(unionId, age?.id, seasonId)?.OrderByDescending(x => x.TotalPoints).ToList();

                var playerInfo = playersInfo?.FirstOrDefault(g => g.UserId == player.UserId);
                if (playerInfo != null)
                {
                    result.Add(new TennisRankForm
                    {
                        AgeId = age?.id,
                        AgeName = age?.age_name,
                        Points = playerInfo.TotalPoints,
                        Rank = playersInfo.IndexOf(playerInfo) + 1
                    });
                }
            }

            return result;
        }

        public List<UnionTennisRankDto> TennisUnionRankDetails(int? unionId, int? ageId, int seasonId)
        {
            var rankList = new List<UnionTennisRankDto>();
            if (ageId.HasValue)
            {
                var fromBirth = db.CompetitionAges.Where(x => x.id == ageId).FirstOrDefault().from_birth;
                var toBirth = db.CompetitionAges.Where(x => x.id == ageId).FirstOrDefault().to_birth;

                var competitionList = leagueRepo.GetByUnion(unionId.Value, seasonId)
                        .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                        .ToList();

                if (competitionList.Any())
                {
                    var players = playersRepo
                        .GetTeamPlayersByUnionIdShort(unionId.Value, seasonId)
                        .Where(x => x.User.BirthDay >= fromBirth && x.User.BirthDay <= toBirth)
                        .GroupBy(x => x.UserId)
                        .ToList();

                    foreach (var player in players)
                    {
                        var item = new UnionTennisRankDto
                        {
                            UserId = player.FirstOrDefault()?.UserId ?? 0,
                            Birthday = player.FirstOrDefault()?.User?.BirthDay,
                            FullName = player.FirstOrDefault()?.User?.FullName,
                            TotalPoints = 0,
                            AveragePoints = 0,
                            PointsToAverage = 0
                        };

                        var club = playersRepo.GetClubOfPlayer(item.UserId);
                        item.TrainingTeam = club == null ? string.Empty : club.Name;
                        item.CompetitionList = new List<UnionTennisCompetitionDto>();
                        var competitionListOfPlayer = competitionList.Where(x => !x.IsArchive && x.LeagueTeams.Any(y => y.Teams.TeamsPlayers.Any(z => z.UserId == item.UserId)));
                        foreach (var competition in competitionListOfPlayer)
                        {
                            var competitionItem = new UnionTennisCompetitionDto
                            {
                                CompetitionId = competition.LeagueId,
                                CompetitionPoints = 0,
                                StartDate = competition.LeagueStartDate,
                                MinParticipationReq = competition.MinParticipationReq ?? 0
                            };
                            item.CompetitionList.Add(competitionItem);
                        }

                        rankList.Add(item);
                    }

                    foreach (var player in players)
                    {
                        foreach (var playerInCategory in player)
                        {
                            var club = clubsRepo.GetByTeamAndSeason(playerInCategory.TeamId, seasonId).FirstOrDefault();
                            //var sectionAlias = "";
                            //if (unionId.HasValue)
                            //{
                            //    var section = unionsRepo.GetSectionByUnionId(unionId.Value);
                            //    sectionAlias = section.Alias;
                            //}
                            //else if (club != null && club.SportSection == null)
                            //{
                            //    var clubSection = club.Union?.Section?.Alias;
                            //    sectionAlias = club.Section?.Alias ?? club.Union?.Section?.Alias;
                            //}
                            //else
                            //{
                            //    sectionAlias = club.SportSection.Alias;
                            //}

                            // Cheng Li
                            // cannot get RankCategory.. so be like at TennisUnionRankDetails method of LeagueRankController.cs
                            var svc = new CategoryRankService(playerInCategory.TeamId);
                            var rCategory = svc.CreateCategoryRankTable(seasonId);

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
                                    rCategory.Players = db.TeamsPlayers.Where(x => x.TeamId == rCategory.CategoryId && x.SeasonId == seasonId).ToList();
                                }
                            }
                            var playersKnockout = new List<int?>();
                            foreach (var stageGroup in rCategory.Stages)
                            {
                                var isEnded = leagueRepo.CheckAllTennisGamesIsEnded(stageGroup.StageId, playerInCategory.TeamId,
                                    stageGroup.Groups.All(g => g.GameType == GameType.Playoff || g.GameType == GameType.Knockout));

                                var hasPlayers = stageGroup.Groups.Any(x => x.Players.Any());
                                if (hasPlayers && isEnded)
                                {
                                    foreach (var group in stageGroup.Groups)
                                    {
                                        foreach (var playerInGroup in group.Players)
                                        {
                                            if (playerInGroup.Id == playerInCategory.Id)
                                            {
                                                if (group.GameType == GameType.Knockout || group.GameType == GameType.Playoff)
                                                {
                                                    if (playersKnockout.Contains(playerInGroup.Id))
                                                    {
                                                        break;
                                                    }
                                                    playersKnockout.Add(playerInGroup.Id);
                                                }
                                                var item = rankList.FirstOrDefault(x => x.UserId == playerInCategory.UserId);

                                                var category = db.Teams.FirstOrDefault(x => x.TeamId == playerInCategory.TeamId);
                                                if (category?.LevelId != null)
                                                {
                                                    var position = int.Parse(playerInGroup.Position);
                                                    var pointSetting = db.LevelPointsSettings.FirstOrDefault(x => x.LevelId == category.LevelId && x.Rank == position);
                                                    if(pointSetting != null)
                                                        item.TotalPoints = item.TotalPoints + pointSetting.Points;

                                                    var league = teamsRepo.GetLeagueByTeamId(category.TeamId);
                                                    var competitionItem = item.CompetitionList.FirstOrDefault(x => x.CompetitionId == league.LeagueId);
                                                    if (competitionItem != null && pointSetting != null)
                                                    {
                                                        competitionItem.CompetitionPoints += pointSetting.Points;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            //TODO: Why we need here playoff table?
                            //ViewBag.PlayoffTable = UpdateTennisPlayoffRank(playerInCategory.TeamId, seasonId);
                        }
                    }
                }

                foreach (var rankItem in rankList)
                {
                    if (rankItem.CompetitionList.Count > 0)
                    {
                        rankItem.CompetitionList = rankItem.CompetitionList.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.CompetitionId).ToList();
                        var minParticipationReq = rankItem.CompetitionList.FirstOrDefault().MinParticipationReq;

                        rankItem.CompetitionList = rankItem.CompetitionList.OrderByDescending(x => x.CompetitionPoints).ToList();
                        var pointsToAverage = 0;
                        if (minParticipationReq > 0)
                        {
                            if (rankItem.CompetitionList.Count < minParticipationReq)
                            {
                                minParticipationReq = rankItem.CompetitionList.Count;
                            }
                            for (var i = 0; i < minParticipationReq; i++)
                            {
                                pointsToAverage += rankItem.CompetitionList[i].CompetitionPoints;
                            }

                            rankItem.PointsToAverage = pointsToAverage;
                            rankItem.AveragePoints = pointsToAverage / minParticipationReq;
                        }
                    }
                }

                rankList = rankList.OrderByDescending(x => x.TotalPoints).ToList();

            }
            return rankList;
        }

    }
}


/*
private List<RankTeam> SetTeamsOrder(List<RankTeam> teams, Group group)
        {
            IOrderedEnumerable<RankTeam> ordTeams = null;

            int[] descriptorsArr = this.settings.SortDescriptors.Split(',').Select(d => Convert.ToInt32(d)).ToArray();
            for (int i = 0; i < descriptorsArr.Length; i++)
            {
                if (i == 0)
                {
                    switch (descriptorsArr[i])
                    {
                        //Points
                        case 0:
                            ordTeams = teams.OrderByDescending(t => t.Points);
                            break;
                        //Wins
                        case 1:
                            ordTeams = teams.OrderByDescending(t => t.Wins);
                            break;
                        //SetDiffs
                        case 2:
                            ordTeams = teams.OrderByDescending(t => t.SetsRatioNumiric);
                            break;
                    }
                }
                else
                {
                    switch (descriptorsArr[i])
                    {
                        //Points
                        case 0:
                            ordTeams = ordTeams.ThenByDescending(t => t.Points);
                            break;
                        //Wins
                        case 1:
                            ordTeams = ordTeams.ThenByDescending(t => t.Wins);
                            break;
                        //SetDiffs
                        case 2:
                            ordTeams = ordTeams.ThenByDescending(t => t.SetsRatioNumiric);
                            break;
                    }
                }
            }

            var teamGroups = ordTeams.GroupBy(t => new { t.Points, t.Wins, t.SetsRatioNumiric });
            int pos = 1;
            List<RankTeam> result = new List<RankTeam>();

            foreach (var tg in teamGroups)
            {

                List<RankTeam> groupList = tg.ToList();
                if (groupList.Count() == 1)
                {
                    var t = groupList.ElementAt(0);
                    t.Position = pos;
                    result.Add(t);
                    pos++;
                }
                else if (groupList.Count() == 2)
                {
                    groupList = GetTheBestOutOfTwoTeams(groupList.ElementAt(0), groupList.ElementAt(1), group);
                    foreach (var t in groupList)
                    {
                        t.Position = pos;
                        pos++;
                    }
                    result.AddRange(groupList);
                }
                else if (groupList.Count() > 2)
                {
                    
                    groupList = groupList.OrderByDescending(t => t.TotalPointsDiffs).ToList();
                    foreach (var t in groupList)
                    {
                        t.Position = pos;
                        pos++;
                    }
                    result.AddRange(groupList);
                }
                 
            }
            return result;
        }

*/
