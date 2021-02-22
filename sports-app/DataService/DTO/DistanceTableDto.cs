namespace DataService.DTO
{
    public class DistanceTableDto
    {
        public int Id { get; set; }
        public int? UnionId { get; set; }
        public int? ClubId { get; set; }
        public string CityFromName { get; set; }
        public string CityToName { get; set; }
        public int Distance { get; set; }
        public string DistanceType { get; set; }
    }
}
