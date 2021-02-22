using System.Collections.Generic;
using AppModel;
using DataService.DTO;

namespace CmsApp.Models
{
    public class TeamStatisticsForm
    {
        public int SeasonId { get; set; }
        public int TeamId { get; set; }
        public IDictionary<Season, IEnumerable<AveragePlayersStatistics>> GuestStatistics { get; set; }
        public IDictionary<Season, IEnumerable<AveragePlayersStatistics>> HomeStatistics { get; set; }
        public IEnumerable<Season> Seasons { get; set; }
        public IDictionary<Season, AveragePlayersStatistics> GeneralStatistics { get; set; }
    }
}