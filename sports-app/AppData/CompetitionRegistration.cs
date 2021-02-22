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
    
    public partial class CompetitionRegistration
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SeasonId { get; set; }
        public int LeagueId { get; set; }
        public int ClubId { get; set; }
        public Nullable<int> CompetitionRouteId { get; set; }
        public Nullable<int> OrderNumber { get; set; }
        public Nullable<double> FinalScore { get; set; }
        public Nullable<int> Position { get; set; }
        public Nullable<int> CompositionNumber { get; set; }
        public Nullable<int> InstrumentId { get; set; }
        public bool IsRegisteredByExcel { get; set; }
        public bool IsActive { get; set; }
        public bool IsTeam { get; set; }
        public string Creator { get; set; }
        public string Created { get; set; }
    
        public virtual Club Club { get; set; }
        public virtual League League { get; set; }
        public virtual CompetitionRoute CompetitionRoute { get; set; }
        public virtual CompetitionTeamRoute CompetitionTeamRoute { get; set; }
        public virtual Season Season { get; set; }
        public virtual User User { get; set; }
        public virtual Instrument Instrument { get; set; }
    }
}
