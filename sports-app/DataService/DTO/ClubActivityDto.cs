using System;
using System.Collections.Generic;

namespace DataService.DTO
{
    public class ClubActivityDto : ClubShort
    {
        public string NGONumber { get; set; }
        public int? ClubNumber { get; set; }
        public DateTime? DateOfApproval { get; set; }
        public int ApprovedCount { get; set; }
        public int FemaleCount { get; set; }
        public int TotalForVotes { get; set; }
        public int TotalForPreffered { get; set; }
        public int TotalForNonPreffered { get; set; }
        public int? Active { get; set; }
        public List<DisciplineDTO> DisciplinesInformation { get; set; }
        public int WaitingCount { get; set; }
        public int ThreeCompetitionsNumber { get; set; }
        public int TwoCompetitionsNumber { get; set; }
        public int OneCompetitionNumber { get; set; }
    }
}
