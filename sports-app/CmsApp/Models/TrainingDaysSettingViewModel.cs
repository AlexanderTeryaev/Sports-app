using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class TrainingDaysSettingViewModel
    {
        public int TeamId { get; set; }
        public int AuditoriumId { get; set; }
        public DayOfWeek TrainingDay { get; set; }
        public string TrainingStartTime { get; set; }
        public string TrainingEndTime { get; set; }
    }
}