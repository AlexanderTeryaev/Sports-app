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
    
    public partial class NotesRecipient
    {
        public int MsgId { get; set; }
        public int UserId { get; set; }
        public bool IsArchive { get; set; }
        public bool IsRead { get; set; }
        public bool IsPushSent { get; set; }
        public bool IsEmailSent { get; set; }
    
        public virtual NotesMessage NotesMessage { get; set; }
        public virtual User User { get; set; }
    }
}
