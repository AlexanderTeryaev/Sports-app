namespace DataService.DTO
{
    public class LeagueShort
    {
        public int Id { get; set; }
        public int? UnionId { get; set; }
        public string Name { get; set; }
        public bool Check { get; set; }
        public int DisciplineId { get; set; }
        public string DisciplineName { get; set; }
        public bool IsEilatTournament { get; set; }
    }
}
