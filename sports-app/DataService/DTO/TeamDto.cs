using System;
using System.Runtime.Serialization;

namespace DataService.DTO
{
    public class TeamDto
    {
        public int TeamId { get; set; }
        public string Title { get; set; }
        public string SchoolName { get; set; }
        public int LeagueId { get; set; }
        public string LeagueName { get; set; }
        public string Logo { get; set; }
        public string Address { get; set; }
        public int ClubId { get; set; }
        public bool IsActive { get; set; }
        public bool IsTrainerPlayer { get; set; }
        public string ClubName { get; set; }

        public RetirementRequestDto RetirementRequest { get; set; }

        [IgnoreDataMember]
        public int? SeasonId { get; set; }

        public int DisciplineId { get; set; }
        public string DisciplineName { get; set; }

        public bool IsApprovedByManager { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public bool IsSchoolTeam { get; set; }
        public bool IsTrainingTeam { get; set; }
        public string UserActionName { get; set; }
        public int SortIndex { get; set; }
    }
}
