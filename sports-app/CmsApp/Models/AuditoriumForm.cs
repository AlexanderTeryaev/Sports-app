using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AppModel;
using DataService.DTO;

namespace CmsApp.Models
{
    public class AuditoriumForm
    {
        public int AuditoriumId { get; set; }

        public int? UnionId { get; set; }
        public int? SeasonId { get; set; }
        public int? ClubId { get; set; }
        public int? Length { get; set; }
        public int? LanesNumber { get; set; }
        public IndoorOutdoor IndoorOutdoor { get; set; }
        public IsHeated IsHeated { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; }

        [Required, StringLength(250)]
        public string Address { get; set; }

        public int? DisciplineId { get; set; }

        public IEnumerable<Auditorium> Auditoriums { get; set; }
        public string SectionAlias { get; set; }
        public IEnumerable<DisciplineDTO> Disciplines { get; set; }
    }

    public enum IndoorOutdoor
    {
        Outdoor,
        Indoor
    }

    public enum IsHeated
    {
        No,
        Yes
    }
}