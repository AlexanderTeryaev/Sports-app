namespace DataService.DTO
{
    public class PlayerTeamDto
    {
        public TeamInformationDto TeamDetails { get; set; }
        public int LeagueId { get; set; }
        public string LeagueName { get; set; }
        public string Position { get; set; }
        public int ShirtNum { get; set; }
    }
}
