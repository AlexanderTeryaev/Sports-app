using System;
using AppModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DataService.DTO;

namespace DataService.LeagueRank
{
    public class LeagueRankService : IDisposable
    {
        private int _leagueId;
        DataEntities db = new DataEntities();
        private LeagueRepo leagueRepo;
        private TeamsRepo teamsRepo;
        private SectionsRepo sectionsRepo;
        private List<Game> settings;
        private GamesRepo gamesRepo;

        public LeagueRankService(int leagueId)
        {
            _leagueId = leagueId;
            leagueRepo = new LeagueRepo(db);
            teamsRepo = new TeamsRepo(db);
            sectionsRepo = new SectionsRepo(db);
            gamesRepo = new GamesRepo(db);
        }

        private string SectionAlias
        {
            get
            {
                return leagueRepo.GetGameAliasByLeagueId(_leagueId);
            }
        }

        public LeagueRankedStanding GetRankedStanding(int seasonId)
        {
            var result = new LeagueRankedStanding
            {
                LeagueId = _leagueId
            };

            var league = db.Leagues
                .Include(x => x.PositionSettings)
                .FirstOrDefault(x => x.LeagueId == _leagueId);

            if (league != null)
            {
                var scoringSettings = league.PositionSettings.Where(x => x.SeasonId == seasonId).ToList();

                result.Teams = new List<LeagueTeamRankedStanding>();
                
                var leagueRank = CreateLeagueRankTable(seasonId);

                var teamsIds = leagueRank
                    .Stages
                    .Where(x => x.StageGamesCompleted && x.RankedStandingEnabled)
                    .SelectMany(x => x.Groups)
                    .SelectMany(x => x.Teams.Select(t => t.Id ?? 0))
                    .Where(x => x != 0)
                    .ToArray();

                var standingCorrections = db.RankedStandingCorrections
                    .Where(x => x.LeagueId == _leagueId && teamsIds.Contains(x.TeamId))
                    .ToList();

                if (leagueRank.Stages?.Any() == true)
                {
                    foreach (var stage in leagueRank.Stages.Where(x => x.StageGamesCompleted && x.RankedStandingEnabled))
                    {
                        if (stage.Groups?.Any() == true)
                        {
                            foreach (var stageGroup in stage.Groups)
                            {
                                if (stageGroup.Teams?.Any() == true)
                                {
                                    var teamPositionInGroup = 1;
                                    foreach (var team in stageGroup.Teams.OrderByDescending(x => x.Points))
                                    {
                                        var scoring = scoringSettings.FirstOrDefault(x => x.Position == teamPositionInGroup);
                                        var correctionValue = standingCorrections.FirstOrDefault(x => x.TeamId == team.Id)?.Value ?? 0;

                                        var existingTeam = result.Teams.FirstOrDefault(x => x.Id == team.Id);
                                        if (existingTeam == null)
                                        {
                                            result.Teams.Add(new LeagueTeamRankedStanding
                                            {
                                                Id = team.Id ?? 0,
                                                Name = team.Title,
                                                Points = scoring?.Points ?? 0,
                                                Correction = correctionValue
                                            });
                                        }
                                        else
                                        {
                                            existingTeam.Points += scoring?.Points ?? 0;
                                        }

                                        teamPositionInGroup++;
                                    }
                                }
                            }
                        }
                    }
                    
                    //apply correction to final points
                    foreach (var team in result.Teams)
                    {
                        team.Points += team.Correction;
                    }
                }
            }

            return result;
        }

        public RankLeague CreateLeagueRankTable(int? seasonId = null, bool isTennisLeague = false)
        {
            //Get League
            var league = leagueRepo.GetByIdForRanks(_leagueId);
            if (league == null)
            {
                return null;
            }

            //Get game settings
            this.settings = league.Games.ToList();
            if (this.settings == null)
            {
                return null;
            }

            var rLeague = new RankLeague
            {
                UnionId = league.UnionId,
                LeagueId = league.LeagueId,
                SeasonId = league.SeasonId ?? 0,
                AboutLeague = league.AboutLeague,
                LeagueStructure = league.LeagueStructure,
                Name = league.Name,
                Logo = league.Logo,
                IsTennisLeague = isTennisLeague
            };

            var stages = league.Stages;
            var index = 0;
            var stagesNotArchive = stages.Where(x => !x.IsArchive && !x.IsCrossesStage).ToList();
            foreach (var stage in stagesNotArchive)
            {
                var rStage = new RankStage
                {
                    StageId = stage.StageId,
                    Number = stage.Number,
                    StageGamesCompleted = stage.GamesCycles.Any() && stage.GamesCycles.All(x => x.GameStatus == "ended"),
                    RankedStandingEnabled = stage.RankedStandingsEnabled,
                    CustomStageName = stage.Name
                };

                rLeague.TeamPenalties.AddRange(stage.TeamPenalties.ToList());

                foreach (var group in stage.Groups.Where(e => !e.IsArchive))
                {
                    //LLCMS-191 don't show playoff or knockout type
                    if (!group.GamesType.Name.Equals(GameType.Division))
                        continue;

                    RankGroup rGroup = CreateGroupRank(group, stagesNotArchive, index, seasonId, isTennisLeague);
                    rStage.Groups.Add(rGroup);
                    rGroup.GameType = group.GamesType.Name;
                    var result = CreateExtendedTable(group, rGroup.Teams);

                    rGroup.ExtendedTables.AddRange(result);
                }
                if (rStage.Groups.Any())
                    rLeague.Stages.Add(rStage);
                index++;
            }
            //  Update extended tables for all groups having "With their records" Point edit type
            //  in Netball (Catchball) leagues
            if (SectionAlias == GamesAlias.NetBall)
            {
                foreach (var stage in rLeague.Stages.OrderByDescending(s => s.Number))
                {
                    if (stage.Groups.Any(g => g.PointsEditType == PointEditType.WithTheirRecords))
                    {
                        var extTables = rLeague.Stages.Where(s => s.Number < stage.Number)
                            .SelectMany(s => s.Groups, (s, g) => g)
                            .SelectMany(g => g.ExtendedTables, (g, et) => et);
                        foreach (var group in stage.Groups.Where(gr => gr.PointsEditType == PointEditType.WithTheirRecords))
                        {
                            foreach (var extTab in group.ExtendedTables)
                            {
                                var prevStagesScores = extTables.Where(et => et.TeamId == extTab.TeamId)
                                        .SelectMany(et => et.Scores, (et, s) => s)
                                        .Where(s => group.Teams.Any(gt => gt.Id == s.OpponentTeamId)).ToList();
                                prevStagesScores.AddRange(extTab.Scores);
                                extTab.Scores = prevStagesScores;
                            }
                        }
                    }
                }
            }

            SetTeamOrderByPosition(rLeague.Stages);

            return rLeague;
        }

        public IEnumerable<PlayoffRank> CreatePlayoffEmptyTable(int seasonId, out bool hasPlayoff, bool isTennisLeague = false)
        {
            var league = leagueRepo.GetByIdForRanks(_leagueId);
            var allGamesEnded = true;
            var gamesOfLeague = Enumerable.Empty<PlayoffRank>();

            var leagueStages = league.Stages.Where(c => c.IsArchive == false && c.Groups.All(g => g.GamesType.Name.Equals(GameType.Playoff)));
            hasPlayoff = leagueStages.Any();

            if (leagueStages != null && leagueStages.Any())
            {
                foreach (var stage in leagueStages)
                {
                    if (stage.GamesCycles.Any(gc => gc.GameStatus != GameStatus.Ended)) allGamesEnded = false;
                }
                if (allGamesEnded)
                {
                    gamesOfLeague = UpdatePlayoffTable(leagueStages.LastOrDefault(), seasonId, isTennisLeague);
                }
            }
            return gamesOfLeague;
        }

        private IEnumerable<PlayoffRank> UpdatePlayoffTable(Stage stage, int seasonId, bool isTennisLeague = false)
        {
            var leagueGames = gamesRepo.GetCyclesByLeague(stage.LeagueId, true)?.Where(c => c.GameType == 2);

            if (leagueGames == null || !leagueGames.Any()) yield break;

            var league = stage.League;
            var needScore = string.Equals(SectionAlias, GamesAlias.BasketBall, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(SectionAlias, GamesAlias.WaterPolo, StringComparison.OrdinalIgnoreCase);

            var leagueTeamsIds = leagueGames.Select(lg => lg.HomeTeamId).Union(leagueGames.Select(lg => lg.GuestTeamId))
                .Where(c => c.HasValue)
                .Select(c => c.Value)
                .Distinct();

            if (!leagueTeamsIds.Any()) yield break;

            foreach (var leagueTeamId in leagueTeamsIds)
            {
                var team = teamsRepo.GetById(leagueTeamId);
                var game = leagueGames.OrderByDescending(c => c.GroupId).Where(c => c.HomeTeamId == leagueTeamId || c.GuestTeamId == leagueTeamId).FirstOrDefault();
                if (team != null)
                {
                    if (isTennisLeague)
                    {
                        yield return new PlayoffRank
                        {
                            TeamName = team.TeamsDetails.OrderByDescending(c => c.Id).FirstOrDefault(c => c.SeasonId == seasonId)?.TeamName ?? team.Title,
                            Rank = game?.WinnerId == leagueTeamId ? game?.MaxPlayoffPos : game?.MinPlayoffPos,
                            GamesCount = leagueGames.Count(g => g.HomeTeamId == leagueTeamId || g.GuestTeamId == leagueTeamId),
                            WinsCount = leagueGames.Count(g => g.WinnerId == leagueTeamId),
                            HomeTeamScore = needScore ? leagueGames.Where(g => g.HomeTeamId == leagueTeamId).Sum(g => g.BasketBallWaterpoloHomeTeamScore) : 0,
                            GuestTeamsScore = needScore ? leagueGames.Where(g => g.GuestTeamId == leagueTeamId).Sum(g => g.BasketBallWaterpoloGuestTeamScore) : 0,
                            HomeMissed = needScore ? leagueGames.Where(g => g.HomeTeamId == leagueTeamId).Sum(g => g.BasketBallWaterpoloGuestTeamScore) : 0,
                            GuestMissed = needScore ? leagueGames.Where(g => g.GuestTeamId == leagueTeamId).Sum(g => g.BasketBallWaterpoloHomeTeamScore) : 0,
                            HomeSetsScore = !needScore ? leagueGames.Where(g => g.HomeTeamId == leagueTeamId).Sum(g => g.HomeTeamScore) : 0,
                            GuestSetsScore = !needScore ? leagueGames.Where(g => g.GuestTeamId == leagueTeamId).Sum(g => g.GuestTeamScore) : 0,
                            HomeSetsMissed = !needScore ? leagueGames.Where(g => g.HomeTeamId == leagueTeamId).Sum(g => g.GuestTeamScore) : 0,
                            GuestSetsMissed = !needScore ? leagueGames.Where(g => g.GuestTeamId == leagueTeamId).Sum(g => g.HomeTeamScore) : 0,
                            GroupName = leagueGames.FirstOrDefault(c => c.HomeTeamId == leagueTeamId || c.GuestTeamId == leagueTeamId)?.GroupName,
                            SeasonId = seasonId,
                            LeagueId = league?.LeagueId,
                            TeamId = team.TeamId,
                            TeamLogo = team.Logo
                        };
                    }
                    else
                    {
                        Game setting = gamesRepo.GetGameSettings(game.StageId, league.LeagueId)
                            ?? settings?.FirstOrDefault(r => r.StageId == game.StageId) ?? settings?.FirstOrDefault()
                            ?? league?.Games?.FirstOrDefault()
                            ?? null;
                        var tennisLeagueHomeGames = leagueGames.Where(g => g.HomeTeamId == team.TeamId);
                        var tennisLeagueGuestGames = leagueGames.Where(g => g.GuestTeamId == team.TeamId);

                        var tennisTeamInfo = new TennisGameInfo();

                        GetLeagueInfoForPlayoff(tennisTeamInfo, tennisLeagueHomeGames, setting, team.TeamId, "Home");
                        GetLeagueInfoForPlayoff(tennisTeamInfo, tennisLeagueGuestGames, setting, team.TeamId, "Guest");

                        yield return new PlayoffRank
                        {
                            TeamName = team.TeamsDetails.OrderByDescending(c => c.Id).FirstOrDefault(c => c.SeasonId == seasonId)?.TeamName ?? team.Title,
                            GroupName = leagueGames.FirstOrDefault(c => c.HomeTeamId == leagueTeamId || c.GuestTeamId == leagueTeamId)?.GroupName,
                            SeasonId = seasonId,
                            LeagueId = league?.LeagueId,
                            TeamId = team.TeamId,
                            TeamLogo = team.Logo,
                            TennisInfo = tennisTeamInfo,
                            Rank = game?.WinnerId == team.TeamId ? game?.MaxPlayoffPos : game?.MinPlayoffPos,
                        };
                    }
                }
            }
        }

        private void GetLeagueInfoForPlayoff(TennisGameInfo tennisInfo, IEnumerable<GamesCycleDto> games, Game setting, int teamId,
            string type)
        {
            foreach (var tennisGame in games)
            {
                var gameCycle = db.GamesCycles.Find(tennisGame.CycleId);
                var tennisSets = gameCycle?.TennisLeagueGames?.ToList();
                gamesRepo.CalculateTennisGameScores(setting, tennisSets, out int homeScore, out int guestScore,
                    out int homeSetsWon, out int guestSetsWon, out int homeGaming, out int guestGaming);
                gamesRepo.DetermineTheTennisLeagueGameWinner(gameCycle, homeScore, guestScore, homeSetsWon, guestSetsWon,
                    homeGaming, guestGaming, false, out int? gameWinnerId, out int? gameLoserId);
                if (type.Equals("Home"))
                {
                    SetTennisGameValues(tennisInfo, homeScore, homeSetsWon, guestSetsWon, homeGaming, guestGaming,
                       gameWinnerId.HasValue && gameWinnerId.Value == teamId,
                       gameLoserId.HasValue && gameLoserId.Value == teamId);
                }
                else
                {
                    SetTennisGameValues(tennisInfo, guestScore, guestSetsWon, homeSetsWon, guestGaming, homeGaming,
                       gameWinnerId.HasValue && gameWinnerId.Value == teamId,
                       gameLoserId.HasValue && gameLoserId.Value == teamId);
                }

            }
        }


        //todo: this looks redundant - need to remove it and use position instead of teamPosition
        private void SetTeamOrderByPosition(List<RankTeam> teams)
        {
            short teamPosition = 1;
            for (int i = 0; i < teams.Count; i++)
            {
                teams[i].TeamPosition = teamPosition++;
            }
        }

        private void SetTeamOrderByPosition(List<RankStage> stages)
        {
            foreach (var rankStage in stages)
            {
                var notAdvancedGroups = rankStage.Groups.Where(x => !x.IsAdvanced);
                foreach (var group in notAdvancedGroups)
                {
                    SetTeamOrderByPosition(group.Teams);
                }
            }
        }

        public RankLeague CreateEmptyRankTable(int? seasonId = null)
        {
            var league = leagueRepo.GetByIdExtended(this._leagueId);
            if (league == null)
            {
                return null;
            }

            //Get game settings
            this.settings = league.Games.ToList();
            if (this.settings == null)
            {
                return null;
            }

            var rLeague = new RankLeague
            {
                LeagueId = league.LeagueId,
                Name = league.Name,
                Logo = league.Logo,
                AboutLeague = league.AboutLeague,
                LeagueStructure = league.LeagueStructure
            };
            var stages = league.Stages;
            var index = 0;
            var stagesNotArchive = stages.Where(e => e.IsArchive == false).ToList();
            foreach (var stage in stages.Where(e => e.IsArchive == false))
            {
                var rStage = new RankStage {Number = stage.Number, CustomStageName = stage.Name};
                foreach (var group in stage.Groups.Where(e => e.IsArchive == false))
                {
                    //LLCMS-191 don't show playoff or knockout type
                    if (group.GamesType.Name.Equals(GameType.Knockout) || group.GamesType.Name.Equals(GameType.Playoff))
                        continue;

                    RankGroup rGroup = CreateGroupRank(group, stagesNotArchive, index, seasonId);

                    SetTeamOrderByPosition(rGroup.Teams);
                    rStage.Groups.Add(rGroup);
                }

                rLeague.Stages.Add(rStage);
                index++;
            }
            return rLeague;
        }

        public List<RankTeam> GetRankedTeams(int leagueId, int teamId)
        {
            var resList = new List<RankTeam>();

            RankLeague rLeague = CreateLeagueRankTable();

            if (rLeague != null)
            {
                var stages = rLeague.Stages.OrderByDescending(t => t.Number);
                var stage = stages.Count() > 1 ? stages.ToArray()[1] : null;
                if (stage == null)
                {
                    return null;
                }

                var group = stage.Groups.Where(gr => gr.Teams.Any(t => t.Id == teamId)).FirstOrDefault();
                if (group == null)
                {
                    return null;
                }

                var teams = group.Teams.OrderBy(t => t.Position).ToList();
                for (int i = 0; i < teams.Count; i++)
                {
                    if (teams[i].Id == teamId)
                    {
                        resList.Add(teams[i]);
                    }
                }
            }

            return resList;
        }

        private int GetTeamIdOfWinnerByGameCycle(GamesCycle game, string sectionAlias)
        {
            if (!string.Equals(sectionAlias, GamesAlias.VolleyBall, StringComparison.OrdinalIgnoreCase) && !string.Equals(sectionAlias, GamesAlias.NetBall, StringComparison.OrdinalIgnoreCase) && !string.Equals(sectionAlias, GamesAlias.Tennis, StringComparison.OrdinalIgnoreCase))
            {
                var t1score = game.GameSets.Sum(c => c.HomeTeamScore);
                var t2score = game.GameSets.Sum(c => c.GuestTeamScore);
                if (t1score > t2score)
                {
                    return game.HomeTeamId ?? 0;
                }
                if (t1score < t2score)
                {
                    return game.GuestTeamId ?? 0;
                }
            }
            else
            {
                if (game.HomeTeamScore > game.GuestTeamScore)
                {
                    return game.HomeTeamId ?? 0;
                }
                if (game.HomeTeamScore < game.GuestTeamScore)
                {
                    return game.GuestTeamId ?? 0;
                }
            }
            return 0;
        }

        private RankGroup CreateGroupRank(Group group, List<Stage> stages, int index, int? seasonId, bool isTennisLeague = false)
        {
            RankGroup rGroup = new RankGroup();
            rGroup.Title = group.Name;
            rGroup.GroupId = group.GroupId;
            rGroup.PointsEditType = group.PointEditType;
            var listGroup = db.GroupsTeams.Include("Team").Include("Team.TeamsDetails").Where(x => x.GroupId == group.GroupId).ToList();
            var teams = new List<RankTeam>();
            foreach (var team in listGroup)
            {
                if (seasonId.HasValue)
                {
                    var details = team.Team?.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId.Value);
                    if (details != null)
                    {
                        team.Team.Title = details.TeamName;
                    }
                }
                var points = group.PointEditType == 2 && team.Points != null ? (int)team.Points : 0;
                var res = AddTeamIfNotExist(team.TeamId, teams, points, seasonId, isTennisLeague);
            }
            rGroup.Teams = teams;
            var rGroupsList = rGroup.Teams.ToList();
            var stageGroups = group.Stage.Groups.ToList();
            if (group.PointEditType == PointEditType.WithTheirRecords && stageGroups.Count >= 2 && stageGroups[0].TypeId == 1 && stageGroups[1].TypeId != 1)
            {
                GetPointsWithTheirRecords(rGroupsList, stages.Take(index + 1).ToList());
            }
            else
            {
                GetPoints(rGroupsList, group, stages, index, false, false, isTennisLeague);
            }

            rGroup.Teams = @group.TypeId == 3 ? SetTeamsOrderPlayOff(rGroupsList, @group) : SetTeamsOrder(rGroupsList, @group);
            if (isTennisLeague)
            {
                rGroup.Teams = rGroup.Teams.OrderByDescending(g => g.TennisInfo.Points).ThenByDescending(g => g.TennisInfo.PlayersSetsWon)
                    .ThenByDescending(g => g.TennisInfo.PlayerGamingDifference)?.ToList();
                
                // be sure that we cover the case when teams have same amount of points but had a game with each other
                rGroup.Teams = SetTeamsOrderTennis(rGroup.Teams, @group);
            }
            rGroup.IsAdvanced = group.IsAdvanced;

            if (group.PlayoffBrackets.Any())
                rGroup.PlayoffBrackets = group.PlayoffBrackets.OrderBy(x => x.MaxPos).First().MinPos / 2;
            else
                rGroup.PlayoffBrackets = 0;

            return rGroup;
        }

        //List<Stage> stages
        private void CalculatePointsForWaterPolo(Group @group, Game setting, bool flag, List<RankTeam> rGroupsList, bool sameRecords, bool withTheirRecords = false, bool isSoftBall = false)
        {
            var games = new List<GamesCycle>();
            games = withTheirRecords
                ? group.GamesCycles.Where(g => !string.IsNullOrEmpty(g.GameStatus) && g.GameStatus.Trim() == GameStatus.Ended && setting != null
                    && rGroupsList.Any(t => t.Id == g.HomeTeamId) && rGroupsList.Any(t => t.Id == g.GuestTeamId))?.ToList()
                : group.GamesCycles.Where(g => !string.IsNullOrEmpty(g.GameStatus) && g.GameStatus.Trim() == GameStatus.Ended && setting != null)?.ToList();

            foreach (var game in group.GamesCycles.Where(g => !string.IsNullOrEmpty(g.GameStatus) && g.GameStatus.Trim() == GameStatus.Ended && setting != null))
            {

                RankTeam homeTeam = flag ? rGroupsList.FirstOrDefault(x => x.Id == game.HomeTeamId) : AddTeamIfNotExist(game.HomeTeamId, rGroupsList);
                RankTeam guestTeam = flag ? rGroupsList.FirstOrDefault(x => x.Id == game.GuestTeamId) : AddTeamIfNotExist(game.GuestTeamId, rGroupsList);

                if (sameRecords)
                {
                    if (homeTeam == null) homeTeam = new RankTeam();
                    if (guestTeam == null) guestTeam = new RankTeam();
                }
                else if (homeTeam == null || guestTeam == null)
                    continue;

                var homeTeamTotalScore = game.GameSets.Where(x => !x.IsPenalties).Sum(x => x.HomeTeamScore);
                var guestTeamTotalScore = game.GameSets.Where(x => !x.IsPenalties).Sum(x => x.GuestTeamScore);


                if(isSoftBall)
                {
                    var homeDIP = game.GameSets.Where(x => !x.IsPenalties).Count(x => !x.IsGuestX);
                    var guestDIP = game.GameSets.Where(x => !x.IsPenalties).Count(x => !x.IsHomeX);
                    homeTeam.DIP = homeTeam.DIP + homeDIP;
                    guestTeam.DIP = guestTeam.DIP + guestDIP;
                }

                guestTeam.HomeTeamFinalScore += game.GameSets.Where(x => !x.IsPenalties).Sum(x => x.GuestTeamScore);
                guestTeam.GuesTeamFinalScore += game.GameSets.Where(x => !x.IsPenalties).Sum(x => x.HomeTeamScore);

                homeTeam.HomeTeamFinalScore += game.GameSets.Where(x => !x.IsPenalties).Sum(x => x.HomeTeamScore);
                homeTeam.GuesTeamFinalScore += game.GameSets.Where(x => !x.IsPenalties).Sum(x => x.GuestTeamScore);


                homeTeam.SetsWon += game.GameSets.Sum(x => x.HomeTeamScore);
                homeTeam.SetsLost += game.GameSets.Sum(x => x.GuestTeamScore);

                guestTeam.SetsWon += game.GameSets.Sum(x => x.GuestTeamScore);
                guestTeam.SetsLost += game.GameSets.Sum(x => x.HomeTeamScore);

                homeTeam.Games++;
                guestTeam.Games++;

                //Technical Win/Lost
                if (game.TechnicalWinnnerId.HasValue)
                {
                    if (game.HomeTeamId == game.TechnicalWinnnerId.Value)
                    {
                        homeTeam.Points += setting.PointsTechWin;
                        guestTeam.Points += setting.PointsTechLoss;
                        homeTeam.Wins++;
                        guestTeam.Loses++;
                    }
                    else
                    {
                        guestTeam.Points += setting.PointsTechWin;
                        homeTeam.Points += setting.PointsTechLoss;
                        homeTeam.Loses++;
                        guestTeam.Wins++;
                    }
                }
                else
                {
                    //Normal Win/Lost
                    var penalty = game.GameSets.FirstOrDefault(x => x.IsPenalties);
                    if (penalty == null)
                    {
                        if (homeTeamTotalScore > guestTeamTotalScore)
                        {
                            //Home Team wins
                            homeTeam.Wins++;
                            guestTeam.Loses++;

                            homeTeam.Points += setting.PointsWin;
                            guestTeam.Points += setting.PointsLoss;
                        }
                        else if (homeTeamTotalScore < guestTeamTotalScore)
                        {
                            //Guest Team Wins
                            homeTeam.Loses++;
                            guestTeam.Wins++;

                            homeTeam.Points += setting.PointsLoss;
                            guestTeam.Points += setting.PointsWin;
                        }
                        else
                        {
                            //Draw
                            homeTeam.Draw++;
                            guestTeam.Draw++;
                            homeTeam.Points += setting.PointsDraw;
                            guestTeam.Points += setting.PointsDraw;
                        }
                    }
                    else
                    {
                        if (homeTeamTotalScore > guestTeamTotalScore)
                        {
                            homeTeam.Wins++;
                            guestTeam.Loses++;

                            homeTeam.Points += setting.PointsWin;
                            guestTeam.Points += setting.PointsLoss;
                        }
                        else if (homeTeamTotalScore < guestTeamTotalScore)
                        {
                            //Guest Team Wins
                            homeTeam.Loses++;
                            guestTeam.Wins++;

                            homeTeam.Points += setting.PointsLoss;
                            guestTeam.Points += setting.PointsWin;
                        }
                        else
                        {
                            if (penalty.HomeTeamScore > penalty.GuestTeamScore)
                            {
                                homeTeam.Wins++;
                                guestTeam.Loses++;

                                homeTeam.Points += setting.PointsWin;
                                guestTeam.Points += setting.PointsLoss;
                            }
                            else if (penalty.HomeTeamScore < penalty.GuestTeamScore)
                            {
                                //Guest Team Wins
                                homeTeam.Loses++;
                                guestTeam.Wins++;

                                homeTeam.Points += setting.PointsLoss;
                                guestTeam.Points += setting.PointsWin;
                            }
                        }
                    }
                }
            }

            var stagePenalties = group.Stage.TeamPenalties.ToList();
            foreach (var penalty in stagePenalties)
            {
                var teamRanks = rGroupsList.Where(x => x.Id == penalty.TeamId);
                foreach (var teamRank in teamRanks)
                {
                    teamRank.Points -= (int)penalty.Points;
                }
            }
        }

        public void GetPoints(List<RankTeam> rGroupsList, Group group, List<Stage> stages, int index, bool flag = false, bool sameRecords = false,
            bool isTennisLeague = false)
        {
            if (group.PointEditType == 0 || group.PointEditType == 3)
            {
                if ((index - 1) >= 0)
                    foreach (var groupPre in stages[index - 1].Groups)
                    {
                        GetPoints(rGroupsList, groupPre, stages, index - 1, true, group.PointEditType == 3);
                    }
            }
            if (group.TypeId == 3 || group.TypeId == 2)
            {
                if ((index - 1) >= 0 && stages[index - 1].Groups.All(x => x.TypeId == 3 || x.TypeId == 2))
                    foreach (var groupPre in stages[index - 1].Groups)
                    {
                        GetPoints(rGroupsList, groupPre, stages, index - 1, false);
                    }
            }
            Game setting = settings.FirstOrDefault(x => x.StageId == @group.StageId) ?? settings.FirstOrDefault();

            if ((group.TypeId == 3 || group.TypeId == 2) && !isTennisLeague)
            {
                var positions = group.PlayoffBrackets.OrderBy(x => x.MaxPos);
                if (positions.Any())
                {
                    setting.PointsWin = positions.First().MinPos / 2;
                    setting.PointsTechWin = positions.First().MinPos / 2;
                }
                setting.PointsWin = 1;
                setting.PointsTechWin = 1;
                setting.PointsTechLoss = 0;
                setting.PointsDraw = 0;
                setting.PointsLoss = 0;
            }
            if (!isTennisLeague)
            {
                switch (SectionAlias)
                {
                    case GamesAlias.WaterPolo:
                    case GamesAlias.Soccer:
                    case GamesAlias.Rugby:
                    case GamesAlias.Softball:
                        CalculatePointsForWaterPolo(group, setting, flag, rGroupsList, sameRecords, false, SectionAlias == GamesAlias.Softball);
                        break;
                    case GamesAlias.BasketBall:
                        CalculatePointsForBasketBall(group, setting, flag, rGroupsList, sameRecords);
                        break;
                    default:
                        CalculatePoints(group, flag, rGroupsList, setting, sameRecords);
                        break;
                }
            }
            else
            {
                CalculatePointsForTennisLeagues(group, setting, flag, rGroupsList, sameRecords);
            }
        }

        private void CalculatePointsForTennisLeagues(Group group, Game setting, bool flag, List<RankTeam> rGroupsList, bool sameRecords,
            bool withTheirRecords = false)
        {
            bool isDivision = group.TypeId == GameTypeId.Division;
            var games = new List<GamesCycle>();
            games = withTheirRecords ? group.GamesCycles.Where(g => rGroupsList.Any(t => t.Id == g.HomeTeamId)
                && rGroupsList.Any(t => t.Id == g.GuestTeamId))?.ToList()
                : group.GamesCycles.ToList();
            foreach (var game in games)
            {
                RankTeam homeTeam = flag ? rGroupsList.FirstOrDefault(x => x.Id == game.HomeTeamId) : AddTeamIfNotExist(game.HomeTeamId, rGroupsList);
                RankTeam guestTeam = flag ? rGroupsList.FirstOrDefault(x => x.Id == game.GuestTeamId) : AddTeamIfNotExist(game.GuestTeamId, rGroupsList);

                if (sameRecords)
                {
                    if (homeTeam == null) homeTeam = new RankTeam();
                    if (guestTeam == null) guestTeam = new RankTeam();
                }
                else if (homeTeam == null || guestTeam == null)
                    continue;

                if (!string.IsNullOrEmpty(game.GameStatus) && game.GameStatus.Trim() == GameStatus.Ended &&
                    setting != null)
                {
                    var tennisPlayerGames = game.TennisLeagueGames?.ToList();
                    gamesRepo.CalculateTennisGameScores(setting, tennisPlayerGames, out int homeScore, out int guestScore,
                        out int homeSetsWon, out int guestSetsWon, out int homeGaming, out int guestGaming);
                    gamesRepo.DetermineTheTennisLeagueGameWinner(game, homeScore, guestScore, homeSetsWon, guestSetsWon,
                        homeGaming, guestGaming, isDivision, out int? gameWinnerId, out int? gameLoserId);
                    SetTennisGameValues(homeTeam.TennisInfo, homeScore, homeSetsWon, guestSetsWon, homeGaming, guestGaming,
                        gameWinnerId.HasValue && gameWinnerId.Value == game.HomeTeamId,
                        gameLoserId.HasValue && gameLoserId.Value == game.HomeTeamId); // Home team values
                    SetTennisGameValues(guestTeam.TennisInfo, guestScore, guestSetsWon, homeSetsWon, guestGaming, homeGaming,
                        gameWinnerId.HasValue && gameWinnerId.Value == game.GuestTeamId,
                        gameLoserId.HasValue && gameLoserId.Value == game.GuestTeamId); // guest team values
                }
            }
        }

        private void SetTennisGameValues(TennisGameInfo info, int score, int currentSetsWon, int opponentSetsWon,
            int currentGaming, int opponentGaming, bool isWinner, bool isLoser)
        {
            info.Matches += 1;
            info.Points += score;
            info.Wins += isWinner ? 1 : 0;
            info.Lost += isLoser ? 1 : 0;
            info.Ties += !isWinner && !isLoser ? 1 : 0;
            info.PlayersSetsWon += currentSetsWon;
            info.PlayersSetsLost += opponentSetsWon;
            info.PlayersGamingWon += currentGaming;
            info.PlayersGamingLost += opponentGaming;
            info.PlayerGamingDifference = currentGaming - opponentGaming;
            //info.Penalties += penalties;
        }

        public void GetPointsWithTheirRecords(List<RankTeam> rGroupsList, List<Stage> stages)
        {
            Game setting = settings.FirstOrDefault(x => x.StageId == stages.FirstOrDefault().StageId) ?? settings.FirstOrDefault();
            foreach (var stage in stages)
            {
                foreach (var group in stage.Groups)
                {
                    switch (SectionAlias)
                    {
                        case GamesAlias.WaterPolo:
                        case GamesAlias.Soccer:
                        case GamesAlias.Rugby:
                        case GamesAlias.Softball:
                            CalculatePointsForWaterPolo(group, setting, true, rGroupsList, false, true);
                            break;
                        case GamesAlias.BasketBall:
                            CalculatePointsForBasketBall(group, setting, true, rGroupsList, false, true);
                            break;
                        default:
                            CalculatePoints(group, true, rGroupsList, setting, false, true);
                            break;
                    }
                }
            }
        }

        private List<ExtendedTable> CreateExtendedTable(Group group, List<RankTeam> rGroupsList)
        {
            var results = new List<ExtendedTable>();
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower().ToCharArray();
            int charIndex = 0;

            foreach (var game in group.GamesCycles)
            {

                RankTeam homeTeam = rGroupsList.FirstOrDefault(x => x.Id == game.HomeTeamId);
                RankTeam guestTeam = rGroupsList.FirstOrDefault(x => x.Id == game.GuestTeamId);

                if (guestTeam == null || homeTeam == null)
                    continue;

                SetExtendedTableScoresForHomeTeam(results, homeTeam, guestTeam, alpha, ref charIndex, game);
                SetExtendedTableScoresForGuestTeam(results, guestTeam, homeTeam, alpha, ref charIndex, game);
            }

            results = results.OrderBy(x => x.TeamName).ToList();
            return results;
        }

        private void SetExtendedTableScoresForHomeTeam(List<ExtendedTable> results, RankTeam homeTeam, RankTeam guestTeam, char[] alpha, ref int charIndex, GamesCycle game)
        {
            var homeTeamForExtended = results.FirstOrDefault(t => t.TeamId == homeTeam.Id.Value);
            //scores 
            if (homeTeamForExtended == null)
            {
                homeTeamForExtended = new ExtendedTable
                {
                    TeamId = homeTeam.Id.Value,
                    TeamName = homeTeam.Title,
                    Letter = alpha[charIndex],
                };

                results.Add(homeTeamForExtended);

                charIndex++;
            }

            if (!string.IsNullOrEmpty(game.GameStatus) && game.GameStatus.Trim() == GameStatus.Ended)
            {
                homeTeamForExtended.Scores.Add(new ExtendedTableScore
                {
                    OpponentTeamId = guestTeam.Id.Value,
                    OpponentScore = game.GuestTeamScore,
                    TeamScore = game.HomeTeamScore,
                    GameId = game.CycleId
                });
            }
        }

        private void SetExtendedTableScoresForGuestTeam(List<ExtendedTable> results, RankTeam guestTeam, RankTeam homeTeam, char[] alpha, ref int guestCharIndex, GamesCycle game)
        {
            var guestTeamForExtended = results.FirstOrDefault(t => t.TeamId == guestTeam.Id.Value);
            if (guestTeamForExtended == null)
            {
                guestTeamForExtended = new ExtendedTable
                {
                    TeamId = guestTeam.Id.Value,
                    TeamName = guestTeam.Title,
                    Letter = alpha[guestCharIndex],
                };

                results.Add(guestTeamForExtended);

                guestCharIndex++;
            }

            if (!string.IsNullOrEmpty(game.GameStatus) && game.GameStatus.Trim() == GameStatus.Ended)
            {
                guestTeamForExtended.Scores.Add(new ExtendedTableScore
                {
                    OpponentTeamId = homeTeam.Id.Value,
                    OpponentScore = game.HomeTeamScore,
                    TeamScore = game.GuestTeamScore,
                    GameId = game.CycleId
                });
            }

        }

        private void CalculatePointsForBasketBall(Group @group, Game setting, bool flag, List<RankTeam> rGroupList, bool sameRecords, bool withTheirRecords = false)
        {
            var games = new List<GamesCycle>();
            games = withTheirRecords
                ? group.GamesCycles.Where(g => !string.IsNullOrEmpty(g.GameStatus) && g.GameStatus.Trim() == GameStatus.Ended && setting != null
                    && rGroupList.Any(t => t.Id == g.HomeTeamId) && rGroupList.Any(t => t.Id == g.GuestTeamId))?.ToList()
                : group.GamesCycles.Where(g => !string.IsNullOrEmpty(g.GameStatus) && g.GameStatus.Trim() == GameStatus.Ended && setting != null)?.ToList();

            foreach (var game in games)
            {

                RankTeam homeTeam = flag ? rGroupList.FirstOrDefault(x => x.Id == game.HomeTeamId) : AddTeamIfNotExist(game.HomeTeamId, rGroupList);
                RankTeam guestTeam = flag ? rGroupList.FirstOrDefault(x => x.Id == game.GuestTeamId) : AddTeamIfNotExist(game.GuestTeamId, rGroupList);

                if (sameRecords)
                {
                    if (homeTeam == null) homeTeam = new RankTeam();
                    if (guestTeam == null) guestTeam = new RankTeam();
                }
                else if (homeTeam == null || guestTeam == null)
                    continue;

                ////count of games
                homeTeam.Games++;
                guestTeam.Games++;

                guestTeam.HomeTeamFinalScore += game.GameSets.Sum(x => x.GuestTeamScore);
                guestTeam.GuesTeamFinalScore += game.GameSets.Sum(x => x.HomeTeamScore);

                homeTeam.HomeTeamFinalScore += game.GameSets.Sum(x => x.HomeTeamScore);
                homeTeam.GuesTeamFinalScore += game.GameSets.Sum(x => x.GuestTeamScore);

                homeTeam.SetsWon += game.GameSets.Sum(x => x.HomeTeamScore);
                homeTeam.SetsLost += game.GameSets.Sum(x => x.GuestTeamScore);

                guestTeam.SetsWon += game.GameSets.Sum(x => x.GuestTeamScore);
                guestTeam.SetsLost += game.GameSets.Sum(x => x.HomeTeamScore);

                //Technical Win/Lost
                if (game.TechnicalWinnnerId.HasValue)
                {
                    if (game.HomeTeamId == game.TechnicalWinnnerId.Value)
                    {
                        homeTeam.Points += setting.PointsTechWin;
                        guestTeam.Points += setting.PointsTechLoss;
                        homeTeam.Wins++;
                        guestTeam.TechLosses++;
                    }
                    else
                    {
                        guestTeam.Points += setting.PointsTechWin;
                        homeTeam.Points += setting.PointsTechLoss;
                        homeTeam.TechLosses++;
                        guestTeam.Wins++;
                    }
                }
                else
                {
                    //Normal Win/Lost
                    var homeTeamScore = game.GameSets.Sum(g => g.HomeTeamScore);
                    var guestTeamScore = game.GameSets.Sum(g => g.GuestTeamScore);
                    if (homeTeamScore > guestTeamScore)
                    {
                        //Home Team wins
                        homeTeam.Wins++;
                        guestTeam.Loses++;

                        homeTeam.Points += setting.PointsWin;
                        guestTeam.Points += setting.PointsLoss;
                    }
                    else if (homeTeamScore < guestTeamScore)
                    {
                        //Guest Team Wins
                        homeTeam.Loses++;
                        guestTeam.Wins++;

                        homeTeam.Points += setting.PointsLoss;
                        guestTeam.Points += setting.PointsWin;
                    }
                    else
                    {
                        //Draw
                        homeTeam.Draw++;
                        guestTeam.Draw++;
                        homeTeam.Points += setting.PointsDraw;
                        guestTeam.Points += setting.PointsDraw;
                    }
                }
            }

            var stagePenalties = group.Stage.TeamPenalties.ToList();
            foreach (var penalty in stagePenalties)
            {
                var teamRanks = rGroupList.Where(x => x.Id == penalty.TeamId);
                foreach (var teamRank in teamRanks)
                {
                    teamRank.Points -= (int)penalty.Points;
                }
            }
        }

        private void CalculatePoints(Group @group, bool flag, List<RankTeam> rGroupsList, Game setting, bool sameRecords, bool withTheirRecords = false)
        {
            var games = new List<GamesCycle>();
            games = withTheirRecords
                ? group.GamesCycles.Where(g => g.GameStatus == GameStatus.Ended && rGroupsList.Any(t => t.Id == g.HomeTeamId)
                                                                                && rGroupsList.Any(t => t.Id == g.GuestTeamId))?.ToList()
                : group.GamesCycles.ToList();
            foreach (var game in games)
            {
                RankTeam homeTeam = flag ? rGroupsList.FirstOrDefault(x => x.Id == game.HomeTeamId) : AddTeamIfNotExist(game.HomeTeamId, rGroupsList);
                RankTeam guestTeam = flag ? rGroupsList.FirstOrDefault(x => x.Id == game.GuestTeamId) : AddTeamIfNotExist(game.GuestTeamId, rGroupsList);

                if (sameRecords)
                {
                    if (homeTeam == null) homeTeam = new RankTeam();
                    if (guestTeam == null) guestTeam = new RankTeam();
                }
                else if (homeTeam == null || guestTeam == null)
                    continue;

                if (!string.IsNullOrEmpty(game.GameStatus) && game.GameStatus.Trim() == GameStatus.Ended &&
                    setting != null)
                {

                    homeTeam.Games++;
                    guestTeam.Games++;

                    //Technical Win/Lost
                    if (game.TechnicalWinnnerId != null)
                    {

                        if (game.HomeTeamId == game.TechnicalWinnnerId)
                        {
                            homeTeam.Points += setting.PointsTechWin;
                            guestTeam.Points += setting.PointsTechLoss;
                            homeTeam.Wins++;
                            guestTeam.Loses++;
                        }
                        else
                        {
                            guestTeam.Points += setting.PointsTechWin;
                            homeTeam.Points += setting.PointsTechLoss;
                            homeTeam.Loses++;
                            guestTeam.Wins++;
                        }
                    }
                    else
                    {
                        //Normal Win/Lost
                        if (game.HomeTeamScore > game.GuestTeamScore)
                        {
                            //Home Team wins
                            homeTeam.Points += setting.PointsWin;
                            guestTeam.Points += setting.PointsLoss;
                            homeTeam.Wins++;
                            guestTeam.Loses++;
                        }
                        else if (game.HomeTeamScore < game.GuestTeamScore)
                        {
                            //Guest Team Wins
                            homeTeam.Points += setting.PointsLoss;
                            guestTeam.Points += setting.PointsWin;
                            homeTeam.Loses++;
                            guestTeam.Wins++;
                        }
                        else
                        {
                            //Drow
                            homeTeam.Points += setting.PointsDraw;
                            guestTeam.Points += setting.PointsDraw;

                        }
                    }

                    homeTeam.SetsWon += game.HomeTeamScore;
                    homeTeam.SetsLost += game.GuestTeamScore;
                    guestTeam.SetsWon += game.GuestTeamScore;
                    guestTeam.SetsLost += game.HomeTeamScore;

                    foreach (GameSet set in game.GameSets)
                    {
                        homeTeam.TotalPointsScored += set.HomeTeamScore;
                        guestTeam.TotalPointsScored += set.GuestTeamScore;
                        homeTeam.TotalPointsLost += set.GuestTeamScore;
                        guestTeam.TotalPointsLost += set.HomeTeamScore;
                        homeTeam.HomeTeamScore += set.HomeTeamScore;
                        guestTeam.GuestTeamScore += set.GuestTeamScore;

                        guestTeam.TotalGuesTeamPoints += set.GuestTeamScore;
                        guestTeam.TotalHomeTeamPoints += set.HomeTeamScore;

                        homeTeam.TotalHomeTeamPoints += set.HomeTeamScore;
                        homeTeam.TotalGuesTeamPoints += set.GuestTeamScore;

                        if (set.HomeTeamScore == set.GuestTeamScore)
                        {
                            homeTeam.Draw++;
                            guestTeam.Draw++;
                        }
                    }
                }
            }

            var stagePenalties = group.Stage.TeamPenalties.ToList();
            foreach (var penalty in stagePenalties)
            {
                var teamRanks = rGroupsList.Where(x => x.Id == penalty.TeamId);
                foreach (var teamRank in teamRanks)
                {
                    teamRank.Points -= (int)penalty.Points;
                }
            }
        }

        private List<RankTeam> SetTeamsOrderPlayOff(List<RankTeam> teams, Group group)
        {
            IOrderedEnumerable<RankTeam> ordTeams = null;


            ordTeams = teams.OrderByDescending(t => t.Points);
            var teamGroups = ordTeams.GroupBy(t => t.Points);
            int pos = 1;
            List<RankTeam> result = new List<RankTeam>();
            foreach (var tg in teamGroups)
            {
                List<RankTeam> groupList = tg.ToList();
                if (groupList.Count() == 1)
                {
                    var t = groupList.ElementAt(0);
                    t.Position = pos.ToString();
                    result.Add(t);
                    pos++;
                }
                else if (groupList.Count() > 1)
                {
                    groupList = SortTeams(groupList, group);
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

        private List<RankTeam> SetTeamsOrder(List<RankTeam> teams, Group group)
        {
            IOrderedEnumerable<RankTeam> ordTeams = teams.OrderByDescending(t => t.Points).ThenByDescending(t => t.SetsRatioNumiric);

            var typeOfGame = sectionsRepo.GetByLeagueId(group.Stage.LeagueId).Alias;
            dynamic teamGroups;
            if (typeOfGame == GamesAlias.WaterPolo || typeOfGame == GamesAlias.BasketBall || typeOfGame == GamesAlias.Rugby)
            {
                teamGroups = ordTeams.GroupBy(t => new { t.Points })
                    .Select(t => t.OrderByDescending(_ => _.PointsDifference).ToList()).ToList();
            }
            else if (typeOfGame == GamesAlias.Softball)
            {
                teamGroups = ordTeams.GroupBy(t => new { t.Points })
                    .Select(t => t.OrderBy(_ => _.DRR).ThenByDescending(_ => _.PointsDifference).ToList()).ToList();
            }
            else
            {
                teamGroups = ordTeams.GroupBy(t => new { t.Points, t.SetsRatioNumiric })
                    .Select(t => t.ToList()).ToList();
            }

            List<RankTeam> result = new List<RankTeam>();
            int position = 1;

            for (int i = 0; i < teamGroups.Count; i++)
            {
                List<RankTeam> teamsInGroup = SortTeams(teamGroups[i], group);

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

        // copy of previous method, but based on Tennis info
        private List<RankTeam> SetTeamsOrderTennis(List<RankTeam> teams, Group group)
        {
            IOrderedEnumerable<RankTeam> ordTeams = teams.OrderByDescending(t => t.TennisInfo.Points)
                .ThenByDescending(t => t.SetsRatioNumiric);
            
            var teamGroups = ordTeams.GroupBy(t => new { t.TennisInfo.Points })
                    .Select(t => t.ToList()).ToList();

            List<RankTeam> result = new List<RankTeam>();
            int position = 1;

            for (int i = 0; i < teamGroups.Count; i++)
            {
                List<RankTeam> teamsInGroup = SortTeams(teamGroups[i], group);

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

        public List<RankTeam> SortTeams(List<RankTeam> groupList, Group group)
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

        private int GetTheBestOutOfTwoTeams(RankTeam team1, RankTeam team2, Group group)
        {
            int team1Points = 0;
            int team2Points = 0;
            var sectionAlias = group.Season?.Union?.Section?.Alias;

            // Team1 = Guest Team & Team2 = Home Team
            var games = group.GamesCycles.Where(gc => gc.GuestTeamId == team1.Id && gc.HomeTeamId == team2.Id);
            foreach (var game in games)
            {

                if (!string.Equals(sectionAlias, GamesAlias.VolleyBall, StringComparison.OrdinalIgnoreCase) && !string.Equals(sectionAlias, GamesAlias.NetBall, StringComparison.OrdinalIgnoreCase) && !string.Equals(sectionAlias, GamesAlias.Tennis, StringComparison.OrdinalIgnoreCase))
                {
                    var t1score = game.GameSets.Sum(c => c.HomeTeamScore);
                    var t2score = game.GameSets.Sum(c => c.GuestTeamScore);
                    if (t1score > t2score)
                    {
                        team2Points++;
                    }
                    if (t1score < t2score)
                    {
                        team1Points++;
                    }
                }
                else
                {
                    if (game.HomeTeamScore > game.GuestTeamScore)
                    {
                        team2Points++;
                    }
                    else if (game.HomeTeamScore < game.GuestTeamScore)
                    {
                        team1Points++;
                    }
                }
            }

            // Team1 = Home Team & Team2 = Guest Team
            games = group.GamesCycles.Where(gc => gc.GuestTeamId == team2.Id && gc.HomeTeamId == team1.Id);

            foreach (var game in games)
            {
                if (!string.Equals(sectionAlias, GamesAlias.VolleyBall, StringComparison.OrdinalIgnoreCase) && !string.Equals(sectionAlias, GamesAlias.NetBall, StringComparison.OrdinalIgnoreCase) && !string.Equals(sectionAlias, GamesAlias.Tennis, StringComparison.OrdinalIgnoreCase))
                {
                    var t1score = game.GameSets.Sum(c => c.HomeTeamScore);
                    var t2score = game.GameSets.Sum(c => c.GuestTeamScore);
                    if (t1score > t2score)
                    {
                        team1Points++;
                    }
                    if (t1score < t2score)
                    {
                        team2Points++;
                    }
                }
                else
                {
                    if (game.HomeTeamScore > game.GuestTeamScore)
                    {
                        team1Points++;
                    }
                    else if (game.HomeTeamScore < game.GuestTeamScore)
                    {
                        team2Points++;
                    }
                }
            }

            if (team1Points > team2Points)
            {
                return 1;
            }
            else if (team1Points < team2Points)
            {
                return -1;
            }
            else if (sectionAlias == GamesAlias.Softball)
            {
                if (team1.DRR < team2.DRR)
                {
                    return 1;
                }
                else if (team1.DRR > team2.DRR)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                //If the two Teams did not play together or have same score
                return 0;
            }
        }

        public RankTeam AddTeamIfNotExist(int? id, List<RankTeam> teams, int points = 0, int? seasonId = null, bool isTennisLeague = false)
        {
            RankTeam t = teams.FirstOrDefault(tm => tm.Id == id);
            if (t == null)
            {
                Team team = teamsRepo.GetById(id);
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

                    if (seasonId.HasValue)
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

        public RankTeam AddTeamIfNotExist(int? id, List<RankTeam> teams, bool isIndividual, int points = 0, int? seasonId = null)
        {
            RankTeam t = teams.FirstOrDefault(tm => tm.Id == id);
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
