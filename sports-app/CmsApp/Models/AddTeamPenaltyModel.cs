using System.Collections.Generic;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class AddTeamPenaltyModel
    {
        public int LeagueId { get; set; }

        public int SeasonId { get; set; }

        public int StageId { get; set; }

        public int TeamId { get; set; }

        public decimal Points { get; set; }

        public List<SelectListItem> TeamsList { get; set; }
    }
}