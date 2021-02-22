namespace DataService.DTO
{
    public class WorkerMainShortDto
    {
        public int RegistrationId { get; set; }
        public int UserJobId { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public bool IsApproved { get; set; }
        public int SessionId { get; set; }
        public int RefereeId { get; set; }
    }
    public class WorkerShortDto : WorkerMainShortDto
    {
        public int UserId { get; set; }
        public int JobId { get; set; }
        public string JobName { get; set; }
        public string OfficialType { get; set; }
    }

    public class RefereeShortDto : WorkerMainShortDto
    {
        public int RegId { get; set; }
        public int UserId { get; set; }
        public int SeasonId { get; set; }
        public int LeagueId { get; set; }
        public int UnionId { get; set; }
        public string SessionIds { get; set; }
        public int isAdd { get; set; }
    }
}
