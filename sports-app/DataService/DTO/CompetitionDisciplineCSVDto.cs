using AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.DTO
{
    public class CompetitionDisciplineCSVDto
    {
        public List<CompetitionDisciplineRegistration> Registrations { get; set; }
        public string DisciplineName { get; set; }
        public string HeatName { get; set; }
        public string DisciplineLength { get; set; }
        public string DisciplineDate { get; set; }
        public string DisciplineTime { get; set; }
        public long StartDateTicks { get; set; }
        public string CompetitionDesciplineId { get; set; }
        public string CategoryName { get; set; }
    }
}
