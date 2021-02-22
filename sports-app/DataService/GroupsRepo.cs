using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using AppModel;

namespace DataService
{
    public class GroupTeam
    {
        public int GroupId { get; set; }
        public int TeamId { get; set; }
        public int StageId { get; set; }
        public string Title { get; set; }
        public string PlayerIdStr { get; set; }
        public int? Pos { get; set; }
    }

    public class GroupsRepo : BaseRepo
    {
        public GroupsRepo() : base()
        {
        }


        public GroupsRepo(DataEntities db) : base(db)
        {
        }

        public Group GetById(int id)
        {
            return db.Groups.Find(id);
        }

        public TennisGroup GetTennisGroupById(int id)
        {
            return db.TennisGroups.Find(id);
        }

        public IEnumerable<GamesType> GetGamesTypes()
        {
            return db.GamesTypes.ToList();
        }

        public void Create(Group item)
        {
            db.Groups.Add(item);
        }

        public void CreateTennisGroup(TennisGroup item)
        {
            db.TennisGroups.Add(item);
        }

        public IEnumerable<Group> GetAll(int leagueId)
        {
            return db.Groups.Include(t => t.GamesType).Where(t => t.IsArchive == false && t.Stage.LeagueId == leagueId).ToList();
        }

        //public IEnumerable<Team> GetGroupsTeams(int leagueId)
        //{

        //    return db.Teams.Include(t => t.GroupsTeams).Where(t => t.IsArchive == false).ToList();
        //}

        public IEnumerable<Group> GetLeagueGroups(int leagueId)
        {
            return (from g in db.Groups
                    from gt in g.GroupsTeams
                    let t = gt.Team
                    from l in t.LeagueTeams
                    where t.IsArchive == false &&
                    l.LeagueId == leagueId &&
                    g.IsArchive == false
                    select g).Distinct().ToList();
        }

        public void UpdateTeams(Group group, int?[] teams, bool isIndividual)
        {
            foreach (var t in group.GroupsTeams.ToList())
                group.GroupsTeams.Remove(t);

            base.Save();

            if (teams != null && !isIndividual)
            {
                for (int i = 0; i < teams.Count(); i++)
                {
                    var gt = new GroupsTeam
                    {
                        Pos = i + 1,
                        TeamId = teams[i],
                        GroupId = group.GroupId
                    };
                    var t = teams[i];
                    var team = db.Teams.FirstOrDefault(x => x.TeamId == t);
                    db.GroupsTeams.Add(gt);
                    db.SaveChanges();
                }
            }
            if (teams != null && isIndividual)
            {
                for (int i = 0; i < teams.Count(); i++)
                {
                    var gt = new GroupsTeam
                    {
                        Pos = i + 1,
                        AthleteId = teams[i],
                        GroupId = group.GroupId,
                    };
                    var t = teams[i];
                    db.GroupsTeams.Add(gt);
                    db.SaveChanges();
                }
            }
        }

        public Group GetGroupByLeagueStageId(int stageId)
        {
            return db.Groups.FirstOrDefault(g => !g.IsArchive && g.StageId == stageId);
        }

        public TennisGroup GetTennisCompetitionGroupByStageId(int stageId)
        {
            return db.TennisGroups.FirstOrDefault(g => !g.IsArchive && g.StageId == stageId);
        }

        public void UpdateTennisTeams(TennisGroup group, string[] teams, bool isIndividual, bool isPairs)
        {
            foreach (var t in group.TennisGroupTeams.ToList())
            {
                db.TennisGroupTeams.Remove(t);
            }

            db.SaveChanges();

            if (teams != null && isIndividual && !isPairs)
            {
                for (int i = 0; i < teams.Count(); i++)
                {
                    var isSuccessParsed = int.TryParse(teams[i], out int playerId);
                    var gt = new TennisGroupTeam
                    {
                        Pos = i + 1,
                        PlayerId = isSuccessParsed ? (int?)playerId : null,
                        GroupId = group.GroupId,
                        SeasonId = group.SeasonId
                    };
                    var t = teams[i];
                    db.TennisGroupTeams.Add(gt);
                    db.SaveChanges();
                }
            }
            if (teams != null && isPairs)
            {
                for (int i = 0; i < teams.Count(); i++)
                {
                    var players = !string.IsNullOrEmpty(teams[i]) ? teams[i]?.Split('/')?.ToArray() : new string[0];
                    var gt = new TennisGroupTeam
                    {
                        Pos = i + 1,
                        PlayerId = players?.Any() == true
                        ? int.TryParse(players[0], out int firstPlayerId)
                            ? (int?)firstPlayerId
                            : null
                        : null,
                        GroupId = group.GroupId,
                        SeasonId = group.SeasonId,
                        PairPlayerId = players?.Any() == true
                        ? int.TryParse(players[1], out int pairPlayerId)
                            ? (int?)pairPlayerId
                            : null
                        : null
                    };
                    var t = teams[i];
                    db.TennisGroupTeams.Add(gt);
                    db.SaveChanges();
                }
            }
        }

        public IList<GroupTeam> GetTeamsGroups(int leagueId)
        {
            return (from g in db.Groups
                    from gt in g.GroupsTeams
                    let t = gt.Team
                    from l in t.LeagueTeams
                    where t.IsArchive == false && l.LeagueId == leagueId && g.IsArchive == false
                    select new GroupTeam
                    {
                        GroupId = g.GroupId,
                        TeamId = t.TeamId,
                        StageId = g.StageId,
                        Title = t.Title,
                        Pos = gt.Pos
                    }).OrderBy(gt => gt.Pos).ToList();
        }

        public IList<GroupTeam> GetTennisTeamsGroups(int categoryId)
        {
            return (from g in db.TennisGroups
                    from gt in g.TennisGroupTeams
                    let t = gt.TeamsPlayer
                    where t.IsActive == true && t.TeamId == categoryId && g.IsArchive == false
                    select new GroupTeam
                    {
                        GroupId = g.GroupId,
                        TeamId = t.TeamId,
                        StageId = g.StageId,
                        Title = string.Concat(t.User.FirstName, " ", t.User.LastName),
                        Pos = gt.Pos
                    }).OrderBy(gt => gt.Pos).ToList();
        }

        public IList<GroupTeam> GetTeamsByGroup(int groupId)
        {
            return (from g in db.Groups
                    from gt in g.GroupsTeams
                    let t = gt.Team
                    from l in t.LeagueTeams
                    where t.IsArchive == false && g.GroupId == groupId && g.IsArchive == false
                    select new GroupTeam
                    {
                        GroupId = g.GroupId,
                        TeamId = t.TeamId,
                        StageId = g.StageId,
                        Title = t.Title
                    }).ToList();
        }

        public int[] GetGroupsArr(int leagueId)
        {
            return db.Groups.Where(t => t.IsArchive == false && t.Stage.LeagueId == leagueId)
                .Select(t => t.GroupId).ToArray();
        }

        public Group GetFirstGroupOfLeague(int leagueId)
        {
            return db.Groups.FirstOrDefault(t => t.IsArchive == false && t.Stage.LeagueId == leagueId);
        }


        public IQueryable<GroupsTeam> GetGroupTeamsByGroupId(int groupId)
        {
            return db.GroupsTeams.Where(gt => gt.GroupId == groupId);
        }

        public List<GroupTeam> GetTennisLeagueGroupTeams(int leagueId)
        {
            return db.GroupsTeams.Where(g => g.Group.Stage.LeagueId == leagueId
                && !g.Team.IsArchive && !g.Team.TeamRegistrations.FirstOrDefault(t => t.LeagueId == leagueId).IsDeleted)
                    .Select(r => new GroupTeam
                    {
                        GroupId = r.GroupId,
                        TeamId = r.Team.TeamId,
                        StageId = r.Group.StageId,
                        Title = r.Team.Title,
                        Pos = r.Pos
                    }).OrderBy(gt => gt.Pos).ToList();
        }
    }
}
