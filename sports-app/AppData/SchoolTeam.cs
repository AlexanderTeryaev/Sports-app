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
    
    public partial class SchoolTeam
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }
        public int TeamId { get; set; }
    
        public virtual School School { get; set; }
        public virtual Team Team { get; set; }
    }
}
