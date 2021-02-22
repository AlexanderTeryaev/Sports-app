using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class ActivityFormGetUnionCustomPersonalPlayerUserModel
    {
        public int Id { get; set; }
        public bool PlayerExist { get; set; }
        public string IdNum { get; set; }

        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public int? Gender { get; set; }
        public string ParentName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string BirthDate { get; set; }

        public string ProfilePicture { get; set; }
        public string MedicalCert { get; set; }
        public string InsuranceCert { get; set; }
        public string IdFile { get; set; }

        public bool HideMedicalCert { get; set; }

        public decimal ApprovedPlayerCustomPricesDiscount { get; set; }
        public decimal EscortPlayerCustomPricesDiscount { get; set; }
        public bool EscortNoInsurance { get; set; }

        public bool RestrictLeaguesByAge { get; set; }

        public List<ActivityFormGetUnionCustomPersonalPlayerUserTeamModel> Teams { get; set; }
        public List<ActivityFormGetUnionCustomPersonalPlayerLeagueModel> Leagues { get; set; }
    }

    public class ActivityFormGetUnionCustomPersonalPlayerLeagueModel
    {
        public int LeagueId { get; set; }
        public string LeagueName { get; set; }
    }

    public class ActivityFormGetUnionCustomPersonalPlayerUserTeamModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }

        public ActivityFormGetUnionCustomPersonalPlayerLeagueModel League { get; set; }

        public decimal RegistrationPrice { get; set; }
        public decimal InsurancePrice { get; set; }
        public decimal MembersFee { get; set; }
        public decimal HandlingFee { get; set; }
    }
}