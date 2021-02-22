using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using AppModel;

namespace CmsApp.Models
{
    public class TeamsAuditoriumForm
    {
        public int TeamId { get; set; }
        [Required]
        public int AuditoriumId { get; set; }
        public int SeasonId { get; set; }
        public bool IsMain { get; set; }
        public IEnumerable<SelectListItem> Auditoriums { get; set; }
        public IEnumerable<TeamsAuditorium> TeamAuditoriums { get; set; }

        public bool? HasArena { get; set; }
    }
}