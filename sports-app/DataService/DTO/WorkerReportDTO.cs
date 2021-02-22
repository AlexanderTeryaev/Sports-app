using System;
using System.Collections.Generic;
using System.Linq;
using AppModel;

namespace DataService.DTO
{
    public class WorkerReportDTO
    {
        public int WorkerId { get; set; }
        public string WorkerIdentNum { get; set; }
        public string WorkerFullName { get; set; }
        public int? OfficialTypeId { get; set; }
        public List<GameShortDto> GamesAssigned { get; set; }
        public int DaysCount { get; set; }
        public int GamesCount { get; set; }
        public string SeasonName { get; set; }
        public int? WithholdingTax { get; set; }
        public decimal TotalFeeCount { get; set; }
        public IEnumerable<IGrouping<League, GameShortDto>> LeaguesGrouped { get; set; }
        public string WorkerRate { get; set; }
        public int JobId { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public int UserJobId { get; set; }
    }

    public class GameShortDto
    {
        public int Id { get; set; }
        public DateTime? StartDate { get; set; }
        public string AuditoriumName { get; set; }
        public string AuditoriumAddress { get; set; }
        public string HomeTeamName { get; set; }
        public string GuestTeamName { get; set; }
        public League League { get; set; }
        ///<summary>Distance should be set in the km (if it is in miles - convert to km)</summary>
        public double? TravelDistance { get; set; }
        public double WorkedHours { get; set; }
        public decimal? LeagueFee { get; set; }
        public Union Union { get; set; }
        public bool IsUnionReport { get; set; }
        public string Role { get; set; }
        public string OfficialAddress { get; set; }
        public string OfficialCity { get; set; }
        public string Comment { get; set; }
        public List<DateTime?> DaysWorked { get; set; }
    }
}