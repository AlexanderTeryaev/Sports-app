namespace DataService.DTO
{
    
    public class TennisPositionSettingsDto
    {
        public int Id { get; set; }
        public int Rank { get; set; }
        public int Points { get; set; }
        public int LevelId { get; set; }
        public int SeasonId { get; set; }
        public int? PointsForPairs { get; set; }
    }
}
