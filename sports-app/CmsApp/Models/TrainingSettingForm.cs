using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class TrainingSettingForm
    {
        public int Id { get; set; }
        public string ChooseAuditorium { get; set; }
        public string DurationTraining { get; set; }
        public bool ConsiderationHolidays { get; set; }
        public bool TrainingSameDay { get; set; }
        public bool TrainingBeforeGame { get; set; }
        public string MinNumTrainingDays { get; set; }
        public bool NoTwoTraining { get; set; }
        public bool DontSetDayAfterDay { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string TrainingDays { get; set; }
        public bool TrainingFollowDay { get; set; }
        public List<SelectListItem> auditoriumData { get; set; }
        public int TeamID { get; set; }
        public int? clubId { get; set; }
        public int seasonId { get; set; }
        //Extra parameter
        public string Name { get; set; }
        public List<TrainingSettingForm> listdata { get; set; }
        //Generator
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string tableHTML { get; set; }
    }
}
