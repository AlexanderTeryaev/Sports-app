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
    
    public partial class ContentType
    {
        public ContentType()
        {
            this.Contents = new HashSet<Content>();
        }
    
        public int TypeId { get; set; }
        public string Name { get; set; }
        public bool IsArchive { get; set; }
        public string ContType { get; set; }
    
        public virtual ICollection<Content> Contents { get; set; }
    }
}
