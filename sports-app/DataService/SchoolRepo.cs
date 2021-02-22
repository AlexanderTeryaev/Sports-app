using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AppModel;

namespace DataService
{
    public class SchoolRepo : BaseRepo
    {
        public SchoolRepo()
        {
        }

        public SchoolRepo(DataEntities db) : base(db)
        {
        }

        public IEnumerable<School> GetCollection(Expression<Func<School, bool>> expression)
        {
            return db.Schools.Where(expression);
        }

        public School GetById(int id)
        {
            return db.Schools.FirstOrDefault(c => c.Id == id);
        }
    }
}
