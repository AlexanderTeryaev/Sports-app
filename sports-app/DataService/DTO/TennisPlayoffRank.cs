using System.Collections.Generic;

namespace DataService.DTO
{
    public class TennisPlayoffRanksGroup
    {
        public List<TennisPlayoffRank> PlayersRanks { get; set; }
        public string GroupName { get; set; }
    }



        public class TennisPlayoffRank
    {
        public string PlayerName { get; set; }
        public int? Rank { get; set; }

        public int? RealMinPos { get; set; }
        public int? RealMaxPos { get; set; }

        public int GamesCount { get; set; }
        public int WinsCount { get; set; }
        public int LostCount => GamesCount - WinsCount;

        public int FirstPlayerScore { get; set; }
        public int SecondPlayerScore { get; set; }

        public int FirstPlayerSetsScore { get; set; }
        public int SecondPlayerSetsScore { get; set; }

        public int HomeSetsMissed { get; set; }
        public int GuestSetsMissed { get; set; }

        public int TotalScored => FirstPlayerScore + SecondPlayerScore;
        public int SetsWon => FirstPlayerSetsScore + SecondPlayerSetsScore;
        public int SetsLost => HomeSetsMissed + GuestSetsMissed;

        public int IndexedRank { get; set; }

        public string SetsRatio
        {
            get
            {
                double scored = SetsWon;
                double lost = SetsLost;
                if (scored == 0 && lost == 0)
                {
                    return string.Format("{0:N2}", 0);
                }
                else if (lost == 0)
                {
                    return "MAX";
                }
                else
                {
                    double ratio = scored / lost;
                    return string.Format("{0:N2}", ratio);
                }
            }
        }

        public string GroupName { get; set; }
        public string PlayerLastRank { get; set; }
        public string PlayerLogo { get; set; }
        public int? PlayerId { get; set; }
        public int? PairPlayerId { get; set; }
        public int? CategoryId { get; set; }
        public int SeasonId { get; set; }
        public int Points { get; set; }
        public bool IsPairGame { get; set; }
        public int? Correction { get; internal set; }
    }
}
