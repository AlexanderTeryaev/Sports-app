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
    
    public partial class TennisLeagueGameScore
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int HomeScore { get; set; }
        public int GuestScore { get; set; }
        public bool IsPairScores { get; set; }
        public bool IsTieBreak { get; set; }
    
        public virtual TennisLeagueGame TennisLeagueGame { get; set; }
    }
}
