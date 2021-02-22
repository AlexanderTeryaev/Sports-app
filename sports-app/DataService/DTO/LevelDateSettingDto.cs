using System;

namespace DataService.DTO
{
    public class LevelDateSettingDto
    {
        public int Id { get; set; }
        public int CompetitionLevelId { get; set; }
        public DateTime? QualificationStartDate { get; set; }
        public DateTime? QualificationEndDate { get; set; }
        public DateTime? FinalStartDate { get; set; }
        public DateTime? FinalEndDate { get; set; }
    }
}
