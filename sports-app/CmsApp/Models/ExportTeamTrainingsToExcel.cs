using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class ExportTeamTrainingsToExcel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int TeamId { get; set; }
        public int? AuditoriumId { get; set; }
        public DateTime TrainingDate { get; set; }
        public string Content { get; set; }
        public string PlayersIds { get; set; }
    }
}