using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppModel;
using System.Linq.Expressions;
using DataService.DTO;
using AutoMapper;

namespace DataService
{
    public class SectionsRepo : BaseRepo
    {

        public SectionsRepo() : base() { }
        public SectionsRepo(DataEntities db) : base(db) { }
        public IQueryable<Section> GetQuery(bool isArchive)
        {
            return db.Sections.AsQueryable();
        }

        public Section GetById(int id)
        {
            return db.Sections.Find(id);
        }

        public Section GetByAlias(string alias)
        {
            return db.Sections.FirstOrDefault(s => s.Alias == alias);
        }

        public IEnumerable<Section> GetSections(int? langId)
        {
            var query = GetQuery(false);

            if (langId.HasValue)
                query = query.Where(t => t.LangId == langId);

            return query.OrderBy(t => t.Name).ToList();
        }

        public IEnumerable<Section> GetSections(Expression<Func<Section, bool>> expression = null)
        {
            return expression == null ? db.Sections : db.Sections.Where(expression);
        }

        public void CreateSection(Section item)
        {
            db.Sections.Add(item);
            db.SaveChanges();
        }

        public Section GetByUnionId(int unionId)
        {
            return db.Unions.Where(t => t.UnionId == unionId).Select(t => t.Section)
                .FirstOrDefault();
        }

        public Section GetByDisciplineId(int disciplineId)
        {
            return db.Disciplines.Where(d => d.DisciplineId == disciplineId).Select(t => t.Union.Section)
                .FirstOrDefault();
        }

        public Section GetByLeagueId(int leagueId)
        {
            var league = db.Leagues.FirstOrDefault(t => t.LeagueId == leagueId);
            if (league != null)
            {
                if (league.UnionId.HasValue)
                {
                    return league.Union.Section;
                }

                if (league.DisciplineId.HasValue)
                {
                    return league.Discipline.Union.Section;
                }
                return league.Club.Section;
            }
            else
            {
                return null;
            }
        }

        public Section GetByClubId(int clubId)
        {
            var club = db.Clubs.Where(c => c.ClubId == clubId).First();
            if (club.IsSectionClub ?? true)
            {
                return club.Section;
            }
            else
            {
                return club.Union.Section;
            }
        }

        public void SectionIndividualStatus(int sectionId, bool isIndividual)
        {
            var section = db.Sections.FirstOrDefault(s => s.SectionId == sectionId);
            if (section != null)
                section.IsIndividual = isIndividual;
            db.SaveChanges();
        }

        public bool CheckSectionIndividualStatus(int id, LogicaName logicalName)
        {
            switch (logicalName)
            {
                case LogicaName.Team:
                    var team = db.Teams.FirstOrDefault(t => t.TeamId == id);
                    return team != null && GetSectionByTeamId(id)?.IsIndividual == true;
                default: throw new NotImplementedException();
            }
        }

        public List<string> CheckSectionAliasesWithIndividualStatus()
        {
            return db.Sections.Where(s => s.IsIndividual).Select(s => s.Alias).ToList();
        }

        public bool IsKarateSection(int id, LogicaName logicalName)
        {
            string sportName = string.Empty;
            switch (logicalName)
            {
                case LogicaName.Union:
                    var union = db.Unions.FirstOrDefault(u => u.UnionId == id);
                    sportName = union?.Sport?.Name ?? string.Empty;
                    break;
                case LogicaName.League:
                    var league = db.Leagues.FirstOrDefault(l => l.LeagueId == id);
                    sportName = league?.Union?.Sport?.Name ?? league?.Club?.Sport?.Name ?? string.Empty;
                    break;
                case LogicaName.Club:
                    var club = db.Clubs.FirstOrDefault(c => c.ClubId == id);
                    sportName = club?.Sport?.Name ?? club?.Union?.Sport?.Name ?? string.Empty;
                    break;
            }
            return string.Equals(sportName, "Karate", StringComparison.OrdinalIgnoreCase);
        }

        public string GetAlias(int? unionId, int? clubId, int? leagueId)
        {
            var sectionAlias = string.Empty;
            if (unionId.HasValue)
            {
                sectionAlias = db.Unions.FirstOrDefault(c => c.UnionId == unionId.Value)?.Section?.Alias;
            }
            else if (leagueId.HasValue)
            {
                var league = db.Leagues.FirstOrDefault(c => c.LeagueId == leagueId);
                sectionAlias = league?.Club?.Section?.Alias
                    ?? league?.Club?.Union?.Section?.Alias
                    ?? league?.Union?.Section?.Alias;
            }
            else if (clubId.HasValue)
            {
                var club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId);
                sectionAlias = club?.Section?.Alias ?? club?.Union?.Section?.Alias;
            }
            return sectionAlias;
        }

        public Section GetSectionByClubId(int clubId)
        {
            var club = db.Clubs.FirstOrDefault(c => c.ClubId == clubId);
            return club?.Section ?? club?.Union?.Section;
        }


        public string GetRelatedLeaguesString(int id)
        {
            var leaguesString = string.Empty;
            var leaguesRelated = db.Leagues.Where(l => l.AgeId == id)?.Select(l => l.Name)?.AsEnumerable();
            if (leaguesRelated.Any())
                leaguesString = string.Join(",", leaguesRelated);
            return leaguesString;
        }
    }
}