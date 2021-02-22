using System;

namespace CmsApp.Models
{
    public class ClubTrainingViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int TeamId { get; set; }
        public int? AuditoriumId { get; set; }
        public DateTime TrainingDate { get; set; }
        public string Content { get; set; }
        public bool IsPublished { get; set; }
    }
}