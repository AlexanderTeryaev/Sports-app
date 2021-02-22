using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using AppModel;

namespace CmsApp.Models
{
    public class PositionsForm
    {
        public int PosId { get; set; }
        public int SectionId { get; set; }
        [Required]
        public string Title { get; set; }
        public IEnumerable<Position> Positions { get; set; }
    }
}