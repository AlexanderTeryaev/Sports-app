using System;
using System.ComponentModel.DataAnnotations;

namespace CmsApp.Models
{
    public class GameForm
    {
        public int GameId { get; set; }
        public int LeagueId { get; set; }
        public int StageId { get; set; }
        public string GameDays { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required, RegularExpression(@"^(\d{2}:\d{2})$")]
        public string GamesInterval { get; set; }
        public int PointsWin { get; set; }
        public int PointsDraw { get; set; }
        public int PointsLoss { get; set; }
        public int PointsTechWin { get; set; }
        public int PointsTechLoss { get; set; }
        public string[] DaysList { get; set; }
        public int ActiveWeeksNumber { get; set; }
        public int BreakWeeksNumber { get; set; }
        public int? BestOfSets { get; set; }
        public int? NumberOfGames { get; set; }
        public bool PairsAsLastGame { get; set; }
        public bool IsTennisLeague { get; set; }
        public int? TechWinHomePoints { get; set; }
        public int? TechWinGuestPoints { get; set; }
        public int? RoundStartCycle { get; set; }
        public int? ShowCyclesOnExternal { get; set; }
        public bool IsDivision { get; set; }

        public bool CreateCrossesStage { get; set; }
        public bool IsCrossesStageApplicable { get; set; }
        public string CyclesStartDate { get; set; }
        public int NumberOfCycles { get; set; }

        public bool IsDateForFirstCycleOnly { get; set; }
    }
}