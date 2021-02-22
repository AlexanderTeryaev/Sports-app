using AppModel;

namespace CmsApp.Models
{
    public class TeamViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string LeagueNames { get; set; }
        public bool IsActive { get; set; }
        public int? DepartmentId { get; set; }
        public int? DepartmentSeasonId { get; set; }
        public string RegistredLeagueName { get; set; }
        public bool? IsUnionInsurance { get; set; }
        public bool? IsClubInsurance { get; set; }
        public int PlayersCount { get; set; }
        public bool IsTeamPossibleToDelete { get; internal set; }
    }
}