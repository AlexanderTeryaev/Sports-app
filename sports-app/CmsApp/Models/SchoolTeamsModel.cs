using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppModel;

namespace CmsApp.Models
{
    public class SchoolTeamsModel
    {
        public int ClubId { get; set; }
        public int SeasonId { get; set; }

        public bool CanManageSchools { get; set; }

        public List<School> Schools { get; set; }
        public List<Team> AvailableTeams { get; set; }
        public bool IsCamp { get; set; }
    }
}