using AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CmsApp.Helpers;

namespace CmsApp.Models
{
    public class ActivityBranchForm
    {
        public ActivityBranchForm() {}

        public ActivityBranchForm(ActivityBranch branch)
        {
            ActivityBranchId = branch.AtivityBranchId;
            ActivityBranchName = branch.BranchName;

            UnionId = branch.UnionId;
            ClubId = branch.ClubId;
            SeasonId = branch.SeasonId;
        }

        public int ActivityBranchId { get; set; }
        public string ActivityBranchName { get; set; }

        public int? UnionId { get; set; }
        public int? ClubId { get; set; }
        public int? SeasonId { get; set; }
    }

    public class ActivityModel
    {
        public ActivityModel()
        {
            ActivityManager = new List<ActivitiesUserForm>();
            ActivityViewer = new List<ActivitiesUserForm>();
            ActivitiesPrices = new List<ActivitiesPriceForm>();
            UnionManager = new List<SelectListItem>();

            FormPayment = ActivityFormPaymentType.Fixed;
            Type = ActivityType.Group;
        }

        public ActivityModel(Activity activity)
        {
            ActivityId = activity.ActivityId;
            AttachDocuments = activity.AttachDocuments;
            ByBenefactor = activity.ByBenefactor;
            Date = activity.Date;
            Description = activity.Description;
            EndDate = activity.EndDate;
            FormPayment = activity.FormPayment;
            InsuranceCertificate = activity.InsuranceCertificate;
            IsPublished = activity.IsPublished;
            MedicalCertificate = activity.MedicalCertificate;
            Name = activity.Name;
            PaymentDescription = activity.PaymentDescription;
            Price = activity.Price;
            StartDate = activity.StartDate;
            Type = activity.Type;
            UnionId = activity.UnionId;
            ClubId = activity.ClubId;
            SeasonId = activity.SeasonId;
            BranchId = activity.ActivityBranchId;
            IsAutomatic = activity.IsAutomatic;

            SecondTeamRegistrationDiscountAmount = activity.SecondTeamDiscount;
            SecondTeamRegistrationNoInsurancePayment = activity.SecondTeamNoInsurance;

            EnableBrotherDiscount = activity.EnableBrotherDiscount;
            BrotherDiscountAmount = activity.BrotherDiscountAmount;
            BrotherDiscountInPercent = activity.BrotherDiscountInPercent;

            RestrictLeagues = activity.RestrictLeagues;
            RestrictSchools = activity.RestrictSchools;
            RestrictTeams = activity.RestrictTeams;

            RegistrationPrice = activity.RegistrationPrice;
            InsurancePrice = activity.InsurancePrice;
            ParticipationPrice = activity.ParticipationPrice;
            MembersFee = activity.MembersFee;
            HandlingFee = activity.HandlingFee;
            AllowNoRegistrationPayment = activity.AllowNoRegistrationPayment;
            AllowNoInsurancePayment = activity.AllowNoInsurancePayment;
            AllowNoParticipationPayment = activity.AllowNoParticipationPayment;
            AllowNoFeePayment = activity.AllowNoFeePayment;
            AllowNoHandlingFeePayment = activity.AllowNoHandlingFeePayment;
            NoPriceOnBuiltInRegistration = activity.NoPriceOnBuiltInRegistration;
            ActivityFormType = activity.GetFormType();
            CustomPricesEnabled = activity.CustomPricesEnabled;
            ActivityCustomPrices = activity.ActivityCustomPrices.ToList();
            NoTeamRegistration = activity.NoTeamRegistration;
            AllowNewTeamRegistration = activity.AllowNewTeamRegistration;

            UnionApprovedPlayerDiscountAmount = activity.UnionApprovedPlayerDiscount;
            UnionApprovedPlayerNoInsurancePayment = activity.UnionApprovedPlayerNoInsurance;
            EscortDiscountAmount = activity.EscortDiscount;
            EscortNoInsurancePayment = activity.EscortNoInsurance;
            AllowEscortRegistration = activity.AllowEscortRegistration;
            RestrictCustomPricesToOneItem = activity.RestrictCustomPricesToOneItem;
            ShowOnlyApprovedTeams = activity.ShowOnlyApprovedTeams;
            DisableRegPaymentForExistingClubs = activity.DisableRegPaymentForExistingClubs;
            RegisterToTrainingTeamsOnly = activity.RegisterToTrainingTeamsOnly;
            AllowNoCustomPricesSelected = activity.AllowNoCustomPricesSelected;

            CreateClubTeam = activity.CreateClubTeam;
            AllowCompetitiveMembers = activity.AllowCompetitiveMembers;
            AllowOnlyApprovedMembers = activity.AllowOnlyApprovedMembers;
            DoNotAllowDuplicateRegistrations = activity.DoNotAllowDuplicateRegistrations;
            OnlyApprovedClubs = activity.OnlyApprovedClubs;

            RegistrationForMembers = (RegistrationForMembers)activity.RegistrationForMembers;

            DefaultLanguage = activity.DefaultLanguage;

            CheckCompetitionAge = activity.CheckCompetitionAge;
            RestricGenders = activity.RestrictGenders;
            RestricGendersByTeams = activity.RestrictGendersByTeam;

            RestrictLeaguesByAge = activity.RestrictLeaguesByAge;

            ClubLeagueTeamsOnly = activity.ClubLeagueTeamsOnly;
            PostponeParticipationPayment = activity.PostponeParticipationPayment;

            MultiTeamRegistration = activity.MultiTeamRegistrations;

            RedirectLinkOnSuccess = activity.RedirectLinkOnSuccess;

            IsUkrainianUnion = activity.UnionId == GlobVars.UkraineGymnasticUnionId ||
                               activity.Club?.UnionId == GlobVars.UkraineGymnasticUnionId;

            MovePlayerToTeam = activity.MovePlayerToTeam;

            AdjustRegistrationPriceByDate = activity.AdjustRegistrationPriceByDate;
            AdjustParticipationPriceByDate = activity.AdjustParticipationPriceByDate;
            AdjustInsurancePriceByDate = activity.AdjustInsurancePriceByDate;
            AllowToEnterDateToAdjustPrices = activity.AllowToEnterDateToAdjustPrices;

            PaymentMethod = (ActivityPaymentMethod)activity.PaymentMethod;

            ForbidToChangeNameForExistingPlayers = activity.ForbidToChangeNameForExistingPlayers;

            IsIndividualSection = activity.Union?.Section?.IsIndividual == true;
            RegistrationsByCompetitionsCategory = activity.RegistrationsByCompetitionsCategory;
        }

        public int ActivityId { get; set; }
        public ActivityFormType ActivityFormType { get; set; }
        public bool IsPublished { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PaymentDescription { get; set; }
        public decimal? Price { get; set; }
        public bool ByBenefactor { get; set; }
        public bool AttachDocuments { get; set; }
        public bool MedicalCertificate { get; set; }
        public bool InsuranceCertificate { get; set; }
        public string FormPayment { get; set; }
        public int? UnionId { get; set; }
        public int? ClubId { get; set; }
        public int? SeasonId { get; set; }
        public int BranchId { get; set; }
        public bool? IsAutomatic { get; set; }

        public decimal? SecondTeamRegistrationDiscountAmount { get; set; }
        public bool SecondTeamRegistrationNoInsurancePayment { get; set; }

        public bool RestrictLeagues { get; set; }
        public List<League> RestrictedLeagues { get; set; }
        public string RestrictedLeaguesJson { get; set; }
        public List<League> Leagues { get; set; }

        public bool RestrictSchools { get; set; }
        public List<School> RestrictedSchools { get; set; }
        public string RestrictedSchoolsJson { get; set; }
        public List<School> Schools { get; set; }

        public bool RestrictTeams{ get; set; }
        public List<DropdownItem> RestrictedTeams { get; set; }
        public string RestrictedTeamsJson { get; set; }
        public List<DropdownItem> Teams { get; set; }

        public bool RegistrationPrice { get; set; }
        public bool InsurancePrice { get; set; }
        public bool ParticipationPrice { get; set; }
        public bool MembersFee { get; set; }
        public bool HandlingFee { get; set; }
        public bool AllowNoRegistrationPayment { get; set; }
        public bool AllowNoInsurancePayment { get; set; }
        public bool AllowNoParticipationPayment { get; set; }
        public bool AllowNoFeePayment { get; set; }
        public bool AllowNoHandlingFeePayment { get; set; }
        public bool NoPriceOnBuiltInRegistration { get; set; }

        public IList<ActivitiesPriceForm> ActivitiesPrices { get; set; }
        public IList<ActivitiesUserForm> ActivityManager { get; set; }
        public IList<ActivitiesUserForm> ActivityViewer { get; set; }
        
        public List<SelectListItem> UnionManager { get; set; }

        public bool CustomPricesEnabled { get; set; }
        public List<ActivityCustomPrice> ActivityCustomPrices { get; set; }

        public bool NoTeamRegistration { get; set; }
        public bool AllowNewTeamRegistration { get; set; }

        public decimal? UnionApprovedPlayerDiscountAmount { get; set; }
        public bool UnionApprovedPlayerNoInsurancePayment { get; set; }
        public decimal? EscortDiscountAmount { get; set; }
        public bool EscortNoInsurancePayment { get; set; }
        public bool AllowEscortRegistration { get; set; }

        public bool RestrictCustomPricesToOneItem { get; set; }
        public bool AllowNoCustomPricesSelected { get; set; }
        public bool ShowOnlyApprovedTeams { get; set; }

        public bool DisableRegPaymentForExistingClubs { get; set; }

        public bool RegisterToTrainingTeamsOnly { get; set; }
        public bool CreateClubTeam { get; set; }

        public bool AllowCompetitiveMembers { get; set; }
        public bool AllowOnlyApprovedMembers { get; set; }
        public bool DoNotAllowDuplicateRegistrations { get; set; }
        public bool OnlyApprovedClubs { get; set; }

        public RegistrationForMembers RegistrationForMembers { get; set; }

        public string DefaultLanguage { get; set; }

        public bool CheckCompetitionAge { get; set; }
        public bool RestricGenders { get; set; }
        public bool RestricGendersByTeams { get; set; }

        public bool ClubLeagueTeamsOnly { get; set; }
        public bool PostponeParticipationPayment { get; set; }

        public bool MultiTeamRegistration { get; set; }

        public string RedirectLinkOnSuccess { get; set; }

        public bool IsUkrainianUnion { get; set; }
        public bool MovePlayerToTeam { get; set; }
        public bool AdjustRegistrationPriceByDate { get; set; }
        public bool AdjustParticipationPriceByDate { get; set; }
        public bool AdjustInsurancePriceByDate { get; set; }
        public bool AllowToEnterDateToAdjustPrices { get; set; }

        public bool EnableBrotherDiscount { get; set; }
        public decimal BrotherDiscountAmount { get; set; }
        public bool BrotherDiscountInPercent { get; set; }

        public bool RestrictLeaguesByAge { get; set; }

        public ActivityPaymentMethod PaymentMethod { get; set; }

        public bool ForbidToChangeNameForExistingPlayers { get; set; }

        public bool IsIndividualSection { get; set; }
        public bool RegistrationsByCompetitionsCategory { get; set; }
    }

    public enum RegistrationForMembers
    {
        Both = 0,
        OnlyCompetitive,
        OnlyRegular
    }

    public class ActivitiesPriceForm
    {
        public int ActivityPeriodId { get; set; }
        public int ActivityId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
        public string PaymentDescription { get; set; }
    }

    public class ActivitiesUserForm
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public enum ActivityPaymentMethod
    {
        None = 0,
        CardCom,
        PayPal
    }
}