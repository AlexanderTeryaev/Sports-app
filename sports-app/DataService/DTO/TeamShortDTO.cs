namespace DataService.DTO
{
    public class TeamShortDTO
    {
        public int Pos { get; set; }
        public int? TeamId { get; set; }
        public string Title { get; set; }
    }

    public class AthleteShortDTO
    {
        public int Pos { get; set; }
        public int? AthleteId { get; set; }
        public string Title { get; set; }
    }
}
