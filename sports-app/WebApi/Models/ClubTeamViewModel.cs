namespace WebApi.Models
{
    public class ClubTeamViewModel
    {
        public int TeamId { get; set; }
        public string Title { get; set; }
        public string Logo { get; set; }
        public int? SeasonId { get; set; }
        public int ParentId { get; set; }
        public int LeagueId { get; set; }
        public int PlayerNumber { get; set; }
        public string ParentName { get; set; }
        public int FanNumber { get; set; }
        public bool IsSchoolTeam { get; set; }
    }
}