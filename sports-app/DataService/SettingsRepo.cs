using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppModel;
using DataService;

namespace DataService
{
    public class SettingsRepo : BaseRepo
    {
        public Settings GetById(int id)
        {
            return db.Settings.Find(id);
        }
    }
}
