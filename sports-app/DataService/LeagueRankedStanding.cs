using System.Collections.Generic;

namespace DataService
{
    public class LeagueRankedStanding
    {
        public int LeagueId { get; set; }
        public List<LeagueTeamRankedStanding> Teams { get; set; }
    }

    public class LeagueTeamRankedStanding
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public int Correction { get; set; }
    }
    public class CompetitionClubRankedStanding
    {
        public int ClubId { get; set; }
        public string ClubName { get; set; }
        public decimal Points { get; set; }
        public decimal Correction { get; set; }
    }
}