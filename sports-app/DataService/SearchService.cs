using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppModel;

namespace DataService
{
    public class SearchService : BaseRepo
    {
        public IEnumerable<ListItemDto> FindLeagueByName(string name, int managerId, int num)
        {
            var unionsQuery = (from u in db.Unions
                               from le in u.Leagues
                               from uj in u.UsersJobs
                               let j = uj.Job.JobsRole
                               where u.IsArchive == false
                                   && le.IsArchive == false
                                   && uj.UserId == managerId
                                   && j.RoleName == JobRole.UnionManager
                                   && le.Name.Contains(name)
                               select new ListItemDto { Id = le.LeagueId, Name = le.Name });

            var leagueQuery = (from le in db.Leagues
                               from uj in le.UsersJobs
                               let j = uj.Job.JobsRole
                               where le.IsArchive == false
                                   && uj.UserId == managerId
                                   && j.RoleName == JobRole.LeagueManager
                                   && le.Name.Contains(name)
                               select new ListItemDto { Id = le.LeagueId, Name = le.Name });

            return unionsQuery.Union(leagueQuery).Take(num).ToList();
        }

        public IEnumerable<ListItemDto> FindTeamByName(string name, int managerId, int num)
        {
            var unionsQuery = (from u in db.Unions
                               from le in u.Leagues
                               from t in le.LeagueTeams
                               from uj in u.UsersJobs
                               let j = uj.Job.JobsRole
                               where u.IsArchive == false
                                   && le.IsArchive == false
                                   && t.Teams.IsArchive == false
                                   && uj.UserId == managerId
                                   && j.RoleName == JobRole.UnionManager
                                   && t.Teams.Title.Contains(name)
                               select new TeamSearchResult { Id = t.TeamId, Team = t.Teams });

            var leagueQuery = (from le in db.Leagues
                               from t in le.LeagueTeams
                               from uj in le.UsersJobs
                               let j = uj.Job.JobsRole
                               where le.IsArchive == false
                                   && t.Teams.IsArchive == false
                                   && uj.UserId == managerId
                                   && j.RoleName == JobRole.LeagueManager
                                   && t.Teams.Title.Contains(name)
                               select new TeamSearchResult { Id = t.TeamId, Team = t.Teams });

            var teamsQuery = (from t in db.Teams
                              from uj in t.UsersJobs
                              let j = uj.Job.JobsRole
                              where t.IsArchive == false
                                  && uj.UserId == managerId
                                  && j.RoleName == JobRole.TeamManager
                                  && t.Title.Contains(name)
                              select new TeamSearchResult { Id = t.TeamId, Team = t });

            var union = unionsQuery.Union(leagueQuery).Union(teamsQuery).Take(num).ToList();
            return union.Select(t => new ListItemDto { Id = t.Id, Name = CreateTeamTitle(t.Team) });
        }

        private string CreateTeamTitle(Team team)
        {
            var leagueTitels = team.LeagueTeams.Select(l => l.Leagues.Name).ToList();
            return team.Title +  " (" + string.Join(", ", leagueTitels) + ")";
        }


        public IEnumerable<ListItemDto> FindPlayerByName(string name, int managerId, int num)
        {
            var unionsQuery = (from u in db.Unions
                               from le in u.Leagues
                               from t in le.LeagueTeams
                               from p in t.Teams.TeamsPlayers
                               from uj in u.UsersJobs
                               let j = uj.Job.JobsRole
                               let usr = p.User
                               let ut = usr.UsersType
                               where u.IsArchive == false
                                   && le.IsArchive == false
                                   && t.Teams.IsArchive == false
                                   && usr.IsArchive == false
                                   && uj.UserId == managerId
                                   && j.RoleName == JobRole.UnionManager
                                   && ut.TypeRole == AppRole.Players
                                   && usr.FullName.Contains(name)
                               select new ListItemDto { Id = usr.UserId, Name = usr.FullName });

            var leagueQuery = (from le in db.Leagues
                               from t in le.LeagueTeams
                               from p in t.Teams.TeamsPlayers
                               from uj in le.UsersJobs
                               let j = uj.Job.JobsRole
                               let usr = p.User
                               let ut = usr.UsersType
                               where le.IsArchive == false
                                   && t.Teams.IsArchive == false
                                   && usr.IsArchive == false
                                   && uj.UserId == managerId
                                   && j.RoleName == JobRole.LeagueManager
                                   && ut.TypeRole == AppRole.Players
                                   && usr.FullName.Contains(name)
                               select new ListItemDto { Id = usr.UserId, Name = usr.FullName });

            var teamsQuery = (from t in db.Teams
                              from p in t.TeamsPlayers
                              from uj in t.UsersJobs
                              let j = uj.Job.JobsRole
                              let usr = p.User
                              let ut = usr.UsersType
                              where t.IsArchive == false
                                  && usr.IsArchive == false
                                  && uj.UserId == managerId
                                  && j.RoleName == JobRole.TeamManager
                                  && ut.TypeRole == AppRole.Players
                                  && usr.FullName.Contains(name)
                              select new ListItemDto { Id = usr.UserId, Name = usr.FullName });

            return unionsQuery.Union(leagueQuery).Union(teamsQuery).Take(num).ToList();
        }
    }
}
