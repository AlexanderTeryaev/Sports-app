using System.Collections.Generic;

namespace DataService.DTO
{
    public class CompetitionRegistrationForm
    {
        public int CompetitionRouteId { get; set; }
        public IEnumerable<int> PlayersIds { get; set; }
        public int? AdditionalGymnasticId { get; set; }
        public int? Composition { get; set; }
        public int CompositionNumber { get; set; }
        public int? InstrumentId { get; set; }
        public bool IsTeam { get; set; }
    }
}
