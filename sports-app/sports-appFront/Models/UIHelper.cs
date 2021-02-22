using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Resources;

namespace LogLigFront.Models
{
    public static class UIHelper
    {
        public static string DefaultImage = "~/content/img/default.png";

        public static string GetTeamLogo(string imgName)
        {
            if (!string.IsNullOrEmpty(imgName))
            {
                return String.Concat(ConfigurationManager.AppSettings["SiteUrl"], "/Assets/teams/" + imgName);
            }
            else
            {
                return VirtualPathUtility.ToAbsolute(DefaultImage);
            }
        }

        public static string GetPlayersImage(string imgName)
        {
            if (!string.IsNullOrEmpty(imgName))
            {
                return String.Concat(ConfigurationManager.AppSettings["SiteUrl"], "/assets/players/" + imgName);
            }
            else
            {
                return VirtualPathUtility.ToAbsolute(DefaultImage);
            }
        }
        public static string GetGenderTitles(int? gender)
        {
            switch (gender)
            {
                case 0:
                    return Messages.Female;
                case 1:
                    return Messages.Male;
                default:
                    return string.Empty;
            }
        }

        public static string GetCompetitionTypeById(int? typeId)
        {
            switch (typeId)
            {
                case 1:
                    return Messages.StadiumCompetition;
                case 2:
                    return Messages.RoadCompetition;
                case 3:
                    return Messages.FieldCompetition;
                default:
                    return string.Empty;
            }
        }
        public static string GetLeagueLogo(string imgName)
        {
            if (!string.IsNullOrEmpty(imgName))
            {
                return String.Concat(ConfigurationManager.AppSettings["SiteUrl"], "/Assets/league/" + imgName);
            }
            else
            {
                return string.Empty;
            }
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
                        return GetPostRenderResult($"{strs2[0]},{strs2[1]}");
                    }
                default:
                    {
                        return GetPostRenderResult(result);
                    }
            }
        }


        private static string removeStringLeft2Zeros(string str)
        {
            str = removeStringLeftZero(str);
            str = removeStringLeftZero(str);
            return str;
        }

        private static string removeStringLeftZero(string str)
        {
            if (str.Length > 0 && str.Substring(0, 1) == "0")
                str = str.Substring(1, str.Length - 1);
            return str;
        }

        private static string removeStringLeftSemiColumn(string str)
        {
            if (str.Length > 0 && str.Substring(0, 1) == ":")
                str = str.Substring(1, str.Length - 1);
            return str;
        }

        private static string GetPostRenderResult(string str)
        {
            return removeStringLeftSemiColumn(str);
        }

        public static string GetCurrentResultTitle(string sectionAlias)
        {

            if (string.IsNullOrEmpty(sectionAlias)) return string.Empty;

            if (string.Equals(sectionAlias, GamesAlias.BasketBall, StringComparison.OrdinalIgnoreCase)) return Messages.BasketRatio;
            if (string.Equals(sectionAlias, GamesAlias.WaterPolo, StringComparison.OrdinalIgnoreCase)) return Messages.GoalRatio;
            if (string.Equals(sectionAlias, GamesAlias.Softball, StringComparison.OrdinalIgnoreCase)) return Messages.Runs;

            return Messages.Acts;
        }

        public static string GetCurrentResultRatio(string sectionAlias)
        {
            if (string.IsNullOrEmpty(sectionAlias)) return string.Empty;
            if (string.Equals(sectionAlias, GamesAlias.Soccer, StringComparison.OrdinalIgnoreCase)) return Messages.GoalRatio;
            if (string.Equals(sectionAlias, GamesAlias.Rugby, StringComparison.OrdinalIgnoreCase)) return Messages.GoalRatio;
            
            return Messages.ActsRatio;
        }
    }
}