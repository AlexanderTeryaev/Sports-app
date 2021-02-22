using System.Collections.Generic;

namespace CmsApp.Models
{
    public class RoutesControlForm
    {
        public int DisciplineId { get; set; }
        public int SeasonId { get; set; }
        public int LeagueId { get; set; }
        public IEnumerable<BasicRouteForm> Routes { get; set; }
        public IEnumerable<BasicRouteForm> TeamsRoutes { get; set; }
    }
}