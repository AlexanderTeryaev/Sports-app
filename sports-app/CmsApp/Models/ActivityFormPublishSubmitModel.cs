using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace CmsApp.Models
{
    public class ActivityFormPublishSubmitModel
    {
        public string PlayerId { get; set; } //Identity number

        public string PlayerFullName { get; set; }
        public string PlayerFirstName { get; set; }
        public string PlayerLastName { get; set; }
        public string PlayerMiddleName { get; set; }

        public int? PlayerGender { get; set; }
        public string PlayerEmail { get; set; }
        public string PlayerPhone { get; set; }
        public string PlayerCity { get; set; }
        public string PlayerAddress { get; set; }
        public string PlayerParentName { get; set; }

        #region Ukraine gymnastic union specific fields

        public string PlayerMotherName { get; set; }
        public string PlayerParentPhone { get; set; }
        public string PlayerIdentCard { get; set; }
        public DateTime? PlayerLicenseDate { get; set; }
        public string PlayerParentEmail { get; set; }

        #endregion

        public DateTime? PlayerBirthDate { get; set; }
        public string PlayerTeam { get; set; }
        public int TeamId { get; set; }
        public int[] PlayerTeamMultiple { get; set; }
        public string PlayerLeague { get; set; }
        public int TeamsRegistrationPrice { get; set; }
  //    public string NameForInvoice { get; set; }

        public int LeagueId { get; set; }
        public int SeasonId { get; set; }
        public string PaymentByBenefactor { get; set; }
        public bool IsPaymentByBenefactor { get; set; }
        public string Document { get; set; }
        public string MedicalCert { get; set; }
        public bool SelfInsurance { get; set; }
        public string InsuranceCert { get; set; }
        public bool NeedShirts { get; set; }
        public string Comments { get; set; }

        public DateTime DateSubmitted { get; set; }

        public string CustomFields { get; set; } //JSON

        public bool DisableMembersFeePayment { get; set; }
        public bool DisableInsurancePayment { get; set; }
        public bool DisableRegistrationPayment { get; set; }

        public bool DisableParticipationPayment { get; set; }
        public bool PostponeParticipationPayment { get; set; }


        public bool DisableHandlingFeePayment { get; set; }
        public bool PlayerIsTrainer { get; set; }
        public bool PlayerIsEscort { get; set; }

        public int? Region { get; set; }

        public int? ClubId { get; set; }
        public string ClubName { get; set; }
        public int? ClubNumberOfCourts { get; set; }
        public string ClubNgoNumber { get; set; }
        public int? ClubSportsCenter { get; set; }
        public string ClubAddress { get; set; }
        public string ClubPhone { get; set; }
        public string ClubEmail { get; set; }

        public bool IsSchoolInsurance { get; set; }
        public bool DoNotPayTenicardPrice { get; set; }

        public bool IsCompetitiveMember { get; set; }

        public string PlayerBrotherIdForDiscount { get; set; }

        public DateTime? PlayerAdjustPricesStartDate { get; set; }

        public string PlayerPostalCode { get; set; }
        public string PlayerPassportNumber { get; set; }
        public string PlayerForeignFirstName { get; set; }
        public string PlayerForeignLastName { get; set; }

        public int[] PlayerCompetitionCategory { get; set; }
        public int[] RemovedRegistrations { get; set; }
    }
}