using System;

namespace DataService.DTO
{
    public class MartialArtsCompetitionDto
    {
        public int RegistrationId { get; set; }
        public string CompetitionName { get; set; }
        public string ClubName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? Points { get; set; }
        public int? Rank { get; set; }
        public int SeasonId { get; set; }
    }
}
