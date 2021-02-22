namespace DataService.DTO
{
    public class ExcelPlayerDto
    {
        public int UserId { get; set; }
        public int TeamId { get; set; }
        public int? ClubId { get; set; }
        public int SeasonId { get; set; }
        public decimal ParticipationDiscount { get; set; }
        public decimal Paid { get; set; }
        public string Comments { get; set; }
    }
}
