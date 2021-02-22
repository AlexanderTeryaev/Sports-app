using AppModel;
using System;
using System.Collections.Generic;

namespace DataService.DTO
{
    public class PlayerStatusViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public bool IsApprovedByClubManager { get; set; }
        public bool IsPlayerRegistered { get; set; }
        public bool IsPlayerRegistrationApproved { get; set; }
        public bool IsApproveChecked { get; set; }
        public bool IsNotApproveChecked { get; set; }
        public bool IsApprovedInSubmitted { get; set; }
        public bool? IsActive { get; set; }
        public int? ClubId { get; set; }
        public bool IsBlockaded { get; set; }
        public bool IsYoung { get; set; }
        public bool HasMedicalCert { get; set; }
        public int? AthletesNumbers { get; set; }
        public bool IsAthleteNumberProduced { get; set; }
        public IEnumerable<int> DisciplinesIds { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int GenderId { get; set; }
        public DateTime? Birthday { get; set; }
        public string IdentNum { get; set; }
        public string PassportNum { get; internal set; }
        public int? CompetitionCount { get; set; }
        public int? ClassS { get; internal set; }
        public int? ClassSB { get; internal set; }
        public int? ClassSM { get; internal set; }
        public string TeamName { get; set; }
        public DateTime? MedExamDate { get; set; }
        public DateTime? RawTenicardValidity { get; set; }
        public DateTime? DateOfInsurance { get; set; }
    }


    public class PlayerViewModel : PlayerStatusViewModel
    {
        public int ShirtNum { get; set; }
        public int? PosId { get; set; }
        public int? SeasonId { get; set; }
        public int TeamId { get; set; }
        public bool? IsLocked { get; set; }
        public string BirthdayString { get; set; }
        public string WeightUnits { get; set; }
        public int? Weight { get; set; }
        public string Email { get; set; }
        public string ShirtSize { get; set; }
        public string Telephone { get; set; }
        public string City { get; set; }
        public Nullable<bool> MedicalCertificate { get; set; }
        public Nullable<bool> Insurance { get; set; }
        public string MedicalCertificateFile { get; set; }
        public string InsuranceFile { get; set; }

        public decimal PlayerRegistrationPrice { get; set; }
        public decimal PlayerInsurancePrice { get; set; }

        public string PlayerImage { get; set; }
        public decimal ManagerRegistrationDiscount { get; set; }
        public decimal ManagerParticipationDiscount { get; set; }

        public decimal PlayerRegistrationAndEquipmentPrice { get; set; }
        public decimal ParticipationPrice { get; set; }
        public bool NoInsurancePayment { get; set; }

        public string LeagueName { get; set; }
        public int? LeagueId { get; set; }
        public string TrainingTeamName { get; set; }
        public string LeagueTeamName { get; set; }

        public bool IsTrainingTeam { get; set; }
        public string Phone { get; internal set; }
        public int? Height { get; internal set; }
        public string Gender { get; internal set; }
        public string Position { get; internal set; }
        public string ParentName { get; internal set; }
        public string IDFile { get; internal set; }
        public string PassportFile { get; internal set; }
        public string ClubName { get; internal set; }
        public string DisciplinesNames { get; set; }
        public string ClubComment { get; set; }
        public string UnionComment { get; set; }
        public bool MedicalCertApproved { get; internal set; }
        public decimal BaseHandicap { get; set; }
        public decimal HandicapReduction { get; internal set; }
        public decimal FinalHandicap { get; internal set; }
        public DateTime? StartPlaying { get; internal set; }
        public string StartPlayingString { get; set; }
        public int? BlockadeId { get; set; }
        public DateTime? EndBlockadeDate { get; set; }
        public string EndBlockadeDateString { get; internal set; }
        public string MedExamDateString { get; set; }
        public string DateOfInsuranceString { get; set; }
		public string Route { get; set; }
		public string Rank { get; set; }
		public string Achievements { get; set; }
		public string AchievementsHeb { get; set; }
        public bool IsExceptional { get; set; }
        public bool ToWaitingStatus { get; set; }
        public DateTime? TenicardValidityDate { get; set; }
        public string TenicardValidity { get; set; }
        public string CompetitiveLicenseNumber { get; set; }
        public string LicenseValidity { get; set; }
        public string LicenseLevel { get; set; }
        public string DriverLicenseFile { get; set; }
        public bool IsActivePlayer { get; set; }
        public string ApprovalDate { get; set; }
        public bool IsNotActive { get; set; }
        public bool IsUnderPenalty { get; set; }
        public string InitialApprovalDate { get; set; }
        public bool IsFemale { get; set; }
        public bool ApprovedInPreviousSeason { get; set; }
        public int? SeasonIdOfCreation { get; set; }
        public string ParentStatementFile { get; internal set; }

        //bicycle readonly fields
        public int? FriendshipTypeId { get; set; }
        public string FriendshipTypeName { get; set; }
        public int? FriendshipPriceTypeId { get; set; }
        public string FriendshipPriceTypeName { get; set; }
        public string RoadHeat { get; set; }
        public string MountainHeat { get; set; }
        public string ChipNumber { get; set; }
        public string UciId { get; set; }
        public int? KitStatusId { get; set; }
        public string KitStatusName { get; set; }
        public string MountainIronNumber { get; set; }
        public string RoadIronNumber { get; set; }
        public bool PaymentForChipNumber { get; set; }
        public bool PaymentForUciId { get; set; }
        public decimal? FriendshipTotalPrice { get; set; }
        public bool FriendshipPaid { get; set; }
        public int? SeasonForAge { get; set; }
        public int? Age { get 
            {
                if (!SeasonForAge.HasValue || !Birthday.HasValue)
                    return null;
                return SeasonForAge.Value - Birthday.Value.Year;
            } 
        }
    }

    public class PlayersListForm
    {
        public int? ClubId { get; set; }
        public int? UnionId { get; set; }
        public IEnumerable<PlayerViewModel> PlayersList { get; set; }
        public int TotalPlayersCount { get; set; }
        public int CompletedPlayersCount { get; set; }
        public int ApprovedPlayersCount { get; set; }
        public int NotApprovedPlayersCount { get; set; }
        public int WaitingForApproval { get; set; }
        public int WaitingWithMedicalCert { get; set; }
        public int UnactivePlayers { get; set; }
        public LogicaName LogicalName { get; set; }
        public bool CanApprove { get; set; }
        public string HiddenColumns { get; set; }
        public int? SeasonId { get; set; }
        public IDictionary<int, Club> ClubsList { get; set; }
        public IDictionary<int, DisciplineDTO> DisciplinesList { get; set; }
        public string SelectedClubsIds { get; set; }
        public string SelectedDisciplinesIds { get; set; }
        public string SelectedStatusesIds { get; set; }
        public bool IsFiltered { get; set; }
        public bool IsHandicapEnabled { get; set; }
        public bool HasDisciplines { get; set; }
        public bool IsOverage { get; set; }
        public string OverageError { get; set; }
        public bool CanBlockade { get; set; }
        public bool IsGymnastic { get; set; }
        public bool IsWeightLifting { get; set; }
        public bool IsWaterpolo { get; set; }
        public bool IsUnionViewer { get; set; }
        public bool IsClubManager { get; set; }
        public bool IsCatchball { get; set; }
        public bool IsMotorsport { get; set; }
        public bool IsIndividual { get; set; }
        public bool CantChangeIfAccepted { get; set; }
        public string PlayerTypeString { get; set; }
        public bool IsAthletics { get; set; }
        public bool BlockRegistration { get; set; }
        public bool IsBasketball { get; set; }
        public bool IsTennis { get; set; }
        public bool IsSwimming { get; set; }
        public bool IsMartialArts { get; set; }
        public bool IsRowing { get; set; }
        public bool IsSurfing { get; set; }
        public bool IsBicycle { get; set; }
        public bool IsClimbing { get; set; }
        public bool ApprovePlayerByClubManagerFirst { get; set; }
    }
}