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
    
    public partial class UsersApprovalDatesHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string TeamName { get; set; }
        public System.DateTime ManagerApprovalDate { get; set; }
        public Nullable<int> ActionUserId { get; set; }
        public int SeasonId { get; set; }
    
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
        public virtual Season Season { get; set; }
    }
}
