using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using AppModel;
using Omu.ValueInjecter;
using Resources;
using DataService.DTO;

namespace CmsApp.Models
{
    public class LeagueDetailsForm
    {
        public LeagueDetailsForm()
        {
            LeaguesTeamRegistrationPrice = new List<LeaguesPricesForm>();
            LeaguesPlayerRegistrationPrice = new List<LeaguesPricesForm>();
            LeaguesPlayerInsurancePrice = new List<LeaguesPricesForm>();
            MemberFees = new List<MemberFeeModel>();
            HandlingFees = new List<HandlingFeeModel>();
        }


        public int LeagueId { get; set; }
        public string Name { get; set; }
        public bool IsTeamCheck { get; set; }
        public bool IsCompetitionLeague { get; set; }
        
        public decimal MaximumHandicapScoreValue { get; set; }
        public bool IsHadicapEnabled { get; set; }
        public string Logo { get; set; }
        public int AgeId { get; set; }
        public int GenderId { get; set; }
        public string Image { get; set; }
        public short Gender { get; set; }
        public string Description { get; set; }
        public string Terms { get; set; }
        public IEnumerable<SelectListItem> Ages { get; set; }
        public IEnumerable<SelectListItem> Genders { get; set; }
        public int DocId { get; set; }
        public int PlayersCount { get; set; }
        public int OfficialsCount { get; set; }
        public int TeamsCount { get; set; }
        public bool IsIndividual { get; set; }
        public int? AuditoriumId { get; set; }
        public int? MaxRegistrations { get; set; }
        public int CompetitionType { get; set; }

        public List<SelectListItem> AuditoriumList { get; set; }
        public bool IsMastersCompetition { get; set; }


        [MaxLength(2000, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLengthAboutLeague")]
        public string AboutLeague { get; set; }

        [MaxLength(3000, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "MaxLengthLeagueStructure")]
        public string LeagueStructure { get; set; }

        public decimal? TeamRegistrationPrice { get; set; }
        public decimal? PlayerRegistrationPrice { get; set; }
        public decimal? PlayerInsurancePrice { get; set; }
        public DateTime? MaximumAge { get; set; }
        public DateTime? MinimumAge { get; set; }
        public string LeagueCode { get; set; }
        public int? MinimumPlayersTeam { get; set; }
        public int? MaximumPlayersTeam { get; set; }
        public DateTime? LeagueStartDate { get; set; }
        public DateTime? LeagueEndDate { get; set; }
        public DateTime? EndRegistrationDate { get; set; }
        public DateTime? StartRegistrationDate { get; set; }
        public int? MinParticipationReq { get; set; }

        public string PlaceOfCompetition { get; set; }

        public bool isTennisCompetition { get; set; }

        public List<LeaguesPricesForm> LeaguesTeamRegistrationPrice { get; set; }
        public List<LeaguesPricesForm> LeaguesPlayerRegistrationPrice { get; set; }
        public List<LeaguesPricesForm> LeaguesPlayerInsurancePrice { get; set; }

        public List<MemberFeeModel> MemberFees { get; set; }
        public List<HandlingFeeModel> HandlingFees { get; set; }

        public ActivityFormsSubmittedData Registration { get; set; }
        public int? UnionId { get; set; }
        public double? FiveHandicapReduction { get; set; }
        public int? Type { get; set; }
        public int? SeasonId { get; set; }
        public bool IsTennisLeague { get; set; }
        public bool NoTeamRegistration { get; set; }

        public IEnumerable<SelectListItem> BicycleDisciplines { get; set; }
        public IEnumerable<SelectListItem> Levels { get; set; }
        public int? BicycleCompetitionDisciplineId { get; set; }
        public int? LevelId { get; set; }

        [Url]
        public string RegistrationLink { get; set; }

        #region Official settings
        public decimal? RefereeFeePerGame { get; set; }
        public CurrencyUnits RefereePaymentCurrencyUnits { get; set; }
        public decimal? RefereePaymentForTravel { get; set; }
        public MetricUnits RefereeTravelMetricUnits { get; set; }
        public CurrencyUnits RefereeTravelCurrencyUnits { get; set; }

        public decimal? SpectatorFeePerGame { get; set; }
        public CurrencyUnits SpectatorPaymentCurrencyUnits { get; set; }
        public decimal? SpectatorPaymentForTravel { get; set; }
        public MetricUnits SpectatorTravelMetricUnits { get; set; }
        public CurrencyUnits SpectatorTravelCurrencyUnits { get; set; }

        public decimal? DeskFeePerGame { get; set; }
        public CurrencyUnits DeskPaymentCurrencyUnits { get; set; }
        public decimal? DeskPaymentForTravel { get; set; }
        public MetricUnits DeskTravelMetricUnits { get; set; }
        public CurrencyUnits DeskTravelCurrencyUnits { get; set; }
        public bool IsOfficialsFeatureEnabled { get; internal set; }
        public string Section { get; set; }

        public decimal? RateAPerGame { get; set; }
        public decimal? RateBPerGame { get; set; }
        public decimal? RateCPerGame { get; set; }
        public decimal? RateAForTravel { get; set; }
        public decimal? RateBForTravel { get; set; }
        public decimal? RateCForTravel { get; set; }
        public IEnumerable<Club> AllowedClubs { get; set; }
        public IEnumerable<int> AllowedClubsIds { get; set; }
        public IEnumerable<int> SelectedClubsIds { get; set; }
        public bool IsAthleticsCompetition { get; set; }
        public LevelDatesSettingsForm LevelDatesSettings { get; set; }
        public DateTime? EndTeamRegistrationDate { get; set; }
        public bool IsDailyCompetition { get; set; }
        public bool IsSeniorCompetition { get; set; }

        //rowing additional fields
        public DateTime? StartTeamRegistrationDate { get; set; }
        public int? MaxParticipationAllowedForSportsman { get; set; }
        public bool CheckClubCompetition { get { return IsClubCompetition == 1; } set { IsClubCompetition = 1; } }
        public int IsClubCompetition { get; set; }


        #endregion

        public decimal? GetPrice(LeaguePriceType pricetype)
        {
            var today = DateTime.Today;

            switch (pricetype)
            {
                case LeaguePriceType.PlayerInsurancePrice:

                    var period1 = this.LeaguesPlayerInsurancePrice.Where(p => p.StartDate.HasValue && p.EndDate.HasValue && (p.StartDate.Value <= today && today <= p.EndDate.Value)).FirstOrDefault();

                    if (period1 != null && period1.Price.HasValue)
                    {
                        return period1.Price;
                    }

                    break;

                case LeaguePriceType.PlayerRegistrationPrice:

                    var period2 = this.LeaguesPlayerRegistrationPrice.Where(p => p.StartDate.HasValue && p.EndDate.HasValue && (p.StartDate.Value <= today && today <= p.EndDate.Value)).FirstOrDefault();

                    if (period2 != null && period2.Price.HasValue)
                    {
                        return period2.Price;
                    }

                    break;

                case LeaguePriceType.TeamRegistrationPrice:

                    var period3 = this.LeaguesTeamRegistrationPrice.Where(p => p.StartDate.HasValue && p.EndDate.HasValue && (p.StartDate.Value <= today && today <= p.EndDate.Value)).FirstOrDefault();

                    if (period3 != null && period3.Price.HasValue)
                    {
                        return period3.Price;
                    }

                    break;

            }

            return null;
        }
    }

    public class LeaguesPricesForm
    {
        public decimal? Price { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CardComProductId { get; set; }
    }

    public class LevelDatesSettingsForm
    {
        public int CompetitionId { get; set; }
        public IEnumerable<LevelDateSettingDto> LevelDatesSettings { get; set; }
        public IEnumerable<CompetitionLevel> LevelList { get; set; }
    }

    public enum LeaguePriceType
    {
        /// <summary>
        /// Team registration price
        /// </summary>
        TeamRegistrationPrice = 1,
        /// <summary>
        /// player registration price
        /// </summary>
        PlayerRegistrationPrice = 2,
        /// <summary>
        /// player insurance price
        /// </summary>
        PlayerInsurancePrice = 3
    }

    public enum MetricUnits
    {
        Kilometer = 0,
        Mile = 1
    }

    public enum CurrencyUnits
    {
        Nis = 0,
        Dollar = 1,
        Euro = 2
    }

    public class LeagueCreateForm
    {
        public TournamentsPDF.EditType Et { get; set; }
        public int? UnionId { get; set; }
        public int? DisciplineId { get; set; }
        public int? ClubId { get; set; }

        public int SeasonId { get; set; }

        public bool IsTeamCheck { get; set; }

        [Required]
        public string Name { get; set; }
        public int? AgeId { get; set; }
        public int? GenderId { get; set; }
        public bool IsHandicapEnabled { get; set; }

        public decimal MaximumHandicapScoreValue { get; set; }
        public IEnumerable<SelectListItem> Ages { get; set; }
        public IEnumerable<SelectListItem> Genders { get; set; }
        public string Section { get; set; }
        public int IsTennisCompetition { get; set; }
        public List<League> LeaguesForDuplicate { get; set; }
        public int? DuplicateLeagueId { get; set; }
        public IEnumerable<SelectListItem> BicycleDisciplines { get; set; }
        public IEnumerable<SelectListItem> Levels { get; set; }
        public int? BicycleCompetitionDisciplineId { get; set; }
        public int? LevelId { get; set; }

        public IEnumerable<SelectListItem> Disciplines { get; set; }
    }
}