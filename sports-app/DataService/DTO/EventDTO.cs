using System;

namespace DataService.DTO
{
    public class TrainingDTO
    {
        public int? ClubId { get; set; }
        public int? SportId { get; set; }
        public string SportName { get; set; }
        public int? TeamId { get; set; }
        public string TeamName { get; set; }
        public DateTime StartDate { get; set; }
        public int? AuditoriumId { get; set; }
        public string AuditoriumName { get; set; }
        public string ClubName { get; set; }
    }

    public class GameDTO
    {
        public int? ClubId { get; set; }
        public int? SportId { get; set; }
        public string SportName { get; set; }
        public int? HomeTeamId { get; set; }
        public string HomeTeamName { get; set; }
        public int? GuestTeamId { get; set; }
        public string GuestTeamName { get; set; }
        public DateTime StartDate { get; set; }
        public int? AuditoriumId { get; set; }
        public string AuditoriumName { get; set; }
        public string GroupName { get; set; }
        public string ClubName { get; set; }
    }

    public class EventDTO
    {
        public int? ClubId { get; set; }
        public int? SportId { get; set; }
        public string SportName { get; set; }
        public string Title { get; set; }
        public string Place { get; set; }
        public DateTime EventTime { get; set; }
        public string ClubName { get; set; }
        public string EventDescription { get; set; }
        public string EventImage { get; set; }
        public int? UnionId { get; set; }
    }
}
