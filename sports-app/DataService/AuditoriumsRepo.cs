using System.Linq;
using System.Collections.Generic;
using AppModel;
using DataService.DTO;
using System;

namespace DataService
{
    public class AuditoriumsRepo : BaseRepo
    {
        public AuditoriumsRepo() : base() { }
        public AuditoriumsRepo(DataEntities db) : base(db) { }
        public Auditorium GetById(int id)
        {
            return db.Auditoriums.Find(id);
        }

        public List<Auditorium> GetByTeamAndSeason(int teamId, int seasonId)
        {
            var aud = db.LeagueTeams.Where(lt => lt.TeamId == teamId && lt.SeasonId == seasonId)
                .Join(db.Leagues, lt => lt.LeagueId, l => l.LeagueId, (lt, l) => l)
                .Join(db.Auditoriums.Where(a => !a.IsArchive && a.SeasonId == seasonId), l => l.UnionId, a => a.UnionId,
                    (l, a) => a)
                .Concat(db.ClubTeams.Where(ct => ct.TeamId == teamId && ct.SeasonId == seasonId)
                    .Join(db.Auditoriums.Where(a => !a.IsArchive),
                        ct => (ct.Club.IsUnionClub.Value
                            ? new {ClubId = ct.ClubId, SeasonId = ct.Club.SeasonId.Value}
                            : new {ClubId = ct.ClubId, SeasonId = seasonId}),
                        a => (a.ClubId.HasValue && a.Club.IsUnionClub.Value
                            ? new {ClubId = a.ClubId.Value, SeasonId = seasonId}
                            : new {ClubId = a.ClubId ?? 0, SeasonId = a.SeasonId ?? 0}),
                        (ct, a) => a))
                .Concat(db.SchoolTeams.Where(ct => ct.TeamId == teamId && ct.School.SeasonId == seasonId)
                    .Join(db.Auditoriums.Where(a => !a.IsArchive),
                        ct => (ct.School.Club.IsUnionClub.Value
                            ? new {ClubId = ct.School.ClubId, SeasonId = ct.School.Club.SeasonId.Value}
                            : new {ClubId = ct.School.ClubId, SeasonId = seasonId}),
                        a => (a.ClubId.HasValue && a.Club.IsUnionClub.Value
                            ? new {ClubId = a.ClubId.Value, SeasonId = seasonId}
                            : new {ClubId = a.ClubId ?? 0, SeasonId = a.SeasonId ?? 0}),
                        (ct, a) => a))
                .Distinct()
                .OrderBy(a => a.Name)
                .ToList();
            return aud;
        }
        public IEnumerable<Auditorium> GetByUnionAndSeason(int? unionId, int seasonId)
        {
            var testMine = db.Auditoriums.FirstOrDefault(a=>a.AuditoriumId == 5969);
            var test = db.Auditoriums.Where(a => !a.IsArchive && (unionId == null || a.UnionId == unionId || (a.ClubId > 0 && a.Club.Union.UnionId == unionId)) && (a.SeasonId == seasonId || (a.ClubId > -1 && a.Club.SeasonId == seasonId))).OrderBy(a => a.AuditoriumId).Select(a => a.AuditoriumId).ToList();
            return db.Auditoriums.Where(a => !a.IsArchive && (unionId == null || a.UnionId == unionId || (a.ClubId > -1 && a.Club.Union.UnionId == unionId)) && (a.SeasonId == seasonId || (a.ClubId > -1 && a.Club.SeasonId == seasonId))).OrderBy(a => a.Name).ToList();
        }
        public IEnumerable<Auditorium> GetByClubAndSeason(int clubId, int? seasonId)
        {
            var club = GetClubById(clubId);
            var audQ = db.Auditoriums.Where(a => !a.IsArchive && a.ClubId == clubId);
            if (!(club.IsUnionClub.Value))
            {
                audQ = audQ.Where(a => !seasonId.HasValue || a.SeasonId == seasonId.Value);
            }
            return audQ.OrderBy(a => a.Name).ToList();
        }
        public IEnumerable<Auditorium> GetAll(int? unionId = null)
        {
            var unionSeason = db.Seasons.FirstOrDefault(x => x.UnionId == unionId);
            var query = db.Auditoriums.Where(x => !x.IsArchive && (unionId == null || x.UnionId == unionId));
            if (unionSeason != null)
            {
                query = query.Where(x => x.SeasonId == unionSeason.Id);
            }
            return query != null ? query.OrderBy(a => a.Name).ToList() : new List<Auditorium>();

        }
        public IEnumerable<AuditoriumShort> GetAuditoriumsFilterList(int unionId, int seasonId, bool addOptionAll = true)
        {
            var auditoriumShortList = new List<AuditoriumShort>();
            if (addOptionAll)
            {
                auditoriumShortList.Add(new AuditoriumShort
                {
                    Name = "All",
                    Id = -1,
                    Check = false
                });
            }
            auditoriumShortList.AddRange(
                GetByUnionAndSeason(unionId, seasonId).Where(x => !x.IsArchive)
                .Select(a =>
                   new AuditoriumShort
                   {
                       Id = a.AuditoriumId,
                       Name = a.Name,
                       Check = false
                   }));
            return auditoriumShortList;
        }
        public void Create(Auditorium item)
        {
            db.Auditoriums.Add(item);
        }

        public IEnumerable<TeamsAuditorium> GetByTeam(int teamId)
        {
            return db.TeamsAuditoriums.Where(t => t.TeamId == teamId).ToList();
        }

        public void AddToTeam(TeamsAuditorium item)
        {
            db.TeamsAuditoriums.Add(item);
        }

        public TeamsAuditorium GetAuditoriumById(int id)
        {
            return db.TeamsAuditoriums.Find(id);
        }

        public void RemoveFromTeam(TeamsAuditorium item)
        {
            db.TeamsAuditoriums.Remove(item);
        }

        public IEnumerable<Auditorium> GetByLeague(int leagueId)
        {
            return (from l in db.Leagues
                    from t in l.LeagueTeams
                    from ta in t.Teams.TeamsAuditoriums
                    let a = ta.Auditorium
                    where t.Teams.IsArchive == false && l.LeagueId == leagueId && a.IsArchive == false
                    select a).ToList();
        }

        public bool IsExistsInTeam(int auditoriumId, int teamId)
        {
            return db.TeamsAuditoriums.Any(t => t.TeamId == teamId && t.AuditoriumId == auditoriumId);
        }
    }
}
