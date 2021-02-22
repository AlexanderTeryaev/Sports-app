using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppModel;

namespace DataService
{
    public class PositionsRepo : BaseRepo
    {
        public PositionsRepo() : base() { }
        public PositionsRepo(DataEntities db) : base(db) { }

        public Position GetById(int id)
        {
            return db.Positions.Find(id);
        }

        public IEnumerable<Position> GetBySection(int sectionId)
        {
            return db.Positions.Where(t => t.SectionId == sectionId && t.IsArchive == false).Distinct().ToList();
        }

        public IEnumerable<Position> GetByTeam(int teamId)
        {
            var positions = (from t in db.Teams
                from l in t.LeagueTeams
                from p in l.Leagues.Union.Section.Positions
                where p.IsArchive == false && t.TeamId == teamId
                select p).Distinct().ToList();

            if (!positions.Any())
            {
                //try by club team
                positions = (from t in db.Teams
                                        from c in t.ClubTeams
                                        from p in c.Club.Section.Positions
                                        where p.IsArchive == false && t.TeamId == teamId
                                        select p).Distinct().ToList();

                if (!positions.Any())
                {
                    //try by school team
                    positions = (from t in db.Teams
                        from c in t.SchoolTeams
                        from p in c.School.Club.Section.Positions
                        where p.IsArchive == false && t.TeamId == teamId
                        select p).Distinct().ToList();

                    if (!positions.Any())
                    {
                        //try by multi-sport team
                        positions = db.ClubTeams.FirstOrDefault(x => x.TeamId == teamId)
                            ?.Club?.SportSection?.Positions
                            .Where(x => !x.IsArchive)
                            .Distinct()
                            .ToList();

                        if (positions == null)
                        {
                            //try by multi-sport school team
                            positions = db.SchoolTeams.FirstOrDefault(x => x.TeamId == teamId)
                                ?.School?.Club?.SportSection?.Positions
                                .Where(x => !x.IsArchive)
                                .Distinct()
                                .ToList();
                        }
                    }
                }
            }
            return positions ?? new List<Position>();
        }

        public void Create(Position item)
        {
            db.Positions.Add(item);
        }
    }
}
