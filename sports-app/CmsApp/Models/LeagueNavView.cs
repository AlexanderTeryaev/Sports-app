namespace CmsApp.Models
{
    public class LeagueNavView
    {
        public int LeagueId { get; set; }
        public string LeagueName { get; set; }
        public int? UnionId { get; set; }
        public int? ClubId { get; set; }
        public bool IsDepartmentLeague { get; set; }
        public int? SectionId { get; set; }
        public int? DisciplineId { get; set; }
        public string DisciplineAlias { get; set; }
        public string SectionAlias { get; set; }
        public int SeasonId { get; set; }
        public string UnionName { get; set; }
        public string ClubName { get; set; }
        public bool IsUnionValid { get; set; }
        public bool IsIndividual { get; set; }
        public bool IsPositionSettingsEnabled { get; set; }
        public bool IsTeam { get; set; }
        public int IsTennisCompetition { get; set; }
        public bool IsAthleticsCompetition => SectionAlias == GamesAlias.Athletics;
        public bool IsWeightLiftingCompetition => SectionAlias == GamesAlias.WeightLifting;
        public bool IsGymnasticsCompetition => SectionAlias == GamesAlias.Gymnastic;
        public bool IsRowingCompetition => SectionAlias == GamesAlias.Rowing;
        public int? AthleticLeagueId { get; internal set; }
    }
}