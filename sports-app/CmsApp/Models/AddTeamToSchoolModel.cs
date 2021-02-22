using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppModel;

namespace CmsApp.Models
{
    public class AddTeamToSchoolModel
    {
        public int SchoolId { get; set; }
        public int TeamId { get; set; }

        public string NewTeamName { get; set; }

        public List<Team> AvailableTeams { get; set; }
        public bool IsCamp { get; set; }
    }
}