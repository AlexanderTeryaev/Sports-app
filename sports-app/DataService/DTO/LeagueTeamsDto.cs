using System.Collections.Generic;

namespace DataService.DTO
{
    public class LeagueTeamsDto
    {
        public int LeagueId { get; set; }

        public string Name { get; set; }

        public IEnumerable<TeamInformationDto> Teams { get; set; }
    }
}
