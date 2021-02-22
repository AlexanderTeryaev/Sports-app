using AppModel;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DataService.DTO;
using System;

namespace DataService
{
    public class BracketsRepo : BaseRepo
    {
        private GamesRepo _gameRepo;

        protected GamesRepo gameRepo
        {
            get
            {
                if (_gameRepo == null)
                {
                    _gameRepo = new GamesRepo(db);
                }
                return _gameRepo;
            }
        }
        public BracketsRepo() : base() { }

        public BracketsRepo(DataEntities db) : base(db) { }

        public BracketsRepo(DataEntities db, GamesRepo gRepo) : base(db)
        {
            _gameRepo = gRepo;
        }

        internal void SaveBrackets(List<PlayoffBracket> brackets)
        {
            db.PlayoffBrackets.AddRange(brackets);
            db.SaveChanges();
        }

        internal void SaveTennisBrackets(List<TennisPlayoffBracket> brackets)
        {
            db.TennisPlayoffBrackets.AddRange(brackets);
            db.SaveChanges();
        }

        internal void SaveTennisBracket(TennisPlayoffBracket bracket)
        {
            db.TennisPlayoffBrackets.Add(bracket);
            db.SaveChanges();
        }

        internal void DeleteAllBracketsAndChildrenBracketsForGroup(Group group)
        {
            List<PlayoffBracket> allBrackets = new List<PlayoffBracket>();
            GetChildrenBracketsRecursively(group.PlayoffBrackets, allBrackets);
            allBrackets = allBrackets.Distinct().ToList();
            allBrackets.ForEach(b =>
            {
                b.ParentBracket1Id = null;
                b.ParentBracket2Id = null;
                db.GamesCycles.RemoveRange(b.GamesCycles);
            });
            int n = db.SaveChanges();
            db.PlayoffBrackets.RemoveRange(allBrackets);
            n = db.SaveChanges();
        }

        internal void DeleteAllTennisBracketsAndChildrenBracketsForGroup(TennisGroup group)
        {
            List<TennisPlayoffBracket> allBrackets = new List<TennisPlayoffBracket>();
            GetChildrenTennisBracketsRecursively(group.TennisPlayoffBrackets, allBrackets);
            allBrackets = allBrackets.Distinct().ToList();
            allBrackets.ForEach(b =>
            {
                b.ParentBracket1Id = null;
                b.ParentBracket2Id = null;
                db.TennisGameCycles.RemoveRange(b.TennisGameCycles);
            });
            int n = db.SaveChanges();
            db.TennisPlayoffBrackets.RemoveRange(allBrackets);
            n = db.SaveChanges();
        }

        private void GetChildrenBracketsRecursively(ICollection<PlayoffBracket> brackets, List<PlayoffBracket> allBrackets)
        {
            foreach (var bracket in brackets)
            {
                allBrackets.Add(bracket);
                GetChildrenBracketsRecursively(bracket.PlayoffBrackets1, allBrackets);
                GetChildrenBracketsRecursively(bracket.PlayoffBrackets11, allBrackets);
            }
        }

        private void GetChildrenTennisBracketsRecursively(ICollection<TennisPlayoffBracket> brackets, List<TennisPlayoffBracket> allBrackets)
        {
            foreach (var bracket in brackets)
            {
                allBrackets.Add(bracket);
                GetChildrenTennisBracketsRecursively(bracket.TennisPlayoffBrackets1, allBrackets);
                GetChildrenTennisBracketsRecursively(bracket.TennisPlayoffBrackets11, allBrackets);
            }
        }


        internal void GameEndedEvent(GamesCycle gc)
        {
            PlayoffBracket bracket = gc.PlayoffBracket;
            bool? isIndividual = bracket?.Group?.IsIndividual;
            if (bracket != null && bracket.GamesCycles.All(g => g.GameStatus == GameStatus.Ended))
            {
                var goldenGame = bracket.GamesCycles.FirstOrDefault(g => g.GameSets.Any(s => s.IsGoldenSet == true));
                var penaltyGame = bracket.GamesCycles.FirstOrDefault(g => g.GameSets.Any(s => s.IsPenalties == true));
                if (goldenGame != null)
                {
                    gameRepo.ResetGame(goldenGame, true, isIndividual);

                    var goldenSet = goldenGame.GameSets.FirstOrDefault(s => s.IsGoldenSet == true);
                    if (goldenSet.HomeTeamScore > goldenSet.GuestTeamScore)
                    {
                        if (isIndividual.HasValue && isIndividual == true)
                        {
                            bracket.WinnerAthleteId = goldenGame.HomeAthleteId;
                            bracket.WinnerAthleteId = goldenGame.GuestAthleteId;
                        }
                        else
                        {
                            bracket.WinnerId = goldenGame.HomeTeamId;
                            bracket.LoserId = goldenGame.GuestTeamId;
                        }
                    }
                    else
                    {
                        if (isIndividual.HasValue && isIndividual == true)
                        {
                            bracket.WinnerAthleteId = goldenGame.HomeAthleteId;
                            bracket.WinnerAthleteId = goldenGame.GuestAthleteId;
                        }
                        else
                        {
                            bracket.WinnerId = goldenGame.GuestTeamId;
                            bracket.LoserId = goldenGame.HomeTeamId;
                        }
                    }
                }
                else if (penaltyGame != null)
                {
                    var penaltyGameSet = penaltyGame.GameSets.FirstOrDefault(s => s.IsPenalties == true);
                    if (penaltyGameSet.HomeTeamScore > penaltyGameSet.GuestTeamScore)
                    {
                        bracket.WinnerId = penaltyGame.HomeTeamId;
                        bracket.LoserId = penaltyGame.GuestTeamId;
                    }
                    else
                    {
                        bracket.WinnerId = penaltyGame.GuestTeamId;
                        bracket.LoserId = penaltyGame.HomeTeamId;
                    }
                }
                else
                {
                    var sectionAlias = bracket?.Group?.Stage?.League?.Club?.Union?.Section?.Alias
                            ?? bracket?.Group?.Stage?.League?.Union?.Section?.Alias
                            ?? bracket?.Group?.Stage?.League?.Club?.Section?.Alias;
                    int t1score, t2score;
                    if (!string.Equals(sectionAlias, GamesAlias.VolleyBall, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(sectionAlias, GamesAlias.Tennis, StringComparison.OrdinalIgnoreCase))
                    {
                        t1score = gc.GameSets.Sum(c => c.HomeTeamScore);
                        t2score = gc.GameSets.Sum(c => c.GuestTeamScore);
                    }
                    else
                    {
                        if (isIndividual.HasValue && isIndividual == true)
                        {
                            t1score = bracket.GamesCycles.Where(g => g.HomeAthleteId == bracket.TeamsPlayer?.Id && g.HomeTeamScore > g.GuestTeamScore)
                            .Concat(bracket.GamesCycles.Where(g => g.GuestAthleteId == bracket.TeamsPlayer?.Id && g.HomeTeamScore < g.GuestTeamScore)).Count();

                            t2score = bracket.GamesCycles.Where(g => g.HomeAthleteId == bracket.TeamsPlayer1?.Id && g.HomeTeamScore > g.GuestTeamScore)
                            .Concat(bracket.GamesCycles.Where(g => g.GuestAthleteId == bracket.TeamsPlayer1?.Id && g.HomeTeamScore < g.GuestTeamScore)).Count();
                        }
                        else
                        {
                            t1score = bracket.GamesCycles.Where(g => g.HomeTeamId == bracket.FirstTeam?.TeamId && g.HomeTeamScore > g.GuestTeamScore)
                            .Concat(bracket.GamesCycles.Where(g => g.GuestTeamId == bracket.FirstTeam?.TeamId && g.HomeTeamScore < g.GuestTeamScore))
                            .Concat(bracket.GamesCycles.Where(g => g.HomeTeamScore == g.GuestTeamScore)).Count();

                            t2score = bracket.GamesCycles.Where(g => g.HomeTeamId == bracket.SecondTeam?.TeamId && g.HomeTeamScore > g.GuestTeamScore)
                            .Concat(bracket.GamesCycles.Where(g => g.GuestTeamId == bracket.SecondTeam?.TeamId && g.HomeTeamScore < g.GuestTeamScore))
                            .Concat(bracket.GamesCycles.Where(g => g.HomeTeamScore == g.GuestTeamScore)).Count();
                        }
                    }

                    if (string.Equals(sectionAlias, GamesAlias.NetBall, StringComparison.OrdinalIgnoreCase))
                    {
                        if (gc.HomeTeamScore != gc.GuestTeamScore)
                        {
                            t1score = gc.HomeTeamScore;
                            t2score = gc.GuestTeamScore;
                        }
                    }

                    if (!string.Equals(sectionAlias, GamesAlias.VolleyBall, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(sectionAlias, GamesAlias.Tennis, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(sectionAlias, GamesAlias.NetBall, StringComparison.OrdinalIgnoreCase))
                    {
                        var gcList = bracket.GamesCycles.Where(c => c.CycleId != gc.CycleId).ToList();
                        gcList.Add(gc);
                        int homeWins = 0;
                        int guestWins = 0;
                        int homeTotalPoints = 0;
                        int guestTotalPoints = 0;
                        foreach (var gameCycle in gcList)
                        {
                            var gct1score = gameCycle.GameSets.Sum(c => c.HomeTeamScore);
                            var gct2score = gameCycle.GameSets.Sum(c => c.GuestTeamScore);
                            if (gct1score > gct2score)
                            {
                                if((gc.HomeTeamId != null && gc.HomeTeamId == gameCycle.HomeTeamId) || (gc.HomeAthleteId != null && gc.HomeAthleteId == gameCycle.HomeAthleteId))
                                {
                                    homeWins += 1;
                                    homeTotalPoints += gct1score;
                                    guestTotalPoints += gct2score;
                                }
                                else
                                {
                                    guestWins += 1;
                                    guestTotalPoints += gct1score;
                                    homeTotalPoints += gct2score;

                                }
                                
                            }
                            if (gct1score < gct2score)
                            {
                                if ((gc.HomeTeamId != null && gc.HomeTeamId == gameCycle.HomeTeamId) || (gc.HomeAthleteId != null && gc.HomeAthleteId == gameCycle.HomeAthleteId))
                                {
                                    guestWins += 1;
                                    guestTotalPoints += gct2score;
                                    homeTotalPoints += gct1score;
                                }
                                else
                                {
                                    homeWins += 1;
                                    homeTotalPoints += gct2score;
                                    guestTotalPoints += gct1score;
                                }
                            }
                        }
                        if (homeWins != guestWins)
                        {
                            t1score = homeWins;
                            t2score = guestWins;
                        }
                        else
                        {
                            t1score = homeTotalPoints;
                            t2score = guestTotalPoints;
                        }
                    }

                    if (t1score > t2score)
                    {
                        if (isIndividual.HasValue && isIndividual == true)
                        {
                            bracket.WinnerAthleteId = bracket.TeamsPlayer.Id;
                            bracket.LoserAthleteId = bracket.TeamsPlayer1.Id;
                        }
                        else
                        {
                            bracket.WinnerId = gc.HomeTeamId;
                            bracket.LoserId = gc.GuestTeamId;
                        }
                    }
                    else if (t1score < t2score)
                    {
                        if (isIndividual.HasValue && isIndividual == true)
                        {
                            bracket.WinnerAthleteId = bracket.TeamsPlayer1.Id;
                            bracket.LoserAthleteId = bracket.TeamsPlayer.Id;
                        }
                        else
                        {
                            bracket.WinnerId = gc.GuestTeamId;
                            bracket.LoserId = gc.HomeTeamId;
                        }
                    }
                }


                foreach (var child in bracket.ChildrenSide1)
                {
                    if (child.Type == (int)PlayoffBracketType.Winner)
                    {
                        if (isIndividual.HasValue && isIndividual == true)
                            child.Athlete1Id = bracket.WinnerAthleteId;
                        else
                            child.Team1Id = bracket.WinnerId;
                    }
                    else if (child.Type == (int)PlayoffBracketType.Loseer || child.Type == (int)PlayoffBracketType.Condolence3rdPlaceBracket)
                    {
                        if (isIndividual.HasValue && isIndividual == true)
                            child.Athlete1Id = bracket.LoserAthleteId;
                        else
                            child.Team1Id = bracket.LoserId;
                    }

                    for (int i = 0; i < child.GamesCycles.Count; i++)
                    {
                        GamesCycle game = child.GamesCycles.ElementAt(i);
                        gameRepo.ResetGame(game, true, isIndividual);
                        if (i % 2 == 0)
                        {
                            if (isIndividual.HasValue && isIndividual == true)
                            {
                                game.HomeAthleteId = child.Athlete1Id;
                                game.GuestAthleteId = child.Athlete2Id;
                            }
                            else
                            {
                                game.HomeTeamId = child.Team1Id;
                                game.GuestTeamId = child.Team2Id;
                            }
                        }
                        else
                        {
                            if (isIndividual.HasValue && isIndividual == true)
                            {
                                game.HomeAthleteId = child.Athlete2Id;
                                game.GuestAthleteId = child.Athlete1Id;
                            }
                            else
                            {
                                game.HomeTeamId = child.Team2Id;
                                game.GuestTeamId = child.Team1Id;
                            }
                        }
                    }
                }


                foreach (var child in bracket.ChildrenSide2)
                {
                    if (child.Type == (int)PlayoffBracketType.Winner)
                    {
                        if (isIndividual.HasValue && isIndividual == true)
                            child.Athlete2Id = bracket.WinnerAthleteId;
                        else
                            child.Team2Id = bracket.WinnerId;
                    }
                    else if (child.Type == (int)PlayoffBracketType.Loseer || child.Type == (int)PlayoffBracketType.Condolence3rdPlaceBracket)
                    {
                        if (isIndividual.HasValue && isIndividual == true)
                            child.Athlete2Id = bracket.LoserAthleteId;
                        else
                            child.Team2Id = bracket.LoserId;
                    }
                    for (int i = 0; i < child.GamesCycles.Count; i++)
                    {
                        GamesCycle game = child.GamesCycles.ElementAt(i);
                        gameRepo.ResetGame(game, true, isIndividual);
                        if (i % 2 == 0)
                        {
                            if (isIndividual.HasValue && isIndividual == true)
                            {
                                game.HomeAthleteId = child.Athlete1Id;
                                game.GuestAthleteId = child.Athlete2Id;
                            }
                            else
                            {
                                game.HomeTeamId = child.Team1Id;
                                game.GuestTeamId = child.Team2Id;
                            }
                        }
                        else
                        {
                            if (isIndividual.HasValue && isIndividual == true)
                            {
                                game.HomeAthleteId = child.Athlete2Id;
                                game.GuestTeamId = child.Athlete1Id;
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
            Save();
        }

        internal void GameEndedEventForBasketballApp(GamesCycle gc)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Titled> GetAllPotintialTeams(int id, int index)
        {
            List<Titled> list = new List<Titled>();
            var bracket = GetById(id);
            FindTeamsRecursively(bracket, list, index);
            return list.Distinct().OrderBy(t => t.Title).ToList();
        }

        private void FindTeamsRecursively(PlayoffBracket bracket, List<Titled> list, int index)
        {
            if (bracket.Type == (int)PlayoffBracketType.Root)
            {
                if (bracket.Team1Id != null)
                {
                    list.Add(new Titled(bracket.FirstTeam.Title));
                }
                else if (bracket.Athlete1Id != null)
                {
                    var player = db.TeamsPlayers
                        .Include(p => p.User)
                        .SingleOrDefault(p => p.Id == bracket.Athlete1Id);
                    var title = player?.User?.FullName;
                    list.Add(new Titled(title));
                }
                else if (bracket.Team1GroupPosition != 0)
                {
                    var title = "Position #" + bracket.Team1GroupPosition;
                    if (list.All(t => t.Title != title))
                        list.Add(new Titled(title));
                }

                if (bracket.Team2Id != null)
                {
                    list.Add(new Titled(bracket.SecondTeam.Title));
                }
                else if (bracket.Athlete2Id != null)
                {
                    var player = db.TeamsPlayers
                        .Include(p => p.User)
                        .SingleOrDefault(p => p.Id == bracket.Athlete2Id);
                    var title = player?.User?.FullName;
                    list.Add(new Titled(title));
                }
                else if (bracket.Team2GroupPosition != 0)
                {
                    var title = "Position #" + bracket.Team2GroupPosition;
                    if (list.All(t => t.Title != title))
                        list.Add(new Titled(title));
                }
            }
            else
            {
                var parent = index == 1 ? bracket.Parent1 : bracket.Parent2;
                if (parent.Team1Id != null)
                {
                    list.Add(new Titled(parent.FirstTeam.Title));
                }
                else if (parent.Athlete1Id != null)
                {
                    var player = db.TeamsPlayers
                        .Include(p => p.User)
                        .SingleOrDefault(p => p.Id == parent.Athlete1Id);
                    var title = player?.User?.FullName;
                    list.Add(new Titled(title));
                }
                else
                {
                    FindTeamsRecursively(parent, list, 1);
                }

                if (parent.Team2Id != null)
                {
                    list.Add(new Titled(parent.SecondTeam.Title));
                }
                else if (parent.Athlete2Id != null)
                {
                    var player = db.TeamsPlayers
                        .Include(p => p.User)
                        .SingleOrDefault(p => p.Id == parent.Athlete2Id);
                    var title = player?.User?.FullName;
                    list.Add(new Titled(title));
                }
                else
                {
                    FindTeamsRecursively(parent, list, 2);
                }
            }
        }

        public PlayoffBracket GetById(int id)
        {
            return db.PlayoffBrackets.Find(id);
        }

        public List<TennisPlayoffBracket> GetTennisBracketsStageAndPos(int groupId, int stage, int maxPos, int minPos)
        {
            return db.TennisPlayoffBrackets.Where(b => b.Stage == stage && b.MaxPos == maxPos && b.MinPos == minPos && b.GroupId == groupId).ToList();
        }


    }
}
