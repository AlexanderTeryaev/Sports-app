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
    
    public partial class TravelInformation
    {
        public int Id { get; set; }
        public int UserJobId { get; set; }
        public Nullable<System.DateTime> FromHour { get; set; }
        public Nullable<System.DateTime> ToHour { get; set; }
        public bool IsUnionTravel { get; set; }
        public Nullable<bool> NoTravel { get; set; }
    
        public virtual UsersJob UsersJob { get; set; }
    }
}
