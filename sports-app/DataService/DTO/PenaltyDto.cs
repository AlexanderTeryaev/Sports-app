using System;

namespace DataService.DTO
{
    public class PenaltyDto
    {
        public int Id { get; set; }
        public DateTime DateOfExclusion { get; set; }
        public int ExclusionNumber { get; set; }
        public string LeagueName { get; set; }
    }

    public class PenaltyInformationDto : PenaltyDto
    {
        public string TeamName { get; set; }
        public bool IsEnded { get; set; }
        public string UserActionName { get; set; }
    }
}