using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.DTO
{
    public class ChangeSwimmerDto
    {
        public int CompetitionDisciplineId { get; set; }
        public int OriginalSwimmerId { get; set; }
        public List<CompDiscRegDTO> Swimmers { get; set; }
    }
}
