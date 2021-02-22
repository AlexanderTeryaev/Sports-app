using System.Collections.Generic;

namespace CmsApp.Models
{
    public class PlayerCountsDetail
    {
        public string TeamName { get; set; }
        public int TotalCount { get; set; }
        public int WaitingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int NotApprovedCount { get; set; }
        public int ActiveCount { get; set; }
    }
    public class ClubMainTabForm
    {
        public int ClubId { get; set; }
        public int? SeasonId { get; set; }
        public int SectionId { get; set; }
        public int PlayersCount { get; set; }
        public int SchoolPlayersCount { get; set; }
        public int UniquePlayersCount { get; set; }
        public int WaitingForApproval { get; set; }
        public int PlayersCompletedRegistrations { get; set; }
        public int PlayersCompletedRegistrationsUnique { get; set; }
        public int PlayersApproved { get; set; }
        public int PlayersApprovedUnique { get; set; }
        public int PlayersNotApproved { get; set; }
        public int PlayersNotApprovedUnique { get; set; }
        public int OfficialsCount { get; set; }
        public int TeamsCount { get; set; }
        public int TeamsCompletedRegistrations { get; set; }
        public int TeamsApproved { get; set; }
        public bool SectionIsIndividual { get; set; }
        public bool IsGymnastics { get; set; }
        public int UniqueSchoolPlayersCount { get; set; }
        public int SchoolPlayersCompletedRegistrations { get; set; }
        public int SchoolPlayersCompletedRegistrationsUnique { get; set; }
        public int SchoolPlayersNotApprovedUnique { get; set; }
        public int SchoolWaitingForApproval { get; set; }
        public int SchoolPlayersNotApproved { get; set; }
        public int SchoolPlayersApprovedUnique { get; set; }
        public int SchoolPlayersApproved { get; set; }
        public bool IsIndividual { get; set; }
        public int ActivePlayers { get; set; }
        public int ActiveSchoolPlayers { get; set; }
        public int ActiveUniquePlayers { get; set; }
        public int ActiveUniqueSchoolPlayers { get; set; }
        public string SectionAlias { get; set; }
        public int TotalWaitingAndApprovedCount { get; set; }
        public string Comment { get; set; }
        public bool IsClubApproved { get; set; }
        public bool HasPermission { get; set; }
        public int TotalTeams { get; set; }
        public bool IsAthletics { get; set; }
        public int? UnionId { get; set; }
        public bool IsUnionClub { get; set; }
        public bool IsRowing { get; set; }
        public bool IsBicycle { get; set; }
        public List<PlayerCountsDetail> CountDetail { get; set; }
        public int Itennis { get; set; }
    }
}