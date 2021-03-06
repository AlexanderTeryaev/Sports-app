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
    
    public partial class FriendshipsType
    {
        public FriendshipsType()
        {
            this.CompetitionAges = new HashSet<CompetitionAge>();
            this.FriendshipPrices = new HashSet<FriendshipPrice>();
            this.TeamsPlayers = new HashSet<TeamsPlayer>();
        }
    
        public int FriendshipsTypesId { get; set; }
        public int UnionId { get; set; }
        public string Name { get; set; }
        public bool IsArchive { get; set; }
        public int SeasonId { get; set; }
        public Nullable<int> Hierarchy { get; set; }
    
        public virtual Season Season { get; set; }
        public virtual Union Union { get; set; }
        public virtual ICollection<CompetitionAge> CompetitionAges { get; set; }
        public virtual ICollection<FriendshipPrice> FriendshipPrices { get; set; }
        public virtual ICollection<TeamsPlayer> TeamsPlayers { get; set; }
    }
}
