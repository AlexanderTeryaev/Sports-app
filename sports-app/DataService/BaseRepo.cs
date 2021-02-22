using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AppModel;
using AutoMapper;
using DataService.DTO;

namespace DataService
{
    public class BaseRepo : IDisposable
    {
        internal DataEntities db;

        public BaseRepo()
        {
            db = new DataEntities();
            // add for production
            //db.Database.Connection.ConnectionString = ConnectionHelper.GetConnectionString();
            db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
            db.Database.CommandTimeout = 600;
        }

        public BaseRepo(DataEntities db)
        {
            this.db = db;
            // add for production
            //db.Database.Connection.ConnectionString = ConnectionHelper.GetConnectionString();
            db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
            db.Database.CommandTimeout = 600;
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
        }

        public Club GetClubById(int clubId)
        {
            return db.Clubs.FirstOrDefault(c => c.ClubId == clubId);
        }

        public Section GetSection(int disciplineId)
        {
            return db.Disciplines.Find(disciplineId)?.Union?.Section;
        }

        public Section GetSectionByTeamId(int teamId)
        {
            Section section;

            //  Try find section by league
            var leagueTeam = db.LeagueTeams.FirstOrDefault(lt => lt.TeamId == teamId);//?.Leagues?.Union?.Section;
            section = leagueTeam?.Leagues?.Union?.Section ?? //by union
                      leagueTeam?.Leagues?.Club?.Union?.Section ?? //by union-club
                      leagueTeam?.Leagues?.Club?.Section; //by section-club(or tournament)
            //  Try by section club
            if (section == null)
            {
                var club = db.ClubTeams.FirstOrDefault(ct => ct.TeamId == teamId)?.Club;
                //  Try by union club
                if (club != null)
                {
                    section = club.IsSectionClub.HasValue && club.IsSectionClub.Value ? club.Section : club.Union.Section;
                }
                else
                {
                    club = db.SchoolTeams.FirstOrDefault(x => x.TeamId == teamId)?.School?.Club;
                    if (club != null)
                    {
                        section = club.IsSectionClub.HasValue && club.IsSectionClub.Value ? club.Section : club.Union.Section;
                    }
                }
            }

            return section;
        }

        public IEnumerable<Language> GetLanguages()
        {
            return db.Languages.ToList();
        }

        public IEnumerable<Country> GetCountries()
        {
            return db.Countries.OrderBy(t => t.Name).ToList();
        }

        public IEnumerable<Age> GetAges()
        {
            return db.Ages.OrderBy(t => t.AgeId).ToList();
        }

        public IEnumerable<Gender> GetGenders()
        {
            return db.Genders.ToList();
        }

        public IEnumerable<TEntity> GetCollection<TEntity>(Expression<Func<TEntity, bool>> expression)
            where TEntity : class
        {
            return expression == null ? db.Set<TEntity>().AsQueryable() : db.Set<TEntity>().Where(expression);
        }
    }

    public class ListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int? SeasonId { get; set; }

        public int TeamId { get; set; }
        public int LeagueId { get; set; }
        public int ClubId { get; set; }
        public int? TeamSeasonId { get; set; }
        public override bool Equals(object obj)
        {
            return ((ListItemDto)obj).Id == Id;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
