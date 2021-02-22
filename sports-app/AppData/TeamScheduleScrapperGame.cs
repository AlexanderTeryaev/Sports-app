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
    
    public partial class TeamScheduleScrapperGame
    {
        public TeamScheduleScrapperGame()
        {
            this.TeamScheduleScrappers = new HashSet<TeamScheduleScrapper>();
        }
    
        public int Id { get; set; }
        public string GameUrl { get; set; }
        public Nullable<int> TeamId { get; set; }
        public Nullable<int> ClubId { get; set; }
        public string ExternalTeamName { get; set; }
        public Nullable<int> SeasonId { get; set; }
    
        public virtual ICollection<TeamScheduleScrapper> TeamScheduleScrappers { get; set; }
        public virtual Season Season { get; set; }
    }
}
