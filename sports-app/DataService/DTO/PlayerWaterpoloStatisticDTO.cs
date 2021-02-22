using AppModel;
using System;

namespace DataService.DTO
{
    public class WaterpoloStatisticDTO
    {
    }

    public class PlayersWaterStatisticsDTO : WaterpoloStatisticDTO
    {
        public int? PlayersId { get; set; }
        public string PlayersName { get; set; }
        public string PlayersImage { get; set; }
        public int GamesCount { get; set; }
    }
}
