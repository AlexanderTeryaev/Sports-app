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
    
    public partial class ResetPasswordRequest
    {
        public int Id { get; set; }
        public System.Guid ResetGuid { get; set; }
        public int UserId { get; set; }
        public System.DateTime DateCreated { get; set; }
        public bool IsCompleted { get; set; }
    
        public virtual User User { get; set; }
    }
}
