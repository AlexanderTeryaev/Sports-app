using System;
using System.Collections.Generic;

namespace CmsApp.Models
{
    public class ClubsViewModel
    {
        public int ClubId { get; set; }
        public int SeasonId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IEnumerable<ClubTrainingsViewModel> ClubTrainings { get; set; }
        public IEnumerable<ClubTrainingDaysViewModel> ClubTrainingDays { get; set; }

        //ClubTrainingsEntities
        public string TrainingDay { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}