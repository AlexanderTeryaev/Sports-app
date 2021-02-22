using AppModel;
using System.Collections.Generic;
using System.Linq;

namespace CmsApp.Models
{
    public class TeamSchedules
    {
        public int TeamId { get; set; }
        public int SeasonId { get; set; }
        public bool IsCatchball { get; set; }
        public IList<IGrouping<League, IGrouping<Stage, GamesCycle>>> LeaguesWithCycles { get; set; }
        public IList<Club> Clubs { get; set; }
        public Dictionary<int, string> RefereesNames { get; set; }
        public bool IsWaterpolo { get; set; }
    }
}