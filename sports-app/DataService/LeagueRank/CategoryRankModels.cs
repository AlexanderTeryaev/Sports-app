using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using AppModel;

namespace DataService.LeagueRank
{

    public class RankCategory
    {
        public List<TennisRankStage> Stages { get; set; }
        public int? UnionId { get; set; }
        public int CategoryId { get; set; }
        public string AboutCategory { get; set; }
        public int SeasonId { get; set; }
        public string CategoryStructure { get; set; }

        public RankCategory()
        {
            Stages = new List<TennisRankStage>();
            Players = new List<TeamsPlayer>();
        }

        public List<TeamsPlayer> Players { get; set; }

        public string Name { get; set; }

        public string Logo { get; set; }
        public bool IsEmptyRankTable { get; set; }
    }

    public class TennisRankStage
    {
        public List<TennisRankGroup> Groups { get; set; }

        public TennisRankStage()
        {
            Groups = new List<TennisRankGroup>();
        }
        public string Name { get; set; }
        public bool Playoff { get; set; }
        public int Number { get; set; }
        public int StageId { get; set; }
    }

    public class TennisRankGroup
    {
        public string GameType { get; set; }
        public List<TennisRankPlayers> Players { get; set; }

        public int? PointsEditType { get; set; }

        public bool IsAdvanced { get; set; }

        public int? PlayoffBrackets { get; set; }

        public TennisRankGroup()
        {
            Players = new List<TennisRankPlayers>();
        }

        public string Title { get; set; }
        public List<TennisExtendedTable> ExtendedTables { get; set; } = new List<TennisExtendedTable>();
    }


    public class TennisRankPlayers
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

        public int FirstPlayerFinalScore { get; set; }

        public int SecondPlayerFinalScore { get; set; }

        public short PlayerPosition { get; set; }

        public int PointsDifference
        {
            get { return FirstPlayerFinalScore - SecondPlayerFinalScore; }
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

        public int TotalFirstPlayerPoints { get; set; }

        public int TotalSecondPlayerPoints { get; set; }

        public int TotalPointsLost { get; set; }

        public int TotalPointsDiffs
        {
            get
            {
                return this.TotalPointsScored - this.TotalPointsLost;
            }
        }

        public int FirstPlayerScore { get; set; }
        public int SecondPlayerScore { get; set; }

        public string Logo { get; set; } = "";

        public int CategoryPoints { get; set; }
        public int CompetitionRank { get; set; }
        public int? PairPlayerId { get; set; }
    }

    public class TennisExtendedTable
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public char Letter { get; set; }

        public List<TennisExtendedTableScore> Scores { get; set; } = new List<TennisExtendedTableScore>();
    }

    public class TennisExtendedTableScore
    {
        public int OpponentPlayerId { get; set; }
        public int OpponentScore { get; set; }
        public int PlayerScore { get; set; }
    }
}
