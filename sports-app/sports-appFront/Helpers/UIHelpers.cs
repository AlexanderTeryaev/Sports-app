using System;
using System.Linq.Expressions;
using System.Threading;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Resources;
using System.Collections.Generic;
using AppModel;
using DataService;
using System.Linq;

namespace LogLigFront.Helpers
{
    public static class UIHelpers
    {
        private readonly static DisciplinesRepo disciplineRepo = new DisciplinesRepo();

        public static string CurrrentUICulture
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }

        public static string GetCompetitionCategoryName(int id)
        {
            return disciplineRepo.GetCompetitionCategoryById(id)?.age_name;
        }
        public static string GetCompetitionDisciplineName(int id)
        {
            return disciplineRepo.GetById(id)?.Name;
        }

        public static string GetGenderCharById(int genderId)
        {
            switch (genderId)
            {
                case 0:
                    return "נ";
                case 1:
                    return "ז";
                default:
                    return "ז/נ";
            }
        }

        public static string removeStringLeft2Zeros(string str) {
			if(str == null)
            {
                return "";
            }
            if (string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }
            str = removeStringLeftZero(str);
            str = removeStringLeftZero(str);
            return str;
        }

        private static string removeStringLeftZero(string str)
        {
            if (str.Length>0 && str.Substring(0, 1) == "0")
                str = str.Substring(1, str.Length-1);
            return str;
        }

        private static string removeStringLeftZeroAndThousandSeparator(string str)
        {
            while (str.Length > 0 && (str.Substring(0, 1) == "0" || str.Substring(0, 1) == ","))
                str = str.Substring(1, str.Length - 1);
            return str;
        }


        private static string removeStringLeftSemiColumn(string str)
        {
            if (str.Length > 0 && str.Substring(0, 1) == ":")
                str = str.Substring(1, str.Length - 1);
            return str;
        }

        public static string RemoveRightSidedZeros(string str)
        {
            if(str == null)
            {
                return "";
            }
            if (str.Contains("."))
            {
                for (int i = str.Length - 1; i >= 0; i--)
                {
                    if (str.EndsWith("0") || str.EndsWith("."))
                    {
                        bool wasPeriod = false;
                        if (str.EndsWith("."))
                            wasPeriod = true;
                        str = str.Substring(0, str.Length - 1);
                        if (wasPeriod)
                            break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return str;
        }

        public static string GetAthleticLeagueTypeById(int? typeId)
        {
            switch (typeId)
            {
                case 1:
                    return Messages.AlLeague;
                case 2:
                    return Messages.PremiereLeague;
                case 3:
                    return Messages.NationalLeague;
                case 4:
                    return Messages.AlefLeague;
                case 11:
                    return Messages.GoldenSpikesU14;
                case 12:
                    return Messages.GoldenSpikesU16;
                default: return "";
            }
        }

        public static string GetStageName(int stageIndex, int stagesNum)
        {
            if (stageIndex == 1)
            {
                return "שלב שני של תחרות וגם הראשון של תנחומים";
            }
            var fromEnd = stagesNum - 1 - stageIndex;
            switch (fromEnd)
            {
                case 0:
                    return "גמר תחרות וגם חצי גמר וגמר תנחומים";
                case 1:
                    return "חצי גמר תחרות וגם רבע גמר תנחומים";
                case 2:
                    return "רבע גמר תחרות וגם שמינית גמר תנחומים";
                case 3:
                    return "שמינית גמר תחרות וגם 1-16 גמר תנחומים";
                default:
                    return 2 * Math.Pow(2, fromEnd) + " אחרונות";
            }
        }

        private static string GetPostRenderResult(string str) {
            return removeStringLeftSemiColumn(str);
        }

        public static string GetAlternativeResultStringByValue(int value)
        {
            switch (value)
            {
                case 0:
                    return string.Empty;
                case 1:
                    return "DNF";
                case 2:
                    return "DQ";
                case 3:
                    return "DNS";
                case 4:
                    return "NM";
            }
            return string.Empty;
        }
        public static string GetCompetitionDisciplineResultString(string result, int? format)
        {
            string[] alternativeResultArray = new string[] { "DNF", "DQ", "DNS", "NM" };
            if (alternativeResultArray.Contains(result))
            {
                return result;
            }
            if (format == null)
            {
                format = 0;
            }
            var strs = new string[0];
            var strs2 = new string[4];
            if (result != null)
            {
                strs = result.Split(new[] { ":", "." }, StringSplitOptions.None);
            };
            strs2[0] = "";
            strs2[1] = "";
            strs2[2] = "";
            strs2[3] = "";
            for (int i = 0; i < strs.Length; i++)
            {
                strs2[i] = strs[i];
            }
            switch (format)
            {
                case 1:
                    {
                        strs2[2] = removeStringLeft2Zeros(strs2[2]);
                        return GetPostRenderResult($"{strs2[2]}.{strs2[3]}");
                    }
                case 2:
                    {
                        strs2[1] = removeStringLeft2Zeros(strs2[1]);
                        return GetPostRenderResult($"{strs2[1]}:{strs2[2]}.{strs2[3]}");
                    }
                case 3:
                    {
                        strs2[1] = removeStringLeft2Zeros(strs2[1]);
                        return GetPostRenderResult($"{strs2[1]}:{strs2[2]}.{strs2[3]}");
                    }
                case 4:
                    {
                        strs2[0] = removeStringLeft2Zeros(strs2[0]);
                        return GetPostRenderResult($"{strs2[0]}:{strs2[1]}:{strs2[2]}.{strs2[3]}");
                    }
                case 5:
                    {
                        if (strs2[0] == "00")
                        {
                            return $"{strs2[1]}:{strs2[2]}";
                        }
                        else
                        {
                            strs2[0] = removeStringLeftZero(strs2[0]);
                            strs2[0] = removeStringLeftSemiColumn(strs2[0]);
                            return GetPostRenderResult($"{strs2[0]}:{strs2[1]}:{strs2[2]}");
                        }
                    }
                case 9:
                    {
                        strs2[1] = removeStringLeft2Zeros(strs2[1]);
                        return GetPostRenderResult($"{strs2[1]}:{strs2[2]}");
                    }
                case 10:
                case 6:
                    {
                        strs2[0] = removeStringLeft2Zeros(strs2[0]);
                        return GetPostRenderResult($"{strs2[0]}.{strs2[1]}");
                    }
                case 11:
                case 7:
                    {
                        strs2[0] = removeStringLeft2Zeros(strs2[0]);
                        return GetPostRenderResult($"{strs2[0]}.{strs2[1]}");
                    }
                case 8:
                    {
                        strs2[0] = removeStringLeft2Zeros(strs2[0]);
                        string res = removeStringLeftZeroAndThousandSeparator($"{strs2[0]},{strs2[1]}");
                        return GetPostRenderResult(res);
                    }
                default:
                    {
                        return GetPostRenderResult(result);
                    }
            }
        }

    }

}