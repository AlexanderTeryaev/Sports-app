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
    
    public partial class RetirementRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TeamId { get; set; }
        public System.DateTime RequestDate { get; set; }
        public string Reason { get; set; }
        public string DocumentFileName { get; set; }
        public bool Approved { get; set; }
        public Nullable<int> ApprovedBy { get; set; }
        public Nullable<System.DateTime> DateApproved { get; set; }
        public string ApproveText { get; set; }
        public int RefundAmount { get; set; }
    
        public virtual User ApproveUser { get; set; }
        public virtual User RequestUser { get; set; }
        public virtual Team Team { get; set; }
    }
}