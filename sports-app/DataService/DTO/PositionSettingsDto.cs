namespace DataService.DTO
{
    
    public class PositionSettingsDto
    {
        public int Id { get; set; }
        public int Position { get; set; }
        public int Points { get; set; }
    }

    public class PositionSettingFormDto : PositionSettingsDto
    {
        public int LeagueId { get; set; }
        public int SeasonId { get; set; }
    }
}
