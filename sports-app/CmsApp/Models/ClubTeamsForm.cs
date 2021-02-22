using System.Collections.Generic;
using AppModel;

namespace CmsApp.Models
{
    public class ClubTeamsForm
    {
        public int ClubId { get; set; }
        public int TeamId { get; set; }
        public int SeasonId { get; set; }
        public int CurrentSeasonId { get; set; }
        public string TeamName { get; set; }
        public string LeagueNames { get; set; }
        public bool IsNew { get; set; }
        public int SectionId { get; set; }
        public IEnumerable<TeamViewModel> Teams { get; set; }
        public bool IsGymnastic { get; set; }
        public bool IsAthletic { get; set; }
        public bool IsDepartment { get; set; }
        public string Section { get; internal set; }
        public int? SportId { get; set; }
        public int? TeamSeasonId { get; set; }
        public int? UnionId { get; set; }
        public bool ClubHasInsuranceFile { get; set; }
        public bool IsClubFeesPaid { get; set; }
        public bool IsClubManagerCanSeePayReport { get; set; }
        
    }

    //modal created for Rowing - do display table in form: Club X - N teams
    public class ClubTeamsNumber
    {
        public int ClubId { get; set; }
        public string ClubName { get; set; }
        public int TeamNumber { get; set; }
    }

    public class ClubTeamsNumberList
    {
        public string Title { get; set; }
        public List<ClubTeamsNumber> ClubTeamsNumbers { get; set; }

        public ClubTeamsNumberList()
        {
            ClubTeamsNumbers = new List<ClubTeamsNumber>();
        }
    }

    public class ClubTeamsInfoWithBoats : ClubTeamsNumber
    {
        public string Boat { get; set; }
        public string Distance { get; set; }
        public string Category { get; set; }
    }

    public class ClubTeamsInfoWithBoatsList
    {
        public int CompetitionId { get; set; }
        public string Title { get; set; }
        public List<ClubTeamsInfoWithBoats> ClubTeamsWithBoats { get; set; }

        public ClubTeamsInfoWithBoatsList()
        {
            ClubTeamsWithBoats = new List<ClubTeamsInfoWithBoats>();
        }
    }
}