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
    
    public partial class Statistic
    {
        public long Id { get; set; }
        public string Abbreviation { get; set; }
        public int GameId { get; set; }
        public double Point_x { get; set; }
        public double Point_y { get; set; }
        public int PlayerId { get; set; }
        public int TeamId { get; set; }
        public System.DateTime Timestamp { get; set; }
        public long GameTime { get; set; }
        public bool IsProcessed { get; set; }
        public string TimeSegmentName { get; set; }
    
        public virtual GamesCycle GamesCycle { get; set; }
        public virtual Team Team { get; set; }
        public virtual TeamsPlayer TeamsPlayer { get; set; }
    }
}
