using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppModel;

namespace DataService
{
    public class SportRanksRepo : BaseRepo
    {
        public SportRanksRepo()
        {
        }

        public SportRanksRepo(DataEntities db) : base(db)
        {
        }

        public IEnumerable<SportRank> GetRanksBySport(int sportId)
        {
            return db.SportRanks.Where(x => x.SportId == sportId);
        }

        public IEnumerable<SportRank> GetRanksBySport(Sport sport)
        {
            return db.SportRanks.Where(x => x.SportId == sport.Id);
        }
    }
}
