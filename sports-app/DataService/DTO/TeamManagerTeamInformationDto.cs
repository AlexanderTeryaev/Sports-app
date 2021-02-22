using System.Collections.Generic;
using AppModel;

namespace DataService.DTO
{
    public class TeamManagerTeamInformationDto
    {
        public int? LeagueId { get; set; }
        public string LeagueName { get; set; }
        public int? ClubId { get; set; }
        public int? SeasonId { get; set; }
        public int? TeamId { get; set; }
        public int? UnionId { get; set; }
        public string Title { get; set; }
    }
}
