using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppModel;

namespace CmsApp.Models
{
    public class ActivityRegistrationsStatusModel
    {
        public int ActivityId { get; set; }
        public Activity Activity { get; set; }
        public int SeasonId { get; set; }
        public bool BenefactorEnabled { get; set; }
        public bool DocumentEnabled { get; set; }
        public bool MedicalCertEnabled { get; set; }
        public bool InsuranceCertEnabled { get; set; }

        public bool CanEdit { get; set; }
        public bool CanPay { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApproveMedicalCert { get; set; }
        public bool CanLockUnlockPlayers { get; set; }

        public string Title { get; set; }

        public CultEnum Culture { get; set; }

        public List<ActivityFormsDetail> FormFields { get; set; }

        public List<ActivityCustomPrice> ActivityCustomPrices { get; set; }

        public IEnumerable<ActivityRegistrationItem> Registrations { get; set; }

        public bool IsDataTruncated { get; set; }

        public int[] HiddenColumns { get; set; }
        public string ColumnsOrder { get; set; }
        public string ColumnsNames { get; set; } //JSON
        public string ColumnsSorting { get; set; }

        public bool IsRegionalLevelEnabled { get; set; }
    }

    public class ActivityRegistrationItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? TeamPlayerId { get; set; }
        public string UserIdNum { get; set; }
        public string BrotherIdNum { get; set; }

        public string PlayerFullName { get; set; }
        public string PlayerFirstName { get; set; }
        public string PlayerLastName { get; set; }
        public string PlayerMiddleName { get; set; }

        public string PlayerFatherName { get; set; }
        public string PlayerMotherName { get; set; }
        public string PlayerParentPhone { get; set; }
        public string PlayerParentEmail { get; set; }

        public string PlayerIdentCard { get; set; }
        public DateTime? PlayerLicenseDate { get; set; }

        public DateTime? DateOfMedicalExamination { get; set; }
        public DateTime? InsuranceValidity { get; set; }

        public string PlayerGender { get; set; }
        public string PlayerEmail { get; set; }
        public string PlayerPhone { get; set; }
        public string PlayerCity { get; set; }
        public string PlayerAddress { get; set; }
        public DateTime? PlayerBirthDate { get; set; }
        public string Team { get; set; }
        public int? TeamId { get; set; }
        public string League { get; set; }
        public int? LeagueId { get; set; }

        public string Club { get; set; }
        public int? ClubId { get; set; }
        public int ClubNumberOfCourts { get; set; }
        public string ClubNGONumber { get; set; }
        public string ClubNameOfSportsCentre { get; set; }
        public string ClubAddress { get; set; }
        public string ClubPhone { get; set; }
        public string ClubEmail { get; set; }
        public bool ClubRegionalApproved { get; set; }
        public string ClubRegion { get; set; }

        public string School { get; set; }
        public string NameForInvoice { get; set; }
        public string PaymentByBenefactor { get; set; }
        public string Document { get; set; }
        public string MedicalCert { get; set; }
        public bool MedicalCertApproved { get; set; }
        public string InsuranceCert { get; set; }
        public string Comments { get; set; }
        public bool NeedShirts { get; set; }
        public bool SelfInsurance { get; set; }
        public DateTime DateSubmitted { get; set; }
        public bool IsPaymentByBenefactor { get; set; }
        public bool IsActive { get; set; }
        public bool? IsLocked { get; set; }
        public bool RegisteredMoreThanOnce { get; set; }

        public decimal? LeaguePrice { get; set; }
        public decimal? ByBenefactorPrice { get; set; }
        public decimal? Paid { get; set; }
        public string ClubComment { get; set; }
        public string UnionComment { get; set; }

        public decimal? InsurancePrice { get; set; }
        public decimal? InsurancePaid { get; set; }

        public decimal? RegistrationPaid { get; set; }

        public List<ActivityFormCustomField> CustomFields { get; set; }
        public List<ActivityCustomPriceModel> CustomPrices { get; set; }

        public string IdFile { get; set; }
        public string ProfilePicture { get; set; }

        public decimal? ParticipationPrice { get; set; }
        public decimal? ParticipationPaid { get; set; }

        public decimal? MembersFee { get; set; }
        public decimal? MembersFeePaid { get; set; }

        public decimal? HandlingFee { get; set; }
        public decimal? HandlingFeePaid { get; set; }

        public decimal? TenicardPrice { get; set; }
        public decimal? TenicardPaid { get; set; }

        public bool? DisableMembersFeePayment { get; set; }
        public bool? DisableHandlingFeePayment { get; set; }
        public bool? DisableInsurancePayment { get; set; }
        public bool? DisableRegistrationPayment { get; set; }
        public bool? DisableParticipationPayment { get; set; }

        public bool IsTrainerPlayer { get; set; }
        public bool IsEscortPlayer { get; set; }
        public bool IsUnionPlayer { get; set; }

        public bool ApprovedPlayerNoInsurance { get; set; }

        public string ClubCertificateOfIncorporation { get; set; }
        public string ClubApprovalOfInsuranceCover { get; set; }
        public string ClubAuthorizedSignatories { get; set; }

        public bool IsSchoolInsurance { get; set; }
        public bool DoNotPayTenicard { get; set; }
        public bool IsCompetitiveMember { get; set; }

        public bool IsTennisCompetition { get; set; }

        public int? CardComNumberOfPayments { get; set; }
        public int? CardComInvoiceNumber { get; set; }
        public DateTime? PlayerStartDate { get; set; }

        public string PlayerPostalCode { get; set; }
        public string PlayerPassportNumber { get; set; }
        public string PlayerForeignFirstName { get; set; }
        public string PlayerForeignLastName { get; set; }

        public int? CompetitionId { get; set; }
        public string CompetitionName { get; set; }
        public string CompetitionCategoryName { get; set; }
    }
}