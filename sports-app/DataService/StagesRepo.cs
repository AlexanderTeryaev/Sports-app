using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using AppModel;


namespace DataService
{
    public class StagesRepo : BaseRepo
    {

        public StagesRepo() : base()
        {

        }

        public StagesRepo(DataEntities db) : base(db)
        {
        }

        private IQueryable<Stage> GetQuery(int leagueId)
        {
            return db.Stages.Include(t => t.Groups).Where(t => t.IsArchive == false && t.LeagueId == leagueId);
        }

        private IQueryable<TennisStage> GetTennisQuery(int categoryId, int seasonId)
        {
            return db.TennisStages.Include(t => t.TennisGroups).Where(t => t.IsArchive == false && t.CategoryId == categoryId && t.SeasonId == seasonId);
        }

        public Stage Create(int leagueId)
        {
            int maxNum = GetQuery(leagueId).Max(t => (int?)t.Number) ?? 0;
            var s = new Stage();
            s.Number = maxNum + 1;
            s.LeagueId = leagueId;

            return db.Stages.Add(s);
        }

        public TennisStage CreateTennisStage(int categoryId, int seasonId)
        {
            int maxNum = GetTennisQuery(categoryId, seasonId).Max(t => (int?)t.Number) ?? 0;
            var s = new TennisStage();
            s.Number = maxNum + 1;
            s.CategoryId = categoryId;
            s.SeasonId = seasonId;
            //s.LeagueId = leagueId;
            return db.TennisStages.Add(s);
        }

        public List<Stage> GetAll(int leagueId, int? seasonId = null)
        {
            var results = GetQuery(leagueId)
                .Include(x => x.Groups)
                .Include(f => f.Groups.Select(x => x.GroupsTeams))
                .Include(f => f.Groups.Select(x => x.PlayoffBrackets))
                .ToList();

            if (seasonId.HasValue)
            {
                var teams = results.SelectMany(x => x.Groups).SelectMany(x => x.PlayoffBrackets).ToList();
                foreach (var team in teams)
                {
                    var firstTeamDetails = team.FirstTeam?.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId.Value);
                    if (firstTeamDetails != null)
                    {
                        team.FirstTeam.Title = firstTeamDetails.TeamName;
                    }

                    var secondTeamDetails = team.SecondTeam?.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId.Value);
                    if (secondTeamDetails != null)
                    {
                        team.SecondTeam.Title = secondTeamDetails.TeamName;
                    }
                }
            }

            return results;
        }

        public IEnumerable<TennisStage> GetAllTennisStages(int categoryId, int? seasonId = null)
        {
            /*if (seasonId.HasValue)
            {
                var results = GetTennisQuery(categoryId, seasonId.Value).Include(x => x.TennisGroups).ToList();

                var teams = results.SelectMany(x => x.TennisGroups).ToList();
                foreach (var team in teams)
                {
                    var firstTeamDetails = team.FirstTeam?.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId.Value);
                    if (firstTeamDetails != null)
                    {
                        team.FirstTeam.Title = firstTeamDetails.TeamName;
                    }

                    var secondTeamDetails = team.SecondTeam?.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId.Value);
                    if (secondTeamDetails != null)
                    {
                        team.SecondTeam.Title = secondTeamDetails.TeamName;
                    }
                }
                return results;
            }*/

            return GetTennisQuery(categoryId, seasonId.Value).ToList();
        }

        public int CountStage(int idLeague)
        {
            return db.Stages.Count(x => x.LeagueId == idLeague && !x.IsArchive);
        }

        public int CountTennisStage(int categoryId, int seasonId)
        {
            return db.TennisStages.Count(x => x.CategoryId == categoryId && !x.IsArchive && x.SeasonId == seasonId);
        }

        public Stage GetById(int id)
        {
            return db.Stages.Find(id);
        }

        public Stage GetByIdExtended(int id)
        {
            return db.Stages
                .Include("Groups")
                .Include("Groups.GroupsTeams")
                .Include("Groups.GroupsTeams.Team")
                .FirstOrDefault(x => x.StageId == id);
        }

        public TennisStage GetTennisStageById(int id)
        {
            return db.TennisStages.Find(id);
        }

        public void DeleteAllGameCycles(int stageId)
        {
            var games = db.GamesCycles.Where(t => t.StageId == stageId).Include(f => f.Users).ToList();
            foreach (var g in games)
            {
                //game fans
                g.Users.Clear();
                RemoveTennisGames(g);
                db.GamesCycles.Remove(g);

            }
            db.SaveChanges();
        }

        private void RemoveTennisGames(GamesCycle g)
        {
            if (g.TennisLeagueGames.Count > 0)
            {
                foreach (var game in g.TennisLeagueGames.ToList())
                {
                    db.TennisLeagueGameScores.RemoveRange(game.TennisLeagueGameScores);
                    db.TennisLeagueGames.Remove(game);
                }
            }
        }

        public void DeleteAllTennisGameCycles(int stageId)
        {
            var games = db.TennisGameCycles.Where(t => t.StageId == stageId).ToList();
            foreach (var g in games)
            {
                //game fans
                var gameSets = g.TennisGameSets;
                db.TennisGameSets.RemoveRange(gameSets);
                db.TennisGameCycles.Remove(g);
            }
            db.SaveChanges();
        }

        public void DeleteAllGroups(int stageId)
        {
            var groups = db.Groups.Where(t => t.StageId == stageId);
            foreach (var g in groups)
            {
                g.IsArchive = true;
            }
            Save();
        }

        public void DeleteAllTennisGroups(int stageId)
        {
            var groups = db.TennisGroups.Where(t => t.StageId == stageId);
            foreach (var g in groups)
            {
                g.IsArchive = true;
            }
            Save();
        }

        internal List<Stage> GetStagesForLeague(int leagueId)
        {
            return db.Stages.Where(t => t.LeagueId == leagueId && t.IsArchive == false)
                   .OrderByDescending(t => t.StageId).ToList();
        }

        internal List<TennisStage> GetTennisStagesForCategory(int categoryId)
        {
            return db.TennisStages.Where(t => t.CategoryId == categoryId && t.IsArchive == false)
                   .OrderByDescending(t => t.StageId).ToList();
        }

        internal Stage GetLastStageForLeague(int leagueId)
        {
            return GetStagesForLeague(leagueId).FirstOrDefault();
        }

        internal TennisStage GetLastTennisStageForCategory(int categoryId)
        {
            return GetTennisStagesForCategory(categoryId).FirstOrDefault();
        }

        internal DateTime? GetLastGameDateFromStageBeforeLast(int leagueId)
        {
            List<Stage> stages = GetStagesForLeague(leagueId);
            if (stages.Count() > 1)
            {
                var forLastStage = stages.ElementAt(1);
                var stageGames = forLastStage.GamesCycles.OrderBy(c => c.StartDate);
                var lastGame = stageGames.LastOrDefault();
                if (lastGame != null)
                {
                    return lastGame.StartDate.AddDays(1);
                }
            }
            return null;
        }
    }
}
