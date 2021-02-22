using System.Collections.Generic;

namespace CmsApp.Models
{
    public class GamesUrlForm
    {
        public int ClubId { get; set; }
        public int TeamId { get; set; }
        public string GamesUrl { get; set; }
        public string TeamName { get; set; }
        public int SeasonId { get; set; }
        
        public List<TeamScheduleForm> TeamSchedule { get; set; }
    }
}