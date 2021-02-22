using AppModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DataService
{
    public class BenefitsRepo : BaseRepo
    {
        public BenefitsRepo() : base() { }
        public BenefitsRepo(DataEntities db) : base(db) { }

        public Benefit GetById(int Id)
        {
            return db.Benefits.Find(Id);
        }
        public Benefit GetByIdIfPublished(int Id)
        {
            return db.Benefits.FirstOrDefault(b => b.BenefitId == Id && b.IsPublished);
        }

        public void Create(Benefit item)
        {
            db.Benefits.Add(item);
            db.SaveChanges();
        }

        public void Delete(int benefitId)
        {
            var bn = db.Benefits.Find(benefitId);
            db.Benefits.Remove(bn);
            db.SaveChanges();
        }

        public void Update(Benefit item)
        {
            db.Benefits.Attach(item);
            db.Entry(item).State = EntityState.Modified;
            db.SaveChanges();
        }

        public List<Benefit> GetBenefits(int unionId, int seasonId)
        {
            return db.Benefits.Where(b => b.UnionId == unionId && b.SeasonId == seasonId).ToList();
        }

        public List<Benefit> GetPublishedBenefits(int unionId, int seasonId)
        {
            return db.Benefits.Where(b => b.UnionId == unionId && b.SeasonId == seasonId && b.IsPublished).ToList();
        }
    }
}
