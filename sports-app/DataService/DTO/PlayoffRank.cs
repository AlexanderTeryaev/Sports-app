using DataService.LeagueRank;

namespace DataService.DTO
{
    public class PlayoffRank
    {
        public string TeamName { get; set; }
        public int? Rank { get; set; }
        public int GamesCount { get; set; }
        public int WinsCount { get; set; }
        public int LostCount => GamesCount - WinsCount;

        public int HomeTeamScore { get; set; }
        public int GuestTeamsScore { get; set; }

        public int HomeMissed { get; set; }
        public int GuestMissed { get; set; }

        public int HomeSetsScore { get; set; }
        public int GuestSetsScore { get; set; }

        public int HomeSetsMissed { get; set; }
        public int GuestSetsMissed { get; set; }

        public int TotalScored => HomeTeamScore + GuestTeamsScore;
        public int TotalMissed => HomeMissed + GuestMissed;
        public int ScoreDifference => TotalScored - TotalMissed;

        public int SetsWon => HomeSetsScore + GuestSetsScore;
        public int SetsLost => HomeSetsMissed + GuestSetsMissed;

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

        public string TeamLogo { get; set; }
        public int TeamId { get; set; }
        public int? LeagueId { get; set; }
        public int SeasonId { get; set; }

        public TennisGameInfo TennisInfo { get; set; }
    }
}
