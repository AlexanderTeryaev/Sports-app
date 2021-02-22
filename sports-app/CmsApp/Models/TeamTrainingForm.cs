using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class TeamTrainingForm
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int TeamId { get; set; }
        public int AuditoriumId { get; set; }
        
        public DateTime TrainingDate { get; set; }
        public string Content { get; set; }
        public List<SelectListItem> AuditoriumBind { get; set; }
        public List<TeamTrainingForm> TeamTrainingList { get; set; }

        public DateTime TrainingStartDate { get; set; }
        public DateTime TrainingEndDate { get; set; }
        
    }
}