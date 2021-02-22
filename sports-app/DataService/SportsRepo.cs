using System.Collections.Generic;
using System.Linq;
using AppModel;

namespace DataService
{
    public class SportsRepo : BaseRepo
    {
        public SportsRepo()
        {
        }

        public SportsRepo(DataEntities db) : base(db)
        {
        }

        public IEnumerable<Sport> GetBySectionId(int sectionId)
        {
            return db.Sports.Where(x => x.SectionId == sectionId);
        }

        public IEnumerable<SportRank> GetSportRanksBySportId(int sportId)
        {
            return db.SportRanks.Where(s => s.SportId == sportId);
        }
    }
}
