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
    
    public partial class ChipPrice
    {
        public int ChipId { get; set; }
        public int UnionId { get; set; }
        public Nullable<int> FromAge { get; set; }
        public Nullable<int> ToAge { get; set; }
        public Nullable<int> Price { get; set; }
        public int SeasonId { get; set; }
    
        public virtual Season Season { get; set; }
        public virtual Union Union { get; set; }
    }
}
