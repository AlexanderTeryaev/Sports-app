using AppModel;

namespace DataService
{
    public class Competitor
    {
        public Team Team { get; set; }
        public TeamsPlayer Athlete { get; set; }
        public TeamsPlayer PairAthlete { get; set; }
    }
}