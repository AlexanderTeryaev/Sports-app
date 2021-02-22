namespace DataService.DTO
{
    public class TennisRankForm
    {
        public int? Id { get; set; }
        public int? Rank { get; set; }
        public int? Points { get; set; }
        public int? AgeId { get; set; }
        public string AgeName { get; internal set; }
    }
}
