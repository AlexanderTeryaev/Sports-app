//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AppModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class TeamsPlayer
    {
        public TeamsPlayer()
        {
            this.TrainingAttendances = new HashSet<TrainingAttendance>();
            this.GamesCycles = new HashSet<GamesCycle>();
            this.GamesCycles1 = new HashSet<GamesCycle>();
            this.GroupsTeams = new HashSet<GroupsTeam>();
            this.PlayoffBrackets = new HashSet<PlayoffBracket>();
            this.PlayoffBrackets1 = new HashSet<PlayoffBracket>();
            this.PlayoffBrackets2 = new HashSet<PlayoffBracket>();
            this.PlayoffBrackets3 = new HashSet<PlayoffBracket>();
            this.NationalTeamInvitements = new HashSet<NationalTeamInvitement>();
            this.Statistics = new HashSet<Statistic>();
            this.GameStatistics = new HashSet<GameStatistic>();
            this.DriverDetails = new HashSet<DriverDetail>();
            this.TennisGameCycles = new HashSet<TennisGameCycle>();
            this.TennisGameCycles1 = new HashSet<TennisGameCycle>();
            this.TennisGroupTeams = new HashSet<TennisGroupTeam>();
            this.TennisGroupTeams1 = new HashSet<TennisGroupTeam>();
            this.TennisGameCycles11 = new HashSet<TennisGameCycle>();
            this.TennisGameCycles3 = new HashSet<TennisGameCycle>();
            this.TennisCategoryPlayoffRanks = new HashSet<TennisCategoryPlayoffRank>();
            this.TennisCategoryPlayoffRanks1 = new HashSet<TennisCategoryPlayoffRank>();
            this.WaterpoloStatistics = new HashSet<WaterpoloStatistic>();
        }
    
        public int Id { get; set; }
        public int TeamId { get; set; }
        public int UserId { get; set; }
        public Nullable<int> PosId { get; set; }
        public int ShirtNum { get; set; }
        public bool IsActive { get; set; }
        public Nullable<int> SeasonId { get; set; }
        public decimal HandicapLevel { get; set; }
        public Nullable<bool> IsLocked { get; set; }
        public bool IsTrainerPlayer { get; set; }
        public Nullable<bool> IsApprovedByManager { get; set; }
        public Nullable<System.DateTime> StartPlaying { get; set; }
        public string ClubComment { get; set; }
        public string UnionComment { get; set; }
        public Nullable<int> LeagueId { get; set; }
        public Nullable<int> ClubId { get; set; }
        public Nullable<System.DateTime> ApprovalDate { get; set; }
        public Nullable<System.DateTime> MedExamDate { get; set; }
        public string Comment { get; set; }
        public decimal Paid { get; set; }
        public bool IsEscortPlayer { get; set; }
        public bool IsExceptionalMoved { get; set; }
        public bool WithoutLeagueRegistration { get; set; }
        public Nullable<System.DateTime> DateOfCreate { get; set; }
        public Nullable<int> TennisPositionOrder { get; set; }
        public Nullable<int> ActionUserId { get; set; }
        public bool NextTournamentRoster { get; set; }
        public Nullable<int> CompetitionParticipationCount { get; set; }
        public Nullable<int> PersonalCoachId { get; set; }
        public Nullable<int> MountainIronNumber { get; set; }
        public Nullable<int> RoadIronNumber { get; set; }
        public Nullable<int> VelodromeIronNumber { get; set; }
        public Nullable<int> KitStatus { get; set; }
        public Nullable<bool> IsApprovedByClubManager { get; set; }
        public Nullable<int> FriendshipTypeId { get; set; }
        public Nullable<int> FriendshipPriceType { get; set; }
        public Nullable<int> RoadDisciplineId { get; set; }
        public Nullable<int> MountaintDisciplineId { get; set; }
        public bool Masters { get; set; }
        public Nullable<int> MedicalInstituteId { get; set; }
        public bool IsNewPlayerInUnion { get; set; }
        public string TeamForUci { get; set; }
    
        public virtual Position Position { get; set; }
        public virtual Season Season { get; set; }
        public virtual Team Team { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<TrainingAttendance> TrainingAttendances { get; set; }
        public virtual ICollection<GamesCycle> GamesCycles { get; set; }
        public virtual ICollection<GamesCycle> GamesCycles1 { get; set; }
        public virtual ICollection<GroupsTeam> GroupsTeams { get; set; }
        public virtual ICollection<PlayoffBracket> PlayoffBrackets { get; set; }
        public virtual ICollection<PlayoffBracket> PlayoffBrackets1 { get; set; }
        public virtual ICollection<PlayoffBracket> PlayoffBrackets2 { get; set; }
        public virtual ICollection<PlayoffBracket> PlayoffBrackets3 { get; set; }
        public virtual ICollection<NationalTeamInvitement> NationalTeamInvitements { get; set; }
        public virtual Club Club { get; set; }
        public virtual League League { get; set; }
        public virtual ICollection<Statistic> Statistics { get; set; }
        public virtual ICollection<GameStatistic> GameStatistics { get; set; }
        public virtual ICollection<DriverDetail> DriverDetails { get; set; }
        public virtual ICollection<TennisGameCycle> TennisGameCycles { get; set; }
        public virtual ICollection<TennisGameCycle> TennisGameCycles1 { get; set; }
        public virtual ICollection<TennisGroupTeam> TennisGroupTeams { get; set; }
        public virtual User User1 { get; set; }
        public virtual ICollection<TennisGroupTeam> TennisGroupTeams1 { get; set; }
        public virtual ICollection<TennisGameCycle> TennisGameCycles11 { get; set; }
        public virtual ICollection<TennisGameCycle> TennisGameCycles3 { get; set; }
        public virtual ICollection<TennisCategoryPlayoffRank> TennisCategoryPlayoffRanks { get; set; }
        public virtual ICollection<TennisCategoryPlayoffRank> TennisCategoryPlayoffRanks1 { get; set; }
        public virtual User User2 { get; set; }
        public virtual Discipline Discipline { get; set; }
        public virtual Discipline Discipline1 { get; set; }
        public virtual FriendshipsType FriendshipsType { get; set; }
        public virtual MedicalInstitute MedicalInstitute { get; set; }
        public virtual ICollection<WaterpoloStatistic> WaterpoloStatistics { get; set; }
    }
}
