using AppModel;
using System;

namespace DataService.DTO
{
    public class StatisticsDTO
    {
        private string timeInFromat;
        public long StatId { get; set; }
        public Season Season { get; set; }
        public string Overall { get; set; }
        public string GP { get; set; }
        public double? Min { get; set; }
        public string MinsInFormat { get; set; }
        public int FG { get; set; }
        public int FGA { get; set; }
        public int ThreePT { get; set; }
        public int ThreePA { get; set; }
        public int TwoPT { get; set; }
        public int TwoPA { get; set; }
        public int FT { get; set; }
        public int FTA { get; set; }
        public int OREB { get; set; }
        public int DREB { get; set; }
        public int REB { get; set; }
        public int AST { get; set; }
        public int STL { get; set; }
        public int BLK { get; set; }
        public int TO { get; set; }
        public int BS { get; set; }
        public int PF { get; set; }
        public int PTS { get; set; }
        public double EFF { get; set; }
        public double PlusMinus { get; set; }
        public int FGM { get; set; } // not descripted
        public int FTM { get; set; } // not desripted
        public int Goal { get; set; }
        public int PGoal { get; set; }
        public int PMiss { get; set; }
        public int Offs { get; set; }
        public int Foul { get; set; }
        public int Exc { get; set; }
        public int BFoul { get; set; }
        public int YC { get; set; }
        public int RD { get; set; }
        public int SSave { get; set; }
        public int Miss { get; set; }
        public double FGP { get; set; }
        public double GSPP { get; set; }
        public double SAR { get; set; }
        public double SCR { get; set; }
    }

    public class PlayersStatisticsDTO : StatisticsDTO
    {
        public int? PlayersId { get; set; }
        public string PlayersName { get; set; }
        public string PlayersImage { get; set; }
        public int GamesCount { get; set; }
    }

    public class AveragePlayersStatistics : PlayersStatisticsDTO
    {
        public double? MinAverage => Math.Round((double)Min / (double)GamesCount, 1);
        public double FGAverage => Math.Round((double)FG / (double)GamesCount, 1);
        public double FGAAverage => Math.Round((double)FGA / (double)GamesCount, 1);
        public double ThreePTAverage => Math.Round((double)ThreePT / (double)GamesCount, 1);
        public double ThreePAAverage => Math.Round((double)ThreePA / (double)GamesCount, 1);
        public double TwoPTAverage => Math.Round((double)TwoPT / (double)GamesCount, 1);
        public double TwoPAAverage => Math.Round((double)TwoPA / (double)GamesCount, 1);
        public double FTAverage => Math.Round((double)FT / (double)GamesCount, 1);
        public double FTAAverage => Math.Round((double)FTA / (double)GamesCount, 1);
        public double OREBAverage => Math.Round((double)OREB / (double)GamesCount, 1);
        public double DREBAverage => Math.Round((double)DREB / (double)GamesCount, 1);
        public double REBAverage => Math.Round((double)REB / (double)GamesCount, 1);
        public double ASTAverage => Math.Round((double)AST / (double)GamesCount, 1);
        public double STLAverage => Math.Round((double)STL / (double)GamesCount, 1);
        public double BLKAverage => Math.Round((double)BLK / (double)GamesCount, 1);
        public double TOAverage => Math.Round((double)TO / (double)GamesCount, 1);
        public double BSAverage => Math.Round((double)BS / (double)GamesCount, 1);
        public double PFAverage => Math.Round((double)PF / (double)GamesCount, 1);
        public double PTSAverage => Math.Round((double)PTS / (double)GamesCount, 1);
        public double EFFAverage => Math.Round((double)EFF / (double)GamesCount, 1);
        public double PlusMinusAverage => Math.Round((double)PlusMinus / (double)GamesCount, 1);
        public double GoalAverage => Math.Round((double) Goal / (double) GamesCount, 1);
        public double PGoalAverage => Math.Round((double) PGoal / (double) GamesCount, 1);
        public double PMissAverage => Math.Round((double) PMiss / (double) GamesCount, 1);
        public double OffsAverage => Math.Round((double) Offs / (double)GamesCount, 1);
        public double FoulAverage => Math.Round((double) Foul / (double)GamesCount, 1);
        public double ExcAverage => Math.Round((double) Exc / (double)GamesCount, 1);
        public double BFoulAverage => Math.Round((double) BFoul / (double)GamesCount, 1);
        public double SSaveAverage => Math.Round((double) SSave / (double)GamesCount, 1);
        public double YCAverage => Math.Round((double) YC / (double)GamesCount, 1);
        public double RDAverage => Math.Round((double) RD / (double)GamesCount, 1);
        public double MissAverage => Math.Round((double) Miss / (double)GamesCount, 1);
        public double FGPAverage => Math.Round((double) FGP / (double)GamesCount, 1);
        public double GSPPAverage => Math.Round((double) GSPP / (double)GamesCount, 1);
        public double SARAverage => Math.Round((double) SAR / (double)GamesCount, 1);
        public double SCRAverage => Math.Round((double) SCR / (double)GamesCount, 1);
    }
}
