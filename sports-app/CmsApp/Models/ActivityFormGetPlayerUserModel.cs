using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class ActivityFormGetPlayerUserModel
    {
        public int Id { get; set; }
        public string IdNum { get; set; }

        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public int? Gender { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string BirthDate { get; set; }

        public string ProfilePicture { get; set; }
        public string MedicalCert { get; set; }
        public string InsuranceCert { get; set; }

        public List<ActivityFormGetPlayerUserTeamModel> Teams { get; set; }
    }

    public class ActivityFormGetPlayerUserTeamModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int LeagueId { get; set; }
        public string League { get; set; }
        public int SeasonId { get; set; }
        public decimal PlayerRegistrationPrice { get; set; }
        public decimal PlayerInsurancePrice { get; set; }
    }
}