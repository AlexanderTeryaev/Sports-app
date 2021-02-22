using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataService.Utils
{
    public class GenerateTeamNumbers
    {
        /// <summary>
        /// Sets an array with the teams split numbers
        /// </summary>
        /// <param name="teamSize">Sise of the team list</param>
        /// <param name="rounds">Number of rounds for all stages</param>
        public GenerateTeamNumbers(int teamSize, int rounds)
        {
            TeamSize = teamSize / rounds;
            Rounds = rounds;
            SetNumbersOfTeam();
        }

        public static int[][] NumSeries = {
               new[] { 0 }, new[] { 0, 1 }, new[] { 0, 2, 1, 3 }, new[] { 0, 4, 2, 6, 1, 5, 3, 7 }, new[]{ 0, 8, 4, 14, 2, 12, 6, 10, 1, 11, 5, 13, 3, 9, 7, 15 }
            };

        public static int[] TeamSizes = { 2, 4, 8, 16, 32, 64 };
        private int _teamSize;
        private MinMax[][] _teamsAndStages;
        private int _totalStages;
        public int Rounds { get; set; }
        public int TeamSize
        {
            get { return _teamSize; }
            set
            {
                if (!TeamSizes.Contains(value))
                {
                    throw new Exception();
                }
                _teamSize = value;
                _totalStages = (int)(Math.Log(value) / Math.Log(2)) - 1;
            }
        }


        /// <summary>
        /// Shows how many rows this stage will have eg. 32 teams stage 3 will have 16 rows
        /// </summary>
        /// <param name="stage">For this class stage index is used only for stages that have more than one row.Eg 32 teams will have 3 stages: 0 1 2 3 and the -1 stage that would be the 32 starting teams in one row.</param>
        /// <returns>Total rows for stages</returns>
        public int GetStageTotalRowsForStage(int stage)
        {
            var res = (int)Math.Pow(2, stage + 1);
            return res;
        }

        /// <summary>
        /// Get total matches each row will have
        /// </summary>
        /// <param name="stage"></param>
        /// <returns></returns>
        public int GetStageTotalMatchesPerRow(int stage)
        {
            var res = _teamSize / (int)Math.Pow(2, stage + 1);
            return res;
        }

        public int MaxSwapIndexForStage(int stage)
        {
            var res = (int)Math.Pow(2, stage + 1) / 2;
            return res;
        }

        public int GetTotalStages()
        {
            return _totalStages;
        }

        private static int[] GetNumSerie(int num)
        {
            switch (num)
            {
                case 1:
                    return NumSeries[0];
                case 2:
                    return NumSeries[1];
                case 4:
                    return NumSeries[2];
                case 8:
                    return NumSeries[3];
                case 16:
                    return NumSeries[4];
                default:
                    throw new Exception();
            }
        }

        public struct MinMax
        {
            public int Min;
            public int Max;
        }
        public void SetNumbersOfTeam()
        {
            var stages = GetTotalStages();
            _teamsAndStages = new MinMax[stages][];
            for (var u = 0; u < stages; u++)
            {
                var totalRows = GetStageTotalRowsForStage(u);
                var winlooseCombinedRows = totalRows / 2;
                var numbers = GetNumSerie(winlooseCombinedRows);
                var arr1 = new MinMax[winlooseCombinedRows];
                var arr2 = new MinMax[winlooseCombinedRows];
                var teamsStep = this._teamSize / totalRows;

                var min = 1;
                var max = teamsStep;
                for (var i = 0; i < winlooseCombinedRows; i++)
                {
                    arr1[numbers[i]] = new MinMax { Min = min, Max = max };
                    min = max + 1;
                    max = max + teamsStep;
                    arr2[numbers[i]] = new MinMax { Min = min, Max = max };
                    min = max + 1;
                    max = max + teamsStep;
                }
                var retArr = new MinMax[totalRows];
                var index = 0;
                for (var i = winlooseCombinedRows - 1; i >= 0; i--)
                {
                    retArr[index] = arr2[i];
                    index++;
                }
                for (var i = winlooseCombinedRows - 1; i >= 0; i--)
                {
                    retArr[index] = arr1[i];
                    index++;
                }
                this._teamsAndStages[u] = retArr;
            }
        }

        /// <summary>
        /// Prints the team index
        /// </summary>
        /// <param name="stage">stage index</param>
        /// <param name="index">row index</param>
        /// <returns></returns>
        public string PrintTeamIndex(int stage, int index)
        {
            try
            {
                return $"{this._teamsAndStages[stage][index].Min} - {this._teamsAndStages[stage][index].Max}";
            }
            catch (Exception e)
            {
                return "not found Index:" + index + "stage:" + stage;
            }
        }

        /// <summary>
        /// ToString for testing purpose eg: 
        /// Just copy this class in a new console project and use this lines in the main
        ///Console.WriteLine("Insert TeamSize");
        ///int teamsize = int.Parse(Console.ReadLine());
        ///var a = new GenerateTeamNumbers(teamsize);
        ///Console.WriteLine(a.ToString());
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var stage = 0;
            var ret = "";

            foreach (var teamsAndStage in _teamsAndStages)
            {
                ret += "stage: " + stage + " ";
                ret = teamsAndStage.Aggregate(ret, (current, minMax) => current + (minMax.Min + "-" + minMax.Max + " "));
                stage++;
                ret += "\n";
            }
            return ret;
        }
    }
}