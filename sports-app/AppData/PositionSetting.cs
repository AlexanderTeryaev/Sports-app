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
    
    public partial class PositionSetting
    {
        public int Id { get; set; }
        public int LeagueId { get; set; }
        public int SeasonId { get; set; }
        public int Position { get; set; }
        public Nullable<int> Points { get; set; }
    
        public virtual League League { get; set; }
        public virtual Season Season { get; set; }
    }
}
