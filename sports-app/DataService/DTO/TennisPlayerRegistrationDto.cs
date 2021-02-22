using System;

namespace DataService.DTO
{
    public class TennisPlayerRegistrationDto
    {
        public int TeamPlayerId { get; set; }
        public string ClubName { get; set; }
        public string TeamName { get; set; }
        public string FullName { get; set; }
        public string IdentNum { get; set; }
        public DateTime? BirthDay { get; set; }
        public string Phone { get; set; }
        public DateTime? TennicardValidity { get; set; }
        public int TeamId { get; set; }
        public DateTime? MedicalValidity { get; set; }
        public DateTime? InsuranceValidity { get; set; }
        public int? TeamPositionOrder { get; set; }
    }

    public class PlayerRegistrationDto
    {
        public int RegistrationId { get; set; }
        public int UserId { get; set; }
        public int? ClubNumber { get; set; }
        public string ClubName { get; set; }
        public string FullName { get; set; }
        public string IdentNum { get; set; }
        public double? FinalScore { get; set; }
        public int? Rank { get; set; }
        public DateTime? Birthday { get; set; }
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; }
        public string Gender { get; set; }

    }
}
