using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class ActivityFormGetTeamManagerUserModel
    {
        public int Id { get; set; }
        public string IdNum { get; set; }

        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string BirthDate { get; set; }
        public List<ActivityFormGetTeamManagerTeamModel> Teams { get; set; }
    }

    public class ActivityFormGetTeamManagerTeamModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int LeagueId { get; set; }
        public string League { get; set; }
        public bool? NeedShirts { get; set; }
        public int SeasonId { get; set; }
        public decimal TeamRegistrationPrice { get; set; }
    }
}