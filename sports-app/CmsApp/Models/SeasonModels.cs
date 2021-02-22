using System;
using System.ComponentModel.DataAnnotations;

namespace CmsApp.Models
{
    public class CreateSeason
    {
        public int EntityId { get; set; }

        public LogicaName RelevantEntityLogicalName { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime? StartDate { get; set; }

        [Required]
        public DateTime? EndDate { get; set; }

        public string Description { get; set; }

        public bool? IsDuplicate { get; set; }
        public int[] Leagues { get; set; }
        public int? SeasonForAge { get; set; }
}
}