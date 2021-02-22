using AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataService
{
    public class PlayoffSchedulingService
    {
        List<Group> db = new List<Group>();
        private List<PlayoffGameBracket> Brackets;

        public void CreateFirstAdvancedStep(int groupId)
        {
            GroupsRepo grrepo = new GroupsRepo();
            Group group = grrepo.GetById(groupId);
            CreateBracketsFromGroup(group, 1);
            db.Add(group);
        }

        public void CreateBracketsFromGroup(Group group, int stage)
        {

            List<GamesCycle> groupGames = group.GamesCycles.ToList();
            var teams = group.GroupsTeams.OrderBy(gt => gt.Pos).Select(gt => gt.Team).ToList();

            if (group.GamesType.TypeId == 1)
            {


            }
            else if (group.GamesType.TypeId == 2 || group.GamesType.TypeId == 3)
            {
                this.Brackets = CreateBracketsFromEarlyPlayoffGames(groupGames, teams);
            }

            SetWinnersAndLosers(this.Brackets);
            db.Add(group);
            CreateNextStep(group);
           // PrintGroups();
        }    
        
        private void CreateNextStep(Group group)
        {
            int newGroupId = group.GroupId + 10;

            var parentBracketGroups = this.Brackets.GroupBy(b => new { b.StageNumber, b.MinPos, b.MaxPos });

            foreach (var parentBracketGroup in parentBracketGroups)
            {
                List<PlayoffGameBracket> winnerBrackets = new List<PlayoffGameBracket>();
                List<PlayoffGameBracket> loserBrackets = new List<PlayoffGameBracket>();
                var brackets = parentBracketGroup;
                List<int> winnerTeams = brackets.Select(b => b.Winner).Select(t => t.TeamId).ToList();
                List<Tuple<int, int>> gamePares = CreatePlayoffGameParesFromTeams(winnerTeams);
                int middelPos = ((parentBracketGroup.Key.MinPos - parentBracketGroup.Key.MaxPos) / 2) + parentBracketGroup.Key.MaxPos;

                foreach (var pare in gamePares)
                {
                    var parent1 = brackets.FirstOrDefault(b => b.Winner.TeamId == pare.Item1);
                    var parent2 = brackets.FirstOrDefault(b => b.Winner.TeamId == pare.Item2);

                    var winnerBracket = new PlayoffGameBracket();
                    //winnerBracket.GroupId = newGroupId;
                    winnerBracket.Team1 = parent1.Winner;
                    winnerBracket.Team2 = parent2.Winner;
                    winnerBracket.StageNumber = parentBracketGroup.Key.StageNumber + 1;
                    winnerBracket.MaxPos = parentBracketGroup.Key.MaxPos;
                    winnerBracket.MinPos = middelPos;
                    winnerBrackets.Add(winnerBracket);


                    var loserBracket = new PlayoffGameBracket();
                    loserBracket.Team1 = parent1.Loser;
                    loserBracket.Team2 = parent2.Loser;
                    //loserBracket.GroupId = newGroupId;
                    loserBracket.StageNumber = parentBracketGroup.Key.StageNumber + 1;
                    loserBracket.MaxPos = middelPos + 1;
                    loserBracket.MinPos = parentBracketGroup.Key.MinPos;
                    loserBrackets.Add(loserBracket);
                    //For Testing
                    //***************
                    winnerBracket.Winner = winnerBracket.Team1;
                    winnerBracket.Loser = winnerBracket.Team2;
                    loserBracket.Winner = loserBracket.Team1;
                    loserBracket.Loser = loserBracket.Team2;
                    //***************
                }

                


                /*
                db.AddRange(winnerBrackets);
                db.AddRange(loserBrackets);
                if (winnerBrackets.Count > 1 && loserBrackets.Count > 1)
                {
                    CreateNextStep(newGroupId);
                }*/
            }
        }

        //private void PrintGroups()
        //{
        //    var stages = db.GroupBy(g => new { g.GroupId, g.StageNumber, g.MaxPos, g.MinPos });
        //    foreach (var stage in stages)
        //    {
        //        System.Diagnostics.Debug.WriteLine("Plyoff Stage Number : " + stage.Key);
        //        var list = stage.OrderBy(s => s.StageNumber).ThenBy(s => s.MaxPos).ThenBy(s => s.MinPos);
        //        foreach (var group in list)
        //        {

        //            System.Diagnostics.Debug.WriteLine(group.Team1.Title + " vs. " + group.Team2.Title + " winner- " + group.Winner.Title);
        //            if (list.Count() == 1)
        //            {
        //                System.Diagnostics.Debug.WriteLine("Pos : " + group.MaxPos + " " + group.Winner.Title);
        //                System.Diagnostics.Debug.WriteLine("Pos : " + group.MinPos + " " + group.Loser.Title);
        //            }

        //        }
        //    }
        //}

        private void SetWinnersAndLosers(List<PlayoffGameBracket> brackets)
        {
            foreach (var bracket in brackets)
            {
                int t1score = bracket.Games.Where(g => g.HomeTeamId == bracket.Team1.TeamId && g.HomeTeamScore > g.GuestTeamScore)
                    .Concat(bracket.Games.Where(g => g.GuestTeamId == bracket.Team1.TeamId && g.HomeTeamScore < g.GuestTeamScore)).Count();
                int t2score = bracket.Games.Where(g => g.HomeTeamId == bracket.Team2.TeamId && g.HomeTeamScore > g.GuestTeamScore)
                    .Concat(bracket.Games.Where(g => g.GuestTeamId == bracket.Team2.TeamId && g.HomeTeamScore < g.GuestTeamScore)).Count(); ;
                if (t1score > t2score)
                {
                    bracket.Winner = bracket.Team1;
                    bracket.Loser = bracket.Team2;
                }
                else if (t1score < t2score)
                {
                    bracket.Winner = bracket.Team2;
                    bracket.Loser = bracket.Team1;
                }
            }
        }

    }
}
