using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class TeamScheduleForm
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public string HomeTeam { get; set; }
        public string GuestTeam { get; set; }
        public string Score { get; set; }
        public string Auditorium { get; set; }
    }
}