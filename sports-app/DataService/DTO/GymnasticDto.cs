using System;
using System.Collections.Generic;

namespace DataService.DTO
{
    public class PlayerOrder
    {
        public int RegistrationId { get; set; }
        public int? PositionOrder { get; set; }
    }
    public class GymnasticExportDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ClubName { get; set; }
        public string Route { get; set; }
        public string Rank { get; set; }
        public string IdentNum { get; set; }
    }
    public class GymnasticShortDto : PlayerOrder
    {

    }
    public class GymnasticDto : GymnasticShortDto
    {
        public string FullName { get; set; }
        public string IdentNum { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Route { get; set; }
        public string Rank { get; set; }
        public string ClubName { get; set; }
        public int? Composition { get; set; }
        public int? CompositionAmount { get; set; }
        public int? ClubNumber { get; set; }
        public double? FinalScore { get; set; }
        public int? Position { get; set; }
        public int? SecondComposition { get; set; }
        public int? ThirdComposition { get; set; }
        public int? FourthComposition { get; set; }
        public int? FifthComposition { get; set; }
        public int? SixthComposition { get; set; }
        public int? SeventhComposition { get; set; }
        public int? EighthComposition { get; set; }
        public int? NinthComposition { get; set; }
        public int? TenthComposition { get; set; }

        public int? CompetitionRouteId { get; set; }
        public int CompositionNumber { get; set; }
        public bool IsAdditional { get; set; }
        public string InstrumentName { get; set; }
        public int? InstrumentId { get; set; }
        public bool IsTeam { get; set; }
        public List<RegistrationInstrument> RegistrationInstruments { get; set; }
        public string PassportNum { get; set; }
        public string Created { get; set; }
        public string Creator { get; set; }
        public bool? IsReligious { get; set; }
    }
}
