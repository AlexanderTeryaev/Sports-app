using System.Collections.Generic;

namespace CmsApp.Models
{
    public class TennisGameViewModel
    {
        public int? GameId { get; set; }

        public int GameCycleId { get; set; }
        public int GameNumber { get; set; }
        public TennisPlayerInformation HomeInformation { get; set; }
        public TennisPlayerInformation GuestInformation { get; set; }
        public List<Set> Sets { get; set; }

        public int PositionIndex => this.GameNumber - 1;
        public bool IsEnded { get; set; }
    }

    public class Set
    {
        public int HomeScore { get; set; }
        public int GuestScore { get; set; }
        public bool IsPairScores { get; set; }
        public bool IsTieBreak { get; set; }
    }

    public class TennisPlayerInformation
    {
        public bool IsTechnicalWinner { get; set; }
        public int? PlayerId { get; set; }
        public int? PairPlayerId { get; set; }
    }
}