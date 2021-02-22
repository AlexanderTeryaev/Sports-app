using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using AppModel;
using AutoMapper;
using DataService.DTO;

namespace DataService
{
    public class UserJobDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string JobName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string IdentNum { get; internal set; }
        public IEnumerable<KarateRefereesRank> KarateRefereeRanks { get; internal set; }
        public string ConnectedClubName { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsCompetitionRegistrationBlocked { get; set; }
        public bool Active { get; set; }
        public int? GamesCount { get; set; }
        public int? CompetitionsParticipationCount { get; set; }

        public List<int> RelatedDisciplinesIds { get; set; }
        public object DisciplinesRelatedNames { get; set; }
        public List<TravelInformationDto> TravelInformationDtos { get; set; }
        public List<DateTime> LeagueDates { get; set; }
        public DateTime CurrentDateInformation { get; set; }

        public int MartialArtsCompetitionsCount { get; set; }
        public string TeamName { get; set; }
        public string PaymentRateType { get; set; }
        public string FormatPermission { get; set; }
        public string ClubName { get; set; }
    }

    public class TravelInformationDto
    {
        public DateTime? FromHour { get; set; }
        public DateTime? ToHour { get; set; }
        public bool IsUnionTravel { get; set; }
        public bool NoTravel { get; set; }
    }

    public class JobsRepo : BaseRepo
    {
        private readonly List<string> _disciplineSections = new List<string> { "Athletics", "Gymnastic", "Motorsport" };
        private readonly List<string> neededJobs = new List<string> { JobRole.Referee, JobRole.Spectator, JobRole.Desk };

        public JobsRepo() : base() { }
        public JobsRepo(DataEntities db) : base(db) { }

        public IEnumerable<UsersJob> GetUsersJobCollection(Expression<Func<UsersJob, bool>> expression)
        {
            return db.UsersJobs.Where(expression);
        }

        public Job GetById(object id)
        {
            return db.Jobs.Find(id);
        }

        public IQueryable<Job> GetQuery(bool isArchive)
        {
            return db.Jobs.Where(t => t.IsArchive == isArchive);
        }

        public IEnumerable<Job> GetAll()
        {
            return db.Jobs;
        }

        public IEnumerable<User> GetClubManagersOfUnion(int unionId, int? seasonId)
        {
            return db.UsersJobs
                .Where(x => x.SeasonId == seasonId &&
                            (x.UnionId == unionId || x.Club.UnionId == unionId) &&
                            (x.Job.JobsRole.RoleName == JobRole.ClubManager || x.Job.JobsRole.RoleName == JobRole.ClubSecretary))
                .GroupBy(x => x.User)
                .Select(x => x.Key)
                .ToList()
                .OrderBy(x => x.FullName)
                .ToList();
        }

        public List<IGrouping<User,UsersJob>> GetTeamManagersOfLeague(int leagueId, int? seasonId)
        {
            return db.UsersJobs
                 .Where(x => x.SeasonId == seasonId && (x.LeagueId == leagueId || x.Team.LeagueTeams.Any(t => t.LeagueId == leagueId)) &&
                            x.Job.JobsRole.RoleName == JobRole.TeamManager)
                .GroupBy(x => x.User)
                .ToList();
        }

        public void Delete(Job entity)
        {
            db.Jobs.Remove(entity);
        }

        public void Add(Job entity)
        {
            db.Jobs.Add(entity);
        }

        public IEnumerable<JobsRole> GetRoles(Expression<Func<JobsRole, bool>> expression = null)
        {
            return expression == null ? db.JobsRoles.ToList() : db.JobsRoles.Where(expression).ToList();
        }

        public IEnumerable<Job> GetBySection(int sectionId)
        {
            return GetQuery(false).Where(t => t.SectionId == sectionId).ToList();
        }

        public IEnumerable<Job> GetByUnion(int unionId)
        {
            return (from j in GetQuery(false)
                    from u in j.Section.Unions
                    where u.UnionId == unionId &&
                    (j.JobsRole.RoleName == JobRole.UnionManager
                    || j.JobsRole.RoleName == JobRole.Referee
                    || j.JobsRole.RoleName == JobRole.Spectator
                    || j.JobsRole.RoleName == JobRole.Activitymanager
                    || j.JobsRole.RoleName == JobRole.ActivityRegistrationActive
                    || j.JobsRole.RoleName == JobRole.Activityviewer
                    || j.JobsRole.RoleName == JobRole.Desk
                    || j.JobsRole.RoleName == JobRole.Unionviewer
                    || j.JobsRole.RoleName == JobRole.CommitteeOfReferees
                    || j.JobsRole.RoleName == JobRole.UnionCoach
                    || j.JobsRole.RoleName == JobRole.RefereeAssignment
                    )
                    select j).ToList();
        }

        public int? GetUnionIdByUnionManagerId(int mngrId)
        {
            return db.UsersJobs.FirstOrDefault(j => j.UserId == mngrId && j.UnionId != null)?.UnionId;
        }

        public IDictionary<int, JobsRole> GetOfficialTypesByJobsIds(IEnumerable<int> jobsIds)
        {
            jobsIds = jobsIds ?? new List<int>();
            var listOfOfficials = new List<JobsRole>();
            foreach (var jobId in jobsIds)
            {
                listOfOfficials.Add(db.UsersJobs.FirstOrDefault(j => j.Id == jobId)?.Job?.JobsRole);
            }
            return listOfOfficials.GroupBy(x => x.RoleId, (key, group) => group.First()).ToDictionary(c => c.RoleId);
        }

        public int? GetLeagueIdByLeagueManagerId(int mngrId)
        {
            return db.UsersJobs.FirstOrDefault(j => j.UserId == mngrId && j.LeagueId != null)?.LeagueId;
        }

        public IEnumerable<Job> GetByDiscipline(int disciplineId)
        {
            var jobs = from j in GetQuery(false)
                       from u in j.Section.Unions
                       from d in u.Disciplines
                       where d.DisciplineId == disciplineId && j.JobsRole.RoleName == JobRole.DisciplineManager
                       select j;
            return jobs.ToList();
        }

        public IEnumerable<Job> GetByLeague(int leagueId, bool isRefereeAssignment = false)
        {
            var league = db.Leagues.FirstOrDefault(x => x.LeagueId == leagueId);

            var sectionId = league?.Union?.SectionId ?? league?.Club?.SectionId;

            if (isRefereeAssignment)
            {
                return db.Jobs.Where(x => !x.IsArchive && x.SectionId == sectionId && (x.JobsRole.RoleName == JobRole.Spectator ||
                                      x.JobsRole.RoleName == JobRole.Referee ||
                                      x.JobsRole.RoleName == JobRole.Desk));
            }
            else
            {
                return db.Jobs.Where(x => !x.IsArchive && x.SectionId == sectionId && (x.JobsRole.RoleName == JobRole.LeagueManager ||
                                          x.JobsRole.RoleName == JobRole.Referee ||
                                          x.JobsRole.RoleName == JobRole.CallRoomManager ||
                                          x.JobsRole.RoleName == JobRole.Desk));
            }

            //return (from j in GetQuery(false)
            //        from u in j.Section.Unions
            //        from l in u.Leagues
            //        where l.LeagueId == leagueId &&
            //        (j.JobsRole.RoleName == JobRole.LeagueManager || j.JobsRole.RoleName == JobRole.Referee || j.JobsRole.RoleName == JobRole.Desk)
            //        select j).ToList();
        }

        public IEnumerable<Job> GetByTeam(int teamId, int? departmentId = null)
        {
            var sectionId = GetSectionByTeamId(teamId)?.SectionId;

            if (departmentId.HasValue && sectionId == null)
            {
                sectionId = db.Clubs.FirstOrDefault(c => c.ClubId == departmentId.Value)?.SportSectionId;
            }

            return db.Jobs.Where(jb => !jb.IsArchive && jb.SectionId == sectionId && jb.JobsRole.RoleName != JobRole.CallRoomManager)
                .Join(db.JobsRoles.Where(jr => jr.RoleName == JobRole.TeamManager || jr.RoleName == JobRole.TeamViewer),
                        jb => jb.RoleId, jr => jr.RoleId, (jb, jr) => jb).ToList();
        }

        public void AddUsersJob(UsersJob job)
        {
            db.UsersJobs.Add(job);
        }

        public List<UserJobDto> GetTeamUsersJobs(int[] teamsIds, int? seasonId = null)
        {
            var result = new List<UserJobDto>();
            
            var userJobs = db.UsersJobs
                .Where(x => teamsIds.Contains(x.TeamId ?? 0) &&
                            !x.User.IsArchive &&
                            x.SeasonId == seasonId)
                .Include(x => x.User)
                .Include(x => x.Job)
                .Include(x => x.Job.JobsRole)
                .ToList();

            if (userJobs.Any())
            {
                var userJobsIds = userJobs.Select(x => x.Id).Distinct().ToArray();

                var travelInformations = db.TravelInformations
                    .Where(x => userJobsIds.Contains(x.UserJobId))
                    .ToList();

                var games = db.GamesCycles.Where(x => (!string.IsNullOrEmpty(x.DeskIds) ||
                                                       !string.IsNullOrEmpty(x.RefereeIds) ||
                                                       !string.IsNullOrEmpty(x.SpectatorIds)) &&
                                                      !x.Stage.League.IsArchive &&
                                                      x.Stage.League.SeasonId == seasonId)
                    .ToList();

                var defaultTravelInformationList = new List<TravelInformationDto> { new TravelInformationDto() };

                foreach (var userJob in userJobs)
                {
                    var jobTravelInformations = travelInformations
                        .Where(x => x.UserJobId == userJob.Id)
                        .ToList();

                    result.Add(new UserJobDto
                    {
                        Id = userJob.Id,
                        JobName = userJob.Job.JobName,
                        UserId = userJob.UserId,
                        FullName = userJob.User.FullName,
                        Address = userJob.User.Address,
                        BirthDate = userJob.User.BirthDay,
                        City = userJob.User.City,
                        Email = userJob.User.Email,

                        TravelInformationDtos = jobTravelInformations.Any()
                            ? jobTravelInformations.Select(x => new TravelInformationDto
                            {
                                FromHour = x.FromHour,
                                NoTravel = x.NoTravel ?? false,
                                ToHour = x.ToHour,
                                IsUnionTravel = x.IsUnionTravel
                            }).ToList()
                            : defaultTravelInformationList,

                        LeagueDates = userJob.League?.LeagueStartDate != null &&
                                      userJob.League?.LeagueEndDate != null && userJob.League.LeagueEndDate >=
                                      userJob.League.LeagueStartDate
                            ? Enumerable.Range(0,
                                    1 + userJob.League.LeagueEndDate.Value
                                        .Subtract(userJob.League.LeagueStartDate.Value).Days)
                                .Select(offset => userJob.League.LeagueStartDate.Value.AddDays(offset))
                                .ToList()
                            : new List<DateTime>(),

                        Phone = userJob.User.Telephone,
                        RoleId = userJob.Job.RoleId ?? 0,
                        RoleName = userJob.Job.JobsRole.RoleName ?? "",
                        IdentNum = userJob.User.IdentNum,

                        KarateRefereeRanks = userJob.KarateRefereesRanks.Where(c => c.RefereeId == userJob.Id),

                        ConnectedClubName = userJob.ConnectedClub?.Name,
                        IsBlocked = userJob.IsBlocked,
                        IsCompetitionRegistrationBlocked = userJob.IsCompetitionRegistrationBlocked,

                        GamesCount = neededJobs.Contains(userJob.Job.JobsRole.RoleName)
                            ? GetCountOfAllGamesUserHasWorkIn(userJob.UserId.ToString(), games, userJob.Id)
                            : null,

                        CompetitionsParticipationCount = GetCompetitionParticipationsWithHoursCount(userJob),

                        DisciplinesRelatedNames = !string.IsNullOrEmpty(userJob.ConnectedDisciplineIds)
                            ? GetDisciplinesNames(userJob.ConnectedDisciplineIds)
                            : string.Empty,

                        MartialArtsCompetitionsCount =
                            userJob.RefereeRegistrations.Count(r => r.IsApproved && !r.IsArchive),

                        TeamName = userJob?.Team?.Title,
                        Active = userJob.Active
                    });
                }
            }
            return result;
        }

        public List<UserJobDto> GetTeamUsersJobs(int teamId, int? seasonId = null)
        {
            return GetTeamUsersJobs(new[] {teamId}, seasonId);
        }

        private string GetDisciplinesNames(string connectedDisciplineIds)
        {
            var disciplinesIds = connectedDisciplineIds.Split(',').Select(int.Parse)?.AsEnumerable();
            var connectedDisciplinesNames = db.Disciplines.AsNoTracking().Where(d => disciplinesIds.Contains(d.DisciplineId))
                ?.Select(d => d.Name);
            return connectedDisciplinesNames.Any() ? string.Join(",", connectedDisciplinesNames) : string.Empty;
        }

        private int? GetCountOfGames(string id, string roleName, List<GamesCycle> games, int officialId)
        {
            switch (roleName)
            {
                case JobRole.Referee:
                    var notIndividualCount = games.Count(c => c.RefereeIds?.Contains(id) == true);
                    var individualCount = db.RefereeRegistrations.Count(c => c.RefereeId == officialId && !c.IsArchive);
                    return notIndividualCount + individualCount;
                case JobRole.Spectator:
                    return games.Count(c => c.SpectatorIds?.Contains(id) == true);
                case JobRole.Desk:
                    return games.Count(c => c.DeskIds?.Contains(id) == true);
                default:
                    return null;
            }
        }

        private int? GetCountOfAllGamesUserHasWorkIn(string id, List<GamesCycle> games, int officialId)
        {
            var notIndividualCount = games.Count(c => c.RefereeIds?.Contains(id) == true);
            var individualCount = db.RefereeRegistrations.Count(c => c.RefereeId == officialId && !c.IsArchive);
            var spectatorsCount = games.Count(c => c.SpectatorIds?.Contains(id) == true);
            var deskJobsCount = games.Count(c => c.DeskIds?.Contains(id) == true);
            return notIndividualCount + individualCount + spectatorsCount + deskJobsCount;
        }

        public IEnumerable GetAllExceptRefereesJobList(int unionId)
        {
            return (from j in GetQuery(false)
                    from u in j.Section.Unions
                    where u.UnionId == unionId &&
                    (j.JobsRole.RoleName == JobRole.UnionManager
                    || j.JobsRole.RoleName == JobRole.Activitymanager
                    || j.JobsRole.RoleName == JobRole.ActivityRegistrationActive
                    || j.JobsRole.RoleName == JobRole.Activityviewer
                    || j.JobsRole.RoleName == JobRole.Desk
                    || j.JobsRole.RoleName == JobRole.Unionviewer
                    || j.JobsRole.RoleName == JobRole.UnionCoach
                    || j.JobsRole.RoleName == JobRole.RefereeAssignment)
                    select j).ToList();
        }

        public IEnumerable GetUnionRefereesJobList(int unionId)
        {
            return (from j in GetQuery(false)
                    from u in j.Section.Unions
                    where u.UnionId == unionId &&
                    (j.JobsRole.RoleName == JobRole.Referee
                    || j.JobsRole.RoleName == JobRole.Spectator
                    || j.JobsRole.RoleName == JobRole.CommitteeOfReferees
                    || j.JobsRole.RoleName == JobRole.RefereeAssignment)
                    select j).ToList();
        }

        public List<League> GetAllTennisLeagues(int userId)
        {
            var userJobs = db.UsersJobs.Where(j => j.UserId == userId);
            var leagueList = new List<League>();
            foreach (var job in userJobs)
            {
                var sectionAlias = job?.Union?.Section?.Alias ?? job?.Club?.Section?.Alias ?? job?.Club?.Union?.Section?.Alias
                    ?? job?.League?.Union?.Section?.Alias ?? job?.League?.Club?.Section?.Alias ?? job?.League?.Club?.Union?.Section?.Alias
                    ?? String.Empty;
                var unionId = job?.Union?.UnionId ?? job?.Club?.UnionId ?? job?.League?.UnionId ?? job?.Club?.UnionId;
                if (sectionAlias.Equals(SectionAliases.Tennis)
                    && job.Job.JobsRole.RoleName == JobRole.Referee || job.Job.JobsRole.RoleName == JobRole.Spectator)
                {
                    var leagues = db.Leagues.Where(r => !r.IsArchive && (r.EilatTournament == null || r.EilatTournament == false)
                            && (r.UnionId == unionId || r.Club.UnionId == unionId));
                    if (leagues.Any())
                    {
                        foreach (var league in leagues)
                        {
                            var leagueGameCycles = db.GamesCycles.Where(r => r.Stage.LeagueId == league.LeagueId);
                            if (leagueGameCycles.Any())
                            {
                                foreach (var gc in leagueGameCycles)
                                {
                                    if (!string.IsNullOrEmpty(gc.RefereeIds) &&
                                        gc.RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?
                                        .Select(int.Parse)
                                        .Any(x => x == userId) == true)
                                    {
                                        leagueList.Add(league);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return leagueList;
        }

        public int? GetTeamIdByTeamManagerId(int mngrId)
        {
            return db.UsersJobs.FirstOrDefault(j => j.UserId == mngrId && j.TeamId != null)?.TeamId;
        }

        public List<UserJobDto> GetDisciplineUsersJobs(params int[] disciplinesIds)
        {
            var allJobs = db.UsersJobs.Where(uj => disciplinesIds.Contains(uj.DisciplineId ?? 0) && !uj.User.IsArchive)
                .ToList();

            var connectedUsers = db.UsersJobs.Where(uj => !string.IsNullOrEmpty(uj.ConnectedDisciplineIds))
                .ToList()
                .Where(uj => uj.ConnectedDisciplineIds.Split(',').Select(int.Parse).Any(disciplinesIds.Contains));

            var userJobs = allJobs.Union(connectedUsers).ToList();

            var result = new List<UserJobDto>();
            if (userJobs.Any())
            {
                foreach (var userJob in userJobs)
                {
                    var relatedDisciplinesIds = new List<int>();
                    if (userJob.DisciplineId > 0)
                    {
                        relatedDisciplinesIds.Add(userJob.DisciplineId.Value);
                    }

                    if (!string.IsNullOrWhiteSpace(userJob.ConnectedDisciplineIds))
                    {
                        var splitted = userJob.ConnectedDisciplineIds.Split(',');
                        foreach (var s in splitted)
                        {
                            int disciplineId;
                            if (int.TryParse(s, out disciplineId))
                            {
                                relatedDisciplinesIds.Add(disciplineId);
                            }
                        }
                    }

                    var defaultTravelInformationList = new List<TravelInformationDto>();
                    defaultTravelInformationList.Add(new TravelInformationDto());
                    result.Add(new UserJobDto
                    {
                        Id = userJob.Id,
                        JobName = userJob.Job.JobName,
                        UserId = userJob.UserId,
                        FullName = userJob.User.FullName,
                        Address = userJob.User.Address,
                        BirthDate = userJob.User.BirthDay,
                        City = userJob.User.City,
                        Email = userJob.User.Email,
                        Phone = userJob.User.Telephone,
                        TravelInformationDtos = userJob.TravelInformations.Any() ? userJob.TravelInformations.Select(x => new TravelInformationDto
                        {
                            FromHour = x.FromHour,
                            NoTravel = x.NoTravel ?? false,
                            ToHour = x.ToHour,
                            IsUnionTravel = x.IsUnionTravel
                        }).ToList() : defaultTravelInformationList,
                        LeagueDates = userJob.League?.LeagueStartDate != null && userJob.League?.LeagueEndDate != null && userJob.League.LeagueEndDate >= userJob.League.LeagueStartDate
                            ? Enumerable.Range(0, 1 + userJob.League.LeagueEndDate.Value.Subtract(userJob.League.LeagueStartDate.Value).Days)
                            .Select(offset => userJob.League.LeagueStartDate.Value.AddDays(offset))
                            .ToList() 
                            : new List<DateTime>(),
                        RoleId = userJob.Job.RoleId ?? 0,
                        RoleName = userJob.Job.JobsRole.RoleName ?? "",
                        IdentNum = userJob.User.IdentNum,
                        KarateRefereeRanks = userJob.KarateRefereesRanks.Where(c => c.RefereeId == userJob.Id),
                        ConnectedClubName = userJob.ConnectedClub?.Name,
                        IsBlocked = userJob.IsBlocked,
                        IsCompetitionRegistrationBlocked = userJob.IsCompetitionRegistrationBlocked,

                        RelatedDisciplinesIds = relatedDisciplinesIds,
                        DisciplinesRelatedNames = !string.IsNullOrEmpty(userJob.ConnectedDisciplineIds)
                            ? GetDisciplinesNames(userJob.ConnectedDisciplineIds)
                            : string.Empty,

                        TeamName = userJob.Team?.Title,
                        Active = userJob.Active
                    });
                }
            }

            return result;
        }

        public List<UserJobDto> GetUnionUsersJobs(int unionId, int? seasonId = null)
        {
            var result = new List<UserJobDto>();
            var union = db.Unions.FirstOrDefault(u => u.UnionId == unionId);
            var userJobs = db.UsersJobs.Where(uj => uj.UnionId == unionId && !uj.User.IsArchive && uj.SeasonId == seasonId);
            var games = db.GamesCycles.Where(g => !string.IsNullOrEmpty(g.DeskIds)
               || !string.IsNullOrEmpty(g.RefereeIds)
               || !string.IsNullOrEmpty(g.SpectatorIds));

            var gamesBySeason = games.Where(g => !g.Stage.League.IsArchive && g.Stage.League.SeasonId == seasonId)?.ToList();

            if (userJobs.Any())
            {
                var defaultTravelInformationList = new List<TravelInformationDto>();
                defaultTravelInformationList.Add(new TravelInformationDto());
                var userJobsList = userJobs.ToList();
                foreach (var userJob in userJobsList)
                {
                    result.Add(new UserJobDto
                    {
                        Id = userJob.Id,
                        JobName = userJob.Job.JobName,
                        UserId = userJob.UserId,
                        FullName = userJob.User.FullName,
                        Address = userJob.User.Address,
                        BirthDate = userJob.User.BirthDay,
                        City = userJob.User.City,
                        Email = userJob.User.Email,
                        TravelInformationDtos = userJob.TravelInformations.Any() ? userJob.TravelInformations.Select(x => new TravelInformationDto
                        {
                            FromHour = x.FromHour,
                            NoTravel = x.NoTravel ?? false,
                            ToHour = x.ToHour,
                            IsUnionTravel = x.IsUnionTravel
                        }).ToList() : defaultTravelInformationList,
                        LeagueDates = userJob.League?.LeagueStartDate != null && userJob.League?.LeagueEndDate != null && userJob.League.LeagueEndDate >= userJob.League.LeagueStartDate
                            ? Enumerable.Range(0, 1 + userJob.League.LeagueEndDate.Value.Subtract(userJob.League.LeagueStartDate.Value).Days)
                            .Select(offset => userJob.League.LeagueStartDate.Value.AddDays(offset))
                            .ToList() 
                            : new List<DateTime>(),
                        Phone = userJob.User.Telephone,
                        RoleId = userJob.Job.RoleId ?? 0,
                        RoleName = userJob.Job.JobsRole.RoleName ?? "",
                        IdentNum = userJob.User.IdentNum,
                        KarateRefereeRanks = userJob.KarateRefereesRanks.Where(c => c.RefereeId == userJob.Id),
                        ConnectedClubName = userJob.ConnectedClub?.Name,
                        IsBlocked = userJob.IsBlocked,
                        IsCompetitionRegistrationBlocked = userJob.IsCompetitionRegistrationBlocked,
                        GamesCount = neededJobs.Contains(userJob.Job.JobsRole.RoleName)
                            ? GetCountOfAllGamesUserHasWorkIn(userJob.UserId.ToString(), gamesBySeason, userJob.Id)
                            : null,
                        CompetitionsParticipationCount = GetCompetitionParticipationsWithHoursCount(userJob),
                        DisciplinesRelatedNames = !string.IsNullOrEmpty(userJob.ConnectedDisciplineIds)
                            ? GetDisciplinesNames(userJob.ConnectedDisciplineIds)
                            : string.Empty,
                        MartialArtsCompetitionsCount = userJob.RefereeRegistrations.Count(r => r.IsApproved && !r.IsArchive),
                        TeamName = userJob?.Team?.Title,
                        ClubName = userJob?.Club?.Name,
                        PaymentRateType = userJob.RateType,
                        Active = userJob.Active
                    });
                }
            }
            return result;
        }

        private int? GetCompetitionParticipationsWithHoursCount(UsersJob userJob)
        {
            return db.UsersJobs.Count(x => x.LeagueId != null &&
                                            x.UserId == userJob.UserId &&
                                            x.SeasonId == userJob.SeasonId &&
                                            x.TravelInformations.Any(t => t.FromHour.HasValue) &&
                                            x.TravelInformations.Any(t => t.ToHour.HasValue) &&
                                            x.Job.JobsRole.RoleName != null &&
                                            neededJobs.Contains(x.Job.JobsRole.RoleName));
        }

        public List<UserJobDto> GetLeagueUsersJobs(int leagueId, int? seasonId = null)
        {
            var result = new List<UserJobDto>();

            var userJobs = db.UsersJobs.Where(x => x.LeagueId == leagueId &&
                                                   !x.User.IsArchive &&
                                                   x.SeasonId == seasonId)
                .Include(x => x.User)
                .Include(x => x.Job)
                .Include(x => x.Job.JobsRole)
                .ToList();

            if (userJobs.Any())
            {
                var userJobsIds = userJobs.Select(x => x.Id).ToArray();

                var travelInformations = db.TravelInformations
                    .Where(x => userJobsIds.Contains(x.UserJobId))
                    .ToList();

                var games = db.GamesCycles.Where(x => (!string.IsNullOrEmpty(x.DeskIds)
                                                       || !string.IsNullOrEmpty(x.RefereeIds)
                                                       || !string.IsNullOrEmpty(x.SpectatorIds)) &&
                                                      !x.Stage.League.IsArchive &&
                                                      x.Stage.League.SeasonId == seasonId)
                    .ToList();

                var defaultTravelInformationList = new List<TravelInformationDto> {new TravelInformationDto()};

                foreach (var userJob in userJobs)
                {
                    var jobTravelInformations = travelInformations
                        .Where(x => x.UserJobId == userJob.Id)
                        .ToList();

                    result.Add(new UserJobDto
                    {
                        Id = userJob.Id,
                        JobName = userJob.Job.JobName,
                        UserId = userJob.UserId,
                        FullName = userJob.User.FullName,
                        Address = userJob.User.Address,
                        BirthDate = userJob.User.BirthDay,
                        City = userJob.User.City,
                        Email = userJob.User.Email,
                        Phone = userJob.User.Telephone,
                        RoleId = userJob.Job.RoleId ?? 0,
                        RoleName = userJob.Job.JobsRole.RoleName ?? "",
                        IdentNum = userJob.User.IdentNum,

                        KarateRefereeRanks = userJob.KarateRefereesRanks.Where(c => c.RefereeId == userJob.Id),

                        ConnectedClubName = userJob.ConnectedClub?.Name,
                        IsBlocked = userJob.IsBlocked,
                        IsCompetitionRegistrationBlocked = userJob.IsCompetitionRegistrationBlocked,

                        GamesCount = neededJobs.Contains(userJob.Job.JobsRole.RoleName)
                            ? GetCountOfAllGamesUserHasWorkIn(userJob.UserId.ToString(), games, userJob.Id)
                            : null,

                        CompetitionsParticipationCount = GetCompetitionParticipationsWithHoursCount(userJob),

                        DisciplinesRelatedNames = !string.IsNullOrEmpty(userJob.ConnectedDisciplineIds)
                            ? GetDisciplinesNames(userJob.ConnectedDisciplineIds)
                            : string.Empty,

                        MartialArtsCompetitionsCount =
                            userJob.RefereeRegistrations.Count(r => r.IsApproved && !r.IsArchive),

                        TeamName = userJob?.Team?.Title,
                        PaymentRateType = userJob.RateType,

                        TravelInformationDtos = jobTravelInformations.Any()
                            ? jobTravelInformations.Select(x => new TravelInformationDto
                            {
                                FromHour = x.FromHour,
                                NoTravel = x.NoTravel ?? false,
                                ToHour = x.ToHour,
                                IsUnionTravel = x.IsUnionTravel
                            }).ToList()
                            : defaultTravelInformationList,

                        LeagueDates = userJob.League?.LeagueStartDate != null &&
                                      userJob.League?.LeagueEndDate != null && userJob.League.LeagueEndDate >=
                                      userJob.League.LeagueStartDate
                            ? Enumerable.Range(0,
                                    1 + userJob.League.LeagueEndDate.Value
                                        .Subtract(userJob.League.LeagueStartDate.Value).Days)
                                .Select(offset => userJob.League.LeagueStartDate.Value.AddDays(offset))
                                .ToList()
                            : new List<DateTime>(),

                        Active = userJob.Active,
                        FormatPermission = userJob.FormatPermissions
                    });
                }
            }
            return result;
        }

        public List<UserJobDto> GetClubUsersJobs(int clubId, int? seasonId = null)
        {
            var result = new List<UserJobDto>();
            var userJobs = db.UsersJobs.Where(uj => uj.ClubId == clubId && !uj.User.IsArchive);
            var games = db.GamesCycles.Where(g => !string.IsNullOrEmpty(g.DeskIds)
               || !string.IsNullOrEmpty(g.RefereeIds)
               || !string.IsNullOrEmpty(g.SpectatorIds));

            var gamesBySeason = games.Where(g => !g.Stage.League.IsArchive && g.Stage.League.SeasonId == seasonId)?.ToList();
            if (userJobs.Any())
            {
                foreach (var userJob in userJobs)
                {
                    result.Add(new UserJobDto
                    {
                        Id = userJob.Id,
                        JobName = userJob.Job.JobName,
                        UserId = userJob.UserId,
                        FullName = userJob.User.FullName,
                        Address = userJob.User.Address,
                        BirthDate = userJob.User.BirthDay,
                        City = userJob.User.City,
                        Email = userJob.User.Email,
                        Phone = userJob.User.Telephone,
                        RoleId = userJob.Job.RoleId ?? 0,
                        RoleName = userJob.Job.JobsRole.RoleName ?? "",
                        IdentNum = userJob.User.IdentNum,
                        KarateRefereeRanks = userJob.KarateRefereesRanks.Where(c => c.RefereeId == userJob.Id),
                        ConnectedClubName = userJob.ConnectedClub?.Name,
                        IsBlocked = userJob.IsBlocked,
                        IsCompetitionRegistrationBlocked = userJob.IsCompetitionRegistrationBlocked,
                        GamesCount = neededJobs.Contains(userJob.Job.JobsRole.RoleName)
                            ? GetCountOfAllGamesUserHasWorkIn(userJob.UserId.ToString(), gamesBySeason, userJob.Id)
                            : null,
                        CompetitionsParticipationCount = GetCompetitionParticipationsWithHoursCount(userJob),
                        DisciplinesRelatedNames = !string.IsNullOrEmpty(userJob.ConnectedDisciplineIds)
                            ? GetDisciplinesNames(userJob.ConnectedDisciplineIds)
                            : string.Empty,
                        MartialArtsCompetitionsCount = userJob.RefereeRegistrations.Count(r => r.IsApproved && !r.IsArchive),
                        TeamName = userJob?.Team?.Title,
                        ClubName = userJob?.Club?.Name,
                        PaymentRateType = userJob.RateType,
                        Active = userJob.Active
                    });
                }
            }
            return result;
        }
        public List<UserJobDto> GetDepartUsersJobs(Club club, int? seasonId = null)
        {
            var result = new List<UserJobDto>();
            var userJobs = db.UsersJobs.Where(uj => uj.Club.ParentClubId == club.ClubId && !uj.User.IsArchive);
            var games = db.GamesCycles.Where(g => !string.IsNullOrEmpty(g.DeskIds)
               || !string.IsNullOrEmpty(g.RefereeIds)
               || !string.IsNullOrEmpty(g.SpectatorIds));

            var gamesBySeason = games.Where(g => !g.Stage.League.IsArchive && g.Stage.League.SeasonId == seasonId)?.ToList();
            if (userJobs.Any())
            {
                foreach (var userJob in userJobs)
                {
                    result.Add(new UserJobDto
                    {
                        Id = userJob.Id,
                        JobName = userJob.Job.JobName,
                        UserId = userJob.UserId,
                        FullName = userJob.User.FullName,
                        Address = userJob.User.Address,
                        BirthDate = userJob.User.BirthDay,
                        City = userJob.User.City,
                        Email = userJob.User.Email,
                        Phone = userJob.User.Telephone,
                        RoleId = userJob.Job.RoleId ?? 0,
                        RoleName = userJob.Job.JobsRole.RoleName ?? "",
                        IdentNum = userJob.User.IdentNum,
                        KarateRefereeRanks = userJob.KarateRefereesRanks.Where(c => c.RefereeId == userJob.Id),
                        ConnectedClubName = userJob.ConnectedClub?.Name,
                        IsBlocked = userJob.IsBlocked,
                        IsCompetitionRegistrationBlocked = userJob.IsCompetitionRegistrationBlocked,
                        GamesCount = neededJobs.Contains(userJob.Job.JobsRole.RoleName)
                            ? GetCountOfAllGamesUserHasWorkIn(userJob.UserId.ToString(), gamesBySeason, userJob.Id)
                            : null,
                        CompetitionsParticipationCount = GetCompetitionParticipationsWithHoursCount(userJob),
                        DisciplinesRelatedNames = !string.IsNullOrEmpty(userJob.ConnectedDisciplineIds)
                            ? GetDisciplinesNames(userJob.ConnectedDisciplineIds)
                            : string.Empty,
                        MartialArtsCompetitionsCount = userJob.RefereeRegistrations.Count(r => r.IsApproved && !r.IsArchive),
                        TeamName = userJob?.Team?.Title,
                        PaymentRateType = userJob.RateType,
                        Active = userJob.Active
                    });
                }
            }
            return result;
        }
        public int CountOfficialsInClub(int clubId, int? seasonId)
        {
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId);
            var count = 0;
            if (club != null)
            {
                var clubOfficialsCount = club.UsersJobs.Where(u => u.SeasonId == seasonId).Distinct().ToList().Count;
                var clubTeamsOfficialsCount = club.ClubTeams.Where(u => u.SeasonId == seasonId).Distinct().ToList().Select(c => c.Team).Sum(c => c.UsersJobs.Count);
                var clubSchoolTeamsOfficialsCount = 0;
                var clubSchoolTeamsOfficials = club.Schools;
                if (clubSchoolTeamsOfficials != null && clubSchoolTeamsOfficials.Any())
                {
                    foreach (var school in clubSchoolTeamsOfficials)
                    {
                        var schoolTeams = school?.SchoolTeams.Select(c => c.Team);
                        if (schoolTeams != null && schoolTeams.Any())
                            clubSchoolTeamsOfficialsCount += schoolTeams.Sum(schoolTeam => schoolTeam.UsersJobs.Where(u => u.SeasonId == seasonId).Distinct().ToList().Count);
                    }
                }
                count = clubOfficialsCount + clubTeamsOfficialsCount + clubSchoolTeamsOfficialsCount;
            }
            return count;
        }

        public int CountOfficialsInLeague(int leagueId)
        {
            return (from userJob in db.UsersJobs
                    join user in db.Users on userJob.UserId equals user.UserId
                    join job in db.Jobs on userJob.JobId equals job.JobId
                    where userJob.LeagueId.HasValue && userJob.LeagueId == leagueId &&
                          user.IsArchive == false
                    select userJob).Count();
        }

        public Job GetJobItem(int jobId)
        {
            return db.Jobs.Find(jobId);
        }

        public UsersJob GetUsersJobById(int id)
        {
            return db.UsersJobs.Find(id);
        }

        public void RemoveUsersJob(UsersJob job)
        {
            db.UsersJobs.Remove(job);
        }


        public void RemoveUsersJob(int id)
        {
            var userJob = db.UsersJobs.FirstOrDefault(x => x.Id == id);
            if (userJob?.KarateRefereesRanks != null && userJob.KarateRefereesRanks.Any())
            {
                db.KarateRefereesRanks.RemoveRange(userJob.KarateRefereesRanks);
            }
            if (userJob?.TravelInformations != null && userJob.TravelInformations.Any())
            {
                db.TravelInformations.RemoveRange(userJob.TravelInformations);
            }
            if (userJob != null && userJob.OfficialGameReportDetails.Count()>0) {
                db.OfficialGameReportDetails.RemoveRange(userJob.OfficialGameReportDetails.ToList());
            }

            db.UsersJobs.Remove(userJob);
            db.SaveChanges();
        }

        public void UpdateWorkTimes(int id, DateTime? FromHour, DateTime? ToHour, DateTime day, bool isUnionTravel, bool noTravel)
        {
            var newFromHour = new DateTime(day.Year, day.Month, day.Day, FromHour.HasValue ? FromHour.Value.Hour : 0, FromHour.HasValue ? FromHour.Value.Minute : 0, 0);
            var newToHour = new DateTime(day.Year, day.Month, day.Day, ToHour.HasValue ? ToHour.Value.Hour : 0, ToHour.HasValue ? ToHour.Value.Minute : 0, 0);

            var travelInformations = db.TravelInformations.Where(x => x.UserJobId == id).ToList();
            var currentTravelInformation = travelInformations.FirstOrDefault(x=>(x.FromHour.HasValue && x.FromHour.Value.Date == day.Date) || (x.ToHour.HasValue && x.ToHour.Value.Date == day.Date));

            if (currentTravelInformation != null)
            {
                currentTravelInformation.FromHour = newFromHour;
                currentTravelInformation.ToHour = newToHour;
                currentTravelInformation.IsUnionTravel = isUnionTravel;
                currentTravelInformation.NoTravel = noTravel;
                db.SaveChanges();
            }
            else
            {
                currentTravelInformation = new TravelInformation();
                currentTravelInformation.FromHour = newFromHour;
                currentTravelInformation.ToHour = newToHour;
                currentTravelInformation.IsUnionTravel = isUnionTravel;
                currentTravelInformation.NoTravel = noTravel;
                currentTravelInformation.UserJobId = id;
                db.TravelInformations.Add(currentTravelInformation);
                db.SaveChanges();
            }
        }

        public void UpdateWorkTimesAll(List<int> userJobsIds, DateTime? FromHour, DateTime? ToHour, DateTime day)
        {
            var newFromHour = new DateTime(day.Year, day.Month, day.Day, FromHour.HasValue ? FromHour.Value.Hour : 0, FromHour.HasValue ? FromHour.Value.Minute : 0, 0);
            var newToHour = new DateTime(day.Year, day.Month, day.Day, ToHour.HasValue ? ToHour.Value.Hour : 0, ToHour.HasValue ? ToHour.Value.Minute : 0, 0);

            var travelInformations = db.TravelInformations.Where(x => userJobsIds.Contains(x.UserJobId)).ToList();
            var travelInformationsForSelectedDates = travelInformations.Where(x=> (x.FromHour.HasValue && x.FromHour.Value.Date == day.Date) || (x.ToHour.HasValue && x.ToHour.Value.Date == day.Date)).ToList();
            foreach (var userJobId in userJobsIds)
            { 
                TravelInformation travelInformation = travelInformationsForSelectedDates.FirstOrDefault(x => x.UserJobId == userJobId);
                if (travelInformation != null)
                {
                    travelInformation.FromHour = newFromHour;
                    travelInformation.ToHour = newToHour;
                }
                else
                {
                    travelInformation = new TravelInformation();
                    travelInformation.FromHour = newFromHour;
                    travelInformation.ToHour = newToHour;
                    travelInformation.UserJobId = userJobId;
                    db.TravelInformations.Add(travelInformation);
                }
            }
            db.SaveChanges();
        }


        public bool IsUserInJob(LogicaName logicaName, int relevantEntityId, int jobId, int userId, int seasonId)
        {
            switch (logicaName)
            {
                case LogicaName.Union:
                    return db.UsersJobs.Any(uj => uj.JobId == jobId && uj.UserId == userId && uj.UnionId == relevantEntityId && uj.SeasonId == seasonId);
                case LogicaName.League:
                    return db.UsersJobs.Any(uj => uj.JobId == jobId && uj.UserId == userId && uj.LeagueId == relevantEntityId && uj.SeasonId == seasonId);
                case LogicaName.Team:
                    return db.UsersJobs.Any(uj => uj.JobId == jobId && uj.UserId == userId && uj.TeamId == relevantEntityId && uj.SeasonId == seasonId);
                case LogicaName.RegionalFederation:
                    return db.UsersJobs.Any(uj => uj.JobId == jobId && uj.UserId == userId && uj.RegionalId == relevantEntityId && uj.SeasonId == seasonId);
            }
            return false;
        }

        public IEnumerable<KarateRefereesRank> GetAllRefereesRanks(int jobId)
        {
            return db.KarateRefereesRanks.Where(c => c.RefereeId == jobId);
        }

        public List<Job> GetClubJobs(int clubId)
        {
            var club = db.Clubs.FirstOrDefault(x => x.ClubId == clubId);
            if (club == null) return new List<Job>();

            int? sectionId = null;
            if (club.IsSectionClub ?? true)
            {
                sectionId = club.SectionId;
            }
            else
            {
                sectionId = club.Union.SectionId;
            }
            var jobs = new List<Job>();
            if (club.ParentClub == null)
            {
                jobs = db.Jobs.Join(db.JobsRoles
                    .Where(jr => jr.RoleName == JobRole.ClubManager || jr.RoleName == JobRole.ClubSecretary || jr.RoleName == JobRole.ActivityRegistrationActive
                    || jr.RoleName == JobRole.Spectator || jr.RoleName == JobRole.Desk)
                    , j => j.RoleId, jr => jr.RoleId, (j, jr) => j)
                    .Where(j => j.SectionId == sectionId).ToList();
            }
            else
            {
                jobs = db.Jobs.Join(db.JobsRoles
                    .Where(jr => jr.RoleName == JobRole.ActivityRegistrationActive
                    || jr.RoleName == JobRole.Spectator || jr.RoleName == JobRole.DepartmentManager || jr.RoleName == JobRole.Desk)
                    , j => j.RoleId, jr => jr.RoleId, (j, jr) => j)
                    .Where(j => j.SectionId == sectionId).ToList();
            }


            return jobs;
        }

        public IEnumerable<UsersJob> GetAllUsersJobs(int userId)
        {
            return db.UsersJobs.Where(j => j.UserId == userId && !j.Job.IsArchive);
        }

        public UsersJob GetUnionUserJob(int userId)
        {
            return db.UsersJobs.FirstOrDefault(j => j.UserId == userId && j.UnionId != null);
        }

        public List<UserJobDto> GetClubOfficials(int clubId, int? seasonId)
        {
            var result = new List<UserJobDto>();

            var userJobs = db.UsersJobs
                .Where(uj => (uj.ClubId == clubId || uj.ConnectedClubId == clubId) && !uj.User.IsArchive && uj.SeasonId == seasonId)
                .ToList()
                .OrderBy(uj => uj.User.FullName)
                .ToList();
            var games = db.GamesCycles.Where(g => !string.IsNullOrEmpty(g.DeskIds)
               || !string.IsNullOrEmpty(g.RefereeIds)
               || !string.IsNullOrEmpty(g.SpectatorIds));

            var gamesBySeason = games.Where(g => !g.Stage.League.IsArchive && g.Stage.League.SeasonId == seasonId)?.ToList();

            if (userJobs.Any())
            {
                var defaultTravelInformationList = new List<TravelInformationDto>();
                defaultTravelInformationList.Add(new TravelInformationDto());
                foreach (var userJob in userJobs)
                {
                    result.Add(new UserJobDto
                    {
                        Id = userJob.Id,
                        JobName = userJob.Job.JobName,
                        UserId = userJob.UserId,
                        FullName = userJob.User.FullName,
                        Address = userJob.User.Address,
                        BirthDate = userJob.User.BirthDay,
                        City = userJob.User.City,
                        Email = userJob.User.Email,
                        TravelInformationDtos = userJob.TravelInformations.Any() ? userJob.TravelInformations.Select(x => new TravelInformationDto
                        {
                            FromHour = x.FromHour,
                            NoTravel = x.NoTravel ?? false,
                            ToHour = x.ToHour,
                            IsUnionTravel = x.IsUnionTravel
                        }).ToList() : defaultTravelInformationList,
                        LeagueDates = userJob.League?.LeagueStartDate != null && userJob.League?.LeagueEndDate != null && userJob.League.LeagueEndDate >= userJob.League.LeagueStartDate
                            ? Enumerable.Range(0, 1 + userJob.League.LeagueEndDate.Value.Subtract(userJob.League.LeagueStartDate.Value).Days)
                            .Select(offset => userJob.League.LeagueStartDate.Value.AddDays(offset))
                            .ToList() 
                            : new List<DateTime>(),
                        Phone = userJob.User.Telephone,
                        RoleId = userJob.Job.RoleId ?? 0,
                        RoleName = userJob.Job.JobsRole.RoleName ?? "",
                        IdentNum = userJob.User.IdentNum,
                        KarateRefereeRanks = userJob.KarateRefereesRanks.Where(c => c.RefereeId == userJob.Id),
                        ConnectedClubName = userJob.ConnectedClub?.Name,
                        IsBlocked = userJob.IsBlocked,
                        IsCompetitionRegistrationBlocked = userJob.IsCompetitionRegistrationBlocked,
                        GamesCount = neededJobs.Contains(userJob.Job.JobsRole.RoleName)
                            ? GetCountOfAllGamesUserHasWorkIn(userJob.UserId.ToString(), gamesBySeason, userJob.Id)
                            : null,
                        CompetitionsParticipationCount = GetCompetitionParticipationsWithHoursCount(userJob),
                        DisciplinesRelatedNames = !string.IsNullOrEmpty(userJob.ConnectedDisciplineIds)
                            ? GetDisciplinesNames(userJob.ConnectedDisciplineIds)
                            : string.Empty,
                        MartialArtsCompetitionsCount = userJob.RefereeRegistrations.Count(r => r.IsApproved && !r.IsArchive),
                        TeamName = userJob.Team?.Title,
                        PaymentRateType = userJob.RateType,
                        Active = userJob.Active
                    });
                }
            }

            return result;
        }

        public int? GetClubIdByClubManagerId(int mngrId)
        {
            var clubId = db.UsersJobs.FirstOrDefault(j => j.UserId == mngrId && j.ClubId != null)?.ClubId;
            return clubId;
        }

        public int? GetClubIdByClubManagerIdForNavSearch(int mngrId, int? seasonId = null)
        {
            var clubId = db.UsersJobs.FirstOrDefault(j => j.UserId == mngrId && j.ClubId != null && (!seasonId.HasValue || seasonId.Value == j.Club.SeasonId || seasonId.Value == j.SeasonId))?.ClubId;
            return clubId;
        }

        public bool IsClubManager(int mngrId, int clubId)
        {
            return db.UsersJobs.Any(j => j.UserId == mngrId && j.ClubId == clubId);
        }

        public bool IsTeamManager(int mngrId, int teamId)
        {
            return db.UsersJobs.Any(j => j.UserId == mngrId && j.TeamId == teamId);
        }

        public bool IsActivityManager(int userId)
        {
            return (from u in db.Users
                    from j in u.UsersJobs
                    let r = j.Job.JobsRole
                    where u.UserId == userId && r.RoleName == JobRole.Activitymanager
                    select r.RoleName).Any();
        }

        public void CreteRankForReferee(int id, string type, DateTime date)
        {
            db.KarateRefereesRanks.Add(new KarateRefereesRank
            {
                RefereeId = id,
                Type = type,
                Date = date
            });
        }

        public void DeleteCurrentRanks(UsersJob userJob)
        {
            if (userJob.KarateRefereesRanks.Any())
            {
                db.KarateRefereesRanks.RemoveRange(userJob.KarateRefereesRanks);
            }
        }

        public bool IsActivityViewer(int userId)
        {
            return (from u in db.Users
                    from j in u.UsersJobs
                    let r = j.Job.JobsRole
                    where u.UserId == userId && r.RoleName == JobRole.Activityviewer
                    select r.RoleName).Any();
        }

        public bool IsActivityRegistrationActive(int userId)
        {
            return (from u in db.Users
                    from j in u.UsersJobs
                    let r = j.Job.JobsRole
                    where u.UserId == userId && r.RoleName == JobRole.ActivityRegistrationActive
                    select r.RoleName).Any();
        }

        public JobsRole GetJobRoleByRoleName(string roleName)
        {
            return db.JobsRoles.FirstOrDefault(x => x.RoleName == roleName);
        }

        public IEnumerable<UserJobDto> GetAllUsersJobs(int id, LogicaName logicalName, int? seasonId)
        {
            var allUserJobs = new List<UserJobDto>();
            switch (logicalName)
            {
                case LogicaName.Union:
                    var union = db.Unions.Include("Leagues.LeagueTeams").FirstOrDefault(u => u.UnionId == id);
                    if (union != null)
                    {
                        allUserJobs.AddRange(GetUnionUsersJobs(union.UnionId, seasonId));

                        var hasDisciplines = _disciplineSections.Contains(union.Section.Alias);

                        foreach (var unionClub in union.Clubs)
                        {
                            allUserJobs.AddRange(GetClubUsersJobs(unionClub.ClubId, seasonId));

                            foreach (var clubTeam in unionClub.ClubTeams)
                            {
                                allUserJobs.AddRange(GetTeamUsersJobs(clubTeam.TeamId, seasonId));
                            }
                        }
                        foreach (var league in union.Leagues.Where(l => l.SeasonId == seasonId))
                        {
                            allUserJobs.AddRange(GetLeagueUsersJobs(league.LeagueId, seasonId));
                            foreach (var leagueTeam in league.LeagueTeams)
                            {
                                allUserJobs.AddRange(GetTeamUsersJobs(leagueTeam.TeamId, seasonId));
                            }
                        }
                        if (hasDisciplines)
                        {
                            foreach (var discipline in union.Disciplines)
                            {
                                allUserJobs.AddRange(GetDisciplineUsersJobs(discipline.DisciplineId));
                            }
                        }
                    }
                    break;
                case LogicaName.Club:
                    var club = db.Clubs.FirstOrDefault(c => c.ClubId == id);
                    if (club != null)
                    {
                        allUserJobs.AddRange(GetClubOfficials(club.ClubId, seasonId));
                        var clubLeagues = club.Leagues;
                        if (clubLeagues.Any())
                        {
                            foreach (var league in clubLeagues)
                            {
                                allUserJobs.AddRange(GetLeagueUsersJobs(league.LeagueId, seasonId));
                            }
                        }
                        foreach (var clubTeam in club.ClubTeams)
                        {
                            allUserJobs.AddRange(GetTeamUsersJobs(clubTeam.TeamId, seasonId));
                        }
                        if (club.ConnectedReferees.Any())
                        {
                            var defaultTravelInformationList = new List<TravelInformationDto>();
                            defaultTravelInformationList.Add(new TravelInformationDto());
                            var games = db.GamesCycles.Where(g => !string.IsNullOrEmpty(g.DeskIds)
                               || !string.IsNullOrEmpty(g.RefereeIds)
                               || !string.IsNullOrEmpty(g.SpectatorIds));
                            var gamesBySeason = games.Where(g => !g.Stage.League.IsArchive && g.Stage.League.SeasonId == seasonId)?.ToList();
                            allUserJobs.AddRange(club.ConnectedReferees.Select(j => new UserJobDto
                            {
                                Id = j.Id,
                                JobName = j.Job.JobName,
                                UserId = j.User.UserId,
                                FullName = j.User.FullName,
                                Address = j.User.Address,
                                BirthDate = j.User.BirthDay,
                                City = j.User.City,
                                Email = j.User.Email,
                                TravelInformationDtos = j.TravelInformations.Any() ? j.TravelInformations.Select(x => new TravelInformationDto
                                {
                                    FromHour = x.FromHour,
                                    NoTravel = x.NoTravel ?? false,
                                    ToHour = x.ToHour,
                                    IsUnionTravel = x.IsUnionTravel
                                }).ToList() : defaultTravelInformationList,
                                LeagueDates = j.League?.LeagueStartDate != null && j.League.LeagueEndDate != null && j.League.LeagueEndDate >= j.League.LeagueStartDate
                                    ? Enumerable.Range(0, 1 + j.League.LeagueEndDate.Value.Subtract(j.League.LeagueStartDate.Value).Days)
                                    .Select(offset => j.League.LeagueStartDate.Value.AddDays(offset))
                                    .ToList() 
                                    : new List<DateTime>(),
                                Phone = j.User.Telephone,
                                RoleId = j.Job.RoleId ?? 0,
                                RoleName = j.Job.JobsRole.RoleName ?? "",
                                IdentNum = j.User.IdentNum,
                                KarateRefereeRanks = j.KarateRefereesRanks.Where(c => c.RefereeId == j.Id),
                                ConnectedClubName = j.ConnectedClub?.Name,
                                IsBlocked = j.IsBlocked,
                                IsCompetitionRegistrationBlocked = j.IsCompetitionRegistrationBlocked,
                                GamesCount = neededJobs.Contains(j.Job.JobsRole.RoleName)
                                ? GetCountOfAllGamesUserHasWorkIn(j.UserId.ToString(), gamesBySeason, j.Id)
                                : null,
                                CompetitionsParticipationCount = GetCompetitionParticipationsWithHoursCount(j),
                                DisciplinesRelatedNames = !string.IsNullOrEmpty(j.ConnectedDisciplineIds)
                            ? GetDisciplinesNames(j.ConnectedDisciplineIds)
                            : string.Empty,
                                MartialArtsCompetitionsCount = j.RefereeRegistrations.Count(r => r.IsApproved && !r.IsArchive),
                                TeamName = j.Team?.Title,
                                PaymentRateType = j.RateType,
                                Active = j.Active
                            }));
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logicalName), logicalName, null);
            }
            return allUserJobs.OrderBy(uj => uj.JobName).ThenBy(uj => uj.FullName).ToList();
        }

        public IEnumerable GetAllJobs(int id)
        {
            return (from j in GetQuery(false)
                    from u in j.Section.Unions
                    where u.UnionId == id &&
                          (j.JobsRole.RoleName == JobRole.UnionManager
                           || j.JobsRole.RoleName == JobRole.LeagueManager
                           || j.JobsRole.RoleName == JobRole.TeamManager
                           || j.JobsRole.RoleName == JobRole.ClubManager
                           || j.JobsRole.RoleName == JobRole.ClubSecretary
                           || j.JobsRole.RoleName == JobRole.Activitymanager
                           || j.JobsRole.RoleName == JobRole.Activityviewer
                           || j.JobsRole.RoleName == JobRole.ActivityRegistrationActive
                           || j.JobsRole.RoleName == JobRole.Referee
                           || j.JobsRole.RoleName == JobRole.Spectator
                           || j.JobsRole.RoleName == JobRole.Activitymanager
                           || j.JobsRole.RoleName == JobRole.ActivityRegistrationActive
                           || j.JobsRole.RoleName == JobRole.Desk)
                    select j).OrderBy(c => c.JobId).ToList();
        }

        public void AddNewDistance(DistanceTableDto frm, int relevantId, LogicaName logicalName, int? seasonId)
        {
            var distance = new DistanceTable();
            switch (logicalName)
            {
                case LogicaName.Union:
                    distance = new DistanceTable { UnionId = relevantId, SeasonId = seasonId, CityFromName = frm.CityFromName, CityToName = frm.CityToName, Distance = frm.Distance, DistanceType = frm.DistanceType };
                    break;
                case LogicaName.Club:
                    distance = new DistanceTable { ClubId = relevantId, SeasonId = seasonId, CityFromName = frm.CityFromName, CityToName = frm.CityToName, Distance = frm.Distance, DistanceType = frm.DistanceType };
                    break;
            }
            db.DistanceTables.Add(distance);
            db.SaveChanges();
        }

        public IEnumerable<DistanceTableDto> GetDistances(int id, LogicaName logicalName, int? seasonId)
        {
            var distanceModel = Enumerable.Empty<DistanceTable>();
            switch (logicalName)
            {
                case LogicaName.Union:
                    distanceModel = db.DistanceTables.Where(dt => dt.UnionId == id && dt.SeasonId == seasonId);
                    break;
                case LogicaName.Club:
                    distanceModel = db.DistanceTables.Where(dt => dt.ClubId == id && dt.SeasonId == seasonId);
                    break;
            }

            Mapper.Initialize(cfg => cfg.CreateMap<DistanceTable, DistanceTableDto>());
            return Mapper.Map<IEnumerable<DistanceTable>, IEnumerable<DistanceTableDto>>(distanceModel);
        }

        public bool UpdateDistance(DistanceTableDto frm, out string message)
        {
            try
            {
                var distanceToUpdate = db.DistanceTables.FirstOrDefault(dt => dt.Id == frm.Id);
                if (distanceToUpdate != null && distanceToUpdate.Id == 0)
                {
                    message = "There is no values with such id";
                    return false;
                }

                distanceToUpdate.CityFromName = frm.CityFromName;
                distanceToUpdate.CityToName = frm.CityToName;
                distanceToUpdate.Distance = frm.Distance;
                distanceToUpdate.DistanceType = frm.DistanceType;

                db.SaveChanges();
                message = string.Empty;
                return true;

            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public bool DeleteDistance(int id, out string message)
        {
            try
            {
                var distanceToDelete = db.DistanceTables.FirstOrDefault(dt => dt.Id == id);
                db.DistanceTables.Remove(distanceToDelete ?? throw new InvalidOperationException("The distance was not found"));
                db.SaveChanges();
                message = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public bool CheckForReportsEnabled(int id, LogicaName logicalName)
        {
            return logicalName == LogicaName.Union
                ? db.Unions.FirstOrDefault(u => u.UnionId == id)?.IsReportsEnabled ?? false
                : db.Clubs.FirstOrDefault(c => c.ClubId == id)?.IsReportsEnabled ?? false;
        }

        public IEnumerable<string> GetUniqueCitiesForDistanceTable(int id, LogicaName logicalName, int? seasonId)
        {
            var instanceCities = Enumerable.Empty<DistanceTable>();
            switch (logicalName)
            {
                case LogicaName.Union:
                    instanceCities = db.DistanceTables.Where(dt => dt.UnionId == id && dt.SeasonId == seasonId);
                    break;
                case LogicaName.Club:
                    instanceCities = db.DistanceTables.Where(dt => dt.ClubId == id && dt.SeasonId == seasonId);
                    break;
            }
            return GetUniqueCities(instanceCities);
        }

        private IEnumerable<string> GetUniqueCities(IEnumerable<DistanceTable> instanceCities)
        {
            var allCities = new List<string>();

            foreach (var city in instanceCities)
            {
                allCities.Add(city.CityFromName?.Trim());
                allCities.Add(city.CityToName?.Trim());
            }

            return allCities.Distinct();
        }

        public int GetDistanceBetweenCities(int id, LogicaName logicalName, int? seasonId, string city1Name, string city2Name, out string distanceType)
        {
            switch (logicalName)
            {
                case LogicaName.Union:
                    var distanceByUnion = db.DistanceTables
                        .FirstOrDefault(dt => dt.SeasonId == seasonId && dt.CityFromName == city1Name && dt.CityToName == city2Name && dt.UnionId == id)
                        ?? db.DistanceTables.FirstOrDefault(dt => dt.SeasonId == seasonId && dt.CityFromName == city2Name && dt.CityToName == city1Name && dt.UnionId == id);
                    distanceType = distanceByUnion?.DistanceType;
                    return distanceByUnion?.Distance ?? 0;
                case LogicaName.Club:
                    var distanceByClub = db.DistanceTables
                        .FirstOrDefault(dt => dt.SeasonId == seasonId && dt.CityFromName == city1Name && dt.CityToName == city2Name && dt.ClubId == id)
                        ?? db.DistanceTables.SingleOrDefault(dt => dt.SeasonId == seasonId && dt.CityFromName == city2Name && dt.CityToName == city1Name && dt.ClubId == id);
                    distanceType = distanceByClub?.DistanceType;
                    return distanceByClub?.Distance ?? 0;
                default:
                    throw new NotImplementedException($"Not implemented for {logicalName.ToString()}");
            }
        }

        public IDictionary<int, UserJobDto> GetCurrentOfficialsForReports(List<UserJobDto> vmUsersList, int seasonId)
        {
            if (vmUsersList == null || !vmUsersList.Any())
            {
                return new Dictionary<int, UserJobDto>();
            }

            var jobsList = new List<string> {JobRole.Referee, JobRole.Spectator, JobRole.Desk};

            var userIds = vmUsersList
                .Where(userJob => jobsList.Contains(userJob.RoleName))
                .Select(c => c.UserId)
                .Distinct()
                .ToList();

            var result = new Dictionary<int, UserJobDto>();

            if (userIds.Any())
            {
                foreach (var userId in userIds)
                {
                    result.Add(userId, GetUserJobDtoItem(userId, seasonId));
                }
            }

            result = result
                .OrderBy(x => x.Value.FullName)
                .ToDictionary(x => x.Key, x => x.Value);

            return result;
        }

        public UserJobDto GetUserJobDtoItem(int userId, int seasonId)
        {
            
            var userJob = db.UsersJobs.FirstOrDefault(uj => uj.UserId == userId && uj.SeasonId == seasonId);
            
            if (userJob != null && userJob.User == null)
            {
                // in some cases the userJob.User is null despite having proper userJob.UserId, so this is possible fix in case the bug occurs.
                var user = db.Users.FirstOrDefault(u=> u.UserId == userId);
                userJob.User = user;
            }
            if (userJob != null && userJob.Job == null)
            {
                var job = db.Jobs.FirstOrDefault(j => j.JobId == userJob.JobId);
                userJob.Job = job;
            }
            var games = db.GamesCycles.Where(g => !string.IsNullOrEmpty(g.DeskIds)
                               || !string.IsNullOrEmpty(g.RefereeIds)
                               || !string.IsNullOrEmpty(g.SpectatorIds));
            var gamesBySeason = games.Where(g => !g.Stage.League.IsArchive && g.Stage.League.SeasonId == seasonId)?.ToList();
            if (userJob!=null)
            {
                return new UserJobDto
                {
                    Id = userJob.Id,
                    JobName = userJob.Job.JobName,
                    UserId = userJob.User.UserId,
                    FullName = userJob.User.FullName,
                    Address = userJob.User.Address,
                    BirthDate = userJob.User.BirthDay,
                    City = userJob.User.City,
                    Email = userJob.User.Email,
                    Phone = userJob.User.Telephone,
                    RoleId = userJob.Job.RoleId ?? 0,
                    RoleName = userJob.Job.JobsRole.RoleName ?? "",
                    IdentNum = userJob.User.IdentNum,
                    KarateRefereeRanks = userJob.KarateRefereesRanks.Where(c => c.RefereeId == userJob.Id),
                    ConnectedClubName = userJob.ConnectedClub?.Name,
                    IsBlocked = userJob.IsBlocked,
                    GamesCount = neededJobs.Contains(userJob.Job.JobsRole.RoleName)
                        ? GetCountOfAllGamesUserHasWorkIn(userJob.UserId.ToString(), gamesBySeason, userJob.Id)
                        : null,
                    CompetitionsParticipationCount = GetCompetitionParticipationsWithHoursCount(userJob),
                    DisciplinesRelatedNames = !string.IsNullOrEmpty(userJob.ConnectedDisciplineIds)
                        ? GetDisciplinesNames(userJob.ConnectedDisciplineIds)
                        : string.Empty,
                    MartialArtsCompetitionsCount = userJob.RefereeRegistrations.Count(r => r.IsApproved && !r.IsArchive),
                    TeamName = userJob.Team?.Title,
                    PaymentRateType = userJob.RateType,
                    Active = userJob.Active
                };
            }
            else
            {
                return new UserJobDto
                {
                    Id = 0,
                    RoleName = "referee"
                };
            }
        }

        public IEnumerable<WorkerShortDto> GetWorkersByIds(IEnumerable<int> jobsJds)
        {
            var workerJobs = db.UsersJobs.Where(c => jobsJds.Contains(c.Id)).ToList();
            return workerJobs.Select(c => new WorkerShortDto
            {
                UserId = c.UserId,
                JobId = c.JobId,
                UserFullName = c.User?.FullName,
                JobName = c.Job?.JobName,
                OfficialType = c.Job?.JobsRole?.RoleName,
                UserJobId = c.Id
            });
        }

        public void ChangeBlockStatus(int id, bool isBlocked)
        {
            var userJob = db.UsersJobs.FirstOrDefault(c => c.Id == id);
            if (userJob != null)
            {
                userJob.IsBlocked = isBlocked;
            }
        }

        public void ChangeActiveStatus(int id)
        {
            var userJob = db.UsersJobs.FirstOrDefault(c => c.Id == id);
            if (userJob != null)
            {
                userJob.Active = !userJob.Active;
            }
        }

        public TeamManagerDto GetTeamManagerInfo(int teamId, int seasonId)
        {
            TeamManagerDto manager = null;
            var teamManager = db.UsersJobs.FirstOrDefault(uj => uj.TeamId == teamId && uj.Job.JobsRole.RoleName == JobRole.TeamManager
                        && uj.SeasonId == seasonId);
            var team = db.Teams.FirstOrDefault(t => t.TeamId == teamId);
            var field = team?.TeamsAuditoriums.Where(r => r.IsMain)?.FirstOrDefault()
                ?? team?.TeamsAuditoriums.Where(r => !r.IsMain)?.FirstOrDefault();

            if (teamManager != null)
            {
                manager = new TeamManagerDto
                {
                    Id = teamManager.Id,
                    Name = teamManager.User.FullName,
                    Phone = teamManager.User.Telephone,
                    FieldName = field?.Auditorium?.Name,
                    FieldAddress = field?.Auditorium?.Address
                };
            }
            return manager;
        }

        #region changes for Regional section

        public IEnumerable<Job> GetRegionalManagerJobs(int regionalId, int sectionId = 0)
        {
            if (sectionId == 0)
                sectionId = db.Regionals.Find(regionalId)?.Union?.SectionId ?? 0;

            var jobs = db.Jobs.Join(db.JobsRoles
                        .Where(jr => jr.RoleName == JobRole.RegionalManager)
                    , j => j.RoleId, jr => jr.RoleId, (j, jr) => j)
                .Where(j => j.SectionId == sectionId).ToList();          
            return jobs;
        }

        public List<UserJobDto> GetRegionalFedOfficials(int regionalId, int? seasonId)
        {
            var result = new List<UserJobDto>();

            var userJobs = db.UsersJobs
                .Where(uj => uj.RegionalId == regionalId &&
                             !uj.User.IsArchive &&
                             uj.SeasonId == seasonId)
                .ToList()
                .OrderBy(uj => uj.User.FullName)
                .ToList();

            var games = db.GamesCycles
                .Where(g => !string.IsNullOrEmpty(g.DeskIds) ||
                            !string.IsNullOrEmpty(g.RefereeIds) ||
                            !string.IsNullOrEmpty(g.SpectatorIds));

            var gamesBySeason = games.Where(g => !g.Stage.League.IsArchive && g.Stage.League.SeasonId == seasonId)?.ToList();

            if (userJobs.Any())
            {
                foreach (var userJob in userJobs)
                {
                    result.Add(new UserJobDto
                    {
                        Id = userJob.Id,
                        JobName = userJob.Job.JobName,
                        UserId = userJob.UserId,
                        FullName = userJob.User.FullName,
                        Address = userJob.User.Address,
                        BirthDate = userJob.User.BirthDay,
                        City = userJob.User.City,
                        Email = userJob.User.Email,
                        Phone = userJob.User.Telephone,
                        RoleId = userJob.Job.RoleId ?? 0,
                        RoleName = userJob.Job.JobsRole.RoleName ?? "",
                        IdentNum = userJob.User.IdentNum,
                        KarateRefereeRanks = userJob.KarateRefereesRanks.Where(c => c.RefereeId == userJob.Id),
                        ConnectedClubName = userJob.ConnectedClub?.Name,
                        IsBlocked = userJob.IsBlocked,
                        IsCompetitionRegistrationBlocked = userJob.IsCompetitionRegistrationBlocked,
                        GamesCount = neededJobs.Contains(userJob.Job.JobsRole.RoleName)
                            ? GetCountOfAllGamesUserHasWorkIn(userJob.UserId.ToString(), gamesBySeason, userJob.Id)
                            : null,
                        CompetitionsParticipationCount = GetCompetitionParticipationsWithHoursCount(userJob),
                        DisciplinesRelatedNames = !string.IsNullOrEmpty(userJob.ConnectedDisciplineIds)
                            ? GetDisciplinesNames(userJob.ConnectedDisciplineIds)
                            : string.Empty,
                        MartialArtsCompetitionsCount = userJob.RefereeRegistrations.Count(r => r.IsApproved && !r.IsArchive),
                        TeamName = userJob.Team?.Title,
                        PaymentRateType = userJob.RateType,
                        Active = userJob.Active
                    });
                }
            }

            return result;
        }

        #endregion
    }
}
