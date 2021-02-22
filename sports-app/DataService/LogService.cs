using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppModel;

namespace DataService
{
    public class LogService : BaseRepo
    {
        public void WriteLog(string evType, string desc)
        {
            var log = new EventsLog();
            log.EventType = evType;
            log.EventDate = DateTime.Now;
            log.Description = desc;

            db.EventsLog.Add(log);
        }
    }
}
