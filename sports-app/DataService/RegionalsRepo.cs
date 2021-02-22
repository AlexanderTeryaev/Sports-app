using System;
using System.Collections.Generic;
using System.Linq;
using AppModel;

namespace DataService
{
    public class RegionalsRepo : BaseRepo
    {
        public RegionalsRepo() : base() { }
        public RegionalsRepo(DataEntities db) : base(db) { }

        public Regional GetById(int id)
        {
            return db.Regionals.Find(id);
        }

        public List<Regional> GetRegionalsByUnionAndSeason(int unionid, int seasonid)
        {
            return db.Regionals.Where(x => x.UnionId == unionid && x.SeasonId == seasonid).ToList();
        }

        public void Delete(int id)
        {
            var regional = db.Regionals.Find(id);
            if (regional != null)
            {
                db.Regionals.Remove(regional);

                db.SaveChanges();
            }
        }

        public void AddRegional(Regional regional)
        {
            regional.CreateDate = DateTime.Now;

            db.Regionals.Add(regional);
            db.SaveChanges();
        }
    }
}