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
    
    public partial class BlockadeNotification
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }
        public int BlockadeId { get; set; }
        public bool IsShown { get; set; }
    
        public virtual PlayersBlockade PlayersBlockade { get; set; }
        public virtual User User { get; set; }
    }
}
