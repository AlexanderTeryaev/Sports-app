using DataService.DTO;
using System.Collections.Generic;

namespace CmsApp.Models
{
    public class TeamNavView
    {
        public int TeamId { get; set; }
        public int SeasonId { get; set; }
        public string TeamName { get; set; }
        public string Address { get; set; }
        public IList<ClubShort> clubs { get; set; }
        public IList<LeagueShort> TeamLeagues { get; set; }
        public IList<LeagueShort> UserLeagues { get; set; }
        public bool IsValidUser { get; set; }
        public bool IsUnionClubManagerUnderPastSeason { get; set; }
        private int? _currentLeague;
        public int? CurrentLeagueId
        {
            get { return TeamLeagues.Count == 1 ? (int?)TeamLeagues[0].Id : _currentLeague; }
            set { _currentLeague = value; }
        }

        public int? ClubId { get { return clubs.Count == 1 ? (int?)clubs[0].Id : null; } } 
        public int? CurrentClubId { get; set; }
        public int SectionId { get; set; }
        public int? UnionId { get; set; }
        public string JobRole { get; set; }
        public TeamInfoForm Details { get; set; }

        public bool IsTrainingEnabled { get; set; }

        public BenefactorViewModel Benefactor { get; set; }

        public TrainingSettingForm trainingSettingform { get; set; }
        public bool IsDepartmentTeam { get; set; }
        public bool IsGymnastic { get; set; }
        public string CurrentClubName { get; internal set; }
        public int? DepartmentId { get; internal set; }
        public bool IsBasketball { get; internal set; }
        public bool IsIndividual { get; internal set; }
		public string Section { get; set; }
        public bool IsEilatTournament { get; set; }
        public int IsTennisCompetition { get; set; }
        public bool IsBlockedForRegistrationByTennisCompetition { get; set; }
        public bool IsTrainingTeam { get; set; }
        public bool IsRugby { get; set; }
        public bool IsCatchball { get; set; }

        public bool IsWaterPolo { get; set; }
        public bool IsRankCalculated { get; internal set; }
    }
}