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
    
    public partial class WeightLiftingSession
    {
        public WeightLiftingSession()
        {
            this.CompetitionDisciplineRegistrations = new HashSet<CompetitionDisciplineRegistration>();
        }
    
        public int Id { get; set; }
        public int CompetitionId { get; set; }
        public int SessionNum { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime WeightStartTime { get; set; }
        public System.DateTime WeightFinishTime { get; set; }
    
        public virtual ICollection<CompetitionDisciplineRegistration> CompetitionDisciplineRegistrations { get; set; }
    }
}