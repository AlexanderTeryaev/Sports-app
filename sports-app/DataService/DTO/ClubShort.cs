namespace DataService.DTO
{
    public class ClubShort
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SeasonId { get; internal set; }
    }
}
