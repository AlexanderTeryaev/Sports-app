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
    
    public partial class NationalTeamInvitement
    {
        public int Id { get; set; }
        public int TeamPlayerId { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<int> SeasonId { get; set; }
    
        public virtual TeamsPlayer TeamsPlayer { get; set; }
        public virtual Season Season { get; set; }
    }
}