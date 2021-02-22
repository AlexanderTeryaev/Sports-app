using System;
using System.Linq;
using AppModel;

namespace DataService
{
    public class ContactsRepo : BaseRepo
    {
        public Contacts GetById(int id)
        {
            return db.Contacts.Find(id);
        }

        public void Create(Contacts item)
        {
            item.SendDate = DateTime.Now;
            db.Contacts.Add(item);
        }

        public IQueryable<Contacts> GetQuery(bool archive)
        {
            return db.Contacts.Where(t => t.IsArchive == archive);
        }

        public void Move(int id, bool archive)
        {
            var item = GetById(id);
            item.IsArchive = archive;
        }

        public int GetRequestsNum(string userIP, int hours)
        {
            var dateTo = DateTime.Now.AddHours(-hours);
            return db.Contacts.Count(t => t.UserIP == userIP && t.SendDate >= dateTo);
        }

        public int ArchiveAll()
        {
            return db.Database.ExecuteSqlCommand("Update [Contacts] Set IsArchive = 1 Where IsArchive = 0");
        }
    }
}
