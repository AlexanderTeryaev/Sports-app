using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class RegisterPlayerToCompDTO
    {
        public int UserId { get; set; }
        public int DisciplineId { get; set; }
        public int SeasonId { get; set; }
        public string SectionAlias { get; set; }
        public decimal? WeightDeclaration { get; set; }
    }
}