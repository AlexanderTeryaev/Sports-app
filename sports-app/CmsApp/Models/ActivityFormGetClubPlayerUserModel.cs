using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class ActivityFormGetClubPlayerUserModel
    {
        public int Id { get; set; }
        public string IdNum { get; set; }

        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public int? Gender { get; set; }
        public string ParentName { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string BirthDate { get; set; }

        public string ProfilePicture { get; set; }
        public string MedicalCert { get; set; }
        public string InsuranceCert { get; set; }

        public bool HideMedicalCert { get; set; }

        public bool MultiTeamEnabled { get; set; }

        public List<ActivityFormGetClubPlayerUserTeamModel> Teams { get; set; }
        public List<ActivityFormGetClubPlayerUserSchoolModel> Schools { get; set; }
    }

    public class ActivityFormGetClubPlayerUserSchoolModel
    {
        public int SchoolId { get; set; }
        public string SchoolName { get; set; }
    }

    public class ActivityFormGetClubPlayerUserTeamModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string LeaguesNames { get; set; }
        public int? SchoolId { get; set; }
        public decimal RegistrationAndEquipmentPrice { get; set; }
        public decimal ParticipationPrice { get; set; }
        public decimal InsurancePrice { get; set; }
    }
}