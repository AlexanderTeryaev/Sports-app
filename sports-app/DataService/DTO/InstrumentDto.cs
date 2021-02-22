namespace DataService.DTO
{
    public class InstrumentShortDto
    {
        public int? Id { get; set; }
        public int InstrumentId { get; set; }
        public string Name { get; set; }
    }

    public class InstrumentDto : InstrumentShortDto
    {
       
        public int SeasonId { get; set; }
        public int DisciplineId { get; set; }
    }

    public class RegistrationInstrument : InstrumentShortDto
    {
        public int? OrderNumber { get; set; }
    }

    public class GymnasticInstrumentImport
    {
        public string InstrumentName { get; set; }
        public int? OrderNumber { get; set; }
    }
}
