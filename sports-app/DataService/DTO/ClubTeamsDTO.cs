namespace DataService.DTO
{
    public class ClubTeamsDTO
    {
        public int ClubId { get; set; }
        public int TeamId { get; set; }
        public int? SeasonId { get; set; }
        public bool IsBlocked { get; set; }
        public int TeamPosition { get; set; }
    }
}
