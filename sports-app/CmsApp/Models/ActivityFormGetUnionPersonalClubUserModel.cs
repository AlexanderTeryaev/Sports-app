using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class ActivityFormGetUnionPersonalClubUserModel
    {
        public int Id { get; set; }
        public string IdNum { get; set; }

        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        #region Ukraine gymnastic union specific fields

        public string MotherName { get; set; }
        public string ParentPhone { get; set; }
        public string IdentCard { get; set; }
        public string LicenseDate { get; set; }
        public string ParentEmail { get; set; }
        public string ParentName { get; set; }
        public bool IsFilteredByRegion { get; set; }

        #endregion

        public int? Gender { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string BirthDate { get; set; }

        public string PostalCode { get; set; }
        public string PassportNum { get; set; }
        public string ForeignFirstName { get; set; }
        public string ForeignLastName { get; set; }

        public string ProfilePicture { get; set; }
        public string IdFile { get; set; }
        public string MedicalCert { get; set; }
        public string InsuranceCert { get; set; }

        public string MedExamDate { get; set; }
        public string DateOfInsuranceValidity { get; set; }
        public string TenicardValidity { get; set; }

        public decimal RegularRegistrationPrice { get; set; }
        public decimal CompetitiveRegistrationPrice { get; set; }
        public decimal InsurancePrice { get; set; }
        public decimal TenicardPrice { get; set; }

        public List<ActivityFormGetUnionPersonalClubClubModel> Clubs { get; set; }
        public List<ActivityFormGetUnionPersonalClubRegionModel> Regions { get; set; }

        public int CurrentClubId { get; set; }
        public int CurrentTeamId { get; set; }

        public bool BirthdateNeeded { get; set; }
    }

    public class ActivityFormGetUnionPersonalClubRegionModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ActivityFormGetUnionPersonalClubClubModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ActivityFormGetUnionPersonalClubTeamsModel
    {
        public List<ActivityFormGetUnionPersonalClubTeamsTeamModel> Teams { get; set; }
        public bool MultiTeamEnabled { get; set; }
    }

    public class ActivityFormGetUnionPersonalClubTeamsTeamModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}