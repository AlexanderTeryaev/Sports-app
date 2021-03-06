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
    
    public partial class TennisGroup
    {
        public TennisGroup()
        {
            this.TennisGameCycles = new HashSet<TennisGameCycle>();
            this.TennisPlayoffBrackets = new HashSet<TennisPlayoffBracket>();
            this.TennisGroupTeams = new HashSet<TennisGroupTeam>();
        }
    
        public int GroupId { get; set; }
        public int StageId { get; set; }
        public int TypeId { get; set; }
        public string Name { get; set; }
        public bool IsArchive { get; set; }
        public Nullable<int> NumberOfCycles { get; set; }
        public bool IsAdvanced { get; set; }
        public Nullable<int> PointEditType { get; set; }
        public Nullable<int> SeasonId { get; set; }
        public bool IsIndividual { get; set; }
        public Nullable<int> NumberOfPlayers { get; set; }
        public bool IsPairs { get; set; }
    
        public virtual Season Season { get; set; }
        public virtual ICollection<TennisGameCycle> TennisGameCycles { get; set; }
        public virtual TennisStage TennisStage { get; set; }
        public virtual ICollection<TennisPlayoffBracket> TennisPlayoffBrackets { get; set; }
        public virtual ICollection<TennisGroupTeam> TennisGroupTeams { get; set; }
    }
}
