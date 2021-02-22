namespace CmsApp.Models
{
    public class ClubTrainingDaysViewModel
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public string Auditorium { get; set; }
        public string TrainingDay { get; set; }
        public string TrainingStartTime { get; set; }
        public string TrainingEndTime { get; set; }
    }
}