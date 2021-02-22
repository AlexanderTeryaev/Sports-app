using System.Collections.Generic;

namespace DataService.DTO
{
    public class CompetitionDisciplineDto
    {
        public int Id { get; set; }
        public int CompetitionId { get; set; }
        public int CategoryId { get; set; }
        public int? DisciplineId { get; set; }
        public int? MaxSportsmen { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsMultiBattle { get; set; }
        public bool IsForScore { get; set; }
        public bool IsResultsManualyRanked { get; set; }
        public System.DateTime? StartTime { get; set; }
        public double? MinResult { get; set; }
        public int RegistrationsCount { get; set; }
        public int PlayersCount { get; set; }
        public string DisciplineName { get; set; }
        public string CategoryName { get; set; }
        public string SectionAlias { get; set; }
        public IEnumerable<PlayerShortDTO> DisciplinePlayers { get; set; }
        public IEnumerable<CompDiscRegDTO> RegisteredPlayers { get; set; }
        public IEnumerable<CompDiscRegRankDTO> ClubsPointed { get; set; }
        public IEnumerable<CompDiscTeamDTO> DisciplineTeams { get; set; }
        public string DistanceName { get; set; }
        public int? Format { get; set; }
        public int? TeamRegistration { get; set; }
        public bool Coxwain { get; set; }
        public List<HeatDto> Heats { get; set; }
        public bool HeatsGenerated { get; set; }
        public bool IsMixed { get; set; }
        public int NumberofSportsmen { get; set; }
		public string Result { get; set; }
		public int Rank { get; set; }
	}
}
