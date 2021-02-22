using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using AppModel;

namespace DataService.LeagueRank
{

    public class RankLeague
    {
        public List<RankStage> Stages { get; set; }
        public int? UnionId { get; set; }
        public int LeagueId { get; set; }
        public string AboutLeague { get; set; }
        public int SeasonId { get; set; }
        public string LeagueStructure { get; set; }

        public RankLeague()
        {
            Stages = new List<RankStage>();
            Teams = new List<Team>();
            TeamPenalties = new List<TeamPenalty>();
        }

        public List<Team> Teams { get; set; }

        public List<TeamPenalty> TeamPenalties { get; set; }
        public bool CanEditPenalties { get; set; }

        public string Name { get; set; }

        public string Logo { get; set; }
        public bool IsEmptyRankTable { get; set; }
        public bool IsTennisLeague { get; set; }
        public LeagueRankedStanding RankedStanding { get; set; }
    }

    public class RankStage
    {
        public List<RankGroup> Groups { get; set; }

        public RankStage()
        {
            Groups = new List<RankGroup>();
        }
        public string Name { get; set; }
        public bool Playoff { get; set; }
        public int Number { get; set; }
        public string CustomStageName { get; set; }
        public int StageId { get; set; }
        public bool StageGamesCompleted { get; set; }
        public bool RankedStandingEnabled { get; set; }
    }

    public class RankGroup
    {
        public string GameType { get; set; }
        public List<RankTeam> Teams { get; set; }

        public int? PointsEditType { get; set; }

        public bool IsAdvanced { get; set; }

        public int? PlayoffBrackets { get; set; }

        public RankGroup()
        {
            Teams = new List<RankTeam>();
        }

        public string Title { get; set; }
        public int GroupId { get; set; }
        public List<ExtendedTable> ExtendedTables { get; set; } = new List<ExtendedTable>();
    }


    public class RankTeam
    {

        public int? Id { get; set; } = 0;

        public int Points { get; set; }
        public int Draw { get; set; } = 0;

        public string Title { get; set; }

        public string Address { get; set; } = "";

        public string Position { get; set; } = "";

        public int PositionNumber { get; set; }

        public int Games { get; set; }

        public int Wins { get; set; }

        public int Loses { get; set; }

        public int TechLosses { get; set; } = 0;

        public int SetsWon { get; set; }

        public int SetsLost { get; set; }

        public int HomeTeamFinalScore { get; set; }

        public int GuesTeamFinalScore { get; set; }

        public short TeamPosition { get; set; }

        public int DIP { get; set; }

        [DisplayFormat(DataFormatString = "{0:#.##}")]
        public decimal DRR
        {
            get { return (DIP ==0 ? GuesTeamFinalScore : (decimal)GuesTeamFinalScore/DIP); }
        }

        public int PointsDifference
        {
            get { return HomeTeamFinalScore - GuesTeamFinalScore; }
        }

        public string SetsRatio
        {
            get
            {
                double scored = this.SetsWon;
                double lost = this.SetsLost;
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

        public double SetsRatioNumiric
        {
            get
            {
                double scored = this.SetsWon;
                double lost = this.SetsLost;
                if (scored == 0 && lost == 0)
                {
                    return 0;
                }
                else if (lost == 0)
                {
                    return double.MaxValue;
                }
                else
                {
                    double ratio = scored / lost;
                    return ratio;
                }
            }
        }


        public int TotalPointsScored { get; set; }

        public int TotalHomeTeamPoints { get; set; }

        public int TotalGuesTeamPoints { get; set; }

        public int TotalPointsLost { get; set; }

        public int TotalPointsDiffs
        {
            get
            {
                return this.TotalPointsScored - this.TotalPointsLost;
            }
        }

        public int HomeTeamScore { get; set; }
        public int GuestTeamScore { get; set; }

        public string Logo { get; set; } = "";
        public TennisGameInfo TennisInfo { get; set; } = new TennisGameInfo();
    }

    public class TennisGameInfo
    {

        /// <summary>
        /// Number of team matches
        /// </summary>
        public int Matches { get; set; }

        /// <summary>
        /// Sum of winning games (players games)
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Number of matches the team won
        /// </summary>
        public int Wins { get; set; }

        /// <summary>
        /// Number of matches the team lost
        /// </summary>
        public int Lost { get; set; }

        /// <summary>
        /// Number of matches the team has tie
        /// </summary>
        public int Ties { get; set; } 

        /// <summary>
        /// Number of sets won by the players
        /// </summary>
        public int PlayersSetsWon { get; set; }

        /// <summary>
        /// Number of sets lost by the players
        /// </summary>
        public int PlayersSetsLost { get; set; }

        /// <summary>
        /// Number of games inside a sets won by the players.
        /// </summary>
        public int PlayersGamingWon { get; set; }

        /// <summary>
        /// Number of games inside a sets lost by the players.
        /// </summary>
        public int PlayersGamingLost { get; set; }

        /// <summary>
        /// Sum of points earned by Tech lost
        /// </summary>
        public int Penalties { get; set; }
        /// <summary>
        /// Difference of gaming
        /// </summary>
        public int PlayerGamingDifference { get; set; }
    }

    public class ExtendedTable
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public char Letter { get; set; }

        public List<ExtendedTableScore> Scores { get; set; } = new List<ExtendedTableScore>();
    }

    public class ExtendedTableScore
    {
        public int OpponentTeamId { get; set; }
        public int OpponentScore { get; set; }
        public int TeamScore { get; set; }
        public int GameId { get; set; }
    }
}
