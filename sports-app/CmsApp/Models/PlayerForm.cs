using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using AppModel;
using DataService.DTO;
using System.Linq;
using DataService.Utils;
using CmsApp.Helpers.DataNotations;

namespace CmsApp.Models
{
    public class PlayerBaseForm
    {
        public int UserId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public string ParentName { get; set; }
        public string MotherName { get; set; }
        public string ParentPhone { get; set; }
        public string ParentEmail { get; set; }
        public string ForeignFirstName { get; set; }
        public string ForeignLastName { get; set; }
        public string Nationality { get; set; }
        public string CountryOfBirth { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public int SeasonId { get; set; }

        [UnAssignedTeamPlayerToClub("SeasonId")]
        public string IdentNum { get; set; }
        [UnAssignedTeamPlayerToClubByPass("SeasonId")]
        public string PassportNum { get; set; }

        [Required]
        public int GenderId { get; set; }
        public bool IsCompetitiveMember { get; set; }

        [Required]
        public DateTime? BirthDay { get; set; }
        public DateTime? PassportValidity { get; set; }
        public bool Masters { get; set; }
        public bool IsReligious { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public int Height { get; set; }
        public string Image { get; set; }
        public string IdentCard { get; set; }
        public string Telephone { get; set; }
        public DateTime? MedExamDate { get; set; }
        public bool MedicalCertificate { get; set; }
        public bool Insurance { get; set; }
        public IEnumerable<SelectListItem> Genders { get; set; }
        public IEnumerable<SelectListItem> ClassSList { get; set; }
        public IEnumerable<SelectListItem> ClassSBList { get; set; }
        public IEnumerable<SelectListItem> ClassSMList { get; set; }
        public List<SelectListItem> PersonalCoachesList { get; set; }
        public int? MedicalInstitutesId { get; set; }
        
        public int? Weight { get; set; }
        public string WeightUnits { get; set; }
        public DateTime? WeightDate { get; set; }

        public string IdType { get; set; }

        public string CompetitiveLicenseNumber { get; set; }
        public DateTime? LicenseValidity { get; set; }
        public string LicenseLevelId { get; set; }
        public int? NumberOfExclusion { get; set; }

        public DateTime? InitialApprovalDate { get; set; }
        public int? AthleteNumber { get; set; }
        public int? PersonalCoachId { get; set; }
        public int? MountainIronNumber { get; set; }
        public int? RoadIronNumber { get; set; }
        public int? VelodromeIronNumber { get; set; }
        public long? UciId { get; set; }
        public string ChipNumber { get; set; }
        public int? KitStatus { get; set; }
        
        public int? Age
        {
            get
            {
                if (BirthDay != null)
                {
                    var today = DateTime.Today;
                    // Calculate the age.
                    var age = today.Year - BirthDay.Value.Year;
                    // Go back to the year the person was born in case of a leap year
                    if (BirthDay > today.AddYears(-age))
                    {
                        age--;
                    }
                    return age;
                }
                else
                {
                    return -1;
                }
            }
        }

        public SelectList FriendshipsList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public SelectList RoadDisciplines { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public SelectList MountainDisciplines { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public SelectList FriendshipsTypeList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public int? FriendshipTypeId { get; set; }
        public int? FriendshipPriceType { get; set; }
        public int? RoadDisciplineId { get; set; }
        public int? MountaintDisciplineId { get; set; }

        public bool PaymentForUciId { get; set; }
        public bool PaymentForChipNumber { get; set; }
        public decimal FriendshipTotalPrice { get; set; }
        public BicycleFriendshipPayment FriendshipPayment { get; set; }
        public int FriendshipTeamPlayerId { get; set; }
        public int? InsuranceTypeId { get; set; }
        public List<SelectListItem> InsuranceTypesList { get; set; }
    }

    public class TeamPlayerForm : PlayerBaseForm
    {
        public int? ClubId { get; set; }
        public int? LeagueId { get; set; }
        public int TeamId { get; set; }
        [Range(0, int.MaxValue)]
        public int ShirtNum { get; set; }
        [Range(0, 9999)]
        public int TestResults { get; set; }
        public bool IsHadicapEnabled { get; set; }
        public int? PosId { get; set; }
        public bool IsActive { get; set; }
        public bool IsSectionTeam { get; set; }
        public IEnumerable<SelectListItem> Positions { get; set; }
        public IEnumerable<Discipline> UnionDisciplines { get; set; }
        public IEnumerable<int> DisciplinesIds { get; set; }
        public IEnumerable<int> PlayerDisciplineIds { get; set; }
        public bool IsGymnastic { get; set; }
        public bool IsUkraineGymnasticUnion { get; set; }
        public bool IsWaterpolo { get; set; }
        public bool IsNetBall { get; set; }
        public bool IsRowing { get; set; }
        public bool IsBicycle { get; set; }
        public bool IsSwimming { get; set; }
        public bool AlternativeId { get; set; }
        public bool IsClimbing { get; set; }

        public string DisciplinesString { get; set; }
        public string RanksString { get; set; }
        public string RoutesString { get; set; }

        public string TeamDisciplinesString { get; set; }
        public string TeamRanksString { get; set; }
        public string TeamRoutesString { get; set; }

        public bool Is31Union { get; set; }
        public bool IsExceptional { get; set; }
        public bool IsTennisExceptional { get; set; }
        public int? DepartmentSportId { get; set; }
        public bool IsMotorsport { get; internal set; }
        public IEnumerable<SelectListItem> DriverLicenceTypeList { get; set; }
        public bool IsAthletics { get; set; }
        public DateTime? ArgometricTestValidity { get; set; }
        public string ForeignName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<SelectListItem> ClubTeams { get; set; }
        public int? ClubTeamId { get; set; }
        public bool IsCurrentUserUnionManager { get; set; }

    }

    public class PlayerFormView : PlayerBaseForm
    {
        [Required]
        public string Password { get; set; }
        public bool IsValidUser { get; set; }
        public IList<TeamDto> PlayerTeams { get; set; }
        public IEnumerable<TeamDto> ManagerTeams { get; set; }
        public int LeagueId { get; set; }
        public int ClubId { get; set; }
        public int CurrentTeamId { get; set; }
        [Range(0, 9999)]
        public int TestResults { get; set; }

        public bool IsHadicapEnabled { get; set; }
        public decimal HandicapLevel { get; set; } = 1.0m;

        public int NumberOfTeamsUserPlays { get; set; }
        public List<PlayerHistoryFormView> PlayerHistories { get; set; }
        public string ShirtSize { get; set; }

        public bool RemoveImage { get; set; }

        public string IDFileName { get; set; }
        public bool RemoveIDFile { get; set; }

        [Obsolete("Use PlayerFiles")]
        public string InsuranceFile { get; set; }
        public bool RemoveInsuranceFile { get; set; }

        [Obsolete("Use PlayerFiles")]
        public string MedicalCertificateFile { get; set; }
        public bool RemoveMedicalCertificateFile { get; set; }

        [Obsolete("Use PlayerFiles")]
        public string ParentStatementFile { get; set; }
        public bool RemoveParentStatementFile { get; set; }

        public SectionModel Section { get; set; }
        public string SportRank { get; set; }

        public bool? IsApprovedByManager { get; set; }
        public List<PlayerFileModel> PlayerFiles { get; set; }
        public bool CanApproveRetirementRequests { get; set; }
        public bool CanApproveMedicalCertificate { get; set; }

        public IEnumerable<NationalTeamInvitementModel> NationalTeamInvitements { get; set; }
        public bool IsSectionTeam { get; set; }
        public IEnumerable<Discipline> UnionDisciplines { get; set; }
        public IEnumerable<int> DisciplinesIds { get; set; }
        public IEnumerable<int> PlayerDisciplineIds { get; set; }
        public DateTime? StartPlaying { get; internal set; }
        public DateTime? BlockadeEndDate { get; set; }
        public bool IsBlockade { get; set; }
        public IEnumerable<BlockadeHistoryDTO> BlockadeHistory { get; set; }
        public List<RegistrationsHistory> RegistrationsHistory { get; set; }
        public List<RetirementRequest> TeamRetirements { get; set; }
        public List<Club> PlayerClubs { get; set; }
        public bool IsReadOnly { get; set; }
        public int? DepartmentId { get; set; }
        public int? DepartmentSeasonId { get; set; }
        public int? SportId { get; set; }
        public CultEnum Culture { get; set; }
        public string ForeignName { get; set; }
        public DateTime? DateOfInsurance { get; set; }
        public DateTime? TenicardValidity { get; set; }
        public IEnumerable<SelectListItem> ListOfTrainingTeams { get; set; }
        public int[] TrainingTeamsIds { get; set; }
        public double? BasketballFiveLevelReduction { get; set; }

        [Obsolete("Use PlayerFiles")]
        public string LicenseFile { get; set; }
        public bool RemoveDriverLicenseFile { get; set; }
        public IEnumerable<SelectListItem> DriverLicenceTypeList { get; internal set; }
        public bool IsIndividualSection { get; internal set; }
        public int? TeamPlayerId { get; internal set; }
        public IEnumerable<PenaltyInformationDto> PlayersPenaltiesHistory { get; internal set; }
        public bool IsUnderPenalty { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string FullNameFormatted { get; set; }

        public int? leagueForExclusionId { get; set; }
        public string PassportFileName { get; set; }
        public bool RemovePassportFile { get; set; }
        public int? AthleteNumber { get; set; }

        public int? ClassS { get; set; }
        public int? ClassSB { get; set; }
        public int? ClassSM { get; set; }

        public bool IsApproved { get; set; }

        public DateTime? ArgometricTestValidity { get; set; }
        public string SectionAlias { get; set; }

        public List<SelectListItem> AthleticTeams { get; set; }
        public int? AthleticTeamId { get; set; }

        public bool IsAlternativeId { get; set; }
        public bool IsUkraineGymnasticUnion { get; set; }

        public int? Age
        {
            get
            {
                if (BirthDay != null)
                {
                    var today = DateTime.Today;
                    // Calculate the age.
                    var age = today.Year - BirthDay.Value.Year;
                    // Go back to the year the person was born in case of a leap year
                    if (BirthDay > today.AddYears(-age))
                    {
                        age--;
                    }
                    return age;
                }
                else
                {
                    return -1;
                }
            }
        }

        public bool IsNationalSportsman { get; set; }

        //for climbing user

        public int? ShoesSize { get; set; }
        public bool RemoveParentApprovalFile { get; set; }
        public DateTime? ArmyDraftDate { get; set; }
        public string MedicalInformation { get; set; }
        public int? AuditoriumId { get; set; }
        public int? PlayerSeasonAge { get; set; }
        public bool RemoveSpecialClassificationFile { get; set; }
        public string BicycleCategory { get; set; }
        public bool EnableIDCorrectionCheck { get; set; }
        //public string BicycleCategory { get; set; }
        public string BicycleMountaintCategory { get; set; }
        public string BicycleRoadCategory { get; set; }
        public string BicycleIsrChampCategory { get; set; }
        public bool SaveAsDraft { get; set; } = false;

        public string TeamForUci { get; set; }
        public int? HeatTypeForUciCard { get; set; }
       
    }

    public class RegistrationsHistory
    {
        public int SeasonId { get; set; }
        public string SeasonName { get; set; }

        public DateTime? ApprovalDate { get; set; }
        public string ActivityName { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string LeagueName { get; set; }

        public decimal RegistrationPaid { get; set; }
        public decimal InsurancePaid { get; set; }
        public decimal TenicardPaid { get; set; }
        public decimal ParticipationPaid { get; set; }
        public decimal MembersFeePaid { get; set; }
        public decimal HandlingFeePaid { get; set; }
        public List<ActivityCustomPriceModel> CustomPrices { get; set; }
        public string UserActionName { get; set; }

        public int? CardComInvoiceNumber { get; set; }
        public int? CardComNumberOfPayments { get; set; }
    }

    public class NationalTeamInvitementModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class FriendshipCardModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int SeasonId { get; set; }
        public string SeasonName { get; set; }
        public string IdentNum { get; set; }
        public int? GenderId { get; set; }
        public string GenderName { get; set; }
        public DateTime? BirthDay { get; set; }
        public string PlayerImage { get; set; }
        public string UnionLogo { get; set; }
        public string UnionName { get; set; }
        public string UnionForeignName { get; set; }
        public string ContentForFriendshipCard { get; set; }
        public string ClubName { get; set; }

        public long? UciId { get; set; }
        public string RoadHeatName { get; set; }
        public string MountainHeatName { get; set; }
        public bool IsHebrew { get; set; }
    }

    public class UciCardModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int SeasonId { get; set; }
        public string SeasonName { get; set; }
        public string IdentNum { get; set; }
        public int? GenderId { get; set; }
        public string GenderName { get; set; }
        public DateTime? BirthDay { get; set; }
        public string PlayerImage { get; set; }
        public string UnionLogo { get; set; }
        public string UnionName { get; set; }
        public string UnionForeignName { get; set; }
        public string ContentForFriendshipCard { get; set; }
        public string ClubName { get; set; }
        public string Nationality { get; set; }
        public string Role { get; set; }
        public string Function { get; set; }
        public string UciCategory { get; set; }
        public string TeamForUci { get; set; }
        public string NationalCategory { get; set; }
        public string UnionAddress { get; set; }
        public string UnionWebSite { get; set; }
        public string UnionEmail { get; set; }
        public string UnionPhone { get; set; }
        public string EmergencyContact { get; set; }


        public long? UciId { get; set; }
        public string RoadHeatName { get; set; }
        public string MountainHeatName { get; set; }
        public bool IsHebrew { get; set; }
    }
}