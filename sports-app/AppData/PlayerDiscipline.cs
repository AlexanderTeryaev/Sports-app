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
    
    public partial class PlayerDiscipline
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int DisciplineId { get; set; }
        public int ClubId { get; set; }
        public int SeasonId { get; set; }
        public string EntryTime { get; set; }
    
        public virtual Club Club { get; set; }
        public virtual Discipline Discipline { get; set; }
        public virtual Season Season { get; set; }
        public virtual User User { get; set; }
    }
}
