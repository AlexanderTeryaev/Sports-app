using DataService.DTO;
using System.Collections.Generic;

namespace CmsApp.Models
{
    public class InstrumentForm
    {
        public int DisciplineId { get; set; }
        public int SeasonId { get; set; }
        public string InstrumentName { get; set; }
        public IEnumerable<InstrumentDto> InstrumentsList { get; set; }
    }
}