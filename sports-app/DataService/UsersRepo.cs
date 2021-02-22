using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using AppModel;

namespace DataService
{
    public class UserSearchItem
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string IdentNum { get; set; }
        public string PassportNum { get; set; }
        public int Status { get; set; }
        public int? AthleteNumber { get; set; }
        public bool AuthorizedToRegister { get; set; }
    }

    public class UsersRepo : BaseRepo
    {
        public UsersRepo() { }
        public UsersRepo(DataEntities db) : base(db) { }
        private const int TeamManagerRoleId = 3;
        private const int ClubManagerRoleId = 5;
        private const int UnionCoachRoleId = 15;
        
        public User GetByUsername(string userName)
        {
            return db.Users.Where(t => t.UserName == userName && t.IsArchive == false).FirstOrDefault();
        }

        public bool CheckIfHasMultipleJobs(int userId)
        {
            var userJobs = db.Users.FirstOrDefault(u => u.UserId == userId)
                ?.UsersJobs
                //?.Where(c => c.Job.JobsRole.RoleName != JobRole.Referee)
                .Select(c => c.Job)
                //.GroupBy(x => x.JobsRole)
                .ToList();

            if (userJobs?.Count > 1 &&
                userJobs.GroupBy(x => x.JobsRole).Count() == 1 &&
                (userJobs.First().JobsRole.RoleName == JobRole.UnionManager ||
                 userJobs.First().JobsRole.RoleName == JobRole.ClubManager))
            {
                return false;
            }

            return userJobs?.Count > 1;
        }

        public string IfHasSingleJobGetName(int userId)
        {
            var userJobs = db.Users.FirstOrDefault(u => u.UserId == userId)
                ?.UsersJobs
                //?.Where(c => c.Job.JobsRole.RoleName != JobRole.Referee)
                .Select(c => c.Job)
                //.GroupBy(x => x.JobsRole)
                .ToList();

            if (userJobs?.Count > 0)
            {
                return userJobs.First().JobName;
            }

            return string.Empty;
        }

        public List<int> GetClubsUserManaging(int userId, int seasonId)
        {
            return db.UsersJobs.Where(uj => uj.UserId == userId && uj.SeasonId == seasonId && uj.ClubId.HasValue && uj.Job.JobsRole.RoleName == JobRole.ClubManager).Select(uj => uj.ClubId ?? 0).ToList();
        }

        public bool IsMultiSectionPlayer(int userId)
        {
            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                var userSections = user.TeamsPlayers.Where(tp => tp.ClubId.HasValue && tp.Club.SectionId.HasValue).Select(c => c.Club.SectionId.Value)
                    .Union(user.TeamsPlayers.Where(tp => tp.ClubId.HasValue && tp.Club.UnionId.HasValue).Select(c => c.Club.Union.SectionId))
                    .Union(user.TeamsPlayers.Where(tp => tp.LeagueId.HasValue && tp.League.ClubId.HasValue && tp.League.Club.SectionId.HasValue).Select(c => c.League.Club.SectionId.Value))
                    .Union(user.TeamsPlayers.Where(tp => tp.LeagueId.HasValue && tp.League.ClubId.HasValue && tp.League.Club.UnionId.HasValue).Select(c => c.League.Club.Union.SectionId))
                    .Union(user.TeamsPlayers.Where(tp => tp.LeagueId.HasValue && tp.League.UnionId.HasValue).Select(c => c.League.Union.SectionId));

                var userJobs = db.UsersJobs.Where(c => c.UserId == userId);

                var userJobsSectionsIds = userJobs.Where(uj => uj.ClubId.HasValue && uj.Club.SectionId.HasValue).Select(c => c.Club.SectionId.Value)
                    .Union(userJobs.Where(uj => uj.ClubId.HasValue && uj.Club.UnionId.HasValue).Select(c => c.Club.Union.SectionId))
                    .Union(userJobs.Where(uj => uj.LeagueId.HasValue && uj.League.ClubId.HasValue && uj.League.Club.SectionId.HasValue).Select(c => c.League.Club.SectionId.Value))
                    .Union(userJobs.Where(uj => uj.LeagueId.HasValue && uj.League.ClubId.HasValue && uj.League.Club.UnionId.HasValue).Select(c => c.League.Club.Union.SectionId))
                    .Union(userJobs.Where(uj => uj.LeagueId.HasValue && uj.League.UnionId.HasValue).Select(c => c.League.Union.SectionId));

                var sectionIds = userJobsSectionsIds.Union(userSections).Distinct();
                if (sectionIds != null && sectionIds.Count() > 1)
                    return true;
            }
            return false;
        }

        public User FindByName(string role, string name)
        {
            return db.Users.Where(t => string.Concat(t.FirstName, " ", t.LastName) == name
                && t.UsersType.TypeRole == role
                && t.IsActive == true).FirstOrDefault();
        }

        public IQueryable<User> GetQuery()
        {
            return db.Users.AsQueryable();
        }

        public IEnumerable<User> GetAll()
        {
            return db.Users.Include(t => t.UsersType).OrderBy(t => t.UserName).ToList();
        }

        public User GetById(int id)
        {
            return db.Users.Find(id);
        }

        public List<User> GetByIds(int[] ids)
        {
            return db.Users.Where(p => ids.Contains(p.UserId)).ToList();
        }

        public List<PlayerHistory> GetPlayerHistory(int userId, int? seasonId = null)
        {
            var history = db.PlayerHistory.Where(x => x.UserId == userId);

            if (seasonId != null)
            {
                history = history.Where(x => x.SeasonId == seasonId);
            }

            return history.ToList();
        }

        public User GetByIdentityNumber(string pId)
        {
            return db.Users.FirstOrDefault(u => u.IdentNum == pId);
        }

        public User GetByEmail(string email)
        {
            return db.Users.FirstOrDefault(u => u.Email == email); 
        }

        public bool IsUsernameExists(string userName, int id)
        {
            return db.Users.Any(t => t.UserName == userName && t.UserId != id);
        }

        public IEnumerable<UsersType> GetTypes()
        {
            return db.UsersTypes.ToList();
        }

        public void Create(User item)
        {
            db.Users.Add(item);
            db.SaveChanges();
        }

        public IEnumerable<ListItemDto> SearchUser(string role, string name, int num, int? clubId = null)
        {
            var users = db.Users.Where(t => string.Concat(t.FirstName, " ", t.LastName).Contains(name) && t.UsersType.TypeRole == role && t.IsArchive == false);
            if (clubId.HasValue)
            {
                var teams = db.ClubTeams.Where(c => c.ClubId == clubId).Select(t => t.TeamId);
                var userIds = db.TeamsPlayers.Where(t => teams.Contains(t.TeamId)).Select(t => t.UserId);
                users = users.Where(u => userIds.Contains(u.UserId));
            }

            return users
                .OrderBy(t => t.FirstName).ThenBy(t => t.LastName)
                .Select(t => new ListItemDto
                {
                    Id = t.UserId,
                    Name = string.Concat(t.FirstName, " ", t.LastName),
                }).Take(num).ToList();
        }
        public IEnumerable<ListItemDto> SearchPlayersWithTeamAndSeason(string role, string name, int num, int? teamId,
            int? leagueId, int? unionId, int? clubId, int? seasonId)
        {
            var players = db.TeamsPlayers.Where(x => (seasonId == null || x.SeasonId == seasonId) &&
                                                     x.User.UsersType.TypeRole == role &&
                                                     string.Concat(x.User.FirstName, " ", x.User.LastName).Contains(name)  &&
                                                     !x.User.IsArchive &&
                                                     (leagueId == null || x.Team.LeagueTeams.Any(lt => lt.LeagueId == x.LeagueId && lt.SeasonId == x.SeasonId)) &&
                                                     (x.LeagueId == null || x.Team.LeagueTeams.Any(lt => lt.LeagueId == x.LeagueId && lt.SeasonId == x.SeasonId))
                                                     && (clubId > 0 && leagueId <= 0 ? x.ClubId == clubId : (x.ClubId == null || x.LeagueId == null)));

            if (teamId.HasValue)
            {
                players = players.Where(x => x.TeamId == teamId.Value);
            }
            if (unionId.HasValue)
            {
                players = players.Where(x => x.League.UnionId == unionId.Value || x.Club.UnionId == unionId.Value);
            }
            if (leagueId.HasValue)
            {
                players = players.Where(x => x.LeagueId == leagueId.Value);
            }
            if (clubId.HasValue)
            {
                players = players.Where(x => x.ClubId == clubId.Value);
            }

            return players
                .OrderBy(x => x.User.FirstName).ThenBy(x => x.User.LastName)
                .ToList()
                .Select(x =>
                {
                    var teamName = x.Team.TeamsDetails.FirstOrDefault(td => td.SeasonId == x.SeasonId)?.TeamName ??
                                   x.Team.Title;

                    return new ListItemDto
                    {
                        Id = x.UserId,
                        Name = $"{x.User.FullName} ({teamName} - {x.League?.Name} - {x.Season.Name})",
                        SeasonId = x.SeasonId,
                        TeamId = x.TeamId,
                        LeagueId = x.LeagueId ?? 0,
                        ClubId = x.ClubId ?? 0
                    };
                }).Take(num);
        }

       public List<ListItemDto> SearchPlayersWithTeamAndSeasonByNav(string role, string name, int num, int? teamId,
       int? leagueId, int? unionId, int? clubId, int? seasonId)
        {
            int athNum;
            bool isNum = int.TryParse(name, out athNum);


            var players = db.TeamsPlayers.Where(x => (seasonId == null || x.SeasonId == seasonId) &&
                                                     x.User.UsersType.TypeRole == role &&
                                                     (string.Concat(x.User.FirstName, " ", x.User.LastName).Contains(name) || x.User.IdentNum == name || (isNum && x.User.AthleteNumbers.FirstOrDefault(y => y.SeasonId == seasonId).AthleteNumber1.HasValue && x.User.AthleteNumbers.FirstOrDefault(y => y.SeasonId == seasonId).AthleteNumber1.Value == athNum)) &&
                                                     !x.User.IsArchive &&
                                                     (leagueId == null || x.Team.LeagueTeams.Any(lt => lt.LeagueId == x.LeagueId && lt.SeasonId == x.SeasonId)) &&
                                                     (x.LeagueId == null || x.Team.LeagueTeams.Any(lt => lt.LeagueId == x.LeagueId && lt.SeasonId == x.SeasonId))
                                                     && (clubId > 0 && leagueId <= 0 ? x.ClubId == clubId : (x.ClubId == null || x.LeagueId == null)));
            if (teamId.HasValue)
            {
                players = players.Where(x => x.TeamId == teamId.Value);
            }
            if (unionId.HasValue)
            {
                players = players.Where(x => x.League.UnionId == unionId.Value || x.Club.UnionId == unionId.Value);
            }
            if (leagueId.HasValue)
            {
                players = players.Where(x => x.LeagueId == leagueId.Value);
            }
            if (clubId.HasValue)
            {
                players = players.Where(x => x.ClubId == clubId.Value);
            }

            return players
                .OrderBy(x => x.User.FirstName).ThenBy(x => x.User.LastName)
                .ToList()
                .Select(x =>
                {
                    var teamName = x.Team.TeamsDetails.FirstOrDefault(td => td.SeasonId == x.SeasonId)?.TeamName ??
                                   x.Team.Title;

                    string athleteNumStr = "";
                    if (x.User != null && x.User.AthleteNumbers!= null && x.User.AthleteNumbers.FirstOrDefault(y => y.SeasonId == seasonId)?.AthleteNumber1.HasValue == true)
                    {
                        athleteNumStr = "- " + x.User.AthleteNumbers.FirstOrDefault(y => y.SeasonId == seasonId).AthleteNumber1.Value.ToString();
                    }

                    return new ListItemDto
                    {
                        Id = x.UserId,
                        Name = $"{x.User.FullName} {athleteNumStr} ({teamName} - {x.League?.Name} {x.Season.Name})",
                        SeasonId = x.SeasonId,
                        TeamId = x.TeamId,
                        LeagueId = x.LeagueId ?? 0,
                        ClubId = x.ClubId ?? 0
                    };
                }).Take(num).ToList();
        }


        public IEnumerable<ListItemDto> SearchTeamPlayers(string role, string name, int num, int? teamId)
        {
            var users = db.Users.Where(t => string.Concat(t.FirstName, " ", t.LastName).Contains(name) && t.UsersType.TypeRole == role && t.IsArchive == false);
            if (teamId.HasValue)
            {
                var teams = db.Teams.Where(c => c.TeamId == teamId && c.IsArchive == false).Select(t => t.TeamId);
                var userIds = db.TeamsPlayers.Where(t => teams.Contains(t.TeamId)).Select(t => t.UserId);
                users = users.Where(u => userIds.Contains(u.UserId));
            }

            return users
                .OrderBy(t => t.FirstName).ThenBy(t => t.LastName)
                .Select(t => new ListItemDto
                {
                    Id = t.UserId,
                    Name = string.Concat(t.FirstName, " ", t.LastName),
                }).Take(num).ToList();
        }

        public IEnumerable<ListItemDto> SearchTeamPlayersByUnionId(string role, string name, int num, int? unionId)
        {
            var users = db.Users.Where(t => string.Concat(t.FirstName, " ", t.LastName).Contains(name) && t.UsersType.TypeRole == role && t.IsArchive == false);
            if (unionId.HasValue)
            {
                var leagues = db.Leagues.Where(l => l.UnionId == unionId).ToList();
                var leagueTeamsIds = new List<int>();
                foreach (var league in leagues)
                {
                    leagueTeamsIds.AddRange(league.LeagueTeams.Where(l => l.Teams.IsArchive == false)
                        .Select(t => t.TeamId));
                }
                var userIds = db.TeamsPlayers.Where(t => leagueTeamsIds.Contains(t.TeamId)).Select(t => t.UserId);
                users = users.Where(u => userIds.Contains(u.UserId));
            }

            return users
                .OrderBy(t => t.FirstName).ThenBy(t => t.LastName)
                .Select(t => new ListItemDto
                {
                    Id = t.UserId,
                    Name = string.Concat(t.FirstName, " ", t.LastName),
                }).Take(num).ToList();
        }

        public IEnumerable<ListItemDto> SearchTeamPlayersByLeagueId(string role, string name, int num, int? leagueId)
        {
            var users = db.Users.Where(t => t.FullName.Contains(name) && t.UsersType.TypeRole == role && t.IsArchive == false);
            if (leagueId.HasValue)
            {
                var teams = db.LeagueTeams.Where(t => t.LeagueId == leagueId && t.Teams.IsArchive == false)
                    .Select(t => t.TeamId).ToList();
                var userIds = db.TeamsPlayers.Where(t => teams.Contains(t.TeamId)).Select(t => t.UserId);
                users = users.Where(u => userIds.Contains(u.UserId));
            }

            return users
                .OrderBy(t => t.FullName)
                .Select(t => new ListItemDto
                {
                    Id = t.UserId,
                    Name = t.FullName,
                }).Take(num).ToList();
        }

        public IEnumerable<ListItemDto> SearchSectionUser(int sectionId, string role, string name, int num)
        {
            return (from j in db.Jobs
                    from uj in j.UsersJobs
                    let u = uj.User
                    where j.SectionId == sectionId
                        && u.FullName.Contains(name)
                        && u.UsersType.TypeRole == role
                    //&& u.IsArchive == false
                    orderby u.FullName
                    select new ListItemDto { Id = u.UserId, Name = u.FullName }).Distinct().Take(num).ToList();
        }

        public string GetTopLevelJob(int userId)
        {
            return (from u in db.Users
                    from j in u.UsersJobs
                    let r = j.Job.JobsRole
                    where u.UserId == userId
                    orderby r.Priority descending
                    select r.RoleName).FirstOrDefault();
        }

        public string GetCurrentJob(int userId, int seasonId)
        {
            var jobs = (from u in db.Users
                from j in u.UsersJobs
                let r = j.Job.JobsRole
                where u.UserId == userId && j.SeasonId == seasonId
                orderby r.Priority descending
                select r.RoleName);

            return jobs.FirstOrDefault();
        }

        public bool HasCurrentJob(int userId, int seasonId, string role)
        {
            var jobs = (from u in db.Users
                        from j in u.UsersJobs
                        let r = j.Job.JobsRole
                        where u.UserId == userId && j.SeasonId == seasonId && r.RoleName == role
                        select r.RoleName);

            return jobs.Count() > 0;
        }



        public string GetCurrentLeagueJob(int userId, int leagueId)
        {
            var jobs = (from u in db.Users
                        from j in u.UsersJobs
                        let r = j.Job.JobsRole
                        where u.UserId == userId && j.LeagueId == leagueId
                        orderby r.Priority descending
                        select r.RoleName);

            return jobs.FirstOrDefault();
        }

        public bool HasLeagueJob(int userId, int leagueId, string role)
        {
            var jobs = (from u in db.Users
                        from j in u.UsersJobs
                        let r = j.Job.JobsRole
                        where u.UserId == userId && j.LeagueId == leagueId && r.RoleName == role
                        select r.RoleName);

            return jobs.Count() > 0;
        }




        public string GetCurrentClubJob(int userId, int clubId)
        {
            var jobs = (from u in db.Users
                        from j in u.UsersJobs
                        let r = j.Job.JobsRole
                        where u.UserId == userId && j.ClubId == clubId
                        orderby r.Priority descending
                        select r.RoleName);

            return jobs.FirstOrDefault();
        }


        public bool HasClubJob(int userId, int clubId, string role)
        {
            var jobs = (from u in db.Users
                        from j in u.UsersJobs
                        let r = j.Job.JobsRole
                        where u.UserId == userId && j.ClubId == clubId && r.RoleName == role
                        select r.RoleName);

            return jobs.Count() > 0;
        }


        public UsersEducation GetEducationByUserId(int userId)
        {
            return db.UsersEducations.FirstOrDefault(u => u.UserId == userId);
        }

        public IEnumerable<User> GetLeagueWorkers(int leagueId, string roleName)
        {
            return (from u in db.Users
                    from uj in u.UsersJobs
                    let r = uj.Job.JobsRole
                    where uj.LeagueId == leagueId && u.IsArchive == false && r.RoleName == roleName
                    select u).ToList();
        }

        public bool IsSectionWorker(int userId, string sectionAlias, string role)
        {
            try
            {
                var currentUser = db.Users.Find(userId);
                return currentUser.UsersJobs.Select(r => r.Job).Any(r => r.JobsRole.RoleName == role && r.Section.Alias == sectionAlias);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<User> GetUnionWorkers(int unionId, string roleName, int? seasonId = null)
        {
            var userIds = new List<int>();

            var users = db.UsersJobs
                .Where(x => x.UnionId == unionId &&
                             !x.User.IsArchive &&
                             x.Job.JobsRole.RoleName == roleName &&
                             (seasonId == null || x.SeasonId == seasonId))
                .Select(t => t.User)
                .ToList();

            foreach (var user in users)
            {
                if (!userIds.Contains(user.UserId))
                {
                    userIds.Add(user.UserId);
                    yield return user;
                }
            }
        }

        public IEnumerable<User> GetUnionAndLeagueReferees(int unionId, int leagueId)
        {
            return (from u in db.Users
                    from uj in u.UsersJobs
                    let r = uj.Job.JobsRole
                    where
                    (uj.UnionId == unionId || uj.LeagueId == leagueId) &&
                    u.IsArchive == false &&
                    r.RoleName == JobRole.Referee
                    select u)
                    .Distinct()
                    .ToList()
                    .OrderBy(r => r.FullName);
        }

        public IEnumerable<User> GetUnionReferees(int unionId)
        {
            return (from u in db.Users
                    from uj in u.UsersJobs
                    let r = uj.Job.JobsRole
                    where
                    (uj.UnionId == unionId) &&
                    u.IsArchive == false &&
                    r.RoleName == JobRole.Referee
                    select u)
                    .Distinct()
                    .ToList()
                    .OrderBy(r => r.FullName);
        }

        public IEnumerable<User> GetClubAndLeagueReferees(int clubId, int leagueId)
        {
            return (from u in db.Users
                    from uj in u.UsersJobs
                    let r = uj.Job.JobsRole
                    where
                    (uj.ClubId == clubId || uj.LeagueId == leagueId) &&
                    u.IsArchive == false &&
                    r.RoleName == JobRole.Referee
                    select u)
                    .Distinct()
                    .ToList()
                    .OrderBy(r => r.FullName);
        }

        public IEnumerable<User> GetClubReferees(int clubId)
        {
            return (from u in db.Users
                    from uj in u.UsersJobs
                    let r = uj.Job.JobsRole
                    where
                    (uj.ClubId == clubId) &&
                    u.IsArchive == false &&
                    r.RoleName == JobRole.Referee
                    select u)
                    .Distinct()
                    .ToList()
                    .OrderBy(r => r.FullName);
        }

        public IEnumerable<User> GetUnionAndLeageDesks(int unionId, int leagueId)
        {
            return (from u in db.Users
                    from uj in u.UsersJobs
                    let r = uj.Job.JobsRole
                    where
                    (uj.UnionId == unionId || uj.LeagueId == leagueId) &&
                    u.IsArchive == false &&
                    r.RoleName == JobRole.Desk
                    select u)
                    .Distinct()
                    .ToList()
                    .OrderBy(r => r.FullName);
        }

        public IEnumerable<User> GetClubAndLeagueDesks(int clubId, int leagueId)
        {
            return (from u in db.Users
                    from uj in u.UsersJobs
                    let r = uj.Job.JobsRole
                    where
                    (uj.ClubId == clubId || uj.LeagueId == leagueId) &&
                    u.IsArchive == false &&
                    r.RoleName == JobRole.Desk
                    select u)
                    .Distinct()
                    .ToList()
                    .OrderBy(r => r.FullName);
        }

        public IEnumerable<User> GetUnionAndLeagueSpectators(int unionId, int leagueId)
        {
            return (from u in db.Users
                    from uj in u.UsersJobs
                    let r = uj.Job.JobsRole
                    where
                    (uj.UnionId == unionId || uj.LeagueId == leagueId) &&
                    u.IsArchive == false &&
                    r.RoleName == JobRole.Spectator
                    select u)
                    .Distinct()
                    .ToList()
                    .OrderBy(r => r.FullName);
        }

        public IEnumerable<User> GetUnionSpectators(int unionId)
        {
            return (from u in db.Users
                    from uj in u.UsersJobs
                    let r = uj.Job.JobsRole
                    where
                    (uj.UnionId == unionId) &&
                    u.IsArchive == false &&
                    r.RoleName == JobRole.Spectator
                    select u)
                    .Distinct()
                    .ToList()
                    .OrderBy(r => r.FullName);
        }

        public IEnumerable<User> GetClubAndLeagueSpectators(int clubId, int leagueId)
        {
            return (from u in db.Users
                    from uj in u.UsersJobs
                    let r = uj.Job.JobsRole
                    where
                    (uj.ClubId == clubId || uj.LeagueId == leagueId) &&
                    u.IsArchive == false &&
                    r.RoleName == JobRole.Spectator
                    select u)
                    .Distinct()
                    .ToList()
                    .OrderBy(r => r.FullName);
        }

        public IEnumerable<User> GetClubSpectators(int clubId)
        {
            return (from u in db.Users
                    from uj in u.UsersJobs
                    let r = uj.Job.JobsRole
                    where
                    (uj.ClubId == clubId) &&
                    u.IsArchive == false &&
                    r.RoleName == JobRole.Spectator
                    select u)
                    .Distinct()
                    .ToList()
                    .OrderBy(r => r.FullName);
        }

        public IEnumerable<UserSearchItem> SearchUserByIdent(int sectionId, int? seasonId, string role, string identNum, int num, bool isOnlyApproved = false)
        {
            var tps =  db.TeamsPlayers.Where(tp =>
                    (tp.Club != null && tp.Club.Union != null && tp.Club.Union.SectionId == sectionId)
                    || (tp.Club != null && tp.Club.SectionId.HasValue && tp.Club.SectionId == sectionId)
                    || (tp.League != null && tp.League.ClubId.HasValue && tp.League.Club.SectionId.HasValue &&
                        tp.League.Club.SectionId == sectionId)
                    || (tp.League != null && tp.League.ClubId.HasValue && tp.League.Club.UnionId.HasValue &&
                        tp.League.Club.Union.SectionId == sectionId)
                    || (tp.League != null && tp.League.UnionId.HasValue && tp.League.Union.SectionId == sectionId))
                .Where(c => c.User.IdentNum.Contains(identNum) && (!seasonId.HasValue || seasonId == c.SeasonId))
                .OrderBy(tp => tp.User.FirstName).ThenBy(tp => tp.User.LastName).ToList();
            foreach (var tp in tps)
            {
                tp.User.TempIsApprovedByManager = tp.IsApprovedByManager;
            }
            if(tps.Count() == 0 && !seasonId.HasValue)
            {
                var us = db.Users.Where(u => u.IdentNum.Contains(identNum)).OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToList();
                return us.Distinct().Take(num).ToList()
                .Select(u => new UserSearchItem
                {
                    UserId = u.UserId,
                    IdentNum = u.IdentNum,
                    FullName = u.FullName,
                    AthleteNumber = seasonId.HasValue ? u.AthleteNumbers.Where(a => a.SeasonId == seasonId).FirstOrDefault()?.AthleteNumber1 : null,
                    AuthorizedToRegister = !isOnlyApproved || u.TempIsApprovedByManager == true
                });
            }
            return tps.Select(tp => tp.User).Distinct().Take(num).ToList()
                .Select(u => new UserSearchItem
                {
                    UserId = u.UserId,
                    IdentNum = u.IdentNum,
                    FullName = u.FullName,
                    AthleteNumber = seasonId.HasValue ? u.AthleteNumbers.Where(a => a.SeasonId == seasonId).FirstOrDefault()?.AthleteNumber1 : null,
                    AuthorizedToRegister = !isOnlyApproved || u.TempIsApprovedByManager == true
                });
        }
        public IEnumerable<UserSearchItem> SearchUserByPassport(int sectionId, int? seasonId, string role, string passport, int num, bool isOnlyApproved = false)
        {
            var tps = db.TeamsPlayers.Where(tp => (tp.Club != null && tp.Club.Union != null && tp.Club.Union.SectionId == sectionId)
                   || (tp.Club != null && tp.Club.SectionId.HasValue && tp.Club.SectionId == sectionId)
                   || (tp.League != null && tp.League.ClubId.HasValue && tp.League.Club.SectionId.HasValue && tp.League.Club.SectionId == sectionId)
                   || (tp.League != null && tp.League.ClubId.HasValue && tp.League.Club.UnionId.HasValue && tp.League.Club.Union.SectionId == sectionId)
                   || (tp.League != null && tp.League.UnionId.HasValue && tp.League.Union.SectionId == sectionId))
                   .Where(c => c.User.PassportNum.Contains(passport) && (!seasonId.HasValue || seasonId == c.SeasonId)).ToList();

            foreach (var tp in tps)
            {
                tp.User.TempIsApprovedByManager = tp.IsApprovedByManager;
            }
            return tps.Select(tp => tp.User).Distinct().Take(num).ToList()
                   .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                        .Select(u => new UserSearchItem {
                            UserId = u.UserId,
                            IdentNum = u.IdentNum,
                            PassportNum = u.PassportNum,
                            FullName = u.FullName,
                            AthleteNumber = seasonId.HasValue ? u.AthleteNumbers.Where(a => a.SeasonId == seasonId).FirstOrDefault()?.AthleteNumber1 : null,
                            AuthorizedToRegister = !isOnlyApproved || u.TempIsApprovedByManager == true
                        });

        }

        public IEnumerable<UserSearchItem> SearchUserByFullName(int sectionId, int? seasonId, string role, string namePart, int num, bool isOnlyApproved = false)
        {
            var tps = db.TeamsPlayers.Where(tp => (tp.Club != null && tp.Club.Union != null && tp.Club.Union.SectionId == sectionId)
                   || (tp.Club != null && tp.Club.SectionId.HasValue && tp.Club.SectionId == sectionId)
                   || (tp.League != null && tp.League.ClubId.HasValue && tp.League.Club.SectionId.HasValue && tp.League.Club.SectionId == sectionId)
                   || (tp.League != null && tp.League.ClubId.HasValue && tp.League.Club.UnionId.HasValue && tp.League.Club.Union.SectionId == sectionId)
                   || (tp.League != null && tp.League.UnionId.HasValue && tp.League.Union.SectionId == sectionId))
                   .Where(c => string.Concat(c.User.FirstName, " ", c.User.LastName).Contains(namePart) && (!seasonId.HasValue || seasonId == c.SeasonId)).ToList();

            foreach (var tp in tps)
            {
                tp.User.TempIsApprovedByManager = tp.IsApprovedByManager;
            }
            return tps.Select(tp => tp.User).Distinct().Take(num).ToList()
                   .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                        .Select(u => new UserSearchItem {
                            UserId = u.UserId,
                            IdentNum = u.IdentNum,
                            PassportNum = u.PassportNum,
                            FullName = u.FullName,
                            AthleteNumber = seasonId.HasValue ? u.AthleteNumbers.Where(a => a.SeasonId == seasonId).FirstOrDefault()?.AthleteNumber1 : null,
                            AuthorizedToRegister = !isOnlyApproved || u.TempIsApprovedByManager == true
                        });

        }

        public IEnumerable<UserSearchItem> SearchUserByAthleteNumber(int sectionId, int seasonId, string role, int athleteNum, int num, bool isOnlyApproved = false)
        {
            var tps = db.TeamsPlayers.Where(tp => (tp.Club != null && tp.Club.Union != null && tp.Club.Union.SectionId == sectionId)
                   || (tp.Club != null && tp.Club.SectionId.HasValue && tp.Club.SectionId == sectionId)
                   || (tp.League != null && tp.League.ClubId.HasValue && tp.League.Club.SectionId.HasValue && tp.League.Club.SectionId == sectionId)
                   || (tp.League != null && tp.League.ClubId.HasValue && tp.League.Club.UnionId.HasValue && tp.League.Club.Union.SectionId == sectionId)
                   || (tp.League != null && tp.League.UnionId.HasValue && tp.League.Union.SectionId == sectionId))
                   .Where(c => seasonId == c.SeasonId && c.User.AthleteNumbers.Any() && c.User.AthleteNumbers.OrderByDescending(t => t.SeasonId)
                                   .FirstOrDefault().AthleteNumber1 == athleteNum && c.User.AthleteNumbers.OrderByDescending(t => t.SeasonId)
                                   .FirstOrDefault().SeasonId == seasonId).ToList();
            foreach (var tp in tps)
            {
                tp.User.TempIsApprovedByManager = tp.IsApprovedByManager;
            }
            return tps.Select(tp => tp.User).Distinct().Take(num).ToList()
                   .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                        .Select(u => new UserSearchItem {
                            UserId = u.UserId,
                            IdentNum = u.IdentNum,
                            PassportNum = u.PassportNum,
                            FullName = u.FullName,
                            AthleteNumber = u.AthleteNumbers.FirstOrDefault(t => t.SeasonId == seasonId).AthleteNumber1,
                            AuthorizedToRegister = !isOnlyApproved || u.TempIsApprovedByManager == true
                        });
        }
        

        public string GetUserNamesStringByIds(IEnumerable<string> refereeIds)
        {
            string result = "";

            if (refereeIds != null)
            {
                var ids = refereeIds.Select(int.Parse).ToList();
                var referees = (from id in ids select db.Users.Find(id) into user where user != null select user.FullName).ToList();
                result = String.Join(",", referees);
            }

            return result;
        }

        public void UpdateEducationInfo(int userId, string education, string placeOfEducation, DateTime? dateOfEdIssue)
        {
            var userEducationInfo = db.UsersEducations.FirstOrDefault(e => e.UserId == userId);
            if (userEducationInfo != null)
            {
                userEducationInfo.Education = education;
                userEducationInfo.PlaceOfEducation = placeOfEducation;
                userEducationInfo.DateOfEdIssue = dateOfEdIssue;
            }
            else
            {
                db.UsersEducations.Add(new UsersEducation
                {
                    UserId = userId,
                    Education = education,
                    PlaceOfEducation = placeOfEducation,
                    DateOfEdIssue = dateOfEdIssue
                });
            }
        }

        public Dictionary<int, string> GetUserNamesByIds(HashSet<int> ids)
        {
            var referees = (from id in ids
                            select db.Users.Find(id)
                    into user
                            where user != null
                            select user)
                .ToDictionary(u => u.UserId, u => u.FullName);

            return referees;
        }

        public string AddAppCredentials(string entity, int id, string frmAppLogin, string frmAppPassword)
        {
            var entityName = entity + "." + id;
            if (string.IsNullOrEmpty(frmAppPassword)) return null;
            if (string.IsNullOrEmpty(frmAppLogin) && !string.IsNullOrEmpty(frmAppPassword))
                return "User name should not be empty";

            var existingUser = db.Users.SingleOrDefault(u => u.UserName == frmAppLogin);
            if (existingUser != null && existingUser.LastName != entityName)
            {
                return "User with such user name alraedy exists.";
            }

            var user = db.Users.SingleOrDefault(u => u.LastName == entityName);
            if (user == null)
            {
                user = new User
                {
                    LastName = entityName,
                    IsActive = true
                };
                db.Users.Add(user);
            }

            user.UserName = frmAppLogin;
            user.Password = Protector.Encrypt(frmAppPassword);
            user.TypeId = 6;

            db.SaveChanges();
            return null;
        }

        public User GetByUnionName(string name)
        {
            return db.Users.SingleOrDefault(u => u.LastName == name);
        }

        public List<ListItemDto> SearchAllUsers(string term, int num, int seasonId, bool isUnionManager = false, int unionId = 0)
        {
            if (isUnionManager)
            {
                var userJobs = db.UsersJobs.Where(c => (c.User.UsersType.TypeRole == AppRole.Players || c.User.UsersType.TypeRole == AppRole.Workers) && string.Concat(c.User.FirstName, " ", c.User.LastName).Contains(term));
                var playerJobs = db.TeamsPlayers.Where(c => (c.User.UsersType.TypeRole == AppRole.Players || c.User.UsersType.TypeRole == AppRole.Workers) && string.Concat(c.User.FirstName, " ", c.User.LastName).Contains(term));

                return GetUsersForUnionManagerRoleSearch(unionId, seasonId, userJobs, playerJobs, num);
            }
            var users = db.Users.Where(c => (c.UsersType.TypeRole == AppRole.Players || c.UsersType.TypeRole == AppRole.Workers) && string.Concat(c.FirstName, " ", c.LastName).Contains(term));
            return users.Select(u => new ListItemDto { Id = u.UserId, Name = string.Concat(u.FirstName, " ", u.LastName) }).Distinct().Take(num).ToList();
        }

        public List<ListItemDto> SearchAllReferees(string term, int num, int unionId = 0, int seasonId = 0)
        {
            var userJobs = db.UsersJobs
                .Include(x => x.User)
                .Where(x => (unionId == 0 || x.UnionId == unionId) &&
                            (seasonId == 0 || x.SeasonId == seasonId) &&
                            string.Concat(x.User.FirstName, " ", x.User.LastName).Contains(term) &&
                            x.Job.JobsRole.RoleName == JobRole.Referee &&
                            x.UnionId != null &&
                            !x.IsBlocked)
                .AsNoTracking()
                .ToList();

            return userJobs.Select(x => new ListItemDto { Id = x.UserId, Name = string.Concat(x.User.FirstName, " ", x.User.LastName) }).Distinct().Take(num).ToList();
        }

        public void UdateGymnasticsValue(string identNum, DateTime initialDate)
        {
            var user = db.Users.FirstOrDefault(g => g.IdentNum == identNum);
            if (user != null)
            {
                db.InitialApprovalDates.Add(new InitialApprovalDate
                {
                    UserId = user.UserId,
                    InitialApprovalDate1 = initialDate,
                    UnionId = 36
                });
            }
        }

        public bool HasPlayerInUnion(int userId, int unionId)
        {
            var checkLeagueTeam = db.LeagueTeams
                .Include(x => x.Leagues)
                .Include(x => x.Leagues.Union)
                .Include(x => x.Teams)
                .Include(x => x.Teams.TeamsPlayers)
                .Where(x => x.Leagues.Union.UnionId == unionId)
                .Any(x => x.Teams.TeamsPlayers.Any(y => y.User.UserId == userId));

            var checkClubTeam = db.ClubTeams
                .Include(x => x.Club)
                .Include(x => x.Club.Union)
                .Include(x => x.Team)
                .Include(x => x.Team.TeamsPlayers)
                .Where(x => x.Club.Union.UnionId == unionId)
                .Any(x => x.Team.TeamsPlayers.Any(y => y.User.UserId == userId));

            return checkLeagueTeam || checkClubTeam;
        }

        public List<ListItemDto> SearchUsersByIdentNum(string term, int num, int seasonId, bool isUnionManager = false, int unionId = 0)
        {
            if (isUnionManager)
            {
                var userJobs = db.UsersJobs.Where(c => (c.User.UsersType.TypeRole == AppRole.Players || c.User.UsersType.TypeRole == AppRole.Workers) && c.User.IdentNum.Contains(term));
                var playerJobs = db.TeamsPlayers.Where(c => (c.User.UsersType.TypeRole == AppRole.Players || c.User.UsersType.TypeRole == AppRole.Workers) && c.User.IdentNum.Contains(term));

                return GetUsersForUnionManagerRoleSearch(unionId, seasonId, userJobs, playerJobs, num);
            }
            var users = db.Users.Where(c => (c.UsersType.TypeRole == AppRole.Players || c.UsersType.TypeRole == AppRole.Workers) && c.IdentNum.Contains(term));
            return users.Select(u => new ListItemDto { Id = u.UserId, Name = string.Concat(u.FirstName, " ", u.LastName) }).Distinct().Take(num).ToList();
        }

        private List<ListItemDto> GetUsersForUnionManagerRoleSearch(int unionId, int seasonId, IQueryable<UsersJob> userJobs, IQueryable<TeamsPlayer> playerJobs, int num)
        {
            var union = db.Unions.FirstOrDefault(x => x.UnionId == unionId);
            var leagues = union.Leagues.Where(x => x.SeasonId == seasonId).Select(x => x.LeagueId).ToList();
            var clubs = union.Clubs.Where(x => x.SeasonId == seasonId).Select(x => x.ClubId).ToList();
            userJobs = userJobs.Where(c => c.UnionId == unionId || (leagues.Contains(c.LeagueId.Value)) || clubs.Contains(c.ClubId.Value));
            playerJobs = playerJobs.Where(c => c.League.UnionId == unionId || (leagues.Contains(c.LeagueId.Value)) || clubs.Contains(c.ClubId.Value));

            var result = userJobs.Select(u => new ListItemDto { Id = u.User.UserId, Name = string.Concat(u.User.FirstName, " ", u.User.LastName) }).Distinct().Take(num).ToList();
            if (result.Count < num)
            {
                var take = num - result.Count;
                result.AddRange(playerJobs.Select(u => new ListItemDto { Id = u.User.UserId, Name = string.Concat(u.User.FirstName, " ", u.User.LastName) }).Distinct().Take(take).ToList());
            }
            return result.Distinct().ToList();
        }

        public bool CheckEmailWithinTheSameUnion(string email, int unionId, int seasonId, int? userId = null)
        {
            var userJobs = db.UsersJobs.Where(c => (c.User.UsersType.TypeRole == AppRole.Players || c.User.UsersType.TypeRole == AppRole.Workers) && c.User.Email == email);
            var playerJobs = db.TeamsPlayers.Where(c => (c.User.UsersType.TypeRole == AppRole.Players || c.User.UsersType.TypeRole == AppRole.Workers) && c.User.Email == email);
            var union = db.Unions.FirstOrDefault(x => x.UnionId == unionId);
            var leagues = union.Leagues.Where(x => x.SeasonId == seasonId).Select(x => x.LeagueId).ToList();
            var clubs = union.Clubs.Where(x => x.SeasonId == seasonId).Select(x => x.ClubId).ToList();
            userJobs = userJobs.Where(c => c.UnionId == unionId || (leagues.Contains(c.LeagueId.Value)) || clubs.Contains(c.ClubId.Value));
            playerJobs = playerJobs.Where(c => c.League.UnionId == unionId || (leagues.Contains(c.LeagueId.Value)) || clubs.Contains(c.ClubId.Value));
            if(userId.HasValue)
            {
                userJobs = userJobs.Where(x => x.User.UserId != userId);
                playerJobs = playerJobs.Where(x => x.User.UserId != userId);
            }

            return userJobs.Count() == 0 && playerJobs.Count() == 0;
        }


        public Section GetManagersSection(int userId, int? seasonId)
        {

            var seasonedUserRole = db.UsersJobs.AsNoTracking().Where(j => j.UserId == userId && seasonId.HasValue && seasonId.Value > 0 && j.SeasonId == seasonId)?.OrderByDescending(j => j.Job.JobsRole.Priority)
                ?.FirstOrDefault();
            if(seasonedUserRole != null && seasonedUserRole.Season.UnionId.HasValue)
            {
                return seasonedUserRole.Season.Union.Section;
            }
            var userRole = db.UsersJobs.AsNoTracking().Where(j => j.UserId == userId)?.OrderByDescending(j => j.Job.JobsRole.Priority)
                ?.FirstOrDefault();
            return userRole?.Union?.Section
                ?? userRole?.Club?.Section ?? userRole?.Club?.Union?.Section
                ?? userRole?.Club1?.Section ?? userRole?.Club1?.Union?.Section
                ?? userRole?.League?.Union?.Section ?? userRole?.League?.Club?.Union?.Section
                ?? userRole?.League?.Club?.Section
                ?? userRole?.Team?.ClubTeams?.FirstOrDefault()?.Club?.Union?.Section
                ?? userRole?.Team?.ClubTeams?.FirstOrDefault()?.Club?.Section
                ?? userRole?.Team?.LeagueTeams?.FirstOrDefault()?.Leagues?.Union?.Section
                ?? userRole?.Team?.LeagueTeams?.FirstOrDefault()?.Leagues?.Club?.Section
                ?? userRole?.Team?.LeagueTeams?.FirstOrDefault()?.Leagues?.Club?.Union?.Section;
        }

        public List<User> GetClubTeamAndUnionCoaches(int? seasonId, int? unionId, int clubId, int teamId)
        {
            List<User> clubTeamAndUnionCoaches = new List<User>();
            if (unionId.HasValue && unionId > 0)
            {
                var unionCoaches = db.UsersJobs.Where(uj=> uj.UnionId == unionId && uj.SeasonId == seasonId && uj.Job.JobsRole.RoleId == UnionCoachRoleId).Select(x => x.User).ToList();
                clubTeamAndUnionCoaches.AddRange(unionCoaches);
            }

            if (clubId > 0)
            {
                var clubManagers = db.UsersJobs.Where(uj => uj.ClubId == clubId && uj.SeasonId == seasonId && uj.Job.JobsRole.RoleId == ClubManagerRoleId).Select(x => x.User).ToList();
                clubTeamAndUnionCoaches.AddRange(clubManagers);
            }

            if (teamId > 0)
            {
                var teamManagers = db.UsersJobs.Where(uj => uj.TeamId == teamId && uj.SeasonId == seasonId && uj.Job.JobsRole.RoleId == TeamManagerRoleId).Select(x => x.User).ToList();
                clubTeamAndUnionCoaches.AddRange(teamManagers);
            }
            return clubTeamAndUnionCoaches;
        }
    }
}
