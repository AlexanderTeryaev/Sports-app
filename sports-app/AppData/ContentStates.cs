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
    
    public partial class ContentStates
    {
        public ContentStates()
        {
            this.Contents = new HashSet<Contents>();
        }
    
        public int StateId { get; set; }
        public string StateName { get; set; }
        public bool IsArchive { get; set; }
    
        public virtual ICollection<Contents> Contents { get; set; }
    }
}
