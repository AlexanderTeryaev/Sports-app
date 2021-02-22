using System.Collections.Generic;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class MoveTeamToLeagueViewModel
    {
        public int[] TeamIds { get; set; }
        public int LeagueId { get; set; }
        public List<SelectListItem> Leagues { get; set; }
        public int SeasonId { get; set; }
        public int CurrentLeagueId { get; set; }
        public bool IsTennisLeague { get; set; }
        public bool IsCopy { get; set; }
    }
}