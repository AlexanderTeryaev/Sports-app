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
    
    public partial class ClubTrainingDay
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public int AuditoriumId { get; set; }
        public string TrainingDay { get; set; }
        public string TrainingStartTime { get; set; }
        public string TrainingEndTime { get; set; }
        public bool IsArchive { get; set; }
    
        public virtual Auditorium Auditorium { get; set; }
        public virtual Club Club { get; set; }
    }
}
