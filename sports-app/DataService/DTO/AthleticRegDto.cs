using System;

namespace DataService.DTO
{
    public class AthleticRegDto
    {
        public int RegistrationId { get; set; }
        public int UserId { get; set; }
        public string DisciplineName { get; set; }
        public string CategoryName { get; set; }
        public string FullName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string IdentNum { get; set; }
        public string PassportNum { get; set; }
        public DateTime? BirthDay { get; set; }
        public double? BestResult { get; set; }
        public string ClubName { get; internal set; }
        public string RelatedClubName { get; set; }
        public int WeightDeclaration { get; set; }
        public int? AthleteNumber { get; set; }
        public bool IsApproved { get; set; }
        public bool IsCharged { get; set; }
        public string Heat { get; set; }
        public int? Lane { get; internal set; }
        public string RowingDistance { get; set; }
        public int? TeamNumber { get; set; }
        public string CoxwainFullName { get; set; }
        public int? ClubId { get; set; }
        public int? DisciplineId { get; set; }
        public int? SeassonId { get; set; }
        public string EntryTime { get; set; }
		public string Result { get; set; }
		public int? Rank { get; set; }
		public DateTime? CompetitionStartDate { get; set; }


		// Vitaly: I need this to get rid of coxwain subquery
		public int? CompetitionDisciplineTeamId { get; set; }
		public bool? IsCoxwain { get; set; }
	}
}
