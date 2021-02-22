
using DataService.DTO;
using System.Collections.Generic;

namespace CmsApp.Models
{
    public class TeamStandingsGameForm
    {
        public TeamStandingsGameForm()
        {
            TeamStandings = new List<TeamStandingsForm>();
        }
        public List<TeamStandingsForm> TeamStandings { get; set; }
        public int TeamStandingId { get; set; }
        public string GamesUrl { get; set; }
        public int TeamId { get; set; }
        public int ClubId { get; set; }
        public string TeamName { get; set; }
        public int? SeasonId { get; set; }

    }

    public class TeamStandingsModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int SeasonId { get; set; }
        public string SectionAlias { get; set; }
        public IList<LeagueShort> Leagues { get; set; }
        public IList<TeamStandingsGameForm> ClubStandings { get; set; }
    }
}