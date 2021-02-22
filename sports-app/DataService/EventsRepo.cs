using AppModel;
using System.Data.Entity;

namespace DataService
{
    public class EventsRepo : BaseRepo
    {
        public EventsRepo() : base() { }
        public EventsRepo(DataEntities db) : base(db) { }

        public Event GetById(int Id)
        {
            return db.Events.Find(Id);
        }

        public void Create(Event item)
        {
            db.Events.Add(item);
            db.SaveChanges();
        }

        public void Delete(int eventId)
        {
            var ev = db.Events.Find(eventId);
            db.Events.Remove(ev);
            db.SaveChanges();
        }

        public void Update(Event item)
        {
            db.Events.Attach(item);
            db.Entry(item).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}
