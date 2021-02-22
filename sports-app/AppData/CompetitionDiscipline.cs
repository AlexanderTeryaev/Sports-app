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
    
    public partial class CompetitionDiscipline
    {
        public CompetitionDiscipline()
        {
            this.CompetitionDisciplineRegistrations = new HashSet<CompetitionDisciplineRegistration>();
            this.CompetitionDisciplineHeatStartTimes = new HashSet<CompetitionDisciplineHeatStartTime>();
            this.CompetitionDisciplineTeams = new HashSet<CompetitionDisciplineTeam>();
            this.CompetitionDisciplineClubsRegistrations = new HashSet<CompetitionDisciplineClubsRegistration>();
            this.ActivityFormsSubmittedDatas = new HashSet<ActivityFormsSubmittedData>();
        }
    
        public int Id { get; set; }
        public int CompetitionId { get; set; }
        public int CategoryId { get; set; }
        public Nullable<int> MaxSportsmen { get; set; }
        public Nullable<double> MinResult { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<int> DisciplineId { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public bool IsResultsManualyRanked { get; set; }
        public bool IsMultiBattle { get; set; }
        public bool IsForScore { get; set; }
        public Nullable<int> DistanceId { get; set; }
        public Nullable<System.DateTime> LastResultUpdate { get; set; }
        public Nullable<bool> IsManualPointCalculation { get; set; }
        public Nullable<int> NumberOfWhoPassesToNextStage { get; set; }
        public string CustomFields { get; set; }
        public Nullable<bool> HeatsGenerated { get; set; }
        public string CompetitionRecord { get; set; }
        public Nullable<long> CompetitionRecordSortValue { get; set; }
        public bool IncludeRecordInStartList { get; set; }
        public Nullable<int> LastResultChangeId { get; set; }
        public Nullable<int> TeamRegistration { get; set; }
    
        public virtual CompetitionAge CompetitionAge { get; set; }
        public virtual League League { get; set; }
        public virtual ICollection<CompetitionDisciplineRegistration> CompetitionDisciplineRegistrations { get; set; }
        public virtual RowingDistance RowingDistance { get; set; }
        public virtual ICollection<CompetitionDisciplineHeatStartTime> CompetitionDisciplineHeatStartTimes { get; set; }
        public virtual ICollection<CompetitionDisciplineTeam> CompetitionDisciplineTeams { get; set; }
        public virtual ICollection<CompetitionDisciplineClubsRegistration> CompetitionDisciplineClubsRegistrations { get; set; }
        public virtual ICollection<ActivityFormsSubmittedData> ActivityFormsSubmittedDatas { get; set; }
    }
}