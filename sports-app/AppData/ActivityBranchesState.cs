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
    
    public partial class ActivityBranchesState
    {
        public int Id { get; set; }
        public int ActivityBranchId { get; set; }
        public int UserId { get; set; }
        public bool Collapsed { get; set; }
    
        public virtual ActivityBranch ActivityBranch { get; set; }
        public virtual User User { get; set; }
    }
}
