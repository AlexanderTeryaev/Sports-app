using AppModel;
using System;
using System.Collections.Generic;

namespace CmsApp.Models
{
    public class TeamTrainingViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int? AuditoriumId { get; set; }
        public List<TeamsAuditorium> TeamArenas { get; set; }
        public DateTime TrainingDate { get; set; }
        public string Content { get; set; }
        public bool isPublished { get; set; }
        public IEnumerable<TeamsPlayer> Players { get; set; }
        public Dictionary<int, List<int>> PlayerAttendance { get; set; }
        public int ClubPosition { get; set; }
        public DayOfWeek TrainingDay { get; set; }
        public TimeSpan TrainingStartTime { get; set; }
        public TimeSpan TrainingEndTime { get; set; }
        public string TrainingReport { get; set; }
    }
}