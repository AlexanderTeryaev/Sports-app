using System;

namespace DataService.DTO
{
    public class SchedulerDTO
    {
        public string Auditorium { get; set; }
        public string HomeTeam { get; set; }
        public string GuestTeam { get; set; }
        public string Time { get; set; }

        public string HomeTeamScore { get; set; }
        public string GuestTeamScore { get; set; }

        public string Url { get; set; }
    }
}
