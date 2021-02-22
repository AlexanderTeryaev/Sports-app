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
    
    public partial class TennisLeagueGame
    {
        public TennisLeagueGame()
        {
            this.TennisLeagueGameScores = new HashSet<TennisLeagueGameScore>();
        }
    
        public int Id { get; set; }
        public int CycleId { get; set; }
        public int GameNumber { get; set; }
        public Nullable<int> HomePlayerId { get; set; }
        public Nullable<int> GuestPlayerId { get; set; }
        public Nullable<int> TechnicalWinnerId { get; set; }
        public bool IsEnded { get; set; }
        public Nullable<int> HomePairPlayerId { get; set; }
        public Nullable<int> GuestPairPlayerId { get; set; }
    
        public virtual GamesCycle GamesCycle { get; set; }
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
        public virtual User User2 { get; set; }
        public virtual ICollection<TennisLeagueGameScore> TennisLeagueGameScores { get; set; }
        public virtual User User3 { get; set; }
        public virtual User User21 { get; set; }
    }
}