using DataService.DTO;
using System.Collections.Generic;
using DataService;

namespace CmsApp.Models
{
    public class UnionCompetitionForm
    {
        public UnionCompetitionForm()
        {
            Competitions = new List<CompetitionDto>();
            ClubPlayersIds = new List<int>();
        }

        public int ClubId { get; set; }
        public int UnionId { get; set; }
        public int SeasonId { get; set; }
        public List<int> ClubPlayersIds { get; set; }
        public List<CompetitionDto> Competitions { get; set; }

        public List<WorkerMainShortDto> ClubReferees { get; set; }

        public Dictionary<int, List<int>> SelectedRefereesIds { get; set; }
        public List<InstrumentDto> Instruments { get; set; }
        public IEnumerable<PlayerViewModel> ClubSportsmen { get; set; }
        public Dictionary<int, IEnumerable<int>> SelectedSportsmenIds { get; set; }

        public string SectionAlias { get; internal set; }

        public bool IsMartialArts => SectionAlias == GamesAlias.MartialArts;
        public bool IsGymnastic => SectionAlias == GamesAlias.Gymnastic;
        public bool IsAthletics => SectionAlias == GamesAlias.Athletics;
        public bool IsWeightLifting => SectionAlias == GamesAlias.WeightLifting;
        public bool IsBicycle => SectionAlias == GamesAlias.Bicycle;
        public bool IsClimbing => SectionAlias == GamesAlias.Climbing;
    }
}