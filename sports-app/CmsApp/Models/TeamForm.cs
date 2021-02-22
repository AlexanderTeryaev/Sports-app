using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AppModel;
using DataService.DTO;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class TeamForm
    {
        public TeamForm()
        {
            TeamsList = new List<Team>();
            StatisticsDictionary = new Dictionary<int, PlayerCountStats>();
        }

        public int LeagueId { get; set; }
        public int SeasonId { get; set; }
        public int? UnionId { get; set; }
        public int? SectionId { get; set; }
        public string Section { get; set; }
        public int? DepartmentId { get; set; }

        [Required]
        public string Title { get; set; }

        public IEnumerable<Team> TeamsList { get; set; }
        public int TeamId { get; set; }
        public bool IsNew { get; set; }
        public string Address { get; set; }

        public int IsTennisCompetition { get; set; }
        public bool IsIndividual { get; set; }
        public Dictionary<int, PlayerCountStats> StatisticsDictionary { get; set; }
        public bool BlockApprovement { get; set; }
    }

    public class PlayerCountStats
    {
        public int Waiting { get; set; }
        public int Approved { get; set; }
        public int NotActive { get; set; }
        public int Registered { get; set; }
    }

    public class TeamInfoForm
    {
        public int TeamId { get; set; }
        public IList<LeagueShort> leagues { get; set; }
        public IList<ClubShort> clubs { get; set; }
        public int? LeagueId { get { return leagues.Count == 1 ? (int?)leagues[0].Id : null; } }
        [Required]
        public string Title { get; set; }
        public string Logo { get; set; }
        public string PersonnelPic { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public int SeasonId { get; set; }
        public bool? IsReserved { get; set; }
        public bool? IsUnderAdult { get; set; }

        public bool? NeedShirts { get; set; }
        public string InsuranceApproval { get; set; }

        public TeamRegistrationModel Registration { get; set; }
        public List<TeamRegistrationModel> CustomRegistrations { get; set; }
        public IEnumerable<SelectListItem> ListAges { get; set; }
        public IEnumerable<SelectListItem> ListRegions { get; set; }
        public IEnumerable<SelectListItem> ListLevels { get; set; }
        public IEnumerable<SelectListItem> ListGenders { get; set; }
        public List<CompetitionAge> CompetitionAges { get; set; }
        public List<CompetitionRegion> CompetitionRegions { get; set; }
        public List<CompetitionLevel> CompetitionLevels { get; set; }

        public int? CompetitionAgeId { get; set; }
        public int? CompetitionRegionId { get; set; }
        public int? CompetitionLevelId { get; set; }
        public int GenderId { get; set; }
        public string GenderName { get; set; }

        public bool IsTrainingEnabled { get; set; }

        public int? ClubId { get; set; }

        public int? MinRank { get; set; }
        public int? MaxRank { get; set; }
        public string PlaceForQualification { get; set; }
        public string PlaceForFinal { get; set; }

        public List<TeamPrice> PlayerRegistrationAndEquipmentPrices { get; set; }
        public List<TeamPrice> ParticipationPrices { get; set; }
        public List<TeamPrice> PlayerInsurancePrices { get; set; }

        public DateTime? MinimumAge { get; set; }
        public DateTime? MaximumAge { get; set; }
        public bool IsGymnastic { get; internal set; }

        public IEnumerable<Discipline> UnionDisciplines { get; set; }
        public IEnumerable<Discipline> TeamDisciplines { get; set; }
        public IEnumerable<int> SelectedDisciplines { get; set; }
        public CultEnum Culture { get; set; }

        public int sectionId { get; set; }
        public int IsTennisCompetition { get; set; }
        public string SectionAlias { get; set; }
        public bool IsLeagueRegistration { get; set; }
        public bool IsReligiousTeam { get; set; }
        public bool IsTrainingTeam { get; set; }
        public bool IsSchoolTeam { get; set; }
        public DateTime? QualificationStartDate { get; set; }
        public DateTime? QualificationEndDate { get; set; }
        public DateTime? FinalStartDate { get; set; }
        public DateTime? FinalEndDate { get; set; }
        public bool IsUnionConnected { get; set; }
        public string TeamForeignName { get; set; }
        public int? MonthlyProvisionForSecurity { get; set; }
    }

    public class TeamPrice
    {
        public decimal? Price { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CardComProductId { get; set; }
    }

    public class BasicRouteForm
    {
        public int DiciplineId { get; set; }
        public int? SeasonId { get; set; }
        public int LeagueId { get; set; }
        public int RouteId { get; set; }
        public int RankId { get; set; }
        public string RouteName { get; set; }
        public string RankName { get; set; }
        public int GymnasticsCount { get; set; }
        public int CompetitionRouteId { get; internal set; }
        public string DisciplineName { get; set; }
        public int? Composition { get; set; }
        public IEnumerable<int> InstrumentsIds { get; set; }
        public string InstrumentName { get; set; }
        public int? SecondComposition { get; set; }
        public int? ThirdComposition { get; set; }
        public int? FourthComposition { get; set; }
        public int? FifthComposition { get; set; }
        public int? SixthComposition { get; set; }
        public int? SeventhComposition { get; set; }
        public int? EighthComposition { get; set; }
        public int? NinthComposition { get; set; }
        public int? TenthComposition { get; set; }
        public bool IsCompetitiveEnabled { get; set; }
        public int? MaxRegistrationsAllowed { get; set; }
        public int? CompetitionRouteToEdit { get; set; }

}

    public class RouteForm : BasicRouteForm
    {
        public IEnumerable<SelectListItem> Routes { get; set; }
        public IDictionary<int, IEnumerable<KeyValuePair<int, string>>> Ranks { get; set; }
        public IEnumerable<SelectListItem> Instruments { get; set; }
    }

    public enum ClubTeamPriceType
    {
        /// <summary>
        /// Player registration & equipment price
        /// </summary>
        PlayerRegistrationAndEquipmentPrice = 1,
        /// <summary>
        /// Participation price
        /// </summary>
        ParticipationPrice = 2,
        /// <summary>
        /// player insurance price
        /// </summary>
        PlayerInsurancePrice = 3
    }
}