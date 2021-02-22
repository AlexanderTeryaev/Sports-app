using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class CompetitionRouteClubDTO
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public string ClubName { get; set; }
        //public int DisciplineId { get; set; }
        public int CompetitionId { get; set; }
        public int? MaxRegistrationsAllowed { get; set; }
    }
}