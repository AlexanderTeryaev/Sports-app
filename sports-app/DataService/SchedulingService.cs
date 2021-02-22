using AppModel;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;

namespace DataService
{
    public class SchedulingService
    {

        #region Ctor
        private DataEntities _db;
        private LeagueRepo _leagueRepo;
        private StagesRepo _stagesRepo;
        private GroupsRepo _groupesRepo;
        private BracketsRepo _bracketsRepo;
        private GamesRepo _gamesRepo;
        private TeamsRepo _teamsRepo;

        protected LeagueRepo leagueRepo
        {
            get
            {
                if (_leagueRepo == null)
                {
                    _leagueRepo = new LeagueRepo(_db);
                }
                return _leagueRepo;
            }
        }
        protected StagesRepo stagesRepo
        {
            get
            {
                if (_stagesRepo == null)
                {
                    _stagesRepo = new StagesRepo(_db);
                }
                return _stagesRepo;
            }
        }
        protected GroupsRepo groupesRepo
        {
            get
            {
                if (_groupesRepo == null)

                {
                    _groupesRepo = new GroupsRepo(_db);
                }
                return _groupesRepo;
            }
        }
        protected BracketsRepo bracketsRepo
        {
            get
            {
                if (_bracketsRepo == null)
                {
                    _bracketsRepo = new BracketsRepo(_db);
                }
                return _bracketsRepo;
            }
        }
        protected GamesRepo gamesRepo
        {
            get
            {
                if (_gamesRepo == null)
                {
                    _gamesRepo = new GamesRepo(_db);
                }
                return _gamesRepo;
            }
        }
        protected TeamsRepo teamsRepo
        {
            get
            {
                if (_teamsRepo == null)
                {
                    _teamsRepo = new TeamsRepo(_db);
                }
                return _teamsRepo;
            }
        }

        public SchedulingService(DataEntities db)
        {
            _db = db;
        }

        #endregion

        #region Early Groups

        public void ScheduleGames(int leagueId)
        {
            //Find League
            var league = leagueRepo.GetById(leagueId);

            //Find Game Settings
            var games = league?.Games.ToList();
            if (games == null)
            {
                return;
            }

            //Get Stages
            var lastStage = stagesRepo.GetLastStageForLeague(league.LeagueId);
            if (lastStage == null)
            {
                return;
            }
            if (lastStage.Groups.Count(t => t.IsArchive == false) == 0)
            {
                return;
            }

            //Delete all games from last stage
            stagesRepo.DeleteAllGameCycles(lastStage.StageId);

            var gamesList = new List<GamesCycle>();
            foreach (var group in lastStage.Groups.Where(g => !g.IsArchive))
            {
                var game = games.FirstOrDefault(x => x.StageId == group.StageId) ?? games.First();
                var groupGames = new List<GamesCycle>();
                var settings = GetGameSettings(game);

                if (group.TypeId == GameTypeId.Division)
                {
                    groupGames = ScheduleDivisionGroup(group, settings);
                }
                else if (group.TypeId == GameTypeId.Playoff || group.TypeId == GameTypeId.Knockout || group.TypeId == GameTypeId.Knockout34)
                {
                    SchedulePlayoffGroup(groupGames, group, settings);
                    stagesRepo.Save();
                    SetWinnersAndLosers(group.PlayoffBrackets.ToList());
                }
                ValidateTimePlaceContradictions(groupGames, TimeSpan.Parse(game.GamesInterval));
                gamesList.AddRange(groupGames);
            }

            if (lastStage.CreateCrossesStage && lastStage.Groups.Count == 2)
            {
                var crossesStage = new Stage
                {
                    Number = league.Stages.Count(x => !x.IsArchive) + 1,
                    LeagueId = leagueId,
                    IsCrossesStage = true,
                    ParentStageId = lastStage.StageId
                };
                _db.Stages.Add(crossesStage);

                var crossesGroup = new Group
                {
                    Stage = crossesStage,
                    TypeId = 1,
                    Name = string.Empty,
                    SeasonId = league.SeasonId
                };
                _db.Groups.Add(crossesGroup);

                var teamsCountToCross = lastStage.Groups.Where(x => !x.IsArchive)
                    .SelectMany(x => x.GroupsTeams)
                    .Count(x => !x.Team?.IsArchive ?? false);

                for (var i = 0; i < teamsCountToCross; i++)
                {
                    _db.GroupsTeams.Add(new GroupsTeam
                    {
                        Group = crossesGroup,
                        Pos = i + 1
                    });
                }

                for (var i = 0; i < teamsCountToCross / 2; i++)
                {
                    _db.GamesCycles.Add(new GamesCycle
                    {
                        Stage = crossesStage,
                        CycleNum = 0,
                        StartDate = DateTime.Now,
                        Group = crossesGroup
                    });
                }
            }
            for (var i = 0; i < gamesList.Count; i++)
            {
                gamesList[i].BracketIndex = i;
            }

            UpdateAuditorium(gamesList);
            gamesRepo.Save();
        }

        public bool ScheduleTennisGames(int categoryId, int seasonId)
        {
            //Find League
            var category = teamsRepo.GetByCategoryAndSeason(categoryId, seasonId).FirstOrDefault();

            //Find Game Settings
            var games = category.TennisGames.ToList();
            if (games == null)
            {
                return true;
            }

            //Get Stages
            var lastStage = stagesRepo.GetLastTennisStageForCategory(categoryId);
            if (lastStage == null)
            {
                return true;
            }
            if (lastStage.TennisGroups.Count(t => t.IsArchive == false) == 0)
            {
                return true;
            }

            //Delete all games from last stage
            stagesRepo.DeleteAllTennisGameCycles(lastStage.StageId);

            var gamesList = new List<TennisGameCycle>();
            foreach (var group in lastStage.TennisGroups.Where(g => !g.IsArchive))
            {
                var game = games.FirstOrDefault(x => x.StageId == group.StageId) ?? games.First();
                if(game == null)
                {
                    return false;
                }
                var groupGames = new List<TennisGameCycle>();
                var settings = GetTennisGameSettings(game);

                if (group.TypeId == GameTypeId.Division)
                {
                    groupGames = ScheduleTennisDivisionGroup(group, settings);
                }
                else if (group.TypeId == GameTypeId.Playoff || group.TypeId == GameTypeId.Knockout || group.TypeId == GameTypeId.Knockout34 || group.TypeId == GameTypeId.Knockout34Consolences1Round || group.TypeId == GameTypeId.Knockout34ConsolencesQuarterRound)
                {
                    ScheduleTennisPlayoffGroup(groupGames, group, settings);
                }


                ValidateTennisTimePlaceContradictions(groupGames, TimeSpan.Parse(game.GamesInterval));
                gamesList.AddRange(groupGames);
            }

            for (var i = 0; i < gamesList.Count; i++)
            {
                gamesList[i].BracketIndex = i;
            }

            UpdateField(gamesList);
            gamesRepo.Save();
            return true;
        }

        #endregion

        private List<GamesCycle> ScheduleDivisionGroup(Group group, GameSettings settings)
        {
            var teamPositions = GetGroupTeamsByPositions(group);
            var numberOfCycles = group.NumberOfCycles ?? 1;
            var matchCombinations = RoundRobin.GetMatches(teamPositions.Keys.ToList());
            var combinations = CreateCombinationCycles(numberOfCycles, matchCombinations);
            var groupGames = CreateGamesFromCombinations(combinations, teamPositions, group.SeasonId);
            groupGames.ForEach(g =>
            {
                g.GroupId = group.GroupId;
                g.StageId = group.StageId;
            });

            var numberOfTeams = teamPositions.Count;
            var numberOfGamesInRound = numberOfTeams / 2;
            var numberOfRoundsInOneCycle = GetNumberOfRoundsInOneCycle(group.TypeId, numberOfTeams);
            var totalNumberOfRoundes = numberOfRoundsInOneCycle * numberOfCycles;
            var sectionAlias = group?.Season?.Union?.Section?.Alias;
            ScheduleGames(settings, numberOfGamesInRound, totalNumberOfRoundes, groupGames, sectionAlias, out var playOffStagesPassed, true);
            SaveGamesList(groupGames);
            return groupGames;
        }

        private List<TennisGameCycle> ScheduleTennisDivisionGroup(TennisGroup group, GameSettings settings)
        {
            var teamPositions = GetTennisGroupTeamsByPositions(group);
            var numberOfCycles = group.NumberOfCycles ?? 1;
            var matchCombinations = RoundRobin.GetMatches(teamPositions.Keys.ToList());
            var combinations = CreateCombinationCycles(numberOfCycles, matchCombinations);
            var groupGames = CreateTennisGamesFromCombinations(combinations, teamPositions);
            groupGames.ForEach(g =>
            {
                g.GroupId = group.GroupId;
                g.StageId = group.StageId;
            });

            var numberOfTeams = teamPositions.Count;
            var numberOfGamesInRound = numberOfTeams / 2;
            var numberOfRoundsInOneCycle = GetNumberOfRoundsInOneCycle(group.TypeId, numberOfTeams);
            var totalNumberOfRoundes = numberOfRoundsInOneCycle * numberOfCycles;
            ScheduleTennisGames(settings, numberOfGamesInRound, totalNumberOfRoundes, groupGames, out var playOffStagesPassed, true);

            foreach (var g in groupGames)
            {
                for (var j = 1; j <= 5; j++)
                {
                    var gameSet = new TennisGameSet();
                    gameSet.GameCycleId = g.CycleId;
                    gameSet.SetNumber = j;
                    gameSet.FirstPlayerScore = 0;
                    gameSet.SecondPlayerScore = 0;
                    g.TennisGameSets.Add(gameSet);
                }
            }

            SaveTennisGamesList(groupGames);
            return groupGames;
        }

        #region Playoff

        private void SchedulePlayoffGroup(List<GamesCycle> games, Group group, GameSettings settings, int stagesLeft = 0, int stagesPassed = 0)
        {
            if (!group.IsAdvanced)
            {
                bracketsRepo.DeleteAllBracketsAndChildrenBracketsForGroup(group);
                CreateBracketsForEarlyStagePlayoffGroup(group);
                var ligitNumberOfTeams = GetPlayoffTeamCount(group.GroupsTeams.Count);
                stagesLeft = Convert.ToInt32(Math.Log(ligitNumberOfTeams, 2)) - 1;
            }
            else
            {
                group = CreateNextPlayoffStep(group);
            }

            var numberOfTeams = group.PlayoffBrackets.Count * 2;
            var numberOfGamesInRound = numberOfTeams / 2;
            var numberOfRoundsInOneCycle = GetNumberOfRoundsInOneCycle(group.TypeId, numberOfTeams);
            var numberOfCycles = group.NumberOfCycles ?? 1;
            var totalNumberOfRoundes = numberOfRoundsInOneCycle * numberOfCycles;

            var groupGames = CreateGamesForPlayoffGroup(group);
            var sectionAlias = group?.Season?.Union?.Section?.Alias;
            ScheduleGames(settings, numberOfGamesInRound, totalNumberOfRoundes, groupGames, sectionAlias, out var playoffStagesPassed, false, group.NumberOfCycles ?? 1, stagesPassed);
            games.AddRange(groupGames);

            if (group.NumberOfCycles != null && group.NumberOfCycles > 1)
            {
                stagesPassed += playoffStagesPassed;
            }
            else
                stagesPassed++;

            if (stagesLeft > 0)
            {
                //stagesRepo.Save();
                var settingsDays = settings.SettingsDays;
                if (group.NumberOfCycles == null || group.NumberOfCycles == 1)
                {
                    settings.ActualStartDate = (stagesPassed) % settings.ActiveWeeksNumber == 0
                        ? GetNextWeekday(settings.ActualStartDate.AddDays(1 + 7 * settings.BreakWeeksNumber), settingsDays[0])
                        : GetNextWeekday(settings.ActualStartDate.AddDays(7), settingsDays[0]);
                }
                //stagesRepo.Save();
                SchedulePlayoffGroup(games, group, settings, stagesLeft - 1, stagesPassed);
            }
        }

        private void ScheduleTennisPlayoffGroup(List<TennisGameCycle> games, TennisGroup group, GameSettings settings, int stagesLeft = 0, int stagesPassed = 0)
        {
            if (!group.IsAdvanced)
            {
                bracketsRepo.DeleteAllTennisBracketsAndChildrenBracketsForGroup(group);
                CreateTennisBracketsForEarlyStagePlayoffGroup(group);
                var ligitNumberOfPlayers = group.TennisGroupTeams.Count;

                if(group.TypeId != GameTypeId.Knockout34Consolences1Round && group.TypeId != GameTypeId.Knockout34ConsolencesQuarterRound)
                {
                    if (ligitNumberOfPlayers <= 8)
                    {
                        ligitNumberOfPlayers = 8;
                    }
                    else if(ligitNumberOfPlayers <= 16){
                        ligitNumberOfPlayers = 16;
                    }
                    else if (ligitNumberOfPlayers <= 32)
                    {
                        ligitNumberOfPlayers = 32;
                    }
                    else if (ligitNumberOfPlayers <= 64)
                    {
                        ligitNumberOfPlayers = 64;
                    }
                }

                stagesLeft = Convert.ToInt32(Math.Log(ligitNumberOfPlayers, 2)) - 1;
                condolenceLoopDone = false;
            }
            else
            {
                if (group.TypeId == GameTypeId.Knockout34Consolences1Round)
                {
                    group = CreateNextTennisPlayoffStep(group, stagesLeft, stagesPassed);
                }
                else if (group.TypeId == GameTypeId.Knockout34ConsolencesQuarterRound)
                {
                    group = CreateNextTennisKnockout34CondolencesUntilQuarterStep(group, stagesLeft, stagesPassed);
                }
                else
                {
                    group = CreateNextTennisPlayoffStep(group);
                }
            }

            var numberOfPlayers = group.TennisPlayoffBrackets.Count * 2;
            var numberOfGamesInRound = numberOfPlayers / 2;
            var numberOfRoundsInOneCycle = GetNumberOfRoundsInOneCycle(group.TypeId, numberOfPlayers);
            var numberOfCycles = group.NumberOfCycles ?? 1;
            var totalNumberOfRoundes = numberOfRoundsInOneCycle * numberOfCycles;

            var groupGames = CreateTennisGamesForPlayoffGroup(group);
            ScheduleTennisGames(settings, numberOfGamesInRound, totalNumberOfRoundes, groupGames, out var playoffStagesPassed, false, group.NumberOfCycles ?? 1, stagesPassed);
            games.AddRange(groupGames);

            if (group.NumberOfCycles != null && group.NumberOfCycles > 1)
            {
                stagesPassed += playoffStagesPassed;
            }
            else
                stagesPassed++;

            
            if ((stagesLeft > 0 && group.TypeId != GameTypeId.Knockout34ConsolencesQuarterRound) || (group.TypeId == GameTypeId.Knockout34ConsolencesQuarterRound  && !condolenceLoopDone))
            {
                var settingsDays = settings.SettingsDays;
                if (group.NumberOfCycles == null || group.NumberOfCycles == 1)
                {
                    settings.ActualStartDate = (stagesPassed) % settings.ActiveWeeksNumber == 0
                        ? GetNextWeekday(settings.ActualStartDate.AddDays(1 + 7 * settings.BreakWeeksNumber), settingsDays[0])
                        : GetNextWeekday(settings.ActualStartDate.AddDays(7), settingsDays[0]);
                }
                ScheduleTennisPlayoffGroup(games, group, settings, stagesLeft - 1, stagesPassed);
            }
        }

        private Group CreateNextPlayoffStep(Group group)
        {
            //Create New Group
            var stage = stagesRepo.Create(group.Stage.LeagueId);
            stagesRepo.Save();
            var newGroup = new Group
            {
                StageId = stage.StageId,
                Name = group.Name,
                IsArchive = false,
                NumberOfCycles = group.NumberOfCycles,
                TypeId = group.TypeId,
                IsAdvanced = true,
                IsIndividual = group.IsIndividual,
                SeasonId = group.SeasonId
            };
            groupesRepo.Create(newGroup);
            groupesRepo.Save();

            //Add Brackets to group
            CreateNextStepBrackets(group, newGroup);
            return newGroup;
        }

        private TennisGroup CreateNextTennisPlayoffStep(TennisGroup group)
        {
            //Create New Group
            var leagueId = teamsRepo.GetLeagueByTeamId(group.TennisStage.CategoryId);
            var stage = stagesRepo.CreateTennisStage(group.TennisStage.CategoryId, group.SeasonId.Value); //, leagueId?.LeagueId
            stagesRepo.Save();
            var newGroup = new TennisGroup
            {
                StageId = stage.StageId,
                Name = group.Name,
                IsArchive = false,
                NumberOfCycles = group.NumberOfCycles,
                TypeId = group.TypeId,
                IsAdvanced = true,
                IsIndividual = group.IsIndividual,
                SeasonId = group.SeasonId,
                NumberOfPlayers = group.NumberOfPlayers / 2
            };
            groupesRepo.CreateTennisGroup(newGroup);
            groupesRepo.Save();

            //Add Brackets to group
            CreateNextTennisStepBrackets(group, newGroup);
            return newGroup;
        }

        private TennisGroup CreateNextTennisPlayoffStep(TennisGroup group, int stepsLeft, int stepsPassed)
        {
            //Create New Group
            TennisStage stage = stagesRepo.CreateTennisStage(group.TennisStage.CategoryId, group.SeasonId.Value);
            stagesRepo.Save();
            TennisGroup newGroup = new TennisGroup
            {
                StageId = stage.StageId,
                Name = group.Name,
                IsArchive = false,
                NumberOfCycles = group.NumberOfCycles,
                TypeId = group.TypeId,
                IsAdvanced = true,
                IsIndividual = group.IsIndividual,
                SeasonId = group.SeasonId,
                NumberOfPlayers = group.NumberOfPlayers / 2
            };
            groupesRepo.CreateTennisGroup(newGroup);
            groupesRepo.Save();

            //Add Brackets to group

            CreateNextTennisStepBracketsCustomized(group, newGroup, stepsLeft, stepsPassed);
            return newGroup;
        }

        private bool condolenceLoopDone = false;
        private TennisGroup CreateNextTennisKnockout34CondolencesUntilQuarterStep(TennisGroup group, int stepsLeft, int stepsPassed)
        {
            //Create New Group
            TennisStage stage = stagesRepo.CreateTennisStage(group.TennisStage.CategoryId, group.SeasonId.Value);
            stagesRepo.Save();
            TennisGroup newGroup = new TennisGroup
            {
                StageId = stage.StageId,
                Name = group.Name,
                IsArchive = false,
                NumberOfCycles = group.NumberOfCycles,
                TypeId = group.TypeId,
                IsAdvanced = true,
                IsIndividual = group.IsIndividual,
                SeasonId = group.SeasonId,
                NumberOfPlayers = group.NumberOfPlayers / 2
            };
            groupesRepo.CreateTennisGroup(newGroup);
            groupesRepo.Save();

            //Add Brackets to group
            condolenceLoopDone = true;
            CreateNextTennisStepBracketsForKnockout34CondolencesUntilQuarter(group, newGroup, stepsLeft, stepsPassed);
            return newGroup;
        }


        private void CreateBracketsForEarlyStagePlayoffGroup(Group group)
        {
            group.IsAdvanced = true;
            var isIndividual = group.IsIndividual;
            var teamPositions = GetGroupTeamsByPositions(group);

            var combinations = CreatePlayoffCombinations(teamPositions.Keys.ToList());
            var brackets = new List<PlayoffBracket>();
            teamPositions[0] = null;
            foreach (var comb in combinations)
            {
                var b = new PlayoffBracket
                {
                    Team1GroupPosition = comb.Item1,
                    Team2GroupPosition = comb.Item2,
                    Team1Id = teamPositions[comb.Item1]?.Team?.TeamId,
                    Team2Id = teamPositions[comb.Item2]?.Team?.TeamId,
                    Athlete1Id = teamPositions[comb.Item1]?.Athlete?.Id,
                    Athlete2Id = teamPositions[comb.Item2]?.Athlete?.Id,
                    MaxPos = 1,
                    MinPos = combinations.Count * 2,
                    Stage = 1,
                    Type = (int)PlayoffBracketType.Root
                };
                if (group.IsIndividual)
                    b.WinnerAthleteId = comb.Item2 == 0 ? teamPositions[comb.Item1]?.Athlete?.Id : null;
                else
                    b.WinnerId = comb.Item2 == 0 ? teamPositions[comb.Item1]?.Team?.TeamId : null;

                brackets.Add(b);
            }
            group.PlayoffBrackets = brackets;
            groupesRepo.Save();
        }

        private void CreateTennisBracketsForEarlyStagePlayoffGroup(TennisGroup group)
        {
            group.IsAdvanced = true;
            var isIndividual = group.IsIndividual;
            var teamPositions = GetTennisGroupTeamsByPositions(group);

            var isRanked = false;
            var combinations = CreatePlayoffCombinations(teamPositions.Keys.ToList());
            if(group.NumberOfPlayers > 0 && group.NumberOfPlayers < group.TennisGroupTeams.Count())
            {
                isRanked = true;
            }

            var brackets = new List<TennisPlayoffBracket>();
            teamPositions[0] = null;
            foreach (var comb in combinations)
            {
                var b = new TennisPlayoffBracket
                {
                    FirstPlayerId = teamPositions[comb.Item1]?.Athlete?.Id,
                    SecondPlayerId = teamPositions[comb.Item2]?.Athlete?.Id,
                    FirstPlayerPairId = teamPositions[comb.Item1]?.PairAthlete?.Id,
                    SecondPlayerPairId = teamPositions[comb.Item2]?.PairAthlete?.Id,
                    MaxPos = 1,
                    MinPos = combinations.Count * 2,
                    Stage = 1,
                    Type = (int)PlayoffBracketType.Root,
                };
                if (group.IsIndividual)
                {
                    b.WinnerId = comb.Item2 == 0 ? teamPositions[comb.Item1]?.Athlete?.Id : null;
                    b.WinnerPlayerPairId = comb.Item2 == 0 ? teamPositions[comb.Item1]?.PairAthlete?.Id : null;
                    if (isRanked)
                    {
                        var firstPlayer = teamPositions[comb.Item1].Athlete;
                        var secondPlayer = teamPositions[comb.Item2].Athlete;
                        if (firstPlayer == null && secondPlayer != null)
                        {
                            b.WinnerId = secondPlayer.Id;
                            //b.LoserId = 0;
                        }
                        if (firstPlayer != null && secondPlayer == null)
                        {
                            b.WinnerId = firstPlayer.Id;
                            //b.LoserId = 0;
                        }
                        var firstPairPlayer = teamPositions[comb.Item1].PairAthlete;
                        var secondPairPlayer = teamPositions[comb.Item2].PairAthlete;
                        if (firstPairPlayer == null && secondPairPlayer != null)
                        {
                            b.WinnerPlayerPairId = secondPairPlayer.Id;
                            //b.LoserPlayerPairId = 0;
                        }
                        if (firstPairPlayer != null && secondPairPlayer == null)
                        {
                            b.WinnerPlayerPairId = firstPairPlayer.Id;
                            //b.LoserPlayerPairId = 0;
                        }
                    }
                }

                brackets.Add(b);
            }
            group.TennisPlayoffBrackets = brackets;
            groupesRepo.Save();
        }

        private List<GamesCycle> CreateGamesForPlayoffGroup(Group group)
        {
            var games = new List<GamesCycle>();
            for (var i = 1; i <= group.NumberOfCycles; i++)
            {
                foreach (var bracket in group.PlayoffBrackets)
                {
                    var g = new GamesCycle
                    {
                        MinPlayoffPos = bracket.MinPos,
                        MaxPlayoffPos = bracket.MaxPos,
                        GroupId = group.GroupId,
                        StageId = group.StageId,
                        RoundNum = i
                    };

                    if (bracket.Type == (int)PlayoffBracketType.Root && (bracket.Team1Id == null || bracket.Team2Id == null))
                    {
                        g.GameStatus = null;
                    }

                    if (i % 2 != 0)
                    {
                        g.HomeTeamPos = bracket.Team1GroupPosition > 0 ? bracket.Team1GroupPosition : null;
                        g.HomeTeamId = bracket.Team1Id;
                        g.HomeAthleteId = bracket.Athlete1Id;
                        g.GuestTeamPos = bracket.Team2GroupPosition > 0 ? bracket.Team2GroupPosition : null;
                        g.GuestTeamId = bracket.Team2Id;
                        g.GuestAthleteId = bracket.Athlete2Id;
                    }
                    else
                    {
                        g.HomeTeamPos = bracket.Team2GroupPosition > 0 ? bracket.Team2GroupPosition : null;
                        g.HomeTeamId = bracket.Team2Id;
                        g.HomeAthleteId = bracket.Athlete2Id;
                        g.GuestTeamPos = bracket.Team1GroupPosition > 0 ? bracket.Team1GroupPosition : null;
                        g.GuestTeamId = bracket.Team1Id;
                        g.GuestAthleteId = bracket.Athlete1Id;
                    }
                    bracket.GamesCycles.Add(g);
                    games.Add(g);
                }
            }
            return games;
        }

        private List<TennisGameCycle> CreateTennisGamesForPlayoffGroup(TennisGroup group)
        {
            var games = new List<TennisGameCycle>();
            for (var i = 1; i <= group.NumberOfCycles; i++)
            {
                foreach (var bracket in group.TennisPlayoffBrackets)
                {
                    var g = new TennisGameCycle
                    {
                        MinPlayoffPos = bracket.MinPos,
                        MaxPlayoffPos = bracket.MaxPos,
                        GroupId = group.GroupId,
                        StageId = group.StageId,
                        RoundNum = i
                    };

                    if (bracket.Type == (int)PlayoffBracketType.Root && (bracket.FirstPlayerId == null || bracket.SecondPlayerId == null))
                    {
                        g.GameStatus = null;
                    }

                    if (i % 2 != 0)
                    {
                        g.FirstPlayerId = bracket.FirstPlayerId;
                        g.SecondPlayerId = bracket.SecondPlayerId;
                        g.FirstPlayerPairId = bracket.FirstPlayerPairId;
                        g.SecondPlayerPairId = bracket.SecondPlayerPairId;
                    }
                    else
                    {
                        g.FirstPlayerId = bracket.FirstPlayerId;
                        g.SecondPlayerId = bracket.SecondPlayerId;
                        g.FirstPlayerPairId = bracket.FirstPlayerPairId;
                        g.SecondPlayerPairId = bracket.SecondPlayerPairId;
                    }
                    bracket.TennisGameCycles.Add(g);
                    for (var j = 1; j <= 5; j++)
                    {
                        var gameSet = new TennisGameSet();
                        gameSet.GameCycleId = g.CycleId;
                        gameSet.SetNumber = j;
                        gameSet.FirstPlayerScore = 0;
                        gameSet.SecondPlayerScore = 0;
                        g.TennisGameSets.Add(gameSet);
                    }
                    games.Add(g);
                }
            }
            return games;
        }

        private void CreateNextStepBrackets(Group parentGroup, Group childGroup)
        {
            var parentBracketGroups = parentGroup.PlayoffBrackets.GroupBy(b => new { b.Stage, b.MinPos, b.MaxPos });

            var nextBrackets = new List<PlayoffBracket>();

            foreach (var brackets in parentBracketGroups)
            {
                var minPos = brackets.Key.MinPos;
                var maxPos = brackets.Key.MaxPos;
                var middelPos = ((brackets.Key.MinPos - brackets.Key.MaxPos) / 2) + brackets.Key.MaxPos;
                var stage = brackets.Key.Stage + 1;

                var startIndex = 0;
                var endIndex = brackets.Count() - 1;
                for (var i = 0; i < brackets.Count() / 2; i++)
                {
                    var parent1 = brackets.ElementAt(startIndex);
                    startIndex++;
                    var parent2 = brackets.ElementAt(endIndex);
                    endIndex--;
                    var winnerBracket = new PlayoffBracket();
                    winnerBracket.Team1Id = parent1.WinnerId;
                    winnerBracket.Team2Id = parent2.WinnerId;
                    winnerBracket.Athlete1Id = parent1.WinnerAthleteId;
                    winnerBracket.Athlete2Id = parent2.WinnerAthleteId;
                    winnerBracket.Stage = stage;
                    winnerBracket.MaxPos = maxPos;
                    winnerBracket.MinPos = middelPos;
                    winnerBracket.Type = (int)PlayoffBracketType.Winner;
                    winnerBracket.ParentBracket1Id = parent1.Id;
                    winnerBracket.ParentBracket2Id = parent2.Id;
                    winnerBracket.GroupId = childGroup.GroupId;
                    nextBrackets.Add(winnerBracket);


                    if (childGroup.TypeId == GameTypeId.Playoff || (childGroup.TypeId == GameTypeId.Knockout34 && brackets.Count() == 2))
                    {
                        var loserBracket = new PlayoffBracket();
                        loserBracket.Team1Id = parent1.LoserId;
                        loserBracket.Team2Id = parent2.LoserId;
                        loserBracket.Stage = brackets.Key.Stage + 1;
                        loserBracket.MaxPos = middelPos + 1;
                        loserBracket.MinPos = minPos;
                        loserBracket.Type = (int)PlayoffBracketType.Loseer;
                        loserBracket.ParentBracket1Id = parent1.Id;
                        loserBracket.ParentBracket2Id = parent2.Id;
                        loserBracket.GroupId = childGroup.GroupId;
                        if (childGroup.TypeId == GameTypeId.Knockout34)
                        {
                            loserBracket.Type = (int)PlayoffBracketType.Condolence3rdPlaceBracket;

                            var finalBracket = nextBrackets.LastOrDefault();
                            nextBrackets.Remove(finalBracket);
                            nextBrackets.Add(loserBracket);
                            nextBrackets.Add(finalBracket);
                        }
                        else
                            nextBrackets.Add(loserBracket);
                    }
                }
            }
            var list = nextBrackets.OrderBy(b => b.Type).ToList();
            bracketsRepo.SaveBrackets(list);
        }

        private void CreateNextTennisStepBrackets(TennisGroup parentGroup, TennisGroup childGroup)
        {
            var parentBracketGroups = parentGroup.TennisPlayoffBrackets.GroupBy(b => new { b.Stage, b.MinPos, b.MaxPos });

            var nextBrackets = new List<TennisPlayoffBracket>();

            foreach (var brackets in parentBracketGroups)
            {
                var minPos = brackets.Key.MinPos;
                var maxPos = brackets.Key.MaxPos;
                var middelPos = ((brackets.Key.MinPos - brackets.Key.MaxPos) / 2) + brackets.Key.MaxPos;
                var stage = brackets.Key.Stage + 1;

                var startIndex = 0;
                var endIndex = brackets.Count() - 1;
                for (var i = 0; i < brackets.Count() / 2; i++)
                {
                    var parent1 = brackets.ElementAt(startIndex);
                    startIndex++;
                    var parent2 = brackets.ElementAt(endIndex);
                    endIndex--;
                    var winnerBracket = new TennisPlayoffBracket();
                    winnerBracket.FirstPlayerId = parent1.WinnerId;
                    winnerBracket.FirstPlayerPairId = parent1.WinnerPlayerPairId;
                    winnerBracket.SecondPlayerId = parent2.WinnerId;
                    winnerBracket.SecondPlayerPairId = parent2.WinnerPlayerPairId;
                    winnerBracket.Stage = stage;
                    winnerBracket.MaxPos = maxPos;
                    winnerBracket.MinPos = middelPos;
                    winnerBracket.Type = (int)PlayoffBracketType.Winner;
                    winnerBracket.ParentBracket1Id = parent1.Id;
                    winnerBracket.ParentBracket2Id = parent2.Id;
                    winnerBracket.GroupId = childGroup.GroupId;
                    nextBrackets.Add(winnerBracket);


                    if (childGroup.TypeId == GameTypeId.Playoff || (childGroup.TypeId == GameTypeId.Knockout34 && brackets.Count() == 2) || childGroup.TypeId == GameTypeId.Knockout34Consolences1Round)
                    {
                        var loserBracket = new TennisPlayoffBracket();
                        loserBracket.FirstPlayerId = parent1.LoserId;
                        loserBracket.FirstPlayerPairId = parent1.LoserPlayerPairId;
                        loserBracket.SecondPlayerId = parent2.LoserId;
                        loserBracket.SecondPlayerPairId = parent2.LoserPlayerPairId;
                        loserBracket.Stage = brackets.Key.Stage + 1;
                        loserBracket.MaxPos = middelPos + 1;
                        loserBracket.MinPos = minPos;
                        loserBracket.Type = (int)PlayoffBracketType.Loseer;
                        loserBracket.ParentBracket1Id = parent1.Id;
                        loserBracket.ParentBracket2Id = parent2.Id;
                        loserBracket.GroupId = childGroup.GroupId;

                        if (childGroup.TypeId == GameTypeId.Knockout34)
                        {
                            loserBracket.Type = (int)PlayoffBracketType.Condolence3rdPlaceBracket;

                            var finalBracket = nextBrackets.LastOrDefault();
                            nextBrackets.Remove(finalBracket);
                            nextBrackets.Add(loserBracket);
                            nextBrackets.Add(finalBracket);
                        }
                        else
                            nextBrackets.Add(loserBracket);
                    }
                }
            }
            var list = nextBrackets.OrderBy(b => b.Type).ToList();
            bracketsRepo.SaveTennisBrackets(list);
        }

        private void CreateNextTennisStepBracketsCustomized(TennisGroup parentGroup, TennisGroup childGroup, int stepsLeft, int stepsPassed)
        {
            var parentBracketGroups = parentGroup.TennisPlayoffBrackets.GroupBy(b => new { b.Stage, b.MinPos, b.MaxPos });
            var averagePos = 0;
            List<TennisPlayoffBracket> nextBrackets = new List<TennisPlayoffBracket>();
            foreach (var brackets in parentBracketGroups)
            {
                averagePos += brackets.Key.MaxPos;
            }
            averagePos = averagePos / parentBracketGroups.Count();
            foreach (var brackets in parentBracketGroups)
            {
                int minPos = brackets.Key.MinPos;
                int maxPos = brackets.Key.MaxPos;
                int middelPos = ((brackets.Key.MinPos - brackets.Key.MaxPos) / 2) + brackets.Key.MaxPos;
                int stage = brackets.Key.Stage + 1;

                int startIndex = 0;
                int endIndex = brackets.Count() - 1;
                for (int i = 0; i < brackets.Count() / 2; i++)
                {
                    var parent1 = brackets.ElementAt(startIndex);
                    startIndex++;
                    var parent2 = brackets.ElementAt(endIndex);
                    endIndex--;
                    TennisPlayoffBracket winnerBracket = new TennisPlayoffBracket();
                    winnerBracket.FirstPlayerId = parent1.WinnerId;
                    winnerBracket.FirstPlayerPairId = parent1.WinnerPlayerPairId;
                    winnerBracket.SecondPlayerId = parent2.WinnerId;
                    winnerBracket.SecondPlayerPairId = parent2.WinnerPlayerPairId;
                    winnerBracket.Stage = stage;
                    winnerBracket.MaxPos = maxPos;
                    winnerBracket.MinPos = middelPos;
                    if (parent1.Type == (int)PlayoffBracketType.Loseer || parent1.Type == (int)PlayoffBracketType.CondolenceWinner)
                    {
                        winnerBracket.Type = (int)PlayoffBracketType.CondolenceWinner;
                    }
                    else
                    {
                        winnerBracket.Type = (int)PlayoffBracketType.Winner;
                    }

                    winnerBracket.ParentBracket1Id = parent1.Id;
                    winnerBracket.ParentBracket2Id = parent2.Id;
                    winnerBracket.GroupId = childGroup.GroupId;
                    nextBrackets.Add(winnerBracket);

                    if (childGroup.TypeId == GameTypeId.Playoff || (childGroup.TypeId == GameTypeId.Knockout34Consolences1Round && (stepsPassed == 1 || (stepsLeft == 0 && parent1.MaxPos < averagePos))))
                    {
                        TennisPlayoffBracket loserBracket = new TennisPlayoffBracket();
                        loserBracket.FirstPlayerId = parent1.LoserId;
                        loserBracket.FirstPlayerPairId = parent1.LoserPlayerPairId;
                        loserBracket.SecondPlayerId = parent2.LoserId;
                        loserBracket.SecondPlayerPairId = parent2.LoserPlayerPairId;
                        loserBracket.Stage = brackets.Key.Stage + 1;
                        loserBracket.MaxPos = middelPos + 1;
                        loserBracket.MinPos = minPos;

                        if (stepsLeft == 0 && parent1.MaxPos < averagePos)
                        {
                            loserBracket.Type = (int)PlayoffBracketType.Condolence3rdPlaceBracket;
                        }
                        else
                        {
                            loserBracket.Type = (int)PlayoffBracketType.Loseer;
                        }


                        loserBracket.ParentBracket1Id = parent1.Id;
                        loserBracket.ParentBracket2Id = parent2.Id;
                        loserBracket.GroupId = childGroup.GroupId;
                        nextBrackets.Add(loserBracket);
                    }
                }
            }
            var list = nextBrackets.OrderBy(b => b.Type).ToList();
            bracketsRepo.SaveTennisBrackets(list);
        }

        private List<Tuple<int, int?, int?>> previousTupleList = new List<Tuple<int, int?, int?>>();

        private void CreateNextTennisStepBracketsForKnockout34CondolencesUntilQuarter(TennisGroup parentGroup, TennisGroup childGroup, int stepsLeft, int stepsPassed)
        {
            var parentBracketGroups = parentGroup.TennisPlayoffBrackets.GroupBy(b => new { b.Stage, b.MinPos, b.MaxPos }).OrderByDescending(x => x.FirstOrDefault().MinPos);
            var averagePos = 0;
            List<TennisPlayoffBracket> nextBrackets = new List<TennisPlayoffBracket>();
            List<TennisPlayoffBracket> condolenceBrackets = new List<TennisPlayoffBracket>();
            List<TennisPlayoffBracket> condolenceBracketsF = new List<TennisPlayoffBracket>();
            List<TennisPlayoffBracket> condolenceBracketsL = new List<TennisPlayoffBracket>();
            var tupleList = new List<Tuple<int, int?, int?>>();
            var tupleListF = new List<Tuple<int, int?, int?>>();
            var tupleListL = new List<Tuple<int, int?, int?>>();
            Tuple<int, int, int> finalCondolenceMatchSearchVars = Tuple.Create(-1, 0, 0);
            TennisPlayoffBracket finalGame = new TennisPlayoffBracket();
            var final = false;
            foreach (var brackets in parentBracketGroups)
            {
                averagePos += brackets.Key.MaxPos;
            }
            var saveTuple = false;
            averagePos = averagePos / parentBracketGroups.Count();
            for (int bracketGroupIndex = 0; bracketGroupIndex < parentBracketGroups.Count(); bracketGroupIndex++)
            {
                var brackets = parentBracketGroups.ElementAt(bracketGroupIndex);
                if(brackets.Count() <= 1)
                {
                    continue;
                }
                condolenceLoopDone = false;
                int minPos = brackets.Key.MinPos;
                int maxPos = brackets.Key.MaxPos;
                int middelPos = ((brackets.Key.MinPos - brackets.Key.MaxPos) / 2) + brackets.Key.MaxPos;
                int stage = brackets.Key.Stage + 1;
                int startIndex = 0;
                int endIndex = brackets.Count() - 1;
                for (int i = 0; i < brackets.Count() / 2; i++)
                {
                    var parent1 = brackets.ElementAt(startIndex);
                    startIndex++;
                    var parent2 = brackets.ElementAt(endIndex);
                    endIndex--;
                    if (stepsPassed > 1 && bracketGroupIndex == 0 && brackets.Count() >= 4)
                    {
                        if (stepsPassed % 2 == 1)
                        {
                            TennisPlayoffBracket winnerCondolence1Bracket = new TennisPlayoffBracket();
                            winnerCondolence1Bracket.FirstPlayerId = parent1.WinnerId;
                            winnerCondolence1Bracket.FirstPlayerPairId = parent1.WinnerPlayerPairId;
                            winnerCondolence1Bracket.SecondPlayerId = parent2.WinnerId;
                            winnerCondolence1Bracket.SecondPlayerPairId = parent2.WinnerPlayerPairId;
                            winnerCondolence1Bracket.Stage = stage;
                            winnerCondolence1Bracket.MaxPos = maxPos;
                            winnerCondolence1Bracket.MinPos = middelPos;
                            winnerCondolence1Bracket.Type = (int)PlayoffBracketType.CondolenceWinner;
                            winnerCondolence1Bracket.ParentBracket1Id = parent1.Id;
                            winnerCondolence1Bracket.ParentBracket2Id = parent2.Id;
                            winnerCondolence1Bracket.GroupId = childGroup.GroupId;
                            nextBrackets.Add(winnerCondolence1Bracket);
                            saveTuple = true;
                        }
                        else
                        {
                            TennisPlayoffBracket winnerCondolence1Bracket = new TennisPlayoffBracket();
                            winnerCondolence1Bracket.FirstPlayerId = parent1.WinnerId;
                            winnerCondolence1Bracket.FirstPlayerPairId = parent1.WinnerPlayerPairId;
                            winnerCondolence1Bracket.Stage = stage;
                            winnerCondolence1Bracket.MaxPos = maxPos;
                            winnerCondolence1Bracket.MinPos = middelPos;
                            winnerCondolence1Bracket.Type = (int)PlayoffBracketType.CondolenceWinnerLooser;
                            winnerCondolence1Bracket.ParentBracket1Id = parent1.Id;
                            winnerCondolence1Bracket.GroupId = childGroup.GroupId;
                            if (bracketGroupIndex == 0)
                            {
                                winnerCondolence1Bracket.MinPos = brackets.Key.MinPos;
                            }
                            condolenceBracketsF.Add(winnerCondolence1Bracket);

                            TennisPlayoffBracket winnerCondolence2Bracket = new TennisPlayoffBracket();
                            winnerCondolence2Bracket.SecondPlayerId = parent2.WinnerId;
                            winnerCondolence2Bracket.SecondPlayerPairId = parent2.WinnerPlayerPairId;
                            winnerCondolence2Bracket.Stage = stage;
                            winnerCondolence2Bracket.MaxPos = maxPos;
                            winnerCondolence2Bracket.MinPos = middelPos;
                            winnerCondolence2Bracket.Type = (int)PlayoffBracketType.CondolenceLooserWinner;
                            winnerCondolence2Bracket.ParentBracket2Id = parent2.Id;
                            winnerCondolence2Bracket.GroupId = childGroup.GroupId;
                            if (bracketGroupIndex == 0)
                            {
                                winnerCondolence2Bracket.MinPos = brackets.Key.MinPos;
                            }
                            condolenceBracketsL.Add(winnerCondolence2Bracket);
                        }
                    }
                    else
                    {
                        TennisPlayoffBracket winnerBracket = new TennisPlayoffBracket();
                        winnerBracket.FirstPlayerId = parent1.WinnerId;
                        winnerBracket.FirstPlayerPairId = parent1.WinnerPlayerPairId;
                        winnerBracket.SecondPlayerId = parent2.WinnerId;
                        winnerBracket.SecondPlayerPairId = parent2.WinnerPlayerPairId;

                        tupleListF.Add(Tuple.Create(parent1.Id, parent1.LoserId, parent1.LoserPlayerPairId));
                        tupleListL.Add(Tuple.Create(parent2.Id, parent2.LoserId, parent2.LoserPlayerPairId));

                        winnerBracket.Stage = stage;
                        winnerBracket.MaxPos = maxPos;
                        winnerBracket.MinPos = middelPos;
                        winnerBracket.Type = (int)PlayoffBracketType.Winner;
                        winnerBracket.ParentBracket1Id = parent1.Id;
                        winnerBracket.ParentBracket2Id = parent2.Id;
                        winnerBracket.GroupId = childGroup.GroupId;

                        if (bracketGroupIndex == 0 && stepsLeft == 0)
                        {
                            finalCondolenceMatchSearchVars = Tuple.Create(stage, maxPos, middelPos);
                        }
                        if ((bracketGroupIndex == parentBracketGroups.Count() - 1) && stepsLeft == 0)
                        {
                            winnerBracket.Stage = stage + 1;
                            finalGame = winnerBracket;
                            final = true;
                        }
                        else
                        {
                            nextBrackets.Add(winnerBracket);
                        }
                        if (stepsPassed == 1 || (stepsLeft == 0 && bracketGroupIndex == parentBracketGroups.Count() - 1))
                        {
                            TennisPlayoffBracket loserBracket = new TennisPlayoffBracket();
                            loserBracket.FirstPlayerId = parent1.LoserId;
                            loserBracket.FirstPlayerPairId = parent1.LoserPlayerPairId;
                            loserBracket.SecondPlayerId = parent2.LoserId;
                            loserBracket.SecondPlayerPairId = parent2.LoserPlayerPairId;
                            loserBracket.Stage = brackets.Key.Stage + 1;
                            loserBracket.MaxPos = middelPos + 1;
                            loserBracket.MinPos = minPos;
                            loserBracket.Type = (int)PlayoffBracketType.Loseer;
                            loserBracket.ParentBracket1Id = parent1.Id;
                            loserBracket.ParentBracket2Id = parent2.Id;
                            loserBracket.GroupId = childGroup.GroupId;

                            nextBrackets.Add(loserBracket);
                        }

                    }

                }
            }

            tupleListL.Reverse();
            tupleList = tupleListL.Concat(tupleListF).ToList();
            condolenceBracketsL.Reverse();
            condolenceBrackets = condolenceBracketsF.Concat(condolenceBracketsL).ToList();
            tupleList.Reverse();
            var isSecondCondolence = false;
            if(saveTuple)
            {
                previousTupleList = tupleList;
            }
            if (condolenceBrackets.Count() != tupleList.Count() && previousTupleList.Count() == condolenceBrackets.Count() && condolenceBrackets.Count() > 0)
            {
                tupleList = previousTupleList;
                isSecondCondolence = true;
            }
                
            if (condolenceBrackets.Count() == tupleList.Count())
            {

                if (condolenceBrackets.Count() == 8)
                {                                        
                    var order = new int[] { 2, 3, 0, 1, 6, 7, 4, 5 };
                    for (int i = 0; i < condolenceBrackets.Count(); i++)
                    {
                        var at = Array.IndexOf(order, i);
                        var bracket = condolenceBrackets.ElementAt(at);
                        var tuple = tupleList.ElementAt(i);
                        if (bracket.Type == (int)PlayoffBracketType.CondolenceLooserWinner)
                        {
                            bracket.ParentBracket1Id = tuple.Item1;
                            bracket.FirstPlayerId = tuple.Item2;
                            bracket.FirstPlayerPairId = tuple.Item3;
                        }
                        else
                        {
                            bracket.ParentBracket2Id = tuple.Item1;
                            bracket.SecondPlayerId = tuple.Item2;
                            bracket.SecondPlayerPairId = tuple.Item3;
                        }
                    }
                    for (int i = 0; i < condolenceBrackets.Count(); i++)
                    {
                        var bracket = condolenceBrackets.ElementAt(i);
                        nextBrackets.Add(bracket);
                    }
                }
                else if (condolenceBrackets.Count() == 4 && isSecondCondolence)
                {
                    Swap(tupleList, 0, 2);
                    Swap(tupleList, 1, 3);
                    for (int i = 0; i < condolenceBrackets.Count(); i++)
                    {
                        var bracket = condolenceBrackets.ElementAt(i);
                        var tuple = tupleList.ElementAt(i);
                        if (bracket.Type == (int)PlayoffBracketType.CondolenceLooserWinner)
                        {
                            bracket.ParentBracket1Id = tuple.Item1;
                            bracket.FirstPlayerId = tuple.Item2;
                            bracket.FirstPlayerPairId = tuple.Item3;
                        }
                        else
                        {
                            bracket.ParentBracket2Id = tuple.Item1;
                            bracket.SecondPlayerId = tuple.Item2;
                            bracket.SecondPlayerPairId = tuple.Item3;
                        }
                        nextBrackets.Add(bracket);
                    }
                }
                else
                {
                    for (int i = 0; i < condolenceBrackets.Count(); i++)
                    {
                        var bracket = condolenceBrackets.ElementAt(i);
                        var tuple = tupleList.ElementAt(i);
                        if (bracket.Type == (int)PlayoffBracketType.CondolenceLooserWinner)
                        {
                            bracket.ParentBracket1Id = tuple.Item1;
                            bracket.FirstPlayerId = tuple.Item2;
                            bracket.FirstPlayerPairId = tuple.Item3;
                        }
                        else
                        {
                            bracket.ParentBracket2Id = tuple.Item1;
                            bracket.SecondPlayerId = tuple.Item2;
                            bracket.SecondPlayerPairId = tuple.Item3;
                        }
                        nextBrackets.Add(bracket);
                    }
                }
            }
            if (!saveTuple)
            {
                previousTupleList.Clear();
            }

            var list = nextBrackets.OrderBy(b => b.Type).ToList();
            bracketsRepo.SaveTennisBrackets(list);

            if(final)
            {
                bracketsRepo.SaveTennisBracket(finalGame);
            }

        }

        public void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }

        public void Swap<T>(Dictionary<int, T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }

        private void SetWinnersAndLosers(List<PlayoffBracket> brackets)
        {
            foreach (var bracket in brackets)
            {
                if (bracket.FirstTeam != null && bracket.SecondTeam == null)
                {
                    bracket.WinnerId = bracket.FirstTeam.TeamId;
                }
                else if (bracket.FirstTeam == null && bracket.SecondTeam != null)
                {
                    bracket.WinnerId = bracket.SecondTeam.TeamId;
                }
                else if (bracket.TeamsPlayer != null && bracket.TeamsPlayer1 == null)
                {
                    bracket.WinnerAthleteId = bracket.TeamsPlayer.Id;
                }
                else if (bracket.TeamsPlayer == null && bracket.TeamsPlayer1 != null)
                {
                    bracket.WinnerAthleteId = bracket.TeamsPlayer1.Id;
                }
            }
            bracketsRepo.Save();
        }

        #endregion

        #region Schedule



        private DateTime GetNextWeekDay(DateTime weekStartGameDate, int day)
        {
            while ((int)weekStartGameDate.DayOfWeek != day)
            {
                weekStartGameDate = weekStartGameDate.AddDays(1);
            }
            return weekStartGameDate;
        }



        private void ScheduleGames(GameSettings settings, int numberOfGamesInRound, int totalNumberOfRoundes, List<GamesCycle> groupGames, string sectionAlias,
            out int playoffStagesPassed, bool isDivision = false, int countOfRounds = 0, int stagesPassed = 0)
        {
            var settingsDays = settings.SettingsDays;
            var gameIndex = 0;
            var roundDate = settings.ActualStartDate;
            playoffStagesPassed = 0;
            var numberOfDaysLeft = settingsDays.Length;

            List<List<GamesCycle>> roundsList = new List<List<GamesCycle>>();

            int roundIndex = 0;
            for (var i = 0; i < groupGames.Count; i++)
            {
                roundIndex = i/numberOfGamesInRound;
                if(roundsList.Count() == roundIndex)
                {
                    roundsList.Add(new List<GamesCycle>());
                }

                var game = groupGames.ElementAt(i);

                Team homeTeam = _db.Teams.FirstOrDefault(x => x.TeamId == game.HomeTeamId);
                Team guestTeam = _db.Teams.FirstOrDefault(x => x.TeamId == game.GuestTeamId);

                var days = homeTeam?.TeamHostingDays?.Select(d => d.DaysForHosting.Day).ToList() ?? new List<int>();
                if (days.Count() == 0 || homeTeam == null)
                {
                    days.AddRange(settingsDays);
                }
                if ((homeTeam != null && homeTeam.IsReligiousTeam) || (guestTeam != null && guestTeam.IsReligiousTeam))
                {
                    days = days.Where(num => num != 6).ToList();
                }
                game.PossibleDaysForGame = days.ToArray();
                roundsList.ElementAt(roundIndex).Add(groupGames.ElementAt(i));
            }

            
            var thisRoundStartDate = settings.CyclesStartDate.Count > 0 ? settings.CyclesStartDate.FirstOrDefault() : roundDate;
            if (!isDivision)
            {
                thisRoundStartDate = roundDate;
            }

            for (var i = 0; i < roundsList.Count; i++)
            {
                var round = roundsList.ElementAt(i);
                Dictionary<int,List<GamesCycle>> schedulerList = new Dictionary<int,List<GamesCycle>>();
                Dictionary<int, int> gamesCountInEachDay = new Dictionary<int, int>();
                List<GamesCycle> gamesWithMoreThanOneDayPossible = new List<GamesCycle>();

                for (int j = 0; j < 7; j++)
                {
                    schedulerList.Add(j,new List<GamesCycle>());
                    gamesCountInEachDay.Add(j,0);
                }

                foreach (var game in round)
                {

                    game.CycleNum = i;
                    var isTennisGame = sectionAlias == GamesAlias.Tennis ? true : false;

                    if (game.PossibleDaysForGame.Count() == 1) // if there is only one day by settings/host can host this game
                    {

                        TimeSpan ts = new TimeSpan(19, 30, 0);
                        if (game.PossibleDaysForGame.ElementAt(0) == (int)DayOfWeek.Friday)
                        {
                            ts = new TimeSpan(13, 30, 0);
                        }
                        if (game.PossibleDaysForGame.ElementAt(0) == (int)DayOfWeek.Saturday)
                        {
                            ts = new TimeSpan(10, 0, 0);
                        }


                        var newDate = isTennisGame ? GetNextWeekDay(thisRoundStartDate, game.PossibleDaysForGame.ElementAt(0)).Date + ts : GetNextWeekDay(thisRoundStartDate, game.PossibleDaysForGame.ElementAt(0));
                        game.StartDate = newDate;

                        int value;
                        var isDefined = gamesCountInEachDay.TryGetValue(game.PossibleDaysForGame.ElementAt(0), out value);
                        if (isDefined)
                        {
                            value++;
                            gamesCountInEachDay.Remove(game.PossibleDaysForGame.ElementAt(0));
                            gamesCountInEachDay.Add(game.PossibleDaysForGame.ElementAt(0), value);
                        }
                    }
                    else if (game.PossibleDaysForGame.Count() == 0) // if no day specified neither by host nor by settings (only possible by religous and saturday on settings only) select tuesday as requested.
                    {
                        TimeSpan ts = new TimeSpan(19, 30, 0);
                        var rawDate = GetNextWeekDay(thisRoundStartDate, 2);
                        if (rawDate.DayOfWeek == DayOfWeek.Friday)
                        {
                            ts = new TimeSpan(13, 30, 0);
                        }
                        if (rawDate.DayOfWeek == DayOfWeek.Saturday)
                        {
                            ts = new TimeSpan(10, 0, 0);
                        }
                        var newDate = isTennisGame ? rawDate.Date + ts : rawDate;

                        game.StartDate = newDate;
                        int value;
                        var isDefined = gamesCountInEachDay.TryGetValue(2, out value);
                        if (isDefined)
                        {
                            value++;
                            gamesCountInEachDay.Remove(2);
                            gamesCountInEachDay.Add(2, value);
                        }
                    }
                    else
                    {
                        gamesWithMoreThanOneDayPossible.Add(game);
                        foreach (var dayKey in game.PossibleDaysForGame)
                        {
                            schedulerList[dayKey].Add(game);
                        }
                    }

                }

                gamesWithMoreThanOneDayPossible = gamesWithMoreThanOneDayPossible.OrderBy(g => g.PossibleDaysForGame.Count()).ToList();

                foreach (var game in gamesWithMoreThanOneDayPossible)
                {
                    var isTennisGame = sectionAlias == GamesAlias.Tennis ? true : false;
                    var minGamesonDayFound = 1000;
                    var dayWithMinGames = -1;
                    foreach (var day in game.PossibleDaysForGame)
                    {
                        int value;
                        var isDefined = gamesCountInEachDay.TryGetValue(day, out value);
                        if (isDefined)
                        {
                            if(value < minGamesonDayFound)
                            {
                                dayWithMinGames = day;
                                minGamesonDayFound = value;
                            }
                        }
                    }
                    if(dayWithMinGames >= 0)
                    {

                        TimeSpan ts = new TimeSpan(19, 30, 0);
                        if (dayWithMinGames == (int)DayOfWeek.Friday)
                        {
                            ts = new TimeSpan(13, 30, 0);
                        }
                        if (dayWithMinGames == (int)DayOfWeek.Saturday)
                        {
                            ts = new TimeSpan(10, 0, 0);
                        }
                        var newDate = isTennisGame ? GetNextWeekDay(thisRoundStartDate, dayWithMinGames).Date + ts : GetNextWeekDay(thisRoundStartDate, dayWithMinGames);
                        game.StartDate = newDate;

                        int value;
                        var isDefined = gamesCountInEachDay.TryGetValue(dayWithMinGames, out value);
                        if (isDefined)
                        {
                            value++;
                            gamesCountInEachDay.Remove(dayWithMinGames);
                            gamesCountInEachDay.Add(dayWithMinGames, value);
                        }

                    }
                    else
                    {
                        //TODO Exception, shouldnt reach this point.
                    }
                }

                if (isDivision || countOfRounds > 1)
                {
                    if (!isDivision)
                    {
                        playoffStagesPassed++;
                    }
                }

                if (!isDivision)
                {
                    thisRoundStartDate = (stagesPassed + i + 1) % settings.ActiveWeeksNumber == 0
                        ? thisRoundStartDate.AddDays(7 + 7 * settings.BreakWeeksNumber)
                        : thisRoundStartDate.AddDays(7);
                }
                else
                {
                    if (i + 1 < settings.CyclesStartDate.Count())
                    {
                        thisRoundStartDate = settings.CyclesStartDate.ElementAtOrDefault(i+1);
                    }
                    else
                    {
                        thisRoundStartDate = (i + 1) % settings.ActiveWeeksNumber == 0
                            ? thisRoundStartDate.AddDays(7 + 7 * settings.BreakWeeksNumber)
                            : thisRoundStartDate.AddDays(7);
                    }

                }

            }

            if (!isDivision && countOfRounds > 1) { settings.ActualStartDate = thisRoundStartDate; }
        }

        private void ScheduleTennisGames(GameSettings settings, int numberOfGamesInRound, int totalNumberOfRoundes, List<TennisGameCycle> groupGames,
            out int playoffStagesPassed, bool isDivision = false, int countOfRounds = 0, int stagesPassed = 0)
        {
            var settingsDays = settings.SettingsDays;
            var gameIndex = 0;
            var roundDate = settings.ActualStartDate;
            playoffStagesPassed = 0;
            var numberOfDaysLeft = settingsDays.Length;
            for (var r = 0; r < totalNumberOfRoundes; r++)
            {
                if (gameIndex >= groupGames.Count)
                {
                    break;
                }
                var gameDate = roundDate;
                while (gameIndex / (r + 1) < numberOfGamesInRound)
                {
                    if ((r > 0 || stagesPassed > 0) && settings.IsDateForFirstCycleOnly)
                    {
                        gameDate = SqlDateTime.MinValue.Value;
                    }
                    if (gameIndex >= groupGames.Count)
                    {
                        break;
                    }

                    var stillNeedToAddThisRound = numberOfGamesInRound * (r + 1) - gameIndex;
                    var numberOfGamesInDay = stillNeedToAddThisRound / numberOfDaysLeft;
                    if (stillNeedToAddThisRound > numberOfGamesInDay * numberOfDaysLeft)
                    {
                        numberOfGamesInDay++;
                    }

                    for (var d = 0; d < numberOfGamesInDay; d++)
                    {
                        if (gameIndex < groupGames.Count)
                        {
                            var gameCycle = groupGames.ElementAt(gameIndex);
                            gameCycle.CycleNum = r;
                            gameCycle.StartDate = gameDate;
                            gameIndex++;
                        }
                    }

                    var currentDayIndex = Array.IndexOf(settingsDays, (int)gameDate.DayOfWeek);
                    var nextDayIndex = (currentDayIndex + 1) % settingsDays.Length;
                    gameDate = GetNextWeekday(gameDate, settingsDays[nextDayIndex]);
                    numberOfDaysLeft--;
                }
                if (isDivision || countOfRounds > 1)
                {
                    if (!isDivision)
                    {
                        roundDate = (stagesPassed + r + 1) % settings.ActiveWeeksNumber == 0
                            ? GetNextWeekday(roundDate.AddDays(1 + 7 * settings.BreakWeeksNumber), settingsDays[0])
                            : GetNextWeekday(roundDate.AddDays(7), settingsDays[0]);
                        playoffStagesPassed++;
                    }
                    else
                    {
                        roundDate = (r + 1) % settings.ActiveWeeksNumber == 0
                            ? GetNextWeekday(roundDate.AddDays(1 + 7 * settings.BreakWeeksNumber), settingsDays[0])
                            : GetNextWeekday(roundDate.AddDays(7), settingsDays[0]);
                    }

                }
                numberOfDaysLeft = settingsDays.Length;
            }
            if (!isDivision && countOfRounds > 1) { settings.ActualStartDate = roundDate; }
        }

        #endregion

        #region Private

        private static Dictionary<int, Competitor> GetGroupTeamsByPositions(Group group)
        {
            if (group.IsIndividual)
                return group.GroupsTeams.Where(gt => !(gt?.Team?.IsArchive ?? false)).OrderBy(gt => gt.Pos)
                    .ToDictionary(gt => gt.Pos, gt => new Competitor { Athlete = gt.TeamsPlayer });
            else
                return group.GroupsTeams.Where(gt => !(gt?.Team?.IsArchive ?? false)).OrderBy(gt => gt.Pos)
                    .ToDictionary(gt => gt.Pos, gt => new Competitor { Team = gt.Team });
        }

        private static Dictionary<int, Competitor> GetTennisGroupTeamsByPositions(TennisGroup group)
        {
            return group.TennisGroupTeams.Where(gt => (gt?.TeamsPlayer?.IsActive ?? true)).OrderBy(gt => gt.Pos)
                .ToDictionary(gt => gt.Pos, gt => new Competitor { Athlete = gt.TeamsPlayer, PairAthlete = gt.TeamsPlayer1 });
        }

        //  Returns the nearest bigger or equal power of 2, starting from 4
        private int GetPlayoffTeamCount(int count)
        {
            Func<int, int> powerOfTwo = (x) =>
            {
                var pow = 4;
                while (pow < count)
                {
                    pow <<= 1;
                }
                return pow;
            };
            return count < 4 ? 4 //  4 is the least 
                : ((count & (count - 1)) == 0 ? count : // if it is already a power of 2, then return itself
                    powerOfTwo(count)); //  Return the nearest bigger power of 2
        }

        public List<Tuple<int, int>> CreatePlayoffCombinations(List<int> listTeam)
        {
            var ligitNumber = GetPlayoffTeamCount(listTeam.Count);
            for (var i = listTeam.Count; i < ligitNumber; i++)
            {
                listTeam.Add(0);
            }
            var numTeams = listTeam.Count;
            var resList = new List<Tuple<int, int>>();
            if (numTeams % 2 != 0)
            {
                return resList;
            }

            for (var i = 0; i < numTeams / 2; i++)
            {
                var t = Tuple.Create(listTeam[i], listTeam[numTeams - 1 - i]);
                resList.Add(t);
            }
            
            if(listTeam.Count == 64)
            {
                Swap(resList, 1, 5);   // 63-2  <=> 7-58
                Swap(resList, 26, 30); // 33-22 <=> 18-47
                Swap(resList, 17, 21); // 18-47 <=> 31-33
                Swap(resList, 10, 14); // 16-49 <=> 52-14
                Swap(resList, 9, 13);
                Swap(resList, 22, 18);
                Swap(resList, 29, 25);
                Swap(resList, 6, 5);
                Swap(resList, 29, 26);
                Swap(resList, 18, 30);
                Swap(resList, 13, 10);
                Swap(resList, 2, 6);
                Swap(resList, 30, 21);
                Swap(resList, 30, 18);
                Swap(resList, 4, 3);
                Swap(resList, 27, 28);
                Swap(resList, 20, 19);
                Swap(resList, 11, 12);
            }
            
            return resList;
        }

        private List<Tuple<int, int, int>> CreateCombinationCycles(int numberOfCycles, List<Tuple<int, int>> combinations)
        {
            var result = new List<Tuple<int, int, int>>();
            for (var i = 1; i <= numberOfCycles; i++)
            {
                foreach (var comb in combinations)
                {
                    if (i % 2 == 0)
                    {
                        result.Add(Tuple.Create(comb.Item2, comb.Item1, i));
                    }
                    else
                    {
                        result.Add(new Tuple<int, int, int>(comb.Item1, comb.Item2, i));
                    }
                }
            }
            return result;
        }



        private List<GamesCycle> CreateGamesFromCombinations(List<Tuple<int, int, int>> combinations, Dictionary<int, Competitor> teamPositions, int? seasonId)
        {
            List<Tuple<int, int>> repititive = new List<Tuple<int, int>>();
            int maxRepititionsAdjustments = combinations.Count();
            int adjustments = 0;
            startAgain:
            repititive = new List<Tuple<int, int>>();
            foreach (var team in teamPositions)
            {
                if (team.Value.Team != null) {
                    var clubId = team.Value.Team.ClubTeams.FirstOrDefault(x => x.SeasonId == seasonId)?.ClubId;
                    if (clubId.HasValue)
                    {
                        var clubsSimilar = teamPositions.Where(x => x.Value.Team.ClubTeams.FirstOrDefault(ct => ct.SeasonId == seasonId)?.ClubId == clubId).ToList();
                        if (clubsSimilar.Count() > 1)
                        {
                            var isAddedAlready = repititive.FirstOrDefault(x => x.Item1 == clubsSimilar[0].Key && x.Item2 == clubsSimilar[1].Key) != null;
                            if (!isAddedAlready)
                            {
                                repititive.Add(Tuple.Create(clubsSimilar[0].Key, clubsSimilar[1].Key));
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < repititive.Count(); i++)
            {
                var reptit = repititive.ElementAtOrDefault(i);
                var neededVs = -1;
                var repetiteToSwap = reptit.Item2;
                for (int j = 0; j < combinations.Count()/teamPositions.Count(); j++)
                {
                    var comb = combinations.ElementAt(j);
                    if (comb.Item1 == reptit.Item1 && comb.Item2 == reptit.Item2)
                    {
                        neededVs = -2;
                        break;
                    }
                    if (comb.Item1 == reptit.Item2 && comb.Item2 == reptit.Item1)
                    {
                        neededVs = -2;
                        break;
                    }
                    if (comb.Item1 == reptit.Item1)
                    {
                        neededVs = comb.Item2;
                    }
                    if (comb.Item2 == reptit.Item1)
                    {
                        neededVs = comb.Item1;
                    }
                    if (neededVs > -1)
                    {
                        break;
                    }
                }
                if (neededVs == -1)
                {
                    repetiteToSwap = reptit.Item1;
                    for (int j = 0; j < combinations.Count() / teamPositions.Count(); j++)
                    {
                        var comb = combinations.ElementAt(j);
                        if (comb.Item1 == reptit.Item2)
                        {
                            neededVs = comb.Item2;
                        }
                        if (comb.Item2 == reptit.Item2)
                        {
                            neededVs = comb.Item1;
                        }
                        if (neededVs > -1)
                        {
                            break;
                        }
                    }
                }
                if (neededVs > -1)
                {
                    Swap(teamPositions, repetiteToSwap, neededVs);
                    adjustments++;
                    if (adjustments < maxRepititionsAdjustments)
                    {
                        goto startAgain;
                    }
                }
            }
           

            var groupGames = new List<GamesCycle>();
            foreach (var comb in combinations)
            {
                var item = new GamesCycle
                {
                    HomeTeamPos = comb.Item1 > 0 ? (int?)comb.Item1 : null,
                    GuestTeamPos = comb.Item2 > 0 ? (int?)comb.Item2 : null,
                    HomeTeamId = teamPositions[comb.Item1]?.Team?.TeamId,
                    GuestTeamId = teamPositions[comb.Item2]?.Team?.TeamId,
                    HomeAthleteId = teamPositions[comb.Item1]?.Athlete?.Id,
                    GuestAthleteId = teamPositions[comb.Item2]?.Athlete?.Id,
                    RoundNum = comb.Item3
                };
                groupGames.Add(item);
            }
            return groupGames;
        }

        private List<TennisGameCycle> CreateTennisGamesFromCombinations(List<Tuple<int, int, int>> combinations, Dictionary<int, Competitor> teamPositions)
        {
            var groupGames = new List<TennisGameCycle>();
            foreach (var comb in combinations)
            {

                    var item = new TennisGameCycle
                    {
                        FirstPlayerPos = comb.Item1 > 0 ? (int?)comb.Item1 : null,
                        SecondPlayerPos = comb.Item2 > 0 ? (int?)comb.Item2 : null,
                        FirstPlayerId = teamPositions[comb.Item1]?.Athlete?.Id,
                        FirstPlayerPairId = teamPositions[comb.Item1]?.PairAthlete?.Id,
                        SecondPlayerId = teamPositions[comb.Item2]?.Athlete?.Id,
                        SecondPlayerPairId = teamPositions[comb.Item2]?.PairAthlete?.Id,
                        RoundNum = comb.Item3
                    };
                    groupGames.Add(item);
                
            }
            return groupGames;
        }

        private GameSettings GetGameSettings(Game game)
        {
            //Set Real Start Day for this Stage
            var actualStartDate = game.StartDate;

            // days list 0,1,2
            var settingsDays = (game.GameDays).Split(',').Select(int.Parse).ToArray();

            while (!settingsDays.Contains((int)actualStartDate.DayOfWeek))
            {
                actualStartDate = actualStartDate.AddDays(1);
            }

            var cyclesStartDate = game.CyclesStartDate ?? "";
            var cyclesStartDateList = cyclesStartDate.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => DateTime.Parse(x)).ToList();

            var settings = new GameSettings
            {
                ActualStartDate = actualStartDate,
                SettingsDays = settingsDays,
                ActiveWeeksNumber = game.ActiveWeeksNumber,
                BreakWeeksNumber = game.BreakWeeksNumber,
                CyclesStartDate = cyclesStartDateList
            };

            return settings;
        }

        private GameSettings GetTennisGameSettings(TennisGame game)
        {
            //Set Real Start Day for this Stage
            var actualStartDate = game.StartDate;

            // days list 0,1,2
            var settingsDays = (game.GameDays).Split(',').Select(int.Parse).ToArray();

            while (!settingsDays.Contains((int)actualStartDate.DayOfWeek))
            {
                actualStartDate = actualStartDate.AddDays(1);
            }


            var settings = new GameSettings
            {
                ActualStartDate = actualStartDate,
                SettingsDays = settingsDays,
                ActiveWeeksNumber = game.ActiveWeeksNumber,
                BreakWeeksNumber = game.BreakWeeksNumber,
                IsDateForFirstCycleOnly = game.IsDateForFirstCycleOnly ?? false
            };

            return settings;
        }

        private int GetNumberOfRoundsInOneCycle(int gamesType, int numberOfTeams)
        {
            var numberOfRoundsInOneCycle = 0;
            if (gamesType == 1)
            {
                //League
                numberOfRoundsInOneCycle = numberOfTeams - 1;
                if (numberOfTeams % 2 != 0)
                {
                    numberOfRoundsInOneCycle++;
                }

            }
            else if (gamesType == GameTypeId.Playoff || gamesType == GameTypeId.Knockout || gamesType == GameTypeId.Knockout34 || gamesType == GameTypeId.Knockout34Consolences1Round || gamesType == GameTypeId.Knockout34ConsolencesQuarterRound)
            {
                //Playoffs OR Knockout
                numberOfRoundsInOneCycle = numberOfTeams / 2;
            }
            return numberOfRoundsInOneCycle;
        }

        private void UpdateReferee(List<GamesCycle> gamesList, int leagueId, int unionId)
        {

            var uRepo = new UsersRepo();
            var referees = uRepo.GetUnionAndLeagueReferees(unionId, leagueId).OrderBy(t => Guid.NewGuid()).ToList();
            if (referees.Count() > 0)
            {
                var refereeIndex = 0;
                foreach (var gc in gamesList.OrderBy(g => g.StartDate))
                {
                    if (refereeIndex < 0 || refereeIndex >= referees.Count())
                    {
                        refereeIndex = 0;
                    }
                    gc.RefereeIds += referees.ElementAt(refereeIndex).UserId;
                    refereeIndex++;
                }
            }
        }

        private void UpdateAuditorium(List<GamesCycle> gamesList)
        {

            foreach (var gc in gamesList)
            {
                var auditoriumId = teamsRepo.GetMainOrFirstAuditoriumForTeam(gc.HomeTeamId);
                gc.AuditoriumId = auditoriumId;
            }
        }

        private void UpdateField(List<TennisGameCycle> gamesList)
        {

            foreach (var gc in gamesList)
            {
                var tennisStage = teamsRepo.GetTennisStage(gc.StageId);
                var auditoriumId = teamsRepo.GetMainOrFirstAuditoriumForTeam(tennisStage.CategoryId);
                gc.FieldId = auditoriumId;
            }
        }

        private void SaveGamesList(List<GamesCycle> gamesList)
        {
            gamesRepo.SaveGames(gamesList);
        }

        private void SaveTennisGamesList(List<TennisGameCycle> gamesList)
        {
            gamesRepo.SaveTennisGames(gamesList);
        }

        public static DateTime GetNextWeekday(DateTime start, int day)
        {
            var daysToAdd = (day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        public void ValidateTimePlaceContradictions(List<GamesCycle> gamesList, TimeSpan gamesInterval)
        {
            gamesRepo.ValidateTimePlaceContradictions(gamesList, gamesInterval);
        }

        public void ValidateTennisTimePlaceContradictions(List<TennisGameCycle> gamesList, TimeSpan gamesInterval)
        {
            gamesRepo.ValidateTennisTimePlaceContradictions(gamesList, gamesInterval);
        }

        private int GetWeekOfYear(DateTime date)
        {
            var cal = System.Globalization.DateTimeFormatInfo.CurrentInfo.Calendar;
            return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, System.DayOfWeek.Sunday);
        }

        #endregion

        #region Actions

        public void MoveCycles(int stageId, int cycleNum, DateTime newDate, bool allAfter)
        {
            var stage = stagesRepo.GetById(stageId);
            var game = stage.League.Games.FirstOrDefault();

            var allGameCycles = stage.GamesCycles.OrderBy(g => g.StartDate).ToList();
            var currentCycles = allGameCycles.Where(g => g.CycleNum == cycleNum).ToList();

            var firstGameOfCurrentCycle = currentCycles.FirstOrDefault();
            if (firstGameOfCurrentCycle == null)
            {
                return;
            }

            var startWeek = GetWeekOfYear(firstGameOfCurrentCycle.StartDate);
            var newWeek = GetWeekOfYear(newDate);
            var weeksDiff = newWeek - startWeek;

            foreach (var gameC in currentCycles)
            {
                gameC.StartDate = newDate;
            }

            if (allAfter && weeksDiff != 0)
            {
                var nextCycles = allGameCycles.Where(g => g.CycleNum > cycleNum).ToList();
                if (nextCycles.Count > 0)
                {
                    foreach (var gameC in nextCycles)
                    {
                        gameC.StartDate = gameC.StartDate.AddDays(weeksDiff * 7);
                    }
                }
            }
            stagesRepo.Save();
        }

        public void MoveTennisCycles(int stageId, int cycleNum, DateTime newDate, bool allAfter)
        {
            var stage = stagesRepo.GetTennisStageById(stageId);
            var game = stage.TennisGames.FirstOrDefault();

            var allGameCycles = stage.TennisGameCycles.OrderBy(g => g.StartDate).ToList();
            var currentCycles = allGameCycles.Where(g => g.CycleNum == cycleNum).ToList();

            var firstGameOfCurrentCycle = currentCycles.FirstOrDefault();
            if (firstGameOfCurrentCycle == null)
            {
                return;
            }

            var startWeek = GetWeekOfYear(firstGameOfCurrentCycle.StartDate);
            var newWeek = GetWeekOfYear(newDate);
            var weeksDiff = newWeek - startWeek;

            foreach (var gameC in currentCycles)
            {
                gameC.StartDate = newDate;
            }

            if (allAfter && weeksDiff != 0)
            {
                var nextCycles = allGameCycles.Where(g => g.CycleNum > cycleNum).ToList();
                if (nextCycles.Count > 0)
                {
                    foreach (var gameC in nextCycles)
                    {
                        gameC.StartDate = gameC.StartDate.AddDays(weeksDiff * 7);
                    }
                }
            }
            stagesRepo.Save();
        }
        public void AddGame(GamesCycle item)
        {
            gamesRepo.AddGame(item);
        }

        #endregion

        #region Helper classes
        private class GameSettings
        {
            public DateTime ActualStartDate { get; set; }
            public int[] SettingsDays { get; set; }
            public int ActiveWeeksNumber { get; set; }
            public int BreakWeeksNumber { get; set; }
            public List<DateTime> CyclesStartDate { get; set; }
            public bool IsDateForFirstCycleOnly { get; set; }
        }
        #endregion
    }
}
