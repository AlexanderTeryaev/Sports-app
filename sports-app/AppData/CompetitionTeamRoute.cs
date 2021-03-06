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
    
    public partial class CompetitionTeamRoute
    {
        public CompetitionTeamRoute()
        {
            this.CompetitionRegistrations = new HashSet<CompetitionRegistration>();
            this.AdditionalTeamGymnastics = new HashSet<AdditionalTeamGymnastic>();
            this.CompetitionTeamRouteClub = new HashSet<CompetitionTeamRouteClub>();
        }
    
        public int Id { get; set; }
        public int DisciplineId { get; set; }
        public int SeasonId { get; set; }
        public int RouteId { get; set; }
        public int RankId { get; set; }
        public int LeagueId { get; set; }
        public Nullable<int> Composition { get; set; }
        public Nullable<int> SecondComposition { get; set; }
        public Nullable<int> ThirdComposition { get; set; }
        public Nullable<int> FourthComposition { get; set; }
        public Nullable<int> FifthComposition { get; set; }
        public Nullable<int> SixthComposition { get; set; }
        public Nullable<int> SeventhComposition { get; set; }
        public string InstrumentIds { get; set; }
        public bool IsCompetitiveEnabled { get; set; }
        public Nullable<int> EighthComposition { get; set; }
        public Nullable<int> NinthComposition { get; set; }
        public Nullable<int> TenthComposition { get; set; }
    
        public virtual Discipline Discipline { get; set; }
        public virtual DisciplineTeamRoute DisciplineTeamRoute { get; set; }
        public virtual Season Season { get; set; }
        public virtual League League { get; set; }
        public virtual ICollection<CompetitionRegistration> CompetitionRegistrations { get; set; }
        public virtual ICollection<AdditionalTeamGymnastic> AdditionalTeamGymnastics { get; set; }
        public virtual RouteTeamRank RouteTeamRank { get; set; }
        public virtual ICollection<CompetitionTeamRouteClub> CompetitionTeamRouteClub { get; set; }
    }
}
