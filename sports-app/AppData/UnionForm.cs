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
    
    public partial class UnionForm
    {
        public int FormId { get; set; }
        public int UnionId { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public int SeasonId { get; set; }
        public bool IsDeleted { get; set; }
    
        public virtual Union Union { get; set; }
        public virtual Season Season { get; set; }
    }
}
