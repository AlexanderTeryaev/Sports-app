using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace DataService.Services
{

    public class GoldenSpikeData
    {
        Dictionary<string, List<double>> data = new Dictionary<string, List<double>>();
        private readonly int maxPoints = 45;
        private readonly int minPoints = 1;

        public static GoldenSpikeData ParseGoldenSpikesExcelFile()
        {
            var goldenSpikeData = new GoldenSpikeData();
            var goldenSpikesExcelFile = Resources.golden;
            Stream stream = new MemoryStream(goldenSpikesExcelFile);
            using (var workBook = new XLWorkbook(stream))
            {
                int index = 0;
                foreach (var workSheet in workBook.Worksheets)
                {
                    int columnSkip = 1;
                    int formatRow = 11;
                    string spikeType = "U16";
                    int genderId = 0;
                    List<string> disciplineColumns = new List<string>();
                    switch (index)
                    {
                        case 0:
                            spikeType = "U16";
                            genderId = 1;
                            formatRow = 11;
                            goldenSpikeData.AddList("100m", genderId, spikeType);
                            goldenSpikeData.AddList("80m", genderId, spikeType);
                            goldenSpikeData.AddList("300m", genderId, spikeType);
                            goldenSpikeData.AddList("600m", genderId, spikeType);
                            goldenSpikeData.AddList("1000m", genderId, spikeType);
                            goldenSpikeData.AddList("2000m", genderId, spikeType);
                            goldenSpikeData.AddList("100mh", genderId, spikeType);
                            goldenSpikeData.AddList("250mh", genderId, spikeType);
                            goldenSpikeData.AddList("high_jump", genderId, spikeType);
                            goldenSpikeData.AddList("pole_vault", genderId, spikeType);
                            goldenSpikeData.AddList("long_jump", genderId, spikeType);
                            goldenSpikeData.AddList("shot_put", genderId, spikeType);
                            goldenSpikeData.AddList("discus_throw", genderId, spikeType);
                            goldenSpikeData.AddList("javelin_throw", genderId, spikeType);

                            disciplineColumns.Add("100m");
                            disciplineColumns.Add("80m");
                            disciplineColumns.Add("300m");
                            disciplineColumns.Add("600m");
                            disciplineColumns.Add("1000m");
                            disciplineColumns.Add("2000m");
                            disciplineColumns.Add("100mh");
                            disciplineColumns.Add("250mh");
                            disciplineColumns.Add("high_jump");
                            disciplineColumns.Add("pole_vault");
                            disciplineColumns.Add("long_jump");
                            disciplineColumns.Add("shot_put");
                            disciplineColumns.Add("discus_throw");
                            disciplineColumns.Add("javelin_throw");
                            break;
                        case 1:
                            spikeType = "U16";
                            genderId = 0;
                            formatRow = 10;
                            goldenSpikeData.AddList("80m", genderId, spikeType);
                            goldenSpikeData.AddList("300m", genderId, spikeType);
                            goldenSpikeData.AddList("600m", genderId, spikeType);
                            goldenSpikeData.AddList("1000m", genderId, spikeType);
                            goldenSpikeData.AddList("2000m", genderId, spikeType);
                            goldenSpikeData.AddList("80mh", genderId, spikeType);
                            goldenSpikeData.AddList("250mh", genderId, spikeType);
                            goldenSpikeData.AddList("high_jump", genderId, spikeType);
                            goldenSpikeData.AddList("pole_vault", genderId, spikeType);
                            goldenSpikeData.AddList("long_jump", genderId, spikeType);
                            goldenSpikeData.AddList("shot_put", genderId, spikeType);
                            goldenSpikeData.AddList("discus_throw", genderId, spikeType);
                            goldenSpikeData.AddList("javelin_throw", genderId, spikeType);

                            disciplineColumns.Add("80m");
                            disciplineColumns.Add("300m");
                            disciplineColumns.Add("600m");
                            disciplineColumns.Add("1000m");
                            disciplineColumns.Add("2000m");
                            disciplineColumns.Add("80mh");
                            disciplineColumns.Add("250mh");
                            disciplineColumns.Add("high_jump");
                            disciplineColumns.Add("pole_vault");
                            disciplineColumns.Add("long_jump");
                            disciplineColumns.Add("shot_put");
                            disciplineColumns.Add("discus_throw");
                            disciplineColumns.Add("javelin_throw");
                            break;
                        case 2:
                            spikeType = "U14";
                            genderId = 1;
                            formatRow = 12;
                            goldenSpikeData.AddList("50m", genderId, spikeType);
                            goldenSpikeData.AddList("60m", genderId, spikeType);
                            goldenSpikeData.AddList("100m", genderId, spikeType);
                            goldenSpikeData.AddList("1000m", genderId, spikeType);
                            goldenSpikeData.AddList("50mh", genderId, spikeType);
                            goldenSpikeData.AddList("60mh", genderId, spikeType);
                            goldenSpikeData.AddList("high_jump", genderId, spikeType);
                            goldenSpikeData.AddList("pole_vault", genderId, spikeType);
                            goldenSpikeData.AddList("long_jump", genderId, spikeType);
                            goldenSpikeData.AddList("triple_jump", genderId, spikeType);
                            goldenSpikeData.AddList("shot_put", genderId, spikeType);
                            goldenSpikeData.AddList("discus_throw", genderId, spikeType);
                            goldenSpikeData.AddList("hockey_ball", genderId, spikeType);
                            goldenSpikeData.AddList("hammer_throw", genderId, spikeType);
                            goldenSpikeData.AddList("javelin_throw", genderId, spikeType);
                            goldenSpikeData.AddList("2000m", genderId, spikeType);
                            goldenSpikeData.AddList("4x60m", genderId, spikeType);

                            disciplineColumns.Add("50m");
                            disciplineColumns.Add("60m");
                            disciplineColumns.Add("100m");
                            disciplineColumns.Add("1000m");
                            disciplineColumns.Add("50mh");
                            disciplineColumns.Add("60mh");
                            disciplineColumns.Add("high_jump");
                            disciplineColumns.Add("pole_vault");
                            disciplineColumns.Add("long_jump");
                            disciplineColumns.Add("triple_jump");
                            disciplineColumns.Add("shot_put");
                            disciplineColumns.Add("discus_throw");
                            disciplineColumns.Add("hockey_ball");
                            disciplineColumns.Add("hammer_throw");
                            disciplineColumns.Add("javelin_throw");
                            disciplineColumns.Add("2000m");
                            disciplineColumns.Add("4x60m");
                            break;
                        case 3:
                            spikeType = "U14";
                            genderId = 0;
                            formatRow = 10;
                            goldenSpikeData.AddList("50m", genderId, spikeType);
                            goldenSpikeData.AddList("60m", genderId, spikeType);
                            goldenSpikeData.AddList("100m", genderId, spikeType);
                            goldenSpikeData.AddList("1000m", genderId, spikeType);
                            goldenSpikeData.AddList("50mh", genderId, spikeType);
                            goldenSpikeData.AddList("60mh", genderId, spikeType);
                            goldenSpikeData.AddList("high_jump", genderId, spikeType);
                            goldenSpikeData.AddList("pole_vault", genderId, spikeType);
                            goldenSpikeData.AddList("long_jump", genderId, spikeType);
                            goldenSpikeData.AddList("triple_jump", genderId, spikeType);
                            goldenSpikeData.AddList("shot_put", genderId, spikeType);
                            goldenSpikeData.AddList("discus_throw", genderId, spikeType);
                            goldenSpikeData.AddList("hockey_ball", genderId, spikeType);
                            goldenSpikeData.AddList("javelin_throw", genderId, spikeType);
                            goldenSpikeData.AddList("2000m", genderId, spikeType);
                            goldenSpikeData.AddList("4x60m", genderId, spikeType);

                            disciplineColumns.Add("50m");
                            disciplineColumns.Add("60m");
                            disciplineColumns.Add("100m");
                            disciplineColumns.Add("1000m");
                            disciplineColumns.Add("50mh");
                            disciplineColumns.Add("60mh");
                            disciplineColumns.Add("high_jump");
                            disciplineColumns.Add("pole_vault");
                            disciplineColumns.Add("long_jump");
                            disciplineColumns.Add("triple_jump");
                            disciplineColumns.Add("shot_put");
                            disciplineColumns.Add("discus_throw");
                            disciplineColumns.Add("hockey_ball");
                            disciplineColumns.Add("javelin_throw");
                            disciplineColumns.Add("2000m");
                            disciplineColumns.Add("4x60m");
                            break;

                    }
                    for (int i = 1 + columnSkip; i < disciplineColumns.Count() + 1 + columnSkip; i++)
                    {
                        var columnCells = workSheet.Column(i);
                        var format = columnCells.Cell(formatRow).GetDouble();
                        var disciplineName = disciplineColumns.ElementAt(i - 1 - columnSkip);
                        foreach (var cell in columnCells.Cells().Where(c => c.Address.RowNumber > formatRow && !string.IsNullOrWhiteSpace(c.GetString())))
                        {

                            if (cell.DataType == XLDataType.DateTime)
                            {
                                var dateScore = cell.GetDateTime();
                                goldenSpikeData.AddValue(dateScore, disciplineName, genderId, spikeType);
                            }
                            else
                            {
                                var rawScore = cell.GetString();
                                goldenSpikeData.AddValue(rawScore, disciplineName, genderId, spikeType, format);
                            }
                        }
                    }


                    int columnIndex = 1;
                    foreach (var column in workSheet.Columns())
                    {
                        columnIndex++;
                    }
                    index++;
                }
            }
            return goldenSpikeData;
        }



        public void AddList(string disciplineName, int GenderId, string spikeType)
        {
            data.Add($"{spikeType}_{GenderId}_{disciplineName}", new List<double>());
        }

        public List<double> GetList(string disciplineName, int GenderId, string spikeType)
        {
            List<double> result;
            data.TryGetValue($"{spikeType}_{GenderId}_{disciplineName}", out result);
            return result;
        }

        private string mustBe2Digits(string str)
        {
            if (str.Length == 1)
            {
                return $"0{str}";
            }
            return str;
        }
        public void AddValue(string value, string disciplineName, int GenderId, string spikeType, double writeFormat)
        {
            var list = GetList(disciplineName, GenderId, spikeType);
            bool success = false;
            switch (writeFormat)
            {
                case 1:
                    var parts = value.Split(',').Reverse().ToList();
                    var milis = parts[0];
                    var seconds = mustBe2Digits(parts[1]);
                    var minutes = "00";
                    if (parts.Count() == 3)
                    {
                        minutes = mustBe2Digits(parts[2]);
                    }

                    int intValue;
                    bool isInt = int.TryParse(seconds, out intValue);
                    if (isInt && intValue >= 60)
                    {
                        var minsIntValue = intValue / 60;
                        var secsIntValue = intValue % 60;
                        seconds = mustBe2Digits(secsIntValue.ToString());
                        minutes = mustBe2Digits(minsIntValue.ToString());
                    }
                    TimeSpan res;
                    success = TimeSpan.TryParse($"00:{minutes}:{seconds}.{milis}", out res);
                    if (success)
                    {
                        list.Add(res.TotalMilliseconds);
                    }
                    break;
                case 3:
                    var parts3 = value.Split(',').Reverse().ToList();
                    var milis3 = parts3[0].Substring(2, 2);
                    var seconds3 = parts3[0].Substring(0,2);
                    var minutes3 = mustBe2Digits(parts3[1]);

                    TimeSpan res3;
                    success = TimeSpan.TryParse($"00:{minutes3}:{seconds3}.{milis3}", out res3);
                    if (success)
                    {
                        list.Add(res3.TotalMilliseconds);
                    }
                    break;
                case 5:
                    var parts5 = value.Replace(',', '.');
                    double res5;
                    success = double.TryParse(parts5, out res5);
                    if (success)
                    {
                        list.Add(res5*1000);
                    }
                    break;
                default:

                    break;
            }
        }

        public void AddValue(DateTime dateScore, string disciplineName, int genderId, string spikeType)
        {
            var list = GetList(disciplineName, genderId, spikeType);
            list.Add(dateScore.TimeOfDay.TotalMilliseconds);
        }

        private bool isAsc(string disciplineName, int genderId, string spikeType)
        {
            var list = GetList(disciplineName, genderId, spikeType);
            var firstItem = list.FirstOrDefault();
            var lastItem = list.LastOrDefault();
            if (firstItem > lastItem)
            {
                return false;
            }
            return true;
        }


        public int? GetPoint(long? score, string disciplineName, int genderId, string spikeType)
        {
            var list = GetList(disciplineName, genderId, spikeType);
            var isDesc = !isAsc(disciplineName, genderId, spikeType);
            for (int i = 0; i < list.Count; i++)
            {
                var at = list.ElementAt(i);
                if (i == 0)
                {
                    //index at begining
                    var next = list.ElementAt(i + 1);
                    var pointsIfBetween = maxPoints - i;
                    if (isDesc)
                    {
                        if (score >= at)
                        {
                            return maxPoints;
                        }
                        else if(next == score)
                        {
                            return pointsIfBetween-1;
                        }
                    }
                    else
                    {
                        if (score <= at)
                        {
                            return maxPoints;
                        }
                        else if (next == score)
                        {
                            return pointsIfBetween - 1;
                        }
                    }
                }
                if (i + 1 == list.Count)
                {
                    // last index
                    var pointsIfBetween = maxPoints - i;
                    if (isDesc)
                    {
                        if (score < at)
                        {
                            return minPoints;
                        }
                        else
                        {
                            return pointsIfBetween;
                        }
                    }
                    else
                    {
                        if (score > at)
                        {
                            return minPoints;
                        }
                        else
                        {
                            return pointsIfBetween;
                        }
                    }
                }
                else
                {
                    var pointsIfBetween = maxPoints - i;
                    var next = list.ElementAt(i + 1);
                    if (isDesc)
                    {
                        if (score >= at && score >= next)
                        {
                            return pointsIfBetween;
                        }
                    }
                    else
                    {
                        if (score <= at && score <= next)
                        {
                            return pointsIfBetween;
                        }
                    }
                }
            }
            return null;
        }

    }
}
